﻿using System;

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
namespace Org.Neo4j.Server.rest.web
{

	using SyntaxException = Org.Neo4j.Cypher.SyntaxException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Database = Org.Neo4j.Server.database.Database;
	using BadPluginInvocationException = Org.Neo4j.Server.plugins.BadPluginInvocationException;
	using ParameterList = Org.Neo4j.Server.plugins.ParameterList;
	using PluginInvocationFailureException = Org.Neo4j.Server.plugins.PluginInvocationFailureException;
	using PluginInvocator = Org.Neo4j.Server.plugins.PluginInvocator;
	using PluginLookupException = Org.Neo4j.Server.plugins.PluginLookupException;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;
	using InputFormat = Org.Neo4j.Server.rest.repr.InputFormat;
	using MappingRepresentation = Org.Neo4j.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Org.Neo4j.Server.rest.repr.MappingSerializer;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;
	using ServerExtensionRepresentation = Org.Neo4j.Server.rest.repr.ServerExtensionRepresentation;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("ext") public class ExtensionService
	public class ExtensionService
	{
		 private const string PATH_EXTENSION = "/{name}";
		 private static readonly string _pathGraphdbExtensionMethod = PATH_EXTENSION + "/graphdb/{method}";
		 private static readonly string _pathNodeExtensionMethod = PATH_EXTENSION + "/node/{nodeId}/{method}";
		 private static readonly string _pathRelationshipExtensionMethod = PATH_EXTENSION + "/relationship/{relationshipId}/{method}";
		 private readonly InputFormat _input;
		 private readonly OutputFormat _output;
		 private readonly PluginInvocator _extensions;
		 private readonly GraphDatabaseAPI _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ExtensionService(@Context InputFormat input, @Context OutputFormat output, @Context PluginInvocator extensions, @Context Database database)
		 public ExtensionService( InputFormat input, OutputFormat output, PluginInvocator extensions, Database database )
		 {
			  this._input = input;
			  this._output = output;
			  this._extensions = extensions;
			  this._graphDb = database.Graph;
		 }

