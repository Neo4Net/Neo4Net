using System;
using System.Collections.Generic;

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
namespace Neo4Net.backup.stresstests
{
	using Test = org.junit.Test;


	using Config = Neo4Net.causalclustering.stresstests.Config;
	using Control = Neo4Net.causalclustering.stresstests.Control;
	using DumpUtils = Neo4Net.Diagnostics.utils.DumpUtils;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using Futures = Neo4Net.Util.concurrent.Futures;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static bool.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.DatabaseConfiguration.configureBackup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.DatabaseConfiguration.configureTxLogRotationAndPruning;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.ensureExistsAndEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.fromEnv;

	/// <summary>
	/// Notice the class name: this is _not_ going to be run as part of the main build.
	/// </summary>
	public class BackupServiceStressTesting
	{
		 private static readonly string _defaultWorkingDir = new File( getProperty( "java.io.tmpdir" ) ).Path;
		 private const string DEFAULT_HOSTNAME = "localhost";
		 private const string DEFAULT_PORT = "8200";
		 private const string DEFAULT_ENABLE_INDEXES = "false";
		 private const string DEFAULT_TX_PRUNE = "50 files";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveCorrectlyUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveCorrectlyUnderStress()
		 {
			  string directory = fromEnv( "BACKUP_SERVICE_STRESS_WORKING_DIRECTORY", _defaultWorkingDir );
			  string backupHostname = fromEnv( "BACKUP_SERVICE_STRESS_BACKUP_HOSTNAME", DEFAULT_HOSTNAME );
			  int backupPort = parseInt( fromEnv( "BACKUP_SERVICE_STRESS_BACKUP_PORT", DEFAULT_PORT ) );
			  string txPrune = fromEnv( "BACKUP_SERVICE_STRESS_TX_PRUNE", DEFAULT_TX_PRUNE );
			  bool enableIndexes = parseBoolean( fromEnv( "BACKUP_SERVICE_STRESS_ENABLE_INDEXES", DEFAULT_ENABLE_INDEXES ) );

			  File store = new File( directory, "db/store" );
			  File work = new File( directory, "backup/work" );
			  FileUtils.deleteRecursively( store );
			  FileUtils.deleteRecursively( work );
			  File storeDirectory = ensureExistsAndEmpty( store );
			  Path workDirectory = ensureExistsAndEmpty( work ).toPath();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> config = configureBackup(configureTxLogRotationAndPruning(new java.util.HashMap<>(), txPrune), backupHostname, backupPort);
			  IDictionary<string, string> config = configureBackup( configureTxLogRotationAndPruning( new Dictionary<string, string>(), txPrune ), backupHostname, backupPort );
			  GraphDatabaseBuilder graphDatabaseBuilder = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDirectory.AbsoluteFile).setConfig(config);

			  Control control = new Control( new Config() );

			  AtomicReference<GraphDatabaseService> dbRef = new AtomicReference<GraphDatabaseService>();
			  ExecutorService service = Executors.newFixedThreadPool( 3 );
			  try
			  {
					dbRef.set( graphDatabaseBuilder.NewGraphDatabase() );
					if ( enableIndexes )
					{
						 TransactionalWorkload.SetupIndexes( dbRef.get() );
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> workload = service.submit(new TransactionalWorkload(control, dbRef::get));
					Future<object> workload = service.submit( new TransactionalWorkload( control, dbRef.get ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> backupWorker = service.submit(new BackupLoad(control, backupHostname, backupPort, workDirectory));
					Future<object> backupWorker = service.submit( new BackupLoad( control, backupHostname, backupPort, workDirectory ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> startStopWorker = service.submit(new StartStop(control, graphDatabaseBuilder::newGraphDatabase, dbRef));
					Future<object> startStopWorker = service.submit( new StartStop( control, graphDatabaseBuilder.newGraphDatabase, dbRef ) );

					control.AwaitEnd( asList( workload, backupWorker, startStopWorker ) );
					control.AssertNoFailure();

					service.shutdown();
					if ( !service.awaitTermination( 30, SECONDS ) )
					{
						 PrintThreadDump();
						 fail( "Didn't manage to shut down the workers correctly, dumped threads for forensic purposes" );
					}
			  }
			  catch ( TimeoutException t )
			  {
					Console.Error.WriteLine( format( "Timeout waiting task completion. Dumping all threads." ) );
					PrintThreadDump();
					throw t;
			  }
			  finally
			  {
					dbRef.get().shutdown();
					service.shutdown();
			  }

			  // let's cleanup disk space when everything went well
			  FileUtils.deleteRecursively( storeDirectory );
			  FileUtils.deletePathRecursively( workDirectory );
		 }

		 private static void PrintThreadDump()
		 {
			  Console.Error.WriteLine( DumpUtils.threadDump() );
		 }
	}

}