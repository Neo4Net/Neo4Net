using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using Neo4Net.com;
	using TransactionStream = Neo4Net.com.TransactionStream;
	using TransactionCommittingResponseUnpacker = Neo4Net.com.storecopy.TransactionCommittingResponseUnpacker;
	using TransactionObligationFulfiller = Neo4Net.com.storecopy.TransactionObligationFulfiller;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using InvalidEpochException = Neo4Net.Kernel.ha.com.master.InvalidEpochException;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using MasterImpl = Neo4Net.Kernel.ha.com.master.MasterImpl;
	using InvalidEpochExceptionHandler = Neo4Net.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using MasterClient = Neo4Net.Kernel.ha.com.slave.MasterClient;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CappedLogger = Neo4Net.Logging.Internal.CappedLogger;
	using CancelListener = Neo4Net.Scheduler.CancelListener;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Able to pull updates from a master and apply onto this slave database.
	/// <para>
	/// Updates are pulled and applied using a single and dedicated thread, created in here. No other threads are allowed to
	/// pull and apply transactions on a slave. Calling one of the <seealso cref="Master.pullUpdates(RequestContext) pullUpdates"/>
	/// <seealso cref="TransactionObligationFulfiller.fulfill(long)"/> will <seealso cref="SlaveUpdatePuller.poke() poke"/> that single thread,
	/// so
	/// that it gets going, if not already doing so, with its usual task of pulling updates and the caller which poked
	/// the update thread will constantly poll to see if the transactions it is obliged to await have been applied.
	/// </para>
	/// <para>
	/// Here comes a diagram of how the classes making up this functionality hangs together:
	/// </para>
	/// <para>
	/// <pre>
	/// 
	///             -------- 1 -------------------->(<seealso cref="MasterImpl master"/>)
	///           /                                   |
	///          |                                   /
	///          | v--------------------- 2 --------
	///       (<seealso cref="MasterClient slave"/>)
	///       | ^ \
	///       | |   -------- 3 -----
	///       | \                   \
	///       |  \                   v
	///       |   ---- 8 -----------(<seealso cref="TransactionCommittingResponseUnpacker response unpacker"/>)
	///       |                        |     ^
	///       9                        |     |
	///    (continue)                  4     7
	///                                |     |
	///                                v     |
	///                             (<seealso cref="UpdatePullingTransactionObligationFulfiller obligation fulfiller"/>)
	///                                |     ^
	///                                |     |
	///                                5     6
	///                                |     |
	///                                v     |
	///                            (<seealso cref="SlaveUpdatePuller update puller"/>)
	/// 
	/// </pre>
	/// </para>
	/// <para>
	/// In the above picture:
	/// </para>
	/// <para>
	/// <ol>
	/// <li>Slave issues request to master, for example for locking an IEntity</li>
	/// <li>Master responds with a <seealso cref="TransactionObligationResponse transaction obligation"/> telling the slave
	/// which transaction it must have applied before continuing</li>
	/// <li>The response from the master gets unpacked by the response unpacker...</li>
	/// <li>...which will be sent to the <seealso cref="UpdatePullingTransactionObligationFulfiller obligation fulfiller"/>...</li>
	/// <li>...which will ask the <seealso cref="SlaveUpdatePuller update puller"/>, a separate thread, to have that transaction
	/// committed and applied. The calling thread will gently wait for that to happen.</li>
	/// <li>The <seealso cref="SlaveUpdatePuller update puller"/> will pull updates until it reaches that desired transaction id,
	/// and might actually continue passed that point if master has more transactions. The obligation fulfiller,
	/// constantly polling for <seealso cref="TransactionIdStore.getLastClosedTransactionId() last applied transaction"/>
	/// will notice when the obligation has been fulfilled.</li>
	/// <li>Response unpacker finishes its call to {@link TransactionObligationFulfiller#fulfill(long) fulfill the
	/// obligation}</li>
	/// <li>Slave has fully received the response and is now able to...</li>
	/// <li>...continue</li>
	/// </ol>
	/// </para>
	/// <para>
	/// All communication, except actually pulling updates, work this way between slave and master. The only difference
	/// in the pullUpdates case is that instead of receiving and fulfilling a transaction obligation,
	/// <seealso cref="TransactionStream transaction data"/> is received and applied to store directly, in batches.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= org.Neo4Net.kernel.ha.UpdatePuller </seealso>
	public class SlaveUpdatePuller : ThreadStart, UpdatePuller, CancelListener
	{
		 public interface Monitor
		 {
			  void PulledUpdates( long lastAppliedTxId );
		 }

		 public static readonly int LogCap = Integer.getInteger( "org.Neo4Net.kernel.ha.SlaveUpdatePuller.LOG_CAP", 10 );
		 public static readonly long ParkNanos = TimeUnit.MILLISECONDS.toNanos( Integer.getInteger( "org.Neo4Net.kernel.ha.SlaveUpdatePuller.PARK_MILLIS", 100 ) );
		 public static readonly int AvailabilityAwaitMillis = Integer.getInteger( "org.Neo4Net.kernel.ha.SlaveUpdatePuller.AVAILABILITY_AWAIT_MILLIS", 5000 );
		 public const string UPDATE_PULLER_THREAD_PREFIX = "UpdatePuller@";

		 private static readonly UpdatePuller_Condition _nextTicket = ( _currentTicket, _targetTicket ) => _currentTicket >= _targetTicket;

		 private volatile bool _halted;
		 private readonly AtomicInteger _targetTicket = new AtomicInteger();
		 private readonly AtomicInteger _currentTicket = new AtomicInteger();
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly Master _master;
		 private readonly Log _logger;
		 private readonly CappedLogger _invalidEpochCappedLogger;
		 private readonly CappedLogger _comExceptionCappedLogger;
		 private readonly LastUpdateTime _lastUpdateTime;
		 private readonly InstanceId _instanceId;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly InvalidEpochExceptionHandler _invalidEpochHandler;
		 private readonly Monitor _monitor;
		 private readonly IJobScheduler _jobScheduler;
		 private volatile Thread _updatePullingThread;
		 private volatile BinaryLatch _shutdownLatch; // Store under synchronised(this), load in update puller thread

		 internal SlaveUpdatePuller( RequestContextFactory requestContextFactory, Master master, LastUpdateTime lastUpdateTime, LogProvider logging, InstanceId instanceId, AvailabilityGuard availabilityGuard, InvalidEpochExceptionHandler invalidEpochHandler, IJobScheduler jobScheduler, Monitor monitor )
		 {
			  this._requestContextFactory = requestContextFactory;
			  this._master = master;
			  this._lastUpdateTime = lastUpdateTime;
			  this._instanceId = instanceId;
			  this._availabilityGuard = availabilityGuard;
			  this._invalidEpochHandler = invalidEpochHandler;
			  this._jobScheduler = jobScheduler;
			  this._monitor = monitor;
			  this._logger = logging.getLog( this.GetType() );
			  this._invalidEpochCappedLogger = ( new CappedLogger( _logger ) ).setCountLimit( LogCap );
			  this._comExceptionCappedLogger = ( new CappedLogger( _logger ) ).setCountLimit( LogCap );
		 }

		 public override void Run()
		 {
			  _updatePullingThread = Thread.CurrentThread;
			  string oldName = _updatePullingThread.Name;
			  _updatePullingThread.Name = UPDATE_PULLER_THREAD_PREFIX + _instanceId;
			  try
			  {
					PeriodicallyPullUpdates();
			  }
			  finally
			  {
					_updatePullingThread.Name = oldName;
					_updatePullingThread = null;
					_shutdownLatch.release();
			  }
		 }

		 public override void Cancelled( bool mayInterruptIfRunning )
		 {
			  _halted = true;
		 }

		 private void PeriodicallyPullUpdates()
		 {
			  while ( !_halted )
			  {
					int round = _targetTicket.get();
					if ( _currentTicket.get() < round )
					{
						 DoPullUpdates();
						 _currentTicket.set( round );
						 continue;
					}

					LockSupport.parkNanos( ParkNanos );
			  }
		 }

		 public override void Start()
		 {
			  if ( _shutdownLatch != null )
			  {
					return; // This SlaveUpdatePuller has already been initialised
			  }

			  _shutdownLatch = new BinaryLatch();
			  JobHandle handle = _jobScheduler.schedule( Group.PULL_UPDATES, this );
			  handle.RegisterCancelListener( this );
		 }

		 public override void Stop() // for removing throw declaration
		 {
			  if ( _shutdownLatch == null )
			  {
					return; // This SlaveUpdatePuller has already been shut down
			  }

			  Thread thread = _updatePullingThread;
			  _halted = true;
			  LockSupport.unpark( thread );
			  _shutdownLatch.await();
			  _shutdownLatch = null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pullUpdates() throws InterruptedException
		 public override void PullUpdates()
		 {
			  if ( !Active || !_availabilityGuard.isAvailable( AvailabilityAwaitMillis ) )
			  {
					return;
			  }

			  TryPullUpdates();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean tryPullUpdates() throws InterruptedException
		 public override bool TryPullUpdates()
		 {
			  return Await( _nextTicket, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pullUpdates(UpdatePuller_Condition condition, boolean strictlyAssertActive) throws InterruptedException
		 public override void PullUpdates( UpdatePuller_Condition condition, bool strictlyAssertActive )
		 {
			  Await( condition, strictlyAssertActive );
		 }

		 /// <summary>
		 /// Gets the update puller going, if it's not already going, and waits for the supplied condition to be
		 /// fulfilled as part of the update pulling happening.
		 /// </summary>
		 /// <param name="condition"> <seealso cref="UpdatePuller.Condition"/> to wait for. </param>
		 /// <param name="strictlyAssertActive"> if {@code true} then observing an inactive update puller, whether
		 /// <seealso cref="stop() halted"/>, will throw an <seealso cref="System.InvalidOperationException"/>,
		 /// otherwise if {@code false} just stop waiting and return {@code false}. </param>
		 /// <returns> whether or not the condition was met. If {@code strictlyAssertActive} either
		 /// {@code true} will be returned or exception thrown, if puller became inactive.
		 /// If {@code !strictlyAssertActive} and puller became inactive then {@code false} is returned. </returns>
		 /// <exception cref="InterruptedException"> if we were interrupted while awaiting the condition. </exception>
		 /// <exception cref="IllegalStateException"> if {@code strictlyAssertActive} and the update puller
		 /// became inactive while awaiting the condition. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean await(UpdatePuller_Condition condition, boolean strictlyAssertActive) throws InterruptedException
		 private bool Await( UpdatePuller_Condition condition, bool strictlyAssertActive )
		 {
			  if ( !CheckActive( strictlyAssertActive ) )
			  {
					return false;
			  }

			  int ticket = Poke();
			  while ( !condition.Evaluate( _currentTicket.get(), ticket ) )
			  {
					if ( !CheckActive( strictlyAssertActive ) )
					{
						 return false;
					}

					Thread.Sleep( 1 );
			  }
			  return true;
		 }

		 /// <returns> {@code true} if active, {@code false} if inactive and {@code !strictlyAssertActive} </returns>
		 /// <exception cref="IllegalStateException"> if inactive and {@code strictlyAssertActive}. </exception>
		 private bool CheckActive( bool strictlyAssertActive )
		 {
			  if ( !Active )
			  {
					// The update puller is observed as being inactive

					// The caller strictly requires the update puller to be active so should throw exception
					if ( strictlyAssertActive )
					{
						 throw new System.InvalidOperationException( this + " is not active" );
					}

					// The caller is OK with ignoring an inactive update puller, so just return
					return false;
			  }
			  return true;
		 }

		 private int Poke()
		 {
			  int result = this._targetTicket.incrementAndGet();
			  LockSupport.unpark( _updatePullingThread );
			  return result;
		 }

		 public virtual bool Active
		 {
			 get
			 {
				  return !_halted;
			 }
		 }

		 public override string ToString()
		 {
			  return "UpdatePuller[halted:" + _halted + ", current:" + _currentTicket + ", target:" + _targetTicket + "]";
		 }

		 private void DoPullUpdates()
		 {
			  try
			  {
					RequestContext context = _requestContextFactory.newRequestContext();
					using ( Response<Void> ignored = _master.pullUpdates( context ) )
					{
						 // Updates would be applied as part of response processing
						 _monitor.pulledUpdates( context.LastAppliedTransaction() );
					}
					_invalidEpochCappedLogger.reset();
					_comExceptionCappedLogger.reset();
			  }
			  catch ( InvalidEpochException e )
			  {
					_invalidEpochHandler.handle();
					_invalidEpochCappedLogger.warn( "Pull updates by " + this + " failed at the epoch check", e );
			  }
			  catch ( ComException e )
			  {
					_invalidEpochCappedLogger.warn( "Pull updates by " + this + " failed due to network error.", e );
			  }
			  catch ( Exception e )
			  {
					_logger.error( "Pull updates by " + this + " failed", e );
			  }
			  _lastUpdateTime.setLastUpdateTime( currentTimeMillis() );
		 }

	}

}