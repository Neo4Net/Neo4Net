using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint
{
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using TimerTransactionTracer = Neo4Net.Kernel.stresstests.transaction.checkpoint.tracers.TimerTransactionTracer;
	using Workload = Neo4Net.Kernel.stresstests.transaction.checkpoint.workload.Workload;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using ParallelBatchImporter = Neo4Net.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using ExecutionMonitors = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helper.StressTestingHelper.ensureExistsAndEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helper.StressTestingHelper.fromEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.stresstests.transaction.checkpoint.mutation.RandomMutationFactory.defaultRandomMutation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;

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
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					Config dbConfig = Config.defaults();
					( new ParallelBatchImporter( DatabaseLayout.of( ensureExistsAndEmpty( storeDir ) ), fileSystem, null, DEFAULT, NullLogService.Instance, ExecutionMonitors.defaultVisible( jobScheduler ), EMPTY, dbConfig, RecordFormatSelector.selectForConfig( dbConfig, NullLogProvider.Instance ), NO_MONITOR, jobScheduler ) ).doImport( new NodeCountInputs( nodeCount ) );
			  }

			  Console.WriteLine( "2/6\tStarting database..." );
			  GraphDatabaseBuilder builder = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir);
			  IGraphDatabaseService db = builder.SetConfig( GraphDatabaseSettings.pagecache_memory, pageCacheMemory ).setConfig( GraphDatabaseSettings.keep_logical_logs, Settings.FALSE ).setConfig( GraphDatabaseSettings.check_point_interval_time, CHECK_POINT_INTERVAL_MINUTES + "m" ).setConfig( GraphDatabaseSettings.tracer, "timer" ).newGraphDatabase();

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