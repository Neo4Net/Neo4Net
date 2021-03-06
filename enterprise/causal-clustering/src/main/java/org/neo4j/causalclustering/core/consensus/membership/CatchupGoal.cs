﻿/*
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
namespace Org.Neo4j.causalclustering.core.consensus.membership
{

	using ReadableRaftLog = Org.Neo4j.causalclustering.core.consensus.log.ReadableRaftLog;
	using FollowerState = Org.Neo4j.causalclustering.core.consensus.roles.follower.FollowerState;

	internal class CatchupGoal
	{
		 private const long MAX_ROUNDS = 10;

		 private readonly ReadableRaftLog _raftLog;
		 private readonly Clock _clock;
		 private readonly long _electionTimeout;

		 private long _targetIndex;
		 private long _roundCount;
		 private long _startTime;

		 internal CatchupGoal( ReadableRaftLog raftLog, Clock clock, long electionTimeout )
		 {
			  this._raftLog = raftLog;
			  this._clock = clock;
			  this._electionTimeout = electionTimeout;
			  this._targetIndex = raftLog.AppendIndex();
			  this._startTime = clock.millis();

			  this._roundCount = 1;
		 }

		 internal virtual bool Achieved( FollowerState followerState )
		 {
			  if ( followerState.MatchIndex >= _targetIndex )
			  {
					if ( ( _clock.millis() - _startTime ) <= _electionTimeout )
					{
						 return true;
					}
					else if ( _roundCount < MAX_ROUNDS )
					{
						 _roundCount++;
						 _startTime = _clock.millis();
						 _targetIndex = _raftLog.appendIndex();
					}
			  }
			  return false;
		 }
	}

}