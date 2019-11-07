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
namespace Neo4Net.Server
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using PrettyJSON = Neo4Net.Server.rest.PrettyJSON;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.dummy.web.service.DummyThirdPartyWebService.DUMMY_WEB_SERVICE_MOUNT_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.helpers.CommunityServerBuilder.serverOnRandomPorts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToList;

	public class BatchOperationHeaderIT : ExclusiveServerTestBase
	{
		 private NeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder folder = new org.junit.rules.TemporaryFolder();
		 public new TemporaryFolder Folder = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanTheDatabase()
		 {
			  _server = serverOnRandomPorts().withThirdPartyJaxRsPackage("org.dummy.web.service", DUMMY_WEB_SERVICE_MOUNT_POINT).usingDataDir(Folder.Root.AbsolutePath).build();
			  _server.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopServer()
		 public virtual void StopServer()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassHeaders()
		 {
			  int httpPort = LocalHttpPort;

			  string jsonData = ( new PrettyJSON() ).array().@object().key("method").value("GET").key("to").value("../.." + DUMMY_WEB_SERVICE_MOUNT_POINT + "/needs-auth-header").key("body").@object().endObject().endObject().endArray().ToString();

			  JaxRsResponse response = ( new RestRequest( null, "user", "pass" ) ).post( _server.baseUri() + "db/data/batch", jsonData );

			  assertEquals( 200, response.Status );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.util.Map<String, Object>> responseData = jsonToList(response.getEntity());
			  IList<IDictionary<string, object>> responseData = jsonToList( response.Entity );

			  IDictionary<string, object> res = ( IDictionary<string, object> ) responseData[0]["body"];

			  /*
			   * {
			   *   Accept=[application/json],
			   *   Content-Type=[application/json],
			   *   Authorization=[Basic dXNlcjpwYXNz],
			   *   User-Agent=[Java/1.6.0_27] <-- ignore that, it changes often
			   *   Host=[localhost:7474],
			   *   Connection=[keep-alive],
			   *   Content-Length=[86]
			   * }
			   */
			  assertEquals( "Basic dXNlcjpwYXNz", res["Authorization"] );
			  assertEquals( "application/json", res["Accept"] );
			  assertEquals( "application/json", res["Content-Type"] );
			  assertEquals( "localhost:" + httpPort, res["Host"] );
			  assertEquals( "keep-alive", res["Connection"] );
		 }

		 private int LocalHttpPort
		 {
			 get
			 {
				  ConnectorPortRegister connectorPortRegister = _server.Database.Graph.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
				  return connectorPortRegister.GetLocalAddress( "http" ).Port;
			 }
		 }
	}

}