using System;

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
namespace Org.Neo4j.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotInTransactionException = Org.Neo4j.Graphdb.NotInTransactionException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using WriteOperationsNotAllowedException = Org.Neo4j.Graphdb.security.WriteOperationsNotAllowedException;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class TestReadOnlyNeo4j
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple()
		 public virtual void TestSimple()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  FileSystemAbstraction fs = TestDirectory.FileSystem;
			  DbRepresentation someData = CreateSomeData( databaseDir, fs );
			  GraphDatabaseService readGraphDb = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newEmbeddedDatabaseBuilder(databaseDir).setConfig(GraphDatabaseSettings.read_only, Settings.TRUE).newGraphDatabase();
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
			  GraphDatabaseService readGraphDb = null;
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
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabase(databaseDir);

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
//ORIGINAL LINE: private void deleteIndexFolder(java.io.File databaseDir, org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 private void DeleteIndexFolder( File databaseDir, FileSystemAbstraction fs )
		 {
			  fs.DeleteRecursively( IndexDirectoryStructure.baseSchemaIndexFolder( databaseDir ) );
		 }

		 private void CreateIndex( File databaseDir, FileSystemAbstraction fs )
		 {
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newEmbeddedDatabase(databaseDir);
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
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabase(databaseDir);
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