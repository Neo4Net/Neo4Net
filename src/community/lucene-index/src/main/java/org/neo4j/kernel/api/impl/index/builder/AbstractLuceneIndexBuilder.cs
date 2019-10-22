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

	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;

	/// <summary>
	/// Base class for lucene index builders.
	/// </summary>
	/// @param <T> actual index type </param>
	public abstract class AbstractLuceneIndexBuilder<T> where T : AbstractLuceneIndexBuilder<T>
	{
		 protected internal LuceneIndexStorageBuilder StorageBuilder = LuceneIndexStorageBuilder.Create();
		 private readonly Config _config;
		 private OperationalMode _operationalMode = OperationalMode.single;

		 public AbstractLuceneIndexBuilder( Config config )
		 {
			  this._config = Objects.requireNonNull( config );
		 }

		 /// <summary>
		 /// Specify index storage
		 /// </summary>
		 /// <param name="indexStorage"> index storage </param>
		 /// <returns> index builder </returns>
		 public virtual T WithIndexStorage( PartitionedIndexStorage indexStorage )
		 {
			  StorageBuilder.withIndexStorage( indexStorage );
			  return ( T ) this;
		 }

		 /// <summary>
		 /// Specify directory factory
		 /// </summary>
		 /// <param name="directoryFactory"> directory factory </param>
		 /// <returns> index builder </returns>
		 public virtual T WithDirectoryFactory( DirectoryFactory directoryFactory )
		 {
			  StorageBuilder.withDirectoryFactory( directoryFactory );
			  return ( T ) this;
		 }

		 /// <summary>
		 /// Specify file system abstraction
		 /// </summary>
		 /// <param name="fileSystem"> file system abstraction </param>
		 /// <returns> index builder </returns>
		 public virtual T WithFileSystem( FileSystemAbstraction fileSystem )
		 {
			  StorageBuilder.withFileSystem( fileSystem );
			  return ( T ) this;
		 }

		 /// <summary>
		 /// Specify index root folder
		 /// </summary>
		 /// <param name="indexRootFolder"> root folder </param>
		 /// <returns> index builder </returns>
		 public virtual T WithIndexRootFolder( File indexRootFolder )
		 {
			  StorageBuilder.withIndexFolder( indexRootFolder );
			  return ( T ) this;
		 }

		 /// <summary>
		 /// Specify db operational mode </summary>
		 /// <param name="operationalMode"> operational mode </param>
		 /// <returns> index builder </returns>
		 public virtual T WithOperationalMode( OperationalMode operationalMode )
		 {
			  this._operationalMode = operationalMode;
			  return ( T ) this;
		 }

		 /// <summary>
		 /// Check if index should be read only </summary>
		 /// <returns> true if index should be read only </returns>
		 protected internal virtual bool ReadOnly
		 {
			 get
			 {
				  return GetConfig( GraphDatabaseSettings.read_only ) && ( OperationalMode.single == _operationalMode );
			 }
		 }

		 /// <summary>
		 /// Lookup a config parameter. </summary>
		 /// <param name="flag"> the parameter to look up. </param>
		 /// @param <F> the type of the parameter. </param>
		 /// <returns> the value of the parameter. </returns>
		 protected internal virtual F GetConfig<F>( Setting<F> flag )
		 {
			  return _config.get( flag );
		 }
	}

}