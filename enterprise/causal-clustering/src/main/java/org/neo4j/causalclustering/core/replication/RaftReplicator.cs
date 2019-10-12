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
namespace Org.Neo4j.causalclustering.core.replication
{

	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using LeaderInfo = Org.Neo4j.causalclustering.core.consensus.LeaderInfo;
	using LeaderListener = Org.Neo4j.causalclustering.core.consensus.LeaderListener;
	using LeaderLocator = Org.Neo4j.causalclustering.core.consensus.LeaderLocator;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ReplicationMonitor = Org.Neo4j.causalclustering.core.replication.monitoring.ReplicationMonitor;
	using LocalSessionPool = Org.Neo4j.causalclustering.core.replication.session.LocalSessionPool;
	using OperationContext = Org.Neo4j.causalclustering.core.replication.session.OperationContext;
	using TimeoutStrategy = Org.Neo4j.causalclustering.helper.TimeoutStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using UnavailableException = Org.Neo4j.Kernel.availability.UnavailableException;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// A replicator implementation suitable in a RAFT context. Will handle resending due to timeouts and leader switches.
	/// </summary>
	public class RaftReplicator : Replicator, LeaderListener
	{
		 private readonly MemberId _me;
		 private readonly Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;
		 private readonly ProgressTracker _progressTracker;
		 private readonly LocalSessionPool _sessionPool;
		 private readonly TimeoutStrategy _progressTimeoutStrategy;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly Log _log;
		 private readonly LocalDatabase _localDatabase;
		 private readonly ReplicationMonitor _replicationMonitor;
		 private readonly long _availabilityTimeoutMillis;
		 private readonly LeaderProvider _leaderProvider;

		 public RaftReplicator( LeaderLocator leaderLocator, MemberId me, Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, LocalSessionPool sessionPool, ProgressTracker progressTracker, TimeoutStrategy progressTimeoutStrategy, long availabilityTimeoutMillis, AvailabilityGuard availabilityGuard, LogProvider logProvider, LocalDatabase localDatabase, Monitors monitors )
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
//ORIGINAL LINE: private java.util.concurrent.Future<Object> replicate0(ReplicatedContent command, boolean trackResult, org.neo4j.causalclustering.identity.MemberId leader) throws ReplicationFailureException
		 private Future<object> Replicate0( ReplicatedContent command, bool trackResult, MemberId leader )
		 {
			  _replicationMonitor.startReplication();
			  try
			  {
					OperationContext session = _sessionPool.acquireSession();

					DistributedOperation operation = new DistributedOperation( command, session.GlobalSession(), session.LocalOperationId() );
					Progress progress = _progressTracker.start( operation );

					Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout progressTimeout = _progressTimeoutStrategy.newTimeout();
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
							  _outbound.send( leader, new Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _me, operation ), true );
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