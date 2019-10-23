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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Functions;
	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using Neo4Net.Kernel.Api.Index;
	using IndexQueryHelper = Neo4Net.Kernel.Api.Index.IndexQueryHelper;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.IndexQuery.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.concurrent.ThreadingRule.waitingWhileIn;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DatabaseIndexAccessorTest
	public class DatabaseIndexAccessorTest
	{
		 public const int PROP_ID = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.ThreadingRule threading = new org.Neo4Net.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public static readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor index;
		 public IndexDescriptor Index;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.Neo4Net.function.IOFunction<org.Neo4Net.kernel.api.impl.index.storage.DirectoryFactory,LuceneIndexAccessor> accessorFactory;
		 public IOFunction<DirectoryFactory, LuceneIndexAccessor> AccessorFactory;

		 private LuceneIndexAccessor _accessor;
		 private readonly long _nodeId = 1;
		 private readonly long _nodeId2 = 2;
		 private readonly object _value = "value";
		 private readonly object _value2 = 40;
		 private Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory _dirFactory;
		 private static readonly IndexDescriptor _generalIndex = TestIndexDescriptorFactory.forLabel( 0, PROP_ID );
		 private static readonly IndexDescriptor _uniqueIndex = TestIndexDescriptorFactory.uniqueForLabel( 1, PROP_ID );
		 private static readonly Config _config = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> implementations()
		 public static ICollection<object[]> Implementations()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File dir = new java.io.File("dir");
			  File dir = new File( "dir" );
			  return Arrays.asList(Arg(_generalIndex, dirFactory1 =>
			  {
						  SchemaIndex index = LuceneSchemaIndexBuilder.Create( _generalIndex, _config ).withFileSystem( FileSystemRule.get() ).withDirectoryFactory(dirFactory1).withIndexRootFolder(new File(dir, "1")).build();

						  index.create();
						  index.open();
						  return new LuceneIndexAccessor( index, _generalIndex );
			  }), Arg(_uniqueIndex, dirFactory1 =>
			  {
						  SchemaIndex index = LuceneSchemaIndexBuilder.Create( _uniqueIndex, _config ).withFileSystem( FileSystemRule.get() ).withDirectoryFactory(dirFactory1).withIndexRootFolder(new File(dir, "testIndex")).build();

						  index.create();
						  index.open();
						  return new LuceneIndexAccessor( index, _uniqueIndex );
					 }));
		 }

		 private static object[] Arg( IndexDescriptor index, IOFunction<DirectoryFactory, LuceneIndexAccessor> foo )
		 {
			  return new object[]{ index, foo };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _dirFactory = new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
			  _accessor = AccessorFactory.apply( _dirFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  _accessor.Dispose();
			  _dirFactory.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexReaderShouldSupportScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexReaderShouldSupportScan()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value), add(nodeId2, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ), Add( _nodeId2, _value2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // WHEN
			  LongIterator results = reader.Query( IndexQuery.exists( PROP_ID ) );

			  // THEN
			  assertEquals( asSet( _nodeId, _nodeId2 ), PrimitiveLongCollections.toSet( results ) );
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID, _value ) ) ) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexStringRangeQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexStringRangeQuery()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(PROP_ID, "A"), add(2, "B"), add(3, "C"), add(4, "")));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( PROP_ID, "A" ), Add( 2, "B" ), Add( 3, "C" ), Add( 4, "" ) } );

			  IndexReader reader = _accessor.newReader();

			  LongIterator rangeFromBInclusive = reader.Query( range( PROP_ID, "B", true, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( rangeFromBInclusive ), LongArrayMatcher.Of( 2, 3 ) );

			  LongIterator rangeFromANonInclusive = reader.Query( range( PROP_ID, "A", false, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( rangeFromANonInclusive ), LongArrayMatcher.Of( 2, 3 ) );

			  LongIterator emptyLowInclusive = reader.Query( range( PROP_ID, "", true, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( emptyLowInclusive ), LongArrayMatcher.Of( PROP_ID, 2, 3, 4 ) );

			  LongIterator emptyUpperNonInclusive = reader.Query( range( PROP_ID, "B", true, "", false ) );
			  assertThat( PrimitiveLongCollections.asArray( emptyUpperNonInclusive ), LongArrayMatcher.EmptyArrayMatcher() );

			  LongIterator emptyInterval = reader.Query( range( PROP_ID, "", true, "", true ) );
			  assertThat( PrimitiveLongCollections.asArray( emptyInterval ), LongArrayMatcher.Of( 4 ) );

			  LongIterator emptyAllNonInclusive = reader.Query( range( PROP_ID, "", false, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( emptyAllNonInclusive ), LongArrayMatcher.Of( PROP_ID, 2, 3 ) );

			  LongIterator nullNonInclusive = reader.Query( range( PROP_ID, ( string ) null, false, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( nullNonInclusive ), LongArrayMatcher.Of( PROP_ID, 2, 3, 4 ) );

			  LongIterator nullInclusive = reader.Query( range( PROP_ID, ( string ) null, false, null, false ) );
			  assertThat( PrimitiveLongCollections.asArray( nullInclusive ), LongArrayMatcher.Of( PROP_ID, 2, 3, 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexNumberRangeQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexNumberRangeQuery()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(1, 1), add(2, 2), add(3, 3), add(4, 4), add(5, Double.NaN)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( 1, 1 ), Add( 2, 2 ), Add( 3, 3 ), Add( 4, 4 ), Add( 5, Double.NaN ) } );

			  IndexReader reader = _accessor.newReader();

			  LongIterator rangeTwoThree = reader.Query( range( PROP_ID, 2, true, 3, true ) );
			  assertThat( PrimitiveLongCollections.asArray( rangeTwoThree ), LongArrayMatcher.Of( 2, 3 ) );

			  LongIterator infiniteMaxRange = reader.Query( range( PROP_ID, 2, true, long.MaxValue, true ) );
			  assertThat( PrimitiveLongCollections.asArray( infiniteMaxRange ), LongArrayMatcher.Of( 2, 3, 4 ) );

			  LongIterator infiniteMinRange = reader.Query( range( PROP_ID, long.MinValue, true, 3, true ) );
			  assertThat( PrimitiveLongCollections.asArray( infiniteMinRange ), LongArrayMatcher.Of( PROP_ID, 2, 3 ) );

			  LongIterator maxNanInterval = reader.Query( range( PROP_ID, 3, true, Double.NaN, true ) );
			  assertThat( PrimitiveLongCollections.asArray( maxNanInterval ), LongArrayMatcher.Of( 3, 4, 5 ) );

			  LongIterator minNanInterval = reader.Query( range( PROP_ID, Double.NaN, true, 5, true ) );
			  assertThat( PrimitiveLongCollections.asArray( minNanInterval ), LongArrayMatcher.EmptyArrayMatcher() );

			  LongIterator nanInterval = reader.Query( range( PROP_ID, Double.NaN, true, Double.NaN, true ) );
			  assertThat( PrimitiveLongCollections.asArray( nanInterval ), LongArrayMatcher.Of( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexReaderShouldHonorRepeatableReads() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexReaderShouldHonorRepeatableReads()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ) } );
			  IndexReader reader = _accessor.newReader();

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(remove(nodeId, value)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Remove( _nodeId, _value ) } );

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID, _value ) ) ) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIndexReadersFromDifferentPointsInTimeCanSeeDifferentResults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIndexReadersFromDifferentPointsInTimeCanSeeDifferentResults()
		 {
			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ) } );
			  IndexReader firstReader = _accessor.newReader();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId2, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId2, _value2 ) } );
			  IndexReader secondReader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( firstReader.Query( exact( PROP_ID, _value ) ) ) );
			  assertEquals( asSet(), PrimitiveLongCollections.toSet(firstReader.Query(exact(PROP_ID, _value2))) );
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( secondReader.Query( exact( PROP_ID, _value ) ) ) );
			  assertEquals( asSet( _nodeId2 ), PrimitiveLongCollections.toSet( secondReader.Query( exact( PROP_ID, _value2 ) ) ) );
			  firstReader.Close();
			  secondReader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddNewData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanAddNewData()
		 {
			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value), add(nodeId2, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ), Add( _nodeId2, _value2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID, _value ) ) ) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canChangeExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanChangeExistingData()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ) } );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(change(nodeId, value, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Change( _nodeId, _value, _value2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID, _value2 ) ) ) );
			  assertEquals( emptySet(), PrimitiveLongCollections.toSet(reader.Query(exact(PROP_ID, _value))) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canRemoveExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanRemoveExistingData()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value), add(nodeId2, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ), Add( _nodeId2, _value2 ) } );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(remove(nodeId, value)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Remove( _nodeId, _value ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId2 ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID, _value2 ) ) ) );
			  assertEquals( asSet(), PrimitiveLongCollections.toSet(reader.Query(exact(PROP_ID, _value))) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopSamplingWhenIndexIsDropped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopSamplingWhenIndexIsDropped()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, value), add(nodeId2, value2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _value ), Add( _nodeId2, _value2 ) } );

			  // when
			  IndexReader indexReader = _accessor.newReader(); // needs to be acquired before drop() is called
			  IndexSampler indexSampler = indexReader.CreateSampler();

			  Future<Void> drop = Threading.executeAndAwait((IOFunction<Void, Void>) nothing =>
			  {
				_accessor.drop();
				return nothing;
			  }, null, waitingWhileIn( typeof( TaskCoordinator ), "awaitCompletion" ), 3, SECONDS);

			  try
			  {
					  using ( IndexReader reader = indexReader, IndexSampler sampler = indexSampler )
					  {
						sampler.SampleIndex();
						fail( "expected exception" );
					  }
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					assertEquals( "Index dropped while sampling.", e.Message );
			  }
			  finally
			  {
					drop.get();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object value)
		 private IndexEntryUpdate<object> Add( long nodeId, object value )
		 {
			  return IndexQueryHelper.add( nodeId, Index.schema(), value );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> remove(long nodeId, Object value)
		 private IndexEntryUpdate<object> Remove( long nodeId, object value )
		 {
			  return IndexQueryHelper.remove( nodeId, Index.schema(), value );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> change(long nodeId, Object valueBefore, Object valueAfter)
		 private IndexEntryUpdate<object> Change( long nodeId, object valueBefore, object valueAfter )
		 {
			  return IndexQueryHelper.change( nodeId, Index.schema(), valueBefore, valueAfter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateAndCommit(java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> nodePropertyUpdates) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void UpdateAndCommit<T1>( IList<T1> nodePropertyUpdates )
		 {
			  using ( IndexUpdater updater = _accessor.newUpdater( IndexUpdateMode.ONLINE ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> update : nodePropertyUpdates)
					foreach ( IndexEntryUpdate<object> update in nodePropertyUpdates )
					{
						 updater.Process( update );
					}
			  }
		 }
	}

}