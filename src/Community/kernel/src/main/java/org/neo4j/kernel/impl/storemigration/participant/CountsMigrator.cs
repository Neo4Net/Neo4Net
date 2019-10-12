using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsComputer = Neo4Net.Kernel.impl.store.CountsComputer;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreFailureException = Neo4Net.Kernel.impl.store.StoreFailureException;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using StoreVersion = Neo4Net.Kernel.impl.store.format.StoreVersion;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using ReadOnlyIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.ReadOnlyIdGeneratorFactory;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.RecordFormatSelector.selectForVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.FileOperation.DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.FileOperation.MOVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.participant.StoreMigratorFileOperation.fileOperation;

	/// <summary>
	/// Rebuilds the count store during migration.
	/// <para>
	/// Since the database may or may not reside in the upgrade directory, depending on whether the new format has
	/// different capabilities or not, we rebuild the count store using the information the store directory if we fail to
	/// open the store in the upgrade directory.
	/// </para>
	/// <para>
	/// Just one out of many potential participants in a <seealso cref="StoreUpgrader migration"/>.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= StoreUpgrader </seealso>
	public class CountsMigrator : AbstractStoreMigrationParticipant
	{
		 private static readonly IEnumerable<DatabaseFile> _countsStoreFiles = Iterables.iterable( DatabaseFile.COUNTS_STORE_A, DatabaseFile.COUNTS_STORE_B );

		 private readonly Config _config;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly PageCache _pageCache;
		 private bool _migrated;

		 public CountsMigrator( FileSystemAbstraction fileSystem, PageCache pageCache, Config config ) : base( "Counts store" )
		 {
			  this._fileSystem = fileSystem;
			  this._pageCache = pageCache;
			  this._config = config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(org.neo4j.io.layout.DatabaseLayout directoryLayout, org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.kernel.impl.util.monitoring.ProgressReporter progressMonitor, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressMonitor, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  if ( CountStoreRebuildRequired( versionToMigrateFrom ) )
			  {
					// create counters from scratch
					fileOperation( DELETE, _fileSystem, migrationLayout, migrationLayout, _countsStoreFiles, true, null );
					File neoStore = directoryLayout.MetadataStore();
					long lastTxId = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_ID );
					try
					{
						 RebuildCountsFromScratch( directoryLayout, migrationLayout, lastTxId, progressMonitor, versionToMigrateTo, _pageCache, NullLogProvider.Instance );
					}
					catch ( StoreFailureException )
					{
						 //This means that we did not perform a full migration, as the formats had the same capabilities. Thus
						 // we should use the store directory for information when rebuilding the count store. Note that we
						 // still put the new count store in the migration directory.
						 RebuildCountsFromScratch( directoryLayout, migrationLayout, lastTxId, progressMonitor, versionToMigrateFrom, _pageCache, NullLogProvider.Instance );
					}
					_migrated = true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToUpgradeTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToUpgradeTo )
		 {

			  if ( _migrated )
			  {
					// Delete any current count files in the store directory.
					fileOperation( DELETE, _fileSystem, directoryLayout, directoryLayout, _countsStoreFiles, true, null );
					// Move the migrated ones into the store directory
					fileOperation( MOVE, _fileSystem, migrationLayout, directoryLayout, _countsStoreFiles, true, ExistingTargetStrategy.OVERWRITE );
					// We do not need to move files with the page cache, as the count files always reside on the normal file system.
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup(org.neo4j.io.layout.DatabaseLayout migrationLayout) throws java.io.IOException
		 public override void Cleanup( DatabaseLayout migrationLayout )
		 {
			  _fileSystem.deleteRecursively( migrationLayout.DatabaseDirectory() );
		 }

		 public override string ToString()
		 {
			  return "Kernel Node Count Rebuilder";
		 }

		 internal static bool CountStoreRebuildRequired( string versionToMigrateFrom )
		 {
			  return StandardV2_3.STORE_VERSION.Equals( versionToMigrateFrom ) || StandardV3_0.STORE_VERSION.Equals( versionToMigrateFrom ) || StoreVersion.HIGH_LIMIT_V3_0_0.versionString().Equals(versionToMigrateFrom) || StoreVersion.HIGH_LIMIT_V3_0_6.versionString().Equals(versionToMigrateFrom) || StoreVersion.HIGH_LIMIT_V3_1_0.versionString().Equals(versionToMigrateFrom);
		 }

		 private void RebuildCountsFromScratch( DatabaseLayout sourceStructure, DatabaseLayout migrationStructure, long lastTxId, ProgressReporter progressMonitor, string expectedStoreVersion, PageCache pageCache, LogProvider logProvider )
		 {
			  RecordFormats recordFormats = selectForVersion( expectedStoreVersion );
			  IdGeneratorFactory idGeneratorFactory = new ReadOnlyIdGeneratorFactory( _fileSystem );
			  StoreFactory storeFactory = new StoreFactory( sourceStructure, _config, idGeneratorFactory, pageCache, _fileSystem, recordFormats, logProvider, EmptyVersionContextSupplier.EMPTY );
			  using ( NeoStores neoStores = storeFactory.OpenNeoStores( StoreType.NODE, StoreType.RELATIONSHIP, StoreType.LABEL_TOKEN, StoreType.RELATIONSHIP_TYPE_TOKEN ) )
			  {
					neoStores.VerifyStoreOk();
					NodeStore nodeStore = neoStores.NodeStore;
					RelationshipStore relationshipStore = neoStores.RelationshipStore;
					using ( Lifespan life = new Lifespan() )
					{
						 int highLabelId = ( int ) neoStores.LabelTokenStore.HighId;
						 int highRelationshipTypeId = ( int ) neoStores.RelationshipTypeTokenStore.HighId;
						 CountsComputer initializer = new CountsComputer( lastTxId, nodeStore, relationshipStore, highLabelId, highRelationshipTypeId, NumberArrayFactory.auto( pageCache, migrationStructure.DatabaseDirectory(), true, Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory_Fields.NoMonitor ), progressMonitor );
						 life.Add( ( new CountsTracker( logProvider, _fileSystem, pageCache, _config, migrationStructure, EmptyVersionContextSupplier.EMPTY ) ).setInitializer( initializer ) );
					}
			  }
		 }
	}

}