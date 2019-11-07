using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Adversary = Neo4Net.Adversaries.Adversary;
	using RandomAdversary = Neo4Net.Adversaries.RandomAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.storecopy.FileChunk.MAX_SIZE;

	public class FileSenderTest
	{
		private bool InstanceFieldsInitialized = false;

		public FileSenderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fs = FsRule.get();
			TestDirectory = TestDirectory.testDirectory( FsRule.get() );
		}

		 private readonly Random _random = new Random();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 private FileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory(fsRule.get());
		 public TestDirectory TestDirectory;
		 private ByteBufAllocator _allocator = mock( typeof( ByteBufAllocator ) );
		 private PageCache _pageCache = mock( typeof( PageCache ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( _pageCache.getExistingMapping( any() ) ).thenReturn(null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendEmptyFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendEmptyFile()
		 {
			  // given
			  File emptyFile = TestDirectory.file( "emptyFile" );
			  _fs.create( emptyFile ).close();
			  FileSender fileSender = new FileSender( new StoreResource( emptyFile, null, 16, _fs ) );

			  // when + then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( new sbyte[0], true ), fileSender.ReadChunk( _allocator ) );
			  assertNull( fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendSmallFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendSmallFile()
		 {
			  // given
			  sbyte[] bytes = new sbyte[10];
			  _random.NextBytes( bytes );

			  File smallFile = TestDirectory.file( "smallFile" );
			  using ( StoreChannel storeChannel = _fs.create( smallFile ) )
			  {
					storeChannel.write( ByteBuffer.wrap( bytes ) );
			  }

			  FileSender fileSender = new FileSender( new StoreResource( smallFile, null, 16, _fs ) );

			  // when + then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( bytes, true ), fileSender.ReadChunk( _allocator ) );
			  assertNull( fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendLargeFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendLargeFile()
		 {
			  // given
			  int dataSize = MAX_SIZE + ( MAX_SIZE / 2 );
			  sbyte[] bytes = new sbyte[dataSize];
			  _random.NextBytes( bytes );

			  File smallFile = TestDirectory.file( "smallFile" );
			  using ( StoreChannel storeChannel = _fs.create( smallFile ) )
			  {
					storeChannel.write( ByteBuffer.wrap( bytes ) );
			  }

			  FileSender fileSender = new FileSender( new StoreResource( smallFile, null, 16, _fs ) );

			  // when + then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, 0, MAX_SIZE ), false ), fileSender.ReadChunk( _allocator ) );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, MAX_SIZE, bytes.Length ), true ), fileSender.ReadChunk( _allocator ) );
			  assertNull( fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendLargeFileWithSizeMultipleOfTheChunkSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendLargeFileWithSizeMultipleOfTheChunkSize()
		 {
			  // given
			  sbyte[] bytes = new sbyte[MAX_SIZE * 3];
			  _random.NextBytes( bytes );

			  File smallFile = TestDirectory.file( "smallFile" );
			  using ( StoreChannel storeChannel = _fs.create( smallFile ) )
			  {
					storeChannel.write( ByteBuffer.wrap( bytes ) );
			  }

			  FileSender fileSender = new FileSender( new StoreResource( smallFile, null, 16, _fs ) );

			  // when + then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, 0, MAX_SIZE ), false ), fileSender.ReadChunk( _allocator ) );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, MAX_SIZE, MAX_SIZE * 2 ), false ), fileSender.ReadChunk( _allocator ) );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, MAX_SIZE * 2, bytes.Length ), true ), fileSender.ReadChunk( _allocator ) );
			  assertNull( fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendEmptyFileWhichGrowsBeforeSendCommences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendEmptyFileWhichGrowsBeforeSendCommences()
		 {
			  // given
			  File file = TestDirectory.file( "file" );
			  StoreChannel writer = _fs.create( file );
			  FileSender fileSender = new FileSender( new StoreResource( file, null, 16, _fs ) );

			  // when
			  sbyte[] bytes = WriteRandomBytes( writer, 1024 );

			  // then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( bytes, true ), fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
			  assertNull( fileSender.ReadChunk( _allocator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendEmptyFileWhichGrowsWithPartialChunkSizes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendEmptyFileWhichGrowsWithPartialChunkSizes()
		 {
			  // given
			  File file = TestDirectory.file( "file" );
			  StoreChannel writer = _fs.create( file );
			  FileSender fileSender = new FileSender( new StoreResource( file, null, 16, _fs ) );

			  // when
			  sbyte[] chunkA = WriteRandomBytes( writer, MAX_SIZE );
			  sbyte[] chunkB = WriteRandomBytes( writer, MAX_SIZE / 2 );

			  // then
			  assertEquals( FileChunk.Create( chunkA, false ), fileSender.ReadChunk( _allocator ) );
			  assertFalse( fileSender.EndOfInput );

			  // when
			  WriteRandomBytes( writer, MAX_SIZE / 2 );

			  // then
			  assertEquals( FileChunk.Create( chunkB, true ), fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
			  assertNull( fileSender.ReadChunk( _allocator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendFileWhichGrowsAfterLastChunkWasSent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendFileWhichGrowsAfterLastChunkWasSent()
		 {
			  // given
			  File file = TestDirectory.file( "file" );
			  StoreChannel writer = _fs.create( file );
			  FileSender fileSender = new FileSender( new StoreResource( file, null, 16, _fs ) );

			  // when
			  sbyte[] chunkA = WriteRandomBytes( writer, MAX_SIZE );
			  FileChunk readChunkA = fileSender.ReadChunk( _allocator );

			  // then
			  assertEquals( FileChunk.Create( chunkA, true ), readChunkA );
			  assertTrue( fileSender.EndOfInput );

			  // when
			  WriteRandomBytes( writer, MAX_SIZE );

			  // then
			  assertTrue( fileSender.EndOfInput );
			  assertNull( fileSender.ReadChunk( _allocator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendLargerFileWhichGrows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendLargerFileWhichGrows()
		 {
			  // given
			  File file = TestDirectory.file( "file" );
			  StoreChannel writer = _fs.create( file );
			  FileSender fileSender = new FileSender( new StoreResource( file, null, 16, _fs ) );

			  // when
			  sbyte[] chunkA = WriteRandomBytes( writer, MAX_SIZE );
			  sbyte[] chunkB = WriteRandomBytes( writer, MAX_SIZE );
			  FileChunk readChunkA = fileSender.ReadChunk( _allocator );

			  // then
			  assertEquals( FileChunk.Create( chunkA, false ), readChunkA );
			  assertFalse( fileSender.EndOfInput );

			  // when
			  sbyte[] chunkC = WriteRandomBytes( writer, MAX_SIZE );
			  FileChunk readChunkB = fileSender.ReadChunk( _allocator );

			  // then
			  assertEquals( FileChunk.Create( chunkB, false ), readChunkB );
			  assertFalse( fileSender.EndOfInput );

			  // when
			  FileChunk readChunkC = fileSender.ReadChunk( _allocator );
			  assertEquals( FileChunk.Create( chunkC, true ), readChunkC );

			  // then
			  assertTrue( fileSender.EndOfInput );
			  assertNull( fileSender.ReadChunk( _allocator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendLargeFileWithUnreliableReadBufferSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendLargeFileWithUnreliableReadBufferSize()
		 {
			  // given
			  sbyte[] bytes = new sbyte[MAX_SIZE * 3];
			  _random.NextBytes( bytes );

			  File smallFile = TestDirectory.file( "smallFile" );
			  using ( StoreChannel storeChannel = _fs.create( smallFile ) )
			  {
					storeChannel.write( ByteBuffer.wrap( bytes ) );
			  }

			  Adversary adversary = new RandomAdversary( 0.9, 0.0, 0.0 );
			  AdversarialFileSystemAbstraction afs = new AdversarialFileSystemAbstraction( adversary, _fs );
			  FileSender fileSender = new FileSender( new StoreResource( smallFile, null, 16, afs ) );

			  // when + then
			  assertFalse( fileSender.EndOfInput );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, 0, MAX_SIZE ), false ), fileSender.ReadChunk( _allocator ) );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, MAX_SIZE, MAX_SIZE * 2 ), false ), fileSender.ReadChunk( _allocator ) );
			  assertEquals( FileChunk.Create( copyOfRange( bytes, MAX_SIZE * 2, bytes.Length ), true ), fileSender.ReadChunk( _allocator ) );
			  assertNull( fileSender.ReadChunk( _allocator ) );
			  assertTrue( fileSender.EndOfInput );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] writeRandomBytes(Neo4Net.io.fs.StoreChannel writer, int size) throws java.io.IOException
		 private sbyte[] WriteRandomBytes( StoreChannel writer, int size )
		 {
			  sbyte[] bytes = new sbyte[size];
			  _random.NextBytes( bytes );
			  writer.WriteAll( ByteBuffer.wrap( bytes ) );
			  return bytes;
		 }
	}

}