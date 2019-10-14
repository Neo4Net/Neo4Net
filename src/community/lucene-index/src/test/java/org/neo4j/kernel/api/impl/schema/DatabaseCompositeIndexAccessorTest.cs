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
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using Neo4Net.Kernel.Api.Index;
	using IndexQueryHelper = Neo4Net.Kernel.Api.Index.IndexQueryHelper;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.ThreadingRule.waitingWhileIn;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DatabaseCompositeIndexAccessorTest
	public class DatabaseCompositeIndexAccessorTest
	{
		 private const int PROP_ID1 = 1;
		 private const int PROP_ID2 = 2;
		 private static readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( 0, PROP_ID1, PROP_ID2 );
		 private static readonly Config _config = Config.defaults();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threading = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public static readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.function.IOFunction<org.neo4j.kernel.api.impl.index.storage.DirectoryFactory,LuceneIndexAccessor> accessorFactory;
		 public IOFunction<DirectoryFactory, LuceneIndexAccessor> AccessorFactory;

		 private LuceneIndexAccessor _accessor;
		 private readonly long _nodeId = 1;
		 private readonly long _nodeId2 = 2;
		 private readonly object[] _values = new object[] { "value1", "values2" };
		 private readonly object[] _values2 = new object[] { 40, 42 };
		 private Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory _dirFactory;
		 private static readonly IndexDescriptor _schemaIndexDescriptor = TestIndexDescriptorFactory.forLabel( 0, PROP_ID1, PROP_ID2 );
		 private static readonly IndexDescriptor _uniqueSchemaIndexDescriptor = TestIndexDescriptorFactory.uniqueForLabel( 1, PROP_ID1, PROP_ID2 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<org.neo4j.function.IOFunction<org.neo4j.kernel.api.impl.index.storage.DirectoryFactory,LuceneIndexAccessor>[]> implementations()
		 public static ICollection<IOFunction<DirectoryFactory, LuceneIndexAccessor>[]> Implementations()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File dir = new java.io.File("dir");
			  File dir = new File( "dir" );
			  return Arrays.asList(Arg(dirFactory1 =>
			  {
						  SchemaIndex index = LuceneSchemaIndexBuilder.Create( _schemaIndexDescriptor, _config ).withFileSystem( FileSystemRule.get() ).withDirectoryFactory(dirFactory1).withIndexRootFolder(new File(dir, "1")).build();

						  index.create();
						  index.open();
						  return new LuceneIndexAccessor( index, _descriptor );
			  }), Arg(dirFactory1 =>
			  {
						  SchemaIndex index = LuceneSchemaIndexBuilder.Create( _uniqueSchemaIndexDescriptor, _config ).withFileSystem( FileSystemRule.get() ).withDirectoryFactory(dirFactory1).withIndexRootFolder(new File(dir, "testIndex")).build();

						  index.create();
						  index.open();
						  return new LuceneIndexAccessor( index, _descriptor );
					 }));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static org.neo4j.function.IOFunction<org.neo4j.kernel.api.impl.index.storage.DirectoryFactory,LuceneIndexAccessor>[] arg(org.neo4j.function.IOFunction<org.neo4j.kernel.api.impl.index.storage.DirectoryFactory,LuceneIndexAccessor> foo)
		 private static IOFunction<DirectoryFactory, LuceneIndexAccessor>[] Arg( IOFunction<DirectoryFactory, LuceneIndexAccessor> foo )
		 {
			  return new IOFunction[]{ foo };
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
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values), add(nodeId2, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ), Add( _nodeId2, _values2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // WHEN
			  LongIterator results = reader.Query( IndexQuery.exists( PROP_ID1 ), IndexQuery.exists( PROP_ID2 ) );

			  // THEN
			  assertEquals( asSet( _nodeId, _nodeId2 ), PrimitiveLongCollections.toSet( results ) );
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID1, _values[0] ), exact( PROP_ID2, _values[1] ) ) ) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIndexReadersFromDifferentPointsInTimeCanSeeDifferentResults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIndexReadersFromDifferentPointsInTimeCanSeeDifferentResults()
		 {
			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ) } );
			  IndexReader firstReader = _accessor.newReader();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId2, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId2, _values2 ) } );
			  IndexReader secondReader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( firstReader.Query( exact( PROP_ID1, _values[0] ), exact( PROP_ID2, _values[1] ) ) ) );
			  assertEquals( asSet(), PrimitiveLongCollections.toSet(firstReader.Query(exact(PROP_ID1, _values2[0]), exact(PROP_ID2, _values2[1]))) );
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( secondReader.Query( exact( PROP_ID1, _values[0] ), exact( PROP_ID2, _values[1] ) ) ) );
			  assertEquals( asSet( _nodeId2 ), PrimitiveLongCollections.toSet( secondReader.Query( exact( PROP_ID1, _values2[0] ), exact( PROP_ID2, _values2[1] ) ) ) );
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
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values), add(nodeId2, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ), Add( _nodeId2, _values2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID1, _values[0] ), exact( PROP_ID2, _values[1] ) ) ) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canChangeExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanChangeExistingData()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ) } );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(change(nodeId, values, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Change( _nodeId, _values, _values2 ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID1, _values2[0] ), exact( PROP_ID2, _values2[1] ) ) ) );
			  assertEquals( emptySet(), PrimitiveLongCollections.toSet(reader.Query(exact(PROP_ID1, _values[0]), exact(PROP_ID2, _values[1]))) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canRemoveExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanRemoveExistingData()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values), add(nodeId2, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ), Add( _nodeId2, _values2 ) } );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(remove(nodeId, values)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Remove( _nodeId, _values ) } );
			  IndexReader reader = _accessor.newReader();

			  // THEN
			  assertEquals( asSet( _nodeId2 ), PrimitiveLongCollections.toSet( reader.Query( exact( PROP_ID1, _values2[0] ), exact( PROP_ID2, _values2[1] ) ) ) );
			  assertEquals( asSet(), PrimitiveLongCollections.toSet(reader.Query(exact(PROP_ID1, _values[0]), exact(PROP_ID2, _values[1]))) );
			  reader.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopSamplingWhenIndexIsDropped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopSamplingWhenIndexIsDropped()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: updateAndCommit(asList(add(nodeId, values), add(nodeId2, values2)));
			  UpdateAndCommit( new IList<IndexEntryUpdate<object>> { Add( _nodeId, _values ), Add( _nodeId2, _values2 ) } );

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
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object... values)
		 private IndexEntryUpdate<object> Add( long nodeId, params object[] values )
		 {
			  return IndexQueryHelper.add( nodeId, _schemaIndexDescriptor.schema(), values );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> remove(long nodeId, Object... values)
		 private IndexEntryUpdate<object> Remove( long nodeId, params object[] values )
		 {
			  return IndexQueryHelper.remove( nodeId, _schemaIndexDescriptor.schema(), values );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> change(long nodeId, Object[] valuesBefore, Object[] valuesAfter)
		 private IndexEntryUpdate<object> Change( long nodeId, object[] valuesBefore, object[] valuesAfter )
		 {
			  return IndexQueryHelper.change( nodeId, _schemaIndexDescriptor.schema(), valuesBefore, valuesAfter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateAndCommit(java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> nodePropertyUpdates) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void UpdateAndCommit<T1>( IList<T1> nodePropertyUpdates )
		 {
			  using ( IndexUpdater updater = _accessor.newUpdater( IndexUpdateMode.ONLINE ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : nodePropertyUpdates)
					foreach ( IndexEntryUpdate<object> update in nodePropertyUpdates )
					{
						 updater.Process( update );
					}
			  }
		 }
	}

}