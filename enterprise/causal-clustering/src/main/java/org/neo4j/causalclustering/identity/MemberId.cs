using System;

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
namespace Org.Neo4j.causalclustering.identity
{

	using Org.Neo4j.causalclustering.core.state.storage;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	[Serializable]
	public class MemberId
	{
		 private const long SERIAL_VERSION_UID = -984540169345015775L;
		 private readonly System.Guid _uuid;
		 // for serialization compatibility with previous versions this field should not be removed.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "FieldMayBeStatic"}) private final String shortName = "";
		 private readonly string _shortName = "";

		 public MemberId( System.Guid uuid )
		 {
			  Objects.requireNonNull( uuid );
			  this._uuid = uuid;
		 }

		 public virtual System.Guid Uuid
		 {
			 get
			 {
				  return _uuid;
			 }
		 }

		 public override string ToString()
		 {
			  return format( "MemberId{%s}", _uuid.ToString().Substring(0, 8) );
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

			  MemberId that = ( MemberId ) o;
			  return Objects.Equals( _uuid, that._uuid );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _uuid );
		 }

		 /// <summary>
		 /// Format:
		 /// ┌──────────────────────────────┐
		 /// │mostSignificantBits    8 bytes│
		 /// │leastSignificantBits   8 bytes│
		 /// └──────────────────────────────┘
		 /// </summary>
		 public class Marshal : SafeStateMarshal<MemberId>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(MemberId memberId, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( MemberId memberId, WritableChannel channel )
			  {
					if ( memberId == null )
					{
						 channel.Put( ( sbyte ) 0 );
					}
					else
					{
						 channel.Put( ( sbyte ) 1 );
						 channel.PutLong( memberId._uuid.MostSignificantBits );
						 channel.PutLong( memberId._uuid.LeastSignificantBits );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public MemberId unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  public override MemberId Unmarshal0( ReadableChannel channel )
			  {
					sbyte nullMarker = channel.Get();
					if ( nullMarker == 0 )
					{
						 return null;
					}
					else
					{
						 long mostSigBits = channel.Long;
						 long leastSigBits = channel.Long;
						 return new MemberId( new System.Guid( mostSigBits, leastSigBits ) );
					}
			  }

			  public override MemberId StartState()
			  {
					return null;
			  }

			  public override long Ordinal( MemberId memberId )
			  {
					return memberId == null ? 0 : 1;
			  }
		 }
	}

}