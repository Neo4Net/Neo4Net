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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v300
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.highlimit.Reference.toAbsolute;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.highlimit.Reference.toRelative;

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
	/// 
	/// => 24B-59B
	/// </summary>
	public class RelationshipRecordFormatV3_0_0 : BaseHighLimitRecordFormatV3_0_0<RelationshipRecord>
	{
		 public const int RECORD_SIZE = 32;

		 private static readonly int _firstInFirstChainBit = 0b0000_1000;
		 private static readonly int _firstInSecondChainBit = 0b0001_0000;
		 private static readonly int _hasFirstChainNextBit = 0b0010_0000;
		 private static readonly int _hasSecondChainNextBit = 0b0100_0000;
		 private static readonly int _hasPropertyBit = 0b1000_0000;

		 public RelationshipRecordFormatV3_0_0() : this(RECORD_SIZE)
		 {
		 }

		 internal RelationshipRecordFormatV3_0_0( int recordSize ) : base( fixedRecordSize( recordSize ), 0 )
		 {
		 }

		 public override RelationshipRecord NewRecord()
		 {
			  return new RelationshipRecord( -1 );
		 }

		 protected internal override void DoReadInternal( RelationshipRecord record, PageCursor cursor, int recordSize, long headerByte, bool inUse )
		 {
			  int type = cursor.Short & 0xFFFF;
			  long recordId = record.Id;
			  record.Initialize( inUse, DecodeCompressedReference( cursor, headerByte, _hasPropertyBit, Null ), DecodeCompressedReference( cursor ), DecodeCompressedReference( cursor ), type, DecodeAbsoluteOrRelative( cursor, headerByte, _firstInFirstChainBit, recordId ), DecodeAbsoluteIfPresent( cursor, headerByte, _hasFirstChainNextBit, recordId ), DecodeAbsoluteOrRelative( cursor, headerByte, _firstInSecondChainBit, recordId ), DecodeAbsoluteIfPresent( cursor, headerByte, _hasSecondChainNextBit, recordId ), has( headerByte, _firstInFirstChainBit ), has( headerByte, _firstInSecondChainBit ) );
		 }

		 private long DecodeAbsoluteOrRelative( PageCursor cursor, long headerByte, int firstInStartBit, long recordId )
		 {
			  return has( headerByte, firstInStartBit ) ? DecodeCompressedReference( cursor ) : toAbsolute( DecodeCompressedReference( cursor ), recordId );
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
			  return Short.BYTES + Length( record.NextProp, Null ) + Length( record.FirstNode ) + Length( record.SecondNode ) + Length( GetFirstPrevReference( record, recordId ) ) + GetRelativeReferenceLength( record.FirstNextRel, recordId ) + Length( GetSecondPrevReference( record, recordId ) ) + GetRelativeReferenceLength( record.SecondNextRel, recordId );
		 }

		 protected internal override void DoWriteInternal( RelationshipRecord record, PageCursor cursor )
		 {
			  cursor.PutShort( ( short ) record.Type );
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
	}

}