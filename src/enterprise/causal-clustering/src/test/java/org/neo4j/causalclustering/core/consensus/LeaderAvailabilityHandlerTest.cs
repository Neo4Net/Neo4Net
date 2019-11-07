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
namespace Neo4Net.causalclustering.core.consensus
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class LeaderAvailabilityHandlerTest
	{
		private bool InstanceFieldsInitialized = false;

		public LeaderAvailabilityHandlerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_handler = new LeaderAvailabilityHandler( @delegate, _leaderAvailabilityTimers, _raftMessageTimerResetMonitor, _term );
			_heartbeat = RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), _clusterId, new RaftMessages_Heartbeat(_leader, _term.AsLong, 0, 0) );
			_voteResponse = RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), _clusterId, new RaftMessages_Vote_Response(_leader, _term.AsLong, false) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Neo4Net.causalclustering.messaging.LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> delegate = org.mockito.Mockito.mock(Neo4Net.causalclustering.messaging.LifecycleMessageHandler.class);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> @delegate = Mockito.mock( typeof( LifecycleMessageHandler ) );
		 private LeaderAvailabilityTimers _leaderAvailabilityTimers = Mockito.mock( typeof( LeaderAvailabilityTimers ) );
		 private ClusterId _clusterId = new ClusterId( System.Guid.randomUUID() );
		 private RaftMessageTimerResetMonitor _raftMessageTimerResetMonitor = new DurationSinceLastMessageMonitor();
		 private System.Func<long> _term = () => 3;

		 private LeaderAvailabilityHandler _handler;

		 private MemberId _leader = new MemberId( System.Guid.randomUUID() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private RaftMessages_ReceivedInstantClusterIdAwareMessage<?> heartbeat = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new RaftMessages_Heartbeat(leader, term.getAsLong(), 0, 0));
		 private RaftMessages_ReceivedInstantClusterIdAwareMessage<object> _heartbeat;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private RaftMessages_ReceivedInstantClusterIdAwareMessage<?> appendEntries = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new RaftMessages_AppendEntries_Request(leader, term.getAsLong(), 0, 0, Neo4Net.causalclustering.core.consensus.log.RaftLogEntry.empty, 0)
		 private RaftMessages_ReceivedInstantClusterIdAwareMessage<object> appendEntries = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(Instant.now(), _clusterId, new RaftMessages_AppendEntries_Request(_leader, _term.AsLong, 0, 0, RaftLogEntry.empty, 0)
				  );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private RaftMessages_ReceivedInstantClusterIdAwareMessage<?> voteResponse = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new RaftMessages_Vote_Response(leader, term.getAsLong(), false));
		 private RaftMessages_ReceivedInstantClusterIdAwareMessage<object> _voteResponse = RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), _clusterId, new RaftMessages_Vote_Response(_leader, _term.AsLong, false) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRenewElectionForHeartbeats() throws Throwable
		 public void shouldRenewElectionForHeartbeats() throws Exception
		 {
			  // given
			  _handler.start( _clusterId );

			  // when
			  _handler.handle( _heartbeat );

			  // then
			  verify( _leaderAvailabilityTimers ).renewElection();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRenewElectionForAppendEntriesRequests() throws Throwable
		 public void shouldRenewElectionForAppendEntriesRequests() throws Exception
		 {
			  // given
			  _handler.start( _clusterId );

			  // when
			  _handler.handle( appendEntries );

			  // then
			  verify( _leaderAvailabilityTimers ).renewElection();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRenewElectionForOtherMessages() throws Throwable
		 public void shouldNotRenewElectionForOtherMessages() throws Exception
		 {
			  // given
			  _handler.start( _clusterId );

			  // when
			  _handler.handle( _voteResponse );

			  // then
			  verify( _leaderAvailabilityTimers, Mockito.never() ).renewElection();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRenewElectionTimeoutsForHeartbeatsFromEarlierTerm() throws Throwable
		 public void shouldNotRenewElectionTimeoutsForHeartbeatsFromEarlierTerm() throws Exception
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: RaftMessages_ReceivedInstantClusterIdAwareMessage<?> heartbeat = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new RaftMessages_Heartbeat(leader, term.getAsLong() - 1, 0, 0));
			  RaftMessages_ReceivedInstantClusterIdAwareMessage<object> heartbeat = RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), _clusterId, new RaftMessages_Heartbeat(_leader, _term.AsLong - 1, 0, 0) );

			  _handler.start( _clusterId );

			  // when
			  _handler.handle( heartbeat );

			  // then
			  verify( _leaderAvailabilityTimers, Mockito.never() ).renewElection();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRenewElectionTimeoutsForAppendEntriesRequestsFromEarlierTerms() throws Throwable
		 public void shouldNotRenewElectionTimeoutsForAppendEntriesRequestsFromEarlierTerms() throws Exception
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: RaftMessages_ReceivedInstantClusterIdAwareMessage<?> appendEntries = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new RaftMessages_AppendEntries_Request(leader, term.getAsLong() - 1, 0, 0, Neo4Net.causalclustering.core.consensus.log.RaftLogEntry.empty, 0)
			  RaftMessages_ReceivedInstantClusterIdAwareMessage<object> appendEntries = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(Instant.now(), _clusterId, new RaftMessages_AppendEntries_Request(_leader, _term.AsLong - 1, 0, 0, RaftLogEntry.empty, 0)
			 );

			  _handler.start( _clusterId );

			  // when
			  _handler.handle( appendEntries );

			  // then
			  verify( _leaderAvailabilityTimers, Mockito.never() ).renewElection();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStart() throws Throwable
		 public void shouldDelegateStart() throws Exception
		 {
			  // when
			  _handler.start( _clusterId );

			  // then
			  verify( @delegate ).start( _clusterId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStop() throws Throwable
		 public void shouldDelegateStop() throws Exception
		 {
			  // when
			  _handler.stop();

			  // then
			  verify( @delegate ).stop();
		 }
	}

}