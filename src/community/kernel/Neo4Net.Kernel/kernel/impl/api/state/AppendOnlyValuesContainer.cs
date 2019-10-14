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
namespace Neo4Net.Kernel.Impl.Api.state
{

	using Resource = Neo4Net.Graphdb.Resource;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using Memory = Neo4Net.Kernel.impl.util.collection.Memory;
	using MemoryAllocator = Neo4Net.Kernel.impl.util.collection.MemoryAllocator;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanArray = Neo4Net.Values.Storable.BooleanArray;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using ByteArray = Neo4Net.Values.Storable.ByteArray;
	using ByteValue = Neo4Net.Values.Storable.ByteValue;
	using CharArray = Neo4Net.Values.Storable.CharArray;
	using CharValue = Neo4Net.Values.Storable.CharValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateArray = Neo4Net.Values.Storable.DateArray;
	using DateTimeArray = Neo4Net.Values.Storable.DateTimeArray;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DoubleArray = Neo4Net.Values.Storable.DoubleArray;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using DurationArray = Neo4Net.Values.Storable.DurationArray;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using FloatArray = Neo4Net.Values.Storable.FloatArray;
	using FloatValue = Neo4Net.Values.Storable.FloatValue;
	using IntArray = Neo4Net.Values.Storable.IntArray;
	using IntValue = Neo4Net.Values.Storable.IntValue;
	using LocalDateTimeArray = Neo4Net.Values.Storable.LocalDateTimeArray;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeArray = Neo4Net.Values.Storable.LocalTimeArray;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using LongArray = Neo4Net.Values.Storable.LongArray;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using NoValue = Neo4Net.Values.Storable.NoValue;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using ShortArray = Neo4Net.Values.Storable.ShortArray;
	using ShortValue = Neo4Net.Values.Storable.ShortValue;
	using StringArray = Neo4Net.Values.Storable.StringArray;
	using StringValue = Neo4Net.Values.Storable.StringValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeArray = Neo4Net.Values.Storable.TimeArray;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using Neo4Net.Values.Storable;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.epochDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.dateArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.dateTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.durationArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.localDateTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.localTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.timeArray;

	public class AppendOnlyValuesContainer : ValuesContainer
	{
		 private static readonly int _chunkSize = ( int ) ByteUnit.kibiBytes( 512 );
		 private const int REMOVED = 0xFF;
		 private static readonly ValueType[] _valueTypes = ValueType.values();

		 private readonly int _chunkSize;
		 private readonly IList<ByteBuffer> _chunks = new List<ByteBuffer>();
		 private readonly IList<Memory> _allocated = new List<Memory>();
		 private readonly Writer _writer;
		 private readonly MemoryAllocator _allocator;
		 private ByteBuffer _currentChunk;
		 private bool _closed;

