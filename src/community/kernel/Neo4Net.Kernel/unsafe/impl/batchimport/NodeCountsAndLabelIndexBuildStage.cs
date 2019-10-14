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
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using BatchFeedStep = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.allIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Counts nodes and their labels and also builds <seealso cref="LabelScanStore label index"/> while doing so.
	/// </summary>
	public class NodeCountsAndLabelIndexBuildStage : Stage
	{
		 public const string NAME = "Node counts and label index build";

		 public NodeCountsAndLabelIndexBuildStage( Configuration config, NodeLabelsCache cache, NodeStore nodeStore, int highLabelId, Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater, ProgressReporter progressReporter, LabelScanStore labelIndex, params StatsProvider[] additionalStatsProviders ) : base( NAME, null, config, ORDER_SEND_DOWNSTREAM | RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, allIn(nodeStore, config), nodeStore.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, nodeStore));
			  Add( new ReadRecordsStep<object>( Control(), config, false, nodeStore ) );
			  Add( new LabelIndexWriterStep( Control(), config, labelIndex, nodeStore ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new RecordProcessorStep<>(control(), "COUNT", config, new NodeCountsProcessor(nodeStore, cache, highLabelId, countsUpdater, progressReporter), true, additionalStatsProviders));
			  Add( new RecordProcessorStep<object>( Control(), "COUNT", config, new NodeCountsProcessor(nodeStore, cache, highLabelId, countsUpdater, progressReporter), true, additionalStatsProviders ) );
		 }
	}

}