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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{

	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using Org.Neo4j.Io.pagecache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class MuninnPageCacheFixture : PageCacheTestSupport.Fixture<MuninnPageCache>
	{
		 internal System.Threading.CountdownEvent BackgroundFlushLatch;
		 private MemoryAllocator _allocator;

		 public override MuninnPageCache CreatePageCache( PageSwapperFactory swapperFactory, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier, VersionContextSupplier contextSupplier, JobScheduler jobScheduler )
		 {
			  long memory = MuninnPageCache.MemoryRequiredForPages( maxPages );
			  _allocator = MemoryAllocator.createAllocator( memory.ToString(), new LocalMemoryTracker() );
			  return new MuninnPageCache( swapperFactory, _allocator, tracer, cursorTracerSupplier, contextSupplier, jobScheduler );
		 }

		 public override void TearDownPageCache( MuninnPageCache pageCache )
		 {
			  if ( BackgroundFlushLatch != null )
			  {
					BackgroundFlushLatch.Signal();
					BackgroundFlushLatch = null;
			  }
			  pageCache.Close();
			  _allocator.close();
		 }
	}

}