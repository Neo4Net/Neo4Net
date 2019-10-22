using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using LongObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Functions;
	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using MisconfiguredIndexException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MisconfiguredIndexException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using IndexSamplingController = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingController;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using Registers = Neo4Net.Register.Registers;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.IndexPopulationFailure.failure;

	/// <summary>
	/// Manages the indexes that were introduced in 2.0. These indexes depend on the normal Neo4Net logical log for
	/// transactionality. Each index has an <seealso cref="StoreIndexDescriptor"/>, which it uses to filter
	/// changes that come into the database. Changes that apply to the the rule are indexed. This way, "normal" changes to
	/// the database can be replayed to perform recovery after a crash.
	/// <para>
	/// <h3>Recovery procedure</h3>
	/// </para>
	/// <para>
	/// Each index has a state, as defined in <seealso cref="InternalIndexState"/>, which is used during
	/// recovery. If an index is anything but <seealso cref="InternalIndexState.ONLINE"/>, it will simply be
	/// destroyed and re-created.
	/// </para>
	/// <para>
	/// If, however, it is <seealso cref="InternalIndexState.ONLINE"/>, the index provider is required to
	/// also guarantee that the index had been flushed to disk.
	/// </para>
	/// </summary>
	public class IndexingService : LifecycleAdapter, IndexingUpdateService, IndexingProvidersService
	{
		 private readonly IndexSamplingController _samplingController;
		 private readonly IndexProxyCreator _indexProxyCreator;
		 private readonly IndexStoreView _storeView;
		 private readonly IndexProviderMap _providerMap;
		 private readonly IndexMapReference _indexMapRef;
		 private readonly IEnumerable<SchemaRule> _schemaRules;
		 private readonly Log _internalLog;
		 private readonly Log _userLog;
		 private readonly bool _readOnly;
		 private readonly TokenNameLookup _tokenNameLookup;
		 private readonly MultiPopulatorFactory _multiPopulatorFactory;
		 private readonly LogProvider _internalLogProvider;
		 private readonly Monitor _monitor;
		 private readonly SchemaState _schemaState;
		 private readonly IndexPopulationJobController _populationJobController;
		 private readonly IDictionary<long, IndexProxy> _indexesToDropAfterCompletedRecovery = new Dictionary<long, IndexProxy>();

		 internal enum State
		 {
			  NotStarted,
			  Starting,
			  Running,
			  Stopped
		 }

		 public interface Monitor
		 {
			  void InitialState( StoreIndexDescriptor descriptor, InternalIndexState state );

			  void PopulationCompleteOn( StoreIndexDescriptor descriptor );

			  void IndexPopulationScanStarting();

			  void IndexPopulationScanComplete();

			  void AwaitingPopulationOfRecoveredIndex( StoreIndexDescriptor descriptor );

			  void PopulationCancelled();

			  void PopulationJobCompleted( long peakDirectMemoryUsage );
		 }

		 public class MonitorAdapter : Monitor
		 {
			  public override void InitialState( StoreIndexDescriptor descriptor, InternalIndexState state )
			  { // Do nothing
			  }

			  public override void PopulationCompleteOn( StoreIndexDescriptor descriptor )
			  { // Do nothing
			  }

			  public override void IndexPopulationScanStarting()
			  { // Do nothing
			  }

			  public override void IndexPopulationScanComplete()
			  { // Do nothing
			  }

			  public override void AwaitingPopulationOfRecoveredIndex( StoreIndexDescriptor descriptor )
			  { // Do nothing
			  }

			  public override void PopulationCancelled()
			  { // Do nothing
			  }

			  public virtual void PopulationJobCompleted( long peakDirectMemoryUsage )
			  { // Do nothing
			  }
		 }

		 public static readonly Monitor NoMonitor = new MonitorAdapter();

		 private volatile State _state = State.NotStarted;

		 internal IndexingService( IndexProxyCreator indexProxyCreator, IndexProviderMap providerMap, IndexMapReference indexMapRef, IndexStoreView storeView, IEnumerable<SchemaRule> schemaRules, IndexSamplingController samplingController, TokenNameLookup tokenNameLookup, IJobScheduler scheduler, SchemaState schemaState, MultiPopulatorFactory multiPopulatorFactory, LogProvider internalLogProvider, LogProvider userLogProvider, Monitor monitor, bool readOnly )
		 {
			  this._indexProxyCreator = indexProxyCreator;
			  this._providerMap = providerMap;
			  this._indexMapRef = indexMapRef;
			  this._storeView = storeView;
			  this._schemaRules = schemaRules;
			  this._samplingController = samplingController;
			  this._tokenNameLookup = tokenNameLookup;
			  this._schemaState = schemaState;
			  this._multiPopulatorFactory = multiPopulatorFactory;
			  this._internalLogProvider = internalLogProvider;
			  this._monitor = monitor;
			  this._populationJobController = new IndexPopulationJobController( scheduler );
			  this._internalLog = internalLogProvider.getLog( this.GetType() );
			  this._userLog = userLogProvider.getLog( this.GetType() );
			  this._readOnly = readOnly;
		 }

		 /// <summary>
		 /// Called while the database starts up, before recovery.
		 /// </summary>
		 public override void Init()
		 {
			  ValidateDefaultProviderExisting();

			  _indexMapRef.modify(indexMap =>
			  {
				IDictionary<InternalIndexState, IList<IndexLogRecord>> indexStates = new Dictionary<InternalIndexState, IList<IndexLogRecord>>( typeof( InternalIndexState ) );
				foreach ( SchemaRule rule in _schemaRules )
				{
					 if ( rule is ConstraintRule )
					 {
						  ConstraintRule constraintRule = ( ConstraintRule ) rule;
						  if ( constraintRule.ConstraintDescriptor.enforcesUniqueness() )
						  {
								indexMap.putUniquenessConstraint( constraintRule );
						  }
						  continue;
					 }

					 StoreIndexDescriptor indexDescriptor = ( StoreIndexDescriptor ) rule;
					 IndexProxy indexProxy;

					 IndexProviderDescriptor providerDescriptor = indexDescriptor.providerDescriptor();
					 IndexProvider provider = _providerMap.lookup( providerDescriptor );
					 InternalIndexState initialState = provider.getInitialState( indexDescriptor );
					 indexStates.computeIfAbsent( initialState, internalIndexState => new List<>() ).add(new IndexLogRecord(indexDescriptor));

					 _internalLog.debug( IndexStateInfo( "init", initialState, indexDescriptor ) );
					 switch ( initialState )
					 {
					 case ONLINE:
						  _monitor.initialState( indexDescriptor, ONLINE );
						  indexProxy = _indexProxyCreator.createOnlineIndexProxy( indexDescriptor );
						  break;
					 case POPULATING:
						  // The database was shut down during population, or a crash has occurred, or some other sad thing.
						  _monitor.initialState( indexDescriptor, POPULATING );
						  indexProxy = _indexProxyCreator.createRecoveringIndexProxy( indexDescriptor );
						  break;
					 case FAILED:
						  _monitor.initialState( indexDescriptor, FAILED );
						  IndexPopulationFailure failure = failure( provider.getPopulationFailure( indexDescriptor ) );
						  indexProxy = _indexProxyCreator.createFailedIndexProxy( indexDescriptor, failure );
						  break;
					 default:
						  throw new System.ArgumentException( "" + initialState );
					 }
					 indexMap.putIndexProxy( indexProxy );
				}
				LogIndexStateSummary( "init", indexStates );
				return indexMap;
			  });
		 }

		 private void ValidateDefaultProviderExisting()
		 {
			  if ( _providerMap == null || _providerMap.DefaultProvider == null )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.InvalidOperationException( "You cannot run the database without an index provider, " + "please make sure that a valid provider (subclass of " + typeof( IndexProvider ).FullName + ") is on your classpath." );
			  }
		 }

		 // Recovery semantics: This is to be called after init, and after the database has run recovery.
		 public override void Start()
		 {
			  _state = State.Starting;

			  // During recovery there could have been dropped indexes. Dropping an index means also updating the counts store,
			  // which is problematic during recovery. So instead drop those indexes here, after recovery completed.
			  PerformRecoveredIndexDropActions();

			  // Recovery will not do refresh (update read views) while applying recovered transactions and instead
			  // do it at one point after recovery... i.e. here
			  _indexMapRef.indexMapSnapshot().forEachIndexProxy(IndexProxyOperation("refresh", IndexProxy.refresh));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor> rebuildingDescriptors = new org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap<>();
			  MutableLongObjectMap<StoreIndexDescriptor> rebuildingDescriptors = new LongObjectHashMap<StoreIndexDescriptor>();
			  _indexMapRef.modify(indexMap =>
			  {
				IDictionary<InternalIndexState, IList<IndexLogRecord>> indexStates = new Dictionary<InternalIndexState, IList<IndexLogRecord>>( typeof( InternalIndexState ) );
				IDictionary<IndexProviderDescriptor, IList<IndexLogRecord>> indexProviders = new Dictionary<IndexProviderDescriptor, IList<IndexLogRecord>>();

				// Find all indexes that are not already online, do not require rebuilding, and create them
				indexMap.forEachIndexProxy((indexId, proxy) =>
				{
					 InternalIndexState state = proxy.State;
					 StoreIndexDescriptor descriptor = proxy.Descriptor;
					 IndexProviderDescriptor providerDescriptor = descriptor.providerDescriptor();
					 IndexLogRecord indexLogRecord = new IndexLogRecord( descriptor );
					 indexStates.computeIfAbsent( state, internalIndexState => new List<>() ).add(indexLogRecord);
					 indexProviders.computeIfAbsent( providerDescriptor, indexProviderDescriptor => new List<>() ).add(indexLogRecord);
					 _internalLog.debug( IndexStateInfo( "start", state, descriptor ) );
					 switch ( state )
					 {
					 case ONLINE:
						  // Don't do anything, index is ok.
						  break;
					 case POPULATING:
						  // Remember for rebuilding
						  rebuildingDescriptors.put( indexId, descriptor );
						  break;
					 case FAILED:
						  // Don't do anything, the user needs to drop the index and re-create
						  break;
					 default:
						  throw new System.InvalidOperationException( "Unknown state: " + state );
					 }
				});
				LogIndexStateSummary( "start", indexStates );
				LogIndexProviderSummary( indexProviders );

				DontRebuildIndexesInReadOnlyMode( rebuildingDescriptors );
				// Drop placeholder proxies for indexes that need to be rebuilt
				DropRecoveringIndexes( indexMap, rebuildingDescriptors.Keys );
				// Rebuild indexes by recreating and repopulating them
				PopulateIndexesOfAllTypes( rebuildingDescriptors, indexMap );

				return indexMap;
			  });

			  _samplingController.recoverIndexSamples();
			  _samplingController.start();

			  // So at this point we've started population of indexes that needs to be rebuilt in the background.
			  // Indexes backing uniqueness constraints are normally built within the transaction creating the constraint
			  // and so we shouldn't leave such indexes in a populating state after recovery.
			  // This is why we now go and wait for those indexes to be fully populated.
			  rebuildingDescriptors.forEachKeyValue((indexId, descriptor) =>
			  {
						  if ( descriptor.type() != IndexDescriptor.Type.UNIQUE )
						  {
								// It's not a uniqueness constraint, so don't wait for it to be rebuilt
								return;
						  }

						  IndexProxy proxy;
						  try
						  {
								proxy = getIndexProxy( indexId );
						  }
						  catch ( IndexNotFoundKernelException e )
						  {
								throw new System.InvalidOperationException( "What? This index was seen during recovery just now, why isn't it available now?", e );
						  }

						  if ( proxy.Descriptor.OwningConstraint == null )
						  {
								// Even though this is an index backing a uniqueness constraint, the uniqueness constraint wasn't created
								// so there's no gain in waiting for this index.
								return;
						  }

						  _monitor.awaitingPopulationOfRecoveredIndex( descriptor );
						  AwaitOnline( proxy );
			  });

			  _state = State.Running;
		 }

		 private void DontRebuildIndexesInReadOnlyMode( MutableLongObjectMap<StoreIndexDescriptor> rebuildingDescriptors )
		 {
			  if ( _readOnly && rebuildingDescriptors.notEmpty() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					string indexString = rebuildingDescriptors.values().Select(string.valueOf).collect(Collectors.joining(", ", "{", "}"));
					throw new System.InvalidOperationException( "Some indexes need to be rebuilt. This is not allowed in read only mode. Please start db in writable mode to rebuild indexes. " + "Indexes needing rebuild: " + indexString );
			  }
		 }

		 private void PopulateIndexesOfAllTypes( MutableLongObjectMap<StoreIndexDescriptor> rebuildingDescriptors, IndexMap indexMap )
		 {
			  IDictionary<EntityType, MutableLongObjectMap<StoreIndexDescriptor>> rebuildingDescriptorsByType = new Dictionary<EntityType, MutableLongObjectMap<StoreIndexDescriptor>>( typeof( IEntityType ) );
			  foreach ( StoreIndexDescriptor descriptor in rebuildingDescriptors )
			  {
					rebuildingDescriptorsByType.computeIfAbsent( descriptor.Schema().entityType(), type => new LongObjectHashMap<>() ).put(descriptor.Id, descriptor);
			  }

			  foreach ( KeyValuePair<EntityType, MutableLongObjectMap<StoreIndexDescriptor>> descriptorToPopulate in rebuildingDescriptorsByType.SetOfKeyValuePairs() )
			  {
					IndexPopulationJob populationJob = NewIndexPopulationJob( descriptorToPopulate.Key, false );
					Populate( descriptorToPopulate.Value, indexMap, populationJob );
			  }
		 }

		 private void PerformRecoveredIndexDropActions()
		 {
			  _indexesToDropAfterCompletedRecovery.Values.forEach(index =>
			  {
				try
				{
					 index.drop();
				}
				catch ( Exception )
				{
					 // This is OK to get during recovery because the underlying index can be in any unknown state
					 // while we're recovering. Let's just move on to closing it instead.
					 try
					 {
						  index.close();
					 }
					 catch ( IOException )
					 {
						  // This is OK for the same reason as above
					 }
				}
			  });
			  _indexesToDropAfterCompletedRecovery.Clear();
		 }

		 private void Populate( MutableLongObjectMap<StoreIndexDescriptor> rebuildingDescriptors, IndexMap indexMap, IndexPopulationJob populationJob )
		 {
			  rebuildingDescriptors.forEachKeyValue((indexId, descriptor) =>
			  {
				IndexProxy proxy = _indexProxyCreator.createPopulatingIndexProxy( descriptor, false, _monitor, populationJob );
				proxy.Start();
				indexMap.PutIndexProxy( proxy );
			  });
			  StartIndexPopulation( populationJob );
		 }

		 /// <summary>
		 /// Polls the <seealso cref="IndexProxy.getState() state of the index"/> and waits for it to be either
		 /// <seealso cref="InternalIndexState.ONLINE"/>, in which case the wait is over, or <seealso cref="InternalIndexState.FAILED"/>,
		 /// in which an exception is thrown.
		 /// </summary>
		 private void AwaitOnline( IndexProxy proxy )
		 {
			  while ( true )
			  {
					switch ( proxy.State )
					{
					case ONLINE:
						 return;
					case FAILED:
						 IndexPopulationFailure populationFailure = proxy.PopulationFailure;
						 string message = string.Format( "Index {0} entered {1} state while recovery waited for it to be fully populated.", proxy.Descriptor, FAILED );
						 string causeOfFailure = populationFailure.AsString();
						 throw new System.InvalidOperationException( IndexPopulationFailure.AppendCauseOfFailure( message, causeOfFailure ) );
					case POPULATING:
						 // Sleep a short while and look at state again the next loop iteration
						 try
						 {
							  Thread.Sleep( 10 );
						 }
						 catch ( InterruptedException e )
						 {
							  throw new System.InvalidOperationException( "Waiting for index to become ONLINE was interrupted", e );
						 }
						 break;
					default:
						 throw new System.InvalidOperationException( proxy.State.name() );
					}
			  }
		 }

		 // We need to stop indexing service on shutdown since we can have transactions that are ongoing/finishing
		 // after we start stopping components and those transactions should be able to finish successfully
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.util.concurrent.ExecutionException, InterruptedException
		 public override void Shutdown()
		 {
			  _state = State.Stopped;
			  _samplingController.stop();
			  _populationJobController.stop();
			  CloseAllIndexes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.register.Register_DoubleLongRegister indexUpdatesAndSize(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual Register_DoubleLongRegister IndexUpdatesAndSize( SchemaDescriptor descriptor )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long indexId = indexMapRef.getOnlineIndexId(descriptor);
			  long indexId = _indexMapRef.getOnlineIndexId( descriptor );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.register.Register_DoubleLongRegister output = org.Neo4Net.register.Registers.newDoubleLongRegister();
			  Register_DoubleLongRegister output = Registers.newDoubleLongRegister();
			  _storeView.indexUpdatesAndSize( indexId, output );
			  return output;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double indexUniqueValuesPercentage(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual double IndexUniqueValuesPercentage( SchemaDescriptor descriptor )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long indexId = indexMapRef.getOnlineIndexId(descriptor);
			  long indexId = _indexMapRef.getOnlineIndexId( descriptor );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.register.Register_DoubleLongRegister output = org.Neo4Net.register.Registers.newDoubleLongRegister();
			  Register_DoubleLongRegister output = Registers.newDoubleLongRegister();
			  _storeView.indexSample( indexId, output );
			  long unique = output.ReadFirst();
			  long size = output.ReadSecond();
			  if ( size == 0 )
			  {
					return 1.0d;
			  }
			  else
			  {
					return ( ( double ) unique ) / ( ( double ) size );
			  }
		 }

		 public override void ValidateBeforeCommit( SchemaDescriptor index, Value[] tuple )
		 {
			  _indexMapRef.validateBeforeCommit( index, tuple );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.storageengine.api.schema.IndexDescriptor getBlessedDescriptorFromProvider(org.Neo4Net.storageengine.api.schema.IndexDescriptor index) throws org.Neo4Net.internal.kernel.api.exceptions.schema.MisconfiguredIndexException
		 public override IndexDescriptor GetBlessedDescriptorFromProvider( IndexDescriptor index )
		 {
			  IndexProvider provider = _providerMap.lookup( index.ProviderDescriptor() );
			  return provider.Bless( index );
		 }

		 public override IndexProviderDescriptor IndexProviderByName( string providerName )
		 {
			  return _providerMap.lookup( providerName ).ProviderDescriptor;
		 }

		 /// <summary>
		 /// Applies updates from the given <seealso cref="IndexUpdates"/>, which may contain updates for one or more indexes.
		 /// As long as index updates are derived from physical commands and store state there's special treatment
		 /// during recovery since we cannot read from an unrecovered store, so in that case the nodes ids are simply
		 /// noted and reindexed after recovery of the store has completed. That is also why <seealso cref="IndexUpdates"/>
		 /// has one additional accessor method for getting the node ids.
		 /// 
		 /// As far as <seealso cref="IndexingService"/> is concerned recovery happens between calls to <seealso cref="init()"/> and
		 /// <seealso cref="start()"/>.
		 /// </summary>
		 /// <param name="updates"> <seealso cref="IndexUpdates"/> to apply. </param>
		 /// <exception cref="UncheckedIOException"> potentially thrown from index updating. </exception>
		 /// <exception cref="IndexEntryConflictException"> potentially thrown from index updating. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(org.Neo4Net.kernel.impl.transaction.state.IndexUpdates updates) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Apply( IndexUpdates updates )
		 {
			  if ( _state == State.NotStarted )
			  {
					// We're in recovery, which means we'll be telling indexes to apply with additional care for making
					// idempotent changes.
					Apply( updates, IndexUpdateMode.Recovery );
			  }
			  else if ( _state == State.Running || _state == State.Starting )
			  {
					Apply( updates, IndexUpdateMode.Online );
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Can't apply index updates " + asList( updates ) + " while indexing service is " + _state );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void apply(Iterable<org.Neo4Net.kernel.api.index.IndexEntryUpdate<org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor>> updates, IndexUpdateMode updateMode) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void Apply( IEnumerable<IndexEntryUpdate<SchemaDescriptor>> updates, IndexUpdateMode updateMode )
		 {
			  using ( IndexUpdaterMap updaterMap = _indexMapRef.createIndexUpdaterMap( updateMode ) )
			  {
					foreach ( IndexEntryUpdate<SchemaDescriptor> indexUpdate in updates )
					{
						 ProcessUpdate( updaterMap, indexUpdate );
					}
			  }
		 }

		 public override IEnumerable<IndexEntryUpdate<SchemaDescriptor>> ConvertToIndexUpdates( IEntityUpdates IEntityUpdates, IEntityType type )
		 {
			  IEnumerable<SchemaDescriptor> relatedIndexes = _indexMapRef.getRelatedIndexes( IEntityUpdates.EntityTokensChanged(), IEntityUpdates.EntityTokensUnchanged(), IEntityUpdates.PropertiesChanged(), IEntityUpdates.PropertyListComplete, type );

			  return IEntityUpdates.ForIndexKeys( relatedIndexes, _storeView, type );
		 }

		 /// <summary>
		 /// Creates one or more indexes. They will all be populated by one and the same store scan.
		 /// 
		 /// This code is called from the transaction infrastructure during transaction commits, which means that
		 /// it is *vital* that it is stable, and handles errors very well. Failing here means that the entire db
		 /// will shut down.
		 /// 
		 /// <seealso cref="IndexPopulator.verifyDeferredConstraints(NodePropertyAccessor)"/> will not be called as part of populating these indexes,
		 /// instead that will be done by code that activates the indexes later.
		 /// </summary>
		 public virtual void CreateIndexes( params StoreIndexDescriptor[] rules )
		 {
			  CreateIndexes( false, rules );
		 }

		 /// <summary>
		 /// Creates one or more indexes. They will all be populated by one and the same store scan.
		 /// 
		 /// This code is called from the transaction infrastructure during transaction commits, which means that
		 /// it is *vital* that it is stable, and handles errors very well. Failing here means that the entire db
		 /// will shut down.
		 /// </summary>
		 /// <param name="verifyBeforeFlipping"> whether or not to call <seealso cref="IndexPopulator.verifyDeferredConstraints(NodePropertyAccessor)"/>
		 /// as part of population, before flipping to a successful state. </param>
		 public virtual void CreateIndexes( bool verifyBeforeFlipping, params StoreIndexDescriptor[] rules )
		 {
			  IndexPopulationStarter populationStarter = new IndexPopulationStarter( this, verifyBeforeFlipping, rules );
			  _indexMapRef.modify( populationStarter );
			  populationStarter.StartPopulation();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void processUpdate(IndexUpdaterMap updaterMap, org.Neo4Net.kernel.api.index.IndexEntryUpdate<org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor> indexUpdate) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ProcessUpdate( IndexUpdaterMap updaterMap, IndexEntryUpdate<SchemaDescriptor> indexUpdate )
		 {
			  IndexUpdater updater = updaterMap.GetUpdater( indexUpdate.IndexKey().schema() );
			  if ( updater != null )
			  {
					updater.Process( indexUpdate );
			  }
		 }

		 public virtual void DropIndex( StoreIndexDescriptor rule )
		 {
			  _indexMapRef.modify(indexMap =>
			  {
				long indexId = rule.Id;
				IndexProxy index = indexMap.removeIndexProxy( indexId );

				if ( _state == State.Running )
				{
					 Debug.Assert( index != null, "Index " + rule + " doesn't exists" );
					 index.Drop();
				}
				else if ( index != null )
				{
					 // Dropping an index means also updating the counts store, which is problematic during recovery.
					 // So instead make a note of it and actually perform the index drops after recovery.
					 _indexesToDropAfterCompletedRecovery[indexId] = index;
				}
				return indexMap;
			  });
		 }

		 public virtual void PutConstraint( ConstraintRule rule )
		 {
			  _indexMapRef.modify(indexMap =>
			  {
				if ( rule.ConstraintDescriptor.enforcesUniqueness() )
				{
					 indexMap.putUniquenessConstraint( rule );
				}
				return indexMap;
			  });
		 }

		 public virtual void RemoveConstraint( long ruleId )
		 {
			  _indexMapRef.modify(indexMap =>
			  {
				indexMap.removeUniquenessConstraint( ruleId );
				return indexMap;
			  });
		 }

		 public virtual void TriggerIndexSampling( IndexSamplingMode mode )
		 {
			  _internalLog.info( "Manual trigger for sampling all indexes [" + mode + "]" );
			  _samplingController.sampleIndexes( mode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void triggerIndexSampling(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, org.Neo4Net.kernel.impl.api.index.sampling.IndexSamplingMode mode) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual void TriggerIndexSampling( SchemaDescriptor descriptor, IndexSamplingMode mode )
		 {
			  string description = descriptor.UserDescription( _tokenNameLookup );
			  _internalLog.info( "Manual trigger for sampling index " + description + " [" + mode + "]" );
			  _samplingController.sampleIndex( _indexMapRef.getIndexId( descriptor ), mode );
		 }

		 private void DropRecoveringIndexes( IndexMap indexMap, LongIterable indexesToRebuild )
		 {
			  indexesToRebuild.forEach(idx =>
			  {
				IndexProxy indexProxy = indexMap.RemoveIndexProxy( idx );
				Debug.Assert( indexProxy != null );
				indexProxy.Drop();
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activateIndex(long indexId) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.index.IndexActivationFailedKernelException, org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException
		 public virtual void ActivateIndex( long indexId )
		 {
			  try
			  {
					if ( _state == State.Running ) // don't do this during recovery.
					{
						 IndexProxy index = GetIndexProxy( indexId );
						 index.AwaitStoreScanCompleted( 0, TimeUnit.MILLISECONDS );
						 index.Activate();
						 _internalLog.info( "Constraint %s is %s.", index.Descriptor, ONLINE.name() );
					}
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted();
					throw new IndexActivationFailedKernelException( e, "Unable to activate index, thread was interrupted." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexProxy getIndexProxy(long indexId) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual IndexProxy GetIndexProxy( long indexId )
		 {
			  return _indexMapRef.getIndexProxy( indexId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexProxy getIndexProxy(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual IndexProxy GetIndexProxy( SchemaDescriptor descriptor )
		 {
			  return _indexMapRef.getIndexProxy( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getIndexId(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual long GetIndexId( SchemaDescriptor descriptor )
		 {
			  return _indexMapRef.getIndexId( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateIndex(long indexId) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException, org.Neo4Net.kernel.api.exceptions.schema.UniquePropertyValueValidationException
		 public virtual void ValidateIndex( long indexId )
		 {
			  GetIndexProxy( indexId ).validate();
		 }

		 public virtual void ForceAll( IOLimiter limiter )
		 {
			  _indexMapRef.indexMapSnapshot().forEachIndexProxy(IndexProxyOperation("force", proxy => proxy.force(limiter)));
		 }

		 private LongObjectProcedure<IndexProxy> IndexProxyOperation( string name, ThrowingConsumer<IndexProxy, Exception> operation )
		 {
			  return ( id, indexProxy ) =>
			  {
				try
				{
					 operation.Accept( indexProxy );
				}
				catch ( Exception e )
				{
					 try
					 {
						  IndexProxy proxy = _indexMapRef.getIndexProxy( id );
						  throw new UnderlyingStorageException( "Unable to " + name + " " + proxy, e );
					 }
					 catch ( IndexNotFoundKernelException )
					 {
						  // index was dropped while trying to operate on it, we can continue to other indexes
					 }
				}
			  };
		 }

		 private void CloseAllIndexes()
		 {
			  _indexMapRef.modify(indexMap =>
			  {
				IEnumerable<IndexProxy> indexesToStop = indexMap.AllIndexProxies;
				foreach ( IndexProxy index in indexesToStop )
				{
					 try
					 {
						  index.Close();
					 }
					 catch ( Exception e )
					 {
						  _internalLog.error( "Unable to close index", e );
					 }
				}
				// Effectively clearing it
				return new IndexMap();
			  });
		 }

		 public virtual LongSet IndexIds
		 {
			 get
			 {
				  IEnumerable<IndexProxy> indexProxies = _indexMapRef.AllIndexProxies;
				  MutableLongSet indexIds = new LongHashSet();
				  foreach ( IndexProxy indexProxy in indexProxies )
				  {
						indexIds.add( indexProxy.Descriptor.Id );
				  }
				  return indexIds;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<java.io.File> snapshotIndexFiles() throws java.io.IOException
		 public virtual ResourceIterator<File> SnapshotIndexFiles()
		 {
			  ICollection<ResourceIterator<File>> snapshots = new List<ResourceIterator<File>>();
			  foreach ( IndexProxy indexProxy in _indexMapRef.AllIndexProxies )
			  {
					snapshots.Add( indexProxy.SnapshotFiles() );
			  }
			  return Iterators.concatResourceIterators( snapshots.GetEnumerator() );
		 }

		 private IndexPopulationJob NewIndexPopulationJob( IEntityType type, bool verifyBeforeFlipping )
		 {
			  MultipleIndexPopulator multiPopulator = _multiPopulatorFactory.create( _storeView, _internalLogProvider, type, _schemaState );
			  return new IndexPopulationJob( multiPopulator, _monitor, verifyBeforeFlipping );
		 }

		 private void StartIndexPopulation( IndexPopulationJob job )
		 {
			  _populationJobController.startIndexPopulation( job );
		 }

		 private string IndexStateInfo( string tag, InternalIndexState state, StoreIndexDescriptor descriptor )
		 {
			  return format( "IndexingService.%s: index %d on %s is %s", tag, descriptor.Id, descriptor.Schema().userDescription(_tokenNameLookup), state.name() );
		 }

		 private void LogIndexStateSummary( string method, IDictionary<InternalIndexState, IList<IndexLogRecord>> indexStates )
		 {
			  if ( indexStates.Count == 0 )
			  {
					return;
			  }
			  int mostPopularStateCount = int.MinValue;
			  InternalIndexState mostPopularState = null;
			  foreach ( KeyValuePair<InternalIndexState, IList<IndexLogRecord>> indexStateEntry in indexStates.SetOfKeyValuePairs() )
			  {
					if ( indexStateEntry.Value.size() > mostPopularStateCount )
					{
						 mostPopularState = indexStateEntry.Key;
						 mostPopularStateCount = indexStateEntry.Value.size();
					}
			  }
			  indexStates.Remove( mostPopularState );
			  foreach ( KeyValuePair<InternalIndexState, IList<IndexLogRecord>> indexStateEntry in indexStates.SetOfKeyValuePairs() )
			  {
					InternalIndexState state = indexStateEntry.Key;
					IList<IndexLogRecord> logRecords = indexStateEntry.Value;
					foreach ( IndexLogRecord logRecord in logRecords )
					{
						 _internalLog.info( IndexStateInfo( method, state, logRecord.Descriptor ) );
					}
			  }
			  _internalLog.info( format( "IndexingService.%s: indexes not specifically mentioned above are %s", method, mostPopularState ) );
		 }

		 private void LogIndexProviderSummary( IDictionary<IndexProviderDescriptor, IList<IndexLogRecord>> indexProviders )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> deprecatedIndexProviders = java.util.org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.values().Where(GraphDatabaseSettings.SchemaIndex::deprecated).Select(GraphDatabaseSettings.SchemaIndex::providerName).collect(Collectors.toSet());
			  StringJoiner joiner = new StringJoiner( ", ", "Deprecated index providers in use: ", ". Use procedure 'db.indexes()' to see what indexes use which index provider." );
			  MutableBoolean anyDeprecated = new MutableBoolean();
			  indexProviders.forEach((indexProviderDescriptor, indexLogRecords) =>
			  {
				if ( deprecatedIndexProviders.Contains( indexProviderDescriptor.name() ) )
				{
					 anyDeprecated.setTrue();
					 int numberOfIndexes = indexLogRecords.size();
					 joiner.add( indexProviderDescriptor.name() + " (" + numberOfIndexes + (numberOfIndexes == 1 ? " index" : " indexes") + ")" );
				}
			  });
			  if ( anyDeprecated.Value )
			  {
					_userLog.info( joiner.ToString() );
			  }
		 }

		 public virtual ICollection<SchemaDescriptor> GetRelatedIndexes( long[] labels, int propertyKeyId, IEntityType IEntityType )
		 {
			  return _indexMapRef.getRelatedIndexes( PrimitiveLongCollections.EMPTY_LONG_ARRAY, labels, new int[]{ propertyKeyId }, false, IEntityType );
		 }

		 public virtual ICollection<SchemaDescriptor> GetRelatedIndexes( long[] labels, int[] propertyKeyIds, IEntityType IEntityType )
		 {
			  return _indexMapRef.getRelatedIndexes( labels, PrimitiveLongCollections.EMPTY_LONG_ARRAY, propertyKeyIds, true, IEntityType );
		 }

		 public virtual ICollection<IndexBackedConstraintDescriptor> GetRelatedUniquenessConstraints( long[] labels, int propertyKeyId, IEntityType IEntityType )
		 {
			  return _indexMapRef.getRelatedConstraints( PrimitiveLongCollections.EMPTY_LONG_ARRAY, labels, new int[] { propertyKeyId }, false, IEntityType );
		 }

		 public virtual ICollection<IndexBackedConstraintDescriptor> GetRelatedUniquenessConstraints( long[] labels, int[] propertyKeyIds, IEntityType IEntityType )
		 {
			  return _indexMapRef.getRelatedConstraints( labels, PrimitiveLongCollections.EMPTY_LONG_ARRAY, propertyKeyIds, true, IEntityType );
		 }

		 public virtual bool HasRelatedSchema( long[] labels, int propertyKey, IEntityType IEntityType )
		 {
			  return _indexMapRef.hasRelatedSchema( labels, propertyKey, IEntityType );
		 }

		 public virtual bool HasRelatedSchema( int label, IEntityType IEntityType )
		 {
			  return _indexMapRef.hasRelatedSchema( label, IEntityType );
		 }

		 private sealed class IndexPopulationStarter : System.Func<IndexMap, IndexMap>
		 {
			 private readonly IndexingService _outerInstance;

			  internal readonly bool VerifyBeforeFlipping;
			  internal readonly StoreIndexDescriptor[] Descriptors;
			  internal IndexPopulationJob NodePopulationJob;
			  internal IndexPopulationJob RelationshipPopulationJob;

			  internal IndexPopulationStarter( IndexingService outerInstance, bool verifyBeforeFlipping, StoreIndexDescriptor[] descriptors )
			  {
				  this._outerInstance = outerInstance;
					this.VerifyBeforeFlipping = verifyBeforeFlipping;
					this.Descriptors = descriptors;
			  }

			  public override IndexMap Apply( IndexMap indexMap )
			  {
					foreach ( StoreIndexDescriptor descriptor in Descriptors )
					{
						 if ( outerInstance.state == State.NotStarted )
						 {
							  // In case of recovery remove any previously recorded INDEX DROP for this particular index rule id,
							  // in some scenario where rule ids may be reused.
							  outerInstance.indexesToDropAfterCompletedRecovery.Remove( descriptor.Id );
						 }
						 IndexProxy index = indexMap.GetIndexProxy( descriptor.Id );
						 if ( index != null && outerInstance.state == State.NotStarted )
						 {
							  // During recovery we might run into this scenario:
							  // - We're starting recovery on a database, where init() is called and all indexes that
							  //   are found in the store, instantiated and put into the IndexMap. Among them is index X.
							  // - While we recover the database we bump into a transaction creating index Y, with the
							  //   same IndexDescriptor, i.e. same label/property, as X. This is possible since this took
							  //   place before the creation of X.
							  // - When Y is dropped in between this creation and the creation of X (it will have to be
							  //   otherwise X wouldn't have had an opportunity to be created) the index is removed from
							  //   the IndexMap, both by id AND descriptor.
							  //
							  // Because of the scenario above we need to put this created index into the IndexMap
							  // again, otherwise it will disappear from the IndexMap (at least for lookup by descriptor)
							  // and not be able to accept changes applied from recovery later on.
							  indexMap.PutIndexProxy( index );
							  continue;
						 }
						 bool flipToTentative = descriptor.CanSupportUniqueConstraint();
						 if ( outerInstance.state == State.Running )
						 {
							  if ( descriptor.Schema().entityType() == IEntityType.NODE )
							  {
									NodePopulationJob = NodePopulationJob == null ? outerInstance.newIndexPopulationJob( IEntityType.NODE, VerifyBeforeFlipping ) : NodePopulationJob;
									index = outerInstance.indexProxyCreator.CreatePopulatingIndexProxy( descriptor, flipToTentative, outerInstance.monitor, NodePopulationJob );
									index.Start();
							  }
							  else
							  {
									RelationshipPopulationJob = RelationshipPopulationJob == null ? outerInstance.newIndexPopulationJob( IEntityType.RELATIONSHIP, VerifyBeforeFlipping ) : RelationshipPopulationJob;
									index = outerInstance.indexProxyCreator.CreatePopulatingIndexProxy( descriptor, flipToTentative, outerInstance.monitor, RelationshipPopulationJob );
									index.Start();
							  }
						 }
						 else
						 {
							  index = outerInstance.indexProxyCreator.CreateRecoveringIndexProxy( descriptor );
						 }

						 indexMap.PutIndexProxy( index );
					}
					return indexMap;
			  }

			  internal void StartPopulation()
			  {
					if ( NodePopulationJob != null )
					{
						 outerInstance.startIndexPopulation( NodePopulationJob );
					}
					if ( RelationshipPopulationJob != null )
					{
						 outerInstance.startIndexPopulation( RelationshipPopulationJob );
					}
			  }
		 }

		 private sealed class IndexLogRecord
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly StoreIndexDescriptor DescriptorConflict;

			  internal IndexLogRecord( StoreIndexDescriptor descriptor )
			  {
					this.DescriptorConflict = descriptor;
			  }

			  public long IndexId
			  {
				  get
				  {
						return DescriptorConflict.Id;
				  }
			  }

			  public StoreIndexDescriptor Descriptor
			  {
				  get
				  {
						return DescriptorConflict;
				  }
			  }
		 }
	}

}