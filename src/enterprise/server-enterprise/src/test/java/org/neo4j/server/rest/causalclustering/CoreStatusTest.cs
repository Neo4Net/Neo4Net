using System;
using System.Collections.Generic;
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
namespace Neo4Net.Server.rest.causalclustering
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using DurationSinceLastMessageMonitor = Neo4Net.causalclustering.core.consensus.DurationSinceLastMessageMonitor;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using RaftMembershipManager = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipManager;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using RoleInfo = Neo4Net.causalclustering.discovery.RoleInfo;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.causalclustering.ReadReplicaStatusTest.responseAsMap;

	public class CoreStatusTest
	{
		 private CausalClusteringStatus _status;

		 private CoreGraphDatabase _db;
		 private Dependencies _dependencyResolver = new Dependencies();
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;

		 // Dependency resolved
		 private RaftMembershipManager _raftMembershipManager;
		 private DatabaseHealth _databaseHealth;
		 private FakeTopologyService _topologyService;
		 private DurationSinceLastMessageMonitor _raftMessageTimerResetMonitor;
		 private RaftMachine _raftMachine;
		 private CommandIndexTracker _commandIndexTracker;

		 private readonly MemberId _myself = new MemberId( new System.Guid( 0x1234, 0x5678 ) );
		 private readonly MemberId _core2 = new MemberId( System.Guid.randomUUID() );
		 private readonly MemberId _core3 = new MemberId( System.Guid.randomUUID() );
		 private readonly MemberId _replica = new MemberId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  OutputFormat output = new OutputFormat( new JsonFormat(), new URI("http://base.local:1234/"), null );
			  _db = mock( typeof( CoreGraphDatabase ) );
			  when( _db.DependencyResolver ).thenReturn( _dependencyResolver );

			  _raftMembershipManager = _dependencyResolver.satisfyDependency( FakeRaftMembershipManager( new HashSet<MemberId>( Arrays.asList( _myself, _core2, _core3 ) ) ) );

			  _databaseHealth = _dependencyResolver.satisfyDependency( new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), _logProvider.getLog( typeof( DatabaseHealth ) ) ) );

			  _topologyService = _dependencyResolver.satisfyDependency( new FakeTopologyService( Arrays.asList( _core2, _core3 ), Collections.singleton( _replica ), _myself, RoleInfo.FOLLOWER ) );

			  _raftMessageTimerResetMonitor = _dependencyResolver.satisfyDependency( new DurationSinceLastMessageMonitor() );
			  _raftMachine = _dependencyResolver.satisfyDependency( mock( typeof( RaftMachine ) ) );
			  _commandIndexTracker = _dependencyResolver.satisfyDependency( new CommandIndexTracker() );

			  _status = CausalClusteringStatusFactory.Build( output, _db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnswersWhenLeader()
		 public virtual void TestAnswersWhenLeader()
		 {
			  // given
			  when( _db.Role ).thenReturn( Role.LEADER );

			  // when
			  Response available = _status.available();
			  Response @readonly = _status.@readonly();
			  Response writable = _status.writable();

			  // then
			  assertEquals( OK.StatusCode, available.Status );
			  assertEquals( "true", available.Entity );

			  assertEquals( NOT_FOUND.StatusCode, @readonly.Status );
			  assertEquals( "false", @readonly.Entity );

			  assertEquals( OK.StatusCode, writable.Status );
			  assertEquals( "true", writable.Entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnswersWhenCandidate()
		 public virtual void TestAnswersWhenCandidate()
		 {
			  // given
			  when( _db.Role ).thenReturn( Role.CANDIDATE );

			  // when
			  Response available = _status.available();
			  Response @readonly = _status.@readonly();
			  Response writable = _status.writable();

			  // then
			  assertEquals( OK.StatusCode, available.Status );
			  assertEquals( "true", available.Entity );

			  assertEquals( OK.StatusCode, @readonly.Status );
			  assertEquals( "true", @readonly.Entity );

			  assertEquals( NOT_FOUND.StatusCode, writable.Status );
			  assertEquals( "false", writable.Entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnswersWhenFollower()
		 public virtual void TestAnswersWhenFollower()
		 {
			  // given
			  when( _db.Role ).thenReturn( Role.FOLLOWER );

			  // when
			  Response available = _status.available();
			  Response @readonly = _status.@readonly();
			  Response writable = _status.writable();

			  // then
			  assertEquals( OK.StatusCode, available.Status );
			  assertEquals( "true", available.Entity );

			  assertEquals( OK.StatusCode, @readonly.Status );
			  assertEquals( "true", @readonly.Entity );

			  assertEquals( NOT_FOUND.StatusCode, writable.Status );
			  assertEquals( "false", writable.Entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void expectedStatusFieldsAreIncluded() throws java.io.IOException, Neo4Net.causalclustering.core.consensus.NoLeaderFoundException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExpectedStatusFieldsAreIncluded()
		 {
			  // given ideal normal conditions
			  _commandIndexTracker.AppliedCommandIndex = 123;
			  when( _raftMachine.Leader ).thenReturn( _core2 );
			  _raftMessageTimerResetMonitor.timerReset();
			  Thread.Sleep( 1 ); // Sometimes the test can be fast. This guarantees at least 1 ms since message received

			  // and helpers
			  IList<string> votingMembers = _raftMembershipManager.votingMembers().Select(memberId => memberId.Uuid.ToString()).OrderBy(c => c).ToList();

			  // when
			  Response description = _status.description();
			  IDictionary<string, object> response = responseAsMap( description );

			  // then
			  assertThat( response, ContainsAndEquals( "core", true ) );
			  assertThat( response, ContainsAndEquals( "lastAppliedRaftIndex", 123 ) );
			  assertThat( response, ContainsAndEquals( "participatingInRaftGroup", true ) );
			  assertThat( response, ContainsAndEquals( "votingMembers", votingMembers ) );
			  assertThat( response, ContainsAndEquals( "healthy", true ) );
			  assertThat( response, ContainsAndEquals( "memberId", _myself.Uuid.ToString() ) );
			  assertThat( response, ContainsAndEquals( "leader", _core2.Uuid.ToString() ) );
			  assertThat( response.ToString(), long.Parse(response["millisSinceLastLeaderMessage"].ToString()), greaterThan(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notParticipatingInRaftGroupWhenNotInVoterSet() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotParticipatingInRaftGroupWhenNotInVoterSet()
		 {
			  // given not in voting set
			  _topologyService.replaceWithRole( _core2, RoleInfo.LEADER );
			  when( _raftMembershipManager.votingMembers() ).thenReturn(new HashSet<>(Arrays.asList(_core2, _core3)));

			  // when
			  Response description = _status.description();

			  // then
			  IDictionary<string, object> response = responseAsMap( description );
			  assertThat( response, ContainsAndEquals( "participatingInRaftGroup", false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notParticipatingInRaftGroupWhenLeaderUnknown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotParticipatingInRaftGroupWhenLeaderUnknown()
		 {
			  // given leader is unknown
			  _topologyService.replaceWithRole( null, RoleInfo.LEADER );

			  // when
			  Response description = _status.description();

			  // then
			  IDictionary<string, object> response = responseAsMap( description );
			  assertThat( response, ContainsAndEquals( "participatingInRaftGroup", false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseHealthIsReflected() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseHealthIsReflected()
		 {
			  // given database is not healthy
			  _databaseHealth.panic( new Exception() );

			  // when
			  Response description = _status.description();
			  IDictionary<string, object> response = responseAsMap( description );

			  // then
			  assertThat( response, ContainsAndEquals( "healthy", false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderNotIncludedIfUnknown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderNotIncludedIfUnknown()
		 {
			  // given no leader
			  _topologyService.replaceWithRole( null, RoleInfo.LEADER );

			  // when
			  Response description = _status.description();

			  // then
			  IDictionary<string, object> response = responseAsMap( description );
			  assertFalse( description.Entity.ToString(), response.ContainsKey("leader") );
		 }

		 internal static RaftMembershipManager FakeRaftMembershipManager( ISet<MemberId> votingMembers )
		 {
			  RaftMembershipManager raftMembershipManager = mock( typeof( RaftMembershipManager ) );
			  when( raftMembershipManager.VotingMembers() ).thenReturn(votingMembers);
			  return raftMembershipManager;
		 }

		 private static Matcher<IDictionary<string, object>> ContainsAndEquals( string key, object target )
		 {
			  return new BaseMatcherAnonymousInnerClass( key, target );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<IDictionary<string, object>>
		 {
			 private string _key;
			 private object _target;

			 public BaseMatcherAnonymousInnerClass( string key, object target )
			 {
				 this._key = key;
				 this._target = target;
			 }

			 private bool containsKey;
			 private bool areEqual;

			 public override bool matches( object item )
			 {
				  IDictionary<string, object> map = ( IDictionary<string, object> ) item;
				  if ( !map.ContainsKey( _key ) )
				  {
						return false;
				  }
				  containsKey = true;
				  if ( !map[_key].Equals( _target ) )
				  {
						return false;
				  }
				  areEqual = true;
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  if ( !containsKey )
				  {
						description.appendText( "did not include key " ).appendValue( _key );
				  }
				  else if ( !areEqual )
				  {
						description.appendText( "key " ).appendValue( _key ).appendText( " did not match value" ).appendValue( _target );
				  }
				  else
				  {
						throw new System.InvalidOperationException( "Matcher failed, conditions should have passed" );
				  }
			 }
		 }
	}

}