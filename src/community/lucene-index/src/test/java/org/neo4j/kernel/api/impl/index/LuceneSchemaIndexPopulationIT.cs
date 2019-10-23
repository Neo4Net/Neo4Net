﻿/*
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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using LuceneIndexAccessor = Neo4Net.Kernel.Api.Impl.Schema.LuceneIndexAccessor;
	using LuceneSchemaIndexBuilder = Neo4Net.Kernel.Api.Impl.Schema.LuceneSchemaIndexBuilder;
	using SchemaIndex = Neo4Net.Kernel.Api.Impl.Schema.SchemaIndex;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexPopulationIT
	internal class LuceneSchemaIndexPopulationIT
	{
		 private readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.uniqueForLabel( 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void before()
		 internal virtual void Before()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", "10" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void after()
		 internal virtual void After()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {7, 11, 14, 20, 35, 58}) void partitionedIndexPopulation(int affectedNodes) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PartitionedIndexPopulation( int affectedNodes )
		 {
			  File rootFolder = new File( _testDir.directory( "partitionIndex" + affectedNodes ), "uniqueIndex" + affectedNodes );
			  using ( SchemaIndex uniqueIndex = LuceneSchemaIndexBuilder.create( _descriptor, Config.defaults() ).withFileSystem(_fileSystem).withIndexRootFolder(rootFolder).build() )
			  {
					uniqueIndex.open();

					// index is empty and not yet exist
					assertEquals( 0, uniqueIndex.allDocumentsReader().maxCount() );
					assertFalse( uniqueIndex.exists() );

					using ( LuceneIndexAccessor indexAccessor = new LuceneIndexAccessor( uniqueIndex, _descriptor ) )
					{
						 GenerateUpdates( indexAccessor, affectedNodes );
						 indexAccessor.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );

						 // now index is online and should contain updates data
						 assertTrue( uniqueIndex.Online );

						 using ( IndexReader indexReader = indexAccessor.NewReader(), IndexSampler indexSampler = indexReader.CreateSampler() )
						 {
							  long[] nodes = PrimitiveLongCollections.asArray( indexReader.Query( IndexQuery.exists( 1 ) ) );
							  assertEquals( affectedNodes, nodes.Length );

							  IndexSample sample = indexSampler.SampleIndex();
							  assertEquals( affectedNodes, sample.IndexSize() );
							  assertEquals( affectedNodes, sample.UniqueValues() );
							  assertEquals( affectedNodes, sample.SampleSize() );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateUpdates(org.Neo4Net.kernel.api.impl.schema.LuceneIndexAccessor indexAccessor, int nodesToUpdate) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void GenerateUpdates( LuceneIndexAccessor indexAccessor, int nodesToUpdate )
		 {
			  using ( IndexUpdater updater = indexAccessor.NewUpdater( IndexUpdateMode.ONLINE ) )
			  {
					for ( int nodeId = 0; nodeId < nodesToUpdate; nodeId++ )
					{
						 updater.Process( Add( nodeId, nodeId ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object value)
		 private IndexEntryUpdate<object> Add( long nodeId, object value )
		 {
			  return IndexEntryUpdate.add( nodeId, _descriptor.schema(), Values.of(value) );
		 }
	}

}