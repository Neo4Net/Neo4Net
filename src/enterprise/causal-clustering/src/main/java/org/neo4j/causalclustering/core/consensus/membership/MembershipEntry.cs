using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// Represents a membership entry in the RAFT log.
	/// </summary>
	public class MembershipEntry
	{
		 private long _logIndex;
		 private ISet<MemberId> _members;

		 public MembershipEntry( long logIndex, ISet<MemberId> members )
		 {
			  this._members = members;
			  this._logIndex = logIndex;
		 }

		 public virtual long LogIndex()
		 {
			  return _logIndex;
		 }

		 public virtual ISet<MemberId> Members()
		 {
			  return _members;
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
			  MembershipEntry that = ( MembershipEntry ) o;
			  return _logIndex == that._logIndex && Objects.Equals( _members, that._members );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _logIndex, _members );
		 }

		 public override string ToString()
		 {
			  return "MembershipEntry{" +
						"logIndex=" + _logIndex +
						", members=" + _members +
						'}';
		 }

		 public class Marshal : SafeStateMarshal<MembershipEntry>
		 {
			  internal MemberId.Marshal MemberMarshal = new MemberId.Marshal();

			  public override MembershipEntry StartState()
			  {
					return null;
			  }

			  public override long Ordinal( MembershipEntry entry )
			  {
					return entry._logIndex;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(MembershipEntry entry, Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( MembershipEntry entry, WritableChannel channel )
			  {
					if ( entry == null )
					{
						 channel.PutInt( 0 );
						 return;
					}
					else
					{
						 channel.PutInt( 1 );
					}

					channel.PutLong( entry._logIndex );
					channel.PutInt( entry._members.Count );
					foreach ( MemberId member in entry._members )
					{
						 MemberMarshal.marshal( member, channel );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected MembershipEntry unmarshal0(Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, Neo4Net.causalclustering.messaging.EndOfStreamException
			  protected internal override MembershipEntry Unmarshal0( ReadableChannel channel )
			  {
					int hasEntry = channel.Int;
					if ( hasEntry == 0 )
					{
						 return null;
					}
					long logIndex = channel.Long;
					int memberCount = channel.Int;
					ISet<MemberId> members = new HashSet<MemberId>();
					for ( int i = 0; i < memberCount; i++ )
					{
						 members.Add( MemberMarshal.unmarshal( channel ) );
					}
					return new MembershipEntry( logIndex, members );
			  }
		 }
	}

}