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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotInTransactionException = Neo4Net.GraphDb.NotInTransactionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using WriteOperationsNotAllowedException = Neo4Net.GraphDb.security.WriteOperationsNotAllowedException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class TestReadOnlyNeo4Net
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple()
		 public virtual void TestSimple()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  FileSystemAbstraction fs = TestDirectory.FileSystem;
			  DbRepresentation someData = CreateSomeData( databaseDir, fs );
			  IGraphDatabaseService readGraphDb = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newEmbeddedDatabaseBuilder(databaseDir).setConfig(GraphDatabaseSettings.read_only, Settings.TRUE).newGraphDatabase();
			  assertEquals( someData, DbRepresentation.of( readGraphDb ) );

			  try
			  {
					  using ( Transaction tx = readGraphDb.BeginTx() )
					  {
						readGraphDb.CreateNode();
						tx.Success();
						fail( "Should have failed" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// good
			  }
			  readGraphDb.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseNotStartInReadOnlyModeWithMissingIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseNotStartInReadOnlyModeWithMissingIndex()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  FileSystemAbstraction fs = TestDirectory.FileSystem;
			  CreateIndex( databaseDir, fs );
			  DeleteIndexFolder( databaseDir, fs );
			  IGraphDatabaseService readGraphDb = null;
			  try
			  {
					readGraphDb = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabaseBuilder(databaseDir).setConfig(GraphDatabaseSettings.read_only, Settings.TRUE).newGraphDatabase();
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					Exception rootCause = Exceptions.rootCause( e );
					assertTrue( rootCause is System.InvalidOperationException );
					assertTrue( rootCause.Message.contains( "Some indexes need to be rebuilt. This is not allowed in read only mode. Please start db in writable mode to rebuild indexes. Indexes " + "needing rebuild:" ) );
			  }
			  finally
			  {
					if ( readGraphDb != null )
					{
						 readGraphDb.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReadOnlyOperationsAndNoTransaction()
		 public virtual void TestReadOnlyOperationsAndNoTransaction()
		 {
			  FileSystemAbstraction fs = TestDirectory.FileSystem;
			  File databaseDir = TestDirectory.databaseDir();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabase(databaseDir);

			  Transaction tx = Db.beginTx();
			  Node node1 = Db.createNode();
			  Node node2 = Db.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, withName( "TEST" ) );
			  node1.SetProperty( "key1", "value1" );
			  rel.SetProperty( "key1", "value1" );
			  tx.Success();
			  tx.Close();

			  // make sure write operations still throw exception
			  try
			  {
					Db.createNode();
					fail( "Write operation and no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					node1.CreateRelationshipTo( node2, withName( "TEST2" ) );
					fail( "Write operation and no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					node1.SetProperty( "key1", "value2" );
					fail( "Write operation and no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }

			  try
			  {
					rel.RemoveProperty( "key1" );
					fail( "Write operation and no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }

			  Transaction transaction = Db.beginTx();
			  assertEquals( node1, Db.getNodeById( node1.Id ) );
			  assertEquals( node2, Db.getNodeById( node2.Id ) );
			  assertEquals( rel, Db.getRelationshipById( rel.Id ) );

			  assertThat( node1, inTx( db, hasProperty( "key1" ).withValue( "value1" ) ) );
			  Relationship loadedRel = node1.GetSingleRelationship( withName( "TEST" ), Direction.OUTGOING );
			  assertEquals( rel, loadedRel );
			  assertThat( loadedRel, inTx( db, hasProperty( "key1" ).withValue( "value1" ) ) );
			  transaction.Close();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteIndexFolder(java.io.File databaseDir, Neo4Net.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 private void DeleteIndexFolder( File databaseDir, FileSystemAbstraction fs )
		 {
			  fs.DeleteRecursively( IndexDirectoryStructure.baseSchemaIndexFolder( databaseDir ) );
		 }

		 private void CreateIndex( File databaseDir, FileSystemAbstraction fs )
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newEmbeddedDatabase(databaseDir);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(Label.label("label")).on("prop").create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
			  Db.shutdown();
		 }

		 private DbRepresentation CreateSomeData( File databaseDir, FileSystemAbstraction fs )
		 {
			  RelationshipType type = withName( "KNOWS" );
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabase(databaseDir);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node prevNode = Db.createNode();
					for ( int i = 0; i < 100; i++ )
					{
						 Node node = Db.createNode();
						 Relationship rel = prevNode.CreateRelationshipTo( node, type );
						 node.SetProperty( "someKey" + i % 10, i % 15 );
						 rel.SetProperty( "since", DateTimeHelper.CurrentUnixTimeMillis() );
					}
					tx.Success();
			  }
			  DbRepresentation result = DbRepresentation.of( db );
			  Db.shutdown();
			  return result;
		 }
	}

}