using System;

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
namespace Neo4Net.Server.rest.web
{

	using SyntaxException = Neo4Net.Cypher.SyntaxException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Database = Neo4Net.Server.database.Database;
	using BadPluginInvocationException = Neo4Net.Server.plugins.BadPluginInvocationException;
	using ParameterList = Neo4Net.Server.plugins.ParameterList;
	using PluginInvocationFailureException = Neo4Net.Server.plugins.PluginInvocationFailureException;
	using PluginInvocator = Neo4Net.Server.plugins.PluginInvocator;
	using PluginLookupException = Neo4Net.Server.plugins.PluginLookupException;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using InputFormat = Neo4Net.Server.rest.repr.InputFormat;
	using MappingRepresentation = Neo4Net.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Neo4Net.Server.rest.repr.MappingSerializer;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using ServerExtensionRepresentation = Neo4Net.Server.rest.repr.ServerExtensionRepresentation;

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
//ORIGINAL LINE: private org.Neo4Net.graphdb.Node node(long id) throws NodeNotFoundException
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
//ORIGINAL LINE: private org.Neo4Net.graphdb.Relationship relationship(long id) throws RelationshipNotFoundException
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
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation extensionList(String extensionName) throws org.Neo4Net.server.plugins.PluginLookupException
		 protected internal virtual Representation ExtensionList( string extensionName )
		 {
			  return new ServerExtensionRepresentation( extensionName, _extensions.describeAll( extensionName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation invokeGraphDatabaseExtension(String extensionName, String method, org.Neo4Net.server.plugins.ParameterList data) throws org.Neo4Net.server.plugins.PluginLookupException, org.Neo4Net.server.rest.repr.BadInputException, org.Neo4Net.server.plugins.PluginInvocationFailureException, org.Neo4Net.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeGraphDatabaseExtension( string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( IGraphDatabaseService ), method, _graphDb, data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation describeGraphDatabaseExtension(String extensionName, String method) throws org.Neo4Net.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeGraphDatabaseExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( IGraphDatabaseService ), method );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation invokeNodeExtension(long nodeId, String extensionName, String method, org.Neo4Net.server.plugins.ParameterList data) throws NodeNotFoundException, org.Neo4Net.server.plugins.PluginLookupException, org.Neo4Net.server.rest.repr.BadInputException, org.Neo4Net.server.plugins.PluginInvocationFailureException, org.Neo4Net.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeNodeExtension( long nodeId, string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( Node ), method, Node( nodeId ), data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation describeNodeExtension(String extensionName, String method) throws org.Neo4Net.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeNodeExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( Node ), method );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation invokeRelationshipExtension(long relationshipId, String extensionName, String method, org.Neo4Net.server.plugins.ParameterList data) throws RelationshipNotFoundException, org.Neo4Net.server.plugins.PluginLookupException, org.Neo4Net.server.rest.repr.BadInputException, org.Neo4Net.server.plugins.PluginInvocationFailureException, org.Neo4Net.server.plugins.BadPluginInvocationException
		 protected internal virtual Representation InvokeRelationshipExtension( long relationshipId, string extensionName, string method, ParameterList data )
		 {
			  return _extensions.invoke( _graphDb, extensionName, typeof( Relationship ), method, Relationship( relationshipId ), data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.rest.repr.Representation describeRelationshipExtension(String extensionName, String method) throws org.Neo4Net.server.plugins.PluginLookupException
		 protected internal virtual Representation DescribeRelationshipExtension( string extensionName, string method )
		 {
			  return _extensions.describe( extensionName, typeof( Relationship ), method );
		 }
	}

}