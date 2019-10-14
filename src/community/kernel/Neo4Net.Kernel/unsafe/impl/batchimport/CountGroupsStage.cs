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
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using BatchFeedStep = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.allIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	/// <summary>
	/// Stage for counting groups per node, populates <seealso cref="RelationshipGroupCache"/>. Steps:
	/// 
	/// <ol>
	/// <li><seealso cref="ReadRecordsStep"/> reads <seealso cref="RelationshipGroupRecord relationship group records"/> for later counting.</li>
	/// <li><seealso cref="CountGroupsStep"/> populates <seealso cref="RelationshipGroupCache"/> with how many relationship groups each
	/// node has. This is useful for calculating how to divide the work of defragmenting the relationship groups
	/// in a <seealso cref="ScanAndCacheGroupsStage later stage"/>.</li>
	/// </ol>
	/// </summary>
	public class CountGroupsStage : Stage
	{
		 public const string NAME = "Count groups";

		 public CountGroupsStage( Configuration config, RecordStore<RelationshipGroupRecord> store, RelationshipGroupCache groupCache, params StatsProvider[] additionalStatsProviders ) : base( NAME, null, config, RECYCLE_BATCHES )
		 {
			  Add( new BatchFeedStep( Control(), config, allIn(store, config), store.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, false, store));
			  Add( new ReadRecordsStep<object>( Control(), config, false, store ) );
			  Add( new CountGroupsStep( Control(), config, groupCache, additionalStatsProviders ) );
		 }
	}

}