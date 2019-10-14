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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexStorageFactory = Neo4Net.Kernel.Api.Impl.Index.storage.IndexStorageFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexQueryHelper = Neo4Net.Kernel.Api.Index.IndexQueryHelper;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneIndexProviderFactory.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProviderKey;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(EphemeralFileSystemExtension.class) class AccessUniqueDatabaseIndexTest
	internal class AccessUniqueDatabaseIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fileSystem;
		 private EphemeralFileSystemAbstraction _fileSystem;
		 private readonly DirectoryFactory _directoryFactory = new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
		 private readonly File _storeDirectory = new File( "db" );
		 private readonly IndexDescriptor _index = TestIndexDescriptorFactory.uniqueForLabel( 1000, 100 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddUniqueEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddUniqueEntries()
		 {
			  // given
			  PartitionedIndexStorage indexStorage = IndexStorage;
			  LuceneIndexAccessor accessor = CreateAccessor( indexStorage );

			  // when
			  UpdateAndCommit( accessor, asList( Add( 1L, "value1" ), Add( 2L, "value2" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 3L, "value3" ) ) );
			  accessor.Dispose();

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( indexStorage, "value1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateUniqueEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUpdateUniqueEntries()
		 {
			  // given
			  PartitionedIndexStorage indexStorage = IndexStorage;

			  LuceneIndexAccessor accessor = CreateAccessor( indexStorage );

			  // when
			  UpdateAndCommit( accessor, asList( Add( 1L, "value1" ) ) );
			  UpdateAndCommit( accessor, asList( Change( 1L, "value1", "value2" ) ) );
			  accessor.Dispose();

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( indexStorage, "value2" ) );
			  assertEquals( emptyList(), GetAllNodes(indexStorage, "value1") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveAndAddEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRemoveAndAddEntries()
		 {
			  // given
			  PartitionedIndexStorage indexStorage = IndexStorage;

			  LuceneIndexAccessor accessor = CreateAccessor( indexStorage );

			  // when
			  UpdateAndCommit( accessor, asList( Add( 1L, "value1" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 2L, "value2" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 3L, "value3" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 4L, "value4" ) ) );
			  UpdateAndCommit( accessor, asList( Remove( 1L, "value1" ) ) );
			  UpdateAndCommit( accessor, asList( Remove( 2L, "value2" ) ) );
			  UpdateAndCommit( accessor, asList( Remove( 3L, "value3" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 1L, "value1" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 3L, "value3b" ) ) );
			  accessor.Dispose();

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( indexStorage, "value1" ) );
			  assertEquals( emptyList(), GetAllNodes(indexStorage, "value2") );
			  assertEquals( emptyList(), GetAllNodes(indexStorage, "value3") );
			  assertEquals( asList( 3L ), GetAllNodes( indexStorage, "value3b" ) );
			  assertEquals( asList( 4L ), GetAllNodes( indexStorage, "value4" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConsiderWholeTransactionForValidatingUniqueness() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldConsiderWholeTransactionForValidatingUniqueness()
		 {
			  // given
			  PartitionedIndexStorage indexStorage = IndexStorage;

			  LuceneIndexAccessor accessor = CreateAccessor( indexStorage );

			  // when
			  UpdateAndCommit( accessor, asList( Add( 1L, "value1" ) ) );
			  UpdateAndCommit( accessor, asList( Add( 2L, "value2" ) ) );
			  UpdateAndCommit( accessor, asList( Change( 1L, "value1", "value2" ), Change( 2L, "value2", "value1" ) ) );
			  accessor.Dispose();

			  // then
			  assertEquals( asList( 2L ), GetAllNodes( indexStorage, "value1" ) );
			  assertEquals( asList( 1L ), GetAllNodes( indexStorage, "value2" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private LuceneIndexAccessor createAccessor(org.neo4j.kernel.api.impl.index.storage.PartitionedIndexStorage indexStorage) throws java.io.IOException
		 private LuceneIndexAccessor CreateAccessor( PartitionedIndexStorage indexStorage )
		 {
			  SchemaIndex luceneIndex = LuceneSchemaIndexBuilder.Create( _index, Config.defaults() ).withIndexStorage(indexStorage).build();
			  luceneIndex.open();
			  return new LuceneIndexAccessor( luceneIndex, _index );
		 }

		 private PartitionedIndexStorage IndexStorage
		 {
			 get
			 {
				  IndexStorageFactory storageFactory = new IndexStorageFactory( _directoryFactory, _fileSystem, directoriesByProviderKey( _storeDirectory ).forProvider( PROVIDER_DESCRIPTOR ) );
				  return storageFactory.IndexStorageOf( 1 );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object propertyValue)
		 private IndexEntryUpdate<object> Add( long nodeId, object propertyValue )
		 {
			  return IndexQueryHelper.add( nodeId, _index.schema(), propertyValue );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> change(long nodeId, Object oldValue, Object newValue)
		 private IndexEntryUpdate<object> Change( long nodeId, object oldValue, object newValue )
		 {
			  return IndexQueryHelper.change( nodeId, _index.schema(), oldValue, newValue );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexEntryUpdate<?> remove(long nodeId, Object oldValue)
		 private IndexEntryUpdate<object> Remove( long nodeId, object oldValue )
		 {
			  return IndexQueryHelper.remove( nodeId, _index.schema(), oldValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<long> getAllNodes(org.neo4j.kernel.api.impl.index.storage.PartitionedIndexStorage indexStorage, String propertyValue) throws java.io.IOException
		 private IList<long> GetAllNodes( PartitionedIndexStorage indexStorage, string propertyValue )
		 {
			  return AllNodesCollector.GetAllNodes( indexStorage.OpenDirectory( indexStorage.GetPartitionFolder( 1 ) ), Values.stringValue( propertyValue ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateAndCommit(org.neo4j.kernel.api.index.IndexAccessor accessor, Iterable<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void UpdateAndCommit<T1>( IndexAccessor accessor, IEnumerable<T1> updates )
		 {
			  using ( IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE ) )
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