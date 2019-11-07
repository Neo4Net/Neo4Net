using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.membership
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using AppendLogEntry = Neo4Net.causalclustering.core.consensus.outcome.AppendLogEntry;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using TruncateLogCommand = Neo4Net.causalclustering.core.consensus.outcome.TruncateLogCommand;
	using Neo4Net.causalclustering.core.state.storage;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
	using static Neo4Net.causalclustering.core.consensus.membership.RaftMembershipState.Marshal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.identity.RaftTestMemberSetBuilder.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.NullLogProvider.getInstance;

	public class RaftMembershipManagerTest
	{
		 private readonly Log _log = NullLog.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.kernel.lifecycle.LifeRule lifeRule = new Neo4Net.kernel.lifecycle.LifeRule(true);
		 public LifeRule LifeRule = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void membershipManagerShouldUseLatestAppendedMembershipSetEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MembershipManagerShouldUseLatestAppendedMembershipSetEntries()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog log = new Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog();
			  InMemoryRaftLog log = new InMemoryRaftLog();

			  RaftMembershipManager membershipManager = LifeRule.add( RaftMembershipManager( log ) );
			  // when
			  membershipManager.processLog(0, asList(new AppendLogEntry(0, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 4))), new AppendLogEntry(1, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 5)))
			 ));

			  // then
			  assertEquals( ( new RaftTestGroup( 1, 2, 3, 5 ) ).Members, membershipManager.VotingMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void membershipManagerShouldRevertToOldMembershipSetAfterTruncationCausesLossOfAllAppendedMembershipSets() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MembershipManagerShouldRevertToOldMembershipSetAfterTruncationCausesLossOfAllAppendedMembershipSets()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog raftLog = new Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog();
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();

			  RaftMembershipManager membershipManager = LifeRule.add( RaftMembershipManager( raftLog ) );

			  // when
			  IList<RaftLogCommand> logCommands = asList(new AppendLogEntry(0, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 4))), new AppendLogEntry(1, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 5))), new TruncateLogCommand(1)
			 );

			  foreach ( RaftLogCommand logCommand in logCommands )
			  {
					logCommand.ApplyTo( raftLog, _log );
			  }
			  membershipManager.ProcessLog( 0, logCommands );

			  // then
			  assertEquals( ( new RaftTestGroup( 1, 2, 3, 4 ) ).Members, membershipManager.VotingMembers() );
			  assertFalse( membershipManager.UncommittedMemberChangeInLog() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void membershipManagerShouldRevertToEarlierAppendedMembershipSetAfterTruncationCausesLossOfLastAppended() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MembershipManagerShouldRevertToEarlierAppendedMembershipSetAfterTruncationCausesLossOfLastAppended()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog raftLog = new Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog();
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();

			  RaftMembershipManager membershipManager = LifeRule.add( RaftMembershipManager( raftLog ) );

			  // when
			  IList<RaftLogCommand> logCommands = asList(new AppendLogEntry(0, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 4))), new AppendLogEntry(1, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 5))), new AppendLogEntry(2, new RaftLogEntry(0, new RaftTestGroup(1, 2, 3, 6))), new TruncateLogCommand(2)
			 );
			  foreach ( RaftLogCommand logCommand in logCommands )
			  {
					logCommand.ApplyTo( raftLog, _log );
			  }
			  membershipManager.ProcessLog( 0, logCommands );

			  // then
			  assertEquals( ( new RaftTestGroup( 1, 2, 3, 5 ) ).Members, membershipManager.VotingMembers() );
		 }

		 private RaftMembershipManager RaftMembershipManager( InMemoryRaftLog log )
		 {
			  RaftMembershipManager raftMembershipManager = new RaftMembershipManager( null, INSTANCE, log, Instance, 3, 1000, Clocks.fakeClock(), 1000, new InMemoryStateStorage<RaftMembershipState>((new Marshal()).startState()) );

			  raftMembershipManager.RecoverFromIndexSupplier = () => 0;
			  return raftMembershipManager;
		 }
	}

}