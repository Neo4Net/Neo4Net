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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using GlobalMemoryTracker = Org.Neo4j.Memory.GlobalMemoryTracker;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

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