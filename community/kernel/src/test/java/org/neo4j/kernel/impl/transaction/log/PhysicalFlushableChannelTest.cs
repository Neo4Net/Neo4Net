using System;
using System.IO;

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
namespace Org.Neo4j.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class PhysicalFlushableChannelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteSmallNumberOfBytes() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteSmallNumberOfBytes()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File firstFile = new java.io.File(directory.directory(), "file1");
			  File firstFile = new File( Directory.directory(), "file1" );
			  StoreChannel storeChannel = FileSystemRule.get().open(firstFile, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  int length = 26_145;
			  sbyte[] bytes = GenerateBytes( length );

			  channel.Put( bytes, length );
			  channel.Dispose();

			  sbyte[] writtenBytes = new sbyte[length];
			  using ( Stream @in = new FileStream( firstFile, FileMode.Open, FileAccess.Read ) )
			  {
					@in.Read( writtenBytes, 0, writtenBytes.Length );
			  }

			  assertArrayEquals( bytes, writtenBytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteValuesGreaterThanHalfTheBufferSize() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteValuesGreaterThanHalfTheBufferSize()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File firstFile = new java.io.File(directory.directory(), "file1");
			  File firstFile = new File( Directory.directory(), "file1" );
			  StoreChannel storeChannel = FileSystemRule.get().open(firstFile, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  int length = 262_145;
			  sbyte[] bytes = GenerateBytes( length );

			  channel.Put( bytes, length );
			  channel.Dispose();

			  sbyte[] writtenBytes = new sbyte[length];
			  using ( Stream @in = new FileStream( firstFile, FileMode.Open, FileAccess.Read ) )
			  {
					@in.Read( writtenBytes, 0, writtenBytes.Length );
			  }

			  assertArrayEquals( bytes, writtenBytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteValuesGreaterThanTheBufferSize() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteValuesGreaterThanTheBufferSize()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File firstFile = new java.io.File(directory.directory(), "file1");
			  File firstFile = new File( Directory.directory(), "file1" );
			  StoreChannel storeChannel = FileSystemRule.get().open(firstFile, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  int length = 1_000_000;
			  sbyte[] bytes = GenerateBytes( length );

			  channel.Put( bytes, length );
			  channel.Dispose();

			  sbyte[] writtenBytes = new sbyte[length];
			  using ( Stream @in = new FileStream( firstFile, FileMode.Open, FileAccess.Read ) )
			  {
					@in.Read( writtenBytes, 0, writtenBytes.Length );
			  }

			  assertArrayEquals( bytes, writtenBytes );
		 }

		 private sbyte[] GenerateBytes( int length )
		 {
			  Random random = new Random();
			  char[] validCharacters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o' };
			  sbyte[] bytes = new sbyte[length];
			  for ( int i = 0; i < length; i++ )
			  {
					bytes[i] = ( sbyte ) validCharacters[random.Next( validCharacters.Length )];
			  }
			  return bytes;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteThroughRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteThroughRotation()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File firstFile = new java.io.File(directory.directory(), "file1");
			  File firstFile = new File( Directory.directory(), "file1" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File secondFile = new java.io.File(directory.directory(), "file2");
			  File secondFile = new File( Directory.directory(), "file2" );
			  StoreChannel storeChannel = FileSystemRule.get().open(firstFile, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  // WHEN writing a transaction, of sorts
			  sbyte byteValue = ( sbyte ) 4;
			  short shortValue = ( short ) 10;
			  int intValue = 3545;
			  long longValue = 45849589L;
			  float floatValue = 45849.332f;
			  double doubleValue = 458493343D;
			  sbyte[] byteArrayValue = new sbyte[] { 1, 4, 2, 5, 3, 6 };

			  channel.Put( byteValue );
			  channel.PutShort( shortValue );
			  channel.PutInt( intValue );
			  channel.PutLong( longValue );
			  channel.PrepareForFlush().flush();
			  channel.Dispose();

			  // "Rotate" and continue
			  storeChannel = FileSystemRule.get().open(secondFile, OpenMode.READ_WRITE);
			  channel.Channel = new PhysicalLogVersionedStoreChannel( storeChannel, 2, ( sbyte ) - 1 );
			  channel.PutFloat( floatValue );
			  channel.PutDouble( doubleValue );
			  channel.Put( byteArrayValue, byteArrayValue.Length );
			  channel.Dispose();

			  // The two chunks of values should end up in two different files
			  ByteBuffer firstFileContents = ReadFile( firstFile );
			  assertEquals( byteValue, firstFileContents.get() );
			  assertEquals( shortValue, firstFileContents.Short );
			  assertEquals( intValue, firstFileContents.Int );
			  assertEquals( longValue, firstFileContents.Long );
			  ByteBuffer secondFileContents = ReadFile( secondFile );
			  assertEquals( floatValue, secondFileContents.Float, 0.0f );
			  assertEquals( doubleValue, secondFileContents.Double, 0.0d );

			  sbyte[] readByteArray = new sbyte[byteArrayValue.Length];
			  secondFileContents.get( readByteArray );
			  assertArrayEquals( byteArrayValue, readByteArray );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeCorrectPositionEvenBeforeEmptyingDataIntoChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeCorrectPositionEvenBeforeEmptyingDataIntoChannel()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(directory.directory(), "file");
			  File file = new File( Directory.directory(), "file" );
			  StoreChannel storeChannel = FileSystemRule.get().open(file, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PositionAwarePhysicalFlushableChannel channel = new PositionAwarePhysicalFlushableChannel( versionedStoreChannel );
			  LogPositionMarker positionMarker = new LogPositionMarker();
			  LogPosition initialPosition = channel.GetCurrentPosition( positionMarker ).newPosition();

			  // WHEN
			  channel.PutLong( 67 );
			  channel.PutInt( 1234 );
			  LogPosition positionAfterSomeData = channel.GetCurrentPosition( positionMarker ).newPosition();

			  // THEN
			  assertEquals( 12, positionAfterSomeData.ByteOffset - initialPosition.ByteOffset );
			  channel.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateExceptionAfterClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIllegalStateExceptionAfterClosed()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(directory.directory(), "file");
			  File file = new File( Directory.directory(), "file" );
			  StoreChannel storeChannel = FileSystemRule.get().open(file, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  // closing the WritableLogChannel, then the underlying channel is what PhysicalLogFile does
			  channel.Dispose();
			  storeChannel.close();

			  // WHEN just appending something to the buffer
			  channel.Put( ( sbyte ) 0 );
			  // and wanting to empty that into the channel
			  try
			  {
					channel.PrepareForFlush();
					fail( "Should have thrown exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN we should get an IllegalStateException, not a ClosedChannelException
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowClosedChannelExceptionWhenChannelUnexpectedlyClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowClosedChannelExceptionWhenChannelUnexpectedlyClosed()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(directory.directory(), "file");
			  File file = new File( Directory.directory(), "file" );
			  StoreChannel storeChannel = FileSystemRule.get().open(file, OpenMode.READ_WRITE);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, 1, ( sbyte ) - 1 );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( versionedStoreChannel );

			  // just close the underlying channel
			  storeChannel.close();

			  // WHEN just appending something to the buffer
			  channel.Put( ( sbyte ) 0 );
			  // and wanting to empty that into the channel
			  try
			  {
					channel.PrepareForFlush();
					fail( "Should have thrown exception" );
			  }
			  catch ( ClosedChannelException )
			  {
					// THEN we should get a ClosedChannelException
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer readFile(java.io.File file) throws java.io.IOException
		 private ByteBuffer ReadFile( File file )
		 {
			  using ( StoreChannel channel = FileSystemRule.get().open(file, OpenMode.READ) )
			  {
					ByteBuffer buffer = ByteBuffer.allocate( ( int ) channel.size() );
					channel.ReadAll( buffer );
					buffer.flip();
					return buffer;
			  }
		 }

	}

}