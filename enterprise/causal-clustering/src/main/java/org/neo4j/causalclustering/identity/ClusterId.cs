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

	public class ClusterId
	{
		 private readonly System.Guid _uuid;

		 public ClusterId( System.Guid uuid )
		 {
			  this._uuid = uuid;
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
			  ClusterId clusterId = ( ClusterId ) o;
			  return Objects.Equals( _uuid, clusterId._uuid );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _uuid );
		 }

		 public virtual System.Guid Uuid()
		 {
			  return _uuid;
		 }

		 public override string ToString()
		 {
			  return "ClusterId{" +
						"uuid=" + _uuid +
						'}';
		 }

		 public class Marshal : SafeChannelMarshal<ClusterId>
		 {
			  public static readonly Marshal Instance = new Marshal();
			  internal static readonly System.Guid Nil = new System.Guid( 0L, 0L );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(ClusterId clusterId, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( ClusterId clusterId, WritableChannel channel )
			  {
					System.Guid uuid = clusterId == null ? Nil : clusterId.uuid;
					channel.PutLong( uuid.MostSignificantBits );
					channel.PutLong( uuid.LeastSignificantBits );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterId unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  public override ClusterId Unmarshal0( ReadableChannel channel )
			  {
					long mostSigBits = channel.Long;
					long leastSigBits = channel.Long;
					System.Guid uuid = new System.Guid( mostSigBits, leastSigBits );

					return uuid.Equals( Nil ) ? null : new ClusterId( uuid );
			  }
		 }
	}

}