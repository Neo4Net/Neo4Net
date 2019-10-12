/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.v1.messaging
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	public class RecordingByteChannelTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteToThenReadFromChannel()
		 public virtual void ShouldBeAbleToWriteToThenReadFromChannel()
		 {
			  // Given
			  RecordingByteChannel channel = new RecordingByteChannel();

			  // When
			  sbyte[] data = new sbyte[]{ 1, 2, 3, 4, 5 };
			  channel.Write( ByteBuffer.wrap( data ) );
			  ByteBuffer buffer = ByteBuffer.allocate( 10 );
			  int bytesRead = channel.Read( buffer );

			  // Then
			  assertThat( bytesRead, equalTo( 5 ) );
			  assertThat( buffer.array(), equalTo(new sbyte[]{ 1, 2, 3, 4, 5, 0, 0, 0, 0, 0 }) );

		 }

	}

}