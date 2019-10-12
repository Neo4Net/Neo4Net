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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.populator
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using Org.Neo4j.Kernel.Api.Index;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class NonUniqueDatabaseIndexPopulatorTest
	internal class NonUniqueDatabaseIndexPopulatorTest
	{
		 private readonly DirectoryFactory _dirFactory = new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

		 private SchemaIndex _index;
		 private NonUniqueLuceneIndexPopulator _populator;
		 private readonly SchemaDescriptor _labelSchemaDescriptor = SchemaDescriptorFactory.forLabel( 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  File folder = _testDir.directory( "folder" );
			  PartitionedIndexStorage indexStorage = new PartitionedIndexStorage( _dirFactory, _fileSystem, folder );

			  IndexDescriptor descriptor = IndexDescriptorFactory.forSchema( _labelSchemaDescriptor );
			  _index = LuceneSchemaIndexBuilder.create( descriptor, Config.defaults() ).withIndexStorage(indexStorage).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TearDown()
		 {
			  if ( _populator != null )
			  {
					_populator.close( false );
			  }
			  IOUtils.closeAll( _index, _dirFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void sampleEmptyIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SampleEmptyIndex()
		 {
			  _populator = NewPopulator();

			  IndexSample sample = _populator.sampleResult();

			  assertEquals( new IndexSample(), sample );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void sampleIncludedUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SampleIncludedUpdates()
		 {
			  _populator = NewPopulator();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Arrays.asList(add(1, labelSchemaDescriptor, "aaa"), add(2, labelSchemaDescriptor, "bbb"), add(3, labelSchemaDescriptor, "ccc"));
			  IList<IndexEntryUpdate<object>> updates = Arrays.asList( add( 1, _labelSchemaDescriptor, "aaa" ), add( 2, _labelSchemaDescriptor, "bbb" ), add( 3, _labelSchemaDescriptor, "ccc" ) );

			  updates.ForEach( _populator.includeSample );

			  IndexSample sample = _populator.sampleResult();

			  assertEquals( new IndexSample( 3, 3, 3 ), sample );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void sampleIncludedUpdatesWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SampleIncludedUpdatesWithDuplicates()
		 {
			  _populator = NewPopulator();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Arrays.asList(add(1, labelSchemaDescriptor, "foo"), add(2, labelSchemaDescriptor, "bar"), add(3, labelSchemaDescriptor, "foo"));
			  IList<IndexEntryUpdate<object>> updates = Arrays.asList( add( 1, _labelSchemaDescriptor, "foo" ), add( 2, _labelSchemaDescriptor, "bar" ), add( 3, _labelSchemaDescriptor, "foo" ) );

			  updates.ForEach( _populator.includeSample );

			  IndexSample sample = _populator.sampleResult();

			  assertEquals( new IndexSample( 3, 2, 3 ), sample );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddUpdates()
		 {
			  _populator = NewPopulator();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Arrays.asList(add(1, labelSchemaDescriptor, "foo"), add(2, labelSchemaDescriptor, "bar"), add(42, labelSchemaDescriptor, "bar"));
			  IList<IndexEntryUpdate<object>> updates = Arrays.asList( add( 1, _labelSchemaDescriptor, "foo" ), add( 2, _labelSchemaDescriptor, "bar" ), add( 42, _labelSchemaDescriptor, "bar" ) );

			  _populator.add( updates );

			  _index.maybeRefreshBlocking();
			  using ( IndexReader reader = _index.IndexReader )
			  {
					int propertyKeyId = _labelSchemaDescriptor.PropertyId;
					LongIterator allEntities = reader.Query( IndexQuery.exists( propertyKeyId ) );
					assertArrayEquals( new long[]{ 1, 2, 42 }, PrimitiveLongCollections.asArray( allEntities ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private NonUniqueLuceneIndexPopulator newPopulator() throws java.io.IOException
		 private NonUniqueLuceneIndexPopulator NewPopulator()
		 {
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
			  NonUniqueLuceneIndexPopulator populator = new NonUniqueLuceneIndexPopulator( _index, samplingConfig );
			  populator.Create();
			  return populator;
		 }
	}

}