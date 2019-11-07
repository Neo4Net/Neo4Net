using System;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.applytx
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.tools.console.input.ConsoleUtil.NULL_PRINT_STREAM;

	public class DatabaseRebuildToolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory directory = Neo4Net.test.rule.TestDirectory.testDirectory();
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
			  assertEquals( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 2, LastAppliedTx( to ) );
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
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler scheduler = createInitializedScheduler(), PageCache pageCache = createPageCache(fileSystem, scheduler) )
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
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
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

		 private static void SetProperties( IPropertyContainer IEntity, int i )
		 {
			  IEntity.SetProperty( "key", "name" + i );
		 }
	}

}