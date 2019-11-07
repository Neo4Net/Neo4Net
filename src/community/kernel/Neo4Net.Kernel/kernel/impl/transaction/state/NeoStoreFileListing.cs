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
namespace Neo4Net.Kernel.impl.transaction.state
{

	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using LabelScanStore = Neo4Net.Kernel.Api.LabelScan.LabelScanStore;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using Neo4Net.Kernel.impl.store.format;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using MultiResource = Neo4Net.Kernel.impl.util.MultiResource;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.resourceIterator;

	public class NeoStoreFileListing
	{
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly LogFiles _logFiles;
		 private readonly StorageEngine _storageEngine;
		 private static readonly System.Func<File, StoreFileMetadata> _toNotAStoreTypeFile = file => new StoreFileMetadata( file, Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE );
		 private static readonly System.Func<File, StoreFileMetadata> _logFileMapper = file => new StoreFileMetadata( file, Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE, true );
		 private readonly NeoStoreFileIndexListing _neoStoreFileIndexListing;
		 private readonly ICollection<StoreFileProvider> _additionalProviders;

		 public NeoStoreFileListing( DatabaseLayout databaseLayout, LogFiles logFiles, LabelScanStore labelScanStore, IndexingService indexingService, ExplicitIndexProvider explicitIndexProviders, StorageEngine storageEngine )
		 {
			  this._databaseLayout = databaseLayout;
			  this._logFiles = logFiles;
			  this._storageEngine = storageEngine;
			  this._neoStoreFileIndexListing = new NeoStoreFileIndexListing( labelScanStore, indexingService, explicitIndexProviders );
			  this._additionalProviders = new CopyOnWriteArraySet<StoreFileProvider>();
		 }

		 public virtual StoreFileListingBuilder Builder()
		 {
			  return new StoreFileListingBuilder( this );
		 }

		 public virtual NeoStoreFileIndexListing NeoStoreFileIndexListing
		 {
			 get
			 {
				  return _neoStoreFileIndexListing;
			 }
		 }

		 public virtual void RegisterStoreFileProvider( StoreFileProvider provider )
		 {
			  _additionalProviders.Add( provider );
		 }

		 public interface IStoreFileProvider
		 {
			  /// <param name="fileMetadataCollection"> the collection to add the files to </param>
			  /// <returns> A <seealso cref="Resource"/> that should be closed when we are done working with the files added to the collection </returns>
			  /// <exception cref="IOException"> if the provider is unable to prepare the file listing </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.graphdb.Resource addFilesTo(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata> fileMetadataCollection) throws java.io.IOException;
			  Resource AddFilesTo( ICollection<StoreFileMetadata> fileMetadataCollection );
		 }

		 private void PlaceMetaDataStoreLast( IList<StoreFileMetadata> files )
		 {
			  int index = 0;
			  foreach ( StoreFileMetadata file in files )
			  {
					if ( _databaseLayout.metadataStore().Equals(file.File()) )
					{
						 break;
					}
					index++;
			  }
			  if ( index < Files.Count - 1 )
			  {
					StoreFileMetadata metaDataStoreFile = Files.RemoveAt( index );
					Files.Add( metaDataStoreFile );
			  }
		 }

		 private void GatherNonRecordStores( ICollection<StoreFileMetadata> files, bool includeLogs )
		 {
			  File[] indexFiles = _databaseLayout.listDatabaseFiles( ( dir, name ) => name.Equals( IndexConfigStore.INDEX_DB_FILE_NAME ) );
			  if ( indexFiles != null )
			  {
					foreach ( File file in indexFiles )
					{
						 Files.Add( _toNotAStoreTypeFile.apply( file ) );
					}
			  }
			  if ( includeLogs )
			  {
					File[] logFiles = this._logFiles.logFiles();
					foreach ( File logFile in logFiles )
					{
						 Files.Add( _logFileMapper.apply( logFile ) );
					}
			  }
		 }

		 public class StoreFileListingBuilder
		 {
			 private readonly NeoStoreFileListing _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeLogFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeNonRecordStoreFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeNeoStoreFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeLabelScanStoreFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeSchemaIndexStoreFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeExplicitIndexStoreFilesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ExcludeAdditionalProvidersConflict;

