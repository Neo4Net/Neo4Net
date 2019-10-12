using System;
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
namespace Org.Neo4j.causalclustering.protocol.handshake
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;


	using StringMarshal = Org.Neo4j.causalclustering.messaging.marshalling.StringMarshal;
	using Org.Neo4j.Helpers.Collection;

	/// <summary>
	/// Decodes messages received by the server.
	/// </summary>
	public class ServerMessageDecoder : ByteToMessageDecoder
	{
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
			  case 1:
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					ApplicationProtocolRequest applicationProtocolRequest = DecodeProtocolRequest( ApplicationProtocolRequest::new, @in, ByteBuf.readInt );
					@out.Add( applicationProtocolRequest );
					return;
			  }
			  case 2:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					ModifierProtocolRequest modifierProtocolRequest = DecodeProtocolRequest( ModifierProtocolRequest::new, @in, StringMarshal.unmarshal );
					@out.Add( modifierProtocolRequest );
					return;
			  case 3:
			  {
					string protocolName = StringMarshal.unmarshal( @in );
					int version = @in.readInt();
					int numberOfModifierProtocols = @in.readInt();
					IList<Pair<string, string>> modifierProtocols = Stream.generate( () => Pair.of(StringMarshal.unmarshal(@in), StringMarshal.unmarshal(@in)) ).limit(numberOfModifierProtocols).collect(Collectors.toList());
					@out.Add( new SwitchOverRequest( protocolName, version, modifierProtocols ) );
					return;
			  }
			  default:
					// TODO
					return;
			  }
		 }

		 private T DecodeProtocolRequest<U, T>( System.Func<string, ISet<U>, T> constructor, ByteBuf @in, System.Func<ByteBuf, U> versionDecoder ) where U : IComparable<U> where T : BaseProtocolRequest<U>
		 {
			  string protocolName = StringMarshal.unmarshal( @in );
			  int versionArrayLength = @in.readInt();

			  ISet<U> versions = Stream.generate( () => versionDecoder(@in) ).limit(versionArrayLength).collect(Collectors.toSet());

			  return constructor( protocolName, versions );
		 }
	}

}