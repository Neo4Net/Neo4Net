using System.Diagnostics;

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
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class AppendEntriesResponseBuilder
	{
		 private bool _success;
		 private long _term = -1;
		 private MemberId _from;
		 private long _matchIndex = -1;
		 private long _appendIndex = -1;

		 public virtual Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response Build()
		 {
			  // a response of false should always have a match index of -1
			  Debug.Assert( _success || _matchIndex == -1 );
			  return new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( _from, _term, _success, _matchIndex, _appendIndex );
		 }

		 public virtual AppendEntriesResponseBuilder From( MemberId from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual AppendEntriesResponseBuilder Term( long term )
		 {
			  this._term = term;
			  return this;
		 }

		 public virtual AppendEntriesResponseBuilder MatchIndex( long matchIndex )
		 {
			  this._matchIndex = matchIndex;
			  return this;
		 }

		 public virtual AppendEntriesResponseBuilder AppendIndex( long appendIndex )
		 {
			  this._appendIndex = appendIndex;
			  return this;
		 }

		 public virtual AppendEntriesResponseBuilder Success()
		 {
			  this._success = true;
			  return this;
		 }

		 public virtual AppendEntriesResponseBuilder Failure()
		 {
			  this._success = false;
			  return this;
		 }
	}

}