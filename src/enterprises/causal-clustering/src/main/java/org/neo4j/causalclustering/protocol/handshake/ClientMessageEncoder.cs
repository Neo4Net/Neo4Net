using System;

/*
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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;

	/// <summary>
	/// Encodes messages sent by the client.
	/// </summary>
	public class ClientMessageEncoder : MessageToByteEncoder<ServerMessage>
	{
		 protected internal override void Encode( ChannelHandlerContext ctx, ServerMessage msg, ByteBuf @out )
		 {
			  msg.Dispatch( new Encoder( this, @out ) );
		 }

		 internal class Encoder : ServerMessageHandler
		 {
			 private readonly ClientMessageEncoder _outerInstance;

			  internal readonly ByteBuf Out;

			  internal Encoder( ClientMessageEncoder outerInstance, ByteBuf @out )
			  {
				  this._outerInstance = outerInstance;
					this.Out = @out;
			  }

			  public override void Handle( InitialMagicMessage magicMessage )
			  {
					Out.writeInt( InitialMagicMessage.MESSAGE_CODE );
					StringMarshal.marshal( Out, magicMessage.Magic() );
			  }

			  public override void Handle( ApplicationProtocolRequest applicationProtocolRequest )
			  {
					Out.writeInt( 1 );
					EncodeProtocolRequest( applicationProtocolRequest, ByteBuf.writeInt );
			  }

			  public override void Handle( ModifierProtocolRequest modifierProtocolRequest )
			  {
					Out.writeInt( 2 );
					EncodeProtocolRequest( modifierProtocolRequest, StringMarshal.marshal );
			  }

			  public override void Handle( SwitchOverRequest switchOverRequest )
			  {
					Out.writeInt( 3 );
					StringMarshal.marshal( Out, switchOverRequest.ProtocolName() );
					Out.writeInt( switchOverRequest.Version() );
					Out.writeInt( switchOverRequest.ModifierProtocols().Count );
					switchOverRequest.ModifierProtocols().ForEach(pair =>
					{
								StringMarshal.marshal( Out, pair.first() );
								StringMarshal.marshal( Out, pair.other() );
					});
			  }

			  internal virtual void EncodeProtocolRequest<U>( BaseProtocolRequest<U> protocolRequest, System.Action<ByteBuf, U> writer ) where U : IComparable<U>
			  {
					StringMarshal.marshal( Out, protocolRequest.ProtocolName() );
					Out.writeInt( protocolRequest.Versions().Count );
					protocolRequest.Versions().forEach(version => writer(Out, version));
			  }
		 }
	}

}