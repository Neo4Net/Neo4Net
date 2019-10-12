using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.core.state
{

	using RaftLog = Org.Neo4j.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLogCommitIndexMonitor = Org.Neo4j.causalclustering.core.consensus.log.monitoring.RaftLogCommitIndexMonitor;
	using DistributedOperation = Org.Neo4j.causalclustering.core.replication.DistributedOperation;
	using ProgressTracker = Org.Neo4j.causalclustering.core.replication.ProgressTracker;
	using CoreReplicatedContent = Org.Neo4j.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;
	using StatUtil = Org.Neo4j.causalclustering.helper.StatUtil;
	using Org.Neo4j.Function;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	public class CommandApplicationProcess
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_applierState = new ApplierState( this );
		}

		 private const long NOTHING = -1;
		 private readonly RaftLog _raftLog;
		 private readonly int _flushEvery;
		 private readonly ProgressTracker _progressTracker;
		 private readonly SessionTracker _sessionTracker;
		 private readonly System.Func<DatabaseHealth> _dbHealth;
		 private readonly InFlightCache _inFlightCache;
		 private readonly Log _log;
		 private readonly CoreState _coreState;
		 private readonly RaftLogCommitIndexMonitor _commitIndexMonitor;
		 private readonly CommandBatcher _batcher;
		 private readonly StatUtil.StatContext _batchStat;

		 private long _lastFlushed = NOTHING;
		 private int _pauseCount = 1; // we are created in the paused state
		 private Thread _applierThread;
		 private ApplierState _applierState;

		 public CommandApplicationProcess( RaftLog raftLog, int maxBatchSize, int flushEvery, System.Func<DatabaseHealth> dbHealth, LogProvider logProvider, ProgressTracker progressTracker, SessionTracker sessionTracker, CoreState coreState, InFlightCache inFlightCache, Monitors monitors )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._raftLog = raftLog;
			  this._flushEvery = flushEvery;
			  this._progressTracker = progressTracker;
			  this._sessionTracker = sessionTracker;
			  this._log = logProvider.getLog( this.GetType() );
			  this._dbHealth = dbHealth;
			  this._coreState = coreState;
			  this._inFlightCache = inFlightCache;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  this._commitIndexMonitor = monitors.NewMonitor( typeof( RaftLogCommitIndexMonitor ), this.GetType().FullName );
			  this._batcher = new CommandBatcher( maxBatchSize, this.applyBatch );
			  this._batchStat = StatUtil.create( "BatchSize", _log, 4096, true );
		 }

		 internal virtual void NotifyCommitted( long commitIndex )
		 {
			  _applierState.notifyCommitted( commitIndex );
		 }

		 private class ApplierState
		 {
			 private readonly CommandApplicationProcess _outerInstance;

			 public ApplierState( CommandApplicationProcess outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  // core applier state, synchronized by ApplierState monitor
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long LastSeenCommitIndexConflict = NOTHING;

			  // owned by applier
			  internal volatile long LastApplied = NOTHING;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool PanicConflict;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool KeepRunningConflict = true; // clear to shutdown the apply job

			  internal virtual long LastSeenCommitIndex
			  {
				  get
				  {
					  lock ( this )
					  {
							return LastSeenCommitIndexConflict;
					  }
				  }
			  }

			  internal virtual void Panic()
			  {
					PanicConflict = true;
					KeepRunningConflict = false;
			  }

			  internal virtual bool KeepRunning
			  {
				  set
				  {
					  lock ( this )
					  {
							if ( PanicConflict )
							{
								 throw new System.InvalidOperationException( "The applier has panicked" );
							}
         
							this.KeepRunningConflict = value;
							Monitor.PulseAll( this );
					  }
				  }
			  }

			  internal virtual long AwaitJob()
			  {
				  lock ( this )
				  {
						while ( LastApplied >= LastSeenCommitIndexConflict && KeepRunningConflict )
						{
							 outerInstance.ignoringInterrupts( this.wait );
						}
						return LastSeenCommitIndexConflict;
				  }
			  }

			  internal virtual void NotifyCommitted( long commitIndex )
			  {
				  lock ( this )
				  {
						if ( LastSeenCommitIndexConflict < commitIndex )
						{
							 LastSeenCommitIndexConflict = commitIndex;
							 outerInstance.commitIndexMonitor.CommitIndex( commitIndex );
							 Monitor.PulseAll( this );
						}
				  }
			  }
		 }

		 private void ApplyJob()
		 {
			  while ( _applierState.keepRunning )
			  {
					try
					{
						 ApplyUpTo( _applierState.awaitJob() );
					}
					catch ( Exception e )
					{
						 _applierState.panic();
						 _log.error( "Failed to apply", e );
						 _dbHealth.get().panic(e);
						 return; // LET THREAD DIE
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyUpTo(long applyUpToIndex) throws Exception
		 private void ApplyUpTo( long applyUpToIndex )
		 {
			  using ( InFlightLogEntryReader logEntrySupplier = new InFlightLogEntryReader( _raftLog, _inFlightCache, true ) )
			  {
					for ( long logIndex = _applierState.lastApplied + 1; _applierState.keepRunning && logIndex <= applyUpToIndex; logIndex++ )
					{
						 RaftLogEntry entry = logEntrySupplier.Get( logIndex );
						 if ( entry == null )
						 {
							  throw new System.InvalidOperationException( format( "Committed log entry at index %d must exist.", logIndex ) );
						 }

						 if ( entry.Content() is DistributedOperation )
						 {
							  DistributedOperation distributedOperation = ( DistributedOperation ) entry.Content();
							  _progressTracker.trackReplication( distributedOperation );
							  _batcher.add( logIndex, distributedOperation );
						 }
						 else
						 {
							  _batcher.flush();
							  // since this last entry didn't get in the batcher we need to update the lastApplied:
							  _applierState.lastApplied = logIndex;
						 }
					}
					_batcher.flush();
			  }
		 }

		 public virtual long LastApplied()
		 {
			  return _applierState.lastApplied;
		 }

		 /// <summary>
		 /// The applier must be paused when installing a snapshot.
		 /// </summary>
		 /// <param name="coreSnapshot"> The snapshot to install. </param>
		 internal virtual void InstallSnapshot( CoreSnapshot coreSnapshot )
		 {
			  Debug.Assert( _pauseCount > 0 );
			  _applierState.lastApplied = _lastFlushed = coreSnapshot.PrevIndex();
		 }

		 internal virtual long LastFlushed()
		 {
			 lock ( this )
			 {
				  return _lastFlushed;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyBatch(long lastIndex, java.util.List<org.neo4j.causalclustering.core.replication.DistributedOperation> batch) throws Exception
		 private void ApplyBatch( long lastIndex, IList<DistributedOperation> batch )
		 {
			  if ( batch.Count == 0 )
			  {
					return;
			  }

			  _batchStat.collect( batch.Count );

			  long startIndex = lastIndex - batch.Count + 1;
			  long lastHandledIndex = HandleOperations( startIndex, batch );
			  Debug.Assert( lastHandledIndex == lastIndex );
			  _applierState.lastApplied = lastIndex;

			  MaybeFlushToDisk();
		 }

		 private long HandleOperations( long commandIndex, IList<DistributedOperation> operations )
		 {
			  using ( CommandDispatcher dispatcher = _coreState.commandDispatcher() )
			  {
					foreach ( DistributedOperation operation in operations )
					{
						 if ( !_sessionTracker.validateOperation( operation.GlobalSession(), operation.OperationId() ) )
						 {
							  _sessionTracker.validateOperation( operation.GlobalSession(), operation.OperationId() );
							  commandIndex++;
							  continue;
						 }

						 CoreReplicatedContent command = ( CoreReplicatedContent ) operation.Content();
						 command.Dispatch( dispatcher, commandIndex, result => _progressTracker.trackResult( operation, result ) );

						 _sessionTracker.update( operation.GlobalSession(), operation.OperationId(), commandIndex );
						 commandIndex++;
					}
			  }
			  return commandIndex - 1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void maybeFlushToDisk() throws java.io.IOException
		 private void MaybeFlushToDisk()
		 {
			  if ( ( _applierState.lastApplied - _lastFlushed ) > _flushEvery )
			  {
					_coreState.flush( _applierState.lastApplied );
					_lastFlushed = _applierState.lastApplied;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void start() throws Exception
		 public virtual void Start()
		 {
			 lock ( this )
			 {
				  // TODO: check None/Partial/Full here, because this is the first level which can
				  // TODO: bootstrapping RAFT can also be performed from here.
      
				  if ( _lastFlushed == NOTHING )
				  {
						_lastFlushed = _coreState.LastFlushed;
				  }
				  _applierState.lastApplied = _lastFlushed;
      
				  _log.info( format( "Restoring last applied index to %d", _lastFlushed ) );
				  _sessionTracker.start();
      
				  /* Considering the order in which state is flushed, the state machines will
				   * always be furthest ahead and indicate the furthest possible state to
				   * which we must replay to reach a consistent state. */
				  long lastPossiblyApplying = max( _coreState.LastAppliedIndex, _applierState.LastSeenCommitIndex );
      
				  if ( lastPossiblyApplying > _applierState.lastApplied )
				  {
						_log.info( "Applying up to: " + lastPossiblyApplying );
						ApplyUpTo( lastPossiblyApplying );
				  }
      
				  ResumeApplier( "startup" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws java.io.IOException
		 public virtual void Stop()
		 {
			 lock ( this )
			 {
				  PauseApplier( "shutdown" );
				  _coreState.flush( _applierState.lastApplied );
			 }
		 }

		 private void SpawnApplierThread()
		 {
			  _applierState.KeepRunning = true;
			  _applierThread = new Thread( this.applyJob, "core-state-applier" );
			  _applierThread.Start();
		 }

		 private void StopApplierThread()
		 {
			  _applierState.KeepRunning = false;
			  IgnoringInterrupts( () => _applierThread.Join() );
		 }

		 public virtual void PauseApplier( string reason )
		 {
			 lock ( this )
			 {
				  if ( _pauseCount < 0 )
				  {
						throw new System.InvalidOperationException( "Unmatched pause/resume" );
				  }
      
				  _pauseCount++;
				  _log.info( format( "Pausing due to %s (count = %d)", reason, _pauseCount ) );
      
				  if ( _pauseCount == 1 )
				  {
						StopApplierThread();
				  }
			 }
		 }

		 public virtual void ResumeApplier( string reason )
		 {
			 lock ( this )
			 {
				  if ( _pauseCount <= 0 )
				  {
						throw new System.InvalidOperationException( "Unmatched pause/resume" );
				  }
      
				  _pauseCount--;
				  _log.info( format( "Resuming after %s (count = %d)", reason, _pauseCount ) );
      
				  if ( _pauseCount == 0 )
				  {
						SpawnApplierThread();
				  }
			 }
		 }

		 /// <summary>
		 /// We do not expect the interrupt system to be used here,
		 /// so we ignore them and log a warning.
		 /// </summary>
		 private void IgnoringInterrupts( ThrowingAction<InterruptedException> action )
		 {
			  try
			  {
					action.Apply();
			  }
			  catch ( InterruptedException e )
			  {
					_log.warn( "Unexpected interrupt", e );
			  }
		 }
	}

}