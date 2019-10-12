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
namespace Org.Neo4j.causalclustering.core.consensus.shipping
{

	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Org.Neo4j.causalclustering.core.consensus.log.ReadableRaftLog;
	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using Timer = Org.Neo4j.causalclustering.core.consensus.schedule.Timer;
	using TimerService = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService;
	using InFlightLogEntryReader = Org.Neo4j.causalclustering.core.state.InFlightLogEntryReader;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Group = Org.Neo4j.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.TimeoutFactory.fixedTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.Timer.CancelMode.ASYNC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.PIPELINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Timeouts.RESEND;

	/// Optimizations
	// TODO: Have several outstanding batches in catchup mode, to bridge the latency gap.
	// TODO: Bisect search for mismatch.
	// TODO: Maximum bound on size of batch in bytes, not just entry count.

	// Production ready

	// Core functionality
	// TODO: Consider making even CommitUpdate a raft-message of its own.

	/// <summary>
	/// This class handles the shipping of raft logs from this node when it is the leader to the followers.
	/// Each instance handles a single follower and acts on events and associated state updates originating
	/// within the main raft state machine.
	/// <para>
	/// It is crucial that all actions happen within the context of the leaders state at some point in time.
	/// </para>
	/// </summary>
	public class RaftLogShipper
	{
		 private const long MIN_INDEX = 1L; // we never ship entry zero, which must be bootstrapped or received as part of a snapshot
		 private readonly int _timerInactive = 0;

		 internal enum Mode
		 {
			  /// <summary>
			  /// In the mismatch mode we are unsure about the follower state, thus
			  /// we tread with caution, going backwards trying to find the point where
			  /// our logs match. We send empty append entries to minimize the cost of
			  /// this mode.
			  /// </summary>
			  Mismatch,
			  /// <summary>
			  /// In the catchup mode we are trying to catch up the follower as quickly
			  /// as possible. The follower receives batches of entries in series until
			  /// it is fully caught up.
			  /// </summary>
			  Catchup,
			  /// <summary>
			  /// In the pipeline mode the follower is treated as caught up and we
			  /// optimistically ship any latest entries without waiting for responses,
			  /// expecting successful responses.
			  /// </summary>
			  Pipeline
		 }

		 public enum Timeouts
		 {
			  Resend
		 }

		 private readonly Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;
		 private readonly Log _log;
		 private readonly ReadableRaftLog _raftLog;
		 private readonly Clock _clock;
		 private readonly MemberId _follower;
		 private readonly MemberId _leader;
		 private readonly long _retryTimeMillis;
		 private readonly int _catchupBatchSize;
		 private readonly int _maxAllowedShippingLag;
		 private readonly InFlightCache _inFlightCache;

		 private TimerService _timerService;
		 private Timer _timer;
		 private long _timeoutAbsoluteMillis;
		 private long _lastSentIndex;
		 private long _matchIndex = -1;
		 private LeaderContext _lastLeaderContext;
		 private Mode _mode = Mode.Mismatch;

		 internal RaftLogShipper( Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, LogProvider logProvider, ReadableRaftLog raftLog, Clock clock, TimerService timerService, MemberId leader, MemberId follower, long leaderTerm, long leaderCommit, long retryTimeMillis, int catchupBatchSize, int maxAllowedShippingLag, InFlightCache inFlightCache )
		 {
			  this._outbound = outbound;
			  this._timerService = timerService;
			  this._catchupBatchSize = catchupBatchSize;
			  this._maxAllowedShippingLag = maxAllowedShippingLag;
			  this._log = logProvider.getLog( this.GetType() );
			  this._raftLog = raftLog;
			  this._clock = clock;
			  this._follower = follower;
			  this._leader = leader;
			  this._retryTimeMillis = retryTimeMillis;
			  this._lastLeaderContext = new LeaderContext( leaderTerm, leaderCommit );
			  this._inFlightCache = inFlightCache;
		 }

		 public virtual object Identity()
		 {
			  return _follower;
		 }

		 public virtual void Start()
		 {
			 lock ( this )
			 {
				  _log.info( "Starting log shipper: %s", StatusAsString() );
				  SendEmpty( _raftLog.appendIndex(), _lastLeaderContext );
			 }
		 }

