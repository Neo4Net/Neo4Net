/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using GeometryType = Neo4Net.Kernel.impl.store.GeometryType;
	using LongerShortString = Neo4Net.Kernel.impl.store.LongerShortString;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using ShortArray = Neo4Net.Kernel.impl.store.ShortArray;
	using TemporalType = Neo4Net.Kernel.impl.store.TemporalType;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using UTF8 = Neo4Net.Strings.UTF8;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using ByteValue = Neo4Net.Values.Storable.ByteValue;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using FloatValue = Neo4Net.Values.Storable.FloatValue;
	using IntValue = Neo4Net.Values.Storable.IntValue;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using ShortValue = Neo4Net.Values.Storable.ShortValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

	internal class RecordPropertyCursor : PropertyRecord, StoragePropertyCursor
	{
		 private const int MAX_BYTES_IN_SHORT_STRING_OR_SHORT_ARRAY = 32;
		 private const int INITIAL_POSITION = -1;

		 private readonly PropertyStore _read;
		 private long _next;
		 private int _block;
		 public ByteBuffer Buffer;
		 private PageCursor _page;
		 private PageCursor _stringPage;
		 private PageCursor _arrayPage;
		 private bool _open;

		 internal RecordPropertyCursor( PropertyStore read ) : base( NO_ID )
		 {
			  this._read = read;
		 }

		 public override void Init( long reference )
		 {
			  if ( Id != NO_ID )
			  {
					Clear();
			  }

			  //Set to high value to force a read
			  this._block = int.MaxValue;
			  if ( reference != NO_ID )
			  {
					if ( _page == null )
					{
						 _page = PropertyPage( reference );
					}
			  }

			  // Store state
			  this._next = reference;
			  this._open = true;
		 }

		 public override bool Next()
		 {
			  while ( true )
			  {
					//Figure out number of blocks of record
					int numberOfBlocks = NumberOfBlocks;
					while ( _block < numberOfBlocks )
					{
						 //We have just read a record, so we are at the beginning
						 if ( _block == INITIAL_POSITION )
						 {
							  _block = 0;
						 }
						 else
						 {
							  //Figure out the type and how many blocks that are used
							  long current = CurrentBlock();
							  PropertyType type = PropertyType.getPropertyTypeOrNull( current );
							  if ( type == null )
							  {
									break;
							  }
							  _block += type.calculateNumberOfBlocksUsed( current );
						 }
						 //nothing left, need to read a new record
						 if ( _block >= numberOfBlocks || Type() == null )
						 {
							  break;
						 }

						 return true;
					}

					if ( _next == NO_ID )
					{
						 return false;
					}

					Property( this, _next, _page );
					_next = NextProp;
					_block = INITIAL_POSITION;
			  }
		 }

		 private long CurrentBlock()
		 {
			  return Blocks[_block];
		 }

		 public override void Reset()
		 {
			  if ( _open )
			  {
					_open = false;
					Clear();
			  }
		 }

		 public override int PropertyKey()
		 {
			  return PropertyBlock.keyIndexId( CurrentBlock() );
		 }

		 public override ValueGroup PropertyType()
		 {
			  PropertyType type = type();
			  if ( type == null )
			  {
					return ValueGroup.NO_VALUE;
			  }
			  switch ( type.innerEnumValue )
			  {
			  case PropertyType.InnerEnum.BOOL:
					return ValueGroup.BOOLEAN;
			  case PropertyType.InnerEnum.BYTE:
			  case PropertyType.InnerEnum.SHORT:
			  case PropertyType.InnerEnum.INT:
			  case PropertyType.InnerEnum.LONG:
			  case PropertyType.InnerEnum.FLOAT:
			  case PropertyType.InnerEnum.DOUBLE:
					return ValueGroup.NUMBER;
			  case PropertyType.InnerEnum.STRING:
			  case PropertyType.InnerEnum.CHAR:
			  case PropertyType.InnerEnum.SHORT_STRING:
					return ValueGroup.TEXT;
			  case PropertyType.InnerEnum.TEMPORAL:
			  case PropertyType.InnerEnum.GEOMETRY:
			  case PropertyType.InnerEnum.SHORT_ARRAY:
			  case PropertyType.InnerEnum.ARRAY:
					// value read is needed to get correct value group since type is not fine grained enough to match all ValueGroups
					return PropertyValue().valueGroup();
			  default:
					throw new System.NotSupportedException( "not implemented" );
			  }
		 }

		 private PropertyType Type()
		 {
			  return PropertyType.getPropertyTypeOrNull( CurrentBlock() );
		 }

		 public override Value PropertyValue()
		 {
			  return ReadValue();
		 }

		 private Value ReadValue()
		 {
			  PropertyType type = type();
			  if ( type == null )
			  {
					return Values.NO_VALUE;
			  }
			  switch ( type.innerEnumValue )
			  {
			  case PropertyType.InnerEnum.BOOL:
					return ReadBoolean();
			  case PropertyType.InnerEnum.BYTE:
					return ReadByte();
			  case PropertyType.InnerEnum.SHORT:
					return ReadShort();
			  case PropertyType.InnerEnum.INT:
					return ReadInt();
			  case PropertyType.InnerEnum.LONG:
					return ReadLong();
			  case PropertyType.InnerEnum.FLOAT:
					return ReadFloat();
			  case PropertyType.InnerEnum.DOUBLE:
					return ReadDouble();
			  case PropertyType.InnerEnum.CHAR:
					return ReadChar();
			  case PropertyType.InnerEnum.SHORT_STRING:
					return ReadShortString();
			  case PropertyType.InnerEnum.SHORT_ARRAY:
					return ReadShortArray();
			  case PropertyType.InnerEnum.STRING:
					return ReadLongString();
			  case PropertyType.InnerEnum.ARRAY:
					return ReadLongArray();
			  case PropertyType.InnerEnum.GEOMETRY:
					return GeometryValue();
			  case PropertyType.InnerEnum.TEMPORAL:
					return TemporalValue();
			  default:
					throw new System.InvalidOperationException( "Unsupported PropertyType: " + type.name() );
			  }
		 }

		 private Value GeometryValue()
		 {
			  return GeometryType.decode( Blocks, _block );
		 }

		 private Value TemporalValue()
		 {
			  return TemporalType.decode( Blocks, _block );
		 }

		 private ArrayValue ReadLongArray()
		 {
			  long reference = PropertyBlock.fetchLong( CurrentBlock() );
			  if ( _arrayPage == null )
			  {
					_arrayPage = _arrayPage( reference );
			  }
			  return Array( this, reference, _arrayPage );
		 }

		 private TextValue ReadLongString()
		 {
			  long reference = PropertyBlock.fetchLong( CurrentBlock() );
			  if ( _stringPage == null )
			  {
					_stringPage = _stringPage( reference );
			  }
			  return String( this, reference, _stringPage );
		 }

		 private Value ReadShortArray()
		 {
			  Bits bits = Bits.bits( MAX_BYTES_IN_SHORT_STRING_OR_SHORT_ARRAY );
			  int blocksUsed = ShortArray.calculateNumberOfBlocksUsed( CurrentBlock() );
			  for ( int i = 0; i < blocksUsed; i++ )
			  {
					bits.put( Blocks[_block + i] );
			  }
			  return ShortArray.decode( bits );
		 }

		 private TextValue ReadShortString()
		 {
			  return LongerShortString.decode( Blocks, _block, LongerShortString.calculateNumberOfBlocksUsed( CurrentBlock() ) );
		 }

		 private TextValue ReadChar()
		 {
			  return Values.charValue( ( char ) PropertyBlock.fetchShort( CurrentBlock() ) );
		 }

		 private DoubleValue ReadDouble()
		 {
			  return Values.doubleValue( Double.longBitsToDouble( Blocks[_block + 1] ) );
		 }

		 private FloatValue ReadFloat()
		 {
			  return Values.floatValue( Float.intBitsToFloat( PropertyBlock.fetchInt( CurrentBlock() ) ) );
		 }

		 private LongValue ReadLong()
		 {
			  if ( PropertyBlock.valueIsInlined( CurrentBlock() ) )
			  {
					return Values.longValue( ( long )( ( ulong )PropertyBlock.fetchLong( CurrentBlock() ) >> 1 ) );
			  }
			  else
			  {
					return Values.longValue( Blocks[_block + 1] );
			  }
		 }

		 private IntValue ReadInt()
		 {
			  return Values.intValue( PropertyBlock.fetchInt( CurrentBlock() ) );
		 }

		 private ShortValue ReadShort()
		 {
			  return Values.shortValue( PropertyBlock.fetchShort( CurrentBlock() ) );
		 }

		 private ByteValue ReadByte()
		 {
			  return Values.byteValue( PropertyBlock.fetchByte( CurrentBlock() ) );
		 }

		 private BooleanValue ReadBoolean()
		 {
			  return Values.booleanValue( PropertyBlock.fetchByte( CurrentBlock() ) == 1 );
		 }

		 public override string ToString()
		 {
			  if ( !_open )
			  {
					return "PropertyCursor[closed state]";
			  }
			  else
			  {
					return "PropertyCursor[id=" + Id + ", open state with: block=" + _block + ", next=" + _next +
							 ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 public override void Close()
		 {
			  if ( _stringPage != null )
			  {
					_stringPage.close();
					_stringPage = null;
			  }
			  if ( _arrayPage != null )
			  {
					_arrayPage.close();
					_arrayPage = null;
			  }
			  if ( _page != null )
			  {
					_page.close();
					_page = null;
			  }
		 }

		 private PageCursor PropertyPage( long reference )
		 {
			  return _read.openPageCursorForReading( reference );
		 }

		 private PageCursor StringPage( long reference )
		 {
			  return _read.openStringPageCursor( reference );
		 }

		 private PageCursor ArrayPage( long reference )
		 {
			  return _read.openArrayPageCursor( reference );
		 }

		 private void Property( PropertyRecord record, long reference, PageCursor pageCursor )
		 {
			  // We need to load forcefully here since otherwise we can have inconsistent reads
			  // for properties across blocks, see org.Neo4Net.graphdb.ConsistentPropertyReadsIT
			  _read.getRecordByCursor( reference, record, RecordLoad.FORCE, pageCursor );
		 }

		 private TextValue String( RecordPropertyCursor cursor, long reference, PageCursor page )
		 {
			  ByteBuffer buffer = cursor.Buffer = _read.loadString( reference, cursor.Buffer, page );
			  buffer.flip();
			  return Values.stringValue( UTF8.decode( buffer.array(), 0, buffer.limit() ) );
		 }

		 private ArrayValue Array( RecordPropertyCursor cursor, long reference, PageCursor page )
		 {
			  ByteBuffer buffer = cursor.Buffer = _read.loadArray( reference, cursor.Buffer, page );
			  buffer.flip();
			  return PropertyStore.readArrayFromBuffer( buffer );
		 }
	}

}