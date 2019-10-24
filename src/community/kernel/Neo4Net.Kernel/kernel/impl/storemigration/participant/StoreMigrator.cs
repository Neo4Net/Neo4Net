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
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordNodeCursor = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordNodeCursor;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using Neo4Net.Kernel.impl.store;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreHeader = Neo4Net.Kernel.impl.store.StoreHeader;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using CapabilityType = Neo4Net.Kernel.impl.store.format.CapabilityType;
	using FormatFamily = Neo4Net.Kernel.impl.store.format.FormatFamily;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using MetaDataRecordFormat = Neo4Net.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using ReadOnlyIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.ReadOnlyIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using SilentProgressReporter = Neo4Net.Kernel.impl.util.monitoring.SilentProgressReporter;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StorageRelationshipScanCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor;
	using AdditionalInitialIds = Neo4Net.@unsafe.Impl.Batchimport.AdditionalInitialIds;
	using BatchImporter = Neo4Net.@unsafe.Impl.Batchimport.BatchImporter;
	using BatchImporterFactory = Neo4Net.@unsafe.Impl.Batchimport.BatchImporterFactory;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.Configuration;
	using InputIterable = Neo4Net.@unsafe.Impl.Batchimport.InputIterable;
	using InputIterator = Neo4Net.@unsafe.Impl.Batchimport.InputIterator;
	using IdMappers = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMappers;
	using Collectors = Neo4Net.@unsafe.Impl.Batchimport.input.Collectors;
	using Input_Estimates = Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;
	using Inputs = Neo4Net.@unsafe.Impl.Batchimport.input.Inputs;
	using CoarseBoundedProgressExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.CoarseBoundedProgressExecutionMonitor;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.standard.MetaDataRecordFormat.FIELD_NOT_PRESENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storemigration.FileOperation.COPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storemigration.FileOperation.DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storemigration.FileOperation.MOVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storemigration.participant.StoreMigratorFileOperation.fileOperation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_LOG_BYTE_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_CHECKSUM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Inputs.knownEstimates;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.staging.ExecutionSupervisors.withDynamicProcessorAssignment;

	/// <summary>
	/// Migrates a Neo4Net kernel database from one version to the next.
	/// <para>
	/// Since only one store migration is supported at any given version (migration from the previous store version)
	/// the migration code is specific for the current upgrade and changes with each store format version.
	/// </para>
	/// <para>
	/// Just one out of many potential participants in a <seealso cref="StoreUpgrader migration"/>.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= StoreUpgrader </seealso>
	public class StoreMigrator : AbstractStoreMigrationParticipant
	{
		 private const char TX_LOG_COUNTERS_SEPARATOR = 'A';

		 private readonly Config _config;
		 private readonly LogService _logService;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly PageCache _pageCache;
		 private readonly IJobScheduler _jobScheduler;

		 public StoreMigrator( FileSystemAbstraction fileSystem, PageCache pageCache, Config config, LogService logService, IJobScheduler jobScheduler ) : base( "Store files" )
		 {
			  this._fileSystem = fileSystem;
			  this._pageCache = pageCache;
			  this._config = config;
			  this._logService = logService;
			  this._jobScheduler = jobScheduler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(org.Neo4Net.io.layout.DatabaseLayout directoryLayout, org.Neo4Net.io.layout.DatabaseLayout migrationLayout, org.Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressReporter, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressReporter, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  // Extract information about the last transaction from legacy neostore
			  File neoStore = directoryLayout.MetadataStore();
			  long lastTxId = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_ID );
			  TransactionId lastTxInfo = ExtractTransactionIdInformation( neoStore, lastTxId );
			  LogPosition lastTxLogPosition = ExtractTransactionLogPosition( neoStore, directoryLayout, lastTxId );
			  // Write the tx checksum to file in migrationStructure, because we need it later when moving files into storeDir
			  WriteLastTxInformation( migrationLayout, lastTxInfo );
			  WriteLastTxLogPosition( migrationLayout, lastTxLogPosition );

			  if ( versionToMigrateFrom.Equals( "vE.H.0" ) )
			  {
					// NOTE for 3.0 here is a special case for vE.H.0 "from" record format.
					// Legend has it that 3.0.5 enterprise changed store format without changing store version.
					// This was done to cheat the migrator to avoid doing store migration since the
					// format itself was backwards compatible. Immediately a problem was detected:
					// if a user uses 3.0.5 for a while and then goes back to a previous 3.0.x patch release
					// the db wouldn't recognize it was an incompatible downgrade and start up normally,
					// but read records with scrambled values and pointers, sort of.
					//
					// This condition has two functions:
					//  1. preventing actual store migration between vE.H.0 --> vE.H.0b
					//  2. making vE.H.0b used in any migration where either vE.H.0 or vE.H.0b is the existing format,
					//     this because vE.H.0b is a superset of vE.H.0 and sometimes (for 3.0.5) vE.H.0
					//     actually means vE.H.0b (in later version).
					//
					// In later versions of Neo4Net there are better mechanics in place so that a non-migration like this
					// can be performed w/o special casing. To not require backporting that functionality
					// this condition is here and should be removed in 3.1.
					versionToMigrateFrom = "vE.H.0b";
			  }
			  RecordFormats oldFormat = selectForVersion( versionToMigrateFrom );
			  RecordFormats newFormat = selectForVersion( versionToMigrateTo );
			  if ( FormatFamily.isHigherFamilyFormat( newFormat, oldFormat ) || ( FormatFamily.isSameFamily( oldFormat, newFormat ) && IsDifferentCapabilities( oldFormat, newFormat ) ) )
			  {
					// TODO if this store has relationship indexes then warn user about that they will be incorrect
					// after migration, because now we're rewriting the relationship ids.

					// Some form of migration is required (a fallback/catch-all option)
					MigrateWithBatchImporter( directoryLayout, migrationLayout, lastTxId, lastTxInfo.Checksum(), lastTxLogPosition.LogVersion, lastTxLogPosition.ByteOffset, progressReporter, oldFormat, newFormat );
			  }
			  // update necessary neostore records
			  LogPosition logPosition = ReadLastTxLogPosition( migrationLayout );
			  UpdateOrAddNeoStoreFieldsAsPartOfMigration( migrationLayout, directoryLayout, versionToMigrateTo, logPosition );
		 }

		 private static bool IsDifferentCapabilities( RecordFormats oldFormat, RecordFormats newFormat )
		 {
			  return !oldFormat.HasCompatibleCapabilities( newFormat, CapabilityType.FORMAT );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeLastTxInformation(org.Neo4Net.io.layout.DatabaseLayout migrationStructure, org.Neo4Net.kernel.impl.store.TransactionId txInfo) throws java.io.IOException
		 internal virtual void WriteLastTxInformation( DatabaseLayout migrationStructure, TransactionId txInfo )
		 {
			  WriteTxLogCounters( _fileSystem, LastTxInformationFile( migrationStructure ), txInfo.TransactionIdConflict(), txInfo.Checksum(), txInfo.CommitTimestamp() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeLastTxLogPosition(org.Neo4Net.io.layout.DatabaseLayout migrationStructure, org.Neo4Net.kernel.impl.transaction.log.LogPosition lastTxLogPosition) throws java.io.IOException
		 internal virtual void WriteLastTxLogPosition( DatabaseLayout migrationStructure, LogPosition lastTxLogPosition )
		 {
			  WriteTxLogCounters( _fileSystem, LastTxLogPositionFile( migrationStructure ), lastTxLogPosition.LogVersion, lastTxLogPosition.ByteOffset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.store.TransactionId readLastTxInformation(org.Neo4Net.io.layout.DatabaseLayout migrationStructure) throws java.io.IOException
		 internal virtual TransactionId ReadLastTxInformation( DatabaseLayout migrationStructure )
		 {
			  long[] counters = ReadTxLogCounters( _fileSystem, LastTxInformationFile( migrationStructure ), 3 );
			  return new TransactionId( counters[0], counters[1], counters[2] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.log.LogPosition readLastTxLogPosition(org.Neo4Net.io.layout.DatabaseLayout migrationStructure) throws java.io.IOException
		 internal virtual LogPosition ReadLastTxLogPosition( DatabaseLayout migrationStructure )
		 {
			  long[] counters = ReadTxLogCounters( _fileSystem, LastTxLogPositionFile( migrationStructure ), 2 );
			  return new LogPosition( counters[0], counters[1] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeTxLogCounters(org.Neo4Net.io.fs.FileSystemAbstraction fs, java.io.File file, long... counters) throws java.io.IOException
		 private static void WriteTxLogCounters( FileSystemAbstraction fs, File file, params long[] counters )
		 {
			  using ( Writer writer = fs.OpenAsWriter( file, StandardCharsets.UTF_8, false ) )
			  {
					writer.write( StringUtils.join( counters, TX_LOG_COUNTERS_SEPARATOR ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long[] readTxLogCounters(org.Neo4Net.io.fs.FileSystemAbstraction fs, java.io.File file, int numberOfCounters) throws java.io.IOException
		 private static long[] ReadTxLogCounters( FileSystemAbstraction fs, File file, int numberOfCounters )
		 {
			  using ( StreamReader reader = new StreamReader( fs.OpenAsReader( file, StandardCharsets.UTF_8 ) ) )
			  {
					string line = reader.ReadLine();
					string[] split = StringUtils.Split( line, TX_LOG_COUNTERS_SEPARATOR );
					if ( split.Length != numberOfCounters )
					{
						 throw new System.ArgumentException( "Unexpected number of tx counters '" + numberOfCounters + "', file contains: '" + line + "'" );
					}
					long[] counters = new long[numberOfCounters];
					for ( int i = 0; i < split.Length; i++ )
					{
						 counters[i] = long.Parse( split[i] );
					}
					return counters;
			  }
		 }

		 private static File LastTxInformationFile( DatabaseLayout migrationStructure )
		 {
			  return migrationStructure.File( "lastxinformation" );
		 }

		 private static File LastTxLogPositionFile( DatabaseLayout migrationStructure )
		 {
			  return migrationStructure.File( "lastxlogposition" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.store.TransactionId extractTransactionIdInformation(java.io.File neoStore, long lastTransactionId) throws java.io.IOException
		 internal virtual TransactionId ExtractTransactionIdInformation( File neoStore, long lastTransactionId )
		 {
			  long checksum = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_CHECKSUM );
			  long commitTimestamp = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_COMMIT_TIMESTAMP );
			  if ( checksum != FIELD_NOT_PRESENT && commitTimestamp != FIELD_NOT_PRESENT )
			  {
					return new TransactionId( lastTransactionId, checksum, commitTimestamp );
			  }

			  return SpecificTransactionInformationSupplier( lastTransactionId );
		 }

		 /// <summary>
		 /// In case if we can't find information about transaction in logs we will create new transaction
		 /// information record.
		 /// Those should be used <b>only</b> in case if we do not have any transaction logs available during
		 /// migration.
		 /// 
		 /// Logs can be absent in two possible scenarios:
		 /// <ol>
		 ///     <li>We do not have any logs since there were not transaction.</li>
		 ///     <li>Logs are missing.</li>
		 /// </ol>
		 /// For both of those cases specific informational records will be produced.
		 /// </summary>
		 /// <param name="lastTransactionId"> last committed transaction id </param>
		 /// <returns> supplier of custom id records. </returns>
		 private static TransactionId SpecificTransactionInformationSupplier( long lastTransactionId )
		 {
			  return lastTransactionId == Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID ? new TransactionId( lastTransactionId, BASE_TX_CHECKSUM, BASE_TX_COMMIT_TIMESTAMP ) : new TransactionId( lastTransactionId, UNKNOWN_TX_CHECKSUM, UNKNOWN_TX_COMMIT_TIMESTAMP );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.log.LogPosition extractTransactionLogPosition(java.io.File neoStore, org.Neo4Net.io.layout.DatabaseLayout sourceDirectoryStructure, long lastTxId) throws java.io.IOException
		 internal virtual LogPosition ExtractTransactionLogPosition( File neoStore, DatabaseLayout sourceDirectoryStructure, long lastTxId )
		 {
			  long lastClosedTxLogVersion = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_VERSION );
			  long lastClosedTxLogByteOffset = MetaDataStore.getRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET );
			  if ( lastClosedTxLogVersion != MetaDataRecordFormat.FIELD_NOT_PRESENT && lastClosedTxLogByteOffset != MetaDataRecordFormat.FIELD_NOT_PRESENT )
			  {
					return new LogPosition( lastClosedTxLogVersion, lastClosedTxLogByteOffset );
			  }

			  // The legacy store we're migrating doesn't have this record in neostore so try to extract it from tx log
			  if ( lastTxId == Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID )
			  {
					return new LogPosition( BASE_TX_LOG_VERSION, BASE_TX_LOG_BYTE_OFFSET );
			  }

			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( sourceDirectoryStructure, _fileSystem, _pageCache ).withConfig( _config ).build();
			  long logVersion = logFiles.HighestLogVersion;
			  if ( logVersion == -1 )
			  {
					return new LogPosition( BASE_TX_LOG_VERSION, BASE_TX_LOG_BYTE_OFFSET );
			  }
			  long offset = _fileSystem.getFileSize( logFiles.HighestLogFile );
			  return new LogPosition( logVersion, offset );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void migrateWithBatchImporter(org.Neo4Net.io.layout.DatabaseLayout sourceDirectoryStructure, org.Neo4Net.io.layout.DatabaseLayout migrationDirectoryStructure, long lastTxId, long lastTxChecksum, long lastTxLogVersion, long lastTxLogByteOffset, org.Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressReporter, org.Neo4Net.kernel.impl.store.format.RecordFormats oldFormat, org.Neo4Net.kernel.impl.store.format.RecordFormats newFormat) throws java.io.IOException
		 private void MigrateWithBatchImporter( DatabaseLayout sourceDirectoryStructure, DatabaseLayout migrationDirectoryStructure, long lastTxId, long lastTxChecksum, long lastTxLogVersion, long lastTxLogByteOffset, ProgressReporter progressReporter, RecordFormats oldFormat, RecordFormats newFormat )
		 {
			  PrepareBatchImportMigration( sourceDirectoryStructure, migrationDirectoryStructure, oldFormat, newFormat );

			  bool requiresDynamicStoreMigration = !newFormat.Dynamic().Equals(oldFormat.Dynamic());
			  bool requiresPropertyMigration = !newFormat.Property().Equals(oldFormat.Property()) || requiresDynamicStoreMigration;
			  File badFile = sourceDirectoryStructure.File( Neo4Net.@unsafe.Impl.Batchimport.Configuration_Fields.BAD_FILE_NAME );
			  using ( NeoStores legacyStore = InstantiateLegacyStore( oldFormat, sourceDirectoryStructure ), Stream badOutput = new BufferedOutputStream( new FileStream( badFile, false ) ) )
			  {
					Configuration importConfig = new Configuration_OverriddenAnonymousInnerClass( this, _config, sourceDirectoryStructure );
					AdditionalInitialIds additionalInitialIds = ReadAdditionalIds( lastTxId, lastTxChecksum, lastTxLogVersion, lastTxLogByteOffset );

					// We have to make sure to keep the token ids if we're migrating properties/labels
					BatchImporter importer = BatchImporterFactory.withHighestPriority().instantiate(migrationDirectoryStructure, _fileSystem, _pageCache, importConfig, _logService, withDynamicProcessorAssignment(MigrationBatchImporterMonitor(legacyStore, progressReporter, importConfig), importConfig), additionalInitialIds, _config, newFormat, NO_MONITOR, _jobScheduler);
					InputIterable nodes = () => LegacyNodesAsInput(legacyStore, requiresPropertyMigration);
					InputIterable relationships = () => LegacyRelationshipsAsInput(legacyStore, requiresPropertyMigration);
					long propertyStoreSize = StoreSize( legacyStore.PropertyStore ) / 2 + StoreSize( legacyStore.PropertyStore.StringStore ) / 2 + StoreSize( legacyStore.PropertyStore.ArrayStore ) / 2;
					Input_Estimates estimates = knownEstimates( legacyStore.NodeStore.NumberOfIdsInUse, legacyStore.RelationshipStore.NumberOfIdsInUse, legacyStore.PropertyStore.NumberOfIdsInUse, legacyStore.PropertyStore.NumberOfIdsInUse, propertyStoreSize / 2, propertyStoreSize / 2, 0 );
					importer.DoImport( Inputs.input( nodes, relationships, IdMappers.actual(), Collectors.badCollector(badOutput, 0), estimates ) );

					// During migration the batch importer doesn't necessarily writes all entities, depending on
					// which stores needs migration. Node, relationship, relationship group stores are always written
					// anyways and cannot be avoided with the importer, but delete the store files that weren't written
					// (left empty) so that we don't overwrite those in the real store directory later.
					ICollection<DatabaseFile> storesToDeleteFromMigratedDirectory = new List<DatabaseFile>();
					storesToDeleteFromMigratedDirectory.Add( DatabaseFile.METADATA_STORE );
					if ( !requiresPropertyMigration )
					{
						 // We didn't migrate properties, so the property stores in the migrated store are just empty/bogus
						 storesToDeleteFromMigratedDirectory.addAll( asList( DatabaseFile.PROPERTY_STORE, DatabaseFile.PROPERTY_STRING_STORE, DatabaseFile.PROPERTY_ARRAY_STORE ) );
					}
					if ( !requiresDynamicStoreMigration )
					{
						 // We didn't migrate labels (dynamic node labels) or any other dynamic store
						 storesToDeleteFromMigratedDirectory.addAll( asList( DatabaseFile.NODE_LABEL_STORE, DatabaseFile.LABEL_TOKEN_STORE, DatabaseFile.LABEL_TOKEN_NAMES_STORE, DatabaseFile.RELATIONSHIP_TYPE_TOKEN_STORE, DatabaseFile.RELATIONSHIP_TYPE_TOKEN_NAMES_STORE, DatabaseFile.PROPERTY_KEY_TOKEN_STORE, DatabaseFile.PROPERTY_KEY_TOKEN_NAMES_STORE, DatabaseFile.SCHEMA_STORE ) );
					}
					fileOperation( DELETE, _fileSystem, migrationDirectoryStructure, migrationDirectoryStructure, storesToDeleteFromMigratedDirectory, true, null );
			  }
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration_Overridden
		 {
			 private readonly StoreMigrator _outerInstance;

			 private DatabaseLayout _sourceDirectoryStructure;

			 public Configuration_OverriddenAnonymousInnerClass( StoreMigrator outerInstance, Config config, DatabaseLayout sourceDirectoryStructure ) : base( config )
			 {
				 this.outerInstance = outerInstance;
				 this._sourceDirectoryStructure = sourceDirectoryStructure;
			 }

			 public override bool highIO()
			 {
				  return FileUtils.highIODevice( _sourceDirectoryStructure.databaseDirectory().toPath(), base.highIO() );
			 }
		 }

		 private static long StoreSize<T1>( CommonAbstractStore<T1> store ) where T1 : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return store.NumberOfIdsInUse * store.RecordSize;
		 }

		 private NeoStores InstantiateLegacyStore( RecordFormats format, DatabaseLayout directoryStructure )
		 {
			  return ( new StoreFactory( directoryStructure, _config, new ReadOnlyIdGeneratorFactory(), _pageCache, _fileSystem, format, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openAllNeoStores(true);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareBatchImportMigration(org.Neo4Net.io.layout.DatabaseLayout sourceDirectoryStructure, org.Neo4Net.io.layout.DatabaseLayout migrationStrcuture, org.Neo4Net.kernel.impl.store.format.RecordFormats oldFormat, org.Neo4Net.kernel.impl.store.format.RecordFormats newFormat) throws java.io.IOException
		 private void PrepareBatchImportMigration( DatabaseLayout sourceDirectoryStructure, DatabaseLayout migrationStrcuture, RecordFormats oldFormat, RecordFormats newFormat )
		 {
			  CreateStore( migrationStrcuture, newFormat );

			  // We use the batch importer for migrating the data, and we use it in a special way where we only
			  // rewrite the stores that have actually changed format. We know that to be node and relationship
			  // stores. Although since the batch importer also populates the counts store, all labels need to
			  // be read, i.e. both inlined and those existing in dynamic records. That's why we need to copy
			  // that dynamic record store over before doing the "batch import".
			  //   Copying this file just as-is assumes that the format hasn't change. If that happens we're in
			  // a different situation, where we first need to migrate this file.

			  // The token stores also need to be migrated because we use those as-is and ask for their high ids
			  // when using the importer in the store migration scenario.
			  DatabaseFile[] storesFilesToMigrate = new DatabaseFile[] { DatabaseFile.LABEL_TOKEN_STORE, DatabaseFile.LABEL_TOKEN_NAMES_STORE, DatabaseFile.PROPERTY_KEY_TOKEN_STORE, DatabaseFile.PROPERTY_KEY_TOKEN_NAMES_STORE, DatabaseFile.RELATIONSHIP_TYPE_TOKEN_STORE, DatabaseFile.RELATIONSHIP_TYPE_TOKEN_NAMES_STORE, DatabaseFile.NODE_LABEL_STORE };
			  if ( newFormat.Dynamic().Equals(oldFormat.Dynamic()) )
			  {
					fileOperation( COPY, _fileSystem, sourceDirectoryStructure, migrationStrcuture, Arrays.asList( storesFilesToMigrate ), true, ExistingTargetStrategy.FAIL );
			  }
			  else
			  {
					// Migrate all token stores, schema store and dynamic node label ids, keeping their ids intact
					DirectRecordStoreMigrator migrator = new DirectRecordStoreMigrator( _pageCache, _fileSystem, _config );

					StoreType[] storesToMigrate = new StoreType[] { StoreType.LABEL_TOKEN, StoreType.LABEL_TOKEN_NAME, StoreType.PROPERTY_KEY_TOKEN, StoreType.PROPERTY_KEY_TOKEN_NAME, StoreType.RELATIONSHIP_TYPE_TOKEN, StoreType.RELATIONSHIP_TYPE_TOKEN_NAME, StoreType.NODE_LABEL, StoreType.SCHEMA };

					// Migrate these stores silently because they are usually very small
					ProgressReporter progressReporter = SilentProgressReporter.INSTANCE;

					migrator.Migrate( sourceDirectoryStructure, oldFormat, migrationStrcuture, newFormat, progressReporter, storesToMigrate, StoreType.NODE );
			  }
		 }

		 private void CreateStore( DatabaseLayout migrationDirectoryStructure, RecordFormats newFormat )
		 {
			  IdGeneratorFactory idGeneratorFactory = new ReadOnlyIdGeneratorFactory( _fileSystem );
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( migrationDirectoryStructure, _config, idGeneratorFactory, _pageCache, _fileSystem, newFormat, logProvider, EmptyVersionContextSupplier.EMPTY );
			  using ( NeoStores neoStores = storeFactory.OpenAllNeoStores( true ) )
			  {
					neoStores.MetaDataStore;
					neoStores.LabelTokenStore;
					neoStores.NodeStore;
					neoStores.PropertyStore;
					neoStores.RelationshipGroupStore;
					neoStores.RelationshipStore;
					neoStores.SchemaStore;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.unsafe.impl.batchimport.AdditionalInitialIds readAdditionalIds(final long lastTxId, final long lastTxChecksum, final long lastTxLogVersion, final long lastTxLogByteOffset)
		 private static AdditionalInitialIds ReadAdditionalIds( long lastTxId, long lastTxChecksum, long lastTxLogVersion, long lastTxLogByteOffset )
		 {
			  return new AdditionalInitialIdsAnonymousInnerClass( lastTxId, lastTxChecksum, lastTxLogVersion, lastTxLogByteOffset );
		 }

		 private class AdditionalInitialIdsAnonymousInnerClass : AdditionalInitialIds
		 {
			 private long _lastTxId;
			 private long _lastTxChecksum;
			 private long _lastTxLogVersion;
			 private long _lastTxLogByteOffset;

			 public AdditionalInitialIdsAnonymousInnerClass( long lastTxId, long lastTxChecksum, long lastTxLogVersion, long lastTxLogByteOffset )
			 {
				 this._lastTxId = lastTxId;
				 this._lastTxChecksum = lastTxChecksum;
				 this._lastTxLogVersion = lastTxLogVersion;
				 this._lastTxLogByteOffset = lastTxLogByteOffset;
			 }

			 public long lastCommittedTransactionId()
			 {
				  return _lastTxId;
			 }

			 public long lastCommittedTransactionChecksum()
			 {
				  return _lastTxChecksum;
			 }

			 public long lastCommittedTransactionLogVersion()
			 {
				  return _lastTxLogVersion;
			 }

			 public long lastCommittedTransactionLogByteOffset()
			 {
				  return _lastTxLogByteOffset;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.unsafe.impl.batchimport.staging.ExecutionMonitor migrationBatchImporterMonitor(org.Neo4Net.kernel.impl.store.NeoStores legacyStore, final org.Neo4Net.kernel.impl.util.monitoring.ProgressReporter progressReporter, org.Neo4Net.unsafe.impl.batchimport.Configuration config)
		 private static ExecutionMonitor MigrationBatchImporterMonitor( NeoStores legacyStore, ProgressReporter progressReporter, Configuration config )
		 {
			  return new BatchImporterProgressMonitor( legacyStore.NodeStore.HighId, legacyStore.RelationshipStore.HighId, config, progressReporter );
		 }

		 private static InputIterator LegacyRelationshipsAsInput( NeoStores legacyStore, bool requiresPropertyMigration )
		 {
			  return new StoreScanAsInputIteratorAnonymousInnerClass( legacyStore.RelationshipStore, legacyStore, requiresPropertyMigration );
		 }

		 private class StoreScanAsInputIteratorAnonymousInnerClass : StoreScanAsInputIterator<RelationshipRecord>
		 {
			 private NeoStores _legacyStore;
			 private bool _requiresPropertyMigration;

			 public StoreScanAsInputIteratorAnonymousInnerClass( Neo4Net.Kernel.impl.store.RelationshipStore getRelationshipStore, NeoStores legacyStore, bool requiresPropertyMigration ) : base( getRelationshipStore )
			 {
				 this._legacyStore = legacyStore;
				 this._requiresPropertyMigration = requiresPropertyMigration;
			 }

			 public override InputChunk newChunk()
			 {
				  return new RelationshipRecordChunk( new RecordStorageReader( _legacyStore ), _requiresPropertyMigration );
			 }
		 }

		 private static InputIterator LegacyNodesAsInput( NeoStores legacyStore, bool requiresPropertyMigration )
		 {
			  return new StoreScanAsInputIteratorAnonymousInnerClass2( legacyStore.NodeStore, legacyStore, requiresPropertyMigration );
		 }

		 private class StoreScanAsInputIteratorAnonymousInnerClass2 : StoreScanAsInputIterator<NodeRecord>
		 {
			 private NeoStores _legacyStore;
			 private bool _requiresPropertyMigration;

			 public StoreScanAsInputIteratorAnonymousInnerClass2( Neo4Net.Kernel.impl.store.NodeStore getNodeStore, NeoStores legacyStore, bool requiresPropertyMigration ) : base( getNodeStore )
			 {
				 this._legacyStore = legacyStore;
				 this._requiresPropertyMigration = requiresPropertyMigration;
			 }

			 public override InputChunk newChunk()
			 {
				  return new NodeRecordChunk( new RecordStorageReader( _legacyStore ), _requiresPropertyMigration );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(org.Neo4Net.io.layout.DatabaseLayout migrationLayout, org.Neo4Net.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToUpgradeTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToUpgradeTo )
		 {
			  // Move the migrated ones into the store directory
			  fileOperation( MOVE, _fileSystem, migrationLayout, directoryLayout, Iterables.iterable( DatabaseFile.values() ), true, ExistingTargetStrategy.OVERWRITE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateOrAddNeoStoreFieldsAsPartOfMigration(org.Neo4Net.io.layout.DatabaseLayout migrationStructure, org.Neo4Net.io.layout.DatabaseLayout sourceDirectoryStructure, String versionToMigrateTo, org.Neo4Net.kernel.impl.transaction.log.LogPosition lastClosedTxLogPosition) throws java.io.IOException
		 private void UpdateOrAddNeoStoreFieldsAsPartOfMigration( DatabaseLayout migrationStructure, DatabaseLayout sourceDirectoryStructure, string versionToMigrateTo, LogPosition lastClosedTxLogPosition )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File storeDirNeoStore = sourceDirectoryStructure.metadataStore();
			  File storeDirNeoStore = sourceDirectoryStructure.MetadataStore();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File migrationDirNeoStore = migrationStructure.metadataStore();
			  File migrationDirNeoStore = migrationStructure.MetadataStore();
			  fileOperation( COPY, _fileSystem, sourceDirectoryStructure, migrationStructure, Iterables.iterable( DatabaseFile.METADATA_STORE ), true, ExistingTargetStrategy.SKIP );

			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.UPGRADE_TRANSACTION_ID, MetaDataStore.getRecord( _pageCache, storeDirNeoStore, MetaDataStore.Position.LAST_TRANSACTION_ID ) );
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.UPGRADE_TIME, DateTimeHelper.CurrentUnixTimeMillis() );

			  // Store the checksum of the transaction id the upgrade is at right now. Store it both as
			  // LAST_TRANSACTION_CHECKSUM and UPGRADE_TRANSACTION_CHECKSUM. Initially the last transaction and the
			  // upgrade transaction will be the same, but imagine this scenario:
			  //  - legacy store is migrated on instance A at transaction T
			  //  - upgraded store is copied, via backup or HA or whatever to instance B
			  //  - instance A performs a transaction
			  //  - instance B would like to communicate with A where B's last transaction checksum
			  //    is verified on A. A, at this point not having logs from pre-migration era, will need to
			  //    know the checksum of transaction T to accommodate for this request from B. A will be able
			  //    to look up checksums for transactions succeeding T by looking at its transaction logs,
			  //    but T needs to be stored in neostore to be accessible. Obviously this scenario is only
			  //    problematic as long as we don't migrate and translate old logs.
			  TransactionId lastTxInfo = ReadLastTxInformation( migrationStructure );

			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.LAST_TRANSACTION_CHECKSUM, lastTxInfo.Checksum() );
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.UPGRADE_TRANSACTION_CHECKSUM, lastTxInfo.Checksum() );
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.LAST_TRANSACTION_COMMIT_TIMESTAMP, lastTxInfo.CommitTimestamp() );
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.UPGRADE_TRANSACTION_COMMIT_TIMESTAMP, lastTxInfo.CommitTimestamp() );

			  // add LAST_CLOSED_TRANSACTION_LOG_VERSION and LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET to the migrated
			  // NeoStore
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_VERSION, lastClosedTxLogPosition.LogVersion );
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET, lastClosedTxLogPosition.ByteOffset );

			  // Upgrade version in NeoStore
			  MetaDataStore.setRecord( _pageCache, migrationDirNeoStore, MetaDataStore.Position.STORE_VERSION, MetaDataStore.versionStringToLong( versionToMigrateTo ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup(org.Neo4Net.io.layout.DatabaseLayout migrationLayout) throws java.io.IOException
		 public override void Cleanup( DatabaseLayout migrationLayout )
		 {
			  _fileSystem.deleteRecursively( migrationLayout.DatabaseDirectory() );
		 }

		 public override string ToString()
		 {
			  return "Kernel StoreMigrator";
		 }

		 private class NodeRecordChunk : StoreScanChunk<RecordNodeCursor>
		 {
			  internal NodeRecordChunk( RecordStorageReader storageReader, bool requiresPropertyMigration ) : base( storageReader.AllocateNodeCursor(), storageReader, requiresPropertyMigration )
			  {
			  }

			  protected internal override void Read( RecordNodeCursor cursor, long id )
			  {
					cursor.Single( id );
			  }

			  protected internal override void VisitRecord( RecordNodeCursor record, InputEntityVisitor visitor )
			  {
					visitor.Id( record.EntityReference() );
					visitor.LabelField( record.LabelField );
					VisitProperties( record, visitor );
			  }
		 }

		 private class RelationshipRecordChunk : StoreScanChunk<StorageRelationshipScanCursor>
		 {
			  internal RelationshipRecordChunk( RecordStorageReader storageReader, bool requiresPropertyMigration ) : base( storageReader.AllocateRelationshipScanCursor(), storageReader, requiresPropertyMigration )
			  {
			  }

			  protected internal override void Read( StorageRelationshipScanCursor cursor, long id )
			  {
					cursor.Single( id );
			  }

			  protected internal override void VisitRecord( StorageRelationshipScanCursor record, InputEntityVisitor visitor )
			  {
					visitor.StartId( record.SourceNodeReference() );
					visitor.EndId( record.TargetNodeReference() );
					visitor.Type( record.Type() );
					VisitProperties( record, visitor );
			  }
		 }

		 private class BatchImporterProgressMonitor : CoarseBoundedProgressExecutionMonitor
		 {
			  internal readonly ProgressReporter ProgressReporter;

			  internal BatchImporterProgressMonitor( long highNodeId, long highRelationshipId, Configuration configuration, ProgressReporter progressReporter ) : base( highNodeId, highRelationshipId, configuration )
			  {
					this.ProgressReporter = progressReporter;
					this.ProgressReporter.start( Total() );
			  }

			  protected internal override void Progress( long progress )
			  {
					ProgressReporter.progress( progress );
			  }
		 }
	}

}