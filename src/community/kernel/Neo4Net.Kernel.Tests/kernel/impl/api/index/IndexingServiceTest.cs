using System;
using System.Collections.Generic;
using System.Threading;

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
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using InOrder = org.mockito.InOrder;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexSamplingController = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingController;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;
	using IJobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using DirectIndexUpdates = Neo4Net.Kernel.impl.transaction.state.DirectIndexUpdates;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogMatcherBuilder = Neo4Net.Logging.AssertableLogProvider.LogMatcherBuilder;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using Barrier = Neo4Net.Test.Barrier;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.LUCENE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.multi_threaded_schema_index_population_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.loop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.schema.SchemaUtil.idTokenNameLookup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.IndexUpdateMode.RECOVERY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.MultiPopulatorFactory.forConfig;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.sampling.IndexSamplingMode.TRIGGER_REBUILD_ALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.register.Registers.newDoubleLongRegister;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.uniqueForSchema;

	public class IndexingServiceTest
	{
		private bool InstanceFieldsInitialized = false;

		public IndexingServiceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_index = forSchema( forLabel( _labelId, _propertyKeyId ), PROVIDER_DESCRIPTOR );
			_uniqueIndex = uniqueForSchema( forLabel( _labelId, _uniquePropertyKeyId ), PROVIDER_DESCRIPTOR );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.kernel.lifecycle.LifeRule life = new org.Neo4Net.kernel.lifecycle.LifeRule();
		 public readonly LifeRule Life = new LifeRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.VerboseTimeout timeoutThreadDumpRule = org.Neo4Net.test.rule.VerboseTimeout.builder().build();
		 public VerboseTimeout TimeoutThreadDumpRule = VerboseTimeout.builder().build();

		 private static readonly AssertableLogProvider.LogMatcherBuilder _logMatch = inLog( typeof( IndexingService ) );
		 private static readonly IndexProviderDescriptor _lucene10Descriptor = new IndexProviderDescriptor( LUCENE10.providerKey(), LUCENE10.providerVersion() );
		 private static readonly IndexProviderDescriptor _native10Descriptor = new IndexProviderDescriptor( NATIVE10.providerKey(), NATIVE10.providerVersion() );
		 private static readonly IndexProviderDescriptor _native20Descriptor = new IndexProviderDescriptor( NATIVE20.providerKey(), NATIVE20.providerVersion() );
		 private static readonly IndexProviderDescriptor _nativeBtree10Descriptor = new IndexProviderDescriptor( NATIVE_BTREE10.providerKey(), NATIVE_BTREE10.providerVersion() );
		 private static readonly IndexProviderDescriptor _fulltextDescriptor = new IndexProviderDescriptor( "fulltext", "1.0" );
		 private readonly SchemaState _schemaState = mock( typeof( SchemaState ) );
		 private readonly int _labelId = 7;
		 private readonly int _propertyKeyId = 15;
		 private readonly int _uniquePropertyKeyId = 15;
		 private IndexDescriptor _index;
		 private IndexDescriptor _uniqueIndex;
		 private readonly IndexPopulator _populator = mock( typeof( IndexPopulator ) );
		 private readonly IndexUpdater _updater = mock( typeof( IndexUpdater ) );
		 private readonly IndexProvider _indexProvider = mock( typeof( IndexProvider ) );
		 private readonly IndexAccessor _accessor = mock( typeof( IndexAccessor ), RETURNS_MOCKS );
		 private readonly IndexStoreView _storeView = mock( typeof( IndexStoreView ) );
		 private readonly TokenNameLookup _nameLookup = mock( typeof( TokenNameLookup ) );
		 private readonly AssertableLogProvider _internalLogProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _populator.sampleResult() ).thenReturn(new IndexSample());
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenAnswer(invocation => invocation.getArgument(1));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noMessagesWhenThereIsNoIndexes()
		 public virtual void NoMessagesWhenThereIsNoIndexes()
		 {
			  IndexMapReference indexMapReference = new IndexMapReference();
			  IndexingService indexingService = CreateIndexServiceWithCustomIndexMap( indexMapReference );
			  indexingService.Start();

			  _internalLogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBringIndexOnlineAndFlipOverToIndexAccessor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBringIndexOnlineAndFlipOverToIndexAccessor()
		 {
			  // given
			  when( _accessor.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( _updater );

			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  Life.start();

			  // when
			  indexingService.createIndexes( _index.withId( 0 ) );
			  IndexProxy proxy = indexingService.getIndexProxy( 0 );

			  WaitForIndexesToComeOnline( indexingService, 0 );
			  verify( _populator, timeout( 10000 ) ).close( true );

			  using ( IndexUpdater updater = proxy.NewUpdater( IndexUpdateMode.Online ) )
			  {
					updater.Process( Add( 10, "foo" ) );
			  }

			  // then
			  assertEquals( InternalIndexState.ONLINE, proxy.State );
			  InOrder order = inOrder( _populator, _accessor, updater );
			  order.verify( _populator ).create();
			  order.verify( _populator ).close( true );
			  order.verify( _accessor ).newUpdater( IndexUpdateMode.OnlineIdempotent );
			  order.verify( updater ).process( Add( 10, "foo" ) );
			  order.verify( updater ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexCreationShouldBeIdempotent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexCreationShouldBeIdempotent()
		 {
			  // given
			  when( _accessor.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( _updater );

			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  Life.start();

			  // when
			  indexingService.createIndexes( _index.withId( 0 ) );
			  indexingService.createIndexes( _index.withId( 0 ) );

			  // We are asserting that the second call to createIndex does not throw an exception.
			  WaitForIndexesToComeOnline( indexingService, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldDeliverUpdatesThatOccurDuringPopulationToPopulator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeliverUpdatesThatOccurDuringPopulationToPopulator()
		 {
			  // given
			  when( _populator.newPopulatingUpdater( _storeView ) ).thenReturn( _updater );

			  System.Threading.CountdownEvent populationLatch = new System.Threading.CountdownEvent( 1 );

			  Neo4Net.Test.Barrier_Control populationStartBarrier = new Neo4Net.Test.Barrier_Control();
			  IndexingService.Monitor monitor = new MonitorAdapterAnonymousInnerClass( this, populationLatch, populationStartBarrier );
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData( AddNodeUpdate( 1, "value1" ) ), monitor );

			  Life.start();

			  // when

			  indexingService.createIndexes( _index.withId( 0 ) );
			  IndexProxy proxy = indexingService.getIndexProxy( 0 );
			  assertEquals( InternalIndexState.POPULATING, proxy.State );
			  populationStartBarrier.Await();
			  populationStartBarrier.Release();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> value2 = add(2, "value2");
			  IndexEntryUpdate<object> value2 = Add( 2, "value2" );
			  using ( IndexUpdater updater = proxy.NewUpdater( IndexUpdateMode.Online ) )
			  {
					updater.Process( value2 );
			  }

			  populationLatch.Signal();

			  WaitForIndexesToComeOnline( indexingService, 0 );
			  verify( _populator ).close( true );

			  // then
			  assertEquals( InternalIndexState.ONLINE, proxy.State );
			  InOrder order = inOrder( _populator, _accessor, updater );
			  order.verify( _populator ).create();
			  order.verify( _populator ).includeSample( Add( 1, "value1" ) );
			  order.verify( _populator, times( 1 ) ).add( any( typeof( System.Collections.ICollection ) ) );
			  order.verify( _populator ).scanCompleted( any( typeof( PhaseTracker ) ) );
			  order.verify( _populator, times( 2 ) ).add( any( typeof( System.Collections.ICollection ) ) );
			  order.verify( _populator ).newPopulatingUpdater( _storeView );
			  order.verify( updater ).close();
			  order.verify( _populator ).sampleResult();
			  order.verify( _populator ).close( true );
			  verifyNoMoreInteractions( updater );
			  verifyNoMoreInteractions( _populator );

			  verifyZeroInteractions( _accessor );
		 }

		 private class MonitorAdapterAnonymousInnerClass : IndexingService.MonitorAdapter
		 {
			 private readonly IndexingServiceTest _outerInstance;

			 private System.Threading.CountdownEvent _populationLatch;
			 private Neo4Net.Test.Barrier_Control _populationStartBarrier;

			 public MonitorAdapterAnonymousInnerClass( IndexingServiceTest outerInstance, System.Threading.CountdownEvent populationLatch, Neo4Net.Test.Barrier_Control populationStartBarrier )
			 {
				 this.outerInstance = outerInstance;
				 this._populationLatch = populationLatch;
				 this._populationStartBarrier = populationStartBarrier;
			 }

			 public override void indexPopulationScanStarting()
			 {
				  _populationStartBarrier.reached();
			 }

			 public override void indexPopulationScanComplete()
			 {
				  try
				  {
						_populationLatch.await();
				  }
				  catch ( InterruptedException e )
				  {
						Thread.CurrentThread.Interrupt();
						throw new Exception( "Index population monitor was interrupted", e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStillReportInternalIndexStateAsPopulatingWhenConstraintIndexIsDonePopulating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStillReportInternalIndexStateAsPopulatingWhenConstraintIndexIsDonePopulating()
		 {
			  // given
			  when( _accessor.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( _updater );

			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  Life.start();

			  // when
			  indexingService.CreateIndexes( ConstraintIndexRule( 0, _labelId, _propertyKeyId, PROVIDER_DESCRIPTOR ) );
			  IndexProxy proxy = indexingService.getIndexProxy( 0 );

			  // don't wait for index to come ONLINE here since we're testing that it doesn't
			  verify( _populator, timeout( 20000 ) ).close( true );

			  using ( IndexUpdater updater = proxy.NewUpdater( IndexUpdateMode.Online ) )
			  {
					updater.Process( Add( 10, "foo" ) );
			  }

			  // then
			  assertEquals( InternalIndexState.POPULATING, proxy.State );
			  InOrder order = inOrder( _populator, _accessor, updater );
			  order.verify( _populator ).create();
			  order.verify( _populator ).close( true );
			  order.verify( _accessor ).newUpdater( IndexUpdateMode.Online );
			  order.verify( updater ).process( Add( 10, "foo" ) );
			  order.verify( updater ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBringConstraintIndexOnlineWhenExplicitlyToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBringConstraintIndexOnlineWhenExplicitlyToldTo()
		 {
			  // given
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  Life.start();

			  // when
			  indexingService.CreateIndexes( ConstraintIndexRule( 0, _labelId, _propertyKeyId, PROVIDER_DESCRIPTOR ) );
			  IndexProxy proxy = indexingService.getIndexProxy( 0 );

			  indexingService.ActivateIndex( 0 );

			  // then
			  assertEquals( ONLINE, proxy.State );
			  InOrder order = inOrder( _populator, _accessor );
			  order.verify( _populator ).create();
			  order.verify( _populator ).close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIndexStateOnInit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIndexStateOnInit()
		 {
			  // given
			  IndexProvider provider = MockIndexProviderWithAccessor( PROVIDER_DESCRIPTOR );
			  Config config = Config.defaults( default_schema_provider, PROVIDER_DESCRIPTOR.name() );
			  IndexProviderMap providerMap = Life.add( new DefaultIndexProviderMap( BuildIndexDependencies( provider ), config ) );
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  StoreIndexDescriptor onlineIndex = StoreIndex( 1, 1, 1, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor populatingIndex = StoreIndex( 2, 1, 2, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor failedIndex = StoreIndex( 3, 2, 2, PROVIDER_DESCRIPTOR );

			  Life.add( IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, mock( typeof( IndexStoreView ) ), mockLookup, asList( onlineIndex, populatingIndex, failedIndex ), _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false ) );

			  when( provider.GetInitialState( onlineIndex ) ).thenReturn( ONLINE );
			  when( provider.GetInitialState( populatingIndex ) ).thenReturn( InternalIndexState.POPULATING );
			  when( provider.GetInitialState( failedIndex ) ).thenReturn( InternalIndexState.FAILED );

			  when( mockLookup.LabelGetName( 1 ) ).thenReturn( "LabelOne" );
			  when( mockLookup.LabelGetName( 2 ) ).thenReturn( "LabelTwo" );
			  when( mockLookup.PropertyKeyGetName( 1 ) ).thenReturn( "propertyOne" );
			  when( mockLookup.PropertyKeyGetName( 2 ) ).thenReturn( "propertyTwo" );

			  // when
			  Life.init();

			  // then
			  _internalLogProvider.assertAtLeastOnce( _logMatch.debug( "IndexingService.init: index 1 on :LabelOne(propertyOne) is ONLINE" ), _logMatch.debug( "IndexingService.init: index 2 on :LabelOne(propertyTwo) is POPULATING" ), _logMatch.debug( "IndexingService.init: index 3 on :LabelTwo(propertyTwo) is FAILED" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIndexStateOnStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIndexStateOnStart()
		 {
			  // given
			  IndexProvider provider = MockIndexProviderWithAccessor( PROVIDER_DESCRIPTOR );
			  Config config = Config.defaults( default_schema_provider, PROVIDER_DESCRIPTOR.name() );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( BuildIndexDependencies( provider ), config );
			  providerMap.Init();
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  StoreIndexDescriptor onlineIndex = StoreIndex( 1, 1, 1, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor populatingIndex = StoreIndex( 2, 1, 2, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor failedIndex = StoreIndex( 3, 2, 2, PROVIDER_DESCRIPTOR );

			  IndexingService indexingService = IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, _storeView, mockLookup, asList( onlineIndex, populatingIndex, failedIndex ), _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false );

			  when( provider.GetInitialState( onlineIndex ) ).thenReturn( ONLINE );
			  when( provider.GetInitialState( populatingIndex ) ).thenReturn( InternalIndexState.POPULATING );
			  when( provider.GetInitialState( failedIndex ) ).thenReturn( InternalIndexState.FAILED );

			  indexingService.Init();

			  when( mockLookup.LabelGetName( 1 ) ).thenReturn( "LabelOne" );
			  when( mockLookup.LabelGetName( 2 ) ).thenReturn( "LabelTwo" );
			  when( mockLookup.PropertyKeyGetName( 1 ) ).thenReturn( "propertyOne" );
			  when( mockLookup.PropertyKeyGetName( 2 ) ).thenReturn( "propertyTwo" );
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenReturn(newDoubleLongRegister(32L, 32L));

			  _internalLogProvider.clear();

			  // when
			  indexingService.Start();

			  // then
			  verify( provider ).getPopulationFailure( failedIndex );
			  _internalLogProvider.assertAtLeastOnce( _logMatch.debug( "IndexingService.start: index 1 on :LabelOne(propertyOne) is ONLINE" ), _logMatch.debug( "IndexingService.start: index 2 on :LabelOne(propertyTwo) is POPULATING" ), _logMatch.debug( "IndexingService.start: index 3 on :LabelTwo(propertyTwo) is FAILED" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogWhenNoDeprecatedIndexesOnInit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogWhenNoDeprecatedIndexesOnInit()
		 {
			  // given
			  StoreIndexDescriptor nativeBtree10Index = StoreIndex( 5, 1, 5, _nativeBtree10Descriptor );
			  StoreIndexDescriptor fulltextIndex = StoreIndex( 6, 1, 6, _fulltextDescriptor );

			  IndexProvider lucene10Provider = MockIndexProviderWithAccessor( _lucene10Descriptor );
			  IndexProvider native10Provider = MockIndexProviderWithAccessor( _native10Descriptor );
			  IndexProvider native20Provider = MockIndexProviderWithAccessor( _native20Descriptor );
			  IndexProvider nativeBtree10Provider = MockIndexProviderWithAccessor( _nativeBtree10Descriptor );
			  IndexProvider fulltextProvider = MockIndexProviderWithAccessor( _fulltextDescriptor );

			  when( nativeBtree10Provider.GetInitialState( nativeBtree10Index ) ).thenReturn( ONLINE );
			  when( fulltextProvider.GetInitialState( fulltextIndex ) ).thenReturn( ONLINE );

			  Config config = Config.defaults( default_schema_provider, _nativeBtree10Descriptor.name() );
			  DependencyResolver dependencies = BuildIndexDependencies( lucene10Provider, native10Provider, native20Provider, nativeBtree10Provider );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( dependencies, config );
			  providerMap.Init();
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  IndexingService indexingService = IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, _storeView, mockLookup, Collections.singletonList( nativeBtree10Index ),_internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false );

			  // when
			  indexingService.Init();

			  // then
			  OnBothLogProviders( logProvider => logProvider.rawMessageMatcher().assertNotContains("IndexingService.init: Deprecated index providers in use:") );
			  OnBothLogProviders( logProvider => _internalLogProvider.rawMessageMatcher().assertNotContains(_nativeBtree10Descriptor.name()) );
			  OnBothLogProviders( logProvider => _internalLogProvider.rawMessageMatcher().assertNotContains(_fulltextDescriptor.name()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogWhenNoDeprecatedIndexesOnStart() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogWhenNoDeprecatedIndexesOnStart()
		 {
			  // given
			  StoreIndexDescriptor nativeBtree10Index = StoreIndex( 5, 1, 5, _nativeBtree10Descriptor );
			  StoreIndexDescriptor fulltextIndex = StoreIndex( 6, 1, 6, _fulltextDescriptor );

			  IndexProvider lucene10Provider = MockIndexProviderWithAccessor( _lucene10Descriptor );
			  IndexProvider native10Provider = MockIndexProviderWithAccessor( _native10Descriptor );
			  IndexProvider native20Provider = MockIndexProviderWithAccessor( _native20Descriptor );
			  IndexProvider nativeBtree10Provider = MockIndexProviderWithAccessor( _nativeBtree10Descriptor );
			  IndexProvider fulltextProvider = MockIndexProviderWithAccessor( _fulltextDescriptor );

			  when( nativeBtree10Provider.GetInitialState( nativeBtree10Index ) ).thenReturn( ONLINE );
			  when( fulltextProvider.GetInitialState( fulltextIndex ) ).thenReturn( ONLINE );

			  Config config = Config.defaults( default_schema_provider, _nativeBtree10Descriptor.name() );
			  DependencyResolver dependencies = BuildIndexDependencies( lucene10Provider, native10Provider, native20Provider, nativeBtree10Provider, fulltextProvider );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( dependencies, config );
			  providerMap.Init();
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  IndexingService indexingService = IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, _storeView, mockLookup, Collections.singletonList( nativeBtree10Index ), _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false );

			  // when
			  indexingService.Init();
			  _internalLogProvider.clear();
			  indexingService.Start();

			  // then
			  AssertableLogProvider.MessageMatcher messageMatcher = _internalLogProvider.rawMessageMatcher();
			  OnBothLogProviders( logProvider => messageMatcher.assertNotContains( "IndexingService.start: Deprecated index providers in use:" ) );
			  OnBothLogProviders( logProvider => messageMatcher.assertNotContains( _nativeBtree10Descriptor.name() ) );
			  OnBothLogProviders( logProvider => messageMatcher.assertNotContains( _fulltextDescriptor.name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDeprecatedIndexesOnStart() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDeprecatedIndexesOnStart()
		 {
			  // given
			  StoreIndexDescriptor lucene10Index = StoreIndex( 1, 1, 1, _lucene10Descriptor );
			  StoreIndexDescriptor native10Index = StoreIndex( 2, 1, 2, _native10Descriptor );
			  StoreIndexDescriptor native20Index1 = StoreIndex( 3, 1, 3, _native20Descriptor );
			  StoreIndexDescriptor native20Index2 = StoreIndex( 4, 1, 4, _native20Descriptor );
			  StoreIndexDescriptor nativeBtree10Index = StoreIndex( 5, 1, 5, _nativeBtree10Descriptor );
			  StoreIndexDescriptor fulltextIndex = StoreIndex( 6, 1, 6, _fulltextDescriptor );

			  IndexProvider lucene10Provider = MockIndexProviderWithAccessor( _lucene10Descriptor );
			  IndexProvider native10Provider = MockIndexProviderWithAccessor( _native10Descriptor );
			  IndexProvider native20Provider = MockIndexProviderWithAccessor( _native20Descriptor );
			  IndexProvider nativeBtree10Provider = MockIndexProviderWithAccessor( _nativeBtree10Descriptor );
			  IndexProvider fulltextProvider = MockIndexProviderWithAccessor( _fulltextDescriptor );

			  when( lucene10Provider.GetInitialState( lucene10Index ) ).thenReturn( ONLINE );
			  when( native10Provider.GetInitialState( native10Index ) ).thenReturn( ONLINE );
			  when( native20Provider.GetInitialState( native20Index1 ) ).thenReturn( ONLINE );
			  when( native20Provider.GetInitialState( native20Index2 ) ).thenReturn( ONLINE );
			  when( nativeBtree10Provider.GetInitialState( nativeBtree10Index ) ).thenReturn( ONLINE );
			  when( fulltextProvider.GetInitialState( fulltextIndex ) ).thenReturn( ONLINE );

			  Config config = Config.defaults( default_schema_provider, _nativeBtree10Descriptor.name() );
			  DependencyResolver dependencies = BuildIndexDependencies( lucene10Provider, native10Provider, native20Provider, nativeBtree10Provider );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( dependencies, config );
			  providerMap.Init();
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  IndexingService indexingService = IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, _storeView, mockLookup, asList( lucene10Index, native10Index, native20Index1, native20Index2, nativeBtree10Index ), _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false );

			  // when
			  indexingService.Init();
			  _userLogProvider.clear();
			  indexingService.Start();

			  // then
			  _userLogProvider.rawMessageMatcher().assertContainsSingle(Matchers.allOf(Matchers.containsString("Deprecated index providers in use:"), Matchers.containsString(_lucene10Descriptor.name() + " (1 index)"), Matchers.containsString(_native10Descriptor.name() + " (1 index)"), Matchers.containsString(_native20Descriptor.name() + " (2 indexes)"), Matchers.containsString("Use procedure 'db.indexes()' to see what indexes use which index provider.")));
			  OnBothLogProviders( logProvider => _internalLogProvider.rawMessageMatcher().assertNotContains(_nativeBtree10Descriptor.name()) );
			  OnBothLogProviders( logProvider => _internalLogProvider.rawMessageMatcher().assertNotContains(_fulltextDescriptor.name()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToStartIfMissingIndexProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToStartIfMissingIndexProvider()
		 {
			  // GIVEN an indexing service that has a schema index provider X
			  string otherProviderKey = "something-completely-different";
			  IndexProviderDescriptor otherDescriptor = new IndexProviderDescriptor( otherProviderKey, "no-version" );
			  StoreIndexDescriptor rule = StoreIndex( 1, 2, 3, otherDescriptor );
			  NewIndexingServiceWithMockedDependencies( mock( typeof( IndexPopulator ) ), mock( typeof( IndexAccessor ) ), new DataUpdates(), rule );

			  // WHEN trying to start up and initialize it with an index from provider Y
			  try
			  {
					Life.init();
					fail( "initIndexes with mismatching index provider should fail" );
			  }
			  catch ( LifecycleException e )
			  { // THEN starting up should fail
					assertThat( e.InnerException.Message, containsString( PROVIDER_DESCRIPTOR.name() ) );
					assertThat( e.InnerException.Message, containsString( otherProviderKey ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSnapshotOnlineIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSnapshotOnlineIndexes()
		 {
			  // GIVEN
			  int indexId = 1;
			  int indexId2 = 2;
			  StoreIndexDescriptor rule1 = StoreIndex( indexId, 2, 3, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor rule2 = StoreIndex( indexId2, 4, 5, PROVIDER_DESCRIPTOR );

			  IndexAccessor indexAccessor = mock( typeof( IndexAccessor ) );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( mock( typeof( IndexPopulator ) ), indexAccessor, new DataUpdates(), rule1, rule2 );
			  File theFile = new File( "Blah" );

			  when( indexAccessor.SnapshotFiles() ).thenAnswer(NewResourceIterator(theFile));
			  when( _indexProvider.getInitialState( rule1 ) ).thenReturn( ONLINE );
			  when( _indexProvider.getInitialState( rule2 ) ).thenReturn( ONLINE );
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenReturn(newDoubleLongRegister(32L, 32L));

			  Life.start();

			  // WHEN
			  ResourceIterator<File> files = indexing.SnapshotIndexFiles();

			  // THEN
			  // We get a snapshot per online index
			  assertThat( asCollection( files ), equalTo( asCollection( iterator( theFile, theFile ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSnapshotPopulatingIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSnapshotPopulatingIndexes()
		 {
			  // GIVEN
			  System.Threading.CountdownEvent populatorLatch = new System.Threading.CountdownEvent( 1 );
			  IndexAccessor indexAccessor = mock( typeof( IndexAccessor ) );
			  int indexId = 1;
			  int indexId2 = 2;
			  StoreIndexDescriptor rule1 = StoreIndex( indexId, 2, 3, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor rule2 = StoreIndex( indexId2, 4, 5, PROVIDER_DESCRIPTOR );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, indexAccessor, new DataUpdates(), rule1, rule2 );
			  File theFile = new File( "Blah" );

			  doAnswer( WaitForLatch( populatorLatch ) ).when( _populator ).create();
			  when( indexAccessor.SnapshotFiles() ).thenAnswer(NewResourceIterator(theFile));
			  when( _indexProvider.getInitialState( rule1 ) ).thenReturn( POPULATING );
			  when( _indexProvider.getInitialState( rule2 ) ).thenReturn( ONLINE );
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenReturn(newDoubleLongRegister(32L, 32L));
			  Life.start();

			  // WHEN
			  ResourceIterator<File> files = indexing.SnapshotIndexFiles();
			  populatorLatch.Signal(); // only now, after the snapshot, is the population job allowed to finish
			  WaitForIndexesToComeOnline( indexing, indexId, indexId2 );

			  // THEN
			  // We get a snapshot from the online index, but no snapshot from the populating one
			  assertThat( asCollection( files ), equalTo( asCollection( iterator( theFile ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreActivateCallDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreActivateCallDuringRecovery()
		 {
			  // given
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  // when
			  indexingService.ActivateIndex( 0 );

			  // then no exception should be thrown.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogTriggerSamplingOnAllIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogTriggerSamplingOnAllIndexes()
		 {
			  // given
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );
			  IndexSamplingMode mode = TRIGGER_REBUILD_ALL;

			  // when
			  indexingService.TriggerIndexSampling( mode );

			  // then
			  _internalLogProvider.assertAtLeastOnce( _logMatch.info( "Manual trigger for sampling all indexes [" + mode + "]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogTriggerSamplingOnAnIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogTriggerSamplingOnAnIndexes()
		 {
			  // given
			  long indexId = 0;
			  IndexSamplingMode mode = TRIGGER_REBUILD_ALL;
			  IndexDescriptor descriptor = forSchema( forLabel( 0, 1 ), PROVIDER_DESCRIPTOR );
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData(), descriptor.WithId(indexId) );
			  Life.init();
			  Life.start();

			  // when
			  indexingService.TriggerIndexSampling( descriptor.Schema(), mode );

			  // then
			  string userDescription = descriptor.Schema().userDescription(_nameLookup);
			  _internalLogProvider.assertAtLeastOnce( _logMatch.info( "Manual trigger for sampling index " + userDescription + " [" + mode + "]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applicationOfIndexUpdatesShouldThrowIfServiceIsShutdown() throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplicationOfIndexUpdatesShouldThrowIfServiceIsShutdown()
		 {
			  // Given
			  IndexingService indexingService = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );
			  Life.start();
			  Life.shutdown();

			  try
			  {
					// When
					indexingService.Apply( Updates( asSet( Add( 1, "foo" ) ) ) );
					fail( "Should have thrown " + typeof( System.InvalidOperationException ).Name );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// Then
					assertThat( e.Message, startsWith( "Can't apply index updates" ) );
			  }
		 }

		 private IndexUpdates Updates( IEnumerable<IndexEntryUpdate<SchemaDescriptor>> updates )
		 {
			  return new DirectIndexUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applicationOfUpdatesShouldFlush() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplicationOfUpdatesShouldFlush()
		 {
			  // Given
			  when( _accessor.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( _updater );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );
			  Life.start();

			  indexing.createIndexes( _index.withId( 0 ) );
			  WaitForIndexesToComeOnline( indexing, 0 );
			  verify( _populator, timeout( 10000 ) ).close( true );

			  // When
			  indexing.Apply( Updates( asList( Add( 1, "foo" ), Add( 2, "bar" ) ) ) );

			  // Then
			  InOrder inOrder = inOrder( _updater );
			  inOrder.verify( _updater ).process( Add( 1, "foo" ) );
			  inOrder.verify( _updater ).process( Add( 2, "bar" ) );
			  inOrder.verify( _updater ).close();
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closingOfValidatedUpdatesShouldCloseUpdaters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClosingOfValidatedUpdatesShouldCloseUpdaters()
		 {
			  // Given
			  long indexId1 = 1;
			  long indexId2 = 2;

			  int labelId1 = 24;
			  int labelId2 = 42;

			  StoreIndexDescriptor index1 = StoreIndex( indexId1, labelId1, _propertyKeyId, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor index2 = StoreIndex( indexId2, labelId2, _propertyKeyId, PROVIDER_DESCRIPTOR );

			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  IndexAccessor accessor1 = mock( typeof( IndexAccessor ) );
			  IndexUpdater updater1 = mock( typeof( IndexUpdater ) );
			  when( accessor1.NewUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( updater1 );

			  IndexAccessor accessor2 = mock( typeof( IndexAccessor ) );
			  IndexUpdater updater2 = mock( typeof( IndexUpdater ) );
			  when( accessor2.NewUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( updater2 );

			  when( _indexProvider.getOnlineAccessor( eq( index1 ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor1 );
			  when( _indexProvider.getOnlineAccessor( eq( index2 ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor2 );

			  Life.start();

			  indexing.CreateIndexes( index1 );
			  indexing.CreateIndexes( index2 );

			  WaitForIndexesToComeOnline( indexing, indexId1, indexId2 );

			  verify( _populator, timeout( 10000 ).times( 2 ) ).close( true );

			  // When
			  indexing.Apply( Updates( asList( Add( 1, "foo", labelId1 ), Add( 2, "bar", labelId2 ) ) ) );

			  // Then
			  verify( updater1 ).close();
			  verify( updater2 ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForIndexesToComeOnline(IndexingService indexing, long... indexRuleIds) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 private void WaitForIndexesToComeOnline( IndexingService indexing, params long[] indexRuleIds )
		 {
			  WaitForIndexesToGetIntoState( indexing, ONLINE, indexRuleIds );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForIndexesToGetIntoState(IndexingService indexing, org.Neo4Net.Kernel.Api.Internal.InternalIndexState state, long... indexRuleIds) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 private void WaitForIndexesToGetIntoState( IndexingService indexing, InternalIndexState state, params long[] indexRuleIds )
		 {
			  long end = currentTimeMillis() + SECONDS.toMillis(30);
			  while ( !AllInState( indexing, state, indexRuleIds ) )
			  {
					if ( currentTimeMillis() > end )
					{
						 fail( "Indexes couldn't come online" );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean allInState(IndexingService indexing, org.Neo4Net.Kernel.Api.Internal.InternalIndexState state, long[] indexRuleIds) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 private bool AllInState( IndexingService indexing, InternalIndexState state, long[] indexRuleIds )
		 {
			  foreach ( long indexRuleId in indexRuleIds )
			  {
					if ( indexing.GetIndexProxy( indexRuleId ).State != state )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private IndexUpdates NodeIdsAsIndexUpdates( params long[] nodeIds )
		 {
			  return new IndexUpdatesAnonymousInnerClass( this, nodeIds );
		 }

		 private class IndexUpdatesAnonymousInnerClass : IndexUpdates
		 {
			 private readonly IndexingServiceTest _outerInstance;

			 private long[] _nodeIds;

			 public IndexUpdatesAnonymousInnerClass( IndexingServiceTest outerInstance, long[] nodeIds )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeIds = nodeIds;
			 }

			 public IEnumerator<IndexEntryUpdate<SchemaDescriptor>> iterator()
			 {
				  IList<IndexEntryUpdate<SchemaDescriptor>> updates = new List<IndexEntryUpdate<SchemaDescriptor>>();
				  foreach ( long nodeId in _nodeIds )
				  {
						updates.Add( IndexEntryUpdate.add( nodeId, _outerInstance.index.schema(), Values.of(1) ) );
				  }
				  return updates.GetEnumerator();
			 }

			 public void feed( IEntityCommandGrouper<NodeCommand>.Cursor nodeCommands, IEntityCommandGrouper<RelationshipCommand>.Cursor relationshipCommands )
			 {
				  throw new System.NotSupportedException();
			 }

			 public bool hasUpdates()
			 {
				  return _nodeIds.Length > 0;
			 }
		 }

		 /*
		  * See comments in IndexingService#createIndex
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoseIndexDescriptorDueToOtherSimilarIndexDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoseIndexDescriptorDueToOtherSimilarIndexDuringRecovery()
		 {
			  // GIVEN
			  long nodeId = 0;
			  long indexId = 1;
			  long otherIndexId = 2;
			  IEntityUpdates update = AddNodeUpdate( nodeId, "value" );
			  when( _storeView.nodeAsUpdates( eq( nodeId ) ) ).thenReturn( update );
			  Register_DoubleLongRegister register = mock( typeof( Register_DoubleLongRegister ) );
			  when( register.ReadSecond() ).thenReturn(42L);
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenReturn(register);
			  // For some reason the usual accessor returned null from newUpdater, even when told to return the updater
			  // so spying on a real object instead.
			  IndexAccessor accessor = spy( new TrackingIndexAccessor() );
			  StoreIndexDescriptor storeIndex = _index.withId( indexId );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, accessor, WithData( update ), storeIndex );
			  when( _indexProvider.getInitialState( storeIndex ) ).thenReturn( ONLINE );
			  Life.init();

			  // WHEN dropping another index, which happens to have the same label/property... while recovering
			  StoreIndexDescriptor otherIndex = storeIndex.WithId( otherIndexId );
			  indexing.CreateIndexes( otherIndex );
			  indexing.DropIndex( otherIndex );
			  // and WHEN finally creating our index again (at a later point in recovery)
			  indexing.CreateIndexes( storeIndex );
			  reset( accessor );
			  indexing.Apply( NodeIdsAsIndexUpdates( nodeId ) );
			  // and WHEN starting, i.e. completing recovery
			  Life.start();

			  verify( accessor ).newUpdater( RECOVERY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitForRecoveredUniquenessConstraintIndexesToBeFullyPopulated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForRecoveredUniquenessConstraintIndexesToBeFullyPopulated()
		 {
			  // I.e. when a uniqueness constraint is created, but database crashes before that schema record
			  // ends up in the store, so that next start have no choice but to rebuild it.

			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch latch = new org.Neo4Net.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();
			  ControlledIndexPopulator populator = new ControlledIndexPopulator( latch );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong indexId = new java.util.concurrent.atomic.AtomicLong(-1);
			  AtomicLong indexId = new AtomicLong( -1 );
			  IndexingService.Monitor monitor = new MonitorAdapterAnonymousInnerClass2( this, latch, indexId );
			  // leaving out the IndexRule here will have the index being populated from scratch
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( populator, _accessor, WithData( AddNodeUpdate( 0, "value", 1 ) ), monitor );

			  // WHEN initializing, i.e. preparing for recovery
			  Life.init();
			  // simulating an index being created as part of applying recovered transactions
			  long fakeOwningConstraintRuleId = 1;
			  indexing.CreateIndexes( ConstraintIndexRule( 2, _labelId, _propertyKeyId, PROVIDER_DESCRIPTOR, fakeOwningConstraintRuleId ) );
			  // and then starting, i.e. considering recovery completed
			  Life.start();

			  // THEN afterwards the index should be ONLINE
			  assertEquals( 2, indexId.get() );
			  assertEquals( ONLINE, indexing.getIndexProxy( indexId.get() ).State );
		 }

		 private class MonitorAdapterAnonymousInnerClass2 : IndexingService.MonitorAdapter
		 {
			 private readonly IndexingServiceTest _outerInstance;

			 private DoubleLatch _latch;
			 private AtomicLong _indexId;

			 public MonitorAdapterAnonymousInnerClass2( IndexingServiceTest outerInstance, DoubleLatch latch, AtomicLong indexId )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._indexId = indexId;
			 }

			 public override void awaitingPopulationOfRecoveredIndex( StoreIndexDescriptor descriptor )
			 {
				  // When we see that we start to await the index to populate, notify the slow-as-heck
				  // populator that it can actually go and complete its job.
				  _indexId.set( descriptor.Id );
				  _latch.startAndWaitForAllToStart();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateMultipleIndexesInOneCall() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateMultipleIndexesInOneCall()
		 {
			  // GIVEN
			  IndexingService.Monitor monitor = IndexingService.NoMonitor;
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData( AddNodeUpdate( 0, "value", 1 ) ), monitor );
			  Life.start();

			  // WHEN
			  StoreIndexDescriptor indexRule1 = StoreIndex( 0, 0, 0, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor indexRule2 = StoreIndex( 1, 0, 1, PROVIDER_DESCRIPTOR );
			  StoreIndexDescriptor indexRule3 = StoreIndex( 2, 1, 0, PROVIDER_DESCRIPTOR );
			  indexing.CreateIndexes( indexRule1, indexRule2, indexRule3 );

			  // THEN
			  verify( _indexProvider ).getPopulator( eq( forSchema( forLabel( 0, 0 ), PROVIDER_DESCRIPTOR ).withId( 0 ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( _indexProvider ).getPopulator( eq( forSchema( forLabel( 0, 1 ), PROVIDER_DESCRIPTOR ).withId( 1 ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( _indexProvider ).getPopulator( eq( forSchema( forLabel( 1, 0 ), PROVIDER_DESCRIPTOR ).withId( 2 ) ), any( typeof( IndexSamplingConfig ) ), any() );

			  WaitForIndexesToComeOnline( indexing, 0, 1, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreIndexFailureWhenFailingToCreateOnlineAccessorAfterPopulating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreIndexFailureWhenFailingToCreateOnlineAccessorAfterPopulating()
		 {
			  // given
			  long indexId = 1;
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData() );

			  IOException exception = new IOException( "Expected failure" );
			  when( _nameLookup.labelGetName( _labelId ) ).thenReturn( "TheLabel" );
			  when( _nameLookup.propertyKeyGetName( _propertyKeyId ) ).thenReturn( "propertyKey" );

			  when( _indexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenThrow( exception );

			  Life.start();
			  ArgumentCaptor<bool> closeArgs = ArgumentCaptor.forClass( typeof( Boolean ) );

			  // when
			  indexing.createIndexes( _index.withId( indexId ) );
			  WaitForIndexesToGetIntoState( indexing, InternalIndexState.FAILED, indexId );
			  verify( _populator, timeout( 10000 ).times( 2 ) ).close( closeArgs.capture() );

			  // then
			  assertEquals( FAILED, indexing.getIndexProxy( 1 ).State );
			  assertEquals( asList( true, false ), closeArgs.AllValues );
			  assertThat( StoredFailure(), containsString(format("java.io.IOException: Expected failure%n\tat ")) );
			  _internalLogProvider.assertAtLeastOnce( inLog( typeof( IndexPopulationJob ) ).error( equalTo( "Failed to populate index: [:TheLabel(propertyKey) [provider: {key=quantum-dex, version=25.0}]]" ), CausedBy( exception ) ) );
			  _internalLogProvider.assertNone( inLog( typeof( IndexPopulationJob ) ).info( "Index population completed. Index is now online: [%s]", ":TheLabel(propertyKey) [provider: {key=quantum-dex, version=25.0}]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreIndexFailureWhenFailingToCreateOnlineAccessorAfterRecoveringPopulatingIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreIndexFailureWhenFailingToCreateOnlineAccessorAfterRecoveringPopulatingIndex()
		 {
			  // given
			  long indexId = 1;
			  StoreIndexDescriptor indexRule = _index.withId( indexId );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData(), indexRule );

			  IOException exception = new IOException( "Expected failure" );
			  when( _nameLookup.labelGetName( _labelId ) ).thenReturn( "TheLabel" );
			  when( _nameLookup.propertyKeyGetName( _propertyKeyId ) ).thenReturn( "propertyKey" );

			  when( _indexProvider.getInitialState( indexRule ) ).thenReturn( POPULATING );
			  when( _indexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenThrow( exception );

			  Life.start();
			  ArgumentCaptor<bool> closeArgs = ArgumentCaptor.forClass( typeof( Boolean ) );

			  // when
			  WaitForIndexesToGetIntoState( indexing, InternalIndexState.FAILED, indexId );
			  verify( _populator, timeout( 10000 ).times( 2 ) ).close( closeArgs.capture() );

			  // then
			  assertEquals( FAILED, indexing.getIndexProxy( 1 ).State );
			  assertEquals( asList( true, false ), closeArgs.AllValues );
			  assertThat( StoredFailure(), containsString(format("java.io.IOException: Expected failure%n\tat ")) );
			  _internalLogProvider.assertAtLeastOnce( inLog( typeof( IndexPopulationJob ) ).error( equalTo( "Failed to populate index: [:TheLabel(propertyKey) [provider: {key=quantum-dex, version=25.0}]]" ), CausedBy( exception ) ) );
			  _internalLogProvider.assertNone( inLog( typeof( IndexPopulationJob ) ).info( "Index population completed. Index is now online: [%s]", ":TheLabel(propertyKey) [provider: {key=quantum-dex, version=25.0}]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000L) public void shouldReportCauseOfPopulationFailureIfPopulationFailsDuringRecovery() throws java.io.IOException, org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCauseOfPopulationFailureIfPopulationFailsDuringRecovery()
		 {
			  // given
			  long indexId = 1;
			  long constraintId = 2;
			  CapableIndexDescriptor indexRule = _uniqueIndex.withIds( indexId, constraintId ).withoutCapabilities();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  System.Threading.CountdownEvent exceptionBarrier = new System.Threading.CountdownEvent( 1 );
			  IndexingService indexing = newIndexingServiceWithMockedDependencies(_populator, _accessor, WithData(), new MonitorAdapterAnonymousInnerClass3(this, barrier)
			 , indexRule);
			  when( _indexProvider.getInitialState( indexRule ) ).thenReturn( POPULATING );

			  Life.init();
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  try
			  {
					AtomicReference<Exception> startException = new AtomicReference<Exception>();
					executor.submit(() =>
					{
					try
					{
						Life.start();
					}
					catch ( Exception t )
					{
						startException.set( t );
						exceptionBarrier.Signal();
					}
					});

					// Thread is just about to start checking index status. We flip to failed proxy to indicate population failure during recovery.
					barrier.Await();
					// Wait for the index to come online, otherwise we'll race the failed flip below with its flip and sometimes the POPULATING -> ONLINE
					// flip will win and make the index NOT fail and therefor hanging this test awaiting on the exceptionBarrier below
					WaitForIndexesToComeOnline( indexing, indexId );
					IndexProxy indexProxy = indexing.GetIndexProxy( indexRule.Schema() );
					assertThat( indexProxy, instanceOf( typeof( ContractCheckingIndexProxy ) ) );
					ContractCheckingIndexProxy contractCheckingIndexProxy = ( ContractCheckingIndexProxy ) indexProxy;
					IndexProxy @delegate = contractCheckingIndexProxy.Delegate;
					assertThat( @delegate, instanceOf( typeof( FlippableIndexProxy ) ) );
					FlippableIndexProxy flippableIndexProxy = ( FlippableIndexProxy ) @delegate;
					Exception expectedCause = new Exception( "index was failed on purpose" );
					IndexPopulationFailure indexFailure = IndexPopulationFailure.Failure( expectedCause );
					flippableIndexProxy.FlipTo( new FailedIndexProxy( indexRule, "string", mock( typeof( IndexPopulator ) ), indexFailure, mock( typeof( IndexCountsRemover ) ), _internalLogProvider ) );
					barrier.Release();
					exceptionBarrier.await();
					Exception actual = startException.get();

					assertThat( actual.Message, Matchers.containsString( indexRule.ToString() ) );
					assertThat( actual.InnerException, instanceOf( typeof( System.InvalidOperationException ) ) );
					assertThat( Exceptions.stringify( actual.InnerException ), Matchers.containsString( Exceptions.stringify( expectedCause ) ) );
			  }
			  finally
			  {
					executor.shutdown();
			  }
		 }

		 private class MonitorAdapterAnonymousInnerClass3 : IndexingService.MonitorAdapter
		 {
			 private readonly IndexingServiceTest _outerInstance;

			 private Neo4Net.Test.Barrier_Control _barrier;

			 public MonitorAdapterAnonymousInnerClass3( IndexingServiceTest outerInstance, Neo4Net.Test.Barrier_Control barrier )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
			 }

			 public override void awaitingPopulationOfRecoveredIndex( StoreIndexDescriptor descriptor )
			 {
				  _barrier.reached();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIndexStateOutliersOnInit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIndexStateOutliersOnInit()
		 {
			  // given
			  IndexProvider provider = MockIndexProviderWithAccessor( PROVIDER_DESCRIPTOR );
			  Config config = Config.defaults( default_schema_provider, PROVIDER_DESCRIPTOR.name() );
			  IndexProviderMap providerMap = Life.add( new DefaultIndexProviderMap( BuildIndexDependencies( provider ), config ) );
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  IList<SchemaRule> indexes = new List<SchemaRule>();
			  int nextIndexId = 1;
			  StoreIndexDescriptor populatingIndex = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
			  when( provider.GetInitialState( populatingIndex ) ).thenReturn( POPULATING );
			  indexes.Add( populatingIndex );
			  StoreIndexDescriptor failedIndex = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
			  when( provider.GetInitialState( failedIndex ) ).thenReturn( FAILED );
			  indexes.Add( failedIndex );
			  for ( int i = 0; i < 10; i++ )
			  {
					StoreIndexDescriptor indexRule = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
					when( provider.GetInitialState( indexRule ) ).thenReturn( ONLINE );
					indexes.Add( indexRule );
			  }
			  for ( int i = 0; i < nextIndexId; i++ )
			  {
					when( mockLookup.LabelGetName( i ) ).thenReturn( "Label" + i );
			  }

			  Life.add( IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, mock( typeof( IndexStoreView ) ), mockLookup, indexes, _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false ) );

			  when( mockLookup.PropertyKeyGetName( 1 ) ).thenReturn( "prop" );

			  // when
			  Life.init();

			  // then
			  _internalLogProvider.assertAtLeastOnce( _logMatch.info( "IndexingService.init: index 1 on :Label1(prop) is POPULATING" ), _logMatch.info( "IndexingService.init: index 2 on :Label2(prop) is FAILED" ), _logMatch.info( "IndexingService.init: indexes not specifically mentioned above are ONLINE" ) );
			  _internalLogProvider.assertNone( _logMatch.info( "IndexingService.init: index 3 on :Label3(prop) is ONLINE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIndexStateOutliersOnStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIndexStateOutliersOnStart()
		 {
			  // given
			  IndexProvider provider = MockIndexProviderWithAccessor( PROVIDER_DESCRIPTOR );
			  Config config = Config.defaults( default_schema_provider, PROVIDER_DESCRIPTOR.name() );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( BuildIndexDependencies( provider ), config );
			  providerMap.Init();
			  TokenNameLookup mockLookup = mock( typeof( TokenNameLookup ) );

			  IList<SchemaRule> indexes = new List<SchemaRule>();
			  int nextIndexId = 1;
			  StoreIndexDescriptor populatingIndex = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
			  when( provider.GetInitialState( populatingIndex ) ).thenReturn( POPULATING );
			  indexes.Add( populatingIndex );
			  StoreIndexDescriptor failedIndex = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
			  when( provider.GetInitialState( failedIndex ) ).thenReturn( FAILED );
			  indexes.Add( failedIndex );
			  for ( int i = 0; i < 10; i++ )
			  {
					StoreIndexDescriptor indexRule = StoreIndex( nextIndexId, nextIndexId++, 1, PROVIDER_DESCRIPTOR );
					when( provider.GetInitialState( indexRule ) ).thenReturn( ONLINE );
					indexes.Add( indexRule );
			  }
			  for ( int i = 0; i < nextIndexId; i++ )
			  {
					when( mockLookup.LabelGetName( i ) ).thenReturn( "Label" + i );
			  }

			  IndexingService indexingService = IndexingServiceFactory.CreateIndexingService( config, mock( typeof( IJobScheduler ) ), providerMap, _storeView, mockLookup, indexes, _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, _schemaState, false );
			  when( _storeView.indexSample( anyLong(), any(typeof(Register_DoubleLongRegister)) ) ).thenReturn(newDoubleLongRegister(32L, 32L));
			  when( mockLookup.PropertyKeyGetName( 1 ) ).thenReturn( "prop" );

			  // when
			  indexingService.Init();
			  _internalLogProvider.clear();
			  indexingService.Start();

			  // then
			  _internalLogProvider.assertAtLeastOnce( _logMatch.info( "IndexingService.start: index 1 on :Label1(prop) is POPULATING" ), _logMatch.info( "IndexingService.start: index 2 on :Label2(prop) is FAILED" ), _logMatch.info( "IndexingService.start: indexes not specifically mentioned above are ONLINE" ) );
			  _internalLogProvider.assertNone( _logMatch.info( "IndexingService.start: index 3 on :Label3(prop) is ONLINE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushAllIndexesWhileSomeOfThemDropped() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlushAllIndexesWhileSomeOfThemDropped()
		 {
			  IndexMapReference indexMapReference = new IndexMapReference();
			  IndexProxy validIndex1 = CreateIndexProxyMock( 1 );
			  IndexProxy validIndex2 = CreateIndexProxyMock( 2 );
			  IndexProxy deletedIndexProxy = CreateIndexProxyMock( 3 );
			  IndexProxy validIndex3 = CreateIndexProxyMock( 4 );
			  IndexProxy validIndex4 = CreateIndexProxyMock( 5 );
			  indexMapReference.Modify(indexMap =>
			  {
				indexMap.putIndexProxy( validIndex1 );
				indexMap.putIndexProxy( validIndex2 );
				indexMap.putIndexProxy( deletedIndexProxy );
				indexMap.putIndexProxy( validIndex3 );
				indexMap.putIndexProxy( validIndex4 );
				return indexMap;
			  });

			  doAnswer(invocation =>
			  {
				indexMapReference.Modify(indexMap =>
				{
					 indexMap.removeIndexProxy( 3 );
					 return indexMap;
				});
				throw new Exception( "Index deleted." );
			  }).when( deletedIndexProxy ).force( any( typeof( IOLimiter ) ) );

			  IndexingService indexingService = CreateIndexServiceWithCustomIndexMap( indexMapReference );

			  indexingService.ForceAll( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  verify( validIndex1, times( 1 ) ).force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  verify( validIndex2, times( 1 ) ).force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  verify( validIndex3, times( 1 ) ).force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  verify( validIndex4, times( 1 ) ).force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failForceAllWhenOneOfTheIndexesFailToForce() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailForceAllWhenOneOfTheIndexesFailToForce()
		 {
			  IndexMapReference indexMapReference = new IndexMapReference();
			  IndexProxy strangeIndexProxy = CreateIndexProxyMock( 1 );
			  doThrow( new UncheckedIOException( new IOException( "Can't force" ) ) ).when( strangeIndexProxy ).force( any( typeof( IOLimiter ) ) );
			  indexMapReference.Modify(indexMap =>
			  {
				IndexProxy validIndex = CreateIndexProxyMock( 0 );
				indexMap.putIndexProxy( validIndex );
				indexMap.putIndexProxy( validIndex );
				indexMap.putIndexProxy( strangeIndexProxy );
				indexMap.putIndexProxy( validIndex );
				indexMap.putIndexProxy( validIndex );
				return indexMap;
			  });

			  IndexingService indexingService = CreateIndexServiceWithCustomIndexMap( indexMapReference );

			  ExpectedException.expectMessage( "Unable to force" );
			  ExpectedException.expect( typeof( UnderlyingStorageException ) );
			  indexingService.ForceAll( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRefreshIndexesOnStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRefreshIndexesOnStart()
		 {
			  // given
			  StoreIndexDescriptor rule = _index.withId( 0 );
			  NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData(), rule );

			  IndexAccessor accessor = mock( typeof( IndexAccessor ) );
			  IndexUpdater updater = mock( typeof( IndexUpdater ) );
			  when( accessor.NewUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( updater );
			  when( _indexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor );

			  Life.init();

			  verify( accessor, never() ).refresh();

			  Life.start();

			  // Then
			  verify( accessor, times( 1 ) ).refresh();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForgetDeferredIndexDropDuringRecoveryIfCreatedIndexWithSameRuleId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForgetDeferredIndexDropDuringRecoveryIfCreatedIndexWithSameRuleId()
		 {
			  // given
			  StoreIndexDescriptor rule = _index.withId( 0 );
			  IndexingService indexing = NewIndexingServiceWithMockedDependencies( _populator, _accessor, WithData(), rule );
			  Life.init();

			  // when
			  indexing.DropIndex( rule );
			  indexing.CreateIndexes( rule );
			  Life.start();

			  // then
			  IndexProxy proxy = indexing.GetIndexProxy( rule.Id );
			  assertNotNull( proxy );
			  verify( _accessor, never() ).drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveToWaitForOrphanedUniquenessIndexInRecovery() throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveToWaitForOrphanedUniquenessIndexInRecovery()
		 {
			  // given that we have a uniqueness index that needs to be recovered and that doesn't have a constraint attached to it
			  CapableIndexDescriptor descriptor = _uniqueIndex.withId( 10 ).withoutCapabilities();
			  IEnumerable<SchemaRule> schemaRules = Collections.singletonList( descriptor );
			  IndexProvider indexProvider = mock( typeof( IndexProvider ) );
			  when( indexProvider.GetInitialState( any() ) ).thenReturn(POPULATING);
			  IndexProviderMap indexProviderMap = mock( typeof( IndexProviderMap ) );
			  when( indexProviderMap.lookup( anyString() ) ).thenReturn(indexProvider);
			  when( indexProviderMap.lookup( any( typeof( IndexProviderDescriptor ) ) ) ).thenReturn( indexProvider );
			  when( indexProviderMap.DefaultProvider ).thenReturn( indexProvider );
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  IndexMapReference indexMapReference = new IndexMapReference();
			  IndexProxyCreator indexProxyCreator = mock( typeof( IndexProxyCreator ) );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexProxy.Descriptor ).thenReturn( descriptor );
			  // Eventually return ONLINE so that this test won't hang if the product code changes in this regard.
			  // This test should still fail below when verifying interactions with the proxy and monitor tho.
			  when( indexProxy.State ).thenReturn( POPULATING, POPULATING, POPULATING, POPULATING, ONLINE );
			  when( indexProxyCreator.CreateRecoveringIndexProxy( any() ) ).thenReturn(indexProxy);
			  when( indexProxyCreator.CreatePopulatingIndexProxy( any(), anyBoolean(), any(), any() ) ).thenReturn(indexProxy);
			  MultiPopulatorFactory multiPopulatorFactory = forConfig( Config.defaults( multi_threaded_schema_index_population_enabled, "false" ) );
			  IJobScheduler scheduler = mock( typeof( IJobScheduler ) );
			  IndexSamplingController samplingController = mock( typeof( IndexSamplingController ) );
			  IndexingService.Monitor monitor = mock( typeof( IndexingService.Monitor ) );
			  IndexingService indexingService = new IndexingService( indexProxyCreator, indexProviderMap, indexMapReference, null, schemaRules, samplingController, idTokenNameLookup, scheduler, null, multiPopulatorFactory, logProvider, logProvider, monitor, false );
			  // and where index population starts
			  indexingService.Init();

			  // when starting the indexing service
			  indexingService.Start();

			  // then it should be able to start without awaiting the completion of the population of the index
			  verify( indexProxyCreator, times( 1 ) ).createRecoveringIndexProxy( any() );
			  verify( indexProxyCreator, times( 1 ) ).createPopulatingIndexProxy( any(), anyBoolean(), any(), any() );
			  verify( indexProxy, never() ).awaitStoreScanCompleted(anyLong(), any());
			  verify( monitor, never() ).awaitingPopulationOfRecoveredIndex(any());
		 }

		 private static IndexProxy CreateIndexProxyMock( long indexId )
		 {
			  IndexProxy proxy = mock( typeof( IndexProxy ) );
			  CapableIndexDescriptor descriptor = StoreIndex( indexId, 1, 2, PROVIDER_DESCRIPTOR ).withoutCapabilities();
			  when( proxy.Descriptor ).thenReturn( descriptor );
			  return proxy;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.hamcrest.Matcher<? extends Throwable> causedBy(final Throwable exception)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static Matcher<Exception> CausedBy( Exception exception )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( exception );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Exception>
		 {
			 private Exception _exception;

			 public TypeSafeMatcherAnonymousInnerClass( Exception exception )
			 {
				 this._exception = exception;
			 }

			 protected internal override bool matchesSafely( Exception item )
			 {
				  while ( item != null )
				  {
						if ( item == _exception )
						{
							 return true;
						}
						item = item.InnerException;
				  }
				  return false;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "exception caused by " ).appendValue( _exception );
			 }
		 }

		 private string StoredFailure()
		 {
			  ArgumentCaptor<string> reason = ArgumentCaptor.forClass( typeof( string ) );
			  verify( _populator ).markAsFailed( reason.capture() );
			  return reason.Value;
		 }

		 private class ControlledIndexPopulator : Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			  internal readonly DoubleLatch Latch;

			  internal ControlledIndexPopulator( DoubleLatch latch )
			  {
					this.Latch = latch;
			  }

			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
					Latch.waitForAllToStart();
			  }

			  public override void Close( bool populationCompletedSuccessfully )
			  {
					Latch.finish();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.mockito.stubbing.Answer<Void> waitForLatch(final java.util.concurrent.CountDownLatch latch)
		 private static Answer<Void> WaitForLatch( System.Threading.CountdownEvent latch )
		 {
			  return invocationOnMock =>
			  {
				latch.await();
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.mockito.stubbing.Answer<org.Neo4Net.graphdb.ResourceIterator<java.io.File>> newResourceIterator(final java.io.File theFile)
		 private static Answer<ResourceIterator<File>> NewResourceIterator( File theFile )
		 {
			  return invocationOnMock => asResourceIterator( iterator( theFile ) );
		 }

		 private IEntityUpdates AddNodeUpdate( long nodeId, object propertyValue )
		 {
			  return AddNodeUpdate( nodeId, propertyValue, _labelId );
		 }

		 private IEntityUpdates AddNodeUpdate( long nodeId, object propertyValue, int labelId )
		 {
			  return IEntityUpdates.ForEntity( nodeId, false ).withTokens( labelId ).added( _index.schema().PropertyId, Values.of(propertyValue) ).build();
		 }

		 private IndexEntryUpdate<SchemaDescriptor> Add( long nodeId, object propertyValue )
		 {
			  return IndexEntryUpdate.add( nodeId, _index.schema(), Values.of(propertyValue) );
		 }

		 private IndexEntryUpdate<SchemaDescriptor> Add( long nodeId, object propertyValue, int labelId )
		 {
			  LabelSchemaDescriptor schema = forLabel( labelId, _index.schema().PropertyId );
			  return IndexEntryUpdate.add( nodeId, schema, Values.of( propertyValue ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexingService newIndexingServiceWithMockedDependencies(org.Neo4Net.kernel.api.index.IndexPopulator populator, org.Neo4Net.kernel.api.index.IndexAccessor accessor, DataUpdates data, org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor... rules) throws java.io.IOException
		 private IndexingService NewIndexingServiceWithMockedDependencies( IndexPopulator populator, IndexAccessor accessor, DataUpdates data, params StoreIndexDescriptor[] rules )
		 {
			  return NewIndexingServiceWithMockedDependencies( populator, accessor, data, IndexingService.NoMonitor, rules );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexingService newIndexingServiceWithMockedDependencies(org.Neo4Net.kernel.api.index.IndexPopulator populator, org.Neo4Net.kernel.api.index.IndexAccessor accessor, DataUpdates data, IndexingService.Monitor monitor, org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor... rules) throws java.io.IOException
		 private IndexingService NewIndexingServiceWithMockedDependencies( IndexPopulator populator, IndexAccessor accessor, DataUpdates data, IndexingService.Monitor monitor, params StoreIndexDescriptor[] rules )
		 {
			  when( _indexProvider.getInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( ONLINE );
			  when( _indexProvider.ProviderDescriptor ).thenReturn( PROVIDER_DESCRIPTOR );
			  when( _indexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  data.GetsProcessedByStoreScanFrom( _storeView );
			  when( _indexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor );
			  when( _indexProvider.storeMigrationParticipant( any( typeof( FileSystemAbstraction ) ), any( typeof( PageCache ) ) ) ).thenReturn( Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating );

			  when( _nameLookup.labelGetName( anyInt() ) ).thenAnswer(new NameLookupAnswer("label"));
			  when( _nameLookup.propertyKeyGetName( anyInt() ) ).thenAnswer(new NameLookupAnswer("property"));

			  Config config = Config.defaults( multi_threaded_schema_index_population_enabled, "false" );
			  config.augment( GraphDatabaseSettings.default_schema_provider, PROVIDER_DESCRIPTOR.name() );

			  DefaultIndexProviderMap providerMap = Life.add( new DefaultIndexProviderMap( BuildIndexDependencies( _indexProvider ), config ) );
			  return Life.add( IndexingServiceFactory.CreateIndexingService( config, Life.add( IJobSchedulerFactory.createScheduler() ), providerMap, _storeView, _nameLookup, loop(iterator(rules)), _internalLogProvider, _userLogProvider, monitor, _schemaState, false ) );
		 }

		 private static DataUpdates WithData( params IEntityUpdates[] updates )
		 {
			  return new DataUpdates( updates );
		 }

		 private class DataUpdates : Answer<StoreScan<IndexPopulationFailedKernelException>>
		 {
			  internal readonly IEntityUpdates[] Updates;

			  internal DataUpdates()
			  {
					this.Updates = new IEntityUpdates[0];
			  }

			  internal DataUpdates( IEntityUpdates[] updates )
			  {
					this.Updates = updates;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") void getsProcessedByStoreScanFrom(IndexStoreView mock)
			  internal virtual void GetsProcessedByStoreScanFrom( IndexStoreView mock )
			  {
					when( mock.VisitNodes( any( typeof( int[] ) ), any( typeof( System.Func<int, bool> ) ), any( typeof( Visitor ) ), Null, anyBoolean() ) ).thenAnswer(this);
			  }

			  public override StoreScan<IndexPopulationFailedKernelException> Answer( InvocationOnMock invocation )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.Visitor<EntityUpdates,org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException> visitor = visitor(invocation.getArgument(2));
					Visitor<EntityUpdates, IndexPopulationFailedKernelException> visitor = visitor( invocation.getArgument( 2 ) );
					return new StoreScanAnonymousInnerClass( this, visitor );
			  }

			  private class StoreScanAnonymousInnerClass : StoreScan<IndexPopulationFailedKernelException>
			  {
				  private readonly DataUpdates _outerInstance;

				  private Visitor<EntityUpdates, IndexPopulationFailedKernelException> _visitor;

				  public StoreScanAnonymousInnerClass( DataUpdates outerInstance, Visitor<EntityUpdates, IndexPopulationFailedKernelException> visitor )
				  {
					  this.outerInstance = outerInstance;
					  this._visitor = visitor;
				  }

				  private volatile bool stop;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException
				  public void run()
				  {
						foreach ( IEntityUpdates update in _outerInstance.updates )
						{
							 if ( stop )
							 {
								  return;
							 }
							 _visitor.visit( update );
						}
				  }

				  public void stop()
				  {
						stop = true;
				  }

				  public void acceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
				  {
						// no-op
				  }

				  public PopulationProgress Progress
				  {
					  get
					  {
							return PopulationProgress.single( 42, 100 );
					  }
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "rawtypes"}) private static org.Neo4Net.helpers.collection.Visitor<EntityUpdates, org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException> visitor(Object v)
			  internal static Visitor<EntityUpdates, IndexPopulationFailedKernelException> Visitor( object v )
			  {
					return ( Visitor ) v;
			  }

			  public override string ToString()
			  {
					return Arrays.ToString( Updates );
			  }
		 }

		 private class NameLookupAnswer : Answer<string>
		 {
			  internal readonly string Kind;

			  internal NameLookupAnswer( string kind )
			  {

					this.Kind = kind;
			  }

			  public override string Answer( InvocationOnMock invocation )
			  {
					int id = invocation.getArgument( 0 );
					return Kind + "[" + id + "]";
			  }
		 }

		 private class TrackingIndexAccessor : Neo4Net.Kernel.Api.Index.IndexAccessor_Adapter
		 {
			  internal readonly IndexUpdater Updater = mock( typeof( IndexUpdater ) );

			  public override void Drop()
			  {
					throw new System.NotSupportedException( "Not required" );
			  }

			  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
			  {
					return Updater;
			  }

			  public override IndexReader NewReader()
			  {
					throw new System.NotSupportedException( "Not required" );
			  }

			  public override BoundedIterable<long> NewAllEntriesReader()
			  {
					throw new System.NotSupportedException( "Not required" );
			  }

			  public override ResourceIterator<File> SnapshotFiles()
			  {
					throw new System.NotSupportedException( "Not required" );
			  }
		 }

		 private static StoreIndexDescriptor StoreIndex( long ruleId, int labelId, int propertyKeyId, IndexProviderDescriptor providerDescriptor )
		 {
			  return forSchema( forLabel( labelId, propertyKeyId ), providerDescriptor ).withId( ruleId );
		 }

		 private static StoreIndexDescriptor ConstraintIndexRule( long ruleId, int labelId, int propertyKeyId, IndexProviderDescriptor providerDescriptor )
		 {
			  return uniqueForSchema( forLabel( labelId, propertyKeyId ), providerDescriptor ).withId( ruleId );
		 }

		 private static StoreIndexDescriptor ConstraintIndexRule( long ruleId, int labelId, int propertyKeyId, IndexProviderDescriptor providerDescriptor, long constraintId )
		 {
			  return uniqueForSchema( forLabel( labelId, propertyKeyId ), providerDescriptor ).withIds( ruleId, constraintId );
		 }

		 private IndexingService CreateIndexServiceWithCustomIndexMap( IndexMapReference indexMapReference )
		 {
			  return new IndexingService( mock( typeof( IndexProxyCreator ) ), mock( typeof( IndexProviderMap ) ), indexMapReference, mock( typeof( IndexStoreView ) ), Collections.emptyList(), mock(typeof(IndexSamplingController)), mock(typeof(TokenNameLookup)), mock(typeof(JobScheduler)), mock(typeof(SchemaState)), mock(typeof(MultiPopulatorFactory)), _internalLogProvider, _userLogProvider, IndexingService.NoMonitor, false );
		 }

		 private static DependencyResolver BuildIndexDependencies( IndexProvider provider )
		 {
			  return BuildIndexDependencies( new IndexProvider[]{ provider } );
		 }

		 private static DependencyResolver BuildIndexDependencies( params IndexProvider[] providers )
		 {
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependencies( ( object[] ) providers );
			  return dependencies;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexProvider mockIndexProviderWithAccessor(org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor descriptor) throws java.io.IOException
		 private IndexProvider MockIndexProviderWithAccessor( IndexProviderDescriptor descriptor )
		 {
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  when( provider.ProviderDescriptor ).thenReturn( descriptor );
			  IndexAccessor indexAccessor = mock( typeof( IndexAccessor ) );
			  when( provider.GetOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( indexAccessor );
			  return provider;
		 }

		 private void OnBothLogProviders( System.Action<AssertableLogProvider> logProviderAction )
		 {
			  logProviderAction( _internalLogProvider );
			  logProviderAction( _userLogProvider );
		 }
	}

}