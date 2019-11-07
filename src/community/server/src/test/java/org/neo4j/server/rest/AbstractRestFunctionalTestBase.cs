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
	using Rule = org.junit.Rule;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.Collections.Helpers;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using GraphHolder = Neo4Net.Test.GraphHolder;
	using Neo4Net.Test;
	using HTTP = Neo4Net.Test.server.HTTP;
	using SharedServerTestBase = Neo4Net.Test.server.SharedServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.createJsonFrom;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_NODE_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_RELATIONSHIPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_RELATIONSHIP_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_SCHEMA_CONSTRAINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_SCHEMA_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class AbstractRestFunctionalTestBase : SharedServerTestBase, GraphHolder
	{
		private bool InstanceFieldsInitialized = false;

		public AbstractRestFunctionalTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Data = TestData.producedThrough( GraphDescription.createGraphFor( this, true ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<java.util.Map<String,Neo4Net.graphdb.Node>> data = Neo4Net.test.TestData.producedThrough(Neo4Net.test.GraphDescription.createGraphFor(this, true));
		 public TestData<IDictionary<string, Node>> Data;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<RESTRequestGenerator> gen = Neo4Net.test.TestData.producedThrough(RESTRequestGenerator.PRODUCER);
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public TestData<RESTRequestGenerator> GenConflict = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final String doCypherRestCall(String endpoint, String scriptTemplate, javax.ws.rs.core.Response.Status status, Neo4Net.helpers.collection.Pair<String, String>... params)
		 public string DoCypherRestCall( string endpoint, string scriptTemplate, Status status, params Pair<string, string>[] @params )
		 {
			  string parameterString = CreateParameterString( @params );

			  return DoCypherRestCall( endpoint, scriptTemplate, status, parameterString );
		 }

		 public virtual string DoCypherRestCall( string endpoint, string scriptTemplate, Status status, string parameterString )
		 {
			  Data.get();

			  string script = CreateScript( scriptTemplate );
			  string queryString = "{\"query\": \"" + script + "\",\"params\":{" + parameterString + "}}";

			  Gen().expectedStatus(status.StatusCode).payload(queryString);
			  return Gen().post(endpoint).entity();
		 }

		 private long? IdFor( string name )
		 {
			  return Data.get()[name].Id;
		 }

		 private string CreateParameterString( Pair<string, string>[] @params )
		 {
			  string paramString = "";
			  foreach ( Pair<string, string> param in @params )
			  {
					string delimiter = paramString.Length == 0 || paramString.EndsWith( "{", StringComparison.Ordinal ) ? "" : ",";

					paramString += delimiter + "\"" + param.First() + "\":\"" + param.Other() + "\"";
			  }

			  return paramString;
		 }

		 protected internal virtual string CreateScript( string template )
		 {
			  foreach ( string key in Data.get().Keys )
			  {
					template = template.Replace( "%" + key + "%", IdFor( key ).ToString() );
			  }
			  return template;
		 }

		 public override IGraphDatabaseService Graphdb()
		 {
			  return Server().Database.Graph;
		 }

		 public virtual T ResolveDependency<T>( Type cls )
		 {
				 cls = typeof( T );
			  return ( ( GraphDatabaseAPI )Graphdb() ).DependencyResolver.resolveDependency(cls);
		 }

		 protected internal static string DataUri
		 {
			 get
			 {
				  return "http://localhost:" + LocalHttpPort + "/db/data/";
			 }
		 }

		 protected internal virtual string DatabaseUri
		 {
			 get
			 {
				  return "http://localhost:" + LocalHttpPort + "/db/";
			 }
		 }

		 protected internal virtual string GetNodeUri( Node node )
		 {
			  return GetNodeUri( node.Id );
		 }

		 protected internal virtual string GetNodeUri( long node )
		 {
			  return DataUri + PATH_NODES + "/" + node;
		 }

		 protected internal virtual string GetRelationshipUri( Relationship relationship )
		 {
			  return DataUri + PATH_RELATIONSHIPS + "/" + relationship.Id;
		 }

		 protected internal virtual string PostNodeIndexUri( string indexName )
		 {
			  return DataUri + PATH_NODE_INDEX + "/" + indexName;
		 }

		 protected internal virtual string PostRelationshipIndexUri( string indexName )
		 {
			  return DataUri + PATH_RELATIONSHIP_INDEX + "/" + indexName;
		 }

		 protected internal virtual string TxUri()
		 {
			  return DataUri + "transaction";
		 }

		 protected internal static string TxCommitUri()
		 {
			  return DataUri + "transaction/commit";
		 }

		 protected internal virtual string TxUri( long txId )
		 {
			  return DataUri + "transaction/" + txId;
		 }

		 public static long ExtractTxId( HTTP.Response response )
		 {
			  int lastSlash = response.Location().LastIndexOf('/');
			  string txIdString = response.Location().Substring(lastSlash + 1);
			  return long.Parse( txIdString );
		 }

		 protected internal virtual Node GetNode( string name )
		 {
			  return Data.get()[name];
		 }

		 protected internal virtual Node[] GetNodes( params string[] names )
		 {
			  Node[] nodes = new Node[] {};
			  List<Node> result = new List<Node>();
			  foreach ( string name in names )
			  {
					result.Add( GetNode( name ) );
			  }
			  return result.toArray( nodes );
		 }

		 public virtual void AssertSize( int expectedSize, string IEntity )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits;
			  ICollection<object> hits;
			  try
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: hits = (java.util.Collection<?>) Neo4Net.server.rest.domain.JsonHelper.readJson(entity);
					hits = ( ICollection<object> ) JsonHelper.readJson( IEntity );
					assertEquals( expectedSize, hits.Count );
			  }
			  catch ( JsonParseException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual string GetPropertiesUri( Relationship rel )
		 {
			  return GetRelationshipUri( rel ) + "/properties";
		 }

		 public virtual string GetPropertiesUri( Node node )
		 {
			  return GetNodeUri( node ) + "/properties";
		 }

		 public virtual RESTRequestGenerator Gen()
		 {
			  return GenConflict.get();
		 }

		 public virtual string LabelsUri
		 {
			 get
			 {
				  return format( "%slabels", DataUri );
			 }
		 }

		 public virtual string PropertyKeysUri
		 {
			 get
			 {
				  return format( "%spropertykeys", DataUri );
			 }
		 }

		 public virtual string GetNodesWithLabelUri( string label )
		 {
			  return format( "%slabel/%s/nodes", DataUri, label );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getNodesWithLabelAndPropertyUri(String label, String property, Object value) throws java.io.UnsupportedEncodingException
		 public virtual string GetNodesWithLabelAndPropertyUri( string label, string property, object value )
		 {
			  return format( "%slabel/%s/nodes?%s=%s", DataUri, label, property, encode( createJsonFrom( value ), StandardCharsets.UTF_8.name() ) );
		 }

		 public virtual string SchemaIndexUri
		 {
			 get
			 {
				  return DataUri + PATH_SCHEMA_INDEX;
			 }
		 }

		 public virtual string GetSchemaIndexLabelUri( string label )
		 {
			  return DataUri + PATH_SCHEMA_INDEX + "/" + label;
		 }

		 public virtual string GetSchemaIndexLabelPropertyUri( string label, string property )
		 {
			  return DataUri + PATH_SCHEMA_INDEX + "/" + label + "/" + property;
		 }

		 public virtual string SchemaConstraintUri
		 {
			 get
			 {
				  return DataUri + PATH_SCHEMA_CONSTRAINT;
			 }
		 }

		 public virtual string GetSchemaConstraintLabelUri( string label )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label;
		 }

		 public virtual string GetSchemaConstraintLabelUniquenessUri( string label )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label + "/uniqueness/";
		 }

		 public virtual string GetSchemaConstraintLabelUniquenessPropertyUri( string label, string property )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label + "/uniqueness/" + property;
		 }

		 public static int LocalHttpPort
		 {
			 get
			 {
				  ConnectorPortRegister connectorPortRegister = Server().Database.Graph.DependencyResolver.resolveDependency(typeof(ConnectorPortRegister));
				  return connectorPortRegister.GetLocalAddress( "http" ).Port;
			 }
		 }

		 public static HTTP.Response RunQuery( string query, params string[] contentTypes )
		 {
			  string resultDataContents = "";
			  if ( contentTypes.Length > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					resultDataContents = ", 'resultDataContents': [" + java.util.contentTypes.Select( unquoted => format( "'%s'", unquoted ) ).collect( joining( "," ) ) + "]";
			  }
			  return POST( TxCommitUri(), quotedJson(format("{'statements': [{'statement': '%s'%s}]}", query, resultDataContents)) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertNoErrors(Neo4Net.test.server.HTTP.Response response) throws Neo4Net.server.rest.domain.JsonParseException
		 public static void AssertNoErrors( HTTP.Response response )
		 {
			  assertEquals( "[]", response.Get( "errors" ).ToString() );
			  assertEquals( 0, response.Get( "errors" ).size() );
		 }
	}

}