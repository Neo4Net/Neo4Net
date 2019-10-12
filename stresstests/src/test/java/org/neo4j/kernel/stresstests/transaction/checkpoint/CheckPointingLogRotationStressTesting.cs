using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.stresstests.transaction.checkpoint
{
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using TimerTransactionTracer = Org.Neo4j.Kernel.stresstests.transaction.checkpoint.tracers.TimerTransactionTracer;
	using Workload = Org.Neo4j.Kernel.stresstests.transaction.checkpoint.workload.Workload;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using ParallelBatchImporter = Org.Neo4j.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using ExecutionMonitors = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.ensureExistsAndEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.fromEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.stresstests.transaction.checkpoint.mutation.RandomMutationFactory.defaultRandomMutation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;

	/// <summary>
	/// Notice the class name: this is _not_ going to be run as part of the main build.
	/// </summary>
	public class CheckPointingLogRotationStressTesting
	{
		 private const string DEFAULT_DURATION_IN_MINUTES = "5";
		 private static readonly string _defaultStoreDir = new File( getProperty( "java.io.tmpdir" ), "store" ).Path;
		 private const string DEFAULT_NODE_COUNT = "100000";
		 private const string DEFAULT_WORKER_THREADS = "16";
		 private const string DEFAULT_PAGE_CACHE_MEMORY = "4g";

		 private const int CHECK_POINT_INTERVAL_MINUTES = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveCorrectlyUnderStress() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveCorrectlyUnderStress()
		 {
			  long durationInMinutes = parseLong( fromEnv( "CHECK_POINT_LOG_ROTATION_STRESS_DURATION", DEFAULT_DURATION_IN_MINUTES ) );
			  File storeDir = new File( fromEnv( "CHECK_POINT_LOG_ROTATION_STORE_DIRECTORY", _defaultStoreDir ) );
			  long nodeCount = parseLong( fromEnv( "CHECK_POINT_LOG_ROTATION_NODE_COUNT", DEFAULT_NODE_COUNT ) );
			  int threads = parseInt( fromEnv( "CHECK_POINT_LOG_ROTATION_WORKER_THREADS", DEFAULT_WORKER_THREADS ) );
			  string pageCacheMemory = fromEnv( "CHECK_POINT_LOG_ROTATION_PAGE_CACHE_MEMORY", DEFAULT_PAGE_CACHE_MEMORY );

			  Console.WriteLine( "1/6\tBuilding initial store..." );
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					Config dbConfig = Config.defaults();
					( new ParallelBatchImporter( DatabaseLayout.of( ensureExistsAndEmpty( storeDir ) ), fileSystem, null, DEFAULT, NullLogService.Instance, ExecutionMonitors.defaultVisible( jobScheduler ), EMPTY, dbConfig, RecordFormatSelector.selectForConfig( dbConfig, NullLogProvider.Instance ), NO_MONITOR, jobScheduler ) ).doImport( new NodeCountInputs( nodeCount ) );
			  }

			  Console.WriteLine( "2/6\tStarting database..." );
			  GraphDatabaseBuilder builder = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir);
			  GraphDatabaseService db = builder.SetConfig( GraphDatabaseSettings.pagecache_memory, pageCacheMemory ).setConfig( GraphDatabaseSettings.keep_logical_logs, Settings.FALSE ).setConfig( GraphDatabaseSettings.check_point_interval_time, CHECK_POINT_INTERVAL_MINUTES + "m" ).setConfig( GraphDatabaseSettings.tracer, "timer" ).newGraphDatabase();

			  Console.WriteLine( "3/6\tWarm up db..." );
			  using ( Workload workload = new Workload( db, defaultRandomMutation( nodeCount, db ), threads ) )
			  {
					// make sure to run at least one checkpoint during warmup
					long warmUpTimeMillis = TimeUnit.SECONDS.toMillis( CHECK_POINT_INTERVAL_MINUTES * 2 );
					workload.Run( warmUpTimeMillis, Workload.TransactionThroughput_Fields.NONE );
			  }

			  Console.WriteLine( "4/6\tStarting workload..." );
			  TransactionThroughputChecker throughput = new TransactionThroughputChecker();
			  using ( Workload workload = new Workload( db, defaultRandomMutation( nodeCount, db ), threads ) )
			  {
					workload.Run( TimeUnit.MINUTES.toMillis( durationInMinutes ), throughput );
			  }

			  Console.WriteLine( "5/6\tShutting down..." );
			  Db.shutdown();

			  try
			  {
					Console.WriteLine( "6/6\tPrinting stats and recorded timings..." );
					TimerTransactionTracer.printStats( System.out );
					throughput.AssertThroughput( System.out );
			  }
			  finally
			  {
					Console.WriteLine( "Done." );
			  }

			  // let's cleanup disk space when everything went well
			  FileUtils.deleteRecursively( storeDir );
		 }
	}

}