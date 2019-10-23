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
namespace Neo4Net.causalclustering.core.replication
{
	using Matchers = org.hamcrest.Matchers;
	using Assertions = org.junit.jupiter.api.Assertions;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using StoreFiles = Neo4Net.causalclustering.catchup.storecopy.StoreFiles;
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ReplicatedInteger = Neo4Net.causalclustering.core.consensus.ReplicatedInteger;
	using ReplicationMonitor = Neo4Net.causalclustering.core.replication.monitoring.ReplicationMonitor;
	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using LocalSessionPool = Neo4Net.causalclustering.core.replication.session.LocalSessionPool;
	using Result = Neo4Net.causalclustering.core.state.Result;
	using ConstantTimeTimeoutStrategy = Neo4Net.causalclustering.helper.ConstantTimeTimeoutStrategy;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Message = Neo4Net.causalclustering.messaging.Message;
	using Neo4Net.causalclustering.messaging;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using UnavailableException = Neo4Net.Kernel.availability.UnavailableException;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertEventually;

	internal class RaftReplicatorTest
	{
		private bool InstanceFieldsInitialized = false;

		public RaftReplicatorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_session = new GlobalSession( System.Guid.randomUUID(), _myself );
			_sessionPool = new LocalSessionPool( _session );
		}

		 private const int DEFAULT_TIMEOUT_MS = 15_000;

