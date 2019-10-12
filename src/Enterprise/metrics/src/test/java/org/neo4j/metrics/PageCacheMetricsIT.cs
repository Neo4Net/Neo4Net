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
namespace Neo4Net.metrics
{
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readDoubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_EVICTIONS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_EVICTION_EXCEPTIONS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_FLUSHES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_HITS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_HIT_RATIO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_PAGE_FAULTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_PINS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_UNPINS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_USAGE_RATIO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class PageCacheMetricsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private File _metricsDirectory;
		 private GraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _metricsDirectory = TestDirectory.directory( "metrics" );
			  _database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(MetricsSettings.MetricsEnabled, Settings.FALSE).setConfig(MetricsSettings.NeoPageCacheEnabled, Settings.TRUE).setConfig(MetricsSettings.CsvEnabled, Settings.TRUE).setConfig(MetricsSettings.CsvInterval, "100ms").setConfig(MetricsSettings.CsvPath, _metricsDirectory.AbsolutePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageCacheMetrics() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageCacheMetrics()
		 {
			  Label testLabel = Label.label( "testLabel" );
			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( testLabel );
					node.SetProperty( "property", "value" );
					transaction.Success();
			  }

			  using ( Transaction ignored = _database.beginTx() )
			  {
					ResourceIterator<Node> nodes = _database.findNodes( testLabel );
					assertEquals( 1, nodes.Count() );
			  }

			  AssertMetrics( "Metrics report should include page cache pins", PC_PINS, greaterThan( 0L ) );
			  AssertMetrics( "Metrics report should include page cache unpins", PC_UNPINS, greaterThan( 0L ) );
			  AssertMetrics( "Metrics report should include page cache evictions", PC_EVICTIONS, greaterThanOrEqualTo( 0L ) );
			  AssertMetrics( "Metrics report should include page cache page faults", PC_PAGE_FAULTS, greaterThan( 0L ) );
			  AssertMetrics( "Metrics report should include page cache hits", PC_HITS, greaterThan( 0L ) );
			  AssertMetrics( "Metrics report should include page cache flushes", PC_FLUSHES, greaterThanOrEqualTo( 0L ) );
			  AssertMetrics( "Metrics report should include page cache exceptions", PC_EVICTION_EXCEPTIONS, equalTo( 0L ) );

			  assertEventually( "Metrics report should include page cache hit ratio", () => readDoubleValue(metricsCsv(_metricsDirectory, PC_HIT_RATIO)), lessThanOrEqualTo(1.0), 5, SECONDS );

			  assertEventually( "Metrics report should include page cache usage ratio", () => readDoubleValue(metricsCsv(_metricsDirectory, PC_USAGE_RATIO)), lessThanOrEqualTo(1.0), 5, SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertMetrics(String message, String metricName, org.hamcrest.Matcher<long> matcher) throws Exception
		 private void AssertMetrics( string message, string metricName, Matcher<long> matcher )
		 {
			  assertEventually( message, () => readLongValue(metricsCsv(_metricsDirectory, metricName)), matcher, 5, SECONDS );
		 }
	}

}