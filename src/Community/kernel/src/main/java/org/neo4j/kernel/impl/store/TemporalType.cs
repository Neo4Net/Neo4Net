using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.Helpers.Collection;
	using StandardFormatSettings = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using LongArray = Neo4Net.Values.Storable.LongArray;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;

	/// <summary>
	/// For the PropertyStore format, check <seealso cref="PropertyStore"/>.
	/// For the array format, check <seealso cref="DynamicArrayStore"/>.
	/// </summary>
	public abstract class TemporalType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_INVALID(0, "Invalid") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { throw new UnsupportedOperationException("Cannot decode invalid temporal"); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return PropertyType.BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { throw new UnsupportedOperationException("Cannot decode invalid temporal array"); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_DATE(1, "Date") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { long epochDay = valueIsInlined(valueBlocks[offset]) ? valueBlocks[offset] >>> 33 : valueBlocks[1 + offset]; return org.neo4j.values.storable.DateValue.epochDate(epochDay); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return valueIsInlined(firstBlock) ? BLOCKS_LONG_INLINED : BLOCKS_LONG_NOT_INLINED; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; LocalDate[] dates = new java.time.LocalDate[numbers.length()]; for(int i = 0; i < dates.length; i++) { dates[i] = java.time.LocalDate.ofEpochDay(numbers.longValue(i)); } return org.neo4j.values.storable.Values.dateArray(dates); } else { throw new InvalidRecordException("Array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } private boolean valueIsInlined(long firstBlock) { return(firstBlock & 0x100000000L) > 0; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_LOCAL_TIME(2, "LocalTime") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { long nanoOfDay = valueIsInlined(valueBlocks[offset]) ? valueBlocks[offset] >>> 33 : valueBlocks[1 + offset]; checkValidNanoOfDay(nanoOfDay); return org.neo4j.values.storable.LocalTimeValue.localTime(nanoOfDay); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return valueIsInlined(firstBlock) ? BLOCKS_LONG_INLINED : BLOCKS_LONG_NOT_INLINED; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; LocalTime[] times = new java.time.LocalTime[numbers.length()]; for(int i = 0; i < times.length; i++) { long nanoOfDay = numbers.longValue(i); checkValidNanoOfDay(nanoOfDay); times[i] = java.time.LocalTime.ofNanoOfDay(nanoOfDay); } return org.neo4j.values.storable.Values.localTimeArray(times); } else { throw new InvalidRecordException("Array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } private boolean valueIsInlined(long firstBlock) { return(firstBlock & 0x100000000L) > 0; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_LOCAL_DATE_TIME(3, "LocalDateTime") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { long nanoOfSecond = valueBlocks[offset] >>> 32; checkValidNanoOfSecond(nanoOfSecond); long epochSecond = valueBlocks[1 + offset]; return org.neo4j.values.storable.LocalDateTimeValue.localDateTime(epochSecond, nanoOfSecond); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return BLOCKS_LOCAL_DATETIME; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; LocalDateTime[] dateTimes = new java.time.LocalDateTime[numbers.length() / BLOCKS_LOCAL_DATETIME]; for(int i = 0; i < dateTimes.length; i++) { long epochSecond = numbers.longValue(i * BLOCKS_LOCAL_DATETIME); long nanoOfSecond = numbers.longValue(i * BLOCKS_LOCAL_DATETIME + 1); checkValidNanoOfSecond(nanoOfSecond); dateTimes[i] = java.time.LocalDateTime.ofInstant(java.time.Instant.ofEpochSecond(epochSecond, nanoOfSecond), UTC); } return org.neo4j.values.storable.Values.localDateTimeArray(dateTimes); } else { throw new InvalidRecordException("Array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_TIME(4, "Time") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { int secondOffset = (int)(valueBlocks[offset] >>> 32); long nanoOfDay = valueBlocks[1 + offset]; checkValidNanoOfDayWithOffset(nanoOfDay, secondOffset); return org.neo4j.values.storable.TimeValue.time(nanoOfDay, java.time.ZoneOffset.ofTotalSeconds(secondOffset)); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return BLOCKS_TIME; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; OffsetTime[] times = new java.time.OffsetTime[(int)(numbers.length() / BLOCKS_TIME)]; for(int i = 0; i < times.length; i++) { long nanoOfDay = numbers.longValue(i * BLOCKS_TIME); int secondOffset = (int) numbers.longValue(i * BLOCKS_TIME + 1); checkValidNanoOfDay(nanoOfDay); times[i] = java.time.OffsetTime.of(java.time.LocalTime.ofNanoOfDay(nanoOfDay), java.time.ZoneOffset.ofTotalSeconds(secondOffset)); } return org.neo4j.values.storable.Values.timeArray(times); } else { throw new InvalidRecordException("Array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_DATE_TIME(5, "DateTime") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { if(storingZoneOffset(valueBlocks[offset])) { int nanoOfSecond = (int)(valueBlocks[offset] >>> 33); checkValidNanoOfSecond(nanoOfSecond); long epochSecond = valueBlocks[1 + offset]; int secondOffset = (int) valueBlocks[2 + offset]; return org.neo4j.values.storable.DateTimeValue.datetime(epochSecond, nanoOfSecond, java.time.ZoneOffset.ofTotalSeconds(secondOffset)); } else { int nanoOfSecond = (int)(valueBlocks[offset] >>> 33); checkValidNanoOfSecond(nanoOfSecond); long epochSecond = valueBlocks[1 + offset]; short zoneNumber = (short) valueBlocks[2 + offset]; return org.neo4j.values.storable.DateTimeValue.datetime(epochSecond, nanoOfSecond, java.time.ZoneId.of(org.neo4j.values.storable.TimeZones.map(zoneNumber))); } } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return BLOCKS_DATETIME; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; ZonedDateTime[] dateTimes = new java.time.ZonedDateTime[numbers.length() / BLOCKS_DATETIME]; for(int i = 0; i < dateTimes.length; i++) { long epochSecond = numbers.longValue(i * BLOCKS_DATETIME); long nanoOfSecond = numbers.longValue(i * BLOCKS_DATETIME + 1); checkValidNanoOfSecond(nanoOfSecond); long zoneValue = numbers.longValue(i * BLOCKS_DATETIME + 2); if((zoneValue & 1) == 1) { int secondOffset = (int)(zoneValue >>> 1); dateTimes[i] = java.time.ZonedDateTime.ofInstant(java.time.Instant.ofEpochSecond(epochSecond, nanoOfSecond), java.time.ZoneOffset.ofTotalSeconds(secondOffset)); } else { short zoneNumber = (short)(zoneValue >>> 1); dateTimes[i] = java.time.ZonedDateTime.ofInstant(java.time.Instant.ofEpochSecond(epochSecond, nanoOfSecond), java.time.ZoneId.of(org.neo4j.values.storable.TimeZones.map(zoneNumber))); } } return org.neo4j.values.storable.Values.dateTimeArray(dateTimes); } else { throw new InvalidRecordException("LocalTime array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } private boolean storingZoneOffset(long firstBlock) { return(firstBlock & 0x100000000L) > 0; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL_DURATION(6, "Duration") { public org.neo4j.values.storable.Value decodeForTemporal(long[] valueBlocks, int offset) { int nanos = (int)(valueBlocks[offset] >>> 32); long months = valueBlocks[1 + offset]; long days = valueBlocks[2 + offset]; long seconds = valueBlocks[3 + offset]; return org.neo4j.values.storable.DurationValue.duration(months, days, seconds, nanos); } public int calculateNumberOfBlocksUsedForTemporal(long firstBlock) { return BLOCKS_DURATION; } public org.neo4j.values.storable.ArrayValue decodeArray(org.neo4j.values.storable.Value dataValue) { if(dataValue instanceof org.neo4j.values.storable.LongArray) { org.neo4j.values.storable.LongArray numbers = (org.neo4j.values.storable.LongArray) dataValue; DurationValue[] durations = new org.neo4j.values.storable.DurationValue[(int)(numbers.length() / BLOCKS_DURATION)]; for(int i = 0; i < durations.length; i++) { durations[i] = org.neo4j.values.storable.DurationValue.duration(numbers.longValue(i * BLOCKS_DURATION), numbers.longValue(i * BLOCKS_DURATION + 1), numbers.longValue(i * BLOCKS_DURATION + 2), numbers.longValue(i * BLOCKS_DURATION + 3)); } return org.neo4j.values.storable.Values.durationArray(durations); } else { throw new InvalidRecordException("Array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: LongArray."); } } };

		 private static readonly IList<TemporalType> valueList = new List<TemporalType>();

		 static TemporalType()
		 {
			 valueList.Add( TEMPORAL_INVALID );
			 valueList.Add( TEMPORAL_DATE );
			 valueList.Add( TEMPORAL_LOCAL_TIME );
			 valueList.Add( TEMPORAL_LOCAL_DATE_TIME );
			 valueList.Add( TEMPORAL_TIME );
			 valueList.Add( TEMPORAL_DATE_TIME );
			 valueList.Add( TEMPORAL_DURATION );
		 }

		 public enum InnerEnum
		 {
			 TEMPORAL_INVALID,
			 TEMPORAL_DATE,
			 TEMPORAL_LOCAL_TIME,
			 TEMPORAL_LOCAL_DATE_TIME,
			 TEMPORAL_TIME,
			 TEMPORAL_DATE_TIME,
			 TEMPORAL_DURATION
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private TemporalType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private const int BLOCKS_LONG_INLINED = 1;
		 private const int BLOCKS_LONG_NOT_INLINED = 2;
		 private const int BLOCKS_LOCAL_DATETIME = 2;
		 private const int BLOCKS_TIME = 2;
		 private const int BLOCKS_DATETIME = 3;
		 private const int BLOCKS_DURATION = 4;

		 /// <summary>
		 /// Handler for header information for Temporal objects and arrays of Temporal objects
		 /// </summary>
		 public static final TemporalType public static class TemporalHeader
		 {
			 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( sbyte[] bytes ) { bytes[0] = ( sbyte ) PropertyType.Temporal.intValue(); bytes[1] = (sbyte) temporalType; } static TemporalHeader fromArrayHeaderBytes(sbyte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
		 }
		 private static final TemporalType[] TYPES = TemporalType.values = new TemporalType("public static class TemporalHeader { private final int temporalType; private TemporalHeader(int temporalType) { this.temporalType = temporalType; } private void writeArrayHeaderTo(byte[] bytes) { bytes[0] = (byte) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); } } private static final TemporalType[] TYPES = TemporalType.values", InnerEnum.public static class TemporalHeader
		 {
			 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( sbyte[] bytes ) { bytes[0] = ( sbyte ) PropertyType.Temporal.intValue(); bytes[1] = (sbyte) temporalType; } static TemporalHeader fromArrayHeaderBytes(sbyte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
		 }
		 private static final TemporalType[] TYPES = TemporalType.values, );

		 private static readonly IList<TemporalType> valueList = new List<TemporalType>();

		 static TemporalType()
		 {
			 valueList.Add( TEMPORAL_INVALID );
			 valueList.Add( TEMPORAL_DATE );
			 valueList.Add( TEMPORAL_LOCAL_TIME );
			 valueList.Add( TEMPORAL_LOCAL_DATE_TIME );
			 valueList.Add( TEMPORAL_TIME );
			 valueList.Add( TEMPORAL_DATE_TIME );
			 valueList.Add( TEMPORAL_DURATION );
			 valueList.Add(public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values);
		 }

		 public enum InnerEnum
		 {
			 TEMPORAL_INVALID,
			 TEMPORAL_DATE,
			 TEMPORAL_LOCAL_TIME,
			 TEMPORAL_LOCAL_DATE_TIME,
			 TEMPORAL_TIME,
			 TEMPORAL_DATE_TIME,
			 TEMPORAL_DURATION,
			 public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private static readonly IDictionary<string, TemporalType> all = new Dictionary<string, TemporalType>( TYPES.length );

		 public static readonly TemporalType static
		 {
			 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
		 }
		 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L = new TemporalType("static { for(TemporalType temporalType : TYPES) { all.put(temporalType.name, temporalType); } } private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L", InnerEnum.static
		 {
			 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
		 }
		 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L);

		 private static readonly IList<TemporalType> valueList = new List<TemporalType>();

		 static TemporalType()
		 {
			 valueList.Add( TEMPORAL_INVALID );
			 valueList.Add( TEMPORAL_DATE );
			 valueList.Add( TEMPORAL_LOCAL_TIME );
			 valueList.Add( TEMPORAL_LOCAL_DATE_TIME );
			 valueList.Add( TEMPORAL_TIME );
			 valueList.Add( TEMPORAL_DATE_TIME );
			 valueList.Add( TEMPORAL_DURATION );
			 valueList.Add(public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values);
			 valueList.Add(static
			 {
				 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
			 }
			 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L);
		 }

		 public enum InnerEnum
		 {
			 TEMPORAL_INVALID,
			 TEMPORAL_DATE,
			 TEMPORAL_LOCAL_TIME,
			 TEMPORAL_LOCAL_DATE_TIME,
			 TEMPORAL_TIME,
			 TEMPORAL_DATE_TIME,
			 TEMPORAL_DURATION,
			 public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values,
			 static
			 {
				 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
			 }
			 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 public static readonly TemporalType private static int getTemporalType( long firstBlock ) { return( int )( ( firstBlock & TEMPORAL_TYPE_MASK ) >> 28 ); } public static int calculateNumberOfBlocksUsed( long firstBlock ) { TemporalType geometryType = find( getTemporalType( firstBlock ) ); return geometryType.calculateNumberOfBlocksUsedForTemporal( firstBlock ); } private static TemporalType find( int temporalType )
		 {
			 if ( temporalType < TYPES.length && temporalType >= 0 ) { return TYPES[temporalType]; } else { return TEMPORAL_INVALID; }
		 }
		 private static void checkValidNanoOfDay( long nanoOfDay )
		 {
			 if ( nanoOfDay > java.time.LocalTime.MAX.toNanoOfDay() || nanoOfDay < java.time.LocalTime.MIN.toNanoOfDay() ) { throw new InvalidRecordException("Nanosecond of day out of range:" + nanoOfDay); }
		 }
		 private static void checkValidNanoOfDayWithOffset( long nanoOfDayUTC, int secondOffset ) { long nanoOfDay = Neo4Net.Values.utils.TemporalUtil.nanosOfDayToLocal( nanoOfDayUTC, secondOffset ); checkValidNanoOfDay( nanoOfDay ); } private static void checkValidNanoOfSecond( long nanoOfSecond )
		 {
			 if ( nanoOfSecond > 999_999_999 || nanoOfSecond < 0 ) { throw new InvalidRecordException( "Nanosecond of second out of range:" + nanoOfSecond ); }
		 }
		 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.impl.store.record.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int temporalType = getTemporalType(firstBlock); return find(temporalType).decodeForTemporal(valueBlocks, offset); } public static long[] encodeDate(int keyId, long epochDay) { return encodeLong(keyId, epochDay, TemporalType.TEMPORAL_DATE.temporalType); } public static long[] encodeLocalTime(int keyId, long nanoOfDay) { return encodeLong(keyId, nanoOfDay, TemporalType.TEMPORAL_LOCAL_TIME.temporalType); } private static long[] encodeLong(int keyId, long val, int temporalType)
		 {
			 int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = temporalType << (idBits + 4); long[] data; if (ShortArray.LONG.getRequiredBits(val) <= 64 - 33) { data = new long[BLOCKS_LONG_INLINED]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (val << 33); } else { data = new long[BLOCKS_LONG_NOT_INLINED]; data[0] = keyAndType | temporalTypeBits; data[1] = val; } return data;
		 }
		 public static long[] encodeLocalDateTime( int keyId, long epochSecond, long nanoOfSecond ) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_LOCAL_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 32); data[1] = epochSecond; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, String zoneId) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(zoneId); long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = zoneNumber; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = secondOffset; return data; } public static long[] encodeTime(int keyId, long nanoOfDayUTC, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_TIME]; data[0] = keyAndType | temporalTypeBits | ((long) secondOffset << 32); data[1] = nanoOfDayUTC; return data; } public static long[] encodeDuration(int keyId, long months, long days, long seconds, int nanos) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DURATION.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DURATION]; data[0] = keyAndType | temporalTypeBits | ((long) nanos << 32); data[1] = months; data[2] = days; data[3] = seconds; return data; } public static byte[] encodeDateArray(java.time.LocalDate[] dates)
		 {
			 long[] data = new long[dates.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = dates[i].toEpochDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeLocalTimeArray( java.time.LocalTime[] times )
		 {
			 long[] data = new long[times.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = times[i].toNanoOfDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeLocalDateTimeArray( java.time.LocalDateTime[] dateTimes )
		 {
			 long[] data = new long[dateTimes.length * BLOCKS_LOCAL_DATETIME]; for ( int i = 0; i < dateTimes.length; i++ ) { data[i * BLOCKS_LOCAL_DATETIME] = dateTimes[i].toEpochSecond( UTC ); data[i * BLOCKS_LOCAL_DATETIME + 1] = dateTimes[i].getNano(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeTimeArray( java.time.OffsetTime[] times )
		 {
			 long[] data = new long[( int )( Math.ceil( times.length * BLOCKS_TIME ) )]; int i; for ( i = 0; i < times.length; i++ ) { data[i * BLOCKS_TIME] = times[i].toLocalTime().toNanoOfDay(); data[i * BLOCKS_TIME + 1] = times[i].getOffset().getTotalSeconds(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeDateTimeArray( java.time.ZonedDateTime[] dateTimes )
		 {
			 long[] data = new long[dateTimes.length * BLOCKS_DATETIME]; int i; for ( i = 0; i < dateTimes.length; i++ )
			 {
				 data[i * BLOCKS_DATETIME] = dateTimes[i].toEpochSecond(); data[i * BLOCKS_DATETIME + 1] = dateTimes[i].getNano(); if (dateTimes[i].getZone() instanceof java.time.ZoneOffset) { java.time.ZoneOffset offset = (java.time.ZoneOffset) dateTimes[i].getZone(); int secondOffset = offset.getTotalSeconds(); data[i * BLOCKS_DATETIME + 2] = secondOffset << 1 | 1L; } else { String timeZoneId = dateTimes[i].getZone().getId(); short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(timeZoneId); data[i * BLOCKS_DATETIME + 2] = zoneNumber << 1; }
			 }
			 TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DATE_TIME.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
		 }
		 public static byte[] encodeDurationArray( Neo4Net.Values.Storable.DurationValue[] durations )
		 {
			 long[] data = new long[durations.length * BLOCKS_DURATION]; for ( int i = 0; i < durations.length; i++ ) { data[i * BLOCKS_DURATION] = durations[i].get( java.time.temporal.ChronoUnit.MONTHS ); data[i * BLOCKS_DURATION + 1] = durations[i].get( java.time.temporal.ChronoUnit.DAYS ); data[i * BLOCKS_DURATION + 2] = durations[i].get( java.time.temporal.ChronoUnit.SECONDS ); data[i * BLOCKS_DURATION + 3] = durations[i].get( java.time.temporal.ChronoUnit.NANOS ); } TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DURATION.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
		 }
		 public static Neo4Net.Values.Storable.ArrayValue decodeTemporalArray( TemporalHeader header, byte[] data ) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader( data ); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy( data, dataHeader.length, dataBody, 0, dataBody.length ); Neo4Net.Values.Storable.Value dataValue = DynamicArrayStore.getRightArray( Neo4Net.Helpers.Collection.Pair.of( dataHeader, dataBody ) ); return find( header.temporalType ).decodeArray( dataValue ); } private final int temporalType = new TemporalType("private static int getTemporalType(long firstBlock) { return(int)((firstBlock & TEMPORAL_TYPE_MASK) >> 28); } public static int calculateNumberOfBlocksUsed(long firstBlock) { TemporalType geometryType = find(getTemporalType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForTemporal(firstBlock); } private static TemporalType find(int temporalType) { if(temporalType < TYPES.length && temporalType >= 0) { return TYPES[temporalType]; } else { return TEMPORAL_INVALID; } } private static void checkValidNanoOfDay(long nanoOfDay) { if(nanoOfDay > java.time.LocalTime.MAX.toNanoOfDay() || nanoOfDay < java.time.LocalTime.MIN.toNanoOfDay()) { throw new InvalidRecordException("Nanosecond of day out of range:" + nanoOfDay); } } private static void checkValidNanoOfDayWithOffset(long nanoOfDayUTC, int secondOffset) { long nanoOfDay = org.neo4j.values.utils.TemporalUtil.nanosOfDayToLocal(nanoOfDayUTC, secondOffset); checkValidNanoOfDay(nanoOfDay); } private static void checkValidNanoOfSecond(long nanoOfSecond) { if(nanoOfSecond > 999_999_999 || nanoOfSecond < 0) { throw new InvalidRecordException("Nanosecond of second out of range:" + nanoOfSecond); } } public static org.neo4j.values.storable.Value decode(org.neo4j.kernel.impl.store.record.PropertyBlock block) { return decode(block.getValueBlocks(), 0); } public static org.neo4j.values.storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int temporalType = getTemporalType(firstBlock); return find(temporalType).decodeForTemporal(valueBlocks, offset); } public static long[] encodeDate(int keyId, long epochDay) { return encodeLong(keyId, epochDay, TemporalType.TEMPORAL_DATE.temporalType); } public static long[] encodeLocalTime(int keyId, long nanoOfDay) { return encodeLong(keyId, nanoOfDay, TemporalType.TEMPORAL_LOCAL_TIME.temporalType); } private static long[] encodeLong(int keyId, long val, int temporalType) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = temporalType << (idBits + 4); long[] data; if(ShortArray.LONG.getRequiredBits(val) <= 64 - 33) { data = new long[BLOCKS_LONG_INLINED]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (val << 33); } else { data = new long[BLOCKS_LONG_NOT_INLINED]; data[0] = keyAndType | temporalTypeBits; data[1] = val; } return data; } public static long[] encodeLocalDateTime(int keyId, long epochSecond, long nanoOfSecond) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_LOCAL_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 32); data[1] = epochSecond; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, String zoneId) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; short zoneNumber = org.neo4j.values.storable.TimeZones.map(zoneId); long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = zoneNumber; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, int secondOffset) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = secondOffset; return data; } public static long[] encodeTime(int keyId, long nanoOfDayUTC, int secondOffset) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_TIME]; data[0] = keyAndType | temporalTypeBits | ((long) secondOffset << 32); data[1] = nanoOfDayUTC; return data; } public static long[] encodeDuration(int keyId, long months, long days, long seconds, int nanos) { int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DURATION.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DURATION]; data[0] = keyAndType | temporalTypeBits | ((long) nanos << 32); data[1] = months; data[2] = days; data[3] = seconds; return data; } public static byte[] encodeDateArray(java.time.LocalDate[] dates) { long[] data = new long[dates.length]; for(int i = 0; i < data.length; i++) { data[i] = dates[i].toEpochDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static byte[] encodeLocalTimeArray(java.time.LocalTime[] times) { long[] data = new long[times.length]; for(int i = 0; i < data.length; i++) { data[i] = times[i].toNanoOfDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static byte[] encodeLocalDateTimeArray(java.time.LocalDateTime[] dateTimes) { long[] data = new long[dateTimes.length * BLOCKS_LOCAL_DATETIME]; for(int i = 0; i < dateTimes.length; i++) { data[i * BLOCKS_LOCAL_DATETIME] = dateTimes[i].toEpochSecond(UTC); data[i * BLOCKS_LOCAL_DATETIME + 1] = dateTimes[i].getNano(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static byte[] encodeTimeArray(java.time.OffsetTime[] times) { long[] data = new long[(int)(Math.ceil(times.length * BLOCKS_TIME))]; int i; for(i = 0; i < times.length; i++) { data[i * BLOCKS_TIME] = times[i].toLocalTime().toNanoOfDay(); data[i * BLOCKS_TIME + 1] = times[i].getOffset().getTotalSeconds(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static byte[] encodeDateTimeArray(java.time.ZonedDateTime[] dateTimes) { long[] data = new long[dateTimes.length * BLOCKS_DATETIME]; int i; for(i = 0; i < dateTimes.length; i++) { data[i * BLOCKS_DATETIME] = dateTimes[i].toEpochSecond(); data[i * BLOCKS_DATETIME + 1] = dateTimes[i].getNano(); if(dateTimes[i].getZone() instanceof java.time.ZoneOffset) { java.time.ZoneOffset offset = (java.time.ZoneOffset) dateTimes[i].getZone(); int secondOffset = offset.getTotalSeconds(); data[i * BLOCKS_DATETIME + 2] = secondOffset << 1 | 1L; } else { String timeZoneId = dateTimes[i].getZone().getId(); short zoneNumber = org.neo4j.values.storable.TimeZones.map(timeZoneId); data[i * BLOCKS_DATETIME + 2] = zoneNumber << 1; } } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static byte[] encodeDurationArray(org.neo4j.values.storable.DurationValue[] durations) { long[] data = new long[durations.length * BLOCKS_DURATION]; for(int i = 0; i < durations.length; i++) { data[i * BLOCKS_DURATION] = durations[i].get(java.time.temporal.ChronoUnit.MONTHS); data[i * BLOCKS_DURATION + 1] = durations[i].get(java.time.temporal.ChronoUnit.DAYS); data[i * BLOCKS_DURATION + 2] = durations[i].get(java.time.temporal.ChronoUnit.SECONDS); data[i * BLOCKS_DURATION + 3] = durations[i].get(java.time.temporal.ChronoUnit.NANOS); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DURATION.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes; } public static org.neo4j.values.storable.ArrayValue decodeTemporalArray(TemporalHeader header, byte[] data) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader(data); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy(data, dataHeader.length, dataBody, 0, dataBody.length); org.neo4j.values.storable.Value dataValue = DynamicArrayStore.getRightArray(org.neo4j.helpers.collection.Pair.of(dataHeader, dataBody)); return find(header.temporalType).decodeArray(dataValue); } private final int temporalType", InnerEnum.private static int getTemporalType(long firstBlock) { return(int)((firstBlock & TEMPORAL_TYPE_MASK) >> 28); } public static int calculateNumberOfBlocksUsed(long firstBlock) { TemporalType geometryType = find(getTemporalType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForTemporal(firstBlock); } private static TemporalType find(int temporalType)
		 {
			 if ( temporalType < TYPES.length && temporalType >= 0 ) { return TYPES[temporalType]; } else { return TEMPORAL_INVALID; }
		 }
		 private static void checkValidNanoOfDay( long nanoOfDay )
		 {
			 if ( nanoOfDay > java.time.LocalTime.MAX.toNanoOfDay() || nanoOfDay < java.time.LocalTime.MIN.toNanoOfDay() ) { throw new InvalidRecordException("Nanosecond of day out of range:" + nanoOfDay); }
		 }
		 private static void checkValidNanoOfDayWithOffset( long nanoOfDayUTC, int secondOffset ) { long nanoOfDay = Neo4Net.Values.utils.TemporalUtil.nanosOfDayToLocal( nanoOfDayUTC, secondOffset ); checkValidNanoOfDay( nanoOfDay ); } private static void checkValidNanoOfSecond( long nanoOfSecond )
		 {
			 if ( nanoOfSecond > 999_999_999 || nanoOfSecond < 0 ) { throw new InvalidRecordException( "Nanosecond of second out of range:" + nanoOfSecond ); }
		 }
		 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.impl.store.record.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int temporalType = getTemporalType(firstBlock); return find(temporalType).decodeForTemporal(valueBlocks, offset); } public static long[] encodeDate(int keyId, long epochDay) { return encodeLong(keyId, epochDay, TemporalType.TEMPORAL_DATE.temporalType); } public static long[] encodeLocalTime(int keyId, long nanoOfDay) { return encodeLong(keyId, nanoOfDay, TemporalType.TEMPORAL_LOCAL_TIME.temporalType); } private static long[] encodeLong(int keyId, long val, int temporalType)
		 {
			 int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = temporalType << (idBits + 4); long[] data; if (ShortArray.LONG.getRequiredBits(val) <= 64 - 33) { data = new long[BLOCKS_LONG_INLINED]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (val << 33); } else { data = new long[BLOCKS_LONG_NOT_INLINED]; data[0] = keyAndType | temporalTypeBits; data[1] = val; } return data;
		 }
		 public static long[] encodeLocalDateTime( int keyId, long epochSecond, long nanoOfSecond ) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_LOCAL_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 32); data[1] = epochSecond; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, String zoneId) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(zoneId); long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = zoneNumber; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = secondOffset; return data; } public static long[] encodeTime(int keyId, long nanoOfDayUTC, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_TIME]; data[0] = keyAndType | temporalTypeBits | ((long) secondOffset << 32); data[1] = nanoOfDayUTC; return data; } public static long[] encodeDuration(int keyId, long months, long days, long seconds, int nanos) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DURATION.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DURATION]; data[0] = keyAndType | temporalTypeBits | ((long) nanos << 32); data[1] = months; data[2] = days; data[3] = seconds; return data; } public static byte[] encodeDateArray(java.time.LocalDate[] dates)
		 {
			 long[] data = new long[dates.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = dates[i].toEpochDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeLocalTimeArray( java.time.LocalTime[] times )
		 {
			 long[] data = new long[times.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = times[i].toNanoOfDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeLocalDateTimeArray( java.time.LocalDateTime[] dateTimes )
		 {
			 long[] data = new long[dateTimes.length * BLOCKS_LOCAL_DATETIME]; for ( int i = 0; i < dateTimes.length; i++ ) { data[i * BLOCKS_LOCAL_DATETIME] = dateTimes[i].toEpochSecond( UTC ); data[i * BLOCKS_LOCAL_DATETIME + 1] = dateTimes[i].getNano(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeTimeArray( java.time.OffsetTime[] times )
		 {
			 long[] data = new long[( int )( Math.ceil( times.length * BLOCKS_TIME ) )]; int i; for ( i = 0; i < times.length; i++ ) { data[i * BLOCKS_TIME] = times[i].toLocalTime().toNanoOfDay(); data[i * BLOCKS_TIME + 1] = times[i].getOffset().getTotalSeconds(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static byte[] encodeDateTimeArray( java.time.ZonedDateTime[] dateTimes )
		 {
			 long[] data = new long[dateTimes.length * BLOCKS_DATETIME]; int i; for ( i = 0; i < dateTimes.length; i++ )
			 {
				 data[i * BLOCKS_DATETIME] = dateTimes[i].toEpochSecond(); data[i * BLOCKS_DATETIME + 1] = dateTimes[i].getNano(); if (dateTimes[i].getZone() instanceof java.time.ZoneOffset) { java.time.ZoneOffset offset = (java.time.ZoneOffset) dateTimes[i].getZone(); int secondOffset = offset.getTotalSeconds(); data[i * BLOCKS_DATETIME + 2] = secondOffset << 1 | 1L; } else { String timeZoneId = dateTimes[i].getZone().getId(); short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(timeZoneId); data[i * BLOCKS_DATETIME + 2] = zoneNumber << 1; }
			 }
			 TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DATE_TIME.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
		 }
		 public static byte[] encodeDurationArray( Neo4Net.Values.Storable.DurationValue[] durations )
		 {
			 long[] data = new long[durations.length * BLOCKS_DURATION]; for ( int i = 0; i < durations.length; i++ ) { data[i * BLOCKS_DURATION] = durations[i].get( java.time.temporal.ChronoUnit.MONTHS ); data[i * BLOCKS_DURATION + 1] = durations[i].get( java.time.temporal.ChronoUnit.DAYS ); data[i * BLOCKS_DURATION + 2] = durations[i].get( java.time.temporal.ChronoUnit.SECONDS ); data[i * BLOCKS_DURATION + 3] = durations[i].get( java.time.temporal.ChronoUnit.NANOS ); } TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DURATION.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
		 }
		 public static Neo4Net.Values.Storable.ArrayValue decodeTemporalArray( TemporalHeader header, byte[] data ) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader( data ); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy( data, dataHeader.length, dataBody, 0, dataBody.length ); Neo4Net.Values.Storable.Value dataValue = DynamicArrayStore.getRightArray( Neo4Net.Helpers.Collection.Pair.of( dataHeader, dataBody ) ); return find( header.temporalType ).decodeArray( dataValue ); } private final int temporalType);

		 private static readonly IList<TemporalType> valueList = new List<TemporalType>();

		 static TemporalType()
		 {
			 valueList.Add( TEMPORAL_INVALID );
			 valueList.Add( TEMPORAL_DATE );
			 valueList.Add( TEMPORAL_LOCAL_TIME );
			 valueList.Add( TEMPORAL_LOCAL_DATE_TIME );
			 valueList.Add( TEMPORAL_TIME );
			 valueList.Add( TEMPORAL_DATE_TIME );
			 valueList.Add( TEMPORAL_DURATION );
			 valueList.Add(public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values);
			 valueList.Add(static
			 {
				 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
			 }
			 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L);
			 valueList.Add(private static int getTemporalType(long firstBlock) { return(int)((firstBlock & TEMPORAL_TYPE_MASK) >> 28); } public static int calculateNumberOfBlocksUsed(long firstBlock) { TemporalType geometryType = find(getTemporalType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForTemporal(firstBlock); } private static TemporalType find(int temporalType)
			 {
				 if ( temporalType < TYPES.length && temporalType >= 0 ) { return TYPES[temporalType]; } else { return TEMPORAL_INVALID; }
			 }
			 private static void checkValidNanoOfDay( long nanoOfDay )
			 {
				 if ( nanoOfDay > java.time.LocalTime.MAX.toNanoOfDay() || nanoOfDay < java.time.LocalTime.MIN.toNanoOfDay() ) { throw new InvalidRecordException("Nanosecond of day out of range:" + nanoOfDay); }
			 }
			 private static void checkValidNanoOfDayWithOffset( long nanoOfDayUTC, int secondOffset ) { long nanoOfDay = Neo4Net.Values.utils.TemporalUtil.nanosOfDayToLocal( nanoOfDayUTC, secondOffset ); checkValidNanoOfDay( nanoOfDay ); } private static void checkValidNanoOfSecond( long nanoOfSecond )
			 {
				 if ( nanoOfSecond > 999_999_999 || nanoOfSecond < 0 ) { throw new InvalidRecordException( "Nanosecond of second out of range:" + nanoOfSecond ); }
			 }
			 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.impl.store.record.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int temporalType = getTemporalType(firstBlock); return find(temporalType).decodeForTemporal(valueBlocks, offset); } public static long[] encodeDate(int keyId, long epochDay) { return encodeLong(keyId, epochDay, TemporalType.TEMPORAL_DATE.temporalType); } public static long[] encodeLocalTime(int keyId, long nanoOfDay) { return encodeLong(keyId, nanoOfDay, TemporalType.TEMPORAL_LOCAL_TIME.temporalType); } private static long[] encodeLong(int keyId, long val, int temporalType)
			 {
				 int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = temporalType << (idBits + 4); long[] data; if (ShortArray.LONG.getRequiredBits(val) <= 64 - 33) { data = new long[BLOCKS_LONG_INLINED]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (val << 33); } else { data = new long[BLOCKS_LONG_NOT_INLINED]; data[0] = keyAndType | temporalTypeBits; data[1] = val; } return data;
			 }
			 public static long[] encodeLocalDateTime( int keyId, long epochSecond, long nanoOfSecond ) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_LOCAL_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 32); data[1] = epochSecond; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, String zoneId) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(zoneId); long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = zoneNumber; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = secondOffset; return data; } public static long[] encodeTime(int keyId, long nanoOfDayUTC, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_TIME]; data[0] = keyAndType | temporalTypeBits | ((long) secondOffset << 32); data[1] = nanoOfDayUTC; return data; } public static long[] encodeDuration(int keyId, long months, long days, long seconds, int nanos) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DURATION.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DURATION]; data[0] = keyAndType | temporalTypeBits | ((long) nanos << 32); data[1] = months; data[2] = days; data[3] = seconds; return data; } public static byte[] encodeDateArray(java.time.LocalDate[] dates)
			 {
				 long[] data = new long[dates.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = dates[i].toEpochDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeLocalTimeArray( java.time.LocalTime[] times )
			 {
				 long[] data = new long[times.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = times[i].toNanoOfDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeLocalDateTimeArray( java.time.LocalDateTime[] dateTimes )
			 {
				 long[] data = new long[dateTimes.length * BLOCKS_LOCAL_DATETIME]; for ( int i = 0; i < dateTimes.length; i++ ) { data[i * BLOCKS_LOCAL_DATETIME] = dateTimes[i].toEpochSecond( UTC ); data[i * BLOCKS_LOCAL_DATETIME + 1] = dateTimes[i].getNano(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeTimeArray( java.time.OffsetTime[] times )
			 {
				 long[] data = new long[( int )( Math.ceil( times.length * BLOCKS_TIME ) )]; int i; for ( i = 0; i < times.length; i++ ) { data[i * BLOCKS_TIME] = times[i].toLocalTime().toNanoOfDay(); data[i * BLOCKS_TIME + 1] = times[i].getOffset().getTotalSeconds(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeDateTimeArray( java.time.ZonedDateTime[] dateTimes )
			 {
				 long[] data = new long[dateTimes.length * BLOCKS_DATETIME]; int i; for ( i = 0; i < dateTimes.length; i++ )
				 {
					 data[i * BLOCKS_DATETIME] = dateTimes[i].toEpochSecond(); data[i * BLOCKS_DATETIME + 1] = dateTimes[i].getNano(); if (dateTimes[i].getZone() instanceof java.time.ZoneOffset) { java.time.ZoneOffset offset = (java.time.ZoneOffset) dateTimes[i].getZone(); int secondOffset = offset.getTotalSeconds(); data[i * BLOCKS_DATETIME + 2] = secondOffset << 1 | 1L; } else { String timeZoneId = dateTimes[i].getZone().getId(); short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(timeZoneId); data[i * BLOCKS_DATETIME + 2] = zoneNumber << 1; }
				 }
				 TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DATE_TIME.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
			 }
			 public static byte[] encodeDurationArray( Neo4Net.Values.Storable.DurationValue[] durations )
			 {
				 long[] data = new long[durations.length * BLOCKS_DURATION]; for ( int i = 0; i < durations.length; i++ ) { data[i * BLOCKS_DURATION] = durations[i].get( java.time.temporal.ChronoUnit.MONTHS ); data[i * BLOCKS_DURATION + 1] = durations[i].get( java.time.temporal.ChronoUnit.DAYS ); data[i * BLOCKS_DURATION + 2] = durations[i].get( java.time.temporal.ChronoUnit.SECONDS ); data[i * BLOCKS_DURATION + 3] = durations[i].get( java.time.temporal.ChronoUnit.NANOS ); } TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DURATION.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
			 }
			 public static Neo4Net.Values.Storable.ArrayValue decodeTemporalArray( TemporalHeader header, byte[] data ) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader( data ); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy( data, dataHeader.length, dataBody, 0, dataBody.length ); Neo4Net.Values.Storable.Value dataValue = DynamicArrayStore.getRightArray( Neo4Net.Helpers.Collection.Pair.of( dataHeader, dataBody ) ); return find( header.temporalType ).decodeArray( dataValue ); } private final int temporalType);
		 }

		 public enum InnerEnum
		 {
			 TEMPORAL_INVALID,
			 TEMPORAL_DATE,
			 TEMPORAL_LOCAL_TIME,
			 TEMPORAL_LOCAL_DATE_TIME,
			 TEMPORAL_TIME,
			 TEMPORAL_DATE_TIME,
			 TEMPORAL_DURATION,
			 public static class TemporalHeader
			 {
				 private final int temporalType; private TemporalHeader( int temporalType ) { this.temporalType = temporalType; } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.TEMPORAL.intValue(); bytes[1] = (byte) temporalType; } static TemporalHeader fromArrayHeaderBytes(byte[] header) { int temporalType = Byte.toUnsignedInt(header[1]); return new TemporalHeader(temporalType); } public static TemporalHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int temporalType = Byte.toUnsignedInt(buffer.get()); return new TemporalHeader(temporalType); }
			 }
			 private static final TemporalType[] TYPES = TemporalType.values,
			 static
			 {
				 for ( TemporalType temporalType : TYPES ) { all.put( temporalType.name, temporalType ); }
			 }
			 private static final long TEMPORAL_TYPE_MASK = 0x00000000F0000000L,
			 private static int getTemporalType( long firstBlock ) { return( int )( ( firstBlock & TEMPORAL_TYPE_MASK ) >> 28 ); } public static int calculateNumberOfBlocksUsed( long firstBlock ) { TemporalType geometryType = find( getTemporalType( firstBlock ) ); return geometryType.calculateNumberOfBlocksUsedForTemporal( firstBlock ); } private static TemporalType find( int temporalType )
			 {
				 if ( temporalType < TYPES.length && temporalType >= 0 ) { return TYPES[temporalType]; } else { return TEMPORAL_INVALID; }
			 }
			 private static void checkValidNanoOfDay( long nanoOfDay )
			 {
				 if ( nanoOfDay > java.time.LocalTime.MAX.toNanoOfDay() || nanoOfDay < java.time.LocalTime.MIN.toNanoOfDay() ) { throw new InvalidRecordException("Nanosecond of day out of range:" + nanoOfDay); }
			 }
			 private static void checkValidNanoOfDayWithOffset( long nanoOfDayUTC, int secondOffset ) { long nanoOfDay = Neo4Net.Values.utils.TemporalUtil.nanosOfDayToLocal( nanoOfDayUTC, secondOffset ); checkValidNanoOfDay( nanoOfDay ); } private static void checkValidNanoOfSecond( long nanoOfSecond )
			 {
				 if ( nanoOfSecond > 999_999_999 || nanoOfSecond < 0 ) { throw new InvalidRecordException( "Nanosecond of second out of range:" + nanoOfSecond ); }
			 }
			 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.impl.store.record.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int temporalType = getTemporalType(firstBlock); return find(temporalType).decodeForTemporal(valueBlocks, offset); } public static long[] encodeDate(int keyId, long epochDay) { return encodeLong(keyId, epochDay, TemporalType.TEMPORAL_DATE.temporalType); } public static long[] encodeLocalTime(int keyId, long nanoOfDay) { return encodeLong(keyId, nanoOfDay, TemporalType.TEMPORAL_LOCAL_TIME.temporalType); } private static long[] encodeLong(int keyId, long val, int temporalType)
			 {
				 int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = temporalType << (idBits + 4); long[] data; if (ShortArray.LONG.getRequiredBits(val) <= 64 - 33) { data = new long[BLOCKS_LONG_INLINED]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (val << 33); } else { data = new long[BLOCKS_LONG_NOT_INLINED]; data[0] = keyAndType | temporalTypeBits; data[1] = val; } return data;
			 }
			 public static long[] encodeLocalDateTime( int keyId, long epochSecond, long nanoOfSecond ) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | ( ( ( long )( PropertyType.TEMPORAL.intValue() ) << idBits ) ); long temporalTypeBits = TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_LOCAL_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 32); data[1] = epochSecond; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, String zoneId) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(zoneId); long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = zoneNumber; return data; } public static long[] encodeDateTime(int keyId, long epochSecond, long nanoOfSecond, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DATE_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DATETIME]; data[0] = keyAndType | temporalTypeBits | (1L << 32) | (nanoOfSecond << 33); data[1] = epochSecond; data[2] = secondOffset; return data; } public static long[] encodeTime(int keyId, long nanoOfDayUTC, int secondOffset) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_TIME.temporalType << (idBits + 4); long[] data = new long[BLOCKS_TIME]; data[0] = keyAndType | temporalTypeBits | ((long) secondOffset << 32); data[1] = nanoOfDayUTC; return data; } public static long[] encodeDuration(int keyId, long months, long days, long seconds, int nanos) { int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.TEMPORAL.intValue()) << idBits)); long temporalTypeBits = TemporalType.TEMPORAL_DURATION.temporalType << (idBits + 4); long[] data = new long[BLOCKS_DURATION]; data[0] = keyAndType | temporalTypeBits | ((long) nanos << 32); data[1] = months; data[2] = days; data[3] = seconds; return data; } public static byte[] encodeDateArray(java.time.LocalDate[] dates)
			 {
				 long[] data = new long[dates.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = dates[i].toEpochDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_DATE.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeLocalTimeArray( java.time.LocalTime[] times )
			 {
				 long[] data = new long[times.length]; for ( int i = 0; i < data.length; i++ ) { data[i] = times[i].toNanoOfDay(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeLocalDateTimeArray( java.time.LocalDateTime[] dateTimes )
			 {
				 long[] data = new long[dateTimes.length * BLOCKS_LOCAL_DATETIME]; for ( int i = 0; i < dateTimes.length; i++ ) { data[i * BLOCKS_LOCAL_DATETIME] = dateTimes[i].toEpochSecond( UTC ); data[i * BLOCKS_LOCAL_DATETIME + 1] = dateTimes[i].getNano(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_LOCAL_DATE_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeTimeArray( java.time.OffsetTime[] times )
			 {
				 long[] data = new long[( int )( Math.ceil( times.length * BLOCKS_TIME ) )]; int i; for ( i = 0; i < times.length; i++ ) { data[i * BLOCKS_TIME] = times[i].toLocalTime().toNanoOfDay(); data[i * BLOCKS_TIME + 1] = times[i].getOffset().getTotalSeconds(); } TemporalHeader header = new TemporalHeader(TemporalType.TEMPORAL_TIME.temporalType); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.TEMPORAL_HEADER_SIZE); header.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static byte[] encodeDateTimeArray( java.time.ZonedDateTime[] dateTimes )
			 {
				 long[] data = new long[dateTimes.length * BLOCKS_DATETIME]; int i; for ( i = 0; i < dateTimes.length; i++ )
				 {
					 data[i * BLOCKS_DATETIME] = dateTimes[i].toEpochSecond(); data[i * BLOCKS_DATETIME + 1] = dateTimes[i].getNano(); if (dateTimes[i].getZone() instanceof java.time.ZoneOffset) { java.time.ZoneOffset offset = (java.time.ZoneOffset) dateTimes[i].getZone(); int secondOffset = offset.getTotalSeconds(); data[i * BLOCKS_DATETIME + 2] = secondOffset << 1 | 1L; } else { String timeZoneId = dateTimes[i].getZone().getId(); short zoneNumber = Neo4Net.Values.Storable.TimeZones.map(timeZoneId); data[i * BLOCKS_DATETIME + 2] = zoneNumber << 1; }
				 }
				 TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DATE_TIME.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
			 }
			 public static byte[] encodeDurationArray( Neo4Net.Values.Storable.DurationValue[] durations )
			 {
				 long[] data = new long[durations.length * BLOCKS_DURATION]; for ( int i = 0; i < durations.length; i++ ) { data[i * BLOCKS_DURATION] = durations[i].get( java.time.temporal.ChronoUnit.MONTHS ); data[i * BLOCKS_DURATION + 1] = durations[i].get( java.time.temporal.ChronoUnit.DAYS ); data[i * BLOCKS_DURATION + 2] = durations[i].get( java.time.temporal.ChronoUnit.SECONDS ); data[i * BLOCKS_DURATION + 3] = durations[i].get( java.time.temporal.ChronoUnit.NANOS ); } TemporalHeader header = new TemporalHeader( TemporalType.TEMPORAL_DURATION.temporalType ); byte[] bytes = DynamicArrayStore.encodeFromNumbers( data, DynamicArrayStore.TEMPORAL_HEADER_SIZE ); header.writeArrayHeaderTo( bytes ); return bytes;
			 }
			 public static Neo4Net.Values.Storable.ArrayValue decodeTemporalArray( TemporalHeader header, byte[] data ) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader( data ); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy( data, dataHeader.length, dataBody, 0, dataBody.length ); Neo4Net.Values.Storable.Value dataValue = DynamicArrayStore.getRightArray( Neo4Net.Helpers.Collection.Pair.of( dataHeader, dataBody ) ); return find( header.temporalType ).decodeArray( dataValue ); } private final int temporalType
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private readonly string name;

		 TemporalType( int temporalType, string name ) { this.temporalType = temporalType; this.name = name; } public abstract Neo4Net.Values.Storable.Value decodeForTemporal( long[] valueBlocks, int offset );

		 public abstract int calculateNumberOfBlocksUsedForTemporal( long firstBlock );

		 public abstract Neo4Net.Values.Storable.ArrayValue decodeArray( Neo4Net.Values.Storable.Value dataValue );

		 public static readonly TemporalType public String getName() { return name; } = new TemporalType("public String getName() { return name; }", InnerEnum.public String getName() { return name; });

		public static IList<TemporalType> values()
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

		public static TemporalType valueOf( string name )
		{
			foreach ( TemporalType enumInstance in TemporalType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}