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
namespace Neo4Net.causalclustering.core.consensus.membership
{
	using Test = org.junit.Test;


	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using FollowerState = Neo4Net.causalclustering.core.consensus.roles.follower.FollowerState;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CatchupGoalTrackerTest
	{

		 private const long ROUND_TIMEOUT = 15;
		 private const long CATCHUP_TIMEOUT = 1_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAchieveGoalIfWithinRoundTimeout()
		 public virtual void ShouldAchieveGoalIfWithinRoundTimeout()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  log.AppendIndex = 10;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  clock.Forward( ROUND_TIMEOUT - 5, TimeUnit.MILLISECONDS );
			  catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(10) );

			  assertTrue( catchupGoalTracker.GoalAchieved );
			  assertTrue( catchupGoalTracker.Finished );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAchieveGoalIfBeyondRoundTimeout()
		 public virtual void ShouldNotAchieveGoalIfBeyondRoundTimeout()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  log.AppendIndex = 10;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  clock.Forward( ROUND_TIMEOUT + 5, TimeUnit.MILLISECONDS );
			  catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(10) );

			  assertFalse( catchupGoalTracker.GoalAchieved );
			  assertFalse( catchupGoalTracker.Finished );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAchieveGoalDueToCatchupTimeoutExpiring()
		 public virtual void ShouldFailToAchieveGoalDueToCatchupTimeoutExpiring()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  log.AppendIndex = 10;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  // when
			  clock.Forward( CATCHUP_TIMEOUT + 10, TimeUnit.MILLISECONDS );
			  catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(4) );

			  // then
			  assertFalse( catchupGoalTracker.GoalAchieved );
			  assertTrue( catchupGoalTracker.Finished );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAchieveGoalDueToCatchupTimeoutExpiringEvenThoughWeDoEventuallyAchieveTarget()
		 public virtual void ShouldFailToAchieveGoalDueToCatchupTimeoutExpiringEvenThoughWeDoEventuallyAchieveTarget()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  log.AppendIndex = 10;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  // when
			  clock.Forward( CATCHUP_TIMEOUT + 10, TimeUnit.MILLISECONDS );
			  catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(10) );

			  // then
			  assertFalse( catchupGoalTracker.GoalAchieved );
			  assertTrue( catchupGoalTracker.Finished );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAchieveGoalDueToRoundExhaustion()
		 public virtual void ShouldFailToAchieveGoalDueToRoundExhaustion()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  long appendIndex = 10;
			  log.AppendIndex = appendIndex;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  for ( int i = 0; i < CatchupGoalTracker.MAX_ROUNDS; i++ )
			  {
					appendIndex += 10;
					log.AppendIndex = appendIndex;
					clock.Forward( ROUND_TIMEOUT + 1, TimeUnit.MILLISECONDS );
					catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(appendIndex) );
			  }

			  // then
			  assertFalse( catchupGoalTracker.GoalAchieved );
			  assertTrue( catchupGoalTracker.Finished );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFinishIfRoundsNotExhausted()
		 public virtual void ShouldNotFinishIfRoundsNotExhausted()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  StubLog log = new StubLog( this );

			  long appendIndex = 10;
			  log.AppendIndex = appendIndex;
			  CatchupGoalTracker catchupGoalTracker = new CatchupGoalTracker( log, clock, ROUND_TIMEOUT, CATCHUP_TIMEOUT );

			  for ( int i = 0; i < CatchupGoalTracker.MAX_ROUNDS - 5; i++ )
			  {
					appendIndex += 10;
					log.AppendIndex = appendIndex;
					clock.Forward( ROUND_TIMEOUT + 1, TimeUnit.MILLISECONDS );
					catchupGoalTracker.UpdateProgress( ( new FollowerState() ).onSuccessResponse(appendIndex) );
			  }

			  // then
			  assertFalse( catchupGoalTracker.GoalAchieved );
			  assertFalse( catchupGoalTracker.Finished );
		 }

		 private class StubLog : ReadableRaftLog
		 {
			 private readonly CatchupGoalTrackerTest _outerInstance;

			 public StubLog( CatchupGoalTrackerTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long AppendIndexConflict;

			  internal virtual long AppendIndex
			  {
				  set
				  {
						this.AppendIndexConflict = value;
				  }
			  }

			  public override long AppendIndex()
			  {
					return AppendIndexConflict;
			  }

			  public override long PrevIndex()
			  {
					return 0;
			  }

			  public override long ReadEntryTerm( long logIndex )
			  {
					return 0;
			  }

			  public override RaftLogCursor GetEntryCursor( long fromIndex )
			  {
					return RaftLogCursor.empty();
			  }
		 }
	}

}