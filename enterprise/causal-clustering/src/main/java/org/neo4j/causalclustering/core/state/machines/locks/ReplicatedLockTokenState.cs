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
namespace Org.Neo4j.causalclustering.core.state.machines.locks
{

	using Org.Neo4j.causalclustering.messaging.marshalling;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest.INVALID_REPLICATED_LOCK_TOKEN_REQUEST;

	public class ReplicatedLockTokenState
	{
		 private ReplicatedLockTokenRequest _currentToken = INVALID_REPLICATED_LOCK_TOKEN_REQUEST;
		 private long _ordinal = -1L;

		 public ReplicatedLockTokenState()
		 {
		 }

		 public ReplicatedLockTokenState( long ordinal, ReplicatedLockTokenRequest currentToken )
		 {
			  this._ordinal = ordinal;
			  this._currentToken = currentToken;
		 }

		 public virtual void Set( ReplicatedLockTokenRequest currentToken, long ordinal )
		 {
			  this._currentToken = currentToken;
			  this._ordinal = ordinal;
		 }

		 public virtual ReplicatedLockTokenRequest Get()
		 {
			  return _currentToken;
		 }

		 internal virtual long Ordinal()
		 {
			  return _ordinal;
		 }

		 public override string ToString()
		 {
			  return string.Format( "ReplicatedLockTokenState{{currentToken={0}, ordinal={1:D}}}", _currentToken, _ordinal );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  ReplicatedLockTokenState that = ( ReplicatedLockTokenState ) o;
			  return _ordinal == that._ordinal && Objects.Equals( _currentToken, that._currentToken );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _currentToken, _ordinal );
		 }

		 internal virtual ReplicatedLockTokenState NewInstance()
		 {
			  return new ReplicatedLockTokenState( _ordinal, _currentToken );
		 }

		 public class Marshal : SafeStateMarshal<ReplicatedLockTokenState>
		 {
			  internal readonly ChannelMarshal<MemberId> MemberMarshal;

			  public Marshal( ChannelMarshal<MemberId> memberMarshal )
			  {
					this.MemberMarshal = memberMarshal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(ReplicatedLockTokenState state, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( ReplicatedLockTokenState state, WritableChannel channel )
			  {
					channel.PutLong( state._ordinal );
					channel.PutInt( state.Get().id() );
					MemberMarshal.marshal( state.Get().owner(), channel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ReplicatedLockTokenState unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  public override ReplicatedLockTokenState Unmarshal0( ReadableChannel channel )
			  {
					long logIndex = channel.Long;
					int candidateId = channel.Int;
					MemberId member = MemberMarshal.unmarshal( channel );

					return new ReplicatedLockTokenState( logIndex, new ReplicatedLockTokenRequest( member, candidateId ) );
			  }

			  public override ReplicatedLockTokenState StartState()
			  {
					return new ReplicatedLockTokenState();
			  }

			  public override long Ordinal( ReplicatedLockTokenState state )
			  {
					return state.Ordinal();
			  }
		 }
	}

}