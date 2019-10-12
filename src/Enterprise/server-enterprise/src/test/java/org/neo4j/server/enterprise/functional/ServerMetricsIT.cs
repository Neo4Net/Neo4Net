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
namespace Neo4Net.Server.enterprise.functional
{
	using Client = com.sun.jersey.api.client.Client;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using MetricsSettings = Neo4Net.metrics.MetricsSettings;
	using ServerMetrics = Neo4Net.metrics.source.server.ServerMetrics;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class ServerMetricsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory folder = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Folder = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowServerMetrics() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowServerMetrics()
		 {
			  // Given
			  File metrics = Folder.file( "metrics" );
			  NeoServer server = EnterpriseServerBuilder.serverOnRandomPorts().usingDataDir(Folder.databaseDir().AbsolutePath).withProperty(MetricsSettings.metricsEnabled.name(), "true").withProperty(MetricsSettings.csvEnabled.name(), "true").withProperty(MetricsSettings.csvPath.name(), metrics.Path).withProperty(MetricsSettings.csvInterval.name(), "100ms").persistent().build();
			  try
			  {
					// when
					server.Start();

					string host = "http://localhost:" + server.BaseUri().Port + ServerSettings.rest_api_path.DefaultValue + "/transaction/commit";

					for ( int i = 0; i < 5; i++ )
					{
						 ClientResponse r = Client.create().resource(host).accept(APPLICATION_JSON).type(APPLICATION_JSON).post(typeof(ClientResponse), "{ 'statements': [ { 'statement': 'CREATE ()' } ] }");
						 assertEquals( 200, r.Status );
					}

					// then
					AssertMetricsExists( metrics, ServerMetrics.THREAD_JETTY_ALL );
					AssertMetricsExists( metrics, ServerMetrics.THREAD_JETTY_IDLE );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertMetricsExists(java.io.File metricsPath, String metricsName) throws InterruptedException
		 private static void AssertMetricsExists( File metricsPath, string metricsName )
		 {
			  File file = metricsCsv( metricsPath, metricsName );
			  assertEventually( () => ThreadCountReader(file), greaterThan(0L), 1, TimeUnit.MINUTES );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static System.Nullable<long> threadCountReader(java.io.File file) throws InterruptedException
		 private static long? ThreadCountReader( File file )
		 {
			  try
			  {
					return readLongValue( file );
			  }
			  catch ( IOException io )
			  {
					throw new UncheckedIOException( io );
			  }
		 }
	}

}