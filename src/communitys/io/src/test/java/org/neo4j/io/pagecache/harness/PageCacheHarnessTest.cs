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
namespace Neo4Net.Io.pagecache.harness
{
	using RepeatedTest = org.junit.jupiter.api.RepeatedTest;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Neo4Net.Io.pagecache;
	using PageCountRecordFormat = Neo4Net.Io.pagecache.randomharness.PageCountRecordFormat;
	using Phase = Neo4Net.Io.pagecache.randomharness.Phase;
	using RandomPageCacheTestHarness = Neo4Net.Io.pagecache.randomharness.RandomPageCacheTestHarness;
	using RecordFormat = Neo4Net.Io.pagecache.randomharness.RecordFormat;
	using StandardRecordFormat = Neo4Net.Io.pagecache.randomharness.StandardRecordFormat;
	using Profiler = Neo4Net.Resources.Profiler;
	using Inject = Neo4Net.Test.extension.Inject;
	using ProfilerExtension = Neo4Net.Test.extension.ProfilerExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.FlushCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.FlushFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.MapFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.ReadMulti;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.ReadRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.UnmapFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.WriteMulti;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.randomharness.Command.WriteRecord;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, ProfilerExtension.class}) abstract class PageCacheHarnessTest<T extends org.neo4j.io.pagecache.PageCache> extends org.neo4j.io.pagecache.PageCacheTestSupport<T>
	internal abstract class PageCacheHarnessTest<T> : PageCacheTestSupport<T> where T : Neo4Net.Io.pagecache.PageCache
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject public org.neo4j.test.rule.TestDirectory directory;
		 public TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject public org.neo4j.resources.Profiler profiler;
		 public Profiler Profiler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(10) void readsAndWritesMustBeMutuallyConsistent()
		 internal virtual void ReadsAndWritesMustBeMutuallyConsistent()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				int filePageCount = 100;
				using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
				{
					 harness.disableCommands( FlushCache, FlushFile, MapFile, UnmapFile );
					 harness.setCommandProbabilityFactor( ReadRecord, 0.5 );
					 harness.setCommandProbabilityFactor( WriteRecord, 0.5 );
					 harness.ConcurrencyLevel = 8;
					 harness.FilePageCount = filePageCount;
					 harness.InitialMappedFiles = 1;
					 harness.Verification = FilesAreCorrectlyWrittenVerification( new StandardRecordFormat(), filePageCount );
					 harness.run( SEMI_LONG_TIMEOUT_MILLIS, TimeUnit.MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentPageFaultingMustNotPutInterleavedDataIntoPages()
		 internal virtual void ConcurrentPageFaultingMustNotPutInterleavedDataIntoPages()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				const int filePageCount = 11;
				RecordFormat recordFormat = new PageCountRecordFormat();
				using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
				{
					 harness.ConcurrencyLevel = 11;
					 harness.UseAdversarialIO = false;
					 harness.CachePageCount = 3;
					 harness.FilePageCount = filePageCount;
					 harness.InitialMappedFiles = 1;
					 harness.CommandCount = 10000;
					 harness.RecordFormat = recordFormat;
					 harness.FileSystem = fs;
					 harness.useProfiler( Profiler );
					 harness.disableCommands( FlushCache, FlushFile, MapFile, UnmapFile, WriteRecord, WriteMulti );
					 harness.Preparation = ( cache, fs, filesTouched ) =>
					 {
						  File file = filesTouched.GetEnumerator().next();
						  using ( PagedFile pf = cache.map( file, cache.pageSize() ), PageCursor cursor = pf.Io(0, PF_SHARED_WRITE_LOCK) )
						  {
								for ( int pageId = 0; pageId < filePageCount; pageId++ )
								{
									 cursor.Next();
									 recordFormat.fillWithRecords( cursor );
								}
						  }
					 };

					 harness.run( LONG_TIMEOUT_MILLIS, MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentFlushingMustNotPutInterleavedDataIntoFile()
		 internal virtual void ConcurrentFlushingMustNotPutInterleavedDataIntoFile()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				RecordFormat recordFormat = new StandardRecordFormat();
				const int filePageCount = 2_000;
				using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
				{
					 harness.ConcurrencyLevel = 16;
					 harness.UseAdversarialIO = false;
					 harness.CachePageCount = filePageCount / 2;
					 harness.FilePageCount = filePageCount;
					 harness.InitialMappedFiles = 3;
					 harness.CommandCount = 15_000;
					 harness.FileSystem = fs;
					 harness.disableCommands( MapFile, UnmapFile, ReadRecord, ReadMulti );
					 harness.Verification = FilesAreCorrectlyWrittenVerification( recordFormat, filePageCount );

					 harness.run( LONG_TIMEOUT_MILLIS, MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentFlushingWithMischiefMustNotPutInterleavedDataIntoFile()
		 internal virtual void ConcurrentFlushingWithMischiefMustNotPutInterleavedDataIntoFile()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				RecordFormat recordFormat = new StandardRecordFormat();
				const int filePageCount = 2_000;
				using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
				{
					 harness.ConcurrencyLevel = 16;
					 harness.UseAdversarialIO = true;
					 harness.MischiefRate = 0.5;
					 harness.FailureRate = 0.0;
					 harness.ErrorRate = 0.0;
					 harness.CachePageCount = filePageCount / 2;
					 harness.FilePageCount = filePageCount;
					 harness.InitialMappedFiles = 3;
					 harness.CommandCount = 15_000;
					 harness.FileSystem = fs;
					 harness.disableCommands( MapFile, UnmapFile, ReadRecord, ReadMulti );
					 harness.Verification = FilesAreCorrectlyWrittenVerification( recordFormat, filePageCount );

					 harness.run( LONG_TIMEOUT_MILLIS, MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentFlushingWithFailuresMustNotPutInterleavedDataIntoFile()
		 internal virtual void ConcurrentFlushingWithFailuresMustNotPutInterleavedDataIntoFile()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				RecordFormat recordFormat = new StandardRecordFormat();
				const int filePageCount = 2_000;
				using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
				{
					 harness.ConcurrencyLevel = 16;
					 harness.UseAdversarialIO = true;
					 harness.MischiefRate = 0.0;
					 harness.FailureRate = 0.5;
					 harness.ErrorRate = 0.0;
					 harness.CachePageCount = filePageCount / 2;
					 harness.FilePageCount = filePageCount;
					 harness.InitialMappedFiles = 3;
					 harness.CommandCount = 15_000;
					 harness.FileSystem = fs;
					 harness.disableCommands( MapFile, UnmapFile, ReadRecord, ReadMulti );
					 harness.Verification = FilesAreCorrectlyWrittenVerification( recordFormat, filePageCount );

					 harness.run( LONG_TIMEOUT_MILLIS, MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.io.pagecache.randomharness.Phase filesAreCorrectlyWrittenVerification(final org.neo4j.io.pagecache.randomharness.RecordFormat recordFormat, final int filePageCount)
		 private Phase FilesAreCorrectlyWrittenVerification( RecordFormat recordFormat, int filePageCount )
		 {
			  return ( cache, fs1, filesTouched ) =>
			  {
				foreach ( File file in filesTouched )
				{
					 using ( PagedFile pf = cache.map( file, cache.pageSize() ), PageCursor cursor = pf.Io(0, PF_SHARED_READ_LOCK) )
					 {
						  for ( int pageId = 0; pageId < filePageCount && cursor.Next(); pageId++ )
						  {
								try
								{
									 recordFormat.AssertRecordsWrittenCorrectly( cursor );
								}
								catch ( Exception th )
								{
									 th.addSuppressed( new Exception( "pageId = " + pageId ) );
									 throw th;
								}
						  }
					 }
					 using ( StoreChannel channel = fs1.open( file, OpenMode.READ ) )
					 {
						  recordFormat.AssertRecordsWrittenCorrectly( file, channel );
					 }
				}
			  };
		 }
	}

}