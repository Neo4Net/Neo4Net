using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.impl.storemigration
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using MigrationProgressMonitor = Org.Neo4j.Kernel.impl.storemigration.monitoring.MigrationProgressMonitor;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// A migration process to migrate <seealso cref="StoreMigrationParticipant migration participants"/>, if there's
	/// need for it, before the database fully starts. Participants can
	/// <seealso cref="addParticipant(StoreMigrationParticipant) register"/> and will be notified when it's time for migration.
	/// The migration will happen to a separate, isolated directory so that an incomplete migration will not affect
	/// the original database. Only when a successful migration has taken place the migrated store will replace
	/// the original database.
	/// <p/>
	/// Migration process at a glance:
	/// <ol>
	/// <li>Participants are asked whether or not there's a need for migration</li>
	/// <li>Those that need are asked to migrate into a separate /upgrade directory. Regardless of who actually
	/// performs migration all participants are asked to satisfy dependencies for downstream participants</li>
	/// <li>Migration is marked as migrated</li>
	/// <li>Participants are asked to move their migrated files into the source directory,
	/// replacing only the existing files, so that if only some store files needed migration the others are left intact</li>
	/// <li>Migration is completed and participant resources are closed</li>
	/// </ol>
	/// <p/>
	/// TODO walk through crash scenarios and how they are handled.
	/// </summary>
	/// <seealso cref= StoreMigrationParticipant </seealso>
	public class StoreUpgrader
	{
		 private readonly Pattern _migrationLeftoversPattern = Pattern.compile( MIGRATION_LEFT_OVERS_DIRECTORY + "(_\\d*)?" );
		 public const string MIGRATION_DIRECTORY = "upgrade";
		 public const string MIGRATION_LEFT_OVERS_DIRECTORY = "upgrade_backup";
		 private const string MIGRATION_STATUS_FILE = "_status";

		 private readonly UpgradableDatabase _upgradableDatabase;
		 private readonly MigrationProgressMonitor _progressMonitor;
		 private readonly IList<StoreMigrationParticipant> _participants = new List<StoreMigrationParticipant>();
		 private readonly Config _config;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly PageCache _pageCache;
		 private readonly Log _log;
		 private readonly LogProvider _logProvider;

		 public StoreUpgrader( UpgradableDatabase upgradableDatabase, MigrationProgressMonitor progressMonitor, Config config, FileSystemAbstraction fileSystem, PageCache pageCache, LogProvider logProvider )
		 {
			  this._upgradableDatabase = upgradableDatabase;
			  this._progressMonitor = progressMonitor;
			  this._fileSystem = fileSystem;
			  this._config = config;
			  this._pageCache = pageCache;
			  this._logProvider = logProvider;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 /// <summary>
		 /// Add migration participant into a participants list.
		 /// Participant will be added into the end of a list and will be executed only after all predecessors. </summary>
		 /// <param name="participant"> - participant to add into migration </param>
		 public virtual void AddParticipant( StoreMigrationParticipant participant )
		 {
			  Debug.Assert( participant != null );
			  if ( !StoreMigrationParticipant_Fields.NotParticipating.Equals( participant ) )
			  {
					this._participants.Add( participant );
			  }
		 }

		 public virtual void MigrateIfNeeded( DatabaseLayout layout )
		 {
			  DatabaseLayout migrationStructure = DatabaseLayout.of( layout.DatabaseDirectory(), MIGRATION_DIRECTORY );

			  CleanupLegacyLeftOverDirsIn( layout.DatabaseDirectory() );

			  File migrationStateFile = migrationStructure.File( MIGRATION_STATUS_FILE );
			  // if migration directory exists than we might have failed to move files into the store dir so do it again
			  if ( _upgradableDatabase.hasCurrentVersion( layout ) && !_fileSystem.fileExists( migrationStateFile ) )
			  {
					// No migration needed
					return;
			  }

			  if ( UpgradeAllowed )
			  {
					MigrateStore( layout, migrationStructure, migrationStateFile );
			  }
			  else if ( !RecordFormatSelector.isStoreAndConfigFormatsCompatible( _config, layout, _fileSystem, _pageCache, _logProvider ) )
			  {
					throw new UpgradeNotAllowedByConfigurationException();
			  }
		 }

		 private void MigrateStore( DatabaseLayout dbDirectoryLayout, DatabaseLayout migrationLayout, File migrationStateFile )
		 {
			  // One or more participants would like to do migration
			  _progressMonitor.started( _participants.Count );

			  MigrationStatus migrationStatus = MigrationStatus.readMigrationStatus( _fileSystem, migrationStateFile );
			  string versionToMigrateFrom = null;
			  // We don't need to migrate if we're at the phase where we have migrated successfully
			  // and it's just a matter of moving over the files to the storeDir.
			  if ( MigrationStatus.Migrating.isNeededFor( migrationStatus ) )
			  {
					versionToMigrateFrom = _upgradableDatabase.checkUpgradable( dbDirectoryLayout ).storeVersion();
					CleanMigrationDirectory( migrationLayout.DatabaseDirectory() );
					MigrationStatus.Migrating.setMigrationStatus( _fileSystem, migrationStateFile, versionToMigrateFrom );
					MigrateToIsolatedDirectory( dbDirectoryLayout, migrationLayout, versionToMigrateFrom );
					MigrationStatus.Moving.setMigrationStatus( _fileSystem, migrationStateFile, versionToMigrateFrom );
			  }

			  if ( MigrationStatus.Moving.isNeededFor( migrationStatus ) )
			  {
					versionToMigrateFrom = MigrationStatus.Moving.maybeReadInfo( _fileSystem, migrationStateFile, versionToMigrateFrom );
					MoveMigratedFilesToStoreDirectory( _participants, migrationLayout, dbDirectoryLayout, versionToMigrateFrom, _upgradableDatabase.currentVersion() );
			  }

			  Cleanup( _participants, migrationLayout );

			  _progressMonitor.completed();
		 }

		 internal virtual IList<StoreMigrationParticipant> Participants
		 {
			 get
			 {
				  return _participants;
			 }
		 }

		 private bool UpgradeAllowed
		 {
			 get
			 {
				  return _config.get( GraphDatabaseSettings.allow_upgrade );
			 }
		 }

		 private void CleanupLegacyLeftOverDirsIn( File databaseDirectory )
		 {
			  File[] leftOverDirs = databaseDirectory.listFiles( ( file, name ) => file.Directory && _migrationLeftoversPattern.matcher( name ).matches() );
			  if ( leftOverDirs != null )
			  {
					foreach ( File leftOverDir in leftOverDirs )
					{
						 DeleteSilently( leftOverDir );
					}
			  }
		 }

		 private static void Cleanup( IEnumerable<StoreMigrationParticipant> participants, DatabaseLayout migrationStructure )
		 {
			  try
			  {
					foreach ( StoreMigrationParticipant participant in participants )
					{
						 participant.Cleanup( migrationStructure );
					}
			  }
			  catch ( IOException e )
			  {
					throw new UnableToUpgradeException( "Failure cleaning up after migration", e );
			  }
		 }

		 private static void MoveMigratedFilesToStoreDirectory( IEnumerable<StoreMigrationParticipant> participants, DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  try
			  {
					foreach ( StoreMigrationParticipant participant in participants )
					{
						 participant.MoveMigratedFiles( migrationLayout, directoryLayout, versionToMigrateFrom, versionToMigrateTo );
					}
			  }
			  catch ( IOException e )
			  {
					throw new UnableToUpgradeException( "Unable to move migrated files into place", e );
			  }
		 }

		 private void MigrateToIsolatedDirectory( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, string versionToMigrateFrom )
		 {
			  try
			  {
					foreach ( StoreMigrationParticipant participant in _participants )
					{
						 ProgressReporter progressReporter = _progressMonitor.startSection( participant.Name );
						 participant.Migrate( directoryLayout, migrationLayout, progressReporter, versionToMigrateFrom, _upgradableDatabase.currentVersion() );
						 progressReporter.Completed();
					}
			  }
			  catch ( Exception e ) when ( e is IOException || e is UncheckedIOException )
			  {
					throw new UnableToUpgradeException( "Failure doing migration", e );
			  }
		 }

		 private void CleanMigrationDirectory( File migrationDirectory )
		 {
			  try
			  {
					if ( _fileSystem.fileExists( migrationDirectory ) )
					{
						 _fileSystem.deleteRecursively( migrationDirectory );
					}
			  }
			  catch ( Exception e ) when ( e is IOException || e is UncheckedIOException )
			  {
					throw new UnableToUpgradeException( "Failure deleting upgrade directory " + migrationDirectory, e );
			  }
			  _fileSystem.mkdir( migrationDirectory );
		 }

		 private void DeleteSilently( File dir )
		 {
			  try
			  {
					_fileSystem.deleteRecursively( dir );
			  }
			  catch ( IOException e )
			  {
					_log.error( "Unable to delete directory: " + dir, e );
			  }
		 }

		 public class UnableToUpgradeException : Exception
		 {
			  public UnableToUpgradeException( string message, Exception cause ) : base( message, cause )
			  {
			  }

			  internal UnableToUpgradeException( string message ) : base( message )
			  {
			  }
		 }

		 internal class UpgradeMissingStoreFilesException : UnableToUpgradeException
		 {
			  internal const string MESSAGE = "Missing required store file '%s'.";

			  internal UpgradeMissingStoreFilesException( string filenameExpectedToExist ) : base( string.format( MESSAGE, filenameExpectedToExist ) )
			  {
			  }
		 }

		 internal class UpgradingStoreVersionNotFoundException : UnableToUpgradeException
		 {
			  internal static readonly string Message = "'%s' does not contain a store version, please ensure that the original database was shut down in a " +
						 "clean state.";

			  internal UpgradingStoreVersionNotFoundException( string filenameWithoutStoreVersion ) : base( string.format( Message, filenameWithoutStoreVersion ) )
			  {
			  }
		 }

		 public class UnexpectedUpgradingStoreVersionException : UnableToUpgradeException
		 {
			  internal const string MESSAGE = "Not possible to upgrade a store with version '%s' to current store version `%s` (Neo4j %s).";

			  internal UnexpectedUpgradingStoreVersionException( string fileVersion, string currentVersion ) : base( string.format( MESSAGE, fileVersion, currentVersion, Version.Neo4jVersion ) )
			  {
			  }
		 }

		 public class AttemptedDowngradeException : UnableToUpgradeException
		 {
			  internal const string MESSAGE = "Downgrading stores are not supported.";

			  internal AttemptedDowngradeException() : base(MESSAGE)
			  {
			  }
		 }

		 public class UnexpectedUpgradingStoreFormatException : UnableToUpgradeException
		 {
			  protected internal const string MESSAGE = "This is an enterprise-only store. Please configure '%s' to open.";

			  internal UnexpectedUpgradingStoreFormatException() : base(string.format(MESSAGE, GraphDatabaseSettings.record_format.name()))
			  {
			  }
		 }

		 internal class DatabaseNotCleanlyShutDownException : UnableToUpgradeException
		 {
			  internal const string MESSAGE = "The database is not cleanly shutdown. The database needs recovery, in order to recover the database, "
						 + "please run the old version of the database on this store.";

			  internal DatabaseNotCleanlyShutDownException() : base(MESSAGE)
			  {
			  }
		 }
	}

}