using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.catchup.tx
{

	using Org.Neo4j.causalclustering.catchup;
	using DatabaseShutdownException = Org.Neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using StoreCopyFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreCopyProcess = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyProcess;
	using Timer = Org.Neo4j.causalclustering.core.consensus.schedule.Timer;
	using TimerService = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService;
	using TimerName = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService.TimerName;
	using TopologyLookupException = Org.Neo4j.causalclustering.core.state.snapshot.TopologyLookupException;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using Suspendable = Org.Neo4j.causalclustering.helper.Suspendable;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using UpstreamDatabaseSelectionException = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseSelectionException;
	using UpstreamDatabaseStrategySelector = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Group = Org.Neo4j.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.PANIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.STORE_COPYING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.TX_PULLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.Timers.TX_PULLER_TIMER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.TimeoutFactory.fixedTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.Timer.CancelMode.SYNC_WAIT;

	/// <summary>
	/// This class is responsible for pulling transactions from a core server and queuing
	/// them to be applied with the <seealso cref="BatchingTxApplier"/>. Pull requests are issued on
	/// a fixed interval.
	/// <para>
	/// If the necessary transactions are not remotely available then a fresh copy of the
	/// entire store will be pulled down.
	/// </para>
	/// </summary>
	public class CatchupPollingProcess : LifecycleAdapter
	{
		 internal enum Timers
		 {
			  TxPullerTimer
		 }

		 internal enum State
		 {
			  TxPulling,
			  StoreCopying,
			  Panic,
			  Cancelled
		 }

		 private readonly LocalDatabase _localDatabase;
		 private readonly Log _log;
		 private readonly Suspendable _enableDisableOnStoreCopy;
		 private readonly StoreCopyProcess _storeCopyProcess;
		 private readonly System.Func<DatabaseHealth> _databaseHealthSupplier;
		 private readonly CatchUpClient _catchUpClient;
		 private readonly UpstreamDatabaseStrategySelector _selectionStrategy;
		 private readonly TimerService _timerService;
		 private readonly long _txPullIntervalMillis;
		 private readonly BatchingTxApplier _applier;
		 private readonly PullRequestMonitor _pullRequestMonitor;
		 private readonly TopologyService _topologyService;

		 private Timer _timer;
		 private volatile State _state = TX_PULLING;
		 private DatabaseHealth _dbHealth;
		 private CompletableFuture<bool> _upToDateFuture; // we are up-to-date when we are successfully pulling
		 private volatile long _latestTxIdOfUpStream;

		 public CatchupPollingProcess( LogProvider logProvider, LocalDatabase localDatabase, Suspendable enableDisableOnSoreCopy, CatchUpClient catchUpClient, UpstreamDatabaseStrategySelector selectionStrategy, TimerService timerService, long txPullIntervalMillis, BatchingTxApplier applier, Monitors monitors, StoreCopyProcess storeCopyProcess, System.Func<DatabaseHealth> databaseHealthSupplier, TopologyService topologyService )

		 {
			  this._localDatabase = localDatabase;
			  this._log = logProvider.getLog( this.GetType() );
			  this._enableDisableOnStoreCopy = enableDisableOnSoreCopy;
			  this._catchUpClient = catchUpClient;
			  this._selectionStrategy = selectionStrategy;
			  this._timerService = timerService;
			  this._txPullIntervalMillis = txPullIntervalMillis;
			  this._applier = applier;
			  this._pullRequestMonitor = monitors.NewMonitor( typeof( PullRequestMonitor ) );
			  this._storeCopyProcess = storeCopyProcess;
			  this._databaseHealthSupplier = databaseHealthSupplier;
			  this._topologyService = topologyService;
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  _state = TX_PULLING;
				  _timer = _timerService.create( TX_PULLER_TIMER, Group.PULL_UPDATES, timeout => onTimeout() );
				  _timer.set( fixedTimeout( _txPullIntervalMillis, MILLISECONDS ) );
				  _dbHealth = _databaseHealthSupplier.get();
				  _upToDateFuture = new CompletableFuture<bool>();
			 }
		 }

		 public virtual Future<bool> UpToDateFuture()
		 {
			  return _upToDateFuture;
		 }

		 public override void Stop()
		 {
			  _state = CANCELLED;
			  _timer.cancel( SYNC_WAIT );
		 }

		 public virtual State State()
		 {
			  return _state;
		 }

		 /// <summary>
		 /// Time to catchup!
		 /// </summary>
		 private void OnTimeout()
		 {
			  try
			  {
					switch ( _state )
					{
					case Org.Neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.TxPulling:
						 PullTransactions();
						 break;

					case Org.Neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.StoreCopying:
						 CopyStore();
						 break;

					default:
						 throw new System.InvalidOperationException( "Tried to execute catchup but was in state " + _state );
					}
			  }
			  catch ( Exception e )
			  {
					Panic( e );
			  }

			  if ( _state != PANIC && _state != CANCELLED )
			  {
					_timer.reset();
			  }
		 }

		 private void Panic( Exception e )
		 {
			 lock ( this )
			 {
				  _log.error( "Unexpected issue in catchup process. No more catchup requests will be scheduled.", e );
				  _dbHealth.panic( e );
				  _upToDateFuture.completeExceptionally( e );
				  _state = PANIC;
			 }
		 }

		 private void PullTransactions()
		 {
			  MemberId upstream;
			  try
			  {
					upstream = _selectionStrategy.bestUpstreamDatabase();
			  }
			  catch ( UpstreamDatabaseSelectionException e )
			  {
					_log.warn( "Could not find upstream database from which to pull.", e );
					return;
			  }

			  StoreId localStoreId = _localDatabase.storeId();

			  bool moreToPull = true;
			  int batchCount = 1;
			  while ( moreToPull )
			  {
					moreToPull = PullAndApplyBatchOfTransactions( upstream, localStoreId, batchCount );
					batchCount++;
			  }
		 }

		 private void HandleTransaction( CommittedTransactionRepresentation tx )
		 {
			 lock ( this )
			 {
				  if ( _state == PANIC )
				  {
						return;
				  }
      
				  try
				  {
						_applier.queue( tx );
				  }
				  catch ( Exception e )
				  {
						Panic( e );
				  }
			 }
		 }

		 private void StreamComplete()
		 {
			 lock ( this )
			 {
				  if ( _state == PANIC )
				  {
						return;
				  }
      
				  try
				  {
						_applier.applyBatch();
				  }
				  catch ( Exception e )
				  {
						Panic( e );
				  }
			 }
		 }

		 private bool PullAndApplyBatchOfTransactions( MemberId upstream, StoreId localStoreId, int batchCount )
		 {
			  long lastQueuedTxId = _applier.lastQueuedTxId();
			  _pullRequestMonitor.txPullRequest( lastQueuedTxId );
			  TxPullRequest txPullRequest = new TxPullRequest( lastQueuedTxId, localStoreId );
			  _log.debug( "Pull transactions from %s where tx id > %d [batch #%d]", upstream, lastQueuedTxId, batchCount );

			  TxStreamFinishedResponse response;
			  try
			  {
					AdvertisedSocketAddress fromAddress = _topologyService.findCatchupAddress( upstream ).orElseThrow( () => new TopologyLookupException(upstream) );
					response = _catchUpClient.makeBlockingRequest( fromAddress, txPullRequest, new CatchUpResponseAdaptorAnonymousInnerClass( this, response ) );
			  }
			  catch ( Exception e ) when ( e is CatchUpClientException || e is TopologyLookupException )
			  {
					_log.warn( "Exception occurred while pulling transactions. Will retry shortly.", e );
					StreamComplete();
					return false;
			  }

			  _latestTxIdOfUpStream = response.LatestTxId();

			  switch ( response.Status() )
			  {
			  case SUCCESS_END_OF_STREAM:
					_log.debug( "Successfully pulled transactions from tx id %d", lastQueuedTxId );
					_upToDateFuture.complete( true );
					return false;
			  case E_TRANSACTION_PRUNED:
					_log.info( "Tx pull unable to get transactions starting from %d since transactions have been pruned. Attempting a store copy.", lastQueuedTxId );
					_state = STORE_COPYING;
					return false;
			  default:
					_log.info( "Tx pull request unable to get transactions > %d " + lastQueuedTxId );
					return false;
			  }
		 }

		 private class CatchUpResponseAdaptorAnonymousInnerClass : CatchUpResponseAdaptor<TxStreamFinishedResponse>
		 {
			 private readonly CatchupPollingProcess _outerInstance;

			 private Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponse _response;

			 public CatchUpResponseAdaptorAnonymousInnerClass( CatchupPollingProcess outerInstance, Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponse response )
			 {
				 this.outerInstance = outerInstance;
				 this._response = response;
			 }

			 public override void onTxPullResponse( CompletableFuture<TxStreamFinishedResponse> signal, TxPullResponse response )
			 {
				  outerInstance.handleTransaction( response.Tx() );
			 }

			 public override void onTxStreamFinishedResponse( CompletableFuture<TxStreamFinishedResponse> signal, TxStreamFinishedResponse response )
			 {
				  outerInstance.streamComplete();
				  signal.complete( response );
			 }
		 }

		 private void CopyStore()
		 {
			  StoreId localStoreId = _localDatabase.storeId();
			  DownloadDatabase( localStoreId );
		 }

		 private void DownloadDatabase( StoreId localStoreId )
		 {
			  try
			  {
					_localDatabase.stopForStoreCopy();
					_enableDisableOnStoreCopy.disable();
			  }
			  catch ( Exception throwable )
			  {
					throw new Exception( throwable );
			  }

			  try
			  {
					MemberId source = _selectionStrategy.bestUpstreamDatabase();
					AdvertisedSocketAddress fromAddress = _topologyService.findCatchupAddress( source ).orElseThrow( () => new TopologyLookupException(source) );
					_storeCopyProcess.replaceWithStoreFrom( new CatchupAddressProvider_SingleAddressProvider( fromAddress ), localStoreId );
			  }
			  catch ( Exception e ) when ( e is IOException || e is StoreCopyFailedException || e is UpstreamDatabaseSelectionException || e is TopologyLookupException )
			  {
					_log.warn( "Error copying store. Will retry shortly.", e );
					return;
			  }
			  catch ( DatabaseShutdownException e )
			  {
					_log.warn( "Store copy aborted due to shutdown.", e );
					return;
			  }

			  try
			  {
					_localDatabase.start();
					_enableDisableOnStoreCopy.enable();
			  }
			  catch ( Exception throwable )
			  {
					throw new Exception( throwable );
			  }

			  _latestTxIdOfUpStream = 0; // we will find out on the next pull request response
			  _state = TX_PULLING;
			  _applier.refreshFromNewStore();
		 }

		 public virtual string DescribeState()
		 {
			  if ( _state == TX_PULLING && _applier.lastQueuedTxId() > 0 && _latestTxIdOfUpStream > 0 )
			  {
					return format( "%s (%d of %d)", TX_PULLING.name(), _applier.lastQueuedTxId(), _latestTxIdOfUpStream );
			  }
			  else
			  {
					return _state.name();
			  }
		 }
	}

}