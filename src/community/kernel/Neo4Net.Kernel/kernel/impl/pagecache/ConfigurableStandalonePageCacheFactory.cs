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
namespace Neo4Net.Kernel.impl.pagecache
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/*
	 * This class is an helper to allow to construct properly a page cache in the few places we need it without all
	 * the graph database stuff, e.g., various store dump programs.
	 *
	 * All other places where a "proper" page cache is available, e.g. in store migration, should have that one injected.
	 * And tests should use the ConfigurablePageCacheRule.
	 */
	public sealed class ConfigurableStandalonePageCacheFactory
	{
		 private ConfigurableStandalonePageCacheFactory()
		 {
		 }

		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, IJobScheduler jobScheduler )
		 {
			  return CreatePageCache( fileSystem, PageCacheTracer.NULL, DefaultPageCursorTracerSupplier.INSTANCE, Config.defaults(), EmptyVersionContextSupplier.EMPTY, jobScheduler );
		 }

		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, Config config, IJobScheduler jobScheduler )
		 {
			  return CreatePageCache( fileSystem, PageCacheTracer.NULL, DefaultPageCursorTracerSupplier.INSTANCE, config, EmptyVersionContextSupplier.EMPTY, jobScheduler );
		 }

		 /// <summary>
		 /// Create page cache </summary>
		 /// <param name="fileSystem"> file system that page cache will be based on </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracer that will provide
		 /// thread local page cache statistics </param>
		 /// <param name="config"> page cache configuration </param>
		 /// <param name="versionContextSupplier"> version context supplier </param>
		 /// <param name="jobScheduler"> page cache job scheduler </param>
		 /// <returns> created page cache instance </returns>
		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, Config config, VersionContextSupplier versionContextSupplier, IJobScheduler jobScheduler )
		 {
			  config.AugmentDefaults( GraphDatabaseSettings.pagecache_memory, "8M" );
			  ZoneId logTimeZone = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
			  FormattedLogProvider logProvider = FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( System.err );
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, config, pageCacheTracer, pageCursorTracerSupplier, logProvider.GetLog( typeof( PageCache ) ), versionContextSupplier, jobScheduler );
			  return pageCacheFactory.OrCreatePageCache;
		 }
	}

}