﻿using System;
using System.Collections.Generic;

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
namespace Recovery
{
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Barrier = Org.Neo4j.Test.Barrier;
	using Race = Org.Neo4j.Test.Race;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;

	public class RecoveryCleanupIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseService _db;
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
					Org.Neo4j.Test.Barrier_Control recoveryCompleteBarrier = new Org.Neo4j.Test.Barrier_Control();
					Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor recoveryBarrierMonitor = new RecoveryBarrierMonitor( this, recoveryCompleteBarrier );
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
//ORIGINAL LINE: private void nativeIndexMustLogCrashPointerCleanupDuringRecovery(org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex setting, String... subTypes) throws Exception
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

		 private void Index( GraphDatabaseService db )
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
//ORIGINAL LINE: private void checkpoint(org.neo4j.graphdb.GraphDatabaseService db) throws java.io.IOException
		 private void Checkpoint( GraphDatabaseService db )
		 {
			  CheckPointer checkPointer = checkPointer( db );
			  checkPointer.ForceCheckPoint( new SimpleTriggerInfo( "test" ) );
		 }

		 private void SomeData( GraphDatabaseService db )
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

		 private GraphDatabaseService StartDatabase()
		 {
			  GraphDatabaseBuilder builder = _factory.newEmbeddedDatabaseBuilder( _storeDir );
			  _testSpecificConfig.forEach( builder.setConfig );
			  return builder.NewGraphDatabase();
		 }

		 private DatabaseHealth DatabaseHealth( GraphDatabaseService db )
		 {
			  return DependencyResolver( db ).resolveDependency( typeof( DatabaseHealth ) );
		 }

		 private CheckPointer CheckPointer( GraphDatabaseService db )
		 {
			  DependencyResolver dependencyResolver = dependencyResolver( db );
			  return dependencyResolver.ResolveDependency( typeof( NeoStoreDataSource ) ).DependencyResolver.resolveDependency( typeof( CheckPointer ) );
		 }

		 private DependencyResolver DependencyResolver( GraphDatabaseService db )
		 {
			  return ( ( GraphDatabaseAPI ) db ).DependencyResolver;
		 }

		 private class RecoveryBarrierMonitor : Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
		 {
			 private readonly RecoveryCleanupIT _outerInstance;

			  internal readonly Org.Neo4j.Test.Barrier_Control Barrier;

			  internal RecoveryBarrierMonitor( RecoveryCleanupIT outerInstance, Org.Neo4j.Test.Barrier_Control barrier )
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