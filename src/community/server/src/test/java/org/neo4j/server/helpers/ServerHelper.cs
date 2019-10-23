﻿using System;
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
namespace Neo4Net.Server.helpers
{
	using FileUtils = org.apache.commons.io.FileUtils;


	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class ServerHelper
	{

		 private ServerHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static void cleanTheDatabase(final org.Neo4Net.server.NeoServer server)
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
//ORIGINAL LINE: public static org.Neo4Net.server.NeoServer createNonPersistentServer() throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer()
		 {
			  return CreateServer( CommunityServerBuilder.Server(), false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.Neo4Net.server.NeoServer createReadOnlyServer(java.io.File path) throws java.io.IOException
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
//ORIGINAL LINE: public static org.Neo4Net.server.NeoServer createNonPersistentServer(org.Neo4Net.logging.LogProvider logProvider) throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer( LogProvider logProvider )
		 {
			  return CreateServer( CommunityServerBuilder.Server( logProvider ), false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.Neo4Net.server.NeoServer createNonPersistentServer(CommunityServerBuilder builder) throws java.io.IOException
		 public static NeoServer CreateNonPersistentServer( CommunityServerBuilder builder )
		 {
			  return CreateServer( builder, false, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.Neo4Net.server.NeoServer createServer(CommunityServerBuilder builder, boolean persistent, java.io.File path) throws java.io.IOException
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