﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus.election
{

	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using MemberIdSet = Org.Neo4j.causalclustering.core.consensus.membership.MemberIdSet;
	using MembershipEntry = Org.Neo4j.causalclustering.core.consensus.membership.MembershipEntry;
	using TimerService = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService;
	using RaftCoreState = Org.Neo4j.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Org.Neo4j.causalclustering.identity.RaftTestMemberSetBuilder;
	using Org.Neo4j.causalclustering.messaging;
	using Predicates = Org.Neo4j.Function.Predicates;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class Fixture
	{
		 private readonly ISet<MemberId> _members = new HashSet<MemberId>();
		 private readonly ISet<BootstrapWaiter> _bootstrapWaiters = new HashSet<BootstrapWaiter>();
		 private readonly IList<TimerService> _timerServices = new List<TimerService>();
		 private readonly JobScheduler _scheduler = createInitialisedScheduler();
		 internal readonly ISet<RaftFixture> Rafts = new HashSet<RaftFixture>();
		 internal readonly TestNetwork Net;

		 internal Fixture( ISet<MemberId> memberIds, TestNetwork net, long electionTimeout, long heartbeatInterval )
		 {
			  this.Net = net;

			  foreach ( MemberId member in memberIds )
			  {
					TestNetwork.Inbound inbound = new TestNetwork.Inbound( net, member );
					TestNetwork.Outbound outbound = new TestNetwork.Outbound( net, member );

					_members.Add( member );

					TimerService timerService = CreateTimerService();

					BootstrapWaiter waiter = new BootstrapWaiter();
					_bootstrapWaiters.Add( waiter );

					InMemoryRaftLog raftLog = new InMemoryRaftLog();
					RaftMachine raftMachine = ( new RaftMachineBuilder( member, memberIds.Count, RaftTestMemberSetBuilder.INSTANCE ) ).electionTimeout( electionTimeout ).heartbeatInterval( heartbeatInterval ).inbound( inbound ).outbound( outbound ).timerService( timerService ).raftLog( raftLog ).commitListener( waiter ).build();

					Rafts.Add( new RaftFixture( this, raftMachine, raftLog ) );
			  }
		 }

		 private TimerService CreateTimerService()
		 {
			  TimerService timerService = new TimerService( _scheduler, NullLogProvider.Instance );
			  _timerServices.Add( timerService );
			  return timerService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void boot() throws Throwable
		 internal virtual void Boot()
		 {
			  _scheduler.start();
			  foreach ( RaftFixture raft in Rafts )
			  {
					raft.RaftLog().append(new RaftLogEntry(0, new MemberIdSet(asSet(_members))));
					raft.RaftMachine().installCoreState(new RaftCoreState(new MembershipEntry(0, _members)));
					raft.RaftMachineConflict.postRecoveryActions();
			  }
			  Net.start();
			  AwaitBootstrapped();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void tearDown() throws Throwable
		 public virtual void TearDown()
		 {
			  Net.stop();
			  foreach ( RaftFixture raft in Rafts )
			  {
					raft.RaftMachine().logShippingManager().stop();
			  }
			  _scheduler.stop();
			  _scheduler.shutdown();
		 }

		 /// <summary>
		 /// This class simply waits for a single entry to have been committed,
		 /// which should be the initial member set entry.
		 /// 
		 /// If all members of the cluster have committed such an entry, it's possible for any member
		 /// to perform elections. We need to meet this condition before we start disconnecting members.
		 /// </summary>
		 private class BootstrapWaiter : RaftMachineBuilder.CommitListener
		 {
			  internal AtomicBoolean Bootstrapped = new AtomicBoolean( false );

			  public override void NotifyCommitted( long commitIndex )
			  {
					if ( commitIndex >= 0 )
					{
						 Bootstrapped.set( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitBootstrapped() throws java.util.concurrent.TimeoutException
		 private void AwaitBootstrapped()
		 {
			  Predicates.await(() =>
			  {
				foreach ( BootstrapWaiter bootstrapWaiter in _bootstrapWaiters )
				{
					 if ( !bootstrapWaiter.Bootstrapped.get() )
					 {
						  return false;
					 }
				}
				return true;
			  }, 30, SECONDS, 100, MILLISECONDS);
		 }

		 internal class RaftFixture
		 {
			 private readonly Fixture _outerInstance;


//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RaftMachine RaftMachineConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly InMemoryRaftLog RaftLogConflict;

			  internal RaftFixture( Fixture outerInstance, RaftMachine raftMachine, InMemoryRaftLog raftLog )
			  {
				  this._outerInstance = outerInstance;
					this.RaftMachineConflict = raftMachine;
					this.RaftLogConflict = raftLog;
			  }

			  public virtual RaftMachine RaftMachine()
			  {
					return RaftMachineConflict;
			  }

			  public virtual InMemoryRaftLog RaftLog()
			  {
					return RaftLogConflict;
			  }
		 }
	}

}