		 public virtual OutputFormat OutputFormat
		 {
			 get
			 {
				  return _output;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Node node(long id) throws NodeNotFoundException
		 private Node Node( long id )
		 {
			  try
			  {
					  using ( Transaction tx = _graphDb.beginTx() )
					  {
						Node node = _graphDb.getNodeById( id );
      
						tx.Success();
						return node;
					  }
			  }
			  catch ( NotFoundException e )
			  {
					throw new NodeNotFoundException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Relationship relationship(long id) throws RelationshipNotFoundException
		 private Relationship Relationship( long id )
		 {
			  try
			  {
					  using ( Transaction tx = _graphDb.beginTx() )
					  {
						Relationship relationship = _graphDb.getRelationshipById( id );
      
						tx.Success();
						return relationship;
					  }
			  }
			  catch ( NotFoundException e )
			  {
					throw new RelationshipNotFoundException( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response getExtensionsList()
		 public virtual Response ExtensionsList
		 {
			 get
			 {
				  return _output.ok( this.ExtensionsList() );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_EXTENSION) public javax.ws.rs.core.Response getExtensionList(@PathParam("name") String name)
		 public virtual Response GetExtensionList( string name )
		 {
			  try
			  {
					return _output.ok( this.ExtensionList( name ) );
			  }
			  catch ( PluginLookupException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_GRAPHDB_EXTENSION_METHOD) public javax.ws.rs.core.Response invokeGraphDatabaseExtension(@PathParam("name") String name, @PathParam("method") String method, String data)
		 public virtual Response InvokeGraphDatabaseExtension( string name, string method, string data )
		 {
			  try
			  {
					return _output.ok( this.InvokeGraphDatabaseExtension( name, method, _input.readParameterList( data ) ) );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( PluginLookupException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( BadPluginInvocationException e )
			  {
					return _output.badRequest( e.InnerException );
			  }
			  catch ( SyntaxException e )
			  {
					return _output.badRequest( e.InnerException );
			  }
			  catch ( PluginInvocationFailureException e )
			  {
					return _output.serverError( e.InnerException );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_GRAPHDB_EXTENSION_METHOD) public javax.ws.rs.core.Response getGraphDatabaseExtensionDescription(@PathParam("name") String name, @PathParam("method") String method)
		 public virtual Response GetGraphDatabaseExtensionDescription( string name, string method )
		 {
			  try
			  {
					return _output.ok( this.DescribeGraphDatabaseExtension( name, method ) );
			  }
			  catch ( PluginLookupException e )
			  {
					return _output.notFound( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_NODE_EXTENSION_METHOD) public javax.ws.rs.core.Response invokeNodeExtension(@PathParam("name") String name, @PathParam("method") String method, @PathParam("nodeId") long nodeId, String data)
		 public virtual Response InvokeNodeExtension( string name, string method, long nodeId, string data )
		 {
			  try
			  {
					return _output.ok( this.InvokeNodeExtension( nodeId, name, method, _input.readParameterList( data ) ) );
			  }
			  catch ( Exception e ) when ( e is NodeNotFoundException || e is PluginLookupException )
			  {
					return _output.notFound( e );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( BadPluginInvocationException e )
			  {
					return _output.badRequest( e.InnerException );
			  }
			  catch ( PluginInvocationFailureException e )
			  {
					return _output.serverError( e.InnerException );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_NODE_EXTENSION_METHOD) public javax.ws.rs.core.Response getNodeExtensionDescription(@PathParam("name") String name, @PathParam("method") String method, @PathParam("nodeId") long nodeId)
		 public virtual Response GetNodeExtensionDescription( string name, string method, long nodeId )
		 {
			  try
			  {
					return _output.ok( this.DescribeNodeExtension( name, method ) );
			  }
			  catch ( PluginLookupException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path(PATH_RELATIONSHIP_EXTENSION_METHOD) public javax.ws.rs.core.Response invokeRelationshipExtension(@PathParam("name") String name, @PathParam("method") String method, @PathParam("relationshipId") long relationshipId, String data)
		 public virtual Response InvokeRelationshipExtension( string name, string method, long relationshipId, string data )
		 {
			  try
			  {
					return _output.ok( this.InvokeRelationshipExtension( relationshipId, name, method, _input.readParameterList( data ) ) );
			  }
			  catch ( Exception e ) when ( e is RelationshipNotFoundException || e is PluginLookupException )
			  {
					return _output.notFound( e );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }
			  catch ( BadPluginInvocationException e )
			  {
					return _output.badRequest( e.InnerException );
			  }
			  catch ( PluginInvocationFailureException e )
			  {
					return _output.serverError( e.InnerException );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(PATH_RELATIONSHIP_EXTENSION_METHOD) public javax.ws.rs.core.Response getRelationshipExtensionDescription(@PathParam("name") String name, @PathParam("method") String method, @PathParam("relationshipId") long relationshipId)
		 public virtual Response GetRelationshipExtensionDescription( string name, string method, long relationshipId )
		 {
			  try
			  {
					return _output.ok( this.DescribeRelationshipExtension( name, method ) );
			  }
			  catch ( PluginLookupException e )
			  {
					return _output.notFound( e );
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

		 // Extensions

		 protected internal virtual Representation ExtensionsList()
		 {
			  return new MappingRepresentationAnonymousInnerClass( this );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly ExtensionService _outerInstance;

			 public MappingRepresentationAnonymousInnerClass( ExtensionService outerInstance ) : base( "extensions" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  foreach ( string extension in _outerInstance.extensions.extensionNames() )
				  {
						serializer.PutRelativeUri( extension, "ext/" + extension );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation extensionList(String extensionName) throws org.neo4j.server.plugins.PluginLookupException
		 protected internal virtual Representation ExtensionList( string extensionName )
		 {
			  return new ServerExtensionRepresentation( extensionName, _extensions.describeAll( extensionName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation invokeGraphDatabaseExtension(String extensionName, String method, org.neo4j.server.plugins.ParameterList data) throws org.neo4j.server.plugins.PluginLookupException, org.neo4j.server.rest.repr.BadInputException, org.neo4j.server.plugins.PluginInvocationFailureException, org.neo4j.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeGraphDatabaseExtension( string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( GraphDatabaseService ), method, _graphDb, data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation describeGraphDatabaseExtension(String extensionName, String method) throws org.neo4j.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeGraphDatabaseExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( GraphDatabaseService ), method );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation invokeNodeExtension(long nodeId, String extensionName, String method, org.neo4j.server.plugins.ParameterList data) throws NodeNotFoundException, org.neo4j.server.plugins.PluginLookupException, org.neo4j.server.rest.repr.BadInputException, org.neo4j.server.plugins.PluginInvocationFailureException, org.neo4j.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeNodeExtension( long nodeId, string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( Node ), method, Node( nodeId ), data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation describeNodeExtension(String extensionName, String method) throws org.neo4j.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeNodeExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( Node ), method );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation invokeRelationshipExtension(long relationshipId, String extensionName, String method, org.neo4j.server.plugins.ParameterList data) throws RelationshipNotFoundException, org.neo4j.server.plugins.PluginLookupException, org.neo4j.server.rest.repr.BadInputException, org.neo4j.server.plugins.PluginInvocationFailureException, org.neo4j.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeRelationshipExtension( long relationshipId, string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( Relationship ), method, Relationship( relationshipId ), data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.Representation describeRelationshipExtension(String extensionName, String method) throws org.neo4j.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeRelationshipExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( Relationship ), method );
		 }
	}

}