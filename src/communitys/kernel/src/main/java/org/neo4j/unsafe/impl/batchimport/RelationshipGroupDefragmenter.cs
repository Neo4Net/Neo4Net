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
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.withBatchSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ExecutionSupervisors.superviseExecution;

	/// <summary>
	/// Defragments <seealso cref="RelationshipGroupRecord"/> so that they end up sequential per node in the group store.
	/// There's one constraint which is assumed to be true here: any relationship group that we see in the store
	/// for any given <seealso cref="RelationshipGroupRecord.getOwningNode() owner"/> must have a lower
	/// <seealso cref="RelationshipGroupRecord.getType() type"/> than any previous group encountered for that node,
	/// i.e. all <seealso cref="RelationshipGroupRecord.getNext() next pointers"/> must be either
	/// <seealso cref="Record.NO_NEXT_RELATIONSHIP NULL"/> or lower than the group id at hand. When this is true,
	/// and the defragmenter verifies this constraint, the groups will be reversed so that types are instead
	/// ascending and groups are always co-located.
	/// </summary>
	public class RelationshipGroupDefragmenter
	{

		 private readonly Configuration _config;
		 private readonly ExecutionMonitor _executionMonitor;
		 private readonly Monitor _monitor;
		 private readonly NumberArrayFactory _numberArrayFactory;

		 public interface Monitor
		 {
			  /// <summary>
			  /// When defragmenting the relationship group store it may happen in chunks, selected by node range.
			  /// Every time a chunk is selected this method is called.
			  /// </summary>
			  /// <param name="fromNodeId"> low node id in the range to process (inclusive). </param>
			  /// <param name="toNodeId"> high node id in the range to process (exclusive). </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void defragmentingNodeRange(long fromNodeId, long toNodeId)
	//		  { // empty
	//		  }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//		  Monitor EMPTY = new Monitor()
	//		  { // empty
	//		  };
		 }

		 public RelationshipGroupDefragmenter( Configuration config, ExecutionMonitor executionMonitor, Monitor monitor, NumberArrayFactory numberArrayFactory )
		 {
			  this._config = config;
			  this._executionMonitor = executionMonitor;
			  this._monitor = monitor;
			  this._numberArrayFactory = numberArrayFactory;
		 }

		 public virtual void Run( long memoryWeCanHoldForCertain, BatchingNeoStores neoStore, long highNodeId )
		 {
			  using ( RelationshipGroupCache groupCache = new RelationshipGroupCache( _numberArrayFactory, memoryWeCanHoldForCertain, highNodeId ) )
			  {
					// Read from the temporary relationship group store...
					RecordStore<RelationshipGroupRecord> fromStore = neoStore.TemporaryRelationshipGroupStore;
					// and write into the main relationship group store
					RecordStore<RelationshipGroupRecord> toStore = neoStore.RelationshipGroupStore;

					// Count all nodes, how many groups each node has each
					Configuration groupConfig = withBatchSize( _config, neoStore.RelationshipGroupStore.RecordsPerPage );
					StatsProvider memoryUsage = new MemoryUsageStatsProvider( neoStore, groupCache );
					ExecuteStage( new CountGroupsStage( groupConfig, fromStore, groupCache, memoryUsage ) );
					long fromNodeId = 0;
					long toNodeId = 0;
					while ( fromNodeId < highNodeId )
					{
						 // See how many nodes' groups we can fit into the cache this iteration of the loop.
						 // Groups that doesn't fit in this round will be included in consecutive rounds.
						 toNodeId = groupCache.Prepare( fromNodeId );
						 _monitor.defragmentingNodeRange( fromNodeId, toNodeId );
						 // Cache those groups
						 ExecuteStage( new ScanAndCacheGroupsStage( groupConfig, fromStore, groupCache, memoryUsage ) );
						 // And write them in sequential order in the store
						 ExecuteStage( new WriteGroupsStage( groupConfig, groupCache, toStore ) );

						 // Make adjustments for the next iteration
						 fromNodeId = toNodeId;
					}

					// Now update nodes to point to the new groups
					ByteArray groupCountCache = groupCache.GroupCountCache;
					groupCountCache.clear();
					Configuration nodeConfig = withBatchSize( _config, neoStore.NodeStore.RecordsPerPage );
					ExecuteStage( new NodeFirstGroupStage( nodeConfig, toStore, neoStore.NodeStore, groupCountCache ) );
			  }
		 }

		 private void ExecuteStage( Stage stage )
		 {
			  superviseExecution( _executionMonitor, stage );
		 }
	}

}