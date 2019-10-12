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
namespace Org.Neo4j.Consistency.repair
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.impl.store;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class RelationshipChainExplorerTest
	{
		private bool InstanceFieldsInitialized = false;

		public RelationshipChainExplorerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule );
		}

		 private const int DEGREE_TWO_NODES = 10;

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule);
		 public RuleChain RuleChain;

		 private StoreAccess _store;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupStoreAccess()
		 public virtual void SetupStoreAccess()
		 {
			  _store = CreateStoreWithOneHighDegreeNodeAndSeveralDegreeTwoNodes( DEGREE_TWO_NODES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDownStoreAccess()
		 public virtual void TearDownStoreAccess()
		 {
			  _store.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadAllConnectedRelationshipRecordsAndTheirFullChainsOfRelationshipRecords()
		 public virtual void ShouldLoadAllConnectedRelationshipRecordsAndTheirFullChainsOfRelationshipRecords()
		 {
			  // given
			  RecordStore<RelationshipRecord> relationshipStore = _store.RelationshipStore;

			  // when
			  int relationshipIdInMiddleOfChain = 10;
			  RecordSet<RelationshipRecord> records = ( new RelationshipChainExplorer( relationshipStore ) ).ExploreRelationshipRecordChainsToDepthTwo( relationshipStore.GetRecord( relationshipIdInMiddleOfChain, relationshipStore.NewRecord(), NORMAL ) );

			  // then
			  assertEquals( DEGREE_TWO_NODES * 2, records.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopeWithAChainThatReferencesNotInUseZeroValueRecords()
		 public virtual void ShouldCopeWithAChainThatReferencesNotInUseZeroValueRecords()
		 {
			  // given
			  RecordStore<RelationshipRecord> relationshipStore = _store.RelationshipStore;
			  BreakTheChain( relationshipStore );

			  // when
			  int relationshipIdInMiddleOfChain = 10;
			  RecordSet<RelationshipRecord> records = ( new RelationshipChainExplorer( relationshipStore ) ).ExploreRelationshipRecordChainsToDepthTwo( relationshipStore.GetRecord( relationshipIdInMiddleOfChain, relationshipStore.NewRecord(), NORMAL ) );

			  // then
			  int recordsInaccessibleBecauseOfBrokenChain = 3;
			  assertEquals( DEGREE_TWO_NODES * 2 - recordsInaccessibleBecauseOfBrokenChain, records.Size() );
		 }

		 private static void BreakTheChain( RecordStore<RelationshipRecord> relationshipStore )
		 {
			  RelationshipRecord record = relationshipStore.GetRecord( 10, relationshipStore.NewRecord(), NORMAL );
			  long relationshipTowardsEndOfChain = record.FirstNode;
			  while ( record.InUse() && !record.FirstInFirstChain )
			  {
					record = relationshipStore.GetRecord( relationshipTowardsEndOfChain, relationshipStore.NewRecord(), FORCE );
					relationshipTowardsEndOfChain = record.FirstPrevRel;
			  }

			  relationshipStore.UpdateRecord( new RelationshipRecord( relationshipTowardsEndOfChain, 0, 0, 0 ) );
		 }

		 internal enum TestRelationshipType
		 {
			  Connected
		 }

		 private StoreAccess CreateStoreWithOneHighDegreeNodeAndSeveralDegreeTwoNodes( int nDegreeTwoNodes )
		 {
			  File storeDirectory = _testDirectory.databaseDir();
			  GraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDirectory).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig("dbms.backup.enabled", "false").newGraphDatabase();

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node denseNode = database.CreateNode();
					for ( int i = 0; i < nDegreeTwoNodes; i++ )
					{
						 Node degreeTwoNode = database.CreateNode();
						 Node leafNode = database.CreateNode();
						 if ( i % 2 == 0 )
						 {
							  denseNode.CreateRelationshipTo( degreeTwoNode, TestRelationshipType.Connected );
						 }
						 else
						 {
							  degreeTwoNode.CreateRelationshipTo( denseNode, TestRelationshipType.Connected );
						 }
						 degreeTwoNode.CreateRelationshipTo( leafNode, TestRelationshipType.Connected );
					}
					transaction.Success();
			  }
			  database.Shutdown();
			  PageCache pageCache = PageCacheRule.getPageCache( _fileSystemRule.get() );
			  StoreAccess storeAccess = new StoreAccess( _fileSystemRule.get(), pageCache, _testDirectory.databaseLayout(), Config.defaults() );
			  return storeAccess.Initialize();
		 }

		 protected internal virtual string RecordFormatName
		 {
			 get
			 {
				  return StringUtils.EMPTY;
			 }
		 }
	}

}