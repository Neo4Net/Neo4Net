using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.rest
{
	using MatcherAssert = org.hamcrest.MatcherAssert;
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using Database = Neo4Net.Server.database.Database;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JmxService = Neo4Net.Server.rest.management.JmxService;
	using RootService = Neo4Net.Server.rest.management.RootService;
	using VersionAndEditionService = Neo4Net.Server.rest.management.VersionAndEditionService;
	using ConsoleService = Neo4Net.Server.rest.management.console.ConsoleService;
	using ConsoleSessionFactory = Neo4Net.Server.rest.management.console.ConsoleSessionFactory;
	using ScriptSession = Neo4Net.Server.rest.management.console.ScriptSession;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;
	using UTF8 = Neo4Net.Strings.UTF8;
	using Neo4Net.Test;
	using EntityOutputFormat = Neo4Net.Test.server.EntityOutputFormat;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class ManageNodeIT : AbstractRestFunctionalDocTestBase
	{
		 private const long NON_EXISTENT_NODE_ID = 999999;
		 private static string _nodeUriPattern = "^.*/node/[0-9]+$";

		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_node()
		 public virtual void CreateNode()
		 {
			  JaxRsResponse response = GenConflict.get().expectedStatus(201).expectedHeader("Location").post(_functionalTestHelper.nodeUri()).response();
			  assertTrue( response.Location.ToString().matches(_nodeUriPattern) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_node_with_properties()
		 public virtual void CreateNodeWithProperties()
		 {
			  JaxRsResponse response = GenConflict.get().payload("{\"foo\" : \"bar\"}").expectedStatus(201).expectedHeader("Location").expectedHeader("Content-Length").post(_functionalTestHelper.nodeUri()).response();
			  assertTrue( response.Location.ToString().matches(_nodeUriPattern) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_node_with_array_properties()
		 public virtual void CreateNodeWithArrayProperties()
		 {
			  string response = GenConflict.get().payload("{\"foo\" : [1,2,3]}").expectedStatus(201).expectedHeader("Location").expectedHeader("Content-Length").post(_functionalTestHelper.nodeUri()).response().Entity;
			  assertThat( response, containsString( "[ 1, 2, 3 ]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Property values can not be null.\n" + "\n" + "This example shows the response you get when trying to set a property to +null+.") @Test public void shouldGet400WhenSupplyingNullValueForAProperty()
		 [Documented("Property values can not be null.\n" + "\n" + "This example shows the response you get when trying to set a property to +null+.")]
		 public virtual void ShouldGet400WhenSupplyingNullValueForAProperty()
		 {
			  GenConflict.get().payload("{\"foo\":null}").expectedStatus(400).post(_functionalTestHelper.nodeUri());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet400WhenCreatingNodeMalformedProperties()
		 public virtual void ShouldGet400WhenCreatingNodeMalformedProperties()
		 {
			  JaxRsResponse response = SendCreateRequestToServer( "this:::isNot::JSON}" );
			  assertEquals( 400, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet400WhenCreatingNodeUnsupportedNestedPropertyValues()
		 public virtual void ShouldGet400WhenCreatingNodeUnsupportedNestedPropertyValues()
		 {
			  JaxRsResponse response = SendCreateRequestToServer( "{\"foo\" : {\"bar\" : \"baz\"}}" );
			  assertEquals( 400, response.Status );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse sendCreateRequestToServer(final String json)
		 private JaxRsResponse SendCreateRequestToServer( string json )
		 {
			  return RestRequest.Req().post(_functionalTestHelper.dataUri() + "node/", json);
		 }

		 private JaxRsResponse SendCreateRequestToServer()
		 {
			  return RestRequest.Req().post(_functionalTestHelper.dataUri() + "node/", null, MediaType.APPLICATION_JSON_TYPE);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetValidLocationHeaderWhenCreatingNode()
		 public virtual void ShouldGetValidLocationHeaderWhenCreatingNode()
		 {
			  JaxRsResponse response = SendCreateRequestToServer();
			  assertNotNull( response.Location );
			  assertTrue( response.Location.ToString().StartsWith(_functionalTestHelper.dataUri() + "node/", StringComparison.Ordinal) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetASingleContentLengthHeaderWhenCreatingANode()
		 public virtual void ShouldGetASingleContentLengthHeaderWhenCreatingANode()
		 {
			  JaxRsResponse response = SendCreateRequestToServer();
			  IList<string> contentLengthHeaders = response.Headers.get( "Content-Length" );
			  assertNotNull( contentLengthHeaders );
			  assertEquals( 1, contentLengthHeaders.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeJSONContentTypeOnResponse()
		 public virtual void ShouldBeJSONContentTypeOnResponse()
		 {
			  JaxRsResponse response = SendCreateRequestToServer();
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetValidNodeRepresentationWhenCreatingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetValidNodeRepresentationWhenCreatingNode()
		 {
			  JaxRsResponse response = SendCreateRequestToServer();
			  string entity = response.Entity;

			  IDictionary<string, object> map = JsonHelper.jsonToMap( entity );

			  assertNotNull( map );
			  assertTrue( map.ContainsKey( "self" ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Delete node.") @Test public void shouldRespondWith204WhenNodeDeleted()
		 [Documented("Delete node.")]
		 public virtual void ShouldRespondWith204WhenNodeDeleted()
		 {
			  long node = _helper.createNode();
			  GenConflict.get().expectedStatus(204).delete(_functionalTestHelper.dataUri() + "node/" + node);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404AndSensibleEntityBodyWhenNodeToBeDeletedCannotBeFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404AndSensibleEntityBodyWhenNodeToBeDeletedCannotBeFound()
		 {
			  JaxRsResponse response = SendDeleteRequestToServer( NON_EXISTENT_NODE_ID );
			  assertEquals( 404, response.Status );

			  IDictionary<string, object> jsonMap = JsonHelper.jsonToMap( response.Entity );
			  assertThat( jsonMap, hasKey( "message" ) );
			  assertNotNull( jsonMap["message"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Nodes with relationships cannot be deleted.\n" + "\n" + "The relationships on a node has to be deleted before the node can be\n" + "deleted.\n" + " \n" + "TIP: You can use `DETACH DELETE` in Cypher to delete nodes and their relationships in one go.") @Test public void shouldRespondWith409AndSensibleEntityBodyWhenNodeCannotBeDeleted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Nodes with relationships cannot be deleted.\n" + "\n" + "The relationships on a node has to be deleted before the node can be\n" + "deleted.\n" + " \n" + "TIP: You can use `DETACH DELETE` in Cypher to delete nodes and their relationships in one go.")]
		 public virtual void ShouldRespondWith409AndSensibleEntityBodyWhenNodeCannotBeDeleted()
		 {
			  long id = _helper.createNode();
			  _helper.createRelationship( "LOVES", id, _helper.createNode() );
			  JaxRsResponse response = SendDeleteRequestToServer( id );
			  assertEquals( 409, response.Status );
			  IDictionary<string, object> jsonMap = JsonHelper.jsonToMap( response.Entity );
			  assertThat( jsonMap, hasKey( "message" ) );
			  assertNotNull( jsonMap["message"] );

			  GenConflict.get().expectedStatus(409).delete(_functionalTestHelper.dataUri() + "node/" + id);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400IfInvalidJsonSentAsNodePropertiesDuringNodeCreation()
		 public virtual void ShouldRespondWith400IfInvalidJsonSentAsNodePropertiesDuringNodeCreation()
		 {
			  string mangledJsonArray = "{\"myprop\":[1,2,\"three\"]}";
			  JaxRsResponse response = SendCreateRequestToServer( mangledJsonArray );
			  assertEquals( 400, response.Status );
			  assertEquals( "text/plain", response.Type.ToString() );
			  assertThat( response.Entity, containsString( mangledJsonArray ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400IfInvalidJsonSentAsNodeProperty()
		 public virtual void ShouldRespondWith400IfInvalidJsonSentAsNodeProperty()
		 {
			  URI nodeLocation = SendCreateRequestToServer().Location;

			  string mangledJsonArray = "[1,2,\"three\"]";
			  JaxRsResponse response = RestRequest.Req().put(nodeLocation.ToString() + "/properties/myprop", mangledJsonArray);
			  assertEquals( 400, response.Status );
			  assertEquals( "text/plain", response.Type.ToString() );
			  assertThat( response.Entity, containsString( mangledJsonArray ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400IfInvalidJsonSentAsNodeProperties()
		 public virtual void ShouldRespondWith400IfInvalidJsonSentAsNodeProperties()
		 {
			  URI nodeLocation = SendCreateRequestToServer().Location;

			  string mangledJsonProperties = "{\"a\":\"b\", \"c\":[1,2,\"three\"]}";
			  JaxRsResponse response = RestRequest.Req().put(nodeLocation.ToString() + "/properties", mangledJsonProperties);
			  assertEquals( 400, response.Status );
			  assertEquals( "text/plain", response.Type.ToString() );
			  assertThat( response.Entity, containsString( mangledJsonProperties ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse sendDeleteRequestToServer(final long id)
		 private JaxRsResponse SendDeleteRequestToServer( long id )
		 {
			  return RestRequest.Req().delete(_functionalTestHelper.dataUri() + "node/" + id);
		 }

		 /*
		     Note that when running this test from within an IDE, the version field will be an empty string. This is because the
		     code that generates the version identifier is written by Maven as part of the build process(!). The tests will pass
		     both in the IDE (where the empty string will be correctly compared).
		      */
		 public class CommunityVersionAndEditionServiceDocIT : ExclusiveServerTestBase
		 {
			  internal static NeoServer Server;
			  internal static FunctionalTestHelper FunctionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.junit.rules.TemporaryFolder staticFolder = new org.junit.rules.TemporaryFolder();
			  public static TemporaryFolder StaticFolder = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.TestData<RESTRequestGenerator> gen = org.neo4j.test.TestData.producedThrough(RESTRequestGenerator.PRODUCER);
			  public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public static void SetupServer()
			  {
					Server = CommunityServerBuilder.server().usingDataDir(StaticFolder.Root.AbsolutePath).build();

					suppressAll().call((Callable<Void>)() =>
					{
					 Server.start();
					 return null;
					});
					FunctionalTestHelper = new FunctionalTestHelper( Server );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
			  public virtual void SetupTheDatabase()
			  {
					// do nothing, we don't care about the database contents here
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public static void StopServer()
			  {
					suppressAll().call((Callable<Void>)() =>
					{
					 Server.stop();
					 return null;
					});
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCommunityEdition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldReportCommunityEdition()
			  {
					// Given
					string releaseVersion = Server.Database.Graph.DependencyResolver.resolveDependency( typeof( KernelData ) ).version().ReleaseVersion;

					// When
					HTTP.Response res = HTTP.GET( FunctionalTestHelper.managementUri() + "/" + VersionAndEditionService.SERVER_PATH );

					// Then
					assertEquals( 200, res.Status() );
					assertThat( res.Get( "edition" ).asText(), equalTo("community") );
					assertThat( res.Get( "version" ).asText(), equalTo(releaseVersion) );
			  }
		 }

		 public class ConfigureEnabledManagementConsolesDocIT : ExclusiveServerTestBase
		 {
			  internal NeoServer Server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopTheServer()
			  public virtual void StopTheServer()
			  {
					Server.stop();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToExplicitlySetConsolesToEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldBeAbleToExplicitlySetConsolesToEnabled()
			  {
					Server = CommunityServerBuilder.server().withProperty(ServerSettings.console_module_engines.name(), "").usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
					Server.start();

					assertThat( Exec( "ls", "shell" ).Status, @is( 400 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shellConsoleShouldBeEnabledByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShellConsoleShouldBeEnabledByDefault()
			  {
					Server = CommunityServerBuilder.server().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
					Server.start();

					assertThat( Exec( "ls", "shell" ).Status, @is( 200 ) );
			  }

			  internal virtual JaxRsResponse Exec( string command, string engine )
			  {
					return RestRequest.Req().post(Server.baseUri() + "db/manage/server/console", "{" + "\"engine\":\"" + engine + "\"," + "\"command\":\"" + command + "\\n\"}");
			  }
		 }

		 public class ConsoleServiceDocTest
		 {
			  internal readonly URI Uri = URI.create( "http://peteriscool.com:6666/" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctRepresentation()
			  public virtual void CorrectRepresentation()
			  {
					ConsoleService consoleService = new ConsoleService( new StubConsoleSessionFactory(), mock(typeof(Database)), NullLogProvider.Instance, new OutputFormat(new JsonFormat(), Uri, null) );

					Response consoleResponse = consoleService.ServiceDefinition;

					assertEquals( 200, consoleResponse.Status );
					string response = Decode( consoleResponse );
					MatcherAssert.assertThat( response, containsString( "resources" ) );
					MatcherAssert.assertThat( response, containsString( Uri.ToString() ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void advertisesAvailableConsoleEngines()
			  public virtual void AdvertisesAvailableConsoleEngines()
			  {
					ConsoleService consoleServiceWithJustShellEngine = new ConsoleService( new StubConsoleSessionFactory(), mock(typeof(Database)), NullLogProvider.Instance, new OutputFormat(new JsonFormat(), Uri, null) );

					string response = Decode( consoleServiceWithJustShellEngine.ServiceDefinition );

					MatcherAssert.assertThat( response, containsString( "\"engines\" : [ \"stub-engine\" ]" ) );

			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String decode(final javax.ws.rs.core.Response response)
			  internal virtual string Decode( Response response )
			  {
					return UTF8.decode( ( sbyte[] ) response.Entity );
			  }

			  private class StubConsoleSessionFactory : ConsoleSessionFactory
			  {
					public override ScriptSession CreateSession( string engineName, Database database, LogProvider logProvider )
					{
						 return null;
					}

					public override IEnumerable<string> SupportedEngines()
					{
						 return Collections.singletonList( "stub-engine" );
					}
			  }
		 }

		 public class JmxServiceDocTest
		 {
			  public JmxService JmxService;
			  internal readonly URI Uri = URI.create( "http://peteriscool.com:6666/" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctRepresentation()
			  public virtual void CorrectRepresentation()
			  {
					Response resp = JmxService.ServiceDefinition;

					assertEquals( 200, resp.Status );

					string json = UTF8.decode( ( sbyte[] ) resp.Entity );
					MatcherAssert.assertThat( json, containsString( "resources" ) );
					MatcherAssert.assertThat( json, containsString( Uri.ToString() ) );
					MatcherAssert.assertThat( json, containsString( "jmx/domain/{domain}/{objectName}" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListDomainsCorrectly()
			  public virtual void ShouldListDomainsCorrectly()
			  {
					Response resp = JmxService.listDomains();

					assertEquals( 200, resp.Status );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testwork()
			  public virtual void Testwork()
			  {
					JmxService.queryBeans( "[\"*:*\"]" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
			  public virtual void SetUp()
			  {
					this.JmxService = new JmxService( new OutputFormat( new JsonFormat(), Uri, null ), null );
			  }

		 }

		 public class RootServiceDocTest
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseServicesWhenAsked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldAdvertiseServicesWhenAsked()
			  {
					UriInfo uriInfo = mock( typeof( UriInfo ) );
					URI uri = new URI( "http://example.org:7474/" );
					when( uriInfo.BaseUri ).thenReturn( uri );

					RootService svc = new RootService( new CommunityNeoServer( Config.defaults( stringMap( ( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).type.name(), "HTTP", (new HttpConnector("http", HttpConnector.Encryption.NONE)).enabled.name(), "true" ) ), GraphDatabaseDependencies.newDependencies().userLogProvider(NullLogProvider.Instance).monitors(new Monitors()) ) );

					EntityOutputFormat output = new EntityOutputFormat( new JsonFormat(), null, null );
					Response serviceDefinition = svc.GetServiceDefinition( uriInfo, output );

					assertEquals( 200, serviceDefinition.Status );
					IDictionary<string, object> result = ( IDictionary<string, object> ) output.ResultAsMap["services"];

					assertThat( result["console"].ToString(), containsString(string.Format("{0}server/console", uri.ToString())) );
					assertThat( result["jmx"].ToString(), containsString(string.Format("{0}server/jmx", uri.ToString())) );
			  }
		 }

		 public class VersionAndEditionServiceTest
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnReadableStringForServiceName()
			  public virtual void ShouldReturnReadableStringForServiceName()
			  {
					// given
					VersionAndEditionService service = new VersionAndEditionService( mock( typeof( CommunityNeoServer ) ) );

					// when
					string serviceName = service.Name;
					// then
					assertEquals( "version", serviceName );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSensiblePathWhereServiceIsHosted()
			  public virtual void ShouldReturnSensiblePathWhereServiceIsHosted()
			  {
					// given
					VersionAndEditionService service = new VersionAndEditionService( mock( typeof( CommunityNeoServer ) ) );

					// when
					string serverPath = service.ServerPath;

					// then
					assertEquals( "server/version", serverPath );
			  }
		 }
	}

}