		 public AppendOnlyValuesContainer( MemoryAllocator allocator ) : this( _chunkSize, allocator )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting AppendOnlyValuesContainer(int chunkSize, org.neo4j.kernel.impl.util.collection.MemoryAllocator allocator)
		 internal AppendOnlyValuesContainer( int chunkSize, MemoryAllocator allocator )
		 {
			  this._chunkSize = chunkSize;
			  this._allocator = allocator;
			  this._writer = new Writer( this );
			  this._currentChunk = AddNewChunk( chunkSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public long add(@Nonnull Value value)
		 public override long Add( Value value )
		 {
			  AssertNotClosed();
			  requireNonNull( value, "value cannot be null" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buf = writer.write(value);
			  ByteBuffer buf = _writer.write( value );
			  if ( buf.remaining() > _currentChunk.remaining() )
			  {
					_currentChunk = AddNewChunk( max( _chunkSize, buf.remaining() ) );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = ((chunks.size() - 1L) << 32) | currentChunk.position();
			  long @ref = ( ( _chunks.Count - 1L ) << 32 ) | _currentChunk.position();
			  _currentChunk.put( buf );
			  return @ref;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.values.storable.Value get(long ref)
		 public override Value Get( long @ref )
		 {
			  AssertNotClosed();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int chunkIdx = (int)(ref >>> 32);
			  int chunkIdx = ( int )( ( long )( ( ulong )@ref >> 32 ) );
			  int offset = ( int ) @ref;

			  checkArgument( chunkIdx >= 0 && chunkIdx < _chunks.Count, "invalid chunk idx %d (total #%d chunks), ref: 0x%X", chunkIdx, _chunks.Count, @ref );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer chunk = chunks.get(chunkIdx);
			  ByteBuffer chunk = _chunks[chunkIdx];
			  checkArgument( offset >= 0 && offset < chunk.position(), "invalid chunk offset (%d), ref: 0x%X", offset, @ref );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int typeId = chunk.get(offset) & 0xFF;
			  int typeId = chunk.get( offset ) & 0xFF;
			  checkArgument( typeId != REMOVED, "element is already removed, ref: 0x%X", @ref );
			  checkArgument( typeId < _valueTypes.Length, "invaling typeId (%d) for ref 0x%X", typeId, @ref );
			  offset++;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ValueType type = VALUE_TYPES[typeId];
			  ValueType type = _valueTypes[typeId];
			  return type.Reader.read( chunk, offset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.values.storable.Value remove(long ref)
		 public override Value Remove( long @ref )
		 {
			  AssertNotClosed();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.Value removed = get(ref);
			  Value removed = Get( @ref );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int chunkIdx = (int)(ref >>> 32);
			  int chunkIdx = ( int )( ( long )( ( ulong )@ref >> 32 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int chunkOffset = (int) ref;
			  int chunkOffset = ( int ) @ref;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer chunk = chunks.get(chunkIdx);
			  ByteBuffer chunk = _chunks[chunkIdx];
			  chunk.put( chunkOffset, ( sbyte ) REMOVED );
			  return removed;
		 }

		 public override void Close()
		 {
			  AssertNotClosed();
			  _closed = true;
			  _allocated.ForEach( Memory.free );
			  _allocated.Clear();
			  _chunks.Clear();
			  _writer.close();
			  _currentChunk = null;
		 }

		 private void AssertNotClosed()
		 {
			  checkState( !_closed, "Container is closed" );
		 }

		 private ByteBuffer AddNewChunk( int size )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.Memory memory = allocator.allocate(size, false);
			  Memory memory = _allocator.allocate( size, false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer chunk = memory.asByteBuffer();
			  ByteBuffer chunk = memory.AsByteBuffer();
			  _allocated.Add( memory );
			  _chunks.Add( chunk );
			  return chunk;
		 }

		 private static BooleanValue ReadBoolean( ByteBuffer chunk, int offset )
		 {
			  return booleanValue( chunk.get( offset ) != 0 );
		 }

		 private static BooleanArray ReadBooleanArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean[] array = new boolean[len];
			  bool[] array = new bool[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.get( offset ) != 0;
					++offset;
			  }
			  return booleanArray( array );
		 }

		 private static ByteValue ReadByte( ByteBuffer chunk, int offset )
		 {
			  return byteValue( chunk.get( offset ) );
		 }

		 private static ByteArray ReadByteArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] array = new byte[len];
			  sbyte[] array = new sbyte[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.get( offset );
					++offset;
			  }
			  return byteArray( array );
		 }

		 private static CharValue ReadChar( ByteBuffer chunk, int offset )
		 {
			  return charValue( chunk.getChar( offset ) );
		 }

		 private static CharArray ReadCharArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char[] array = new char[len];
			  char[] array = new char[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getChar( offset );
					offset += Character.BYTES;
			  }
			  return charArray( array );
		 }

		 private static DateValue ReadDate( ByteBuffer chunk, int offset )
		 {
			  return epochDate( chunk.getLong( offset ) );
		 }

		 private static ArrayValue ReadDateArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.LocalDate[] array = new java.time.LocalDate[len];
			  LocalDate[] array = new LocalDate[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = LocalDate.ofEpochDay( bb.getLong( offset ) );
					offset += Long.BYTES;
			  }
			  return dateArray( array );
		 }

		 private static DoubleValue ReadDouble( ByteBuffer chunk, int offset )
		 {
			  return doubleValue( chunk.getDouble( offset ) );
		 }

		 private static DoubleArray ReadDoubleArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double[] array = new double[len];
			  double[] array = new double[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getDouble( offset );
					offset += Long.BYTES;
			  }
			  return doubleArray( array );
		 }

		 private static DurationValue ReadDuration( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long months = bb.getLong(offset);
			  long months = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long days = bb.getLong(offset);
			  long days = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long seconds = bb.getLong(offset);
			  long seconds = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nanos = bb.getInt(offset);
			  int nanos = bb.getInt( offset );
			  return DurationValue.duration( months, days, seconds, nanos );
		 }

		 private static ArrayValue ReadDurationArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.DurationValue[] array = new org.neo4j.values.storable.DurationValue[len];
			  DurationValue[] array = new DurationValue[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = ReadDuration( bb, offset );
					offset += 3 * Long.BYTES + Integer.BYTES;
			  }
			  return durationArray( array );
		 }

		 private static FloatValue ReadFloat( ByteBuffer chunk, int offset )
		 {
			  return floatValue( chunk.getFloat( offset ) );
		 }

		 private static FloatArray ReadFloatArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
			  float[] array = new float[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getFloat( offset );
					offset += Float.BYTES;
			  }
			  return floatArray( array );
		 }

		 private static IntValue ReadInt( ByteBuffer chunk, int offset )
		 {
			  return intValue( chunk.getInt( offset ) );
		 }

		 private static IntArray ReadIntArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] array = new int[len];
			  int[] array = new int[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getInt( offset );
					offset += Integer.BYTES;
			  }
			  return intArray( array );
		 }

