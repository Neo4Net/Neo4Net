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
	using Service = Org.Neo4j.Helpers.Service;
	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using OsBeanUtil = Org.Neo4j.Io.os.OsBeanUtil;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageSwapperFactory = Org.Neo4j.Io.pagecache.PageSwapperFactory;
	using SingleFilePageSwapperFactory = Org.Neo4j.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Org.Neo4j.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using GlobalMemoryTracker = Org.Neo4j.Memory.GlobalMemoryTracker;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.mapped_memory_page_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_swapper;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BYTES;

	public class ConfiguringPageCacheFactory
	{
		 private PageSwapperFactory _swapperFactory;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Config _config;
		 private readonly PageCacheTracer _pageCacheTracer;
		 private readonly Log _log;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private PageCache _pageCache;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly JobScheduler _scheduler;

		 /// <summary>
		 /// Construct configuring page cache factory </summary>
		 /// <param name="fs"> fileSystem file system that page cache will be based on </param>
		 /// <param name="config"> page swapper configuration </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracer that will provide
		 /// thread local page cache statistics </param>
		 /// <param name="log"> page cache factory log </param>
		 /// <param name="versionContextSupplier"> cursor context factory </param>
		 /// <param name="scheduler"> job scheduler to execute page cache jobs </param>
		 public ConfiguringPageCacheFactory( FileSystemAbstraction fs, Config config, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, Log log, VersionContextSupplier versionContextSupplier, JobScheduler scheduler )
		 {
			  this._fs = fs;
			  this._versionContextSupplier = versionContextSupplier;
			  this._config = config;
			  this._pageCacheTracer = pageCacheTracer;
			  this._log = log;
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
			  this._scheduler = scheduler;
		 }

		 public virtual PageCache OrCreatePageCache
		 {
			 get
			 {
				 lock ( this )
				 {
					  if ( _pageCache == null )
					  {
							this._swapperFactory = CreateAndConfigureSwapperFactory( _fs, _config, _log );
							this._pageCache = CreatePageCache();
					  }
					  return _pageCache;
				 }
			 }
		 }

		 protected internal virtual PageCache CreatePageCache()
		 {
			  CheckPageSize( _config );
			  MemoryAllocator memoryAllocator = BuildMemoryAllocator( _config );
			  return new MuninnPageCache( _swapperFactory, memoryAllocator, _pageCacheTracer, _pageCursorTracerSupplier, _versionContextSupplier, _scheduler );
		 }

		 private MemoryAllocator BuildMemoryAllocator( Config config )
		 {
			  string pageCacheMemorySetting = config.Get( pagecache_memory );
			  if ( string.ReferenceEquals( pageCacheMemorySetting, null ) )
			  {
					long heuristic = DefaultHeuristicPageCacheMemory();
					_log.warn( "The " + pagecache_memory.name() + " setting has not been configured. It is recommended that this " + "setting is always explicitly configured, to ensure the system has a balanced configuration. " + "Until then, a computed heuristic value of " + heuristic + " bytes will be used instead. " + "Run `neo4j-admin memrec` for memory configuration suggestions." );
					pageCacheMemorySetting = "" + heuristic;
			  }

			  return MemoryAllocator.createAllocator( pageCacheMemorySetting, GlobalMemoryTracker.INSTANCE );
		 }

		 public static long DefaultHeuristicPageCacheMemory()
		 {
			  // First check if we have a default override...
			  string defaultMemoryOverride = System.getProperty( "dbms.pagecache.memory.default.override" );
			  if ( !string.ReferenceEquals( defaultMemoryOverride, null ) )
			  {
					return BYTES.apply( defaultMemoryOverride );
			  }

			  double ratioOfFreeMem = 0.50;
			  string defaultMemoryRatioOverride = System.getProperty( "dbms.pagecache.memory.ratio.default.override" );
			  if ( !string.ReferenceEquals( defaultMemoryRatioOverride, null ) )
			  {
					ratioOfFreeMem = double.Parse( defaultMemoryRatioOverride );
			  }

			  // Try to compute (RAM - maxheap) * 0.50 if we can get reliable numbers...
			  long maxHeapMemory = Runtime.Runtime.maxMemory();
			  if ( 0 < maxHeapMemory && maxHeapMemory < long.MaxValue )
			  {
					try
					{
						 long physicalMemory = OsBeanUtil.TotalPhysicalMemory;
						 if ( 0 < physicalMemory && physicalMemory < long.MaxValue && maxHeapMemory < physicalMemory )
						 {
							  long heuristic = ( long )( ( physicalMemory - maxHeapMemory ) * ratioOfFreeMem );
							  long min = ByteUnit.mebiBytes( 32 ); // We'd like at least 32 MiBs.
							  long max = Math.Min( maxHeapMemory * 70, ByteUnit.gibiBytes( 20 ) );
							  // Don't heuristically take more than 20 GiBs, and don't take more than 70 times our max heap.
							  // 20 GiBs of page cache memory is ~2.6 million 8 KiB pages. If each page has an overhead of
							  // 72 bytes, then this will take up ~175 MiBs of heap memory. We should be able to tolerate that
							  // in most environments. The "no more than 70 times heap" heuristic is based on the page size over
							  // the per page overhead, 8192 / 72 ~= 114, plus leaving some extra room on the heap for the rest
							  // of the system. This means that we won't heuristically try to create a page cache that is too
							  // large to fit on the heap.
							  return Math.Min( max, Math.Max( min, heuristic ) );
						 }
					}
					catch ( Exception )
					{
					}
			  }
			  // ... otherwise we just go with 2 GiBs.
			  return ByteUnit.gibiBytes( 2 );
		 }

		 public virtual void CheckPageSize( Config config )
		 {
			  if ( config.Get( mapped_memory_page_size ).intValue() != 0 )
			  {
					_log.warn( "The setting unsupported.dbms.memory.pagecache.pagesize does not have any effect. It is " + "deprecated and will be removed in a future version." );
			  }
		 }

		 public virtual void DumpConfiguration()
		 {
			  CheckPageSize( _config );
			  string pageCacheMemory = _config.get( pagecache_memory );
			  long totalPhysicalMemory = OsBeanUtil.TotalPhysicalMemory;
			  string totalPhysicalMemMb = ( totalPhysicalMemory == OsBeanUtil.VALUE_UNAVAILABLE ) ? "?" : "" + ByteUnit.Byte.toMebiBytes( totalPhysicalMemory );
			  long maxVmUsageMb = ByteUnit.Byte.toMebiBytes( Runtime.Runtime.maxMemory() );
			  string msg = "Physical mem: " + totalPhysicalMemMb + " MiB," +
								" Heap size: " + maxVmUsageMb + " MiB," +
								" Page cache: " + pageCacheMemory + ".";

			  _log.info( msg );
		 }

		 private static PageSwapperFactory CreateAndConfigureSwapperFactory( FileSystemAbstraction fs, Config config, Log log )
		 {
			  PageSwapperFactory factory = GetPageSwapperFactory( config, log );
			  factory.Open( fs, config );
			  return factory;
		 }

		 private static PageSwapperFactory GetPageSwapperFactory( Config config, Log log )
		 {
			  string desiredImplementation = config.Get( pagecache_swapper );
			  if ( !string.ReferenceEquals( desiredImplementation, null ) )
			  {
					foreach ( PageSwapperFactory factory in Service.load( typeof( PageSwapperFactory ) ) )
					{
						 if ( factory.ImplementationName().Equals(desiredImplementation) )
						 {
							  log.Info( "Configured " + pagecache_swapper.name() + ": " + desiredImplementation );
							  return factory;
						 }
					}
					throw new System.ArgumentException( "Cannot find PageSwapperFactory: " + desiredImplementation );
			  }
			  return new SingleFilePageSwapperFactory();
		 }
	}

}