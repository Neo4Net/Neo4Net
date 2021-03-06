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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using DefaultIndexProviderMap = Org.Neo4j.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogMatcherBuilder = Org.Neo4j.Logging.AssertableLogProvider.LogMatcherBuilder;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;
	using Org.Neo4j.Test;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.IndexingService.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class IndexPopulationJobTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

		 private GraphDatabaseAPI _db;

		 private readonly Label _first = Label.label( "FIRST" );
		 private readonly Label _second = Label.label( "SECOND" );
		 private readonly RelationshipType _likes = RelationshipType.withName( "likes" );
		 private readonly RelationshipType _knows = RelationshipType.withName( "knows" );
		 private readonly string _name = "name";
		 private readonly string _age = "age";

		 private Kernel _kernel;
		 private IndexStoreView _indexStoreView;
		 private DatabaseSchemaState _stateHolder;
		 private int _labelId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
			  _kernel = _db.DependencyResolver.resolveDependency( typeof( Kernel ) );
			  _stateHolder = new DatabaseSchemaState( NullLogProvider.Instance );
			  _indexStoreView = _indexStoreView();

			  using ( Transaction tx = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					_labelId = tx.TokenWrite().labelGetOrCreateForName(_first.name());
					tx.TokenWrite().labelGetOrCreateForName(_second.name());
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndexWithOneNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateIndexWithOneNode()
		 {
			  // GIVEN
			  string value = "Taylor";
			  long nodeId = CreateNode( map( _name, value ), _first );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( 0, 0 );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.NODE, IndexDescriptorFactory.forSchema(descriptor) );

			  // WHEN
			  job.Run();

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update = org.neo4j.kernel.api.index.IndexEntryUpdate.add(nodeId, descriptor, org.neo4j.values.storable.Values.of(value));
			  IndexEntryUpdate<object> update = IndexEntryUpdate.add( nodeId, descriptor, Values.of( value ) );

			  verify( populator ).create();
			  verify( populator ).includeSample( update );
			  verify( populator, times( 2 ) ).add( any( typeof( System.Collections.ICollection ) ) );
			  verify( populator ).sampleResult();
			  verify( populator ).close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndexWithOneRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateIndexWithOneRelationship()
		 {
			  // GIVEN
			  string value = "Taylor";
			  long nodeId = CreateNode( map( _name, value ), _first );
			  long relationship = CreateRelationship( map( _name, _age ), _likes, nodeId, nodeId );
			  IndexDescriptor descriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forRelType( 0, 0 ) );
			  IndexPopulator populator = spy( IndexPopulator( descriptor ) );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.RELATIONSHIP, descriptor );

			  // WHEN
			  job.Run();

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update = org.neo4j.kernel.api.index.IndexEntryUpdate.add(relationship, descriptor, org.neo4j.values.storable.Values.of(age));
			  IndexEntryUpdate<object> update = IndexEntryUpdate.add( relationship, descriptor, Values.of( _age ) );

			  verify( populator ).create();
			  verify( populator ).includeSample( update );
			  verify( populator, times( 2 ) ).add( any( typeof( System.Collections.ICollection ) ) );
			  verify( populator ).sampleResult();
			  verify( populator ).close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushSchemaStateAfterPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushSchemaStateAfterPopulation()
		 {
			  // GIVEN
			  string value = "Taylor";
			  CreateNode( map( _name, value ), _first );
			  _stateHolder.put( "key", "original_value" );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.NODE, IndexDescriptor(_first, _name, false) );

			  // WHEN
			  job.Run();

			  // THEN
			  string result = _stateHolder.get( "key" );
			  assertNull( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndexWithASmallDataset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateIndexWithASmallDataset()
		 {
			  // GIVEN
			  string value = "Mattias";
			  long node1 = CreateNode( map( _name, value ), _first );
			  CreateNode( map( _name, value ), _second );
			  CreateNode( map( _age, 31 ), _first );
			  long node4 = CreateNode( map( _age, 35, _name, value ), _first );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( 0, 0 );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.NODE, IndexDescriptorFactory.forSchema(descriptor) );

			  // WHEN
			  job.Run();

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update1 = add(node1, descriptor, org.neo4j.values.storable.Values.of(value));
			  IndexEntryUpdate<object> update1 = add( node1, descriptor, Values.of( value ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update2 = add(node4, descriptor, org.neo4j.values.storable.Values.of(value));
			  IndexEntryUpdate<object> update2 = add( node4, descriptor, Values.of( value ) );

			  verify( populator ).create();
			  verify( populator ).includeSample( update1 );
			  verify( populator ).includeSample( update2 );
			  verify( populator, times( 2 ) ).add( anyCollection() );
			  verify( populator ).sampleResult();
			  verify( populator ).close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateRelatonshipIndexWithASmallDataset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateRelatonshipIndexWithASmallDataset()
		 {
			  // GIVEN
			  string value = "Philip J.Fry";
			  long node1 = CreateNode( map( _name, value ), _first );
			  long node2 = CreateNode( map( _name, value ), _second );
			  long node3 = CreateNode( map( _age, 31 ), _first );
			  long node4 = CreateNode( map( _age, 35, _name, value ), _first );

			  long rel1 = CreateRelationship( map( _name, value ), _likes, node1, node3 );
			  CreateRelationship( map( _name, value ), _knows, node3, node1 );
			  CreateRelationship( map( _age, 31 ), _likes, node2, node1 );
			  long rel4 = CreateRelationship( map( _age, 35, _name, value ), _likes, node4, node4 );

			  IndexDescriptor descriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forRelType( 0, 0 ) );
			  IndexPopulator populator = spy( IndexPopulator( descriptor ) );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.RELATIONSHIP, descriptor );

			  // WHEN
			  job.Run();

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update1 = add(rel1, descriptor, org.neo4j.values.storable.Values.of(value));
			  IndexEntryUpdate<object> update1 = add( rel1, descriptor, Values.of( value ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update2 = add(rel4, descriptor, org.neo4j.values.storable.Values.of(value));
			  IndexEntryUpdate<object> update2 = add( rel4, descriptor, Values.of( value ) );

			  verify( populator ).create();
			  verify( populator ).includeSample( update1 );
			  verify( populator ).includeSample( update2 );
			  verify( populator, times( 2 ) ).add( anyCollection() );
			  verify( populator ).sampleResult();
			  verify( populator ).close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexConcurrentUpdatesWhilePopulating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexConcurrentUpdatesWhilePopulating()
		 {
			  // GIVEN
			  object value1 = "Mattias";
			  object value2 = "Jacob";
			  object value3 = "Stefan";
			  object changedValue = "changed";
			  long node1 = CreateNode( map( _name, value1 ), _first );
			  long node2 = CreateNode( map( _name, value2 ), _first );
			  long node3 = CreateNode( map( _name, value3 ), _first );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnnecessaryLocalVariable") long changeNode = node1;
			  long changeNode = node1;
			  int propertyKeyId = GetPropertyKeyForName( _name );
			  NodeChangingWriter populator = new NodeChangingWriter( this, changeNode, propertyKeyId, value1, changedValue, _labelId );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.NODE, IndexDescriptor(_first, _name, false) );
			  populator.Job = job;

			  // WHEN
			  job.Run();

			  // THEN
			  ISet<Pair<long, object>> expected = asSet( Pair.of( node1, value1 ), Pair.of( node2, value2 ), Pair.of( node3, value3 ), Pair.of( node1, changedValue ) );
			  assertEquals( expected, populator.Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveViaConcurrentIndexUpdatesWhilePopulating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveViaConcurrentIndexUpdatesWhilePopulating()
		 {
			  // GIVEN
			  string value1 = "Mattias";
			  string value2 = "Jacob";
			  string value3 = "Stefan";
			  long node1 = CreateNode( map( _name, value1 ), _first );
			  long node2 = CreateNode( map( _name, value2 ), _first );
			  long node3 = CreateNode( map( _name, value3 ), _first );
			  int propertyKeyId = GetPropertyKeyForName( _name );
			  NodeDeletingWriter populator = new NodeDeletingWriter( this, node2, propertyKeyId, value2, _labelId );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, new FlippableIndexProxy(), EntityType.NODE, IndexDescriptor(_first, _name, false) );
			  populator.Job = job;

			  // WHEN
			  job.Run();

			  // THEN
			  IDictionary<long, object> expectedAdded = genericMap( node1, value1, node2, value2, node3, value3 );
			  assertEquals( expectedAdded, populator.Added );
			  IDictionary<long, object> expectedRemoved = genericMap( node2, value2 );
			  assertEquals( expectedRemoved, populator.Removed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTransitionToFailedStateIfPopulationJobCrashes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTransitionToFailedStateIfPopulationJobCrashes()
		 {
			  // GIVEN
			  IndexPopulator failingPopulator = mock( typeof( IndexPopulator ) );
			  doThrow( new Exception( "BORK BORK" ) ).when( failingPopulator ).add( any( typeof( System.Collections.ICollection ) ) );

			  FlippableIndexProxy index = new FlippableIndexProxy();

			  CreateNode( map( _name, "Taylor" ), _first );
			  IndexPopulationJob job = NewIndexPopulationJob( failingPopulator, index, EntityType.NODE, IndexDescriptor( _first, _name, false ) );

			  // WHEN
			  job.Run();

			  // THEN
			  assertThat( index.State, equalTo( InternalIndexState.FAILED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCancelPopulationJob() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCancelPopulationJob()
		 {
			  // GIVEN
			  CreateNode( map( _name, "Mattias" ), _first );
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  FlippableIndexProxy index = mock( typeof( FlippableIndexProxy ) );
			  IndexStoreView storeView = mock( typeof( IndexStoreView ) );
			  ControlledStoreScan storeScan = new ControlledStoreScan();
			  when( storeView.VisitNodes( any( typeof( int[] ) ), any( typeof( System.Func<int, bool> ) ), ArgumentMatchers.any(), ArgumentMatchers.any<Visitor<NodeLabelUpdate, Exception>>(), anyBoolean() ) ).thenReturn(storeScan);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexPopulationJob job = newIndexPopulationJob(populator, index, storeView, org.neo4j.logging.NullLogProvider.getInstance(), org.neo4j.storageengine.api.EntityType.NODE, indexDescriptor(FIRST, name, false));
			  IndexPopulationJob job = NewIndexPopulationJob( populator, index, storeView, NullLogProvider.Instance, EntityType.NODE, IndexDescriptor( _first, _name, false ) );

			  OtherThreadExecutor<Void> populationJobRunner = Cleanup.add( new OtherThreadExecutor<Void>( "Population job test runner", null ) );
			  Future<Void> runFuture = populationJobRunner.ExecuteDontWait(state =>
			  {
						  job.Run();
						  return null;
			  });

			  storeScan.Latch.waitForAllToStart();
			  job.Cancel().get();
			  storeScan.Latch.waitForAllToFinish();

			  // WHEN
			  runFuture.get();

			  // THEN
			  verify( populator, times( 1 ) ).close( false );
			  verify( index, never() ).flip(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogJobProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogJobProgress()
		 {
			  // Given
			  CreateNode( map( _name, "irrelephant" ), _first );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FlippableIndexProxy index = mock( typeof( FlippableIndexProxy ) );
			  when( index.State ).thenReturn( InternalIndexState.ONLINE );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  try
			  {
					IndexPopulationJob job = NewIndexPopulationJob( populator, index, _indexStoreView, logProvider, EntityType.NODE, IndexDescriptor( _first, _name, false ) );

					// When
					job.Run();

					// Then
					AssertableLogProvider.LogMatcherBuilder match = inLog( typeof( IndexPopulationJob ) );
					logProvider.AssertExactly( match.info( "Index population started: [%s]", ":FIRST(name)" ), match.info( "Index creation finished. Index [%s] is %s.", ":FIRST(name)", "ONLINE" ), match.info( containsString( "TIME/PHASE Final: SCAN[" ) ) );
			  }
			  finally
			  {
					populator.Close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logConstraintJobProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogConstraintJobProgress()
		 {
			  // Given
			  CreateNode( map( _name, "irrelephant" ), _first );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FlippableIndexProxy index = mock( typeof( FlippableIndexProxy ) );
			  when( index.State ).thenReturn( InternalIndexState.POPULATING );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  try
			  {
					IndexPopulationJob job = NewIndexPopulationJob( populator, index, _indexStoreView, logProvider, EntityType.NODE, IndexDescriptor( _first, _name, true ) );

					// When
					job.Run();

					// Then
					AssertableLogProvider.LogMatcherBuilder match = inLog( typeof( IndexPopulationJob ) );
					logProvider.AssertExactly( match.info( "Index population started: [%s]", ":FIRST(name)" ), match.info( "Index created. Starting data checks. Index [%s] is %s.", ":FIRST(name)", "POPULATING" ), match.info( containsString( "TIME/PHASE Final: SCAN[" ) ) );
			  }
			  finally
			  {
					populator.Close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogJobFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogJobFailure()
		 {
			  // Given
			  CreateNode( map( _name, "irrelephant" ), _first );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FlippableIndexProxy index = mock( typeof( FlippableIndexProxy ) );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, index, _indexStoreView, logProvider, EntityType.NODE, IndexDescriptor( _first, _name, false ) );

			  Exception failure = new System.InvalidOperationException( "not successful" );
			  doThrow( failure ).when( populator ).create();

			  // When
			  job.Run();

			  // Then
			  AssertableLogProvider.LogMatcherBuilder match = inLog( typeof( IndexPopulationJob ) );
			  logProvider.AssertAtLeastOnce( match.error( @is( "Failed to populate index: [:FIRST(name)]" ), sameInstance( failure ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlipToFailedUsingFailedIndexProxyFactory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlipToFailedUsingFailedIndexProxyFactory()
		 {
			  // Given
			  FailedIndexProxyFactory failureDelegateFactory = mock( typeof( FailedIndexProxyFactory ) );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  IndexPopulationJob job = NewIndexPopulationJob( failureDelegateFactory, populator, new FlippableIndexProxy(), _indexStoreView, NullLogProvider.Instance, EntityType.NODE, IndexDescriptor(_first, _name, false) );

			  System.InvalidOperationException failure = new System.InvalidOperationException( "not successful" );
			  doThrow( failure ).when( populator ).close( true );

			  // When
			  job.Run();

			  // Then
			  verify( failureDelegateFactory ).create( any( typeof( Exception ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAndFailOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAndFailOnFailure()
		 {
			  CreateNode( map( _name, "irrelephant" ), _first );
			  LogProvider logProvider = NullLogProvider.Instance;
			  FlippableIndexProxy index = mock( typeof( FlippableIndexProxy ) );
			  IndexPopulator populator = spy( IndexPopulator( false ) );
			  IndexPopulationJob job = NewIndexPopulationJob( populator, index, _indexStoreView, logProvider, EntityType.NODE, IndexDescriptor( _first, _name, false ) );

			  string failureMessage = "not successful";
			  System.InvalidOperationException failure = new System.InvalidOperationException( failureMessage );
			  doThrow( failure ).when( populator ).create();

			  // When
			  job.Run();

			  // Then
			  verify( populator ).markAsFailed( contains( failureMessage ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseMultiPopulatorOnSuccessfulPopulation()
		 public virtual void ShouldCloseMultiPopulatorOnSuccessfulPopulation()
		 {
			  // given
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  TrackingMultipleIndexPopulator populator = new TrackingMultipleIndexPopulator( IndexStoreView_Fields.Empty, logProvider, EntityType.NODE, new DatabaseSchemaState( logProvider ) );
			  IndexPopulationJob populationJob = new IndexPopulationJob( populator, NO_MONITOR, false );

			  // when
			  populationJob.Run();

			  // then
			  assertTrue( populator.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseMultiPopulatorOnFailedPopulation()
		 public virtual void ShouldCloseMultiPopulatorOnFailedPopulation()
		 {
			  // given
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  IndexStoreView failingStoreView = new IndexStoreView_AdaptorAnonymousInnerClass( this );
			  TrackingMultipleIndexPopulator populator = new TrackingMultipleIndexPopulator( failingStoreView, logProvider, EntityType.NODE, new DatabaseSchemaState( logProvider ) );
			  IndexPopulationJob populationJob = new IndexPopulationJob( populator, NO_MONITOR, false );

			  // when
			  populationJob.Run();

			  // then
			  assertTrue( populator.Closed );
		 }

		 private class IndexStoreView_AdaptorAnonymousInnerClass : IndexStoreView_Adaptor
		 {
			 private readonly IndexPopulationJobTest _outerInstance;

			 public IndexStoreView_AdaptorAnonymousInnerClass( IndexPopulationJobTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override StoreScan<FAILURE> visitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
			 {
				  return new StoreScanAnonymousInnerClass( this );
			 }

			 private class StoreScanAnonymousInnerClass : StoreScan<FAILURE>
			 {
				 private readonly IndexStoreView_AdaptorAnonymousInnerClass _outerInstance;

				 public StoreScanAnonymousInnerClass( IndexStoreView_AdaptorAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public void run()
				 {
					  throw new Exception( "Just failing" );
				 }

				 public void stop()
				 {
				 }

				 public void acceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
				 {
				 }

				 public PopulationProgress Progress
				 {
					 get
					 {
						  return null;
					 }
				 }
			 }
		 }

		 private class ControlledStoreScan : StoreScan<Exception>
		 {
			  internal readonly DoubleLatch Latch = new DoubleLatch();

			  public override void Run()
			  {
					Latch.startAndWaitForAllToStartAndFinish();
			  }

			  public override void Stop()
			  {
					Latch.finish();
			  }

			  public override void AcceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
			  {
			  }

			  public virtual PopulationProgress Progress
			  {
				  get
				  {
						return PopulationProgress.single( 42, 100 );
				  }
			  }
		 }

		 private class NodeChangingWriter : Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			 private readonly IndexPopulationJobTest _outerInstance;

			  internal readonly ISet<Pair<long, object>> Added = new HashSet<Pair<long, object>>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IndexPopulationJob JobConflict;
			  internal readonly long NodeToChange;
			  internal readonly Value NewValue;
			  internal readonly Value PreviousValue;
			  internal readonly LabelSchemaDescriptor Index;

			  internal NodeChangingWriter( IndexPopulationJobTest outerInstance, long nodeToChange, int propertyKeyId, object previousValue, object newValue, int label )
			  {
				  this._outerInstance = outerInstance;
					this.NodeToChange = nodeToChange;
					this.PreviousValue = Values.of( previousValue );
					this.NewValue = Values.of( newValue );
					this.Index = SchemaDescriptorFactory.forLabel( label, propertyKeyId );
			  }

			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Org.Neo4j.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 Add( update );
					}
			  }

			  internal virtual void Add<T1>( IndexEntryUpdate<T1> update )
			  {
					if ( update.EntityId == 2 )
					{
						 JobConflict.update( IndexEntryUpdate.change( NodeToChange, Index, PreviousValue, NewValue ) );
					}
					Added.Add( Pair.of( update.EntityId, update.Values()[0].asObjectCopy() ) );
			  }

			  public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
			  {
					return new IndexUpdaterAnonymousInnerClass( this );
			  }

			  private class IndexUpdaterAnonymousInnerClass : IndexUpdater
			  {
				  private readonly NodeChangingWriter _outerInstance;

				  public IndexUpdaterAnonymousInnerClass( NodeChangingWriter outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public void process<T1>( IndexEntryUpdate<T1> update )
				  {
						switch ( update.UpdateMode() )
						{
							 case ADDED:
							 case CHANGED:
								  _outerInstance.added.Add( Pair.of( update.EntityId, update.Values()[0].asObjectCopy() ) );
								  break;
							 default:
								  throw new System.ArgumentException( update.UpdateMode().name() );
						}
				  }

				  public void close()
				  {
				  }
			  }

			  public virtual IndexPopulationJob Job
			  {
				  set
				  {
						this.JobConflict = value;
				  }
			  }
		 }

		 private class NodeDeletingWriter : Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			 private readonly IndexPopulationJobTest _outerInstance;

			  internal readonly IDictionary<long, object> Added = new Dictionary<long, object>();
			  internal readonly IDictionary<long, object> Removed = new Dictionary<long, object>();
			  internal readonly long NodeToDelete;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IndexPopulationJob JobConflict;
			  internal readonly Value ValueToDelete;
			  internal readonly LabelSchemaDescriptor Index;

			  internal NodeDeletingWriter( IndexPopulationJobTest outerInstance, long nodeToDelete, int propertyKeyId, object valueToDelete, int label )
			  {
				  this._outerInstance = outerInstance;
					this.NodeToDelete = nodeToDelete;
					this.ValueToDelete = Values.of( valueToDelete );
					this.Index = SchemaDescriptorFactory.forLabel( label, propertyKeyId );
			  }

			  public virtual IndexPopulationJob Job
			  {
				  set
				  {
						this.JobConflict = value;
				  }
			  }

			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Org.Neo4j.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 Add( update );
					}
			  }

			  internal virtual void Add<T1>( IndexEntryUpdate<T1> update )
			  {
					if ( update.EntityId == 2 )
					{
						 JobConflict.update( IndexEntryUpdate.remove( NodeToDelete, Index, ValueToDelete ) );
					}
					Added[update.EntityId] = update.Values()[0].asObjectCopy();
			  }

			  public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
			  {
					return new IndexUpdaterAnonymousInnerClass( this );
			  }

			  private class IndexUpdaterAnonymousInnerClass : IndexUpdater
			  {
				  private readonly NodeDeletingWriter _outerInstance;

				  public IndexUpdaterAnonymousInnerClass( NodeDeletingWriter outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public void process<T1>( IndexEntryUpdate<T1> update )
				  {
						switch ( update.UpdateMode() )
						{
							 case ADDED:
							 case CHANGED:
								  _outerInstance.added[update.EntityId] = update.Values()[0].asObjectCopy();
								  break;
							 case REMOVED:
								  _outerInstance.removed[update.EntityId] = update.Values()[0].asObjectCopy(); // on remove, value is the before value
								  break;
							 default:
								  throw new System.ArgumentException( update.UpdateMode().name() );
						}
				  }

				  public void close()
				  {
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexPopulator indexPopulator(boolean constraint) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
		 private IndexPopulator IndexPopulator( bool constraint )
		 {
			  IndexDescriptor descriptor = IndexDescriptor( _first, _name, constraint );
			  return IndexPopulator( descriptor );
		 }

		 private IndexPopulator IndexPopulator( IndexDescriptor descriptor )
		 {
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
			  IndexProvider indexProvider = _db.DependencyResolver.resolveDependency( typeof( DefaultIndexProviderMap ) ).DefaultProvider;
			  return indexProvider.GetPopulator( descriptor.WithId( 21 ), samplingConfig, heapBufferFactory( 1024 ) );
		 }

		 private IndexPopulationJob NewIndexPopulationJob( IndexPopulator populator, FlippableIndexProxy flipper, EntityType type, IndexDescriptor descriptor )
		 {
			  return NewIndexPopulationJob( populator, flipper, _indexStoreView, NullLogProvider.Instance, type, descriptor );
		 }

		 private IndexPopulationJob NewIndexPopulationJob( IndexPopulator populator, FlippableIndexProxy flipper, IndexStoreView storeView, LogProvider logProvider, EntityType type, IndexDescriptor descriptor )
		 {
			  return NewIndexPopulationJob( mock( typeof( FailedIndexProxyFactory ) ), populator, flipper, storeView, logProvider, type, descriptor );
		 }

		 private IndexPopulationJob NewIndexPopulationJob( FailedIndexProxyFactory failureDelegateFactory, IndexPopulator populator, FlippableIndexProxy flipper, IndexStoreView storeView, LogProvider logProvider, EntityType type, IndexDescriptor descriptor )
		 {
			  long indexId = 0;
			  flipper.FlipTarget = mock( typeof( IndexProxyFactory ) );

			  MultipleIndexPopulator multiPopulator = new MultipleIndexPopulator( storeView, logProvider, type, _stateHolder );
			  IndexPopulationJob job = new IndexPopulationJob( multiPopulator, NO_MONITOR, false );
			  job.AddPopulator( populator, descriptor.WithId( indexId ).withoutCapabilities(), format(":%s(%s)", _first.name(), _name), flipper, failureDelegateFactory );
			  return job;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.storageengine.api.schema.IndexDescriptor indexDescriptor(org.neo4j.graphdb.Label label, String propertyKey, boolean constraint) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
		 private IndexDescriptor IndexDescriptor( Label label, string propertyKey, bool constraint )
		 {
			  using ( Transaction tx = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					int labelId = tx.TokenWrite().labelGetOrCreateForName(label.Name());
					int propertyKeyId = tx.TokenWrite().propertyKeyGetOrCreateForName(propertyKey);
					SchemaDescriptor schema = SchemaDescriptorFactory.forLabel( labelId, propertyKeyId );
					IndexDescriptor descriptor = constraint ? IndexDescriptorFactory.uniqueForSchema( schema, PROVIDER_DESCRIPTOR ) : IndexDescriptorFactory.forSchema( schema, PROVIDER_DESCRIPTOR );
					tx.Success();
					return descriptor;
			  }
		 }

		 private long CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Org.Neo4j.Graphdb.Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( labels );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return node.Id;
			  }
		 }

		 private long CreateRelationship( IDictionary<string, object> properties, RelationshipType relType, long fromNode, long toNode )
		 {
			  using ( Org.Neo4j.Graphdb.Transaction tx = _db.beginTx() )
			  {
					Node node1 = _db.getNodeById( fromNode );
					Node node2 = _db.getNodeById( toNode );
					Relationship relationship = node1.CreateRelationshipTo( node2, relType );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 relationship.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return relationship.Id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getPropertyKeyForName(String name) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private int GetPropertyKeyForName( string name )
		 {
			  using ( Transaction tx = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					int result = tx.TokenRead().propertyKey(name);
					tx.Success();
					return result;
			  }
		 }

		 private IndexStoreView IndexStoreView()
		 {
			  return _db.DependencyResolver.resolveDependency( typeof( IndexStoreView ) );
		 }

		 private class TrackingMultipleIndexPopulator : MultipleIndexPopulator
		 {
			  internal volatile bool Closed;

			  internal TrackingMultipleIndexPopulator( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState ) : base( storeView, logProvider, type, schemaState )
			  {
			  }

			  public override void Close( bool populationCompletedSuccessfully )
			  {
					Closed = true;
					base.Close( populationCompletedSuccessfully );
			  }
		 }
	}

}