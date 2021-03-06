﻿/*
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

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

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

		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, JobScheduler jobScheduler )
		 {
			  return CreatePageCache( fileSystem, PageCacheTracer.NULL, DefaultPageCursorTracerSupplier.INSTANCE, Config.defaults(), EmptyVersionContextSupplier.EMPTY, jobScheduler );
		 }

		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, Config config, JobScheduler jobScheduler )
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
		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, Config config, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler )
		 {
			  config.AugmentDefaults( GraphDatabaseSettings.pagecache_memory, "8M" );
			  ZoneId logTimeZone = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
			  FormattedLogProvider logProvider = FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( System.err );
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, config, pageCacheTracer, pageCursorTracerSupplier, logProvider.GetLog( typeof( PageCache ) ), versionContextSupplier, jobScheduler );
			  return pageCacheFactory.OrCreatePageCache;
		 }
	}

}