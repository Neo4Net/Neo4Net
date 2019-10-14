using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.store
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.store.format;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// An abstract representation of a dynamic store. Record size is set at creation as the contents of the
	/// first record and read and used when opening the store in future sessions.
	/// <para>
	/// Instead of a fixed record this class uses blocks to store a record. If a
	/// record size is greater than the block size the record will use one or more
	/// blocks to store its data.
	/// </para>
	/// <para>
	/// A dynamic store don't have a <seealso cref="IdGenerator"/> because the position of a
	/// record can't be calculated just by knowing the id. Instead one should use
	/// another store and store the start block of the record located in the
	/// dynamic store. Note: This class makes use of an id generator internally for
	/// managing free and non free blocks.
	/// </para>
	/// <para>
	/// Note, the first block of a dynamic store is reserved and contains information
	/// about the store.
	/// </para>
	/// <para>
	/// About configuring block size: Record size is the whole record size including the header (next pointer
	/// and what not). The term block size is equivalent to data size, which is the size of the record - header size.
	/// User configures block size and the block size is what is passed into the constructor to the store.
	/// The record size is what's stored in the header (first record). <seealso cref="getRecordDataSize()"/> returns
	/// the size which was configured at the store creation, <seealso cref="getRecordSize()"/> returns what the store header says.
	/// </para>
	/// </summary>
	public abstract class AbstractDynamicStore : CommonAbstractStore<DynamicRecord, IntStoreHeader>, DynamicRecordAllocator
	{
		 public AbstractDynamicStore( File file, File idFile, Config conf, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, string typeDescriptor, int dataSizeFromConfiguration, RecordFormat<DynamicRecord> recordFormat, string storeVersion, params OpenOption[] openOptions ) : base( file, idFile, conf, idType, idGeneratorFactory, pageCache, logProvider, typeDescriptor, recordFormat, new DynamicStoreHeaderFormat( dataSizeFromConfiguration, recordFormat ), storeVersion, openOptions )
		 {
		 }

		 public static void AllocateRecordsFromBytes( ICollection<DynamicRecord> recordList, sbyte[] src, DynamicRecordAllocator dynamicRecordAllocator )
		 {
			  Debug.Assert( src != null, "Null src argument" );
			  DynamicRecord nextRecord = dynamicRecordAllocator.NextRecord();
			  int srcOffset = 0;
			  int dataSize = dynamicRecordAllocator.RecordDataSize;
			  do
			  {
					DynamicRecord record = nextRecord;
					record.StartRecord = srcOffset == 0;
					if ( src.Length - srcOffset > dataSize )
					{
						 sbyte[] data = new sbyte[dataSize];
						 Array.Copy( src, srcOffset, data, 0, dataSize );
						 record.Data = data;
						 nextRecord = dynamicRecordAllocator.NextRecord();
						 record.NextBlock = nextRecord.Id;
						 srcOffset += dataSize;
					}
					else
					{
						 sbyte[] data = new sbyte[src.Length - srcOffset];
						 Array.Copy( src, srcOffset, data, 0, data.Length );
						 record.Data = data;
						 nextRecord = null;
						 record.NextBlock = Record.NO_NEXT_BLOCK.intValue();
					}
					recordList.Add( record );
					Debug.Assert( record.Data != null );
			  } while ( nextRecord != null );
		 }

		 /// <returns> a <seealso cref="ByteBuffer.slice() sliced"/> <seealso cref="ByteBuffer"/> wrapping {@code target} or,
		 /// if necessary a new larger {@code byte[]} and containing exactly all concatenated data read from records </returns>
		 public static ByteBuffer ConcatData( ICollection<DynamicRecord> records, sbyte[] target )
		 {
			  int totalLength = 0;
			  foreach ( DynamicRecord record in records )
			  {
					totalLength += record.Length;
			  }

			  if ( target.Length < totalLength )
			  {
					target = new sbyte[totalLength];
			  }

			  ByteBuffer buffer = ByteBuffer.wrap( target, 0, totalLength );
			  foreach ( DynamicRecord record in records )
			  {
					buffer.put( record.Data );
			  }
			  buffer.position( 0 );
			  return buffer;
		 }

		 /// <returns> Pair&lt; header-in-first-record , all-other-bytes &gt; </returns>
		 public static Pair<sbyte[], sbyte[]> ReadFullByteArrayFromHeavyRecords( IEnumerable<DynamicRecord> records, PropertyType propertyType )
		 {
			  sbyte[] header = null;
			  IList<sbyte[]> byteList = new List<sbyte[]>();
			  int totalSize = 0;
			  int i = 0;
			  foreach ( DynamicRecord record in records )
			  {
					int offset = 0;
					if ( i++ == 0 )
					{ // This is the first one, read out the header separately
						 header = propertyType.readDynamicRecordHeader( record.Data );
						 offset = header.Length;
					}

					byteList.Add( record.Data );
					totalSize += record.Data.Length - offset;
			  }
			  sbyte[] bArray = new sbyte[totalSize];
			  Debug.Assert( header != null, "header should be non-null since records should not be empty: " + Iterables.ToString( records, ", " ) );
			  int sourceOffset = header.Length;
			  int offset = 0;
			  foreach ( sbyte[] currentArray in byteList )
			  {
					Array.Copy( currentArray, sourceOffset, bArray, offset, currentArray.Length - sourceOffset );
					offset += currentArray.Length - sourceOffset;
					sourceOffset = 0;
			  }
			  return Pair.of( header, bArray );
		 }

		 public override DynamicRecord NextRecord()
		 {
			  return StandardDynamicRecordAllocator.AllocateRecord( NextId() );
		 }

		 internal virtual void AllocateRecordsFromBytes( ICollection<DynamicRecord> target, sbyte[] src )
		 {
			  AllocateRecordsFromBytes( target, src, this );
		 }

		 public override string ToString()
		 {
			  return base.ToString() + "[fileName:" + StorageFileConflict.Name +
						 ", blockSize:" + RecordDataSize + "]";
		 }

		 internal virtual Pair<sbyte[], sbyte[]> ReadFullByteArray( IEnumerable<DynamicRecord> records, PropertyType propertyType )
		 {
			  foreach ( DynamicRecord record in records )
			  {
					EnsureHeavy( record );
			  }

			  return ReadFullByteArrayFromHeavyRecords( records, propertyType );
		 }

		 private class DynamicStoreHeaderFormat : IntStoreHeaderFormat
		 {
			  internal DynamicStoreHeaderFormat( int dataSizeFromConfiguration, RecordFormat<DynamicRecord> recordFormat ) : base( dataSizeFromConfiguration + recordFormat.RecordHeaderSize )
			  {
			  }

			  public override void WriteHeader( PageCursor cursor )
			  {
					if ( Header < 1 || Header > 0xFFFF )
					{
						 throw new System.ArgumentException( "Illegal block size[" + Header + "], limit is 65535" );
					}
					base.WriteHeader( cursor );
			  }
		 }
	}

}