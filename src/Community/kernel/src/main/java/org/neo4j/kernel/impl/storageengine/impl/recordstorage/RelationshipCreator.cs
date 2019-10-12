using System;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using DirectionIdentifier = Neo4Net.Kernel.impl.transaction.state.DirectionIdentifier;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordAccessSet = Neo4Net.Kernel.impl.transaction.state.RecordAccessSet;
	using DirectionWrapper = Neo4Net.Kernel.impl.util.DirectionWrapper;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;

	public class RelationshipCreator
	{
		 private readonly RelationshipGroupGetter _relGroupGetter;
		 private readonly int _denseNodeThreshold;

		 public RelationshipCreator( RelationshipGroupGetter relGroupGetter, int denseNodeThreshold )
		 {
			  this._relGroupGetter = relGroupGetter;
			  this._denseNodeThreshold = denseNodeThreshold;
		 }

		 /// <summary>
		 /// Creates a relationship with the given id, from the nodes identified by id
		 /// and of type typeId
		 /// </summary>
		 /// <param name="id"> The id of the relationship to create. </param>
		 /// <param name="type"> The id of the relationship type this relationship will
		 ///            have. </param>
		 /// <param name="firstNodeId"> The id of the start node. </param>
		 /// <param name="secondNodeId"> The id of the end node. </param>
		 public virtual void RelationshipCreate( long id, int type, long firstNodeId, long secondNodeId, RecordAccessSet recordChangeSet, ResourceLocker locks )
		 {
			  // TODO could be unnecessary to mark as changed here already, dense nodes may not need to change
			  NodeRecord firstNode = recordChangeSet.NodeRecords.getOrLoad( firstNodeId, null ).forChangingLinkage();
			  NodeRecord secondNode = recordChangeSet.NodeRecords.getOrLoad( secondNodeId, null ).forChangingLinkage();
			  ConvertNodeToDenseIfNecessary( firstNode, recordChangeSet.RelRecords, recordChangeSet.RelGroupRecords, locks );
			  ConvertNodeToDenseIfNecessary( secondNode, recordChangeSet.RelRecords, recordChangeSet.RelGroupRecords, locks );
			  RelationshipRecord record = recordChangeSet.RelRecords.create( id, null ).forChangingLinkage();
			  record.SetLinks( firstNodeId, secondNodeId, type );
			  record.InUse = true;
			  record.SetCreated();
			  ConnectRelationship( firstNode, secondNode, record, recordChangeSet.RelRecords, recordChangeSet.RelGroupRecords, locks );
		 }

		 internal static int RelCount( long nodeId, RelationshipRecord rel )
		 {
			  return ( int )( nodeId == rel.FirstNode ? rel.FirstPrevRel : rel.SecondPrevRel );
		 }

		 private void ConvertNodeToDenseIfNecessary( NodeRecord node, RecordAccess<RelationshipRecord, Void> relRecords, RecordAccess<RelationshipGroupRecord, int> relGroupRecords, ResourceLocker locks )
		 {
			  if ( node.Dense )
			  {
					return;
			  }
			  long relId = node.NextRel;
			  if ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RecordAccess_RecordProxy<RelationshipRecord, Void> relChange = relRecords.GetOrLoad( relId, null );
					RelationshipRecord rel = relChange.ForReadingLinkage();
					if ( RelCount( node.Id, rel ) >= _denseNodeThreshold )
					{
						 locks.AcquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, relId );
						 // Re-read the record after we've locked it since another transaction might have
						 // changed in the meantime.
						 relChange = relRecords.GetOrLoad( relId, null );

						 ConvertNodeToDenseNode( node, relChange.ForChangingLinkage(), relRecords, relGroupRecords, locks );
					}
			  }
		 }

		 private void ConnectRelationship( NodeRecord firstNode, NodeRecord secondNode, RelationshipRecord rel, RecordAccess<RelationshipRecord, Void> relRecords, RecordAccess<RelationshipGroupRecord, int> relGroupRecords, ResourceLocker locks )
		 {
			  // Assertion interpreted: if node is a normal node and we're trying to create a
			  // relationship that we already have as first rel for that node --> error
			  Debug.Assert( firstNode.NextRel != rel.Id || firstNode.Dense );
			  Debug.Assert( secondNode.NextRel != rel.Id || secondNode.Dense );

			  if ( !firstNode.Dense )
			  {
					rel.FirstNextRel = firstNode.NextRel;
			  }
			  if ( !secondNode.Dense )
			  {
					rel.SecondNextRel = secondNode.NextRel;
			  }

			  if ( !firstNode.Dense )
			  {
					Connect( firstNode, rel, relRecords, locks );
			  }
			  else
			  {
					ConnectRelationshipToDenseNode( firstNode, rel, relRecords, relGroupRecords, locks );
			  }

			  if ( !secondNode.Dense )
			  {
					if ( firstNode.Id != secondNode.Id )
					{
						 Connect( secondNode, rel, relRecords, locks );
					}
					else
					{
						 rel.FirstInFirstChain = true;
						 rel.SecondPrevRel = rel.FirstPrevRel;
					}
			  }
			  else if ( firstNode.Id != secondNode.Id )
			  {
					ConnectRelationshipToDenseNode( secondNode, rel, relRecords, relGroupRecords, locks );
			  }

			  if ( !firstNode.Dense )
			  {
					firstNode.NextRel = rel.Id;
			  }
			  if ( !secondNode.Dense )
			  {
					secondNode.NextRel = rel.Id;
			  }
		 }

		 private void ConnectRelationshipToDenseNode( NodeRecord node, RelationshipRecord rel, RecordAccess<RelationshipRecord, Void> relRecords, RecordAccess<RelationshipGroupRecord, int> relGroupRecords, ResourceLocker locks )
		 {
			  RelationshipGroupRecord group = _relGroupGetter.getOrCreateRelationshipGroup( node, rel.Type, relGroupRecords ).forChangingData();
			  DirectionWrapper dir = DirectionIdentifier.wrapDirection( rel, node );
			  long nextRel = dir.getNextRel( group );
			  SetCorrectNextRel( node, rel, nextRel );
			  Connect( node.Id, nextRel, rel, relRecords, locks );
			  dir.setNextRel( group, rel.Id );
		 }

		 private void Connect( NodeRecord node, RelationshipRecord rel, RecordAccess<RelationshipRecord, Void> relRecords, ResourceLocker locks )
		 {
			  Connect( node.Id, node.NextRel, rel, relRecords, locks );
		 }

		 private void ConvertNodeToDenseNode( NodeRecord node, RelationshipRecord firstRel, RecordAccess<RelationshipRecord, Void> relRecords, RecordAccess<RelationshipGroupRecord, int> relGroupRecords, ResourceLocker locks )
		 {
			  node.Dense = true;
			  node.NextRel = Record.NO_NEXT_RELATIONSHIP.intValue();
			  long relId = firstRel.Id;
			  RelationshipRecord relRecord = firstRel;
			  while ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					// Get the next relationship id before connecting it (where linkage is overwritten)
					relId = RelChain( relRecord, node.Id ).get( relRecord );
					ConnectRelationshipToDenseNode( node, relRecord, relRecords, relGroupRecords, locks );
					if ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
					{ // Lock and load the next relationship in the chain
						 locks.AcquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, relId );
						 relRecord = relRecords.GetOrLoad( relId, null ).forChangingLinkage();
					}
			  }
		 }

		 private void Connect( long nodeId, long firstRelId, RelationshipRecord rel, RecordAccess<RelationshipRecord, Void> relRecords, ResourceLocker locks )
		 {
			  long newCount = 1;
			  if ( firstRelId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					locks.AcquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, firstRelId );
					RelationshipRecord firstRel = relRecords.GetOrLoad( firstRelId, null ).forChangingLinkage();
					bool changed = false;
					if ( firstRel.FirstNode == nodeId )
					{
						 newCount = firstRel.FirstPrevRel + 1;
						 firstRel.FirstPrevRel = rel.Id;
						 firstRel.FirstInFirstChain = false;
						 changed = true;
					}
					if ( firstRel.SecondNode == nodeId )
					{
						 newCount = firstRel.SecondPrevRel + 1;
						 firstRel.SecondPrevRel = rel.Id;
						 firstRel.FirstInSecondChain = false;
						 changed = true;
					}
					if ( !changed )
					{
						 throw new InvalidRecordException( nodeId + " doesn't match " + firstRel );
					}
			  }

			  // Set the relationship count
			  if ( rel.FirstNode == nodeId )
			  {
					rel.FirstPrevRel = newCount;
					rel.FirstInFirstChain = true;
			  }
			  if ( rel.SecondNode == nodeId )
			  {
					rel.SecondPrevRel = newCount;
					rel.FirstInSecondChain = true;
			  }
		 }

		 private void SetCorrectNextRel( NodeRecord node, RelationshipRecord rel, long nextRel )
		 {
			  if ( node.Id == rel.FirstNode )
			  {
					rel.FirstNextRel = nextRel;
			  }
			  if ( node.Id == rel.SecondNode )
			  {
					rel.SecondNextRel = nextRel;
			  }
		 }

		 private static RelationshipConnection RelChain( RelationshipRecord rel, long nodeId )
		 {
			  if ( rel.FirstNode == nodeId )
			  {
					return RelationshipConnection.StartNext;
			  }
			  if ( rel.SecondNode == nodeId )
			  {
					return RelationshipConnection.EndNext;
			  }
			  throw new Exception( nodeId + " neither start not end node in " + rel );
		 }
	}

}