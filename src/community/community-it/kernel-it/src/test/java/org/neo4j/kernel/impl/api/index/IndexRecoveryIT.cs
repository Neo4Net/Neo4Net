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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using MisconfiguredIndexException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.MisconfiguredIndexException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Neo4Net.Kernel.extension;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using SwallowingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using CollectingIndexUpdater = Neo4Net.Kernel.Impl.Index.Schema.CollectingIndexUpdater;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getIndexes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.haveState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class IndexRecoveryIT
	{
		private bool InstanceFieldsInitialized = false;

		private class ImpermanentDatabaseRuleAnonymousInnerClass : Neo4Net.Test.rule.ImpermanentDatabaseRule
		{
			protected internal override GraphDatabaseFactory newFactory()
			{
				 TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
				 return factory.AddKernelExtension( index );
			}
		}

		private class IndexProviderAnonymousInnerClass : IndexProvider
		{
			private readonly IndexPopulationMissConcurrentUpdateIT outerInstance;

			public IndexProviderAnonymousInnerClass( IndexPopulationMissConcurrentUpdateIT outerInstance, UnknownType indexProvider, UnknownType directoriesByProvider ) : base( indexProvider, directoriesByProvider )
			{
				this.outerInstance = outerInstance;
			}

			public override IndexPopulator getPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			{
				 return new IndexPopulatorAnonymousInnerClass( this );
			}

			private class IndexPopulatorAnonymousInnerClass : IndexPopulator
			{
				private readonly IndexProviderAnonymousInnerClass _outerInstance;

				public IndexPopulatorAnonymousInnerClass( IndexProviderAnonymousInnerClass outerInstance )
				{
					this.outerInstance = outerInstance;
				}

				public void create()
				{
				}

				public void drop()
				{
				}

				public void add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
				{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> update : updates)
					 foreach ( IndexEntryUpdate<object> update in updates )
					 {
						  bool added = entitiesByScan.add( update.EntityId );
						  assertTrue( added ); // scans should never see multiple updates from the same IEntityId
						  if ( update.EntityId > SCAN_BARRIER_NODE_ID_THRESHOLD )
						  {
								populationAtId = update.EntityId;
								barrier.reached();
						  }
					 }
				}

				public void verifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
				{
				}

				public IndexUpdater newPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
				{
					 return new IndexUpdaterAnonymousInnerClass( this );
				}

				private class IndexUpdaterAnonymousInnerClass : IndexUpdater
				{
					private readonly IndexPopulatorAnonymousInnerClass _outerInstance;

					public IndexUpdaterAnonymousInnerClass( IndexPopulatorAnonymousInnerClass outerInstance )
					{
						this.outerInstance = outerInstance;
					}

					public void process<T1>( IndexEntryUpdate<T1> update )
					{
						 bool added = entitiesByUpdater.add( update.EntityId );
						 assertTrue( added ); // we know that in this test we won't apply multiple updates for an IEntityId
					}

					public void close()
					{
					}
				}

				public void close( bool populationCompletedSuccessfully )
				{
					 assertTrue( populationCompletedSuccessfully );
				}

				public void markAsFailed( string failure )
				{
					 throw new System.NotSupportedException();
				}

				public void includeSample<T1>( IndexEntryUpdate<T1> update )
				{
				}

				public IndexSample sampleResult()
				{
					 return new IndexSample( 0, 0, 0 );
				}
			}

			public override IndexAccessor getOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			{
				 return mock( typeof( IndexAccessor ) );
			}

			public override string getPopulationFailure( StoreIndexDescriptor descriptor )
			{
				 throw new System.InvalidOperationException();
			}

			public override InternalIndexState getInitialState( StoreIndexDescriptor descriptor )
			{
				 return POPULATING;
			}

			public override IndexCapability getCapability( StoreIndexDescriptor descriptor )
			{
				 return IndexCapability.NO_CAPABILITY;
			}

			public override StoreMigrationParticipant storeMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			{
				 return NOT_PARTICIPATING;
			}
		}

		public IndexRecoveryIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_mockedIndexProviderFactory = singleInstanceIndexProviderFactory( PROVIDER_DESCRIPTOR.Key, _mockedIndexProvider );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverInTheMiddleOfPopulatingAnIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverInTheMiddleOfPopulatingAnIndex()
		 {
			  // Given
			  StartDb();

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(IndexPopulatorWithControlledCompletionTiming(latch));
			  CreateIndex( _myLabel );

			  // And Given
			  Future<Void> killFuture = KillDbInSeparateThread();
			  latch.Signal();
			  killFuture.get();

			  // When
			  when( _mockedIndexProvider.getInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( InternalIndexState.POPULATING );
			  latch = new System.Threading.CountdownEvent( 1 );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(IndexPopulatorWithControlledCompletionTiming(latch));
			  StartDb();

			  // Then
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, hasSize( 1 ) ) );
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Populating ) ) );
			  verify( _mockedIndexProvider, times( 2 ) ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( _mockedIndexProvider, never() ).getOnlineAccessor(any(typeof(StoreIndexDescriptor)), any(typeof(IndexSamplingConfig)));
			  latch.Signal();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverInTheMiddleOfPopulatingAnIndexWhereLogHasRotated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverInTheMiddleOfPopulatingAnIndexWhereLogHasRotated()
		 {
			  // Given
			  StartDb();

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(IndexPopulatorWithControlledCompletionTiming(latch));
			  CreateIndex( _myLabel );
			  RotateLogsAndCheckPoint();

			  // And Given
			  Future<Void> killFuture = KillDbInSeparateThread();
			  latch.Signal();
			  killFuture.get();
			  latch = new System.Threading.CountdownEvent( 1 );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(IndexPopulatorWithControlledCompletionTiming(latch));
			  when( _mockedIndexProvider.getInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( InternalIndexState.POPULATING );

			  // When
			  StartDb();

			  // Then
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, hasSize( 1 ) ) );
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Populating ) ) );
			  verify( _mockedIndexProvider, times( 2 ) ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( _mockedIndexProvider, never() ).getOnlineAccessor(any(typeof(StoreIndexDescriptor)), any(typeof(IndexSamplingConfig)));
			  latch.Signal();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverAndUpdateOnlineIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverAndUpdateOnlineIndex()
		 {
			  // Given
			  StartDb();

			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  when( populator.SampleResult() ).thenReturn(new IndexSample());
			  IndexAccessor mockedAccessor = mock( typeof( IndexAccessor ) );
			  when( mockedAccessor.NewUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( SwallowingIndexUpdater.INSTANCE );
			  when( _mockedIndexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( mockedAccessor );
			  CreateIndexAndAwaitPopulation( _myLabel );
			  // rotate logs
			  RotateLogsAndCheckPoint();
			  // make updates
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> expectedUpdates = createSomeBananas(myLabel);
			  ISet<IndexEntryUpdate<object>> expectedUpdates = CreateSomeBananas( _myLabel );

			  // And Given
			  KillDb();
			  when( _mockedIndexProvider.getInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( InternalIndexState.ONLINE );
			  GatheringIndexWriter writer = new GatheringIndexWriter();
			  when( _mockedIndexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( writer );

			  // When
			  StartDb();

			  // Then
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, hasSize( 1 ) ) );
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Online ) ) );
			  verify( _mockedIndexProvider, times( 1 ) ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  int onlineAccessorInvocationCount = 2; // once when we create the index, and once when we restart the db
			  verify( _mockedIndexProvider, times( onlineAccessorInvocationCount ) ).getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) );
			  assertEquals( expectedUpdates, writer.BatchedUpdates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepFailedIndexesAsFailedAfterRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepFailedIndexesAsFailedAfterRestart()
		 {
			  // Given
			  IndexPopulator indexPopulator = mock( typeof( IndexPopulator ) );
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(indexPopulator);
			  IndexAccessor indexAccessor = mock( typeof( IndexAccessor ) );
			  when( _mockedIndexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( indexAccessor );
			  StartDb();
			  CreateIndex( _myLabel );
			  RotateLogsAndCheckPoint();

			  // And Given
			  KillDb();
			  when( _mockedIndexProvider.getInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( InternalIndexState.FAILED );

			  // When
			  StartDb();

			  // Then
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, hasSize( 1 ) ) );
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Failed ) ) );
			  verify( _mockedIndexProvider, times( 2 ) ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
		 }

		 private GraphDatabaseAPI _db;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
		 private readonly IndexProvider _mockedIndexProvider = mock( typeof( IndexProvider ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.kernel.extension.KernelExtensionFactory<?> mockedIndexProviderFactory = singleInstanceIndexProviderFactory(PROVIDER_DESCRIPTOR.getKey(), mockedIndexProvider);
		 private KernelExtensionFactory<object> _mockedIndexProviderFactory;
		 private readonly string _key = "number_of_bananas_owned";
		 private readonly Label _myLabel = label( "MyLabel" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.MisconfiguredIndexException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  when( _mockedIndexProvider.ProviderDescriptor ).thenReturn( PROVIDER_DESCRIPTOR );
			  when( _mockedIndexProvider.storeMigrationParticipant( any( typeof( FileSystemAbstraction ) ), any( typeof( PageCache ) ) ) ).thenReturn( Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating );
			  when( _mockedIndexProvider.bless( any( typeof( IndexDescriptor ) ) ) ).thenCallRealMethod();
		 }

		 private void StartDb()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }

			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  factory.FileSystem = Fs.get();
			  factory.KernelExtensions = Collections.singletonList( _mockedIndexProviderFactory );
			  _db = ( GraphDatabaseAPI ) factory.NewImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.default_schema_provider, PROVIDER_DESCRIPTOR.name()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void killDb() throws Exception
		 private void KillDb()
		 {
			  if ( _db != null )
			  {
					Fs.snapshot(() =>
					{
					 _db.shutdown();
					 _db = null;
					});
			  }
		 }

		 private Future<Void> KillDbInSeparateThread()
		 {
			  ExecutorService executor = newSingleThreadExecutor();
			  Future<Void> result = executor.submit(() =>
			  {
				KillDb();
				return null;
			  });
			  executor.shutdown();
			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rotateLogsAndCheckPoint() throws java.io.IOException
		 private void RotateLogsAndCheckPoint()
		 {
			  _db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );
		 }

		 private void CreateIndexAndAwaitPopulation( Label label )
		 {
			  IndexDefinition index = CreateIndex( label );
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexOnline(index, 10, SECONDS);
					tx.Success();
			  }
		 }

		 private IndexDefinition CreateIndex( Label label )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().indexFor(label).on(_key).create();
					tx.Success();
					return index;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Set<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> createSomeBananas(org.Neo4Net.graphdb.Label label)
		 private ISet<IndexEntryUpdate<object>> CreateSomeBananas( Label label )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.HashSet<>();
			  ISet<IndexEntryUpdate<object>> updates = new HashSet<IndexEntryUpdate<object>>();
			  using ( Transaction tx = _db.beginTx() )
			  {
					ThreadToStatementContextBridge ctxSupplier = _db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
					KernelTransaction ktx = ctxSupplier.GetKernelTransactionBoundToThisThread( true );

					int labelId = ktx.TokenRead().nodeLabel(label.Name());
					int propertyKeyId = ktx.TokenRead().propertyKey(_key);
					LabelSchemaDescriptor schemaDescriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyId );
					foreach ( int number in new int[]{ 4, 10 } )
					{
						 Node node = _db.createNode( label );
						 node.SetProperty( _key, number );
						 updates.Add( IndexEntryUpdate.add( node.Id, schemaDescriptor, Values.of( number ) ) );
					}
					tx.Success();
					return updates;
			  }
		 }

		 public class GatheringIndexWriter : Neo4Net.Kernel.Api.Index.IndexAccessor_Adapter
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Set<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> regularUpdates = new java.util.HashSet<>();
			  internal readonly ISet<IndexEntryUpdate<object>> RegularUpdates = new HashSet<IndexEntryUpdate<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Set<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> batchedUpdates = new java.util.HashSet<>();
			  internal readonly ISet<IndexEntryUpdate<object>> BatchedUpdates = new HashSet<IndexEntryUpdate<object>>();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
			  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
			  {
					return new CollectingIndexUpdater(updates =>
					{
					 switch ( mode.innerEnumValue )
					 {
						  case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.ONLINE:
								RegularUpdates.addAll( updates );
								break;

						  case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.RECOVERY:
								BatchedUpdates.addAll( updates );
								break;

						  default:
								throw new System.NotSupportedException();
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.kernel.api.index.IndexPopulator indexPopulatorWithControlledCompletionTiming(final java.util.concurrent.CountDownLatch latch)
		 private static IndexPopulator IndexPopulatorWithControlledCompletionTiming( System.Threading.CountdownEvent latch )
		 {
			  return new IndexPopulator_AdapterAnonymousInnerClass( latch );
		 }

		 private class IndexPopulator_AdapterAnonymousInnerClass : Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			 private System.Threading.CountdownEvent _latch;

			 public IndexPopulator_AdapterAnonymousInnerClass( System.Threading.CountdownEvent latch )
			 {
				 this._latch = latch;
			 }

			 public override void create()
			 {
				  try
				  {
						_latch.await();
				  }
				  catch ( InterruptedException )
				  {
						// fall through and return early
				  }
				  throw new Exception( "this is expected" );
			 }
		 }
	}

}