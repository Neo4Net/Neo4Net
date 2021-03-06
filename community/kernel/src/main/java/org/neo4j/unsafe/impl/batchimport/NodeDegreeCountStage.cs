﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.forwards;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using BatchFeedStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

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
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, store));
			  Add( new ReadRecordsStep<object>( Control(), config, false, store ) );
			  Add( new CalculateDenseNodesStep( Control(), config, cache, memoryUsageStatsProvider ) );
		 }
	}

}