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

	public class CatchupGoalTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void goalAchievedWhenCatchupRoundDurationLessThanTarget()
		 public virtual void GoalAchievedWhenCatchupRoundDurationLessThanTarget()
		 {
			  FakeClock clock = Clocks.fakeClock();
			  long electionTimeout = 15;
			  StubLog log = new StubLog( this );

			  log.AppendIndex = 10;
			  CatchupGoal goal = new CatchupGoal( log, clock, electionTimeout );

			  log.AppendIndex = 20;
			  clock.Forward( 10, MILLISECONDS );
			  assertFalse( goal.Achieved( new FollowerState() ) );

			  log.AppendIndex = 30;
			  clock.Forward( 10, MILLISECONDS );
			  assertFalse( goal.Achieved( ( new FollowerState() ).onSuccessResponse(10) ) );

			  log.AppendIndex = 40;
			  clock.Forward( 10, MILLISECONDS );
			  assertTrue( goal.Achieved( ( new FollowerState() ).onSuccessResponse(30) ) );
		 }

		 private class StubLog : ReadableRaftLog
		 {
			 private readonly CatchupGoalTest _outerInstance;

			 public StubLog( CatchupGoalTest outerInstance )
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