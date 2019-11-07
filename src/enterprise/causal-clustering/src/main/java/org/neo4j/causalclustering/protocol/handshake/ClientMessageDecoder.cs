using System;
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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;


	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;
	using Neo4Net.Functions;

	/// <summary>
	/// Decodes messages received by the client.
	/// </summary>
	public class ClientMessageDecoder : ByteToMessageDecoder
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf in, java.util.List<Object> out) throws ClientHandshakeException
		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
		 {
			  int messageCode = @in.readInt();

			  switch ( messageCode )
			  {
			  case InitialMagicMessage.MESSAGE_CODE:
			  {
					string magic = StringMarshal.unmarshal( @in );
					@out.Add( new InitialMagicMessage( magic ) );
					return;
			  }
			  case 0:
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					ApplicationProtocolResponse applicationProtocolResponse = DecodeProtocolResponse( ApplicationProtocolResponse::new, ByteBuf.readInt, @in );

					@out.Add( applicationProtocolResponse );
					return;
			  }
			  case 1:
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					ModifierProtocolResponse modifierProtocolResponse = DecodeProtocolResponse( ModifierProtocolResponse::new, StringMarshal.unmarshal, @in );

					@out.Add( modifierProtocolResponse );
					return;
			  }
			  case 2:
			  {
					int statusCodeValue = @in.readInt();
					Optional<StatusCode> statusCode = StatusCode.fromCodeValue( statusCodeValue );
					if ( statusCode.Present )
					{
						 @out.Add( new SwitchOverResponse( statusCode.get() ) );
					}
					else
					{
						 // TODO
					}
					return;
			  }
			  default:
					// TODO
					return;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <U extends Comparable<U>,T extends BaseProtocolResponse<U>> T decodeProtocolResponse(Neo4Net.function.TriFunction<StatusCode,String,U,T> constructor, System.Func<io.netty.buffer.ByteBuf,U> reader, io.netty.buffer.ByteBuf in) throws ClientHandshakeException
		 private T DecodeProtocolResponse<U, T>( TriFunction<StatusCode, string, U, T> constructor, System.Func<ByteBuf, U> reader, ByteBuf @in ) where U : IComparable<U> where T : BaseProtocolResponse<U>
		 {
			  int statusCodeValue = @in.readInt();
			  string identifier = StringMarshal.unmarshal( @in );
			  U version = reader( @in );

			  Optional<StatusCode> statusCode = StatusCode.fromCodeValue( statusCodeValue );

			  return statusCode.map( status => constructor( status, identifier, version ) ).orElseThrow( () => new ClientHandshakeException(string.Format("Unknown status code {0} for protocol {1} version {2}", statusCodeValue, identifier, version)) );
		 }
	}

}