		 public virtual void Stop()
		 {
			 lock ( this )
			 {
				  _log.info( "Stopping log shipper %s", StatusAsString() );
				  AbortTimeout();
			 }
		 }

		 public virtual void OnMismatch( long lastRemoteAppendIndex, LeaderContext leaderContext )
		 {
			 lock ( this )
			 {
				  switch ( _mode )
				  {
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Mismatch:
							 long logIndex = max( min( _lastSentIndex - 1, lastRemoteAppendIndex ), MIN_INDEX );
							 SendEmpty( logIndex, leaderContext );
							 break;
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Pipeline:
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Catchup:
							 _log.info( "%s: mismatch in mode %s from follower %s, moving to MISMATCH mode", StatusAsString(), _mode, _follower );
							 _mode = Mode.Mismatch;
							 SendEmpty( _lastSentIndex, leaderContext );
							 break;
      
						default:
							 throw new System.InvalidOperationException( "Unknown mode: " + _mode );
				  }
      
				  _lastLeaderContext = leaderContext;
			 }
		 }

		 public virtual void OnMatch( long newMatchIndex, LeaderContext leaderContext )
		 {
			 lock ( this )
			 {
				  bool progress = newMatchIndex > _matchIndex;
				  if ( newMatchIndex > _matchIndex )
				  {
						_matchIndex = newMatchIndex;
				  }
				  else
				  {
						_log.warn( "%s: match index not progressing. This should be transient.", StatusAsString() );
				  }
      
				  switch ( _mode )
				  {
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Mismatch:
							 if ( SendNextBatchAfterMatch( leaderContext ) )
							 {
								  _log.info( "%s: caught up after mismatch, moving to PIPELINE mode", StatusAsString() );
								  _mode = PIPELINE;
							 }
							 else
							 {
								  _log.info( "%s: starting catch up after mismatch, moving to CATCHUP mode", StatusAsString() );
								  _mode = Mode.Catchup;
							 }
							 break;
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Catchup:
							 if ( _matchIndex >= _lastSentIndex )
							 {
								  if ( SendNextBatchAfterMatch( leaderContext ) )
								  {
										_log.info( "%s: caught up, moving to PIPELINE mode", StatusAsString() );
										_mode = PIPELINE;
								  }
							 }
							 break;
						case Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShipper.Mode.Pipeline:
							 if ( _matchIndex == _lastSentIndex )
							 {
								  AbortTimeout();
							 }
							 else if ( progress )
							 {
								  ScheduleTimeout( _retryTimeMillis );
							 }
							 break;
      
						default:
							 throw new System.InvalidOperationException( "Unknown mode: " + _mode );
				  }
      
				  _lastLeaderContext = leaderContext;
			 }
		 }

		 public virtual void OnNewEntries( long prevLogIndex, long prevLogTerm, RaftLogEntry[] newLogEntries, LeaderContext leaderContext )
		 {
			 lock ( this )
			 {
				  if ( _mode == Mode.Pipeline )
				  {
						while ( _lastSentIndex <= prevLogIndex )
						{
							 if ( prevLogIndex - _matchIndex <= _maxAllowedShippingLag )
							 {
								  // all sending functions update lastSentIndex
								  SendNewEntries( prevLogIndex, prevLogTerm, newLogEntries, leaderContext );
							 }
							 else
							 {
								  /* The timer is still set at this point. Either we will send the next batch
								   * as soon as the follower has caught up with the last pipelined entry,
								   * or when we timeout and resend. */
								  _log.info( "%s: follower has fallen behind (target prevLogIndex was %d, maxAllowedShippingLag " + "is %d), moving to CATCHUP mode", StatusAsString(), prevLogIndex, _maxAllowedShippingLag );
								  _mode = Mode.Catchup;
								  break;
							 }
						}
				  }
      
				  _lastLeaderContext = leaderContext;
			 }
		 }

		 public virtual void OnCommitUpdate( LeaderContext leaderContext )
		 {
			 lock ( this )
			 {
				  if ( _mode == Mode.Pipeline )
				  {
						SendCommitUpdate( leaderContext );
				  }
      
				  _lastLeaderContext = leaderContext;
			 }
		 }

