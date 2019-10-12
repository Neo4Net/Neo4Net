using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using FileHandle = Neo4Net.Io.fs.FileHandle;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;

	public class StoreFiles
	{
		 private static readonly FilenameFilter _storeFileFilter = ( dir, name ) =>
		 {
		  // Skip log files and tx files from temporary database
		  return !name.StartsWith( "metrics" ) && !name.StartsWith( "temp-copy" ) && !name.StartsWith( "raft-messages." ) && !name.StartsWith( "debug." ) && !name.StartsWith( "data" ) && !name.StartsWith( "store_lock" );
		 };

		 private readonly FilenameFilter _fileFilter;
		 private FileSystemAbstraction _fs;
		 private PageCache _pageCache;

		 public StoreFiles( FileSystemAbstraction fs, PageCache pageCache ) : this( fs, pageCache, _storeFileFilter )
		 {
		 }

		 public StoreFiles( FileSystemAbstraction fs, PageCache pageCache, FilenameFilter fileFilter )
		 {
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._fileFilter = fileFilter;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void delete(java.io.File storeDir, org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
		 public virtual void Delete( File storeDir, LogFiles logFiles )
		 {
			  // 'files' can be null if the directory doesn't exist. This is fine, we just ignore it then.
			  File[] files = _fs.listFiles( storeDir, _fileFilter );
			  if ( files != null )
			  {
					foreach ( File file in files )
					{
						 _fs.deleteRecursively( file );
					}
			  }

			  File[] txLogs = _fs.listFiles( logFiles.LogFilesDirectory() );
			  if ( txLogs != null )
			  {
					foreach ( File txLog in txLogs )
					{
						 _fs.deleteFile( txLog );
					}
			  }

			  IEnumerable<FileHandle> iterator = AcceptedPageCachedFiles( storeDir ).iterator;
			  foreach ( FileHandle fh in iterator )
			  {
					fh.Delete();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.stream.Stream<org.neo4j.io.fs.FileHandle> acceptedPageCachedFiles(java.io.File databaseDirectory) throws java.io.IOException
		 private Stream<FileHandle> AcceptedPageCachedFiles( File databaseDirectory )
		 {
			  try
			  {
					Stream<FileHandle> stream = _fs.streamFilesRecursive( databaseDirectory );
					System.Predicate<FileHandle> acceptableFiles = fh => _fileFilter.accept( databaseDirectory, fh.RelativeFile.Path );
					return stream.filter( acceptableFiles );
			  }
			  catch ( NoSuchFileException )
			  {
					// This is fine. Just ignore empty or non-existing directories.
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveTo(java.io.File source, java.io.File target, org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
		 public virtual void MoveTo( File source, File target, LogFiles logFiles )
		 {
			  _fs.mkdirs( logFiles.LogFilesDirectory() );
			  foreach ( File candidate in _fs.listFiles( source, _fileFilter ) )
			  {
					File destination = logFiles.IsLogFile( candidate ) ? logFiles.LogFilesDirectory() : target;
					_fs.moveToDirectory( candidate, destination );
			  }

			  IEnumerable<FileHandle> fileHandles = AcceptedPageCachedFiles( source ).iterator;
			  foreach ( FileHandle fh in fileHandles )
			  {
					fh.Rename( new File( target, fh.RelativeFile.Path ), StandardCopyOption.REPLACE_EXISTING );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isEmpty(java.io.File storeDir, java.util.Collection<java.io.File> filesToLookFor) throws java.io.IOException
		 public virtual bool IsEmpty( File storeDir, ICollection<File> filesToLookFor )
		 {
			  // 'files' can be null if the directory doesn't exist. This is fine, we just ignore it then.
			  File[] files = _fs.listFiles( storeDir, _fileFilter );
			  if ( files != null )
			  {
					foreach ( File file in files )
					{
						 if ( filesToLookFor.Contains( file ) )
						 {
							  return false;
						 }
					}
			  }

			  IEnumerable<FileHandle> fileHandles = AcceptedPageCachedFiles( storeDir ).iterator;
			  foreach ( FileHandle fh in fileHandles )
			  {
					if ( filesToLookFor.Contains( fh.File ) )
					{
						 return false;
					}
			  }

			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.identity.StoreId readStoreId(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public virtual StoreId ReadStoreId( DatabaseLayout databaseLayout )
		 {
			  File neoStoreFile = databaseLayout.MetadataStore();
			  Neo4Net.Storageengine.Api.StoreId kernelStoreId = MetaDataStore.getStoreId( _pageCache, neoStoreFile );
			  return new StoreId( kernelStoreId.CreationTime, kernelStoreId.RandomId, kernelStoreId.UpgradeTime, kernelStoreId.UpgradeId );
		 }
	}

}