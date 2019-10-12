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

	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using BatchFeedStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using BatchingNeoStores = Org.Neo4j.@unsafe.Impl.Batchimport.store.BatchingNeoStores;
	using PrepareIdSequence = Org.Neo4j.@unsafe.Impl.Batchimport.store.PrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RecordIdIterator.forwards;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.RECYCLE_BATCHES;

	public class RelationshipLinkforwardStage : Stage
	{
		 public const string NAME = "Relationship --> Relationship";

		 public RelationshipLinkforwardStage( string topic, Configuration config, BatchingNeoStores stores, NodeRelationshipCache cache, System.Predicate<RelationshipRecord> readFilter, System.Predicate<RelationshipRecord> denseChangeFilter, int nodeTypes, params StatsProvider[] additionalStatsProvider ) : base( NAME, topic, config, ORDER_SEND_DOWNSTREAM | RECYCLE_BATCHES )
		 {
			  RelationshipStore store = stores.RelationshipStore;
			  Add( new BatchFeedStep( Control(), config, forwards(0, store.HighId, config), store.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.neo4j.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, true, store, new org.neo4j.unsafe.impl.batchimport.staging.RecordDataAssembler<>(store::newRecord, readFilter)));
			  Add( new ReadRecordsStep<object>( Control(), config, true, store, new RecordDataAssembler<object>(store.newRecord, readFilter) ) );
			  Add( new RelationshipLinkforwardStep( Control(), config, cache, denseChangeFilter, nodeTypes, additionalStatsProvider ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new UpdateRecordsStep<>(control(), config, store, org.neo4j.unsafe.impl.batchimport.store.PrepareIdSequence.of(stores.usesDoubleRelationshipRecordUnits())));
			  Add( new UpdateRecordsStep<object>( Control(), config, store, PrepareIdSequence.of(stores.UsesDoubleRelationshipRecordUnits()) ) );
		 }
	}

}