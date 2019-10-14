using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ClassGuardedAdversary = Neo4Net.Adversaries.ClassGuardedAdversary;
	using CountingAdversary = Neo4Net.Adversaries.CountingAdversary;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using DatabaseCreator = Neo4Net.Graphdb.factory.GraphDatabaseBuilder.DatabaseCreator;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Neo4Net.Helpers.Collections;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using ExistingTargetStrategy = Neo4Net.Kernel.impl.storemigration.ExistingTargetStrategy;
	using FileOperation = Neo4Net.Kernel.impl.storemigration.FileOperation;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using RecoveryMonitor = Neo4Net.Kernel.recovery.RecoveryMonitor;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using AdversarialPageCacheGraphDatabaseFactory = Neo4Net.Test.AdversarialPageCacheGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestGraphDatabaseFactoryState = Neo4Net.Test.TestGraphDatabaseFactoryState;
	using TestLabels = Neo4Net.Test.TestLabels;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Config.defaults;

	public class RecoveryIT
	{
		private bool InstanceFieldsInitialized = false;

		public RecoveryIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _random ).around( _fileSystemRule ).around( _directory );
		}

		 private static readonly string[] _tokens = new string[] { "Token1", "Token2", "Token3", "Token4", "Token5" };

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(random).around(fileSystemRule).around(directory);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void idGeneratorsRebuildAfterRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IdGeneratorsRebuildAfterRecovery()
		 {
			  GraphDatabaseService database = StartDatabase( _directory.databaseDir() );
			  int numberOfNodes = 10;
			  using ( Transaction transaction = database.BeginTx() )
			  {
					for ( int nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++ )
					{
						 database.CreateNode();
					}
					transaction.Success();
			  }

			  // copying only transaction log simulate non clean shutdown db that should be able to recover just from logs
			  File restoreDbStoreDir = CopyTransactionLogs();

			  GraphDatabaseService recoveredDatabase = StartDatabase( restoreDbStoreDir );
			  using ( Transaction tx = recoveredDatabase.BeginTx() )
			  {
					assertEquals( numberOfNodes, count( recoveredDatabase.AllNodes ) );

					// Make sure id generator has been rebuilt so this doesn't throw null pointer exception
					recoveredDatabase.CreateNode();
			  }

			  database.Shutdown();
			  recoveredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportProgressOnRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportProgressOnRecovery()
		 {
			  GraphDatabaseService database = StartDatabase( _directory.databaseDir() );
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 database.CreateNode();
						 transaction.Success();
					}
			  }

			  File restoreDbStoreDir = CopyTransactionLogs();
			  GraphDatabaseService recoveredDatabase = StartDatabase( restoreDbStoreDir );
			  using ( Transaction transaction = recoveredDatabase.BeginTx() )
			  {
					assertEquals( 10, count( recoveredDatabase.AllNodes ) );
			  }
			  _logProvider.rawMessageMatcher().assertContains("10% completed");
			  _logProvider.rawMessageMatcher().assertContains("100% completed");

			  database.Shutdown();
			  recoveredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverIdsCorrectlyWhenWeCreateAndDeleteANodeInTheSameRecoveryRun() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverIdsCorrectlyWhenWeCreateAndDeleteANodeInTheSameRecoveryRun()
		 {
			  GraphDatabaseService database = StartDatabase( _directory.databaseDir() );
			  Label testLabel = Label.label( "testLabel" );
			  const string propertyToDelete = "propertyToDelete";
			  const string validPropertyName = "validProperty";

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.AddLabel( testLabel );
					transaction.Success();
			  }

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = FindNodeByLabel( database, testLabel );
					node.SetProperty( propertyToDelete, CreateLongString() );
					node.SetProperty( validPropertyName, CreateLongString() );
					transaction.Success();
			  }

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = FindNodeByLabel( database, testLabel );
					node.RemoveProperty( propertyToDelete );
					transaction.Success();
			  }

			  // copying only transaction log simulate non clean shutdown db that should be able to recover just from logs
			  File restoreDbStoreDir = CopyTransactionLogs();

			  // database should be restored and node should have expected properties
			  GraphDatabaseService recoveredDatabase = StartDatabase( restoreDbStoreDir );
			  using ( Transaction ignored = recoveredDatabase.BeginTx() )
			  {
					Node node = FindNodeByLabel( recoveredDatabase, testLabel );
					assertFalse( node.HasProperty( propertyToDelete ) );
					assertTrue( node.HasProperty( validPropertyName ) );
			  }

			  database.Shutdown();
			  recoveredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void recoveryShouldFixPartiallyAppliedSchemaIndexUpdates()
		 public virtual void RecoveryShouldFixPartiallyAppliedSchemaIndexUpdates()
		 {
			  Label label = Label.label( "Foo" );
			  string property = "Bar";

			  // cause failure during 'relationship.delete()' command application
			  ClassGuardedAdversary adversary = new ClassGuardedAdversary( new CountingAdversary( 1, true ), typeof( Command.RelationshipCommand ) );
			  adversary.Disable();

			  File databaseDir = _directory.databaseDir();
			  GraphDatabaseService db = AdversarialPageCacheGraphDatabaseFactory.create( _fileSystemRule.get(), adversary ).newEmbeddedDatabaseBuilder(databaseDir).newGraphDatabase();
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(label).assertPropertyIsUnique(property).create();
						 tx.Success();
					}

					long relationshipId = CreateRelationship( db );

					TransactionFailureException txFailure = null;
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 Node node = Db.createNode( label );
							 node.SetProperty( property, "B" );
							 Db.getRelationshipById( relationshipId ).delete(); // this should fail because of the adversary
							 tx.Success();
							 adversary.Enable();
							}
					}
					catch ( TransactionFailureException e )
					{
						 txFailure = e;
					}
					assertNotNull( txFailure );
					adversary.Disable();

					HealthOf( db ).healed(); // heal the db so it is possible to inspect the data

					// now we can observe partially committed state: node is in the index and relationship still present
					using ( Transaction tx = Db.beginTx() )
					{
						 assertNotNull( FindNode( db, label, property, "B" ) );
						 assertNotNull( Db.getRelationshipById( relationshipId ) );
						 tx.Success();
					}

					HealthOf( db ).panic( txFailure.InnerException ); // panic the db again to force recovery on the next startup

					// restart the database, now with regular page cache
					File databaseDirectory = ( ( GraphDatabaseAPI ) db ).databaseLayout().databaseDirectory();
					Db.shutdown();
					db = StartDatabase( databaseDirectory );

					// now we observe correct state: node is in the index and relationship is removed
					using ( Transaction tx = Db.beginTx() )
					{
						 assertNotNull( FindNode( db, label, property, "B" ) );
						 AssertRelationshipNotExist( db, relationshipId );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSameIndexUpdatesDuringRecoveryAsFromNormalIndexApplication() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeSameIndexUpdatesDuringRecoveryAsFromNormalIndexApplication()
		 {
			  // Previously indexes weren't really participating in recovery, instead there was an after-phase
			  // where nodes that was changed during recovery were reindexed. Do be able to do this reindexing
			  // the index had to support removing arbitrary entries based on node id alone. Lucene can do this,
			  // but at least at the time of writing this not the native index. For this the recovery process
			  // was changed to rewind neostore back to how it looked at the last checkpoint and then replay
			  // transactions from that point, including indexes. This test verifies that there's no mismatch
			  // between applying transactions normally and recovering them after a crash, index update wise.

			  // given
			  File storeDir = _directory.absolutePath();
			  EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: UpdateCapturingIndexProvider updateCapturingIndexProvider = new UpdateCapturingIndexProvider(org.neo4j.kernel.api.index.IndexProvider.EMPTY, new java.util.HashMap<>());
			  UpdateCapturingIndexProvider updateCapturingIndexProvider = new UpdateCapturingIndexProvider( this, IndexProvider.EMPTY, new Dictionary<long, ICollection<IndexEntryUpdate<object>>>() );
			  GraphDatabaseAPI db = StartDatabase( storeDir, fs, updateCapturingIndexProvider );
			  Label label = TestLabels.LABEL_ONE;
			  string key1 = "key1";
			  string key2 = "key2";
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label).on(key1).create();
					Db.schema().indexFor(label).on(key1).on(key2).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
			  CheckPoint( db );

			  ProduceRandomNodePropertyAndLabelUpdates( db, _random.intBetween( 20, 40 ), label, key1, key2 );
			  CheckPoint( db );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> updatesAtLastCheckPoint = updateCapturingIndexProvider.snapshot();
			  IDictionary<long, ICollection<IndexEntryUpdate<object>>> updatesAtLastCheckPoint = updateCapturingIndexProvider.Snapshot();

			  // when
			  ProduceRandomNodePropertyAndLabelUpdates( db, _random.intBetween( 40, 100 ), label, key1, key2 );

			  // Snapshot
			  Flush( db );
			  EphemeralFileSystemAbstraction crashedFs = fs.Snapshot();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> updatesAtCrash = updateCapturingIndexProvider.snapshot();
			  IDictionary<long, ICollection<IndexEntryUpdate<object>>> updatesAtCrash = updateCapturingIndexProvider.Snapshot();

			  // Crash and start anew
			  UpdateCapturingIndexProvider recoveredUpdateCapturingIndexProvider = new UpdateCapturingIndexProvider( this, IndexProvider.EMPTY, updatesAtLastCheckPoint );
			  long lastCommittedTxIdBeforeRecovered = LastCommittedTxId( db );
			  Db.shutdown();
			  fs.Dispose();

			  db = StartDatabase( storeDir, crashedFs, recoveredUpdateCapturingIndexProvider );
			  long lastCommittedTxIdAfterRecovered = LastCommittedTxId( db );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> updatesAfterRecovery = recoveredUpdateCapturingIndexProvider.snapshot();
			  IDictionary<long, ICollection<IndexEntryUpdate<object>>> updatesAfterRecovery = recoveredUpdateCapturingIndexProvider.Snapshot();

			  // then
			  assertEquals( lastCommittedTxIdBeforeRecovered, lastCommittedTxIdAfterRecovered );
			  AssertSameUpdates( updatesAtCrash, updatesAfterRecovery );
			  Db.shutdown();
			  crashedFs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeTheSameRecordsAtCheckpointAsAfterReverseRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeTheSameRecordsAtCheckpointAsAfterReverseRecovery()
		 {
			  // given
			  EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabase(_directory.databaseDir());
			  ProduceRandomGraphUpdates( db, 100 );
			  CheckPoint( db );
			  EphemeralFileSystemAbstraction checkPointFs = fs.Snapshot();

			  // when
			  ProduceRandomGraphUpdates( db, 100 );
			  Flush( db );
			  EphemeralFileSystemAbstraction crashedFs = fs.Snapshot();
			  Db.shutdown();
			  fs.Dispose();
			  Monitors monitors = new Monitors();
			  AtomicReference<PageCache> pageCache = new AtomicReference<PageCache>();
			  AtomicReference<EphemeralFileSystemAbstraction> reversedFs = new AtomicReference<EphemeralFileSystemAbstraction>();
			  monitors.AddMonitorListener( new RecoveryMonitorAnonymousInnerClass( this, crashedFs, pageCache, reversedFs ) );
			  new TestGraphDatabaseFactoryAnonymousInnerClass( this, pageCache )
						 .setFileSystem( crashedFs ).setMonitors( monitors ).newImpermanentDatabase( _directory.databaseDir() ).shutdown();

			  // then
			  fs.Dispose();

			  try
			  {
					// Here we verify that the neostore contents, record by record are exactly the same when comparing
					// the store as it was right after the checkpoint with the store as it was right after reverse recovery completed.
					AssertSameStoreContents( checkPointFs, reversedFs.get(), _directory.databaseLayout() );
			  }
			  finally
			  {
					checkPointFs.Dispose();
					reversedFs.get().close();
			  }
		 }

		 private class RecoveryMonitorAnonymousInnerClass : RecoveryMonitor
		 {
			 private readonly RecoveryIT _outerInstance;

			 private EphemeralFileSystemAbstraction _crashedFs;
			 private AtomicReference<PageCache> _pageCache;
			 private AtomicReference<EphemeralFileSystemAbstraction> _reversedFs;

			 public RecoveryMonitorAnonymousInnerClass( RecoveryIT outerInstance, EphemeralFileSystemAbstraction crashedFs, AtomicReference<PageCache> pageCache, AtomicReference<EphemeralFileSystemAbstraction> reversedFs )
			 {
				 this.outerInstance = outerInstance;
				 this._crashedFs = crashedFs;
				 this._pageCache = pageCache;
				 this._reversedFs = reversedFs;
			 }

			 public void reverseStoreRecoveryCompleted( long checkpointTxId )
			 {
				  try
				  {
						// Flush the page cache which will fished out of the PlatformModule at the point of constructing the database
						_pageCache.get().flushAndForce();
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }

				  // The stores should now be equal in content to the db as it was right after the checkpoint.
				  // Grab a snapshot so that we can compare later.
				  _reversedFs.set( _crashedFs.snapshot() );
			 }
		 }

		 private class TestGraphDatabaseFactoryAnonymousInnerClass : TestGraphDatabaseFactory
		 {
			 private readonly RecoveryIT _outerInstance;

			 private AtomicReference<PageCache> _pageCache;

			 public TestGraphDatabaseFactoryAnonymousInnerClass( RecoveryIT outerInstance, AtomicReference<PageCache> pageCache )
			 {
				 this.outerInstance = outerInstance;
				 this._pageCache = pageCache;
			 }

									// This nested constructing is done purely to be able to fish out PlatformModule
									// (and its PageCache inside it). It would be great if this could be done in a prettier way.

			 protected internal override GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
			 {
				  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			 }

			 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			 {
				 private readonly TestGraphDatabaseFactoryAnonymousInnerClass _outerInstance;

				 private File _storeDir;
				 private TestGraphDatabaseFactoryState _state;

				 public DatabaseCreatorAnonymousInnerClass( TestGraphDatabaseFactoryAnonymousInnerClass outerInstance, File storeDir, TestGraphDatabaseFactoryState state )
				 {
					 this.outerInstance = outerInstance;
					 this._storeDir = storeDir;
					 this._state = state;
				 }


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public org.neo4j.graphdb.GraphDatabaseService newDatabase(@Nonnull Config config)
				 public GraphDatabaseService newDatabase( Config config )
				 {
					  TestGraphDatabaseFacadeFactory factory = new TestGraphDatabaseFacadeFactoryAnonymousInnerClass( this, _state, config );
					  return factory.NewFacade( _storeDir, config, newDependencies( _state.databaseDependencies() ) );
				 }

				 private class TestGraphDatabaseFacadeFactoryAnonymousInnerClass : TestGraphDatabaseFacadeFactory
				 {
					 private readonly DatabaseCreatorAnonymousInnerClass _outerInstance;

					 private Config _config;

					 public TestGraphDatabaseFacadeFactoryAnonymousInnerClass( DatabaseCreatorAnonymousInnerClass outerInstance, TestGraphDatabaseFactoryState state, Config config ) : base( state, true )
					 {
						 this.outerInstance = outerInstance;
						 this._config = config;
					 }

					 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
					 {
						  PlatformModule platform = base.createPlatform( storeDir, config, dependencies );
						  // nice way of getting the page cache dependency before db is created, huh?
						  _outerInstance.outerInstance.pageCache.set( platform.PageCache );
						  return platform;
					 }
				 }
			 }
		 }

		 private static long LastCommittedTxId( GraphDatabaseService db )
		 {
			  return ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;
		 }

		 private static void AssertSameStoreContents( EphemeralFileSystemAbstraction fs1, EphemeralFileSystemAbstraction fs2, DatabaseLayout databaseLayout )
		 {
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  VersionContextSupplier contextSupplier = EmptyVersionContextSupplier.EMPTY;
			  try (ThreadPoolJobScheduler jobScheduler = new ThreadPoolJobScheduler(); PageCache pageCache1 = new ConfiguringPageCacheFactory(fs1, defaults(), PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, contextSupplier, jobScheduler)
									.OrCreatePageCache;
						 PageCache pageCache2 = ( new ConfiguringPageCacheFactory( fs2, defaults(), PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, contextSupplier, jobScheduler ) ).OrCreatePageCache;
						 NeoStores store1 = ( new StoreFactory( databaseLayout, defaults(), new DefaultIdGeneratorFactory(fs1), pageCache1, fs1, logProvider, contextSupplier ) ).openAllNeoStores();
						 NeoStores store2 = ( new StoreFactory( databaseLayout, defaults(), new DefaultIdGeneratorFactory(fs2), pageCache2, fs2, logProvider, contextSupplier ) ).openAllNeoStores())
						 {
					foreach ( StoreType storeType in StoreType.values() )
					{
						 if ( storeType.RecordStore )
						 {
							  AssertSameStoreContents( store1.GetRecordStore( storeType ), store2.GetRecordStore( storeType ) );
						 }
					}
						 }
		 }

		 private static void AssertSameStoreContents<RECORD>( RecordStore<RECORD> store1, RecordStore<RECORD> store2 ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  long highId1 = store1.HighId;
			  long highId2 = store2.HighId;
			  long maxHighId = max( highId1, highId2 );
			  RECORD record1 = store1.NewRecord();
			  RECORD record2 = store2.NewRecord();
			  for ( long id = store1.NumberOfReservedLowIds; id < maxHighId; id++ )
			  {
					store1.GetRecord( id, record1, RecordLoad.CHECK );
					store2.GetRecord( id, record2, RecordLoad.CHECK );
					assertEquals( record1, record2 );
			  }
		 }

		 private static void Flush( GraphDatabaseService db )
		 {
			  ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( StorageEngine ) ).flushAndForce( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkPoint(org.neo4j.graphdb.GraphDatabaseService db) throws java.io.IOException
		 private static void CheckPoint( GraphDatabaseService db )
		 {
			  ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "Manual trigger" ) );
		 }

		 private void ProduceRandomGraphUpdates( GraphDatabaseService db, int numberOfTransactions )
		 {
			  // Load all existing nodes
			  IList<Node> nodes = new List<Node>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( ResourceIterator<Node> allNodes = Db.AllNodes.GetEnumerator() )
					{
						 while ( allNodes.MoveNext() )
						 {
							  nodes.Add( allNodes.Current );
						 }
					}
					tx.Success();
			  }

			  for ( int i = 0; i < numberOfTransactions; i++ )
			  {
					int transactionSize = _random.intBetween( 1, 30 );
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int j = 0; j < transactionSize; j++ )
						 {
							  float operationType = _random.nextFloat();
							  float operation = _random.nextFloat();
							  if ( operationType < 0.5 )
							  { // create
									if ( operation < 0.5 )
									{ // create node (w/ random label, prop)
										 Node node = Db.createNode( _random.nextBoolean() ? array(RandomLabel()) : new Label[0] );
										 if ( _random.nextBoolean() )
										 {
											  node.SetProperty( RandomKey(), _random.nextValueAsObject() );
										 }
									}
									else
									{ // create relationship (w/ random prop)
										 if ( nodes.Count > 0 )
										 {
											  Relationship relationship = _random.among( nodes ).createRelationshipTo( _random.among( nodes ), RandomRelationshipType() );
											  if ( _random.nextBoolean() )
											  {
													relationship.SetProperty( RandomKey(), _random.nextValueAsObject() );
											  }
										 }
									}
							  }
							  else if ( operationType < 0.8 )
							  { // change
									if ( operation < 0.25 )
									{ // add label
										 _random.among( nodes, node => node.addLabel( RandomLabel() ) );
									}
									else if ( operation < 0.5 )
									{ // remove label
										 _random.among( nodes, node => node.removeLabel( RandomLabel() ) );
									}
									else if ( operation < 0.75 )
									{ // set node property
										 _random.among( nodes, node => node.setProperty( RandomKey(), _random.nextValueAsObject() ) );
									}
									else
									{ // set relationship property
										 OnRandomRelationship( nodes, relationship => relationship.setProperty( RandomKey(), _random.nextValueAsObject() ) );
									}
							  }
							  else
							  { // delete

									if ( operation < 0.25 )
									{ // remove node property
										 _random.among( nodes, node => node.removeProperty( RandomKey() ) );
									}
									else if ( operation < 0.5 )
									{ // remove relationship property
										 OnRandomRelationship( nodes, relationship => relationship.removeProperty( RandomKey() ) );
									}
									else if ( operation < 0.9 )
									{ // delete relationship
										 OnRandomRelationship( nodes, Relationship.delete );
									}
									else
									{ // delete node
										 _random.among(nodes, node =>
										 {
										  foreach ( Relationship relationship in node.Relationships )
										  {
												relationship.delete();
										  }
										  node.delete();
										  nodes.Remove( node );
										 });
									}
							  }
						 }
						 tx.Success();
					}
			  }
		 }

		 private void OnRandomRelationship( IList<Node> nodes, System.Action<Relationship> action )
		 {
			  _random.among( nodes, node => _random.among( asList( node.Relationships ), action ) );
		 }

		 private RelationshipType RandomRelationshipType()
		 {
			  return RelationshipType.withName( _random.among( _tokens ) );
		 }

		 private string RandomKey()
		 {
			  return _random.among( _tokens );
		 }

		 private Label RandomLabel()
		 {
			  return Label.label( _random.among( _tokens ) );
		 }

		 private void AssertSameUpdates<T1, T2>( IDictionary<T1> updatesAtCrash, IDictionary<T2> recoveredUpdatesSnapshot )
		 {
			  // The UpdateCapturingIndexProvider just captures updates made to indexes. The order in this test
			  // should be the same during online transaction application and during recovery since everything
			  // is single threaded. However there's a bunch of placing where entries and keys and what not
			  // ends up in hash maps and so may change order. The super important thing we need to verify is
			  // that updates for a particular transaction are the same during normal application and recovery,
			  // regardless of ordering differences within the transaction.

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>>> crashUpdatesPerNode = splitPerNode(updatesAtCrash);
			  IDictionary<long, IDictionary<long, ICollection<IndexEntryUpdate<object>>>> crashUpdatesPerNode = SplitPerNode( updatesAtCrash );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>>> recoveredUpdatesPerNode = splitPerNode(recoveredUpdatesSnapshot);
			  IDictionary<long, IDictionary<long, ICollection<IndexEntryUpdate<object>>>> recoveredUpdatesPerNode = SplitPerNode( recoveredUpdatesSnapshot );
			  assertEquals( crashUpdatesPerNode, recoveredUpdatesPerNode );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Map<long,java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>>> splitPerNode(java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> updates)
		 private static IDictionary<long, IDictionary<long, ICollection<IndexEntryUpdate<object>>>> SplitPerNode<T1>( IDictionary<T1> updates )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>>> result = new java.util.HashMap<>();
			  IDictionary<long, IDictionary<long, ICollection<IndexEntryUpdate<object>>>> result = new Dictionary<long, IDictionary<long, ICollection<IndexEntryUpdate<object>>>>();
			  updates.forEach( ( indexId, indexUpdates ) => result.put( indexId, SplitPerNode( indexUpdates ) ) );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> splitPerNode(java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates)
		 private static IDictionary<long, ICollection<IndexEntryUpdate<object>>> SplitPerNode<T1>( ICollection<T1> updates )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> perNode = new java.util.HashMap<>();
			  IDictionary<long, ICollection<IndexEntryUpdate<object>>> perNode = new Dictionary<long, ICollection<IndexEntryUpdate<object>>>();
			  updates.forEach( update => perNode.computeIfAbsent( update.EntityId, nodeId => new List<>() ).add(update) );
			  return perNode;
		 }

		 private void ProduceRandomNodePropertyAndLabelUpdates( GraphDatabaseService db, int numberOfTransactions, Label label, params string[] keys )
		 {
			  // Load all existing nodes
			  IList<Node> nodes = new List<Node>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( ResourceIterator<Node> allNodes = Db.AllNodes.GetEnumerator() )
					{
						 while ( allNodes.MoveNext() )
						 {
							  nodes.Add( allNodes.Current );
						 }
					}
					tx.Success();
			  }

			  for ( int i = 0; i < numberOfTransactions; i++ )
			  {
					int transactionSize = _random.intBetween( 1, 30 );
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int j = 0; j < transactionSize; j++ )
						 {
							  float operation = _random.nextFloat();
							  if ( operation < 0.1 )
							  { // Delete node
									if ( nodes.Count > 0 )
									{
										 nodes.RemoveAt( _random.Next( nodes.Count ) ).delete();
									}
							  }
							  else if ( operation < 0.3 )
							  { // Create node
									Node node = Db.createNode( _random.nextBoolean() ? array(label) : new Label[0] );
									foreach ( string key in keys )
									{
										 if ( _random.nextBoolean() )
										 {
											  node.SetProperty( key, _random.nextValueAsObject() );
										 }
									}
									nodes.Add( node );
							  }
							  else if ( operation < 0.4 )
							  { // Remove label
									_random.among( nodes, node => node.removeLabel( label ) );
							  }
							  else if ( operation < 0.6 )
							  { // Add label
									_random.among( nodes, node => node.addLabel( label ) );
							  }
							  else if ( operation < 0.85 )
							  { // Set property
									_random.among( nodes, node => node.setProperty( _random.among( keys ), _random.nextValueAsObject() ) );
							  }
							  else
							  { // Remove property
									_random.among( nodes, node => node.removeProperty( _random.among( keys ) ) );
							  }
						 }
						 tx.Success();
					}
			  }
		 }

		 private static Node FindNodeByLabel( GraphDatabaseService database, Label testLabel )
		 {
			  using ( ResourceIterator<Node> nodes = database.FindNodes( testLabel ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return nodes.next();
			  }
		 }

		 private static Node FindNode( GraphDatabaseService db, Label label, string property, string value )
		 {
			  using ( ResourceIterator<Node> nodes = Db.findNodes( label, property, value ) )
			  {
					return Iterators.single( nodes );
			  }
		 }

		 private static long CreateRelationship( GraphDatabaseService db )
		 {
			  long relationshipId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode( Label.label( DateTimeHelper.CurrentUnixTimeMillis() + "" ) );
					Node end = Db.createNode( Label.label( DateTimeHelper.CurrentUnixTimeMillis() + "" ) );
					relationshipId = start.CreateRelationshipTo( end, withName( "KNOWS" ) ).Id;
					tx.Success();
			  }
			  return relationshipId;
		 }

		 private static void AssertRelationshipNotExist( GraphDatabaseService db, long id )
		 {
			  try
			  {
					Db.getRelationshipById( id );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( NotFoundException ) ) );
			  }
		 }

		 private static DatabaseHealth HealthOf( GraphDatabaseService db )
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) db ).DependencyResolver;
			  return resolver.ResolveDependency( typeof( DatabaseHealth ) );
		 }

		 private static string CreateLongString()
		 {
			  string[] strings = new string[( int ) ByteUnit.kibiBytes( 2 )];
			  Arrays.fill( strings, "a" );
			  return Arrays.ToString( strings );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File copyTransactionLogs() throws java.io.IOException
		 private File CopyTransactionLogs()
		 {
			  File restoreDbStore = _directory.storeDir( "restore-db" );
			  File restoreDbStoreDir = _directory.databaseDir( restoreDbStore );
			  Move( _fileSystemRule.get(), this._directory.databaseDir(), restoreDbStoreDir );
			  return restoreDbStoreDir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void move(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 private static void Move( FileSystemAbstraction fs, File fromDirectory, File toDirectory )
		 {
			  assertTrue( fs.IsDirectory( fromDirectory ) );
			  assertTrue( fs.IsDirectory( toDirectory ) );

			  LogFiles transactionLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( fromDirectory, fs ).build();
			  File[] logFiles = transactionLogFiles.LogFilesConflict();
			  foreach ( File logFile in logFiles )
			  {
					FileOperation.MOVE.perform( fs, logFile.Name, fromDirectory, false, toDirectory, ExistingTargetStrategy.FAIL );
			  }
		 }

		 private static GraphDatabaseAPI StartDatabase( File storeDir, EphemeralFileSystemAbstraction fs, UpdateCapturingIndexProvider indexProvider )
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(fs).setKernelExtensions(singletonList(new IndexExtensionFactory(indexProvider))).newImpermanentDatabaseBuilder(storeDir).setConfig(default_schema_provider, indexProvider.ProviderDescriptor.name()).newGraphDatabase();
		 }

		 private GraphDatabaseService StartDatabase( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).setInternalLogProvider(_logProvider).newEmbeddedDatabase(storeDir);
		 }

		 public class UpdateCapturingIndexProvider : IndexProvider
		 {
			 private readonly RecoveryIT _outerInstance;

			  internal readonly IndexProvider Actual;
			  internal readonly IDictionary<long, UpdateCapturingIndexAccessor> Indexes = new ConcurrentDictionary<long, UpdateCapturingIndexAccessor>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> initialUpdates;
			  internal readonly IDictionary<long, ICollection<IndexEntryUpdate<object>>> InitialUpdates;

			  internal UpdateCapturingIndexProvider<T1>( RecoveryIT outerInstance, IndexProvider actual, IDictionary<T1> initialUpdates ) : base( actual )
			  {
				  this._outerInstance = outerInstance;
					this.Actual = actual;
					this.InitialUpdates = initialUpdates;
			  }

			  public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			  {
					return Actual.getPopulator( descriptor, samplingConfig, bufferFactory );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
			  public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			  {
					IndexAccessor actualAccessor = Actual.getOnlineAccessor( descriptor, samplingConfig );
					return Indexes.computeIfAbsent( descriptor.Id, id => new UpdateCapturingIndexAccessor( _outerInstance, actualAccessor, InitialUpdates[id] ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
			  public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
			  {
					return Actual.getPopulationFailure( descriptor );
			  }

			  public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
			  {
					return Actual.getInitialState( descriptor );
			  }

			  public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
			  {
					return Actual.getCapability( descriptor );
			  }

			  public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			  {
					return Actual.storeMigrationParticipant( fs, pageCache );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> snapshot()
			  public virtual IDictionary<long, ICollection<IndexEntryUpdate<object>>> Snapshot()
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<long,java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> result = new java.util.HashMap<>();
					IDictionary<long, ICollection<IndexEntryUpdate<object>>> result = new Dictionary<long, ICollection<IndexEntryUpdate<object>>>();
					Indexes.forEach( ( indexId, index ) => result.put( indexId, index.snapshot() ) );
					return result;
			  }
		 }

		 public class UpdateCapturingIndexAccessor : IndexAccessor
		 {
			 private readonly RecoveryIT _outerInstance;

			  internal readonly IndexAccessor Actual;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  internal readonly ICollection<IndexEntryUpdate<object>> Updates = new List<IndexEntryUpdate<object>>();

			  internal UpdateCapturingIndexAccessor<T1>( RecoveryIT outerInstance, IndexAccessor actual, ICollection<T1> initialUpdates )
			  {
				  this._outerInstance = outerInstance;
					this.Actual = actual;
					if ( initialUpdates != null )
					{
						 this.Updates.addAll( initialUpdates );
					}
			  }

			  public override void Drop()
			  {
					Actual.drop();
			  }

			  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
			  {
					return Wrap( Actual.newUpdater( mode ) );
			  }

			  internal virtual IndexUpdater Wrap( IndexUpdater actual )
			  {
					return new UpdateCapturingIndexUpdater( _outerInstance, actual, Updates );
			  }

			  public override void Force( IOLimiter ioLimiter )
			  {
					Actual.force( ioLimiter );
			  }

			  public override void Refresh()
			  {
					Actual.refresh();
			  }

			  public override void Close()
			  {
					Actual.Dispose();
			  }

			  public override IndexReader NewReader()
			  {
					return Actual.newReader();
			  }

			  public override BoundedIterable<long> NewAllEntriesReader()
			  {
					return Actual.newAllEntriesReader();
			  }

			  public override ResourceIterator<File> SnapshotFiles()
			  {
					return Actual.snapshotFiles();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor propertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void VerifyDeferredConstraints( NodePropertyAccessor propertyAccessor )
			  {
					Actual.verifyDeferredConstraints( propertyAccessor );
			  }

			  public virtual bool Dirty
			  {
				  get
				  {
						return Actual.Dirty;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> snapshot()
			  public virtual ICollection<IndexEntryUpdate<object>> Snapshot()
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new java.util.ArrayList<>(updates);
					return new List<IndexEntryUpdate<object>>( Updates );
			  }

			  public override bool ConsistencyCheck( ReporterFactory reporterFactory )
			  {
					return Actual.consistencyCheck( reporterFactory );
			  }
		 }

		 public class UpdateCapturingIndexUpdater : IndexUpdater
		 {
			 private readonly RecoveryIT _outerInstance;

			  internal readonly IndexUpdater Actual;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updatesTarget;
			  internal readonly ICollection<IndexEntryUpdate<object>> UpdatesTarget;

			  internal UpdateCapturingIndexUpdater<T1>( RecoveryIT outerInstance, IndexUpdater actual, ICollection<T1> updatesTarget )
			  {
				  this._outerInstance = outerInstance;
					this.Actual = actual;
					this.UpdatesTarget = updatesTarget;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void Process<T1>( IndexEntryUpdate<T1> update )
			  {
					Actual.process( update );
					UpdatesTarget.Add( update );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void Close()
			  {
					Actual.close();
			  }
		 }

		 private class IndexExtensionFactory : KernelExtensionFactory<IndexExtensionFactory.Dependencies>
		 {
			  internal readonly IndexProvider IndexProvider;

			  internal interface Dependencies
			  {
			  }

			  internal IndexExtensionFactory( IndexProvider indexProvider ) : base( "customExtension" )
			  {
					this.IndexProvider = indexProvider;
			  }

			  public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
			  {
					return IndexProvider;
			  }
		 }
	}

}