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
namespace Neo4Net.Server.plugins
{
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using RegExp = Neo4Net.Server.plugins.PluginFunctionalTestHelper.RegExp;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using NodeRepresentationTest = Neo4Net.Server.rest.repr.NodeRepresentationTest;
	using RelationshipRepresentationTest = Neo4Net.Server.rest.repr.RelationshipRepresentationTest;
	using SharedServerTestBase = Neo4Net.Test.server.SharedServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.endsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class PluginFunctionalTestIT extends Neo4Net.test.server.SharedServerTestBase
	public class PluginFunctionalTestIT : SharedServerTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( SharedServerTestBase.server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase()
		 public virtual void CleanTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( SharedServerTestBase.server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetGraphDatabaseExtensionList() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanGetGraphDatabaseExtensionList()
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  assertThat( map["extensions"], instanceOf( typeof( System.Collections.IDictionary ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetExtensionDefinitionForReferenceNodeExtension() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanGetExtensionDefinitionForReferenceNodeExtension()
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  map = ( IDictionary<string, object> ) map["extensions"];

			  assertThat( map[typeof( FunctionalTestPlugin ).Name], instanceOf( typeof( System.Collections.IDictionary ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetExtensionDataForCreateNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanGetExtensionDataForCreateNode()
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  assertThat( ( string ) map[FunctionalTestPlugin.CREATE_NODE], RegExp.EndsWith( string.Format( "/ext/{0}/graphdb/{1}", typeof( FunctionalTestPlugin ).Name, FunctionalTestPlugin.CREATE_NODE ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetExtensionDescription() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanGetExtensionDescription()
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  string uri = ( string ) map[FunctionalTestPlugin.CREATE_NODE];
			  PluginFunctionalTestHelper.MakeGet( uri );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokeExtensionMethodWithNoArguments() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokeExtensionMethodWithNoArguments()
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  string uri = ( string ) map[FunctionalTestPlugin.CREATE_NODE];
			  IDictionary<string, object> description = PluginFunctionalTestHelper.MakePostMap( uri );

			  NodeRepresentationTest.verifySerialisation( description );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokeNodePlugin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokeNodePlugin()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();

			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.nodeUri( n ) );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  string uri = ( string ) map[FunctionalTestPlugin.GET_CONNECTED_NODES];
			  IList<IDictionary<string, object>> response = PluginFunctionalTestHelper.MakePostList( uri );
			  VerifyNodes( response );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void verifyNodes(final java.util.List<java.util.Map<String, Object>> response)
		 private void VerifyNodes( IList<IDictionary<string, object>> response )
		 {
			  foreach ( IDictionary<string, object> nodeMap in response )
			  {
					NodeRepresentationTest.verifySerialisation( nodeMap );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokePluginWithParam() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokePluginWithParam()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();

			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.dataUri() );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  string uri = ( string ) map["methodWithIntParam"];
			  IDictionary<string, object> @params = MapUtil.map( "id", n );
			  IDictionary<string, object> node = PluginFunctionalTestHelper.MakePostMap( uri, @params );

			  NodeRepresentationTest.verifySerialisation( node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokePluginOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokePluginOnRelationship()
		 {
			  long n1 = _functionalTestHelper.GraphDbHelper.createNode();
			  long n2 = _functionalTestHelper.GraphDbHelper.createNode();
			  long relId = _functionalTestHelper.GraphDbHelper.createRelationship( "pals", n1, n2 );

			  string uri = GetPluginMethodUri( _functionalTestHelper.relationshipUri( relId ), "methodOnRelationship" );

			  IDictionary<string, object> @params = MapUtil.map( "id", relId );
			  IList<IDictionary<string, object>> nodes = PluginFunctionalTestHelper.MakePostList( uri, @params );

			  VerifyNodes( nodes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getPluginMethodUri(String startUrl, String methodName) throws Neo4Net.server.rest.domain.JsonParseException
		 private string GetPluginMethodUri( string startUrl, string methodName )
		 {
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( startUrl );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];
			  return ( string ) map[methodName];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToInvokePluginWithLotsOfParams() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToInvokePluginWithLotsOfParams()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithAllParams" );
			  string a = "a";
			  sbyte b = unchecked( ( sbyte ) 0xff );
			  char c = 'c';
			  short d = ( short ) 4;
			  int e = 365;
			  long f = ( long ) 4;
			  float g = ( float ) 4.5;
			  double h = Math.PI;
			  bool i = false;
			  IDictionary<string, object> @params = MapUtil.map( "id", a, "id2", b, "id3", c, "id4", d, "id5", e, "id6", f, "id7", g, "id8", h, "id9", i );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  assertThat( FunctionalTestPlugin._string, @is( a ) );
			  assertThat( FunctionalTestPlugin._byte, @is( b ) );
			  assertThat( FunctionalTestPlugin._character, @is( c ) );
			  assertThat( FunctionalTestPlugin._short, @is( d ) );
			  assertThat( FunctionalTestPlugin._integer, @is( e ) );
			  assertThat( FunctionalTestPlugin._long, @is( f ) );
			  assertThat( FunctionalTestPlugin._float, @is( g ) );
			  assertThat( FunctionalTestPlugin._double, @is( h ) );
			  assertThat( FunctionalTestPlugin._boolean, @is( i ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleOptionalValuesCorrectly1() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleOptionalValuesCorrectly1()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.nodeUri( n ), "getThisNodeOrById" );
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakePostMap( methodUri );
			  NodeRepresentationTest.verifySerialisation( map );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleOptionalValuesCorrectly2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleOptionalValuesCorrectly2()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.nodeUri( n ), "getThisNodeOrById" );
			  long id = _functionalTestHelper.GraphDbHelper.FirstNode;
			  IDictionary<string, object> @params = MapUtil.map( "id", id );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  assertThat( FunctionalTestPlugin.Optional, @is( id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokePluginWithNodeParam() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokePluginWithNodeParam()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();
			  long m = _functionalTestHelper.GraphDbHelper.createNode();
			  _functionalTestHelper.GraphDbHelper.createRelationship( "LOVES", n, m );
			  _functionalTestHelper.GraphDbHelper.createRelationship( "LOVES", m, n );
			  _functionalTestHelper.GraphDbHelper.createRelationship( "KNOWS", m, _functionalTestHelper.GraphDbHelper.createNode() );
			  _functionalTestHelper.GraphDbHelper.createRelationship( "KNOWS", n, _functionalTestHelper.GraphDbHelper.createNode() );

			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.nodeUri( n ) );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];

			  string uri = ( string ) map["getRelationshipsBetween"];
			  IList<IDictionary<string, object>> response = PluginFunctionalTestHelper.MakePostList( uri, MapUtil.map( "other", _functionalTestHelper.nodeUri( m ) ) );
			  assertEquals( 2, response.Count );
			  VerifyRelationships( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokePluginWithNodeListParam() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokePluginWithNodeListParam()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();
			  IDictionary<string, object> map = PluginFunctionalTestHelper.MakeGet( _functionalTestHelper.nodeUri( n ) );
			  map = ( IDictionary<string, object> ) map["extensions"];
			  map = ( IDictionary<string, object> ) map[typeof( FunctionalTestPlugin ).Name];
			  IList<string> nodes = Arrays.asList( _functionalTestHelper.nodeUri( _functionalTestHelper.GraphDbHelper.createNode() ), _functionalTestHelper.nodeUri(_functionalTestHelper.GraphDbHelper.createNode()), _functionalTestHelper.nodeUri(_functionalTestHelper.GraphDbHelper.createNode()) );

			  string uri = ( string ) map["createRelationships"];
			  IList<IDictionary<string, object>> response = PluginFunctionalTestHelper.MakePostList( uri, MapUtil.map( "type", "KNOWS", "nodes", nodes ) );
			  assertEquals( nodes.Count, response.Count );
			  VerifyRelationships( response );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void verifyRelationships(final java.util.List<java.util.Map<String, Object>> response)
		 private void VerifyRelationships( IList<IDictionary<string, object>> response )
		 {
			  foreach ( IDictionary<string, object> relMap in response )
			  {
					RelationshipRepresentationTest.verifySerialisation( relMap );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSets() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleSets()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithSet" );
			  IList<string> strings = Arrays.asList( "aaa", "bbb", "aaa" );
			  IDictionary<string, object> @params = MapUtil.map( "strings", strings );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  ISet<string> stringsSet = new HashSet<string>( strings );

			  assertThat( FunctionalTestPlugin.StringSet, @is( stringsSet ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleJsonLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleJsonLists()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithList" );
			  IList<string> strings = Arrays.asList( "aaa", "bbb", "aaa" );
			  IDictionary<string, object> @params = MapUtil.map( "strings", strings );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  IList<string> stringsList = new List<string>( strings );

			  assertThat( FunctionalTestPlugin.StringList, @is( stringsList ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUrlEncodedLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUrlEncodedLists()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithList" );

			  string postBody = "strings[]=aaa&strings[]=bbb&strings[]=ccc";

			  RestRequest.req().post(methodUri,postBody,MediaType.APPLICATION_FORM_URLENCODED_TYPE);

			  IList<string> strings = Arrays.asList( "aaa", "bbb", "ccc" );

			  IList<string> stringsList = new List<string>( strings );

			  assertThat( FunctionalTestPlugin.StringList, @is( stringsList ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUrlEncodedListsAndInt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUrlEncodedListsAndInt()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithListAndInt" );

			  string postBody = "strings[]=aaa&strings[]=bbb&strings[]=ccc&count=3";

			  RestRequest.req().post(methodUri,postBody,MediaType.APPLICATION_FORM_URLENCODED_TYPE);

			  IList<string> strings = Arrays.asList( "aaa", "bbb", "ccc" );

			  IList<string> stringsList = new List<string>( strings );

			  assertThat( FunctionalTestPlugin.StringList, @is( stringsList ) );
			  assertThat( FunctionalTestPlugin._integer, @is( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleArrays()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithArray" );
			  string[] stringArray = new string[] { "aaa", "bbb", "aaa" };
			  IList<string> strings = Arrays.asList( stringArray );
			  IDictionary<string, object> @params = MapUtil.map( "strings", strings );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  assertThat( FunctionalTestPlugin.StringArray, @is( stringArray ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePrimitiveArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandlePrimitiveArrays()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithIntArray" );
			  int?[] intArray = new int?[] { 5, 6, 7, 8 };
			  IList<int> ints = Arrays.asList( intArray );
			  IDictionary<string, object> @params = MapUtil.map( "ints", ints );

			  PluginFunctionalTestHelper.MakePostMap( methodUri, @params );

			  assertThat( FunctionalTestPlugin.IntArray, @is( new int[] { 5, 6, 7, 8 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleOptionalArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleOptionalArrays()
		 {
			  string methodUri = GetPluginMethodUri( _functionalTestHelper.dataUri(), "methodWithOptionalArray" );

			  PluginFunctionalTestHelper.MakePostMap( methodUri );

			  assertThat( FunctionalTestPlugin.IntArray, @is( nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnPaths() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReturnPaths()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();
			  long r = _functionalTestHelper.GraphDbHelper.FirstNode;
			  _functionalTestHelper.GraphDbHelper.createRelationship( "friend", n, r );

			  string methodUri = GetPluginMethodUri( _functionalTestHelper.nodeUri( n ), "pathToReference" );

			  IDictionary<string, object> maps = PluginFunctionalTestHelper.MakePostMap( methodUri );

			  assertThat( ( string ) maps["start"], endsWith( Convert.ToString( r ) ) );
			  assertThat( ( string ) maps["end"], endsWith( Convert.ToString( n ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNullPath()
		 {
			  long n = _functionalTestHelper.GraphDbHelper.createNode();

			  string url = GetPluginMethodUri( _functionalTestHelper.nodeUri( n ), "pathToReference" );

			  JaxRsResponse response = ( new RestRequest() ).post(url, null);

			  assertThat( response.Entity, response.Status, @is( 204 ) );
			  response.Close();
		 }

	}

}