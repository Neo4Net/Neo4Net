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
namespace Org.Neo4j.causalclustering.messaging.marshalling.storeid
{
	using DecoderException = io.netty.handler.codec.DecoderException;

	using Org.Neo4j.causalclustering.core.state.storage;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public sealed class StoreIdMarshal : SafeChannelMarshal<StoreId>
	{
		 public static readonly StoreIdMarshal Instance = new StoreIdMarshal();

		 private StoreIdMarshal()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.neo4j.causalclustering.identity.StoreId storeId, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public override void Marshal( StoreId storeId, WritableChannel channel )
		 {
			  if ( storeId == null )
			  {
					channel.Put( ( sbyte ) 0 );
					return;
			  }

			  channel.Put( ( sbyte ) 1 );
			  channel.PutLong( storeId.CreationTime );
			  channel.PutLong( storeId.RandomId );
			  channel.PutLong( storeId.UpgradeTime );
			  channel.PutLong( storeId.UpgradeId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.causalclustering.identity.StoreId unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 protected internal override StoreId Unmarshal0( ReadableChannel channel )
		 {
			  sbyte exists = channel.Get();
			  if ( exists == 0 )
			  {
					return null;
			  }
			  else if ( exists != 1 )
			  {
					throw new DecoderException( "Unexpected value: " + exists );
			  }

			  long creationTime = channel.Long;
			  long randomId = channel.Long;
			  long upgradeTime = channel.Long;
			  long upgradeId = channel.Long;
			  return new StoreId( creationTime, randomId, upgradeTime, upgradeId );
		 }
	}

}