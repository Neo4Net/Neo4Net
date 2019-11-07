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
namespace Neo4Net.causalclustering.core.state.machines.id
{

	using CoreReplicatedContent = Neo4Net.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

	/// <summary>
	/// This type is handled by the ReplicatedIdAllocationStateMachine. 
	/// </summary>
	public class ReplicatedIdAllocationRequest : CoreReplicatedContent
	{
		 private readonly MemberId _owner;
		 private readonly IdType _idType;
		 private readonly long _idRangeStart;
		 private readonly int _idRangeLength;

		 public ReplicatedIdAllocationRequest( MemberId owner, IdType idType, long idRangeStart, int idRangeLength )
		 {
			  this._owner = owner;
			  this._idType = idType;
			  this._idRangeStart = idRangeStart;
			  this._idRangeLength = idRangeLength;
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

			  ReplicatedIdAllocationRequest that = ( ReplicatedIdAllocationRequest ) o;

			  if ( _idRangeStart != that._idRangeStart )
			  {
					return false;
			  }
			  if ( _idRangeLength != that._idRangeLength )
			  {
					return false;
			  }
			  if ( !_owner.Equals( that._owner ) )
			  {
					return false;
			  }
			  return _idType == that._idType;
		 }

		 public override int GetHashCode()
		 {
			  int result = _owner.GetHashCode();
			  result = 31 * result + _idType.GetHashCode();
			  result = 31 * result + ( int )( _idRangeStart ^ ( ( long )( ( ulong )_idRangeStart >> 32 ) ) );
			  result = 31 * result + _idRangeLength;
			  return result;
		 }

		 public virtual MemberId Owner()
		 {
			  return _owner;
		 }

		 public virtual IdType IdType()
		 {
			  return _idType;
		 }

		 internal virtual long IdRangeStart()
		 {
			  return _idRangeStart;
		 }

		 internal virtual int IdRangeLength()
		 {
			  return _idRangeLength;
		 }

		 public override string ToString()
		 {
			  return format( "ReplicatedIdAllocationRequest{owner=%s, idType=%s, idRangeStart=%d, idRangeLength=%d}", _owner, _idType, _idRangeStart, _idRangeLength );
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