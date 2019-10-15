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
namespace Neo4Net.Test.rule
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;

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
			  PageCursorTracerSupplier cursorTracerSupplier = SelectConfig( BaseConfig.pageCursorTracerSupplier, pageCacheConfig.PageCursorTracerSupplier, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  config.AugmentDefaults( GraphDatabaseSettings.pagecache_memory, "8M" );
			  FormattedLogProvider logProvider = FormattedLogProvider.toOutputStream( System.err );
			  InitializeJobScheduler();
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fs, config, tracer, cursorTracerSupplier, logProvider.GetLog( typeof( PageCache ) ), EmptyVersionContextSupplier.EMPTY, IJobScheduler );
			  return pageCacheFactory.OrCreatePageCache;
		 }
	}

}