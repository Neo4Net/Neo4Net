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
namespace Org.Neo4j.causalclustering.core.consensus.vote
{

	using Org.Neo4j.causalclustering.messaging.marshalling;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public class VoteState
	{
		 private MemberId _votedFor;
		 private long _term = -1;

		 public VoteState()
		 {
		 }

		 private VoteState( MemberId votedFor, long term )
		 {
			  this._term = term;
			  this._votedFor = votedFor;
		 }

		 public virtual MemberId VotedFor()
		 {
			  return _votedFor;
		 }

		 public virtual bool Update( MemberId votedFor, long term )
		 {
			  if ( TermChanged( term ) )
			  {
					this._votedFor = votedFor;
					this._term = term;
					return true;
			  }
			  else
			  {
					if ( this._votedFor == null )
					{
						 if ( votedFor != null )
						 {
							  this._votedFor = votedFor;
							  return true;
						 }
					}
					else if ( !this._votedFor.Equals( votedFor ) )
					{
						 throw new System.ArgumentException( "Can only vote once per term." );
					}
					return false;
			  }
		 }

		 private bool TermChanged( long term )
		 {
			  return term != this._term;
		 }

		 public virtual long Term()
		 {
			  return _term;
		 }

		 public class Marshal : SafeStateMarshal<VoteState>
		 {
			  internal readonly ChannelMarshal<MemberId> MemberMarshal;

			  public Marshal( ChannelMarshal<MemberId> memberMarshal )
			  {
					this.MemberMarshal = memberMarshal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(VoteState state, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( VoteState state, WritableChannel channel )
			  {
					channel.PutLong( state._term );
					MemberMarshal.marshal( state.VotedFor(), channel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public VoteState unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  public override VoteState Unmarshal0( ReadableChannel channel )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long term = channel.getLong();
					long term = channel.Long;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId member = memberMarshal.unmarshal(channel);
					MemberId member = MemberMarshal.unmarshal( channel );
					return new VoteState( member, term );
			  }

			  public override VoteState StartState()
			  {
					return new VoteState();
			  }

			  public override long Ordinal( VoteState state )
			  {
					return state.Term();
			  }
		 }

		 public override string ToString()
		 {
			  return "VoteState{" +
						"votedFor=" + _votedFor +
						", term=" + _term +
						'}';
		 }
	}

}