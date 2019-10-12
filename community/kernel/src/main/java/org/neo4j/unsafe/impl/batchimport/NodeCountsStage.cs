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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using NodeLabelsCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using BatchFeedStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.allIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Reads all records from <seealso cref="NodeStore"/> and process the counts in them, populating <seealso cref="NodeLabelsCache"/>
	/// for later use of <seealso cref="RelationshipCountsStage"/>.
	/// </summary>
	public class NodeCountsStage : Stage
	{
		 public const string NAME = "Node counts";

		 public NodeCountsStage( Configuration config, NodeLabelsCache cache, NodeStore nodeStore, int highLabelId, Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater, ProgressReporter progressReporter, params StatsProvider[] additionalStatsProviders ) : base( NAME, null, config, RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, allIn(nodeStore, config), nodeStore.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, nodeStore));
			  Add( new ReadRecordsStep<object>( Control(), config, false, nodeStore ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new RecordProcessorStep<>(control(), "COUNT", config, new NodeCountsProcessor(nodeStore, cache, highLabelId, countsUpdater, progressReporter), true, additionalStatsProviders));
			  Add( new RecordProcessorStep<object>( Control(), "COUNT", config, new NodeCountsProcessor(nodeStore, cache, highLabelId, countsUpdater, progressReporter), true, additionalStatsProviders ) );
		 }
	}

}