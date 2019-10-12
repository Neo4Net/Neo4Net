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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PrepareStoreCopyRequestMarshalTest
	{
		 internal EmbeddedChannel EmbeddedChannel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  EmbeddedChannel = new EmbeddedChannel( new PrepareStoreCopyRequestEncoder(), new PrepareStoreCopyRequestDecoder() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeIdIsTransmitted()
		 public virtual void StoreIdIsTransmitted()
		 {
			  // given store id requests transmit store id
			  StoreId storeId = new StoreId( 1, 2, 3, 4 );
			  PrepareStoreCopyRequest prepareStoreCopyRequest = new PrepareStoreCopyRequest( storeId );

			  // when transmitted
			  SendToChannel( prepareStoreCopyRequest, EmbeddedChannel );

			  // then it can be received/deserialised
			  PrepareStoreCopyRequest prepareStoreCopyRequestRead = EmbeddedChannel.readInbound();
			  assertEquals( prepareStoreCopyRequest.StoreId, prepareStoreCopyRequestRead.StoreId );
		 }

		 public static void SendToChannel<E>( E e, EmbeddedChannel embeddedChannel )
		 {
			  embeddedChannel.writeOutbound( e );

			  ByteBuf @object = embeddedChannel.readOutbound();
			  embeddedChannel.writeInbound( @object );
		 }
	}

}