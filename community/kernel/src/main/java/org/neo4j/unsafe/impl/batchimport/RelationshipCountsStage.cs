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
	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using NodeLabelsCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NumberArrayFactory = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using BatchFeedStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.allIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Reads all records from <seealso cref="RelationshipStore"/> and process the counts in them. Uses a <seealso cref="NodeLabelsCache"/>
	/// previously populated by f.ex <seealso cref="NodeCountsStage"/>.
	/// </summary>
	public class RelationshipCountsStage : Stage
	{
		 public const string NAME = "Relationship counts";

		 public RelationshipCountsStage( Configuration config, NodeLabelsCache cache, RelationshipStore relationshipStore, int highLabelId, int highRelationshipTypeId, Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater, NumberArrayFactory cacheFactory, ProgressReporter progressReporter ) : base( NAME, null, config, RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, allIn(relationshipStore, config), relationshipStore.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, relationshipStore));
			  Add( new ReadRecordsStep<object>( Control(), config, false, relationshipStore ) );
			  Add( new ProcessRelationshipCountsDataStep( Control(), cache, config, highLabelId, highRelationshipTypeId, countsUpdater, cacheFactory, progressReporter ) );
		 }
	}

}