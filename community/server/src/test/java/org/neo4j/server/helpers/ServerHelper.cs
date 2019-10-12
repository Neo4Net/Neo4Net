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
namespace Org.Neo4j.Server.helpers
{
	using FileUtils = org.apache.commons.io.FileUtils;


	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class ServerHelper
	{

		 private ServerHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static void cleanTheDatabase(final org.neo4j.server.NeoServer server)
		 public static void CleanTheDatabase( NeoServer server )
		 {
			  if ( server == null )
			  {
					return;
			  }

			  RollbackAllOpenTransactions( server );

			  CleanTheDatabase( server.Database.Graph );

			  RemoveLogs( server );
		 }

		 public static void CleanTheDatabase( GraphDatabaseAPI db )
		 {
			  ( new Transactor( db, new DeleteAllData( db ), 10 ) ).Execute();
			  ( new Transactor( db, new DeleteAllSchema( db ), 10 ) ).Execute();
		 }

		 private static void RemoveLogs( NeoServer server )
		 {
			  File logDir = new File( server.Database.Location + File.separator + ".." + File.separator + "log" );
			  try
			  {
					FileUtils.deleteDirectory( logDir );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.server.NeoServer createNonPersistentServer() throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer()
		 {
			  return CreateServer( CommunityServerBuilder.Server(), false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.server.NeoServer createReadOnlyServer(java.io.File path) throws java.io.IOException
		 public static NeoServer CreateReadOnlyServer( File path )
		 {
			  // Start writable server to create all store files needed
			  CommunityServerBuilder builder = CommunityServerBuilder.Server();
			  builder.WithProperty( "dbms.connector.bolt.listen_address", ":0" );
			  CreateServer( builder, true, path ).stop();
			  // Then start server in read only mode
			  builder.WithProperty( GraphDatabaseSettings.read_only.name(), "true" );
			  return CreateServer( builder, true, path );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.server.NeoServer createNonPersistentServer(org.neo4j.logging.LogProvider logProvider) throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer( LogProvider logProvider )
		 {
			  return CreateServer( CommunityServerBuilder.Server( logProvider ), false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.server.NeoServer createNonPersistentServer(CommunityServerBuilder builder) throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer( CommunityServerBuilder builder )
		 {
			  return CreateServer( builder, false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.server.NeoServer createServer(CommunityServerBuilder builder, boolean persistent, java.io.File path) throws java.io.IOException
		 private static NeoServer CreateServer( CommunityServerBuilder builder, bool persistent, File path )
		 {
			  if ( persistent )
			  {
					builder = builder.Persistent();
			  }
			  builder.OnRandomPorts();
			  NeoServer server = builder.UsingDataDir( path != null ? path.AbsolutePath : null ).build();

			  server.Start();
			  return server;
		 }

		 private static void RollbackAllOpenTransactions( NeoServer server )
		 {
			  server.TransactionRegistry.rollbackAllSuspendedTransactions();
		 }

		 private class DeleteAllData : UnitOfWork
		 {
			  internal readonly GraphDatabaseAPI Db;

			  internal DeleteAllData( GraphDatabaseAPI db )
			  {
					this.Db = db;
			  }

			  public override void DoWork()
			  {
					DeleteAllNodesAndRelationships();
					DeleteAllIndexes();
			  }

			  internal virtual void DeleteAllNodesAndRelationships()
			  {
					IEnumerable<Node> allNodes = Db.AllNodes;
					foreach ( Node n in allNodes )
					{
						 IEnumerable<Relationship> relationships = n.Relationships;
						 foreach ( Relationship rel in relationships )
						 {
							  rel.Delete();
						 }
						 n.Delete();
					}
			  }

			  internal virtual void DeleteAllIndexes()
			  {
					IndexManager indexManager = Db.index();

					foreach ( string indexName in indexManager.NodeIndexNames() )
					{
						 try
						 {
							  Db.index().forNodes(indexName).delete();
						 }
						 catch ( System.NotSupportedException )
						 {
							  // Encountered a read-only index.
						 }
					}

					foreach ( string indexName in indexManager.RelationshipIndexNames() )
					{
						 try
						 {
							  Db.index().forRelationships(indexName).delete();
						 }
						 catch ( System.NotSupportedException )
						 {
							  // Encountered a read-only index.
						 }
					}

					foreach ( string k in indexManager.NodeAutoIndexer.AutoIndexedProperties )
					{
						 indexManager.NodeAutoIndexer.stopAutoIndexingProperty( k );
					}
					indexManager.NodeAutoIndexer.Enabled = false;

					foreach ( string k in indexManager.RelationshipAutoIndexer.AutoIndexedProperties )
					{
						 indexManager.RelationshipAutoIndexer.stopAutoIndexingProperty( k );
					}
					indexManager.RelationshipAutoIndexer.Enabled = false;
			  }
		 }

		 private class DeleteAllSchema : UnitOfWork
		 {
			  internal readonly GraphDatabaseAPI Db;

			  internal DeleteAllSchema( GraphDatabaseAPI db )
			  {
					this.Db = db;
			  }

			  public override void DoWork()
			  {
					DeleteAllIndexRules();
					DeleteAllConstraints();
			  }

			  internal virtual void DeleteAllIndexRules()
			  {
					foreach ( IndexDefinition index in Db.schema().Indexes )
					{
						 if ( !index.ConstraintIndex )
						 {
							  index.Drop();
						 }
					}
			  }

			  internal virtual void DeleteAllConstraints()
			  {
					foreach ( ConstraintDefinition constraint in Db.schema().Constraints )
					{
						 constraint.Drop();
					}
			  }
		 }
	}

}