using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus
{

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using OnDemandTimerService = Neo4Net.causalclustering.core.consensus.schedule.OnDemandTimerService;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Neo4Net.causalclustering.identity.RaftTestMemberSetBuilder;
	using Neo4Net.causalclustering.logging;
	using Neo4Net.causalclustering.logging;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class RaftTestFixture
	{
		 private Members _members = new Members();
		 // Does not need to be closed
		 private StringWriter _writer = new StringWriter();

		 public RaftTestFixture( DirectNetworking net, int expectedClusterSize, params MemberId[] ids )
		 {
			  foreach ( MemberId id in ids )
			  {
					MemberFixture fixtureMember = new MemberFixture( this );

					FakeClock clock = Clocks.fakeClock();
					fixtureMember.TimerServiceConflict = new OnDemandTimerService( clock );

					fixtureMember.RaftLogConflict = new InMemoryRaftLog();
					fixtureMember.MemberConflict = id;

					MessageLogger<MemberId> messageLogger = new BetterMessageLogger<MemberId>( id, new PrintWriter( _writer ), Clocks.systemClock() );
					Inbound<RaftMessages_RaftMessage> inbound = new LoggingInbound<RaftMessages_RaftMessage>( new Neo4Net.causalclustering.core.consensus.DirectNetworking.Inbound<>( net, fixtureMember.MemberConflict ), messageLogger, fixtureMember.MemberConflict );
					Outbound<MemberId, RaftMessages_RaftMessage> outbound = new LoggingOutbound<MemberId, RaftMessages_RaftMessage>( new Neo4Net.causalclustering.core.consensus.DirectNetworking.Outbound( net, id ), fixtureMember.MemberConflict, messageLogger );

					fixtureMember.RaftMachine = ( new RaftMachineBuilder( fixtureMember.MemberConflict, expectedClusterSize, RaftTestMemberSetBuilder.INSTANCE ) ).inbound( inbound ).outbound( outbound ).raftLog( fixtureMember.RaftLogConflict ).clock( clock ).timerService( fixtureMember.TimerServiceConflict ).build();

					_members.put( fixtureMember );
			  }
		 }

		 public virtual Members Members()
		 {
			  return _members;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void bootstrap(org.neo4j.causalclustering.identity.MemberId[] members) throws java.io.IOException
		 public virtual void Bootstrap( MemberId[] members )
		 {
			  foreach ( MemberFixture member in members() )
			  {
					member.RaftLog().append(new RaftLogEntry(0, new MemberIdSet(asSet(members))));
					member.RaftInstance().installCoreState(new RaftCoreState(new MembershipEntry(0, asSet(members))));
					member.RaftInstance().postRecoveryActions();
			  }
		 }

		 public virtual string MessageLog()
		 {
			  return _writer.ToString();
		 }

		 public class Members : IEnumerable<MemberFixture>
		 {
			  internal IDictionary<MemberId, MemberFixture> MemberMap = new Dictionary<MemberId, MemberFixture>();

			  internal virtual MemberFixture Put( MemberFixture value )
			  {
					return MemberMap[value.MemberConflict] = value;
			  }

			  public virtual MemberFixture WithId( MemberId id )
			  {
					return MemberMap[id];
			  }

			  public virtual Members WithIds( params MemberId[] ids )
			  {
					Members filteredMembers = new Members();
					foreach ( MemberId id in ids )
					{
						 if ( MemberMap.ContainsKey( id ) )
						 {
							  filteredMembers.Put( MemberMap[id] );
						 }
					}
					return filteredMembers;
			  }

			  public virtual Members WithRole( Role role )
			  {
					Members filteredMembers = new Members();

					foreach ( KeyValuePair<MemberId, MemberFixture> entry in MemberMap.SetOfKeyValuePairs() )
					{
						 if ( entry.Value.raftInstance().currentRole() == role )
						 {
							  filteredMembers.Put( entry.Value );
						 }
					}
					return filteredMembers;
			  }

			  public virtual ISet<MemberId> TargetMembershipSet
			  {
				  set
				  {
						foreach ( MemberFixture memberFixture in MemberMap.Values )
						{
							 memberFixture.RaftMachine.TargetMembershipSet = value;
						}
				  }
			  }

			  public virtual void InvokeTimeout( TimerService.TimerName name )
			  {
					foreach ( MemberFixture memberFixture in MemberMap.Values )
					{
						 memberFixture.TimerServiceConflict.invoke( name );
					}
			  }

			  public override IEnumerator<MemberFixture> Iterator()
			  {
					return MemberMap.Values.GetEnumerator();
			  }

			  public virtual int Size()
			  {
					return MemberMap.Count;
			  }

			  public override string ToString()
			  {
					return format( "Members%s", MemberMap );
			  }
		 }

		 public class MemberFixture
		 {
			 private readonly RaftTestFixture _outerInstance;

			 public MemberFixture( RaftTestFixture outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal MemberId MemberConflict;
			  internal RaftMachine RaftMachine;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal OnDemandTimerService TimerServiceConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal RaftLog RaftLogConflict;

			  public virtual MemberId Member()
			  {
					return MemberConflict;
			  }

			  public virtual RaftMachine RaftInstance()
			  {
					return RaftMachine;
			  }

			  public virtual OnDemandTimerService TimerService()
			  {
					return TimerServiceConflict;
			  }

			  public virtual RaftLog RaftLog()
			  {
					return RaftLogConflict;
			  }

			  public override string ToString()
			  {
					return "FixtureMember{" +
							  "raftInstance=" + RaftMachine +
							  ", timeoutService=" + TimerServiceConflict +
							  ", raftLog=" + RaftLogConflict +
							  '}';
			  }
		 }
	}

}