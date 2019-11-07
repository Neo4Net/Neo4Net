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
namespace Neo4Net.Kernel.impl.storemigration
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.contains;

	/// <summary>
	/// Idea is to migrate a <seealso cref="NeoStores"/> store by store, record by record in a sequential fashion for
	/// quick migration from one <seealso cref="RecordFormats"/> to another.
	/// </summary>
	public class DirectRecordStoreMigrator
	{
		 private readonly PageCache _pageCache;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Config _config;

		 public DirectRecordStoreMigrator( PageCache pageCache, FileSystemAbstraction fs, Config config )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._config = config;
		 }

		 public virtual void Migrate( DatabaseLayout fromDirectoryStructure, RecordFormats fromFormat, DatabaseLayout toDirectoryStructure, RecordFormats toFormat, ProgressReporter progressReporter, StoreType[] types, params StoreType[] additionalTypesToOpen )
		 {
			  StoreType[] storesToOpen = ArrayUtil.concat( types, additionalTypesToOpen );
			  progressReporter.Start( storesToOpen.Length );

			  try (NeoStores fromStores = new StoreFactory(fromDirectoryStructure, _config, new DefaultIdGeneratorFactory(_fs), _pageCache, _fs, fromFormat, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY)
									.openNeoStores( true, storesToOpen );
						 NeoStores toStores = ( new StoreFactory( toDirectoryStructure, WithPersistedStoreHeadersAsConfigFrom( fromStores, storesToOpen ), new DefaultIdGeneratorFactory( _fs ), _pageCache, _fs, toFormat, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openNeoStores( true, storesToOpen ))
						 {
					foreach ( StoreType type in types )
					{
						 // This condition will exclude counts store first and foremost.
						 if ( type.RecordStore )
						 {
							  Migrate( fromStores.getRecordStore( type ), toStores.GetRecordStore( type ) );
							  progressReporter.Progress( 1 );
						 }
					}
						 }
		 }

		 private static void Migrate<RECORD>( RecordStore<RECORD> from, RecordStore<RECORD> to ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  to.HighestPossibleIdInUse = from.HighestPossibleIdInUse;

			  from.ScanAllRecords(record =>
			  {
				to.PrepareForCommit( record );
				to.UpdateRecord( record );
				return false;
			  });
		 }

		 /// <summary>
		 /// Creates a configuration to include dynamic record store sizes where data size in old a new format
		 /// will be the same. This is important because we're doing a record-to-record migration and so
		 /// data which fits into one record must fit into the other as to not needing additional blocks
		 /// in the dynamic record chain.
		 /// </summary>
		 /// <param name="legacyStores"> <seealso cref="NeoStores"/> to read dynamic record data sizes from. </param>
		 /// <param name="types"> array of <seealso cref="StoreType"/> which we know that legacy stores have opened. </param>
		 /// <returns> a <seealso cref="Config"/> which mimics dynamic record data sizes from the {@code legacyStores}. </returns>
		 private Config WithPersistedStoreHeadersAsConfigFrom( NeoStores legacyStores, StoreType[] types )
		 {
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  if ( contains( types, StoreType.RELATIONSHIP_GROUP ) )
			  {
					config[GraphDatabaseSettings.dense_node_threshold.name()] = (legacyStores.RelationshipGroupStore.StoreHeaderInt).ToString();
			  }
			  if ( contains( types, StoreType.PROPERTY ) )
			  {
					config[GraphDatabaseSettings.array_block_size.name()] = (legacyStores.PropertyStore.ArrayStore.RecordDataSize).ToString();
					config[GraphDatabaseSettings.string_block_size.name()] = (legacyStores.PropertyStore.StringStore.RecordDataSize).ToString();
			  }
			  if ( contains( types, StoreType.NODE_LABEL ) )
			  {
					config[GraphDatabaseSettings.label_block_size.name()] = (legacyStores.NodeStore.DynamicLabelStore.RecordDataSize).ToString();
			  }
			  this._config.augment( config );
			  return this._config;
		 }
	}

}