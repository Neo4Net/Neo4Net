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
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using BatchFeedStep = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.allInReversed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Scans <seealso cref="RelationshipGroupRecord"/> from store in reverse, this because during import the relationships
	/// are imported per type in descending type id order, i.e. with highest type first. This stage runs as part
	/// of defragmenting the relationship group store so that relationship groups for a particular node will be
	/// co-located on disk. The <seealso cref="RelationshipGroupCache"/> given to this stage has already been primed with
	/// information about which groups to cache, i.e. for which nodes (id range). This step in combination
	/// with <seealso cref="WriteGroupsStage"/> alternating each other can run multiple times to limit max memory consumption
	/// caching relationship groups.
	/// </summary>
	public class ScanAndCacheGroupsStage : Stage
	{
		 public const string NAME = "Gather";

		 public ScanAndCacheGroupsStage( Configuration config, RecordStore<RelationshipGroupRecord> store, RelationshipGroupCache cache, params StatsProvider[] additionalStatsProviders ) : base( NAME, null, config, RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, allInReversed(store, config), store.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, store));
			  Add( new ReadRecordsStep<object>( Control(), config, false, store ) );
			  Add( new CacheGroupsStep( Control(), config, cache, additionalStatsProviders ) );
		 }
	}

}