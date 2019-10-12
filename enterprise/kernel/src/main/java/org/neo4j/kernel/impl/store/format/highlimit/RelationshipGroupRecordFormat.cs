/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.impl.store.format.highlimit
{
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Org.Neo4j.Kernel.impl.store.format;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;

	/// <summary>
	/// LEGEND:
	/// V: variable between 3B-8B
	/// 
	/// Record format:
	/// 1B   header
	/// 2B   relationship type
	/// VB   first outgoing relationships
	/// VB   first incoming relationships
	/// VB   first loop relationships
	/// VB   owning node
	/// VB   next relationship group record
	/// => 18B-43B
	/// 
	/// Fixed reference format:
	/// 1B   header
	/// 1B   modifiers
	/// 2B   relationship type
	/// 4B   next relationship
	/// 4B   first outgoing relationship
	/// 4B   first incoming relationship
	/// 4B   first loop
	/// 4B   owning node
	/// => 24B
	/// </summary>
	internal class RelationshipGroupRecordFormat : BaseHighLimitRecordFormat<RelationshipGroupRecord>
	{
		 private const int TYPE_BYTES = 3;

		 internal const int RECORD_SIZE = 32;
		 internal static readonly int FixedFormatRecordSize = HeaderByte + Byte.BYTES + TYPE_BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES;

		 private static readonly int _hasOutgoingBit = 0b0000_1000;
		 private static readonly int _hasIncomingBit = 0b0001_0000;
		 private static readonly int _hasLoopBit = 0b0010_0000;
		 private static readonly int _hasNextBit = 0b0100_0000;

		 private static readonly int _nextRecordBit = 0b0000_0001;
		 private static readonly int _firstOutBit = 0b0000_0010;
		 private static readonly int _firstInBit = 0b0000_0100;
		 private static readonly int _firstLoopBit = 0b0000_1000;
		 private static readonly int _owningNodeBit = 0b0001_0000;

		 private const long ONE_BIT_OVERFLOW_BIT_MASK = unchecked( ( long )0xFFFF_FFFE_0000_0000L );
		 private const long HIGH_DWORD_LAST_BIT_MASK = 0x100000000L;

		 internal RelationshipGroupRecordFormat() : this(RECORD_SIZE)
		 {
		 }

		 internal RelationshipGroupRecordFormat( int recordSize ) : base( fixedRecordSize( recordSize ), 0, HighLimitFormatSettings.RELATIONSHIP_GROUP_MAXIMUM_ID_BITS )
		 {
		 }

		 public override RelationshipGroupRecord NewRecord()
		 {
			  return new RelationshipGroupRecord( -1 );
		 }

		 protected internal override void DoReadInternal( RelationshipGroupRecord record, PageCursor cursor, int recordSize, long headerByte, bool inUse )
		 {
			  if ( record.UseFixedReferences )
			  {
					// read record in fixed references format
					ReadFixedReferencesMethod( record, cursor, inUse );
					record.UseFixedReferences = true;
			  }
			  else
			  {
					int type = GetType( cursor );
					record.Initialize( inUse, type, DecodeCompressedReference( cursor, headerByte, _hasOutgoingBit, Null ), DecodeCompressedReference( cursor, headerByte, _hasIncomingBit, Null ), DecodeCompressedReference( cursor, headerByte, _hasLoopBit, Null ), DecodeCompressedReference( cursor ), DecodeCompressedReference( cursor, headerByte, _hasNextBit, Null ) );
			  }
		 }

		 protected internal override sbyte HeaderBits( RelationshipGroupRecord record )
		 {
			  sbyte header = 0;
			  header = Set( header, _hasOutgoingBit, record.FirstOut, Null );
			  header = Set( header, _hasIncomingBit, record.FirstIn, Null );
			  header = Set( header, _hasLoopBit, record.FirstLoop, Null );
			  header = Set( header, _hasNextBit, record.Next, Null );
			  return header;
		 }

		 protected internal override int RequiredDataLength( RelationshipGroupRecord record )
		 {
			  return TYPE_BYTES + Length( record.FirstOut, Null ) + Length( record.FirstIn, Null ) + Length( record.FirstLoop, Null ) + Length( record.OwningNode ) + Length( record.Next, Null );
		 }

		 protected internal override void DoWriteInternal( RelationshipGroupRecord record, PageCursor cursor )
		 {
			  if ( record.UseFixedReferences )
			  {
					// write record in fixed references format
					WriteFixedReferencesRecord( record, cursor );
			  }
			  else
			  {
					WriteType( cursor, record.Type );
					Encode( cursor, record.FirstOut, Null );
					Encode( cursor, record.FirstIn, Null );
					Encode( cursor, record.FirstLoop, Null );
					Encode( cursor, record.OwningNode );
					Encode( cursor, record.Next, Null );
			  }
		 }

		 protected internal override bool CanUseFixedReferences( RelationshipGroupRecord record, int recordSize )
		 {
			  return IsRecordBigEnoughForFixedReferences( recordSize ) && ( record.Next == Null || ( record.Next & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( record.FirstOut == Null || ( record.FirstOut & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( record.FirstIn == Null || ( record.FirstIn & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( record.FirstLoop == Null || ( record.FirstLoop & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( record.OwningNode == Null || ( record.OwningNode & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 );
		 }

		 private bool IsRecordBigEnoughForFixedReferences( int recordSize )
		 {
			  return FixedFormatRecordSize <= recordSize;
		 }

		 private void ReadFixedReferencesMethod( RelationshipGroupRecord record, PageCursor cursor, bool inUse )
		 {
			  // [    ,   x] high next bits
			  // [    ,  x ] high firstOut bits
			  // [    , x  ] high firstIn bits
			  // [    ,x   ] high firstLoop bits
			  // [   x,    ] high owner bits
			  long modifiers = cursor.Byte;

			  int type = GetType( cursor );

			  long nextLowBits = cursor.Int & 0xFFFFFFFFL;
			  long firstOutLowBits = cursor.Int & 0xFFFFFFFFL;
			  long firstInLowBits = cursor.Int & 0xFFFFFFFFL;
			  long firstLoopLowBits = cursor.Int & 0xFFFFFFFFL;
			  long owningNodeLowBits = cursor.Int & 0xFFFFFFFFL;

			  long nextMod = ( modifiers & _nextRecordBit ) << 32;
			  long firstOutMod = ( modifiers & _firstOutBit ) << 31;
			  long firstInMod = ( modifiers & _firstInBit ) << 30;
			  long firstLoopMod = ( modifiers & _firstLoopBit ) << 29;
			  long owningNodeMod = ( modifiers & _owningNodeBit ) << 28;

			  record.Initialize( inUse, type, BaseRecordFormat.longFromIntAndMod( firstOutLowBits, firstOutMod ), BaseRecordFormat.longFromIntAndMod( firstInLowBits, firstInMod ), BaseRecordFormat.longFromIntAndMod( firstLoopLowBits, firstLoopMod ), BaseRecordFormat.longFromIntAndMod( owningNodeLowBits, owningNodeMod ), BaseRecordFormat.longFromIntAndMod( nextLowBits, nextMod ) );
		 }

		 private void WriteFixedReferencesRecord( RelationshipGroupRecord record, PageCursor cursor )
		 {
			  long nextMod = record.Next == Null ? 0 : ( record.Next & HIGH_DWORD_LAST_BIT_MASK ) >> 32;
			  long firstOutMod = record.FirstOut == Null ? 0 : ( record.FirstOut & HIGH_DWORD_LAST_BIT_MASK ) >> 31;
			  long firstInMod = record.FirstIn == Null ? 0 : ( record.FirstIn & HIGH_DWORD_LAST_BIT_MASK ) >> 30;
			  long firstLoopMod = record.FirstLoop == Null ? 0 : ( record.FirstLoop & HIGH_DWORD_LAST_BIT_MASK ) >> 29;
			  long owningNodeMod = record.OwningNode == Null ? 0 : ( record.OwningNode & HIGH_DWORD_LAST_BIT_MASK ) >> 28;

			  // [    ,   x] high next bits
			  // [    ,  x ] high firstOut bits
			  // [    , x  ] high firstIn bits
			  // [    ,x   ] high firstLoop bits
			  // [   x,    ] high owner bits
			  cursor.PutByte( ( sbyte )( nextMod | firstOutMod | firstInMod | firstLoopMod | owningNodeMod ) );

			  WriteType( cursor, record.Type );

			  cursor.PutInt( ( int ) record.Next );
			  cursor.PutInt( ( int ) record.FirstOut );
			  cursor.PutInt( ( int ) record.FirstIn );
			  cursor.PutInt( ( int ) record.FirstLoop );
			  cursor.PutInt( ( int ) record.OwningNode );
		 }

		 private int GetType( PageCursor cursor )
		 {
			  int typeLowWord = cursor.Short & 0xFFFF;
			  int typeHighByte = cursor.Byte & 0xFF;
			  return ( typeHighByte << ( sizeof( short ) * 8 ) ) | typeLowWord;
		 }

		 private void WriteType( PageCursor cursor, int type )
		 {
			  cursor.PutShort( ( short ) type );
			  cursor.PutByte( ( sbyte )( ( int )( ( uint )type >> ( sizeof( short ) * 8 ) ) ) );
		 }
	}

}