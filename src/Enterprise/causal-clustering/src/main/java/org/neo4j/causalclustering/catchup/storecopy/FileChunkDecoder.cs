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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToMessageDecoder = io.netty.handler.codec.MessageToMessageDecoder;

	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;

	/// <summary>
	/// This class does not consume bytes during the decode method. Instead, it puts a <seealso cref="FileChunk"/> object with
	/// a reference to the buffer, to be consumed later. This is the reason it does not extend
	/// <seealso cref="io.netty.handler.codec.ByteToMessageDecoder"/>, since that class fails if an object is added in the out
	/// list but no bytes have been consumed.
	/// </summary>
	public class FileChunkDecoder : MessageToMessageDecoder<ByteBuf>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf msg, java.util.List<Object> out) throws Exception
		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf msg, IList<object> @out )
		 {
			  @out.Add( FileChunk.Marshal().unmarshal(new NetworkReadableClosableChannelNetty4(msg)) );
		 }
	}

}