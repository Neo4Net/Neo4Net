using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
namespace Neo4Net.storeupgrade
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Enclosed = org.junit.experimental.runners.Enclosed;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using OnlineBackupSettings = Neo4Net.backup.OnlineBackupSettings;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using InwardKernel = Neo4Net.Kernel.api.InwardKernel;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using Registers = Neo4Net.Register.Registers;
	using CommunityBootstrapper = Neo4Net.Server.CommunityBootstrapper;
	using ServerBootstrapper = Neo4Net.Server.ServerBootstrapper;
	using ServerTestUtils = Neo4Net.Server.ServerTestUtils;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Unzip = Neo4Net.Test.Unzip;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.store.StoreAssertions.assertConsistentStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Enclosed.class) public class StoreUpgradeIT
	public class StoreUpgradeIT
	{
		 // NOTE: the zip files must contain the databases files and NOT the database folder itself!!!
		 private static readonly IList<Store[]> _stores23 = Arrays.asList( new Store[]{ new Store( "0.A.6-empty.zip", 0, 1, Selectivities(), IndexCounts() ) }, new Store[]{ new Store("0.A.6-data.zip", 174, 30, Selectivities(1.0, 1.0, 1.0), IndexCounts(Counts(0, 38, 38, 38), Counts(0, 1, 1, 1), Counts(0, 133, 133, 133))) } );
		 private static readonly IList<Store[]> _stores300 = Arrays.asList( new Store[]{ new Store( "E.H.0-empty.zip", 0, 1, Selectivities(), IndexCounts(), HighLimit.NAME ) }, new Store[]{ new Store("E.H.0-data.zip", 174, 30, Selectivities(1.0, 1.0, 1.0), IndexCounts(Counts(0, 38, 38, 38), Counts(0, 1, 1, 1), Counts(0, 133, 133, 133)), HighLimit.NAME) } );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class StoreUpgradeTest
		 public class StoreUpgradeTest
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public Store store;
			  public Store Store;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Store[]> stores()
			  public static ICollection<Store[]> Stores()
			  {
					return Iterables.asCollection( Iterables.concat( _stores23, _stores300 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
			  public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
			  public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void embeddedDatabaseShouldStartOnOlderStoreWhenUpgradeIsEnabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void EmbeddedDatabaseShouldStartOnOlderStoreWhenUpgradeIsEnabled()
			  {
					File databaseDirectory = Store.prepareDirectory( TestDir.databaseDir() );

					GraphDatabaseFactory factory = new TestGraphDatabaseFactory();
					GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( TestDir.databaseDir() );
					builder.SetConfig( GraphDatabaseSettings.allow_upgrade, "true" );
					builder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
					builder.setConfig( GraphDatabaseSettings.logs_directory, TestDir.directory( "logs" ).AbsolutePath );
					builder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
					GraphDatabaseService db = builder.NewGraphDatabase();
					try
					{
						 CheckInstance( Store, ( GraphDatabaseAPI ) db );

					}
					finally
					{
						 Db.shutdown();
					}

					assertConsistentStore( DatabaseLayout.of( databaseDirectory ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverDatabaseShouldStartOnOlderStoreWhenUpgradeIsEnabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ServerDatabaseShouldStartOnOlderStoreWhenUpgradeIsEnabled()
			  {
					File rootDir = TestDir.directory();
					File databaseDirectory = Config.defaults( GraphDatabaseSettings.data_directory, rootDir.ToString() ).get(GraphDatabaseSettings.database_path);

					Store.prepareDirectory( databaseDirectory );

					File configFile = new File( rootDir, Config.DEFAULT_CONFIG_FILE_NAME );
					Properties props = new Properties();
					props.putAll( ServerTestUtils.DefaultRelativeProperties );
					props.setProperty( GraphDatabaseSettings.data_directory.name(), rootDir.AbsolutePath );
					props.setProperty( GraphDatabaseSettings.logs_directory.name(), rootDir.AbsolutePath );
					props.setProperty( GraphDatabaseSettings.allow_upgrade.name(), "true" );
					props.setProperty( GraphDatabaseSettings.pagecache_memory.name(), "8m" );
					props.setProperty( ( new HttpConnector( "http" ) ).type.name(), "HTTP" );
					props.setProperty( ( new HttpConnector( "http" ) ).enabled.name(), "true" );
					props.setProperty( ( new HttpConnector( "http" ) ).listen_address.name(), "localhost:0" );
					props.setProperty( ( new HttpConnector( "https" ) ).enabled.name(), Settings.FALSE );
					props.setProperty( OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE );
					props.setProperty( ( new BoltConnector( "bolt" ) ).enabled.name(), Settings.FALSE );
					using ( StreamWriter writer = new StreamWriter( configFile ) )
					{
						 props.store( writer, "" );
					}

					ServerBootstrapper bootstrapper = new CommunityBootstrapper();
					try
					{
						 ServerBootstrapper.start( rootDir.AbsoluteFile, configFile, Collections.emptyMap() );
						 assertTrue( bootstrapper.Running );
						 CheckInstance( Store, bootstrapper.Server.Database.Graph );
					}
					finally
					{
						 bootstrapper.Stop();
					}

					assertConsistentStore( DatabaseLayout.of( databaseDirectory ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migratingOlderDataAndThanStartAClusterUsingTheNewerDataShouldWork() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void MigratingOlderDataAndThanStartAClusterUsingTheNewerDataShouldWork()
			  {
					// migrate the store using a single instance
					File storeDir = TestDir.storeDir( "initialData" );
					File databaseDirectory = Store.prepareDirectory( TestDir.databaseDir( storeDir ) );
					GraphDatabaseFactory factory = new TestGraphDatabaseFactory();
					GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( databaseDirectory );
					builder.SetConfig( GraphDatabaseSettings.allow_upgrade, "true" );
					builder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
					builder.setConfig( GraphDatabaseSettings.logs_directory, TestDir.directory( "logs" ).AbsolutePath );
					builder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
					GraphDatabaseService db = builder.NewGraphDatabase();
					try
					{
						 CheckInstance( Store, ( GraphDatabaseAPI ) db );
					}
					finally
					{
						 Db.shutdown();
					}

					assertConsistentStore( DatabaseLayout.of( databaseDirectory ) );

					// start the cluster with the db migrated from the old instance
					File haDir = TestDir.storeDir( "ha-stuff" );
					ClusterManager clusterManager = ( new ClusterManager.Builder( haDir ) ).withSeedDir( databaseDirectory ).withCluster( clusterOfSize( 2 ) ).build();

					HighlyAvailableGraphDatabase master;
					HighlyAvailableGraphDatabase slave;
					try
					{
						 clusterManager.Start();
						 ClusterManager.ManagedCluster cluster = clusterManager.Cluster;

						 cluster.Await( allSeesAllAsAvailable() );

						 master = cluster.Master;
						 CheckInstance( Store, master );
						 slave = cluster.AnySlave;
						 CheckInstance( Store, slave );
						 clusterManager.SafeShutdown();

						 assertConsistentStore( master.DatabaseLayout() );
						 assertConsistentStore( slave.DatabaseLayout() );
					}
					finally
					{
						 clusterManager.SafeShutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class StoreUpgradeFailingTest
		 public class StoreUpgradeFailingTest
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
			  public readonly TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String ignored;
			  public string Ignored; // to make JUnit happy...
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public String dbFileName;
			  public string DbFileName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<String[]> parameters()
			  public static ICollection<string[]> Parameters()
			  {
					return Arrays.asList( new string[]{ "on a not cleanly shutdown database", "0.A.3-to-be-recovered.zip" }, new string[]{ "on a 1.9 store", "0.A.0-db.zip" }, new string[]{ "on a 2.0 store", "0.A.1-db.zip" }, new string[]{ "on a 2.1 store", "0.A.3-data.zip" }, new string[]{ "on a 2.2 store", "0.A.5-data.zip" } );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrationShouldFail() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void MigrationShouldFail()
			  {
					// migrate the store using a single instance
					File databaseDirectory = Unzip.unzip( this.GetType(), DbFileName, TestDir.databaseDir() );
					( new File( databaseDirectory, "debug.log" ) ).delete(); // clear the log
					GraphDatabaseFactory factory = new TestGraphDatabaseFactory();
					GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( TestDir.databaseDir() );
					builder.SetConfig( GraphDatabaseSettings.allow_upgrade, "true" );
					builder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
					try
					{
						 builder.NewGraphDatabase();
						 fail( "It should have failed." );
					}
					catch ( Exception ex )
					{
						 assertTrue( ex.InnerException is LifecycleException );
						 Exception realException = ex.InnerException.InnerException;
						 assertTrue( "Unexpected exception", Exceptions.contains( realException, typeof( StoreUpgrader.UnexpectedUpgradingStoreVersionException ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class StoreUpgrade22Test
		 public class StoreUpgrade22Test
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public Store store;
			  public Store Store;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Store[]> stores()
			  public static ICollection<Store[]> Stores()
			  {
					return Iterables.asCollection( Iterables.concat( _stores23, _stores300 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
			  public readonly TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpgradeAStoreWithoutIdFilesAsBackups() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldBeAbleToUpgradeAStoreWithoutIdFilesAsBackups()
			  {
					File databaseDirectory = Store.prepareDirectory( TestDir.databaseDir() );

					// remove id files
					File[] idFiles = databaseDirectory.listFiles( ( dir1, name ) => name.EndsWith( ".id" ) );

					foreach ( File idFile in idFiles )
					{
						 assertTrue( idFile.delete() );
					}

					GraphDatabaseFactory factory = new TestGraphDatabaseFactory();
					GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( TestDir.databaseDir() );
					builder.SetConfig( GraphDatabaseSettings.allow_upgrade, "true" );
					builder.SetConfig( GraphDatabaseSettings.record_format, Store.FormatFamily );
					builder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
					GraphDatabaseService db = builder.NewGraphDatabase();
					try
					{
						 CheckInstance( Store, ( GraphDatabaseAPI ) db );

					}
					finally
					{
						 Db.shutdown();
					}

					assertConsistentStore( DatabaseLayout.of( databaseDirectory ) );
			  }
		 }

		 private class Store
		 {
			  internal readonly string ResourceName;
			  internal readonly long ExpectedNodeCount;
			  internal readonly long LastTxId;
			  internal readonly double[] IndexSelectivity;
			  internal readonly long[][] IndexCounts;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string FormatFamilyConflict;

			  internal Store( string resourceName, long expectedNodeCount, long lastTxId, double[] indexSelectivity, long[][] indexCounts ) : this( resourceName, expectedNodeCount, lastTxId, indexSelectivity, indexCounts, Standard.LATEST_NAME )
			  {
			  }

			  internal Store( string resourceName, long expectedNodeCount, long lastTxId, double[] indexSelectivity, long[][] indexCounts, string formatFamily )
			  {
					this.ResourceName = resourceName;
					this.ExpectedNodeCount = expectedNodeCount;
					this.LastTxId = lastTxId;
					this.IndexSelectivity = indexSelectivity;
					this.IndexCounts = indexCounts;
					this.FormatFamilyConflict = formatFamily;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.File prepareDirectory(java.io.File databaseDirectory) throws java.io.IOException
			  internal virtual File PrepareDirectory( File databaseDirectory )
			  {
					if ( !databaseDirectory.exists() && !databaseDirectory.mkdirs() )
					{
						 throw new IOException( "Could not create directory " + databaseDirectory );
					}
					Unzip.unzip( this.GetType(), ResourceName, databaseDirectory );
					( new File( databaseDirectory, "debug.log" ) ).delete(); // clear the log
					return databaseDirectory;
			  }

			  public override string ToString()
			  {
					return "Store: " + ResourceName;
			  }

			  internal virtual long Indexes()
			  {
					return IndexCounts.Length;
			  }

			  internal virtual string FormatFamily
			  {
				  get
				  {
						return FormatFamilyConflict;
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkInstance(Store store, org.Neo4Net.kernel.internal.GraphDatabaseAPI db) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private static void CheckInstance( Store store, GraphDatabaseAPI db )
		 {
			  CheckProvidedParameters( store, db );
			  CheckGlobalNodeCount( store, db );
			  CheckLabelCounts( db );
			  CheckIndexCounts( store, db );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkIndexCounts(Store store, org.Neo4Net.kernel.internal.GraphDatabaseAPI db) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private static void CheckIndexCounts( Store store, GraphDatabaseAPI db )
		 {
			  InwardKernel kernel = Db.DependencyResolver.resolveDependency( typeof( InwardKernel ) );
			  using ( KernelTransaction tx = kernel.BeginTransaction( @implicit, AnonymousContext.read() ), Statement ignore = tx.AcquireStatement() )
			  {
					SchemaRead schemaRead = tx.SchemaRead();
					IEnumerator<IndexReference> indexes = IndexReference.sortByType( GetAllIndexes( schemaRead ) );
					Register_DoubleLongRegister register = Registers.newDoubleLongRegister();
					for ( int i = 0; indexes.MoveNext(); i++ )
					{
						 IndexReference reference = indexes.Current;

						 // wait index to be online since sometimes we need to rebuild the indexes on migration
						 AwaitOnline( schemaRead, reference );

						 AssertDoubleLongEquals( store.IndexCounts[i][0], store.IndexCounts[i][1], schemaRead.IndexUpdatesAndSize( reference, register ) );
						 AssertDoubleLongEquals( store.IndexCounts[i][2], store.IndexCounts[i][3], schemaRead.IndexSample( reference, register ) );
						 double selectivity = schemaRead.IndexUniqueValuesSelectivity( reference );
						 assertEquals( store.IndexSelectivity[i], selectivity, 0.0000001d );
					}
			  }
		 }

		 private static IEnumerator<IndexReference> GetAllIndexes( SchemaRead schemaRead )
		 {
			  return schemaRead.IndexesGetAll();
		 }

		 private static void CheckLabelCounts( GraphDatabaseAPI db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					Dictionary<Label, long> counts = new Dictionary<Label, long>();
					foreach ( Node node in Db.AllNodes )
					{
						 foreach ( Label label in node.Labels )
						 {
							  long? count = counts[label];
							  if ( count != null )
							  {
									counts[label] = count + 1;
							  }
							  else
							  {
									counts[label] = 1L;
							  }
						 }
					}

					ThreadToStatementContextBridge bridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
					KernelTransaction kernelTransaction = bridge.GetKernelTransactionBoundToThisThread( true );

					foreach ( KeyValuePair<Label, long> entry in Counts.SetOfKeyValuePairs() )
					{
						 assertEquals( entry.Value.longValue(), kernelTransaction.DataRead().countsForNode(kernelTransaction.TokenRead().nodeLabel(entry.Key.name())) );
					}
			  }
		 }

		 private static void CheckGlobalNodeCount( Store store, GraphDatabaseAPI db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					ThreadToStatementContextBridge bridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
					KernelTransaction kernelTransaction = bridge.GetKernelTransactionBoundToThisThread( true );

					assertThat( kernelTransaction.DataRead().countsForNode(-1), @is(store.ExpectedNodeCount) );
			  }
		 }

		 private static void CheckProvidedParameters( Store store, GraphDatabaseAPI db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					// count nodes
					long nodeCount = count( Db.AllNodes );
					assertThat( nodeCount, @is( store.ExpectedNodeCount ) );

					// count indexes
					long indexCount = count( Db.schema().Indexes );
					assertThat( indexCount, @is( store.Indexes() ) );

					// check last committed tx
					TransactionIdStore txIdStore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
					long lastCommittedTxId = txIdStore.LastCommittedTransactionId;

					using ( Statement statement = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true ).acquireStatement() )
					{
						 long countsTxId = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().Counts.txId();
						 assertEquals( lastCommittedTxId, countsTxId );
						 assertThat( lastCommittedTxId, @is( store.LastTxId ) );
					}
			  }
		 }

		 private static void AssertDoubleLongEquals( long expectedFirst, long expectedSecond, Register_DoubleLongRegister register )
		 {
			  long first = register.ReadFirst();
			  long second = register.ReadSecond();
			  string msg = string.Format( "Expected ({0:D},{1:D}), got ({2:D},{3:D})", expectedFirst, expectedSecond, first, second );
			  assertEquals( msg, expectedFirst, first );
			  assertEquals( msg, expectedSecond, second );
		 }

		 private static double[] Selectivities( params double[] selectivity )
		 {
			  return selectivity;
		 }

		 private static long[][] IndexCounts( params long[][] counts )
		 {
			  return counts;
		 }

		 private static long[] Counts( long upgrade, long size, long unique, long sampleSize )
		 {
			  return new long[]{ upgrade, size, unique, sampleSize };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.Neo4Net.Kernel.Api.Internal.IndexReference awaitOnline(org.Neo4Net.Kernel.Api.Internal.SchemaRead schemRead, org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private static IndexReference AwaitOnline( SchemaRead schemRead, IndexReference index )
		 {
			  long start = DateTimeHelper.CurrentUnixTimeMillis();
			  long end = start + 20_000;
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < end )
			  {
					switch ( schemRead.IndexGetState( index ) )
					{
					case ONLINE:
						 return index;

					case FAILED:
						 throw new System.InvalidOperationException( "Index failed instead of becoming ONLINE" );

					default:
						 break;
					}

					try
					{
						 Thread.Sleep( 100 );
					}
					catch ( InterruptedException )
					{
						 // ignored
					}
			  }
			  throw new System.InvalidOperationException( "Index did not become ONLINE within reasonable time" );
		 }
	}

}