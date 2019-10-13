﻿/*
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
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

	public class OwningNodeRelationshipChain
	{
		 private readonly RelationshipChainExplorer _relationshipChainExplorer;
		 private readonly RecordStore<NodeRecord> _nodeStore;
		 private readonly NodeRecord _nodeRecord;

		 public OwningNodeRelationshipChain( RelationshipChainExplorer relationshipChainExplorer, RecordStore<NodeRecord> nodeStore )
		 {
			  this._relationshipChainExplorer = relationshipChainExplorer;
			  this._nodeStore = nodeStore;
			  this._nodeRecord = nodeStore.NewRecord();
		 }

		 public virtual RecordSet<RelationshipRecord> FindRelationshipChainsThatThisRecordShouldBelongTo( RelationshipRecord relationship )
		 {
			  RecordSet<RelationshipRecord> records = new RecordSet<RelationshipRecord>();
			  foreach ( RelationshipNodeField field in RelationshipNodeField.values() )
			  {
					long nodeId = field.get( relationship );
					_nodeStore.getRecord( nodeId, _nodeRecord, RecordLoad.FORCE );
					records.AddAll( _relationshipChainExplorer.followChainFromNode( nodeId, _nodeRecord.NextRel ) );
			  }
			  return records;
		 }
	}

}