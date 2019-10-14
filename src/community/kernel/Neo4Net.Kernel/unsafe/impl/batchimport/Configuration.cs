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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.round;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.gibiBytes;

	/// <summary>
	/// User controlled configuration for a <seealso cref="BatchImporter"/>.
	/// </summary>
	public interface Configuration
	{
		 /// <summary>
		 /// File name in which bad entries from the import will end up. This file will be created in the
		 /// database directory of the imported database, i.e. <into>/bad.log.
		 /// </summary>

		 /// <summary>
		 /// A <seealso cref="Stage"/> works with batches going through one or more <seealso cref="Step steps"/> where one or more threads
		 /// process batches at each <seealso cref="Step"/>. This setting dictates how big the batches that are passed around are.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int batchSize()
	//	 {
	//		  return 10_000;
	//	 }

		 /// <summary>
		 /// For statistics the average processing time is based on total processing time divided by
		 /// number of batches processed. A total average is probably not that interesting so this configuration
		 /// option specifies how many of the latest processed batches counts in the equation above.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int movingAverageSize()
	//	 {
	//		  return 100;
	//	 }

		 /// <summary>
		 /// Rough max number of processors (CPU cores) simultaneously used in total by importer at any given time.
		 /// This value should be set while taking the necessary IO threads into account; the page cache and the operating
		 /// system will require a couple of threads between them, to handle the IO workload the importer generates.
		 /// Defaults to the value provided by the <seealso cref="Runtime.availableProcessors() jvm"/>. There's a discrete
		 /// number of threads that needs to be used just to get the very basics of the import working,
		 /// so for that reason there's no lower bound to this value.
		 ///   "Processor" in the context of the batch importer is different from "thread" since when discovering
		 /// how many processors are fully in use there's a calculation where one thread takes up 0 < fraction <= 1
		 /// of a processor.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int maxNumberOfProcessors()
	//	 {
	//		  return allAvailableProcessors();
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static int allAvailableProcessors()
	//	 {
	//		  return Runtime.getRuntime().availableProcessors();
	//	 }

		 /// <returns> number of relationships threshold for considering a node dense. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int denseNodeThreshold()
	//	 {
	//		  return int.Parse(dense_node_threshold.getDefaultValue());
	//	 }

		 /// <returns> amount of memory to reserve for the page cache. This should just be "enough" for it to be able
		 /// to sequentially read and write a couple of stores at a time. If configured too high then there will
		 /// be less memory available for other caches which are critical during the import. Optimal size is
		 /// estimated to be 100-200 MiB. The importer will figure out an optimal page size from this value,
		 /// with slightly bigger page size than "normal" random access use cases. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long pageCacheMemory()
	//	 {
	//		  // Get the upper bound of what we can get from the default config calculation
	//		  // We even want to limit amount of memory a bit more since we don't need very much during import
	//		  long defaultPageCacheMemory = ConfiguringPageCacheFactory.defaultHeuristicPageCacheMemory();
	//		  return min(MAX_PAGE_CACHE_MEMORY, defaultPageCacheMemory);
	//	 }

		 /// <returns> max memory to use for import cache data structures while importing.
		 /// This should exclude the memory acquired by this JVM. By default this returns total physical
		 /// memory on the machine it's running on minus the max memory of this JVM.
		 /// {@value #DEFAULT_MAX_MEMORY_PERCENT}% of that figure. </returns>
		 /// <exception cref="UnsupportedOperationException"> if available memory couldn't be determined. </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long maxMemoryUsage()
	//	 {
	//		  return calculateMaxMemoryFromPercent(DEFAULT_MAX_MEMORY_PERCENT);
	//	 }

		 /// <returns> whether or not to do sequential flushing of the page cache in the during stages which
		 /// import nodes and relationships. Having this {@code true} will reduce random I/O and make most
		 /// writes happen in this single background thread and will greatly benefit hardware which generally
		 /// benefits from single sequential writer. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean sequentialBackgroundFlushing()
	//	 {
	//		  return true;
	//	 }

		 /// <summary>
		 /// Controls whether or not to write records in parallel. Multiple threads writing records in parallel
		 /// doesn't necessarily mean concurrent I/O because writing is separate from page cache eviction/flushing.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean parallelRecordWrites()
	//	 {
	//		  // Defaults to true since this benefits virtually all environments
	//		  return true;
	//	 }

		 /// <summary>
		 /// Controls whether or not to read records in parallel in stages where there's no record writing.
		 /// Enabling this may result in multiple pages being read from underlying storage concurrently.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean parallelRecordReads()
	//	 {
	//		  // Defaults to true since this benefits most environments
	//		  return true;
	//	 }

		 /// <summary>
		 /// Controls whether or not to read records in parallel in stages where there's concurrent record writing.
		 /// Enabling will probably increase concurrent I/O to a point which reduces performance if underlying storage
		 /// isn't great at concurrent I/O, especially if also <seealso cref="parallelRecordWrites()"/> is enabled.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean highIO()
	//	 {
	//		  // Defaults to false since some environments sees less performance with this enabled
	//		  return false;
	//	 }

		 /// <summary>
		 /// Whether or not to allocate memory for holding the cache on heap. The first alternative is to allocate
		 /// off-heap, but if there's no more available memory, but there might be in the heap the importer will
		 /// try to allocate chunks of the cache on heap instead. This config control whether or not to allow
		 /// this allocation to happen on heap.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean allowCacheAllocationOnHeap()
	//	 {
	//		  return false;
	//	 }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Configuration DEFAULT = new Configuration()
	//	 {
	//	 };

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static Configuration withBatchSize(Configuration config, int batchSize)
	//	 {
	//		  return new Overridden(config)
	//		  {
	//				@@Override public int batchSize()
	//				{
	//					 return batchSize;
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static boolean canDetectFreeMemory()
	//	 {
	//		  return OsBeanUtil.getFreePhysicalMemory() != OsBeanUtil.VALUE_UNAVAILABLE;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static long calculateMaxMemoryFromPercent(int percent)
	//	 {
	//		  if (percent < 1)
	//		  {
	//				throw new IllegalArgumentException("Expected percentage to be > 0, was " + percent);
	//		  }
	//		  if (percent > 100)
	//		  {
	//				throw new IllegalArgumentException("Expected percentage to be < 100, was " + percent);
	//		  }
	//		  long totalPhysicalMemory = OsBeanUtil.getTotalPhysicalMemory();
	//		  if (totalPhysicalMemory == OsBeanUtil.VALUE_UNAVAILABLE)
	//		  {
	//				// Unable to detect amount of free memory, so rather max memory should be explicitly set
	//				// in order to get best performance. However let's just go with a default of 2G in this case.
	//				return gibiBytes(2);
	//		  }
	//
	//		  double factor = percent / 100D;
	//		  return round((totalPhysicalMemory - Runtime.getRuntime().maxMemory()) * factor);
	//	 }
	}

	public static class Configuration_Fields
	{
		 public const string BAD_FILE_NAME = "bad.log";
		 public static readonly long MaxPageCacheMemory = gibiBytes( 1 );
		 public const int DEFAULT_MAX_MEMORY_PERCENT = 90;
	}

	 public class Configuration_Overridden : Configuration
	 {
		  internal readonly Configuration Defaults;
		  internal readonly Config Config;

		  public Configuration_Overridden( Configuration defaults ) : this( defaults, Config.defaults() )
		  {
		  }

		  public Configuration_Overridden( Configuration defaults, Config config )
		  {
				this.Defaults = defaults;
				this.Config = config;
		  }

		  public Configuration_Overridden( Config config ) : this( Configuration.DEFAULT, config )
		  {
		  }

		  public override long PageCacheMemory()
		  {
				string pageCacheMemory = Config.get( pagecache_memory );
				if ( string.ReferenceEquals( pageCacheMemory, null ) )
				{
					 pageCacheMemory = ConfiguringPageCacheFactory.defaultHeuristicPageCacheMemory() + "";
				}
				return min( Configuration_Fields.MaxPageCacheMemory, ByteUnit.parse( pageCacheMemory ) );
		  }

		  public override int DenseNodeThreshold()
		  {
				return Config.Raw.ContainsKey( dense_node_threshold.name() ) ? Config.get(dense_node_threshold) : Defaults.denseNodeThreshold();
		  }

		  public override int MovingAverageSize()
		  {
				return Defaults.movingAverageSize();
		  }

		  public override bool SequentialBackgroundFlushing()
		  {
				return Defaults.sequentialBackgroundFlushing();
		  }

		  public override int BatchSize()
		  {
				return Defaults.batchSize();
		  }

		  public override int MaxNumberOfProcessors()
		  {
				return Defaults.maxNumberOfProcessors();
		  }

		  public override bool ParallelRecordWrites()
		  {
				return Defaults.parallelRecordWrites();
		  }

		  public override bool ParallelRecordReads()
		  {
				return Defaults.parallelRecordReads();
		  }

		  public override bool HighIO()
		  {
				return Defaults.highIO();
		  }

		  public override long MaxMemoryUsage()
		  {
				return Defaults.maxMemoryUsage();
		  }

		  public override bool AllowCacheAllocationOnHeap()
		  {
				return Defaults.allowCacheAllocationOnHeap();
		  }
	 }

}