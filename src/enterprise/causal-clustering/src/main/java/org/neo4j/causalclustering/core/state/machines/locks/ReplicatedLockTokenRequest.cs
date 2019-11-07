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
namespace Neo4Net.causalclustering.core.state.machines.locks
{

	using CoreReplicatedContent = Neo4Net.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;

	public class ReplicatedLockTokenRequest : CoreReplicatedContent, LockToken
	{
		 private readonly MemberId _owner;
		 private readonly int _candidateId;

		 internal static readonly ReplicatedLockTokenRequest InvalidReplicatedLockTokenRequest = new ReplicatedLockTokenRequest( null, LockToken_Fields.INVALID_LOCK_TOKEN_ID );

		 public ReplicatedLockTokenRequest( MemberId owner, int candidateId )
		 {
			  this._owner = owner;
			  this._candidateId = candidateId;
		 }

		 public override int Id()
		 {
			  return _candidateId;
		 }

		 public override MemberId Owner()
		 {
			  return _owner;
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
			  ReplicatedLockTokenRequest that = ( ReplicatedLockTokenRequest ) o;
			  return _candidateId == that._candidateId && Objects.Equals( _owner, that._owner );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _owner, _candidateId );
		 }

		 public override string ToString()
		 {
			  return format( "ReplicatedLockTokenRequest{owner=%s, candidateId=%d}", _owner, _candidateId );
		 }

		 public override void Dispatch( CommandDispatcher commandDispatcher, long commandIndex, System.Action<Result> callback )
		 {
			  commandDispatcher.Dispatch( this, commandIndex, callback );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }
	}

}