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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v306
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;

	/// <summary>
	/// LEGEND:
	/// V: variable between 3B-8B
	/// 
	/// Record format:
	/// 1B   header
	/// VB   first relationship
	/// VB   first property
	/// 5B   labels
	/// => 12B-22B
	/// 
	/// Fixed reference record format:
	/// 1B   header
	/// 1B   modifiers
	/// 4B   first relationship
	/// 4B   first property
	/// 5B   labels
	/// => 15B
	/// </summary>
	internal class NodeRecordFormatV3_0_6 : BaseHighLimitRecordFormatV3_0_6<NodeRecord>
	{
		 internal const int RECORD_SIZE = 16;
		 // size of the record in fixed references format;
		 internal static readonly int FixedFormatRecordSize = HeaderByte + Byte.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Byte.BYTES;

		 private static readonly long _nullLabels = Record.NO_LABELS_FIELD.intValue();
		 private static readonly int _denseNodeBit = 0b0000_1000;
		 private static readonly int _hasRelationshipBit = 0b0001_0000;
		 private static readonly int _hasPropertyBit = 0b0010_0000;
		 private static readonly int _hasLabelsBit = 0b0100_0000;

		 private const long HIGH_DWORD_LOWER_NIBBLE_CHECK_MASK = 0xF_0000_0000L;
		 private const long HIGH_DWORD_LOWER_NIBBLE_MASK = unchecked( ( long )0xFFFF_FFF0_0000_0000L );
		 private const long LOWER_NIBBLE_READ_MASK = 0xFL;
		 private const long HIGHER_NIBBLE_READ_MASK = 0xF0L;

		 internal NodeRecordFormatV3_0_6() : this(RECORD_SIZE)
		 {
		 }

		 internal NodeRecordFormatV3_0_6( int recordSize ) : base( fixedRecordSize( recordSize ), 0 )
		 {
		 }

		 public override NodeRecord NewRecord()
		 {
			  return new NodeRecord( -1 );
		 }

		 protected internal override void DoReadInternal( NodeRecord record, PageCursor cursor, int recordSize, long headerByte, bool inUse )
		 {
			  // Interpret the header byte
			  bool dense = has( headerByte, _denseNodeBit );
			  if ( record.UseFixedReferences )
			  {
					// read record in a fixed reference format
					ReadFixedReferencesRecord( record, cursor, inUse, dense );
					record.UseFixedReferences = true;
			  }
			  else
			  {
					// Now read the rest of the data. The adapter will take care of moving the cursor over to the
					// other unit when we've exhausted the first one.
					long nextRel = DecodeCompressedReference( cursor, headerByte, _hasRelationshipBit, Null );
					long nextProp = DecodeCompressedReference( cursor, headerByte, _hasPropertyBit, Null );
					long labelField = DecodeCompressedReference( cursor, headerByte, _hasLabelsBit, _nullLabels );
					record.Initialize( inUse, nextProp, dense, nextRel, labelField );
			  }
		 }

		 public override int RequiredDataLength( NodeRecord record )
		 {
			  return Length( record.NextRel, Null ) + Length( record.NextProp, Null ) + Length( record.LabelField, _nullLabels );
		 }

		 protected internal override sbyte HeaderBits( NodeRecord record )
		 {
			  sbyte header = 0;
			  header = set( header, _denseNodeBit, record.Dense );
			  header = Set( header, _hasRelationshipBit, record.NextRel, Null );
			  header = Set( header, _hasPropertyBit, record.NextProp, Null );
			  header = Set( header, _hasLabelsBit, record.LabelField, _nullLabels );
			  return header;
		 }

		 protected internal override bool CanUseFixedReferences( NodeRecord record, int recordSize )
		 {
			  return IsRecordBigEnoughForFixedReferences( recordSize ) && ( record.NextProp == Null || ( record.NextProp & HIGH_DWORD_LOWER_NIBBLE_MASK ) == 0 ) && ( record.NextRel == Null || ( record.NextRel & HIGH_DWORD_LOWER_NIBBLE_MASK ) == 0 );
		 }

		 private bool IsRecordBigEnoughForFixedReferences( int recordSize )
		 {
			  return FixedFormatRecordSize <= recordSize;
		 }

		 protected internal override void DoWriteInternal( NodeRecord record, PageCursor cursor )
		 {
			  if ( record.UseFixedReferences )
			  {
					// write record in fixed reference format
					WriteFixedReferencesRecord( record, cursor );
			  }
			  else
			  {
					Encode( cursor, record.NextRel, Null );
					Encode( cursor, record.NextProp, Null );
					Encode( cursor, record.LabelField, _nullLabels );
			  }
		 }

		 private void ReadFixedReferencesRecord( NodeRecord record, PageCursor cursor, bool inUse, bool dense )
		 {
			  sbyte modifiers = cursor.Byte;
			  long relModifier = ( modifiers & LOWER_NIBBLE_READ_MASK ) << 32;
			  long propModifier = ( modifiers & HIGHER_NIBBLE_READ_MASK ) << 28;

			  long nextRel = cursor.Int & 0xFFFFFFFFL;
			  long nextProp = cursor.Int & 0xFFFFFFFFL;

			  long lsbLabels = cursor.Int & 0xFFFFFFFFL;
			  long hsbLabels = cursor.Byte & 0xFF; // so that a negative byte won't fill the "extended" bits with ones.
			  long labels = lsbLabels | ( hsbLabels << 32 );

			  record.Initialize( inUse, BaseHighLimitRecordFormatV3_0_6.longFromIntAndMod( nextProp, propModifier ), dense, BaseHighLimitRecordFormatV3_0_6.longFromIntAndMod( nextRel, relModifier ), labels );
		 }

		 private void WriteFixedReferencesRecord( NodeRecord record, PageCursor cursor )
		 {
			  long nextRel = record.NextRel;
			  long nextProp = record.NextProp;

			  short relModifier = nextRel == Null ? 0 : ( short )( ( nextRel & HIGH_DWORD_LOWER_NIBBLE_CHECK_MASK ) >> 32 );
			  short propModifier = nextProp == Null ? 0 : ( short )( ( nextProp & HIGH_DWORD_LOWER_NIBBLE_CHECK_MASK ) >> 28 );

			  // [    ,xxxx] higher bits for rel id
			  // [xxxx,    ] higher bits for prop id
			  short modifiers = ( short )( relModifier | propModifier );

			  cursor.PutByte( ( sbyte ) modifiers );
			  cursor.PutInt( ( int ) nextRel );
			  cursor.PutInt( ( int ) nextProp );

			  // lsb of labels
			  long labelField = record.LabelField;
			  cursor.PutInt( ( int ) labelField );
			  // msb of labels
			  cursor.PutByte( ( sbyte )( ( labelField & 0xFF_0000_0000L ) >> 32 ) );
		 }
	}

}