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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using CheckIndex = Org.Apache.Lucene.Index.CheckIndex;
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Directory = org.apache.lucene.store.Directory;


	using Neo4Net.GraphDb;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using IOUtils = Neo4Net.Io.IOUtils;
	using WritableIndexSnapshotFileIterator = Neo4Net.Kernel.Api.Impl.Index.backup.WritableIndexSnapshotFileIterator;
	using AbstractIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using IndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.IndexPartitionFactory;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using PartitionedIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.PartitionedIndexWriter;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;


	/// <summary>
	/// Abstract implementation of a partitioned index.
	/// Such index may consist of one or multiple separate Lucene indexes that are represented as independent
	/// <seealso cref="AbstractIndexPartition partitions"/>.
	/// Class and it's subclasses should not be directly used, instead please use corresponding writable or read only
	/// wrapper. </summary>
	/// <seealso cref= WritableAbstractDatabaseIndex </seealso>
	/// <seealso cref= ReadOnlyAbstractDatabaseIndex </seealso>
	public abstract class AbstractLuceneIndex<READER> where READER : Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader
	{
		 private const string KEY_STATUS = "status";
		 private const string ONLINE = "online";
		 private static readonly IDictionary<string, string> _onlineCommitUserData = singletonMap( KEY_STATUS, ONLINE );
		 protected internal readonly PartitionedIndexStorage IndexStorage;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly IndexDescriptor DescriptorConflict;
		 private readonly IndexPartitionFactory _partitionFactory;

		 // Note that we rely on the thread-safe internal snapshot feature of the CopyOnWriteArrayList
		 // for the thread-safety of this and derived classes.
		 private CopyOnWriteArrayList<AbstractIndexPartition> _partitions = new CopyOnWriteArrayList<AbstractIndexPartition>();

		 private volatile bool _open;

		 public AbstractLuceneIndex( PartitionedIndexStorage indexStorage, IndexPartitionFactory partitionFactory, IndexDescriptor descriptor )
		 {
			  this.IndexStorage = indexStorage;
			  this._partitionFactory = partitionFactory;
			  this.DescriptorConflict = descriptor;
		 }

		 /// <summary>
		 /// Creates new index.
		 /// As part of creation process index will allocate all required folders, index failure storage
		 /// and will create its first partition.
		 /// <para>
		 /// <b>Index creation do not automatically open it. To be able to use index please open it first.</b>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void create() throws java.io.IOException
		 public virtual void Create()
		 {
			  EnsureNotOpen();
			  IndexStorage.prepareFolder( IndexStorage.IndexFolder );
			  IndexStorage.reserveIndexFailureStorage();
			  CreateNewPartitionFolder();
		 }

		 /// <summary>
		 /// Open index with all allocated partitions.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void open() throws java.io.IOException
		 public virtual void Open()
		 {
			  ISet<KeyValuePair<File, Directory>> indexDirectories = IndexStorage.openIndexDirectories().SetOfKeyValuePairs();
			  IList<AbstractIndexPartition> list = new List<AbstractIndexPartition>( indexDirectories.Count );
			  foreach ( KeyValuePair<File, Directory> entry in indexDirectories )
			  {
					list.Add( _partitionFactory.createPartition( entry.Key, entry.Value ) );
			  }
			  _partitions.addAll( list );
			  _open = true;
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _open;
			 }
		 }

		 /// <summary>
		 /// Check lucene index existence within all allocated partitions.
		 /// </summary>
		 /// <returns> true if index exist in all partitions, false when index is empty or does not exist </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean exists() throws java.io.IOException
		 public virtual bool Exists()
		 {
			  IList<File> folders = IndexStorage.listFolders();
			  if ( folders.Count == 0 )
			  {
					return false;
			  }
			  foreach ( File folder in folders )
			  {
					if ( !LuceneDirectoryExists( folder ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <summary>
		 /// Verify state of the index.
		 /// If index is already open and in use method assume that index is valid since lucene already operating with it,
		 /// otherwise necessary checks perform.
		 /// </summary>
		 /// <returns> true if lucene confirm that index is in valid clean state or index is already open. </returns>
		 public virtual bool Valid
		 {
			 get
			 {
				  if ( _open )
				  {
						return true;
				  }
				  ICollection<Directory> directories = null;
				  try
				  {
						directories = IndexStorage.openIndexDirectories().Values;
						foreach ( Directory directory in directories )
						{
							 // it is ok for index directory to be empty
							 // this can happen if it is opened and closed without any writes in between
							 if ( !ArrayUtil.isEmpty( directory.listAll() ) )
							 {
								  using ( CheckIndex checker = new CheckIndex( directory ) )
								  {
										CheckIndex.Status status = checker.checkIndex();
										if ( !status.clean )
										{
											 return false;
										}
								  }
							 }
						}
				  }
				  catch ( IOException )
				  {
						return false;
				  }
				  finally
				  {
						IOUtils.closeAllSilently( directories );
				  }
				  return true;
			 }
		 }

		 public virtual LuceneIndexWriter GetIndexWriter( WritableAbstractDatabaseIndex writableAbstractDatabaseIndex )
		 {
			  EnsureOpen();
			  return new PartitionedIndexWriter( writableAbstractDatabaseIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public READER getIndexReader() throws java.io.IOException
		 public virtual READER IndexReader
		 {
			 get
			 {
				  EnsureOpen();
				  IList<AbstractIndexPartition> partitions = Partitions;
				  return HasSinglePartition( partitions ) ? CreateSimpleReader( partitions ) : CreatePartitionedReader( partitions );
			 }
		 }

		 public virtual IndexDescriptor Descriptor
		 {
			 get
			 {
				  return DescriptorConflict;
			 }
		 }

		 /// <summary>
		 /// Close index and deletes all it's partitions.
		 /// </summary>
		 public virtual void Drop()
		 {
			  try
			  {
					Close();
					IndexStorage.cleanupFolder( IndexStorage.IndexFolder );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Commits all index partitions.
		 /// </summary>
		 /// <param name="merge"> also merge all segments together. This should be done before reading term frequencies. </param>
		 /// <exception cref="IOException"> on Lucene I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush(boolean merge) throws java.io.IOException
		 public virtual void Flush( bool merge )
		 {
			  IList<AbstractIndexPartition> partitions = Partitions;
			  foreach ( AbstractIndexPartition partition in partitions )
			  {
					IndexWriter writer = partition.IndexWriter;
					writer.commit();
					if ( merge )
					{
						 writer.forceMerge( 1 );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public virtual void Close()
		 {
			  _open = false;
			  IOUtils.closeAll( _partitions );
			  _partitions.clear();
		 }

		 /// <summary>
		 /// Creates an iterable over all <seealso cref="org.apache.lucene.document.Document document"/>s in all partitions.
		 /// </summary>
		 /// <returns> LuceneAllDocumentsReader over all documents </returns>
		 public virtual LuceneAllDocumentsReader AllDocumentsReader()
		 {
			  EnsureOpen();
			  IList<PartitionSearcher> searchers = new List<PartitionSearcher>( _partitions.size() );
			  try
			  {
					foreach ( AbstractIndexPartition partition in _partitions )
					{
						 searchers.Add( partition.AcquireSearcher() );
					}

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					IList<LucenePartitionAllDocumentsReader> partitionReaders = searchers.Select( LucenePartitionAllDocumentsReader::new ).ToList();

					return new LuceneAllDocumentsReader( partitionReaders );
			  }
			  catch ( IOException e )
			  {
					IOUtils.closeAllSilently( searchers );
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Snapshot of all file in all index partitions.
		 /// </summary>
		 /// <returns> iterator over all index files. </returns>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= WritableIndexSnapshotFileIterator </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException
		 public virtual ResourceIterator<File> Snapshot()
		 {
			  EnsureOpen();
			  IList<ResourceIterator<File>> snapshotIterators = null;
			  try
			  {
					IList<AbstractIndexPartition> partitions = Partitions;
					snapshotIterators = new List<ResourceIterator<File>>( partitions.Count );
					foreach ( AbstractIndexPartition partition in partitions )
					{
						 snapshotIterators.Add( partition.Snapshot() );
					}
					return Iterators.concatResourceIterators( snapshotIterators.GetEnumerator() );
			  }
			  catch ( Exception e )
			  {
					if ( snapshotIterators != null )
					{
						 try
						 {
							  IOUtils.closeAll( snapshotIterators );
						 }
						 catch ( IOException ex )
						 {
							  e.addSuppressed( ex );
						 }
					}
					throw e;
			  }
		 }

		 /// <summary>
		 /// Refresh all partitions to make newly inserted data visible for readers.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void maybeRefreshBlocking() throws java.io.IOException
		 public virtual void MaybeRefreshBlocking()
		 {
			  try
			  {
					Partitions.ForEach( this.maybeRefreshPartition );
			  }
			  catch ( UncheckedIOException e )
			  {
					throw e.InnerException;
			  }
		 }

		 private void MaybeRefreshPartition( AbstractIndexPartition partition )
		 {
			  try
			  {
					partition.MaybeRefreshBlocking();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public virtual IList<AbstractIndexPartition> Partitions
		 {
			 get
			 {
				  EnsureOpen();
				  return _partitions;
			 }
		 }

		 public virtual bool HasSinglePartition( IList<AbstractIndexPartition> partitions )
		 {
			  return partitions.Count == 1;
		 }

		 public virtual AbstractIndexPartition GetFirstPartition( IList<AbstractIndexPartition> partitions )
		 {
			  return partitions[0];
		 }

		 /// <summary>
		 /// Add new partition to the index.
		 /// </summary>
		 /// <returns> newly created partition </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition addNewPartition() throws java.io.IOException
		 internal virtual AbstractIndexPartition AddNewPartition()
		 {
			  EnsureOpen();
			  File partitionFolder = CreateNewPartitionFolder();
			  Directory directory = IndexStorage.openDirectory( partitionFolder );
			  AbstractIndexPartition indexPartition = _partitionFactory.createPartition( partitionFolder, directory );
			  _partitions.add( indexPartition );
			  return indexPartition;
		 }

		 protected internal virtual void EnsureOpen()
		 {
			  if ( !_open )
			  {
					throw new System.InvalidOperationException( "Please open lucene index before working with it." );
			  }
		 }

		 protected internal virtual void EnsureNotOpen()
		 {
			  if ( _open )
			  {
					throw new System.InvalidOperationException( "Lucene index should not be open to be able to perform required " + "operation." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.List<org.Neo4Net.kernel.api.impl.index.partition.PartitionSearcher> acquireSearchers(java.util.List<org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 protected internal static IList<PartitionSearcher> AcquireSearchers( IList<AbstractIndexPartition> partitions )
		 {
			  IList<PartitionSearcher> searchers = new List<PartitionSearcher>( partitions.Count );
			  try
			  {
					foreach ( AbstractIndexPartition partition in partitions )
					{
						 searchers.Add( partition.AcquireSearcher() );
					}
					return searchers;
			  }
			  catch ( IOException e )
			  {
					IOUtils.closeAllSilently( searchers );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean luceneDirectoryExists(java.io.File folder) throws java.io.IOException
		 private bool LuceneDirectoryExists( File folder )
		 {
			  using ( Directory directory = IndexStorage.openDirectory( folder ) )
			  {
					return DirectoryReader.indexExists( directory );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNewPartitionFolder() throws java.io.IOException
		 private File CreateNewPartitionFolder()
		 {
			  File partitionFolder = IndexStorage.getPartitionFolder( _partitions.size() + 1 );
			  IndexStorage.prepareFolder( partitionFolder );
			  return partitionFolder;
		 }

		 /// <summary>
		 /// Check if this index is marked as online.
		 /// </summary>
		 /// <returns> <code>true</code> if index is online, <code>false</code> otherwise </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isOnline() throws java.io.IOException
		 public virtual bool Online
		 {
			 get
			 {
				  EnsureOpen();
				  AbstractIndexPartition partition = GetFirstPartition( Partitions );
				  Directory directory = partition.Directory;
				  using ( DirectoryReader reader = DirectoryReader.open( directory ) )
				  {
						IDictionary<string, string> userData = reader.IndexCommit.UserData;
						return ONLINE.Equals( userData[KEY_STATUS] );
				  }
			 }
		 }

		 /// <summary>
		 /// Marks index as online by including "status" -> "online" map into commit metadata of the first partition.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsOnline() throws java.io.IOException
		 public virtual void MarkAsOnline()
		 {
			  EnsureOpen();
			  AbstractIndexPartition partition = GetFirstPartition( Partitions );
			  IndexWriter indexWriter = partition.IndexWriter;
			  indexWriter.CommitData = _onlineCommitUserData;
			  Flush( false );
		 }

		 /// <summary>
		 /// Writes the given failure message to the failure storage.
		 /// </summary>
		 /// <param name="failure"> the failure message. </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsFailed(String failure) throws java.io.IOException
		 public virtual void MarkAsFailed( string failure )
		 {
			  IndexStorage.storeIndexFailure( failure );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract READER createSimpleReader(java.util.List<org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException;
		 protected internal abstract READER CreateSimpleReader( IList<AbstractIndexPartition> partitions );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract READER createPartitionedReader(java.util.List<org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException;
		 protected internal abstract READER CreatePartitionedReader( IList<AbstractIndexPartition> partitions );
	}

}