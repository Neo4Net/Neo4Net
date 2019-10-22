using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.pagecache
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Resource = Neo4Net.GraphDb.Resource;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;

	public class PageCacheWarmerTest
	{
		private bool InstanceFieldsInitialized = false;

		public PageCacheWarmerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			Rules = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _pageCacheRule );
		}

		 private FileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;
		 private PageCacheRule _pageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(pageCacheRule);
		 public RuleChain Rules;

		 private LifeSupport _life;
		 private IJobScheduler _scheduler;
		 private DefaultPageCacheTracer _cacheTracer;
		 private DefaultPageCursorTracerSupplier _cursorTracer;
		 private PageCacheRule.PageCacheConfig _cfg;
		 private File _file;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _life = new LifeSupport();
			  _scheduler = _life.add( createScheduler() );
			  _life.start();
			  _cacheTracer = new DefaultPageCacheTracer();
			  _cursorTracer = DefaultPageCursorTracerSupplier.INSTANCE;
			  ClearTracerCounts();
			  _cfg = PageCacheRule.config().withTracer(_cacheTracer).withCursorTracerSupplier(_cursorTracer);
			  _file = new File( _testDirectory.databaseDir(), "a" );
			  _fs.create( _file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _life.shutdown();
		 }

		 private void ClearTracerCounts()
		 {
			  _cursorTracer.get().init(PageCacheTracer.NULL);
			  _cursorTracer.get().reportEvents();
			  _cursorTracer.get().init(_cacheTracer);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotReheatAfterStop() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotReheatAfterStop()
		 {
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile ignore = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Stop();
					assertSame( long?.empty(), warmer.Reheat() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNoProfileAfterStop() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNoProfileAfterStop()
		 {
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile ignore = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Stop();
					assertSame( long?.empty(), warmer.Profile() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listOnlyDatabaseRelaterFilesInListOfMetadata() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListOnlyDatabaseRelaterFilesInListOfMetadata()
		 {
			  File ignoredFile = new File( _testDirectory.storeDir(), "b" );
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile include = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ), PagedFile ignore = pageCache.Map(ignoredFile, pageCache.PageSize(), StandardOpenOption.CREATE) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Profile();

					List<StoreFileMetadata> filesMetadata = new List<StoreFileMetadata>();
					warmer.AddFilesTo( filesMetadata );

					assertThat( filesMetadata, hasSize( 1 ) );
					assertTrue( filesMetadata[0].File().Name.StartsWith(_file.Name) );

					warmer.Stop();
					assertSame( long?.empty(), warmer.Profile() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void profileAndReheatAfterRestart() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProfileAndReheatAfterRestart()
		 {
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Stop();
					warmer.Start();
					using ( PageCursor writer = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 assertTrue( writer.Next( 1 ) );
						 assertTrue( writer.Next( 3 ) );
					}
					warmer.Profile();
					assertNotSame( long?.empty(), warmer.Reheat() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustDoNothingWhenReheatingUnprofiledPageCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDoNothingWhenReheatingUnprofiledPageCache()
		 {

			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile ignore = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Reheat();
			  }
			  _cursorTracer.get().reportEvents();
			  assertThat( _cacheTracer.faults(), @is(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReheatProfiledPageCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReheatProfiledPageCache()
		 {
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					using ( PageCursor writer = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 assertTrue( writer.Next( 1 ) );
						 assertTrue( writer.Next( 3 ) );
					}
					pf.FlushAndForce();
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Profile();
			  }

			  ClearTracerCounts();
			  long initialFaults = _cacheTracer.faults();
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize() ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Reheat();

					pageCache.ReportEvents();
					assertThat( _cacheTracer.faults(), @is(initialFaults + 2L) );

					using ( PageCursor reader = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( reader.Next( 1 ) );
						 assertTrue( reader.Next( 3 ) );
					}

					// No additional faults must have been reported.
					pageCache.ReportEvents();
					assertThat( _cacheTracer.faults(), @is(initialFaults + 2L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reheatingMustWorkOnLargeNumberOfPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReheatingMustWorkOnLargeNumberOfPages()
		 {
			  int maxPagesInMemory = 1_000;
			  int[] pageIds = RandomSortedPageIds( maxPagesInMemory );

			  string pageCacheMemory = ( maxPagesInMemory * ByteUnit.kibiBytes( 9 ) ).ToString();
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg.withMemory( pageCacheMemory ) ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					using ( PageCursor writer = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 foreach ( int pageId in pageIds )
						 {
							  assertTrue( writer.Next( pageId ) );
						 }
					}
					pf.FlushAndForce();
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Profile();
			  }

			  long initialFaults = _cacheTracer.faults();
			  ClearTracerCounts();
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize() ) )
			  {
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Reheat();

					pageCache.ReportEvents();
					assertThat( _cacheTracer.faults(), @is(initialFaults + pageIds.Length) );

					using ( PageCursor reader = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
					{
						 foreach ( int pageId in pageIds )
						 {
							  assertTrue( reader.Next( pageId ) );
						 }
					}

					// No additional faults must have been reported.
					pageCache.ReportEvents();
					assertThat( _cacheTracer.faults(), @is(initialFaults + pageIds.Length) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Test public void profileMustNotDeleteFilesCurrentlyExposedViaFileListing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProfileMustNotDeleteFilesCurrentlyExposedViaFileListing()
		 {
			  using ( PageCache pageCache = _pageCacheRule.getPageCache( _fs, _cfg ), PagedFile pf = pageCache.Map( _file, pageCache.PageSize(), StandardOpenOption.CREATE ) )
			  {
					using ( PageCursor writer = pf.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 assertTrue( writer.Next( 1 ) );
						 assertTrue( writer.Next( 3 ) );
					}
					pf.FlushAndForce();
					PageCacheWarmer warmer = new PageCacheWarmer( _fs, pageCache, _scheduler, _testDirectory.databaseDir() );
					warmer.Start();
					warmer.Profile();
					warmer.Profile();
					warmer.Profile();

					IList<StoreFileMetadata> fileListing = new List<StoreFileMetadata>();
					using ( Resource firstListing = warmer.AddFilesTo( fileListing ) )
					{
						 warmer.Profile();
						 warmer.Profile();

						 // The files in the file listing cannot be deleted while the listing is in use.
						 assertThat( fileListing.Count, greaterThan( 0 ) );
						 AssertFilesExists( fileListing );
						 warmer.Profile();
						 using ( Resource secondListing = warmer.AddFilesTo( new List<StoreFileMetadata>() ) )
						 {
							  warmer.Profile();
							  // This must hold even when there are file listings overlapping in time.
							  AssertFilesExists( fileListing );
						 }
						 warmer.Profile();
						 // And continue to hold after other overlapping listing finishes.
						 AssertFilesExists( fileListing );
					}
					// Once we are done with the file listing, profile should remove those files.
					warmer.Profile();
					warmer.Stop();
					AssertFilesNotExists( fileListing );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void profilesMustSortByPagedFileAndProfileSequenceId()
		 public virtual void ProfilesMustSortByPagedFileAndProfileSequenceId()
		 {
			  File fileAA = new File( "aa" );
			  File fileAB = new File( "ab" );
			  File fileBA = new File( "ba" );
			  Profile aa;
			  Profile ab;
			  Profile ba;
			  IList<Profile> sortedProfiles = Arrays.asList( aa = Profile.First( fileAA ), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa = aa.Next(), aa.Next(), ab = Profile.First(fileAB), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab = ab.Next(), ab.Next(), ba = Profile.First(fileBA), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba = ba.Next(), ba.Next() );
			  IList<Profile> resortedProfiles = new List<Profile>( sortedProfiles );
			  Collections.shuffle( resortedProfiles );
			  resortedProfiles.Sort();
			  assertThat( resortedProfiles, @is( sortedProfiles ) );
		 }

		 private void AssertFilesExists( IList<StoreFileMetadata> fileListing )
		 {
			  foreach ( StoreFileMetadata fileMetadata in fileListing )
			  {
					assertTrue( _fs.fileExists( fileMetadata.File() ) );
			  }
		 }

		 private void AssertFilesNotExists( IList<StoreFileMetadata> fileListing )
		 {
			  foreach ( StoreFileMetadata fileMetadata in fileListing )
			  {
					assertFalse( _fs.fileExists( fileMetadata.File() ) );
			  }
		 }

		 private static int[] RandomSortedPageIds( int maxPagesInMemory )
		 {
			  MutableIntSet setIds = new IntHashSet();
			  ThreadLocalRandom rng = ThreadLocalRandom.current();
			  for ( int i = 0; i < maxPagesInMemory; i++ )
			  {
					setIds.add( rng.Next( maxPagesInMemory * 7 ) );
			  }
			  int[] pageIds = new int[setIds.size()];
			  IntIterator itr = setIds.intIterator();
			  int i = 0;
			  while ( itr.hasNext() )
			  {
					pageIds[i] = itr.next();
					i++;
			  }
			  Arrays.sort( pageIds );
			  return pageIds;
		 }
	}

}