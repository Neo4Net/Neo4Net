using System;
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
namespace Recovery
{
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Barrier = Neo4Net.Test.Barrier;
	using Race = Neo4Net.Test.Race;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;

	public class RecoveryCleanupIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private IGraphDatabaseService _db;
		 private File _storeDir;
		 private readonly TestGraphDatabaseFactory _factory = new TestGraphDatabaseFactory();
		 private readonly ExecutorService _executor = Executors.newFixedThreadPool( 2 );
		 private readonly Label _label = Label.label( "label" );
		 private readonly string _propKey = "propKey";
		 private IDictionary<Setting, string> _testSpecificConfig = new Dictionary<Setting, string>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _storeDir = TestDirectory.storeDir();
			  _testSpecificConfig.Clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _executor.shutdown();
			  _executor.awaitTermination( 10, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoveryCleanupShouldBlockRecoveryWritingToCleanedIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoveryCleanupShouldBlockRecoveryWritingToCleanedIndexes()
		 {
			  // GIVEN
			  AtomicReference<Exception> error = new AtomicReference<Exception>();
			  try
			  {
					DirtyDatabase();

					// WHEN
					Neo4Net.Test.Barrier_Control recoveryCompleteBarrier = new Neo4Net.Test.Barrier_Control();
					Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor recoveryBarrierMonitor = new RecoveryBarrierMonitor( this, recoveryCompleteBarrier );
					Monitor = recoveryBarrierMonitor;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> recovery = executor.submit(() ->
					Future<object> recovery = _executor.submit(() =>
					{
					 _db = StartDatabase();
					});
					recoveryCompleteBarrier.AwaitUninterruptibly(); // Ensure we are mid recovery cleanup

					// THEN
					ShouldWait( recovery );
					recoveryCompleteBarrier.Release();
					Recovery.get();

					_db.shutdown();
			  }
			  finally
			  {
					Exception throwable = error.get();
					if ( throwable != null )
					{
						 throw throwable;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanStoreMustLogCrashPointerCleanupDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanStoreMustLogCrashPointerCleanupDuringRecovery()
		 {
			  // given
			  DirtyDatabase();

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  _factory.UserLogProvider = logProvider;
			  _factory.InternalLogProvider = logProvider;
			  StartDatabase().shutdown();

			  // then
			  logProvider.RawMessageMatcher().assertContains("Label index cleanup job registered");
			  logProvider.RawMessageMatcher().assertContains("Label index cleanup job started");
			  logProvider.RawMessageMatcher().assertContains(Matchers.stringContainsInOrder(Iterables.asIterable("Label index cleanup job finished", "Number of pages visited", "Number of cleaned crashed pointers", "Time spent")));
			  logProvider.RawMessageMatcher().assertContains("Label index cleanup job closed");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nativeIndexFusion10MustLogCrashPointerCleanupDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NativeIndexFusion10MustLogCrashPointerCleanupDuringRecovery()
		 {
			  NativeIndexMustLogCrashPointerCleanupDuringRecovery( GraphDatabaseSettings.SchemaIndex.NATIVE10, "native", "spatial", "temporal" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nativeIndexFusion20MustLogCrashPointerCleanupDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NativeIndexFusion20MustLogCrashPointerCleanupDuringRecovery()
		 {
			  NativeIndexMustLogCrashPointerCleanupDuringRecovery( GraphDatabaseSettings.SchemaIndex.NATIVE20, "string", "native", "spatial", "temporal" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nativeIndexBTreeMustLogCrashPointerCleanupDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NativeIndexBTreeMustLogCrashPointerCleanupDuringRecovery()
		 {
			  NativeIndexMustLogCrashPointerCleanupDuringRecovery( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10, "index" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void nativeIndexMustLogCrashPointerCleanupDuringRecovery(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex setting, String... subTypes) throws Exception
		 private void NativeIndexMustLogCrashPointerCleanupDuringRecovery( GraphDatabaseSettings.SchemaIndex setting, params string[] subTypes )
		 {
			  // given
			  SetTestConfig( GraphDatabaseSettings.default_schema_provider, setting.providerName() );
			  DirtyDatabase();

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  _factory.InternalLogProvider = logProvider;
			  StartDatabase().shutdown();

			  // then
			  IList<Matcher<string>> matchers = new List<Matcher<string>>();
			  foreach ( string subType in subTypes )
			  {
					matchers.Add( IndexRecoveryLogMatcher( "Schema index cleanup job registered", subType ) );
					matchers.Add( IndexRecoveryLogMatcher( "Schema index cleanup job started", subType ) );
					matchers.Add( IndexRecoveryFinishedLogMatcher( subType ) );
					matchers.Add( IndexRecoveryLogMatcher( "Schema index cleanup job closed", subType ) );
			  }
			  AssertableLogProvider.MessageMatcher messageMatcher = logProvider.RawMessageMatcher();
			  matchers.ForEach( messageMatcher.assertContainsSingle );
		 }

		 private Matcher<string> IndexRecoveryLogMatcher( string logMessage, string subIndexProviderKey )
		 {

			  return Matchers.stringContainsInOrder( Iterables.asIterable( logMessage, "descriptor", "indexFile=", File.separator + subIndexProviderKey ) );
		 }

		 private Matcher<string> IndexRecoveryFinishedLogMatcher( string subIndexProviderKey )
		 {

			  return Matchers.stringContainsInOrder( Iterables.asIterable( "Schema index cleanup job finished", "descriptor", "indexFile=", File.separator + subIndexProviderKey, "Number of pages visited", "Number of cleaned crashed pointers", "Time spent" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dirtyDatabase() throws java.io.IOException
		 private void DirtyDatabase()
		 {
			  _db = StartDatabase();

			  DatabaseHealth databaseHealth = databaseHealth( _db );
			  Index( _db );
			  SomeData( _db );
			  Checkpoint( _db );
			  SomeData( _db );
			  databaseHealth.Panic( new Exception( "Trigger recovery on next startup" ) );
			  _db.shutdown();
			  _db = null;
		 }

		 private void SetTestConfig<T1>( Setting<T1> setting, string value )
		 {
			  _testSpecificConfig[setting] = value;
		 }

		 private object Monitor
		 {
			 set
			 {
				  Monitors monitors = new Monitors();
				  monitors.AddMonitorListener( value );
				  _factory.Monitors = monitors;
			 }
		 }

		 private void Index( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(_propKey).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private void ReportError( Race.ThrowingRunnable checkpoint, AtomicReference<Exception> error )
		 {
			  try
			  {
					checkpoint.Run();
			  }
			  catch ( Exception t )
			  {
					error.compareAndSet( null, t );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkpoint(org.Neo4Net.graphdb.GraphDatabaseService db) throws java.io.IOException
		 private void Checkpoint( IGraphDatabaseService db )
		 {
			  CheckPointer checkPointer = checkPointer( db );
			  checkPointer.ForceCheckPoint( new SimpleTriggerInfo( "test" ) );
		 }

		 private void SomeData( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _label ).setProperty( _propKey, 1 );
					Db.createNode( _label ).setProperty( _propKey, "string" );
					Db.createNode( _label ).setProperty( _propKey, Values.pointValue( Cartesian, 0.5, 0.5 ) );
					Db.createNode( _label ).setProperty( _propKey, LocalTime.of( 0, 0 ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldWait(java.util.concurrent.Future<?> future)throws InterruptedException, java.util.concurrent.ExecutionException
		 private void ShouldWait<T1>( Future<T1> future )
		 {
			  try
			  {
					future.get( 200L, TimeUnit.MILLISECONDS );
					fail( "Expected timeout" );
			  }
			  catch ( TimeoutException )
			  {
					// good
			  }
		 }

		 private IGraphDatabaseService StartDatabase()
		 {
			  GraphDatabaseBuilder builder = _factory.newEmbeddedDatabaseBuilder( _storeDir );
			  _testSpecificConfig.forEach( builder.setConfig );
			  return builder.NewGraphDatabase();
		 }

		 private DatabaseHealth DatabaseHealth( IGraphDatabaseService db )
		 {
			  return DependencyResolver( db ).resolveDependency( typeof( DatabaseHealth ) );
		 }

		 private CheckPointer CheckPointer( IGraphDatabaseService db )
		 {
			  DependencyResolver dependencyResolver = dependencyResolver( db );
			  return dependencyResolver.ResolveDependency( typeof( NeoStoreDataSource ) ).DependencyResolver.resolveDependency( typeof( CheckPointer ) );
		 }

		 private DependencyResolver DependencyResolver( IGraphDatabaseService db )
		 {
			  return ( ( GraphDatabaseAPI ) db ).DependencyResolver;
		 }

		 private class RecoveryBarrierMonitor : Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
		 {
			 private readonly RecoveryCleanupIT _outerInstance;

			  internal readonly Neo4Net.Test.Barrier_Control Barrier;

			  internal RecoveryBarrierMonitor( RecoveryCleanupIT outerInstance, Neo4Net.Test.Barrier_Control barrier )
			  {
				  this._outerInstance = outerInstance;
					this.Barrier = barrier;
			  }

			  public override void RecoveryCleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			  {
					Barrier.reached();
			  }
		 }
	}

}