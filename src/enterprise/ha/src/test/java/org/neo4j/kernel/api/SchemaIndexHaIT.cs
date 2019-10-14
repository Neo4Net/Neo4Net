using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.api
{
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Predicates = Neo4Net.Functions.Predicates;
	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Schema_IndexState = Neo4Net.Graphdb.schema.Schema_IndexState;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.Internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using NativeLuceneFusionIndexProviderFactory20 = Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory20;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Neo4Net.Kernel.ha.UpdatePuller;
	using HighAvailabilityMemberState = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberState;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using FusionIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asUniqueSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.deleteRecursively;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.given;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;

	public class SchemaIndexHaIT
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaIndexHaIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_label = _label( "label" );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public static DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

		 private static readonly IndexProviderDescriptor _controlledProviderDescriptor = new IndexProviderDescriptor( "controlled", "1.0" );
		 private static readonly System.Predicate<GraphDatabaseService> _isMaster = item => item is HighlyAvailableGraphDatabase && ( ( HighlyAvailableGraphDatabase ) item ).Master;

		 private readonly string _key = "key";
		 private Label _label;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexOnMasterShouldHaveSlavesBuildItAsWell() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatingIndexOnMasterShouldHaveSlavesBuildItAsWell()
		 {
			  // GIVEN
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  IDictionary<object, Node> data = CreateSomeData( master );

			  // WHEN
			  IndexDefinition index = CreateIndex( master );
			  cluster.Sync();

			  // THEN
			  AwaitIndexOnline( index, cluster, data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexOnSlaveIsNotAllowed()
		 public virtual void CreatingIndexOnSlaveIsNotAllowed()
		 {
			  // GIVEN
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  // WHEN
			  try
			  {
					CreateIndex( slave );
					fail( "should have thrown exception" );
			  }
			  catch ( ConstraintViolationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexPopulationJobsShouldContinueThroughRoleSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexPopulationJobsShouldContinueThroughRoleSwitch()
		 {
			  // GIVEN a cluster of 3
			  ControlledGraphDatabaseFactory dbFactory = new ControlledGraphDatabaseFactory();
			  ClusterManager.ManagedCluster cluster = ClusterRule.withDbFactory( dbFactory ).withSharedSetting( GraphDatabaseSettings.default_schema_provider, _controlledProviderDescriptor.name() ).startCluster();
			  HighlyAvailableGraphDatabase firstMaster = cluster.Master;

			  // where the master gets some data created as well as an index
			  IDictionary<object, Node> data = CreateSomeData( firstMaster );
			  CreateIndex( firstMaster );
			  //dbFactory.awaitPopulationStarted( firstMaster );
			  dbFactory.TriggerFinish( firstMaster );

			  // Pick a slave, pull the data and the index
			  HighlyAvailableGraphDatabase aSlave = cluster.AnySlave;
			  aSlave.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();

			  // and await the index population to start. It will actually block as long as we want it to
			  dbFactory.AwaitPopulationStarted( aSlave );

			  // WHEN we shut down the master
			  cluster.Shutdown( firstMaster );

			  dbFactory.TriggerFinish( aSlave );
			  cluster.Await( masterAvailable( firstMaster ) );
			  // get the new master, which should be the slave we pulled from above
			  HighlyAvailableGraphDatabase newMaster = cluster.Master;

			  // THEN
			  assertEquals( "Unexpected new master", aSlave, newMaster );
			  using ( Transaction tx = newMaster.BeginTx() )
			  {
					IndexDefinition index = single( newMaster.Schema().Indexes );
					AwaitIndexOnline( index, newMaster, data );
					tx.Success();
			  }
			  // FINALLY: let all db's finish
			  foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
			  {
					dbFactory.TriggerFinish( db );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingSchemaIndicesOnMasterShouldBeBroughtOnlineOnSlavesAfterStoreCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingSchemaIndicesOnMasterShouldBeBroughtOnlineOnSlavesAfterStoreCopy()
		 {
			  /*
			  The master has an index that is currently populating.
			  Then a slave comes online and contacts the master to get copies of the store files.
			  Because the index is still populating, it won't be copied. Instead the slave will build its own.
			  We want to observe that the slave builds an index that eventually comes online.
			   */

			  // GIVEN
			  ControlledGraphDatabaseFactory dbFactory = new ControlledGraphDatabaseFactory( _isMaster );

			  ClusterManager.ManagedCluster cluster = ClusterRule.withDbFactory( dbFactory ).withSharedSetting( GraphDatabaseSettings.default_schema_provider, NativeLuceneFusionIndexProviderFactory20.DESCRIPTOR.name() ).startCluster();

			  try
			  {
					cluster.Await( allSeesAllAsAvailable() );

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;

					// A slave is offline, and has no store files
					ClusterManager.RepairKit slaveDown = BringSlaveOfflineAndRemoveStoreFiles( cluster, slave );

					// And I create an index on the master, and wait for population to start
					HighlyAvailableGraphDatabase master = cluster.Master;
					IDictionary<object, Node> data = CreateSomeData( master );
					CreateIndex( master );
					dbFactory.AwaitPopulationStarted( master );

					// WHEN the slave comes online before population has finished on the master
					slave = slaveDown.Repair();
					cluster.Await( allSeesAllAsAvailable(), 180 );
					cluster.Sync();

					// THEN, population should finish successfully on both master and slave
					dbFactory.TriggerFinish( master );

					// Check master
					IndexDefinition index;
					using ( Transaction tx = master.BeginTx() )
					{
						 index = single( master.Schema().Indexes );
						 AwaitIndexOnline( index, master, data );
						 tx.Success();
					}

					// Check slave
					using ( Transaction tx = slave.BeginTx() )
					{
						 AwaitIndexOnline( index, slave, data );
						 tx.Success();
					}
			  }
			  finally
			  {
					foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
					{
						 dbFactory.TriggerFinish( db );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineSchemaIndicesOnMasterShouldBeBroughtOnlineOnSlavesAfterStoreCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OnlineSchemaIndicesOnMasterShouldBeBroughtOnlineOnSlavesAfterStoreCopy()
		 {
			  /*
			  The master has an index that is online.
			  Then a slave comes online and contacts the master to get copies of the store files.
			  Because the index is online, it should be copied, and the slave should successfully bring the index online.
			   */

			  // GIVEN
			  ControlledGraphDatabaseFactory dbFactory = new ControlledGraphDatabaseFactory();

			  ClusterManager.ManagedCluster cluster = ClusterRule.withDbFactory( dbFactory ).withSharedSetting( GraphDatabaseSettings.default_schema_provider, _controlledProviderDescriptor.name() ).startCluster();
			  cluster.Await( allSeesAllAsAvailable(), 120 );

			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  // All slaves in the cluster, except the one I care about, proceed as normal
			  ProceedAsNormalWithIndexPopulationOnAllSlavesExcept( dbFactory, cluster, slave );

			  // A slave is offline, and has no store files
			  ClusterManager.RepairKit slaveDown = BringSlaveOfflineAndRemoveStoreFiles( cluster, slave );

			  // And I create an index on the master, and wait for population to start
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  IDictionary<object, Node> data = CreateSomeData( master );
			  CreateIndex( master );
			  dbFactory.AwaitPopulationStarted( master );

			  // And the population finishes
			  dbFactory.TriggerFinish( master );
			  IndexDefinition index;
			  using ( Transaction tx = master.BeginTx() )
			  {
					index = single( master.Schema().Indexes );
					AwaitIndexOnline( index, master, data );
					tx.Success();
			  }

			  // WHEN the slave comes online after population has finished on the master
			  slave = slaveDown.Repair();
			  cluster.Await( allSeesAllAsAvailable() );
			  cluster.Sync();

			  // THEN the index should work on the slave
			  dbFactory.TriggerFinish( slave );
			  using ( Transaction tx = slave.BeginTx() )
			  {
					AwaitIndexOnline( index, slave, data );
					tx.Success();
			  }
		 }

		 private void ProceedAsNormalWithIndexPopulationOnAllSlavesExcept( ControlledGraphDatabaseFactory dbFactory, ClusterManager.ManagedCluster cluster, HighlyAvailableGraphDatabase slaveToIgnore )
		 {
			  foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
			  {
					if ( db != slaveToIgnore && Db.InstanceState == HighAvailabilityMemberState.SLAVE )
					{
						 dbFactory.TriggerFinish( db );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ResultOfMethodCallIgnored") private org.neo4j.kernel.impl.ha.ClusterManager.RepairKit bringSlaveOfflineAndRemoveStoreFiles(org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster cluster, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase slave) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private ClusterManager.RepairKit BringSlaveOfflineAndRemoveStoreFiles( ClusterManager.ManagedCluster cluster, HighlyAvailableGraphDatabase slave )
		 {
			  ClusterManager.RepairKit slaveDown = cluster.Shutdown( slave );

			  File databaseDir = slave.DatabaseLayout().databaseDirectory();
			  deleteRecursively( databaseDir );
			  databaseDir.mkdir();
			  return slaveDown;
		 }

		 private IDictionary<object, Node> CreateSomeData( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IDictionary<object, Node> result = new Dictionary<object, Node>();
					for ( int i = 0; i < 10; i++ )
					{
						 Node node = Db.createNode( _label );
						 object propertyValue = i;
						 node.SetProperty( _key, propertyValue );
						 result[propertyValue] = node;
					}
					tx.Success();
					return result;
			  }
		 }

		 private IndexDefinition CreateIndex( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexDefinition index = Db.schema().indexFor(_label).on(_key).create();
					tx.Success();
					return index;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitIndexOnline(org.neo4j.graphdb.schema.IndexDefinition index, org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster cluster, java.util.Map<Object,org.neo4j.graphdb.Node> expectedDdata) throws InterruptedException
		 private static void AwaitIndexOnline( IndexDefinition index, ClusterManager.ManagedCluster cluster, IDictionary<object, Node> expectedDdata )
		 {
			  foreach ( GraphDatabaseService db in cluster.AllMembers )
			  {
					AwaitIndexOnline( index, db, expectedDdata );
			  }
		 }

		 private static IndexDefinition ReHomedIndexDefinition( GraphDatabaseService db, IndexDefinition definition )
		 {
			  foreach ( IndexDefinition candidate in Db.schema().Indexes )
			  {
					if ( candidate.Equals( definition ) )
					{
						 return candidate;
					}
			  }
			  throw new NoSuchElementException( "New database doesn't have requested index" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitIndexOnline(org.neo4j.graphdb.schema.IndexDefinition requestedIndex, org.neo4j.graphdb.GraphDatabaseService db, java.util.Map<Object,org.neo4j.graphdb.Node> expectedData) throws InterruptedException
		 private static void AwaitIndexOnline( IndexDefinition requestedIndex, GraphDatabaseService db, IDictionary<object, Node> expectedData )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexDefinition index = ReHomedIndexDefinition( db, requestedIndex );

					long timeout = DateTimeHelper.CurrentUnixTimeMillis() + SECONDS.toMillis(120);
					while ( !IndexOnline( index, db ) )
					{
						 Thread.Sleep( 1 );
						 if ( DateTimeHelper.CurrentUnixTimeMillis() > timeout )
						 {
							  fail( "Expected index to come online within a reasonable time." );
						 }
					}

					AssertIndexContents( index, db, expectedData );
					tx.Success();
			  }
		 }

		 private static void AssertIndexContents( IndexDefinition index, GraphDatabaseService db, IDictionary<object, Node> expectedData )
		 {
			  foreach ( KeyValuePair<object, Node> entry in expectedData.SetOfKeyValuePairs() )
			  {
					assertEquals( asSet( entry.Value ), asUniqueSet( Db.findNodes( single( index.Labels ), single( index.PropertyKeys ), entry.Key ) ) );
			  }
		 }

		 private static bool IndexOnline( IndexDefinition index, GraphDatabaseService db )
		 {
			  try
			  {
					return Db.schema().getIndexState(index) == Schema_IndexState.ONLINE;
			  }
			  catch ( NotFoundException )
			  {
					return false;
			  }
		 }

		 private class ControlledIndexPopulator : IndexPopulator
		 {
			  internal readonly DoubleLatch Latch;
			  internal readonly IndexPopulator Delegate;

			  internal ControlledIndexPopulator( IndexPopulator @delegate, DoubleLatch latch )
			  {
					this.Delegate = @delegate;
					this.Latch = latch;
			  }

			  public override void Create()
			  {
					Delegate.create();
			  }

			  public override void Drop()
			  {
					Delegate.drop();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
					Delegate.add( updates );
					Latch.startAndWaitForAllToStartAndFinish();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
			  {
					Delegate.verifyDeferredConstraints( nodePropertyAccessor );
			  }

			  public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
			  {
					return Delegate.newPopulatingUpdater( nodePropertyAccessor );
			  }

			  public override void Close( bool populationCompletedSuccessfully )
			  {
					Delegate.close( populationCompletedSuccessfully );
					assertTrue( "Expected population to succeed :(", populationCompletedSuccessfully );
					Latch.finish();
			  }

			  public override void MarkAsFailed( string failure )
			  {
					Delegate.markAsFailed( failure );
			  }

			  public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
			  {
					Delegate.includeSample( update );
			  }

			  public override IndexSample SampleResult()
			  {
					return Delegate.sampleResult();
			  }
		 }

		 private class ControlledIndexProvider : IndexProvider
		 {
			  internal readonly IndexProvider Delegate;
			  internal readonly DoubleLatch Latch = new DoubleLatch();

			  internal ControlledIndexProvider( IndexProvider @delegate ) : base( _controlledProviderDescriptor, given( @delegate.DirectoryStructure() ) )
			  {
					this.Delegate = @delegate;
			  }

			  public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			  {
					IndexPopulator populator = Delegate.getPopulator( descriptor, samplingConfig, bufferFactory );
					return new ControlledIndexPopulator( populator, Latch );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
			  public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			  {
					return Delegate.getOnlineAccessor( descriptor, samplingConfig );
			  }

			  public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
			  {
					return Delegate.getInitialState( descriptor );
			  }

			  public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
			  {
					return Delegate.getCapability( descriptor );
			  }

			  public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			  {
					return Delegate.storeMigrationParticipant( fs, pageCache );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
			  public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
			  {
					return Delegate.getPopulationFailure( descriptor );
			  }
		 }

		 internal interface IndexProviderDependencies
		 {
			  GraphDatabaseService Db();
			  Config Config();
			  PageCache PageCache();
			  RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector();
		 }

		 private class ControllingIndexProviderFactory : KernelExtensionFactory<IndexProviderDependencies>
		 {
			  internal readonly IDictionary<GraphDatabaseService, IndexProvider> PerDbIndexProvider;
			  internal readonly System.Predicate<GraphDatabaseService> InjectLatchPredicate;

			  internal ControllingIndexProviderFactory( IDictionary<GraphDatabaseService, IndexProvider> perDbIndexProvider, System.Predicate<GraphDatabaseService> injectLatchPredicate ) : base( ExtensionType.DATABASE, _controlledProviderDescriptor.Key )
			  {
					this.PerDbIndexProvider = perDbIndexProvider;
					this.InjectLatchPredicate = injectLatchPredicate;
			  }

			  public override Lifecycle NewInstance( KernelContext context, SchemaIndexHaIT.IndexProviderDependencies deps )
			  {
					PageCache pageCache = deps.PageCache();
					File databaseDirectory = context.Directory();
					DefaultFileSystemAbstraction fs = FileSystemRule.get();
					IndexProvider.Monitor monitor = IndexProvider.Monitor_Fields.EMPTY;
					Config config = deps.Config();
					OperationalMode operationalMode = context.DatabaseInfo().OperationalMode;
					RecoveryCleanupWorkCollector recoveryCleanupWorkCollector = deps.RecoveryCleanupWorkCollector();

					FusionIndexProvider fusionIndexProvider = NativeLuceneFusionIndexProviderFactory20.create( pageCache, databaseDirectory, fs, monitor, config, operationalMode, recoveryCleanupWorkCollector );

					if ( InjectLatchPredicate.test( deps.Db() ) )
					{
						 ControlledIndexProvider provider = new ControlledIndexProvider( fusionIndexProvider );
						 PerDbIndexProvider[deps.Db()] = provider;
						 return provider;
					}
					else
					{
						 return fusionIndexProvider;
					}
			  }
		 }

		 private class ControlledGraphDatabaseFactory : TestHighlyAvailableGraphDatabaseFactory
		 {
			  internal readonly IDictionary<GraphDatabaseService, IndexProvider> PerDbIndexProvider = new ConcurrentDictionary<GraphDatabaseService, IndexProvider>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.kernel.extension.KernelExtensionFactory<?> factory;
			  internal readonly KernelExtensionFactory<object> Factory;

			  internal ControlledGraphDatabaseFactory() : this(Predicates.alwaysTrue())
			  {
			  }

			  internal ControlledGraphDatabaseFactory( System.Predicate<GraphDatabaseService> dbsToControlIndexingOn )
			  {
					Factory = new ControllingIndexProviderFactory( PerDbIndexProvider, dbsToControlIndexingOn );
					CurrentState.removeKernelExtensions( kef => kef.GetType().Name.Contains("IndexProvider") );
					CurrentState.addKernelExtensions( Collections.singletonList( Factory ) );
			  }

			  public override GraphDatabaseBuilder NewEmbeddedDatabaseBuilder( File file )
			  {
					return base.NewEmbeddedDatabaseBuilder( file );
			  }

			  internal virtual void AwaitPopulationStarted( GraphDatabaseService db )
			  {
					ControlledIndexProvider provider = ( ControlledIndexProvider ) PerDbIndexProvider[db];
					if ( provider != null )
					{
						 provider.Latch.waitForAllToStart();
					}
			  }

			  internal virtual void TriggerFinish( GraphDatabaseService db )
			  {
					ControlledIndexProvider provider = ( ControlledIndexProvider ) PerDbIndexProvider[db];
					if ( provider != null )
					{
						 provider.Latch.finish();
					}
			  }
		 }
	}

}