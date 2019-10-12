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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v300
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;

	/// <summary>
	/// LEGEND:
	/// V: variable between 3B-8B
	/// 
	/// Record format:
	/// 1B   header
	/// VB   first relationship
	/// VB   first property
	/// 5B   labels
	/// 
	/// => 12B-22B
	/// </summary>
	public class NodeRecordFormatV3_0_0 : BaseHighLimitRecordFormatV3_0_0<NodeRecord>
	{
		 public const int RECORD_SIZE = 16;

		 private static readonly long _nullLabels = Record.NO_LABELS_FIELD.intValue();
		 private static readonly int _denseNodeBit = 0b0000_1000;
		 private static readonly int _hasRelationshipBit = 0b0001_0000;
		 private static readonly int _hasPropertyBit = 0b0010_0000;
		 private static readonly int _hasLabelsBit = 0b0100_0000;

		 public NodeRecordFormatV3_0_0() : this(RECORD_SIZE)
		 {
		 }

		 private NodeRecordFormatV3_0_0( int recordSize ) : base( fixedRecordSize( recordSize ), 0 )
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

			  // Now read the rest of the data. The adapter will take care of moving the cursor over to the
			  // other unit when we've exhausted the first one.
			  long nextRel = DecodeCompressedReference( cursor, headerByte, _hasRelationshipBit, Null );
			  long nextProp = DecodeCompressedReference( cursor, headerByte, _hasPropertyBit, Null );
			  long labelField = DecodeCompressedReference( cursor, headerByte, _hasLabelsBit, _nullLabels );
			  record.Initialize( inUse, nextProp, dense, nextRel, labelField );
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

		 protected internal override void DoWriteInternal( NodeRecord record, PageCursor cursor )
		 {
			  Encode( cursor, record.NextRel, Null );
			  Encode( cursor, record.NextProp, Null );
			  Encode( cursor, record.LabelField, _nullLabels );
		 }
	}

}