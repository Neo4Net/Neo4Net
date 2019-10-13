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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v306
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.Reference.toAbsolute;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.Reference.toRelative;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.v306.BaseHighLimitRecordFormatV3_0_6.HEADER_BIT_FIXED_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.v306.BaseHighLimitRecordFormatV3_0_6.HEADER_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.v306.BaseHighLimitRecordFormatV3_0_6.NULL;

	/// <summary>
	/// <pre>
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
	/// => 39B-49B
	/// 
	/// Fixed reference format:
	/// 1B   header
	/// 6B   previous property
	/// 6B   next property
	/// 3B   padding
	/// 8B   property block
	/// 8B   property block
	/// 8B   property block
	/// 8B   property block
	/// => 48B
	/// 
	/// </pre>
	/// Unlike other high limit records <seealso cref="BaseHighLimitRecordFormatV3_0_6"/> fixed reference marker in property record
	/// format header is not inverted: 1 - fixed reference format used; 0 - variable length format used.
	/// </summary>
	internal class PropertyRecordFormatV3_0_6 : BaseOneByteHeaderRecordFormat<PropertyRecord>
	{
		 internal const int RECORD_SIZE = 48;
		 private const int PROPERTY_BLOCKS_PADDING = 3;
		 internal static readonly int FixedFormatRecordSize = HEADER_BYTE + Short.BYTES + Integer.BYTES + Short.BYTES + Integer.BYTES + PROPERTY_BLOCKS_PADDING;

		 private const long HIGH_DWORD_LOWER_WORD_MASK = 0xFFFF_0000_0000L;
		 private const long HIGH_DWORD_LOWER_WORD_CHECK_MASK = unchecked( ( long )0xFFFF_0000_0000_0000L );

		 protected internal PropertyRecordFormatV3_0_6() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, HighLimitV3_0_6.DEFAULT_MAXIMUM_BITS_PER_ID)
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
			  bool useFixedReferences = Has( headerByte, HEADER_BIT_FIXED_REFERENCE );
			  if ( mode.shouldLoad( inUse ) )
			  {
					int blockCount = ( int )( ( uint )headerByte >> 4 );
					long recordId = record.Id;

					if ( useFixedReferences )
					{
						 // read record in a fixed reference format
						 ReadFixedReferencesRecord( record, cursor );
					}
					else
					{
						 record.Initialize( inUse, toAbsolute( Reference.decode( cursor ), recordId ), toAbsolute( Reference.decode( cursor ), recordId ) );
					}
					record.UseFixedReferences = useFixedReferences;
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
					sbyte headerByte = ( sbyte )( IN_USE_BIT | NumberOfBlocks( record ) << 4 );
					bool canUseFixedReferences = canUseFixedReferences( record, recordSize );
					record.UseFixedReferences = canUseFixedReferences;
					headerByte = Set( headerByte, HEADER_BIT_FIXED_REFERENCE, canUseFixedReferences );
					cursor.PutByte( headerByte );

					long recordId = record.Id;

					if ( canUseFixedReferences )
					{
						 // write record in fixed reference format
						 WriteFixedReferencesRecord( record, cursor );
					}
					else
					{
						 Reference.encode( toRelative( record.PrevProp, recordId ), cursor );
						 Reference.encode( toRelative( record.NextProp, recordId ), cursor );
					}
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

		 private bool CanUseFixedReferences( PropertyRecord record, int recordSize )
		 {
			  return IsRecordBigEnoughForFixedReferences( recordSize ) && ( record.NextProp == NULL || ( record.NextProp & HIGH_DWORD_LOWER_WORD_CHECK_MASK ) == 0 ) && ( record.PrevProp == NULL || ( record.PrevProp & HIGH_DWORD_LOWER_WORD_CHECK_MASK ) == 0 );
		 }

		 private bool IsRecordBigEnoughForFixedReferences( int recordSize )
		 {
			  return FixedFormatRecordSize <= recordSize;
		 }

		 private void ReadFixedReferencesRecord( PropertyRecord record, PageCursor cursor )
		 {
			  // since fixed reference limits property reference to 34 bits, 6 bytes is ample.
			  long prevMod = cursor.Short & 0xFFFFL;
			  long prevProp = cursor.Int & 0xFFFFFFFFL;
			  long nextMod = cursor.Short & 0xFFFFL;
			  long nextProp = cursor.Int & 0xFFFFFFFFL;
			  record.Initialize( true, BaseHighLimitRecordFormatV3_0_6.longFromIntAndMod( prevProp, prevMod << 32 ), BaseHighLimitRecordFormatV3_0_6.longFromIntAndMod( nextProp, nextMod << 32 ) );
			  // skip padding bytes
			  cursor.Offset = cursor.Offset + PROPERTY_BLOCKS_PADDING;
		 }

		 private void WriteFixedReferencesRecord( PropertyRecord record, PageCursor cursor )
		 {
			  // Set up the record header
			  short prevModifier = record.PrevProp == NULL ? 0 : ( short )( ( record.PrevProp & HIGH_DWORD_LOWER_WORD_MASK ) >> 32 );
			  short nextModifier = record.NextProp == NULL ? 0 : ( short )( ( record.NextProp & HIGH_DWORD_LOWER_WORD_MASK ) >> 32 );
			  cursor.PutShort( prevModifier );
			  cursor.PutInt( ( int ) record.PrevProp );
			  cursor.PutShort( nextModifier );
			  cursor.PutInt( ( int ) record.NextProp );
			  // skip bytes before start reading property blocks to have
			  // aligned access and fixed position of property blocks
			  cursor.Offset = cursor.Offset + PROPERTY_BLOCKS_PADDING;
		 }
	}

}