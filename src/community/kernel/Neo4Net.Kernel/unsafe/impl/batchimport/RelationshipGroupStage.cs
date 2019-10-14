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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Neo4Net.Kernel.impl.store;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StorePrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.StorePrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Takes information about relationship groups in the <seealso cref="NodeRelationshipCache"/>, which is produced
	/// as a side-effect of linking relationships together, and writes them out into <seealso cref="RelationshipGroupStore"/>.
	/// </summary>
	public class RelationshipGroupStage : Stage
	{
		 public const string NAME = "RelationshipGroup";

		 public RelationshipGroupStage( string topic, Configuration config, RecordStore<RelationshipGroupRecord> store, NodeRelationshipCache cache ) : base( NAME, topic, config, RECYCLE_BATCHES )
		 {
			  Add( new ReadGroupRecordsByCacheStep( Control(), config, store, cache ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new UpdateRecordsStep<>(control(), config, store, new org.neo4j.unsafe.impl.batchimport.store.StorePrepareIdSequence()));
			  Add( new UpdateRecordsStep<object>( Control(), config, store, new StorePrepareIdSequence() ) );
		 }
	}

}