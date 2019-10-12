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
namespace Org.Neo4j.Kernel.impl.store
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormatPropertyConfigurator = Org.Neo4j.Kernel.impl.store.format.RecordFormatPropertyConfigurator;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.RecordFormatSelector.selectForStoreOrConfig;

	/// <summary>
	/// Factory for Store implementations. Can also be used to create empty stores.
	/// </summary>
	public class StoreFactory
	{
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly Config _config;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly LogProvider _logProvider;
		 private readonly PageCache _pageCache;
		 private readonly RecordFormats _recordFormats;
		 private readonly OpenOption[] _openOptions;
		 private readonly VersionContextSupplier _versionContextSupplier;

		 public StoreFactory( DatabaseLayout directoryStructure, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, FileSystemAbstraction fileSystemAbstraction, LogProvider logProvider, VersionContextSupplier versionContextSupplier ) : this( directoryStructure, config, idGeneratorFactory, pageCache, fileSystemAbstraction, selectForStoreOrConfig( config, directoryStructure, fileSystemAbstraction, pageCache, logProvider ), logProvider, versionContextSupplier )
		 {
		 }

		 public StoreFactory( DatabaseLayout databaseLayout, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, FileSystemAbstraction fileSystemAbstraction, RecordFormats recordFormats, LogProvider logProvider, VersionContextSupplier versionContextSupplier, params OpenOption[] openOptions )
		 {
			  this._databaseLayout = databaseLayout;
			  this._config = config;
			  this._idGeneratorFactory = idGeneratorFactory;
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._versionContextSupplier = versionContextSupplier;
			  this._recordFormats = recordFormats;
			  this._openOptions = openOptions;
			  ( new RecordFormatPropertyConfigurator( recordFormats, config ) ).configure();

			  this._logProvider = logProvider;
			  this._pageCache = pageCache;
		 }

		 /// <summary>
		 /// Open <seealso cref="NeoStores"/> with all possible stores. If some store does not exist it will <b>not</b> be created. </summary>
		 /// <returns> container with all opened stores </returns>
		 public virtual NeoStores OpenAllNeoStores()
		 {
			  return openNeoStores( false, StoreType.values() );
		 }

		 /// <summary>
		 /// Open <seealso cref="NeoStores"/> with all possible stores with a possibility to create store if it not exist. </summary>
		 /// <param name="createStoreIfNotExists"> - should store be created if it's not exist </param>
		 /// <returns> container with all opened stores </returns>
		 public virtual NeoStores OpenAllNeoStores( bool createStoreIfNotExists )
		 {
			  return openNeoStores( createStoreIfNotExists, StoreType.values() );
		 }

		 /// <summary>
		 /// Open <seealso cref="NeoStores"/> for requested and store types. If requested store depend from non request store,
		 /// it will be automatically opened as well.
		 /// If some store does not exist it will <b>not</b> be created. </summary>
		 /// <param name="storeTypes"> - types of stores to be opened. </param>
		 /// <returns> container with opened stores </returns>
		 public virtual NeoStores OpenNeoStores( params StoreType[] storeTypes )
		 {
			  return OpenNeoStores( false, storeTypes );
		 }

		 /// <summary>
		 /// Open <seealso cref="NeoStores"/> for requested and store types. If requested store depend from non request store,
		 /// it will be automatically opened as well. </summary>
		 /// <param name="createStoreIfNotExists"> - should store be created if it's not exist </param>
		 /// <param name="storeTypes"> - types of stores to be opened. </param>
		 /// <returns> container with opened stores </returns>
		 public virtual NeoStores OpenNeoStores( bool createStoreIfNotExists, params StoreType[] storeTypes )
		 {
			  if ( createStoreIfNotExists )
			  {
					try
					{
						 _fileSystemAbstraction.mkdirs( _databaseLayout.databaseDirectory() );
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( "Could not create database directory: " + _databaseLayout.databaseDirectory(), e );
					}
			  }
			  return new NeoStores( _databaseLayout, _config, _idGeneratorFactory, _pageCache, _logProvider, _fileSystemAbstraction, _versionContextSupplier, _recordFormats, createStoreIfNotExists, storeTypes, _openOptions );
		 }
	}

}