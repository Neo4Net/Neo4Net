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
namespace Neo4Net.Kernel.impl.store
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ImpermanentGraphDatabase = Neo4Net.Test.ImpermanentGraphDatabase;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.RecordStore.getRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public class RelationshipGroupStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule(config().withInconsistentReads(false));
		 public PageCacheRule PageCacheRule = new PageCacheRule( config().withInconsistentReads(false) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();
		 private int _defaultThreshold;
		 private FileSystemAbstraction _fs;
		 private ImpermanentGraphDatabase _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _fs = new DefaultFileSystemAbstraction();
			  _defaultThreshold = parseInt( GraphDatabaseSettings.dense_node_threshold.DefaultValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createWithDefaultThreshold()
		 public virtual void CreateWithDefaultThreshold()
		 {
			  CreateAndVerify( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createWithCustomThreshold()
		 public virtual void CreateWithCustomThreshold()
		 {
			  CreateAndVerify( _defaultThreshold * 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createDenseNodeWithLowThreshold()
		 public virtual void CreateDenseNodeWithLowThreshold()
		 {
			  NewDb( 2 );

			  // Create node with two relationships
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
					assertEquals( 2, node.Degree );
					assertEquals( 1, node.GetDegree( MyRelTypes.TEST ) );
					assertEquals( 1, node.GetDegree( MyRelTypes.TEST2 ) );
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					tx.Success();
			  }

			  _db.shutdown();
		 }

		 private void NewDb( int denseNodeThreshold )
		 {
			  _db = new ImpermanentGraphDatabase( MapUtil.stringMap( "dbms.relationship_grouping_threshold", "" + denseNodeThreshold ) );
			  _fs = _db.DependencyResolver.resolveDependency( typeof( FileSystemAbstraction ) );
		 }

		 private void CreateAndVerify( int? customThreshold )
		 {
			  int expectedThreshold = customThreshold != null ? customThreshold.Value : _defaultThreshold;
			  StoreFactory factory = factory( customThreshold );
			  NeoStores neoStores = factory.OpenAllNeoStores( true );
			  assertEquals( expectedThreshold, neoStores.RelationshipGroupStore.StoreHeaderInt );
			  neoStores.Close();

			  // Next time we open it it should be the same
			  neoStores = factory.OpenAllNeoStores();
			  assertEquals( expectedThreshold, neoStores.RelationshipGroupStore.StoreHeaderInt );
			  neoStores.Close();

			  // Even if we open with a different config setting it should just ignore it
			  factory = factory( 999999 );
			  neoStores = factory.OpenAllNeoStores();
			  assertEquals( expectedThreshold, neoStores.RelationshipGroupStore.StoreHeaderInt );
			  neoStores.Close();
		 }

		 private StoreFactory Factory( int? customThreshold )
		 {
			  return Factory( customThreshold, PageCacheRule.getPageCache( _fs ) );
		 }

		 private StoreFactory Factory( int? customThreshold, PageCache pageCache )
		 {
			  IDictionary<string, string> customConfig = new Dictionary<string, string>();
			  if ( customThreshold != null )
			  {
					customConfig[GraphDatabaseSettings.dense_node_threshold.name()] = "" + customThreshold;
			  }
			  return new StoreFactory( TestDir.databaseLayout(), Config.defaults(customConfig), new DefaultIdGeneratorFactory(_fs), pageCache, _fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRelationshipGroupsNextAndPrevGetsAssignedCorrectly()
		 public virtual void MakeSureRelationshipGroupsNextAndPrevGetsAssignedCorrectly()
		 {
			  NewDb( 1 );

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					Node node0 = _db.createNode();
					Node node2 = _db.createNode();
					node0.CreateRelationshipTo( node, MyRelTypes.TEST );
					node.CreateRelationshipTo( node2, MyRelTypes.TEST2 );

					foreach ( Relationship rel in node.Relationships )
					{
						 rel.Delete();
					}
					node.Delete();
					tx.Success();
			  }

			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyRecordsForDenseNodeWithOneRelType()
		 public virtual void VerifyRecordsForDenseNodeWithOneRelType()
		 {
			  NewDb( 2 );

			  Node node;
			  Relationship rel1;
			  Relationship rel2;
			  Relationship rel3;
			  Relationship rel4;
			  Relationship rel5;
			  Relationship rel6;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					rel1 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					rel2 = _db.createNode().createRelationshipTo(node, MyRelTypes.TEST);
					rel3 = node.CreateRelationshipTo( node, MyRelTypes.TEST );
					rel4 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					rel5 = _db.createNode().createRelationshipTo(node, MyRelTypes.TEST);
					rel6 = node.CreateRelationshipTo( node, MyRelTypes.TEST );
					tx.Success();
			  }

			  NeoStores neoStores = _db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  NodeStore nodeStore = neoStores.NodeStore;
			  NodeRecord nodeRecord = getRecord( nodeStore, node.Id );
			  long group = nodeRecord.NextRel;
			  RecordStore<RelationshipGroupRecord> groupStore = neoStores.RelationshipGroupStore;
			  RelationshipGroupRecord groupRecord = getRecord( groupStore, group );
			  assertEquals( -1, groupRecord.Next );
			  assertEquals( -1, groupRecord.Prev );
			  AssertRelationshipChain( neoStores.RelationshipStore, node, groupRecord.FirstOut, rel1.Id, rel4.Id );
			  AssertRelationshipChain( neoStores.RelationshipStore, node, groupRecord.FirstIn, rel2.Id, rel5.Id );
			  AssertRelationshipChain( neoStores.RelationshipStore, node, groupRecord.FirstLoop, rel3.Id, rel6.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyRecordsForDenseNodeWithTwoRelTypes()
		 public virtual void VerifyRecordsForDenseNodeWithTwoRelTypes()
		 {
			  NewDb( 2 );

			  Node node;
			  Relationship rel1;
			  Relationship rel2;
			  Relationship rel3;
			  Relationship rel4;
			  Relationship rel5;
			  Relationship rel6;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					rel1 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					rel2 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					rel3 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
					rel4 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
					rel5 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
					rel6 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
					tx.Success();
			  }

			  NeoStores neoStores = _db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  NodeStore nodeStore = neoStores.NodeStore;
			  NodeRecord nodeRecord = getRecord( nodeStore, node.Id );
			  long group = nodeRecord.NextRel;

			  RecordStore<RelationshipGroupRecord> groupStore = neoStores.RelationshipGroupStore;
			  RelationshipGroupRecord groupRecord = getRecord( groupStore, group );
			  assertNotEquals( groupRecord.Next, -1 );
			  AssertRelationshipChain( neoStores.RelationshipStore, node, groupRecord.FirstOut, rel1.Id, rel2.Id, rel3.Id );

			  RelationshipGroupRecord otherGroupRecord = RecordStore.getRecord( groupStore, groupRecord.Next );
			  assertEquals( -1, otherGroupRecord.Next );
			  AssertRelationshipChain( neoStores.RelationshipStore, node, otherGroupRecord.FirstOut, rel4.Id, rel5.Id, rel6.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyGroupIsDeletedWhenNeeded()
		 public virtual void VerifyGroupIsDeletedWhenNeeded()
		 {
			  // TODO test on a lower level instead

			  NewDb( 2 );

			  Transaction tx = _db.beginTx();
			  Node node = _db.createNode();
			  Relationship rel1 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
			  Relationship rel2 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
			  Relationship rel3 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST );
			  Relationship rel4 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
			  Relationship rel5 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
			  Relationship rel6 = node.CreateRelationshipTo( _db.createNode(), MyRelTypes.TEST2 );
			  tx.Success();
			  tx.Close();

			  NeoStores neoStores = _db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  NodeStore nodeStore = neoStores.NodeStore;
			  NodeRecord nodeRecord = getRecord( nodeStore, node.Id );
			  long group = nodeRecord.NextRel;

			  RecordStore<RelationshipGroupRecord> groupStore = neoStores.RelationshipGroupStore;
			  RelationshipGroupRecord groupRecord = getRecord( groupStore, group );
			  assertNotEquals( groupRecord.Next, -1 );
			  RelationshipGroupRecord otherGroupRecord = groupStore.GetRecord( groupRecord.Next, groupStore.NewRecord(), NORMAL );
			  assertEquals( -1, otherGroupRecord.Next );

			  // TODO Delete all relationships of one type and see to that the correct group is deleted.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkingIfRecordIsInUseMustHappenAfterConsistentRead()
		 public virtual void CheckingIfRecordIsInUseMustHappenAfterConsistentRead()
		 {
			  AtomicBoolean nextReadIsInconsistent = new AtomicBoolean( false );
			  PageCache pageCache = PageCacheRule.getPageCache( _fs, config().withInconsistentReads(nextReadIsInconsistent) );
			  StoreFactory factory = factory( null, pageCache );

			  using ( NeoStores neoStores = factory.OpenAllNeoStores( true ) )
			  {
					RecordStore<RelationshipGroupRecord> relationshipGroupStore = neoStores.RelationshipGroupStore;
					RelationshipGroupRecord record = ( new RelationshipGroupRecord( 1 ) ).initialize( true, 2, 3, 4, 5, 6, Record.NO_NEXT_RELATIONSHIP.intValue() );
					relationshipGroupStore.UpdateRecord( record );
					nextReadIsInconsistent.set( true );
					// Now the following should not throw any RecordNotInUse exceptions
					RelationshipGroupRecord readBack = relationshipGroupStore.GetRecord( 1, relationshipGroupStore.NewRecord(), NORMAL );
					assertThat( readBack.ToString(), equalTo(record.ToString()) );
			  }
		 }

		 private void AssertRelationshipChain( RelationshipStore relationshipStore, Node node, long firstId, params long[] chainedIds )
		 {
			  long nodeId = node.Id;
			  RelationshipRecord record = relationshipStore.GetRecord( firstId, relationshipStore.NewRecord(), NORMAL );
			  ISet<long> readChain = new HashSet<long>();
			  readChain.Add( firstId );
			  while ( true )
			  {
					long nextId = record.FirstNode == nodeId ? record.FirstNextRel : record.SecondNextRel;
					if ( nextId == -1 )
					{
						 break;
					}

					readChain.Add( nextId );
					relationshipStore.GetRecord( nextId, record, NORMAL );
			  }

			  ISet<long> expectedChain = new HashSet<long>( asList( firstId ) );
			  foreach ( long id in chainedIds )
			  {
					expectedChain.Add( id );
			  }
			  assertEquals( expectedChain, readChain );
		 }
	}

}