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
namespace Org.Neo4j.Csv.Reader
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class ThreadAheadReadableTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadAhead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReadAhead()
		 {
			  // GIVEN
			  TrackingReader actual = new TrackingReader( 23 );
			  int bufferSize = 5;
			  CharReadable aheadReader = ThreadAheadReadable.ThreadAhead( actual, bufferSize );
			  SectionedCharBuffer buffer = new SectionedCharBuffer( bufferSize );

			  // WHEN starting it up it should read and fill the buffer to the brim
			  assertEquals( bufferSize, actual.AwaitCompletedReadAttempts( 1 ) );

			  // WHEN we read one buffer
			  int read = 0;
			  buffer = aheadReader.Read( buffer, buffer.Front() );
			  AssertBuffer( Chars( read, bufferSize ), buffer, 0, bufferSize );
			  read += buffer.Available();

			  // and simulate reading all characters, i.e. back section will be empty in the new buffer
			  buffer = aheadReader.Read( buffer, buffer.Front() );
			  AssertBuffer( Chars( read, bufferSize ), buffer, 0, bufferSize );
			  read += buffer.Available();

			  // then simulate reading some characters, i.e. back section will contain some characters
			  int keep = 2;
			  buffer = aheadReader.Read( buffer, buffer.Front() - keep );
			  AssertBuffer( Chars( read - keep, bufferSize + keep ), buffer, keep, bufferSize );
			  read += buffer.Available();

			  keep = 3;
			  buffer = aheadReader.Read( buffer, buffer.Front() - keep );
			  AssertBuffer( Chars( read - keep, bufferSize + keep ), buffer, keep, bufferSize );
			  read += buffer.Available();

			  keep = 1;
			  buffer = aheadReader.Read( buffer, buffer.Front() - keep );
			  assertEquals( 3, buffer.Available() );
			  AssertBuffer( Chars( read - keep, buffer.Available() + keep ), buffer, keep, 3 );
			  read += buffer.Available();
			  assertEquals( 23, read );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleReadAheadEmptyData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleReadAheadEmptyData()
		 {
			  // GIVEN
			  TrackingReader actual = new TrackingReader( 0 );
			  int bufferSize = 10;
			  CharReadable aheadReadable = ThreadAheadReadable.ThreadAhead( actual, bufferSize );

			  // WHEN
			  actual.AwaitCompletedReadAttempts( 1 );

			  // THEN
			  SectionedCharBuffer buffer = new SectionedCharBuffer( bufferSize );
			  buffer = aheadReadable.Read( buffer, buffer.Front() );
			  assertEquals( buffer.Pivot(), buffer.Back() );
			  assertEquals( buffer.Pivot(), buffer.Front() );
		 }

		 private static void AssertBuffer( char[] expectedChars, SectionedCharBuffer buffer, int charsInBack, int charsInFront )
		 {
			  assertEquals( buffer.Pivot() - charsInBack, buffer.Back() );
			  assertEquals( buffer.Pivot() + charsInFront, buffer.Front() );
			  assertArrayEquals( expectedChars, copyOfRange( buffer.Array(), buffer.Back(), buffer.Front() ) );
		 }

		 private class TrackingReader : CharReadable_Adapter
		 {
			  internal int BytesRead;
			  internal volatile int ReadsCompleted;
			  internal readonly CharReadable Actual;
			  internal readonly long Bytes;

			  internal TrackingReader( int length )
			  {
					this.Bytes = length * 2;
					this.Actual = Readables.Wrap( new CharArrayReader( Chars( 0, length ) ), length * 2 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException
			  public override SectionedCharBuffer Read( SectionedCharBuffer buffer, int from )
			  {
					try
					{
						 return RegisterBytesRead( Actual.read( buffer, from ) );
					}
					finally
					{
						 ReadsCompleted++;
					}
			  }

			  public override int Read( char[] into, int offset, int length )
			  {
					throw new System.NotSupportedException();
			  }

			  internal virtual SectionedCharBuffer RegisterBytesRead( SectionedCharBuffer buffer )
			  {
					BytesRead += buffer.Available();
					return buffer;
			  }

			  public override void Close()
			  { // Nothing to close
			  }

			  internal virtual int AwaitCompletedReadAttempts( int ticket )
			  {
					while ( ReadsCompleted < ticket )
					{
						 LockSupport.parkNanos( 10_000_000 );
					}
					return BytesRead;
			  }

			  public override long Position()
			  {
					return Actual.position();
			  }

			  public override string SourceDescription()
			  {
					return this.GetType().Name;
			  }

			  public override long Length()
			  {
					return Bytes;
			  }
		 }

		 private static char[] Chars( int start, int length )
		 {
			  char[] result = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					result[i] = ( char )( start + i );
			  }
			  return result;
		 }
	}

}