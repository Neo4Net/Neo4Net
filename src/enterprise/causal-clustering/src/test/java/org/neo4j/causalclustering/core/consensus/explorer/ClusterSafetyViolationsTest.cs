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
namespace Neo4Net.causalclustering.core.consensus.explorer
{
	using Test = org.junit.Test;

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.explorer.ClusterSafetyViolations.inconsistentCommittedLogEntries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.explorer.ClusterSafetyViolations.multipleLeadersInSameTerm;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	public class ClusterSafetyViolationsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecogniseInconsistentCommittedContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecogniseInconsistentCommittedContent()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ), member( 1 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 2 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 3 ) ) );

			  Commit( clusterState, member( 0 ), 0 );
			  Commit( clusterState, member( 1 ), 0 );

			  // then
			  assertFalse( inconsistentCommittedLogEntries( clusterState ) );

			  // when
			  Commit( clusterState, member( 0 ), 1 );
			  Commit( clusterState, member( 1 ), 1 );

			  // then
			  assertTrue( inconsistentCommittedLogEntries( clusterState ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecogniseInconsistentTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecogniseInconsistentTerm()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ), member( 1 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 2 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 2, ValueOf( 2 ) ) );

			  Commit( clusterState, member( 0 ), 0 );
			  Commit( clusterState, member( 1 ), 0 );

			  // then
			  assertFalse( inconsistentCommittedLogEntries( clusterState ) );

			  // when
			  Commit( clusterState, member( 0 ), 1 );
			  Commit( clusterState, member( 1 ), 1 );

			  // then
			  assertTrue( inconsistentCommittedLogEntries( clusterState ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecogniseSomeMembersBeingInconsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecogniseSomeMembersBeingInconsistent()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ), member( 1 ), member( 2 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );
			  clusterState.States[member( 2 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 1 ) ) );

			  clusterState.States[member( 0 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 2 ) ) );
			  clusterState.States[member( 1 )].entryLog.append( new RaftLogEntry( 1, ValueOf( 2 ) ) );
			  clusterState.States[member( 2 )].entryLog.append( new RaftLogEntry( 2, ValueOf( 2 ) ) );

			  Commit( clusterState, member( 0 ), 0 );
			  Commit( clusterState, member( 1 ), 0 );
			  Commit( clusterState, member( 2 ), 0 );

			  // then
			  assertFalse( inconsistentCommittedLogEntries( clusterState ) );

			  // when
			  Commit( clusterState, member( 0 ), 1 );
			  Commit( clusterState, member( 1 ), 1 );

			  // then
			  assertFalse( inconsistentCommittedLogEntries( clusterState ) );

			  // when
			  Commit( clusterState, member( 2 ), 1 );

			  // then
			  assertTrue( inconsistentCommittedLogEntries( clusterState ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecogniseTwoLeadersInTheSameTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecogniseTwoLeadersInTheSameTerm()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ), member( 1 ), member( 2 ) ) );

			  // when
			  clusterState.States[member( 0 )].term = 21;
			  clusterState.States[member( 1 )].term = 21;
			  clusterState.States[member( 2 )].term = 21;

			  clusterState.Roles[member( 0 )] = Role.LEADER;
			  clusterState.Roles[member( 1 )] = Role.LEADER;
			  clusterState.Roles[member( 2 )] = Role.FOLLOWER;

			  // then
			  assertTrue( multipleLeadersInSameTerm( clusterState ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecogniseTwoLeadersInDifferentTerms() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecogniseTwoLeadersInDifferentTerms()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ), member( 1 ), member( 2 ) ) );

			  // when
			  clusterState.States[member( 0 )].term = 21;
			  clusterState.States[member( 1 )].term = 22;
			  clusterState.States[member( 2 )].term = 21;

			  clusterState.Roles[member( 0 )] = Role.LEADER;
			  clusterState.Roles[member( 1 )] = Role.LEADER;
			  clusterState.Roles[member( 2 )] = Role.FOLLOWER;

			  // then
			  assertFalse( multipleLeadersInSameTerm( clusterState ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void commit(ClusterState clusterState, org.Neo4Net.causalclustering.identity.MemberId member, long commitIndex) throws java.io.IOException
		 private void Commit( ClusterState clusterState, MemberId member, long commitIndex )
		 {
			  ComparableRaftState state = clusterState.States[member];
			  Outcome outcome = new Outcome( clusterState.Roles[member], state );
			  outcome.CommitIndex = commitIndex;
			  state.Update( outcome );
		 }
	}

}