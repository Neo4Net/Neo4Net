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
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class HeartbeatBuilder
	{
		 private long _commitIndex = -1;
		 private long _leaderTerm = -1;
		 private long _commitIndexTerm = -1;
		 private MemberId _from;

		 public virtual RaftMessages_Heartbeat Build()
		 {
			  return new RaftMessages_Heartbeat( _from, _leaderTerm, _commitIndex, _commitIndexTerm );
		 }

		 public virtual HeartbeatBuilder From( MemberId from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual HeartbeatBuilder LeaderTerm( long leaderTerm )
		 {
			  this._leaderTerm = leaderTerm;
			  return this;
		 }

		 public virtual HeartbeatBuilder CommitIndex( long commitIndex )
		 {
			  this._commitIndex = commitIndex;
			  return this;
		 }

		 public virtual HeartbeatBuilder CommitIndexTerm( long commitIndexTerm )
		 {
			  this._commitIndexTerm = commitIndexTerm;
			  return this;
		 }
	}

}