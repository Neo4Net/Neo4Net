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
namespace Neo4Net.Kernel.Api.Impl.Index.builder
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;

	/// <summary>
	/// Helper builder class to simplify construction of lucene index storages.
	/// Most of the values already have most useful default value, that still can be overridden by corresponding
	/// builder methods.
	/// </summary>
	public class LuceneIndexStorageBuilder
	{
		 private DirectoryFactory _directoryFactory = DirectoryFactory.PERSISTENT;
		 private FileSystemAbstraction _fileSystem;
		 private File _indexRootFolder;
		 private PartitionedIndexStorage _indexStorage;

		 private LuceneIndexStorageBuilder()
		 {
		 }

		 /// <summary>
		 /// Create new lucene index storage builder
		 /// </summary>
		 /// <returns> index builder </returns>
		 public static LuceneIndexStorageBuilder Create()
		 {
			  return new LuceneIndexStorageBuilder();
		 }

		 /// <summary>
		 /// Build lucene index storage with specified configuration
		 /// </summary>
		 /// <returns> lucene index storage </returns>
		 public virtual PartitionedIndexStorage Build()
		 {
			  if ( _indexStorage == null )
			  {
					Objects.requireNonNull( _directoryFactory );
					Objects.requireNonNull( _fileSystem );
					Objects.requireNonNull( _indexRootFolder );
					_indexStorage = new PartitionedIndexStorage( _directoryFactory, _fileSystem, _indexRootFolder );
			  }
			  return _indexStorage;
		 }

		 /// <summary>
		 /// Specify directory factory
		 /// </summary>
		 /// <param name="directoryFactory"> directory factory </param>
		 /// <returns> index storage builder </returns>
		 public virtual LuceneIndexStorageBuilder WithDirectoryFactory( DirectoryFactory directoryFactory )
		 {
			  this._directoryFactory = directoryFactory;
			  return this;
		 }

		 /// <summary>
		 /// Specify file system abstraction
		 /// </summary>
		 /// <param name="fileSystem"> file system abstraction </param>
		 /// <returns> index storage builder </returns>
		 public virtual LuceneIndexStorageBuilder WithFileSystem( FileSystemAbstraction fileSystem )
		 {
			  this._fileSystem = fileSystem;
			  return this;
		 }

		 /// <summary>
		 /// Specify index root folder
		 /// </summary>
		 /// <param name="indexRootFolder"> root folder </param>
		 /// <returns> index storage builder </returns>
		 public virtual LuceneIndexStorageBuilder WithIndexFolder( File indexRootFolder )
		 {
			  this._indexRootFolder = indexRootFolder;
			  return this;
		 }

		 /// <summary>
		 /// Specify partitioned index storage
		 /// </summary>
		 /// <param name="indexStorage"> index storage </param>
		 /// <returns> index storage builder </returns>
		 public virtual LuceneIndexStorageBuilder WithIndexStorage( PartitionedIndexStorage indexStorage )
		 {
			  this._indexStorage = indexStorage;
			  return this;
		 }
	}

}