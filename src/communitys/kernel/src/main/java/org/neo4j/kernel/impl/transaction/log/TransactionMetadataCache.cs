using System;

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

	using Neo4Net.Helpers.Collections;

	public class TransactionMetadataCache
	{
		 private const int DEFAULT_TRANSACTION_CACHE_SIZE = 100_000;
		 private readonly LruCache<long, TransactionMetadata> _txStartPositionCache;

		 public TransactionMetadataCache() : this(DEFAULT_TRANSACTION_CACHE_SIZE)
		 {
		 }
		 public TransactionMetadataCache( int transactionCacheSize )
		 {
			  this._txStartPositionCache = new LruCache<long, TransactionMetadata>( "Tx start position cache", transactionCacheSize );
		 }

		 public virtual void Clear()
		 {
			  _txStartPositionCache.clear();
		 }

		 public virtual TransactionMetadata GetTransactionMetadata( long txId )
		 {
			  return _txStartPositionCache.get( txId );
		 }

		 public virtual TransactionMetadata CacheTransactionMetadata( long txId, LogPosition position, int masterId, int authorId, long checksum, long timeWritten )
		 {
			  if ( position.ByteOffset == -1 )
			  {
					throw new Exception( "StartEntry.position is " + position );
			  }

			  TransactionMetadata result = new TransactionMetadata( masterId, authorId, position, checksum, timeWritten );
			  _txStartPositionCache.put( txId, result );
			  return result;
		 }

		 public class TransactionMetadata
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int MasterIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int AuthorIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LogPosition StartPositionConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long ChecksumConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long TimeWrittenConflict;

			  public TransactionMetadata( int masterId, int authorId, LogPosition startPosition, long checksum, long timeWritten )
			  {
					this.MasterIdConflict = masterId;
					this.AuthorIdConflict = authorId;
					this.StartPositionConflict = startPosition;
					this.ChecksumConflict = checksum;
					this.TimeWrittenConflict = timeWritten;
			  }

			  public virtual int MasterId
			  {
				  get
				  {
						return MasterIdConflict;
				  }
			  }

			  public virtual int AuthorId
			  {
				  get
				  {
						return AuthorIdConflict;
				  }
			  }

			  public virtual LogPosition StartPosition
			  {
				  get
				  {
						return StartPositionConflict;
				  }
			  }

			  public virtual long Checksum
			  {
				  get
				  {
						return ChecksumConflict;
				  }
			  }

			  public virtual long TimeWritten
			  {
				  get
				  {
						return TimeWrittenConflict;
				  }
			  }

			  public override string ToString()
			  {
					return "TransactionMetadata{" +
							 "masterId=" + MasterIdConflict +
							 ", authorId=" + AuthorIdConflict +
							 ", startPosition=" + StartPositionConflict +
							 ", checksum=" + ChecksumConflict +
							 ", timeWritten=" + TimeWrittenConflict +
							 '}';
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					TransactionMetadata that = ( TransactionMetadata ) o;
					return MasterIdConflict == that.MasterIdConflict && AuthorIdConflict == that.AuthorIdConflict && ChecksumConflict == that.ChecksumConflict && TimeWrittenConflict == that.TimeWrittenConflict && Objects.Equals( StartPositionConflict, that.StartPositionConflict );
			  }

			  public override int GetHashCode()
			  {
					int result = MasterIdConflict;
					result = 31 * result + AuthorIdConflict;
					result = 31 * result + StartPositionConflict.GetHashCode();
					result = 31 * result + ( int )( ChecksumConflict ^ ( ( long )( ( ulong )ChecksumConflict >> 32 ) ) );
					result = 31 * result + ( int )( TimeWrittenConflict ^ ( ( long )( ( ulong )TimeWrittenConflict >> 32 ) ) );
					return result;
			  }
		 }
	}

}