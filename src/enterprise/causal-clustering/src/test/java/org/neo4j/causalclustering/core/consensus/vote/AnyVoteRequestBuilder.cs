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
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public abstract class AnyVoteRequestBuilder<T> where T : Neo4Net.causalclustering.core.consensus.RaftMessages_AnyVote_Request
	{
		 protected internal AnyVoteRequestBuilder( Constructor<T> constructor )
		 {
			  this._constructor = constructor;
		 }

		 internal interface Constructor<T> where T : Neo4Net.causalclustering.core.consensus.RaftMessages_AnyVote_Request
		 {
			  T Construct( MemberId from, long term, MemberId candidate, long lastLogIndex, long lastLogTerm );
		 }

		 private long _term = -1;
		 private MemberId _from;
		 private MemberId _candidate;
		 private long _lastLogIndex;
		 private long _lastLogTerm;

		 private readonly Constructor<T> _constructor;

		 public virtual T Build()
		 {
			  return _constructor.construct( _from, _term, _candidate, _lastLogIndex, _lastLogTerm );
		 }

		 public virtual AnyVoteRequestBuilder<T> From( MemberId from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual AnyVoteRequestBuilder<T> Term( long term )
		 {
			  this._term = term;
			  return this;
		 }

		 public virtual AnyVoteRequestBuilder<T> Candidate( MemberId candidate )
		 {
			  this._candidate = candidate;
			  return this;
		 }

		 public virtual AnyVoteRequestBuilder<T> LastLogIndex( long lastLogIndex )
		 {
			  this._lastLogIndex = lastLogIndex;
			  return this;
		 }

		 public virtual AnyVoteRequestBuilder<T> LastLogTerm( long lastLogTerm )
		 {
			  this._lastLogTerm = lastLogTerm;
			  return this;
		 }
	}

}