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
namespace Neo4Net.metrics.output
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using TransactionMetrics = Neo4Net.metrics.source.db.TransactionMetrics;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.csvMaxArchives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.csvPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.csvRotationThreshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.readLongValueAndAssert;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class RotatableCsvOutputIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _outputPath;
		 private IGraphDatabaseService _database;
		 private static readonly System.Func<long, long, bool> _monotonic = ( newValue, currentValue ) => newValue >= currentValue;
		 private const int MAX_ARCHIVES = 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _outputPath = TestDirectory.directory( "metrics" );
			  _database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(csvPath, _outputPath.AbsolutePath).setConfig(csvRotationThreshold, "21").setConfig(csvMaxArchives, MAX_ARCHIVES.ToString()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rotateMetricsFile() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RotateMetricsFile()
		 {
			  // Commit a transaction and wait for rotation to happen
			  DoTransaction();
			  WaitForRotation( _outputPath, TransactionMetrics.TX_COMMITTED );

			  // Latest file should now have recorded the transaction
			  File metricsFile = MetricsCsv( _outputPath, TransactionMetrics.TX_COMMITTED );
			  long committedTransactions = readLongValueAndAssert( metricsFile, _monotonic );
			  assertEquals( 1, committedTransactions );

			  // Commit yet another transaction and wait for rotation to happen again
			  DoTransaction();
			  WaitForRotation( _outputPath, TransactionMetrics.TX_COMMITTED );

			  // Latest file should now have recorded the new transaction
			  File metricsFile2 = MetricsCsv( _outputPath, TransactionMetrics.TX_COMMITTED );
			  long committedTransactions2 = readLongValueAndAssert( metricsFile2, _monotonic );
			  assertEquals( 2, committedTransactions2 );
		 }

		 private void DoTransaction()
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.createNode();
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void waitForRotation(java.io.File dbDir, String metric) throws InterruptedException
		 private static void WaitForRotation( File dbDir, string metric )
		 {
			  // Find highest missing file
			  int i = 0;
			  while ( GetMetricFile( dbDir, metric, i ).exists() )
			  {
					i++;
			  }

			  if ( i >= MAX_ARCHIVES )
			  {
					fail( "Test did not finish before " + MAX_ARCHIVES + " rotations, which means we have rotated away from the " + "file we want to assert on." );
			  }

			  // wait for file to exists
			  MetricsCsv( dbDir, metric, i );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File metricsCsv(java.io.File dbDir, String metric) throws InterruptedException
		 private static File MetricsCsv( File dbDir, string metric )
		 {
			  return MetricsCsv( dbDir, metric, 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File metricsCsv(java.io.File dbDir, String metric, long index) throws InterruptedException
		 private static File MetricsCsv( File dbDir, string metric, long index )
		 {
			  File csvFile = GetMetricFile( dbDir, metric, index );
			  assertEventually( "Metrics file should exist", csvFile.exists, @is( true ), 40, SECONDS );
			  return csvFile;
		 }

		 private static File GetMetricFile( File dbDir, string metric, long index )
		 {
			  return new File( dbDir, index > 0 ? metric + ".csv." + index : metric + ".csv" );
		 }
	}

}