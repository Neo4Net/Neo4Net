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
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.prometheusEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.prometheusEndpoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.EntityCountMetrics.COUNTS_NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.source.db.EntityCountMetrics.COUNTS_RELATIONSHIP_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.PortUtils.getConnectorAddress;

	public class PrometheusOutputIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private IGraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(prometheusEnabled, Settings.TRUE).setConfig(prometheusEndpoint, "localhost:0").newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void httpEndpointShouldBeAvailableAndResponsive() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HttpEndpointShouldBeAvailableAndResponsive()
		 {
			  string url = "http://" + getConnectorAddress( ( GraphDatabaseAPI ) _database, "prometheus" ) + "/metrics";
			  URLConnection connection = ( new URL( url ) ).openConnection();
			  connection.DoOutput = true;
			  connection.connect();
			  Scanner s = ( new Scanner( connection.InputStream, "UTF-8" ) ).useDelimiter( "\\A" );

			  assertTrue( s.hasNext() );
			  string response = s.next();
			  assertTrue( response.Contains( COUNTS_NODE ) );
			  assertTrue( response.Contains( COUNTS_RELATIONSHIP_TYPE ) );
		 }
	}

}