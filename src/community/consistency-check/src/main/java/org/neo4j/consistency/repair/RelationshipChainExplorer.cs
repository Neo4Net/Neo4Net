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
namespace Neo4Net.Consistency.repair
{
	using Neo4Net.Kernel.impl.store;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.repair.RelationshipChainDirection.NEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.repair.RelationshipChainDirection.PREV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class RelationshipChainExplorer
	{
		 private readonly RecordStore<RelationshipRecord> _recordStore;

		 public RelationshipChainExplorer( RecordStore<RelationshipRecord> recordStore )
		 {
			  this._recordStore = recordStore;
		 }

		 public virtual RecordSet<RelationshipRecord> ExploreRelationshipRecordChainsToDepthTwo( RelationshipRecord record )
		 {
			  RecordSet<RelationshipRecord> records = new RecordSet<RelationshipRecord>();
			  foreach ( RelationshipNodeField nodeField in RelationshipNodeField.values() )
			  {
					long nodeId = nodeField.get( record );
					records.AddAll( ExpandChains( ExpandChainInBothDirections( record, nodeId ), nodeId ) );
			  }
			  return records;
		 }

		 private RecordSet<RelationshipRecord> ExpandChains( RecordSet<RelationshipRecord> records, long otherNodeId )
		 {
			  RecordSet<RelationshipRecord> chains = new RecordSet<RelationshipRecord>();
			  foreach ( RelationshipRecord record in records )
			  {
					chains.AddAll( ExpandChainInBothDirections( record, record.FirstNode == otherNodeId ? record.SecondNode : record.FirstNode ) );
			  }
			  return chains;
		 }

		 private RecordSet<RelationshipRecord> ExpandChainInBothDirections( RelationshipRecord record, long nodeId )
		 {
			  return ExpandChain( record, nodeId, PREV ).union( ExpandChain( record, nodeId, NEXT ) );
		 }

		 protected internal virtual RecordSet<RelationshipRecord> FollowChainFromNode( long nodeId, long relationshipId )
		 {
			  return ExpandChain( _recordStore.getRecord( relationshipId, _recordStore.newRecord(), NORMAL ), nodeId, NEXT );
		 }

		 private RecordSet<RelationshipRecord> ExpandChain( RelationshipRecord record, long nodeId, RelationshipChainDirection direction )
		 {
			  RecordSet<RelationshipRecord> chain = new RecordSet<RelationshipRecord>();
			  chain.Add( record );
			  RelationshipRecord currentRecord = record;
			  long nextRelId = direction.fieldFor( nodeId, currentRecord ).relOf( currentRecord );
			  while ( currentRecord.InUse() && !direction.fieldFor(nodeId, currentRecord).endOfChain(currentRecord) )
			  {
					currentRecord = _recordStore.getRecord( nextRelId, _recordStore.newRecord(), FORCE );
					chain.Add( currentRecord );
					nextRelId = direction.fieldFor( nodeId, currentRecord ).relOf( currentRecord );
			  }
			  return chain;
		 }
	}

}