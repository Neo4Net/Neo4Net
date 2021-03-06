﻿using System;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.tools.applytx
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.NULL_PRINT_STREAM;

	public class DatabaseRebuildToolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildDbFromTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildDbFromTransactions()
		 {
			  // This tests the basic functionality of this tool, there are more things, but it's not as important
			  // to test as the functionality of applying transactions.

			  // GIVEN
			  File from = Directory.directory( "from" );
			  File to = Directory.directory( "to" );
			  DatabaseWithSomeTransactions( from );
			  DatabaseRebuildTool tool = new DatabaseRebuildTool( System.in, NULL_PRINT_STREAM, NULL_PRINT_STREAM );

			  // WHEN
			  tool.Run( "--from", DatabaseDirectory( from ).AbsolutePath, "--to", to.AbsolutePath, "apply last" );

			  // THEN
			  assertEquals( DbRepresentation.of( DatabaseDirectory( from ) ), DbRepresentation.of( DatabaseDirectory( to ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplySomeTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplySomeTransactions()
		 {
			  // This tests the basic functionality of this tool, there are more things, but it's not as important
			  // to test as the functionality of applying transactions.

			  // GIVEN
			  File from = Directory.directory( "from" );
			  DatabaseLayout to = Directory.databaseLayout( "to" );
			  DatabaseWithSomeTransactions( from );
			  DatabaseRebuildTool tool = new DatabaseRebuildTool( Input( "apply next", "apply next", "cc", "exit" ), NULL_PRINT_STREAM, NULL_PRINT_STREAM );

			  // WHEN
			  tool.Run( "--from", from.AbsolutePath, "--to", to.DatabaseDirectory().Path, "-i" );

			  // THEN
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 2, LastAppliedTx( to ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpNodePropertyChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpNodePropertyChain()
		 {
			  ShouldPrint( "dump node properties 0", "Property", "name0" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpRelationshipPropertyChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpRelationshipPropertyChain()
		 {
			  ShouldPrint( "dump relationship properties 0", "Property", "name0" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpRelationships()
		 {
			  ShouldPrint( "dump node relationships 0", "Relationship[0,", "Relationship[10," );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpRelationshipTypeTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpRelationshipTypeTokens()
		 {
			  ShouldPrint( "dump tokens relationship-type", "TYPE_0", "TYPE_1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpLabelTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpLabelTokens()
		 {
			  ShouldPrint( "dump tokens label", "Label_0", "Label_1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpPropertyKeyTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpPropertyKeyTokens()
		 {
			  ShouldPrint( "dump tokens property-key", "name" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldPrint(String command, String... expectedResultContaining) throws Exception
		 private void ShouldPrint( string command, params string[] expectedResultContaining )
		 {
			  // GIVEN
			  File from = Directory.directory( "from" );
			  File to = Directory.directory( "to" );
			  DatabaseWithSomeTransactions( to );
			  MemoryStream byteArrayOut = new MemoryStream();
			  PrintStream @out = new PrintStream( byteArrayOut );
			  DatabaseRebuildTool tool = new DatabaseRebuildTool( Input( command, "exit" ), @out, NULL_PRINT_STREAM );

			  // WHEN
			  tool.Run( "--from", from.AbsolutePath, "--to", to.AbsolutePath, "-i" );

			  // THEN
			  @out.flush();
			  string dump = StringHelper.NewString( byteArrayOut.toByteArray() );
			  foreach ( string @string in expectedResultContaining )
			  {
					assertThat( "dump from command '" + command + "'", dump, containsString( @string ) );
			  }
		 }

		 private File DatabaseDirectory( File storeDir )
		 {
			  return Directory.databaseDir( storeDir );
		 }

		 private static long LastAppliedTx( DatabaseLayout databaseLayout )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), JobScheduler scheduler = createInitialisedScheduler(), PageCache pageCache = createPageCache(fileSystem, scheduler) )
					  {
						return MetaDataStore.getRecord( pageCache, databaseLayout.MetadataStore(), MetaDataStore.Position.LAST_TRANSACTION_ID );
					  }
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static Stream Input( params string[] strings )
		 {
			  StringBuilder all = new StringBuilder();
			  foreach ( string @string in strings )
			  {
					all.Append( @string ).Append( format( "%n" ) );
			  }
			  return new MemoryStream( all.ToString().GetBytes() );
		 }

		 private static void DatabaseWithSomeTransactions( File storeDir )
		 {
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
			  Node[] nodes = new Node[10];
			  for ( int i = 0; i < nodes.Length; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.createNode( label( "Label_" + ( i % 2 ) ) );
						 SetProperties( node, i );
						 nodes[i] = node;
						 tx.Success();
					}
			  }
			  for ( int i = 0; i < 40; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Relationship relationship = nodes[i % nodes.Length].CreateRelationshipTo( nodes[( i + 1 ) % nodes.Length], withName( "TYPE_" + ( i % 3 ) ) );
						 SetProperties( relationship, i );
						 tx.Success();
					}
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = nodes[nodes.Length - 1];
					foreach ( Relationship relationship in node.Relationships )
					{
						 relationship.Delete();
					}
					node.Delete();
					tx.Success();
			  }
			  Db.shutdown();
		 }

		 private static void SetProperties( PropertyContainer entity, int i )
		 {
			  entity.SetProperty( "key", "name" + i );
		 }
	}

}