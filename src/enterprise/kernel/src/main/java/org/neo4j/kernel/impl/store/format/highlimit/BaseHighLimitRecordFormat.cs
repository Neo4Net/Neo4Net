/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using CompositePageCursor = Neo4Net.Io.pagecache.impl.CompositePageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using Neo4Net.Kernel.impl.store.format;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.RecordPageLocationCalculator.offsetForId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.RecordPageLocationCalculator.pageIdForRecord;

	/// <summary>
	/// Base class for record format which utilizes dynamically sized references to other record IDs and with ability
	/// to use record units, meaning that a record may span two physical records in the store. This to keep store size
	/// low and only have records that have big references occupy double amount of space. This format supports up to
	/// 58-bit IDs, which is roughly 280 quadrillion. With that size the ID limits can be considered highlimit,
	/// hence the format name. The IDs take up between 3-8B depending on the size of the ID where relative ID
	/// references are used as often as possible. See <seealso cref="Reference"/>.
	/// 
	/// In case when record is small enough to fit into one record unit and references are not that big yet
	/// record can be stored in a fixed reference format. Representing record in this format allow
	/// to save some time on reference encoding/decoding since they will be saved in fixed format instead of
	/// variable length encoding.
	/// Fixed reference encoding can be applied only to single record unit records.
	/// And since record will always contain only one single unit we can reuse bit number 4 as a marker for fixed
	/// reference.
	/// To be able to read previously stored records and distinguish them from fixed reference records marker bit is
	/// inverted: 0 - fixed reference format use, 1 - variable length encoding used.
	/// 
	/// For consistency, all formats have a one-byte header specifying:
	/// 
	/// <ol>
	/// <li>0x1: inUse [0=unused, 1=used]</li>
	/// <li>0x2: record unit [0=single record, 1=multiple records]</li>
	/// <li>0x4: record unit type [1=first, 0=consecutive]; fixed reference mark [0=fixed reference; 1=variable length
	/// encoding]
	/// <li>0x8 - 0x80 other flags for this record specific to each type</li>
	/// </ol>
	/// 
	/// NOTE to the rest of the flags is that a good use of them is to denote whether or not an ID reference is
	/// null (-1) as to save 3B (smallest compressed size) by not writing a reference at all.
	/// 
	/// For records that are the first out of multiple record units, then immediately following the header byte is
	/// the reference (3-8B) to the secondary ID. After that the "statically sized" data and in the end the
	/// dynamically sized data. The general thinking is that the break-off into the secondary record will happen in
	/// the sequence of dynamically sized references and this will allow for crossing the record boundary
	/// in between, but even in the middle of, references quite easily since the <seealso cref="CompositePageCursor"/>
	/// handles the transition seamlessly.
	/// 
	/// Assigning secondary record unit IDs is done outside of this format implementation, it is just assumed
	/// that records that gets <seealso cref="RecordFormat.write(AbstractBaseRecord, PageCursor, int) written"/> have already
	/// been assigned all required such data.
	/// 
	/// Usually each records are written and read atomically, so this format requires additional logic to be able to
	/// write and read multiple records together atomically. For writing then currently this is guarded by
	/// higher level IEntity write locks and so the <seealso cref="PageCursor"/> can simply move from the first on to the second
	/// record and continue writing. For reading, which is optimistic and may require retry, one additional
	/// <seealso cref="PageCursor"/> needs to be acquired over the second record, checking <seealso cref="PageCursor.shouldRetry()"/>
	/// on both and potentially re-reading the second or both until a consistent read was had.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/> </param>
	internal abstract class BaseHighLimitRecordFormat<RECORD> : BaseOneByteHeaderRecordFormat<RECORD> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 internal static readonly int HeaderByte = Byte.BYTES;

		 internal static readonly long Null = Record.NULL_REFERENCE.intValue();
		 internal static readonly int HeaderBitRecordUnit = 0b0000_0010;
		 internal static readonly int HeaderBitFirstRecordUnit = 0b0000_0100;
		 internal static readonly int HeaderBitFixedReference = 0b0000_0100;

		 protected internal BaseHighLimitRecordFormat( System.Func<StoreHeader, int> recordSize, int recordHeaderSize, int maxIdBits ) : base( recordSize, recordHeaderSize, IN_USE_BIT, maxIdBits )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(RECORD record, Neo4Net.io.pagecache.PageCursor primaryCursor, Neo4Net.kernel.impl.store.record.RecordLoad mode, int recordSize) throws java.io.IOException
		 public override void Read( RECORD record, PageCursor primaryCursor, RecordLoad mode, int recordSize )
		 {
			  int primaryStartOffset = primaryCursor.Offset;
			  sbyte headerByte = primaryCursor.Byte;
			  bool inUse = isInUse( headerByte );
			  bool doubleRecordUnit = has( headerByte, HeaderBitRecordUnit );
			  record.UseFixedReferences = false;
			  if ( doubleRecordUnit )
			  {
					bool firstRecordUnit = has( headerByte, HeaderBitFirstRecordUnit );
					if ( !firstRecordUnit )
					{
						 // This is a record unit and not even the first one, so you cannot go here directly and read it,
						 // it may only be read as part of reading the primary unit.
						 record.clear();
						 // Return and try again
						 primaryCursor.CursorException = "Expected record to be the first unit in the chain, but record header says it's not";
						 return;
					}

					// This is a record that is split into multiple record units. We need a bit more clever
					// data structures here. For the time being this means instantiating one object,
					// but the trade-off is a great reduction in complexity.
					long secondaryId = Reference.decode( primaryCursor );
					long pageId = pageIdForRecord( secondaryId, primaryCursor.CurrentPageSize, recordSize );
					int offset = offsetForId( secondaryId, primaryCursor.CurrentPageSize, recordSize );
					PageCursor secondaryCursor = primaryCursor.OpenLinkedCursor( pageId );
					if ( ( !secondaryCursor.Next() ) | offset < 0 )
					{
						 // We must have made an inconsistent read of the secondary record unit reference.
						 // No point in trying to read this.
						 record.clear();
						 primaryCursor.CursorException = IllegalSecondaryReferenceMessage( pageId );
						 return;
					}
					secondaryCursor.Offset = offset + HeaderByte;
					int primarySize = recordSize - ( primaryCursor.Offset - primaryStartOffset );
					// We *could* sanity check the secondary record header byte here, but we won't. If it is wrong, then we most
					// likely did an inconsistent read, in which case we'll just retry. Otherwise, if the header byte is wrong,
					// then there is little we can do about it here, since we are not allowed to throw exceptions.

					int secondarySize = recordSize - HeaderByte;
					PageCursor composite = CompositePageCursor.compose( primaryCursor, primarySize, secondaryCursor, secondarySize );
					DoReadInternal( record, composite, recordSize, headerByte, inUse );
					record.SecondaryUnitId = secondaryId;
			  }
			  else
			  {
					record.UseFixedReferences = IsUseFixedReferences( headerByte );
					DoReadInternal( record, primaryCursor, recordSize, headerByte, inUse );
			  }

			  // Set cursor offset to next record to prepare next read in case of scanning.
			  primaryCursor.Offset = primaryStartOffset + recordSize;
		 }

		 private bool IsUseFixedReferences( sbyte headerByte )
		 {
			  return !has( headerByte, HeaderBitFixedReference );
		 }

		 private string IllegalSecondaryReferenceMessage( long secondaryId )
		 {
			  return "Illegal secondary record reference: " + secondaryId;
		 }

		 protected internal abstract void DoReadInternal( RECORD record, PageCursor cursor, int recordSize, long inUseByte, bool inUse );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(RECORD record, Neo4Net.io.pagecache.PageCursor primaryCursor, int recordSize) throws java.io.IOException
		 public override void Write( RECORD record, PageCursor primaryCursor, int recordSize )
		 {
			  if ( record.inUse() )
			  {
					// Let the specific implementation provide the additional header bits and we'll provide the core format bits.
					sbyte headerByte = HeaderBits( record );
					assert( headerByte & 0x7 ) == 0 : "Format-specific header bits (" + headerByte +
							  ") collides with format-generic header bits";
					headerByte = set( headerByte, IN_USE_BIT, record.inUse() );
					headerByte = set( headerByte, HeaderBitRecordUnit, record.requiresSecondaryUnit() );
					if ( record.requiresSecondaryUnit() )
					{
						 headerByte = set( headerByte, HeaderBitFirstRecordUnit, true );
					}
					else
					{
						 headerByte = set( headerByte, HeaderBitFixedReference, !record.UseFixedReferences );
					}
					primaryCursor.PutByte( headerByte );

					if ( record.requiresSecondaryUnit() )
					{
						 // Write using the normal adapter since the first reference we write cannot really overflow
						 // into the secondary record
						 long secondaryUnitId = record.SecondaryUnitId;
						 long pageId = pageIdForRecord( secondaryUnitId, primaryCursor.CurrentPageSize, recordSize );
						 int offset = offsetForId( secondaryUnitId, primaryCursor.CurrentPageSize, recordSize );
						 PageCursor secondaryCursor = primaryCursor.OpenLinkedCursor( pageId );
						 if ( !secondaryCursor.Next() )
						 {
							  // We are not allowed to write this much data to the file, apparently.
							  record.clear();
							  return;
						 }
						 secondaryCursor.Offset = offset;
						 secondaryCursor.PutByte( ( sbyte )( IN_USE_BIT | HeaderBitRecordUnit ) );
						 int recordSizeWithoutHeader = recordSize - HeaderByte;
						 PageCursor composite = CompositePageCursor.compose( primaryCursor, recordSizeWithoutHeader, secondaryCursor, recordSizeWithoutHeader );

						 Reference.encode( secondaryUnitId, composite );
						 DoWriteInternal( record, composite );
					}
					else
					{
						 DoWriteInternal( record, primaryCursor );
					}
			  }
			  else
			  {
					MarkAsUnused( primaryCursor, record, recordSize );
			  }
		 }

		 /*
		  * Use this instead of {@link #markFirstByteAsUnused(PageCursor)} to mark both record units,
		  * if record has a reference to a secondary unit.
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void markAsUnused(Neo4Net.io.pagecache.PageCursor cursor, RECORD record, int recordSize) throws java.io.IOException
		 protected internal virtual void MarkAsUnused( PageCursor cursor, RECORD record, int recordSize )
		 {
			  markAsUnused( cursor );
			  if ( record.hasSecondaryUnitId() )
			  {
					long secondaryUnitId = record.SecondaryUnitId;
					long pageIdForSecondaryRecord = pageIdForRecord( secondaryUnitId, cursor.CurrentPageSize, recordSize );
					int offsetForSecondaryId = offsetForId( secondaryUnitId, cursor.CurrentPageSize, recordSize );
					if ( !cursor.Next( pageIdForSecondaryRecord ) )
					{
						 throw new UnderlyingStorageException( "Couldn't move to secondary page " + pageIdForSecondaryRecord );
					}
					cursor.Offset = offsetForSecondaryId;
					markAsUnused( cursor );
			  }
		 }

		 protected internal abstract void DoWriteInternal( RECORD record, PageCursor cursor );

		 protected internal abstract sbyte HeaderBits( RECORD record );

		 public override void Prepare( RECORD record, int recordSize, IdSequence idSequence )
		 {
			  if ( record.inUse() )
			  {
					record.UseFixedReferences = CanUseFixedReferences( record, recordSize );
					if ( !record.UseFixedReferences )
					{
						 int requiredLength = HeaderByte + RequiredDataLength( record );
						 bool requiresSecondaryUnit = requiredLength > recordSize;
						 record.RequiresSecondaryUnit = requiresSecondaryUnit;
						 if ( record.requiresSecondaryUnit() && !record.hasSecondaryUnitId() )
						 {
							  // Allocate a new id at this point, but this is not the time to free this ID the the case where
							  // this record doesn't need this secondary unit anymore... that needs to be done when applying to store.
							  record.SecondaryUnitId = idSequence.NextId();
						 }
					}
			  }
		 }

		 protected internal abstract bool CanUseFixedReferences( RECORD record, int recordSize );

		 /// <summary>
		 /// Required length of the data in the given record (without the header byte).
		 /// </summary>
		 /// <param name="record"> data to check how much space it would require. </param>
		 /// <returns> length required to store the data in the given record. </returns>
		 protected internal abstract int RequiredDataLength( RECORD record );

		 protected internal static int Length( long reference )
		 {
			  return Reference.length( reference );
		 }

		 protected internal static int Length( long reference, long nullValue )
		 {
			  return reference == nullValue ? 0 : Length( reference );
		 }

		 protected internal static long DecodeCompressedReference( PageCursor cursor )
		 {
			  return Reference.decode( cursor );
		 }

		 protected internal static long DecodeCompressedReference( PageCursor cursor, long headerByte, int headerBitMask, long nullValue )
		 {
			  return has( headerByte, headerBitMask ) ? DecodeCompressedReference( cursor ) : nullValue;
		 }

		 protected internal static void Encode( PageCursor cursor, long reference )
		 {
			  Reference.encode( reference, cursor );
		 }

		 protected internal static void Encode( PageCursor cursor, long reference, long nullValue )
		 {
			  if ( reference != nullValue )
			  {
					Reference.encode( reference, cursor );
			  }
		 }

		 protected internal static sbyte Set( sbyte header, int bitMask, long reference, long nullValue )
		 {
			  return set( header, bitMask, reference != nullValue );
		 }
	}

}