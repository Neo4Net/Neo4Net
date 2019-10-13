using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Capability = Neo4Net.Kernel.impl.store.format.Capability;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using UnsupportedFormatCapabilityException = Neo4Net.Kernel.impl.store.format.UnsupportedFormatCapabilityException;
	using StandardFormatSettings = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using UTF8 = Neo4Net.@string.UTF8;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.getRightArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeaderFormat.NO_STORE_HEADER_FORMAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	/// <summary>
	/// Implementation of the property store. This implementation has two dynamic
	/// stores. One used to store keys and another for string property values.
	/// Primitives are directly stored in the PropertyStore using this format:
	/// <pre>
	///  0: high bits  ( 1 byte)
	///  1: next       ( 4 bytes)    where new property records are added
	///  5: prev       ( 4 bytes)    points to more PropertyRecords in this chain
	///  9: payload    (32 bytes - 4 x 8 byte blocks)
	/// </pre>
	/// <h2>high bits</h2>
	/// <pre>
	/// [    ,xxxx] high(next)
	/// [xxxx,    ] high(prev)
	/// </pre>
	/// <h2>block structure</h2>
	/// <pre>
	/// [][][][] [    ,xxxx] [    ,    ] [    ,    ] [    ,    ] type (0x0000_0000_0F00_0000)
	/// [][][][] [    ,    ] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] key  (0x0000_0000_00FF_FFFF)
	/// </pre>
	/// <h2>property types</h2>
	/// <pre>
	///  1: BOOL
	///  2: BYTE
	///  3: SHORT
	///  4: CHAR
	///  5: INT
	///  6: LONG
	///  7: FLOAT
	///  8: DOUBLE
	///  9: STRING REFERENCE
	/// 10: ARRAY  REFERENCE
	/// 11: SHORT STRING
	/// 12: SHORT ARRAY
	/// 13: GEOMETRY
	/// </pre>
	/// <h2>value formats</h2>
	/// <pre>
	/// BOOL:      [    ,    ] [    ,    ] [    ,    ] [    ,    ] [   x,type][K][K][K]           (0x0000_0000_1000_0000)
	/// BYTE:      [    ,    ] [    ,    ] [    ,    ] [    ,xxxx] [xxxx,type][K][K][K]    (>>28) (0x0000_000F_F000_0000)
	/// SHORT:     [    ,    ] [    ,    ] [    ,xxxx] [xxxx,xxxx] [xxxx,type][K][K][K]    (>>28) (0x0000_0FFF_F000_0000)
	/// CHAR:      [    ,    ] [    ,    ] [    ,xxxx] [xxxx,xxxx] [xxxx,type][K][K][K]    (>>28) (0x0000_0FFF_F000_0000)
	/// INT:       [    ,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,type][K][K][K]    (>>28) (0x0FFF_FFFF_F000_0000)
	/// LONG:      [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxx1,type][K][K][K] inline>>29(0xFFFF_FFFF_E000_0000)
	/// LONG:      [    ,    ] [    ,    ] [    ,    ] [    ,    ] [   0,type][K][K][K] value in next long block
	/// FLOAT:     [    ,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,type][K][K][K]    (>>28) (0x0FFF_FFFF_F000_0000)
	/// DOUBLE:    [    ,    ] [    ,    ] [    ,    ] [    ,    ] [    ,type][K][K][K] value in next long block
	/// REFERENCE: [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,type][K][K][K]    (>>28) (0xFFFF_FFFF_F000_0000)
	/// SHORT STR: [    ,    ] [    ,    ] [    ,    ] [    ,   x] [xxxx,type][K][K][K] encoding  (0x0000_0001_F000_0000)
	///            [    ,    ] [    ,    ] [    ,    ] [ xxx,xxx ] [    ,type][K][K][K] length    (0x0000_007E_0000_0000)
	///            [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [x   ,    ] payload(+ maybe in next block) (0xFFFF_FF80_0000_0000)
	///                                                            bits are densely packed, bytes torn across blocks
	/// SHORT ARR: [    ,    ] [    ,    ] [    ,    ] [    ,    ] [xxxx,type][K][K][K] data type (0x0000_0000_F000_0000)
	///            [    ,    ] [    ,    ] [    ,    ] [  xx,xxxx] [    ,type][K][K][K] length    (0x0000_003F_0000_0000)
	///            [    ,    ] [    ,    ] [    ,xxxx] [xx  ,    ] [    ,type][K][K][K] bits/item (0x0000_003F_0000_0000)
	///                                                                                 0 means 64, other values "normal"
	///            [xxxx,xxxx] [xxxx,xxxx] [xxxx,    ] [    ,    ] payload(+ maybe in next block) (0xFFFF_FF00_0000_0000)
	///                                                            bits are densely packed, bytes torn across blocks
	/// POINT:     [    ,    ] [    ,    ] [    ,    ] [    ,    ] [xxxx,type][K][K][K] geometry subtype
	///            [    ,    ] [    ,    ] [    ,    ] [    ,xxxx] [    ,type][K][K][K] dimension
	///            [    ,    ] [    ,    ] [    ,    ] [xxxx,    ] [    ,type][K][K][K] CRSTable
	///            [    ,    ] [xxxx,xxxx] [xxxx,xxxx] [    ,    ] [    ,type][K][K][K] CRS code
	///            [    ,   x] [    ,    ] [    ,    ] [    ,    ] [    ,type][K][K][K] Precision flag: 0=double, 1=float
	///            values in next dimension long blocks
	/// DATE:      [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxx1] [ 01 ,type][K][K][K] epochDay
	/// DATE:      [    ,    ] [    ,    ] [    ,    ] [    ,   0] [ 01 ,type][K][K][K] epochDay in next long block
	/// LOCALTIME: [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxx1] [ 02 ,type][K][K][K] nanoOfDay
	/// LOCALTIME: [    ,    ] [    ,    ] [    ,    ] [    ,   0] [ 02 ,type][K][K][K] nanoOfDay in next long block
	/// LOCALDTIME:[xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [ 03 ,type][K][K][K] nanoOfSecond
	///            epochSecond in next long block
	/// TIME:      [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [ 04 ,type][K][K][K] secondOffset (=ZoneOffset)
	///            nanoOfDay in next long block
	/// DATETIME:  [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxx1] [ 05 ,type][K][K][K] nanoOfSecond
	///            epochSecond in next long block
	///            secondOffset in next long block
	/// DATETIME:  [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxx0] [ 05 ,type][K][K][K] nanoOfSecond
	///            epochSecond in next long block
	///            timeZone number in next long block
	/// DURATION:  [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [xxxx,xxxx] [ 06 ,type][K][K][K] nanoOfSecond
	///            months in next long block
	///            days in next long block
	///            seconds in next long block
	/// </pre>
	/// </summary>
	public class PropertyStore : CommonAbstractStore<PropertyRecord, NoStoreHeader>
	{
		 public const string TYPE_DESCRIPTOR = "PropertyStore";

		 private readonly DynamicStringStore _stringStore;
		 private readonly PropertyKeyTokenStore _propertyKeyTokenStore;
		 private readonly DynamicArrayStore _arrayStore;

		 // In 3.4 we introduced capabilities to store points and temporal data types
		 // this variable here can be removed once the support for older store versions (that do not have these two
		 // capabilities) has ceased, the variable can be removed.
		 private readonly bool _allowStorePointsAndTemporal;

		 public PropertyStore( File file, File idFile, Config configuration, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, DynamicStringStore stringPropertyStore, PropertyKeyTokenStore propertyKeyTokenStore, DynamicArrayStore arrayPropertyStore, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, configuration, IdType.PROPERTY, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, recordFormats.Property(), NO_STORE_HEADER_FORMAT, recordFormats.StoreVersion(), openOptions )
		 {
			  this._stringStore = stringPropertyStore;
			  this._propertyKeyTokenStore = propertyKeyTokenStore;
			  this._arrayStore = arrayPropertyStore;
			  _allowStorePointsAndTemporal = recordFormats.HasCapability( Capability.POINT_PROPERTIES ) && recordFormats.HasCapability( Capability.TEMPORAL_PROPERTIES );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.PropertyRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, PropertyRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessProperty( this, record );
		 }

		 public virtual DynamicStringStore StringStore
		 {
			 get
			 {
				  return _stringStore;
			 }
		 }

		 public virtual DynamicArrayStore ArrayStore
		 {
			 get
			 {
				  return _arrayStore;
			 }
		 }

		 public virtual PropertyKeyTokenStore PropertyKeyTokenStore
		 {
			 get
			 {
				  return _propertyKeyTokenStore;
			 }
		 }

		 public override void UpdateRecord( PropertyRecord record )
		 {
			  UpdatePropertyBlocks( record );
			  base.UpdateRecord( record );
		 }

		 private void UpdatePropertyBlocks( PropertyRecord record )
		 {
			  if ( record.InUse() )
			  {
					// Go through the blocks
					foreach ( PropertyBlock block in record )
					{
						 /*
						  * For each block we need to update its dynamic record chain if
						  * it is just created. Deleted dynamic records are in the property
						  * record and dynamic records are never modified. Also, they are
						  * assigned as a whole, so just checking the first should be enough.
						  */
						 if ( !block.Light && block.ValueRecords[0].Created )
						 {
							  UpdateDynamicRecords( block.ValueRecords );
						 }
					}
			  }
			  UpdateDynamicRecords( record.DeletedRecords );
		 }

		 private void UpdateDynamicRecords( IList<DynamicRecord> records )
		 {
			  foreach ( DynamicRecord valueRecord in records )
			  {
					PropertyType recordType = valueRecord.getType();
					if ( recordType == PropertyType.String )
					{
						 _stringStore.updateRecord( valueRecord );
					}
					else if ( recordType == PropertyType.Array )
					{
						 _arrayStore.updateRecord( valueRecord );
					}
					else
					{
						 throw new InvalidRecordException( "Unknown dynamic record" + valueRecord );
					}
			  }
		 }

		 public override void EnsureHeavy( PropertyRecord record )
		 {
			  foreach ( PropertyBlock block in record )
			  {
					EnsureHeavy( block );
			  }
		 }

		 public virtual void EnsureHeavy( PropertyBlock block )
		 {
			  if ( !block.Light )
			  {
					return;
			  }

			  PropertyType type = block.Type;
			  RecordStore<DynamicRecord> dynamicStore = DynamicStoreForValueType( type );
			  if ( dynamicStore != null )
			  {
					IList<DynamicRecord> dynamicRecords = dynamicStore.GetRecords( block.SingleValueLong, NORMAL );
					foreach ( DynamicRecord dynamicRecord in dynamicRecords )
					{
						 dynamicRecord.SetType( type.intValue() );
					}
					block.ValueRecords = dynamicRecords;
			  }
		 }

		 private RecordStore<DynamicRecord> DynamicStoreForValueType( PropertyType type )
		 {
			  switch ( type.innerEnumValue )
			  {
			  case Neo4Net.Kernel.impl.store.PropertyType.InnerEnum.ARRAY:
				  return _arrayStore;
			  case Neo4Net.Kernel.impl.store.PropertyType.InnerEnum.STRING:
				  return _stringStore;
			  default:
				  return null;
			  }
		 }

		 public virtual Value GetValue( PropertyBlock propertyBlock )
		 {
			  return propertyBlock.Type.value( propertyBlock, this );
		 }

		 private static void AllocateStringRecords( ICollection<DynamicRecord> target, sbyte[] chars, DynamicRecordAllocator allocator )
		 {
			  AbstractDynamicStore.AllocateRecordsFromBytes( target, chars, allocator );
		 }

		 private static void AllocateArrayRecords( ICollection<DynamicRecord> target, object array, DynamicRecordAllocator allocator, bool allowStorePoints )
		 {
			  DynamicArrayStore.AllocateRecords( target, array, allocator, allowStorePoints );
		 }

		 public virtual void EncodeValue( PropertyBlock block, int keyId, Value value )
		 {
			  EncodeValue( block, keyId, value, _stringStore, _arrayStore, _allowStorePointsAndTemporal );
		 }

		 public static void EncodeValue( PropertyBlock block, int keyId, Value value, DynamicRecordAllocator stringAllocator, DynamicRecordAllocator arrayAllocator, bool allowStorePointsAndTemporal )
		 {
			  if ( value is ArrayValue )
			  {
					object asObject = value.AsObject();

					// Try short array first, i.e. inlined in the property block
					if ( ShortArray.encode( keyId, asObject, block, PropertyType.PayloadSize ) )
					{
						 return;
					}

					// Fall back to dynamic array store
					IList<DynamicRecord> arrayRecords = new List<DynamicRecord>();
					AllocateArrayRecords( arrayRecords, asObject, arrayAllocator, allowStorePointsAndTemporal );
					SetSingleBlockValue( block, keyId, PropertyType.Array, Iterables.first( arrayRecords ).Id );
					foreach ( DynamicRecord valueRecord in arrayRecords )
					{
						 valueRecord.SetType( PropertyType.Array.intValue() );
					}
					block.ValueRecords = arrayRecords;
			  }
			  else
			  {
					value.WriteTo( new PropertyBlockValueWriter( block, keyId, stringAllocator, allowStorePointsAndTemporal ) );
			  }
		 }

		 public virtual PageCursor OpenStringPageCursor( long reference )
		 {
			  return _stringStore.openPageCursorForReading( reference );
		 }

		 public virtual PageCursor OpenArrayPageCursor( long reference )
		 {
			  return _arrayStore.openPageCursorForReading( reference );
		 }

		 public virtual ByteBuffer LoadString( long reference, ByteBuffer buffer, PageCursor page )
		 {
			  return ReadDynamic( _stringStore, reference, buffer, page );
		 }

		 public virtual ByteBuffer LoadArray( long reference, ByteBuffer buffer, PageCursor page )
		 {
			  return ReadDynamic( _arrayStore, reference, buffer, page );
		 }

		 private static ByteBuffer ReadDynamic( AbstractDynamicStore store, long reference, ByteBuffer buffer, PageCursor page )
		 {
			  if ( buffer == null )
			  {
					buffer = ByteBuffer.allocate( 512 );
			  }
			  else
			  {
					buffer.clear();
			  }
			  DynamicRecord record = store.NewRecord();
			  do
			  {
					//We need to load forcefully here since otherwise we can have inconsistent reads
					//for properties across blocks, see org.neo4j.graphdb.ConsistentPropertyReadsIT
					store.GetRecordByCursor( reference, record, RecordLoad.FORCE, page );
					reference = record.NextBlock;
					sbyte[] data = record.Data;
					if ( buffer.remaining() < data.Length )
					{
						 buffer = Grow( buffer, data.Length );
					}
					buffer.put( data, 0, data.Length );
			  } while ( reference != NO_ID );
			  return buffer;
		 }

		 private static ByteBuffer Grow( ByteBuffer buffer, int required )
		 {
			  buffer.flip();
			  int capacity = buffer.capacity();
			  do
			  {
					capacity *= 2;
			  } while ( capacity - buffer.limit() < required );
			  return ByteBuffer.allocate( capacity ).order( ByteOrder.LITTLE_ENDIAN ).put( buffer );
		 }

		 private class PropertyBlockValueWriter : TemporalValueWriterAdapter<System.ArgumentException>
		 {

			  internal readonly PropertyBlock Block;
			  internal readonly int KeyId;
			  internal readonly DynamicRecordAllocator StringAllocator;
			  internal readonly bool AllowStorePointsAndTemporal;
			  internal PropertyBlockValueWriter( PropertyBlock block, int keyId, DynamicRecordAllocator stringAllocator, bool allowStorePointsAndTemporal )
			  {
					this.Block = block;
					this.KeyId = keyId;
					this.StringAllocator = stringAllocator;
					this.AllowStorePointsAndTemporal = allowStorePointsAndTemporal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNull() throws IllegalArgumentException
			  public override void WriteNull()
			  {
					throw new System.ArgumentException( "Cannot write null values to the property store" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolean(boolean value) throws IllegalArgumentException
			  public override void WriteBoolean( bool value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Bool, value ? 1L : 0L );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(byte value) throws IllegalArgumentException
			  public override void WriteInteger( sbyte value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Byte, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(short value) throws IllegalArgumentException
			  public override void WriteInteger( short value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Short, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(int value) throws IllegalArgumentException
			  public override void WriteInteger( int value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Int, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(long value) throws IllegalArgumentException
			  public override void WriteInteger( long value )
			  {
					long keyAndType = KeyId | ( ( ( long ) PropertyType.Long.intValue() ) << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS );
					if ( ShortArray.Long.getRequiredBits( value ) <= 35 )
					{ // We only need one block for this value, special layout compared to, say, an integer
						 Block.SingleBlock = keyAndType | ( 1L << 28 ) | ( value << 29 );
					}
					else
					{ // We need two blocks for this value
						 Block.ValueBlocks = new long[]{ keyAndType, value };
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(float value) throws IllegalArgumentException
			  public override void WriteFloatingPoint( float value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Float, Float.floatToRawIntBits( value ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(double value) throws IllegalArgumentException
			  public override void WriteFloatingPoint( double value )
			  {
					Block.ValueBlocks = new long[]{ KeyId | ( ( ( long ) PropertyType.Double.intValue() ) << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS ), Double.doubleToRawLongBits(value) };
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(String value) throws IllegalArgumentException
			  public override void WriteString( string value )
			  {
					// Try short string first, i.e. inlined in the property block
					if ( LongerShortString.encode( KeyId, value, Block, PropertyType.PayloadSize ) )
					{
						 return;
					}

					// Fall back to dynamic string store
					sbyte[] encodedString = EncodeString( value );
					IList<DynamicRecord> valueRecords = new List<DynamicRecord>();
					AllocateStringRecords( valueRecords, encodedString, StringAllocator );
					SetSingleBlockValue( Block, KeyId, PropertyType.String, Iterables.first( valueRecords ).Id );
					foreach ( DynamicRecord valueRecord in valueRecords )
					{
						 valueRecord.SetType( PropertyType.String.intValue() );
					}
					Block.ValueRecords = valueRecords;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(char value) throws IllegalArgumentException
			  public override void WriteString( char value )
			  {
					SetSingleBlockValue( Block, KeyId, PropertyType.Char, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginArray(int size, ArrayType arrayType) throws IllegalArgumentException
			  public override void BeginArray( int size, ArrayType arrayType )
			  {
					throw new System.ArgumentException( "Cannot persist arrays to property store using ValueWriter" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endArray() throws IllegalArgumentException
			  public override void EndArray()
			  {
					throw new System.ArgumentException( "Cannot persist arrays to property store using ValueWriter" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws IllegalArgumentException
			  public override void WriteByteArray( sbyte[] value )
			  {
					throw new System.ArgumentException( "Cannot persist arrays to property store using ValueWriter" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(org.neo4j.values.storable.CoordinateReferenceSystem crs, double[] coordinate) throws IllegalArgumentException
			  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = GeometryType.encodePoint( KeyId, crs, coordinate );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.POINT_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws IllegalArgumentException
			  public override void WriteDuration( long months, long days, long seconds, int nanos )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeDuration( KeyId, months, days, seconds, nanos );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(long epochDay) throws IllegalArgumentException
			  public override void WriteDate( long epochDay )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeDate( KeyId, epochDay );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(long nanoOfDay) throws IllegalArgumentException
			  public override void WriteLocalTime( long nanoOfDay )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeLocalTime( KeyId, nanoOfDay );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(long nanosOfDayUTC, int offsetSeconds) throws IllegalArgumentException
			  public override void WriteTime( long nanosOfDayUTC, int offsetSeconds )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeTime( KeyId, nanosOfDayUTC, offsetSeconds );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(long epochSecond, int nano) throws IllegalArgumentException
			  public override void WriteLocalDateTime( long epochSecond, int nano )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeLocalDateTime( KeyId, epochSecond, nano );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(long epochSecondUTC, int nano, int offsetSeconds) throws IllegalArgumentException
			  public override void WriteDateTime( long epochSecondUTC, int nano, int offsetSeconds )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeDateTime( KeyId, epochSecondUTC, nano, offsetSeconds );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(long epochSecondUTC, int nano, String zoneId) throws IllegalArgumentException
			  public override void WriteDateTime( long epochSecondUTC, int nano, string zoneId )
			  {
					if ( AllowStorePointsAndTemporal )
					{
						 Block.ValueBlocks = TemporalType.encodeDateTime( KeyId, epochSecondUTC, nano, zoneId );
					}
					else
					{
						 throw new UnsupportedFormatCapabilityException( Capability.TEMPORAL_PROPERTIES );
					}
			  }

		 }
		 public static void SetSingleBlockValue( PropertyBlock block, int keyId, PropertyType type, long longValue )
		 {
			  block.SingleBlock = SingleBlockLongValue( keyId, type, longValue );
		 }

		 public static long SingleBlockLongValue( int keyId, PropertyType type, long longValue )
		 {
			  return keyId | ( ( ( long ) type.intValue() ) << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS ) | (longValue << 28);
		 }

		 public static sbyte[] EncodeString( string @string )
		 {
			  return UTF8.encode( @string );
		 }

		 public static string DecodeString( sbyte[] byteArray )
		 {
			  return UTF8.decode( byteArray );
		 }

		 internal virtual string GetStringFor( PropertyBlock propertyBlock )
		 {
			  EnsureHeavy( propertyBlock );
			  return GetStringFor( propertyBlock.ValueRecords );
		 }

		 private string GetStringFor( ICollection<DynamicRecord> dynamicRecords )
		 {
			  Pair<sbyte[], sbyte[]> source = _stringStore.readFullByteArray( dynamicRecords, PropertyType.String );
			  // A string doesn't have a header in the data array
			  return DecodeString( source.Other() );
		 }

		 internal virtual Value GetArrayFor( PropertyBlock propertyBlock )
		 {
			  EnsureHeavy( propertyBlock );
			  return GetArrayFor( propertyBlock.ValueRecords );
		 }

		 private Value GetArrayFor( IEnumerable<DynamicRecord> records )
		 {
			  return getRightArray( _arrayStore.readFullByteArray( records, PropertyType.Array ) );
		 }

		 public override string ToString()
		 {
			  return base.ToString() + "[blocksPerRecord:" + PropertyType.PayloadSizeLongs + "]";
		 }

		 public virtual ICollection<PropertyRecord> GetPropertyRecordChain( long firstRecordId )
		 {
			  long nextProp = firstRecordId;
			  IList<PropertyRecord> toReturn = new LinkedList<PropertyRecord>();
			  while ( nextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					PropertyRecord propRecord = new PropertyRecord( nextProp );
					GetRecord( nextProp, propRecord, RecordLoad.NORMAL );
					toReturn.Add( propRecord );
					nextProp = propRecord.NextProp;
			  }
			  return toReturn;
		 }

		 public override PropertyRecord NewRecord()
		 {
			  return new PropertyRecord( -1 );
		 }

		 public virtual bool AllowStorePointsAndTemporal()
		 {
			  return _allowStorePointsAndTemporal;
		 }

		 /// <returns> a calculator of property value sizes. The returned instance is designed to be used multiple times by a single thread only. </returns>
		 public virtual System.Func<Value[], int> NewValueEncodedSizeCalculator()
		 {
			  return new PropertyValueRecordSizeCalculator( this );
		 }

		 public static ArrayValue ReadArrayFromBuffer( ByteBuffer buffer )
		 {
			  if ( buffer.limit() <= 0 )
			  {
					throw new System.InvalidOperationException( "Given buffer is empty" );
			  }

			  sbyte typeId = buffer.get();
			  buffer.order( ByteOrder.BIG_ENDIAN );
			  try
			  {
					if ( typeId == PropertyType.String.intValue() )
					{
						 int arrayLength = buffer.Int;
						 string[] result = new string[arrayLength];

						 for ( int i = 0; i < arrayLength; i++ )
						 {
							  int byteLength = buffer.Int;
							  result[i] = UTF8.decode( buffer.array(), buffer.position(), byteLength );
							  buffer.position( buffer.position() + byteLength );
						 }
						 return Values.stringArray( result );
					}
					else if ( typeId == PropertyType.Geometry.intValue() )
					{
						 GeometryType.GeometryHeader header = GeometryType.GeometryHeader.fromArrayHeaderByteBuffer( buffer );
						 sbyte[] byteArray = new sbyte[buffer.limit() - buffer.position()];
						 buffer.get( byteArray );
						 return GeometryType.decodeGeometryArray( header, byteArray );
					}
					else if ( typeId == PropertyType.Temporal.intValue() )
					{
						 TemporalType.TemporalHeader header = TemporalType.TemporalHeader.fromArrayHeaderByteBuffer( buffer );
						 sbyte[] byteArray = new sbyte[buffer.limit() - buffer.position()];
						 buffer.get( byteArray );
						 return TemporalType.decodeTemporalArray( header, byteArray );
					}
					else
					{
						 ShortArray type = ShortArray.typeOf( typeId );
						 int bitsUsedInLastByte = buffer.get();
						 int requiredBits = buffer.get();
						 if ( requiredBits == 0 )
						 {
							  return type.createEmptyArray();
						 }
						 if ( type == ShortArray.Byte && requiredBits == ( sizeof( sbyte ) * 8 ) )
						 { // Optimization for byte arrays (probably large ones)
							  sbyte[] byteArray = new sbyte[buffer.limit() - buffer.position()];
							  buffer.get( byteArray );
							  return Values.byteArray( byteArray );
						 }
						 else
						 { // Fallback to the generic approach, which is a slower
							  Bits bits = Bits.bitsFromBytes( buffer.array(), buffer.position() );
							  int length = ( ( buffer.limit() - buffer.position() ) * 8 - (8 - bitsUsedInLastByte) ) / requiredBits;
							  return type.createArray( length, bits, requiredBits );
						 }
					}
			  }
			  finally
			  {
					buffer.order( ByteOrder.LITTLE_ENDIAN );
			  }
		 }
	}

}