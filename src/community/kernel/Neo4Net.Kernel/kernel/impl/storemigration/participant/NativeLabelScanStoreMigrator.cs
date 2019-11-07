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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using Predicates = Neo4Net.Functions.Predicates;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileHandle = Neo4Net.Io.fs.FileHandle;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using NodeLabelUpdate = Neo4Net.Kernel.Api.LabelScan.NodeLabelUpdate;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexStoreView = Neo4Net.Kernel.Impl.Api.index.IndexStoreView;
	using FullLabelStream = Neo4Net.Kernel.Impl.Api.scan.FullLabelStream;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using ReadOnlyIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.ReadOnlyIdGeneratorFactory;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.locking.LockService.NO_LOCK_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForVersion;

	public class NativeLabelScanStoreMigrator : AbstractStoreMigrationParticipant
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly PageCache _pageCache;
		 private readonly Config _config;
		 private bool _nativeLabelScanStoreMigrated;

		 public NativeLabelScanStoreMigrator( FileSystemAbstraction fileSystem, PageCache pageCache, Config config ) : base( "Native label scan index" )
		 {
			  this._fileSystem = fileSystem;
			  this._pageCache = pageCache;
			  this._config = config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(Neo4Net.io.layout.DatabaseLayout directoryLayout, Neo4Net.io.layout.DatabaseLayout migrationLayout, Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressReporter, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressReporter, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  if ( IsNativeLabelScanStoreMigrationRequired( directoryLayout ) )
			  {
					StoreFactory storeFactory = GetStoreFactory( directoryLayout, versionToMigrateFrom );
					using ( NeoStores neoStores = storeFactory.OpenAllNeoStores(), Lifespan lifespan = new Lifespan() )
					{
						 neoStores.VerifyStoreOk();
						 // Remove any existing file to ensure we always do migration
						 DeleteNativeIndexFile( migrationLayout );

						 progressReporter.Start( neoStores.NodeStore.NumberOfIdsInUse );
						 NativeLabelScanStore nativeLabelScanStore = GetNativeLabelScanStore( migrationLayout, progressReporter, neoStores );
						 lifespan.Add( nativeLabelScanStore );
					}
					_nativeLabelScanStoreMigrated = true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(Neo4Net.io.layout.DatabaseLayout migrationLayout, Neo4Net.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToMigrateTo )
		 {
			  if ( _nativeLabelScanStoreMigrated )
			  {
					File nativeLabelIndex = migrationLayout.LabelScanStore();
					MoveNativeIndexFile( directoryLayout, nativeLabelIndex );
					DeleteLuceneLabelIndex( GetLuceneStoreDirectory( directoryLayout ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteNativeIndexFile(Neo4Net.io.layout.DatabaseLayout directoryStructure) throws java.io.IOException
		 private void DeleteNativeIndexFile( DatabaseLayout directoryStructure )
		 {
			  Optional<FileHandle> indexFile = _fileSystem.streamFilesRecursive( NativeLabelScanStore.getLabelScanStoreFile( directoryStructure ) ).findFirst();

			  if ( indexFile.Present )
			  {
					try
					{
						 indexFile.get().delete();
					}
					catch ( NoSuchFileException )
					{
						 // Already deleted, ignore
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void moveNativeIndexFile(Neo4Net.io.layout.DatabaseLayout storeStructure, java.io.File nativeLabelIndex) throws java.io.IOException
		 private void MoveNativeIndexFile( DatabaseLayout storeStructure, File nativeLabelIndex )
		 {
			  Optional<FileHandle> nativeIndexFileHandle = _fileSystem.streamFilesRecursive( nativeLabelIndex ).findFirst();
			  if ( nativeIndexFileHandle.Present )
			  {
					nativeIndexFileHandle.get().rename(storeStructure.LabelScanStore());
			  }
		 }

		 private NativeLabelScanStore GetNativeLabelScanStore( DatabaseLayout migrationDirectoryStructure, ProgressReporter progressReporter, NeoStores neoStores )
		 {
			  NeoStoreIndexStoreView neoStoreIndexStoreView = new NeoStoreIndexStoreView( NO_LOCK_SERVICE, neoStores );
			  return new NativeLabelScanStore( _pageCache, migrationDirectoryStructure, _fileSystem, new MonitoredFullLabelStream( neoStoreIndexStoreView, progressReporter ), false, new Monitors(), RecoveryCleanupWorkCollector.immediate() );
		 }

		 private StoreFactory GetStoreFactory( DatabaseLayout directoryStructure, string versionToMigrateFrom )
		 {
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  RecordFormats recordFormats = selectForVersion( versionToMigrateFrom );
			  IdGeneratorFactory idGeneratorFactory = new ReadOnlyIdGeneratorFactory( _fileSystem );
			  return new StoreFactory( directoryStructure, _config, idGeneratorFactory, _pageCache, _fileSystem, recordFormats, logProvider, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isNativeLabelScanStoreMigrationRequired(Neo4Net.io.layout.DatabaseLayout directoryStructure) throws java.io.IOException
		 private bool IsNativeLabelScanStoreMigrationRequired( DatabaseLayout directoryStructure )
		 {
			  return _fileSystem.streamFilesRecursive( directoryStructure.LabelScanStore() ).noneMatch(Predicates.alwaysTrue());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteLuceneLabelIndex(java.io.File indexRootDirectory) throws java.io.IOException
		 private void DeleteLuceneLabelIndex( File indexRootDirectory )
		 {
			  _fileSystem.deleteRecursively( indexRootDirectory );
		 }

		 private static File GetLuceneStoreDirectory( DatabaseLayout directoryStructure )
		 {
			  return new File( new File( new File( directoryStructure.DatabaseDirectory(), "schema" ), "label" ), "lucene" );
		 }

		 private class MonitoredFullLabelStream : FullLabelStream
		 {

			  internal readonly ProgressReporter ProgressReporter;

			  internal MonitoredFullLabelStream( IndexStoreView indexStoreView, ProgressReporter progressReporter ) : base( indexStoreView )
			  {
					this.ProgressReporter = progressReporter;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(Neo4Net.kernel.api.labelscan.NodeLabelUpdate update) throws java.io.IOException
			  public override bool Visit( NodeLabelUpdate update )
			  {
					bool visit = base.Visit( update );
					ProgressReporter.progress( 1 );
					return visit;
			  }
		 }
	}

}