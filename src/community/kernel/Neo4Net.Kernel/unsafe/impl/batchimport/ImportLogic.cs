using System;
using System.Collections.Generic;
using System.Diagnostics;

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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using IntSet = org.eclipse.collections.api.set.primitive.IntSet;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using Neo4Net.Kernel.impl.store;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using MigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.MigrationProgressMonitor;
	using SilentMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.SilentMigrationProgressMonitor;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using RelationshipTypeCount = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.RelationshipTypeCount;
	using GatheringMemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.GatheringMemoryStatsVisitor;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using NodeType = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeType;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using PageCacheArrayFactoryMonitor = Neo4Net.@unsafe.Impl.Batchimport.cache.PageCacheArrayFactoryMonitor;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using EstimationSanityChecker = Neo4Net.@unsafe.Impl.Batchimport.input.EstimationSanityChecker;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using Input_Estimates = Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using ExecutionSupervisors = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionSupervisors;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.alwaysTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NodeRelationshipCache.calculateMaxMemoryUsage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.auto;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ExecutionSupervisors.superviseExecution;

	/// <summary>
	/// Contains all algorithms and logic for doing an import. It exposes all stages as methods so that
	/// it's possible to implement a <seealso cref="BatchImporter"/> which calls those.
	/// This class has state which typically gets modified in each invocation of an import method.
	/// 
	/// To begin with the methods are fairly coarse-grained, but can and probably will be split up into smaller parts
	/// to allow external implementors have greater control over the flow.
	/// </summary>
	public class ImportLogic : System.IDisposable
	{
		 public interface Monitor
		 {
			  void DoubleRelationshipRecordUnitsEnabled();

			  void MayExceedNodeIdCapacity( long capacity, long estimatedCount );

			  void MayExceedRelationshipIdCapacity( long capacity, long estimatedCount );

			  void InsufficientHeapSize( long optimalMinimalHeapSize, long heapSize );

			  void AbundantHeapSize( long optimalMinimalHeapSize, long heapSize );

			  void InsufficientAvailableMemory( long estimatedCacheSize, long optimalMinimalHeapSize, long availableMemory );
		 }

		 public static readonly Monitor NO_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
			 public void mayExceedRelationshipIdCapacity( long capacity, long estimatedCount )
			 { // no-op
			 }

			 public void mayExceedNodeIdCapacity( long capacity, long estimatedCount )
			 { // no-op
			 }

			 public void doubleRelationshipRecordUnitsEnabled()
			 { // no-op
			 }

			 public void insufficientHeapSize( long optimalMinimalHeapSize, long heapSize )
			 { // no-op
			 }

			 public void abundantHeapSize( long optimalMinimalHeapSize, long heapSize )
			 { // no-op
			 }

			 public void insufficientAvailableMemory( long estimatedCacheSize, long optimalMinimalHeapSize, long availableMemory )
			 { // no-op
			 }
		 }

		 private readonly File _storeDir;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly BatchingNeoStores _neoStore;
		 private readonly Configuration _config;
		 private readonly Log _log;
		 private readonly ExecutionMonitor _executionMonitor;
		 private readonly RecordFormats _recordFormats;
		 private readonly DataImporter.Monitor _storeUpdateMonitor = new DataImporter.Monitor();
		 private readonly long _maxMemory;
		 private readonly Dependencies _dependencies = new Dependencies();
		 private readonly Monitor _monitor;
		 private Input _input;
		 private bool _successful;

		 // This map contains additional state that gets populated, created and used throughout the stages.
		 // The reason that this is a map is to allow for a uniform way of accessing and loading this stage
		 // from the outside. Currently these things live here:
		 //   - RelationshipTypeDistribution
		 private readonly IDictionary<Type, object> _accessibleState = new Dictionary<Type, object>();

		 // components which may get assigned and unassigned in some methods
		 private NodeRelationshipCache _nodeRelationshipCache;
		 private NodeLabelsCache _nodeLabelsCache;
		 private long _startTime;
		 private NumberArrayFactory _numberArrayFactory;
		 private Collector _badCollector;
		 private IdMapper _idMapper;
		 private long _peakMemoryUsage;
		 private long _availableMemoryForLinking;

		 /// <param name="storeDir"> directory which the db will be created in. </param>
		 /// <param name="fileSystem"> <seealso cref="FileSystemAbstraction"/> that the {@code storeDir} lives in. </param>
		 /// <param name="neoStore"> <seealso cref="BatchingNeoStores"/> to import into. </param>
		 /// <param name="config"> import-specific <seealso cref="Configuration"/>. </param>
		 /// <param name="logService"> <seealso cref="LogService"/> to use. </param>
		 /// <param name="executionMonitor"> <seealso cref="ExecutionMonitor"/> to follow progress as the import proceeds. </param>
		 /// <param name="recordFormats"> which <seealso cref="RecordFormats record format"/> to use for the created db. </param>
		 /// <param name="monitor"> <seealso cref="Monitor"/> for some events. </param>
		 public ImportLogic( File storeDir, FileSystemAbstraction fileSystem, BatchingNeoStores neoStore, Configuration config, LogService logService, ExecutionMonitor executionMonitor, RecordFormats recordFormats, Monitor monitor )
		 {
			  this._storeDir = storeDir;
			  this._fileSystem = fileSystem;
			  this._neoStore = neoStore;
			  this._config = config;
			  this._recordFormats = recordFormats;
			  this._monitor = monitor;
			  this._log = logService.InternalLogProvider.getLog( this.GetType() );
			  this._executionMonitor = ExecutionSupervisors.withDynamicProcessorAssignment( executionMonitor, config );
			  this._maxMemory = config.MaxMemoryUsage();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initialize(org.neo4j.unsafe.impl.batchimport.input.Input input) throws java.io.IOException
		 public virtual void Initialize( Input input )
		 {
			  _log.info( "Import starting" );
			  _startTime = currentTimeMillis();
			  this._input = input;
			  PageCacheArrayFactoryMonitor numberArrayFactoryMonitor = new PageCacheArrayFactoryMonitor();
			  _numberArrayFactory = auto( _neoStore.PageCache, _storeDir, _config.allowCacheAllocationOnHeap(), numberArrayFactoryMonitor );
			  _badCollector = input.BadCollector();
			  // Some temporary caches and indexes in the import
			  _idMapper = input.IdMapper( _numberArrayFactory );
			  _nodeRelationshipCache = new NodeRelationshipCache( _numberArrayFactory, _config.denseNodeThreshold() );
			  Input_Estimates inputEstimates = input.CalculateEstimates( _neoStore.PropertyStore.newValueEncodedSizeCalculator() );

			  // Sanity checking against estimates
			  ( new EstimationSanityChecker( _recordFormats, _monitor ) ).sanityCheck( inputEstimates );
			  ( new HeapSizeSanityChecker( _monitor ) ).sanityCheck( inputEstimates, _recordFormats, _neoStore, _nodeRelationshipCache.memoryEstimation( inputEstimates.NumberOfNodes() ), _idMapper.memoryEstimation(inputEstimates.NumberOfNodes()) );

			  _dependencies.satisfyDependencies( inputEstimates, _idMapper, _neoStore, _nodeRelationshipCache, numberArrayFactoryMonitor );

			  if ( _neoStore.determineDoubleRelationshipRecordUnits( inputEstimates ) )
			  {
					_monitor.doubleRelationshipRecordUnitsEnabled();
			  }

			  _executionMonitor.initialize( _dependencies );
		 }

		 /// <summary>
		 /// Accesses state of a certain {@code type}. This is state that may be long- or short-lived and perhaps
		 /// created in one part of the import to be used in another.
		 /// </summary>
		 /// <param name="type"> <seealso cref="System.Type"/> of the state to get. </param>
		 /// <returns> the state of the given type. </returns>
		 /// <exception cref="IllegalStateException"> if the state of the given {@code type} isn't available. </exception>
		 public virtual T GetState<T>( Type type )
		 {
				 type = typeof( T );
			  return type.cast( _accessibleState[type] );
		 }

		 /// <summary>
		 /// Puts state of a certain type.
		 /// </summary>
		 /// <param name="state"> state instance to set. </param>
		 /// <seealso cref= #getState(Class) </seealso>
		 /// <exception cref="IllegalStateException"> if state of this type has already been defined. </exception>
		 public virtual void PutState<T>( T state )
		 {
			  _accessibleState[state.GetType()] = state;
			  _dependencies.satisfyDependency( state );
		 }

		 /// <summary>
		 /// Imports nodes w/ their properties and labels from <seealso cref="Input.nodes()"/>. This will as a side-effect populate the <seealso cref="IdMapper"/>,
		 /// to later be used for looking up ID --> nodeId in <seealso cref="importRelationships()"/>. After a completed node import,
		 /// <seealso cref="prepareIdMapper()"/> must be called.
		 /// </summary>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void importNodes() throws java.io.IOException
		 public virtual void ImportNodes()
		 {
			  // Import nodes, properties, labels
			  _neoStore.startFlushingPageCache();
			  DataImporter.ImportNodes( _config.maxNumberOfProcessors(), _input, _neoStore, _idMapper, _executionMonitor, _storeUpdateMonitor );
			  _neoStore.stopFlushingPageCache();
			  UpdatePeakMemoryUsage();
		 }

		 /// <summary>
		 /// Prepares <seealso cref="IdMapper"/> to be queried for ID --> nodeId lookups. This is required for running <seealso cref="importRelationships()"/>.
		 /// </summary>
		 public virtual void PrepareIdMapper()
		 {
			  if ( _idMapper.needsPreparation() )
			  {
					MemoryUsageStatsProvider memoryUsageStats = new MemoryUsageStatsProvider( _neoStore, _idMapper );
					System.Func<long, object> inputIdLookup = new NodeInputIdPropertyLookup( _neoStore.TemporaryPropertyStore );
					ExecuteStage( new IdMapperPreparationStage( _config, _idMapper, inputIdLookup, _badCollector, memoryUsageStats ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator duplicateNodeIds = idMapper.leftOverDuplicateNodesIds();
					LongIterator duplicateNodeIds = _idMapper.leftOverDuplicateNodesIds();
					if ( duplicateNodeIds.hasNext() )
					{
						 ExecuteStage( new DeleteDuplicateNodesStage( _config, duplicateNodeIds, _neoStore, _storeUpdateMonitor ) );
					}
					UpdatePeakMemoryUsage();
			  }
		 }

		 /// <summary>
		 /// Uses <seealso cref="IdMapper"/> as lookup for ID --> nodeId and imports all relationships from <seealso cref="Input.relationships()"/>
		 /// and writes them into the <seealso cref="RelationshipStore"/>. No linking between relationships is done in this method,
		 /// it's done later in <seealso cref="linkRelationships(int)"/>.
		 /// </summary>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void importRelationships() throws java.io.IOException
		 public virtual void ImportRelationships()
		 {
			  // Import relationships (unlinked), properties
			  _neoStore.startFlushingPageCache();
			  DataStatistics typeDistribution = DataImporter.ImportRelationships( _config.maxNumberOfProcessors(), _input, _neoStore, _idMapper, _badCollector, _executionMonitor, _storeUpdateMonitor, !_badCollector.CollectingBadRelationships );
			  _neoStore.stopFlushingPageCache();
			  UpdatePeakMemoryUsage();
			  _idMapper.close();
			  _idMapper = null;
			  PutState( typeDistribution );
		 }

		 /// <summary>
		 /// Populates <seealso cref="NodeRelationshipCache"/> with node degrees, which is required to know how to physically layout each
		 /// relationship chain. This is required before running <seealso cref="linkRelationships(int)"/>.
		 /// </summary>
		 public virtual void CalculateNodeDegrees()
		 {
			  Configuration relationshipConfig = ConfigWithRecordsPerPageBasedBatchSize( _config, _neoStore.RelationshipStore );
			  _nodeRelationshipCache.NodeCount = _neoStore.NodeStore.HighId;
			  MemoryUsageStatsProvider memoryUsageStats = new MemoryUsageStatsProvider( _neoStore, _nodeRelationshipCache );
			  NodeDegreeCountStage nodeDegreeStage = new NodeDegreeCountStage( relationshipConfig, _neoStore.RelationshipStore, _nodeRelationshipCache, memoryUsageStats );
			  ExecuteStage( nodeDegreeStage );
			  _nodeRelationshipCache.countingCompleted();
			  _availableMemoryForLinking = _maxMemory - TotalMemoryUsageOf( _nodeRelationshipCache, _neoStore );
		 }

		 /// <summary>
		 /// Performs one round of linking together relationships with each other. Number of rounds required
		 /// is dictated by available memory. The more dense nodes and relationship types, the more memory required.
		 /// Every round all relationships of one or more types are linked.
		 /// 
		 /// Links together:
		 /// <ul>
		 /// <li>
		 /// Relationship <--> Relationship. Two sequential passes are made over the relationship store.
		 /// The forward pass links next pointers, each next pointer pointing "backwards" to lower id.
		 /// The backward pass links prev pointers, each prev pointer pointing "forwards" to higher id.
		 /// </li>
		 /// Sparse Node --> Relationship. Sparse nodes are updated with relationship heads of completed chains.
		 /// This is done in the first round only, if there are multiple rounds.
		 /// </li>
		 /// </ul>
		 /// 
		 /// A linking loop (from external caller POV) typically looks like:
		 /// <pre>
		 /// int type = 0;
		 /// do
		 /// {
		 ///    type = logic.linkRelationships( type );
		 /// }
		 /// while ( type != -1 );
		 /// </pre>
		 /// </summary>
		 /// <param name="startingFromType"> relationship type to start from. </param>
		 /// <returns> the next relationship type to start linking and, if != -1, should be passed into next call to this method. </returns>
		 public virtual int LinkRelationships( int startingFromType )
		 {
			  Debug.Assert( startingFromType >= 0, startingFromType );

			  // Link relationships together with each other, their nodes and their relationship groups
			  DataStatistics relationshipTypeDistribution = GetState( typeof( DataStatistics ) );
			  MemoryUsageStatsProvider memoryUsageStats = new MemoryUsageStatsProvider( _neoStore, _nodeRelationshipCache );

			  // Figure out which types we can fit in node-->relationship cache memory.
			  // Types go from biggest to smallest group and so towards the end there will be
			  // smaller and more groups per round in this loop
			  int upToType = NextSetOfTypesThatFitInMemory( relationshipTypeDistribution, startingFromType, _availableMemoryForLinking, _nodeRelationshipCache.NumberOfDenseNodes );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.IntSet typesToLinkThisRound = relationshipTypeDistribution.types(startingFromType, upToType);
			  IntSet typesToLinkThisRound = relationshipTypeDistribution.Types( startingFromType, upToType );
			  int typesImported = typesToLinkThisRound.size();
			  bool thisIsTheFirstRound = startingFromType == 0;
			  bool thisIsTheOnlyRound = thisIsTheFirstRound && upToType == relationshipTypeDistribution.NumberOfRelationshipTypes;

			  Configuration relationshipConfig = ConfigWithRecordsPerPageBasedBatchSize( _config, _neoStore.RelationshipStore );
			  Configuration nodeConfig = ConfigWithRecordsPerPageBasedBatchSize( _config, _neoStore.NodeStore );
			  Configuration groupConfig = ConfigWithRecordsPerPageBasedBatchSize( _config, _neoStore.RelationshipGroupStore );

			  _nodeRelationshipCache.setForwardScan( true, true );
			  string range = typesToLinkThisRound.size() == 1 ? OneBased(startingFromType).ToString() : OneBased(startingFromType) + "-" + (startingFromType + typesImported);
			  string topic = " " + range + "/" + relationshipTypeDistribution.NumberOfRelationshipTypes;
			  int nodeTypes = thisIsTheFirstRound ? NodeType.NODE_TYPE_ALL : NodeType.NODE_TYPE_DENSE;
			  System.Predicate<RelationshipRecord> readFilter = thisIsTheFirstRound ? alwaysTrue() : record => typesToLinkThisRound.contains(record.Type);
			  System.Predicate<RelationshipRecord> denseChangeFilter = thisIsTheOnlyRound ? alwaysTrue() : record => typesToLinkThisRound.contains(record.Type);

			  // LINK Forward
			  RelationshipLinkforwardStage linkForwardStage = new RelationshipLinkforwardStage( topic, relationshipConfig, _neoStore, _nodeRelationshipCache, readFilter, denseChangeFilter, nodeTypes, new RelationshipLinkingProgress(), memoryUsageStats );
			  ExecuteStage( linkForwardStage );

			  // Write relationship groups cached from the relationship import above
			  ExecuteStage( new RelationshipGroupStage( topic, groupConfig, _neoStore.TemporaryRelationshipGroupStore, _nodeRelationshipCache ) );
			  if ( thisIsTheFirstRound )
			  {
					// Set node nextRel fields for sparse nodes
					ExecuteStage( new SparseNodeFirstRelationshipStage( nodeConfig, _neoStore.NodeStore, _nodeRelationshipCache ) );
			  }

			  // LINK backward
			  _nodeRelationshipCache.setForwardScan( false, true );
			  ExecuteStage( new RelationshipLinkbackStage( topic, relationshipConfig, _neoStore, _nodeRelationshipCache, readFilter, denseChangeFilter, nodeTypes, new RelationshipLinkingProgress(), memoryUsageStats ) );

			  UpdatePeakMemoryUsage();

			  if ( upToType == relationshipTypeDistribution.NumberOfRelationshipTypes )
			  {
					// This means that we've linked all the types
					_nodeRelationshipCache.close();
					_nodeRelationshipCache = null;
					return -1;
			  }

			  return upToType;
		 }

		 /// <summary>
		 /// Links relationships of all types, potentially doing multiple passes, each pass calling <seealso cref="linkRelationships(int)"/>
		 /// with a type range.
		 /// </summary>
		 public virtual void LinkRelationshipsOfAllTypes()
		 {
			  int type = 0;
			  do
			  {
					type = LinkRelationships( type );
			  } while ( type != -1 );
		 }

		 /// <summary>
		 /// Convenience method (for code reading) to have a zero-based value become one based (for printing/logging).
		 /// </summary>
		 private static int OneBased( int value )
		 {
			  return value + 1;
		 }

		 /// <returns> index (into <seealso cref="DataStatistics"/>) of last relationship type that fit in memory this round. </returns>
		 internal static int NextSetOfTypesThatFitInMemory( DataStatistics typeDistribution, int startingFromType, long freeMemoryForDenseNodeCache, long numberOfDenseNodes )
		 {
			  Debug.Assert( startingFromType >= 0, startingFromType );

			  long currentSetOfRelationshipsMemoryUsage = 0;
			  int numberOfTypes = typeDistribution.NumberOfRelationshipTypes;
			  int toType = startingFromType;
			  for ( ; toType < numberOfTypes; toType++ )
			  {
					// Calculate worst-case scenario
					RelationshipTypeCount type = typeDistribution.Get( toType );
					long relationshipCountForThisType = type.Count;
					long memoryUsageForThisType = calculateMaxMemoryUsage( numberOfDenseNodes, relationshipCountForThisType );
					long memoryUsageUpToAndIncludingThisType = currentSetOfRelationshipsMemoryUsage + memoryUsageForThisType;
					if ( memoryUsageUpToAndIncludingThisType > freeMemoryForDenseNodeCache && currentSetOfRelationshipsMemoryUsage > 0 )
					{
						 // OK the current set of types is enough to fill the cache
						 break;
					}

					currentSetOfRelationshipsMemoryUsage += memoryUsageForThisType;
			  }
			  return toType;
		 }

		 /// <summary>
		 /// Optimizes the relationship groups store by physically locating groups for each node together.
		 /// </summary>
		 public virtual void DefragmentRelationshipGroups()
		 {
			  // Defragment relationships groups for better performance
			  ( new RelationshipGroupDefragmenter( _config, _executionMonitor, RelationshipGroupDefragmenter.Monitor.EMPTY, _numberArrayFactory ) ).run( max( _maxMemory, _peakMemoryUsage ), _neoStore, _neoStore.NodeStore.HighId );
		 }

		 /// <summary>
		 /// Builds the counts store. Requires that <seealso cref="importNodes()"/> and <seealso cref="importRelationships()"/> has run.
		 /// </summary>
		 public virtual void BuildCountsStore()
		 {
			  // Count nodes per label and labels per node
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater = _neoStore.CountsStore.reset( _neoStore.LastCommittedTransactionId ) )
			  {
					MigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
					_nodeLabelsCache = new NodeLabelsCache( _numberArrayFactory, _neoStore.LabelRepository.HighId );
					MemoryUsageStatsProvider memoryUsageStats = new MemoryUsageStatsProvider( _neoStore, _nodeLabelsCache );
					ExecuteStage( new NodeCountsAndLabelIndexBuildStage( _config, _nodeLabelsCache, _neoStore.NodeStore, _neoStore.LabelRepository.HighId, countsUpdater, progressMonitor.StartSection( "Nodes" ), _neoStore.LabelScanStore, memoryUsageStats ) );
					// Count label-[type]->label
					ExecuteStage( new RelationshipCountsStage( _config, _nodeLabelsCache, _neoStore.RelationshipStore, _neoStore.LabelRepository.HighId, _neoStore.RelationshipTypeRepository.HighId, countsUpdater, _numberArrayFactory, progressMonitor.StartSection( "Relationships" ) ) );
			  }
		 }

		 public virtual void Success()
		 {
			  _neoStore.success();
			  _successful = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  // We're done, do some final logging about it
			  long totalTimeMillis = currentTimeMillis() - _startTime;
			  DataStatistics state = GetState( typeof( DataStatistics ) );
			  string additionalInformation = Objects.ToString( state, "Data statistics is not available." );
			  _executionMonitor.done( _successful, totalTimeMillis, format( "%n%s%nPeak memory usage: %s", additionalInformation, bytes( _peakMemoryUsage ) ) );
			  _log.info( "Import completed successfully, took " + duration( totalTimeMillis ) + ". " + additionalInformation );
			  closeAll( _nodeRelationshipCache, _nodeLabelsCache, _idMapper );
		 }

		 private void UpdatePeakMemoryUsage()
		 {
			  _peakMemoryUsage = max( _peakMemoryUsage, TotalMemoryUsageOf( _nodeRelationshipCache, _idMapper, _neoStore ) );
		 }

		 public static BatchingNeoStores InstantiateNeoStores( FileSystemAbstraction fileSystem, File storeDir, PageCache externalPageCache, RecordFormats recordFormats, Configuration config, LogService logService, AdditionalInitialIds additionalInitialIds, Config dbConfig, JobScheduler scheduler )
		 {
			  if ( externalPageCache == null )
			  {
					return BatchingNeoStores.batchingNeoStores( fileSystem, storeDir, recordFormats, config, logService, additionalInitialIds, dbConfig, scheduler );
			  }

			  return BatchingNeoStores.batchingNeoStoresWithExternalPageCache( fileSystem, externalPageCache, PageCacheTracer.NULL, storeDir, recordFormats, config, logService, additionalInitialIds, dbConfig );
		 }

		 private static long TotalMemoryUsageOf( params Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] users )
		 {
			  GatheringMemoryStatsVisitor total = new GatheringMemoryStatsVisitor();
			  foreach ( Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable user in users )
			  {
					if ( user != null )
					{
						 user.AcceptMemoryStatsVisitor( total );
					}
			  }
			  return total.HeapUsage + total.OffHeapUsage;
		 }

		 private static Configuration ConfigWithRecordsPerPageBasedBatchSize<T1>( Configuration source, RecordStore<T1> store )
		 {
			  return Configuration.withBatchSize( source, store.RecordsPerPage * 10 );
		 }

		 private void ExecuteStage( Stage stage )
		 {
			  superviseExecution( _executionMonitor, stage );
		 }
	}

}