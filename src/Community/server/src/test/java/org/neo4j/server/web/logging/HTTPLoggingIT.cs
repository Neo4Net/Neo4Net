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
namespace Neo4Net.Server.web.logging
{
	using HttpStatus = org.apache.http.HttpStatus;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using TestName = org.junit.rules.TestName;


	using Neo4Net.Functions;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.readTextFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class HTTPLoggingIT : ExclusiveServerTestBase
	{
		private bool InstanceFieldsInitialized = false;

		public HTTPLoggingIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _exception ).around( _testName ).around( _testDirectory );
		}


		 private readonly ExpectedException _exception = ExpectedException.none();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly TestName _testName = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(exception).around(testName).around(testDirectory);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenExplicitlyDisabledServerLoggingConfigurationShouldNotLogAccesses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenExplicitlyDisabledServerLoggingConfigurationShouldNotLogAccesses()
		 {
			  // given
			  string directoryPrefix = _testName.MethodName;
			  File logDirectory = _testDirectory.directory( directoryPrefix + "-logdir" );

			  NeoServer server = serverOnRandomPorts().withDefaultDatabaseTuning().persistent().withProperty(ServerSettings.http_logging_enabled.name(), Settings.FALSE).withProperty(GraphDatabaseSettings.logs_directory.name(), logDirectory.ToString()).withProperty((new BoltConnector("bolt")).listen_address.name(), ":0").usingDataDir(_testDirectory.directory(directoryPrefix + "-dbdir").AbsolutePath).build();
			  try
			  {
					server.Start();
					FunctionalTestHelper functionalTestHelper = new FunctionalTestHelper( server );

					// when
					string query = "?implicitlyDisabled" + RandomString();
					JaxRsResponse response = ( new RestRequest() ).get(functionalTestHelper.ManagementUri() + query);

					assertThat( response.Status, @is( HttpStatus.SC_OK ) );
					response.Close();

					// then
					File httpLog = new File( logDirectory, "http.log" );
					assertThat( httpLog.exists(), @is(false) );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenExplicitlyEnabledServerLoggingConfigurationShouldLogAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenExplicitlyEnabledServerLoggingConfigurationShouldLogAccess()
		 {
			  // given
			  string directoryPrefix = _testName.MethodName;
			  File logDirectory = _testDirectory.directory( directoryPrefix + "-logdir" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String query = "?explicitlyEnabled=" + randomString();
			  string query = "?explicitlyEnabled=" + RandomString();

			  NeoServer server = serverOnRandomPorts().withDefaultDatabaseTuning().persistent().withProperty(ServerSettings.http_logging_enabled.name(), Settings.TRUE).withProperty(GraphDatabaseSettings.logs_directory.name(), logDirectory.AbsolutePath).withProperty((new BoltConnector("bolt")).listen_address.name(), ":0").usingDataDir(_testDirectory.directory(directoryPrefix + "-dbdir").AbsolutePath).build();
			  try
			  {
					server.Start();

					FunctionalTestHelper functionalTestHelper = new FunctionalTestHelper( server );

					// when
					JaxRsResponse response = ( new RestRequest() ).get(functionalTestHelper.ManagementUri() + query);
					assertThat( response.Status, @is( HttpStatus.SC_OK ) );
					response.Close();

					// then
					File httpLog = new File( logDirectory, "http.log" );
					assertEventually( "request appears in log", FileContentSupplier( httpLog ), containsString( query ), 5, TimeUnit.SECONDS );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.function.ThrowingSupplier<String, java.io.IOException> fileContentSupplier(final java.io.File file)
		 private ThrowingSupplier<string, IOException> FileContentSupplier( File file )
		 {
			  return () => readTextFile(file, StandardCharsets.UTF_8);
		 }

		 private string RandomString()
		 {
			  return System.Guid.randomUUID().ToString();
		 }
	}

}