		 private LeaderLocator _leaderLocator = mock( typeof( LeaderLocator ) );
		 private MemberId _myself = new MemberId( System.Guid.randomUUID() );
		 private LeaderInfo _leaderInfo = new LeaderInfo( new MemberId( System.Guid.randomUUID() ), 1 );
		 private GlobalSession _session;
		 private LocalSessionPool _sessionPool;
		 private TimeoutStrategy _noWaitTimeoutStrategy = new ConstantTimeTimeoutStrategy( 0, MILLISECONDS );
		 private DatabaseAvailabilityGuard _databaseAvailabilityGuard;
		 private DatabaseHealth _databaseHealth;
		 private LocalDatabase _localDatabase;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _databaseAvailabilityGuard = new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, Clocks.systemClock(), NullLog.Instance );
			  _databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLog.Instance );
			  _localDatabase = StubLocalDatabase.Create( () => _databaseHealth, _databaseAvailabilityGuard );
			  _localDatabase.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSendReplicatedContentToLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSendReplicatedContentToLeader()
		 {
			  // given
			  Monitors monitors = new Monitors();
			  ReplicationMonitor replicationMonitor = mock( typeof( ReplicationMonitor ) );
			  monitors.AddMonitorListener( replicationMonitor );
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, monitors );
			  replicator.OnLeaderSwitch( _leaderInfo );

			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  Thread replicatingThread = replicatingThread( replicator, content, false );

			  // when
			  replicatingThread.Start();
			  // then
			  assertEventually( "making progress", () => capturedProgress.Last, not(equalTo(null)), DEFAULT_TIMEOUT_MS, MILLISECONDS );

			  // when
			  capturedProgress.Last.setReplicated();

			  // then
			  replicatingThread.Join( DEFAULT_TIMEOUT_MS );
			  assertEquals( _leaderInfo.memberId(), outbound.LastTo );

			  verify( replicationMonitor, times( 1 ) ).startReplication();
			  verify( replicationMonitor, atLeast( 1 ) ).replicationAttempt();
			  verify( replicationMonitor, times( 1 ) ).successfulReplication();
			  verify( replicationMonitor, never() ).failedReplication(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResendAfterTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldResendAfterTimeout()
		 {
			  // given
			  Monitors monitors = new Monitors();
			  ReplicationMonitor replicationMonitor = mock( typeof( ReplicationMonitor ) );
			  monitors.AddMonitorListener( replicationMonitor );
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, monitors );
			  replicator.OnLeaderSwitch( _leaderInfo );

			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  Thread replicatingThread = replicatingThread( replicator, content, false );

			  // when
			  replicatingThread.Start();
			  // then
			  assertEventually( "send count", () => outbound.Count, greaterThan(2), DEFAULT_TIMEOUT_MS, MILLISECONDS );

			  // cleanup
			  capturedProgress.Last.setReplicated();
			  replicatingThread.Join( DEFAULT_TIMEOUT_MS );

			  verify( replicationMonitor, times( 1 ) ).startReplication();
			  verify( replicationMonitor, atLeast( 2 ) ).replicationAttempt();
			  verify( replicationMonitor, times( 1 ) ).successfulReplication();
			  verify( replicationMonitor, never() ).failedReplication(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseSessionWhenFinished() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReleaseSessionWhenFinished()
		 {
			  // given
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, new Monitors() );
			  replicator.OnLeaderSwitch( _leaderInfo );
			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  Thread replicatingThread = replicatingThread( replicator, content, true );

			  // when
			  replicatingThread.Start();

			  // then
			  assertEventually( "making progress", () => capturedProgress.Last, not(equalTo(null)), DEFAULT_TIMEOUT_MS, MILLISECONDS );
			  assertEquals( 1, _sessionPool.openSessionCount() );

			  // when
			  capturedProgress.Last.setReplicated();
			  capturedProgress.Last.futureResult().complete(5);
			  replicatingThread.Join( DEFAULT_TIMEOUT_MS );

			  // then
			  assertEquals( 0, _sessionPool.openSessionCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stopReplicationOnShutdown() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StopReplicationOnShutdown()
		 {
			  // given
			  Monitors monitors = new Monitors();
			  ReplicationMonitor replicationMonitor = mock( typeof( ReplicationMonitor ) );
			  monitors.AddMonitorListener( replicationMonitor );
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, monitors );
			  replicator.OnLeaderSwitch( _leaderInfo );
			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  ReplicatingThread replicatingThread = replicatingThread( replicator, content, true );

			  // when
			  replicatingThread.Start();

			  _databaseAvailabilityGuard.shutdown();
			  replicatingThread.Join();
			  assertThat( replicatingThread.ReplicationException.InnerException, Matchers.instanceOf( typeof( UnavailableException ) ) );

			  verify( replicationMonitor, times( 1 ) ).startReplication();
			  verify( replicationMonitor, atLeast( 1 ) ).replicationAttempt();
			  verify( replicationMonitor, never() ).successfulReplication();
			  verify( replicationMonitor, times( 1 ) ).failedReplication( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stopReplicationWhenUnavailable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StopReplicationWhenUnavailable()
		 {
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, new Monitors() );
			  replicator.OnLeaderSwitch( _leaderInfo );

			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  ReplicatingThread replicatingThread = replicatingThread( replicator, content, true );

			  // when
			  replicatingThread.Start();

			  _databaseAvailabilityGuard.require( () => "Database not unavailable" );
			  replicatingThread.Join();
			  assertThat( replicatingThread.ReplicationException.InnerException, Matchers.instanceOf( typeof( UnavailableException ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stopReplicationWhenUnHealthy() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StopReplicationWhenUnHealthy()
		 {
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, new Monitors() );
			  replicator.OnLeaderSwitch( _leaderInfo );

			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  ReplicatingThread replicatingThread = replicatingThread( replicator, content, true );

			  // when
			  replicatingThread.Start();

			  _databaseHealth.panic( new System.InvalidOperationException( "PANIC" ) );
			  replicatingThread.Join();
			  Assertions.assertNotNull( replicatingThread.ReplicationException );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailIfNoLeaderIsAvailable()
		 internal virtual void ShouldFailIfNoLeaderIsAvailable()
		 {
			  // given
			  CapturingProgressTracker capturedProgress = new CapturingProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, new Monitors() );
			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );

			  // when
			  assertThrows( typeof( ReplicationFailureException ), () => replicator.Replicate(content, true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldListenToLeaderUpdates() throws ReplicationFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldListenToLeaderUpdates()
		 {
			  OneProgressTracker oneProgressTracker = new OneProgressTracker( this );
			  oneProgressTracker.Last.setReplicated();
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();
			  RaftReplicator replicator = GetReplicator( outbound, oneProgressTracker, new Monitors() );
			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );

			  LeaderInfo lastLeader = _leaderInfo;

			  // set initial leader, sens to that leader
			  replicator.OnLeaderSwitch( lastLeader );
			  replicator.Replicate( content, false );
			  assertEquals( outbound.LastTo, lastLeader.MemberId() );

			  // update with valid new leader, sends to new leader
			  lastLeader = new LeaderInfo( new MemberId( System.Guid.randomUUID() ), 1 );
			  replicator.OnLeaderSwitch( lastLeader );
			  replicator.Replicate( content, false );
			  assertEquals( outbound.LastTo, lastLeader.MemberId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSuccessfullySendIfLeaderIsLostAndFound() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSuccessfullySendIfLeaderIsLostAndFound()
		 {
			  OneProgressTracker capturedProgress = new OneProgressTracker( this );
			  CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound = new CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();

			  RaftReplicator replicator = GetReplicator( outbound, capturedProgress, new Monitors() );
			  replicator.OnLeaderSwitch( _leaderInfo );

			  ReplicatedInteger content = ReplicatedInteger.ValueOf( 5 );
			  ReplicatingThread replicatingThread = replicatingThread( replicator, content, false );

			  // when
			  replicatingThread.Start();

			  // then
			  assertEventually( "send count", () => outbound.Count, greaterThan(1), DEFAULT_TIMEOUT_MS, MILLISECONDS );
			  replicator.OnLeaderSwitch( new LeaderInfo( null, 1 ) );
			  capturedProgress.Last.setReplicated();
			  replicator.OnLeaderSwitch( _leaderInfo );

			  replicatingThread.Join( DEFAULT_TIMEOUT_MS );
		 }

		 private RaftReplicator GetReplicator( CapturingOutbound<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, ProgressTracker progressTracker, Monitors monitors )
		 {
			  return new RaftReplicator( _leaderLocator, _myself, outbound, _sessionPool, progressTracker, _noWaitTimeoutStrategy, 10, _databaseAvailabilityGuard, NullLogProvider.Instance, _localDatabase, monitors );
		 }

		 private ReplicatingThread ReplicatingThread( RaftReplicator replicator, ReplicatedInteger content, bool trackResult )
		 {
			  return new ReplicatingThread( this, replicator, content, trackResult );
		 }

		 private class ReplicatingThread : Thread
		 {
			 private readonly RaftReplicatorTest _outerInstance;


			  internal readonly RaftReplicator Replicator;
			  internal readonly ReplicatedInteger Content;
			  internal readonly bool TrackResult;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile Exception ReplicationExceptionConflict;

			  internal ReplicatingThread( RaftReplicatorTest outerInstance, RaftReplicator replicator, ReplicatedInteger content, bool trackResult )
			  {
				  this._outerInstance = outerInstance;
					this.Replicator = replicator;
					this.Content = content;
					this.TrackResult = trackResult;
			  }

			  public override void Run()
			  {
					try
					{
						 Future<object> futureResult = Replicator.replicate( Content, TrackResult );
						 if ( TrackResult )
						 {
							  try
							  {
									futureResult.get();
							  }
							  catch ( ExecutionException e )
							  {
									ReplicationExceptionConflict = e;
									throw new System.InvalidOperationException();
							  }
						 }
					}
					catch ( Exception e )
					{
						 ReplicationExceptionConflict = e;
					}
			  }

			  internal virtual Exception ReplicationException
			  {
				  get
				  {
						return ReplicationExceptionConflict;
				  }
			  }
		 }

		 private class OneProgressTracker : ProgressTrackerAdaptor
		 {
			 private readonly RaftReplicatorTest _outerInstance;

			  internal OneProgressTracker( RaftReplicatorTest outerInstance ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Last = new Progress();
			  }

			  public override Progress Start( DistributedOperation operation )
			  {
					return Last;
			  }
		 }

		 private class CapturingProgressTracker : ProgressTrackerAdaptor
		 {
			 private readonly RaftReplicatorTest _outerInstance;

			 public CapturingProgressTracker( RaftReplicatorTest outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override Progress Start( DistributedOperation operation )
			  {
					Last = new Progress();
					return Last;
			  }
		 }

		 private abstract class ProgressTrackerAdaptor : ProgressTracker
		 {
			 public abstract Progress Start( DistributedOperation operation );
			 private readonly RaftReplicatorTest _outerInstance;

			 public ProgressTrackerAdaptor( RaftReplicatorTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  protected internal Progress Last;

			  public override void TrackReplication( DistributedOperation operation )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void TrackResult( DistributedOperation operation, Result result )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void Abort( DistributedOperation operation )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void TriggerReplicationEvent()
			  {
					// do nothing
			  }

			  public override int InProgressCount()
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 private class CapturingOutbound<MESSAGE> : Outbound<MemberId, MESSAGE> where MESSAGE : Neo4Net.causalclustering.messaging.Message
		 {
			  internal MemberId LastTo;
			  internal int Count;

			  public override void Send( MemberId to, MESSAGE message, bool block )
			  {
					this.LastTo = to;
					this.Count++;
			  }

		 }

		 private class StubLocalDatabase : LocalDatabase
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.Neo4Net.causalclustering.catchup.storecopy.LocalDatabase create(System.Func<org.Neo4Net.kernel.internal.DatabaseHealth> databaseHealthSupplier, org.Neo4Net.kernel.availability.AvailabilityGuard availabilityGuard) throws java.io.IOException
			  internal static LocalDatabase Create( System.Func<DatabaseHealth> databaseHealthSupplier, AvailabilityGuard availabilityGuard )
			  {
					StoreFiles storeFiles = mock( typeof( StoreFiles ) );
					when( storeFiles.ReadStoreId( any() ) ).thenReturn(new StoreId(1, 2, 3, 4));

					DataSourceManager dataSourceManager = mock( typeof( DataSourceManager ) );
					return new StubLocalDatabase( storeFiles, dataSourceManager, databaseHealthSupplier, availabilityGuard );
			  }

			  internal StubLocalDatabase( StoreFiles storeFiles, DataSourceManager dataSourceManager, System.Func<DatabaseHealth> databaseHealthSupplier, AvailabilityGuard availabilityGuard ) : base( null, storeFiles, null, dataSourceManager, databaseHealthSupplier, availabilityGuard, NullLogProvider.Instance )
			  {
			  }
		 }
	}

}