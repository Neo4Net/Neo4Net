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
namespace Neo4Net.Server.rest.transactional
{
	using JsonFactory = org.codehaus.jackson.JsonFactory;
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Test = org.junit.Test;


	using MapRow = Neo4Net.Cypher.Internal.javacompat.MapRow;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Statement = Neo4Net.Kernel.api.Statement;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Property = Neo4Net.Test.Property;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.Property.property;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.GraphMock.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.GraphMock.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.GraphMock.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.Link.link;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.Properties.properties;

	public class GraphExtractionWriterTest
	{
		private bool InstanceFieldsInitialized = false;

		public GraphExtractionWriterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_r1 = relationship( 7, _n1, "ONE", _n2, property( "name", "r1" ) );
			_r2 = relationship( 8, _n1, "TWO", _n3, property( "name", "r2" ) );
		}

		 private readonly Node _n1 = node( 17, properties( property( "name", "n1" ) ), "Foo" );
		 private readonly Node _n2 = node( 666, properties( property( "name", "n2" ) ) );
		 private readonly Node _n3 = node( 42, properties( property( "name", "n3" ) ), "Foo", "Bar" );
		 private Relationship _r1;
		 private Relationship _r2;
		 private readonly TransactionStateChecker _checker = new TransactionStateChecker( mock( typeof( Statement ) ), id => false, id => false );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractNodesFromRow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractNodesFromRow()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  row["n1"] = _n1;
			  row["n2"] = _n2;
			  row["n3"] = _n3;
			  row["other.thing"] = "hello";
			  row["some.junk"] = 0x0099cc;

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  assertEquals( "there should be no relationships", 0, result.get( "graph" ).get( "relationships" ).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractRelationshipsFromRowAndNodesFromRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractRelationshipsFromRowAndNodesFromRelationships()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  row["r1"] = _r1;
			  row["r2"] = _r2;

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractPathFromRowAndExtractNodesAndRelationshipsFromPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractPathFromRowAndExtractNodesAndRelationshipsFromPath()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  row["p"] = path( _n2, link( _r1, _n1 ), link( _r2, _n3 ) );

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractGraphFromMapInTheRow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractGraphFromMapInTheRow()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  row["map"] = map;
			  map["r1"] = _r1;
			  map["r2"] = _r2;

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractGraphFromListInTheRow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractGraphFromListInTheRow()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  IList<object> list = new List<object>();
			  row["list"] = list;
			  list.Add( _r1 );
			  list.Add( _r2 );

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractGraphFromListInMapInTheRow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractGraphFromListInMapInTheRow()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  IList<object> list = new List<object>();
			  map["list"] = list;
			  row["map"] = map;
			  list.Add( _r1 );
			  list.Add( _r2 );

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractGraphFromMapInListInTheRow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractGraphFromMapInListInTheRow()
		 {
			  // given
			  IDictionary<string, object> row = new Dictionary<string, object>();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  IList<object> list = new List<object>();
			  list.Add( map );
			  row["list"] = list;
			  map["r1"] = _r1;
			  map["r2"] = _r2;

			  // when
			  JsonNode result = Write( row );

			  // then
			  AssertNodes( result );
			  AssertRelationships( result );
		 }

		 // The code under test

		 private JsonFactory _jsonFactory = new JsonFactory();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.codehaus.jackson.JsonNode write(java.util.Map<String, Object> row) throws java.io.IOException, org.Neo4Net.server.rest.domain.JsonParseException
		 private JsonNode Write( IDictionary<string, object> row )
		 {
			  MemoryStream @out = new MemoryStream();
			  JsonGenerator json = _jsonFactory.createJsonGenerator( @out );
			  json.writeStartObject();
			  try
			  {
					( new GraphExtractionWriter() ).Write(json, row.Keys, new MapRow(row), _checker);
			  }
			  finally
			  {
					json.writeEndObject();
					json.flush();
			  }
			  return JsonHelper.jsonNode( @out.ToString( UTF_8.name() ) );
		 }

		 // The expected format of the result

		 private void AssertNodes( JsonNode result )
		 {
			  JsonNode nodes = result.get( "graph" ).get( "nodes" );
			  assertEquals( "there should be 3 nodes", 3, nodes.size() );
			  AssertNode( "17", nodes, new IList<string> { "Foo" }, property( "name", "n1" ) );
			  AssertNode( "666", nodes, Arrays.asList(), property("name", "n2") );
			  AssertNode( "42", nodes, new IList<string> { "Foo", "Bar" }, property( "name", "n3" ) );
		 }

		 private void AssertRelationships( JsonNode result )
		 {
			  JsonNode relationships = result.get( "graph" ).get( "relationships" );
			  assertEquals( "there should be 2 relationships", 2, relationships.size() );
			  AssertRelationship( "7", relationships, "17", "ONE", "666", property( "name", "r1" ) );
			  AssertRelationship( "8", relationships, "17", "TWO", "42", property( "name", "r2" ) );
		 }

		 // Helpers

		 private static void AssertNode( string id, JsonNode nodes, IList<string> labels, params Property[] properties )
		 {
			  JsonNode node = Get( nodes, id );
			  AssertListEquals( "Node[" + id + "].labels", labels, node.get( "labels" ) );
			  JsonNode props = node.get( "properties" );
			  assertEquals( "length( Node[" + id + "].properties )", properties.Length, props.size() );
			  foreach ( Property property in properties )
			  {
					AssertJsonEquals( "Node[" + id + "].properties[" + property.Key() + "]", property.Value(), props.get(property.Key()) );
			  }
		 }

		 private static void AssertRelationship( string id, JsonNode relationships, string startNodeId, string type, string endNodeId, params Property[] properties )
		 {
			  JsonNode relationship = Get( relationships, id );
			  assertEquals( "Relationship[" + id + "].labels", type, relationship.get( "type" ).TextValue );
			  assertEquals( "Relationship[" + id + "].startNode", startNodeId, relationship.get( "startNode" ).TextValue );
			  assertEquals( "Relationship[" + id + "].endNode", endNodeId, relationship.get( "endNode" ).TextValue );
			  JsonNode props = relationship.get( "properties" );
			  assertEquals( "length( Relationship[" + id + "].properties )", properties.Length, props.size() );
			  foreach ( Property property in properties )
			  {
					AssertJsonEquals( "Relationship[" + id + "].properties[" + property.Key() + "]", property.Value(), props.get(property.Key()) );
			  }
		 }

		 private static void AssertJsonEquals( string message, object expected, JsonNode actual )
		 {
			  if ( expected == null )
			  {
					assertTrue( message, actual == null || actual.Null );
			  }
			  else if ( expected is string )
			  {
					assertEquals( message, expected, actual.TextValue );
			  }
			  else if ( expected is Number )
			  {
					assertEquals( message, expected, actual.NumberValue );
			  }
			  else
			  {
					fail( message + " - unexpected type - " + expected );
			  }
		 }

		 private static void AssertListEquals( string what, IList<string> expected, JsonNode jsonNode )
		 {
			  assertTrue( what + " - should be a list", jsonNode.Array );
			  IList<string> actual = new List<string>( jsonNode.size() );
			  foreach ( JsonNode node in jsonNode )
			  {
					actual.Add( node.TextValue );
			  }
			  assertEquals( what, expected, actual );
		 }

		 private static JsonNode Get( IEnumerable<JsonNode> jsonNodes, string id )
		 {
			  foreach ( JsonNode jsonNode in jsonNodes )
			  {
					if ( id.Equals( jsonNode.get( "id" ).TextValue ) )
					{
						 return jsonNode;
					}
			  }
			  return null;
		 }
	}

}