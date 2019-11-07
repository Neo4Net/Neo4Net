/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.v1.transport.socket
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using UnpooledByteBufAllocator = io.netty.buffer.UnpooledByteBufAllocator;
	using Channel = io.netty.channel.Channel;

	using TransportThrottleGroup = Neo4Net.Bolt.transport.TransportThrottleGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.transport.ChunkedOutput.CHUNK_HEADER_SIZE;

	/// <summary>
	/// Helper to chunk up serialized data for testing </summary>
	public class Chunker
	{
		 private Chunker()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] chunk(int maxChunkSize, byte[][] messages) throws java.io.IOException
		 public static sbyte[] Chunk( int maxChunkSize, sbyte[][] messages )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer outputBuffer = ByteBuffer.allocate(1024 * 8);
			  ByteBuffer outputBuffer = ByteBuffer.allocate( 1024 * 8 );

			  Channel ch = mock( typeof( Channel ) );
			  when( ch.alloc() ).thenReturn(UnpooledByteBufAllocator.DEFAULT);
			  when( ch.writeAndFlush( any(), Null ) ).then(inv =>
			  {
				ByteBuf buf = inv.getArgument( 0 );
				outputBuffer.limit( outputBuffer.position() + buf.readableBytes() );
				buf.readBytes( outputBuffer );
				buf.release();
				return null;
			  });

			  int maxBufferSize = maxChunkSize + CHUNK_HEADER_SIZE;
			  ChunkedOutput @out = new ChunkedOutput( ch, maxBufferSize, maxBufferSize, TransportThrottleGroup.NO_THROTTLE );

			  foreach ( sbyte[] message in messages )
			  {
					@out.BeginMessage();
					@out.WriteBytes( message, 0, message.Length );
					@out.MessageSucceeded();
			  }
			  @out.Flush();
			  @out.Dispose();

			  sbyte[] bytes = new sbyte[outputBuffer.limit()];
			  outputBuffer.position( 0 );
			  outputBuffer.get( bytes );
			  return bytes;
		 }
	}

}