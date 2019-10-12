using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb.mockfs
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static ByteBuffer.allocate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class EphemeralFileSystemAbstractionTest
	{
		 private EphemeralFileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _fs = new EphemeralFileSystemAbstraction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TearDown()
		 {
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allowStoreThatExceedDefaultSize() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AllowStoreThatExceedDefaultSize()
		 {
			  File aFile = new File( "test" );
			  StoreChannel channel = _fs.open( aFile, OpenMode.READ_WRITE );

			  ByteBuffer buffer = allocate( Long.BYTES );
			  int mebiBytes = ( int ) ByteUnit.mebiBytes( 1 );
			  for ( int position = mebiBytes + 42; position < 10_000_000; position += mebiBytes )
			  {
					buffer.putLong( 1 );
					buffer.flip();
					channel.WriteAll( buffer, position );
					buffer.clear();
			  }
			  channel.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void growEphemeralFileBuffer()
		 internal virtual void GrowEphemeralFileBuffer()
		 {
			  EphemeralFileSystemAbstraction.DynamicByteBuffer byteBuffer = new EphemeralFileSystemAbstraction.DynamicByteBuffer();

			  sbyte[] testBytes = new sbyte[] { 1, 2, 3, 4 };
			  int length = testBytes.Length;
			  byteBuffer.Put( 0, testBytes, 0, length );
			  assertEquals( ( int ) ByteUnit.kibiBytes( 1 ), byteBuffer.Buf().capacity() );

			  byteBuffer.Put( ( int )( ByteUnit.kibiBytes( 1 ) + 2 ), testBytes, 0, length );
			  assertEquals( ( int ) ByteUnit.kibiBytes( 2 ), byteBuffer.Buf().capacity() );

			  byteBuffer.Put( ( int )( ByteUnit.kibiBytes( 5 ) + 2 ), testBytes, 0, length );
			  assertEquals( ( int ) ByteUnit.kibiBytes( 8 ), byteBuffer.Buf().capacity() );

			  byteBuffer.Put( ( int )( ByteUnit.mebiBytes( 2 ) + 2 ), testBytes, 0, length );
			  assertEquals( ( int ) ByteUnit.mebiBytes( 4 ), byteBuffer.Buf().capacity() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotLoseDataForcedBeforeFileSystemCrashes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotLoseDataForcedBeforeFileSystemCrashes()
		 {
			  using ( EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction() )
			  {
					// given
					int numberOfBytesForced = 8;

					File aFile = new File( "yo" );

					StoreChannel channel = fs.Open( aFile, OpenMode.READ_WRITE );
					WriteLong( channel, 1111 );

					// when
					channel.Force( true );
					WriteLong( channel, 2222 );
					fs.Crash();

					// then
					StoreChannel readChannel = fs.Open( aFile, OpenMode.READ );
					assertEquals( numberOfBytesForced, readChannel.size() );

					assertEquals( 1111, ReadLong( readChannel ).Long );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeConsistentAfterConcurrentWritesAndCrashes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeConsistentAfterConcurrentWritesAndCrashes()
		 {
			  ExecutorService executorService = Executors.newCachedThreadPool();
			  try
			  {
					  using ( EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction() )
					  {
						File aFile = new File( "contendedFile" );
						for ( int attempt = 0; attempt < 100; attempt++ )
						{
							 ICollection<Callable<Void>> workers = new List<Callable<Void>>();
							 for ( int i = 0; i < 100; i++ )
							 {
								  workers.Add(() =>
								  {
									try
									{
										 StoreChannel channel = fs.Open( aFile, OpenMode.READ_WRITE );
										 channel.position( 0 );
										 WriteLong( channel, 1 );
									}
									catch ( IOException e )
									{
										 throw new Exception( e );
									}
									return null;
								  });
      
								  workers.Add(() =>
								  {
									fs.Crash();
									return null;
								  });
							 }
      
							 IList<Future<Void>> futures = executorService.invokeAll( workers );
							 foreach ( Future<Void> future in futures )
							 {
								  future.get();
							 }
							 VerifyFileIsEitherEmptyOrContainsLongIntegerValueOne( fs.Open( aFile, OpenMode.READ_WRITE ) );
						}
					  }
			  }
			  finally
			  {
					executorService.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeConsistentAfterConcurrentWritesAndForces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeConsistentAfterConcurrentWritesAndForces()
		 {
			  ExecutorService executorService = Executors.newCachedThreadPool();

			  try
			  {
					for ( int attempt = 0; attempt < 100; attempt++ )
					{
						 using ( EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction() )
						 {
							  File aFile = new File( "contendedFile" );

							  ICollection<Callable<Void>> workers = new List<Callable<Void>>();
							  for ( int i = 0; i < 100; i++ )
							  {
									workers.Add(() =>
									{
									 try
									 {
										  StoreChannel channel = fs.Open( aFile, OpenMode.READ_WRITE );
										  channel.position( channel.size() );
										  WriteLong( channel, 1 );
									 }
									 catch ( IOException e )
									 {
										  throw new Exception( e );
									 }
									 return null;
									});

									workers.Add(() =>
									{
									 StoreChannel channel = fs.Open( aFile, OpenMode.READ_WRITE );
									 channel.force( true );
									 return null;
									});
							  }

							  IList<Future<Void>> futures = executorService.invokeAll( workers );
							  foreach ( Future<Void> future in futures )
							  {
									future.get();
							  }

							  fs.Crash();
							  VerifyFileIsFullOfLongIntegerOnes( fs.Open( aFile, OpenMode.READ_WRITE ) );
						 }
					}
			  }
			  finally
			  {
					executorService.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void releaseResourcesOnClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReleaseResourcesOnClose()
		 {
			  using ( EphemeralFileSystemAbstraction fileSystemAbstraction = new EphemeralFileSystemAbstraction() )
			  {
					File testDir = new File( "testDir" );
					File testFile = new File( "testFile" );
					fileSystemAbstraction.Mkdir( testDir );
					fileSystemAbstraction.Create( testFile );

					assertTrue( fileSystemAbstraction.FileExists( testFile ) );
					assertTrue( fileSystemAbstraction.FileExists( testFile ) );

					fileSystemAbstraction.Dispose();

					assertTrue( fileSystemAbstraction.Closed );
					assertFalse( fileSystemAbstraction.FileExists( testFile ) );
					assertFalse( fileSystemAbstraction.FileExists( testFile ) );
			  }
		 }

		 private static void VerifyFileIsFullOfLongIntegerOnes( StoreChannel channel )
		 {
			  try
			  {
					long claimedSize = channel.size();
					ByteBuffer buffer = allocate( ( int ) claimedSize );
					channel.ReadAll( buffer );
					buffer.flip();

					for ( int position = 0; position < claimedSize; position += 8 )
					{
						 long value = buffer.getLong( position );
						 assertEquals( 1, value );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static void VerifyFileIsEitherEmptyOrContainsLongIntegerValueOne( StoreChannel channel )
		 {
			  try
			  {
					long claimedSize = channel.size();
					ByteBuffer buffer = allocate( 8 );
					channel.Read( buffer, 0 );
					buffer.flip();

					if ( claimedSize == 8 )
					{
						 assertEquals( 1, buffer.Long );
					}
					else
					{
						 assertThrows( typeof( BufferUnderflowException ), buffer.getLong, "Should have thrown an exception" );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static ByteBuffer readLong(org.neo4j.io.fs.StoreChannel readChannel) throws java.io.IOException
		 private static ByteBuffer ReadLong( StoreChannel readChannel )
		 {
			  ByteBuffer readBuffer = allocate( 8 );
			  readChannel.ReadAll( readBuffer );
			  readBuffer.flip();
			  return readBuffer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeLong(org.neo4j.io.fs.StoreChannel channel, long value) throws java.io.IOException
		 private static void WriteLong( StoreChannel channel, long value )
		 {
			  ByteBuffer buffer = allocate( 8 );
			  buffer.putLong( value );
			  buffer.flip();
			  channel.write( buffer );
		 }
	}

}