using System;

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
namespace Org.Neo4j.Kernel.impl.pagecache
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MuninnPageCache = Org.Neo4j.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_swapper;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.pagecache.PageSwapperFactoryForTesting.TEST_PAGESWAPPER_NAME;

	public class ConfiguringPageCacheFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 private JobScheduler _jobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
			  PageSwapperFactoryForTesting.CreatedCounter.set( 0 );
			  PageSwapperFactoryForTesting.ConfiguredCounter.set( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFitAsManyPagesAsItCan()
		 public virtual void ShouldFitAsManyPagesAsItCan()
		 {
			  // Given
			  long pageCount = 60;
			  long memory = MuninnPageCache.memoryRequiredForPages( pageCount );
			  Config config = Config.defaults( pagecache_memory, Convert.ToString( memory ) );

			  // When
			  ConfiguringPageCacheFactory factory = new ConfiguringPageCacheFactory( FsRule.get(), config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, EmptyVersionContextSupplier.EMPTY, _jobScheduler );

			  // Then
			  using ( PageCache cache = factory.OrCreatePageCache )
			  {
					assertThat( cache.PageSize(), equalTo(Org.Neo4j.Io.pagecache.PageCache_Fields.PAGE_SIZE) );
					assertThat( cache.MaxCachedPages(), equalTo(pageCount) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnWhenCreatedWithConfiguredPageCache()
		 public virtual void ShouldWarnWhenCreatedWithConfiguredPageCache()
		 {
			  // Given
			  Config config = Config.defaults( stringMap( GraphDatabaseSettings.mapped_memory_page_size.name(), "4096", pagecache_swapper.name(), TEST_PAGESWAPPER_NAME ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.GetLog( typeof( PageCache ) );

			  // When
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( FsRule.get(), config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, log, EmptyVersionContextSupplier.EMPTY, _jobScheduler );

			  // Then
			  using ( PageCache ignore = pageCacheFactory.OrCreatePageCache )
			  {
					logProvider.RawMessageMatcher().assertContains("The setting unsupported.dbms.memory.pagecache.pagesize does not have any effect. It is " + "deprecated and will be removed in a future version.");
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustUseAndLogConfiguredPageSwapper()
		 public virtual void MustUseAndLogConfiguredPageSwapper()
		 {
			  // Given
			  Config config = Config.defaults( stringMap( pagecache_memory.name(), "8m", pagecache_swapper.name(), TEST_PAGESWAPPER_NAME ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.GetLog( typeof( PageCache ) );

			  // When
			  ConfiguringPageCacheFactory cacheFactory = new ConfiguringPageCacheFactory( FsRule.get(), config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, log, EmptyVersionContextSupplier.EMPTY, _jobScheduler );
			  cacheFactory.OrCreatePageCache.close();

			  // Then
			  assertThat( PageSwapperFactoryForTesting.CountCreatedPageSwapperFactories(), @is(1) );
			  assertThat( PageSwapperFactoryForTesting.CountConfiguredPageSwapperFactories(), @is(1) );
			  logProvider.RawMessageMatcher().assertContains(TEST_PAGESWAPPER_NAME);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowIfConfiguredPageSwapperCannotBeFound()
		 public virtual void MustThrowIfConfiguredPageSwapperCannotBeFound()
		 {
			  // Given
			  Config config = Config.defaults( stringMap( pagecache_memory.name(), "8m", pagecache_swapper.name(), "non-existing" ) );

			  // When
			  ( new ConfiguringPageCacheFactory( FsRule.get(), config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, EmptyVersionContextSupplier.EMPTY, _jobScheduler ) ).OrCreatePageCache.close();
		 }
	}

}