using System.Collections.Generic;

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
namespace Neo4Net.backup.impl
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelFuture = org.jboss.netty.channel.ChannelFuture;
	using ChannelFutureListener = org.jboss.netty.channel.ChannelFutureListener;


	using ChunkingChannelBuffer = Neo4Net.com.ChunkingChannelBuffer;
	using Neo4Net.Functions;

	/// <summary>
	/// <seealso cref="ChunkingChannelBuffer Chunking buffer"/> that is able to reuse up to <seealso cref="MAX_WRITE_AHEAD_CHUNKS"/>
	/// netty channel buffers.
	/// <para>
	/// Buffer is considered to be free when future corresponding to the call <seealso cref="Channel.write(object)"/> is completed.
	/// Argument to <seealso cref="Channel.write(object)"/> is <seealso cref="ChannelBuffer"/>.
	/// Method <seealso cref="ChannelFutureListener.operationComplete(ChannelFuture)"/> is called upon future completion and
	/// than <seealso cref="ChannelBuffer"/> is returned to the queue of free buffers.
	/// </para>
	/// <para>
	/// Allocation of buffers is traded for allocation of <seealso cref="ChannelFutureListener"/>s that returned buffers to the
	/// queue of free buffers.
	/// </para>
	/// </summary>
	internal class BufferReusingChunkingChannelBuffer : ChunkingChannelBuffer
	{
		 private static readonly IFactory<ChannelBuffer> _defaultChannelBufferFactory = ChannelBuffers.dynamicBuffer;

		 private readonly IFactory<ChannelBuffer> _bufferFactory;
		 private readonly LinkedList<ChannelBuffer> _freeBuffers = new LinkedBlockingQueue<ChannelBuffer>( MAX_WRITE_AHEAD_CHUNKS );

		 internal BufferReusingChunkingChannelBuffer( ChannelBuffer initialBuffer, Channel channel, int capacity, sbyte internalProtocolVersion, sbyte applicationProtocolVersion ) : this( initialBuffer, _defaultChannelBufferFactory, channel, capacity, internalProtocolVersion, applicationProtocolVersion )
		 {
		 }

		 internal BufferReusingChunkingChannelBuffer( ChannelBuffer initialBuffer, IFactory<ChannelBuffer> bufferFactory, Channel channel, int capacity, sbyte internalProtocolVersion, sbyte applicationProtocolVersion ) : base( initialBuffer, channel, capacity, internalProtocolVersion, applicationProtocolVersion )
		 {
			  this._bufferFactory = bufferFactory;
		 }

		 protected internal override ChannelBuffer NewChannelBuffer()
		 {
			  ChannelBuffer buffer = _freeBuffers.RemoveFirst();
			  return ( buffer == null ) ? _bufferFactory.newInstance() : buffer;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.jboss.netty.channel.ChannelFutureListener newChannelFutureListener(final org.jboss.netty.buffer.ChannelBuffer buffer)
		 protected internal override ChannelFutureListener NewChannelFutureListener( ChannelBuffer buffer )
		 {
			  return future =>
			  {
				buffer.clear();
				_freeBuffers.AddLast( buffer );
				BufferReusingChunkingChannelBuffer.this.OperationComplete( future );
			  };
		 }
	}

}