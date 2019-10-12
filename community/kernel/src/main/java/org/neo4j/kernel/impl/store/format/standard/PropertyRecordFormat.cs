/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store.format.standard
{
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Org.Neo4j.Kernel.impl.store.format;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

	public class PropertyRecordFormat : BaseRecordFormat<PropertyRecord>
	{
		 public const int DEFAULT_DATA_BLOCK_SIZE = 120;
		 public const int DEFAULT_PAYLOAD_SIZE = 32;

		 public static readonly int RecordSize = 1 + 4 + 4 + DEFAULT_PAYLOAD_SIZE;
		 // = 41

		 public PropertyRecordFormat() : base(FixedRecordSize(RecordSize), 0, StandardFormatSettings.PROPERTY_MAXIMUM_ID_BITS)
		 {
		 }

		 public override PropertyRecord NewRecord()
		 {
			  return new PropertyRecord( -1 );
		 }

		 public override void Read( PropertyRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  int offsetAtBeginning = cursor.Offset;

			  /*
			   * [pppp,nnnn] previous, next high bits
			   */
			  sbyte modifiers = cursor.Byte;
			  long prevMod = ( modifiers & 0xF0L ) << 28;
			  long nextMod = ( modifiers & 0x0FL ) << 32;
			  long prevProp = cursor.Int & 0xFFFFFFFFL;
			  long nextProp = cursor.Int & 0xFFFFFFFFL;
			  record.Initialize( false, BaseRecordFormat.longFromIntAndMod( prevProp, prevMod ), BaseRecordFormat.longFromIntAndMod( nextProp, nextMod ) );
			  while ( cursor.Offset - offsetAtBeginning < RecordSize )
			  {
					long block = cursor.Long;
					PropertyType type = PropertyType.getPropertyTypeOrNull( block );
					if ( type == null )
					{
						 // We assume that storage is defragged
						 break;
					}

					record.InUse = true;
					record.AddLoadedBlock( block );
					int numberOfBlocksUsed = type.calculateNumberOfBlocksUsed( block );
					if ( numberOfBlocksUsed == PropertyType.BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING )
					{
						 cursor.CursorException = "Invalid type or encoding of property block: " + block + " (type = " + type + ")";
						 return;
					}
					int additionalBlocks = numberOfBlocksUsed - 1;
					if ( additionalBlocks * Long.BYTES > RecordSize - ( cursor.Offset - offsetAtBeginning ) )
					{
						 cursor.CursorException = "PropertyRecord claims to have more property blocks than can fit in a record";
						 return;
					}
					while ( additionalBlocks-- > 0 )
					{
						 record.AddLoadedBlock( cursor.Long );
					}
			  }
		 }

		 public override void Write( PropertyRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					// Set up the record header
					short prevModifier = record.PrevProp == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (short)((record.PrevProp & 0xF00000000L) >> 28);
					short nextModifier = record.NextProp == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (short)((record.NextProp & 0xF00000000L) >> 32);
					sbyte modifiers = ( sbyte )( prevModifier | nextModifier );
					/*
					 * [pppp,nnnn] previous, next high bits
					 */
					cursor.PutByte( modifiers );
					cursor.PutInt( ( int ) record.PrevProp );
					cursor.PutInt( ( int ) record.NextProp );

					// Then go through the blocks
					int longsAppended = 0; // For marking the end of blocks
					foreach ( PropertyBlock block in record )
					{
						 long[] propBlockValues = block.ValueBlocks;
						 foreach ( long propBlockValue in propBlockValues )
						 {
							  cursor.PutLong( propBlockValue );
						 }

						 longsAppended += propBlockValues.Length;
					}
					if ( longsAppended < PropertyType.PayloadSizeLongs )
					{
						 cursor.PutLong( 0 );
					}
			  }
			  else
			  {
					// skip over the record header, nothing useful there
					cursor.Offset = cursor.Offset + 9;
					cursor.PutLong( 0 );
			  }
		 }

		 public override long GetNextRecordReference( PropertyRecord record )
		 {
			  return record.NextProp;
		 }

		 /// <summary>
		 /// For property records there's no "inUse" byte and we need to read the whole record to
		 /// see if there are any PropertyBlocks in use in it.
		 /// </summary>
		 public override bool IsInUse( PageCursor cursor )
		 {
			  cursor.Offset = cursor.Offset + 1 + 4 + 4;
			  int blocks = PropertyType.PayloadSizeLongs;
			  for ( int i = 0; i < blocks; i++ )
			  {
					long block = cursor.Long;
					// Since there's no inUse byte we have to check the special case of first block == 0, which will mean that it's deleted
					if ( i == 0 && block == 0 )
					{
						 return false;
					}
					if ( PropertyType.getPropertyTypeOrNull( block ) != null )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}