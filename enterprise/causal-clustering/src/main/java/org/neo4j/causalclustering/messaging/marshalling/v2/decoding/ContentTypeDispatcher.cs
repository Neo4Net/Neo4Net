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
namespace Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;

	using Org.Neo4j.causalclustering.catchup;

	public class ContentTypeDispatcher : ChannelInboundHandlerAdapter
	{
		 private readonly Protocol<ContentType> _contentTypeProtocol;

		 public ContentTypeDispatcher( Protocol<ContentType> contentTypeProtocol )
		 {
			  this._contentTypeProtocol = contentTypeProtocol;
		 }

		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  if ( msg is ByteBuf )
			  {
					ByteBuf buffer = ( ByteBuf ) msg;
					if ( _contentTypeProtocol.isExpecting( ContentType.ContentType ) )
					{
						 sbyte messageCode = buffer.readByte();
						 ContentType contentType = GetContentType( messageCode );
						 _contentTypeProtocol.expect( contentType );
						 if ( buffer.readableBytes() == 0 )
						 {
							  ReferenceCountUtil.release( msg );
							  return;
						 }
					}
			  }
			  ctx.fireChannelRead( msg );
		 }

		 private ContentType GetContentType( sbyte messageCode )
		 {
			  foreach ( ContentType contentType in ContentType.values() )
			  {
					if ( contentType.get() == messageCode )
					{
						 return contentType;
					}
			  }
			  throw new System.ArgumentException( "Illegal inbound. Could not find a ContentType with value " + messageCode );
		 }
	}

}