			  internal StoreFileListingBuilder( NeoStoreFileListing outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal virtual void ExcludeAll( bool initiateInclusive )
			  {
					this.ExcludeLogFilesConflict = this.ExcludeNonRecordStoreFilesConflict = this.ExcludeNeoStoreFilesConflict = this.ExcludeLabelScanStoreFilesConflict = this.ExcludeSchemaIndexStoreFilesConflict = this.ExcludeAdditionalProvidersConflict = this.ExcludeExplicitIndexStoreFilesConflict = initiateInclusive;
			  }

			  public virtual StoreFileListingBuilder ExcludeAll()
			  {
					ExcludeAll( true );
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeAll()
			  {
					ExcludeAll( false );
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeLogFiles()
			  {
					ExcludeLogFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeNonRecordStoreFiles()
			  {
					ExcludeNonRecordStoreFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeNeoStoreFiles()
			  {
					ExcludeNeoStoreFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeLabelScanStoreFiles()
			  {
					ExcludeLabelScanStoreFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeSchemaIndexStoreFiles()
			  {
					ExcludeSchemaIndexStoreFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeExplicitIndexStoreFiles()
			  {
					ExcludeExplicitIndexStoreFilesConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder ExcludeAdditionalProviders()
			  {
					ExcludeAdditionalProvidersConflict = true;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeLogFiles()
			  {
					ExcludeLogFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeNonRecordStoreFiles()
			  {
					ExcludeNonRecordStoreFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeNeoStoreFiles()
			  {
					ExcludeNeoStoreFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeLabelScanStoreFiles()
			  {
					ExcludeLabelScanStoreFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeSchemaIndexStoreFiles()
			  {
					ExcludeSchemaIndexStoreFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeExplicitIndexStoreStoreFiles()
			  {
					ExcludeExplicitIndexStoreFilesConflict = false;
					return this;
			  }

			  public virtual StoreFileListingBuilder IncludeAdditionalProviders()
			  {
					ExcludeAdditionalProvidersConflict = false;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.graphdb.ResourceIterator<Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata> build() throws java.io.IOException
			  public virtual IResourceIterator<StoreFileMetadata> Build()
			  {
					IList<StoreFileMetadata> files = new List<StoreFileMetadata>();
					IList<Resource> resources = new List<Resource>();
					try
					{
						 if ( !ExcludeNonRecordStoreFilesConflict )
						 {
							  outerInstance.gatherNonRecordStores( files, !ExcludeLogFilesConflict );
						 }
						 if ( !ExcludeNeoStoreFilesConflict )
						 {
							  outerInstance.gatherNeoStoreFiles( files );
						 }
						 if ( !ExcludeLabelScanStoreFilesConflict )
						 {
							  resources.Add( outerInstance.neoStoreFileIndexListing.GatherLabelScanStoreFiles( files ) );
						 }
						 if ( !ExcludeSchemaIndexStoreFilesConflict )
						 {
							  resources.Add( outerInstance.neoStoreFileIndexListing.GatherSchemaIndexFiles( files ) );
						 }
						 if ( !ExcludeExplicitIndexStoreFilesConflict )
						 {
							  resources.Add( outerInstance.neoStoreFileIndexListing.GatherExplicitIndexFiles( files ) );
						 }
						 if ( !ExcludeAdditionalProvidersConflict )
						 {
							  foreach ( StoreFileProvider additionalProvider in outerInstance.additionalProviders )
							  {
									resources.Add( additionalProvider.AddFilesTo( files ) );
							  }
						 }

						 outerInstance.placeMetaDataStoreLast( files );
					}
					catch ( IOException e )
					{
						 try
						 {
							  IOUtils.closeAll( resources );
						 }
						 catch ( IOException e1 )
						 {
							  e = Exceptions.chain( e, e1 );
						 }
						 throw e;
					}

					return resourceIterator( Files.GetEnumerator(), new MultiResource(resources) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void gatherNeoStoreFiles(final java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata> targetFiles)
		 private void GatherNeoStoreFiles( ICollection<StoreFileMetadata> targetFiles )
		 {
			  targetFiles.addAll( _storageEngine.listStorageFiles() );
		 }
	}

}