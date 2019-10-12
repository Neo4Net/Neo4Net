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
namespace Neo4Net.Kernel.impl.store.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using SilentProgressReporter = Neo4Net.Kernel.impl.util.monitoring.SilentProgressReporter;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Register = Neo4Net.Register.Register;
	using Registers = Neo4Net.Register.Registers;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class CountsComputerTest
	{
		private bool InstanceFieldsInitialized = false;

		public CountsComputerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDir = TestDirectory.testDirectory( _fsRule );
			RuleChain = RuleChain.outerRule( _pcRule ).around( _fsRule ).around( _testDir );
		}

		 private static readonly NullLogProvider _logProvider = NullLogProvider.Instance;
		 private static readonly Config _config = Config.defaults();
		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pcRule = new PageCacheRule();
		 private TestDirectory _testDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(pcRule).around(fsRule).around(testDir);
		 public RuleChain RuleChain;

		 private FileSystemAbstraction _fs;
		 private GraphDatabaseBuilder _dbBuilder;
		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fs = _fsRule.get();
			  _dbBuilder = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fs)).newImpermanentDatabaseBuilder(_testDir.databaseDir());
			  _pageCache = _pcRule.getPageCache( _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipPopulationWhenNodeAndRelationshipStoresAreEmpty()
		 public virtual void SkipPopulationWhenNodeAndRelationshipStoresAreEmpty()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  InvocationTrackingProgressReporter progressReporter = new InvocationTrackingProgressReporter();
			  RebuildCounts( lastCommittedTransactionId, progressReporter );

			  CheckEmptyCountStore();
			  assertTrue( progressReporter.CompleteInvoked );
			  assertFalse( progressReporter.StartInvoked );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnEmptyCountsStoreFromAnEmptyDatabase()
		 public virtual void ShouldCreateAnEmptyCountsStoreFromAnEmptyDatabase()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  CheckEmptyCountStore();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateACountsStoreWhenThereAreNodesInTheDB()
		 public virtual void ShouldCreateACountsStoreWhenThereAreNodesInTheDB()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( Label.label( "A" ) );
					Db.createNode( Label.label( "C" ) );
					Db.createNode( Label.label( "D" ) );
					Db.createNode();
					tx.Success();
			  }
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID + 1 + 1 + 1 + 1, store.TxId() );
					assertEquals( 4, store.TotalEntriesStored() );
					assertEquals( 4, Get( store, nodeKey( -1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 0 ) ) );
					assertEquals( 1, Get( store, nodeKey( 1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 2 ) ) );
					assertEquals( 0, Get( store, nodeKey( 3 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateACountsStoreWhenThereAreUnusedNodeRecordsInTheDB()
		 public virtual void ShouldCreateACountsStoreWhenThereAreUnusedNodeRecordsInTheDB()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( Label.label( "A" ) );
					Db.createNode( Label.label( "C" ) );
					Node node = Db.createNode( Label.label( "D" ) );
					Db.createNode();
					node.Delete();
					tx.Success();
			  }
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID + 1 + 1 + 1 + 1, store.TxId() );
					assertEquals( 3, store.TotalEntriesStored() );
					assertEquals( 3, Get( store, nodeKey( -1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 0 ) ) );
					assertEquals( 1, Get( store, nodeKey( 1 ) ) );
					assertEquals( 0, Get( store, nodeKey( 2 ) ) );
					assertEquals( 0, Get( store, nodeKey( 3 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateACountsStoreWhenThereAreUnusedRelationshipRecordsInTheDB()
		 public virtual void ShouldCreateACountsStoreWhenThereAreUnusedRelationshipRecordsInTheDB()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node nodeA = Db.createNode( Label.label( "A" ) );
					Node nodeC = Db.createNode( Label.label( "C" ) );
					Relationship rel = nodeA.CreateRelationshipTo( nodeC, RelationshipType.withName( "TYPE1" ) );
					nodeC.CreateRelationshipTo( nodeA, RelationshipType.withName( "TYPE2" ) );
					rel.Delete();
					tx.Success();
			  }
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID + 1 + 1 + 1 + 1 + 1, store.TxId() );
					assertEquals( 9, store.TotalEntriesStored() );
					assertEquals( 2, Get( store, nodeKey( -1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 0 ) ) );
					assertEquals( 1, Get( store, nodeKey( 1 ) ) );
					assertEquals( 0, Get( store, nodeKey( 2 ) ) );
					assertEquals( 0, Get( store, nodeKey( 3 ) ) );
					assertEquals( 0, Get( store, relationshipKey( -1, 0, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 1, -1 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateACountsStoreWhenThereAreNodesAndRelationshipsInTheDB()
		 public virtual void ShouldCreateACountsStoreWhenThereAreNodesAndRelationshipsInTheDB()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node nodeA = Db.createNode( Label.label( "A" ) );
					Node nodeC = Db.createNode( Label.label( "C" ) );
					Node nodeD = Db.createNode( Label.label( "D" ) );
					Node node = Db.createNode();
					nodeA.CreateRelationshipTo( nodeD, RelationshipType.withName( "TYPE" ) );
					node.CreateRelationshipTo( nodeC, RelationshipType.withName( "TYPE2" ) );
					tx.Success();
			  }
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID + 1 + 1 + 1 + 1 + 1 + 1, store.TxId() );
					assertEquals( 13, store.TotalEntriesStored() );
					assertEquals( 4, Get( store, nodeKey( -1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 0 ) ) );
					assertEquals( 1, Get( store, nodeKey( 1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 2 ) ) );
					assertEquals( 0, Get( store, nodeKey( 3 ) ) );
					assertEquals( 2, Get( store, relationshipKey( -1, -1, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 0, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 1, -1 ) ) );
					assertEquals( 0, Get( store, relationshipKey( -1, 2, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 1, 1 ) ) );
					assertEquals( 0, Get( store, relationshipKey( -1, 0, 1 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateACountStoreWhenDBContainsDenseNodes()
		 public virtual void ShouldCreateACountStoreWhenDBContainsDenseNodes()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") final org.neo4j.kernel.internal.GraphDatabaseAPI db = (org.neo4j.kernel.internal.GraphDatabaseAPI) dbBuilder.setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "2").newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.setConfig( GraphDatabaseSettings.dense_node_threshold, "2" ).newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node nodeA = Db.createNode( Label.label( "A" ) );
					Node nodeC = Db.createNode( Label.label( "C" ) );
					Node nodeD = Db.createNode( Label.label( "D" ) );
					nodeA.CreateRelationshipTo( nodeA, RelationshipType.withName( "TYPE1" ) );
					nodeA.CreateRelationshipTo( nodeC, RelationshipType.withName( "TYPE2" ) );
					nodeA.CreateRelationshipTo( nodeD, RelationshipType.withName( "TYPE3" ) );
					nodeD.CreateRelationshipTo( nodeC, RelationshipType.withName( "TYPE4" ) );
					tx.Success();
			  }
			  long lastCommittedTransactionId = GetLastTxId( db );
			  Db.shutdown();

			  RebuildCounts( lastCommittedTransactionId );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1, store.TxId() );
					assertEquals( 22, store.TotalEntriesStored() );
					assertEquals( 3, Get( store, nodeKey( -1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 0 ) ) );
					assertEquals( 1, Get( store, nodeKey( 1 ) ) );
					assertEquals( 1, Get( store, nodeKey( 2 ) ) );
					assertEquals( 0, Get( store, nodeKey( 3 ) ) );
					assertEquals( 4, Get( store, relationshipKey( -1, -1, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 0, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 1, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 2, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 3, -1 ) ) );
					assertEquals( 0, Get( store, relationshipKey( -1, 4, -1 ) ) );
					assertEquals( 1, Get( store, relationshipKey( -1, 1, 1 ) ) );
					assertEquals( 2, Get( store, relationshipKey( -1, -1, 1 ) ) );
					assertEquals( 3, Get( store, relationshipKey( 0, -1, -1 ) ) );
			  }
		 }

		 private File AlphaStoreFile()
		 {
			  return _testDir.databaseLayout().countStoreA();
		 }

		 private File BetaStoreFile()
		 {
			  return _testDir.databaseLayout().countStoreB();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private long getLastTxId(@SuppressWarnings("deprecation") org.neo4j.kernel.internal.GraphDatabaseAPI db)
		 private long GetLastTxId( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;
		 }

		 private void CheckEmptyCountStore()
		 {
			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker() );
					assertEquals( BASE_TX_ID, store.TxId() );
					assertEquals( 0, store.TotalEntriesStored() );
			  }
		 }

		 private void CleanupCountsForRebuilding()
		 {
			  _fs.deleteFile( AlphaStoreFile() );
			  _fs.deleteFile( BetaStoreFile() );
		 }

		 private CountsTracker CreateCountsTracker()
		 {
			  return new CountsTracker( _logProvider, _fs, _pageCache, _config, _testDir.databaseLayout(), EmptyVersionContextSupplier.EMPTY );
		 }

		 private void RebuildCounts( long lastCommittedTransactionId )
		 {
			  RebuildCounts( lastCommittedTransactionId, SilentProgressReporter.INSTANCE );
		 }

		 private void RebuildCounts( long lastCommittedTransactionId, ProgressReporter progressReporter )
		 {
			  CleanupCountsForRebuilding();

			  IdGeneratorFactory idGenFactory = new DefaultIdGeneratorFactory( _fs );
			  StoreFactory storeFactory = new StoreFactory( _testDir.databaseLayout(), _config, idGenFactory, _pageCache, _fs, _logProvider, EmptyVersionContextSupplier.EMPTY );
			  using ( Lifespan life = new Lifespan(), NeoStores neoStores = storeFactory.OpenAllNeoStores() )
			  {
					NodeStore nodeStore = neoStores.NodeStore;
					RelationshipStore relationshipStore = neoStores.RelationshipStore;
					int highLabelId = ( int ) neoStores.LabelTokenStore.HighId;
					int highRelationshipTypeId = ( int ) neoStores.RelationshipTypeTokenStore.HighId;
					CountsComputer countsComputer = new CountsComputer( lastCommittedTransactionId, nodeStore, relationshipStore, highLabelId, highRelationshipTypeId, Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory_Fields.AutoWithoutPagecache, progressReporter );
					CountsTracker countsTracker = CreateCountsTracker();
					life.Add( countsTracker.setInitializer( countsComputer ) );
			  }
		 }

		 private long Get( CountsTracker store, CountsKey key )
		 {
			  Neo4Net.Register.Register_DoubleLongRegister value = Registers.newDoubleLongRegister();
			  store.Get( key, value );
			  return value.ReadSecond();
		 }

		 private class InvocationTrackingProgressReporter : ProgressReporter
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool StartInvokedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CompleteInvokedConflict;

			  public override void Start( long max )
			  {
					StartInvokedConflict = true;
			  }

			  public override void Progress( long add )
			  {

			  }

			  public override void Completed()
			  {
					CompleteInvokedConflict = true;
			  }

			  internal virtual bool StartInvoked
			  {
				  get
				  {
						return StartInvokedConflict;
				  }
			  }

			  internal virtual bool CompleteInvoked
			  {
				  get
				  {
						return CompleteInvokedConflict;
				  }
			  }
		 }
	}

}