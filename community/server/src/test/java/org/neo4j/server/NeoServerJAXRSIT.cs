﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server
{
	using DummyThirdPartyWebService = Org.Dummy.Web.Service.DummyThirdPartyWebService;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using CommunityServerBuilder = Org.Neo4j.Server.helpers.CommunityServerBuilder;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using ServerHelper = Org.Neo4j.Server.helpers.ServerHelper;
	using Transactor = Org.Neo4j.Server.helpers.Transactor;
	using JaxRsResponse = Org.Neo4j.Server.rest.JaxRsResponse;
	using RestRequest = Org.Neo4j.Server.rest.RestRequest;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.FunctionalTestHelper.CLIENT;

	public class NeoServerJAXRSIT : ExclusiveServerTestBase
	{
		 private NeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase()
		 public virtual void CleanTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( _server );
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
//ORIGINAL LINE: @Test public void shouldMakeJAXRSClassesAvailableViaHTTP() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeJAXRSClassesAvailableViaHTTP()
		 {
			  CommunityServerBuilder builder = CommunityServerBuilder.server();
			  _server = ServerHelper.createNonPersistentServer( builder );
			  FunctionalTestHelper functionalTestHelper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = ( new RestRequest() ).get(functionalTestHelper.ManagementUri());
			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadThirdPartyJaxRsClasses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadThirdPartyJaxRsClasses()
		 {
			  _server = CommunityServerBuilder.serverOnRandomPorts().withThirdPartyJaxRsPackage("org.dummy.web.service", DummyThirdPartyWebService.DUMMY_WEB_SERVICE_MOUNT_POINT).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  URI thirdPartyServiceUri = ( new URI( _server.baseUri().ToString() + DummyThirdPartyWebService.DUMMY_WEB_SERVICE_MOUNT_POINT ) ).normalize();
			  string response = CLIENT.resource( thirdPartyServiceUri.ToString() ).get(typeof(string));
			  assertEquals( "hello", response );

			  // Assert that extensions gets initialized
			  int nodesCreated = CreateSimpleDatabase( _server.Database.Graph );
			  thirdPartyServiceUri = ( new URI( _server.baseUri().ToString() + DummyThirdPartyWebService.DUMMY_WEB_SERVICE_MOUNT_POINT + "/inject-test" ) ).normalize();
			  response = CLIENT.resource( thirdPartyServiceUri.ToString() ).get(typeof(string));
			  assertEquals( nodesCreated.ToString(), response );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private int createSimpleDatabase(final org.neo4j.kernel.internal.GraphDatabaseAPI graph)
		 private int CreateSimpleDatabase( GraphDatabaseAPI graph )
		 {
			  const int numberOfNodes = 10;
			  (new Transactor(graph, () =>
			  {
				for ( int i = 0; i < numberOfNodes; i++ )
				{
					 graph.CreateNode();
				}

				foreach ( Node n1 in graph.AllNodes )
				{
					 foreach ( Node n2 in graph.AllNodes )
					 {
						  if ( n1.Equals( n2 ) )
						  {
								continue;
						  }

						  n1.createRelationshipTo( n2, RelationshipType.withName( "REL" ) );
					 }
				}
			  })).execute();

			  return numberOfNodes;
		 }
	}

}