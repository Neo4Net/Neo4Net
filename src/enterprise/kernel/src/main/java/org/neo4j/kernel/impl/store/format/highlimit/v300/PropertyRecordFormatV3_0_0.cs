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
	using Neo4Net.Kernel.impl.store.format;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

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
	/// VB   previous property
	/// VB   next property
	/// 8B   property block
	/// 8B   property block
	/// 8B   property block
	/// 8B   property block
	/// 
	/// => 39B-49B
	/// </summary>
	public class PropertyRecordFormatV3_0_0 : BaseOneByteHeaderRecordFormat<PropertyRecord>
	{
		 public const int RECORD_SIZE = 48;

		 public PropertyRecordFormatV3_0_0() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, HighLimitV3_0_0.DEFAULT_MAXIMUM_BITS_PER_ID)
		 {
		 }

		 public override PropertyRecord NewRecord()
		 {
			  return new PropertyRecord( -1 );
		 }

		 public override void Read( PropertyRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  int offset = cursor.Offset;
			  sbyte headerByte = cursor.Byte;
			  bool inUse = IsInUse( headerByte );
			  if ( mode.shouldLoad( inUse ) )
			  {
					int blockCount = ( int )( ( uint )headerByte >> 4 );
					long recordId = record.Id;
					record.Initialize( inUse, toAbsolute( Reference.decode( cursor ), recordId ), toAbsolute( Reference.decode( cursor ), recordId ) );
					if ( ( blockCount > record.BlockCapacity ) | ( RECORD_SIZE - ( cursor.Offset - offset ) < blockCount * Long.BYTES ) )
					{
						 cursor.CursorException = "PropertyRecord claims to contain more blocks than can fit in a record";
						 return;
					}
					while ( blockCount-- > 0 )
					{
						 record.AddLoadedBlock( cursor.Long );
					}
			  }
		 }

		 public override void Write( PropertyRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					cursor.PutByte( ( sbyte )( IN_USE_BIT | NumberOfBlocks( record ) << 4 ) );
					long recordId = record.Id;
					Reference.encode( toRelative( record.PrevProp, recordId ), cursor );
					Reference.encode( toRelative( record.NextProp, recordId ), cursor );
					foreach ( PropertyBlock block in record )
					{
						 foreach ( long propertyBlock in block.ValueBlocks )
						 {
							  cursor.PutLong( propertyBlock );
						 }
					}
			  }
			  else
			  {
					MarkAsUnused( cursor );
			  }
		 }

		 private int NumberOfBlocks( PropertyRecord record )
		 {
			  int count = 0;
			  foreach ( PropertyBlock block in record )
			  {
					count += block.ValueBlocks.Length;
			  }
			  return count;
		 }

		 public override long GetNextRecordReference( PropertyRecord record )
		 {
			  return record.NextProp;
		 }
	}

}