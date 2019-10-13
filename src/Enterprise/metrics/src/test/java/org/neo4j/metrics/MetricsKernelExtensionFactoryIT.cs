﻿using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.metrics
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using ClusterMetrics = Neo4Net.metrics.source.cluster.ClusterMetrics;
	using CheckPointingMetrics = Neo4Net.metrics.source.db.CheckPointingMetrics;
	using CypherMetrics = Neo4Net.metrics.source.db.CypherMetrics;
	using EntityCountMetrics = Neo4Net.metrics.source.db.EntityCountMetrics;
	using TransactionMetrics = Neo4Net.metrics.source.db.TransactionMetrics;
	using ThreadMetrics = Neo4Net.metrics.source.jvm.ThreadMetrics;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.check_point_interval_time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.cypher_min_replan_interval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.graphiteInterval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.metricsEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readLongValueAndAssert;

	public class MetricsKernelExtensionFactoryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.record_id_batch_size, "1");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedSetting(GraphDatabaseSettings.record_id_batch_size, "1");

		 private HighlyAvailableGraphDatabase _db;
		 private File _outputPath;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _outputPath = ClusterRule.directory( "metrics" );
			  IDictionary<string, string> config = stringMap( MetricsSettings.NeoEnabled.name(), Settings.TRUE, metricsEnabled.name(), Settings.TRUE, csvEnabled.name(), Settings.TRUE, cypher_min_replan_interval.name(), "0m", csvPath.name(), _outputPath.AbsolutePath, check_point_interval_time.name(), "100ms", graphiteInterval.name(), "1s", OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE );
			  _db = ClusterRule.withSharedConfig( config ).withCluster( clusterOfSize( 1 ) ).startCluster().Master;
			  AddNodes( 1 ); // to make sure creation of label and property key tokens do not mess up with assertions in tests
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowTxCommittedMetricsWhenMetricsEnabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowTxCommittedMetricsWhenMetricsEnabled()
		 {
			  // GIVEN
			  long lastCommittedTransactionId = _db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;

			  // Create some activity that will show up in the metrics data.
			  AddNodes( 1000 );
			  File metricsFile = metricsCsv( _outputPath, TransactionMetrics.TX_COMMITTED );

			  // WHEN
			  // We should at least have a "timestamp" column, and a "neo4j.transaction.committed" column
			  long committedTransactions = readLongValueAndAssert( metricsFile, ( newValue, currentValue ) => newValue >= currentValue );

			  // THEN
			  assertThat( committedTransactions, greaterThanOrEqualTo( lastCommittedTransactionId ) );
			  assertThat( committedTransactions, lessThanOrEqualTo( lastCommittedTransactionId + 1001L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowEntityCountMetricsWhenMetricsEnabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowEntityCountMetricsWhenMetricsEnabled()
		 {
			  // GIVEN
			  // Create some activity that will show up in the metrics data.
			  AddNodes( 1000 );
			  File metricsFile = metricsCsv( _outputPath, EntityCountMetrics.COUNTS_NODE );

			  // WHEN
			  // We should at least have a "timestamp" column, and a "neo4j.transaction.committed" column
			  long committedTransactions = readLongValueAndAssert( metricsFile, ( newValue, currentValue ) => newValue >= currentValue );

			  // THEN
			  assertThat( committedTransactions, lessThanOrEqualTo( 1001L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowClusterMetricsWhenMetricsEnabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowClusterMetricsWhenMetricsEnabled()
		 {
			  // GIVEN
			  // Create some activity that will show up in the metrics data.
			  AddNodes( 1000 );
			  File metricsFile = metricsCsv( _outputPath, ClusterMetrics.IS_MASTER );

			  // WHEN
			  // We should at least have a "timestamp" column, and a "neo4j.transaction.committed" column
			  long committedTransactions = readLongValueAndAssert( metricsFile, ( newValue, currentValue ) => newValue >= currentValue );

			  // THEN
			  assertThat( committedTransactions, equalTo( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void showReplanEvents() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShowReplanEvents()
		 {
			  // GIVEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "match (n:Label {name: 'Pontus'}) return n.name" ).close();
					tx.Success();
			  }

			  //add some data, should make plan stale
			  AddNodes( 10 );

			  // WHEN
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction tx = _db.beginTx() )
					{
						 _db.execute( "match (n:Label {name: 'Pontus'}) return n.name" ).close();
						 tx.Success();
					}
					AddNodes( 1 );
			  }

			  File replanCountMetricFile = metricsCsv( _outputPath, CypherMetrics.REPLAN_EVENTS );
			  File replanWaitMetricFile = metricsCsv( _outputPath, CypherMetrics.REPLAN_WAIT_TIME );

			  // THEN see that the replan metric have pickup up at least one replan event
			  // since reporting happens in an async fashion then give it some time and check now and then
			  long endTime = currentTimeMillis() + TimeUnit.SECONDS.toMillis(10);
			  long events = 0;
			  while ( currentTimeMillis() < endTime && events == 0 )
			  {
					readLongValueAndAssert( replanWaitMetricFile, ( newValue, currentValue ) => newValue >= currentValue );
					events = readLongValueAndAssert( replanCountMetricFile, ( newValue, currentValue ) => newValue >= currentValue );
					if ( events == 0 )
					{
						 Thread.Sleep( 300 );
					}
			  }
			  assertThat( events, greaterThan( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseEventBasedReportingCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseEventBasedReportingCorrectly()
		 {
			  // GIVEN
			  AddNodes( 100 );

			  // WHEN
			  CheckPointer checkPointer = _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) );
			  checkPointer.CheckPointIfNeeded( new SimpleTriggerInfo( "test" ) );

			  // wait for the file to be written before shutting down the cluster
			  File metricFile = metricsCsv( _outputPath, CheckPointingMetrics.CHECK_POINT_DURATION );

			  long result = readLongValueAndAssert( metricFile, ( newValue, currentValue ) => newValue >= 0 );

			  // THEN
			  assertThat( result, greaterThanOrEqualTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowMetricsForThreads() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowMetricsForThreads()
		 {
			  // WHEN
			  AddNodes( 100 );

			  // wait for the file to be written before shutting down the cluster
			  File threadTotalFile = metricsCsv( _outputPath, ThreadMetrics.THREAD_TOTAL );
			  File threadCountFile = metricsCsv( _outputPath, ThreadMetrics.THREAD_COUNT );

			  long threadTotalResult = readLongValueAndAssert( threadTotalFile, ( newValue, currentValue ) => newValue >= 0 );
			  long threadCountResult = readLongValueAndAssert( threadCountFile, ( newValue, currentValue ) => newValue >= 0 );

			  // THEN
			  assertThat( threadTotalResult, greaterThanOrEqualTo( 0L ) );
			  assertThat( threadCountResult, greaterThanOrEqualTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToStartWithNullTracer()
		 public virtual void MustBeAbleToStartWithNullTracer()
		 {
			  // Start the database
			  File disabledTracerDb = ClusterRule.directory( "disabledTracerDb" );
			  GraphDatabaseBuilder builder = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(disabledTracerDb);
			  GraphDatabaseService nullTracerDatabase = builder.SetConfig( MetricsSettings.NeoEnabled, Settings.TRUE ).setConfig( csvEnabled, Settings.TRUE ).setConfig( csvPath, _outputPath.AbsolutePath ).setConfig( GraphDatabaseSettings.tracer, "null" ).setConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).newGraphDatabase();
			  try
			  {
					  using ( Transaction tx = nullTracerDatabase.BeginTx() )
					  {
						Node node = nullTracerDatabase.CreateNode();
						node.SetProperty( "all", "is well" );
						tx.Success();
					  }
			  }
			  finally
			  {
					nullTracerDatabase.Shutdown();
			  }
			  // We assert that no exception is thrown during startup or the operation of the database.
		 }

		 private void AddNodes( int numberOfNodes )
		 {
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					using ( Transaction tx = _db.beginTx() )
					{
						 Node node = _db.createNode( Label.label( "Label" ) );
						 node.SetProperty( "name", System.Guid.randomUUID().ToString() );
						 tx.Success();
					}
			  }
		 }
	}

}