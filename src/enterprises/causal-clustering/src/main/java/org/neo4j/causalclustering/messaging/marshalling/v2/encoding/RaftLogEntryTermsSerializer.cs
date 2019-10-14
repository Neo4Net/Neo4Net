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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.encoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;

	internal class RaftLogEntryTermsSerializer
	{
		 internal static ByteBuf SerializeTerms( RaftLogEntry[] raftLogEntries, ByteBufAllocator byteBufAllocator )
		 {
			  int capacity = ( sizeof( sbyte ) * 8 ) + ( sizeof( int ) * 8 ) + ( sizeof( long ) * 8 ) * raftLogEntries.Length;
			  ByteBuf buffer = byteBufAllocator.buffer( capacity, capacity );
			  buffer.writeByte( ContentType.RaftLogEntryTerms.get() );
			  buffer.writeInt( raftLogEntries.Length );
			  foreach ( RaftLogEntry raftLogEntry in raftLogEntries )
			  {
					buffer.writeLong( raftLogEntry.Term() );
			  }
			  return buffer;
		 }
	}

}