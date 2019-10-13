using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.helpers
{
	using Client = com.sun.jersey.api.client.Client;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using RestfulGraphDatabase = Neo4Net.Server.rest.web.RestfulGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.RestfulGraphDatabase.PATH_AUTO_INDEX;

	public sealed class FunctionalTestHelper
	{
		 private readonly NeoServer _server;
		 private readonly GraphDbHelper _helper;

		 public static readonly Client Client = Client.create();
		 private RestRequest _request;

		 public FunctionalTestHelper( NeoServer server )
		 {
			  if ( server.Database == null )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new Exception( "Server must be started before using " + this.GetType().FullName );
			  }
			  this._helper = new GraphDbHelper( server.Database );
			  this._server = server;
			  this._request = new RestRequest( server.BaseUri().resolve("db/data/") );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<String[]> arrayContains(final String element)
		 public static Matcher<string[]> ArrayContains( string element )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( element );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<string[]>
		 {
			 private string _element;

			 public TypeSafeMatcherAnonymousInnerClass( string element )
			 {
				 this._element = element;
			 }

			 private string[] array;

			 public override void describeTo( Description descr )
			 {
				  descr.appendText( "The array " ).appendText( Arrays.ToString( array ) ).appendText( " does not contain <" ).appendText( _element ).appendText( ">" );
			 }

			 public override bool matchesSafely( string[] array )
			 {
				  this.array = array;
				  foreach ( string @string in array )
				  {
						if ( string.ReferenceEquals( _element, null ) )
						{
							 if ( string.ReferenceEquals( @string, null ) )
							 {
								  return true;
							 }
						}
						else if ( _element.Equals( @string ) )
						{
							 return true;
						}
				  }
				  return false;
			 }
		 }

		 public GraphDbHelper GraphDbHelper
		 {
			 get
			 {
				  return _helper;
			 }
		 }

		 public string DataUri()
		 {
			  return _server.baseUri().ToString() + "db/data/";
		 }

		 public string NodeUri()
		 {
			  return DataUri() + "node";
		 }

		 public string NodeUri( long id )
		 {
			  return NodeUri() + "/" + id;
		 }

		 public string NodePropertiesUri( long id )
		 {
			  return NodeUri( id ) + "/properties";
		 }

		 public string NodePropertyUri( long id, string key )
		 {
			  return NodePropertiesUri( id ) + "/" + key;
		 }

		 internal string RelationshipUri()
		 {
			  return DataUri() + "relationship";
		 }

		 public string RelationshipUri( long id )
		 {
			  return RelationshipUri() + "/" + id;
		 }

		 public string RelationshipPropertiesUri( long id )
		 {
			  return RelationshipUri( id ) + "/properties";
		 }

		 public string RelationshipsUri( long nodeId, string dir, params string[] types )
		 {
			  StringBuilder typesString = new StringBuilder();
			  foreach ( string type in types )
			  {
					typesString.Append( typesString.Length > 0 ? "&" : "" );
					typesString.Append( type );
			  }
			  return NodeUri( nodeId ) + "/relationships/" + dir + "/" + typesString;
		 }

		 public string IndexUri()
		 {
			  return DataUri() + "index/";
		 }

		 public string NodeIndexUri()
		 {
			  return IndexUri() + "node/";
		 }

		 public string RelationshipIndexUri()
		 {
			  return IndexUri() + "relationship/";
		 }

		 public string ManagementUri()
		 {
			  return _server.baseUri().ToString() + "db/manage";
		 }

		 public string IndexNodeUri( string indexName )
		 {
			  return NodeIndexUri() + indexName;
		 }

		 public string IndexNodeUri( string indexName, string key, object value )
		 {
			  return IndexNodeUri( indexName ) + "/" + key + "/" + value;
		 }

		 public string IndexRelationshipUri( string indexName )
		 {
			  return RelationshipIndexUri() + indexName;
		 }

		 public string IndexRelationshipUri( string indexName, string key, object value )
		 {
			  return IndexRelationshipUri( indexName ) + "/" + key + "/" + value;
		 }

		 public string ExtensionUri()
		 {
			  return DataUri() + "ext";
		 }

		 public JaxRsResponse Get( string path )
		 {
			  return _request.get( path );
		 }

		 public long GetNodeIdFromUri( string nodeUri )
		 {
			  return Convert.ToInt64( StringHelper.SubstringSpecial( nodeUri, nodeUri.LastIndexOf( '/' ) + 1, nodeUri.Length ) );
		 }

		 public long GetRelationshipIdFromUri( string relationshipUri )
		 {
			  return GetNodeIdFromUri( relationshipUri );
		 }

		 public IDictionary<string, object> RemoveAnyAutoIndex( IDictionary<string, object> map )
		 {
			  IDictionary<string, object> result = new Dictionary<string, object>();
			  foreach ( KeyValuePair<string, object> entry in map.SetOfKeyValuePairs() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> innerMap = (java.util.Map<?,?>) entry.getValue();
					IDictionary<object, ?> innerMap = ( IDictionary<object, ?> ) entry.Value;
					string template = innerMap["template"].ToString();
					if ( !template.Contains( PATH_AUTO_INDEX.replace( "{type}", RestfulGraphDatabase.NODE_AUTO_INDEX_TYPE ) ) && !template.Contains( PATH_AUTO_INDEX.replace( "{type}", RestfulGraphDatabase.RELATIONSHIP_AUTO_INDEX_TYPE ) ) && !template.Contains( "_auto_" ) )
					{
						 result[entry.Key] = entry.Value;
					}
			  }
			  return result;
		 }

		 public URI BaseUri()
		 {
			  return _server.baseUri();
		 }

	}

}