using System;
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
namespace Neo4Net.Kernel.Api.Impl.Index.storage
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Directory = org.apache.lucene.store.Directory;


	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using FolderLayout = Neo4Net.Kernel.Api.Impl.Index.storage.layout.FolderLayout;
	using IndexFolderLayout = Neo4Net.Kernel.Api.Impl.Index.storage.layout.IndexFolderLayout;
	using NumberAwareStringComparator = Neo4Net.Kernel.impl.util.NumberAwareStringComparator;

	/// <summary>
	/// Utility class that manages directory structure for a partitioned lucene index.
	/// It is aware of the <seealso cref="FileSystemAbstraction file system"/> structure of all index related folders, lucene
	/// <seealso cref="Directory directories"/> and <seealso cref="FailureStorage failure storage"/>.
	/// </summary>
	public class PartitionedIndexStorage
	{
		 private static readonly IComparer<File> _fileComparator = ( o1, o2 ) => NumberAwareStringComparator.INSTANCE.Compare( o1.Name, o2.Name );

		 private readonly DirectoryFactory _directoryFactory;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly FolderLayout _folderLayout;
		 private readonly FailureStorage _failureStorage;

		 public PartitionedIndexStorage( DirectoryFactory directoryFactory, FileSystemAbstraction fileSystem, File rootFolder )
		 {
			  this._fileSystem = fileSystem;
			  this._folderLayout = new IndexFolderLayout( rootFolder );
			  this._directoryFactory = directoryFactory;
			  this._failureStorage = new FailureStorage( fileSystem, _folderLayout );
		 }

		 /// <summary>
		 /// Opens a <seealso cref="Directory lucene directory"/> for the given folder.
		 /// </summary>
		 /// <param name="folder"> the folder that denotes a lucene directory. </param>
		 /// <returns> the lucene directory denoted by the given folder. </returns>
		 /// <exception cref="IOException"> if directory can't be opened. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.Directory openDirectory(java.io.File folder) throws java.io.IOException
		 public virtual Directory OpenDirectory( File folder )
		 {
			  return _directoryFactory.open( folder );
		 }

		 /// <summary>
		 /// Resolves a folder for the partition with the given index.
		 /// </summary>
		 /// <param name="partition"> the partition index. </param>
		 /// <returns> the folder where partition's lucene directory should be located. </returns>
		 public virtual File GetPartitionFolder( int partition )
		 {
			  return _folderLayout.getPartitionFolder( partition );
		 }

		 /// <summary>
		 /// Resolves root folder for the given index.
		 /// </summary>
		 /// <returns> the folder containing index partition folders. </returns>
		 public virtual File IndexFolder
		 {
			 get
			 {
				  return _folderLayout.IndexFolder;
			 }
		 }

		 /// <summary>
		 /// Create a failure storage in the <seealso cref="getIndexFolder() index folder"/>.
		 /// </summary>
		 /// <exception cref="IOException"> if failure storage creation fails. </exception>
		 /// <seealso cref= FailureStorage#reserveForIndex() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reserveIndexFailureStorage() throws java.io.IOException
		 public virtual void ReserveIndexFailureStorage()
		 {
			  _failureStorage.reserveForIndex();
		 }

		 /// <summary>
		 /// Writes index failure into the failure storage.
		 /// </summary>
		 /// <param name="failure"> the cause of the index failure. </param>
		 /// <exception cref="IOException"> if writing to the failure storage file failed. </exception>
		 /// <seealso cref= FailureStorage#storeIndexFailure(String) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void storeIndexFailure(String failure) throws java.io.IOException
		 public virtual void StoreIndexFailure( string failure )
		 {
			  _failureStorage.storeIndexFailure( failure );
		 }

		 /// <summary>
		 /// Retrieves stored index failure.
		 /// </summary>
		 /// <returns> index failure as string or {@code null} if there is no failure. </returns>
		 /// <seealso cref= FailureStorage#loadIndexFailure() </seealso>
		 public virtual string StoredIndexFailure
		 {
			 get
			 {
				  return _failureStorage.loadIndexFailure();
			 }
		 }

		 /// <summary>
		 /// For the given <seealso cref="File folder"/> removes all nested folders from both <seealso cref="FileSystemAbstraction file system"/>
		 /// and <seealso cref="Directory lucene directories"/>.
		 /// </summary>
		 /// <param name="folder"> the folder to clean up. </param>
		 /// <exception cref="IOException"> if some removal operation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void prepareFolder(java.io.File folder) throws java.io.IOException
		 public virtual void PrepareFolder( File folder )
		 {
			  CleanupFolder( folder );
			  _fileSystem.mkdirs( folder );
		 }

		 /// <summary>
		 /// For the given <seealso cref="File folder"/> removes the folder itself and all nested folders from both
		 /// <seealso cref="FileSystemAbstraction file system"/> and <seealso cref="Directory lucene directories"/>.
		 /// </summary>
		 /// <param name="folder"> the folder to remove. </param>
		 /// <exception cref="IOException"> if some removal operation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanupFolder(java.io.File folder) throws java.io.IOException
		 public virtual void CleanupFolder( File folder )
		 {
			  IList<File> partitionFolders = ListFolders( folder );
			  if ( partitionFolders.Count > 0 )
			  {
					foreach ( File partitionFolder in partitionFolders )
					{
						 CleanupLuceneDirectory( partitionFolder );
					}
			  }
			  _fileSystem.deleteRecursively( folder );
		 }

		 /// <summary>
		 /// Opens all <seealso cref="Directory lucene directories"/> contained in the <seealso cref="getIndexFolder() index folder"/>.
		 /// </summary>
		 /// <returns> the map from file system  <seealso cref="File directory"/> to the corresponding <seealso cref="Directory lucene directory"/>. </returns>
		 /// <exception cref="IOException"> if opening of some lucene directory (via <seealso cref="DirectoryFactory.open(File)"/>) fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<java.io.File,org.apache.lucene.store.Directory> openIndexDirectories() throws java.io.IOException
		 public virtual IDictionary<File, Directory> OpenIndexDirectories()
		 {
			  IDictionary<File, Directory> directories = new LinkedHashMap<File, Directory>();
			  try
			  {
					foreach ( File dir in ListFolders() )
					{
						 directories[dir] = _directoryFactory.open( dir );
					}
			  }
			  catch ( IOException oe )
			  {
					try
					{
						 IOUtils.closeAll( directories.Values );
					}
					catch ( Exception ce )
					{
						 oe.addSuppressed( ce );
					}
					throw oe;
			  }
			  return directories;
		 }

		 /// <summary>
		 /// List all folders in the <seealso cref="getIndexFolder() index folder"/>.
		 /// </summary>
		 /// <returns> the list of index partition folders or <seealso cref="Collections.emptyList() empty list"/> if index folder is
		 /// empty. </returns>
		 public virtual IList<File> ListFolders()
		 {
			  return ListFolders( IndexFolder );
		 }

		 private IList<File> ListFolders( File rootFolder )
		 {
			  File[] files = _fileSystem.listFiles( rootFolder );
			  return files == null ? Collections.emptyList() : Stream.of(files).filter(f => _fileSystem.isDirectory(f) && StringUtils.isNumeric(f.Name)).sorted(_fileComparator).collect(toList());

		 }

		 /// <summary>
		 /// Removes content of the lucene directory denoted by the given <seealso cref="File file"/>. This might seem unnecessary
		 /// since we cleanup the folder using <seealso cref="FileSystemAbstraction file system"/> but in fact for testing we often use
		 /// in-memory directories whose content can't be removed via the file system.
		 /// <para>
		 /// Uses <seealso cref="FileUtils.windowsSafeIOOperation(FileUtils.Operation)"/> underneath.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="folder"> the path to the directory to cleanup. </param>
		 /// <exception cref="IOException"> if removal operation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void cleanupLuceneDirectory(java.io.File folder) throws java.io.IOException
		 private void CleanupLuceneDirectory( File folder )
		 {
			  using ( Directory dir = _directoryFactory.open( folder ) )
			  {
					string[] indexFiles = dir.listAll();
					foreach ( string indexFile in indexFiles )
					{
						 FileUtils.windowsSafeIOOperation( () => dir.deleteFile(indexFile) );
					}
			  }
		 }
	}

}