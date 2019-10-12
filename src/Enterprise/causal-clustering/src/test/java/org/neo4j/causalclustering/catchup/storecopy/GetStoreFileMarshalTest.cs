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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using StoreId = Neo4Net.causalclustering.identity.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class GetStoreFileMarshalTest
	{
		 internal EmbeddedChannel EmbeddedChannel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  EmbeddedChannel = new EmbeddedChannel( new GetStoreFileRequest.Encoder(), new GetStoreFileRequest.Decoder() );
		 }

		 private static readonly StoreId _expectedStore = new StoreId( 1, 2, 3, 4 );
		 private static readonly File _expectedFile = new File( "abc.123" );
		 private const long? EXPECTED_LAST_TRANSACTION = 1234L;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getsTransmitted()
		 public virtual void GetsTransmitted()
		 {
			  // given
			  GetStoreFileRequest expectedStoreRequest = new GetStoreFileRequest( _expectedStore, _expectedFile, EXPECTED_LAST_TRANSACTION.Value );

			  // when
			  SendToChannel( expectedStoreRequest, EmbeddedChannel );

			  // then
			  GetStoreFileRequest actualStoreRequest = EmbeddedChannel.readInbound();
			  assertEquals( _expectedStore, actualStoreRequest.ExpectedStoreId() );
			  assertEquals( _expectedFile, actualStoreRequest.File() );
			  assertEquals( EXPECTED_LAST_TRANSACTION.Value, actualStoreRequest.RequiredTransactionId() );
		 }

		 private static void SendToChannel( GetStoreFileRequest getStoreFileRequest, EmbeddedChannel embeddedChannel )
		 {
			  embeddedChannel.writeOutbound( getStoreFileRequest );

			  ByteBuf @object = embeddedChannel.readOutbound();
			  embeddedChannel.writeInbound( @object );
		 }
	}

}