		 private static LocalDateTimeValue ReadLocalDateTime( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long epochSecond = bb.getLong(offset);
			  long epochSecond = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nanos = bb.getInt(offset);
			  int nanos = bb.getInt( offset );
			  return LocalDateTimeValue.localDateTime( epochSecond, nanos );
		 }

		 private static ArrayValue ReadLocalDateTimeArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.LocalDateTime[] array = new java.time.LocalDateTime[len];
			  DateTime[] array = new DateTime[len];
			  for ( int i = 0; i < len; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long epochSecond = bb.getLong(offset);
					long epochSecond = bb.getLong( offset );
					offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nanos = bb.getInt(offset);
					int nanos = bb.getInt( offset );
					offset += Integer.BYTES;
					array[i] = DateTime.ofEpochSecond( epochSecond, nanos, UTC );
			  }
			  return localDateTimeArray( array );
		 }

		 private static LocalTimeValue ReadLocalTime( ByteBuffer chunk, int offset )
		 {
			  return LocalTimeValue.localTime( chunk.getLong( offset ) );
		 }

		 private static ArrayValue ReadLocalTimeArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.LocalTime[] array = new java.time.LocalTime[len];
			  LocalTime[] array = new LocalTime[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = LocalTime.ofNanoOfDay( bb.getLong( offset ) );
					offset += Long.BYTES;
			  }
			  return localTimeArray( array );
		 }

		 private static LongValue ReadLong( ByteBuffer chunk, int offset )
		 {
			  return longValue( chunk.getLong( offset ) );
		 }

		 private static LongArray ReadLongArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] array = new long[len];
			  long[] array = new long[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getLong( offset );
					offset += Long.BYTES;
			  }
			  return longArray( array );
		 }

		 private static PointValue ReadPoint( ByteBuffer chunk, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int crsCode = chunk.getInt(offset);
			  int crsCode = chunk.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.CoordinateReferenceSystem crs = org.neo4j.values.storable.CoordinateReferenceSystem.get(crsCode);
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( crsCode );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double[] coordinate = new double[crs.getDimension()];
			  double[] coordinate = new double[crs.Dimension];
			  for ( int i = 0; i < coordinate.Length; i++ )
			  {
					coordinate[i] = chunk.getDouble( offset );
					offset += Double.BYTES;
			  }
			  return pointValue( crs, coordinate );
		 }

		 private static PointArray ReadPointArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.PointValue[] array = new org.neo4j.values.storable.PointValue[len];
			  PointValue[] array = new PointValue[len];
			  for ( int i = 0; i < len; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.PointValue point = readPoint(bb, offset);
					PointValue point = ReadPoint( bb, offset );
					array[i] = point;
					offset += Integer.BYTES + point.CoordinateReferenceSystem.Dimension * Double.BYTES;
			  }
			  return pointArray( array );
		 }

		 private static string ReadRawString( ByteBuffer chunk, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = chunk.getInt(offset);
			  int len = chunk.getInt( offset );
			  if ( len == 0 )
			  {
					return "";
			  }
			  offset += Integer.BYTES;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char[] chars = new char[len];
			  char[] chars = new char[len];
			  for ( int i = 0; i < len; i++ )
			  {
					chars[i] = chunk.getChar( offset );
					offset += Character.BYTES;
			  }
			  return UnsafeUtil.newSharedArrayString( chars );
		 }

		 private static ShortValue ReadShort( ByteBuffer chunk, int offset )
		 {
			  return shortValue( chunk.getShort( offset ) );
		 }

		 private static ShortArray ReadShortArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final short[] array = new short[len];
			  short[] array = new short[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = bb.getShort( offset );
					offset += Short.BYTES;
			  }
			  return shortArray( array );
		 }

		 private static TextValue ReadString( ByteBuffer chunk, int offset )
		 {
			  return stringValue( ReadRawString( chunk, offset ) );
		 }

		 private static ArrayValue ReadStringArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] array = new String[len];
			  string[] array = new string[len];
			  for ( int i = 0; i < len; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String str = readRawString(bb, offset);
					string str = ReadRawString( bb, offset );
					array[i] = str;
					offset += Integer.BYTES + str.Length * Character.BYTES;
			  }
			  return stringArray( array );
		 }

		 private static TimeValue ReadTime( ByteBuffer bb, int offset )
		 {
			  return TimeValue.time( ReadRawTime( bb, offset ) );
		 }

		 private static ArrayValue ReadTimeArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.OffsetTime[] array = new java.time.OffsetTime[len];
			  OffsetTime[] array = new OffsetTime[len];
			  for ( int i = 0; i < len; i++ )
			  {
					array[i] = ReadRawTime( bb, offset );
					offset += Long.BYTES + Integer.BYTES;
			  }
			  return timeArray( array );
		 }

		 private static OffsetTime ReadRawTime( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nanosOfDayUTC = bb.getLong(offset);
			  long nanosOfDayUTC = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int offsetSeconds = bb.getInt(offset);
			  int offsetSeconds = bb.getInt( offset );
			  return OffsetTime.ofInstant( Instant.ofEpochSecond( 0, nanosOfDayUTC ), ZoneOffset.ofTotalSeconds( offsetSeconds ) );
		 }

		 private static DateTimeValue ReadDateTime( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long epocSeconds = bb.getLong(offset);
			  long epocSeconds = bb.getLong( offset );
			  offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nanos = bb.getInt(offset);
			  int nanos = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int z = bb.getInt(offset);
			  int z = bb.getInt( offset );
			  return DateTimeValue.datetime( epocSeconds, nanos, ToZoneId( z ) );
		 }

		 private static ZoneId ToZoneId( int z )
		 {
			  // if lowest bit is set to 1 then it's a shifted zone id
			  if ( ( z & 1 ) != 0 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String zoneId = org.neo4j.values.storable.TimeZones.map((short)(z >> 1));
					string zoneId = TimeZones.map( ( short )( z >> 1 ) );
					return ZoneId.of( zoneId );
			  }
			  // otherwise it's a shifted offset seconds value
			  // preserve sign bit for negative offsets
			  return ZoneOffset.ofTotalSeconds( z >> 1 );
		 }

		 private static ArrayValue ReadDateTimeArray( ByteBuffer bb, int offset )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = bb.getInt(offset);
			  int len = bb.getInt( offset );
			  offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.ZonedDateTime[] array = new java.time.ZonedDateTime[len];
			  ZonedDateTime[] array = new ZonedDateTime[len];
			  for ( int i = 0; i < len; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long epocSeconds = bb.getLong(offset);
					long epocSeconds = bb.getLong( offset );
					offset += Long.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nanos = bb.getInt(offset);
					int nanos = bb.getInt( offset );
					offset += Integer.BYTES;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int z = bb.getInt(offset);
					int z = bb.getInt( offset );
					offset += Integer.BYTES;
					array[i] = ZonedDateTime.ofInstant( Instant.ofEpochSecond( epocSeconds, nanos ), ToZoneId( z ) );
			  }
			  return dateTimeArray( array );
		 }

		 private sealed class ValueType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NO_VALUE(org.neo4j.values.storable.NoValue.class, (unused, unused2) -> org.neo4j.values.storable.NoValue.NO_VALUE),
			  public static readonly ValueType Boolean = new ValueType( "Boolean", InnerEnum.Boolean, typeof( Neo4Net.Values.Storable.BooleanValue ), AppendOnlyValuesContainer.readBoolean );
			  public static readonly ValueType BooleanArray = new ValueType( "BooleanArray", InnerEnum.BooleanArray, typeof( Neo4Net.Values.Storable.BooleanArray ), AppendOnlyValuesContainer.readBooleanArray );
			  public static readonly ValueType Byte = new ValueType( "Byte", InnerEnum.Byte, typeof( Neo4Net.Values.Storable.ByteValue ), AppendOnlyValuesContainer.readByte );
			  public static readonly ValueType ByteArray = new ValueType( "ByteArray", InnerEnum.ByteArray, typeof( Neo4Net.Values.Storable.ByteArray ), AppendOnlyValuesContainer.readByteArray );
			  public static readonly ValueType Short = new ValueType( "Short", InnerEnum.Short, typeof( Neo4Net.Values.Storable.ShortValue ), AppendOnlyValuesContainer.readShort );
			  public static readonly ValueType ShortArray = new ValueType( "ShortArray", InnerEnum.ShortArray, typeof( Neo4Net.Values.Storable.ShortArray ), AppendOnlyValuesContainer.readShortArray );
			  public static readonly ValueType Int = new ValueType( "Int", InnerEnum.Int, typeof( Neo4Net.Values.Storable.IntValue ), AppendOnlyValuesContainer.readInt );
			  public static readonly ValueType IntArray = new ValueType( "IntArray", InnerEnum.IntArray, typeof( Neo4Net.Values.Storable.IntArray ), AppendOnlyValuesContainer.readIntArray );
			  public static readonly ValueType Long = new ValueType( "Long", InnerEnum.Long, typeof( Neo4Net.Values.Storable.LongValue ), AppendOnlyValuesContainer.readLong );
			  public static readonly ValueType LongArray = new ValueType( "LongArray", InnerEnum.LongArray, typeof( Neo4Net.Values.Storable.LongArray ), AppendOnlyValuesContainer.readLongArray );
			  public static readonly ValueType Float = new ValueType( "Float", InnerEnum.Float, typeof( Neo4Net.Values.Storable.FloatValue ), AppendOnlyValuesContainer.readFloat );
			  public static readonly ValueType FloatArray = new ValueType( "FloatArray", InnerEnum.FloatArray, typeof( Neo4Net.Values.Storable.FloatArray ), AppendOnlyValuesContainer.readFloatArray );
			  public static readonly ValueType Double = new ValueType( "Double", InnerEnum.Double, typeof( Neo4Net.Values.Storable.DoubleValue ), AppendOnlyValuesContainer.readDouble );
			  public static readonly ValueType DoubleArray = new ValueType( "DoubleArray", InnerEnum.DoubleArray, typeof( Neo4Net.Values.Storable.DoubleArray ), AppendOnlyValuesContainer.readDoubleArray );
			  public static readonly ValueType String = new ValueType( "String", InnerEnum.String, typeof( Neo4Net.Values.Storable.StringValue ), AppendOnlyValuesContainer.readString );
			  public static readonly ValueType StringArray = new ValueType( "StringArray", InnerEnum.StringArray, typeof( Neo4Net.Values.Storable.StringArray ), AppendOnlyValuesContainer.readStringArray );
			  public static readonly ValueType Char = new ValueType( "Char", InnerEnum.Char, typeof( Neo4Net.Values.Storable.CharValue ), AppendOnlyValuesContainer.readChar );
			  public static readonly ValueType CharArray = new ValueType( "CharArray", InnerEnum.CharArray, typeof( Neo4Net.Values.Storable.CharArray ), AppendOnlyValuesContainer.readCharArray );
			  public static readonly ValueType Point = new ValueType( "Point", InnerEnum.Point, typeof( Neo4Net.Values.Storable.PointValue ), AppendOnlyValuesContainer.readPoint );
			  public static readonly ValueType PointArray = new ValueType( "PointArray", InnerEnum.PointArray, typeof( Neo4Net.Values.Storable.PointArray ), AppendOnlyValuesContainer.readPointArray );
			  public static readonly ValueType Duration = new ValueType( "Duration", InnerEnum.Duration, typeof( Neo4Net.Values.Storable.DurationValue ), AppendOnlyValuesContainer.readDuration );
			  public static readonly ValueType DurationArray = new ValueType( "DurationArray", InnerEnum.DurationArray, typeof( Neo4Net.Values.Storable.DurationArray ), AppendOnlyValuesContainer.readDurationArray );
			  public static readonly ValueType Date = new ValueType( "Date", InnerEnum.Date, typeof( Neo4Net.Values.Storable.DateValue ), AppendOnlyValuesContainer.readDate );
			  public static readonly ValueType DateArray = new ValueType( "DateArray", InnerEnum.DateArray, typeof( Neo4Net.Values.Storable.DateArray ), AppendOnlyValuesContainer.readDateArray );
			  public static readonly ValueType Time = new ValueType( "Time", InnerEnum.Time, typeof( Neo4Net.Values.Storable.TimeValue ), AppendOnlyValuesContainer.readTime );
			  public static readonly ValueType TimeArray = new ValueType( "TimeArray", InnerEnum.TimeArray, typeof( Neo4Net.Values.Storable.TimeArray ), AppendOnlyValuesContainer.readTimeArray );
			  public static readonly ValueType DateTime = new ValueType( "DateTime", InnerEnum.DateTime, typeof( Neo4Net.Values.Storable.DateTimeValue ), AppendOnlyValuesContainer.readDateTime );
			  public static readonly ValueType DateTimeArray = new ValueType( "DateTimeArray", InnerEnum.DateTimeArray, typeof( Neo4Net.Values.Storable.DateTimeArray ), AppendOnlyValuesContainer.readDateTimeArray );
			  public static readonly ValueType LocalTime = new ValueType( "LocalTime", InnerEnum.LocalTime, typeof( Neo4Net.Values.Storable.LocalTimeValue ), AppendOnlyValuesContainer.readLocalTime );
			  public static readonly ValueType LocalTimeArray = new ValueType( "LocalTimeArray", InnerEnum.LocalTimeArray, typeof( Neo4Net.Values.Storable.LocalTimeArray ), AppendOnlyValuesContainer.readLocalTimeArray );
			  public static readonly ValueType LocalDateTime = new ValueType( "LocalDateTime", InnerEnum.LocalDateTime, typeof( Neo4Net.Values.Storable.LocalDateTimeValue ), AppendOnlyValuesContainer.readLocalDateTime );
			  public static readonly ValueType LocalDateTimeArray = new ValueType( "LocalDateTimeArray", InnerEnum.LocalDateTimeArray, typeof( Neo4Net.Values.Storable.LocalDateTimeArray ), AppendOnlyValuesContainer.readLocalDateTimeArray );

			  private static readonly IList<ValueType> valueList = new List<ValueType>();

			  static ValueType()
			  {
				  valueList.Add( NO_VALUE );
				  valueList.Add( Boolean );
				  valueList.Add( BooleanArray );
				  valueList.Add( Byte );
				  valueList.Add( ByteArray );
				  valueList.Add( Short );
				  valueList.Add( ShortArray );
				  valueList.Add( Int );
				  valueList.Add( IntArray );
				  valueList.Add( Long );
				  valueList.Add( LongArray );
				  valueList.Add( Float );
				  valueList.Add( FloatArray );
				  valueList.Add( Double );
				  valueList.Add( DoubleArray );
				  valueList.Add( String );
				  valueList.Add( StringArray );
				  valueList.Add( Char );
				  valueList.Add( CharArray );
				  valueList.Add( Point );
				  valueList.Add( PointArray );
				  valueList.Add( Duration );
				  valueList.Add( DurationArray );
				  valueList.Add( Date );
				  valueList.Add( DateArray );
				  valueList.Add( Time );
				  valueList.Add( TimeArray );
				  valueList.Add( DateTime );
				  valueList.Add( DateTimeArray );
				  valueList.Add( LocalTime );
				  valueList.Add( LocalTimeArray );
				  valueList.Add( LocalDateTime );
				  valueList.Add( LocalDateTimeArray );
			  }

			  public enum InnerEnum
			  {
				  NO_VALUE,
				  Boolean,
				  BooleanArray,
				  Byte,
				  ByteArray,
				  Short,
				  ShortArray,
				  Int,
				  IntArray,
				  Long,
				  LongArray,
				  Float,
				  FloatArray,
				  Double,
				  DoubleArray,
				  String,
				  StringArray,
				  Char,
				  CharArray,
				  Point,
				  PointArray,
				  Duration,
				  DurationArray,
				  Date,
				  DateArray,
				  Time,
				  TimeArray,
				  DateTime,
				  DateTimeArray,
				  LocalTime,
				  LocalTimeArray,
				  LocalDateTime,
				  LocalDateTimeArray
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal Private readonly;

			  internal ValueType<T>( string name, InnerEnum innerEnum, Type valueClass, ValueReader<T> reader )
			  {
					this._valueClass = valueClass;
					this._reader = reader;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static ValueType ForValue( Neo4Net.Values.Storable.Value value )
			  {
					foreach ( ValueType valueType in values() )
					{
						 if ( valueType._valueClass.IsAssignableFrom( value.GetType() ) )
						 {
							  return valueType;
						 }
					}
					throw new System.ArgumentException( "Unsupported value type: " + value.GetType() );
			  }

			  internal ValueReader Reader
			  {
				  get
				  {
						return _reader;
				  }
			  }

			 public static IList<ValueType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static ValueType valueOf( string name )
			 {
				 foreach ( ValueType enumInstance in ValueType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal interface ValueReader<T> where T : Neo4Net.Values.Storable.Value
		 {
			  T Read( ByteBuffer bb, int offset );
		 }

		 private class Writer : ValueWriter<Exception>, Resource
		 {
			 private readonly AppendOnlyValuesContainer _outerInstance;

			  internal ByteBuffer Buf;
			  internal Memory BufMemory;

			  internal Writer( AppendOnlyValuesContainer outerInstance )
			  {
				  this._outerInstance = outerInstance;
					AllocateBuf( outerInstance.chunkSize );
			  }

			  public override void Close()
			  {
					BufMemory.free();
					BufMemory = null;
					Buf = null;
			  }

			  internal virtual ByteBuffer Write( Value value )
			  {
					checkState( Buf != null, "Writer is closed" );
					try
					{
						 Buf.clear();
						 Buf.put( ( sbyte ) ValueType.forValue( value ).ordinal() );
						 value.WriteTo( this );
						 Buf.flip();
						 return Buf;
					}
					catch ( BufferOverflowException )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newSize = buf.capacity() * 2;
						 int newSize = Buf.capacity() * 2;
						 BufMemory.free();
						 AllocateBuf( newSize );
						 return Write( value );
					}
			  }

			  internal virtual void AllocateBuf( int size )
			  {
					this.BufMemory = outerInstance.allocator.Allocate( size, false );
					this.Buf = BufMemory.asByteBuffer();
			  }

			  public override void WriteNull()
			  {
					// nop
			  }

			  public override void WriteBoolean( bool value )
			  {
					Buf.put( ( sbyte )( value ? 1 : 0 ) );
			  }

			  public override void WriteInteger( sbyte value )
			  {
					Buf.put( value );
			  }

			  public override void WriteInteger( short value )
			  {
					Buf.putShort( value );
			  }

			  public override void WriteInteger( int value )
			  {
					Buf.putInt( value );
			  }

			  public override void WriteInteger( long value )
			  {
					Buf.putLong( value );
			  }

			  public override void WriteFloatingPoint( float value )
			  {
					Buf.putFloat( value );
			  }

			  public override void WriteFloatingPoint( double value )
			  {
					Buf.putDouble( value );
			  }

			  public override void WriteString( string value )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int len = value.length();
					int len = value.Length;
					Buf.putInt( value.Length );
					for ( int i = 0; i < len; i++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char c = value.charAt(i);
						 char c = value[i];
						 Buf.putChar( c );
					}
			  }

			  public override void WriteString( char value )
			  {
					Buf.putChar( value );
			  }

			  public override void BeginArray( int size, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
			  {
					Buf.putInt( size );
			  }

			  public override void EndArray()
			  {
					// nop
			  }

			  public override void WriteByteArray( sbyte[] value )
			  {
					Buf.putInt( value.Length );
					Buf.put( value );
			  }

			  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					checkArgument( coordinate.Length == crs.Dimension, "Dimension for %s is %d, got %d", crs.Name, crs.Dimension, coordinate.Length );
					Buf.putInt( crs.Code );
					for ( int i = 0; i < crs.Dimension; i++ )
					{
						 Buf.putDouble( coordinate[i] );
					}
			  }

			  public override void WriteDuration( long months, long days, long seconds, int nanos )
			  {
					Buf.putLong( months );
					Buf.putLong( days );
					Buf.putLong( seconds );
					Buf.putInt( nanos );
			  }

			  public override void WriteDate( LocalDate localDate )
			  {
					Buf.putLong( localDate.toEpochDay() );
			  }

			  public override void WriteLocalTime( LocalTime localTime )
			  {
					Buf.putLong( localTime.toNanoOfDay() );
			  }

			  public override void WriteTime( OffsetTime offsetTime )
			  {
					Buf.putLong( TemporalUtil.getNanosOfDayUTC( offsetTime ) );
					Buf.putInt( offsetTime.Offset.TotalSeconds );
			  }

			  public override void WriteLocalDateTime( DateTime localDateTime )
			  {
					Buf.putLong( localDateTime.toEpochSecond( UTC ) );
					Buf.putInt( localDateTime.Nano );
			  }

			  public override void WriteDateTime( ZonedDateTime zonedDateTime )
			  {
					Buf.putLong( zonedDateTime.toEpochSecond() );
					Buf.putInt( zonedDateTime.Nano );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.ZoneId zone = zonedDateTime.getZone();
					ZoneId zone = zonedDateTime.Zone;
					if ( zone is ZoneOffset )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int offsetSeconds = ((java.time.ZoneOffset) zone).getTotalSeconds();
						 int offsetSeconds = ( ( ZoneOffset ) zone ).TotalSeconds;
						 // lowest bit set to 0: it's a zone offset in seconds
						 Buf.putInt( offsetSeconds << 1 );
					}
					else
					{
						 // lowest bit set to 1: it's a zone id
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int zoneId = (org.neo4j.values.storable.TimeZones.map(zone.getId()) << 1) | 1;
						 int zoneId = ( TimeZones.map( zone.Id ) << 1 ) | 1;
						 Buf.putInt( zoneId );
					}
			  }
		 }
	}

}