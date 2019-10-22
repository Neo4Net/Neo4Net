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
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public abstract class AnyVoteResponseBuilder<T> where T : Neo4Net.causalclustering.core.consensus.RaftMessages_AnyVote_Response
	{
		 protected internal AnyVoteResponseBuilder( Constructor<T> constructor )
		 {
			  this._constructor = constructor;
		 }

		 internal interface Constructor<T> where T : Neo4Net.causalclustering.core.consensus.RaftMessages_AnyVote_Response
		 {
			  T Construct( MemberId from, long term, bool voteGranted );
		 }

		 private bool _voteGranted;
		 private long _term = -1;
		 private MemberId _from;
		 private readonly Constructor<T> _constructor;

		 public virtual T Build()
		 {
			  return _constructor.construct( _from, _term, _voteGranted );
		 }

		 public virtual AnyVoteResponseBuilder<T> From( MemberId from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual AnyVoteResponseBuilder<T> Term( long term )
		 {
			  this._term = term;
			  return this;
		 }

		 public virtual AnyVoteResponseBuilder<T> Grant()
		 {
			  this._voteGranted = true;
			  return this;
		 }

		 public virtual AnyVoteResponseBuilder<T> Deny()
		 {
			  this._voteGranted = false;
			  return this;
		 }
	}

}