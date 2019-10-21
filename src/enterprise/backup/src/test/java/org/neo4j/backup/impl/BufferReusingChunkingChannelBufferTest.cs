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
	using Test = org.junit.Test;

	using Neo4Net.Functions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BufferReusingChunkingChannelBufferTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void newBuffersAreCreatedIfNoFreeBuffersAreAvailable()
		 public virtual void NewBuffersAreCreatedIfNoFreeBuffersAreAvailable()
		 {
			  CountingChannelBufferFactory bufferFactory = new CountingChannelBufferFactory();
			  BufferReusingChunkingChannelBuffer buffer = NewBufferReusingChunkingChannelBuffer( 10, bufferFactory );

			  buffer.WriteLong( 1 );
			  buffer.WriteLong( 2 );
			  buffer.WriteLong( 3 );

			  assertEquals( 3, bufferFactory.InstancesCreated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void freeBuffersAreReused() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FreeBuffersAreReused()
		 {
			  CountingChannelBufferFactory bufferFactory = new CountingChannelBufferFactory();
			  BufferReusingChunkingChannelBuffer buffer = NewBufferReusingChunkingChannelBuffer( 10, bufferFactory );

			  buffer.WriteLong( 1 );
			  buffer.WriteLong( 2 );

			  // return 2 buffers to the pool
			  ChannelBuffer reusedBuffer1 = TriggerOperationCompleteCallback( buffer );
			  ChannelBuffer reusedBuffer2 = TriggerOperationCompleteCallback( buffer );

			  buffer.WriteLong( 3 );
			  buffer.WriteLong( 4 );

			  // 2 buffers were created
			  assertEquals( 2, bufferFactory.InstancesCreated );

			  // and 2 buffers were reused
			  verify( reusedBuffer1 ).writeLong( 3 );
			  verify( reusedBuffer2 ).writeLong( 4 );
		 }

		 private static BufferReusingChunkingChannelBuffer NewBufferReusingChunkingChannelBuffer( int capacity, CountingChannelBufferFactory bufferFactory )
		 {
			  ChannelBuffer buffer = ChannelBuffers.dynamicBuffer();

			  Channel channel = mock( typeof( Channel ) );
			  ChannelFuture channelFuture = mock( typeof( ChannelFuture ) );
			  when( channel.Open ).thenReturn( true );
			  when( channel.Connected ).thenReturn( true );
			  when( channel.Bound ).thenReturn( true );
			  when( channel.write( any() ) ).thenReturn(channelFuture);

			  return new BufferReusingChunkingChannelBuffer( buffer, bufferFactory, channel, capacity, ( sbyte ) 1, ( sbyte ) 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.jboss.netty.buffer.ChannelBuffer triggerOperationCompleteCallback(BufferReusingChunkingChannelBuffer buffer) throws Exception
		 private static ChannelBuffer TriggerOperationCompleteCallback( BufferReusingChunkingChannelBuffer buffer )
		 {
			  ChannelBuffer reusedBuffer = spy( ChannelBuffers.dynamicBuffer() );

			  ChannelFuture channelFuture = mock( typeof( ChannelFuture ) );
			  when( channelFuture.Done ).thenReturn( true );
			  when( channelFuture.Success ).thenReturn( true );

			  buffer.NewChannelFutureListener( reusedBuffer ).operationComplete( channelFuture );
			  return reusedBuffer;
		 }

		 private class CountingChannelBufferFactory : IFactory<ChannelBuffer>
		 {
			  internal int InstancesCreated;

			  public override ChannelBuffer NewInstance()
			  {
					InstancesCreated++;
					return ChannelBuffers.dynamicBuffer();
			  }
		 }
	}

}