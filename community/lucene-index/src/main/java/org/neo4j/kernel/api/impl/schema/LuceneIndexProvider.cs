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

	using IndexCapability = Org.Neo4j.@internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexWriterConfigs = Org.Neo4j.Kernel.Api.Impl.Index.IndexWriterConfigs;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexStorageFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.IndexStorageFactory;
	using PartitionedIndexStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using NonUniqueLuceneIndexPopulator = Org.Neo4j.Kernel.Api.Impl.Schema.populator.NonUniqueLuceneIndexPopulator;
	using UniqueLuceneIndexPopulator = Org.Neo4j.Kernel.Api.Impl.Schema.populator.UniqueLuceneIndexPopulator;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using ByteBufferFactory = Org.Neo4j.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using StoreMigrationParticipant = Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant;
	using SchemaIndexMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.SchemaIndexMigrator;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;

	public class LuceneIndexProvider : IndexProvider
	{
		 private readonly IndexStorageFactory _indexStorageFactory;
		 private readonly Config _config;
		 private readonly OperationalMode _operationalMode;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly Monitor _monitor;

		 public LuceneIndexProvider( FileSystemAbstraction fileSystem, DirectoryFactory directoryFactory, IndexDirectoryStructure.Factory directoryStructureFactory, Monitor monitor, Config config, OperationalMode operationalMode ) : base( LuceneIndexProviderFactory.ProviderDescriptor, directoryStructureFactory )
		 {
			  this._monitor = monitor;
			  this._indexStorageFactory = BuildIndexStorageFactory( fileSystem, directoryFactory );
			  this._fileSystem = fileSystem;
			  this._config = config;
			  this._operationalMode = operationalMode;
		 }

		 public static IndexDirectoryStructure.Factory DefaultDirectoryStructure( File storeDir )
		 {
			  return IndexDirectoryStructure.directoriesByProviderKey( storeDir );
		 }

		 /// <summary>
		 /// Visible <b>only</b> for testing.
		 /// </summary>
		 protected internal virtual IndexStorageFactory BuildIndexStorageFactory( FileSystemAbstraction fileSystem, DirectoryFactory directoryFactory )
		 {
			  return new IndexStorageFactory( directoryFactory, fileSystem, DirectoryStructure() );
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  SchemaIndex luceneIndex = LuceneSchemaIndexBuilder.Create( descriptor, _config ).withFileSystem( _fileSystem ).withOperationalMode( _operationalMode ).withSamplingConfig( samplingConfig ).withIndexStorage( GetIndexStorage( descriptor.Id ) ).withWriterConfig( IndexWriterConfigs.population ).build();
			  if ( luceneIndex.ReadOnly )
			  {
					throw new System.NotSupportedException( "Can't create populator for read only index" );
			  }
			  if ( descriptor.Type() == UNIQUE )
			  {
					return new UniqueLuceneIndexPopulator( luceneIndex, descriptor );
			  }
			  else
			  {
					return new NonUniqueLuceneIndexPopulator( luceneIndex, samplingConfig );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  SchemaIndex luceneIndex = LuceneSchemaIndexBuilder.Create( descriptor, _config ).withOperationalMode( _operationalMode ).withSamplingConfig( samplingConfig ).withIndexStorage( GetIndexStorage( descriptor.Id ) ).build();
			  luceneIndex.open();
			  return new LuceneIndexAccessor( luceneIndex, descriptor );
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  PartitionedIndexStorage indexStorage = GetIndexStorage( descriptor.Id );
			  string failure = indexStorage.StoredIndexFailure;
			  if ( !string.ReferenceEquals( failure, null ) )
			  {
					return InternalIndexState.FAILED;
			  }
			  try
			  {
					return IndexIsOnline( indexStorage, descriptor ) ? InternalIndexState.ONLINE : InternalIndexState.POPULATING;
			  }
			  catch ( IOException e )
			  {
					_monitor.failedToOpenIndex( descriptor, "Requesting re-population.", e );
					return InternalIndexState.POPULATING;
			  }
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return IndexCapability.NO_CAPABILITY;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.impl.storemigration.StoreMigrationParticipant storeMigrationParticipant(final org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache)
		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return new SchemaIndexMigrator( fs, this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  string failure = GetIndexStorage( descriptor.Id ).StoredIndexFailure;
			  if ( string.ReferenceEquals( failure, null ) )
			  {
					throw new System.InvalidOperationException( "Index " + descriptor.Id + " isn't failed" );
			  }
			  return failure;
		 }

		 private PartitionedIndexStorage GetIndexStorage( long indexId )
		 {
			  return _indexStorageFactory.indexStorageOf( indexId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean indexIsOnline(org.neo4j.kernel.api.impl.index.storage.PartitionedIndexStorage indexStorage, org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws java.io.IOException
		 private bool IndexIsOnline( PartitionedIndexStorage indexStorage, IndexDescriptor descriptor )
		 {
			  using ( SchemaIndex index = LuceneSchemaIndexBuilder.Create( descriptor, _config ).withIndexStorage( indexStorage ).build() )
			  {
					if ( index.exists() )
					{
						 index.open();
						 return index.Online;
					}
					return false;
			  }
		 }
	}

}