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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Buffers = Neo4Net.causalclustering.helpers.Buffers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class StringMarshalTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.causalclustering.helpers.Buffers buffers = new org.neo4j.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeString()
		 public virtual void ShouldSerializeAndDeserializeString()
		 {
			  // given
			  const string testString = "ABC123_?";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf buffer = buffers.buffer();
			  ByteBuf buffer = Buffers.buffer();

			  // when
			  StringMarshal.Marshal( buffer, testString );
			  string reconstructed = StringMarshal.Unmarshal( buffer );

			  // then
			  assertNotSame( testString, reconstructed );
			  assertEquals( testString, reconstructed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeEmptyString()
		 public virtual void ShouldSerializeAndDeserializeEmptyString()
		 {
			  // given
			  const string testString = "";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf buffer = buffers.buffer();
			  ByteBuf buffer = Buffers.buffer();

			  // when
			  StringMarshal.Marshal( buffer, testString );
			  string reconstructed = StringMarshal.Unmarshal( buffer );

			  // then
			  assertNotSame( testString, reconstructed );
			  assertEquals( testString, reconstructed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeNull()
		 public virtual void ShouldSerializeAndDeserializeNull()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf buffer = buffers.buffer();
			  ByteBuf buffer = Buffers.buffer();

			  // when
			  StringMarshal.Marshal( buffer, null );
			  string reconstructed = StringMarshal.Unmarshal( buffer );

			  // then
			  assertNull( reconstructed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeStringUsingChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeStringUsingChannel()
		 {
			  // given
			  const string testString = "ABC123_?";
			  MemoryStream outputStream = new MemoryStream();
			  OutputStreamWritableChannel writableChannel = new OutputStreamWritableChannel( outputStream );

			  // when
			  StringMarshal.Marshal( writableChannel, testString );

			  MemoryStream inputStream = new MemoryStream( outputStream.toByteArray() );
			  InputStreamReadableChannel readableChannel = new InputStreamReadableChannel( inputStream );
			  string reconstructed = StringMarshal.Unmarshal( readableChannel );

			  // then
			  assertNotSame( testString, reconstructed );
			  assertEquals( testString, reconstructed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeEmptyStringUsingChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeEmptyStringUsingChannel()
		 {
			  // given
			  const string testString = "";
			  MemoryStream outputStream = new MemoryStream();
			  OutputStreamWritableChannel writableChannel = new OutputStreamWritableChannel( outputStream );

			  // when
			  StringMarshal.Marshal( writableChannel, testString );

			  MemoryStream inputStream = new MemoryStream( outputStream.toByteArray() );
			  InputStreamReadableChannel readableChannel = new InputStreamReadableChannel( inputStream );
			  string reconstructed = StringMarshal.Unmarshal( readableChannel );

			  // then
			  assertNotSame( testString, reconstructed );
			  assertEquals( testString, reconstructed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeNullUsingChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeNullUsingChannel()
		 {
			  // given
			  MemoryStream outputStream = new MemoryStream();
			  OutputStreamWritableChannel writableChannel = new OutputStreamWritableChannel( outputStream );

			  // when
			  StringMarshal.Marshal( writableChannel, null );

			  MemoryStream inputStream = new MemoryStream( outputStream.toByteArray() );
			  InputStreamReadableChannel readableChannel = new InputStreamReadableChannel( inputStream );
			  string reconstructed = StringMarshal.Unmarshal( readableChannel );

			  // then
			  assertNull( reconstructed );
		 }
	}

}