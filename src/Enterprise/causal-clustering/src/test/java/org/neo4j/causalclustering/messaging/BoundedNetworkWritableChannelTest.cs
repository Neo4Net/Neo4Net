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
namespace Neo4Net.causalclustering.messaging
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class BoundedNetworkWritableChannelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectSizeLimit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectSizeLimit()
		 {
			  // Given
			  int sizeLimit = 100;
			  BoundedNetworkWritableChannel channel = new BoundedNetworkWritableChannel( Unpooled.buffer(), sizeLimit );

			  // when
			  for ( int i = 0; i < sizeLimit; i++ )
			  {
					channel.Put( ( sbyte ) 1 );
			  }

			  try
			  {
					channel.Put( ( sbyte ) 1 );
					fail( "Should not allow more bytes than what the limit dictates" );
			  }
			  catch ( MessageTooBigException )
			  {
					// then
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sizeLimitShouldWorkWithArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SizeLimitShouldWorkWithArrays()
		 {
			  // Given
			  int sizeLimit = 100;
			  BoundedNetworkWritableChannel channel = new BoundedNetworkWritableChannel( Unpooled.buffer(), sizeLimit );

			  // When
			  int padding = 10;
			  for ( int i = 0; i < sizeLimit - padding; i++ )
			  {
					channel.Put( ( sbyte ) 0 );
			  }

			  try
			  {
					channel.Put( new sbyte[padding * 2], padding * 2 );
					fail( "Should not allow more bytes than what the limit dictates" );
			  }
			  catch ( MessageTooBigException )
			  {
					// then
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountBytesAlreadyInBuffer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountBytesAlreadyInBuffer()
		 {
			  // Given
			  int sizeLimit = 100;
			  ByteBuf buffer = Unpooled.buffer();

			  int padding = Long.BYTES;
			  buffer.writeLong( 0 );

			  BoundedNetworkWritableChannel channel = new BoundedNetworkWritableChannel( buffer, sizeLimit );

			  // When
			  for ( int i = 0; i < sizeLimit - padding; i++ )
			  {
					channel.Put( ( sbyte ) 0 );
			  }
			  // then it should be ok
			  // again, when
			  for ( int i = 0; i < padding; i++ )
			  {
					channel.Put( ( sbyte ) 0 );
			  }
			  // then again, it should work
			  // finally, when we pass the limit
			  try
			  {
					channel.Put( ( sbyte ) 0 );
					fail( "Should not allow more bytes than what the limit dictates" );
			  }
			  catch ( MessageTooBigException )
			  {
					// then
			  }
		 }
	}

}