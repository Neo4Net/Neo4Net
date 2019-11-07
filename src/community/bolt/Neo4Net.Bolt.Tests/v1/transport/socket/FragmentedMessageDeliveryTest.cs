using System.Collections.Generic;
using System.Text;

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
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using SynchronousBoltConnection = Neo4Net.Bolt.runtime.SynchronousBoltConnection;
	using BoltRequestMessageWriter = Neo4Net.Bolt.v1.messaging.BoltRequestMessageWriter;
	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using RecordingByteChannel = Neo4Net.Bolt.v1.messaging.RecordingByteChannel;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using Neo4NetPackV2 = Neo4Net.Bolt.v2.messaging.Neo4NetPackV2;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.Unpooled.wrappedBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	/// <summary>
	/// This tests network fragmentation of messages. Given a set of messages, it will serialize and chunk the message up
	/// to a specified chunk size. Then it will split that data into a specified number of fragments, trying every possible
	/// permutation of fragment sizes for the specified number. For instance, assuming an unfragmented message size of 15,
	/// and a fragment count of 3, it will create fragment size permutations like:
	/// <p/>
	/// [1,1,13]
	/// [1,2,12]
	/// [1,3,11]
	/// ..
	/// [12,1,1]
	/// <p/>
	/// For each permutation, it delivers the fragments to the protocol implementation, and asserts the protocol handled
	/// them properly.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FragmentedMessageDeliveryTest
	public class FragmentedMessageDeliveryTest
	{
		 private EmbeddedChannel _channel;
		 // Only test one chunk size for now, this can be parameterized to test lots of different ones
		 private int _chunkSize = 16;

		 // Only test one message for now. This can be parameterized later to test lots of different ones
		 private RequestMessage[] _messages = new RequestMessage[]{ new RunMessage( "Mjölnir" ) };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack;
		 public Neo4NetPack Neo4NetPack;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<Neo4Net.bolt.messaging.Neo4NetPack> parameters()
		 public static IList<Neo4NetPack> Parameters()
		 {
			  return Arrays.asList( new Neo4NetPackV1(), new Neo4NetPackV2() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _channel != null )
			  {
					_channel.finishAndReleaseAll();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFragmentedMessageDelivery() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFragmentedMessageDelivery()
		 {
			  // Given
			  sbyte[] unfragmented = Serialize( _chunkSize, _messages );

			  // When & Then
			  int n = unfragmented.Length;
			  for ( int i = 1; i < n - 1; i++ )
			  {
					for ( int j = 1; j < n - i; j++ )
					{
						 TestPermutation( unfragmented, i, j, n - i - j );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testPermutation(byte[] unfragmented, int... sizes) throws Exception
		 private void TestPermutation( sbyte[] unfragmented, params int[] sizes )
		 {
			  int pos = 0;
			  ByteBuf[] fragments = new ByteBuf[sizes.Length];
			  for ( int i = 0; i < sizes.Length; i++ )
			  {
					fragments[i] = wrappedBuffer( unfragmented, pos, sizes[i] );
					pos += sizes[i];
			  }
			  TestPermutation( unfragmented, fragments );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testPermutation(byte[] unfragmented, io.netty.buffer.ByteBuf[] fragments) throws Exception
		 private void TestPermutation( sbyte[] unfragmented, ByteBuf[] fragments )
		 {
			  // Given
			  _channel = new EmbeddedChannel();
			  BoltChannel boltChannel = NewBoltChannel( _channel );

			  BoltStateMachine machine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection boltConnection = new SynchronousBoltConnection( machine );
			  NullLogService logging = NullLogService.Instance;
			  BoltProtocol boltProtocol = new BoltProtocolV1( boltChannel, ( ch, s ) => boltConnection, ( v, ch ) => machine, logging );
			  boltProtocol.Install();

			  // When data arrives split up according to the current permutation
			  foreach ( ByteBuf fragment in fragments )
			  {
					_channel.writeInbound( fragment.readerIndex( 0 ).retain() );
			  }

			  // Then the session should've received the specified messages, and the protocol should be in a nice clean state
			  try
			  {
					RequestMessage run = new RunMessage( "Mjölnir", EMPTY_MAP );
					verify( machine ).process( eq( run ), any( typeof( BoltResponseHandler ) ) );
			  }
			  catch ( AssertionError e )
			  {
					throw new AssertionError( "Failed to handle fragmented delivery.\n" + "Messages: " + Arrays.ToString( _messages ) + "\n" + "Chunk size: " + _chunkSize + "\n" + "Serialized data delivered in fragments: " + DescribeFragments( fragments ) + "\n" + "Unfragmented data: " + HexPrinter.hex( unfragmented ) + "\n", e );
			  }
		 }

		 private string DescribeFragments( ByteBuf[] fragments )
		 {
			  StringBuilder sb = new StringBuilder();
			  for ( int i = 0; i < fragments.Length; i++ )
			  {
					if ( i > 0 )
					{
						 sb.Append( "," );
					}
					sb.Append( fragments[i].capacity() );
			  }
			  return sb.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] serialize(int chunkSize, Neo4Net.bolt.messaging.RequestMessage... msgs) throws java.io.IOException
		 private sbyte[] Serialize( int chunkSize, params RequestMessage[] msgs )
		 {
			  sbyte[][] serialized = new sbyte[msgs.Length][];
			  for ( int i = 0; i < msgs.Length; i++ )
			  {
					RecordingByteChannel channel = new RecordingByteChannel();

					BoltRequestMessageWriter writer = new BoltRequestMessageWriter( ( new Neo4NetPackV1() ).newPacker(new BufferedChannelOutput(channel)) );
					writer.Write( msgs[i] ).flush();
					serialized[i] = channel.Bytes;
			  }
			  return Chunker.Chunk( chunkSize, serialized );
		 }

		 private static BoltChannel NewBoltChannel( EmbeddedChannel rawChannel )
		 {
			  BoltChannel boltChannel = mock( typeof( BoltChannel ) );
			  when( boltChannel.RawChannel() ).thenReturn(rawChannel);
			  return boltChannel;
		 }
	}

}