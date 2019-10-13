﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;


	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	public class Protocol214 : Protocol
	{
		 public Protocol214( int chunkSize, sbyte applicationProtocolVersion, sbyte internalProtocolVersion ) : base( chunkSize, applicationProtocolVersion, internalProtocolVersion )
		 {
		 }

		 protected internal override StoreId ReadStoreId( ChannelBuffer source, ByteBuffer byteBuffer )
		 {
			  byteBuffer.clear();
			  byteBuffer.limit( 8 + 8 + 8 + 8 + 8 ); // creation time, random id, store version, upgrade time, upgrade id
			  source.readBytes( byteBuffer );
			  byteBuffer.flip();
			  // read order matters - see Server.writeStoreId() for version 2.1.4
			  long creationTime = byteBuffer.Long;
			  long randomId = byteBuffer.Long;
			  long storeVersion = byteBuffer.Long;
			  long upgradeTime = byteBuffer.Long;
			  long upgradeId = byteBuffer.Long;
			  return new StoreId( creationTime, randomId, storeVersion, upgradeTime, upgradeId );
		 }
	}

}