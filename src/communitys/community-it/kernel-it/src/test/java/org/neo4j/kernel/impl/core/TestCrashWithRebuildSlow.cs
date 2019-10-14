using System;
using System.Collections.Generic;
using System.IO;

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
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using Neo4Net.Kernel.impl.store;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	/// <summary>
	/// Test for making sure that slow id generator rebuild is exercised
	/// </summary>
	public class TestCrashWithRebuildSlow
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void crashAndRebuildSlowWithDynamicStringDeletions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CrashAndRebuildSlowWithDynamicStringDeletions()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(testDir.databaseDir()).setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabaseBuilder(TestDir.databaseDir()).setConfig(GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
			  IList<long> deletedNodeIds = ProduceNonCleanDefraggedStringStore( db );
			  IDictionary<IdType, long> highIdsBeforeCrash = GetHighIds( db );

			  // Make sure all of our changes are actually written to the files, since any background flushing could
			  // mess up the check-sums in non-deterministic ways
			  Db.DependencyResolver.resolveDependency( typeof( PageCache ) ).flushAndForce();

			  long checksumBefore = Fs.get().checksum();
			  long checksumBefore2 = Fs.get().checksum();

			  assertThat( checksumBefore, Matchers.equalTo( checksumBefore2 ) );

			  EphemeralFileSystemAbstraction snapshot = Fs.snapshot( Db.shutdown );

			  long snapshotChecksum = snapshot.Checksum();
			  if ( snapshotChecksum != checksumBefore )
			  {
					using ( Stream @out = new FileStream( TestDir.file( "snapshot.zip" ), FileMode.Create, FileAccess.Write ) )
					{
						 snapshot.DumpZip( @out );
					}
					using ( Stream @out = new FileStream( TestDir.file( "fs.zip" ), FileMode.Create, FileAccess.Write ) )
					{
						 Fs.get().dumpZip(@out);
					}
			  }
			  assertThat( snapshotChecksum, equalTo( checksumBefore ) );

			  // Recover with unsupported.dbms.id_generator_fast_rebuild_enabled=false
			  AssertNumberOfFreeIdsEquals( TestDir.databaseDir(), snapshot, 0 );
			  GraphDatabaseAPI newDb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(snapshot).newImpermanentDatabaseBuilder(TestDir.databaseDir()).setConfig(GraphDatabaseSettings.rebuild_idgenerators_fast, FALSE).newGraphDatabase();
			  IDictionary<IdType, long> highIdsAfterCrash = GetHighIds( newDb );
			  assertEquals( highIdsBeforeCrash, highIdsAfterCrash );

			  try
			  {
					  using ( Transaction tx = newDb.BeginTx() )
					  {
						// Verify that the data we didn't delete is still around
						int nameCount = 0;
						int relCount = 0;
						foreach ( Node node in newDb.AllNodes )
						{
							 nameCount++;
							 assertThat( node, inTx( newDb, hasProperty( "name" ), true ) );
							 relCount += ( int )Iterables.count( node.GetRelationships( Direction.OUTGOING ) );
						}
      
						assertEquals( 16, nameCount );
						assertEquals( 12, relCount );
      
						// Verify that the ids of the nodes we deleted are reused
						IList<long> newIds = new List<long>();
						newIds.Add( newDb.CreateNode().Id );
						newIds.Add( newDb.CreateNode().Id );
						newIds.Add( newDb.CreateNode().Id );
						newIds.Add( newDb.CreateNode().Id );
						assertThat( newIds, @is( deletedNodeIds ) );
						tx.Success();
					  }
			  }
			  finally
			  {
					newDb.Shutdown();
					snapshot.Dispose();
			  }
		 }

		 private static IList<long> ProduceNonCleanDefraggedStringStore( GraphDatabaseService db )
		 {
			  // Create some strings
			  IList<Node> nodes = new List<Node>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node previous = null;
					for ( int i = 0; i < 20; i++ )
					{
						 Node node = Db.createNode();
						 node.SetProperty( "name", "a looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong" + " string" );
						 nodes.Add( node );
						 if ( previous != null )
						 {
							  Relationship rel = previous.CreateRelationshipTo( node, MyRelTypes.TEST );
						 }
						 previous = node;
					}
					tx.Success();
			  }

			  // Delete some of them, but leave some in between deletions
			  IList<long> deletedNodeIds = new List<long>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node a = nodes[5];
					Node b = nodes[7];
					Node c = nodes[8];
					Node d = nodes[10];
					deletedNodeIds.Add( a.Id );
					deletedNodeIds.Add( b.Id );
					deletedNodeIds.Add( c.Id );
					deletedNodeIds.Add( d.Id );
					Delete( a );
					Delete( b );
					Delete( c );
					Delete( d );
					tx.Success();
			  }
			  return deletedNodeIds;
		 }

		 private static void Delete( Node node )
		 {
			  foreach ( Relationship rel in node.Relationships )
			  {
					rel.Delete();
			  }
			  node.Delete();
		 }

		 private static IDictionary<IdType, long> GetHighIds( GraphDatabaseAPI db )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<org.neo4j.kernel.impl.store.id.IdType,long> highIds = new java.util.HashMap<>();
			  IDictionary<IdType, long> highIds = new Dictionary<IdType, long>();
			  NeoStores neoStores = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  Visitor<CommonAbstractStore, Exception> visitor = store =>
			  {
				highIds[store.IdType] = store.HighId;
				return true;
			  };
			  neoStores.VisitStore( visitor );
			  return highIds;
		 }

		 private static void AssertNumberOfFreeIdsEquals( File databaseDirectory, FileSystemAbstraction fs, long numberOfFreeIds )
		 {
			  long fileSize = fs.GetFileSize( new File( databaseDirectory, "neostore.propertystore.db.strings.id" ) );
			  long fileSizeWithoutHeader = fileSize - 9;
			  long actualFreeIds = fileSizeWithoutHeader / 8;

			  assertThat( "Id file should at least have a 9 byte header", fileSize, greaterThanOrEqualTo( 9L ) );
			  assertThat( "File should contain the expected number of free ids", actualFreeIds, @is( numberOfFreeIds ) );
			  assertThat( "File size should not contain more bytes than expected", 8 * numberOfFreeIds, @is( fileSizeWithoutHeader ) );
		 }
	}

}