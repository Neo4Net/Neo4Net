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
namespace Neo4Net.Server.plugins
{
	using ClientHandlerException = com.sun.jersey.api.client.ClientHandlerException;
	using UniformInterfaceException = com.sun.jersey.api.client.UniformInterfaceException;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CloneSubgraphPluginTestIT : ExclusiveServerTestBase
	{
		 private static readonly RelationshipType _knows = RelationshipType.withName( "knows" );
		 private static readonly RelationshipType _workedFor = RelationshipType.withName( "worked_for" );

		 private static NeoServer _server;
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetupServer()
		 {
			  _server = ServerHelper.createNonPersistentServer();
			  _functionalTestHelper = new FunctionalTestHelper( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void shutdownServer()
		 public static void ShutdownServer()
		 {
			  try
			  {
					if ( _server != null )
					{
						 _server.stop();
					}
			  }
			  finally
			  {
					_server = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( _server );
			  CreateASocialNetwork( _server.Database.Graph );
		 }

		 private Node _jw;

		 private void CreateASocialNetwork( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					_jw = Db.createNode();
					_jw.setProperty( "name", "jim" );
					Node sp = Db.createNode();
					sp.SetProperty( "name", "savas" );
					Node bg = Db.createNode();
					bg.SetProperty( "name", "bill" );
					Node th = Db.createNode();
					th.SetProperty( "name", "tony" );
					Node rj = Db.createNode();
					rj.SetProperty( "name", "rhodri" );
					rj.SetProperty( "hobby", "family" );
					Node nj = Db.createNode();
					nj.SetProperty( "name", "ned" );
					nj.SetProperty( "hobby", "cs" );
					Node ml = Db.createNode();
					ml.SetProperty( "name", "mark" );
					Node mf = Db.createNode();
					mf.SetProperty( "name", "martin" );
					Node rp = Db.createNode();
					rp.SetProperty( "name", "rebecca" );
					Node rs = Db.createNode();
					rs.SetProperty( "name", "roy" );
					Node sc = Db.createNode();
					sc.SetProperty( "name", "steve" );
					sc.SetProperty( "hobby", "cloud" );
					Node sw = Db.createNode();
					sw.SetProperty( "name", "stuart" );
					sw.SetProperty( "hobby", "cs" );

					_jw.createRelationshipTo( sp, _knows );
					_jw.createRelationshipTo( mf, _knows );
					_jw.createRelationshipTo( rj, _knows );
					rj.CreateRelationshipTo( nj, _knows );

					mf.CreateRelationshipTo( rp, _knows );
					mf.CreateRelationshipTo( rs, _knows );

					sp.CreateRelationshipTo( bg, _knows );
					sp.CreateRelationshipTo( th, _knows );
					sp.CreateRelationshipTo( mf, _knows );
					sp.CreateRelationshipTo( ml, _workedFor );

					ml.CreateRelationshipTo( sc, _knows );
					ml.CreateRelationshipTo( sw, _knows );

					_jw.setProperty( "hobby", "cs" );
					sp.SetProperty( "hobby", "cs" );
					bg.SetProperty( "hobby", "cs" );
					ml.SetProperty( "hobby", "cs" );
					mf.SetProperty( "hobby", "cs" );

					rp.SetProperty( "hobby", "lisp" );
					rs.SetProperty( "hobby", "socialism" );
					th.SetProperty( "hobby", "fishing" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseExtensionThatPluginCreates() throws org.neo4j.server.rest.domain.JsonParseException, com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdvertiseExtensionThatPluginCreates()
		 {
			  int originalCount = NodeCount();

			  // Find the start node URI from the server
			  JaxRsResponse response = ( new RestRequest() ).get(_functionalTestHelper.dataUri() + "node/1");

			  string entity = response.Entity;

			  IDictionary<string, object> map = JsonHelper.jsonToMap( entity );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.HashMap<?, ?> extensionsMap = (java.util.HashMap<?, ?>) map.get("extensions");
			  Dictionary<object, ?> extensionsMap = ( Dictionary<object, ?> ) map["extensions"];

			  assertNotNull( extensionsMap );
			  assertFalse( extensionsMap.Count == 0 );

			  const string graphClonerKey = "GraphCloner";
			  assertTrue( extensionsMap.Keys.Contains( graphClonerKey ) );

			  const string cloneSubgraphKey = "clonedSubgraph";
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: String clonedSubgraphUri = (String)((java.util.HashMap<?, ?>) extensionsMap.get(GRAPH_CLONER_KEY)).get(CLONE_SUBGRAPH_KEY);
			  string clonedSubgraphUri = ( string )( ( Dictionary<object, ?> ) extensionsMap[graphClonerKey] )[cloneSubgraphKey];
			  assertNotNull( clonedSubgraphUri );

			  const string cloneDepthMuchLargerThanTheGraph = "99";
			  response.Close();
			  response = ( new RestRequest() ).post(clonedSubgraphUri, "depth=" + cloneDepthMuchLargerThanTheGraph, MediaType.APPLICATION_FORM_URLENCODED_TYPE);

			  assertEquals( response.Entity, 200, response.Status );

			  int doubleTheNumberOfNodes = originalCount * 2;
			  assertEquals( doubleTheNumberOfNodes, NodeCount() );
		 }

		 private int NodeCount()
		 {
			  using ( Transaction ignore = _server.Database.Graph.beginTx() )
			  {
					return Math.toIntExact( _server.Database.Graph.AllNodes.Count() );
			  }
		 }
	}

}