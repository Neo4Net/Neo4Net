﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
namespace Schema
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Org.Neo4j.Helpers.Collection;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Org.Neo4j.Kernel.api.SilentTokenNameLookup;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using IndexActivationFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SchemaState = Org.Neo4j.Kernel.Impl.Api.SchemaState;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexingServiceFactory = Org.Neo4j.Kernel.Impl.Api.index.IndexingServiceFactory;
	using Org.Neo4j.Kernel.Impl.Api.index;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using SchemaStorage = Org.Neo4j.Kernel.impl.store.SchemaStorage;
	using DirectIndexUpdates = Org.Neo4j.Kernel.impl.transaction.state.DirectIndexUpdates;
	using DynamicIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.DynamicIndexStoreView;
	using EntityIdIterator = Org.Neo4j.Kernel.impl.transaction.state.storeview.EntityIdIterator;
	using Org.Neo4j.Kernel.impl.transaction.state.storeview;
	using NeoStoreIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	//[NodePropertyUpdate[0, prop:0 add:Sweden, labelsBefore:[], labelsAfter:[0]]]
	//[NodePropertyUpdate[1, prop:0 add:USA, labelsBefore:[], labelsAfter:[0]]]
	//[NodePropertyUpdate[2, prop:0 add:red, labelsBefore:[], labelsAfter:[1]]]
	//[NodePropertyUpdate[3, prop:0 add:green, labelsBefore:[], labelsAfter:[0]]]
	//[NodePropertyUpdate[4, prop:0 add:Volvo, labelsBefore:[], labelsAfter:[2]]]
	//[NodePropertyUpdate[5, prop:0 add:Ford, labelsBefore:[], labelsAfter:[2]]]
	//TODO: check count store counts
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class MultiIndexPopulationConcurrentUpdatesIT
	public class MultiIndexPopulationConcurrentUpdatesIT
	{
		 private const string NAME_PROPERTY = "name";
		 private const string COUNTRY_LABEL = "country";
		 private const string COLOR_LABEL = "color";
		 private const string CAR_LABEL = "car";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule embeddedDatabase = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule EmbeddedDatabase = new EmbeddedDatabaseRule();
		 private StoreIndexDescriptor[] _rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex[] parameters()
		 public static GraphDatabaseSettings.SchemaIndex[] Parameters()
		 {
			  return GraphDatabaseSettings.SchemaIndex.values();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex;
		 public GraphDatabaseSettings.SchemaIndex SchemaIndex;

		 private IndexingService _indexService;
		 private int _propertyId;
		 private IDictionary<string, int> _labelsNameIdMap;
		 private IDictionary<int, string> _labelsIdNameMap;
		 private Node _country1;
		 private Node _country2;
		 private Node _color1;
		 private Node _color2;
		 private Node _car1;
		 private Node _car2;
		 private Node[] _otherNodes;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _indexService != null )
			  {
					_indexService.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  PrepareDb();

			  _labelsNameIdMap = LabelsNameIdMap;
			  _labelsIdNameMap = _labelsNameIdMap.SetOfKeyValuePairs().ToDictionary(DictionaryEntry.getValue, DictionaryEntry.getKey);
			  _propertyId = PropertyId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applyConcurrentDeletesToPopulatedIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplyConcurrentDeletesToPopulatedIndex()
		 {
			  IList<EntityUpdates> updates = new List<EntityUpdates>( 2 );
			  updates.Add( EntityUpdates.forEntity( _country1.Id, false ).withTokens( Id( COUNTRY_LABEL ) ).removed( _propertyId, Values.of( "Sweden" ) ).build() );
			  updates.Add( EntityUpdates.forEntity( _color2.Id, false ).withTokens( Id( COLOR_LABEL ) ).removed( _propertyId, Values.of( "green" ) ).build() );

			  LaunchCustomIndexPopulation( _labelsNameIdMap, _propertyId, new UpdateGenerator( this, updates ) );
			  WaitAndActivateIndexes( _labelsNameIdMap, _propertyId );

			  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
			  {
					int? countryLabelId = _labelsNameIdMap[COUNTRY_LABEL];
					int? colorLabelId = _labelsNameIdMap[COLOR_LABEL];
					using ( IndexReader indexReader = GetIndexReader( _propertyId, countryLabelId ) )
					{
						 assertEquals( "Should be removed by concurrent remove.", 0, indexReader.CountIndexedNodes( 0, new int[] { _propertyId }, Values.of( "Sweden" ) ) );
					}

					using ( IndexReader indexReader = GetIndexReader( _propertyId, colorLabelId ) )
					{
						 assertEquals( "Should be removed by concurrent remove.", 0, indexReader.CountIndexedNodes( 3, new int[] { _propertyId }, Values.of( "green" ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applyConcurrentAddsToPopulatedIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplyConcurrentAddsToPopulatedIndex()
		 {
			  IList<EntityUpdates> updates = new List<EntityUpdates>( 2 );
			  updates.Add( EntityUpdates.forEntity( _otherNodes[0].Id, false ).withTokens( Id( COUNTRY_LABEL ) ).added( _propertyId, Values.of( "Denmark" ) ).build() );
			  updates.Add( EntityUpdates.forEntity( _otherNodes[1].Id, false ).withTokens( Id( CAR_LABEL ) ).added( _propertyId, Values.of( "BMW" ) ).build() );

			  LaunchCustomIndexPopulation( _labelsNameIdMap, _propertyId, new UpdateGenerator( this, updates ) );
			  WaitAndActivateIndexes( _labelsNameIdMap, _propertyId );

			  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
			  {
					int? countryLabelId = _labelsNameIdMap[COUNTRY_LABEL];
					int? carLabelId = _labelsNameIdMap[CAR_LABEL];
					using ( IndexReader indexReader = GetIndexReader( _propertyId, countryLabelId ) )
					{
						 assertEquals( "Should be added by concurrent add.", 1, indexReader.CountIndexedNodes( _otherNodes[0].Id, new int[] { _propertyId }, Values.of( "Denmark" ) ) );
					}

					using ( IndexReader indexReader = GetIndexReader( _propertyId, carLabelId ) )
					{
						 assertEquals( "Should be added by concurrent add.", 1, indexReader.CountIndexedNodes( _otherNodes[1].Id, new int[] { _propertyId }, Values.of( "BMW" ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applyConcurrentChangesToPopulatedIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ApplyConcurrentChangesToPopulatedIndex()
		 {
			  IList<EntityUpdates> updates = new List<EntityUpdates>( 2 );
			  updates.Add( EntityUpdates.forEntity( _color2.Id, false ).withTokens( Id( COLOR_LABEL ) ).changed( _propertyId, Values.of( "green" ), Values.of( "pink" ) ).build() );
			  updates.Add( EntityUpdates.forEntity( _car2.Id, false ).withTokens( Id( CAR_LABEL ) ).changed( _propertyId, Values.of( "Ford" ), Values.of( "SAAB" ) ).build() );

			  LaunchCustomIndexPopulation( _labelsNameIdMap, _propertyId, new UpdateGenerator( this, updates ) );
			  WaitAndActivateIndexes( _labelsNameIdMap, _propertyId );

			  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
			  {
					int? colorLabelId = _labelsNameIdMap[COLOR_LABEL];
					int? carLabelId = _labelsNameIdMap[CAR_LABEL];
					using ( IndexReader indexReader = GetIndexReader( _propertyId, colorLabelId ) )
					{
						 assertEquals( format( "Should be deleted by concurrent change. Reader is: %s, ", indexReader ), 0, indexReader.CountIndexedNodes( _color2.Id, new int[] { _propertyId }, Values.of( "green" ) ) );
					}
					using ( IndexReader indexReader = GetIndexReader( _propertyId, colorLabelId ) )
					{
						 assertEquals( "Should be updated by concurrent change.", 1, indexReader.CountIndexedNodes( _color2.Id, new int[] { _propertyId }, Values.of( "pink" ) ) );
					}

					using ( IndexReader indexReader = GetIndexReader( _propertyId, carLabelId ) )
					{
						 assertEquals( "Should be added by concurrent change.", 1, indexReader.CountIndexedNodes( _car2.Id, new int[] { _propertyId }, Values.of( "SAAB" ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropOneOfTheIndexesWhilePopulationIsOngoingDoesInfluenceOtherPopulators() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DropOneOfTheIndexesWhilePopulationIsOngoingDoesInfluenceOtherPopulators()
		 {
			  LaunchCustomIndexPopulation( _labelsNameIdMap, _propertyId, new IndexDropAction( this, _labelsNameIdMap[COLOR_LABEL] ) );
			  _labelsNameIdMap.Remove( COLOR_LABEL );
			  WaitAndActivateIndexes( _labelsNameIdMap, _propertyId );

			  CheckIndexIsOnline( _labelsNameIdMap[CAR_LABEL] );
			  CheckIndexIsOnline( _labelsNameIdMap[COUNTRY_LABEL] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexDroppedDuringPopulationDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexDroppedDuringPopulationDoesNotExist()
		 {
			  int? labelToDropId = _labelsNameIdMap[COLOR_LABEL];
			  LaunchCustomIndexPopulation( _labelsNameIdMap, _propertyId, new IndexDropAction( this, labelToDropId.Value ) );
			  _labelsNameIdMap.Remove( COLOR_LABEL );
			  WaitAndActivateIndexes( _labelsNameIdMap, _propertyId );
			  try
			  {
					_indexService.getIndexProxy( SchemaDescriptorFactory.forLabel( labelToDropId.Value, _propertyId ) );
					fail( "Index does not exist, we should fail to find it." );
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkIndexIsOnline(int labelId) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void CheckIndexIsOnline( int labelId )
		 {
			  IndexProxy indexProxy = _indexService.getIndexProxy( SchemaDescriptorFactory.forLabel( labelId, _propertyId ) );
			  assertSame( indexProxy.State, InternalIndexState.ONLINE );
		 }

		 private long[] Id( string label )
		 {
			  return new long[]{ _labelsNameIdMap[label] };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.storageengine.api.schema.IndexReader getIndexReader(int propertyId, System.Nullable<int> countryLabelId) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private IndexReader GetIndexReader( int propertyId, int? countryLabelId )
		 {
			  return _indexService.getIndexProxy( SchemaDescriptorFactory.forLabel( countryLabelId.Value, propertyId ) ).newReader();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void launchCustomIndexPopulation(java.util.Map<String,int> labelNameIdMap, int propertyId, Runnable customAction) throws Exception
		 private void LaunchCustomIndexPopulation( IDictionary<string, int> labelNameIdMap, int propertyId, ThreadStart customAction )
		 {
			  NeoStores neoStores = NeoStores;
			  LabelScanStore labelScanStore = LabelScanStore;
			  ThreadToStatementContextBridge transactionStatementContextBridge = TransactionStatementContextBridge;

			  using ( Transaction transaction = EmbeddedDatabase.beginTx(), KernelTransaction ktx = transactionStatementContextBridge.GetKernelTransactionBoundToThisThread(true) )
			  {
					DynamicIndexStoreView storeView = DynamicIndexStoreViewWrapper( customAction, neoStores, labelScanStore );

					IndexProviderMap providerMap = IndexProviderMap;
					JobScheduler scheduler = JobScheduler;
					TokenNameLookup tokenNameLookup = new SilentTokenNameLookup( ktx.TokenRead() );

					NullLogProvider nullLogProvider = NullLogProvider.Instance;
					_indexService = IndexingServiceFactory.createIndexingService( Config.defaults(), scheduler, providerMap, storeView, tokenNameLookup, GetIndexRules(neoStores), nullLogProvider, nullLogProvider, IndexingService.NO_MONITOR, SchemaState, false );
					_indexService.start();

					_rules = CreateIndexRules( labelNameIdMap, propertyId );

					_indexService.createIndexes( _rules );
					transaction.Success();
			  }
		 }

		 private DynamicIndexStoreView DynamicIndexStoreViewWrapper( ThreadStart customAction, NeoStores neoStores, LabelScanStore labelScanStore )
		 {
			  LockService locks = LockService.NO_LOCK_SERVICE;
			  NeoStoreIndexStoreView neoStoreIndexStoreView = new NeoStoreIndexStoreView( locks, neoStores );
			  return new DynamicIndexStoreViewWrapper( this, neoStoreIndexStoreView, labelScanStore, locks, neoStores, customAction );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitAndActivateIndexes(java.util.Map<String,int> labelsIds, int propertyId) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException, org.neo4j.kernel.api.exceptions.index.IndexActivationFailedKernelException
		 private void WaitAndActivateIndexes( IDictionary<string, int> labelsIds, int propertyId )
		 {
			  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
			  {
					foreach ( int labelId in labelsIds.Values )
					{
						 WaitIndexOnline( _indexService, propertyId, labelId );
					}
			  }
		 }

		 private int PropertyId
		 {
			 get
			 {
				  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
				  {
						return GetPropertyIdByName( NAME_PROPERTY );
				  }
			 }
		 }

		 private IDictionary<string, int> LabelsNameIdMap
		 {
			 get
			 {
				  using ( Transaction ignored = EmbeddedDatabase.beginTx() )
				  {
						return GetLabelIdsByName( COUNTRY_LABEL, COLOR_LABEL, CAR_LABEL );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitIndexOnline(org.neo4j.kernel.impl.api.index.IndexingService indexService, int propertyId, int labelId) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException, org.neo4j.kernel.api.exceptions.index.IndexActivationFailedKernelException
		 private void WaitIndexOnline( IndexingService indexService, int propertyId, int labelId )
		 {
			  IndexProxy indexProxy = indexService.getIndexProxy( SchemaDescriptorFactory.forLabel( labelId, propertyId ) );
			  indexProxy.AwaitStoreScanCompleted( 0, TimeUnit.MILLISECONDS );
			  while ( indexProxy.State != InternalIndexState.ONLINE )
			  {
					Thread.Sleep( 10 );
			  }
			  indexProxy.Activate();
		 }

		 private StoreIndexDescriptor[] CreateIndexRules( IDictionary<string, int> labelNameIdMap, int propertyId )
		 {
			  IndexProvider lookup = IndexProviderMap.lookup( SchemaIndex.providerName() );
			  IndexProviderDescriptor providerDescriptor = lookup.ProviderDescriptor;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return labelNameIdMap.Values.Select( index => IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( index, propertyId ), providerDescriptor ).withId( index ) ).ToArray( StoreIndexDescriptor[]::new );
		 }

		 private IList<SchemaRule> GetIndexRules( NeoStores neoStores )
		 {
			  return Iterators.asList( ( new SchemaStorage( neoStores.SchemaStore ) ).loadAllSchemaRules() );
		 }

		 private IDictionary<string, int> GetLabelIdsByName( params string[] names )
		 {
			  ThreadToStatementContextBridge transactionStatementContextBridge = TransactionStatementContextBridge;
			  IDictionary<string, int> labelNameIdMap = new Dictionary<string, int>();
			  KernelTransaction ktx = transactionStatementContextBridge.GetKernelTransactionBoundToThisThread( true );
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					TokenRead tokenRead = ktx.TokenRead();
					foreach ( string name in names )
					{
						 labelNameIdMap[name] = tokenRead.NodeLabel( name );
					}
			  }
			  return labelNameIdMap;
		 }

		 private int GetPropertyIdByName( string name )
		 {
			  ThreadToStatementContextBridge transactionStatementContextBridge = TransactionStatementContextBridge;
			  KernelTransaction ktx = transactionStatementContextBridge.GetKernelTransactionBoundToThisThread( true );
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					return ktx.TokenRead().propertyKey(name);
			  }
		 }

		 private void PrepareDb()
		 {
			  Label countryLabel = Label.label( COUNTRY_LABEL );
			  Label color = Label.label( COLOR_LABEL );
			  Label car = Label.label( CAR_LABEL );
			  using ( Transaction transaction = EmbeddedDatabase.beginTx() )
			  {
					_country1 = CreateNamedLabeledNode( countryLabel, "Sweden" );
					_country2 = CreateNamedLabeledNode( countryLabel, "USA" );

					_color1 = CreateNamedLabeledNode( color, "red" );
					_color2 = CreateNamedLabeledNode( color, "green" );

					_car1 = CreateNamedLabeledNode( car, "Volvo" );
					_car2 = CreateNamedLabeledNode( car, "Ford" );

					_otherNodes = new Node[250];
					for ( int i = 0; i < 250; i++ )
					{
						 _otherNodes[i] = EmbeddedDatabase.createNode();
					}

					transaction.Success();
			  }
		 }

		 private Node CreateNamedLabeledNode( Label label, string name )
		 {
			  Node node = EmbeddedDatabase.createNode( label );
			  node.SetProperty( NAME_PROPERTY, name );
			  return node;
		 }

		 private LabelScanStore LabelScanStore
		 {
			 get
			 {
				  return EmbeddedDatabase.resolveDependency( typeof( LabelScanStore ) );
			 }
		 }

		 private NeoStores NeoStores
		 {
			 get
			 {
				  RecordStorageEngine recordStorageEngine = EmbeddedDatabase.resolveDependency( typeof( RecordStorageEngine ) );
				  return recordStorageEngine.TestAccessNeoStores();
			 }
		 }

		 private SchemaState SchemaState
		 {
			 get
			 {
				  return EmbeddedDatabase.resolveDependency( typeof( SchemaState ) );
			 }
		 }

		 private ThreadToStatementContextBridge TransactionStatementContextBridge
		 {
			 get
			 {
				  return EmbeddedDatabase.resolveDependency( typeof( ThreadToStatementContextBridge ) );
			 }
		 }

		 private IndexProviderMap IndexProviderMap
		 {
			 get
			 {
				  return EmbeddedDatabase.resolveDependency( typeof( IndexProviderMap ) );
			 }
		 }

		 private JobScheduler JobScheduler
		 {
			 get
			 {
				  return EmbeddedDatabase.resolveDependency( typeof( JobScheduler ) );
			 }
		 }

		 private class DynamicIndexStoreViewWrapper : DynamicIndexStoreView
		 {
			 private readonly MultiIndexPopulationConcurrentUpdatesIT _outerInstance;

			  internal readonly ThreadStart CustomAction;
			  internal readonly NeoStores NeoStores;

			  internal DynamicIndexStoreViewWrapper( MultiIndexPopulationConcurrentUpdatesIT outerInstance, NeoStoreIndexStoreView neoStoreIndexStoreView, LabelScanStore labelScanStore, LockService locks, NeoStores neoStores, ThreadStart customAction ) : base( neoStoreIndexStoreView, labelScanStore, locks, neoStores, NullLogProvider.Instance )
			  {
				  this._outerInstance = outerInstance;
					this.CustomAction = customAction;
					this.NeoStores = neoStores;
			  }

			  public override StoreScan<FAILURE> VisitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
			  {
					StoreScan<FAILURE> storeScan = base.VisitNodes( labelIds, propertyKeyIdFilter, propertyUpdatesVisitor, labelUpdateVisitor, forceStoreScan );
					return new LabelScanViewNodeStoreWrapper<FAILURE>( _outerInstance, new RecordStorageReader( NeoStores ), Locks, _outerInstance.LabelScanStore, element => false, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter, ( LabelScanViewNodeStoreScan<FAILURE> ) storeScan, CustomAction );
			  }
		 }

		 private class LabelScanViewNodeStoreWrapper<FAILURE> : LabelScanViewNodeStoreScan<FAILURE> where FAILURE : Exception
		 {
			 private readonly MultiIndexPopulationConcurrentUpdatesIT _outerInstance;

			  internal readonly LabelScanViewNodeStoreScan<FAILURE> Delegate;
			  internal readonly ThreadStart CustomAction;

			  internal LabelScanViewNodeStoreWrapper( MultiIndexPopulationConcurrentUpdatesIT outerInstance, StorageReader storageReader, LockService locks, LabelScanStore labelScanStore, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, LabelScanViewNodeStoreScan<FAILURE> @delegate, ThreadStart customAction ) : base( storageReader, locks, labelScanStore, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
					this.CustomAction = customAction;
			  }

			  public override EntityIdIterator EntityIdIterator
			  {
				  get
				  {
						EntityIdIterator originalIterator = Delegate.EntityIdIterator;
						return new DelegatingEntityIdIterator( _outerInstance, originalIterator, CustomAction );
				  }
			  }
		 }

		 private class DelegatingEntityIdIterator : EntityIdIterator
		 {
			 private readonly MultiIndexPopulationConcurrentUpdatesIT _outerInstance;

			  internal readonly ThreadStart CustomAction;
			  internal readonly EntityIdIterator Delegate;

			  internal DelegatingEntityIdIterator( MultiIndexPopulationConcurrentUpdatesIT outerInstance, EntityIdIterator @delegate, ThreadStart customAction )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
					this.CustomAction = customAction;
			  }

			  public override bool HasNext()
			  {
					return Delegate.hasNext();
			  }

			  public override long Next()
			  {
					long value = Delegate.next();
					if ( !HasNext() )
					{
						 CustomAction.run();
					}
					return value;
			  }

			  public override void Close()
			  {
					Delegate.close();
			  }

			  public override void InvalidateCache()
			  {
					Delegate.invalidateCache();
			  }
		 }

		 private class UpdateGenerator : ThreadStart
		 {
			 private readonly MultiIndexPopulationConcurrentUpdatesIT _outerInstance;


			  internal IEnumerable<EntityUpdates> Updates;

			  internal UpdateGenerator( MultiIndexPopulationConcurrentUpdatesIT outerInstance, IEnumerable<EntityUpdates> updates )
			  {
				  this._outerInstance = outerInstance;
					this.Updates = updates;
			  }

			  public override void Run()
			  {
					foreach ( EntityUpdates update in Updates )
					{
							  using ( Transaction transaction = outerInstance.EmbeddedDatabase.beginTx() )
							  {
									Node node = outerInstance.EmbeddedDatabase.getNodeById( update.EntityId );
									foreach ( int labelId in outerInstance.labelsNameIdMap.Values )
									{
										 LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( labelId, outerInstance.propertyId );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> indexUpdate : update.forIndexKeys(java.util.Collections.singleton(schema)))
										 foreach ( IndexEntryUpdate<object> indexUpdate in update.ForIndexKeys( Collections.singleton( schema ) ) )
										 {
											  switch ( indexUpdate.UpdateMode() )
											  {
											  case CHANGED:
											  case ADDED:
													node.AddLabel( Label.label( outerInstance.labelsIdNameMap[Schema.LabelId] ) );
													node.SetProperty( NAME_PROPERTY, indexUpdate.Values()[0].asObject() );
													break;
											  case REMOVED:
													node.AddLabel( Label.label( outerInstance.labelsIdNameMap[Schema.LabelId] ) );
													node.Delete();
													break;
											  default:
													throw new System.ArgumentException( indexUpdate.UpdateMode().name() );
											  }
										 }
									}
									transaction.Success();
							  }
					}
						 try
						 {
							  foreach ( EntityUpdates update in Updates )
							  {
									IEnumerable<IndexEntryUpdate<SchemaDescriptor>> entryUpdates = outerInstance.indexService.ConvertToIndexUpdates( update, EntityType.NODE );
									DirectIndexUpdates directIndexUpdates = new DirectIndexUpdates( entryUpdates );
									outerInstance.indexService.Apply( directIndexUpdates );
							  }
						 }
						 catch ( Exception e ) when ( e is UncheckedIOException || e is IndexEntryConflictException )
						 {
							  throw new Exception( e );
						 }
			  }
		 }

		 private class IndexDropAction : ThreadStart
		 {
			 private readonly MultiIndexPopulationConcurrentUpdatesIT _outerInstance;

			  internal int LabelIdToDropIndexFor;

			  internal IndexDropAction( MultiIndexPopulationConcurrentUpdatesIT outerInstance, int labelIdToDropIndexFor )
			  {
				  this._outerInstance = outerInstance;
					this.LabelIdToDropIndexFor = labelIdToDropIndexFor;
			  }

			  public override void Run()
			  {
					Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( LabelIdToDropIndexFor, outerInstance.propertyId );
					StoreIndexDescriptor rule = FindRuleForLabel( descriptor );
					outerInstance.indexService.DropIndex( rule );
			  }

			  internal virtual StoreIndexDescriptor FindRuleForLabel( LabelSchemaDescriptor schemaDescriptor )
			  {
					foreach ( StoreIndexDescriptor rule in outerInstance.rules )
					{
						 if ( rule.Schema().Equals(schemaDescriptor) )
						 {
							  return rule;
						 }
					}
					return null;
			  }
		 }
	}

}