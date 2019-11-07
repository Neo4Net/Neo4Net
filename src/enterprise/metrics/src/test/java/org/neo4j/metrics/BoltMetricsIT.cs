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
namespace Neo4Net.metrics
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.MESSAGES_DONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.MESSAGES_RECEIVED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.MESSAGES_STARTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.SESSIONS_STARTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.TOTAL_PROCESSING_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.BoltMetrics.TOTAL_QUEUE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.PortUtils.getBoltPort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class BoltMetricsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseAPI _db;
		 private TransportConnection _conn;

		 private readonly TransportTestUtil _util = new TransportTestUtil( new Neo4NetPackV1() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMonitorBolt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorBolt()
		 {
			  // Given
			  File metricsFolder = TestDirectory.directory( "metrics" );
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig((new BoltConnector("bolt")).type, "BOLT").setConfig((new BoltConnector("bolt")).enabled, "true").setConfig((new BoltConnector("bolt")).listen_address, "localhost:0").setConfig(GraphDatabaseSettings.auth_enabled, "false").setConfig(MetricsSettings.MetricsEnabled, "false").setConfig(MetricsSettings.BoltMessagesEnabled, "true").setConfig(MetricsSettings.CsvEnabled, "true").setConfig(MetricsSettings.CsvInterval, "100ms").setConfig(MetricsSettings.CsvPath, metricsFolder.AbsolutePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  // When
			  _conn = ( new SocketConnection() ).connect(new HostnamePort("localhost", getBoltPort(_db))).send(_util.acceptedVersions(1, 0, 0, 0)).send(_util.chunk(new InitMessage("TestClient", map("scheme", "basic", "principal", "Neo4Net", "credentials", "Neo4Net"))));

			  // Then
			  assertEventually( "session shows up as started", () => readLongValue(metricsCsv(metricsFolder, SESSIONS_STARTED)), equalTo(1L), 5, SECONDS );
			  assertEventually( "init request shows up as received", () => readLongValue(metricsCsv(metricsFolder, MESSAGES_RECEIVED)), equalTo(1L), 5, SECONDS );
			  assertEventually( "init request shows up as started", () => readLongValue(metricsCsv(metricsFolder, MESSAGES_STARTED)), equalTo(1L), 5, SECONDS );
			  assertEventually( "init request shows up as done", () => readLongValue(metricsCsv(metricsFolder, MESSAGES_DONE)), equalTo(1L), 5, SECONDS );

			  assertEventually( "queue time shows up", () => readLongValue(metricsCsv(metricsFolder, TOTAL_QUEUE_TIME)), greaterThanOrEqualTo(0L), 5, SECONDS );
			  assertEventually( "processing time shows up", () => readLongValue(metricsCsv(metricsFolder, TOTAL_PROCESSING_TIME)), greaterThanOrEqualTo(0L), 5, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  _conn.disconnect();
			  _db.shutdown();
		 }
	}

}