		 /// <summary>
		 /// Callback invoked by the external timer service.
		 /// </summary>
		 private void OnScheduledTimeoutExpiry()
		 {
			 lock ( this )
			 {
				  if ( TimedOut() )
				  {
						OnTimeout();
				  }
				  else if ( _timeoutAbsoluteMillis != _timerInactive )
				  {
						/* Timer was moved, so we need to reschedule. */
						long timeLeft = _timeoutAbsoluteMillis - _clock.millis();
						if ( timeLeft > 0 )
						{
							 ScheduleTimeout( timeLeft );
						}
						else
						{
							 /* However it managed to expire, so we can just handle it immediately. */
							 OnTimeout();
						}
				  }
			 }
		 }

		 internal virtual void OnTimeout()
		 {
			  if ( _mode == PIPELINE )
			  {
					/* The follower seems unresponsive and we do not want to spam it with new entries.
					 * The catchup will pick-up when the last sent pipelined entry matches. */
					_log.info( "%s: timed out, moving to CATCHUP mode", StatusAsString() );
					_mode = Mode.Catchup;
					ScheduleTimeout( _retryTimeMillis );
			  }
			  else if ( _mode == CATCHUP )
			  {
					/* The follower seems unresponsive so we move back to mismatch mode to
					 * slowly poke it and figure out what is going on. Catchup will resume
					 * on the next match. */
					_log.info( "%s: timed out, moving to MISMATCH mode", StatusAsString() );
					_mode = Mode.Mismatch;
			  }

			  if ( _lastLeaderContext != null )
			  {
					SendEmpty( _lastSentIndex, _lastLeaderContext );
			  }
		 }

		 /// <summary>
		 /// This function is necessary because the scheduled callback blocks on the monitor before
		 /// entry and the expiry time of the timer might have been moved or even cancelled before
		 /// the entry is granted.
		 /// </summary>
		 /// <returns> True if we actually timed out, otherwise false. </returns>
		 private bool TimedOut()
		 {
			  return _timeoutAbsoluteMillis != _timerInactive && ( _clock.millis() - _timeoutAbsoluteMillis ) >= 0;
		 }

		 private void ScheduleTimeout( long deltaMillis )
		 {
			  _timeoutAbsoluteMillis = _clock.millis() + deltaMillis;

			  if ( _timer == null )
			  {
					_timer = _timerService.create( RESEND, Group.RAFT_TIMER, timeout => onScheduledTimeoutExpiry() );
			  }

			  _timer.set( fixedTimeout( deltaMillis, MILLISECONDS ) );
		 }

		 private void AbortTimeout()
		 {
			  if ( _timer != null )
			  {
					_timer.cancel( ASYNC );
			  }
			  _timeoutAbsoluteMillis = _timerInactive;
		 }

		 /// <summary>
		 /// Returns true if this sent the last batch.
		 /// </summary>
		 private bool SendNextBatchAfterMatch( LeaderContext leaderContext )
		 {
			  long lastIndex = _raftLog.appendIndex();

			  if ( lastIndex > _matchIndex )
			  {
					long endIndex = min( lastIndex, _matchIndex + _catchupBatchSize );

					ScheduleTimeout( _retryTimeMillis );
					SendRange( _matchIndex + 1, endIndex, leaderContext );
					return endIndex == lastIndex;
			  }
			  else
			  {
					return true;
			  }
		 }

		 private void SendCommitUpdate( LeaderContext leaderContext )
		 {
			  /*
			   * This is a commit update. That means that we just received enough success responses to an append
			   * request to allow us to send a commit. By Raft invariants, this means that the term for the committed
			   * entry is the current term.
			   */
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat appendRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat( _leader, leaderContext.Term, leaderContext.CommitIndex, leaderContext.Term );

			  _outbound.send( _follower, appendRequest );
		 }

		 private void SendNewEntries( long prevLogIndex, long prevLogTerm, RaftLogEntry[] newEntries, LeaderContext leaderContext )
		 {
			  ScheduleTimeout( _retryTimeMillis );

			  _lastSentIndex = prevLogIndex + 1;

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, leaderContext.Term, prevLogIndex, prevLogTerm, newEntries, leaderContext.CommitIndex );

			  _outbound.send( _follower, appendRequest );
		 }

