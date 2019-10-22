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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v320
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.highlimit.Reference.toAbsolute;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.highlimit.Reference.toRelative;

	/// <summary>
	/// LEGEND:
	/// V: variable between 3B-8B
	/// 
	/// Record format:
	/// 1B   header
	/// 2B   relationship type
	/// VB   first property
	/// VB   start node
	/// VB   end node
	/// VB   start node chain previous relationship
	/// VB   start node chain next relationship
	/// VB   end node chain previous relationship
	/// VB   end node chain next relationship
	/// => 24B-59B
	/// 
	/// Fixed reference format:
	/// 1B   header
	/// 2B   relationship type
	/// 1B   modifiers
	/// 4B   start node
	/// 4B   end node
	/// 4B   start node chain previous relationship
	/// 4B   start node chain next relationship
	/// 4B   end node chain previous relationship
	/// 4B   end node chain next relationship
	/// => 28B
	/// </summary>
	internal class RelationshipRecordFormatV3_2_0 : BaseHighLimitRecordFormatV3_2_0<RelationshipRecord>
	{
		 internal const int RECORD_SIZE = 32;
		 internal static readonly int FixedFormatRecordSize = HeaderByte + Short.BYTES + Byte.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES;

		 private const int TYPE_FIELD_BYTES = 3;

		 private static readonly int _firstInFirstChainBit = 0b0000_1000;
		 private static readonly int _firstInSecondChainBit = 0b0001_0000;
		 private static readonly int _hasFirstChainNextBit = 0b0010_0000;
		 private static readonly int _hasSecondChainNextBit = 0b0100_0000;
		 private static readonly int _hasPropertyBit = 0b1000_0000;

		 private static readonly long _firstNodeBit = 0b0000_0001L;
		 private static readonly long _secondNodeBit = 0b0000_0010L;
		 private static readonly long _firstPrevRelBit = 0b0000_0100L;
		 private static readonly long _firstNextRelBit = 0b0000_1000L;
		 private static readonly long _secondRrevRelBit = 0b0001_0000L;
		 private static readonly long _secondNextRelBit = 0b0010_0000L;
		 private static readonly long _nextPropBit = 0b1100_0000L;

		 private const long ONE_BIT_OVERFLOW_BIT_MASK = unchecked( ( long )0xFFFF_FFFE_0000_0000L );
		 private const long THREE_BITS_OVERFLOW_BIT_MASK = unchecked( ( long )0xFFFF_FFFC_0000_0000L );
		 private const long HIGH_DWORD_LAST_BIT_MASK = 0x100000000L;

		 private const long TWO_BIT_FIXED_REFERENCE_BIT_MASK = 0x300000000L;

		 internal RelationshipRecordFormatV3_2_0() : this(RECORD_SIZE)
		 {
		 }

		 internal RelationshipRecordFormatV3_2_0( int recordSize ) : base( fixedRecordSize( recordSize ), 0, HighLimitFormatSettingsV3_2_0.RELATIONSHIP_MAXIMUM_ID_BITS )
		 {
		 }

		 public override RelationshipRecord NewRecord()
		 {
			  return new RelationshipRecord( -1 );
		 }

		 protected internal override void DoReadInternal( RelationshipRecord record, PageCursor cursor, int recordSize, long headerByte, bool inUse )
		 {
			  if ( record.UseFixedReferences )
			  {
					int type = cursor.Short & 0xFFFF;
					// read record in fixed reference format
					ReadFixedReferencesRecord( record, cursor, headerByte, inUse, type );
					record.UseFixedReferences = true;
			  }
			  else
			  {
					int typeLowWord = cursor.Short & 0xFFFF;
					int typeHighWord = cursor.Byte & 0xFF;
					int type = ( typeHighWord << ( sizeof( short ) * 8 ) ) | typeLowWord;
					long recordId = record.Id;
					record.Initialize( inUse, DecodeCompressedReference( cursor, headerByte, _hasPropertyBit, Null ), DecodeCompressedReference( cursor ), DecodeCompressedReference( cursor ), type, DecodeAbsoluteOrRelative( cursor, headerByte, _firstInFirstChainBit, recordId ), DecodeAbsoluteIfPresent( cursor, headerByte, _hasFirstChainNextBit, recordId ), DecodeAbsoluteOrRelative( cursor, headerByte, _firstInSecondChainBit, recordId ), DecodeAbsoluteIfPresent( cursor, headerByte, _hasSecondChainNextBit, recordId ), has( headerByte, _firstInFirstChainBit ), has( headerByte, _firstInSecondChainBit ) );
			  }
		 }

		 protected internal override sbyte HeaderBits( RelationshipRecord record )
		 {
			  sbyte header = 0;
			  header = set( header, _firstInFirstChainBit, record.FirstInFirstChain );
			  header = set( header, _firstInSecondChainBit, record.FirstInSecondChain );
			  header = Set( header, _hasPropertyBit, record.NextProp, Null );
			  header = Set( header, _hasFirstChainNextBit, record.FirstNextRel, Null );
			  header = Set( header, _hasSecondChainNextBit, record.SecondNextRel, Null );
			  return header;
		 }

		 protected internal override int RequiredDataLength( RelationshipRecord record )
		 {
			  long recordId = record.Id;
			  return TYPE_FIELD_BYTES + Length( record.NextProp, Null ) + Length( record.FirstNode ) + Length( record.SecondNode ) + Length( GetFirstPrevReference( record, recordId ) ) + GetRelativeReferenceLength( record.FirstNextRel, recordId ) + Length( GetSecondPrevReference( record, recordId ) ) + GetRelativeReferenceLength( record.SecondNextRel, recordId );
		 }

		 protected internal override void DoWriteInternal( RelationshipRecord record, PageCursor cursor )
		 {
			  if ( record.UseFixedReferences )
			  {
					// write record in fixed reference format
					WriteFixedReferencesRecord( record, cursor );
			  }
			  else
			  {
					int type = record.Type;
					cursor.PutShort( ( short ) type );
					cursor.PutByte( ( sbyte )( ( int )( ( uint )type >> ( sizeof( short ) * 8 ) ) ) );

					long recordId = record.Id;
					Encode( cursor, record.NextProp, Null );
					Encode( cursor, record.FirstNode );
					Encode( cursor, record.SecondNode );

					Encode( cursor, GetFirstPrevReference( record, recordId ) );
					if ( record.FirstNextRel != Null )
					{
						 Encode( cursor, toRelative( record.FirstNextRel, recordId ) );
					}
					Encode( cursor, GetSecondPrevReference( record, recordId ) );
					if ( record.SecondNextRel != Null )
					{
						 Encode( cursor, toRelative( record.SecondNextRel, recordId ) );
					}
			  }
		 }

		 protected internal override bool CanUseFixedReferences( RelationshipRecord record, int recordSize )
		 {
				  return ( IsRecordBigEnoughForFixedReferences( recordSize ) && ( record.Type < ( 1 << ( sizeof( short ) * 8 ) ) ) && ( record.FirstNode & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( ( record.SecondNode & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) && ( ( record.FirstPrevRel == Null ) || ( ( record.FirstPrevRel & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) ) && ( ( record.FirstNextRel == Null ) || ( ( record.FirstNextRel & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) ) && ( ( record.SecondPrevRel == Null ) || ( ( record.SecondPrevRel & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) ) && ( ( record.SecondNextRel == Null ) || ( ( record.SecondNextRel & ONE_BIT_OVERFLOW_BIT_MASK ) == 0 ) ) && ( ( record.NextProp == Null ) || ( ( record.NextProp & THREE_BITS_OVERFLOW_BIT_MASK ) == 0 ) );
		 }

		 private bool IsRecordBigEnoughForFixedReferences( int recordSize )
		 {
			  return FixedFormatRecordSize <= recordSize;
		 }

		 private long DecodeAbsoluteOrRelative( PageCursor cursor, long headerByte, int firstInStartBit, long recordId )
		 {
			  return has( headerByte, firstInStartBit ) ? DecodeCompressedReference( cursor ) : toAbsolute( DecodeCompressedReference( cursor ), recordId );
		 }

		 private long GetSecondPrevReference( RelationshipRecord record, long recordId )
		 {
			  return record.FirstInSecondChain ? record.SecondPrevRel : toRelative( record.SecondPrevRel, recordId );
		 }

		 private long GetFirstPrevReference( RelationshipRecord record, long recordId )
		 {
			  return record.FirstInFirstChain ? record.FirstPrevRel : toRelative( record.FirstPrevRel, recordId );
		 }

		 private int GetRelativeReferenceLength( long absoluteReference, long recordId )
		 {
			  return absoluteReference != Null ? Length( toRelative( absoluteReference, recordId ) ) : 0;
		 }

		 private long DecodeAbsoluteIfPresent( PageCursor cursor, long headerByte, int conditionBit, long recordId )
		 {
			  return has( headerByte, conditionBit ) ? toAbsolute( DecodeCompressedReference( cursor ), recordId ) : Null;
		 }

		 private void ReadFixedReferencesRecord( RelationshipRecord record, PageCursor cursor, long headerByte, bool inUse, int type )
		 {
			  // [    ,   x] first node higher order bits
			  // [    ,  x ] second node high order bits
			  // [    , x  ] first prev high order bits
			  // [    ,x   ] first next high order bits
			  // [   x,    ] second prev high order bits
			  // [  x ,    ] second next high order bits
			  // [xx  ,    ] next prop high order bits
			  long modifiers = cursor.Byte;

			  long firstNode = cursor.Int & 0xFFFFFFFFL;
			  long firstNodeMod = ( modifiers & _firstNodeBit ) << 32;

			  long secondNode = cursor.Int & 0xFFFFFFFFL;
			  long secondNodeMod = ( modifiers & _secondNodeBit ) << 31;

			  long firstPrevRel = cursor.Int & 0xFFFFFFFFL;
			  long firstPrevRelMod = ( modifiers & _firstPrevRelBit ) << 30;

			  long firstNextRel = cursor.Int & 0xFFFFFFFFL;
			  long firstNextRelMod = ( modifiers & _firstNextRelBit ) << 29;

			  long secondPrevRel = cursor.Int & 0xFFFFFFFFL;
			  long secondPrevRelMod = ( modifiers & _secondRrevRelBit ) << 28;

			  long secondNextRel = cursor.Int & 0xFFFFFFFFL;
			  long secondNextRelMod = ( modifiers & _secondNextRelBit ) << 27;

			  long nextProp = cursor.Int & 0xFFFFFFFFL;
			  long nextPropMod = ( modifiers & _nextPropBit ) << 26;

			  record.Initialize( inUse, BaseRecordFormat.longFromIntAndMod( nextProp, nextPropMod ), BaseRecordFormat.longFromIntAndMod( firstNode, firstNodeMod ), BaseRecordFormat.longFromIntAndMod( secondNode, secondNodeMod ), type, BaseRecordFormat.longFromIntAndMod( firstPrevRel, firstPrevRelMod ), BaseRecordFormat.longFromIntAndMod( firstNextRel, firstNextRelMod ), BaseRecordFormat.longFromIntAndMod( secondPrevRel, secondPrevRelMod ), BaseRecordFormat.longFromIntAndMod( secondNextRel, secondNextRelMod ), has( headerByte, _firstInFirstChainBit ), has( headerByte, _firstInSecondChainBit ) );
		 }

		 private void WriteFixedReferencesRecord( RelationshipRecord record, PageCursor cursor )
		 {
			  cursor.PutShort( ( short ) record.Type );

			  long firstNode = record.FirstNode;
			  short firstNodeMod = ( short )( ( firstNode & HIGH_DWORD_LAST_BIT_MASK ) >> 32 );

			  long secondNode = record.SecondNode;
			  long secondNodeMod = ( secondNode & HIGH_DWORD_LAST_BIT_MASK ) >> 31;

			  long firstPrevRel = record.FirstPrevRel;
			  long firstPrevRelMod = firstPrevRel == Null ? 0 : ( firstPrevRel & HIGH_DWORD_LAST_BIT_MASK ) >> 30;

			  long firstNextRel = record.FirstNextRel;
			  long firstNextRelMod = firstNextRel == Null ? 0 : ( firstNextRel & HIGH_DWORD_LAST_BIT_MASK ) >> 29;

			  long secondPrevRel = record.SecondPrevRel;
			  long secondPrevRelMod = secondPrevRel == Null ? 0 : ( secondPrevRel & HIGH_DWORD_LAST_BIT_MASK ) >> 28;

			  long secondNextRel = record.SecondNextRel;
			  long secondNextRelMod = secondNextRel == Null ? 0 : ( secondNextRel & HIGH_DWORD_LAST_BIT_MASK ) >> 27;

			  long nextProp = record.NextProp;
			  long nextPropMod = nextProp == Null ? 0 : ( nextProp & TWO_BIT_FIXED_REFERENCE_BIT_MASK ) >> 26;

			  // [    ,   x] first node higher order bits
			  // [    ,  x ] second node high order bits
			  // [    , x  ] first prev high order bits
			  // [    ,x   ] first next high order bits
			  // [   x,    ] second prev high order bits
			  // [  x ,    ] second next high order bits
			  // [xx  ,    ] next prop high order bits
			  short modifiers = ( short )( firstNodeMod | secondNodeMod | firstPrevRelMod | firstNextRelMod | secondPrevRelMod | secondNextRelMod | nextPropMod );

			  cursor.PutByte( ( sbyte ) modifiers );
			  cursor.PutInt( ( int ) firstNode );
			  cursor.PutInt( ( int ) secondNode );
			  cursor.PutInt( ( int ) firstPrevRel );
			  cursor.PutInt( ( int ) firstNextRel );
			  cursor.PutInt( ( int ) secondPrevRel );
			  cursor.PutInt( ( int ) secondNextRel );
			  cursor.PutInt( ( int ) nextProp );
		 }
	}

}