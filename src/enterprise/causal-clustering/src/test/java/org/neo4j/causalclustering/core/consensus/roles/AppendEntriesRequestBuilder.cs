using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.roles
{

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class AppendEntriesRequestBuilder
	{
		 private IList<RaftLogEntry> _logEntries = new LinkedList<RaftLogEntry>();
		 private long _leaderCommit = -1;
		 private long _prevLogTerm = -1;
		 private long _prevLogIndex = -1;
		 private long _leaderTerm = -1;
		 private MemberId _from;

		 public virtual Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request Build()
		 {
			  return new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _from, _leaderTerm, _prevLogIndex, _prevLogTerm, _logEntries.ToArray(), _leaderCommit );
		 }

		 public virtual AppendEntriesRequestBuilder From( MemberId from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual AppendEntriesRequestBuilder LeaderTerm( long leaderTerm )
		 {
			  this._leaderTerm = leaderTerm;
			  return this;
		 }

		 public virtual AppendEntriesRequestBuilder PrevLogIndex( long prevLogIndex )
		 {
			  this._prevLogIndex = prevLogIndex;
			  return this;
		 }

		 public virtual AppendEntriesRequestBuilder PrevLogTerm( long prevLogTerm )
		 {
			  this._prevLogTerm = prevLogTerm;
			  return this;
		 }

		 public virtual AppendEntriesRequestBuilder LogEntry( RaftLogEntry logEntry )
		 {
			  _logEntries.Add( logEntry );
			  return this;
		 }

		 public virtual AppendEntriesRequestBuilder LeaderCommit( long leaderCommit )
		 {
			  this._leaderCommit = leaderCommit;
			  return this;
		 }
	}

}