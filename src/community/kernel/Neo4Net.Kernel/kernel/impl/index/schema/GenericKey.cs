using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using UTF8 = Neo4Net.Strings.UTF8;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PrimitiveArrayWriting = Neo4Net.Values.Storable.PrimitiveArrayWriting;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PageCache_Fields.PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.DurationIndexKey.AVG_DAY_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.DurationIndexKey.AVG_MONTH_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.LOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.Type.booleanOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.Types.HIGHEST_BY_VALUE_GROUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.Types.LOWEST_BY_VALUE_GROUP;

	/// <summary>
	/// A key instance which can handle all types of single values, i.e. not composite keys, but all value types.
	/// See <seealso cref="CompositeGenericKey"/> for implementation which supports composite keys.
	/// 
	/// Regarding why the "internal" versions of some methods which are overridden by the CompositeGenericKey sub-class. Example:
	/// - Consider a method a() which is used by some part of the implementation of the generic index provider.
	/// - Sometimes the instance the method is called on will be a CompositeGenericKey.
	/// - CompositeGenericKey overrides a() to loop over multiple state slots. Each slot is a GenericKey too.
	/// - Simply overriding a() and call slot[i].a() would result in StackOverflowError since it would be calling itself.
	/// This is why aInternal() exists and GenericKey#a() is implemented by simply forwarding to aInternal().
	/// CompositeGenericKey#a() is implemented by looping over multiple GenericKey instances, also calling aInternal() in each of those, instead of a().
	/// </summary>
	public class GenericKey : NativeIndexKey<GenericKey>
	{
		 /// <summary>
		 /// This is the biggest size a static (as in non-dynamic, like string), non-array value can have.
		 /// </summary>
		 internal static readonly int BiggestStaticSize = Long.BYTES * 4; // long0, long1, long2, long3

		 // TODO copy-pasted from individual keys
		 // TODO also put this in Type enum
		 public const int SIZE_GEOMETRY_HEADER = 3; // 2b tableId and 22b code
		 public static readonly int SizeGeometry = Long.BYTES; // rawValueBits
		 internal static readonly int SizeGeometryCoordinate = Long.BYTES; // one coordinate
		 public static readonly int SizeZonedDateTime = Long.BYTES + Integer.BYTES + Integer.BYTES; // timeZone
		 public static readonly int SizeLocalDateTime = Long.BYTES + Integer.BYTES; // nanoOfSecond
		 public static readonly int SizeDate = Long.BYTES; // epochDay
		 public static readonly int SizeZonedTime = Long.BYTES + Integer.BYTES; // zoneOffsetSeconds
		 public static readonly int SizeLocalTime = Long.BYTES; // nanoOfDay
		 public static readonly int SizeDuration = Long.BYTES + Integer.BYTES + Long.BYTES + Long.BYTES; // days
		 public static readonly int SizeStringLength = Short.BYTES; // length of string byte array
		 public static readonly int SizeBoolean = Byte.BYTES; // byte for this boolean value
		 public static readonly int SizeNumberType = Byte.BYTES; // type of value
		 public static readonly int SizeNumberByte = Byte.BYTES; // raw value bits
		 public static readonly int SizeNumberShort = Short.BYTES; // raw value bits
		 public static readonly int SizeNumberInt = Integer.BYTES; // raw value bits
		 public static readonly int SizeNumberLong = Long.BYTES; // raw value bits
		 public static readonly int SizeNumberFloat = Integer.BYTES; // raw value bits
		 public static readonly int SizeNumberDouble = Long.BYTES; // raw value bits
		 public static readonly int SizeArrayLength = Short.BYTES;
		 internal static readonly int BiggestReasonableArrayLength = PAGE_SIZE / 2 / SizeNumberByte;

		 internal const long TRUE = 1;
		 internal const long FALSE = 0;
		 internal const int NO_ENTITY_ID = -1;
		 private static readonly int _typeIdSize = Byte.BYTES;
		 private static readonly double[] _noCoordinates = new double[0];

		 // Immutable
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _settings;

		 // Mutable, meta-state
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal Type TypeConflict;
		 internal NativeIndexKey.Inclusion Inclusion;
		 internal bool IsArray;

		 // Mutable, non-array values
		 internal long Long0;
		 internal long Long1;
		 internal long Long2;
		 internal long Long3;
		 internal sbyte[] ByteArray;

		 // Mutable, array values
		 internal long[] Long0Array;
		 internal long[] Long1Array;
		 internal long[] Long2Array;
		 internal long[] Long3Array;
		 internal sbyte[][] ByteArrayArray;
		 internal bool IsHighestArray;
		 internal int ArrayLength;
		 internal int CurrentArrayOffset;

		 // Mutable, spatial values
		 /*
		  * Settings for a specific crs retrieved from #settings using #long1 and #long2.
		  */
		 internal SpaceFillingCurve SpaceFillingCurve;

		 internal GenericKey( IndexSpecificSpaceFillingCurveSettingsCache settings )
		 {
			  this._settings = settings;
		 }

		 /* <initializers> */
		 internal virtual void Clear()
		 {
			  if ( TypeConflict == Types.Text && booleanOf( Long1 ) )
			  {
					// Clear byteArray if it has been dereferenced
					ByteArray = null;
			  }
			  TypeConflict = null;
			  Long0 = 0;
			  Long1 = 0;
			  Long2 = 0;
			  Long3 = 0;
			  Inclusion = NEUTRAL;
			  IsArray = false;
			  ArrayLength = 0;
			  IsHighestArray = false;
			  CurrentArrayOffset = 0;
			  SpaceFillingCurve = null;
		 }

		 internal virtual void InitializeToDummyValue()
		 {
			  IEntityId = long.MinValue;
			  InitializeToDummyValueInternal();
		 }

		 internal virtual void InitializeToDummyValueInternal()
		 {
			  Clear();
			  WriteInteger( 0 );
			  Inclusion = NEUTRAL;
		 }

		 internal virtual void InitValueAsLowest( ValueGroup valueGroup )
		 {
			  Clear();
			  TypeConflict = valueGroup == ValueGroup.UNKNOWN ? LOWEST_BY_VALUE_GROUP : Types.ByGroup[valueGroup.ordinal()];
			  TypeConflict.initializeAsLowest( this );
		 }

		 internal virtual void InitValueAsHighest( ValueGroup valueGroup )
		 {
			  Clear();
			  TypeConflict = valueGroup == ValueGroup.UNKNOWN ? HIGHEST_BY_VALUE_GROUP : Types.ByGroup[valueGroup.ordinal()];
			  TypeConflict.initializeAsHighest( this );
		 }

		 internal virtual void InitAsPrefixLow( TextValue prefix )
		 {
			  prefix.WriteTo( this );
			  Long2 = FALSE;
			  Inclusion = LOW;
			  // Don't set ignoreLength = true here since the "low" a.k.a. left side of the range should care about length.
			  // This will make the prefix lower than those that matches the prefix (their length is >= that of the prefix)
		 }

		 internal virtual void InitAsPrefixHigh( TextValue prefix )
		 {
			  prefix.WriteTo( this );
			  Long2 = TRUE; // ignoreLength
			  Inclusion = HIGH;
		 }

		 /* </initializers> */
		 internal virtual void CopyFrom( GenericKey key )
		 {
			  IEntityId = key.EntityId;
			  CompareId = key.CompareId;
			  CopyFromInternal( key );
		 }

		 internal virtual void CopyFromInternal( GenericKey key )
		 {
			  CopyMetaFrom( key );
			  TypeConflict.copyValue( this, key );
		 }

		 internal virtual void CopyMetaFrom( GenericKey key )
		 {
			  this.TypeConflict = key.TypeConflict;
			  this.Inclusion = key.Inclusion;
			  this.IsArray = key.IsArray;
			  if ( key.IsArray )
			  {
					this.ArrayLength = key.ArrayLength;
					this.CurrentArrayOffset = key.CurrentArrayOffset;
					this.IsHighestArray = key.IsHighestArray;
			  }
		 }

		 internal virtual void WriteValue( Value value, NativeIndexKey.Inclusion inclusion )
		 {
			  IsArray = false;
			  value.WriteTo( this );
			  this.Inclusion = inclusion;
		 }

		 internal override void WriteValue( int stateSlot, Value value, Inclusion inclusion )
		 {
			  WriteValue( value, inclusion );
		 }

		 internal override void AssertValidValue( int stateSlot, Value value )
		 {
			  // No need, we can handle all values
		 }

		 internal override Value[] AsValues()
		 {
			  return new Value[] { AsValue() };
		 }

		 internal override void InitValueAsLowest( int stateSlot, ValueGroup valueGroup )
		 {
			  InitValueAsLowest( valueGroup );
		 }

		 internal override void InitValueAsHighest( int stateSlot, ValueGroup valueGroup )
		 {
			  InitValueAsHighest( valueGroup );
		 }

		 internal virtual GenericKey StateSlot( int slot )
		 {
			  Debug.Assert( slot == 0 );
			  return this;
		 }

		 internal override int NumberOfStateSlots()
		 {
			  return 1;
		 }

		 internal override int CompareValueTo( GenericKey other )
		 {
			  return CompareValueToInternal( other );
		 }

		 internal virtual int CompareValueToInternal( GenericKey other )
		 {
			  if ( TypeConflict != other.TypeConflict )
			  {
					// These null checks guard for inconsistent reading where we're expecting a retry to occur
					// Unfortunately it's the case that SeekCursor calls these methods inside a shouldRetry.
					// Fortunately we only need to do these checks if the types aren't equal, and one of the two
					// are guaranteed to be a "real" state, i.e. not inside a shouldRetry.
					if ( TypeConflict == null )
					{
						 return -1;
					}
					if ( other.TypeConflict == null )
					{
						 return 1;
					}
					return Type.Comparator.Compare( TypeConflict, other.TypeConflict );
			  }

			  int valueComparison = TypeConflict.compareValue( this, other );
			  if ( valueComparison != 0 )
			  {
					return valueComparison;
			  }

			  return Inclusion.compareTo( other.Inclusion );
		 }

		 internal virtual void MinimalSplitter( GenericKey left, GenericKey right, GenericKey into )
		 {
			  into.CompareId = right.CompareId;
			  if ( left.CompareValueTo( right ) != 0 )
			  {
					into.EntityId = NO_ENTITY_ID;
			  }
			  else
			  {
					// There was no minimal splitter to be found so IEntity id will serve as divider
					into.EntityId = right.EntityId;
			  }
			  MinimalSplitterInternal( left, right, into );
		 }

		 internal virtual void MinimalSplitterInternal( GenericKey left, GenericKey right, GenericKey into )
		 {
			  into.Clear();
			  into.CopyMetaFrom( right );
			  right.TypeConflict.minimalSplitter( left, right, into );
		 }

		 internal virtual int Size()
		 {
			  return IEntityIdSize + SizeInternal();
		 }

		 internal virtual int SizeInternal()
		 {
			  return TypeConflict.valueSize( this ) + _typeIdSize;
		 }

		 internal virtual Value AsValue()
		 {
			  return TypeConflict.asValue( this );
		 }

		 internal virtual void Put( PageCursor cursor )
		 {
			  cursor.PutLong( IEntityId );
			  PutInternal( cursor );
		 }

		 internal virtual void PutInternal( PageCursor cursor )
		 {
			  cursor.PutByte( TypeConflict.typeId );
			  TypeConflict.putValue( cursor, this );
		 }

		 internal virtual bool Get( PageCursor cursor, int size )
		 {
			  if ( size < IEntityIdSize )
			  {
					InitializeToDummyValue();
					cursor.CursorException = format( "Failed to read " + this.GetType().Name + " due to keySize < IEntity_ID_SIZE, more precisely %d", size );
					return false;
			  }

			  Initialize( cursor.Long );
			  if ( !GetInternal( cursor, size ) )
			  {
					InitializeToDummyValue();
					return false;
			  }
			  return true;
		 }

		 internal virtual bool GetInternal( PageCursor cursor, int size )
		 {
			  if ( size <= _typeIdSize )
			  {
					SetCursorException( cursor, "slot size less than TYPE_ID_SIZE, " + size );
					return false;
			  }

			  sbyte typeId = cursor.Byte;
			  if ( typeId < 0 || typeId >= Types.ById.Length )
			  {
					SetCursorException( cursor, "non-valid typeId, " + typeId );
					return false;
			  }

			  Inclusion = NEUTRAL;
			  return setType( Types.ById[typeId] ).ReadValue( cursor, size - _typeIdSize, this );
		 }

		 /* <write> (write to field state from Value or cursor) */

		 private T setType<T>( T type ) where T : Type
		 {
			  if ( this.TypeConflict != null && type != this.TypeConflict )
			  {
					Clear();
			  }
			  this.TypeConflict = type;
			  return type;
		 }

		 protected internal override void WriteDate( long epochDay )
		 {
			  if ( !IsArray )
			  {
					setType( Types.Date ).write( this, epochDay );
			  }
			  else
			  {
					Types.DateArray.write( this, CurrentArrayOffset++, epochDay );
			  }
		 }

		 protected internal override void WriteLocalTime( long nanoOfDay )
		 {
			  if ( !IsArray )
			  {
					setType( Types.LocalTime ).write( this, nanoOfDay );
			  }
			  else
			  {
					Types.LocalTimeArray.write( this, CurrentArrayOffset++, nanoOfDay );
			  }
		 }

		 protected internal override void WriteTime( long nanosOfDayUTC, int offsetSeconds )
		 {
			  if ( !IsArray )
			  {
					setType( Types.ZonedTime ).write( this, nanosOfDayUTC, offsetSeconds );
			  }
			  else
			  {
					Types.ZonedTimeArray.write( this, CurrentArrayOffset++, nanosOfDayUTC, offsetSeconds );
			  }
		 }

		 protected internal override void WriteLocalDateTime( long epochSecond, int nano )
		 {
			  if ( !IsArray )
			  {
					setType( Types.LocalDateTime ).write( this, epochSecond, nano );
			  }
			  else
			  {
					Types.LocalDateTimeArray.write( this, CurrentArrayOffset++, epochSecond, nano );
			  }
		 }

		 protected internal override void WriteDateTime( long epochSecondUTC, int nano, int offsetSeconds )
		 {
			  WriteDateTime( epochSecondUTC, nano, ( short ) - 1, offsetSeconds );
		 }

		 protected internal override void WriteDateTime( long epochSecondUTC, int nano, string zoneId )
		 {
			  WriteDateTime( epochSecondUTC, nano, TimeZones.map( zoneId ) );
		 }

		 protected internal virtual void WriteDateTime( long epochSecondUTC, int nano, short zoneId )
		 {
			  WriteDateTime( epochSecondUTC, nano, zoneId, 0 );
		 }

		 private void WriteDateTime( long epochSecondUTC, int nano, short zoneId, int offsetSeconds )
		 {
			  if ( !IsArray )
			  {
					setType( Types.ZonedDateTime ).write( this, epochSecondUTC, nano, zoneId, offsetSeconds );
			  }
			  else
			  {
					Types.ZonedDateTimeArray.write( this, CurrentArrayOffset++, epochSecondUTC, nano, zoneId, offsetSeconds );
			  }
		 }

		 public override void WriteBoolean( bool value )
		 {
			  if ( !IsArray )
			  {
					setType( Types.Boolean ).write( this, value );
			  }
			  else
			  {
					Types.BooleanArray.write( this, CurrentArrayOffset++, value );
			  }
		 }

		 private void WriteNumber( long value, sbyte numberType )
		 {
			  if ( !IsArray )
			  {
					setType( Types.Number ).write( this, value, numberType );
			  }
			  else
			  {
					Types.NumberArray.write( this, CurrentArrayOffset++, value );
			  }
		 }

		 public override void WriteInteger( sbyte value )
		 {
			  WriteNumber( value, RawBits.BYTE );
		 }

		 public override void WriteInteger( short value )
		 {
			  WriteNumber( value, RawBits.SHORT );
		 }

		 public override void WriteInteger( int value )
		 {
			  WriteNumber( value, RawBits.INT );
		 }

		 internal static short ToNonNegativeShortExact( long value )
		 {
			  if ( ( value & ~0x7FFF ) != 0 )
			  {
					throw new System.ArgumentException( value + " is bigger than maximum for a signed short (2B) " + 0x7FFF );
			  }
			  return ( short ) value;
		 }

		 public override void WriteInteger( long value )
		 {
			  WriteNumber( value, RawBits.LONG );
		 }

		 public override void WriteFloatingPoint( float value )
		 {
			  WriteNumber( Float.floatToIntBits( value ), RawBits.FLOAT );
		 }

		 public override void WriteFloatingPoint( double value )
		 {
			  WriteNumber( System.BitConverter.DoubleToInt64Bits( value ), RawBits.DOUBLE );
		 }

		 public override void WriteString( string value )
		 {
			  WriteStringBytes( UTF8.encode( value ), false );
		 }

		 public override void WriteString( char value )
		 {
			  WriteStringBytes( UTF8.encode( value.ToString() ), true );
		 }

		 public override void WriteUTF8( sbyte[] bytes, int offset, int length )
		 {
			  sbyte[] dest = new sbyte[length];
			  Array.Copy( bytes, offset, dest, 0, length );
			  WriteStringBytes( dest, false );
		 }

		 private void WriteStringBytes( sbyte[] bytes, bool isCharType )
		 {
			  if ( !IsArray )
			  {
					setType( Types.Text ).write( this, bytes, isCharType );
			  }
			  else
			  {
					// in the array case we've already noted the char/string type in beginArray
					Types.TextArray.write( this, CurrentArrayOffset++, bytes );
			  }
			  Long1 = FALSE; // long1 is dereferenced true/false
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  long totalAvgSeconds = months * AVG_MONTH_SECONDS + days * AVG_DAY_SECONDS + seconds;
			  WriteDurationWithTotalAvgSeconds( months, days, totalAvgSeconds, nanos );
		 }

		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  if ( !IsArray )
			  {
					Type = Types.Geometry;
					UpdateCurve( crs.Table.TableId, crs.Code );
					Types.Geometry.write( this, SpaceFillingCurve.derivedValueFor( coordinate ).Value, coordinate );
			  }
			  else
			  {
					if ( CurrentArrayOffset == 0 )
					{
						 UpdateCurve( crs.Table.TableId, crs.Code );
					}
					else if ( this.Long1 != crs.Table.TableId || this.Long2 != crs.Code )
					{
						 throw new System.InvalidOperationException( format( "Tried to assign a geometry array containing different coordinate reference systems, first:%s, violating:%s at array position:%d", CoordinateReferenceSystem.get( ( int ) Long1, ( int ) Long2 ), crs, CurrentArrayOffset ) );
					}
					Types.GeometryArray.write( this, CurrentArrayOffset++, SpaceFillingCurve.derivedValueFor( coordinate ).Value, coordinate );
			  }
		 }

		 internal virtual void WritePointDerived( CoordinateReferenceSystem crs, long derivedValue, NativeIndexKey.Inclusion inclusion )
		 {
			  if ( IsArray )
			  {
					throw new System.InvalidOperationException( "This method is intended to be called when querying, where one or more sub-ranges are derived " + "from a queried range and each sub-range written to separate keys. " + "As such it's unexpected that this key state thinks that it's holds state for an array" );
			  }
			  UpdateCurve( crs.Table.TableId, crs.Code );
			  setType( Types.Geometry ).write( this, derivedValue, _noCoordinates );
			  this.Inclusion = inclusion;
		 }

		 private void UpdateCurve( int tableId, int code )
		 {
			  if ( this.Long1 != tableId || this.Long2 != code )
			  {
					Long1 = tableId;
					Long2 = code;
					SpaceFillingCurve = _settings.forCrs( tableId, code, true );
			  }
		 }

		 internal virtual void WriteDurationWithTotalAvgSeconds( long months, long days, long totalAvgSeconds, int nanos )
		 {
			  if ( !IsArray )
			  {
					setType( Types.Duration ).write( this, months, days, totalAvgSeconds, nanos );
			  }
			  else
			  {
					Types.DurationArray.write( this, CurrentArrayOffset++, months, days, totalAvgSeconds, nanos );
			  }
		 }
		 // Write byte array is a special case,

		 // instead of calling beginArray and writing the bytes one-by-one
		 // writeByteArray is called so that the bytes can be written in batches.
		 // We don't care about that though so just delegate.
		 public override void WriteByteArray( sbyte[] value )
		 {
			  PrimitiveArrayWriting.WriteTo( this, value );
		 }

		 public override void BeginArray( int size, ArrayType arrayType )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: AbstractArrayType<?> arrayValueType = Types.BY_ARRAY_TYPE[arrayType.ordinal()];
			  AbstractArrayType<object> arrayValueType = Types.ByArrayType[arrayType.ordinal()];
			  Type = arrayValueType;
			  InitializeArrayMeta( size );
			  arrayValueType.InitializeArray( this, size, arrayType );
		 }

		 internal virtual void InitializeArrayMeta( int size )
		 {
			  IsArray = true;
			  ArrayLength = size;
			  CurrentArrayOffset = 0;
		 }

		 public override void EndArray()
		 { // no-op
		 }
		 /* </write.array> */

		 /* </write> */

		 public override string ToString()
		 {
			  return "[" + ToStringInternal() + "],entityId=" + IEntityId;
		 }

		 internal virtual string ToStringInternal()
		 {
			  return TypeConflict.ToString( this );
		 }

		 internal virtual string ToDetailedString()
		 {
			  return "[" + ToDetailedStringInternal() + "],entityId=" + IEntityId;
		 }

		 internal virtual string ToDetailedStringInternal()
		 {
			  return TypeConflict.toDetailedString( this );
		 }

		 internal static void SetCursorException( PageCursor cursor, string reason )
		 {
			  cursor.CursorException = format( "Unable to read generic key slot due to %s", reason );
		 }
	}

}