using System;

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
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;

	/// <summary>
	/// Encodes messages sent by the server.
	/// </summary>
	public class ServerMessageEncoder : MessageToByteEncoder<ClientMessage>
	{
		 protected internal override void Encode( ChannelHandlerContext ctx, ClientMessage msg, ByteBuf @out )
		 {
			  msg.Dispatch( new Encoder( this, @out ) );
		 }

		 internal class Encoder : ClientMessageHandler
		 {
			 private readonly ServerMessageEncoder _outerInstance;

			  internal readonly ByteBuf Out;

			  internal Encoder( ServerMessageEncoder outerInstance, ByteBuf @out )
			  {
				  this._outerInstance = outerInstance;
					this.Out = @out;
			  }

			  public override void Handle( InitialMagicMessage magicMessage )
			  {
					Out.writeInt( InitialMagicMessage.MESSAGE_CODE );
					StringMarshal.marshal( Out, magicMessage.Magic() );
			  }

			  public override void Handle( ApplicationProtocolResponse applicationProtocolResponse )
			  {
					Out.writeInt( 0 );
					EncodeProtocolResponse( applicationProtocolResponse, ByteBuf.writeInt );
			  }

			  public override void Handle( ModifierProtocolResponse modifierProtocolResponse )
			  {
					Out.writeInt( 1 );
					EncodeProtocolResponse( modifierProtocolResponse, StringMarshal.marshal );
			  }

			  public override void Handle( SwitchOverResponse switchOverResponse )
			  {
					Out.writeInt( 2 );
					Out.writeInt( switchOverResponse.Status().codeValue() );
			  }

			  internal virtual void EncodeProtocolResponse<U>( BaseProtocolResponse<U> protocolResponse, System.Action<ByteBuf, U> writer ) where U : IComparable<U>
			  {
					Out.writeInt( protocolResponse.StatusCode().codeValue() );
					StringMarshal.marshal( Out, protocolResponse.ProtocolName() );
					writer( Out, protocolResponse.Version() );
			  }
		 }
	}

}