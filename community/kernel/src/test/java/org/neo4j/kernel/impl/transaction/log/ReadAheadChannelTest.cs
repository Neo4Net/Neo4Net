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
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using EnumSource = org.junit.jupiter.@params.provider.EnumSource;


	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using ReadPastEndException = Org.Neo4j.Storageengine.Api.ReadPastEndException;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.ReadAheadChannel.DEFAULT_READ_AHEAD_SIZE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(EphemeralFileSystemExtension.class) class ReadAheadChannelTest
	internal class ReadAheadChannelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fileSystem;
		 protected internal EphemeralFileSystemAbstraction FileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(BufferFactories.class) void shouldThrowExceptionForReadAfterEOFIfNotEnoughBytesExist(System.Func<int, ByteBuffer> bufferFactory) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowExceptionForReadAfterEOFIfNotEnoughBytesExist( System.Func<int, ByteBuffer> bufferFactory )
		 {
			  // Given
			  StoreChannel storeChannel = FileSystem.open( new File( "foo.txt" ), OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( 1 );
			  buffer.put( ( sbyte ) 1 );
			  buffer.flip();
			  storeChannel.WriteAll( buffer );
			  storeChannel.Force( false );
			  storeChannel.close();

			  storeChannel = FileSystem.open( new File( "foo.txt" ), OpenMode.READ );

			  ReadAheadChannel<StoreChannel> channel = new ReadAheadChannel<StoreChannel>( storeChannel, bufferFactory( DEFAULT_READ_AHEAD_SIZE ) );
			  assertEquals( ( sbyte ) 1, channel.Get() );

			  try
			  {
					channel.Get();
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }

			  try
			  {
					channel.Get();
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(BufferFactories.class) void shouldReturnValueIfSufficientBytesAreBufferedEvenIfEOFHasBeenEncountered(System.Func<int, ByteBuffer> bufferFactory) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnValueIfSufficientBytesAreBufferedEvenIfEOFHasBeenEncountered( System.Func<int, ByteBuffer> bufferFactory )
		 {
			  // Given
			  StoreChannel storeChannel = FileSystem.open( new File( "foo.txt" ), OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( 1 );
			  buffer.put( ( sbyte ) 1 );
			  buffer.flip();
			  storeChannel.WriteAll( buffer );
			  storeChannel.Force( false );
			  storeChannel.close();

			  storeChannel = FileSystem.open( new File( "foo.txt" ), OpenMode.READ );
			  ReadAheadChannel<StoreChannel> channel = new ReadAheadChannel<StoreChannel>( storeChannel, bufferFactory( DEFAULT_READ_AHEAD_SIZE ) );

			  try
			  {
					channel.Short;
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }

			  assertEquals( ( sbyte ) 1, channel.Get() );

			  try
			  {
					channel.Get();
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(BufferFactories.class) void shouldHandleRunningOutOfBytesWhenRequestSpansMultipleFiles(System.Func<int, ByteBuffer> bufferFactory) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleRunningOutOfBytesWhenRequestSpansMultipleFiles( System.Func<int, ByteBuffer> bufferFactory )
		 {
			  // Given
			  StoreChannel storeChannel1 = FileSystem.open( new File( "foo.1" ), OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( 2 );
			  buffer.put( ( sbyte ) 0 );
			  buffer.put( ( sbyte ) 0 );
			  buffer.flip();
			  storeChannel1.WriteAll( buffer );
			  storeChannel1.Force( false );
			  storeChannel1.close();

			  buffer.flip();

			  StoreChannel storeChannel2 = FileSystem.open( new File( "foo.2" ), OpenMode.READ );
			  buffer.put( ( sbyte ) 0 );
			  buffer.put( ( sbyte ) 1 );
			  buffer.flip();
			  storeChannel2.WriteAll( buffer );
			  storeChannel2.Force( false );
			  storeChannel2.close();

			  storeChannel1 = FileSystem.open( new File( "foo.1" ), OpenMode.READ );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.StoreChannel storeChannel2Copy = fileSystem.open(new java.io.File("foo.2"), org.neo4j.io.fs.OpenMode.READ);
			  StoreChannel storeChannel2Copy = FileSystem.open( new File( "foo.2" ), OpenMode.READ );

			  ReadAheadChannel<StoreChannel> channel = new ReadAheadChannelAnonymousInnerClass( this, storeChannel1, bufferFactory( DEFAULT_READ_AHEAD_SIZE ), storeChannel2Copy );

			  try
			  {
					channel.Long;
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }

			  assertEquals( 1, channel.Int );

			  try
			  {
					channel.Get();
					fail( "Should have thrown exception signalling end of file reached" );
			  }
			  catch ( ReadPastEndException )
			  {
					// outstanding
			  }
		 }

		 private class ReadAheadChannelAnonymousInnerClass : ReadAheadChannel<StoreChannel>
		 {
			 private readonly ReadAheadChannelTest _outerInstance;

			 private StoreChannel _storeChannel2Copy;

			 public ReadAheadChannelAnonymousInnerClass( ReadAheadChannelTest outerInstance, StoreChannel storeChannel1, UnknownType apply, StoreChannel storeChannel2Copy ) : base( storeChannel1, apply )
			 {
				 this.outerInstance = outerInstance;
				 this._storeChannel2Copy = storeChannel2Copy;
			 }

			 protected internal override StoreChannel next( StoreChannel channel )
			 {
				  return _storeChannel2Copy;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(BufferFactories.class) void shouldReturnPositionWithinBufferedStream(System.Func<int, ByteBuffer> bufferFactory) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnPositionWithinBufferedStream( System.Func<int, ByteBuffer> bufferFactory )
		 {
			  // given
			  File file = new File( "foo.txt" );

			  int readAheadSize = 512;
			  int fileSize = readAheadSize * 8;

			  CreateFile( FileSystem, file, fileSize );
			  ReadAheadChannel<StoreChannel> bufferedReader = new ReadAheadChannel<StoreChannel>( FileSystem.open( file, OpenMode.READ ), bufferFactory( readAheadSize ) );

			  // when
			  for ( int i = 0; i < fileSize / Long.BYTES; i++ )
			  {
					assertEquals( Long.BYTES * i, bufferedReader.Position() );
					bufferedReader.Long;
			  }

			  assertEquals( fileSize, bufferedReader.Position() );

			  try
			  {
					bufferedReader.Long;
					fail();
			  }
			  catch ( ReadPastEndException )
			  {
					// expected
			  }

			  assertEquals( fileSize, bufferedReader.Position() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFile(org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fsa, java.io.File name, int bufferSize) throws java.io.IOException
		 private void CreateFile( EphemeralFileSystemAbstraction fsa, File name, int bufferSize )
		 {
			  StoreChannel storeChannel = fsa.Open( name, OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( bufferSize );
			  for ( int i = 0; i < bufferSize; i++ )
			  {
					buffer.put( ( sbyte ) i );
			  }
			  buffer.flip();
			  storeChannel.WriteAll( buffer );
			  storeChannel.close();
		 }

		 internal enum BufferFactories
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: HEAP { @Override public ByteBuffer apply(int value) { return ByteBuffer.allocate(value); } },
			  HEAP
			  {
				  public ByteBuffer apply( int value ) { return ByteBuffer.allocate( value ); }
			  },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: DIRECT { @Override public ByteBuffer apply(int value) { return ByteBuffer.allocateDirect(value); } }
			  DIRECT
			  {
				  public ByteBuffer apply( int value ) { return ByteBuffer.allocateDirect( value ); }
			  }
		 }
	}

}