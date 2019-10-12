using System;

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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Answers = org.mockito.Answers;
	using InjectMocks = org.mockito.InjectMocks;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using Org.Neo4j.Helpers.Collection;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using FlipFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.FlipFailedKernelException;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexPopulation = Org.Neo4j.Kernel.Impl.Api.index.MultipleIndexPopulator.IndexPopulation;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class MultipleIndexPopulatorTest
	public class MultipleIndexPopulatorTest
	{
		 private readonly LabelSchemaDescriptor _index1 = SchemaDescriptorFactory.forLabel( 1, 1 );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_MOCKS) private IndexStoreView indexStoreView;
		 private IndexStoreView _indexStoreView;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_MOCKS) private org.neo4j.logging.LogProvider logProvider;
		 private LogProvider _logProvider;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.kernel.impl.api.SchemaState schemaState;
		 private SchemaState _schemaState;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @InjectMocks private MultipleIndexPopulator multipleIndexPopulator;
		 private MultipleIndexPopulator _multipleIndexPopulator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canceledPopulationNotAbleToCreateNewIndex() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanceledPopulationNotAbleToCreateNewIndex()
		 {
			  IndexPopulator populator = CreateIndexPopulator();
			  IndexPopulation indexPopulation = AddPopulator( populator, 1 );

			  indexPopulation.Cancel();

			  _multipleIndexPopulator.create();

			  verify( populator, never() ).create();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canceledPopulationNotAbleToFlip() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanceledPopulationNotAbleToFlip()
		 {
			  IndexPopulator populator = CreateIndexPopulator();
			  IndexPopulation indexPopulation = AddPopulator( populator, 1 );

			  indexPopulation.Cancel();

			  indexPopulation.Flip( false );

			  verify( indexPopulation.Populator, never() ).sampleResult();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flippedPopulationAreNotCanceable() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlippedPopulationAreNotCanceable()
		 {
			  IndexPopulator populator = CreateIndexPopulator();
			  IndexPopulation indexPopulation = AddPopulator( populator, 1 );

			  indexPopulation.Flip( false );

			  indexPopulation.Cancel();

			  verify( indexPopulation.Populator, never() ).close(false);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cancelAndDropShouldCallDropOnPopulator() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CancelAndDropShouldCallDropOnPopulator()
		 {
			  IndexPopulator populator = CreateIndexPopulator();
			  IndexPopulation indexPopulation = AddPopulator( populator, 1 );

			  indexPopulation.CancelAndDrop();

			  verify( populator, never() ).close(false);
			  verify( populator ).drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiplePopulatorsCreation() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultiplePopulatorsCreation()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();
			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.create();

			  verify( indexPopulator1 ).create();
			  verify( indexPopulator2 ).create();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiplePopulatorCreationFailure() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultiplePopulatorCreationFailure()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();
			  IndexPopulator indexPopulator3 = CreateIndexPopulator();

			  doThrow( PopulatorException ).when( indexPopulator1 ).create();
			  doThrow( PopulatorException ).when( indexPopulator3 ).create();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );
			  AddPopulator( indexPopulator3, 3 );

			  _multipleIndexPopulator.create();

			  CheckPopulatorFailure( indexPopulator1 );
			  CheckPopulatorFailure( indexPopulator3 );

			  verify( indexPopulator2 ).create();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testHasPopulators() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestHasPopulators()
		 {
			  assertFalse( _multipleIndexPopulator.hasPopulators() );

			  AddPopulator( CreateIndexPopulator(), 42 );

			  assertTrue( _multipleIndexPopulator.hasPopulators() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cancelingSinglePopulatorDoNotCancelAnyOther() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CancelingSinglePopulatorDoNotCancelAnyOther()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  IndexPopulation populationToCancel = AddPopulator( indexPopulator1, 1 );
			  IndexPopulation populationToKeepActive = AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.create();

			  _multipleIndexPopulator.cancelIndexPopulation( populationToCancel );

			  _multipleIndexPopulator.indexAllEntities();

			  assertTrue( _multipleIndexPopulator.hasPopulators() );

			  _multipleIndexPopulator.flipAfterPopulation( false );

			  verify( populationToKeepActive.Flipper ).flip( any( typeof( Callable ) ), any( typeof( FailedIndexProxyFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canceledPopulatorDoNotFlipWhenPopulationCompleted() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanceledPopulatorDoNotFlipWhenPopulationCompleted()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  IndexPopulation populationToCancel = AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.create();

			  _multipleIndexPopulator.cancelIndexPopulation( populationToCancel );

			  _multipleIndexPopulator.indexAllEntities();

			  assertTrue( _multipleIndexPopulator.hasPopulators() );

			  _multipleIndexPopulator.flipAfterPopulation( false );

			  verify( populationToCancel.Flipper, never() ).flip(any(typeof(Callable)), any(typeof(FailedIndexProxyFactory)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexAllNodes() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexAllNodes()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.create();
			  _multipleIndexPopulator.indexAllEntities();

			  verify( _indexStoreView ).visitNodes( any( typeof( int[] ) ), any( typeof( System.Func<int, bool> ) ), any( typeof( Visitor ) ), Null, anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailPopulator() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailPopulator()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.fail( PopulatorException );

			  CheckPopulatorFailure( indexPopulator1 );
			  CheckPopulatorFailure( indexPopulator2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailByPopulation() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailByPopulation()
		 {
			  IndexPopulator populator1 = CreateIndexPopulator();
			  IndexPopulator populator2 = CreateIndexPopulator();

			  AddPopulator( populator1, 1 );
			  IndexPopulation population2 = AddPopulator( populator2, 2 );

			  _multipleIndexPopulator.fail( population2, PopulatorException );

			  verify( populator1, never() ).markAsFailed(anyString());
			  CheckPopulatorFailure( populator2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailByPopulationRemovesPopulator() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailByPopulationRemovesPopulator()
		 {
			  IndexPopulator populator1 = CreateIndexPopulator();
			  IndexPopulator populator2 = CreateIndexPopulator();

			  IndexPopulation population1 = AddPopulator( populator1, 1 );
			  IndexPopulation population2 = AddPopulator( populator2, 2 );

			  _multipleIndexPopulator.fail( population1, PopulatorException );
			  _multipleIndexPopulator.fail( population2, PopulatorException );

			  CheckPopulatorFailure( populator1 );
			  CheckPopulatorFailure( populator2 );
			  assertFalse( _multipleIndexPopulator.hasPopulators() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailByNonExistingPopulation() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailByNonExistingPopulation()
		 {
			  IndexPopulation nonExistingPopulation = mock( typeof( IndexPopulation ) );
			  IndexPopulator populator = CreateIndexPopulator();

			  AddPopulator( populator, 1 );

			  _multipleIndexPopulator.fail( nonExistingPopulation, PopulatorException );

			  verify( populator, never() ).markAsFailed(anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFlipAfterPopulation() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFlipAfterPopulation()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  FlippableIndexProxy flipper1 = AddPopulator( indexPopulator1, 1 ).Flipper;
			  FlippableIndexProxy flipper2 = AddPopulator( indexPopulator2, 2 ).Flipper;

			  _multipleIndexPopulator.flipAfterPopulation( false );

			  verify( flipper1 ).flip( any( typeof( Callable ) ), any( typeof( FailedIndexProxyFactory ) ) );
			  verify( flipper2 ).flip( any( typeof( Callable ) ), any( typeof( FailedIndexProxyFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populationsRemovedDuringFlip() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulationsRemovedDuringFlip()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  assertTrue( _multipleIndexPopulator.hasPopulators() );

			  _multipleIndexPopulator.flipAfterPopulation( false );

			  assertFalse( _multipleIndexPopulator.hasPopulators() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCancelPopulation() throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCancelPopulation()
		 {
			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  _multipleIndexPopulator.cancel();

			  verify( _indexStoreView, times( 2 ) ).replaceIndexCounts( anyLong(), eq(0L), eq(0L), eq(0L) );
			  verify( indexPopulator1 ).close( false );
			  verify( indexPopulator2 ).close( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexFlip()
		 public virtual void TestIndexFlip()
		 {
			  IndexProxyFactory indexProxyFactory = mock( typeof( IndexProxyFactory ) );
			  FailedIndexProxyFactory failedIndexProxyFactory = mock( typeof( FailedIndexProxyFactory ) );
			  FlippableIndexProxy flipper = new FlippableIndexProxy();
			  flipper.FlipTarget = indexProxyFactory;

			  IndexPopulator indexPopulator1 = CreateIndexPopulator();
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();
			  AddPopulator( indexPopulator1, 1, flipper, failedIndexProxyFactory );
			  AddPopulator( indexPopulator2, 2, flipper, failedIndexProxyFactory );

			  when( indexPopulator1.SampleResult() ).thenThrow(SampleError);

			  _multipleIndexPopulator.indexAllEntities();
			  _multipleIndexPopulator.flipAfterPopulation( false );

			  verify( indexPopulator1 ).close( false );
			  verify( failedIndexProxyFactory, times( 1 ) ).create( any( typeof( Exception ) ) );

			  verify( indexPopulator2 ).close( true );
			  verify( indexPopulator2 ).sampleResult();
			  verify( _indexStoreView ).replaceIndexCounts( anyLong(), anyLong(), anyLong(), anyLong() );
			  verify( _schemaState ).clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiplePopulatorUpdater() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultiplePopulatorUpdater()
		 {
			  IndexUpdater indexUpdater1 = mock( typeof( IndexUpdater ) );
			  IndexPopulator indexPopulator1 = CreateIndexPopulator( indexUpdater1 );
			  IndexPopulator indexPopulator2 = CreateIndexPopulator();

			  AddPopulator( indexPopulator1, 1 );
			  AddPopulator( indexPopulator2, 2 );

			  doThrow( PopulatorException ).when( indexPopulator2 ).newPopulatingUpdater( any( typeof( NodePropertyAccessor ) ) );

			  IndexUpdater multipleIndexUpdater = _multipleIndexPopulator.newPopulatingUpdater( mock( typeof( NodePropertyAccessor ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> propertyUpdate = createIndexEntryUpdate(index1);
			  IndexEntryUpdate<object> propertyUpdate = CreateIndexEntryUpdate( _index1 );
			  multipleIndexUpdater.Process( propertyUpdate );

			  CheckPopulatorFailure( indexPopulator2 );
			  verify( indexUpdater1 ).process( propertyUpdate );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNonApplicableUpdaterDoNotUpdatePopulator() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNonApplicableUpdaterDoNotUpdatePopulator()
		 {
			  IndexUpdater indexUpdater1 = mock( typeof( IndexUpdater ) );
			  IndexPopulator indexPopulator1 = CreateIndexPopulator( indexUpdater1 );

			  AddPopulator( indexPopulator1, 2 );

			  IndexUpdater multipleIndexUpdater = _multipleIndexPopulator.newPopulatingUpdater( mock( typeof( NodePropertyAccessor ) ) );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> propertyUpdate = createIndexEntryUpdate(index1);
			  IndexEntryUpdate<object> propertyUpdate = CreateIndexEntryUpdate( _index1 );
			  multipleIndexUpdater.Process( propertyUpdate );

			  verifyZeroInteractions( indexUpdater1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPropertyUpdateFailure() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPropertyUpdateFailure()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> propertyUpdate = createIndexEntryUpdate(index1);
			  IndexEntryUpdate<object> propertyUpdate = CreateIndexEntryUpdate( _index1 );
			  IndexUpdater indexUpdater1 = mock( typeof( IndexUpdater ) );
			  IndexPopulator indexPopulator1 = CreateIndexPopulator( indexUpdater1 );

			  AddPopulator( indexPopulator1, 1 );

			  doThrow( PopulatorException ).when( indexUpdater1 ).process( propertyUpdate );

			  IndexUpdater multipleIndexUpdater = _multipleIndexPopulator.newPopulatingUpdater( mock( typeof( NodePropertyAccessor ) ) );

			  multipleIndexUpdater.Process( propertyUpdate );

			  verify( indexUpdater1 ).close();
			  CheckPopulatorFailure( indexPopulator1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultiplePropertyUpdateFailures() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultiplePropertyUpdateFailures()
		 {
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update1 = add(1, index1, "foo");
			  IndexEntryUpdate<object> update1 = add( 1, _index1, "foo" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update2 = add(2, index1, "bar");
			  IndexEntryUpdate<object> update2 = add( 2, _index1, "bar" );
			  IndexUpdater updater = mock( typeof( IndexUpdater ) );
			  IndexPopulator populator = CreateIndexPopulator( updater );

			  AddPopulator( populator, 1 );

			  doThrow( PopulatorException ).when( updater ).process( any( typeof( IndexEntryUpdate ) ) );

			  IndexUpdater multipleIndexUpdater = _multipleIndexPopulator.newPopulatingUpdater( nodePropertyAccessor );

			  multipleIndexUpdater.Process( update1 );
			  multipleIndexUpdater.Process( update2 );

			  verify( updater ).process( update1 );
			  verify( updater, never() ).process(update2);
			  verify( updater ).close();
			  CheckPopulatorFailure( populator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVerifyConstraintsBeforeFlippingIfToldTo() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVerifyConstraintsBeforeFlippingIfToldTo()
		 {
			  // given
			  IndexProxyFactory indexProxyFactory = mock( typeof( IndexProxyFactory ) );
			  FailedIndexProxyFactory failedIndexProxyFactory = mock( typeof( FailedIndexProxyFactory ) );
			  FlippableIndexProxy flipper = new FlippableIndexProxy();
			  flipper.FlipTarget = indexProxyFactory;
			  IndexPopulator indexPopulator = CreateIndexPopulator();
			  AddPopulator( indexPopulator, 1, flipper, failedIndexProxyFactory );
			  when( indexPopulator.SampleResult() ).thenReturn(new IndexSample());

			  // when
			  _multipleIndexPopulator.indexAllEntities();
			  _multipleIndexPopulator.flipAfterPopulation( true );

			  // then
			  verify( indexPopulator ).verifyDeferredConstraints( any( typeof( NodePropertyAccessor ) ) );
			  verify( indexPopulator ).close( true );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> createIndexEntryUpdate(org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor schemaDescriptor)
		 private IndexEntryUpdate<object> CreateIndexEntryUpdate( LabelSchemaDescriptor schemaDescriptor )
		 {
			  return add( 1, schemaDescriptor, "theValue" );
		 }

		 private static Exception SampleError
		 {
			 get
			 {
				  return new Exception( "sample error" );
			 }
		 }

		 private static IndexPopulator CreateIndexPopulator( IndexUpdater indexUpdater )
		 {
			  IndexPopulator indexPopulator = CreateIndexPopulator();
			  when( indexPopulator.NewPopulatingUpdater( any( typeof( NodePropertyAccessor ) ) ) ).thenReturn( indexUpdater );
			  return indexPopulator;
		 }

		 private static IndexPopulator CreateIndexPopulator()
		 {
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  when( populator.SampleResult() ).thenReturn(new IndexSample());
			  return populator;
		 }

		 private static UncheckedIOException PopulatorException
		 {
			 get
			 {
				  return new UncheckedIOException( new IOException( "something went wrong" ) );
			 }
		 }

		 private static void CheckPopulatorFailure( IndexPopulator populator )
		 {
			  verify( populator ).markAsFailed( contains( "something went wrong" ) );
			  verify( populator ).close( false );
		 }

		 private IndexPopulation AddPopulator( IndexPopulator indexPopulator, int id, FlippableIndexProxy flippableIndexProxy, FailedIndexProxyFactory failedIndexProxyFactory )
		 {
			  return AddPopulator( _multipleIndexPopulator, indexPopulator, id, flippableIndexProxy, failedIndexProxyFactory );
		 }

		 private IndexPopulation AddPopulator( MultipleIndexPopulator multipleIndexPopulator, IndexPopulator indexPopulator, int id, FlippableIndexProxy flippableIndexProxy, FailedIndexProxyFactory failedIndexProxyFactory )
		 {
			  return AddPopulator( multipleIndexPopulator, TestIndexDescriptorFactory.forLabel( id, id ).withId( id ), indexPopulator, flippableIndexProxy, failedIndexProxyFactory );
		 }

		 private IndexPopulation AddPopulator( MultipleIndexPopulator multipleIndexPopulator, StoreIndexDescriptor descriptor, IndexPopulator indexPopulator, FlippableIndexProxy flippableIndexProxy, FailedIndexProxyFactory failedIndexProxyFactory )
		 {
			  return multipleIndexPopulator.AddPopulator( indexPopulator, descriptor.WithoutCapabilities(), flippableIndexProxy, failedIndexProxyFactory, "userIndexDescription" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.api.index.MultipleIndexPopulator.IndexPopulation addPopulator(org.neo4j.kernel.api.index.IndexPopulator indexPopulator, int id) throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
		 private IndexPopulation AddPopulator( IndexPopulator indexPopulator, int id )
		 {
			  FlippableIndexProxy indexProxy = mock( typeof( FlippableIndexProxy ) );
			  when( indexProxy.State ).thenReturn( InternalIndexState.ONLINE );
			  doAnswer(invocation =>
			  {
				Callable argument = invocation.getArgument( 0 );
				return argument.call();
			  }).when( indexProxy ).flip( any( typeof( Callable ) ), any( typeof( FailedIndexProxyFactory ) ) );
			  return AddPopulator( indexPopulator, id, indexProxy, mock( typeof( FailedIndexProxyFactory ) ) );
		 }
	}

}