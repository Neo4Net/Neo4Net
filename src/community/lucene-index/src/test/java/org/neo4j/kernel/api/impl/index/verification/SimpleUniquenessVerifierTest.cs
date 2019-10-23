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
namespace Neo4Net.Kernel.Api.Impl.Index.verification
{
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Collector = org.apache.lucene.search.Collector;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using SearcherFactory = org.apache.lucene.search.SearcherFactory;
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using Directory = org.apache.lucene.store.Directory;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using IOUtils = Neo4Net.Io.IOUtils;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using LuceneDocumentStructure = Neo4Net.Kernel.Api.Impl.Schema.LuceneDocumentStructure;
	using SimpleUniquenessVerifier = Neo4Net.Kernel.Api.Impl.Schema.verification.SimpleUniquenessVerifier;
	using UniquenessVerifier = Neo4Net.Kernel.Api.Impl.Schema.verification.UniquenessVerifier;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.LuceneTestUtil.valueTupleList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class SimpleUniquenessVerifierTest
	internal class SimpleUniquenessVerifierTest
	{
		 private static readonly int[] _propertyKeyIds = new int[]{ 42 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 private DirectoryFactory _dirFactory;
		 private IndexWriter _writer;
		 private SearcherManager _searcherManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void initLuceneResources() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void InitLuceneResources()
		 {
			  _dirFactory = new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
			  Directory dir = _dirFactory.open( _testDir.directory( "test" ) );
			  _writer = new IndexWriter( dir, IndexWriterConfigs.standard() );
			  _searcherManager = new SearcherManager( _writer, true, new SearcherFactory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void closeLuceneResources() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseLuceneResources()
		 {
			  IOUtils.closeAll( _searcherManager, _writer, _dirFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void partitionSearcherIsClosed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PartitionSearcherIsClosed()
		 {
			  PartitionSearcher partitionSearcher = mock( typeof( PartitionSearcher ) );
			  SimpleUniquenessVerifier verifier = new SimpleUniquenessVerifier( partitionSearcher );

			  verifier.Dispose();

			  verify( partitionSearcher ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void populationVerificationNoDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PopulationVerificationNoDuplicates()
		 {
			  IList<object> data = new IList<object> { "string1", 42, 43, 44, 45L, ( sbyte ) 46, 47.0, ( float ) 48.1, "string2" };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertNoDuplicates( nodePropertyAccessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void populationVerificationOneDuplicate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PopulationVerificationOneDuplicate()
		 {
			  IList<object> data = new IList<object> { "cat", 21, 22, 23, 24L, ( sbyte ) 25, 26.0, ( float ) 22, "dog" };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertHasDuplicates( nodePropertyAccessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void populationVerificationManyDuplicate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PopulationVerificationManyDuplicate()
		 {
			  IList<object> data = new IList<object> { "dog", "cat", "dog", "dog", "dog", "dog" };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertHasDuplicates( nodePropertyAccessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updatesVerificationNoDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void UpdatesVerificationNoDuplicates()
		 {
			  IList<object> data = new IList<object> { "lucene", 1337975550, 43.10, 'a', 'b', 'c', ( sbyte ) 12 };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertNoDuplicatesCreated( nodePropertyAccessor, valueTupleList( 1337975550, 'c', ( sbyte ) 12 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updatesVerificationOneDuplicate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void UpdatesVerificationOneDuplicate()
		 {
			  IList<object> data = new IList<object> { "foo", "bar", "baz", 100, 200, 'q', 'u', 'x', "aa", 300, 'u', -100 };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertDuplicatesCreated( nodePropertyAccessor, valueTupleList( "aa", 'u', -100 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updatesVerificationManyDuplicate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void UpdatesVerificationManyDuplicate()
		 {
			  IList<object> data = new IList<object> { -99, 'a', -10.0, -99.99999, "apa", ( float ) - 99.99999, "mod", "div", "div", -10 };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  AssertDuplicatesCreated( nodePropertyAccessor, valueTupleList( ( float ) - 99.99999, 'a', -10, "div" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numericIndexVerificationNoDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NumericIndexVerificationNoDuplicates()
		 {
			  IList<object> data = new IList<object> { int.MaxValue - 2, int.MaxValue - 1, int.MaxValue };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  IndexSearcher indexSearcher = spy( _searcherManager.acquire() );
			  RunUniquenessVerification( nodePropertyAccessor, indexSearcher );

			  verify( indexSearcher, never() ).search(any(typeof(Query)), any(typeof(Collector)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numericIndexVerificationSomePossibleDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NumericIndexVerificationSomePossibleDuplicates()
		 {
			  IList<object> data = new IList<object> { 42, long.MaxValue - 1, long.MaxValue };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  IndexSearcher indexSearcher = spy( _searcherManager.acquire() );
			  RunUniquenessVerification( nodePropertyAccessor, indexSearcher );

			  verify( indexSearcher ).search( any( typeof( Query ) ), any( typeof( Collector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numericIndexVerificationSomeWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NumericIndexVerificationSomeWithDuplicates()
		 {
			  IList<object> data = new IList<object> { int.MaxValue, long.MaxValue, 42, long.MaxValue };
			  NodePropertyAccessor nodePropertyAccessor = NewPropertyAccessor( data );

			  Insert( data );

			  IndexSearcher indexSearcher = spy( _searcherManager.acquire() );
			  assertThrows( typeof( IndexEntryConflictException ), () => runUniquenessVerification(nodePropertyAccessor, indexSearcher) );
			  verify( indexSearcher ).search( any( typeof( Query ) ), any( typeof( Collector ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void runUniquenessVerification(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor, org.apache.lucene.search.IndexSearcher indexSearcher) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void RunUniquenessVerification( NodePropertyAccessor nodePropertyAccessor, IndexSearcher indexSearcher )
		 {
			  try
			  {
					PartitionSearcher partitionSearcher = mock( typeof( PartitionSearcher ) );
					when( partitionSearcher.IndexSearcher ).thenReturn( indexSearcher );

					using ( UniquenessVerifier verifier = new SimpleUniquenessVerifier( partitionSearcher ) )
					{
						 verifier.Verify( nodePropertyAccessor, _propertyKeyIds );
					}
			  }
			  finally
			  {
					_searcherManager.release( indexSearcher );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNoDuplicates(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor) throws Exception
		 private void AssertNoDuplicates( NodePropertyAccessor nodePropertyAccessor )
		 {
			  using ( UniquenessVerifier verifier = NewSimpleUniquenessVerifier() )
			  {
					verifier.Verify( nodePropertyAccessor, _propertyKeyIds );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNoDuplicatesCreated(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor, java.util.List<org.Neo4Net.values.storable.Value[]> updatedPropertyValues) throws Exception
		 private void AssertNoDuplicatesCreated( NodePropertyAccessor nodePropertyAccessor, IList<Value[]> updatedPropertyValues )
		 {
			  using ( UniquenessVerifier verifier = NewSimpleUniquenessVerifier() )
			  {
					verifier.Verify( nodePropertyAccessor, _propertyKeyIds, updatedPropertyValues );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertHasDuplicates(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor) throws java.io.IOException
		 private void AssertHasDuplicates( NodePropertyAccessor nodePropertyAccessor )
		 {
			  using ( UniquenessVerifier verifier = NewSimpleUniquenessVerifier() )
			  {
					assertThrows( typeof( IndexEntryConflictException ), () => verifier.verify(nodePropertyAccessor, _propertyKeyIds) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertDuplicatesCreated(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor, java.util.List<org.Neo4Net.values.storable.Value[]> updatedPropertyValues) throws java.io.IOException
		 private void AssertDuplicatesCreated( NodePropertyAccessor nodePropertyAccessor, IList<Value[]> updatedPropertyValues )
		 {
			  using ( UniquenessVerifier verifier = NewSimpleUniquenessVerifier() )
			  {
					assertThrows( typeof( IndexEntryConflictException ), () => verifier.verify(nodePropertyAccessor, _propertyKeyIds, updatedPropertyValues) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(java.util.List<Object> data) throws java.io.IOException
		 private void Insert( IList<object> data )
		 {
			  for ( int i = 0; i < data.Count; i++ )
			  {
					Document doc = LuceneDocumentStructure.documentRepresentingProperties( i, Values.of( data[i] ) );
					_writer.addDocument( doc );
			  }
			  _searcherManager.maybeRefreshBlocking();
		 }

		 private NodePropertyAccessor NewPropertyAccessor( IList<object> propertyValues )
		 {
			  return new TestPropertyAccessor( propertyValues.Select( Values.of ).ToList() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.impl.schema.verification.UniquenessVerifier newSimpleUniquenessVerifier() throws java.io.IOException
		 private UniquenessVerifier NewSimpleUniquenessVerifier()
		 {
			  PartitionSearcher partitionSearcher = new PartitionSearcher( _searcherManager );
			  return new SimpleUniquenessVerifier( partitionSearcher );
		 }
	}

}