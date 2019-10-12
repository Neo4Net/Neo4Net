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
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{

	using Predicates = Org.Neo4j.Function.Predicates;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileHandle = Org.Neo4j.Io.fs.FileHandle;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using FullLabelStream = Org.Neo4j.Kernel.Impl.Api.scan.FullLabelStream;
	using NativeLabelScanStore = Org.Neo4j.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using ReadOnlyIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.ReadOnlyIdGeneratorFactory;
	using NeoStoreIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.LockService.NO_LOCK_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.RecordFormatSelector.selectForVersion;

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
//ORIGINAL LINE: public void migrate(org.neo4j.io.layout.DatabaseLayout directoryLayout, org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.kernel.impl.util.monitoring.ProgressReporter progressReporter, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
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
//ORIGINAL LINE: public void moveMigratedFiles(org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToMigrateTo) throws java.io.IOException
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
//ORIGINAL LINE: private void deleteNativeIndexFile(org.neo4j.io.layout.DatabaseLayout directoryStructure) throws java.io.IOException
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
//ORIGINAL LINE: private void moveNativeIndexFile(org.neo4j.io.layout.DatabaseLayout storeStructure, java.io.File nativeLabelIndex) throws java.io.IOException
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
//ORIGINAL LINE: private boolean isNativeLabelScanStoreMigrationRequired(org.neo4j.io.layout.DatabaseLayout directoryStructure) throws java.io.IOException
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
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.api.labelscan.NodeLabelUpdate update) throws java.io.IOException
			  public override bool Visit( NodeLabelUpdate update )
			  {
					bool visit = base.Visit( update );
					ProgressReporter.progress( 1 );
					return visit;
			  }
		 }
	}

}