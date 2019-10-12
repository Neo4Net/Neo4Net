using System;
using System.Collections.Generic;

/*
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
namespace Neo4Net.Server.rest.web
{
	using Configuration = org.apache.commons.configuration.Configuration;


	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Neo4Net.Helpers.Collection;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using EndNodeNotFoundException = Neo4Net.Server.rest.domain.EndNodeNotFoundException;
	using PropertySettingStrategy = Neo4Net.Server.rest.domain.PropertySettingStrategy;
	using StartNodeNotFoundException = Neo4Net.Server.rest.domain.StartNodeNotFoundException;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using IndexedEntityRepresentation = Neo4Net.Server.rest.repr.IndexedEntityRepresentation;
	using InputFormat = Neo4Net.Server.rest.repr.InputFormat;
	using InvalidArgumentsException = Neo4Net.Server.rest.repr.InvalidArgumentsException;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RelationshipDirection = Neo4Net.Server.rest.web.DatabaseActions.RelationshipDirection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.toMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_LABELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_NODE_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_RELATIONSHIPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_RELATIONSHIP_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_SCHEMA_CONSTRAINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_SCHEMA_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.Surface_Fields.PATH_SCHEMA_RELATIONSHIP_CONSTRAINT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/") public class RestfulGraphDatabase
	public class RestfulGraphDatabase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public static class AmpersandSeparatedCollection extends java.util.LinkedHashSet<String>
		 public class AmpersandSeparatedCollection : LinkedHashSet<string>
		 {
			  public AmpersandSeparatedCollection( string path )
			  {
					foreach ( string e in path.Split( "&", true ) )
					{
						 if ( e.Trim().Length > 0 )
						 {
							  add( e );
						 }
					}
			  }
		 }

		 private static readonly string _pathNode = PATH_NODES + "/{nodeId}";
		 private static readonly string _pathNodeProperties = _pathNode + "/properties";
		 private static readonly string _pathNodeProperty = _pathNodeProperties + "/{key}";
		 private static readonly string _pathNodeRelationships = _pathNode + "/relationships";
		 private static readonly string _pathRelationship = PATH_RELATIONSHIPS + "/{relationshipId}";
		 private static readonly string _pathNodeRelationshipsWDir = _pathNodeRelationships + "/{direction}";
		 private static readonly string _pathNodeRelationshipsWDirNTypes = _pathNodeRelationshipsWDir + "/{types}";
		 private static readonly string _pathRelationshipProperties = _pathRelationship + "/properties";
		 private static readonly string _pathRelationshipProperty = _pathRelationshipProperties + "/{key}";
		 private static readonly string _pathNodePath = _pathNode + "/path";
		 private static readonly string _pathNodePaths = _pathNode + "/paths";
		 private static readonly string _pathNodeLabels = _pathNode + "/labels";
		 private static readonly string _pathNodeLabel = _pathNode + "/labels/{label}";
		 private static readonly string _pathNodeDegree = _pathNode + "/degree";
		 private static readonly string _pathNodeDegreeWDir = _pathNodeDegree + "/{direction}";
		 private static readonly string _pathNodeDegreeWDirNTypes = _pathNodeDegreeWDir + "/{types}";

		 private const string PATH_PROPERTY_KEYS = "propertykeys";

		 protected internal static readonly string PathNamedNodeIndex = PATH_NODE_INDEX + "/{indexName}";
		 protected internal static readonly string PathNodeIndexGet = PathNamedNodeIndex + "/{key}/{value}";
		 protected internal static readonly string PathNodeIndexQueryWithKey = PathNamedNodeIndex + "/{key}";
		 // http://localhost/db/data/index/node/foo?query=somelucenestuff
		 protected internal static readonly string PathNodeIndexId = PathNodeIndexGet + "/{id}";
		 protected internal static readonly string PathNodeIndexRemoveKey = PathNamedNodeIndex + "/{key}/{id}";
		 protected internal static readonly string PathNodeIndexRemove = PathNamedNodeIndex + "/{id}";

		 protected internal static readonly string PathNamedRelationshipIndex = PATH_RELATIONSHIP_INDEX + "/{indexName}";
		 protected internal static readonly string PathRelationshipIndexGet = PathNamedRelationshipIndex + "/{key}/{value}";
		 protected internal static readonly string PathRelationshipIndexQueryWithKey = PathNamedRelationshipIndex + "/{key}";
		 protected internal static readonly string PathRelationshipIndexId = PathRelationshipIndexGet + "/{id}";
		 protected internal static readonly string PathRelationshipIndexRemoveKey = PathNamedRelationshipIndex + "/{key}/{id}";
		 protected internal static readonly string PathRelationshipIndexRemove = PathNamedRelationshipIndex + "/{id}";

		 public const string PATH_AUTO_INDEX = "index/auto/{type}";
		 protected internal static readonly string PathAutoIndexStatus = PATH_AUTO_INDEX + "/status";
		 protected internal static readonly string PathAutoIndexedProperties = PATH_AUTO_INDEX + "/properties";
		 protected internal static readonly string PathAutoIndexPropertyDelete = PathAutoIndexedProperties + "/{property}";
		 protected internal static readonly string PathAutoIndexGet = PATH_AUTO_INDEX + "/{key}/{value}";

		 public const string PATH_ALL_NODES_LABELED = "label/{label}/nodes";

		 public static readonly string PathSchemaIndexLabel = PATH_SCHEMA_INDEX + "/{label}";
		 public static readonly string PathSchemaIndexLabelProperty = PathSchemaIndexLabel + "/{property}";

		 public static readonly string PathSchemaConstraintLabel = PATH_SCHEMA_CONSTRAINT + "/{label}";
		 public static readonly string PathSchemaConstraintLabelUniqueness = PathSchemaConstraintLabel + "/uniqueness";
		 public static readonly string PathSchemaConstraintLabelExistence = PathSchemaConstraintLabel + "/existence";
		 public static readonly string PathSchemaConstraintLabelUniquenessProperty = PathSchemaConstraintLabelUniqueness + "/{property}";
		 public static readonly string PathSchemaConstraintLabelExistenceProperty = PathSchemaConstraintLabelExistence + "/{property}";

		 public static readonly string PathSchemaRelationshipConstraintType = PATH_SCHEMA_RELATIONSHIP_CONSTRAINT + "/{type}";
		 public static readonly string PathSchemaRelationshipConstraintTypeExistence = PathSchemaRelationshipConstraintType + "/existence";
		 public static readonly string PathSchemaRelationshipConstraintExistenceProperty = PathSchemaRelationshipConstraintTypeExistence + "/{property}";

		 public const string NODE_AUTO_INDEX_TYPE = "node";
		 public const string RELATIONSHIP_AUTO_INDEX_TYPE = "relationship";

		 private const string UNIQUENESS_MODE_GET_OR_CREATE = "get_or_create";
		 private const string UNIQUENESS_MODE_CREATE_OR_FAIL = "create_or_fail";

		 private readonly DatabaseActions _actions;
		 private Configuration _config;
		 private readonly OutputFormat _output;
		 private readonly InputFormat _input;

		 private enum UniqueIndexType
		 {
			  None,
			  GetOrCreate,
			  CreateOrFail
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public RestfulGraphDatabase(@Context InputFormat input, @Context OutputFormat output, @Context DatabaseActions actions, @Context Configuration config)
		 public RestfulGraphDatabase( InputFormat input, OutputFormat output, DatabaseActions actions, Configuration config )
		 {
			  this._input = input;
			  this._output = output;
			  this._actions = actions;
			  this._config = config;
		 }

		 public virtual OutputFormat OutputFormat
		 {
			 get
			 {
				  return _output;
			 }
		 }

		 private Response Nothing()
		 {
			  return _output.noContent();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<long> extractNodeIdOrNull(String uri) throws org.neo4j.server.rest.repr.BadInputException
		 private long? ExtractNodeIdOrNull( string uri )
		 {
			  if ( string.ReferenceEquals( uri, null ) )
			  {
					return null;
			  }
			  return ExtractNodeId( uri );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long extractNodeId(String uri) throws org.neo4j.server.rest.repr.BadInputException
		 private long ExtractNodeId( string uri )
		 {
			  try
			  {
					return long.Parse( uri.Substring( uri.LastIndexOf( '/' ) + 1 ) );
			  }
			  catch ( Exception ex ) when ( ex is System.FormatException || ex is System.NullReferenceException )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<long> extractRelationshipIdOrNull(String uri) throws org.neo4j.server.rest.repr.BadInputException
		 private long? ExtractRelationshipIdOrNull( string uri )
		 {
			  if ( string.ReferenceEquals( uri, null ) )
			  {
					return null;
			  }
			  return ExtractRelationshipId( uri );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long extractRelationshipId(String uri) throws org.neo4j.server.rest.repr.BadInputException
		 private long ExtractRelationshipId( string uri )
		 {
			  return ExtractNodeId( uri );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response getRoot()
		 public virtual Response Root
		 {
			 get
			 {
				  return _output.ok( _actions.root() );
			 }
		 }

		 // Nodes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODES) public javax.ws.rs.core.Response createNode(String body)
		 public virtual Response CreateNode( string body )
		 {
			  try
			  {
					return _output.created( _actions.createNode( _input.readMap( body ) ) );
			  }
			  catch ( ArrayStoreException )
			  {
					return GenerateBadRequestDueToMangledJsonResponse( body );
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is System.InvalidCastException )
			  {
					return _output.badRequest( e );
			  }
		 }

		 private Response GenerateBadRequestDueToMangledJsonResponse( string body )
		 {
			  return _output.badRequest( MediaType.TEXT_PLAIN_TYPE, "Invalid JSON array in POST body: " + body );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE) public javax.ws.rs.core.Response getNode(@PathParam("nodeId") long nodeId)
		 public virtual Response GetNode( long nodeId )
		 {
			  try
			  {
					return _output.ok( _actions.getNode( nodeId ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE) public javax.ws.rs.core.Response deleteNode(@PathParam("nodeId") long nodeId)
		 public virtual Response DeleteNode( long nodeId )
		 {
			  try
			  {
					_actions.deleteNode( nodeId );
					return Nothing();
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

		 // Node properties

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_NODE_PROPERTIES) public javax.ws.rs.core.Response setAllNodeProperties(@PathParam("nodeId") long nodeId, String body)
		 public virtual Response SetAllNodeProperties( long nodeId, string body )
		 {
			  try
			  {
					_actions.setAllNodeProperties( nodeId, _input.readMap( body ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ArrayStoreException )
			  {
					return GenerateBadRequestDueToMangledJsonResponse( body );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_PROPERTIES) public javax.ws.rs.core.Response getAllNodeProperties(@PathParam("nodeId") long nodeId)
		 public virtual Response GetAllNodeProperties( long nodeId )
		 {
			  try
			  {
					return _output.response( Response.Status.OK, _actions.getAllNodeProperties( nodeId ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_NODE_PROPERTY) public javax.ws.rs.core.Response setNodeProperty(@PathParam("nodeId") long nodeId, @PathParam("key") String key, String body)
		 public virtual Response SetNodeProperty( long nodeId, string key, string body )
		 {
			  try
			  {
					_actions.setNodeProperty( nodeId, key, _input.readValue( body ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ArrayStoreException )
			  {
					return GenerateBadRequestDueToMangledJsonResponse( body );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_PROPERTY) public javax.ws.rs.core.Response getNodeProperty(@PathParam("nodeId") long nodeId, @PathParam("key") String key)
		 public virtual Response GetNodeProperty( long nodeId, string key )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeProperty( nodeId, key ) );
			  }
			  catch ( Exception e ) when ( e is NodeNotFoundException || e is NoSuchPropertyException )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_PROPERTY) public javax.ws.rs.core.Response deleteNodeProperty(@PathParam("nodeId") long nodeId, @PathParam("key") String key)
		 public virtual Response DeleteNodeProperty( long nodeId, string key )
		 {
			  try
			  {
					_actions.removeNodeProperty( nodeId, key );
			  }
			  catch ( Exception e ) when ( e is NodeNotFoundException || e is NoSuchPropertyException )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_PROPERTIES) public javax.ws.rs.core.Response deleteAllNodeProperties(@PathParam("nodeId") long nodeId)
		 public virtual Response DeleteAllNodeProperties( long nodeId )
		 {
			  try
			  {
					_actions.removeAllNodeProperties( nodeId );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( PropertyValueException e )
			  {
					return _output.badRequest( e );
			  }
			  return Nothing();
		 }

		 // Node Labels

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODE_LABELS) public javax.ws.rs.core.Response addNodeLabel(@PathParam("nodeId") long nodeId, String body)
		 public virtual Response AddNodeLabel( long nodeId, string body )
		 {
			  try
			  {
					object rawInput = _input.readValue( body );
					if ( rawInput is string )
					{
						 List<string> s = new List<string>();
						 s.Add( ( string ) rawInput );
						 _actions.addLabelToNode( nodeId, s );
					}
					else if ( rawInput is System.Collections.ICollection )
					{
						 _actions.addLabelToNode( nodeId, ( ICollection<string> ) rawInput );
					}
					else
					{
						 throw new InvalidArgumentsException( format( "Label name must be a string. Got: '%s'", rawInput ) );
					}
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ArrayStoreException )
			  {
					return GenerateBadRequestDueToMangledJsonResponse( body );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_NODE_LABELS) public javax.ws.rs.core.Response setNodeLabels(@PathParam("nodeId") long nodeId, String body)
		 public virtual Response SetNodeLabels( long nodeId, string body )
		 {
			  try
			  {
					object rawInput = _input.readValue( body );
					if ( !( rawInput is System.Collections.ICollection ) )
					{
						 throw new InvalidArgumentsException( format( "Input must be an array of Strings. Got: '%s'", rawInput ) );
					}
					else
					{
						 _actions.setLabelsOnNode( nodeId, ( ICollection<string> ) rawInput );
					}
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ArrayStoreException )
			  {
					return GenerateBadRequestDueToMangledJsonResponse( body );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_LABEL) public javax.ws.rs.core.Response removeNodeLabel(@PathParam("nodeId") long nodeId, @PathParam("label") String labelName)
		 public virtual Response RemoveNodeLabel( long nodeId, string labelName )
		 {
			  try
			  {
					_actions.removeLabelFromNode( nodeId, labelName );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_LABELS) public javax.ws.rs.core.Response getNodeLabels(@PathParam("nodeId") long nodeId)
		 public virtual Response GetNodeLabels( long nodeId )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeLabels( nodeId ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_ALL_NODES_LABELED) public javax.ws.rs.core.Response getNodesWithLabelAndProperty(@PathParam("label") String labelName, @Context UriInfo uriInfo)
		 public virtual Response GetNodesWithLabelAndProperty( string labelName, UriInfo uriInfo )
		 {
			  try
			  {
					if ( labelName.Length == 0 )
					{
						 throw new InvalidArgumentsException( "Empty label name" );
					}

					IDictionary<string, object> properties = toMap( map( queryParamsToProperties, uriInfo.QueryParameters.entrySet() ) );

					return _output.ok( _actions.getNodesWithLabel( labelName, properties ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_LABELS) public javax.ws.rs.core.Response getAllLabels(@QueryParam("in_use") @DefaultValue("true") boolean inUse)
		 public virtual Response GetAllLabels( bool inUse )
		 {
			  return _output.ok( _actions.getAllLabels( inUse ) );
		 }

		 // Property keys

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_PROPERTY_KEYS) public javax.ws.rs.core.Response getAllPropertyKeys()
		 public virtual Response AllPropertyKeys
		 {
			 get
			 {
				  return _output.ok( _actions.AllPropertyKeys );
			 }
		 }

		 // Relationships

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @POST @Path(PATH_NODE_RELATIONSHIPS) public javax.ws.rs.core.Response createRelationship(@PathParam("nodeId") long startNodeId, String body)
		 public virtual Response CreateRelationship( long startNodeId, string body )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> data;
			  IDictionary<string, object> data;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endNodeId;
			  long endNodeId;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String type;
			  string type;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> properties;
			  IDictionary<string, object> properties;
			  try
			  {
					data = _input.readMap( body );
					endNodeId = ExtractNodeId( ( string ) data["to"] );
					type = ( string ) data["type"];
					properties = ( IDictionary<string, object> ) data["data"];
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is System.InvalidCastException )
			  {
					return _output.badRequest( e );
			  }
			  try
			  {
					return _output.created( _actions.createRelationship( startNodeId, endNodeId, type, properties ) );
			  }
			  catch ( StartNodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( Exception e ) when ( e is EndNodeNotFoundException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP) public javax.ws.rs.core.Response getRelationship(@PathParam("relationshipId") long relationshipId)
		 public virtual Response GetRelationship( long relationshipId )
		 {
			  try
			  {
					return _output.ok( _actions.getRelationship( relationshipId ) );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP) public javax.ws.rs.core.Response deleteRelationship(@PathParam("relationshipId") long relationshipId)
		 public virtual Response DeleteRelationship( long relationshipId )
		 {
			  try
			  {
					_actions.deleteRelationship( relationshipId );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_RELATIONSHIPS_W_DIR) public javax.ws.rs.core.Response getNodeRelationships(@PathParam("nodeId") long nodeId, @PathParam("direction") org.neo4j.server.rest.web.DatabaseActions.RelationshipDirection direction)
		 public virtual Response GetNodeRelationships( long nodeId, RelationshipDirection direction )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeRelationships( nodeId, direction, Collections.emptyList() ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_RELATIONSHIPS_W_DIR_N_TYPES) public javax.ws.rs.core.Response getNodeRelationships(@PathParam("nodeId") long nodeId, @PathParam("direction") org.neo4j.server.rest.web.DatabaseActions.RelationshipDirection direction, @PathParam("types") AmpersandSeparatedCollection types)
		 public virtual Response GetNodeRelationships( long nodeId, RelationshipDirection direction, AmpersandSeparatedCollection types )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeRelationships( nodeId, direction, types ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

		 // Degrees

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_DEGREE_W_DIR) public javax.ws.rs.core.Response getNodeDegree(@PathParam("nodeId") long nodeId, @PathParam("direction") org.neo4j.server.rest.web.DatabaseActions.RelationshipDirection direction)
		 public virtual Response GetNodeDegree( long nodeId, RelationshipDirection direction )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeDegree( nodeId, direction, Collections.emptyList() ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_DEGREE_W_DIR_N_TYPES) public javax.ws.rs.core.Response getNodeDegree(@PathParam("nodeId") long nodeId, @PathParam("direction") org.neo4j.server.rest.web.DatabaseActions.RelationshipDirection direction, @PathParam("types") AmpersandSeparatedCollection types)
		 public virtual Response GetNodeDegree( long nodeId, RelationshipDirection direction, AmpersandSeparatedCollection types )
		 {
			  try
			  {
					return _output.ok( _actions.getNodeDegree( nodeId, direction, types ) );
			  }
			  catch ( NodeNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

		 // Relationship properties

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_PROPERTIES) public javax.ws.rs.core.Response getAllRelationshipProperties(@PathParam("relationshipId") long relationshipId)
		 public virtual Response GetAllRelationshipProperties( long relationshipId )
		 {
			  try
			  {
					return _output.response( Response.Status.OK, _actions.getAllRelationshipProperties( relationshipId ) );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_PROPERTY) public javax.ws.rs.core.Response getRelationshipProperty(@PathParam("relationshipId") long relationshipId, @PathParam("key") String key)
		 public virtual Response GetRelationshipProperty( long relationshipId, string key )
		 {
			  try
			  {
					return _output.ok( _actions.getRelationshipProperty( relationshipId, key ) );
			  }
			  catch ( Exception e ) when ( e is RelationshipNotFoundException || e is NoSuchPropertyException )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_RELATIONSHIP_PROPERTIES) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response setAllRelationshipProperties(@PathParam("relationshipId") long relationshipId, String body)
		 public virtual Response SetAllRelationshipProperties( long relationshipId, string body )
		 {
			  try
			  {
					_actions.setAllRelationshipProperties( relationshipId, _input.readMap( body ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_RELATIONSHIP_PROPERTY) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response setRelationshipProperty(@PathParam("relationshipId") long relationshipId, @PathParam("key") String key, String body)
		 public virtual Response SetRelationshipProperty( long relationshipId, string key, string body )
		 {
			  try
			  {
					_actions.setRelationshipProperty( relationshipId, key, _input.readValue( body ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP_PROPERTIES) public javax.ws.rs.core.Response deleteAllRelationshipProperties(@PathParam("relationshipId") long relationshipId)
		 public virtual Response DeleteAllRelationshipProperties( long relationshipId )
		 {
			  try
			  {
					_actions.removeAllRelationshipProperties( relationshipId );
			  }
			  catch ( RelationshipNotFoundException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( PropertyValueException e )
			  {
					return _output.badRequest( e );
			  }
			  return Nothing();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP_PROPERTY) public javax.ws.rs.core.Response deleteRelationshipProperty(@PathParam("relationshipId") long relationshipId, @PathParam("key") String key)
		 public virtual Response DeleteRelationshipProperty( long relationshipId, string key )
		 {
			  try
			  {
					_actions.removeRelationshipProperty( relationshipId, key );
			  }
			  catch ( Exception e ) when ( e is RelationshipNotFoundException || e is NoSuchPropertyException )
			  {
					return _output.notFound( e );
			  }
			  return Nothing();
		 }

		 // Index

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_INDEX) public javax.ws.rs.core.Response getNodeIndexRoot()
		 public virtual Response NodeIndexRoot
		 {
			 get
			 {
				  if ( _actions.NodeIndexNames.Length == 0 )
				  {
						return _output.noContent();
				  }
				  return _output.ok( _actions.nodeIndexRoot() );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODE_INDEX) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response jsonCreateNodeIndex(String json)
		 public virtual Response JsonCreateNodeIndex( string json )
		 {
			  try
			  {
					return _output.created( _actions.createNodeIndex( _input.readMap( json ) ) );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_INDEX) public javax.ws.rs.core.Response getRelationshipIndexRoot()
		 public virtual Response RelationshipIndexRoot
		 {
			 get
			 {
				  if ( _actions.RelationshipIndexNames.Length == 0 )
				  {
						return _output.noContent();
				  }
				  return _output.ok( _actions.relationshipIndexRoot() );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_RELATIONSHIP_INDEX) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response jsonCreateRelationshipIndex(String json)
		 public virtual Response JsonCreateRelationshipIndex( string json )
		 {
			  try
			  {
					return _output.created( _actions.createRelationshipIndex( _input.readMap( json ) ) );
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is System.ArgumentException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NAMED_NODE_INDEX) public javax.ws.rs.core.Response getIndexedNodesByQuery(@PathParam("indexName") String indexName, @QueryParam("query") String query, @QueryParam("order") String order)
		 public virtual Response GetIndexedNodesByQuery( string indexName, string query, string order )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedNodesByQuery( indexName, query, order ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_AUTO_INDEX) public javax.ws.rs.core.Response getAutoIndexedNodesByQuery(@PathParam("type") String type, @QueryParam("query") String query)
		 public virtual Response GetAutoIndexedNodesByQuery( string type, string query )
		 {
			  try
			  {
					switch ( type )
					{
					case NODE_AUTO_INDEX_TYPE:
						 return _output.ok( _actions.getAutoIndexedNodesByQuery( query ) );
					case RELATIONSHIP_AUTO_INDEX_TYPE:
						 return _output.ok( _actions.getAutoIndexedRelationshipsByQuery( query ) );
					default:
						 return _output.badRequest( new Exception( "Unrecognized auto-index type, " + "expected '" + NODE_AUTO_INDEX_TYPE + "' or '" + RELATIONSHIP_AUTO_INDEX_TYPE + "'" ) );
					}
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NAMED_NODE_INDEX) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response deleteNodeIndex(@PathParam("indexName") String indexName)
		 public virtual Response DeleteNodeIndex( string indexName )
		 {
			  try
			  {
					_actions.removeNodeIndex( indexName );
					return _output.noContent();
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NAMED_RELATIONSHIP_INDEX) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response deleteRelationshipIndex(@PathParam("indexName") String indexName)
		 public virtual Response DeleteRelationshipIndex( string indexName )
		 {
			  try
			  {
					_actions.removeRelationshipIndex( indexName );
					return _output.noContent();
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NAMED_NODE_INDEX) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response addToNodeIndex(@PathParam("indexName") String indexName, @QueryParam("unique") String unique, @QueryParam("uniqueness") String uniqueness, String postBody)
		 public virtual Response AddToNodeIndex( string indexName, string unique, string uniqueness, string postBody )
		 {
			  int otherHeaders = 512;
			  int maximumSizeInBytes = _config.getInt( ServerSettings.maximum_response_header_size.name() ) - otherHeaders;

			  try
			  {
					IDictionary<string, object> entityBody;
					Pair<IndexedEntityRepresentation, bool> result;

					switch ( unique( unique, uniqueness ) )
					{
						 case GetOrCreate:
							  entityBody = _input.readMap( postBody, "key", "value" );

							  string getOrCreateValue = entityBody["value"].ToString();
							  if ( getOrCreateValue.Length > maximumSizeInBytes )
							  {
									return ValueTooBig();
							  }

							  result = _actions.getOrCreateIndexedNode( indexName, entityBody["key"].ToString(), getOrCreateValue, ExtractNodeIdOrNull(GetStringOrNull(entityBody, "uri")), GetMapOrNull(entityBody, "properties") );
							  return result.Other() ? _output.created(result.First()) : _output.okIncludeLocation(result.First());

						 case CreateOrFail:
							  entityBody = _input.readMap( postBody, "key", "value" );

							  string createOrFailValue = entityBody["value"].ToString();
							  if ( createOrFailValue.Length > maximumSizeInBytes )
							  {
									return ValueTooBig();
							  }

							  result = _actions.getOrCreateIndexedNode( indexName, entityBody["key"].ToString(), createOrFailValue, ExtractNodeIdOrNull(GetStringOrNull(entityBody, "uri")), GetMapOrNull(entityBody, "properties") );
							  if ( result.Other() )
							  {
									return _output.created( result.First() );
							  }

							  string uri = GetStringOrNull( entityBody, "uri" );

							  if ( string.ReferenceEquals( uri, null ) )
							  {
									return _output.conflict( result.First() );
							  }

							  long idOfNodeToBeIndexed = ExtractNodeId( uri );
							  long idOfNodeAlreadyInIndex = ExtractNodeId( result.First().Identity );

							  if ( idOfNodeToBeIndexed == idOfNodeAlreadyInIndex )
							  {
									return _output.created( result.First() );
							  }

							  return _output.conflict( result.First() );

						 default:
							  entityBody = _input.readMap( postBody, "key", "value", "uri" );
							  string value = entityBody["value"].ToString();

							  if ( value.Length > maximumSizeInBytes )
							  {
									return ValueTooBig();
							  }

							  return _output.created( _actions.addToNodeIndex( indexName, entityBody["key"].ToString(), value, ExtractNodeId(entityBody["uri"].ToString()) ) );

					}
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

		 private Response ValueTooBig()
		 {
			  return Response.status( 413 ).entity( string.Format( "The property value provided was too large. The maximum size is currently set to {0:D} bytes. " + "You can configure this by setting the '{1}' property.", _config.getInt( ServerSettings.maximum_response_header_size.name() ), ServerSettings.maximum_response_header_size.name() ) ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NAMED_RELATIONSHIP_INDEX) public javax.ws.rs.core.Response addToRelationshipIndex(@PathParam("indexName") String indexName, @QueryParam("unique") String unique, @QueryParam("uniqueness") String uniqueness, String postBody)
		 public virtual Response AddToRelationshipIndex( string indexName, string unique, string uniqueness, string postBody )
		 {
			  try
			  {
					IDictionary<string, object> entityBody;
					Pair<IndexedEntityRepresentation, bool> result;

					switch ( unique( unique, uniqueness ) )
					{
						 case GetOrCreate:
							  entityBody = _input.readMap( postBody, "key", "value" );
							  result = _actions.getOrCreateIndexedRelationship( indexName, entityBody["key"].ToString(), entityBody["value"].ToString(), ExtractRelationshipIdOrNull(GetStringOrNull(entityBody, "uri")), ExtractNodeIdOrNull(GetStringOrNull(entityBody, "start")), GetStringOrNull(entityBody, "type"), ExtractNodeIdOrNull(GetStringOrNull(entityBody, "end")), GetMapOrNull(entityBody, "properties") );
							  return result.Other() ? _output.created(result.First()) : _output.ok(result.First());

						 case CreateOrFail:
							  entityBody = _input.readMap( postBody, "key", "value" );
							  result = _actions.getOrCreateIndexedRelationship( indexName, entityBody["key"].ToString(), entityBody["value"].ToString(), ExtractRelationshipIdOrNull(GetStringOrNull(entityBody, "uri")), ExtractNodeIdOrNull(GetStringOrNull(entityBody, "start")), GetStringOrNull(entityBody, "type"), ExtractNodeIdOrNull(GetStringOrNull(entityBody, "end")), GetMapOrNull(entityBody, "properties") );
							  if ( result.Other() )
							  {
									return _output.created( result.First() );
							  }

							  string uri = GetStringOrNull( entityBody, "uri" );

							  if ( string.ReferenceEquals( uri, null ) )
							  {
									return _output.conflict( result.First() );
							  }

							  long idOfRelationshipToBeIndexed = ExtractRelationshipId( uri );
							  long idOfRelationshipAlreadyInIndex = ExtractRelationshipId( result.First().Identity );

							  if ( idOfRelationshipToBeIndexed == idOfRelationshipAlreadyInIndex )
							  {
									return _output.created( result.First() );
							  }

							  return _output.conflict( result.First() );

						 default:
							  entityBody = _input.readMap( postBody, "key", "value", "uri" );
							  return _output.created( _actions.addToRelationshipIndex( indexName, entityBody["key"].ToString(), entityBody["value"].ToString(), ExtractRelationshipId(entityBody["uri"].ToString()) ) );

					}
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

		 private UniqueIndexType Unique( string uniqueParam, string uniquenessParam )
		 {
			  UniqueIndexType unique = UniqueIndexType.None;
			  if ( string.ReferenceEquals( uniquenessParam, null ) || uniquenessParam.Equals( "" ) )
			  {
					// Backward compatibility check
					if ( "".Equals( uniqueParam ) || bool.Parse( uniqueParam ) )
					{
						 unique = UniqueIndexType.GetOrCreate;
					}

			  }
			  else if ( UNIQUENESS_MODE_GET_OR_CREATE.Equals( uniquenessParam, StringComparison.OrdinalIgnoreCase ) )
			  {
					unique = UniqueIndexType.GetOrCreate;

			  }
			  else if ( UNIQUENESS_MODE_CREATE_OR_FAIL.Equals( uniquenessParam, StringComparison.OrdinalIgnoreCase ) )
			  {
					unique = UniqueIndexType.CreateOrFail;

			  }

			  return unique;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getStringOrNull(java.util.Map<String, Object> map, String key) throws org.neo4j.server.rest.repr.BadInputException
		 private string GetStringOrNull( IDictionary<string, object> map, string key )
		 {
			  object @object = map[key];
			  if ( @object is string )
			  {
					return ( string ) @object;
			  }
			  if ( @object == null )
			  {
					return null;
			  }
			  throw new InvalidArgumentsException( "\"" + key + "\" should be a string" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.Map<String, Object> getMapOrNull(java.util.Map<String, Object> data, String key) throws org.neo4j.server.rest.repr.BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IDictionary<string, object> GetMapOrNull( IDictionary<string, object> data, string key )
		 {
			  object @object = data[key];
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (object instanceof java.util.Map<?, ?>)
			  if ( @object is IDictionary<object, ?> )
			  {
					return ( IDictionary<string, object> ) @object;
			  }
			  if ( @object == null )
			  {
					return null;
			  }
			  throw new InvalidArgumentsException( "\"" + key + "\" should be a map" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_INDEX_ID) public javax.ws.rs.core.Response getNodeFromIndexUri(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value, @PathParam("id") long id)
		 public virtual Response GetNodeFromIndexUri( string indexName, string key, string value, long id )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedNode( indexName, key, value, id ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_INDEX_ID) public javax.ws.rs.core.Response getRelationshipFromIndexUri(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value, @PathParam("id") long id)
		 public virtual Response GetRelationshipFromIndexUri( string indexName, string key, string value, long id )
		 {
			  return _output.ok( _actions.getIndexedRelationship( indexName, key, value, id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_INDEX_GET) public javax.ws.rs.core.Response getIndexedNodes(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value)
		 public virtual Response GetIndexedNodes( string indexName, string key, string value )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedNodes( indexName, key, value ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_AUTO_INDEX_GET) public javax.ws.rs.core.Response getAutoIndexedNodes(@PathParam("type") String type, @PathParam("key") String key, @PathParam("value") String value)
		 public virtual Response GetAutoIndexedNodes( string type, string key, string value )
		 {
			  try
			  {
					switch ( type )
					{
					case NODE_AUTO_INDEX_TYPE:
						 return _output.ok( _actions.getAutoIndexedNodes( key, value ) );
					case RELATIONSHIP_AUTO_INDEX_TYPE:
						 return _output.ok( _actions.getAutoIndexedRelationships( key, value ) );
					default:
						 return _output.badRequest( new Exception( "Unrecognized auto-index type, " + "expected '" + NODE_AUTO_INDEX_TYPE + "' or '" + RELATIONSHIP_AUTO_INDEX_TYPE + "'" ) );
					}
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_INDEX_QUERY_WITH_KEY) public javax.ws.rs.core.Response getIndexedNodesByQuery(@PathParam("indexName") String indexName, @PathParam("key") String key, @QueryParam("query") String query, @PathParam("order") String order)
		 public virtual Response GetIndexedNodesByQuery( string indexName, string key, string query, string order )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedNodesByQuery( indexName, key, query, order ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_INDEX_GET) public javax.ws.rs.core.Response getIndexedRelationships(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value)
		 public virtual Response GetIndexedRelationships( string indexName, string key, string value )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedRelationships( indexName, key, value ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_AUTO_INDEX_STATUS) public javax.ws.rs.core.Response isAutoIndexerEnabled(@PathParam("type") String type)
		 public virtual Response IsAutoIndexerEnabled( string type )
		 {
			  return _output.ok( _actions.isAutoIndexerEnabled( type ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PUT @Path(PATH_AUTO_INDEX_STATUS) public javax.ws.rs.core.Response setAutoIndexerEnabled(@PathParam("type") String type, String enable)
		 public virtual Response SetAutoIndexerEnabled( string type, string enable )
		 {
			  _actions.setAutoIndexerEnabled( type, bool.Parse( enable ) );
			  return _output.ok( Representation.emptyRepresentation() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_AUTO_INDEXED_PROPERTIES) public javax.ws.rs.core.Response getAutoIndexedProperties(@PathParam("type") String type)
		 public virtual Response GetAutoIndexedProperties( string type )
		 {
			  return _output.ok( _actions.getAutoIndexedProperties( type ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_AUTO_INDEXED_PROPERTIES) public javax.ws.rs.core.Response startAutoIndexingProperty(@PathParam("type") String type, String property)
		 public virtual Response StartAutoIndexingProperty( string type, string property )
		 {
			  _actions.startAutoIndexingProperty( type, property );
			  return _output.ok( Representation.emptyRepresentation() );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_AUTO_INDEX_PROPERTY_DELETE) public javax.ws.rs.core.Response stopAutoIndexingProperty(@PathParam("type") String type, @PathParam("property") String property)
		 public virtual Response StopAutoIndexingProperty( string type, string property )
		 {
			  _actions.stopAutoIndexingProperty( type, property );
			  return _output.ok( Representation.emptyRepresentation() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NAMED_RELATIONSHIP_INDEX) public javax.ws.rs.core.Response getIndexedRelationshipsByQuery(@PathParam("indexName") String indexName, @QueryParam("query") String query, @QueryParam("order") String order)
		 public virtual Response GetIndexedRelationshipsByQuery( string indexName, string query, string order )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedRelationshipsByQuery( indexName, query, order ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_INDEX_QUERY_WITH_KEY) public javax.ws.rs.core.Response getIndexedRelationshipsByQuery(@PathParam("indexName") String indexName, @PathParam("key") String key, @QueryParam("query") String query, @QueryParam("order") String order)
		 public virtual Response GetIndexedRelationshipsByQuery( string indexName, string key, string query, string order )
		 {
			  try
			  {
					return _output.ok( _actions.getIndexedRelationshipsByQuery( indexName, key, query, order ) );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_INDEX_ID) public javax.ws.rs.core.Response deleteFromNodeIndex(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value, @PathParam("id") long id)
		 public virtual Response DeleteFromNodeIndex( string indexName, string key, string value, long id )
		 {
			  try
			  {
					_actions.removeFromNodeIndex( indexName, key, value, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_INDEX_REMOVE_KEY) public javax.ws.rs.core.Response deleteFromNodeIndexNoValue(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("id") long id)
		 public virtual Response DeleteFromNodeIndexNoValue( string indexName, string key, long id )
		 {
			  try
			  {
					_actions.removeFromNodeIndexNoValue( indexName, key, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_NODE_INDEX_REMOVE) public javax.ws.rs.core.Response deleteFromNodeIndexNoKeyValue(@PathParam("indexName") String indexName, @PathParam("id") long id)
		 public virtual Response DeleteFromNodeIndexNoKeyValue( string indexName, long id )
		 {
			  try
			  {
					_actions.removeFromNodeIndexNoKeyValue( indexName, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP_INDEX_ID) public javax.ws.rs.core.Response deleteFromRelationshipIndex(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("value") String value, @PathParam("id") long id)
		 public virtual Response DeleteFromRelationshipIndex( string indexName, string key, string value, long id )
		 {
			  try
			  {
					_actions.removeFromRelationshipIndex( indexName, key, value, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP_INDEX_REMOVE_KEY) public javax.ws.rs.core.Response deleteFromRelationshipIndexNoValue(@PathParam("indexName") String indexName, @PathParam("key") String key, @PathParam("id") long id)
		 public virtual Response DeleteFromRelationshipIndexNoValue( string indexName, string key, long id )
		 {
			  try
			  {
					_actions.removeFromRelationshipIndexNoValue( indexName, key, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_RELATIONSHIP_INDEX_REMOVE) public javax.ws.rs.core.Response deleteFromRelationshipIndex(@PathParam("indexName") String indexName, @PathParam("id") long id)
		 public virtual Response DeleteFromRelationshipIndex( string indexName, long id )
		 {
			  try
			  {
					_actions.removeFromRelationshipIndexNoKeyValue( indexName, id );
					return Nothing();
			  }
			  catch ( System.NotSupportedException e )
			  {
					return _output.methodNotAllowed( e );
			  }
			  catch ( NotFoundException nfe )
			  {
					return _output.notFound( nfe );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODE_PATH) public javax.ws.rs.core.Response singlePath(@PathParam("nodeId") long startNode, String body)
		 public virtual Response SinglePath( long startNode, string body )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> description;
			  IDictionary<string, object> description;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endNode;
			  long endNode;
			  try
			  {
					description = _input.readMap( body );
					endNode = ExtractNodeId( ( string ) description["to"] );
					return _output.ok( _actions.findSinglePath( startNode, endNode, description ) );
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is System.InvalidCastException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( NotFoundException e )
			  {
					return _output.notFound( e );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODE_PATHS) public javax.ws.rs.core.Response allPaths(@PathParam("nodeId") long startNode, String body)
		 public virtual Response AllPaths( long startNode, string body )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> description;
			  IDictionary<string, object> description;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endNode;
			  long endNode;
			  try
			  {
					description = _input.readMap( body );
					endNode = ExtractNodeId( ( string ) description["to"] );
					return _output.ok( _actions.findPaths( startNode, endNode, description ) );
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is System.InvalidCastException )
			  {
					return _output.badRequest( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_SCHEMA_INDEX_LABEL) public javax.ws.rs.core.Response createSchemaIndex(@PathParam("label") String labelName, String body)
		 public virtual Response CreateSchemaIndex( string labelName, string body )
		 {
			  try
			  {
					IDictionary<string, object> data = _input.readMap( body, "property_keys" );
					IEnumerable<string> singlePropertyKey = SingleOrList( data, "property_keys" );
					if ( singlePropertyKey == null )
					{
						 return _output.badRequest( new System.ArgumentException( "Supply single property key or list of property keys" ) );
					}
					return _output.ok( _actions.createSchemaIndex( labelName, singlePropertyKey ) );
			  }
			  catch ( Exception e ) when ( e is System.NotSupportedException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

		 private IEnumerable<string> SingleOrList( IDictionary<string, object> data, string key )
		 {
			  object propertyKeys = data[key];
			  IEnumerable<string> singlePropertyKey = null;
			  if ( propertyKeys is System.Collections.IList )
			  {
					singlePropertyKey = ( IList<string> ) propertyKeys;
			  }
			  else if ( propertyKeys is string )
			  {
					singlePropertyKey = Collections.singletonList( ( string ) propertyKeys );
			  }
			  return singlePropertyKey;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_SCHEMA_INDEX_LABEL_PROPERTY) public javax.ws.rs.core.Response dropSchemaIndex(@PathParam("label") String labelName, @PathParam("property") AmpersandSeparatedCollection properties)
		 public virtual Response DropSchemaIndex( string labelName, AmpersandSeparatedCollection properties )
		 {
			  // TODO assumption, only a single property key
			  if ( properties.size() != 1 )
			  {
					return _output.badRequest( new System.ArgumentException( "Single property key assumed" ) );
			  }

			  string property = Iterables.single( properties );
			  try
			  {
					if ( _actions.dropSchemaIndex( labelName, property ) )
					{
						 return Nothing();
					}
					else
					{
						 return _output.notFound();
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_INDEX) public javax.ws.rs.core.Response getSchemaIndexes()
		 public virtual Response SchemaIndexes
		 {
			 get
			 {
				  return _output.ok( _actions.SchemaIndexes );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_INDEX_LABEL) public javax.ws.rs.core.Response getSchemaIndexesForLabel(@PathParam("label") String labelName)
		 public virtual Response GetSchemaIndexesForLabel( string labelName )
		 {
			  return _output.ok( _actions.getSchemaIndexes( labelName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_SCHEMA_CONSTRAINT_LABEL_UNIQUENESS) public javax.ws.rs.core.Response createPropertyUniquenessConstraint(@PathParam("label") String labelName, String body)
		 public virtual Response CreatePropertyUniquenessConstraint( string labelName, string body )
		 {
			  try
			  {
					IDictionary<string, object> data = _input.readMap( body, "property_keys" );
					IEnumerable<string> singlePropertyKey = SingleOrList( data, "property_keys" );
					if ( singlePropertyKey == null )
					{
						 return _output.badRequest( new System.ArgumentException( "Supply single property key or list of property keys" ) );
					}
					return _output.ok( _actions.createPropertyUniquenessConstraint( labelName, singlePropertyKey ) );
			  }
			  catch ( Exception e ) when ( e is System.NotSupportedException || e is BadInputException )
			  {
					return _output.badRequest( e );
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_SCHEMA_CONSTRAINT_LABEL_UNIQUENESS_PROPERTY) public javax.ws.rs.core.Response dropPropertyUniquenessConstraint(@PathParam("label") String labelName, @PathParam("property") AmpersandSeparatedCollection properties)
		 public virtual Response DropPropertyUniquenessConstraint( string labelName, AmpersandSeparatedCollection properties )
		 {
			  try
			  {
					if ( _actions.dropPropertyUniquenessConstraint( labelName, properties ) )
					{
						 return Nothing();
					}
					else
					{
						 return _output.notFound();
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_SCHEMA_CONSTRAINT_LABEL_EXISTENCE_PROPERTY) public javax.ws.rs.core.Response dropNodePropertyExistenceConstraint(@PathParam("label") String labelName, @PathParam("property") AmpersandSeparatedCollection properties)
		 public virtual Response DropNodePropertyExistenceConstraint( string labelName, AmpersandSeparatedCollection properties )
		 {
			  try
			  {
					if ( _actions.dropNodePropertyExistenceConstraint( labelName, properties ) )
					{
						 return Nothing();
					}
					else
					{
						 return _output.notFound();
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path(PATH_SCHEMA_RELATIONSHIP_CONSTRAINT_EXISTENCE_PROPERTY) public javax.ws.rs.core.Response dropRelationshipPropertyExistenceConstraint(@PathParam("type") String typeName, @PathParam("property") AmpersandSeparatedCollection properties)
		 public virtual Response DropRelationshipPropertyExistenceConstraint( string typeName, AmpersandSeparatedCollection properties )
		 {
			  try
			  {
					if ( _actions.dropRelationshipPropertyExistenceConstraint( typeName, properties ) )
					{
						 return Nothing();
					}
					else
					{
						 return _output.notFound();
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					return _output.conflict( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT) public javax.ws.rs.core.Response getSchemaConstraints()
		 public virtual Response SchemaConstraints
		 {
			 get
			 {
				  return _output.ok( _actions.Constraints );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT_LABEL) public javax.ws.rs.core.Response getSchemaConstraintsForLabel(@PathParam("label") String labelName)
		 public virtual Response GetSchemaConstraintsForLabel( string labelName )
		 {
			  return _output.ok( _actions.getLabelConstraints( labelName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT_LABEL_UNIQUENESS) public javax.ws.rs.core.Response getSchemaConstraintsForLabelAndUniqueness(@PathParam("label") String labelName)
		 public virtual Response GetSchemaConstraintsForLabelAndUniqueness( string labelName )
		 {
			  return _output.ok( _actions.getLabelUniquenessConstraints( labelName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT_LABEL_EXISTENCE) public javax.ws.rs.core.Response getSchemaConstraintsForLabelAndExistence(@PathParam("label") String labelName)
		 public virtual Response GetSchemaConstraintsForLabelAndExistence( string labelName )
		 {
			  return _output.ok( _actions.getLabelExistenceConstraints( labelName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_RELATIONSHIP_CONSTRAINT_TYPE_EXISTENCE) public javax.ws.rs.core.Response getSchemaConstraintsForRelationshipTypeAndExistence(@PathParam("type") String typeName)
		 public virtual Response GetSchemaConstraintsForRelationshipTypeAndExistence( string typeName )
		 {
			  return _output.ok( _actions.getRelationshipTypeExistenceConstraints( typeName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT_LABEL_UNIQUENESS_PROPERTY) public javax.ws.rs.core.Response getSchemaConstraintsForLabelAndPropertyUniqueness(@PathParam("label") String labelName, @PathParam("property") AmpersandSeparatedCollection propertyKeys)
		 public virtual Response GetSchemaConstraintsForLabelAndPropertyUniqueness( string labelName, AmpersandSeparatedCollection propertyKeys )
		 {
			  try
			  {
					ListRepresentation constraints = _actions.getPropertyUniquenessConstraint( labelName, propertyKeys );
					return _output.ok( constraints );
			  }
			  catch ( System.ArgumentException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_CONSTRAINT_LABEL_EXISTENCE_PROPERTY) public javax.ws.rs.core.Response getSchemaConstraintsForLabelAndPropertyExistence(@PathParam("label") String labelName, @PathParam("property") AmpersandSeparatedCollection propertyKeys)
		 public virtual Response GetSchemaConstraintsForLabelAndPropertyExistence( string labelName, AmpersandSeparatedCollection propertyKeys )
		 {
			  try
			  {
					ListRepresentation constraints = _actions.getNodePropertyExistenceConstraint( labelName, propertyKeys );
					return _output.ok( constraints );
			  }
			  catch ( System.ArgumentException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_SCHEMA_RELATIONSHIP_CONSTRAINT_EXISTENCE_PROPERTY) public javax.ws.rs.core.Response getSchemaConstraintsForRelationshipTypeAndPropertyExistence(@PathParam("type") String typeName, @PathParam("property") AmpersandSeparatedCollection propertyKeys)
		 public virtual Response GetSchemaConstraintsForRelationshipTypeAndPropertyExistence( string typeName, AmpersandSeparatedCollection propertyKeys )
		 {
			  try
			  {
					ListRepresentation constraints = _actions.getRelationshipPropertyExistenceConstraint( typeName, propertyKeys );
					return _output.ok( constraints );
			  }
			  catch ( System.ArgumentException e )
			  {
					return _output.notFound( e );
			  }
		 }

		 private readonly System.Func<KeyValuePair<string, IList<string>>, Pair<string, object>> queryParamsToProperties = ( KeyValuePair<string, IList<string>> queryEntry ) =>
		 {
			try
			{
				 object propertyValue = _input.readValue( queryEntry.Value.get( 0 ) );
				 if ( propertyValue is ICollection<object> )
				 {
					  propertyValue = PropertySettingStrategy.convertToNativeArray( ( ICollection<object> ) propertyValue );
				 }
				 return Pair.of( queryEntry.Key, propertyValue );
			}
			catch ( BadInputException e )
			{
				 throw new System.ArgumentException( string.Format( "Unable to deserialize property value for {0}.", queryEntry.Key ), e );
			}
		 };
	}

}