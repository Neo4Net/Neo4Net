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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.RecordIdIterator.forwards;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using BatchFeedStep = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Goes through <seealso cref="RelationshipStore"/> and increments counts per start/end node,
	/// calling <seealso cref="NodeRelationshipCache.incrementCount(long)"/>. This is in preparation of linking relationships.
	/// </summary>
	public class NodeDegreeCountStage : Stage
	{
		 public const string NAME = "Node Degrees";

		 public NodeDegreeCountStage( Configuration config, RelationshipStore store, NodeRelationshipCache cache, StatsProvider memoryUsageStatsProvider ) : base( NAME, null, config, RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, forwards(0, store.HighId, config), store.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new Neo4Net.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, store));
			  Add( new ReadRecordsStep<object>( Control(), config, false, store ) );
			  Add( new CalculateDenseNodesStep( Control(), config, cache, memoryUsageStatsProvider ) );
		 }
	}

}