		 private void SendEmpty( long logIndex, LeaderContext leaderContext )
		 {
			  ScheduleTimeout( _retryTimeMillis );

			  logIndex = max( _raftLog.prevIndex() + 1, logIndex );
			  _lastSentIndex = logIndex;

			  try
			  {
					long prevLogIndex = logIndex - 1;
					long prevLogTerm = _raftLog.readEntryTerm( prevLogIndex );

					if ( prevLogTerm > leaderContext.Term )
					{
						 _log.warn( "%s: aborting send. Not leader anymore? %s, prevLogTerm=%d", StatusAsString(), leaderContext, prevLogTerm );
						 return;
					}

					if ( DoesNotExistInLog( prevLogIndex, prevLogTerm ) )
					{
						 _log.warn( "%s: Entry was pruned when sending empty (prevLogIndex=%d, prevLogTerm=%d)", StatusAsString(), prevLogIndex, prevLogTerm );
						 return;
					}

					Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, leaderContext.Term, prevLogIndex, prevLogTerm, RaftLogEntry.empty, leaderContext.CommitIndex );
					_outbound.send( _follower, appendRequest );
			  }
			  catch ( IOException e )
			  {
					_log.warn( StatusAsString() + " exception during empty send", e );
			  }
		 }

		 private void SendRange( long startIndex, long endIndex, LeaderContext leaderContext )
		 {
			  if ( startIndex > endIndex )
			  {
					return;
			  }

			  _lastSentIndex = endIndex;

			  try
			  {
					int batchSize = ( int )( endIndex - startIndex + 1 );
					RaftLogEntry[] entries = new RaftLogEntry[batchSize];

					long prevLogIndex = startIndex - 1;
					long prevLogTerm = _raftLog.readEntryTerm( prevLogIndex );

					if ( prevLogTerm > leaderContext.Term )
					{
						 _log.warn( "%s aborting send. Not leader anymore? %s, prevLogTerm=%d", StatusAsString(), leaderContext, prevLogTerm );
						 return;
					}

					bool entryMissing = false;
					using ( InFlightLogEntryReader logEntrySupplier = new InFlightLogEntryReader( _raftLog, _inFlightCache, false ) )
					{
						 for ( int offset = 0; offset < batchSize; offset++ )
						 {
							  entries[offset] = logEntrySupplier.Get( startIndex + offset );
							  if ( entries[offset] == null )
							  {
									entryMissing = true;
									break;
							  }
							  if ( entries[offset].Term() > leaderContext.Term )
							  {
									_log.warn( "%s aborting send. Not leader anymore? %s, entryTerm=%d", StatusAsString(), leaderContext, entries[offset].Term() );
									return;
							  }
						 }
					}

					if ( entryMissing || DoesNotExistInLog( prevLogIndex, prevLogTerm ) )
					{
						 if ( _raftLog.prevIndex() >= prevLogIndex )
						 {
							  SendLogCompactionInfo( leaderContext );
						 }
						 else
						 {
							  _log.error( "%s: Could not send compaction info and entries were missing, but log is not behind.", StatusAsString() );
						 }
					}
					else
					{
						 Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, leaderContext.Term, prevLogIndex, prevLogTerm, entries, leaderContext.CommitIndex );

						 _outbound.send( _follower, appendRequest );
					}
			  }
			  catch ( IOException e )
			  {
					_log.warn( StatusAsString() + " exception during batch send", e );
			  }
		 }

		 private bool DoesNotExistInLog( long logIndex, long logTerm )
		 {
			  return logTerm == -1 && logIndex != -1;
		 }

		 private void SendLogCompactionInfo( LeaderContext leaderContext )
		 {
			  _log.warn( "Sending log compaction info. Log pruned? Status=%s, LeaderContext=%s", StatusAsString(), leaderContext );

			  _outbound.send( _follower, new Org.Neo4j.causalclustering.core.consensus.RaftMessages_LogCompactionInfo( _leader, leaderContext.Term, _raftLog.prevIndex() ) );
		 }

		 private string StatusAsString()
		 {
			  return format( "%s[matchIndex: %d, lastSentIndex: %d, localAppendIndex: %d, mode: %s]", _follower, _matchIndex, _lastSentIndex, _raftLog.appendIndex(), _mode );
		 }
	}

}