using System;

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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using Predicates = Neo4Net.Functions.Predicates;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.responseMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.serialize;

	public class TransportTestUtil
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly Neo4NetPack Neo4NetPackConflict;
		 private readonly MessageEncoder _messageEncoder;

		 public TransportTestUtil( Neo4NetPack Neo4NetPack ) : this( Neo4NetPack, new MessageEncoderV1() )
		 {
		 }

		 public TransportTestUtil( Neo4NetPack Neo4NetPack, MessageEncoder messageEncoder )
		 {
			  this.Neo4NetPackConflict = Neo4NetPack;
			  this._messageEncoder = messageEncoder;
		 }

		 public virtual Neo4NetPack Neo4NetPack
		 {
			 get
			 {
				  return Neo4NetPackConflict;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] chunk(org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException
		 public virtual sbyte[] Chunk( params RequestMessage[] messages )
		 {
			  return Chunk( 32, messages );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] chunk(org.Neo4Net.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
		 public virtual sbyte[] Chunk( params ResponseMessage[] messages )
		 {
			  return Chunk( 32, messages );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] chunk(int chunkSize, org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException
		 public virtual sbyte[] Chunk( int chunkSize, params RequestMessage[] messages )
		 {
			  sbyte[][] serializedMessages = new sbyte[messages.Length][];
			  for ( int i = 0; i < messages.Length; i++ )
			  {
					serializedMessages[i] = _messageEncoder.encode( Neo4NetPackConflict, messages[i] );
			  }
			  return chunk( chunkSize, serializedMessages );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] chunk(int chunkSize, org.Neo4Net.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
		 public virtual sbyte[] Chunk( int chunkSize, params ResponseMessage[] messages )
		 {
			  sbyte[][] serializedMessages = new sbyte[messages.Length][];
			  for ( int i = 0; i < messages.Length; i++ )
			  {
					serializedMessages[i] = serialize( Neo4NetPackConflict, messages[i] );
			  }
			  return chunk( chunkSize, serializedMessages );
		 }

		 public virtual sbyte[] Chunk( int chunkSize, params sbyte[][] messages )
		 {
			  ByteBuffer output = ByteBuffer.allocate( 10000 ).order( BIG_ENDIAN );

			  foreach ( sbyte[] wholeMessage in messages )
			  {
					int left = wholeMessage.Length;
					while ( left > 0 )
					{
						 int size = Math.Min( left, chunkSize );
						 output.putShort( ( short ) size );

						 int offset = wholeMessage.Length - left;
						 output.put( wholeMessage, offset, size );

						 left -= size;
					}
					output.putShort( ( short ) 0 );
			  }

			  output.flip();

			  sbyte[] arrayOutput = new sbyte[output.limit()];
			  output.get( arrayOutput );
			  return arrayOutput;
		 }

		 public virtual sbyte[] DefaultAcceptedVersions()
		 {
			  return AcceptedVersions( Neo4NetPackConflict.version(), 0, 0, 0 );
		 }

		 public virtual sbyte[] AcceptedVersions( long option1, long option2, long option3, long option4 )
		 {
			  ByteBuffer bb = ByteBuffer.allocate( 5 * Integer.BYTES ).order( BIG_ENDIAN );
			  bb.putInt( 0x6060B017 );
			  bb.putInt( ( int ) option1 );
			  bb.putInt( ( int ) option2 );
			  bb.putInt( ( int ) option3 );
			  bb.putInt( ( int ) option4 );
			  return bb.array();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final org.hamcrest.Matcher<org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection> eventuallyReceives(final org.hamcrest.Matcher<org.Neo4Net.bolt.messaging.ResponseMessage>... messages)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public Matcher<TransportConnection> EventuallyReceives( params Matcher<ResponseMessage>[] messages )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( this, messages );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<TransportConnection>
		 {
			 private readonly TransportTestUtil _outerInstance;

			 private Matcher<ResponseMessage>[] _messages;

			 public TypeSafeMatcherAnonymousInnerClass( TransportTestUtil outerInstance, Matcher<ResponseMessage>[] messages )
			 {
				 this.outerInstance = outerInstance;
				 this._messages = messages;
			 }

			 protected internal override bool matchesSafely( TransportConnection conn )
			 {
				  try
				  {
						foreach ( Matcher<ResponseMessage> matchesMessage in _messages )
						{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.bolt.messaging.ResponseMessage message = receiveOneResponseMessage(conn);
							 ResponseMessage message = outerInstance.ReceiveOneResponseMessage( conn );
							 assertThat( message, matchesMessage );
						}
						return true;
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValueList( "Messages[", ",", "]", _messages );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.messaging.ResponseMessage receiveOneResponseMessage(org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection conn) throws java.io.IOException, InterruptedException
		 public virtual ResponseMessage ReceiveOneResponseMessage( TransportConnection conn )
		 {
			  MemoryStream bytes = new MemoryStream();
			  while ( true )
			  {
					int size = ReceiveChunkHeader( conn );

					if ( size > 0 )
					{
						 sbyte[] received = conn.Recv( size );
						 bytes.Write( received, 0, received.Length );
					}
					else
					{
						 return responseMessage( Neo4NetPackConflict, bytes.toByteArray() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int receiveChunkHeader(org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection conn) throws java.io.IOException, InterruptedException
		 public virtual int ReceiveChunkHeader( TransportConnection conn )
		 {
			  sbyte[] raw = conn.Recv( 2 );
			  return ( ( raw[0] & 0xff ) << 8 | ( raw[1] & 0xff ) ) & 0xffff;
		 }

		 public virtual Matcher<TransportConnection> EventuallyReceivesSelectedProtocolVersion()
		 {
			  return EventuallyReceives( new sbyte[]{ 0, 0, 0, ( sbyte ) Neo4NetPackConflict.version() } );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection> eventuallyReceives(final byte[] expected)
		 public static Matcher<TransportConnection> EventuallyReceives( sbyte[] expected )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( expected );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<TransportConnection>
		 {
			 private sbyte[] _expected;

			 public TypeSafeMatcherAnonymousInnerClass2( sbyte[] expected )
			 {
				 this._expected = expected;
			 }

			 internal sbyte[] received;

			 protected internal override bool matchesSafely( TransportConnection item )
			 {
				  try
				  {
						received = item.Recv( _expected.Length );
						return Arrays.Equals( received, _expected );
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "to receive " );
				  appendBytes( description, _expected );
			 }

			 protected internal override void describeMismatchSafely( TransportConnection item, Description mismatchDescription )
			 {
				  mismatchDescription.appendText( "received " );
				  appendBytes( mismatchDescription, received );
			 }

			 internal void appendBytes( Description description, sbyte[] bytes )
			 {
				  description.appendValueList( "RawBytes[", ",", "]", bytes );
			 }
		 }

		 public static Matcher<TransportConnection> EventuallyDisconnects()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass3();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass3 : TypeSafeMatcher<TransportConnection>
		 {
			 protected internal override bool matchesSafely( TransportConnection connection )
			 {
				  System.Func<bool> condition = () =>
				  {
					try
					{
						 connection.Send( new sbyte[]{ 0, 0 } );
						 connection.Recv( 1 );
					}
					catch ( Exception e )
					{
						 // take an IOException on send/receive as evidence of disconnection
						 return e is IOException;
					}
					return false;
				  };
				  try
				  {
						Predicates.await( condition, 2, TimeUnit.SECONDS );
						return true;
				  }
				  catch ( Exception )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Eventually Disconnects" );
			 }
		 }

		 public static Matcher<TransportConnection> ServerImmediatelyDisconnects()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass4();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass4 : TypeSafeMatcher<TransportConnection>
		 {
			 protected internal override bool matchesSafely( TransportConnection connection )
			 {
				  try
				  {
						connection.Recv( 1 );
				  }
				  catch ( Exception e )
				  {
						// take an IOException on send/receive as evidence of disconnection
						return e is IOException;
				  }
				  return false;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Eventually Disconnects" );
			 }
		 }

		 public interface MessageEncoder
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException;
			  sbyte[] Encode( Neo4NetPack Neo4NetPack, params RequestMessage[] messages );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.ResponseMessage... messages) throws java.io.IOException;
			  sbyte[] Encode( Neo4NetPack Neo4NetPack, params ResponseMessage[] messages );
		 }

		 private class MessageEncoderV1 : MessageEncoder
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException
			  public override sbyte[] Encode( Neo4NetPack Neo4NetPack, params RequestMessage[] messages )
			  {
					return serialize( Neo4NetPack, messages );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
			  public override sbyte[] Encode( Neo4NetPack Neo4NetPack, params ResponseMessage[] messages )
			  {
					return serialize( Neo4NetPack, messages );
			  }
		 }
	}

}