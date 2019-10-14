using System;
using System.Collections.Generic;
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

	using Neo4Net.Functions;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using FlipFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.FlipFailedKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.utility.ArrayIterate.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.IndexPopulationFailure.failure;

	/// <summary>
	/// <seealso cref="IndexPopulator"/> that allow population of multiple indexes during one iteration.
	/// Performs operations by calling corresponding operations of particular index populators.
	/// 
	/// There are two ways data is fed to this multi-populator:
	/// <ul>
	/// <li><seealso cref="indexAllEntities()"/>, which is a blocking call and will scan the entire store and
	/// and generate updates that are fed into the <seealso cref="IndexPopulator populators"/>. Only a single call to this
	/// method should be made during the life time of a <seealso cref="MultipleIndexPopulator"/> and should be called by the
	/// same thread instantiating this instance.</li>
	/// <li><seealso cref="queueUpdate(IndexEntryUpdate)"/> which queues updates which will be read by the thread currently executing
	/// <seealso cref="indexAllEntities()"/> and incorporated into that data stream. Calls to this method may come from any number
	/// of concurrent threads.</li>
	/// </ul>
	/// 
	/// Usage of this class should be something like:
	/// <ol>
	/// <li>Instantiation.</li>
	/// <li>One or more calls to <seealso cref="addPopulator(IndexPopulator, CapableIndexDescriptor, FlippableIndexProxy, FailedIndexProxyFactory, string)"/>.</li>
	/// <li>Call to <seealso cref="create()"/> to create data structures and files to start accepting updates.</li>
	/// <li>Call to <seealso cref="indexAllEntities()"/> (blocking call).</li>
	/// <li>While all nodes are being indexed, calls to <seealso cref="queueUpdate(IndexEntryUpdate)"/> are accepted.</li>
	/// <li>Call to <seealso cref="flipAfterPopulation(bool)"/> after successful population, or <seealso cref="fail(System.Exception)"/> if not</li>
	/// </ol>
	/// </summary>
	public class MultipleIndexPopulator : IndexPopulator
	{
		 public const string QUEUE_THRESHOLD_NAME = "queue_threshold";
		 public const string BATCH_SIZE_NAME = "batch_size";

		 internal readonly int QueueThreshold = FeatureToggles.getInteger( this.GetType(), QUEUE_THRESHOLD_NAME, 20_000 );
		 internal readonly int BatchSize = FeatureToggles.getInteger( typeof( BatchingMultipleIndexPopulator ), BATCH_SIZE_NAME, 10_000 );
		 internal readonly bool PrintDebug = FeatureToggles.flag( typeof( MultipleIndexPopulator ), "print_debug", false );

		 // Concurrency queue since multiple concurrent threads may enqueue updates into it. It is important for this queue
		 // to have fast #size() method since it might be drained in batches
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: final java.util.Queue<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updatesQueue = new java.util.concurrent.LinkedBlockingQueue<>();
		 internal readonly LinkedList<IndexEntryUpdate<object>> UpdatesQueue = new LinkedBlockingQueue<IndexEntryUpdate<object>>();

		 // Populators are added into this list. The same thread adding populators will later call #indexAllEntities.
		 // Multiple concurrent threads might fail individual populations.
		 // Failed populations are removed from this list while iterating over it.
		 internal readonly IList<IndexPopulation> Populations = new CopyOnWriteArrayList<IndexPopulation>();

		 private readonly IndexStoreView _storeView;
		 private readonly LogProvider _logProvider;
		 protected internal readonly Log Log;
		 private readonly EntityType _type;
		 private readonly SchemaState _schemaState;
		 private readonly PhaseTracker _phaseTracker;
		 private StoreScan<IndexPopulationFailedKernelException> _storeScan;

		 public MultipleIndexPopulator( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState )
		 {
			  this._storeView = storeView;
			  this._logProvider = logProvider;
			  this.Log = logProvider.GetLog( typeof( IndexPopulationJob ) );
			  this._type = type;
			  this._schemaState = schemaState;
			  this._phaseTracker = new LoggingPhaseTracker( logProvider.GetLog( typeof( IndexPopulationJob ) ) );
		 }

		 internal virtual IndexPopulation AddPopulator( IndexPopulator populator, CapableIndexDescriptor capableIndexDescriptor, FlippableIndexProxy flipper, FailedIndexProxyFactory failedIndexProxyFactory, string indexUserDescription )
		 {
			  IndexPopulation population = CreatePopulation( populator, capableIndexDescriptor, flipper, failedIndexProxyFactory, indexUserDescription );
			  Populations.Add( population );
			  return population;
		 }

		 private IndexPopulation CreatePopulation( IndexPopulator populator, CapableIndexDescriptor capableIndexDescriptor, FlippableIndexProxy flipper, FailedIndexProxyFactory failedIndexProxyFactory, string indexUserDescription )
		 {
			  return new IndexPopulation( this, populator, capableIndexDescriptor, flipper, failedIndexProxyFactory, indexUserDescription );
		 }

		 internal virtual bool HasPopulators()
		 {
			  return Populations.Count > 0;
		 }

		 public override void Create()
		 {
			  ForEachPopulation(population =>
			  {
				Log.info( "Index population started: [%s]", population.indexUserDescription );
				population.create();
			  });
		 }

		 public override void Drop()
		 {
			  throw new System.NotSupportedException( "Can't drop indexes from this populator implementation" );
		 }

		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  throw new System.NotSupportedException( "Can't populate directly using this populator implementation. " );
		 }

		 internal virtual StoreScan<IndexPopulationFailedKernelException> IndexAllEntities()
		 {
			  int[] entityTokenIds = entityTokenIds();
			  int[] propertyKeyIds = propertyKeyIds();
			  System.Func<int, bool> propertyKeyIdFilter = propertyKeyId => contains( propertyKeyIds, propertyKeyId );

			  if ( _type == EntityType.RELATIONSHIP )
			  {
					_storeScan = _storeView.visitRelationships( entityTokenIds, propertyKeyIdFilter, new EntityPopulationVisitor( this ) );
			  }
			  else
			  {
					_storeScan = _storeView.visitNodes( entityTokenIds, propertyKeyIdFilter, new EntityPopulationVisitor( this ), null, false );
			  }
			  _storeScan.PhaseTracker = _phaseTracker;
			  return new DelegatingStoreScanAnonymousInnerClass( this, _storeScan );
		 }

		 private class DelegatingStoreScanAnonymousInnerClass : DelegatingStoreScan<IndexPopulationFailedKernelException>
		 {
			 private readonly MultipleIndexPopulator _outerInstance;

			 public DelegatingStoreScanAnonymousInnerClass( MultipleIndexPopulator outerInstance, Neo4Net.Kernel.Impl.Api.index.StoreScan<IndexPopulationFailedKernelException> storeScan ) : base( storeScan )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException
			 public override void run()
			 {
				  base.run();
				  outerInstance.FlushAll();
			 }
		 }

		 /// <summary>
		 /// Queues an update to be fed into the index populators. These updates come from changes being made
		 /// to storage while a concurrent scan is happening to keep populators up to date with all latest changes.
		 /// </summary>
		 /// <param name="update"> <seealso cref="IndexEntryUpdate"/> to queue. </param>
		 internal virtual void QueueUpdate<T1>( IndexEntryUpdate<T1> update )
		 {
			  UpdatesQueue.AddLast( update );
		 }

		 /// <summary>
		 /// Called if forced failure from the outside
		 /// </summary>
		 /// <param name="failure"> index population failure. </param>
		 public virtual void Fail( Exception failure )
		 {
			  foreach ( IndexPopulation population in Populations )
			  {
					Fail( population, failure );
			  }
		 }

		 protected internal virtual void Fail( IndexPopulation population, Exception failure )
		 {
			  if ( !RemoveFromOngoingPopulations( population ) )
			  {
					return;
			  }

			  // If the cause of index population failure is a conflict in a (unique) index, the conflict is the failure
			  if ( failure is IndexPopulationFailedKernelException )
			  {
					Exception cause = failure.InnerException;
					if ( cause is IndexEntryConflictException )
					{
						 failure = cause;
					}
			  }

			  Log.error( format( "Failed to populate index: [%s]", population.IndexUserDescription ), failure );

			  // The flipper will have already flipped to a failed index context here, but
			  // it will not include the cause of failure, so we do another flip to a failed
			  // context that does.

			  // The reason for having the flipper transition to the failed index context in the first
			  // place is that we would otherwise introduce a race condition where updates could come
			  // in to the old context, if something failed in the job we send to the flipper.
			  IndexPopulationFailure indexPopulationFailure = failure( failure );
			  population.FlipToFailed( indexPopulationFailure );
			  try
			  {
					population.Populator.markAsFailed( indexPopulationFailure.AsString() );
					population.Populator.close( false );
			  }
			  catch ( Exception e )
			  {
					Log.error( format( "Unable to close failed populator for index: [%s]", population.IndexUserDescription ), e );
			  }
		 }

		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  throw new System.NotSupportedException( "Should not be called directly" );
		 }

		 public override MultipleIndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  IDictionary<SchemaDescriptor, Pair<IndexPopulation, IndexUpdater>> updaters = new Dictionary<SchemaDescriptor, Pair<IndexPopulation, IndexUpdater>>();
			  ForEachPopulation(population =>
			  {
				IndexUpdater updater = population.populator.newPopulatingUpdater( accessor );
				updaters[population.schema()] = Pair.of(population, updater);
			  });
			  return new MultipleIndexUpdater( this, updaters, _logProvider );
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			  _phaseTracker.stop();
			  // closing the populators happens in flip, fail or individually when they are completed
		 }

		 public override void MarkAsFailed( string failure )
		 {
			  throw new System.NotSupportedException( "Multiple index populator can't be marked as failed." );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  throw new System.NotSupportedException( "Multiple index populator can't perform index sampling." );
		 }

		 public override IndexSample SampleResult()
		 {
			  throw new System.NotSupportedException( "Multiple index populator can't perform index sampling." );
		 }

		 public override void ScanCompleted( PhaseTracker phaseTracker )
		 {
			  throw new System.NotSupportedException( "Not supposed to be called" );
		 }

		 internal virtual void ResetIndexCounts()
		 {
			  ForEachPopulation( this.resetIndexCountsForPopulation );
		 }

		 private void ResetIndexCountsForPopulation( IndexPopulation indexPopulation )
		 {
			  _storeView.replaceIndexCounts( indexPopulation.IndexId, 0, 0, 0 );
		 }

		 internal virtual void FlipAfterPopulation( bool verifyBeforeFlipping )
		 {
			  foreach ( IndexPopulation population in Populations )
			  {
					try
					{
						 population.ScanCompleted();
						 population.Flip( verifyBeforeFlipping );
					}
					catch ( Exception t )
					{
						 Fail( population, t );
					}
			  }
		 }

		 private int[] PropertyKeyIds()
		 {
			  return Populations.stream().flatMapToInt(this.propertyKeyIds).distinct().toArray();
		 }

		 private IntStream PropertyKeyIds( IndexPopulation population )
		 {
			  return IntStream.of( population.Schema().PropertyIds );
		 }

		 private int[] EntityTokenIds()
		 {
			  return Populations.stream().flatMapToInt(population => Arrays.stream(population.schema().EntityTokenIds)).toArray();
		 }

		 public virtual void Cancel()
		 {
			  ForEachPopulation( this.cancelIndexPopulation );
		 }

		 internal virtual void CancelIndexPopulation( IndexPopulation indexPopulation )
		 {
			  indexPopulation.Cancel();
		 }

		 internal virtual void DropIndexPopulation( IndexPopulation indexPopulation )
		 {
			  indexPopulation.CancelAndDrop();
		 }

		 private bool RemoveFromOngoingPopulations( IndexPopulation indexPopulation )
		 {
			  return Populations.Remove( indexPopulation );
		 }

		 internal virtual bool PopulateFromQueueBatched( long currentlyIndexedNodeId )
		 {
			  return PopulateFromQueue( QueueThreshold, currentlyIndexedNodeId );
		 }

		 internal virtual void FlushAll()
		 {
			  Populations.ForEach( this.flush );
		 }

		 protected internal virtual void Flush( IndexPopulation population )
		 {
			  _phaseTracker.enterPhase( PhaseTracker_Phase.Write );
			  DoFlush( population );
		 }

		 internal virtual void DoFlush( IndexPopulation population )
		 {
			  try
			  {
					population.Populator.add( population.TakeCurrentBatch() );
			  }
			  catch ( Exception failure )
			  {
					Fail( population, failure );
			  }
		 }

		 /// <summary>
		 /// Populates external updates from the update queue if there are {@code queueThreshold} or more queued updates.
		 /// </summary>
		 /// <returns> whether or not there were external updates applied. </returns>
		 internal virtual bool PopulateFromQueue( int queueThreshold, long currentlyIndexedNodeId )
		 {
			  int queueSize = UpdatesQueue.Count;
			  if ( queueSize > 0 && queueSize >= queueThreshold )
			  {
					if ( PrintDebug )
					{
						 Log.info( "Populating from queue at %d", currentlyIndexedNodeId );
					}
					// Before applying updates from the updates queue any pending scan updates needs to be applied, i.e. flushed.
					// This is because 'currentlyIndexedNodeId' is based on how far the scan has come.
					FlushAll();

					using ( MultipleIndexUpdater updater = NewPopulatingUpdater( _storeView ) )
					{
						 do
						 {
							  // no need to check for null as nobody else is emptying this queue
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update = updatesQueue.poll();
							  IndexEntryUpdate<object> update = UpdatesQueue.RemoveFirst();
							  _storeScan.acceptUpdate( updater, update, currentlyIndexedNodeId );
							  if ( PrintDebug )
							  {
									Log.info( "Applied %s from queue" + update );
							  }
						 } while ( UpdatesQueue.Count > 0 );
					}
					if ( PrintDebug )
					{
						 Log.info( "Done applying updates from queue" );
					}
					return true;
			  }
			  return false;
		 }

		 private void ForEachPopulation( ThrowingConsumer<IndexPopulation, Exception> action )
		 {
			  foreach ( IndexPopulation population in Populations )
			  {
					try
					{
						 action.Accept( population );
					}
					catch ( Exception failure )
					{
						 Fail( population, failure );
					}
			  }
		 }

		 public class MultipleIndexUpdater : IndexUpdater
		 {
			  internal readonly IDictionary<SchemaDescriptor, Pair<IndexPopulation, IndexUpdater>> PopulationsWithUpdaters;
			  internal readonly MultipleIndexPopulator MultipleIndexPopulator;
			  internal readonly Log Log;

			  internal MultipleIndexUpdater( MultipleIndexPopulator multipleIndexPopulator, IDictionary<SchemaDescriptor, Pair<IndexPopulation, IndexUpdater>> populationsWithUpdaters, LogProvider logProvider )
			  {
					this.MultipleIndexPopulator = multipleIndexPopulator;
					this.PopulationsWithUpdaters = populationsWithUpdaters;
					this.Log = logProvider.getLog( this.GetType() );
			  }

			  public override void Process<T1>( IndexEntryUpdate<T1> update )
			  {
					Pair<IndexPopulation, IndexUpdater> pair = PopulationsWithUpdaters[update.IndexKey().schema()];
					if ( pair != null )
					{
						 IndexPopulation population = pair.First();
						 IndexUpdater updater = pair.Other();

						 try
						 {
							  population.Populator.includeSample( update );
							  updater.Process( update );
						 }
						 catch ( Exception t )
						 {
							  try
							  {
									updater.Close();
							  }
							  catch ( Exception ce )
							  {
									Log.error( format( "Failed to close index updater: [%s]", updater ), ce );
							  }
							  PopulationsWithUpdaters.Remove( update.IndexKey().schema() );
							  MultipleIndexPopulator.fail( population, t );
						 }
					}
			  }

			  public override void Close()
			  {
					foreach ( Pair<IndexPopulation, IndexUpdater> pair in PopulationsWithUpdaters.Values )
					{
						 IndexPopulation population = pair.First();
						 IndexUpdater updater = pair.Other();

						 try
						 {
							  updater.Close();
						 }
						 catch ( Exception t )
						 {
							  MultipleIndexPopulator.fail( population, t );
						 }
					}
					PopulationsWithUpdaters.Clear();
			  }
		 }

		 public class IndexPopulation : SchemaDescriptorSupplier
		 {
			 private readonly MultipleIndexPopulator _outerInstance;

			  public readonly IndexPopulator Populator;
			  internal readonly FlippableIndexProxy Flipper;
			  internal readonly long IndexId;
			  internal readonly CapableIndexDescriptor CapableIndexDescriptor;
			  internal readonly IndexCountsRemover IndexCountsRemover;
			  internal readonly FailedIndexProxyFactory FailedIndexProxyFactory;
			  internal readonly string IndexUserDescription;
			  internal bool PopulationOngoing = true;
			  internal readonly ReentrantLock PopulatorLock = new ReentrantLock();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batchedUpdates;
			  internal IList<IndexEntryUpdate<object>> BatchedUpdates;

			  internal IndexPopulation( MultipleIndexPopulator outerInstance, IndexPopulator populator, CapableIndexDescriptor capableIndexDescriptor, FlippableIndexProxy flipper, FailedIndexProxyFactory failedIndexProxyFactory, string indexUserDescription )
			  {
				  this._outerInstance = outerInstance;
					this.Populator = populator;
					this.CapableIndexDescriptor = capableIndexDescriptor;
					this.IndexId = capableIndexDescriptor.Id;
					this.Flipper = flipper;
					this.FailedIndexProxyFactory = failedIndexProxyFactory;
					this.IndexUserDescription = indexUserDescription;
					this.IndexCountsRemover = new IndexCountsRemover( outerInstance.storeView, IndexId );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: this.batchedUpdates = new java.util.ArrayList<>(BATCH_SIZE);
					this.BatchedUpdates = new List<IndexEntryUpdate<object>>( outerInstance.BatchSize );
			  }

			  internal virtual void FlipToFailed( IndexPopulationFailure failure )
			  {
					Flipper.flipTo( new FailedIndexProxy( CapableIndexDescriptor, IndexUserDescription, Populator, failure, IndexCountsRemover, outerInstance.logProvider ) );
			  }

			  internal virtual void Create()
			  {
					PopulatorLock.@lock();
					try
					{
						 if ( PopulationOngoing )
						 {
							  Populator.create();
						 }
					}
					finally
					{
						 PopulatorLock.unlock();
					}
			  }

			  internal virtual void Cancel()
			  {
					Cancel( () => Populator.close(false) );
			  }

			  internal virtual void CancelAndDrop()
			  {
					Cancel( Populator.drop );
			  }

			  /// <summary>
			  /// Cancels population also executing a specific operation on the populator </summary>
			  /// <param name="specificPopulatorOperation"> specific operation in addition to closing the populator. </param>
			  internal virtual void Cancel( ThreadStart specificPopulatorOperation )
			  {
					PopulatorLock.@lock();
					try
					{
						 if ( PopulationOngoing )
						 {
							  // First of all remove this population from the list of ongoing populations so that it won't receive more updates.
							  // This is good because closing the populator may wait for an opportunity to perform the close, among the incoming writes to it.
							  outerInstance.removeFromOngoingPopulations( this );
							  specificPopulatorOperation.run();
							  outerInstance.resetIndexCountsForPopulation( this );
							  PopulationOngoing = false;
						 }
					}
					finally
					{
						 PopulatorLock.unlock();
					}
			  }

			  internal virtual void OnUpdate<T1>( IndexEntryUpdate<T1> update )
			  {
					Populator.includeSample( update );
					if ( Batch( update ) )
					{
						 outerInstance.Flush( this );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flip(boolean verifyBeforeFlipping) throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
			  internal virtual void Flip( bool verifyBeforeFlipping )
			  {
					outerInstance.phaseTracker.EnterPhase( PhaseTracker_Phase.Flip );
					Flipper.flip(() =>
					{
					 PopulatorLock.@lock();
					 try
					 {
						  if ( PopulationOngoing )
						  {
								Populator.add( TakeCurrentBatch() );
								outerInstance.PopulateFromQueue( 0, long.MaxValue );
								if ( outerInstance.Populations.Contains( IndexPopulation.this ) )
								{
									 if ( verifyBeforeFlipping )
									 {
										  Populator.verifyDeferredConstraints( outerInstance.storeView );
									 }
									 IndexSample sample = Populator.sampleResult();
									 outerInstance.storeView.ReplaceIndexCounts( IndexId, sample.uniqueValues(), sample.sampleSize(), sample.indexSize() );
									 Populator.close( true );
									 outerInstance.schemaState.Clear();
									 return true;
								}
						  }
						  return false;
					 }
					 finally
					 {
						  PopulationOngoing = false;
						  PopulatorLock.unlock();
					 }
					}, FailedIndexProxyFactory);
					outerInstance.removeFromOngoingPopulations( this );
					LogCompletionMessage();
			  }

			  internal virtual void LogCompletionMessage()
			  {
					InternalIndexState postPopulationState = Flipper.State;
					string messageTemplate = IsIndexPopulationOngoing( postPopulationState ) ? "Index created. Starting data checks. Index [%s] is %s." : "Index creation finished. Index [%s] is %s.";
					outerInstance.Log.info( messageTemplate, IndexUserDescription, postPopulationState.name() );
			  }

			  internal virtual bool IsIndexPopulationOngoing( InternalIndexState postPopulationState )
			  {
					return InternalIndexState.POPULATING == postPopulationState;
			  }

			  public override SchemaDescriptor Schema()
			  {
					return CapableIndexDescriptor.schema();
			  }

			  public virtual bool Batch<T1>( IndexEntryUpdate<T1> update )
			  {
					BatchedUpdates.Add( update );
					return BatchedUpdates.Count >= outerInstance.BatchSize;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> takeCurrentBatch()
			  internal virtual IList<IndexEntryUpdate<object>> TakeCurrentBatch()
			  {
					if ( BatchedUpdates.Count == 0 )
					{
						 return Collections.emptyList();
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch = batchedUpdates;
					IList<IndexEntryUpdate<object>> batch = BatchedUpdates;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: batchedUpdates = new java.util.ArrayList<>(BATCH_SIZE);
					BatchedUpdates = new List<IndexEntryUpdate<object>>( outerInstance.BatchSize );
					return batch;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void scanCompleted() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  internal virtual void ScanCompleted()
			  {
					Populator.scanCompleted( outerInstance.phaseTracker );
			  }

			  internal virtual PopulationProgress Progress( PopulationProgress storeScanProgress )
			  {
					return Populator.progress( storeScanProgress );
			  }
		 }

		 private class EntityPopulationVisitor : Visitor<EntityUpdates, IndexPopulationFailedKernelException>
		 {
			 private readonly MultipleIndexPopulator _outerInstance;

			 public EntityPopulationVisitor( MultipleIndexPopulator outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool Visit( EntityUpdates updates )
			  {
					Add( updates );
					if ( outerInstance.PrintDebug )
					{
						 outerInstance.Log.info( "Added scan updates for entity %d", updates.EntityId );
					}
					return outerInstance.PopulateFromQueueBatched( updates.EntityId );
			  }

			  internal virtual void Add( EntityUpdates updates )
			  {
					// This is called from a full store node scan, meaning that all node properties are included in the
					// EntityUpdates object. Therefore no additional properties need to be loaded.
					foreach ( IndexEntryUpdate<IndexPopulation> indexUpdate in updates.ForIndexKeys( outerInstance.Populations ) )
					{
						 indexUpdate.IndexKey().onUpdate(indexUpdate);
					}
			  }
		 }

		 protected internal class DelegatingStoreScan<E> : StoreScan<E> where E : Exception
		 {
			  internal readonly StoreScan<E> Delegate;

			  internal DelegatingStoreScan( StoreScan<E> @delegate )
			  {
					this.Delegate = @delegate;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws E
			  public override void Run()
			  {
					Delegate.run();
			  }

			  public override void Stop()
			  {
					Delegate.stop();
			  }

			  public override void AcceptUpdate<T1>( MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
			  {
					Delegate.acceptUpdate( updater, update, currentlyIndexedNodeId );
			  }

			  public virtual PopulationProgress Progress
			  {
				  get
				  {
						return Delegate.Progress;
				  }
			  }

			  public virtual PhaseTracker PhaseTracker
			  {
				  set
				  {
						Delegate.PhaseTracker = value;
				  }
			  }
		 }
	}

}