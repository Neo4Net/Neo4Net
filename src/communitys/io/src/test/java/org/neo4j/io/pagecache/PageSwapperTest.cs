using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.pagecache
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public abstract class PageSwapperTest
	public abstract class PageSwapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject public org.neo4j.test.rule.TestDirectory testDir;
		 public TestDirectory TestDir;
		 public const long X = unchecked( ( long )0xcafebabedeadbeefL );
		 public static readonly long Y = X ^ ( X << 1 );
		 public const int Z = unchecked( ( int )0xfefefefe );

		 protected internal static readonly PageEvictionCallback NoCallback = filePageId =>
		 {
		 };

		 private const int CACHE_PAGE_SIZE = 32;
		 private readonly ConcurrentLinkedQueue<PageSwapperFactory> _openedFactories = new ConcurrentLinkedQueue<PageSwapperFactory>();
		 private readonly ConcurrentLinkedQueue<PageSwapper> _openedSwappers = new ConcurrentLinkedQueue<PageSwapper>();
		 private readonly MemoryAllocator _mman = MemoryAllocator.createAllocator( "32 KiB", new LocalMemoryTracker() );

		 protected internal abstract PageSwapperFactory SwapperFactory();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void mkdirs(java.io.File dir) throws java.io.IOException;
		 protected internal abstract void Mkdirs( File dir );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach @AfterEach void clearStrayInterrupts()
		 internal virtual void ClearStrayInterrupts()
		 {
			  Thread.interrupted();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void closeOpenedPageSwappers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseOpenedPageSwappers()
		 {
			  Exception exception = null;
			  PageSwapperFactory factory;
			  PageSwapper swapper;

			  while ( ( swapper = _openedSwappers.poll() ) != null )
			  {
					try
					{
						 swapper.Close();
					}
					catch ( IOException e )
					{
						 if ( exception == null )
						 {
							  exception = e;
						 }
						 else
						 {
							  exception.addSuppressed( e );
						 }
					}
			  }

			  while ( ( factory = _openedFactories.poll() ) != null )
			  {
					try
					{
						 factory.Close();
					}
					catch ( Exception e )
					{
						 if ( exception == null )
						 {
							  exception = e;
						 }
						 else
						 {
							  exception.addSuppressed( e );
						 }
					}
			  }

			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readMustNotSwallowInterrupts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReadMustNotSwallowInterrupts()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PutInt( page, 0, 1 );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  assertThat( Write( swapper, 0, page ), @is( SizeOfAsLong( page ) ) );
						 PutInt( page, 0, 0 );
			  Thread.CurrentThread.Interrupt();

			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );

			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustNotSwallowInterrupts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustNotSwallowInterrupts()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PutInt( page, 0, 1 );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  assertThat( Write( swapper, 0, page ), @is( SizeOfAsLong( page ) ) );
						 PutInt( page, 0, 0 );
			  Thread.CurrentThread.Interrupt();

			  assertThat( Read( swapper, 0, new long[]{ page }, 0, 1 ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );

			  assertThat( Read( swapper, 0, new long[] { page }, 0, 1 ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeMustNotSwallowInterrupts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void WriteMustNotSwallowInterrupts()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PutInt( page, 0, 1 );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  Thread.CurrentThread.Interrupt();

			  assertThat( Write( swapper, 0, page ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );

			  PutInt( page, 0, 0 );
			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );

			  assertThat( Write( swapper, 0, page ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );

			  PutInt( page, 0, 0 );
			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustNotSwallowInterrupts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustNotSwallowInterrupts()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PutInt( page, 0, 1 );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  Thread.CurrentThread.Interrupt();

			  assertThat( Write( swapper, 0, new long[] { page }, 0, 1 ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );

			  PutInt( page, 0, 0 );
			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );

			  assertThat( Write( swapper, 0, new long[]{ page }, 0, 1 ), @is( SizeOfAsLong( page ) ) );
			  assertTrue( Thread.CurrentThread.Interrupted );

			  PutInt( page, 0, 0 );
			  assertThat( Read( swapper, 0, SizeOfAsInt( page ), page ), @is( SizeOfAsLong( page ) ) );
			  assertThat( GetInt( page, 0 ), @is( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forcingMustNotSwallowInterrupts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ForcingMustNotSwallowInterrupts()
		 {
			  File file = file( "a" );

			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  Thread.CurrentThread.Interrupt();
			  swapper.Force();
			  assertTrue( Thread.CurrentThread.Interrupted );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReopenChannelWhenReadFailsWithAsynchronousCloseException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustReopenChannelWhenReadFailsWithAsynchronousCloseException()
		 {
			  File file = file( "a" );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  long page = CreatePage();
			  PutLong( page, 0, X );
			  PutLong( page, 8, Y );
			  PutInt( page, 16, Z );
			  Write( swapper, 0, page );

			  Thread.CurrentThread.Interrupt();

			  Read( swapper, 0, SizeOfAsInt( page ), page );

			  // Clear the interrupted flag and assert that it was still raised
			  assertTrue( Thread.interrupted() );

			  assertThat( GetLong( page, 0 ), @is( X ) );
			  assertThat( GetLong( page, 8 ), @is( Y ) );
			  assertThat( GetInt( page, 16 ), @is( Z ) );

			  // This must not throw because we should still have a usable channel
			  swapper.Force();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReopenChannelWhenVectoredReadFailsWithAsynchronousCloseException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustReopenChannelWhenVectoredReadFailsWithAsynchronousCloseException()
		 {
			  File file = file( "a" );
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  long page = CreatePage();
			  PutLong( page, 0, X );
			  PutLong( page, 8, Y );
			  PutInt( page, 16, Z );
			  Write( swapper, 0, page );

			  Thread.CurrentThread.Interrupt();

			  Read( swapper, 0, new long[]{ page }, 0, 1 );

			  // Clear the interrupted flag and assert that it was still raised
			  assertTrue( Thread.interrupted() );

			  assertThat( GetLong( page, 0 ), @is( X ) );
			  assertThat( GetLong( page, 8 ), @is( Y ) );
			  assertThat( GetInt( page, 16 ), @is( Z ) );

			  // This must not throw because we should still have a usable channel
			  swapper.Force();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReopenChannelWhenWriteFailsWithAsynchronousCloseException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustReopenChannelWhenWriteFailsWithAsynchronousCloseException()
		 {
			  long page = CreatePage();
			  PutLong( page, 0, X );
			  PutLong( page, 8, Y );
			  PutInt( page, 16, Z );
			  File file = file( "a" );

			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  Thread.CurrentThread.Interrupt();

			  Write( swapper, 0, page );

			  // Clear the interrupted flag and assert that it was still raised
			  assertTrue( Thread.interrupted() );

			  // This must not throw because we should still have a usable channel
			  swapper.Force();

			  Clear( page );
			  Read( swapper, 0, SizeOfAsInt( page ), page );
			  assertThat( GetLong( page, 0 ), @is( X ) );
			  assertThat( GetLong( page, 8 ), @is( Y ) );
			  assertThat( GetInt( page, 16 ), @is( Z ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReopenChannelWhenVectoredWriteFailsWithAsynchronousCloseException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustReopenChannelWhenVectoredWriteFailsWithAsynchronousCloseException()
		 {
			  long page = CreatePage();
			  PutLong( page, 0, X );
			  PutLong( page, 8, Y );
			  PutInt( page, 16, Z );
			  File file = file( "a" );

			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  Thread.CurrentThread.Interrupt();

			  Write( swapper, 0, new long[] { page }, 0, 1 );

			  // Clear the interrupted flag and assert that it was still raised
			  assertTrue( Thread.interrupted() );

			  // This must not throw because we should still have a usable channel
			  swapper.Force();

			  Clear( page );
			  Read( swapper, 0, SizeOfAsInt( page ), page );
			  assertThat( GetLong( page, 0 ), @is( X ) );
			  assertThat( GetLong( page, 8 ), @is( Y ) );
			  assertThat( GetInt( page, 16 ), @is( Z ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReopenChannelWhenForceFailsWithAsynchronousCloseException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustReopenChannelWhenForceFailsWithAsynchronousCloseException()
		 {
			  File file = file( "a" );

			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );

			  for ( int i = 0; i < 10; i++ )
			  {
					Thread.CurrentThread.Interrupt();

					// This must not throw
					swapper.Force();

					// Clear the interrupted flag and assert that it was still raised
					assertTrue( Thread.interrupted() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readMustNotReopenExplicitlyClosedChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReadMustNotReopenExplicitlyClosedChannel()
		 {
			  string filename = "a";
			  File file = file( filename );

			  long page = CreatePage();
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );
			  Write( swapper, 0, page );
			  swapper.Close();

			  assertThrows( typeof( ClosedChannelException ), () => Read(swapper, 0, SizeOfAsInt(page), page) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustNotReopenExplicitlyClosedChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustNotReopenExplicitlyClosedChannel()
		 {
			  string filename = "a";
			  File file = file( filename );

			  long page = CreatePage();
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );
			  Write( swapper, 0, page );
			  swapper.Close();

			  assertThrows( typeof( ClosedChannelException ), () => Read(swapper, 0, new long[]{ page }, 0, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeMustNotReopenExplicitlyClosedChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void WriteMustNotReopenExplicitlyClosedChannel()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );
			  swapper.Close();

			  assertThrows( typeof( ClosedChannelException ), () => Write(swapper, 0, page) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustNotReopenExplicitlyClosedChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustNotReopenExplicitlyClosedChannel()
		 {
			  File file = file( "a" );

			  long page = CreatePage();
			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );
			  swapper.Close();

			  assertThrows( typeof( ClosedChannelException ), () => Write(swapper, 0, new long[]{ page }, 0, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forceMustNotReopenExplicitlyClosedChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ForceMustNotReopenExplicitlyClosedChannel()
		 {
			  File file = file( "a" );

			  PageSwapperFactory swapperFactory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( swapperFactory, file );
			  swapper.Close();

			  assertThrows( typeof( ClosedChannelException ), swapper.force );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotOverwriteDataInOtherFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotOverwriteDataInOtherFiles()
		 {
			  File fileA = File( "a" );
			  File fileB = File( "b" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapperA = CreateSwapperAndFile( factory, fileA );
			  PageSwapper swapperB = CreateSwapperAndFile( factory, fileB );

			  long page = CreatePage();
			  Clear( page );
			  PutLong( page, 0, X );
			  Write( swapperA, 0, page );
			  PutLong( page, 8, Y );
			  Write( swapperB, 0, page );

			  Clear( page );
			  assertThat( GetLong( page, 0 ), @is( 0L ) );
			  assertThat( GetLong( page, 8 ), @is( 0L ) );

			  Read( swapperA, 0, SizeOfAsInt( page ), page );

			  assertThat( GetLong( page, 0 ), @is( X ) );
			  assertThat( GetLong( page, 8 ), @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRunEvictionCallbackOnEviction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustRunEvictionCallbackOnEviction()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong callbackFilePageId = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong callbackFilePageId = new AtomicLong();
			  PageEvictionCallback callback = callbackFilePageId.set;
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, file, CachePageSize(), callback, true, false );
			  swapper.Evicted( 42 );
			  assertThat( callbackFilePageId.get(), @is(42L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotIssueEvictionCallbacksAfterSwapperHasBeenClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotIssueEvictionCallbacksAfterSwapperHasBeenClosed()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean gotCallback = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean gotCallback = new AtomicBoolean();
			  PageEvictionCallback callback = filePageId => gotCallback.set( true );
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, file, CachePageSize(), callback, true, false );
			  swapper.Close();
			  swapper.Evicted( 42 );
			  assertFalse( gotCallback.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowExceptionIfFileDoesNotExist()
		 internal virtual void MustThrowExceptionIfFileDoesNotExist()
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  assertThrows( typeof( NoSuchFileException ), () => CreateSwapper(factory, File("does not exist"), CachePageSize(), NoCallback, false, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCreateNonExistingFileWithCreateFlag() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCreateNonExistingFileWithCreateFlag()
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper pageSwapper = CreateSwapperAndFile( factory, File( "does not exist" ) );

			  // After creating the file, we must also be able to read and write
			  long page = CreatePage();
			  PutLong( page, 0, X );
			  Write( pageSwapper, 0, page );

			  Clear( page );
			  Read( pageSwapper, 0, SizeOfAsInt( page ), page );

			  assertThat( GetLong( page, 0 ), @is( X ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void truncatedFilesMustBeEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TruncatedFilesMustBeEmpty()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file );

			  assertThat( swapper.LastPageId, @is( -1L ) );

			  long page = CreatePage();
			  PutInt( page, 0, unchecked( ( int )0xcafebabe ) );
			  Write( swapper, 10, page );
			  Clear( page );
			  Read( swapper, 10, SizeOfAsInt( page ), page );
			  assertThat( GetInt( page, 0 ), @is( 0xcafebabe ) );
			  assertThat( swapper.LastPageId, @is( 10L ) );

			  swapper.Close();
			  swapper = CreateSwapper( factory, file, CachePageSize(), NoCallback, false, false );
			  Clear( page );
			  Read( swapper, 10, SizeOfAsInt( page ), page );
			  assertThat( GetInt( page, 0 ), @is( 0xcafebabe ) );
			  assertThat( swapper.LastPageId, @is( 10L ) );

			  swapper.Truncate();
			  Clear( page );
			  Read( swapper, 10, SizeOfAsInt( page ), page );
			  assertThat( GetInt( page, 0 ), @is( 0 ) );
			  assertThat( swapper.LastPageId, @is( -1L ) );

			  swapper.Close();
			  swapper = CreateSwapper( factory, file, CachePageSize(), NoCallback, false, false );
			  Clear( page );
			  Read( swapper, 10, SizeOfAsInt( page ), page );
			  assertThat( GetInt( page, 0 ), @is( 0 ) );
			  assertThat( swapper.LastPageId, @is( -1L ) );

			  swapper.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredWriteMustFlushAllBuffersInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredWriteMustFlushAllBuffersInOrder()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  long pageC = CreatePage( 4 );
			  long pageD = CreatePage( 4 );

			  PutInt( pageA, 0, 2 );
			  PutInt( pageB, 0, 3 );
			  PutInt( pageC, 0, 4 );
			  PutInt( pageD, 0, 5 );

			  Write( swapper, 1, new long[]{ pageA, pageB, pageC, pageD }, 0, 4 );

			  long result = CreatePage( 4 );

			  Read( swapper, 0, SizeOfAsInt( result ), result );
			  assertThat( GetInt( result, 0 ), @is( 0 ) );
			  PutInt( result, 0, 0 );
			  assertThat( Read( swapper, 1, SizeOfAsInt( result ), result ), @is( 4L ) );
			  assertThat( GetInt( result, 0 ), @is( 2 ) );
			  PutInt( result, 0, 0 );
			  assertThat( Read( swapper, 2, SizeOfAsInt( result ), result ), @is( 4L ) );
			  assertThat( GetInt( result, 0 ), @is( 3 ) );
			  PutInt( result, 0, 0 );
			  assertThat( Read( swapper, 3, SizeOfAsInt( result ), result ), @is( 4L ) );
			  assertThat( GetInt( result, 0 ), @is( 4 ) );
			  PutInt( result, 0, 0 );
			  assertThat( Read( swapper, 4, SizeOfAsInt( result ), result ), @is( 4L ) );
			  assertThat( GetInt( result, 0 ), @is( 5 ) );
			  PutInt( result, 0, 0 );
			  assertThat( Read( swapper, 5, SizeOfAsInt( result ), result ), @is( 0L ) );
			  assertThat( GetInt( result, 0 ), @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadMustFillAllBuffersInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadMustFillAllBuffersInOrder()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long output = CreatePage();

			  PutInt( output, 0, 2 );
			  Write( swapper, 1, output );
			  PutInt( output, 0, 3 );
			  Write( swapper, 2, output );
			  PutInt( output, 0, 4 );
			  Write( swapper, 3, output );
			  PutInt( output, 0, 5 );
			  Write( swapper, 4, output );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  long pageC = CreatePage( 4 );
			  long pageD = CreatePage( 4 );

			  // Read 4 pages of 4 bytes each
			  assertThat( Read( swapper, 1, new long[]{ pageA, pageB, pageC, pageD }, 0, 4 ), @is( 4 * 4L ) );

			  assertThat( GetInt( pageA, 0 ), @is( 2 ) );
			  assertThat( GetInt( pageB, 0 ), @is( 3 ) );
			  assertThat( GetInt( pageC, 0 ), @is( 4 ) );
			  assertThat( GetInt( pageD, 0 ), @is( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadFromEmptyFileMustFillPagesWithZeros() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadFromEmptyFileMustFillPagesWithZeros()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long page = CreatePage( 4 );
			  PutInt( page, 0, 1 );
			  assertThat( Read( swapper, 0, new long[]{ page }, 0, 1 ), @is( 0L ) );
			  assertThat( GetInt( page, 0 ), @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadBeyondEndOfFileMustFillPagesWithZeros() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadBeyondEndOfFileMustFillPagesWithZeros()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long output = CreatePage( 4 );
			  PutInt( output, 0, unchecked( ( int )0xFFFF_FFFF ) );
			  Write( swapper, 0, new long[]{ output, output, output }, 0, 3 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  PutInt( pageA, 0, -1 );
			  PutInt( pageB, 0, -1 );
			  assertThat( Read( swapper, 3, new long[]{ pageA, pageB }, 0, 2 ), @is( 0L ) );
			  assertThat( GetInt( pageA, 0 ), @is( 0 ) );
			  assertThat( GetInt( pageB, 0 ), @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadWhereLastPageExtendBeyondEndOfFileMustHaveRemainderZeroFilled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadWhereLastPageExtendBeyondEndOfFileMustHaveRemainderZeroFilled()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long output = CreatePage( 4 );
			  PutInt( output, 0, unchecked( ( int )0xFFFF_FFFF ) );
			  Write( swapper, 0, new long[]{ output, output, output, output, output }, 0, 5 );
			  swapper.Close();

			  swapper = CreateSwapper( factory, file, 8, NoCallback, false, false );
			  long pageA = CreatePage( 8 );
			  long pageB = CreatePage( 8 );
			  PutLong( pageA, 0, X );
			  PutLong( pageB, 0, Y );
			  assertThat( Read( swapper, 1, new long[]{ pageA, pageB }, 0, 2 ), isOneOf( 12L, 16L ) );
			  assertThat( GetLong( pageA, 0 ), @is( 0xFFFF_FFFF_FFFF_FFFFL ) );

	//        assertThat( getLong( 0, pageB ), is( 0xFFFF_FFFF_0000_0000L ) );
			  assertThat( GetByte( pageB, 0 ), @is( unchecked( ( sbyte ) 0xFF ) ) );
			  assertThat( GetByte( pageB, 1 ), @is( unchecked( ( sbyte ) 0xFF ) ) );
			  assertThat( GetByte( pageB, 2 ), @is( unchecked( ( sbyte ) 0xFF ) ) );
			  assertThat( GetByte( pageB, 3 ), @is( unchecked( ( sbyte ) 0xFF ) ) );
			  assertThat( GetByte( pageB, 4 ), @is( ( sbyte ) 0x00 ) );
			  assertThat( GetByte( pageB, 5 ), @is( ( sbyte ) 0x00 ) );
			  assertThat( GetByte( pageB, 6 ), @is( ( sbyte ) 0x00 ) );
			  assertThat( GetByte( pageB, 7 ), @is( ( sbyte ) 0x00 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadWhereSecondLastPageExtendBeyondEndOfFileMustHaveRestZeroFilled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadWhereSecondLastPageExtendBeyondEndOfFileMustHaveRestZeroFilled()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long output = CreatePage( 4 );
			  PutInt( output, 0, 1 );
			  Write( swapper, 0, output );
			  PutInt( output, 0, 2 );
			  Write( swapper, 1, output );
			  PutInt( output, 0, 3 );
			  Write( swapper, 2, output );
			  swapper.Close();

			  swapper = CreateSwapper( factory, file, 8, NoCallback, false, false );
			  long pageA = CreatePage( 8 );
			  long pageB = CreatePage( 8 );
			  long pageC = CreatePage( 8 );
			  PutInt( pageA, 0, -1 );
			  PutInt( pageB, 0, -1 );
			  PutInt( pageC, 0, -1 );
			  assertThat( Read( swapper, 0, new long[]{ pageA, pageB, pageC }, 0, 3 ), isOneOf( 12L, 16L ) );
			  assertThat( GetInt( pageA, 0 ), @is( 1 ) );
			  assertThat( GetInt( pageA, 4 ), @is( 2 ) );
			  assertThat( GetInt( pageB, 0 ), @is( 3 ) );
			  assertThat( GetInt( pageB, 4 ), @is( 0 ) );
			  assertThat( GetLong( pageC, 0 ), @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentPositionedVectoredReadsAndWritesMustNotInterfere() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ConcurrentPositionedVectoredReadsAndWritesMustNotInterfere()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PageSwapper swapper = createSwapperAndFile(factory, file, 4);
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );
			  const int pageCount = 100;
			  const int iterations = 20000;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( 1 );
			  long output = CreatePage( 4 );
			  for ( int i = 0; i < pageCount; i++ )
			  {
					PutInt( output, 0, i + 1 );
					Write( swapper, i, output );
			  }

			  Callable<Void> work = () =>
			  {
				ThreadLocalRandom rng = ThreadLocalRandom.current();
				long[] pages = new long[10];
				for ( int i = 0; i < pages.Length; i++ )
				{
					 pages[i] = CreatePage( 4 );
				}

				startLatch.await();
				for ( int i = 0; i < iterations; i++ )
				{
					 long startFilePageId = rng.nextLong( 0, pageCount - pages.Length );
					 if ( rng.nextBoolean() )
					 {
						  // Do read
						  long bytesRead = Read( swapper, startFilePageId, pages, 0, pages.Length );
						  assertThat( bytesRead, @is( pages.Length * 4L ) );
						  for ( int j = 0; j < pages.Length; j++ )
						  {
								int expectedValue = ( int )( 1 + j + startFilePageId );
								int actualValue = GetInt( pages[j], 0 );
								assertThat( actualValue, @is( expectedValue ) );
						  }
					 }
					 else
					 {
						  // Do write
						  for ( int j = 0; j < pages.Length; j++ )
						  {
								int value = ( int )( 1 + j + startFilePageId );
								PutInt( pages[j], 0, value );
						  }
						  assertThat( Write( swapper, startFilePageId, pages, 0, pages.Length ), @is( pages.Length * 4L ) );
					 }
				}
				return null;
			  };

			  int threads = 8;
			  ExecutorService executor = Executors.newFixedThreadPool(threads, r =>
			  {
				Thread thread = Executors.defaultThreadFactory().newThread(r);
				thread.Daemon = true;
				return thread;
			  });
			  IList<Future<Void>> futures = new List<Future<Void>>( threads );
			  for ( int i = 0; i < threads; i++ )
			  {
					futures.Add( executor.submit( work ) );
			  }

			  startLatch.Signal();
			  foreach ( Future<Void> future in futures )
			  {
					future.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredReadMustWorkOnSubsequenceOfGivenArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredReadMustWorkOnSubsequenceOfGivenArray()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  long pageC = CreatePage( 4 );
			  long pageD = CreatePage( 4 );

			  PutInt( pageA, 0, 1 );
			  PutInt( pageB, 0, 2 );
			  PutInt( pageC, 0, 3 );
			  PutInt( pageD, 0, 4 );

			  long[] pages = new long[] { pageA, pageB, pageC, pageD };
			  long bytesWritten = Write( swapper, 0, pages, 0, 4 );
			  assertThat( bytesWritten, @is( 16L ) );

			  PutInt( pageA, 0, 5 );
			  PutInt( pageB, 0, 6 );
			  PutInt( pageC, 0, 7 );
			  PutInt( pageD, 0, 8 );

			  long bytesRead = Read( swapper, 1, pages, 1, 2 );
			  assertThat( bytesRead, @is( 8L ) );

			  int[] actualValues = new int[] { GetInt( pageA, 0 ), GetInt( pageB, 0 ), GetInt( pageC, 0 ), GetInt( pageD, 0 ) };
			  int[] expectedValues = new int[] { 5, 2, 3, 8 };
			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void positionedVectoredWriteMustWorkOnSubsequenceOfGivenArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PositionedVectoredWriteMustWorkOnSubsequenceOfGivenArray()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  long pageC = CreatePage( 4 );
			  long pageD = CreatePage( 4 );

			  PutInt( pageA, 0, 1 );
			  PutInt( pageB, 0, 2 );
			  PutInt( pageC, 0, 3 );
			  PutInt( pageD, 0, 4 );

			  long[] pages = new long[] { pageA, pageB, pageC, pageD };
			  long bytesWritten = Write( swapper, 0, pages, 0, 4 );
			  assertThat( bytesWritten, @is( 16L ) );

			  PutInt( pageB, 0, 6 );
			  PutInt( pageC, 0, 7 );

			  bytesWritten = Write( swapper, 1, pages, 1, 2 );
			  assertThat( bytesWritten, @is( 8L ) );

			  PutInt( pageA, 0, 0 );
			  PutInt( pageB, 0, 0 );
			  PutInt( pageC, 0, 0 );
			  PutInt( pageD, 0, 0 );

			  long bytesRead = Read( swapper, 0, pages, 0, 4 );
			  assertThat( bytesRead, @is( 16L ) );

			  int[] actualValues = new int[] { GetInt( pageA, 0 ), GetInt( pageB, 0 ), GetInt( pageC, 0 ), GetInt( pageD, 0 ) };
			  int[] expectedValues = new int[] { 1, 6, 7, 4 };
			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowNullPointerExceptionFromReadWhenPageArrayIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustThrowNullPointerExceptionFromReadWhenPageArrayIsNull()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long page = CreatePage( 4 );

			  Write( swapper, 0, new long[]{ page, page, page, page }, 0, 4 );

			  assertThrows( typeof( System.NullReferenceException ), () => Read(swapper, 0, null, 0, 4), "vectored read with null array should have thrown" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowNullPointerExceptionFromWriteWhenPageArrayIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustThrowNullPointerExceptionFromWriteWhenPageArrayIsNull()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  assertThrows( typeof( System.NullReferenceException ), () => Write(swapper, 0, null, 0, 4), "vectored write with null array should have thrown" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readMustThrowForNegativeFilePageIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReadMustThrowForNegativeFilePageIds()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  assertThrows( typeof( IOException ), () => Read(swapper, -1, SizeOfAsInt(CreatePage(4)), CreatePage(4)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeMustThrowForNegativeFilePageIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void WriteMustThrowForNegativeFilePageIds()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  assertThrows( typeof( IOException ), () => Write(swapper, -1, CreatePage(4)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustThrowForNegativeFilePageIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustThrowForNegativeFilePageIds()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  assertThrows( typeof( IOException ), () => Read(swapper, -1, new long[]{ CreatePage(4), CreatePage(4) }, 0, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustThrowForNegativeFilePageIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustThrowForNegativeFilePageIds()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  assertThrows( typeof( IOException ), () => Write(swapper, -1, new long[]{ CreatePage(4), CreatePage(4) }, 0, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustThrowForNegativeArrayOffsets() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustThrowForNegativeArrayOffsets()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  Write( swapper, 0, pages, 0, 2 );
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Read(swapper, 0, pages, -1, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustThrowForNegativeArrayOffsets() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustThrowForNegativeArrayOffsets()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Write(swapper, 0, pages, -1, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustThrowWhenLengthGoesBeyondArraySize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustThrowWhenLengthGoesBeyondArraySize()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  Write( swapper, 0, pages, 0, 2 );
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Read(swapper, 0, pages, 1, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustThrowWhenLengthGoesBeyondArraySize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustThrowWhenLengthGoesBeyondArraySize()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Write(swapper, 0, pages, 1, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustThrowWhenArrayOffsetIsEqualToArrayLength() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustThrowWhenArrayOffsetIsEqualToArrayLength()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  Write( swapper, 0, pages, 0, 2 );
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Read(swapper, 0, pages, 2, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustThrowWhenArrayOffsetIsEqualToArrayLength() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustThrowWhenArrayOffsetIsEqualToArrayLength()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Write(swapper, 0, pages, 2, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustThrowWhenArrayOffsetIsGreaterThanArrayLength() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustThrowWhenArrayOffsetIsGreaterThanArrayLength()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  Write( swapper, 0, pages, 0, 2 );
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Read(swapper, 0, pages, 3, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustThrowWhenArrayOffsetIsGreaterThanArrayLength() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustThrowWhenArrayOffsetIsGreaterThanArrayLength()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long[] pages = new long[] { CreatePage( 4 ), CreatePage( 4 ) };
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => Write(swapper, 0, pages, 3, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredReadMustReadNothingWhenLengthIsZero() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredReadMustReadNothingWhenLengthIsZero()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  PutInt( pageA, 0, 1 );
			  PutInt( pageB, 0, 2 );
			  long[] pages = new long[] { pageA, pageB };
			  Write( swapper, 0, pages, 0, 2 );
			  PutInt( pageA, 0, 3 );
			  PutInt( pageB, 0, 4 );
			  Read( swapper, 0, pages, 0, 0 );

			  int[] expectedValues = new int[] { 3, 4 };
			  int[] actualValues = new int[] { GetInt( pageA, 0 ), GetInt( pageB, 0 ) };
			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vectoredWriteMustReadNothingWhenLengthIsZero() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VectoredWriteMustReadNothingWhenLengthIsZero()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );

			  long pageA = CreatePage( 4 );
			  long pageB = CreatePage( 4 );
			  PutInt( pageA, 0, 1 );
			  PutInt( pageB, 0, 2 );
			  long[] pages = new long[] { pageA, pageB };
			  Write( swapper, 0, pages, 0, 2 );
			  PutInt( pageA, 0, 3 );
			  PutInt( pageB, 0, 4 );
			  Write( swapper, 0, pages, 0, 0 );
			  Read( swapper, 0, pages, 0, 2 );

			  int[] expectedValues = new int[] { 1, 2 };
			  int[] actualValues = new int[] { GetInt( pageA, 0 ), GetInt( pageB, 0 ) };
			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustDeleteFileIfClosedWithCloseAndDelete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustDeleteFileIfClosedWithCloseAndDelete()
		 {
			  File file = file( "file" );
			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapperAndFile( factory, file, 4 );
			  swapper.CloseAndDelete();

			  assertThrows( typeof( IOException ), () => CreateSwapper(factory, file, 4, NoCallback, false, false), "should not have been able to create a page swapper for non-existing file" );
		 }

		 protected internal PageSwapperFactory CreateSwapperFactory()
		 {
			  PageSwapperFactory factory = SwapperFactory();
			  _openedFactories.add( factory );
			  return factory;
		 }

		 protected internal virtual long CreatePage( int cachePageSize )
		 {
			  long address = _mman.allocateAligned( cachePageSize + Integer.BYTES, 1 );
			  UnsafeUtil.putInt( address, cachePageSize );
			  return address + Integer.BYTES;
		 }

		 protected internal virtual void Clear( long address )
		 {
			  sbyte b = ( sbyte ) 0;
			  for ( int i = 0; i < CachePageSize(); i++ )
			  {
					UnsafeUtil.putByte( address + i, b );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PageSwapper createSwapper(PageSwapperFactory factory, java.io.File file, int filePageSize, PageEvictionCallback callback, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException
		 protected internal virtual PageSwapper CreateSwapper( PageSwapperFactory factory, File file, int filePageSize, PageEvictionCallback callback, bool createIfNotExist, bool noChannelStriping )
		 {
			  PageSwapper swapper = factory.CreatePageSwapper( file, filePageSize, callback, createIfNotExist, noChannelStriping );
			  _openedSwappers.add( swapper );
			  return swapper;
		 }

		 protected internal virtual int SizeOfAsInt( long page )
		 {
			  return UnsafeUtil.getInt( page - Integer.BYTES );
		 }

		 protected internal virtual void PutInt( long address, int offset, int value )
		 {
			  UnsafeUtil.putInt( address + offset, value );
		 }

		 protected internal virtual int GetInt( long address, int offset )
		 {
			  return UnsafeUtil.getInt( address + offset );
		 }

		 protected internal virtual void PutLong( long address, int offset, long value )
		 {
			  UnsafeUtil.putLong( address + offset, value );
		 }

		 protected internal virtual long GetLong( long address, int offset )
		 {
			  return UnsafeUtil.getLong( address + offset );
		 }

		 protected internal virtual sbyte GetByte( long address, int offset )
		 {
			  return UnsafeUtil.getByte( address + offset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long write(PageSwapper swapper, int filePageId, long address) throws java.io.IOException
		 private long Write( PageSwapper swapper, int filePageId, long address )
		 {
			  return swapper.Write( filePageId, address );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long read(PageSwapper swapper, int filePageId, int bufferSize, long address) throws java.io.IOException
		 private long Read( PageSwapper swapper, int filePageId, int bufferSize, long address )
		 {
			  return swapper.Read( filePageId, address, bufferSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long read(PageSwapper swapper, long startFilePageId, long[] pages, int arrayOffset, int length) throws java.io.IOException
		 private long Read( PageSwapper swapper, long startFilePageId, long[] pages, int arrayOffset, int length )
		 {
			  if ( pages.Length == 0 )
			  {
					return 0;
			  }
			  int bufferSize = SizeOfAsInt( pages[0] );
			  return swapper.Read( startFilePageId, pages, bufferSize, arrayOffset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long write(PageSwapper swapper, long startFilePageId, long[] pages, int arrayOffset, int length) throws java.io.IOException
		 private long Write( PageSwapper swapper, long startFilePageId, long[] pages, int arrayOffset, int length )
		 {
			  if ( pages.Length == 0 )
			  {
					return 0;
			  }
			  return swapper.Write( startFilePageId, pages, arrayOffset, length );
		 }

		 private int CachePageSize()
		 {
			  return CACHE_PAGE_SIZE;
		 }

		 private long CreatePage()
		 {
			  return CreatePage( CachePageSize() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PageSwapper createSwapperAndFile(PageSwapperFactory factory, java.io.File file) throws java.io.IOException
		 private PageSwapper CreateSwapperAndFile( PageSwapperFactory factory, File file )
		 {
			  return CreateSwapperAndFile( factory, file, CachePageSize() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PageSwapper createSwapperAndFile(PageSwapperFactory factory, java.io.File file, int filePageSize) throws java.io.IOException
		 private PageSwapper CreateSwapperAndFile( PageSwapperFactory factory, File file, int filePageSize )
		 {
			  return CreateSwapper( factory, file, filePageSize, NoCallback, true, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File file(String filename) throws java.io.IOException
		 private File File( string filename )
		 {
			  File file = TestDir.file( filename );
			  Mkdirs( file.ParentFile );
			  return file;
		 }

		 private long SizeOfAsLong( long page )
		 {
			  return SizeOfAsInt( page );
		 }
	}

}