﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using Document = org.apache.lucene.document.Document;
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using TopDocs = org.apache.lucene.search.TopDocs;
	using Directory = org.apache.lucene.store.Directory;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexQueryHelper = Org.Neo4j.Kernel.Api.Index.IndexQueryHelper;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexPopulatorTest
	internal class LuceneSchemaIndexPopulatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fs;
		 private DefaultFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 private IndexStoreView _indexStoreView;
		 private LuceneIndexProvider _provider;
		 private Directory _directory;
		 private IndexPopulator _indexPopulator;
		 private IndexReader _reader;
		 private IndexSearcher _searcher;
		 private const int PROPERTY_KEY_ID = 666;
		 private StoreIndexDescriptor _index;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Before()
		 {
			  _directory = new RAMDirectory();
			  DirectoryFactory directoryFactory = new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_Single( new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_UncloseableDirectory( _directory ) );
			  _provider = new LuceneIndexProvider( _fs, directoryFactory, defaultDirectoryStructure( _testDir.directory( "folder" ) ), IndexProvider.Monitor_Fields.EMPTY, Config.defaults(), OperationalMode.single );
			  _indexStoreView = mock( typeof( IndexStoreView ) );
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
			  _index = IndexDescriptorFactory.forSchema( forLabel( 42, PROPERTY_KEY_ID ), _provider.ProviderDescriptor ).withId( 0 );
			  _indexPopulator = _provider.getPopulator( _index, samplingConfig, heapBufferFactory( 1024 ) );
			  _indexPopulator.create();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void after() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void After()
		 {
			  if ( _reader != null )
			  {
					_reader.close();
			  }
			  _directory.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addingValuesShouldPersistThem() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddingValuesShouldPersistThem()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "First" );
			  AddUpdate( _indexPopulator, 2, "Second" );
			  AddUpdate( _indexPopulator, 3, ( sbyte ) 1 );
			  AddUpdate( _indexPopulator, 4, ( short ) 2 );
			  AddUpdate( _indexPopulator, 5, 3 );
			  AddUpdate( _indexPopulator, 6, 4L );
			  AddUpdate( _indexPopulator, 7, 5F );
			  AddUpdate( _indexPopulator, 8, 6D );

			  // THEN
			  AssertIndexedValues( hit( "First", 1 ), hit( "Second", 2 ), hit( ( sbyte )1, 3 ), hit( ( short )2, 4 ), hit( 3, 5 ), hit( 4L, 6 ), hit( 5F, 7 ), hit( 6D, 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void multipleEqualValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MultipleEqualValues()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "value" );
			  AddUpdate( _indexPopulator, 2, "value" );
			  AddUpdate( _indexPopulator, 3, "value" );

			  // THEN
			  AssertIndexedValues( hit( "value", 1L, 2L, 3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void multipleEqualValuesWithUpdateThatRemovesOne() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MultipleEqualValuesWithUpdateThatRemovesOne()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "value" );
			  AddUpdate( _indexPopulator, 2, "value" );
			  AddUpdate( _indexPopulator, 3, "value" );
			  UpdatePopulator( _indexPopulator, singletonList( Remove( 2, "value" ) ), _indexStoreView );

			  // THEN
			  AssertIndexedValues( hit( "value", 1L, 3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changeUpdatesInterleavedWithAdds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangeUpdatesInterleavedWithAdds()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "1" );
			  AddUpdate( _indexPopulator, 2, "2" );
			  UpdatePopulator( _indexPopulator, singletonList( Change( 1, "1", "1a" ) ), _indexStoreView );
			  AddUpdate( _indexPopulator, 3, "3" );

			  // THEN
			  AssertIndexedValues( No( "1" ), hit( "1a", 1 ), hit( "2", 2 ), hit( "3", 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addUpdatesInterleavedWithAdds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddUpdatesInterleavedWithAdds()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "1" );
			  AddUpdate( _indexPopulator, 2, "2" );
			  UpdatePopulator( _indexPopulator, asList( Remove( 1, "1" ), Add( 1, "1a" ) ), _indexStoreView );
			  AddUpdate( _indexPopulator, 3, "3" );

			  // THEN
			  AssertIndexedValues( hit( "1a", 1 ), hit( "2", 2 ), hit( "3", 3 ), No( "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removeUpdatesInterleavedWithAdds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemoveUpdatesInterleavedWithAdds()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "1" );
			  AddUpdate( _indexPopulator, 2, "2" );
			  UpdatePopulator( _indexPopulator, singletonList( Remove( 2, "2" ) ), _indexStoreView );
			  AddUpdate( _indexPopulator, 3, "3" );

			  // THEN
			  AssertIndexedValues( hit( "1", 1 ), No( "2" ), hit( "3", 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void multipleInterleaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MultipleInterleaves()
		 {
			  // WHEN
			  AddUpdate( _indexPopulator, 1, "1" );
			  AddUpdate( _indexPopulator, 2, "2" );
			  UpdatePopulator( _indexPopulator, asList( Change( 1, "1", "1a" ), Change( 2, "2", "2a" ) ), _indexStoreView );
			  AddUpdate( _indexPopulator, 3, "3" );
			  AddUpdate( _indexPopulator, 4, "4" );
			  UpdatePopulator( _indexPopulator, asList( Change( 1, "1a", "1b" ), Change( 4, "4", "4a" ) ), _indexStoreView );

			  // THEN
			  AssertIndexedValues( No( "1" ), No( "1a" ), hit( "1b", 1 ), No( "2" ), hit( "2a", 2 ), hit( "3", 3 ), No( "4" ), hit( "4a", 4 ) );
		 }

		 private Hit Hit( object value, params Long[] nodeIds )
		 {
			  return new Hit( value, nodeIds );
		 }

		 private Hit Hit( object value, long nodeId )
		 {
			  return new Hit( value, nodeId );
		 }

		 private Hit No( object value )
		 {
			  return new Hit( value );
		 }

		 private class Hit
		 {
			  internal readonly Value Value;
			  internal readonly long?[] NodeIds;

			  internal Hit( object value, params Long[] nodeIds )
			  {
					this.Value = Values.of( value );
					this.NodeIds = nodeIds;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object value)
		 private IndexEntryUpdate<object> Add( long nodeId, object value )
		 {
			  return IndexQueryHelper.add( nodeId, _index.schema(), value );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> change(long nodeId, Object valueBefore, Object valueAfter)
		 private IndexEntryUpdate<object> Change( long nodeId, object valueBefore, object valueAfter )
		 {
			  return IndexQueryHelper.change( nodeId, _index.schema(), valueBefore, valueAfter );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> remove(long nodeId, Object removedValue)
		 private IndexEntryUpdate<object> Remove( long nodeId, object removedValue )
		 {
			  return IndexQueryHelper.remove( nodeId, _index.schema(), removedValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexedValues(Hit... expectedHits) throws java.io.IOException
		 private void AssertIndexedValues( params Hit[] expectedHits )
		 {
			  SwitchToVerification();

			  foreach ( Hit hit in expectedHits )
			  {
					TopDocs hits = _searcher.search( LuceneDocumentStructure.NewSeekQuery( hit.Value ), 10 );
					assertEquals( hit.NodeIds.Length, hits.totalHits, "Unexpected number of index results from " + hit.Value );
					ISet<long> foundNodeIds = new HashSet<long>();
					for ( int i = 0; i < hits.totalHits; i++ )
					{
						 Document document = _searcher.doc( hits.scoreDocs[i].doc );
						 foundNodeIds.Add( parseLong( document.get( "id" ) ) );
					}
					assertEquals( asSet( hit.NodeIds ), foundNodeIds );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void switchToVerification() throws java.io.IOException
		 private void SwitchToVerification()
		 {
			  _indexPopulator.close( true );
			  assertEquals( InternalIndexState.ONLINE, _provider.getInitialState( _index ) );
			  _reader = DirectoryReader.open( _directory );
			  _searcher = new IndexSearcher( _reader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addUpdate(org.neo4j.kernel.api.index.IndexPopulator populator, long nodeId, Object value) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void AddUpdate( IndexPopulator populator, long nodeId, object value )
		 {
			  populator.Add( singletonList( IndexQueryHelper.add( nodeId, _index.schema(), value ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void updatePopulator(org.neo4j.kernel.api.index.IndexPopulator populator, Iterable<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates, org.neo4j.storageengine.api.NodePropertyAccessor accessor) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void UpdatePopulator<T1>( IndexPopulator populator, IEnumerable<T1> updates, NodePropertyAccessor accessor )
		 {
			  using ( IndexUpdater updater = populator.NewPopulatingUpdater( accessor ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 updater.Process( update );
					}
			  }
		 }
	}

}