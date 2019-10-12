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
namespace Org.Neo4j.Kernel.impl.store
{

	using Org.Neo4j.Helpers.Collection;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.impl.store.format;
	using MetaDataRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using MetaDataRecord = Org.Neo4j.Kernel.impl.store.record.MetaDataRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using LogVersionRepository = Org.Neo4j.Kernel.impl.transaction.log.LogVersionRepository;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using ArrayQueueOutOfOrderSequence = Org.Neo4j.Kernel.impl.util.ArrayQueueOutOfOrderSequence;
	using Bits = Org.Neo4j.Kernel.impl.util.Bits;
	using OutOfOrderSequence = Org.Neo4j.Kernel.impl.util.OutOfOrderSequence;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Logger = Org.Neo4j.Logging.Logger;
	using CappedLogger = Org.Neo4j.Logging.@internal.CappedLogger;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.MetaDataRecordFormat.FIELD_NOT_PRESENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.MetaDataRecordFormat.RECORD_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class MetaDataStore : CommonAbstractStore<MetaDataRecord, NoStoreHeader>, TransactionIdStore, LogVersionRepository
	{
		 public const string TYPE_DESCRIPTOR = "NeoStore";
		 // This value means the field has not been refreshed from the store. Normally, this should happen only once
		 public static readonly long FieldNotInitialized = long.MinValue;
		 /*
		  *  9 longs in header (long + in use), time | random | version | txid | store version | graph next prop | latest
		  *  constraint tx | upgrade time | upgrade id
		  */
		 // Positions of meta-data records

		 public sealed class Position
		 {
			  public static readonly Position Time = new Position( "Time", InnerEnum.Time, 0, "Creation time" );
			  public static readonly Position RandomNumber = new Position( "RandomNumber", InnerEnum.RandomNumber, 1, "Random number for store id" );
			  public static readonly Position LogVersion = new Position( "LogVersion", InnerEnum.LogVersion, 2, "Current log version" );
			  public static readonly Position LastTransactionId = new Position( "LastTransactionId", InnerEnum.LastTransactionId, 3, "Last committed transaction" );
			  public static readonly Position StoreVersion = new Position( "StoreVersion", InnerEnum.StoreVersion, 4, "Store format version" );
			  public static readonly Position FirstGraphProperty = new Position( "FirstGraphProperty", InnerEnum.FirstGraphProperty, 5, "First property record containing graph properties" );
			  public static readonly Position LastConstraintTransaction = new Position( "LastConstraintTransaction", InnerEnum.LastConstraintTransaction, 6, "Last committed transaction containing constraint changes" );
			  public static readonly Position UpgradeTransactionId = new Position( "UpgradeTransactionId", InnerEnum.UpgradeTransactionId, 7, "Transaction id most recent upgrade was performed at" );
			  public static readonly Position UpgradeTime = new Position( "UpgradeTime", InnerEnum.UpgradeTime, 8, "Time of last upgrade" );
			  public static readonly Position LastTransactionChecksum = new Position( "LastTransactionChecksum", InnerEnum.LastTransactionChecksum, 9, "Checksum of last committed transaction" );
			  public static readonly Position UpgradeTransactionChecksum = new Position( "UpgradeTransactionChecksum", InnerEnum.UpgradeTransactionChecksum, 10, "Checksum of transaction id the most recent upgrade was performed at" );
			  public static readonly Position LastClosedTransactionLogVersion = new Position( "LastClosedTransactionLogVersion", InnerEnum.LastClosedTransactionLogVersion, 11, "Log version where the last transaction commit entry has been written into" );
			  public static readonly Position LastClosedTransactionLogByteOffset = new Position( "LastClosedTransactionLogByteOffset", InnerEnum.LastClosedTransactionLogByteOffset, 12, "Byte offset in the log file where the last transaction commit entry " + "has been written into" );
			  public static readonly Position LastTransactionCommitTimestamp = new Position( "LastTransactionCommitTimestamp", InnerEnum.LastTransactionCommitTimestamp, 13, "Commit time timestamp for last committed transaction" );
			  public static readonly Position UpgradeTransactionCommitTimestamp = new Position( "UpgradeTransactionCommitTimestamp", InnerEnum.UpgradeTransactionCommitTimestamp, 14, "Commit timestamp of transaction the most recent upgrade was performed at" );

			  private static readonly IList<Position> valueList = new List<Position>();

			  static Position()
			  {
				  valueList.Add( Time );
				  valueList.Add( RandomNumber );
				  valueList.Add( LogVersion );
				  valueList.Add( LastTransactionId );
				  valueList.Add( StoreVersion );
				  valueList.Add( FirstGraphProperty );
				  valueList.Add( LastConstraintTransaction );
				  valueList.Add( UpgradeTransactionId );
				  valueList.Add( UpgradeTime );
				  valueList.Add( LastTransactionChecksum );
				  valueList.Add( UpgradeTransactionChecksum );
				  valueList.Add( LastClosedTransactionLogVersion );
				  valueList.Add( LastClosedTransactionLogByteOffset );
				  valueList.Add( LastTransactionCommitTimestamp );
				  valueList.Add( UpgradeTransactionCommitTimestamp );
			  }

			  public enum InnerEnum
			  {
				  Time,
				  RandomNumber,
				  LogVersion,
				  LastTransactionId,
				  StoreVersion,
				  FirstGraphProperty,
				  LastConstraintTransaction,
				  UpgradeTransactionId,
				  UpgradeTime,
				  LastTransactionChecksum,
				  UpgradeTransactionChecksum,
				  LastClosedTransactionLogVersion,
				  LastClosedTransactionLogByteOffset,
				  LastTransactionCommitTimestamp,
				  UpgradeTransactionCommitTimestamp
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;

			  internal Position( string name, InnerEnum innerEnum, int id, string description )
			  {
					this._id = id;
					this._description = description;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public int Id()
			  {
					return _id;
			  }

			  public string Description()
			  {
					return _description;
			  }

			 public static IList<Position> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Position valueOf( string name )
			 {
				 foreach ( Position enumInstance in Position.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 // Fields the neostore keeps cached and must be initialized on startup
		 private volatile long _creationTimeField = FieldNotInitialized;
		 private volatile long _randomNumberField = FieldNotInitialized;
		 private volatile long _versionField = FieldNotInitialized;
		 // This is an atomic long since we, when incrementing last tx id, won't set the record in the page,
		 // we do that when flushing, which performs better and fine from a recovery POV.
		 private readonly AtomicLong _lastCommittingTxField = new AtomicLong( FieldNotInitialized );
		 private volatile long _storeVersionField = FieldNotInitialized;
		 private volatile long _graphNextPropField = FieldNotInitialized;
		 private volatile long _latestConstraintIntroducingTxField = FieldNotInitialized;
		 private volatile long _upgradeTxIdField = FieldNotInitialized;
		 private volatile long _upgradeTxChecksumField = FieldNotInitialized;
		 private volatile long _upgradeTimeField = FieldNotInitialized;
		 private volatile long _upgradeCommitTimestampField = FieldNotInitialized;

		 private volatile TransactionId _upgradeTransaction = new TransactionId( FieldNotInitialized, FieldNotInitialized, FieldNotInitialized );

		 // This is not a field in the store, but something keeping track of which is the currently highest
		 // committed transaction id, together with its checksum.
		 private readonly HighestTransactionId _highestCommittedTransaction = new HighestTransactionId( FieldNotInitialized, FieldNotInitialized, FieldNotInitialized );

		 // This is not a field in the store, but something keeping track of which of the committed
		 // transactions have been closed. Useful in rotation and shutdown.
		 private readonly OutOfOrderSequence _lastClosedTx = new ArrayQueueOutOfOrderSequence( -1, 200, new long[2] );

		 // We use these objects and their monitors as "entity" locks on the records, because page write locks are not
		 // exclusive. Therefor, these locks are only used when *writing* records, not when reading them.
		 private readonly object _upgradeTimeLock = new object();
		 private readonly object _creationTimeLock = new object();
		 private readonly object _randomNumberLock = new object();
		 private readonly object _upgradeTransactionLock = new object();
		 private readonly object _logVersionLock = new object();
		 private readonly object _storeVersionLock = new object();
		 private readonly object _graphNextPropLock = new object();
		 private readonly object _lastConstraintIntroducingTxLock = new object();
		 private readonly object _transactionCommittedLock = new object();
		 private readonly object _transactionClosedLock = new object();

		 internal MetaDataStore( File file, File idFile, Config conf, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, RecordFormat<MetaDataRecord> recordFormat, string storeVersion, params OpenOption[] openOptions ) : base( file, idFile, conf, IdType.NEOSTORE_BLOCK, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, recordFormat, NoStoreHeaderFormat.NoStoreHeaderFormatConflict, storeVersion, openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void initialiseNewStoreFile(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException
		 protected internal override void InitialiseNewStoreFile( PagedFile file )
		 {
			  base.InitialiseNewStoreFile( file );

			  long storeVersionAsLong = MetaDataStore.VersionStringToLong( StoreVersion );
			  StoreId storeId = new StoreId( storeVersionAsLong );

			  PagedFile = file;
			  CreationTime = storeId.CreationTime;
			  RandomNumber = storeId.RandomId;
			  // If metaDataStore.creationTime == metaDataStore.upgradeTime && metaDataStore.upgradeTransactionId == BASE_TX_ID
			  // then store has never been upgraded
			  UpgradeTime = storeId.CreationTime;
			  SetUpgradeTransaction( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP );
			  CurrentLogVersion = 0;
			  SetLastCommittedAndClosedTransactionId( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BaseTxLogByteOffset, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_LOG_VERSION );
			  StoreVersion = storeVersionAsLong;
			  GraphNextProp = -1;
			  LatestConstraintIntroducingTx = 0;

			  Flush();
			  PagedFile = null;
		 }

		 protected internal override void CheckAndLoadStorage( bool createIfNotExists )
		 {
			  base.CheckAndLoadStorage( createIfNotExists );
			  InitHighId();
		 }

		 // Only for initialization and recovery, so we don't need to lock the records

		 public override void SetLastCommittedAndClosedTransactionId( long transactionId, long checksum, long commitTimestamp, long byteOffset, long logVersion )
		 {
			  AssertNotClosed();
			  SetRecord( Position.LastTransactionId, transactionId );
			  SetRecord( Position.LastTransactionChecksum, checksum );
			  SetRecord( Position.LastClosedTransactionLogVersion, logVersion );
			  SetRecord( Position.LastClosedTransactionLogByteOffset, byteOffset );
			  SetRecord( Position.LastTransactionCommitTimestamp, commitTimestamp );
			  CheckInitialized( _lastCommittingTxField.get() );
			  _lastCommittingTxField.set( transactionId );
			  _lastClosedTx.set( transactionId, new long[]{ logVersion, byteOffset } );
			  _highestCommittedTransaction.set( transactionId, checksum, commitTimestamp );
		 }
		 /// <summary>
		 /// Writes a record in a neostore file.
		 /// This method only works for neostore files of the current version.
		 /// </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> the {@code neostore} file lives in. </param>
		 /// <param name="neoStore"> <seealso cref="File"/> pointing to the neostore. </param>
		 /// <param name="position"> record <seealso cref="Position"/>. </param>
		 /// <param name="value"> value to write in that record. </param>
		 /// <returns> the previous value before writing. </returns>
		 /// <exception cref="IOException"> if any I/O related error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long setRecord(org.neo4j.io.pagecache.PageCache pageCache, java.io.File neoStore, Position position, long value) throws java.io.IOException
		 public static long SetRecord( PageCache pageCache, File neoStore, Position position, long value )
		 {
			  long previousValue = FieldNotInitialized;
			  int pageSize = GetPageSize( pageCache );
			  using ( PagedFile pagedFile = pageCache.Map( neoStore, pageSize ) )
			  {
					int offset = offset( position );
					using ( PageCursor cursor = pagedFile.Io( 0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 if ( cursor.Next() )
						 {
							  // We're overwriting a record, get the previous value
							  cursor.Offset = offset;
							  sbyte inUse = cursor.Byte;
							  long record = cursor.Long;

							  if ( inUse == Record.IN_USE.byteValue() )
							  {
									previousValue = record;
							  }

							  // Write the value
							  cursor.Offset = offset;
							  cursor.PutByte( Record.IN_USE.byteValue() );
							  cursor.PutLong( value );
							  if ( cursor.CheckAndClearBoundsFlag() )
							  {
									MetaDataRecord neoStoreRecord = new MetaDataRecord();
									neoStoreRecord.Id = position.id;
									throw new UnderlyingStorageException( BuildOutOfBoundsExceptionMessage( neoStoreRecord, 0, offset, RECORD_SIZE, pageSize, neoStore.AbsolutePath ) );
							  }
						 }
					}
			  }
			  return previousValue;
		 }

		 private void InitHighId()
		 {
			  Position[] values = Position.values();
			  long highestPossibleId = values[values.Length - 1].id;
			  HighestPossibleIdInUse = highestPossibleId;
		 }

		 private static int Offset( Position position )
		 {
			  return RECORD_SIZE * position.id;
		 }

		 /// <summary>
		 /// Reads a record from a neostore file.
		 /// </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> the {@code neostore} file lives in. </param>
		 /// <param name="neoStore"> <seealso cref="File"/> pointing to the neostore. </param>
		 /// <param name="position"> record <seealso cref="Position"/>. </param>
		 /// <returns> the read record value specified by <seealso cref="Position"/>. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long getRecord(org.neo4j.io.pagecache.PageCache pageCache, java.io.File neoStore, Position position) throws java.io.IOException
		 public static long GetRecord( PageCache pageCache, File neoStore, Position position )
		 {
			  MetaDataRecordFormat format = new MetaDataRecordFormat();
			  int pageSize = GetPageSize( pageCache );
			  long value = FIELD_NOT_PRESENT;
			  using ( PagedFile pagedFile = pageCache.Map( neoStore, pageSize ) )
			  {
					if ( pagedFile.LastPageId >= 0 )
					{
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
						 {
							  if ( cursor.Next() )
							  {
									MetaDataRecord record = new MetaDataRecord();
									record.Id = position.id;
									do
									{
										 format.Read( record, cursor, RecordLoad.CHECK, RECORD_SIZE );
										 if ( record.InUse() )
										 {
											  value = record.Value;
										 }
										 else
										 {
											  value = FIELD_NOT_PRESENT;
										 }
									} while ( cursor.ShouldRetry() );
									if ( cursor.CheckAndClearBoundsFlag() )
									{
										 int offset = offset( position );
										 throw new UnderlyingStorageException( BuildOutOfBoundsExceptionMessage( record, 0, offset, RECORD_SIZE, pageSize, neoStore.AbsolutePath ) );
									}
							  }
						 }
					}
			  }
			  return value;
		 }

		 internal static int GetPageSize( PageCache pageCache )
		 {
			  return pageCache.PageSize() - pageCache.PageSize() % RECORD_SIZE;
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return new StoreId( CreationTime, RandomNumber, StoreVersion, UpgradeTime, _upgradeTxIdField );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.storageengine.api.StoreId getStoreId(org.neo4j.io.pagecache.PageCache pageCache, java.io.File neoStore) throws java.io.IOException
		 public static StoreId getStoreId( PageCache pageCache, File neoStore )
		 {
			  return new StoreId( GetRecord( pageCache, neoStore, Position.Time ), GetRecord( pageCache, neoStore, Position.RandomNumber ), GetRecord( pageCache, neoStore, Position.StoreVersion ), GetRecord( pageCache, neoStore, Position.UpgradeTime ), GetRecord( pageCache, neoStore, Position.UpgradeTransactionId ) );
		 }

		 public virtual long UpgradeTime
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _upgradeTimeField );
				  return _upgradeTimeField;
			 }
			 set
			 {
				  lock ( _upgradeTimeLock )
				  {
						SetRecord( Position.UpgradeTime, value );
						_upgradeTimeField = value;
				  }
			 }
		 }


		 public virtual void SetUpgradeTransaction( long id, long checksum, long timestamp )
		 {
			  long pageId = PageIdForRecord( Position.UpgradeTransactionId.id );
			  Debug.Assert( pageId == PageIdForRecord( Position.UpgradeTransactionChecksum.id ) );
			  lock ( _upgradeTransactionLock )
			  {
					try
					{
							using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK ) )
							{
							 if ( !cursor.Next() )
							 {
								  throw new UnderlyingStorageException( "Could not access MetaDataStore page " + pageId );
							 }
							 SetRecord( cursor, Position.UpgradeTransactionId, id );
							 SetRecord( cursor, Position.UpgradeTransactionChecksum, checksum );
							 SetRecord( cursor, Position.UpgradeTransactionCommitTimestamp, timestamp );
							 _upgradeTxIdField = id;
							 _upgradeTxChecksumField = checksum;
							 _upgradeCommitTimestampField = timestamp;
							 _upgradeTransaction = new TransactionId( id, checksum, timestamp );
							}
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( e );
					}
			  }
		 }

		 public virtual long CreationTime
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _creationTimeField );
				  return _creationTimeField;
			 }
			 set
			 {
				  lock ( _creationTimeLock )
				  {
						SetRecord( Position.Time, value );
						_creationTimeField = value;
				  }
			 }
		 }


		 public virtual long RandomNumber
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _randomNumberField );
				  return _randomNumberField;
			 }
			 set
			 {
				  lock ( _randomNumberLock )
				  {
						SetRecord( Position.RandomNumber, value );
						_randomNumberField = value;
				  }
			 }
		 }


		 public virtual long CurrentLogVersion
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _versionField );
				  return _versionField;
			 }
			 set
			 {
				  lock ( _logVersionLock )
				  {
						SetRecord( Position.LogVersion, value );
						_versionField = value;
				  }
			 }
		 }


		 public virtual long LastTransactionCommitTimestamp
		 {
			 set
			 {
				  // Preventing race with transactionCommitted() and assure record is consistent with highestCommittedTransaction
				  lock ( _transactionCommittedLock )
				  {
						SetRecord( Position.LastTransactionCommitTimestamp, value );
						TransactionId transactionId = _highestCommittedTransaction.get();
						_highestCommittedTransaction.set( transactionId.TransactionIdConflict(), transactionId.Checksum(), value );
				  }
			 }
		 }

		 public override long IncrementAndGetVersion()
		 {
			  // This method can expect synchronisation at a higher level,
			  // and be effectively single-threaded.
			  long pageId = PageIdForRecord( Position.LogVersion.id );
			  long version;
			  lock ( _logVersionLock )
			  {
					try
					{
							using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK ) )
							{
							 if ( cursor.Next() )
							 {
								  IncrementVersion( cursor );
							 }
							 version = _versionField;
							}
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( e );
					}
			  }
			  Flush(); // make sure the new version value is persisted
			  return version;
		 }

		 private void IncrementVersion( PageCursor cursor )
		 {
			  if ( !cursor.WriteLocked )
			  {
					throw new System.ArgumentException( "Cannot increment log version on page cursor that is not write-locked" );
			  }
			  // offsets plus one to skip the inUse byte
			  int offset = ( Position.LogVersion.id * RecordSize ) + 1;
			  long value = cursor.GetLong( offset ) + 1;
			  cursor.PutLong( offset, value );
			  CheckForDecodingErrors( cursor, Position.LogVersion.id, NORMAL );
			  _versionField = value;
		 }

		 public virtual long StoreVersion
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _storeVersionField );
				  return _storeVersionField;
			 }
			 set
			 {
				  lock ( _storeVersionLock )
				  {
						SetRecord( Position.StoreVersion, value );
						_storeVersionField = value;
				  }
			 }
		 }


		 public virtual long GraphNextProp
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _graphNextPropField );
				  return _graphNextPropField;
			 }
			 set
			 {
				  lock ( _graphNextPropLock )
				  {
						SetRecord( Position.FirstGraphProperty, value );
						_graphNextPropField = value;
				  }
			 }
		 }


		 public virtual long LatestConstraintIntroducingTx
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _latestConstraintIntroducingTxField );
				  return _latestConstraintIntroducingTxField;
			 }
			 set
			 {
				  lock ( _lastConstraintIntroducingTxLock )
				  {
						SetRecord( Position.LastConstraintTransaction, value );
						_latestConstraintIntroducingTxField = value;
				  }
			 }
		 }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readAllFields(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void ReadAllFields( PageCursor cursor )
		 {
			  do
			  {
					_creationTimeField = GetRecordValue( cursor, Position.Time );
					_randomNumberField = GetRecordValue( cursor, Position.RandomNumber );
					_versionField = GetRecordValue( cursor, Position.LogVersion );
					long lastCommittedTxId = GetRecordValue( cursor, Position.LastTransactionId );
					_lastCommittingTxField.set( lastCommittedTxId );
					_storeVersionField = GetRecordValue( cursor, Position.StoreVersion );
					_graphNextPropField = GetRecordValue( cursor, Position.FirstGraphProperty );
					_latestConstraintIntroducingTxField = GetRecordValue( cursor, Position.LastConstraintTransaction );
					_upgradeTxIdField = GetRecordValue( cursor, Position.UpgradeTransactionId );
					_upgradeTxChecksumField = GetRecordValue( cursor, Position.UpgradeTransactionChecksum );
					_upgradeTimeField = GetRecordValue( cursor, Position.UpgradeTime );
					long lastClosedTransactionLogVersion = GetRecordValue( cursor, Position.LastClosedTransactionLogVersion );
					long lastClosedTransactionLogByteOffset = GetRecordValue( cursor, Position.LastClosedTransactionLogByteOffset );
					_lastClosedTx.set( lastCommittedTxId, new long[]{ lastClosedTransactionLogVersion, lastClosedTransactionLogByteOffset } );
					_highestCommittedTransaction.set( lastCommittedTxId, GetRecordValue( cursor, Position.LastTransactionChecksum ), GetRecordValue( cursor, Position.LastTransactionCommitTimestamp, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP ) );
					_upgradeCommitTimestampField = GetRecordValue( cursor, Position.UpgradeTransactionCommitTimestamp, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP );

					_upgradeTransaction = new TransactionId( _upgradeTxIdField, _upgradeTxChecksumField, _upgradeCommitTimestampField );
			  } while ( cursor.ShouldRetry() );
			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					throw new UnderlyingStorageException( "Out of page bounds when reading all meta-data fields. The page in question is page " + cursor.CurrentPageId + " of file " + StorageFileConflict.AbsolutePath + ", which is " + cursor.CurrentPageSize + " bytes in size" );
			  }
		 }

		 internal virtual long GetRecordValue( PageCursor cursor, Position position )
		 {
			  return GetRecordValue( cursor, position, FIELD_NOT_PRESENT );
		 }

		 private long GetRecordValue( PageCursor cursor, Position position, long defaultValue )
		 {
			  MetaDataRecord record = NewRecord();
			  try
			  {
					record.Id = position.id;
					RecordFormat.read( record, cursor, FORCE, RECORD_SIZE );
					if ( record.InUse() )
					{
						 return record.Value;
					}
					return defaultValue;
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 private void RefreshFields()
		 {
			  ScanAllFields(PF_SHARED_READ_LOCK, element =>
			  {
				ReadAllFields( element );
				return false;
			  });
		 }

		 private void ScanAllFields( int pfFlags, Visitor<PageCursor, IOException> visitor )
		 {
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( 0, pfFlags ) )
					  {
						if ( cursor.Next() )
						{
							 visitor.Visit( cursor );
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 private void SetRecord( Position position, long value )
		 {
			  MetaDataRecord record = new MetaDataRecord();
			  record.Initialize( true, value );
			  record.Id = position.id;
			  UpdateRecord( record );
		 }

		 private void SetRecord( PageCursor cursor, Position position, long value )
		 {
			  if ( !cursor.WriteLocked )
			  {
					throw new System.ArgumentException( "Cannot write record without a page cursor that is write-locked" );
			  }
			  int offset = OffsetForId( position.id );
			  cursor.Offset = offset;
			  cursor.PutByte( Record.IN_USE.byteValue() );
			  cursor.PutLong( value );
			  CheckForDecodingErrors( cursor, position.id, NORMAL );
		 }

		 public virtual NeoStoreRecord GraphPropertyRecord()
		 {
			  NeoStoreRecord result = new NeoStoreRecord();
			  result.NextProp = GraphNextProp;
			  return result;
		 }

		 /*
		  * The following two methods encode and decode a string that is presumably
		  * the store version into a long via Latin1 encoding. This leaves room for
		  * 7 characters and 1 byte for the length. Current string is
		  * 0.A.0 which is 5 chars, so we have room for expansion. When that
		  * becomes a problem we will be in a yacht, sipping alcoholic
		  * beverages of our choice. Or taking turns crashing golden
		  * helicopters. Anyway, it should suffice for some time and by then
		  * it should have become SEP.
		  */
		 public static long VersionStringToLong( string storeVersion )
		 {
			  if ( CommonAbstractStore.UNKNOWN_VERSION.Equals( storeVersion ) )
			  {
					return -1;
			  }
			  Bits bits = Bits.bits( 8 );
			  int length = storeVersion.Length;
			  if ( length == 0 || length > 7 )
			  {
					throw new System.ArgumentException( format( "The given string %s is not of proper size for a store version string", storeVersion ) );
			  }
			  bits.Put( length, 8 );
			  for ( int i = 0; i < length; i++ )
			  {
					char c = storeVersion[i];
					if ( c >= ( char )256 )
					{
						 throw new System.ArgumentException( format( "Store version strings should be encode-able as Latin1 - %s is not", storeVersion ) );
					}
					bits.put( c, 8 ); // Just the lower byte
			  }
			  return bits.Long;
		 }

		 public static string VersionLongToString( long storeVersion )
		 {
			  if ( storeVersion == -1 )
			  {
					return CommonAbstractStore.UNKNOWN_VERSION;
			  }
			  Bits bits = Bits.bitsFromLongs( new long[]{ storeVersion } );
			  int length = bits.GetShort( 8 );
			  if ( length == 0 || length > 7 )
			  {
					throw new System.ArgumentException( format( "The read version string length %d is not proper.", length ) );
			  }
			  char[] result = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					result[i] = ( char ) bits.GetShort( 8 );
			  }
			  return new string( result );
		 }

		 public override long NextCommittingTransactionId()
		 {
			  AssertNotClosed();
			  CheckInitialized( _lastCommittingTxField.get() );
			  return _lastCommittingTxField.incrementAndGet();
		 }

		 public override long CommittingTransactionId()
		 {
			  AssertNotClosed();
			  CheckInitialized( _lastCommittingTxField.get() );
			  return _lastCommittingTxField.get();
		 }

		 public override void TransactionCommitted( long transactionId, long checksum, long commitTimestamp )
		 {
			  AssertNotClosed();
			  CheckInitialized( _lastCommittingTxField.get() );
			  if ( _highestCommittedTransaction.offer( transactionId, checksum, commitTimestamp ) )
			  {
					// We need to synchronize here in order to guarantee that the three fields are written consistently
					// together. Note that having a write lock on the page is not enough for 3 reasons:
					// 1. page write locks are not exclusive
					// 2. the records might be in different pages
					// 3. some other thread might kick in while we have been written only one record
					lock ( _transactionCommittedLock )
					{
						 // Double-check with highest tx id under the lock, so that there haven't been
						 // another higher transaction committed between our id being accepted and
						 // acquiring this monitor.
						 if ( _highestCommittedTransaction.get().transactionId() == transactionId )
						 {
							  long pageId = PageIdForRecord( Position.LastTransactionId.id );
							  Debug.Assert( pageId == PageIdForRecord( Position.LastTransactionChecksum.id ) );
							  try
							  {
									  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK ) )
									  {
										if ( cursor.Next() )
										{
											 SetRecord( cursor, Position.LastTransactionId, transactionId );
											 SetRecord( cursor, Position.LastTransactionChecksum, checksum );
											 SetRecord( Position.LastTransactionCommitTimestamp, commitTimestamp );
										}
									  }
							  }
							  catch ( IOException e )
							  {
									throw new UnderlyingStorageException( e );
							  }
						 }
					}
			  }
		 }

		 public virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _lastCommittingTxField.get() );
				  return _highestCommittedTransaction.get().transactionId();
			 }
		 }

		 public virtual TransactionId LastCommittedTransaction
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _lastCommittingTxField.get() );
				  return _highestCommittedTransaction.get();
			 }
		 }

		 public virtual TransactionId UpgradeTransaction
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _upgradeTxIdField );
				  return _upgradeTransaction;
			 }
		 }

		 public virtual long LastClosedTransactionId
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _lastCommittingTxField.get() );
				  return _lastClosedTx.HighestGapFreeNumber;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitClosedTransactionId(long txId, long timeoutMillis) throws java.util.concurrent.TimeoutException, InterruptedException
		 public override void AwaitClosedTransactionId( long txId, long timeoutMillis )
		 {
			  AssertNotClosed();
			  CheckInitialized( _lastCommittingTxField.get() );
			  _lastClosedTx.await( txId, timeoutMillis );
		 }

		 public virtual long[] LastClosedTransaction
		 {
			 get
			 {
				  AssertNotClosed();
				  CheckInitialized( _lastCommittingTxField.get() );
				  return _lastClosedTx.get();
			 }
		 }

		 /// <summary>
		 /// Ensures that all fields are read from the store, by checking the initial value of the field in question
		 /// </summary>
		 /// <param name="field"> the value </param>
		 private void CheckInitialized( long field )
		 {
			  if ( field == FieldNotInitialized )
			  {
					RefreshFields();
			  }
		 }

		 public override void TransactionClosed( long transactionId, long logVersion, long byteOffset )
		 {
			  if ( _lastClosedTx.offer( transactionId, new long[]{ logVersion, byteOffset } ) )
			  {
					long pageId = PageIdForRecord( Position.LastClosedTransactionLogVersion.id );
					Debug.Assert( pageId == PageIdForRecord( Position.LastClosedTransactionLogByteOffset.id ) );
					lock ( _transactionClosedLock )
					{
						 try
						 {
								 using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK ) )
								 {
								  if ( cursor.Next() )
								  {
										long[] lastClosedTransactionData = _lastClosedTx.get();
										SetRecord( cursor, Position.LastClosedTransactionLogVersion, lastClosedTransactionData[1] );
										SetRecord( cursor, Position.LastClosedTransactionLogByteOffset, lastClosedTransactionData[2] );
								  }
								 }
						 }
						 catch ( IOException e )
						 {
							  throw new UnderlyingStorageException( e );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void logRecords(final org.neo4j.logging.Logger msgLog)
		 public virtual void LogRecords( Logger msgLog )
		 {
			  ScanAllFields(PF_SHARED_READ_LOCK, cursor =>
			  {
				foreach ( Position position in Position.values() )
				{
					 long value;
					 do
					 {
						  value = GetRecordValue( cursor, position );
					 } while ( cursor.shouldRetry() );
					 bool bounds = cursor.checkAndClearBoundsFlag();
					 msgLog.Log( position.name() + " (" + position.description() + "): " + value + (bounds ? " (out-of-bounds detected; value cannot be trusted)" : "") );
				}
				return false;
			  });
		 }

		 public override MetaDataRecord NewRecord()
		 {
			  return new MetaDataRecord();
		 }

		 public override void Accept<FAILURE>( Org.Neo4j.Kernel.impl.store.RecordStore_Processor<FAILURE> processor, MetaDataRecord record ) where FAILURE : Exception
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PrepareForCommit( MetaDataRecord record )
		 { // No need to do anything with these records before commit
		 }

		 public override void PrepareForCommit( MetaDataRecord record, IdSequence idSequence )
		 { // No need to do anything with these records before commit
		 }
	}

}