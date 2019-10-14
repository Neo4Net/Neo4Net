/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.consensus.vote
{
	using Test = org.junit.Test;

	using Voting = Neo4Net.causalclustering.core.consensus.roles.Voting;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class VotingTest
	{
		 internal MemberId Candidate = new MemberId( System.Guid.randomUUID() );

		 internal long LogTerm = 10;
		 internal long CurrentTerm = 20;
		 internal long AppendIndex = 1000;

		 internal Log Log = NullLog.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestWithIdenticalLog()
		 public virtual void ShouldAcceptRequestWithIdenticalLog()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm, AppendIndex, AppendIndex, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectRequestFromOldTerm()
		 public virtual void ShouldRejectRequestFromOldTerm()
		 {
			  assertFalse( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm - 1, LogTerm, LogTerm, AppendIndex, AppendIndex, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectRequestIfCandidateLogEndsAtLowerTerm()
		 public virtual void ShouldRejectRequestIfCandidateLogEndsAtLowerTerm()
		 {
			  assertFalse( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm - 1, AppendIndex, AppendIndex, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectRequestIfLogsEndInSameTermButCandidateLogIsShorter()
		 public virtual void ShouldRejectRequestIfLogsEndInSameTermButCandidateLogIsShorter()
		 {
			  assertFalse( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm, AppendIndex, AppendIndex - 1, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestIfLogsEndInSameTermAndCandidateLogIsSameLength()
		 public virtual void ShouldAcceptRequestIfLogsEndInSameTermAndCandidateLogIsSameLength()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm, AppendIndex, AppendIndex, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestIfLogsEndInSameTermAndCandidateLogIsLonger()
		 public virtual void ShouldAcceptRequestIfLogsEndInSameTermAndCandidateLogIsLonger()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm, AppendIndex, AppendIndex + 1, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestIfLogsEndInHigherTermAndCandidateLogIsShorter()
		 public virtual void ShouldAcceptRequestIfLogsEndInHigherTermAndCandidateLogIsShorter()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm + 1, AppendIndex, AppendIndex - 1, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestIfLogEndsAtHigherTermAndCandidateLogIsSameLength()
		 public virtual void ShouldAcceptRequestIfLogEndsAtHigherTermAndCandidateLogIsSameLength()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm + 1, AppendIndex, AppendIndex, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRequestIfLogEndsAtHigherTermAndCandidateLogIsLonger()
		 public virtual void ShouldAcceptRequestIfLogEndsAtHigherTermAndCandidateLogIsLonger()
		 {
			  assertTrue( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm + 1, AppendIndex, AppendIndex + 1, false, Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectRequestIfAlreadyVotedForOtherCandidate()
		 public virtual void ShouldRejectRequestIfAlreadyVotedForOtherCandidate()
		 {
			  assertFalse( Voting.shouldVoteFor( Candidate, CurrentTerm, CurrentTerm, LogTerm, LogTerm, AppendIndex, AppendIndex, true, Log ) );
		 }
	}

}