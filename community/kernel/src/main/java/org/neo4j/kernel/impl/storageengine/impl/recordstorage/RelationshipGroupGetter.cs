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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using Org.Neo4j.Kernel.impl.transaction.state;

	public class RelationshipGroupGetter
	{
		 private readonly IdSequence _idGenerator;

		 public RelationshipGroupGetter( IdSequence idGenerator )
		 {
			  this._idGenerator = idGenerator;
		 }

		 public virtual RelationshipGroupPosition GetRelationshipGroup( NodeRecord node, int type, RecordAccess<RelationshipGroupRecord, int> relGroupRecords )
		 {
			  long groupId = node.NextRel;
			  long previousGroupId = Record.NO_NEXT_RELATIONSHIP.intValue();
			  RecordAccess_RecordProxy<RelationshipGroupRecord, int> previous = null;
			  RecordAccess_RecordProxy<RelationshipGroupRecord, int> current;
			  while ( groupId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					current = relGroupRecords.GetOrLoad( groupId, null );
					RelationshipGroupRecord record = current.ForReadingData();
					record.Prev = previousGroupId; // not persistent so not a "change"
					if ( record.Type == type )
					{
						 return new RelationshipGroupPosition( previous, current );
					}
					else if ( record.Type > type )
					{ // The groups are sorted in the chain, so if we come too far we can return
						 // empty handed right away
						 return new RelationshipGroupPosition( previous, null );
					}
					previousGroupId = groupId;
					groupId = record.Next;
					previous = current;
			  }
			  return new RelationshipGroupPosition( previous, null );
		 }

		 public virtual RecordAccess_RecordProxy<RelationshipGroupRecord, int> GetOrCreateRelationshipGroup( NodeRecord node, int type, RecordAccess<RelationshipGroupRecord, int> relGroupRecords )
		 {
			  RelationshipGroupPosition existingGroup = GetRelationshipGroup( node, type, relGroupRecords );
			  RecordAccess_RecordProxy<RelationshipGroupRecord, int> change = existingGroup.Group();
			  if ( change == null )
			  {
					Debug.Assert( node.Dense, "Node " + node + " should have been dense at this point" );
					long id = _idGenerator.nextId();
					change = relGroupRecords.Create( id, type );
					RelationshipGroupRecord record = change.ForChangingData();
					record.InUse = true;
					record.SetCreated();
					record.OwningNode = node.Id;

					// Attach it...
					RecordAccess_RecordProxy<RelationshipGroupRecord, int> closestPreviousChange = existingGroup.ClosestPrevious();
					if ( closestPreviousChange != null )
					{ // ...after the closest previous one
						 RelationshipGroupRecord closestPrevious = closestPreviousChange.ForChangingLinkage();
						 record.Next = closestPrevious.Next;
						 record.Prev = closestPrevious.Id;
						 closestPrevious.Next = id;
					}
					else
					{ // ...first in the chain
						 long firstGroupId = node.NextRel;
						 if ( firstGroupId != Record.NO_NEXT_RELATIONSHIP.intValue() )
						 { // There are others, make way for this new group
							  RelationshipGroupRecord previousFirstRecord = relGroupRecords.GetOrLoad( firstGroupId, type ).forReadingData();
							  record.Next = previousFirstRecord.Id;
							  previousFirstRecord.Prev = id;
						 }
						 node.NextRel = id;
					}
			  }
			  return change;
		 }

		 public class RelationshipGroupPosition
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RecordAccess_RecordProxy<RelationshipGroupRecord, int> ClosestPreviousConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RecordAccess_RecordProxy<RelationshipGroupRecord, int> GroupConflict;

			  public RelationshipGroupPosition( RecordAccess_RecordProxy<RelationshipGroupRecord, int> closestPrevious, RecordAccess_RecordProxy<RelationshipGroupRecord, int> group )
			  {
					this.ClosestPreviousConflict = closestPrevious;
					this.GroupConflict = group;
			  }

			  public virtual RecordAccess_RecordProxy<RelationshipGroupRecord, int> Group()
			  {
					return GroupConflict;
			  }

			  public virtual RecordAccess_RecordProxy<RelationshipGroupRecord, int> ClosestPrevious()
			  {
					return ClosestPreviousConflict;
			  }
		 }
	}

}