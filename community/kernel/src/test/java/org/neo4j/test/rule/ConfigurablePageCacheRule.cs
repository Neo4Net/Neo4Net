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
namespace Org.Neo4j.Test.rule
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;

	public class ConfigurablePageCacheRule : PageCacheRule
	{
		 public virtual PageCache GetPageCache( FileSystemAbstraction fs, Config config )
		 {
			  return GetPageCache( fs, config(), config );
		 }

		 public virtual PageCache GetPageCache( FileSystemAbstraction fs, PageCacheConfig pageCacheConfig, Config config )
		 {
			  CloseExistingPageCache();
			  PageCache = CreatePageCache( fs, pageCacheConfig, config );
			  PageCachePostConstruct( pageCacheConfig );
			  return PageCache;
		 }

		 private PageCache CreatePageCache( FileSystemAbstraction fs, PageCacheConfig pageCacheConfig, Config config )
		 {
			  PageCacheTracer tracer = SelectConfig( BaseConfig.tracer, pageCacheConfig.Tracer, PageCacheTracer.NULL );
			  PageCursorTracerSupplier cursorTracerSupplier = SelectConfig( BaseConfig.pageCursorTracerSupplier, pageCacheConfig.PageCursorTracerSupplier, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  config.AugmentDefaults( GraphDatabaseSettings.pagecache_memory, "8M" );
			  FormattedLogProvider logProvider = FormattedLogProvider.toOutputStream( System.err );
			  InitializeJobScheduler();
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fs, config, tracer, cursorTracerSupplier, logProvider.GetLog( typeof( PageCache ) ), EmptyVersionContextSupplier.EMPTY, JobScheduler );
			  return pageCacheFactory.OrCreatePageCache;
		 }
	}

}