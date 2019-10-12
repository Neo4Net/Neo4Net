using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using UnmodifiableMap = org.eclipse.collections.impl.UnmodifiableMap;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using UnifiedSet = org.eclipse.collections.impl.set.mutable.UnifiedSet;
	using DynamicTest = org.junit.jupiter.api.DynamicTest;
	using Nested = org.junit.jupiter.api.Nested;
	using Test = org.junit.jupiter.api.Test;
	using TestFactory = org.junit.jupiter.api.TestFactory;
	using Mockito = org.mockito.Mockito;


	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using AddedAndRemoved = Org.Neo4j.Kernel.Impl.Newapi.TxStateIndexChanges.AddedAndRemoved;
	using AddedWithValuesAndRemoved = Org.Neo4j.Kernel.Impl.Newapi.TxStateIndexChanges.AddedWithValuesAndRemoved;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using OnHeapCollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.OnHeapCollectionsFactory;
	using MutableLongDiffSetsImpl = Org.Neo4j.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueTuple = Org.Neo4j.Values.Storable.ValueTuple;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyIterable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForRangeSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForRangeSeekByPrefix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForScan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForSuffixOrContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForRangeSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForRangeSeekByPrefix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForScan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForSuffixOrContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	internal class TxStateIndexChangesTest
	{
		 private readonly IndexDescriptor _index = TestIndexDescriptorFactory.forLabel( 1, 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForScanOnAnEmptyTxState()
		 internal virtual void ShouldComputeIndexUpdatesForScanOnAnEmptyTxState()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = org.mockito.Mockito.mock(org.neo4j.storageengine.api.txstate.ReadableTransactionState.class);
			  ReadableTransactionState state = Mockito.mock( typeof( ReadableTransactionState ) );

			  // WHEN
			  AddedAndRemoved changes = indexUpdatesForScan( state, _index, IndexOrder.NONE );
			  AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForScan( state, _index, IndexOrder.NONE );

			  // THEN
			  assertTrue( changes.Empty );
			  assertTrue( changesWithValues.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForScanWhenThereAreNewNodes()
		 internal virtual void ShouldComputeIndexUpdatesForScanWhenThereAreNewNodes()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(42L, "foo").withAdded(43L, "bar").build();
			  ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "foo").withAdded(43L, "bar").build();

			  // WHEN
			  AddedAndRemoved changes = indexUpdatesForScan( state, _index, IndexOrder.NONE );
			  AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForScan( state, _index, IndexOrder.NONE );

			  // THEN
			  AssertContains( changes.Added, 42L, 43L );
			  AssertContains( changesWithValues.Added, NodeWithPropertyValues( 42L, "foo" ), NodeWithPropertyValues( 43L, "bar" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForScan()
		 internal virtual void ShouldComputeIndexUpdatesForScan()
		 {
			  AssertScanWithOrder( IndexOrder.NONE );
			  AssertScanWithOrder( IndexOrder.ASCENDING );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForScanWithDescendingOrder()
		 internal virtual void ShouldComputeIndexUpdatesForScanWithDescendingOrder()
		 {
			  AssertScanWithOrder( IndexOrder.DESCENDING );
		 }

		 private void AssertScanWithOrder( IndexOrder indexOrder )
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(40L, "Aaron").withAdded(41L, "Agatha").withAdded(42L, "Andreas").withAdded(43L, "Barbarella").withAdded(44L, "Andrea").withAdded(45L, "Aristotle").withAdded(46L, "Barbara").withAdded(47L, "Cinderella").build();
			  ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Aaron").withAdded(41L, "Agatha").withAdded(42L, "Andreas").withAdded(43L, "Barbarella").withAdded(44L, "Andrea").withAdded(45L, "Aristotle").withAdded(46L, "Barbara").withAdded(47L, "Cinderella").build();

			  // WHEN
			  AddedAndRemoved changes = indexUpdatesForScan( state, _index, indexOrder );
			  AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForScan( state, _index, indexOrder );

			  NodeWithPropertyValues[] expectedNodesWithValues = new NodeWithPropertyValues[] { NodeWithPropertyValues( 40L, "Aaron" ), NodeWithPropertyValues( 41L, "Agatha" ), NodeWithPropertyValues( 44L, "Andrea" ), NodeWithPropertyValues( 42L, "Andreas" ), NodeWithPropertyValues( 45L, "Aristotle" ), NodeWithPropertyValues( 46L, "Barbara" ), NodeWithPropertyValues( 43L, "Barbarella" ), NodeWithPropertyValues( 47L, "Cinderella" ) };

			  // THEN
			  AssertContains( indexOrder, changes, changesWithValues, expectedNodesWithValues );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForSeekWhenThereAreNewNodes()
		 internal virtual void ShouldComputeIndexUpdatesForSeekWhenThereAreNewNodes()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(42L, "foo").withAdded(43L, "bar").build();
			  ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "foo").withAdded(43L, "bar").build();

			  // WHEN
			  AddedAndRemoved changes = indexUpdatesForSeek( state, _index, ValueTuple.of( "bar" ) );

			  // THEN
			  AssertContains( changes.Added, 43L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Collection<org.junit.jupiter.api.DynamicTest> rangeTests()
		 internal virtual ICollection<DynamicTest> RangeTests()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(42L, 510).withAdded(43L, 520).withAdded(44L, 550).withAdded(45L, 500).withAdded(46L, 530).withAdded(47L, 560).withAdded(48L, 540).build();
			  ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, 510).withAdded(43L, 520).withAdded(44L, 550).withAdded(45L, 500).withAdded(46L, 530).withAdded(47L, 560).withAdded(48L, 540).build();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.junit.jupiter.api.DynamicTest> tests = new java.util.ArrayList<>();
			  ICollection<DynamicTest> tests = new List<DynamicTest>();

			  tests.addAll( RangeTest( state, Values.of( 510 ), true, Values.of( 550 ), true, NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 510 ), true, Values.of( 550 ), false, NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 510 ), false, Values.of( 550 ), true, NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 510 ), false, Values.of( 550 ), false, NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ) ) );
			  tests.addAll( RangeTest( state, null, false, Values.of( 550 ), true, NodeWithPropertyValues( 45L, 500 ), NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ) ) );
			  tests.addAll( RangeTest( state, null, true, Values.of( 550 ), true, NodeWithPropertyValues( 45L, 500 ), NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ) ) );
			  tests.addAll( RangeTest( state, null, false, Values.of( 550 ), false, NodeWithPropertyValues( 45L, 500 ), NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ) ) );
			  tests.addAll( RangeTest( state, null, true, Values.of( 550 ), false, NodeWithPropertyValues( 45L, 500 ), NodeWithPropertyValues( 42L, 510 ), NodeWithPropertyValues( 43L, 520 ), NodeWithPropertyValues( 46L, 530 ), NodeWithPropertyValues( 48L, 540 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 540 ), true, null, true, NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ), NodeWithPropertyValues( 47L, 560 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 540 ), true, null, false, NodeWithPropertyValues( 48L, 540 ), NodeWithPropertyValues( 44L, 550 ), NodeWithPropertyValues( 47L, 560 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 540 ), false, null, true, NodeWithPropertyValues( 44L, 550 ), NodeWithPropertyValues( 47L, 560 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 540 ), false, null, false, NodeWithPropertyValues( 44L, 550 ), NodeWithPropertyValues( 47L, 560 ) ) );
			  tests.addAll( RangeTest( state, Values.of( 560 ), false, Values.of( 800 ), true ) );

			  return tests;
		 }

		 private ICollection<DynamicTest> RangeTest( ReadableTransactionState state, Value lo, bool includeLo, Value hi, bool includeHi, params NodeWithPropertyValues[] expected )
		 {
			  return Arrays.asList( RangeTest( state, IndexOrder.NONE, lo, includeLo, hi, includeHi, expected ), RangeTest( state, IndexOrder.ASCENDING, lo, includeLo, hi, includeHi, expected ), RangeTest( state, IndexOrder.DESCENDING, lo, includeLo, hi, includeHi, expected ) );
		 }

		 private DynamicTest RangeTest( ReadableTransactionState state, IndexOrder indexOrder, Value lo, bool includeLo, Value hi, bool includeHi, params NodeWithPropertyValues[] expected )
		 {
			  return DynamicTest.dynamicTest(string.Format("range seek: lo={0} (incl: {1}), hi={2} (incl: {3})", lo, includeLo, hi, includeHi), () =>
			  {
				// Internal production code relies on null for unbounded, and cannot cope with NO_VALUE in this case
				Debug.Assert( lo != NO_VALUE );
				Debug.Assert( hi != NO_VALUE );
				AddedAndRemoved changes = indexUpdatesForRangeSeek( state, _index, IndexQuery.range( -1, lo, includeLo, hi, includeHi ), indexOrder );
				AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForRangeSeek( state, _index, IndexQuery.range( -1, lo, includeLo, hi, includeHi ), indexOrder );

				AssertContains( indexOrder, changes, changesWithValues, expected );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class SuffixOrContains
		 internal class SuffixOrContains
		 {
			 private readonly TxStateIndexChangesTest _outerInstance;

			 public SuffixOrContains( TxStateIndexChangesTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByContainsWhenThereAreNoMatchingNodes()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByContainsWhenThereAreNoMatchingNodes()
			  {
					// GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(42L, "foo").withAdded(43L, "bar").build();
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "foo").withAdded(43L, "bar").build();

					// WHEN
					IndexQuery.StringContainsPredicate indexQuery = IndexQuery.stringContains( outerInstance.index.Schema().PropertyId, stringValue("eulav") );
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );

					// THEN
					assertTrue( changes.Added.Empty );
					assertFalse( changesWithValues.Added.GetEnumerator().hasNext() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekBySuffixWhenThereArePartiallyMatchingNewNodes()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekBySuffixWhenThereArePartiallyMatchingNewNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Aaron").withAdded(41L, "Agatha").withAdded(42L, "Andreas").withAdded(43L, "Andrea").withAdded(44L, "Aristotle").withAdded(45L, "Barbara").withAdded(46L, "Barbarella").withAdded(47L, "Cinderella").build();

					// WHEN
					IndexQuery.StringSuffixPredicate indexQuery = IndexQuery.stringSuffix( outerInstance.index.Schema().PropertyId, stringValue("ella") );
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );

					// THEN
					outerInstance.AssertContains( changes.Added, 46L, 47L );
					outerInstance.AssertContains( changesWithValues.Added, NodeWithPropertyValues( 46L, "Barbarella" ), NodeWithPropertyValues( 47L, "Cinderella" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForSuffixWithAscendingOrder()
			  internal virtual void ShouldComputeIndexUpdatesForSuffixWithAscendingOrder()
			  {
					AssertRangeSeekBySuffixForOrder( IndexOrder.ASCENDING );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForSuffixWithDescendingOrder()
			  internal virtual void ShouldComputeIndexUpdatesForSuffixWithDescendingOrder()
			  {
					AssertRangeSeekBySuffixForOrder( IndexOrder.DESCENDING );
			  }

			  internal virtual void AssertRangeSeekBySuffixForOrder( IndexOrder indexOrder )
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Aaron").withAdded(41L, "Bonbon").withAdded(42L, "Crayfish").withAdded(43L, "Mayonnaise").withAdded(44L, "Seashell").withAdded(45L, "Ton").withAdded(46L, "Macron").withAdded(47L, "Tony").withAdded(48L, "Evon").withAdded(49L, "Andromeda").build();

					// WHEN
					IndexQuery indexQuery = IndexQuery.stringSuffix( outerInstance.index.Schema().PropertyId, stringValue("on") );
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( state, outerInstance.index, indexQuery, indexOrder );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForSuffixOrContains( state, outerInstance.index, indexQuery, indexOrder );

					NodeWithPropertyValues[] expected = new NodeWithPropertyValues[] { NodeWithPropertyValues( 40L, "Aaron" ), NodeWithPropertyValues( 41L, "Bonbon" ), NodeWithPropertyValues( 48L, "Evon" ), NodeWithPropertyValues( 46L, "Macron" ), NodeWithPropertyValues( 45L, "Ton" ) };

					// THEN
					outerInstance.AssertContains( indexOrder, changes, changesWithValues, expected );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByContainsWhenThereArePartiallyMatchingNewNodes()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByContainsWhenThereArePartiallyMatchingNewNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Aaron").withAdded(41L, "Agatha").withAdded(42L, "Andreas").withAdded(43L, "Andrea").withAdded(44L, "Aristotle").withAdded(45L, "Barbara").withAdded(46L, "Barbarella").withAdded(47L, "Cinderella").build();

					// WHEN
					IndexQuery.StringContainsPredicate indexQuery = IndexQuery.stringContains( outerInstance.index.Schema().PropertyId, stringValue("arbar") );
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForSuffixOrContains( state, outerInstance.index, indexQuery, IndexOrder.NONE );

					// THEN
					outerInstance.AssertContains( changes.Added, 45L, 46L );
					outerInstance.AssertContains( changesWithValues.Added, NodeWithPropertyValues( 45L, "Barbara" ), NodeWithPropertyValues( 46L, "Barbarella" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForContainsWithAscendingOrder()
			  internal virtual void ShouldComputeIndexUpdatesForContainsWithAscendingOrder()
			  {
					AssertRangeSeekByContainsForOrder( IndexOrder.ASCENDING );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForContainsWithDescendingOrder()
			  internal virtual void ShouldComputeIndexUpdatesForContainsWithDescendingOrder()
			  {
					AssertRangeSeekByContainsForOrder( IndexOrder.DESCENDING );
			  }

			  internal virtual void AssertRangeSeekByContainsForOrder( IndexOrder indexOrder )
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Smashing").withAdded(41L, "Bashley").withAdded(42L, "Crasch").withAdded(43L, "Mayonnaise").withAdded(44L, "Seashell").withAdded(45L, "Ton").withAdded(46L, "The Flash").withAdded(47L, "Strayhound").withAdded(48L, "Trashy").withAdded(49L, "Andromeda").build();

					// WHEN
					IndexQuery indexQuery = IndexQuery.stringContains( outerInstance.index.Schema().PropertyId, stringValue("ash") );
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( state, outerInstance.index, indexQuery, indexOrder );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForSuffixOrContains( state, outerInstance.index, indexQuery, indexOrder );

					NodeWithPropertyValues[] expected = new NodeWithPropertyValues[] { NodeWithPropertyValues( 41L, "Bashley" ), NodeWithPropertyValues( 44L, "Seashell" ), NodeWithPropertyValues( 40L, "Smashing" ), NodeWithPropertyValues( 46L, "The Flash" ), NodeWithPropertyValues( 48L, "Trashy" ) };

					// THEN
					outerInstance.AssertContains( indexOrder, changes, changesWithValues, expected );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class Prefix
		 internal class Prefix
		 {
			 private readonly TxStateIndexChangesTest _outerInstance;

			 public Prefix( TxStateIndexChangesTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByPrefixWhenThereAreNoMatchingNodes()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByPrefixWhenThereAreNoMatchingNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "value42").withAdded(43L, "value43").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForRangeSeekByPrefix( state, outerInstance.index, stringValue( "eulav" ), IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForRangeSeekByPrefix( state, outerInstance.index, stringValue( "eulav" ), IndexOrder.NONE );

					// THEN
					assertTrue( changes.Added.Empty );
					assertFalse( changesWithValues.Added.GetEnumerator().hasNext() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByPrefix()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByPrefix()
			  {
					AssertRangeSeekByPrefixForOrder( IndexOrder.NONE );
					AssertRangeSeekByPrefixForOrder( IndexOrder.ASCENDING );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByPrefixWithDescendingOrder()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByPrefixWithDescendingOrder()
			  {
					AssertRangeSeekByPrefixForOrder( IndexOrder.DESCENDING );
			  }

			  internal virtual void AssertRangeSeekByPrefixForOrder( IndexOrder indexOrder )
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(40L, "Aaron").withAdded(41L, "Agatha").withAdded(42L, "Andreas").withAdded(43L, "Barbarella").withAdded(44L, "Andrea").withAdded(45L, "Aristotle").withAdded(46L, "Barbara").withAdded(47L, "Andy").withAdded(48L, "Cinderella").withAdded(49L, "Andromeda").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForRangeSeekByPrefix( state, outerInstance.index, stringValue( "And" ), indexOrder );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForRangeSeekByPrefix( state, outerInstance.index, stringValue( "And" ), indexOrder );

					NodeWithPropertyValues[] expected = new NodeWithPropertyValues[] { NodeWithPropertyValues( 44L, "Andrea" ), NodeWithPropertyValues( 42L, "Andreas" ), NodeWithPropertyValues( 49L, "Andromeda" ), NodeWithPropertyValues( 47L, "Andy" ) };

					// THEN
					outerInstance.AssertContains( indexOrder, changes, changesWithValues, expected );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeIndexUpdatesForRangeSeekByPrefixWhenThereAreNonStringNodes()
			  internal virtual void ShouldComputeIndexUpdatesForRangeSeekByPrefixWhenThereAreNonStringNodes()
			  {
					// GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = new TxStateBuilder().withAdded(42L, "barry").withAdded(44L, 101L).withAdded(43L, "bar").build();
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "barry").withAdded(44L, 101L).withAdded(43L, "bar").build();

					// WHEN
					AddedAndRemoved changes = TxStateIndexChanges.IndexUpdatesForRangeSeekByPrefix( state, outerInstance.index, stringValue( "bar" ), IndexOrder.NONE );

					// THEN
					outerInstance.AssertContainsInOrder( changes.Added, 43L, 42L );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class CompositeIndex
		 internal class CompositeIndex
		 {
			 private readonly TxStateIndexChangesTest _outerInstance;

			 public CompositeIndex( TxStateIndexChangesTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly IndexDescriptor CompositeIndexConflict = TestIndexDescriptorFactory.forLabel( 1, 1, 2 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSeekOnAnEmptyTxState()
			  internal virtual void ShouldSeekOnAnEmptyTxState()
			  {
					// GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState state = org.mockito.Mockito.mock(org.neo4j.storageengine.api.txstate.ReadableTransactionState.class);
					ReadableTransactionState state = Mockito.mock( typeof( ReadableTransactionState ) );

					// WHEN
					AddedAndRemoved changes = indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( "43value1", "43value2" ) );

					// THEN
					assertTrue( changes.Empty );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldScanWhenThereAreNewNodes()
			  internal virtual void ShouldScanWhenThereAreNewNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "42value1", "42value2").withAdded(43L, "43value1", "43value2").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForScan( state, CompositeIndexConflict, IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForScan( state, CompositeIndexConflict, IndexOrder.NONE );

					// THEN
					outerInstance.AssertContains( changes.Added, 42L, 43L );
					outerInstance.AssertContains( changesWithValues.Added, NodeWithPropertyValues( 42L, "42value1", "42value2" ), NodeWithPropertyValues( 43L, "43value1", "43value2" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSeekWhenThereAreNewStringNodes()
			  internal virtual void ShouldSeekWhenThereAreNewStringNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "42value1", "42value2").withAdded(43L, "43value1", "43value2").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( "43value1", "43value2" ) );

					// THEN
					outerInstance.AssertContains( changes.Added, 43L );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSeekWhenThereAreNewNumberNodes()
			  internal virtual void ShouldSeekWhenThereAreNewNumberNodes()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, 42001.0, 42002.0).withAdded(43L, 43001.0, 43002.0).build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( 43001.0, 43002.0 ) );

					// THEN
					outerInstance.AssertContains( changes.Added, 43L );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMixedAddsAndRemovesEntry()
			  internal virtual void ShouldHandleMixedAddsAndRemovesEntry()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "42value1", "42value2").withAdded(43L, "43value1", "43value2").withRemoved(43L, "43value1", "43value2").withRemoved(44L, "44value1", "44value2").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForScan( state, CompositeIndexConflict, IndexOrder.NONE );
					AddedWithValuesAndRemoved changesWithValues = indexUpdatesWithValuesForScan( state, CompositeIndexConflict, IndexOrder.NONE );

					// THEN
					outerInstance.AssertContains( changes.Added, 42L );
					outerInstance.AssertContains( changesWithValues.Added, NodeWithPropertyValues( 42L, "42value1", "42value2" ) );
					outerInstance.AssertContains( changes.Removed, 44L );
					outerInstance.AssertContains( changesWithValues.Removed, 44L );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSeekWhenThereAreManyEntriesWithTheSameValues()
			  internal virtual void ShouldSeekWhenThereAreManyEntriesWithTheSameValues()
			  {
					// GIVEN (note that 44 has the same properties as 43)
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(42L, "42value1", "42value2").withAdded(43L, "43value1", "43value2").withAdded(44L, "43value1", "43value2").build();

					// WHEN
					AddedAndRemoved changes = indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( "43value1", "43value2" ) );

					// THEN
					outerInstance.AssertContains( changes.Added, 43L, 44L );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSeekInComplexMix()
			  internal virtual void ShouldSeekInComplexMix()
			  {
					// GIVEN
					ReadableTransactionState state = ( new TxStateBuilder() ).WithAdded(10L, "hi", 3).withAdded(11L, 9L, 33L).withAdded(12L, "sneaker", false).withAdded(13L, new int[]{ 10, 100 }, "array-buddy").withAdded(14L, 40.1, 40.2).build();

					// THEN
					outerInstance.AssertContains( indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( "hi", 3 ) ).Added, 10L );
					outerInstance.AssertContains( indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( 9L, 33L ) ).Added, 11L );
					outerInstance.AssertContains( indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( "sneaker", false ) ).Added, 12L );
					outerInstance.AssertContains( indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( new int[]{ 10, 100 }, "array-buddy" ) ).Added, 13L );
					outerInstance.AssertContains( indexUpdatesForSeek( state, CompositeIndexConflict, ValueTuple.of( 40.1, 40.2 ) ).Added, 14L );
			  }

		 }

		 private void AssertContains( IndexOrder indexOrder, AddedAndRemoved changes, AddedWithValuesAndRemoved changesWithValues, NodeWithPropertyValues[] expected )
		 {
			  if ( indexOrder == IndexOrder.DESCENDING )
			  {
					ArrayUtils.reverse( expected );
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  long[] expectedNodeIds = java.util.expected.Select( NodeWithPropertyValues::getNodeId ).ToArray();

			  if ( indexOrder == IndexOrder.NONE )
			  {
					AssertContains( changes.Added, expectedNodeIds );
					AssertContains( changesWithValues.Added, expected );
			  }
			  else
			  {
					AssertContainsInOrder( changes.Added, expectedNodeIds );
					AssertContainsInOrder( changesWithValues.Added, expected );
			  }
		 }

		 private static NodeWithPropertyValues NodeWithPropertyValues( long nodeId, params object[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new NodeWithPropertyValues( nodeId, java.util.values.Select( ValueUtils.of ).ToArray( Value[]::new ) );
		 }

		 private void AssertContains( LongIterable iterable, params long[] nodeIds )
		 {
			  assertEquals( newSetWith( nodeIds ), LongSets.immutable.ofAll( iterable ) );
		 }

		 private void AssertContains( IEnumerable<NodeWithPropertyValues> iterable, params NodeWithPropertyValues[] expected )
		 {
			  assertEquals( UnifiedSet.newSetWith( expected ), UnifiedSet.newSet( iterable ) );
		 }

		 private void AssertContainsInOrder( LongIterable iterable, params long[] nodeIds )
		 {
			  assertThat( Arrays.asList( iterable.toArray() ), contains(nodeIds) );
		 }

		 private void AssertContainsInOrder( IEnumerable<NodeWithPropertyValues> iterable, params NodeWithPropertyValues[] expected )
		 {
			  if ( expected.Length == 0 )
			  {
					assertThat( iterable, emptyIterable() );
			  }
			  else
			  {
					assertThat( iterable, contains( expected ) );
			  }
		 }

		 private class TxStateBuilder
		 {
			  internal IDictionary<ValueTuple, MutableLongDiffSetsImpl> Updates = new Dictionary<ValueTuple, MutableLongDiffSetsImpl>();

			  internal virtual TxStateBuilder WithAdded( long id, params object[] value )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.ValueTuple valueTuple = org.neo4j.values.storable.ValueTuple.of((Object[]) value);
					ValueTuple valueTuple = ValueTuple.of( ( object[] ) value );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSetsImpl changes = updates.computeIfAbsent(valueTuple, ignore -> new org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSetsImpl(org.neo4j.kernel.impl.util.collection.OnHeapCollectionsFactory.INSTANCE));
					MutableLongDiffSetsImpl changes = Updates.computeIfAbsent( valueTuple, ignore => new MutableLongDiffSetsImpl( OnHeapCollectionsFactory.INSTANCE ) );
					changes.Add( id );
					return this;
			  }

			  internal virtual TxStateBuilder WithRemoved( long id, params object[] value )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.ValueTuple valueTuple = org.neo4j.values.storable.ValueTuple.of((Object[]) value);
					ValueTuple valueTuple = ValueTuple.of( ( object[] ) value );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSetsImpl changes = updates.computeIfAbsent(valueTuple, ignore -> new org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSetsImpl(org.neo4j.kernel.impl.util.collection.OnHeapCollectionsFactory.INSTANCE));
					MutableLongDiffSetsImpl changes = Updates.computeIfAbsent( valueTuple, ignore => new MutableLongDiffSetsImpl( OnHeapCollectionsFactory.INSTANCE ) );
					changes.Remove( id );
					return this;
			  }

			  internal virtual ReadableTransactionState Build()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.ReadableTransactionState mock = org.mockito.Mockito.mock(org.neo4j.storageengine.api.txstate.ReadableTransactionState.class);
					ReadableTransactionState mock = Mockito.mock( typeof( ReadableTransactionState ) );
					doReturn( new UnmodifiableMap<>( Updates ) ).when( mock ).getIndexUpdates( any( typeof( SchemaDescriptor ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.TreeMap<org.neo4j.values.storable.ValueTuple, org.neo4j.kernel.impl.util.diffsets.MutableLongDiffSetsImpl> sortedMap = new java.util.TreeMap<>(org.neo4j.values.storable.ValueTuple.COMPARATOR);
					SortedDictionary<ValueTuple, MutableLongDiffSetsImpl> sortedMap = new SortedDictionary<ValueTuple, MutableLongDiffSetsImpl>( ValueTuple.COMPARATOR );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					sortedMap.putAll( Updates );
					doReturn( sortedMap ).when( mock ).getSortedIndexUpdates( any( typeof( SchemaDescriptor ) ) );
					return mock;
			  }
		 }

	}

}