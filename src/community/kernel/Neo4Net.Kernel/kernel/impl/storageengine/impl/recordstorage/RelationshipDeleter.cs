using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using InvalidRecordException = Neo4Net.Kernel.impl.store.InvalidRecordException;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using DirectionIdentifier = Neo4Net.Kernel.impl.transaction.state.DirectionIdentifier;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordAccessSet = Neo4Net.Kernel.impl.transaction.state.RecordAccessSet;
	using DirectionWrapper = Neo4Net.Kernel.impl.util.DirectionWrapper;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.RelationshipCreator.relCount;

	internal class RelationshipDeleter
	{
		 private readonly RelationshipGroupGetter _relGroupGetter;
		 private readonly PropertyDeleter _propertyChainDeleter;

		 internal RelationshipDeleter( RelationshipGroupGetter relGroupGetter, PropertyDeleter propertyChainDeleter )
		 {
			  this._relGroupGetter = relGroupGetter;
			  this._propertyChainDeleter = propertyChainDeleter;
		 }

		 /// <summary>
		 /// Deletes a relationship by its id, returning its properties which are now
		 /// removed. It is assumed that the nodes it connects have already been
		 /// deleted in this
		 /// transaction.
		 /// </summary>
		 /// <param name="id"> The id of the relationship to delete. </param>
		 internal virtual void RelDelete( long id, RecordAccessSet recordChanges, ResourceLocker locks )
		 {
			  RelationshipRecord record = recordChanges.RelRecords.getOrLoad( id, null ).forChangingLinkage();
			  _propertyChainDeleter.deletePropertyChain( record, recordChanges.PropertyRecords );
			  DisconnectRelationship( record, recordChanges, locks );
			  UpdateNodesForDeletedRelationship( record, recordChanges, locks );
			  record.InUse = false;
		 }

		 private void DisconnectRelationship( RelationshipRecord rel, RecordAccessSet recordChangeSet, ResourceLocker locks )
		 {
			  Disconnect( rel, RelationshipConnection.StartNext, recordChangeSet.RelRecords, locks );
			  Disconnect( rel, RelationshipConnection.StartPrev, recordChangeSet.RelRecords, locks );
			  Disconnect( rel, RelationshipConnection.EndNext, recordChangeSet.RelRecords, locks );
			  Disconnect( rel, RelationshipConnection.EndPrev, recordChangeSet.RelRecords, locks );
		 }

		 private void Disconnect( RelationshipRecord rel, RelationshipConnection pointer, RecordAccess<RelationshipRecord, Void> relChanges, ResourceLocker locks )
		 {
			  long otherRelId = pointer.otherSide().get(rel);
			  if ( otherRelId == Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					return;
			  }

			  locks.AcquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, otherRelId );
			  RelationshipRecord otherRel = relChanges.GetOrLoad( otherRelId, null ).forChangingLinkage();
			  bool changed = false;
			  long newId = pointer.get( rel );
			  bool newIsFirst = pointer.isFirstInChain( rel );
			  if ( otherRel.FirstNode == pointer.compareNode( rel ) )
			  {
					pointer.start().set(otherRel, newId, newIsFirst);
					changed = true;
			  }
			  if ( otherRel.SecondNode == pointer.compareNode( rel ) )
			  {
					pointer.end().set(otherRel, newId, newIsFirst);
					changed = true;
			  }
			  if ( !changed )
			  {
					throw new InvalidRecordException( otherRel + " don't match " + rel );
			  }
		 }

		 private void UpdateNodesForDeletedRelationship( RelationshipRecord rel, RecordAccessSet recordChanges, ResourceLocker locks )
		 {
			  RecordAccess_RecordProxy<NodeRecord, Void> startNodeChange = recordChanges.NodeRecords.getOrLoad( rel.FirstNode, null );
			  RecordAccess_RecordProxy<NodeRecord, Void> endNodeChange = recordChanges.NodeRecords.getOrLoad( rel.SecondNode, null );

			  NodeRecord startNode = recordChanges.NodeRecords.getOrLoad( rel.FirstNode, null ).forReadingLinkage();
			  NodeRecord endNode = recordChanges.NodeRecords.getOrLoad( rel.SecondNode, null ).forReadingLinkage();
			  bool loop = startNode.Id == endNode.Id;

			  if ( !startNode.Dense )
			  {
					if ( rel.FirstInFirstChain )
					{
						 startNode = startNodeChange.ForChangingLinkage();
						 startNode.NextRel = rel.FirstNextRel;
					}
					DecrementTotalRelationshipCount( startNode.Id, rel, startNode.NextRel, recordChanges.RelRecords, locks );
			  }
			  else
			  {
					RecordAccess_RecordProxy<RelationshipGroupRecord, int> groupChange = _relGroupGetter.getRelationshipGroup( startNode, rel.Type, recordChanges.RelGroupRecords ).group();
					Debug.Assert( groupChange != null, "Relationship group " + rel.Type + " should have existed here" );
					RelationshipGroupRecord group = groupChange.ForReadingData();
					DirectionWrapper dir = DirectionIdentifier.wrapDirection( rel, startNode );
					if ( rel.FirstInFirstChain )
					{
						 group = groupChange.ForChangingData();
						 dir.setNextRel( group, rel.FirstNextRel );
						 if ( GroupIsEmpty( group ) )
						 {
							  DeleteGroup( startNodeChange, group, recordChanges.RelGroupRecords );
						 }
					}
					DecrementTotalRelationshipCount( startNode.Id, rel, dir.getNextRel( group ), recordChanges.RelRecords, locks );
			  }

			  if ( !endNode.Dense )
			  {
					if ( rel.FirstInSecondChain )
					{
						 endNode = endNodeChange.ForChangingLinkage();
						 endNode.NextRel = rel.SecondNextRel;
					}
					if ( !loop )
					{
						 DecrementTotalRelationshipCount( endNode.Id, rel, endNode.NextRel, recordChanges.RelRecords, locks );
					}
			  }
			  else
			  {
					RecordAccess_RecordProxy<RelationshipGroupRecord, int> groupChange = _relGroupGetter.getRelationshipGroup( endNode, rel.Type, recordChanges.RelGroupRecords ).group();
					DirectionWrapper dir = DirectionIdentifier.wrapDirection( rel, endNode );
					Debug.Assert( groupChange != null || loop, "Group has been deleted" );
					if ( groupChange != null )
					{
						 RelationshipGroupRecord group;
						 if ( rel.FirstInSecondChain )
						 {
							  group = groupChange.ForChangingData();
							  dir.setNextRel( group, rel.SecondNextRel );
							  if ( GroupIsEmpty( group ) )
							  {
									DeleteGroup( endNodeChange, group, recordChanges.RelGroupRecords );
							  }
						 }
					} // Else this is a loop-rel and the group was deleted when dealing with the start node
					if ( !loop )
					{
						 DecrementTotalRelationshipCount( endNode.Id, rel, dir.getNextRel( groupChange.ForChangingData() ), recordChanges.RelRecords, locks );
					}
			  }
		 }

		 private void DecrementTotalRelationshipCount( long nodeId, RelationshipRecord rel, long firstRelId, RecordAccess<RelationshipRecord, Void> relRecords, ResourceLocker locks )
		 {
			  if ( firstRelId == Record.NO_PREV_RELATIONSHIP.intValue() )
			  {
					return;
			  }
			  bool firstInChain = RelIsFirstInChain( nodeId, rel );
			  if ( !firstInChain )
			  {
					locks.AcquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, firstRelId );
			  }
			  RelationshipRecord firstRel = relRecords.GetOrLoad( firstRelId, null ).forChangingLinkage();
			  if ( nodeId == firstRel.FirstNode )
			  {
					firstRel.FirstPrevRel = firstInChain ? relCount( nodeId, rel ) - 1 : relCount( nodeId, firstRel ) - 1;
					firstRel.FirstInFirstChain = true;
			  }
			  if ( nodeId == firstRel.SecondNode )
			  {
					firstRel.SecondPrevRel = firstInChain ? relCount( nodeId, rel ) - 1 : relCount( nodeId, firstRel ) - 1;
					firstRel.FirstInSecondChain = true;
			  }
		 }

		 private void DeleteGroup( RecordAccess_RecordProxy<NodeRecord, Void> nodeChange, RelationshipGroupRecord group, RecordAccess<RelationshipGroupRecord, int> relGroupRecords )
		 {
			  long previous = group.Prev;
			  long next = group.Next;
			  if ( previous == Record.NO_NEXT_RELATIONSHIP.intValue() )
			  { // This is the first one, just point the node to the next group
					nodeChange.ForChangingLinkage().NextRel = next;
			  }
			  else
			  { // There are others before it, point the previous to the next group
					RelationshipGroupRecord previousRecord = relGroupRecords.GetOrLoad( previous, null ).forChangingLinkage();
					previousRecord.Next = next;
			  }

			  if ( next != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  { // There are groups after this one, point that next group to the previous of the group to be deleted
					RelationshipGroupRecord nextRecord = relGroupRecords.GetOrLoad( next, null ).forChangingLinkage();
					nextRecord.Prev = previous;
			  }
			  group.InUse = false;
		 }

		 private bool GroupIsEmpty( RelationshipGroupRecord group )
		 {
			  return group.FirstOut == Record.NO_NEXT_RELATIONSHIP.intValue() && group.FirstIn == Record.NO_NEXT_RELATIONSHIP.intValue() && group.FirstLoop == Record.NO_NEXT_RELATIONSHIP.intValue();
		 }

		 private bool RelIsFirstInChain( long nodeId, RelationshipRecord rel )
		 {
			  return ( nodeId == rel.FirstNode && rel.FirstInFirstChain ) || ( nodeId == rel.SecondNode && rel.FirstInSecondChain );
		 }
	}

}