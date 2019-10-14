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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/*
	 * This class is an helper to allow to construct properly a page cache in the few places we need it without all
	 * the graph database stuff, e.g., various store dump programs.
	 *
	 * All other places where a "proper" page cache is available, e.g. in store migration, should have that one injected.
	 * And tests should use the PageCacheRule.
	 */
	public sealed class StandalonePageCacheFactory
	{
		 private StandalonePageCacheFactory()
		 {
		 }

		 public static PageCache CreatePageCache( FileSystemAbstraction fileSystem, JobScheduler jobScheduler )
		 {
			  SingleFilePageSwapperFactory factory = new SingleFilePageSwapperFactory();
			  factory.Open( fileSystem, Configuration.EMPTY );

			  PageCacheTracer cacheTracer = PageCacheTracer.NULL;
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = DefaultPageCursorTracerSupplier.INSTANCE;
			  VersionContextSupplier versionContextSupplier = EmptyVersionContextSupplier.EMPTY;
			  MemoryAllocator memoryAllocator = MemoryAllocator.createAllocator( "8 MiB", GlobalMemoryTracker.INSTANCE );
			  return new MuninnPageCache( factory, memoryAllocator, cacheTracer, cursorTracerSupplier, versionContextSupplier, jobScheduler );
		 }
	}

}