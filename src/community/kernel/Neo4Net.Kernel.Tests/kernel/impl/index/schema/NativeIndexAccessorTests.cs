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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Graphdb;
	using IndexCapability = Neo4Net.Internal.Kernel.Api.IndexCapability;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexValueCapability = Neo4Net.Internal.Kernel.Api.IndexValueCapability;
	using IndexNotApplicableKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;
	using SimpleNodeValueClient = Neo4Net.Storageengine.Api.schema.SimpleNodeValueClient;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.alwaysTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.@in;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asUniqueSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.remove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.IndexUpdateMode.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ValueCreatorUtil.countUniqueValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.of;

	/// <summary>
	/// Tests for
	/// <ul>
	/// <li><seealso cref="NumberIndexAccessor"/></li>
	/// <li><seealso cref="NativeIndexUpdater"/></li>
	/// <li><seealso cref="NumberIndexReader"/></li>
	/// </ul>
	/// </summary>
	public abstract class NativeIndexAccessorTests<KEY, VALUE> : NativeIndexTestUtil<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private NativeIndexAccessor<KEY, VALUE> _accessor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupAccessor() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupAccessor()
		 {
			  _accessor = MakeAccessor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract NativeIndexAccessor<KEY,VALUE> makeAccessor() throws java.io.IOException;
		 internal abstract NativeIndexAccessor<KEY, VALUE> MakeAccessor();

		 internal abstract IndexCapability IndexCapability();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeAccessor()
		 public virtual void CloseAccessor()
		 {
			  _accessor.Dispose();
		 }

		 // UPDATER

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCloseWithoutCallsToProcess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleCloseWithoutCallsToProcess()
		 {
			  // given
			  IndexUpdater updater = _accessor.newUpdater( ONLINE );

			  // when
			  updater.Close();

			  // then
			  // ... should be fine
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processMustThrowAfterClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessMustThrowAfterClose()
		 {
			  // given
			  IndexUpdater updater = _accessor.newUpdater( ONLINE );
			  updater.Close();

			  // then
			  Expected.expect( typeof( System.InvalidOperationException ) );

			  // when
			  updater.Process( SimpleUpdate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexAdd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexAdd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  using ( IndexUpdater updater = _accessor.newUpdater( ONLINE ) )
			  {
					// when
					ProcessAll( updater, updates );
			  }

			  // then
			  ForceAndCloseAccessor();
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexChange()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );
			  IEnumerator<IndexEntryUpdate<IndexDescriptor>> generator = filter( SkipExisting( updates ), valueCreatorUtil.randomUpdateGenerator( random ) );

			  for ( int i = 0; i < updates.Length; i++ )
			  {
					IndexEntryUpdate<IndexDescriptor> update = updates[i];
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Value newValue = generator.next().values()[0];
					updates[i] = change( update.EntityId, indexDescriptor, update.Values()[0], newValue );
			  }

			  // when
			  ProcessAll( updates );

			  // then
			  ForceAndCloseAccessor();
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexRemove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexRemove()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  for ( int i = 0; i < updates.Length; i++ )
			  {
					// when
					IndexEntryUpdate<IndexDescriptor> update = updates[i];
					IndexEntryUpdate<IndexDescriptor> remove = remove( update.EntityId, indexDescriptor, update.Values() );
					ProcessAll( remove );
					ForceAndCloseAccessor();

					// then
					verifyUpdates( Arrays.copyOfRange( updates, i + 1, updates.Length ) );
					SetupAccessor();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRandomUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRandomUpdates()
		 {
			  // given
			  ISet<IndexEntryUpdate<IndexDescriptor>> expectedData = new HashSet<IndexEntryUpdate<IndexDescriptor>>();
			  IEnumerator<IndexEntryUpdate<IndexDescriptor>> newDataGenerator = valueCreatorUtil.randomUpdateGenerator( random );

			  // when
			  int rounds = 50;
			  for ( int round = 0; round < rounds; round++ )
			  {
					// generate a batch of updates (add, change, remove)
					IndexEntryUpdate<IndexDescriptor>[] batch = GenerateRandomUpdates( expectedData, newDataGenerator, random.Next( 5, 20 ), ( float ) round / rounds * 2 );
					// apply to tree
					ProcessAll( batch );
					// apply to expectedData
					ApplyUpdatesToExpectedData( expectedData, batch );
					// verifyUpdates
					ForceAndCloseAccessor();
					//noinspection unchecked
					verifyUpdates( expectedData.toArray( new IndexEntryUpdate[0] ) );
					SetupAccessor();
			  }
		 }

		 // === READER ===

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnZeroCountForEmptyIndex()
		 public virtual void ShouldReturnZeroCountForEmptyIndex()
		 {
			  // given
			  using ( IndexReader reader = _accessor.newReader() )
			  {
					// when
					IndexEntryUpdate<IndexDescriptor> update = valueCreatorUtil.randomUpdateGenerator( random ).next();
					long count = reader.CountIndexedNodes( 123, valueCreatorUtil.indexDescriptor.properties(), update.Values()[0] );

					// then
					assertEquals( 0, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCountOneForExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCountOneForExistingData()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  // when
			  using ( IndexReader reader = _accessor.newReader() )
			  {
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 long count = reader.CountIndexedNodes( update.EntityId, valueCreatorUtil.indexDescriptor.properties(), update.Values() );

						 // then
						 assertEquals( 1, count );
					}

					// and when
					IEnumerator<IndexEntryUpdate<IndexDescriptor>> generator = filter( SkipExisting( updates ), valueCreatorUtil.randomUpdateGenerator( random ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					long count = reader.CountIndexedNodes( 123, valueCreatorUtil.indexDescriptor.properties(), generator.next().values()[0] );

					// then
					assertEquals( 0, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCountZeroForMismatchingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCountZeroForMismatchingData()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates();
			  ProcessAll( updates );

			  // when
			  IndexReader reader = _accessor.newReader();

			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					int[] propKeys = valueCreatorUtil.indexDescriptor.properties();
					long countWithMismatchingData = reader.CountIndexedNodes( update.EntityId + 1, propKeys, update.Values() );
					long countWithNonExistentEntityId = reader.CountIndexedNodes( NON_EXISTENT_ENTITY_ID, propKeys, update.Values() );
					long countWithNonExistentValue = reader.CountIndexedNodes( update.EntityId, propKeys, GenerateUniqueValue( updates ) );

					// then
					assertEquals( 0, countWithMismatchingData );
					assertEquals( 0, countWithNonExistentEntityId );
					assertEquals( 0, countWithNonExistentValue );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAllEntriesForExistsPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnAllEntriesForExistsPredicate()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, IndexQuery.exists( 0 ) );

			  // then
			  AssertEntityIdHits( ExtractEntityIds( updates, alwaysTrue() ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoEntriesForExistsPredicateForEmptyIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoEntriesForExistsPredicateForEmptyIndex()
		 {
			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, IndexQuery.exists( 0 ) );

			  // then
			  long[] actual = PrimitiveLongCollections.asArray( result );
			  assertEquals( 0, actual.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForExactPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForExactPredicate()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					Value value = update.Values()[0];
					LongIterator result = Query( reader, IndexQuery.exact( 0, value ) );
					AssertEntityIdHits( ExtractEntityIds( updates, @in( value ) ), result );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoEntriesForMismatchingExactPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoEntriesForMismatchingExactPredicate()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  object value = GenerateUniqueValue( updates );
			  LongIterator result = Query( reader, IndexQuery.exact( 0, value ) );
			  AssertEntityIdHits( EMPTY_LONG_ARRAY, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndExclusiveEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndExclusiveEnd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), true, ValueOf( updates[updates.Length - 1] ), false ) );
			  AssertEntityIdHits( ExtractEntityIds( Arrays.copyOf( updates, updates.Length - 1 ), alwaysTrue() ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd( updates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 internal virtual void ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), true, ValueOf( updates[updates.Length - 1] ), true ) );
			  AssertEntityIdHits( ExtractEntityIds( updates, alwaysTrue() ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndExclusiveEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndExclusiveEnd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), false, ValueOf( updates[updates.Length - 1] ), false ) );
			  AssertEntityIdHits( ExtractEntityIds( Arrays.copyOfRange( updates, 1, updates.Length - 1 ), alwaysTrue() ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndInclusiveEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndInclusiveEnd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), false, ValueOf( updates[updates.Length - 1] ), true ) );
			  AssertEntityIdHits( ExtractEntityIds( Arrays.copyOfRange( updates, 1, updates.Length ), alwaysTrue() ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoEntriesForRangePredicateOutsideAnyMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoEntriesForRangePredicateOutsideAnyMatch()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  valueCreatorUtil.sort( updates );
			  ProcessAll( updates[0], updates[1], updates[updates.Length - 1], updates[updates.Length - 2] );

			  // when
			  IndexReader reader = _accessor.newReader();
			  LongIterator result = Query( reader, valueCreatorUtil.rangeQuery( ValueOf( updates[2] ), true, ValueOf( updates[updates.Length - 3] ), true ) );
			  AssertEntityIdHits( EMPTY_LONG_ARRAY, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000L) public void mustHandleNestedQueries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustHandleNestedQueries()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  MustHandleNestedQueries( updates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void mustHandleNestedQueries(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 internal virtual void MustHandleNestedQueries( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();

			  IndexQuery outerQuery = valueCreatorUtil.rangeQuery( ValueOf( updates[2] ), true, ValueOf( updates[3] ), true );
			  IndexQuery innerQuery = valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), true, ValueOf( updates[1] ), true );

			  long[] expectedOuter = new long[]{ EntityIdOf( updates[2] ), EntityIdOf( updates[3] ) };
			  long[] expectedInner = new long[]{ EntityIdOf( updates[0] ), EntityIdOf( updates[1] ) };

			  LongIterator outerIter = Query( reader, outerQuery );
			  ICollection<long> outerResult = new List<long>();
			  while ( outerIter.hasNext() )
			  {
					outerResult.Add( outerIter.next() );
					LongIterator innerIter = Query( reader, innerQuery );
					AssertEntityIdHits( expectedInner, innerIter );
			  }
			  AssertEntityIdHits( expectedOuter, outerResult );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandleMultipleNestedQueries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustHandleMultipleNestedQueries()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  MustHandleMultipleNestedQueries( updates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void mustHandleMultipleNestedQueries(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 internal virtual void MustHandleMultipleNestedQueries( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );

			  // when
			  IndexReader reader = _accessor.newReader();

			  IndexQuery query1 = valueCreatorUtil.rangeQuery( ValueOf( updates[4] ), true, ValueOf( updates[5] ), true );
			  IndexQuery query2 = valueCreatorUtil.rangeQuery( ValueOf( updates[2] ), true, ValueOf( updates[3] ), true );
			  IndexQuery query3 = valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), true, ValueOf( updates[1] ), true );

			  long[] expected1 = new long[]{ EntityIdOf( updates[4] ), EntityIdOf( updates[5] ) };
			  long[] expected2 = new long[]{ EntityIdOf( updates[2] ), EntityIdOf( updates[3] ) };
			  long[] expected3 = new long[]{ EntityIdOf( updates[0] ), EntityIdOf( updates[1] ) };

			  ICollection<long> result1 = new List<long>();
			  LongIterator iter1 = Query( reader, query1 );
			  while ( iter1.hasNext() )
			  {
					result1.Add( iter1.next() );

					ICollection<long> result2 = new List<long>();
					LongIterator iter2 = Query( reader, query2 );
					while ( iter2.hasNext() )
					{
						 result2.Add( iter2.next() );

						 ICollection<long> result3 = new List<long>();
						 LongIterator iter3 = Query( reader, query3 );
						 while ( iter3.hasNext() )
						 {
							  result3.Add( iter3.next() );
						 }
						 AssertEntityIdHits( expected3, result3 );
					}
					AssertEntityIdHits( expected2, result2 );
			  }
			  AssertEntityIdHits( expected1, result1 );
		 }

		 private long EntityIdOf( IndexEntryUpdate<IndexDescriptor> update )
		 {
			  return update.EntityId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleConsecutiveUpdaters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleConsecutiveUpdaters()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();

			  // when
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					using ( IndexUpdater updater = _accessor.newUpdater( ONLINE ) )
					{
						 updater.Process( update );
					}
			  }

			  // then
			  ForceAndCloseAccessor();
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requestForSecondUpdaterMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RequestForSecondUpdaterMustThrow()
		 {
			  // given
			  using ( IndexUpdater ignored = _accessor.newUpdater( ONLINE ) )
			  {
					// then
					Expected.expect( typeof( System.InvalidOperationException ) );

					// when
					_accessor.newUpdater( ONLINE );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropShouldDeleteAndCloseIndex()
		 public virtual void DropShouldDeleteAndCloseIndex()
		 {
			  // given
			  assertFilePresent();

			  // when
			  _accessor.drop();

			  // then
			  assertFileNotPresent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceShouldCheckpointTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ForceShouldCheckpointTree()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] data = SomeUpdatesSingleType();
			  ProcessAll( data );

			  // when
			  _accessor.force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  _accessor.Dispose();

			  // then
			  verifyUpdates( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void closeShouldCloseTreeWithoutCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseShouldCloseTreeWithoutCheckpoint()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] data = SomeUpdatesSingleType();
			  ProcessAll( data );

			  // when
			  _accessor.Dispose();

			  // then
			  verifyUpdates( new IndexEntryUpdate[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotFilesShouldReturnIndexFile()
		 public virtual void SnapshotFilesShouldReturnIndexFile()
		 {
			  // when
			  ResourceIterator<File> files = _accessor.snapshotFiles();

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( Files.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( IndexFile, Files.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( Files.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSampleIndex()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );
			  using ( IndexReader reader = _accessor.newReader(), IndexSampler sampler = reader.CreateSampler() )
			  {
					// when
					IndexSample sample = sampler.SampleIndex();

					// then
					assertEquals( updates.Length, sample.IndexSize() );
					assertEquals( updates.Length, sample.SampleSize() );
					assertEquals( countUniqueValues( updates ), sample.UniqueValues() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readingAfterDropShouldThrow()
		 public virtual void ReadingAfterDropShouldThrow()
		 {
			  // given
			  _accessor.drop();

			  // then
			  Expected.expect( typeof( System.InvalidOperationException ) );

			  // when
			  _accessor.newReader();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writingAfterDropShouldThrow()
		 public virtual void WritingAfterDropShouldThrow()
		 {
			  // given
			  _accessor.drop();

			  // then
			  Expected.expect( typeof( System.InvalidOperationException ) );

			  // when
			  _accessor.newUpdater( IndexUpdateMode.ONLINE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readingAfterCloseShouldThrow()
		 public virtual void ReadingAfterCloseShouldThrow()
		 {
			  // given
			  _accessor.Dispose();

			  // then
			  Expected.expect( typeof( System.InvalidOperationException ) );

			  // when
			  _accessor.newReader();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writingAfterCloseShouldThrow()
		 public virtual void WritingAfterCloseShouldThrow()
		 {
			  // given
			  _accessor.Dispose();

			  // then
			  Expected.expect( typeof( System.InvalidOperationException ) );

			  // when
			  _accessor.newUpdater( IndexUpdateMode.ONLINE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAllEntriesInAllEntriesReader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAllEntriesInAllEntriesReader()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleType();
			  ProcessAll( updates );

			  // when
			  ISet<long> ids = asUniqueSet( _accessor.newAllEntriesReader() );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ISet<long> expectedIds = Stream.of( updates ).map( IndexEntryUpdate::getEntityId ).collect( Collectors.toCollection( HashSet<object>::new ) );
			  assertEquals( expectedIds, ids );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNoEntriesInAllEntriesReaderOnEmptyIndex()
		 public virtual void ShouldSeeNoEntriesInAllEntriesReaderOnEmptyIndex()
		 {
			  // when
			  ISet<long> ids = asUniqueSet( _accessor.newAllEntriesReader() );

			  // then
			  ISet<long> expectedIds = Collections.emptySet();
			  assertEquals( expectedIds, ids );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeFilteredEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeFilteredEntries()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = SomeUpdatesSingleTypeNoDuplicates( SupportedTypesExcludingNonOrderable() );
			  ProcessAll( updates );
			  valueCreatorUtil.sort( updates );
			  IndexReader reader = _accessor.newReader();

			  // when
			  NodeValueIterator iter = new NodeValueIterator();
			  IndexQuery.ExactPredicate filter = IndexQuery.exact( 0, ValueOf( updates[1] ) );
			  IndexQuery rangeQuery = valueCreatorUtil.rangeQuery( ValueOf( updates[0] ), true, ValueOf( updates[2] ), true );
			  Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient filterClient = filterClient( iter, filter );
			  reader.Query( filterClient, IndexOrder.NONE, false, rangeQuery );

			  // then
			  assertTrue( iter.HasNext() );
			  assertEquals( EntityIdOf( updates[1] ), iter.Next() );
			  assertFalse( iter.HasNext() );
		 }

		 // <READER ordering>

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void respectIndexOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RespectIndexOrder()
		 {
			  // given
			  int nUpdates = 10000;
			  ValueType[] types = SupportedTypesExcludingNonOrderable();
			  IEnumerator<IndexEntryUpdate<IndexDescriptor>> randomUpdateGenerator = valueCreatorUtil.randomUpdateGenerator( random, types );
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] someUpdates = new IndexEntryUpdate[nUpdates];
			  for ( int i = 0; i < nUpdates; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					someUpdates[i] = randomUpdateGenerator.next();
			  }
			  ProcessAll( someUpdates );
			  Value[] allValues = valueCreatorUtil.extractValuesFromUpdates( someUpdates );

			  // when
			  using ( IndexReader reader = _accessor.newReader() )
			  {
					ValueGroup valueGroup = random.among( allValues ).valueGroup();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> supportedQuery = org.neo4j.internal.kernel.api.IndexQuery.range(0, valueGroup);
					IndexQuery.RangePredicate<object> supportedQuery = IndexQuery.range( 0, valueGroup );

					IndexOrder[] supportedOrders = IndexCapability().orderCapability(valueGroup.category());
					foreach ( IndexOrder supportedOrder in supportedOrders )
					{
						 if ( supportedOrder == IndexOrder.NONE )
						 {
							  continue;
						 }
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Value[] expectedValues = java.util.allValues.Where( v => v.valueGroup() == valueGroup ).ToArray(Value[]::new);
						 if ( supportedOrder == IndexOrder.ASCENDING )
						 {
							  Arrays.sort( expectedValues, Values.COMPARATOR );
						 }
						 if ( supportedOrder == IndexOrder.DESCENDING )
						 {
							  Arrays.sort( expectedValues, Values.COMPARATOR.reversed() );
						 }

						 SimpleNodeValueClient client = new SimpleNodeValueClient();
						 reader.Query( client, supportedOrder, true, supportedQuery );
						 int i = 0;
						 while ( client.Next() )
						 {
							  assertEquals( "values in order", expectedValues[i++], client.Values[0] );
						 }
						 assertEquals( "found all values", i, expectedValues.Length );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwForUnsupportedIndexOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowForUnsupportedIndexOrder()
		 {
			  // given
			  // Unsupported index order for query
			  using ( IndexReader reader = _accessor.newReader() )
			  {
					IndexOrder unsupportedOrder = IndexOrder.DESCENDING;
					IndexQuery.ExactPredicate unsupportedQuery = IndexQuery.exact( 0, PointValue.MAX_VALUE ); // <- Any spatial value would do

					// then
					Expected.expect( typeof( System.NotSupportedException ) );
					Expected.expectMessage( CoreMatchers.allOf( CoreMatchers.containsString( "unsupported order" ), CoreMatchers.containsString( unsupportedOrder.ToString() ), CoreMatchers.containsString(unsupportedQuery.ToString()) ) );

					// when
					reader.Query( new SimpleNodeValueClient(), unsupportedOrder, false, unsupportedQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getValues() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getValues()
		 {
			  // given
			  int nUpdates = 10000;
			  IEnumerator<IndexEntryUpdate<IndexDescriptor>> randomUpdateGenerator = valueCreatorUtil.randomUpdateGenerator( random );
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] someUpdates = new IndexEntryUpdate[nUpdates];
			  for ( int i = 0; i < nUpdates; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					someUpdates[i] = randomUpdateGenerator.next();
			  }
			  ProcessAll( someUpdates );
			  Value[] allValues = valueCreatorUtil.extractValuesFromUpdates( someUpdates );

			  // Pick one out of all added values and do a range query for the value group of that value
			  Value value = random.among( allValues );
			  ValueGroup valueGroup = value.ValueGroup();

			  IndexValueCapability valueCapability = IndexCapability().valueCapability(valueGroup.category());
			  if ( !valueCapability.Equals( IndexValueCapability.YES ) )
			  {
					// We don't need to do this test
					return;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> supportedQuery;
			  IndexQuery.RangePredicate<object> supportedQuery;
			  IList<Value> expectedValues;
			  if ( Values.isGeometryValue( value ) )
			  {
					// Unless it's a point value in which case we query for the specific coordinate reference system instead
					CoordinateReferenceSystem crs = ( ( PointValue ) value ).CoordinateReferenceSystem;
					supportedQuery = IndexQuery.range( 0, crs );
					expectedValues = java.util.allValues.Where( v => v.valueGroup() == ValueGroup.GEOMETRY ).Where(v => ((PointValue) v).CoordinateReferenceSystem == crs).ToList();
			  }
			  else
			  {
					supportedQuery = IndexQuery.range( 0, valueGroup );
					expectedValues = java.util.allValues.Where( v => v.valueGroup() == valueGroup ).ToList();
			  }

			  // when
			  using ( IndexReader reader = _accessor.newReader() )
			  {
						 SimpleNodeValueClient client = new SimpleNodeValueClient();
						 reader.Query( client, IndexOrder.NONE, true, supportedQuery );

						 // then
						 while ( client.Next() )
						 {
							  Value foundValue = client.Values[0];
							  assertTrue( "found value that was not expected " + foundValue, expectedValues.Remove( foundValue ) );
						 }
						 assertThat( "did not find all expected values", expectedValues.Count, CoreMatchers.@is( 0 ) );
			  }
		 }

		 // </READER ordering>

		 private Value GenerateUniqueValue( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  return filter( SkipExisting( updates ), valueCreatorUtil.randomUpdateGenerator( random ) ).next().values()[0];
		 }

		 private static System.Predicate<IndexEntryUpdate<IndexDescriptor>> SkipExisting( IndexEntryUpdate<IndexDescriptor>[] existing )
		 {
			  return update =>
			  {
				foreach ( IndexEntryUpdate<IndexDescriptor> e in existing )
				{
					 if ( e.values().SequenceEqual(update.values()) )
					 {
						  return false;
					 }
				}
				return true;
			  };
		 }

		 private Value ValueOf( IndexEntryUpdate<IndexDescriptor> update )
		 {
			  return update.Values()[0];
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient filterClient(final NodeValueIterator iter, final org.neo4j.internal.kernel.api.IndexQuery filter)
		 private Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient FilterClient( NodeValueIterator iter, IndexQuery filter )
		 {
			  return new IndexProgressor_NodeValueClientAnonymousInnerClass( this, iter, filter );
		 }

		 private class IndexProgressor_NodeValueClientAnonymousInnerClass : Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient
		 {
			 private readonly NativeIndexAccessorTests<KEY, VALUE> _outerInstance;

			 private Neo4Net.Kernel.Impl.Index.Schema.NodeValueIterator _iter;
			 private IndexQuery _filter;

			 public IndexProgressor_NodeValueClientAnonymousInnerClass( NativeIndexAccessorTests<KEY, VALUE> outerInstance, Neo4Net.Kernel.Impl.Index.Schema.NodeValueIterator iter, IndexQuery filter )
			 {
				 this.outerInstance = outerInstance;
				 this._iter = iter;
				 this._filter = filter;
			 }

			 public void initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
			 {
				  _iter.initialize( descriptor, progressor, query, indexOrder, needsValues );
			 }

			 public bool acceptNode( long reference, params Value[] values )
			 {
				  //noinspection SimplifiableIfStatement
				  if ( values.Length > 1 )
				  {
						return false;
				  }
				  return _filter.acceptsValue( values[0] ) && _iter.acceptNode( reference, values );
			 }

			 public bool needsValues()
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.eclipse.collections.api.iterator.LongIterator query(org.neo4j.storageengine.api.schema.IndexReader reader, org.neo4j.internal.kernel.api.IndexQuery query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 private LongIterator Query( IndexReader reader, IndexQuery query )
		 {
			  NodeValueIterator client = new NodeValueIterator();
			  reader.Query( client, IndexOrder.NONE, false, query );
			  return client;
		 }

		 private void AssertEntityIdHits( long[] expected, LongIterator result )
		 {
			  long[] actual = PrimitiveLongCollections.asArray( result );
			  AssertSameContent( expected, actual );
		 }

		 private void AssertEntityIdHits( long[] expected, ICollection<long> result )
		 {
			  long[] actual = new long[result.Count];
			  int index = 0;
			  foreach ( long? aLong in result )
			  {
					actual[index++] = aLong.Value;
			  }
			  AssertSameContent( expected, actual );
		 }

		 private void AssertSameContent( long[] expected, long[] actual )
		 {
			  Arrays.sort( actual );
			  Arrays.sort( expected );
			  assertArrayEquals( format( "Expected arrays to be equal but wasn't.%nexpected:%s%n  actual:%s%n", Arrays.ToString( expected ), Arrays.ToString( actual ) ), expected, actual );
		 }

		 private long[] ExtractEntityIds<T1>( IndexEntryUpdate<T1>[] updates, System.Predicate<Value> valueFilter )
		 {
			  long[] entityIds = new long[updates.Length];
			  int cursor = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					if ( valueFilter( update.Values()[0] ) )
					{
						 entityIds[cursor++] = update.EntityId;
					}
			  }
			  return Arrays.copyOf( entityIds, cursor );
		 }

		 private void ApplyUpdatesToExpectedData( ISet<IndexEntryUpdate<IndexDescriptor>> expectedData, IndexEntryUpdate<IndexDescriptor>[] batch )
		 {
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in batch )
			  {
					IndexEntryUpdate<IndexDescriptor> addition = null;
					IndexEntryUpdate<IndexDescriptor> removal = null;
					switch ( update.UpdateMode() )
					{
					case ADDED:
						 addition = valueCreatorUtil.add( update.EntityId, update.Values()[0] );
						 break;
					case CHANGED:
						 addition = valueCreatorUtil.add( update.EntityId, update.Values()[0] );
						 removal = valueCreatorUtil.add( update.EntityId, update.BeforeValues()[0] );
						 break;
					case REMOVED:
						 removal = valueCreatorUtil.add( update.EntityId, update.Values()[0] );
						 break;
					default:
						 throw new System.ArgumentException( update.UpdateMode().name() );
					}

					if ( removal != null )
					{
						 expectedData.remove( removal );
					}
					if ( addition != null )
					{
						 expectedData.Add( addition );
					}
			  }
		 }

		 private IndexEntryUpdate<IndexDescriptor>[] GenerateRandomUpdates( ISet<IndexEntryUpdate<IndexDescriptor>> expectedData, IEnumerator<IndexEntryUpdate<IndexDescriptor>> newDataGenerator, int count, float removeFactor )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates = new org.neo4j.kernel.api.index.IndexEntryUpdate[count];
			  IndexEntryUpdate<IndexDescriptor>[] updates = new IndexEntryUpdate[count];
			  float addChangeRatio = 0.5f;
			  for ( int i = 0; i < count; i++ )
			  {
					float factor = random.nextFloat();
					if ( expectedData.Count > 0 && factor < removeFactor )
					{
						 // remove something
						 IndexEntryUpdate<IndexDescriptor> toRemove = SelectRandomItem( expectedData );
						 updates[i] = remove( toRemove.EntityId, indexDescriptor, toRemove.Values() );
					}
					else if ( expectedData.Count > 0 && factor < ( 1 - removeFactor ) * addChangeRatio )
					{
						 // change
						 IndexEntryUpdate<IndexDescriptor> toChange = SelectRandomItem( expectedData );
						 // use the data generator to generate values, even if the whole update as such won't be used
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 IndexEntryUpdate<IndexDescriptor> updateContainingValue = newDataGenerator.next();
						 updates[i] = change( toChange.EntityId, indexDescriptor, toChange.Values(), updateContainingValue.Values() );
					}
					else
					{
						 // add
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 updates[i] = newDataGenerator.next();
					}
			  }
			  return updates;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor> selectRandomItem(java.util.Set<org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>> expectedData)
		 private IndexEntryUpdate<IndexDescriptor> SelectRandomItem( ISet<IndexEntryUpdate<IndexDescriptor>> expectedData )
		 {
			  return expectedData.toArray( new IndexEntryUpdate[0] )[random.Next( expectedData.Count )];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final void processAll(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>... updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void ProcessAll( params IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  using ( IndexUpdater updater = _accessor.newUpdater( ONLINE ) )
			  {
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 updater.Process( update );
					}
			  }
		 }

		 private void ForceAndCloseAccessor()
		 {
			  _accessor.force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  CloseAccessor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void processAll(org.neo4j.kernel.api.index.IndexUpdater updater, org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ProcessAll( IndexUpdater updater, IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					updater.Process( update );
			  }
		 }

		 private IndexEntryUpdate<IndexDescriptor> SimpleUpdate()
		 {
			  return IndexEntryUpdate.add( 0, indexDescriptor, of( 0 ) );
		 }

		 private IndexEntryUpdate<IndexDescriptor>[] SomeUpdatesSingleType()
		 {
			  ValueType type = random.randomValues().among(valueCreatorUtil.supportedTypes());
			  return valueCreatorUtil.someUpdates( random, new ValueType[]{ type }, true );
		 }

		 private IndexEntryUpdate<IndexDescriptor>[] SomeUpdatesSingleTypeNoDuplicates()
		 {
			  return SomeUpdatesSingleTypeNoDuplicates( valueCreatorUtil.supportedTypes() );
		 }

		 private IndexEntryUpdate<IndexDescriptor>[] SomeUpdatesSingleTypeNoDuplicates( ValueType[] types )
		 {
			  ValueType type;
			  do
			  {
					// Can not generate enough unique values of boolean
					type = random.randomValues().among(types);
			  } while ( type == ValueType.BOOLEAN );
			  return valueCreatorUtil.someUpdates( random, new ValueType[]{ type }, false );
		 }

		 private ValueType[] SupportedTypesExcludingNonOrderable()
		 {
			  return RandomValues.excluding( valueCreatorUtil.supportedTypes(), t => t.valueGroup == ValueGroup.GEOMETRY || t.valueGroup == ValueGroup.GEOMETRY_ARRAY || t == ValueType.STRING || t == ValueType.STRING_ARRAY );
		 }

		 // TODO: multiple query predicates... actually Lucene SimpleIndexReader only supports single predicate
		 //       so perhaps we should wait with this until we know exactly how this works and which combinations
		 //       that should be supported/optimized for.
	}

}