using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClientMessageEncodingTest
	public class ClientMessageEncodingTest
	{
		 private readonly ClientMessage _message;
		 private readonly ServerMessageEncoder _encoder = new ServerMessageEncoder();
		 private readonly ClientMessageDecoder _decoder = new ClientMessageDecoder();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<Object> encodeDecode(ClientMessage message) throws ClientHandshakeException
		 private IList<object> EncodeDecode( ClientMessage message )
		 {
			  ByteBuf byteBuf = Unpooled.directBuffer();
			  IList<object> output = new List<object>();

			  _encoder.encode( null, message, byteBuf );
			  _decoder.decode( null, byteBuf, output );

			  return output;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "ResponseMessage-{0}") public static java.util.Collection<ClientMessage> data()
		 public static ICollection<ClientMessage> Data()
		 {
			  return Arrays.asList(new ApplicationProtocolResponse(StatusCode.Failure, "protocol", 13), new ModifierProtocolResponse(StatusCode.Success, "modifier", "7"), new SwitchOverResponse(StatusCode.Failure)
						);
		 }

		 public ClientMessageEncodingTest( ClientMessage message )
		 {
			  this._message = message;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteEncodingRoundTrip() throws ClientHandshakeException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompleteEncodingRoundTrip()
		 {
			  //when
			  IList<object> output = EncodeDecode( _message );

			  //then
			  assertThat( output, hasSize( 1 ) );
			  assertThat( output[0], equalTo( _message ) );
		 }
	}

}