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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using CapabilityType = Neo4Net.Kernel.impl.store.format.CapabilityType;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ExplicitIndexMigrationException = Neo4Net.Upgrade.Lucene.ExplicitIndexMigrationException;
	using LuceneExplicitIndexUpgrader = Neo4Net.Upgrade.Lucene.LuceneExplicitIndexUpgrader;
	using Monitor = Neo4Net.Upgrade.Lucene.LuceneExplicitIndexUpgrader.Monitor;

	/// <summary>
	/// Migrates explicit lucene indexes between different Neo4Net versions.
	/// Participates in store upgrade as one of the migration participants.
	/// </summary>
	public class ExplicitIndexMigrator : AbstractStoreMigrationParticipant
	{
		 private const string LUCENE_EXPLICIT_INDEX_PROVIDER_NAME = "lucene";
		 private readonly ExplicitIndexProvider _explicitIndexProvider;
		 private readonly FileSystemAbstraction _fileSystem;
		 private File _migrationExplicitIndexesRoot;
		 private File _originalExplicitIndexesRoot;
		 private readonly Log _log;
		 private bool _explicitIndexMigrated;

		 public ExplicitIndexMigrator( FileSystemAbstraction fileSystem, ExplicitIndexProvider explicitIndexProvider, LogProvider logProvider ) : base( "Explicit indexes" )
		 {
			  this._fileSystem = fileSystem;
			  this._explicitIndexProvider = explicitIndexProvider;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(Neo4Net.io.layout.DatabaseLayout directoryLayout, Neo4Net.io.layout.DatabaseLayout migrationLayout, Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressMonitor, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressMonitor, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  IndexImplementation indexImplementation = _explicitIndexProvider.getProviderByName( LUCENE_EXPLICIT_INDEX_PROVIDER_NAME );
			  if ( indexImplementation != null )
			  {
					RecordFormats from = RecordFormatSelector.selectForVersion( versionToMigrateFrom );
					RecordFormats to = RecordFormatSelector.selectForVersion( versionToMigrateTo );
					if ( !from.HasCompatibleCapabilities( to, CapabilityType.INDEX ) )
					{
						 _originalExplicitIndexesRoot = indexImplementation.GetIndexImplementationDirectory( directoryLayout );
						 _migrationExplicitIndexesRoot = indexImplementation.GetIndexImplementationDirectory( migrationLayout );
						 if ( IsNotEmptyDirectory( _originalExplicitIndexesRoot ) )
						 {
							  MigrateExplicitIndexes( progressMonitor );
							  _explicitIndexMigrated = true;
						 }
					}
			  }
			  else
			  {
					_log.debug( "Lucene index provider not found, nothing to migrate." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(Neo4Net.io.layout.DatabaseLayout migrationLayout, Neo4Net.io.layout.DatabaseLayout directoryLayout, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  if ( _explicitIndexMigrated )
			  {
					_fileSystem.deleteRecursively( _originalExplicitIndexesRoot );
					_fileSystem.moveToDirectory( _migrationExplicitIndexesRoot, _originalExplicitIndexesRoot.ParentFile );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup(Neo4Net.io.layout.DatabaseLayout migrationLayout) throws java.io.IOException
		 public override void Cleanup( DatabaseLayout migrationLayout )
		 {
			  if ( IndexMigrationDirectoryExists )
			  {
					_fileSystem.deleteRecursively( _migrationExplicitIndexesRoot );
			  }
		 }

		 private bool IndexMigrationDirectoryExists
		 {
			 get
			 {
				  return _migrationExplicitIndexesRoot != null && _fileSystem.fileExists( _migrationExplicitIndexesRoot );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void migrateExplicitIndexes(Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressMonitor) throws java.io.IOException
		 private void MigrateExplicitIndexes( ProgressReporter progressMonitor )
		 {
			  try
			  {
					_fileSystem.copyRecursively( _originalExplicitIndexesRoot, _migrationExplicitIndexesRoot );
					Path indexRootPath = _migrationExplicitIndexesRoot.toPath();
					LuceneExplicitIndexUpgrader indexUpgrader = CreateLuceneExplicitIndexUpgrader( indexRootPath, progressMonitor );
					indexUpgrader.UpgradeIndexes();
			  }
			  catch ( ExplicitIndexMigrationException lime )
			  {
					_log.error( "Migration of explicit indexes failed. Index: " + lime.FailedIndexName + " can't be " + "migrated.", lime );
					throw new IOException( "Explicit index migration failed.", lime );
			  }
		 }

		 private bool IsNotEmptyDirectory( File file )
		 {
			  if ( _fileSystem.isDirectory( file ) )
			  {
					File[] files = _fileSystem.listFiles( file );
					return files != null && Files.Length > 0;
			  }
			  return false;
		 }

		 internal virtual LuceneExplicitIndexUpgrader CreateLuceneExplicitIndexUpgrader( Path indexRootPath, ProgressReporter progressMonitor )
		 {
			  return new LuceneExplicitIndexUpgrader( indexRootPath, progressMonitor( progressMonitor ) );
		 }

		 private static LuceneExplicitIndexUpgrader.Monitor ProgressMonitor( ProgressReporter progressMonitor )
		 {
			  return new MonitorAnonymousInnerClass( progressMonitor );
		 }

		 private class MonitorAnonymousInnerClass : LuceneExplicitIndexUpgrader.Monitor
		 {
			 private ProgressReporter _progressMonitor;

			 public MonitorAnonymousInnerClass( ProgressReporter progressMonitor )
			 {
				 this._progressMonitor = progressMonitor;
			 }

			 public void starting( int total )
			 {
				  _progressMonitor.start( total );
			 }

			 public void migrated( string name )
			 {
				  _progressMonitor.progress( 1 );
			 }
		 }
	}

}