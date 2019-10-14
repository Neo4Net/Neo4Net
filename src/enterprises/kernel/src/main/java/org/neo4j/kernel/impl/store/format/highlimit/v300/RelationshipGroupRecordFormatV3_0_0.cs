/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v300
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;

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
	/// 
	/// => 18B-43B
	/// </summary>
	public class RelationshipGroupRecordFormatV3_0_0 : BaseHighLimitRecordFormatV3_0_0<RelationshipGroupRecord>
	{
		 public const int RECORD_SIZE = 32;

		 private static readonly int _hasOutgoingBit = 0b0000_1000;
		 private static readonly int _hasIncomingBit = 0b0001_0000;
		 private static readonly int _hasLoopBit = 0b0010_0000;
		 private static readonly int _hasNextBit = 0b0100_0000;

		 public RelationshipGroupRecordFormatV3_0_0() : this(RECORD_SIZE)
		 {
		 }

		 private RelationshipGroupRecordFormatV3_0_0( int recordSize ) : base( fixedRecordSize( recordSize ), 0 )
		 {
		 }

		 public override RelationshipGroupRecord NewRecord()
		 {
			  return new RelationshipGroupRecord( -1 );
		 }

		 protected internal override void DoReadInternal( RelationshipGroupRecord record, PageCursor cursor, int recordSize, long headerByte, bool inUse )
		 {
			  record.Initialize( inUse, cursor.Short & 0xFFFF, DecodeCompressedReference( cursor, headerByte, _hasOutgoingBit, Null ), DecodeCompressedReference( cursor, headerByte, _hasIncomingBit, Null ), DecodeCompressedReference( cursor, headerByte, _hasLoopBit, Null ), DecodeCompressedReference( cursor ), DecodeCompressedReference( cursor, headerByte, _hasNextBit, Null ) );
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
			  return 2 + Length( record.FirstOut, Null ) + Length( record.FirstIn, Null ) + Length( record.FirstLoop, Null ) + Length( record.OwningNode ) + Length( record.Next, Null );
		 }

		 protected internal override void DoWriteInternal( RelationshipGroupRecord record, PageCursor cursor )
		 {
			  cursor.PutShort( ( short ) record.Type );
			  Encode( cursor, record.FirstOut, Null );
			  Encode( cursor, record.FirstIn, Null );
			  Encode( cursor, record.FirstLoop, Null );
			  Encode( cursor, record.OwningNode );
			  Encode( cursor, record.Next, Null );
		 }
	}

}