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
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using NodeType = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeType;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StorePrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.StorePrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Updates sparse <seealso cref="NodeRecord node records"/> with relationship heads after relationship linking. Steps:
	/// 
	/// <ol>
	/// <li><seealso cref="ReadNodeIdsByCacheStep"/> looks at <seealso cref="NodeRelationshipCache"/> for which nodes have had
	/// relationships imported and loads those <seealso cref="NodeRecord records"/> from store.</li>
	/// <li><seealso cref="RecordProcessorStep"/> / <seealso cref="SparseNodeFirstRelationshipProcessor"/> uses <seealso cref="NodeRelationshipCache"/>
	/// to update each <seealso cref="NodeRecord.setNextRel(long)"/>.
	/// <li><seealso cref="UpdateRecordsStep"/> writes the updated records back into store.</li>
	/// </ol>
	/// </summary>
	public class SparseNodeFirstRelationshipStage : Stage
	{
		 public const string NAME = "Node --> Relationship";

		 public SparseNodeFirstRelationshipStage( Configuration config, NodeStore nodeStore, NodeRelationshipCache cache ) : base( NAME, null, config, ORDER_SEND_DOWNSTREAM | RECYCLE_BATCHES )
		 {
			  Add( new ReadNodeIdsByCacheStep( Control(), config, cache, NodeType.NODE_TYPE_SPARSE ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, true, nodeStore));
			  Add( new ReadRecordsStep<object>( Control(), config, true, nodeStore ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new RecordProcessorStep<>(control(), "LINK", config, new SparseNodeFirstRelationshipProcessor(cache), false));
			  Add( new RecordProcessorStep<object>( Control(), "LINK", config, new SparseNodeFirstRelationshipProcessor(cache), false ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new UpdateRecordsStep<>(control(), config, nodeStore, new org.neo4j.unsafe.impl.batchimport.store.StorePrepareIdSequence()));
			  Add( new UpdateRecordsStep<object>( Control(), config, nodeStore, new StorePrepareIdSequence() ) );
		 }
	}

}