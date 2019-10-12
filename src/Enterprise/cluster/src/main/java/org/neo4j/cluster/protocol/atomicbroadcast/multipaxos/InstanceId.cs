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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using Neo4Net.cluster.com.message;

	/// <summary>
	/// Id of a particular Paxos instance.
	/// </summary>
	[Serializable]
	public class InstanceId : IComparable<InstanceId>
	{
		 private const long SERIAL_VERSION_UID = 2505002855546341672L;

		 public const string INSTANCE = "instance";

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal long IdConflict;

		 public InstanceId( Message message ) : this( message.getHeader( INSTANCE ) )
		 {
		 }

		 public InstanceId( string @string ) : this( long.Parse( @string ) )
		 {
		 }

		 public InstanceId( long id )
		 {
			  this.IdConflict = id;
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return IdConflict;
			 }
		 }

		 public override int CompareTo( InstanceId o )
		 {
			  return Long.compare( IdConflict, o.Id );
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

			  InstanceId that = ( InstanceId ) o;

			  return IdConflict == that.id;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( IdConflict ^ ( ( long )( ( ulong )IdConflict >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return Convert.ToString( IdConflict );
		 }
	}

}