using System.Collections.Generic;
using System.Reflection;
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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using UnmodifiableMap = org.eclipse.collections.impl.UnmodifiableMap;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestRule = org.junit.rules.TestRule;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Predicates = Neo4Net.Functions.Predicates;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using ConstraintValidationException = Neo4Net.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using CachingOffHeapBlockAllocator = Neo4Net.Kernel.impl.util.collection.CachingOffHeapBlockAllocator;
	using CollectionsFactory = Neo4Net.Kernel.impl.util.collection.CollectionsFactory;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using OffHeapCollectionsFactory = Neo4Net.Kernel.impl.util.collection.OffHeapCollectionsFactory;
	using MutableLongDiffSets = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSets;
	using MutableLongDiffSetsImpl = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using Neo4Net.Storageengine.Api.txstate;
	using LongDiffSets = Neo4Net.Storageengine.Api.txstate.LongDiffSets;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.verification.VerificationModeFactory.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Pair.of;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TxStateTest
	public class TxStateTest
	{
		 private static readonly CachingOffHeapBlockAllocator _blockAllocator = new CachingOffHeapBlockAllocator();

		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestRule repeatWithDifferentRandomization()
		 public TestRule RepeatWithDifferentRandomization()
		 {
			  return RuleChain.outerRule( new RepeatRule() ).around(Random);
		 }

		 private readonly IndexDescriptor _indexOn_1_1 = TestIndexDescriptorFactory.forLabel( 1, 1 );
		 private readonly IndexDescriptor _indexOn_2_1 = TestIndexDescriptorFactory.forLabel( 2, 1 );
		 private readonly IndexDescriptor _indexOnRels = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forRelType( 3, 1 ) );

		 private CollectionsFactory _collectionsFactory;
		 private TxState _state;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public org.neo4j.kernel.impl.util.collection.CollectionsFactorySupplier collectionsFactorySupplier;
		 public CollectionsFactorySupplier CollectionsFactorySupplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<org.neo4j.kernel.impl.util.collection.CollectionsFactorySupplier> data()
		 public static IList<CollectionsFactorySupplier> Data()
		 {
			  return asList(new CollectionsFactorySupplierAnonymousInnerClass()
			 , new CollectionsFactorySupplierAnonymousInnerClass2());
		 }

		 private class CollectionsFactorySupplierAnonymousInnerClass : CollectionsFactorySupplier
		 {
			 public CollectionsFactory create()
			 {
				  return Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier_Fields.OnHeap();
			 }

			 public override string ToString()
			 {
				  return "On heap";
			 }
		 }

		 private class CollectionsFactorySupplierAnonymousInnerClass2 : CollectionsFactorySupplier
		 {
			 public CollectionsFactory create()
			 {
				  return new OffHeapCollectionsFactory( _blockAllocator );
			 }

			 public override string ToString()
			 {
				  return "Off heap";
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void afterAll()
		 public static void AfterAll()
		 {
			  _blockAllocator.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _collectionsFactory = spy( CollectionsFactorySupplier() );
			  _state = new TxState( _collectionsFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _collectionsFactory.release();
			  assertEquals( "Seems like native memory is leaking", 0L, _collectionsFactory.MemoryTracker.usedDirectMemory() );
		 }

		 //region node label update tests

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAddedLabels()
		 public virtual void ShouldGetAddedLabels()
		 {
			  // GIVEN
			  _state.nodeDoAddLabel( 1, 0 );
			  _state.nodeDoAddLabel( 1, 1 );
			  _state.nodeDoAddLabel( 2, 1 );

			  // WHEN
			  LongSet addedLabels = _state.nodeStateLabelDiffSets( 1 ).Added;

			  // THEN
			  assertEquals( newSetWith( 1, 2 ), addedLabels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRemovedLabels()
		 public virtual void ShouldGetRemovedLabels()
		 {
			  // GIVEN
			  _state.nodeDoRemoveLabel( 1, 0 );
			  _state.nodeDoRemoveLabel( 1, 1 );
			  _state.nodeDoRemoveLabel( 2, 1 );

			  // WHEN
			  LongSet removedLabels = _state.nodeStateLabelDiffSets( 1 ).Removed;

			  // THEN
			  assertEquals( newSetWith( 1, 2 ), removedLabels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeAddedLabelShouldRemoveFromAdded()
		 public virtual void RemoveAddedLabelShouldRemoveFromAdded()
		 {
			  // GIVEN
			  _state.nodeDoAddLabel( 1, 0 );
			  _state.nodeDoAddLabel( 1, 1 );
			  _state.nodeDoAddLabel( 2, 1 );

			  // WHEN
			  _state.nodeDoRemoveLabel( 1, 1 );

			  // THEN
			  assertEquals( newSetWith( 2 ), _state.nodeStateLabelDiffSets( 1 ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemovedLabelShouldRemoveFromRemoved()
		 public virtual void AddRemovedLabelShouldRemoveFromRemoved()
		 {
			  // GIVEN
			  _state.nodeDoRemoveLabel( 1, 0 );
			  _state.nodeDoRemoveLabel( 1, 1 );
			  _state.nodeDoRemoveLabel( 2, 1 );

			  // WHEN
			  _state.nodeDoAddLabel( 1, 1 );

			  // THEN
			  assertEquals( newSetWith( 2 ), _state.nodeStateLabelDiffSets( 1 ).Removed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapFromRemovedLabelToNodes()
		 public virtual void ShouldMapFromRemovedLabelToNodes()
		 {
			  // GIVEN
			  _state.nodeDoRemoveLabel( 1, 0 );
			  _state.nodeDoRemoveLabel( 2, 0 );
			  _state.nodeDoRemoveLabel( 1, 1 );
			  _state.nodeDoRemoveLabel( 3, 1 );
			  _state.nodeDoRemoveLabel( 2, 2 );

			  // WHEN
			  LongSet nodes = _state.nodesWithLabelChanged( 2 ).Removed;

			  // THEN
			  assertEquals( newSetWith( 0L, 2L ), nodes );
		 }

		 //endregion

		 //region index updates

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeIndexUpdatesOnUninitializedTxState()
		 public virtual void ShouldComputeIndexUpdatesOnUninitializedTxState()
		 {
			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.eclipse.collections.impl.UnmodifiableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getIndexUpdates(indexOn_1_1.schema());
			  UnmodifiableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getIndexUpdates( _indexOn_1_1.schema() );

			  // THEN
			  assertNull( diffSets );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeSortedIndexUpdatesOnUninitializedTxState()
		 public virtual void ShouldComputeSortedIndexUpdatesOnUninitializedTxState()
		 {
			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getSortedIndexUpdates(indexOn_1_1.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getSortedIndexUpdates( _indexOn_1_1.schema() );

			  // THEN
			  assertNull( diffSets );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeIndexUpdatesOnEmptyTxState()
		 public virtual void ShouldComputeIndexUpdatesOnEmptyTxState()
		 {
			  // GIVEN
			  AddNodesToIndex( _indexOn_2_1 ).withDefaultStringProperties( 42L );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.eclipse.collections.impl.UnmodifiableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getIndexUpdates(indexOn_1_1.schema());
			  UnmodifiableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getIndexUpdates( _indexOn_1_1.schema() );

			  // THEN
			  assertNull( diffSets );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeSortedIndexUpdatesOnEmptyTxState()
		 public virtual void ShouldComputeSortedIndexUpdatesOnEmptyTxState()
		 {
			  // GIVEN
			  AddNodesToIndex( _indexOn_2_1 ).withDefaultStringProperties( 42L );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getSortedIndexUpdates(indexOn_1_1.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getSortedIndexUpdates( _indexOn_1_1.schema() );

			  // THEN
			  assertNull( diffSets );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeIndexUpdatesOnTxStateWithAddedNodes()
		 public virtual void ShouldComputeIndexUpdatesOnTxStateWithAddedNodes()
		 {
			  // GIVEN
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 42L );
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 43L );
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 41L );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.eclipse.collections.impl.UnmodifiableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getIndexUpdates(indexOn_1_1.schema());
			  UnmodifiableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getIndexUpdates( _indexOn_1_1.schema() );

			  // THEN
			  AssertEqualDiffSets( AddedNodes( 42L ), diffSets.get( ValueTuple.of( stringValue( "value42" ) ) ) );
			  AssertEqualDiffSets( AddedNodes( 43L ), diffSets.get( ValueTuple.of( stringValue( "value43" ) ) ) );
			  AssertEqualDiffSets( AddedNodes( 41L ), diffSets.get( ValueTuple.of( stringValue( "value41" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeSortedIndexUpdatesOnTxStateWithAddedNodes()
		 public virtual void ShouldComputeSortedIndexUpdatesOnTxStateWithAddedNodes()
		 {
			  // GIVEN
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 42L );
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 43L );
			  AddNodesToIndex( _indexOn_1_1 ).withDefaultStringProperties( 41L );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<org.neo4j.values.storable.ValueTuple,? extends org.neo4j.storageengine.api.txstate.LongDiffSets> diffSets = state.getSortedIndexUpdates(indexOn_1_1.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> diffSets = _state.getSortedIndexUpdates( _indexOn_1_1.schema() );

			  SortedDictionary<ValueTuple, LongDiffSets> expected = SortedAddedNodesDiffSets( 42, 41, 43 );
			  // THEN
			  assertEquals( expected.Keys, diffSets.Keys );
			  foreach ( ValueTuple key in expected.Keys )
			  {
					AssertEqualDiffSets( expected[key], diffSets.get( key ) );
			  }
		 }

		 // endregion

		 //region index rule tests

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddAndGetByLabel()
		 public virtual void ShouldAddAndGetByLabel()
		 {
			  // WHEN
			  _state.indexDoAdd( _indexOn_1_1 );
			  _state.indexDoAdd( _indexOn_2_1 );

			  // THEN
			  assertEquals( asSet( _indexOn_1_1 ), _state.indexDiffSetsByLabel( _indexOn_1_1.schema().keyId() ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddAndGetByRelType()
		 public virtual void ShouldAddAndGetByRelType()
		 {
			  // WHEN
			  _state.indexDoAdd( _indexOnRels );
			  _state.indexDoAdd( _indexOn_2_1 );

			  // THEN
			  assertEquals( asSet( _indexOnRels ), _state.indexDiffSetsByRelationshipType( _indexOnRels.schema().keyId() ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddAndGetByRuleId()
		 public virtual void ShouldAddAndGetByRuleId()
		 {
			  // GIVEN
			  _state.indexDoAdd( _indexOn_1_1 );

			  // THEN
			  assertEquals( asSet( _indexOn_1_1 ), _state.indexChanges().Added );
		 }

		 // endregion

		 //region miscellaneous

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListNodeAsDeletedIfItIsDeleted()
		 public virtual void ShouldListNodeAsDeletedIfItIsDeleted()
		 {
			  // Given

			  // When
			  long nodeId = 1337L;
			  _state.nodeDoDelete( nodeId );

			  // Then
			  assertThat( _state.addedAndRemovedNodes().Removed, equalTo(newSetWith(nodeId)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddUniquenessConstraint()
		 public virtual void ShouldAddUniquenessConstraint()
		 {
			  // when
			  UniquenessConstraintDescriptor constraint = ConstraintDescriptorFactory.uniqueForLabel( 1, 17 );
			  _state.constraintDoAdd( constraint, 7 );

			  // then
			  DiffSets<ConstraintDescriptor> diff = _state.constraintsChangesForLabel( 1 );

			  assertEquals( singleton( constraint ), diff.Added );
			  assertTrue( diff.Removed.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingUniquenessConstraintShouldBeIdempotent()
		 public virtual void AddingUniquenessConstraintShouldBeIdempotent()
		 {
			  // given
			  UniquenessConstraintDescriptor constraint1 = ConstraintDescriptorFactory.uniqueForLabel( 1, 17 );
			  _state.constraintDoAdd( constraint1, 7 );

			  // when
			  UniquenessConstraintDescriptor constraint2 = ConstraintDescriptorFactory.uniqueForLabel( 1, 17 );
			  _state.constraintDoAdd( constraint2, 19 );

			  // then
			  assertEquals( constraint1, constraint2 );
			  assertEquals( singleton( constraint1 ), _state.constraintsChangesForLabel( 1 ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDifferentiateBetweenUniquenessConstraintsForDifferentLabels()
		 public virtual void ShouldDifferentiateBetweenUniquenessConstraintsForDifferentLabels()
		 {
			  // when
			  UniquenessConstraintDescriptor constraint1 = ConstraintDescriptorFactory.uniqueForLabel( 1, 17 );
			  _state.constraintDoAdd( constraint1, 7 );
			  UniquenessConstraintDescriptor constraint2 = ConstraintDescriptorFactory.uniqueForLabel( 2, 17 );
			  _state.constraintDoAdd( constraint2, 19 );

			  // then
			  assertEquals( singleton( constraint1 ), _state.constraintsChangesForLabel( 1 ).Added );
			  assertEquals( singleton( constraint2 ), _state.constraintsChangesForLabel( 2 ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddRelationshipPropertyExistenceConstraint()
		 public virtual void ShouldAddRelationshipPropertyExistenceConstraint()
		 {
			  // Given
			  ConstraintDescriptor constraint = ConstraintDescriptorFactory.existsForRelType( 1, 42 );

			  // When
			  _state.constraintDoAdd( constraint );

			  // Then
			  assertEquals( singleton( constraint ), _state.constraintsChangesForRelationshipType( 1 ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingRelationshipPropertyExistenceConstraintConstraintShouldBeIdempotent()
		 public virtual void AddingRelationshipPropertyExistenceConstraintConstraintShouldBeIdempotent()
		 {
			  // Given
			  ConstraintDescriptor constraint1 = ConstraintDescriptorFactory.existsForRelType( 1, 42 );
			  ConstraintDescriptor constraint2 = ConstraintDescriptorFactory.existsForRelType( 1, 42 );

			  // When
			  _state.constraintDoAdd( constraint1 );
			  _state.constraintDoAdd( constraint2 );

			  // Then
			  assertEquals( constraint1, constraint2 );
			  assertEquals( singleton( constraint1 ), _state.constraintsChangesForRelationshipType( 1 ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropRelationshipPropertyExistenceConstraint()
		 public virtual void ShouldDropRelationshipPropertyExistenceConstraint()
		 {
			  // Given
			  ConstraintDescriptor constraint = ConstraintDescriptorFactory.existsForRelType( 1, 42 );
			  _state.constraintDoAdd( constraint );

			  // When
			  _state.constraintDoDrop( constraint );

			  // Then
			  assertTrue( _state.constraintsChangesForRelationshipType( 1 ).Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDifferentiateRelationshipPropertyExistenceConstraints()
		 public virtual void ShouldDifferentiateRelationshipPropertyExistenceConstraints()
		 {
			  // Given
			  ConstraintDescriptor constraint1 = ConstraintDescriptorFactory.existsForRelType( 1, 11 );
			  ConstraintDescriptor constraint2 = ConstraintDescriptorFactory.existsForRelType( 1, 22 );
			  ConstraintDescriptor constraint3 = ConstraintDescriptorFactory.existsForRelType( 3, 33 );

			  // When
			  _state.constraintDoAdd( constraint1 );
			  _state.constraintDoAdd( constraint2 );
			  _state.constraintDoAdd( constraint3 );

			  // Then
			  assertEquals( asSet( constraint1, constraint2 ), _state.constraintsChangesForRelationshipType( 1 ).Added );
			  assertEquals( singleton( constraint1 ), _state.constraintsChangesForSchema( constraint1.Schema() ).Added );
			  assertEquals( singleton( constraint2 ), _state.constraintsChangesForSchema( constraint2.Schema() ).Added );
			  assertEquals( singleton( constraint3 ), _state.constraintsChangesForRelationshipType( 3 ).Added );
			  assertEquals( singleton( constraint3 ), _state.constraintsChangesForSchema( constraint3.Schema() ).Added );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRelationshipsAsCreatedIfCreated()
		 public virtual void ShouldListRelationshipsAsCreatedIfCreated()
		 {
			  // When
			  long relId = 10;
			  _state.relationshipDoCreate( relId, 0, 1, 2 );

			  // Then
			  assertTrue( _state.hasChanges() );
			  assertTrue( _state.relationshipIsAddedInThisTx( relId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeRecordForCreatedAndDeletedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotChangeRecordForCreatedAndDeletedNode()
		 {
			  // GIVEN
			  _state.nodeDoCreate( 0 );
			  _state.nodeDoDelete( 0 );
			  _state.nodeDoCreate( 1 );

			  // WHEN
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass( this ) );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 public TxStateVisitor_AdapterAnonymousInnerClass( TxStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void visitCreatedNode( long id )
			 {
				  assertEquals( "Should not create any other node than 1", 1, id );
			 }

			 public override void visitDeletedNode( long id )
			 {
				  fail( "Should not delete any node" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVisitDeletedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitDeletedNode()
		 {
			  // Given
			  _state.nodeDoDelete( 42 );

			  // When
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass2( this ) );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass2 : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 public TxStateVisitor_AdapterAnonymousInnerClass2( TxStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void visitDeletedNode( long id )
			 {
				  // Then
				  assertEquals( "Wrong deleted node id", 42, id );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDeletedNodeIfItWasCreatedAndDeletedInSameTx()
		 public virtual void ShouldReportDeletedNodeIfItWasCreatedAndDeletedInSameTx()
		 {
			  // Given
			  long nodeId = 42;

			  // When
			  _state.nodeDoCreate( nodeId );
			  _state.nodeDoDelete( nodeId );

			  // Then
			  assertTrue( _state.nodeIsDeletedInThisTx( nodeId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportDeletedNodeIfItIsNotDeleted()
		 public virtual void ShouldNotReportDeletedNodeIfItIsNotDeleted()
		 {
			  // Given
			  long nodeId = 42;

			  // When
			  _state.nodeDoCreate( nodeId );

			  // Then
			  assertFalse( _state.nodeIsDeletedInThisTx( nodeId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeRecordForCreatedAndDeletedRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotChangeRecordForCreatedAndDeletedRelationship()
		 {
			  // GIVEN
			  _state.relationshipDoCreate( 0, 0, 1, 2 );
			  _state.relationshipDoDelete( 0, 0, 1, 2 );
			  _state.relationshipDoCreate( 1, 0, 2, 3 );

			  // WHEN
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass3( this ) );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass3 : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 public TxStateVisitor_AdapterAnonymousInnerClass3( TxStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void visitCreatedRelationship( long id, int type, long startNode, long endNode )
			 {
				  assertEquals( "Should not create any other relationship than 1", 1, id );
			 }

			 public override void visitDeletedRelationship( long id )
			 {
				  fail( "Should not delete any relationship" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotVisitNotModifiedPropertiesOnModifiedNodes() throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException, org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotVisitNotModifiedPropertiesOnModifiedNodes()
		 {
			  _state.nodeDoAddLabel( 5, 1 );
			  MutableBoolean labelsChecked = new MutableBoolean();
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass4( this, labelsChecked ) );
			  assertTrue( labelsChecked.booleanValue() );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass4 : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 private MutableBoolean _labelsChecked;

			 public TxStateVisitor_AdapterAnonymousInnerClass4( TxStateTest outerInstance, MutableBoolean labelsChecked )
			 {
				 this.outerInstance = outerInstance;
				 this._labelsChecked = labelsChecked;
			 }

			 public override void visitNodeLabelChanges( long id, LongSet added, LongSet removed )
			 {
				  _labelsChecked.setTrue();
				  assertEquals( 1, id );
				  assertEquals( 1, added.size() );
				  assertTrue( added.contains( 5 ) );
				  assertTrue( removed.Empty );
			 }

			 public override void visitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			 {
				  fail( "Properties were not changed." );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotVisitNotModifiedLabelsOnModifiedNodes() throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException, org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotVisitNotModifiedLabelsOnModifiedNodes()
		 {
			  _state.nodeDoAddProperty( 1, 2, stringValue( "propertyValue" ) );
			  MutableBoolean propertiesChecked = new MutableBoolean();
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass5( this, propertiesChecked ) );
			  assertTrue( propertiesChecked.booleanValue() );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass5 : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 private MutableBoolean _propertiesChecked;

			 public TxStateVisitor_AdapterAnonymousInnerClass5( TxStateTest outerInstance, MutableBoolean propertiesChecked )
			 {
				 this.outerInstance = outerInstance;
				 this._propertiesChecked = propertiesChecked;
			 }

			 public override void visitNodeLabelChanges( long id, LongSet added, LongSet removed )
			 {
				  fail( "Labels were not changed." );
			 }

			 public override void visitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			 {
				  _propertiesChecked.setTrue();
				  assertEquals( 1, id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  assertFalse( changed.hasNext() );
				  assertTrue( removed.Empty );
				  assertEquals( 1, Iterators.count( added, Predicates.alwaysTrue() ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVisitDeletedRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitDeletedRelationship()
		 {
			  // Given
			  _state.relationshipDoDelete( 42, 2, 3, 4 );

			  // When
			  _state.accept( new TxStateVisitor_AdapterAnonymousInnerClass6( this ) );
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass6 : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			 public TxStateVisitor_AdapterAnonymousInnerClass6( TxStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void visitDeletedRelationship( long id )
			 {
				  // Then
				  assertEquals( "Wrong deleted relationship id", 42, id );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDeletedRelationshipIfItWasCreatedAndDeletedInSameTx()
		 public virtual void ShouldReportDeletedRelationshipIfItWasCreatedAndDeletedInSameTx()
		 {
			  // Given
			  long startNodeId = 1;
			  long relationshipId = 2;
			  int relationshipType = 3;
			  long endNodeId = 4;

			  // When
			  _state.relationshipDoCreate( relationshipId, relationshipType, startNodeId, endNodeId );
			  _state.relationshipDoDelete( relationshipId, relationshipType, startNodeId, endNodeId );

			  // Then
			  assertTrue( _state.relationshipIsDeletedInThisTx( relationshipId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportDeletedRelationshipIfItIsNotDeleted()
		 public virtual void ShouldNotReportDeletedRelationshipIfItIsNotDeleted()
		 {
			  // Given
			  long startNodeId = 1;
			  long relationshipId = 2;
			  int relationshipType = 3;
			  long endNodeId = 4;

			  // When
			  _state.relationshipDoCreate( relationshipId, relationshipType, startNodeId, endNodeId );

			  // Then
			  assertFalse( _state.relationshipIsDeletedInThisTx( relationshipId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @RepeatRule.Repeat(times = 100) public void shouldVisitCreatedNodesBeforeDeletedNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitCreatedNodesBeforeDeletedNodes()
		 {
			  // when
			  _state.accept( new VisitationOrderAnonymousInnerClass( this, Random.Next( 100 ) ) );
		 }

		 private class VisitationOrderAnonymousInnerClass : VisitationOrder
		 {
			 private readonly TxStateTest _outerInstance;

			 public VisitationOrderAnonymousInnerClass( TxStateTest outerInstance, int nextInt ) : base( outerInstance, nextInt )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // given

			 internal override void createEarlyState()
			 {
				  _outerInstance.state.nodeDoCreate( _outerInstance.random.Next( 1 << 20 ) );
			 }

			 internal override void createLateState()
			 {
				  _outerInstance.state.nodeDoDelete( _outerInstance.random.Next( 1 << 20 ) );
			 }

			 // then

			 public override void visitCreatedNode( long id )
			 {
				  visitEarly();
			 }

			 public override void visitDeletedNode( long id )
			 {
				  visitLate();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @RepeatRule.Repeat(times = 100) public void shouldVisitCreatedNodesBeforeCreatedRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitCreatedNodesBeforeCreatedRelationships()
		 {
			  // when
			  _state.accept( new VisitationOrderAnonymousInnerClass2( this, Random.Next( 100 ) ) );
		 }

		 private class VisitationOrderAnonymousInnerClass2 : VisitationOrder
		 {
			 private readonly TxStateTest _outerInstance;

			 public VisitationOrderAnonymousInnerClass2( TxStateTest outerInstance, int nextInt ) : base( outerInstance, nextInt )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // given

			 internal override void createEarlyState()
			 {
				  _outerInstance.state.nodeDoCreate( _outerInstance.random.Next( 1 << 20 ) );
			 }

			 internal override void createLateState()
			 {
				  _outerInstance.state.relationshipDoCreate( _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 128 ), _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 1 << 20 ) );
			 }

			 // then

			 public override void visitCreatedNode( long id )
			 {
				  visitEarly();
			 }

			 public override void visitCreatedRelationship( long id, int type, long startNode, long endNode )
			 {
				  visitLate();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @RepeatRule.Repeat(times = 100) public void shouldVisitCreatedRelationshipsBeforeDeletedRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitCreatedRelationshipsBeforeDeletedRelationships()
		 {
			  // when
			  _state.accept( new VisitationOrderAnonymousInnerClass3( this, Random.Next( 100 ) ) );
		 }

		 private class VisitationOrderAnonymousInnerClass3 : VisitationOrder
		 {
			 private readonly TxStateTest _outerInstance;

			 public VisitationOrderAnonymousInnerClass3( TxStateTest outerInstance, int nextInt ) : base( outerInstance, nextInt )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // given

			 internal override void createEarlyState()
			 {
				  _outerInstance.state.relationshipDoCreate( _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 128 ), _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 1 << 20 ) );
			 }

			 internal override void createLateState()
			 {
				  _outerInstance.state.relationshipDoDelete( _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 128 ), _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 1 << 20 ) );
			 }

			 // then
			 public override void visitCreatedRelationship( long id, int type, long startNode, long endNode )
			 {
				  visitEarly();
			 }

			 public override void visitDeletedRelationship( long id )
			 {
				  visitLate();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @RepeatRule.Repeat(times = 100) public void shouldVisitDeletedNodesAfterDeletedRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitDeletedNodesAfterDeletedRelationships()
		 {
			  // when
			  _state.accept( new VisitationOrderAnonymousInnerClass4( this, Random.Next( 100 ) ) );
		 }

		 private class VisitationOrderAnonymousInnerClass4 : VisitationOrder
		 {
			 private readonly TxStateTest _outerInstance;

			 public VisitationOrderAnonymousInnerClass4( TxStateTest outerInstance, int nextInt ) : base( outerInstance, nextInt )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // given

			 internal override void createEarlyState()
			 {
				  _outerInstance.state.relationshipDoCreate( _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 128 ), _outerInstance.random.Next( 1 << 20 ), _outerInstance.random.Next( 1 << 20 ) );
			 }

			 internal override void createLateState()
			 {
				  _outerInstance.state.nodeDoDelete( _outerInstance.random.Next( 1 << 20 ) );
			 }

			 // then

			 public override void visitDeletedRelationship( long id )
			 {
				  visitEarly();
			 }

			 public override void visitDeletedNode( long id )
			 {
				  visitLate();
			 }
		 }

	//    getOrCreateLabelStateNodeDiffSets

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateNodeState_props_useCollectionsFactory()
		 public virtual void getOrCreateNodeState_props_useCollectionsFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NodeStateImpl nodeState = state.getOrCreateNodeState(1);
			  NodeStateImpl nodeState = _state.getOrCreateNodeState( 1 );

			  nodeState.AddProperty( 2, stringValue( "foo" ) );
			  nodeState.RemoveProperty( 3 );
			  nodeState.ChangeProperty( 4, stringValue( "bar" ) );

			  verify( _collectionsFactory, times( 2 ) ).newValuesMap();
			  verify( _collectionsFactory, times( 1 ) ).newLongSet();
			  verifyNoMoreInteractions( _collectionsFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateGraphState_useCollectionsFactory()
		 public virtual void getOrCreateGraphState_useCollectionsFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GraphStateImpl graphState = state.getOrCreateGraphState();
			  GraphStateImpl graphState = _state.OrCreateGraphState;

			  graphState.AddProperty( 2, stringValue( "foo" ) );
			  graphState.RemoveProperty( 3 );
			  graphState.ChangeProperty( 4, stringValue( "bar" ) );

			  verify( _collectionsFactory, times( 2 ) ).newValuesMap();
			  verify( _collectionsFactory, times( 1 ) ).newLongSet();

			  verifyNoMoreInteractions( _collectionsFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateLabelStateNodeDiffSets_useCollectionsFactory()
		 public virtual void getOrCreateLabelStateNodeDiffSets_useCollectionsFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSets diffSets = state.getOrCreateLabelStateNodeDiffSets(1);
			  MutableLongDiffSets diffSets = _state.getOrCreateLabelStateNodeDiffSets( 1 );

			  diffSets.Add( 1 );
			  diffSets.Remove( 2 );

			  verify( _collectionsFactory, times( 2 ) ).newLongSet();
			  verifyNoMoreInteractions( _collectionsFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateIndexUpdatesForSeek_useCollectionsFactory()
		 public virtual void getOrCreateIndexUpdatesForSeek_useCollectionsFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSets diffSets = state.getOrCreateIndexUpdatesForSeek(new java.util.HashMap<>(), org.neo4j.values.storable.ValueTuple.of(stringValue("test")));
			  MutableLongDiffSets diffSets = _state.getOrCreateIndexUpdatesForSeek( new Dictionary<ValueTuple, MutableLongDiffSets>(), ValueTuple.of(stringValue("test")) );
			  diffSets.Add( 1 );
			  diffSets.Remove( 2 );
			  verify( _collectionsFactory, times( 2 ) ).newLongSet();
			  verifyNoMoreInteractions( _collectionsFactory );
		 }

		 private LongDiffSets AddedNodes( params long[] added )
		 {
			  return new MutableLongDiffSetsImpl( LongSets.mutable.of( added ), LongSets.mutable.empty(), _collectionsFactory );
		 }

		 private SortedDictionary<ValueTuple, LongDiffSets> SortedAddedNodesDiffSets( params long[] added )
		 {
			  SortedDictionary<ValueTuple, LongDiffSets> map = new SortedDictionary<ValueTuple, LongDiffSets>( ValueTuple.COMPARATOR );
			  foreach ( long node in added )
			  {

					map[ValueTuple.of( stringValue( "value" + node ) )] = AddedNodes( node );
			  }
			  return map;
		 }

		 internal abstract class VisitationOrder : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly TxStateTest _outerInstance;

			  internal readonly ISet<string> VisitMethods = new HashSet<string>();

			  internal VisitationOrder( TxStateTest outerInstance, int size )
			  {
				  this._outerInstance = outerInstance;
					foreach ( System.Reflection.MethodInfo method in this.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) )
					{
						 if ( method.Name.StartsWith( "visit" ) )
						 {
							  VisitMethods.Add( method.Name );
						 }
					}
					assertEquals( "should implement exactly two visit*(...) methods", 2, VisitMethods.Count );
					do
					{
						 if ( outerInstance.Random.nextBoolean() )
						 {
							  CreateEarlyState();
						 }
						 else
						 {
							  CreateLateState();
						 }
					} while ( size-- > 0 );
			  }

			  internal abstract void CreateEarlyState();

			  internal abstract void CreateLateState();

			  internal bool Late;

			  internal void VisitEarly()
			  {
					if ( Late )
					{
						 string early = "the early visit*-method";
						 string late = "the late visit*-method";
						 foreach ( StackTraceElement trace in Thread.CurrentThread.StackTrace )
						 {
							  if ( VisitMethods.Contains( trace.MethodName ) )
							  {
									early = trace.MethodName;
									foreach ( string method in VisitMethods )
									{
										 if ( !method.Equals( early ) )
										 {
											  late = method;
										 }
									}
									break;
							  }
						 }
						 fail( early + "(...) should not be invoked after " + late + "(...)" );
					}
			  }

			  internal void VisitLate()
			  {
					Late = true;
			  }
		 }

		 private interface IndexUpdater
		 {
			  void WithDefaultStringProperties( params long[] nodeIds );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private IndexUpdater addNodesToIndex(final org.neo4j.storageengine.api.schema.IndexDescriptor descriptor)
		 private IndexUpdater AddNodesToIndex( IndexDescriptor descriptor )
		 {
			  return new IndexUpdaterAnonymousInnerClass( this, descriptor );
		 }

		 private class IndexUpdaterAnonymousInnerClass : IndexUpdater
		 {
			 private readonly TxStateTest _outerInstance;

			 private IndexDescriptor _descriptor;

			 public IndexUpdaterAnonymousInnerClass( TxStateTest outerInstance, IndexDescriptor descriptor )
			 {
				 this.outerInstance = outerInstance;
				 this._descriptor = descriptor;
			 }

			 public void withDefaultStringProperties( params long[] nodeIds )
			 {
				  ICollection<Pair<long, string>> entries = new List<Pair<long, string>>( nodeIds.Length );
				  foreach ( long nodeId in nodeIds )
				  {
						entries.Add( of( nodeId, "value" + nodeId ) );
				  }
				  withProperties( entries );
			 }

			 private void withProperties<T>( ICollection<Pair<long, T>> nodesWithValues )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int labelId = descriptor.schema().keyId();
				  int labelId = _descriptor.schema().keyId();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int propertyKeyId = descriptor.schema().getPropertyId();
				  int propertyKeyId = _descriptor.schema().PropertyId;
				  foreach ( Pair<long, T> entry in nodesWithValues )
				  {
						long nodeId = entry.First();
						_outerInstance.state.nodeDoCreate( nodeId );
						_outerInstance.state.nodeDoAddLabel( labelId, nodeId );
						Value valueAfter = Values.of( entry.Other() );
						_outerInstance.state.nodeDoAddProperty( nodeId, propertyKeyId, valueAfter );
						_outerInstance.state.indexDoUpdateEntry( _descriptor.schema(), nodeId, null, ValueTuple.of(valueAfter) );
				  }
			 }
		 }

		 private static void AssertEqualDiffSets( LongDiffSets expected, LongDiffSets actual )
		 {
			  assertEquals( expected.Removed, actual.Removed );
			  assertEquals( expected.Added, actual.Added );
		 }
	}

}