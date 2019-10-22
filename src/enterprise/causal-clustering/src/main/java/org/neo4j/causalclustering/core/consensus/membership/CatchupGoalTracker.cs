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

	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using FollowerState = Neo4Net.causalclustering.core.consensus.roles.follower.FollowerState;

	internal class CatchupGoalTracker
	{
		 internal const long MAX_ROUNDS = 10;

		 private readonly ReadableRaftLog _raftLog;
		 private readonly Clock _clock;

		 private long _startTime;
		 private long _roundStartTime;
		 private readonly long _roundTimeout;
		 private long _roundCount;
		 private long _catchupTimeout;

		 private long _targetIndex;
		 private bool _finished;
		 private bool _goalAchieved;

		 internal CatchupGoalTracker( ReadableRaftLog raftLog, Clock clock, long roundTimeout, long catchupTimeout )
		 {
			  this._raftLog = raftLog;
			  this._clock = clock;
			  this._roundTimeout = roundTimeout;
			  this._catchupTimeout = catchupTimeout;
			  this._targetIndex = raftLog.AppendIndex();
			  this._startTime = clock.millis();
			  this._roundStartTime = clock.millis();

			  this._roundCount = 1;
		 }

		 internal virtual void UpdateProgress( FollowerState followerState )
		 {
			  if ( _finished )
			  {
					return;
			  }

			  bool achievedTarget = followerState.MatchIndex >= _targetIndex;
			  if ( achievedTarget && ( _clock.millis() - _roundStartTime ) <= _roundTimeout )
			  {
					_goalAchieved = true;
					_finished = true;
			  }
			  else if ( _clock.millis() > (_startTime + _catchupTimeout) )
			  {
					_finished = true;
			  }
			  else if ( achievedTarget )
			  {
					if ( _roundCount < MAX_ROUNDS )
					{
						 _roundCount++;
						 _roundStartTime = _clock.millis();
						 _targetIndex = _raftLog.appendIndex();
					}
					else
					{
						 _finished = true;
					}
			  }
		 }

		 internal virtual bool Finished
		 {
			 get
			 {
				  return _finished;
			 }
		 }

		 internal virtual bool GoalAchieved
		 {
			 get
			 {
				  return _goalAchieved;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "CatchupGoalTracker{{startTime={0:D}, roundStartTime={1:D}, roundTimeout={2:D}, roundCount={3:D}, " + "catchupTimeout={4:D}, targetIndex={5:D}, finished={6}, goalAchieved={7}}}", _startTime, _roundStartTime, _roundTimeout, _roundCount, _catchupTimeout, _targetIndex, _finished, _goalAchieved );
		 }
	}

}