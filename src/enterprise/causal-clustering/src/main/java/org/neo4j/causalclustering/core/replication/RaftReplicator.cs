using System;

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
namespace Neo4Net.causalclustering.core.replication
{

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using LeaderListener = Neo4Net.causalclustering.core.consensus.LeaderListener;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ReplicationMonitor = Neo4Net.causalclustering.core.replication.monitoring.ReplicationMonitor;
	using LocalSessionPool = Neo4Net.causalclustering.core.replication.session.LocalSessionPool;
	using OperationContext = Neo4Net.causalclustering.core.replication.session.OperationContext;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using UnavailableException = Neo4Net.Kernel.availability.UnavailableException;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// A replicator implementation suitable in a RAFT context. Will handle resending due to timeouts and leader switches.
	/// </summary>
	public class RaftReplicator : Replicator, LeaderListener
	{
		 private readonly MemberId _me;
		 private readonly Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;
		 private readonly ProgressTracker _progressTracker;
		 private readonly LocalSessionPool _sessionPool;
		 private readonly TimeoutStrategy _progressTimeoutStrategy;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly Log _log;
		 private readonly LocalDatabase _localDatabase;
		 private readonly ReplicationMonitor _replicationMonitor;
		 private readonly long _availabilityTimeoutMillis;
		 private readonly LeaderProvider _leaderProvider;

		 public RaftReplicator( LeaderLocator leaderLocator, MemberId me, Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, LocalSessionPool sessionPool, ProgressTracker progressTracker, TimeoutStrategy progressTimeoutStrategy, long availabilityTimeoutMillis, AvailabilityGuard availabilityGuard, LogProvider logProvider, LocalDatabase localDatabase, Monitors monitors )
		 {
			  this._me = me;
			  this._outbound = outbound;
			  this._progressTracker = progressTracker;
			  this._sessionPool = sessionPool;
			  this._progressTimeoutStrategy = progressTimeoutStrategy;
			  this._availabilityTimeoutMillis = availabilityTimeoutMillis;
			  this._availabilityGuard = availabilityGuard;
			  this._log = logProvider.getLog( this.GetType() );
			  this._localDatabase = localDatabase;
			  this._replicationMonitor = monitors.NewMonitor( typeof( ReplicationMonitor ) );
			  this._leaderProvider = new LeaderProvider();
			  leaderLocator.RegisterListener( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.concurrent.Future<Object> replicate(ReplicatedContent command, boolean trackResult) throws ReplicationFailureException
		 public override Future<object> Replicate( ReplicatedContent command, bool trackResult )
		 {
			  MemberId currentLeader = _leaderProvider.currentLeader();
			  if ( currentLeader == null )
			  {
					throw new ReplicationFailureException( "Replication aborted since no leader was available" );
			  }
			  return Replicate0( command, trackResult, currentLeader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Future<Object> replicate0(ReplicatedContent command, boolean trackResult, org.Neo4Net.causalclustering.identity.MemberId leader) throws ReplicationFailureException
		 private Future<object> Replicate0( ReplicatedContent command, bool trackResult, MemberId leader )
		 {
			  _replicationMonitor.startReplication();
			  try
			  {
					OperationContext session = _sessionPool.acquireSession();

					DistributedOperation operation = new DistributedOperation( command, session.GlobalSession(), session.LocalOperationId() );
					Progress progress = _progressTracker.start( operation );

					Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout progressTimeout = _progressTimeoutStrategy.newTimeout();
					int attempts = 0;
					try
					{
						 while ( true )
						 {
							  attempts++;
							  if ( attempts > 1 )
							  {
									_log.info( "Retrying replication. Current attempt: %d Content: %s", attempts, command );
							  }
							  _replicationMonitor.replicationAttempt();
							  AssertDatabaseAvailable();
							  // blocking at least until the send has succeeded or failed before retrying
							  _outbound.send( leader, new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _me, operation ), true );
							  progress.AwaitReplication( progressTimeout.Millis );
							  if ( progress.Replicated )
							  {
									break;
							  }
							  progressTimeout.Increment();
							  leader = _leaderProvider.awaitLeader();
						 }
					}
					catch ( InterruptedException e )
					{
						 _progressTracker.abort( operation );
						 throw new ReplicationFailureException( "Interrupted while replicating", e );
					}

					System.Action<object, Exception> cleanup = ( ignored1, ignored2 ) => _sessionPool.releaseSession( session );

					if ( trackResult )
					{
						 progress.FutureResult().whenComplete(cleanup);
					}
					else
					{
						 cleanup( null, null );
					}
					_replicationMonitor.successfulReplication();
					return progress.FutureResult();
			  }
			  catch ( Exception t )
			  {
					_replicationMonitor.failedReplication( t );
					throw t;
			  }

		 }

		 public override void OnLeaderSwitch( LeaderInfo leaderInfo )
		 {
			  _progressTracker.triggerReplicationEvent();
			  MemberId newLeader = leaderInfo.MemberId();
			  MemberId oldLeader = _leaderProvider.currentLeader();
			  if ( newLeader == null && oldLeader != null )
			  {
					_log.info( "Lost previous leader '%s'. Currently no available leader", oldLeader );
			  }
			  else if ( newLeader != null && oldLeader == null )
			  {
					_log.info( "A new leader has been detected: '%s'", newLeader );
			  }
			  _leaderProvider.Leader = newLeader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertDatabaseAvailable() throws ReplicationFailureException
		 private void AssertDatabaseAvailable()
		 {
			  _localDatabase.assertHealthy( typeof( ReplicationFailureException ) );
			  try
			  {
					_availabilityGuard.await( _availabilityTimeoutMillis );
			  }
			  catch ( UnavailableException e )
			  {
					throw new ReplicationFailureException( "Database is not available, transaction cannot be replicated.", e );
			  }
		 }
	}

}