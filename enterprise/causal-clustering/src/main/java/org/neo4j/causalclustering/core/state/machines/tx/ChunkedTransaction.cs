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
namespace Org.Neo4j.causalclustering.core.state.machines.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;


	using ErrorHandler = Org.Neo4j.causalclustering.helper.ErrorHandler;
	using ChunkingNetworkChannel = Org.Neo4j.causalclustering.messaging.ChunkingNetworkChannel;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;

	internal class ChunkedTransaction : ChunkedInput<ByteBuf>
	{
		 private const int CHUNK_SIZE = 32 * 1024;
		 private readonly ReplicatedTransactionFactory.TransactionRepresentationWriter _txWriter;
		 private ChunkingNetworkChannel _channel;
		 private LinkedList<ByteBuf> _chunks = new LinkedList<ByteBuf>();

		 internal ChunkedTransaction( TransactionRepresentation tx )
		 {
			  _txWriter = ReplicatedTransactionFactory.TransactionalRepresentationWriter( tx );
		 }

		 public override bool EndOfInput
		 {
			 get
			 {
				  return _channel != null && _channel.closed() && _chunks.Count == 0;
			 }
		 }

		 public override void Close()
		 {
			  using ( ErrorHandler errorHandler = new ErrorHandler( "Closing ChunkedTransaction" ) )
			  {
					if ( _channel != null )
					{
						 errorHandler.Execute( () => _channel.close() );
					}
					_chunks.forEach( byteBuf => errorHandler.execute( () => ReferenceCountUtil.release(byteBuf) ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		 public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
		 {
			  return ReadChunk( ctx.alloc() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.buffer.ByteBufAllocator allocator) throws Exception
		 public override ByteBuf ReadChunk( ByteBufAllocator allocator )
		 {
			  if ( EndOfInput )
			  {
					return null;
			  }
			  if ( _channel == null )
			  {
					// Ensure that the written buffers does not overflow the allocators chunk size.
					_channel = new ChunkingNetworkChannel( allocator, CHUNK_SIZE, _chunks );
			  }

			  // write to chunks if empty and there is more to write
			  while ( _txWriter.canWrite() && _chunks.Count == 0 )
			  {
					_txWriter.write( _channel );
			  }
			  // nothing more to write, close the channel to get the potential last buffer
			  if ( _chunks.Count == 0 )
			  {
					_channel.close();
			  }
			  return _chunks.RemoveFirst();
		 }

		 public override long Length()
		 {
			  return -1;
		 }

		 public override long Progress()
		 {
			  return 0;
		 }
	}

}