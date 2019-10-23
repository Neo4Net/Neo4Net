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
namespace Neo4Net.causalclustering.messaging.marshalling.storeid
{
	using DecoderException = io.netty.handler.codec.DecoderException;

	using Neo4Net.causalclustering.core.state.storage;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public sealed class StoreIdMarshal : SafeChannelMarshal<StoreId>
	{
		 public static readonly StoreIdMarshal Instance = new StoreIdMarshal();

		 private StoreIdMarshal()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.Neo4Net.causalclustering.identity.StoreId storeId, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
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
//ORIGINAL LINE: protected org.Neo4Net.causalclustering.identity.StoreId unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
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