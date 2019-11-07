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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.getRecord;

	public class ReadOnlyTransactionIdStore : TransactionIdStore
	{
		 private readonly long _transactionId;
		 private readonly long _transactionChecksum;
		 private readonly long _logVersion;
		 private readonly long _byteOffset;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ReadOnlyTransactionIdStore(Neo4Net.io.pagecache.PageCache pageCache, Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public ReadOnlyTransactionIdStore( PageCache pageCache, DatabaseLayout databaseLayout )
		 {
			  long id = 0;
			  long checksum = 0;
			  long logVersion = 0;
			  long byteOffset = 0;
			  if ( NeoStores.isStorePresent( pageCache, databaseLayout ) )
			  {
					File neoStore = databaseLayout.MetadataStore();
					id = getRecord( pageCache, neoStore, Position.LAST_TRANSACTION_ID );
					checksum = getRecord( pageCache, neoStore, Position.LAST_TRANSACTION_CHECKSUM );
					logVersion = getRecord( pageCache, neoStore, Position.LAST_CLOSED_TRANSACTION_LOG_VERSION );
					byteOffset = getRecord( pageCache, neoStore, Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET );
			  }

			  this._transactionId = id;
			  this._transactionChecksum = checksum;
			  this._logVersion = logVersion;
			  this._byteOffset = byteOffset;
		 }

		 public override long NextCommittingTransactionId()
		 {
			  throw new System.NotSupportedException( "Read-only transaction ID store" );
		 }

		 public override long CommittingTransactionId()
		 {
			  throw new System.NotSupportedException( "Read-only transaction ID store" );
		 }

		 public override void TransactionCommitted( long transactionId, long checksum, long commitTimestamp )
		 {
			  throw new System.NotSupportedException( "Read-only transaction ID store" );
		 }

		 public virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  return _transactionId;
			 }
		 }

		 public virtual TransactionId LastCommittedTransaction
		 {
			 get
			 {
				  return new TransactionId( _transactionId, _transactionChecksum, TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP );
			 }
		 }

		 public virtual TransactionId UpgradeTransaction
		 {
			 get
			 {
				  return LastCommittedTransaction;
			 }
		 }

		 public virtual long LastClosedTransactionId
		 {
			 get
			 {
				  return _transactionId;
			 }
		 }

		 public override void AwaitClosedTransactionId( long txId, long timeoutMillis )
		 {
			  throw new System.NotSupportedException( "Not implemented" );
		 }

		 public virtual long[] LastClosedTransaction
		 {
			 get
			 {
				  return new long[]{ _transactionId, _logVersion, _byteOffset };
			 }
		 }

		 public override void SetLastCommittedAndClosedTransactionId( long transactionId, long checksum, long commitTimestamp, long logByteOffset, long logVersion )
		 {
			  throw new System.NotSupportedException( "Read-only transaction ID store" );
		 }

		 public override void TransactionClosed( long transactionId, long logVersion, long logByteOffset )
		 {
			  throw new System.NotSupportedException( "Read-only transaction ID store" );
		 }

		 public override void Flush()
		 { // Nothing to flush
		 }
	}

}