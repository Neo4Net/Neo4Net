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

	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public abstract class ShortArray
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BOOLEAN(PropertyType.BOOL, 1, Boolean.class, boolean.class) { int getRequiredBits(Object array, int arrayLength) { return 1; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(boolean value : (boolean[]) array) { result.put(value ? 1 : 0, 1); } } else { for(boolean value : (System.Nullable<bool>[]) array) { result.put(value ? 1 : 0, 1); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_BOOLEAN_ARRAY; } final boolean[] result = new boolean[length]; for(int i = 0; i < length; i++) { result[i] = bits.getByte(requiredBits) != 0; } return org.neo4j.values.storable.Values.booleanArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_BOOLEAN_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BYTE(PropertyType.BYTE, 8, Byte.class, byte.class) { int getRequiredBits(byte value) { long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & value) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(byte value : (byte[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(byte value : (System.Nullable<sbyte>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(byte b : (byte[]) array) { result.put(b, requiredBits); } } else { for(byte b : (System.Nullable<sbyte>[]) array) { result.put(b, requiredBits); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_BYTE_ARRAY; } final byte[] result = new byte[length]; for(int i = 0; i < length; i++) { result[i] = bits.getByte(requiredBits); } return org.neo4j.values.storable.Values.byteArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_BYTE_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SHORT(PropertyType.SHORT, 16, Short.class, short.class) { int getRequiredBits(short value) { long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & value) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(short value : (short[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(short value : (System.Nullable<short>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(short value : (short[]) array) { result.put(value, requiredBits); } } else { for(short value : (System.Nullable<short>[]) array) { result.put(value, requiredBits); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_SHORT_ARRAY; } final short[] result = new short[length]; for(int i = 0; i < length; i++) { result[i] = bits.getShort(requiredBits); } return org.neo4j.values.storable.Values.shortArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_SHORT_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CHAR(PropertyType.CHAR, 16, Character.class, char.class) { int getRequiredBits(char value) { long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & value) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(char value : (char[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(char value : (System.Nullable<char>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(char value : (char[]) array) { result.put(value, requiredBits); } } else { for(char value : (System.Nullable<char>[]) array) { result.put(value, requiredBits); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_CHAR_ARRAY; } final char[] result = new char[length]; for(int i = 0; i < length; i++) { result[i] = (char) bits.getShort(requiredBits); } return org.neo4j.values.storable.Values.charArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_CHAR_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INT(PropertyType.INT, 32, Integer.class, int.class) { int getRequiredBits(int value) { long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & value) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(int value : (int[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(int value : (System.Nullable<int>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(int value : (int[]) array) { result.put(value, requiredBits); } } else { for(int value : (System.Nullable<int>[]) array) { result.put(value, requiredBits); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_INT_ARRAY; } final int[] result = new int[length]; for(int i = 0; i < length; i++) { result[i] = bits.getInt(requiredBits); } return org.neo4j.values.storable.Values.intArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_INT_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LONG(PropertyType.LONG, 64, Long.class, long.class) { public int getRequiredBits(long value) { long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & value) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(long value : (long[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(long value : (System.Nullable<long>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(long value : (long[]) array) { result.put(value, requiredBits); } } else { for(long value : (System.Nullable<long>[]) array) { result.put(value, requiredBits); } } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_LONG_ARRAY; } final long[] result = new long[length]; for(int i = 0; i < length; i++) { result[i] = bits.getLong(requiredBits); } return org.neo4j.values.storable.Values.longArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_LONG_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FLOAT(PropertyType.FLOAT, 32, Float.class, float.class) { int getRequiredBits(float value) { int v = Float.floatToIntBits(value); long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & v) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(float value : (float[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(float value : (System.Nullable<float>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(float value : (float[]) array) { result.put(Float.floatToIntBits(value), requiredBits); } } else { for(float value : (System.Nullable<float>[]) array) { result.put(Float.floatToIntBits(value), requiredBits); } } } public void writeAll(Object array, byte[] result, int offset) { if(isPrimitive(array)) { float[] values = (float[]) array; for(int i = 0; i < values.length; i++) { writeFloat(values[i], result, offset + i * 4); } } else { Float[] values = (System.Nullable<float>[]) array; for(int i = 0; i < values.length; i++) { writeFloat(values[i], result, offset + i * 4); } } } private void writeFloat(float floaValue, byte[] result, int offset) { long value = Float.floatToIntBits(floaValue); for(int b = 0; b < 4; b++) { result[offset + b] = (byte)((value >> (b * 8)) & 0xFFL); } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_FLOAT_ARRAY; } final float[] result = new float[length]; for(int i = 0; i < length; i++) { result[i] = Float.intBitsToFloat(bits.getInt(requiredBits)); } return org.neo4j.values.storable.Values.floatArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_FLOAT_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DOUBLE(PropertyType.DOUBLE, 64, Double.class, double.class) { int getRequiredBits(double value) { long v = Double.doubleToLongBits(value); long mask = 1L << maxBits - 1; for(int i = maxBits; i > 0; i--, mask >>= 1) { if((mask & v) != 0) { return i; } } return 1; } int getRequiredBits(Object array, int arrayLength) { int highest = 1; if(isPrimitive(array)) { for(double value : (double[]) array) { highest = Math.max(getRequiredBits(value), highest); } } else { for(double value : (System.Nullable<double>[]) array) { highest = Math.max(getRequiredBits(value), highest); } } return highest; } public void writeAll(Object array, int length, int requiredBits, org.neo4j.kernel.impl.util.Bits result) { if(isPrimitive(array)) { for(double value : (double[]) array) { result.put(Double.doubleToLongBits(value), requiredBits); } } else { for(double value : (System.Nullable<double>[]) array) { result.put(Double.doubleToLongBits(value), requiredBits); } } } public void writeAll(Object array, byte[] result, int offset) { if(isPrimitive(array)) { double[] values = (double[]) array; for(int i = 0; i < values.length; i++) { writeDouble(values[i], result, offset + i * 8); } } else { Double[] values = (System.Nullable<double>[]) array; for(int i = 0; i < values.length; i++) { writeDouble(values[i], result, offset + i * 8); } } } private void writeDouble(double doubleValue, byte[] result, int offset) { long value = Double.doubleToLongBits(doubleValue); for(int b = 0; b < 8; b++) { result[offset + b] = (byte)((value >> (b * 8)) & 0xFFL); } } public org.neo4j.values.storable.ArrayValue createArray(int length, org.neo4j.kernel.impl.util.Bits bits, int requiredBits) { if(length == 0) { return org.neo4j.values.storable.Values.EMPTY_DOUBLE_ARRAY; } final double[] result = new double[length]; for(int i = 0; i < length; i++) { result[i] = Double.longBitsToDouble(bits.getLong(requiredBits)); } return org.neo4j.values.storable.Values.doubleArray(result); } public org.neo4j.values.storable.ArrayValue createEmptyArray() { return org.neo4j.values.storable.Values.EMPTY_DOUBLE_ARRAY; } };

		 private static readonly IList<ShortArray> valueList = new List<ShortArray>();

		 static ShortArray()
		 {
			 valueList.Add( BOOLEAN );
			 valueList.Add( BYTE );
			 valueList.Add( SHORT );
			 valueList.Add( CHAR );
			 valueList.Add( INT );
			 valueList.Add( LONG );
			 valueList.Add( FLOAT );
			 valueList.Add( DOUBLE );
		 }

		 public enum InnerEnum
		 {
			 BOOLEAN,
			 BYTE,
			 SHORT,
			 CHAR,
			 INT,
			 LONG,
			 FLOAT,
			 DOUBLE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private ShortArray( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static final ShortArray private static bool isPrimitive( object array ) { return array.GetType().GetElementType().Primitive; } private static final ShortArray[] TYPES = ShortArray.values = new ShortArray("private static boolean isPrimitive(Object array) { return array.getClass().getComponentType().isPrimitive(); } private static final ShortArray[] TYPES = ShortArray.values", InnerEnum.private static bool isPrimitive(object array) { return array.GetType().GetElementType().Primitive; } private static final ShortArray[] TYPES = ShortArray.values, );

		 private static readonly IList<ShortArray> valueList = new List<ShortArray>();

		 static ShortArray()
		 {
			 valueList.Add( BOOLEAN );
			 valueList.Add( BYTE );
			 valueList.Add( SHORT );
			 valueList.Add( CHAR );
			 valueList.Add( INT );
			 valueList.Add( LONG );
			 valueList.Add( FLOAT );
			 valueList.Add( DOUBLE );
			 valueList.Add( private static boolean isPrimitive( Object array ) { return array.getClass().getComponentType().isPrimitive(); } private static final ShortArray[] TYPES = ShortArray.values );
		 }

		 public enum InnerEnum
		 {
			 BOOLEAN,
			 BYTE,
			 SHORT,
			 CHAR,
			 INT,
			 LONG,
			 FLOAT,
			 DOUBLE,
			 private static boolean isPrimitive( Object array ) { return array.getClass().getComponentType().isPrimitive(); } private static final ShortArray[] TYPES = ShortArray.values
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private static readonly IDictionary<Type, ShortArray> all = new java.util.IdentityHashMap<Type, ShortArray>( TYPES.length * 2 );

		 public static readonly ShortArray static
		 {
			 for ( ShortArray shortArray : TYPES ) { all.put( shortArray.primitiveClass, shortArray ); all.put( shortArray.boxedClass, shortArray ); }
		 }
		 final int maxBits = new ShortArray("static { for(ShortArray shortArray : TYPES) { all.put(shortArray.primitiveClass, shortArray); all.put(shortArray.boxedClass, shortArray); } } final int maxBits", InnerEnum.static
		 {
			 for ( ShortArray shortArray : TYPES ) { all.put( shortArray.primitiveClass, shortArray ); all.put( shortArray.boxedClass, shortArray ); }
		 }
		 final int maxBits);

		 private static readonly IList<ShortArray> valueList = new List<ShortArray>();

		 static ShortArray()
		 {
			 valueList.Add( BOOLEAN );
			 valueList.Add( BYTE );
			 valueList.Add( SHORT );
			 valueList.Add( CHAR );
			 valueList.Add( INT );
			 valueList.Add( LONG );
			 valueList.Add( FLOAT );
			 valueList.Add( DOUBLE );
			 valueList.Add( private static boolean isPrimitive( Object array ) { return array.getClass().getComponentType().isPrimitive(); } private static final ShortArray[] TYPES = ShortArray.values );
			 valueList.Add(static
			 {
				 for ( ShortArray shortArray : TYPES ) { all.put( shortArray.primitiveClass, shortArray ); all.put( shortArray.boxedClass, shortArray ); }
			 }
			 final int maxBits);
		 }

		 public enum InnerEnum
		 {
			 BOOLEAN,
			 BYTE,
			 SHORT,
			 CHAR,
			 INT,
			 LONG,
			 FLOAT,
			 DOUBLE,
			 private static boolean isPrimitive( Object array ) { return array.getClass().getComponentType().isPrimitive(); } private static final ShortArray[] TYPES = ShortArray.values,
			 static
			 {
				 for ( ShortArray shortArray : TYPES ) { all.put( shortArray.primitiveClass, shortArray ); all.put( shortArray.boxedClass, shortArray ); }
			 }
			 final int maxBits
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly Type boxedClass;
		 private readonly Type primitiveClass;
		 private readonly PropertyType type;

		 internal ShortArray( string name, InnerEnum innerEnum, PropertyType type, int maxBits, Type boxedClass, Type primitiveClass )
		 {
			  this._type = type;
			  this.maxBits = maxBits;
			  this._boxedClass = boxedClass;
			  this._primitiveClass = primitiveClass;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public int IntValue()
		 {
			  return _type.intValue();
		 }

		 public abstract Neo4Net.Values.Storable.ArrayValue createArray( int length, Neo4Net.Kernel.impl.util.Bits bits, int requiredBits );

		 public static bool Encode( int keyId, object array, Neo4Net.Kernel.impl.store.record.PropertyBlock target, int payloadSizeInBytes )
		 {
			  /*
			   *  If the array is huge, we don't have to check anything else.
			   *  So do the length check first.
			   */
			  int arrayLength = Array.getLength( array );
			  if ( arrayLength > 63 ) //because we only use 6 bits for length
			  {
					return false;
			  }

			  ShortArray type = TypeOf( array );
			  if ( type == null )
			  {
					return false;
			  }

			  int requiredBits = type.CalculateRequiredBitsForArray( array, arrayLength );
			  if ( !WillFit( requiredBits, arrayLength, payloadSizeInBytes ) )
			  {
					// Too big array
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numberOfBytes = calculateNumberOfBlocksUsed(arrayLength, requiredBits) * 8;
			  int numberOfBytes = CalculateNumberOfBlocksUsed( arrayLength, requiredBits ) * 8;
			  if ( Bits.requiredLongs( numberOfBytes ) > PropertyType.PayloadSizeLongs )
			  {
					return false;
			  }
			  Bits result = Bits.bits( numberOfBytes );
			  // [][][    ,bbbb][bbll,llll][yyyy,tttt][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			  WriteHeader( keyId, type, arrayLength, requiredBits, result );
			  type.WriteAll( array, arrayLength, requiredBits, result );
			  target.ValueBlocks = result.Longs;
			  return true;
		 }

		 private static void WriteHeader( int keyId, ShortArray type, int arrayLength, int requiredBits, Neo4Net.Kernel.impl.util.Bits result )
		 {
			  result.Put( keyId, 24 );
			  result.put( PropertyType.ShortArray.intValue(), 4 );
			  result.put( type._type.intValue(), 4 );
			  result.Put( arrayLength, 6 );
			  result.Put( requiredBits, 6 );
		 }

		 public static Neo4Net.Values.Storable.Value Decode( Neo4Net.Kernel.impl.store.record.PropertyBlock block )
		 {
			  Bits bits = Bits.bitsFromLongs( Arrays.copyOf( block.ValueBlocks, block.ValueBlocks.Length ) );
			  return Decode( bits );
		 }

		 public static Neo4Net.Values.Storable.Value Decode( Neo4Net.Kernel.impl.util.Bits bits )
		 {
			  // [][][    ,bbbb][bbll,llll][yyyy,tttt][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			  bits.GetInt( 24 ); // Get rid of key
			  bits.GetByte( 4 ); // Get rid of short array type
			  int typeId = bits.GetByte( 4 );
			  int arrayLength = bits.GetByte( 6 );
			  int requiredBits = bits.GetByte( 6 );
			  /*
			   * So, it can be the case that values require 64 bits to store. However, you cannot encode this
			   * value with 6 bits. calculateRequiredBitsForArray never returns 0, because even for an array of
			   * all 0s one bit is required for every value. So when writing, we let it overflow and write out
			   * 0. When we are reading back, we just have to make sure that reading in 0 means 64.
			   */
			  if ( requiredBits == 0 )
			  {
					requiredBits = 64;
			  }
			  ShortArray type = TypeOf( ( sbyte )typeId );
			  return type.CreateArray( arrayLength, bits, requiredBits );
		 }

		 private static bool WillFit( int requiredBits, int arrayLength, int payloadSizeInBytes )
		 {
			  int totalBitsRequired = requiredBits * arrayLength;
			  int maxBits = payloadSizeInBytes * 8 - 24 - 4 - 4 - 6 - 6;
			  return totalBitsRequired <= maxBits;
		 }

		 public int CalculateRequiredBitsForArray( object array, int arrayLength )
		 {
			  if ( arrayLength == 0 )
			  {
					return 0;
			  }
			  // return getRequiredBits(findBiggestValue(array, arrayLength));
			  return GetRequiredBits( array, arrayLength );
		 }

		 public int GetRequiredBits( long value )
		 {
			  int highest = 1;
			  long mask = 1;
			  for ( int i = 1; i <= maxBits; i++, mask <<= 1 )
			  {
					if ( ( mask & value ) != 0 )
					{
						 highest = i;
					}
			  }
			  return highest;
		 }

		 internal abstract int getRequiredBits( object array, int arrayLength );

		 public static ShortArray TypeOf( sbyte typeId )
		 {
			  return TYPES[typeId - 1];
		 }

		 public static ShortArray TypeOf( object array )
		 {
			  return ShortArray.all.get( array.GetType().GetElementType() );
		 }

		 public static int CalculateNumberOfBlocksUsed( long firstBlock )
		 {
			  // inside the high 4B of the first block of a short array sits the header
			  int highInt = ( int )( ( long )( ( ulong )firstBlock >> 32 ) );
			  // bits 32-37 contains number of items (length)
			  int arrayLength = highInt & 0b11_1111;
			  highInt = ( int )( ( uint )highInt >> 6 );
			  // bits 38-43 contains number of requires bits per item
			  int requiredBits = highInt & 0b11_1111;
			  // no values can be represented by 0 bits, so we use that value for 64 instead
			  if ( requiredBits == 0 )
			  {
					requiredBits = 64;
			  }
			  return CalculateNumberOfBlocksUsed( arrayLength, requiredBits );
		 }

		 public static int CalculateNumberOfBlocksUsed( int arrayLength, int requiredBits )
		 {
			  int bitsForItems = arrayLength * requiredBits;
			  /*
			   * Key, Property Type (ARRAY), Array Type, Array Length, Bits Per Member, Data
			   */
			  int totalBits = 24 + 4 + 4 + 6 + 6 + bitsForItems;
			  int result = ( totalBits - 1 ) / 64 + 1;
			  return result;
		 }

		 public abstract void writeAll( object array, int length, int requiredBits, Neo4Net.Kernel.impl.util.Bits result );

		 public void WriteAll( object array, sbyte[] result, int offset )
		 {
			  throw new System.InvalidOperationException( "Types that skip bit compaction should implement this method" );
		 }

		 public abstract Neo4Net.Values.Storable.ArrayValue createEmptyArray();

		public static IList<ShortArray> values()
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

		public static ShortArray valueOf( string name )
		{
			foreach ( ShortArray enumInstance in ShortArray.valueList )
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