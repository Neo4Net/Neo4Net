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
namespace Neo4Net.causalclustering.core.consensus.vote
{
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class VoteStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreVote()
		 public virtual void ShouldStoreVote()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member = new MemberId( System.Guid.randomUUID() );

			  // when
			  voteState.Update( member, 0 );

			  // then
			  assertEquals( member, voteState.VotedFor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithNoVote()
		 public virtual void ShouldStartWithNoVote()
		 {
			  // given
			  VoteState voteState = new VoteState();

			  // then
			  assertNull( voteState.VotedFor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateVote()
		 public virtual void ShouldUpdateVote()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member1 = new MemberId( System.Guid.randomUUID() );
			  MemberId member2 = new MemberId( System.Guid.randomUUID() );

			  // when
			  voteState.Update( member1, 0 );
			  voteState.Update( member2, 1 );

			  // then
			  assertEquals( member2, voteState.VotedFor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearVote()
		 public virtual void ShouldClearVote()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member = new MemberId( System.Guid.randomUUID() );

			  voteState.Update( member, 0 );

			  // when
			  voteState.Update( null, 1 );

			  // then
			  assertNull( voteState.VotedFor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateVoteForSameTerm()
		 public virtual void ShouldNotUpdateVoteForSameTerm()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member1 = new MemberId( System.Guid.randomUUID() );
			  MemberId member2 = new MemberId( System.Guid.randomUUID() );

			  voteState.Update( member1, 0 );

			  try
			  {
					// when
					voteState.Update( member2, 0 );
					Assert.fail( "Should have thrown IllegalArgumentException" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotClearVoteForSameTerm()
		 public virtual void ShouldNotClearVoteForSameTerm()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member = new MemberId( System.Guid.randomUUID() );

			  voteState.Update( member, 0 );

			  try
			  {
					// when
					voteState.Update( null, 0 );
					Assert.fail( "Should have thrown IllegalArgumentException" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNoUpdateWhenVoteStateUnchanged()
		 public virtual void ShouldReportNoUpdateWhenVoteStateUnchanged()
		 {
			  // given
			  VoteState voteState = new VoteState();
			  MemberId member1 = new MemberId( System.Guid.randomUUID() );
			  MemberId member2 = new MemberId( System.Guid.randomUUID() );

			  // when
			  assertTrue( voteState.Update( null, 0 ) );
			  assertFalse( voteState.Update( null, 0 ) );
			  assertTrue( voteState.Update( member1, 0 ) );
			  assertFalse( voteState.Update( member1, 0 ) );
			  assertTrue( voteState.Update( member2, 1 ) );
			  assertFalse( voteState.Update( member2, 1 ) );
		 }
	}

}