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
namespace Neo4Net.@unsafe.Impl.@internal.Dragons
{
	using Test = org.junit.jupiter.api.Test;


	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.allocateMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.arrayBaseOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.arrayIndexScale;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.arrayOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.assertHasUnsafe;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.compareAndSetMaxLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.compareAndSwapLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.compareAndSwapObject;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.free;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getAndAddInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getAndSetLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getAndSetObject;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getBooleanVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getByteVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getChar;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getCharVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getDouble;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getDoubleVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getFieldOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getFloatVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getIntVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getLongVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getObject;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getObjectVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getShortVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.initDirectByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.newDirectByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.pageSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putBooleanVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putByteVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putChar;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putCharVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putDouble;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putDoubleVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putFloatVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putIntVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putLongVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putObject;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putObjectVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putShortVolatile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.setMemory;

	internal class UnsafeUtilTest
	{
		 internal class Obj
		 {
			  internal bool ABoolean;
			  internal sbyte AByte;
			  internal short AShort;
			  internal float AFloat;
			  internal char AChar;
			  internal int AnInt;
			  internal long ALong;
			  internal double ADouble;
			  internal object Object;

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					Obj obj = ( Obj ) o;
					return ABoolean == obj.ABoolean && AByte == obj.AByte && AShort == obj.AShort && obj.AFloat.CompareTo( AFloat ) == 0 && AChar == obj.AChar && AnInt == obj.AnInt && ALong == obj.ALong && obj.ADouble.CompareTo( ADouble ) == 0 && Objects.Equals( Object, obj.Object );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( ABoolean, AByte, AShort, AFloat, AChar, AnInt, ALong, ADouble, Object );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustHaveUnsafe()
		 internal virtual void MustHaveUnsafe()
		 {
			  assertHasUnsafe();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageSizeIsPowerOfTwo()
		 internal virtual void PageSizeIsPowerOfTwo()
		 {
			  assertThat( pageSize(), isOneOf(1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustSupportReadingFromAndWritingToFields()
		 internal virtual void MustSupportReadingFromAndWritingToFields()
		 {
			  Obj obj;

			  long aBooleanOffset = getFieldOffset( typeof( Obj ), "aBoolean" );
			  obj = new Obj();
			  putBoolean( obj, aBooleanOffset, true );
			  assertThat( obj.ABoolean, @is( true ) );
			  assertThat( getBoolean( obj, aBooleanOffset ), @is( true ) );
			  obj.ABoolean = false;
			  assertThat( obj, @is( new Obj() ) );
			  putBooleanVolatile( obj, aBooleanOffset, true );
			  assertThat( obj.ABoolean, @is( true ) );
			  assertThat( getBooleanVolatile( obj, aBooleanOffset ), @is( true ) );
			  obj.ABoolean = false;
			  assertThat( obj, @is( new Obj() ) );

			  long aByteOffset = getFieldOffset( typeof( Obj ), "aByte" );
			  obj = new Obj();
			  putByte( obj, aByteOffset, ( sbyte ) 1 );
			  assertThat( obj.AByte, @is( ( sbyte ) 1 ) );
			  assertThat( getByte( obj, aByteOffset ), @is( ( sbyte ) 1 ) );
			  obj.AByte = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putByteVolatile( obj, aByteOffset, ( sbyte ) 2 );
			  assertThat( obj.AByte, @is( ( sbyte ) 2 ) );
			  assertThat( getByteVolatile( obj, aByteOffset ), @is( ( sbyte ) 2 ) );
			  obj.AByte = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long aShortOffset = getFieldOffset( typeof( Obj ), "aShort" );
			  obj = new Obj();
			  putShort( obj, aShortOffset, ( sbyte ) 1 );
			  assertThat( obj.AShort, @is( ( short ) 1 ) );
			  assertThat( getShort( obj, aShortOffset ), @is( ( short ) 1 ) );
			  obj.AShort = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putShortVolatile( obj, aShortOffset, ( short ) 2 );
			  assertThat( obj.AShort, @is( ( short ) 2 ) );
			  assertThat( getShortVolatile( obj, aShortOffset ), @is( ( short ) 2 ) );
			  obj.AShort = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long aFloatOffset = getFieldOffset( typeof( Obj ), "aFloat" );
			  obj = new Obj();
			  putFloat( obj, aFloatOffset, 1 );
			  assertThat( obj.AFloat, @is( ( float ) 1 ) );
			  assertThat( getFloat( obj, aFloatOffset ), @is( ( float ) 1 ) );
			  obj.AFloat = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putFloatVolatile( obj, aFloatOffset, 2 );
			  assertThat( obj.AFloat, @is( ( float ) 2 ) );
			  assertThat( getFloatVolatile( obj, aFloatOffset ), @is( ( float ) 2 ) );
			  obj.AFloat = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long aCharOffset = getFieldOffset( typeof( Obj ), "aChar" );
			  obj = new Obj();
			  putChar( obj, aCharOffset, '1' );
			  assertThat( obj.AChar, @is( '1' ) );
			  assertThat( getChar( obj, aCharOffset ), @is( '1' ) );
			  obj.AChar = ( char )0;
			  assertThat( obj, @is( new Obj() ) );
			  putCharVolatile( obj, aCharOffset, '2' );
			  assertThat( obj.AChar, @is( '2' ) );
			  assertThat( getCharVolatile( obj, aCharOffset ), @is( '2' ) );
			  obj.AChar = ( char )0;
			  assertThat( obj, @is( new Obj() ) );

			  long anIntOffset = getFieldOffset( typeof( Obj ), "anInt" );
			  obj = new Obj();
			  putInt( obj, anIntOffset, 1 );
			  assertThat( obj.AnInt, @is( 1 ) );
			  assertThat( getInt( obj, anIntOffset ), @is( 1 ) );
			  obj.AnInt = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putIntVolatile( obj, anIntOffset, 2 );
			  assertThat( obj.AnInt, @is( 2 ) );
			  assertThat( getIntVolatile( obj, anIntOffset ), @is( 2 ) );
			  obj.AnInt = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long aLongOffset = getFieldOffset( typeof( Obj ), "aLong" );
			  obj = new Obj();
			  putLong( obj, aLongOffset, 1 );
			  assertThat( obj.ALong, @is( 1L ) );
			  assertThat( getLong( obj, aLongOffset ), @is( 1L ) );
			  obj.ALong = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putLongVolatile( obj, aLongOffset, 2 );
			  assertThat( obj.ALong, @is( 2L ) );
			  assertThat( getLongVolatile( obj, aLongOffset ), @is( 2L ) );
			  obj.ALong = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long aDoubleOffset = getFieldOffset( typeof( Obj ), "aDouble" );
			  obj = new Obj();
			  putDouble( obj, aDoubleOffset, 1 );
			  assertThat( obj.ADouble, @is( ( double ) 1 ) );
			  assertThat( getDouble( obj, aDoubleOffset ), @is( ( double ) 1 ) );
			  obj.ADouble = 0;
			  assertThat( obj, @is( new Obj() ) );
			  putDoubleVolatile( obj, aDoubleOffset, 2 );
			  assertThat( obj.ADouble, @is( ( double ) 2 ) );
			  assertThat( getDoubleVolatile( obj, aDoubleOffset ), @is( ( double ) 2 ) );
			  obj.ADouble = 0;
			  assertThat( obj, @is( new Obj() ) );

			  long objectOffset = getFieldOffset( typeof( Obj ), "object" );
			  obj = new Obj();
			  object a = new object();
			  object b = new object();
			  putObject( obj, objectOffset, a );
			  assertThat( obj.Object, @is( a ) );
			  assertThat( getObject( obj, objectOffset ), @is( a ) );
			  obj.Object = null;
			  assertThat( obj, @is( new Obj() ) );
			  putObjectVolatile( obj, objectOffset, b );
			  assertThat( obj.Object, @is( b ) );
			  assertThat( getObjectVolatile( obj, objectOffset ), @is( b ) );
			  obj.Object = null;
			  assertThat( obj, @is( new Obj() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustSupportReadingAndWritingOfPrimitivesToMemory()
		 internal virtual void MustSupportReadingAndWritingOfPrimitivesToMemory()
		 {
			  int sizeInBytes = 8;
			  long address = allocateMemory( sizeInBytes );
			  try
			  {
					putByte( address, ( sbyte ) 1 );
					assertThat( getByte( address ), @is( ( sbyte ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getByte( address ), @is( ( sbyte ) 0 ) );

					putByteVolatile( address, ( sbyte ) 1 );
					assertThat( getByteVolatile( address ), @is( ( sbyte ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getByteVolatile( address ), @is( ( sbyte ) 0 ) );

					putShort( address, ( short ) 1 );
					assertThat( getShort( address ), @is( ( short ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getShort( address ), @is( ( short ) 0 ) );

					putShortVolatile( address, ( short ) 1 );
					assertThat( getShortVolatile( address ), @is( ( short ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getShortVolatile( address ), @is( ( short ) 0 ) );

					putFloat( address, 1 );
					assertThat( getFloat( address ), @is( ( float ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getFloat( address ), @is( ( float ) 0 ) );

					putFloatVolatile( address, 1 );
					assertThat( getFloatVolatile( address ), @is( ( float ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getFloatVolatile( address ), @is( ( float ) 0 ) );

					putChar( address, '1' );
					assertThat( getChar( address ), @is( '1' ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getChar( address ), @is( ( char ) 0 ) );

					putCharVolatile( address, '1' );
					assertThat( getCharVolatile( address ), @is( '1' ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getCharVolatile( address ), @is( ( char ) 0 ) );

					putInt( address, 1 );
					assertThat( getInt( address ), @is( 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getInt( address ), @is( 0 ) );

					putIntVolatile( address, 1 );
					assertThat( getIntVolatile( address ), @is( 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getIntVolatile( address ), @is( 0 ) );

					putLong( address, 1 );
					assertThat( getLong( address ), @is( 1L ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getLong( address ), @is( 0L ) );

					putLongVolatile( address, 1 );
					assertThat( getLongVolatile( address ), @is( 1L ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getLongVolatile( address ), @is( 0L ) );

					putDouble( address, 1 );
					assertThat( getDouble( address ), @is( ( double ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getDouble( address ), @is( ( double ) 0 ) );

					putDoubleVolatile( address, 1 );
					assertThat( getDoubleVolatile( address ), @is( ( double ) 1 ) );
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					assertThat( getDoubleVolatile( address ), @is( ( double ) 0 ) );
			  }
			  finally
			  {
					free( address, sizeInBytes, GlobalMemoryTracker.INSTANCE );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getAndAddIntOfField()
		 internal virtual void getAndAddIntOfField()
		 {
			  Obj obj = new Obj();
			  long anIntOffset = getFieldOffset( typeof( Obj ), "anInt" );
			  assertThat( getAndAddInt( obj, anIntOffset, 3 ), @is( 0 ) );
			  assertThat( getAndAddInt( obj, anIntOffset, 2 ), @is( 3 ) );
			  assertThat( obj.AnInt, @is( 5 ) );
			  obj.AnInt = 0;
			  assertThat( obj, @is( new Obj() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void compareAndSwapLongField()
		 internal virtual void CompareAndSwapLongField()
		 {
			  Obj obj = new Obj();
			  long aLongOffset = getFieldOffset( typeof( Obj ), "aLong" );
			  assertTrue( compareAndSwapLong( obj, aLongOffset, 0, 5 ) );
			  assertFalse( compareAndSwapLong( obj, aLongOffset, 0, 5 ) );
			  assertTrue( compareAndSwapLong( obj, aLongOffset, 5, 0 ) );
			  assertThat( obj, @is( new Obj() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void compareAndSwapObjectField()
		 internal virtual void CompareAndSwapObjectField()
		 {
			  Obj obj = new Obj();
			  long objectOffset = getFieldOffset( typeof( Obj ), "object" );
			  assertTrue( compareAndSwapObject( obj, objectOffset, null, obj ) );
			  assertFalse( compareAndSwapObject( obj, objectOffset, null, obj ) );
			  assertTrue( compareAndSwapObject( obj, objectOffset, obj, null ) );
			  assertThat( obj, @is( new Obj() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getAndSetObjectField()
		 internal virtual void getAndSetObjectField()
		 {
			  Obj obj = new Obj();
			  long objectOffset = getFieldOffset( typeof( Obj ), "object" );
			  assertThat( getAndSetObject( obj, objectOffset, obj ), @is( nullValue() ) );
			  assertThat( getAndSetObject( obj, objectOffset, null ), sameInstance( obj ) );
			  assertThat( obj, @is( new Obj() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getAndSetLongField()
		 internal virtual void getAndSetLongField()
		 {
			  Obj obj = new Obj();
			  long offset = getFieldOffset( typeof( Obj ), "aLong" );
			  assertThat( getAndSetLong( obj, offset, 42L ), equalTo( 0L ) );
			  assertThat( getAndSetLong( obj, offset, -1 ), equalTo( 42L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void compareAndSetMaxLongField()
		 internal virtual void CompareAndSetMaxLongField()
		 {
			  Obj obj = new Obj();
			  long offset = getFieldOffset( typeof( Obj ), "aLong" );
			  assertThat( getAndSetLong( obj, offset, 42L ), equalTo( 0L ) );

			  compareAndSetMaxLong( obj, offset, 5 );
			  assertEquals( 42, getLong( obj, offset ) );

			  compareAndSetMaxLong( obj, offset, 105 );
			  assertEquals( 105, getLong( obj, offset ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void unsafeArrayElementAccess()
		 internal virtual void UnsafeArrayElementAccess()
		 {
			  int len = 3;
			  int scale;
			  int @base;

			  bool[] booleans = new bool[len];
			  scale = arrayIndexScale( booleans.GetType() );
			  @base = arrayBaseOffset( booleans.GetType() );
			  putBoolean( booleans, arrayOffset( 1, @base, scale ), true );
			  assertThat( booleans[0], @is( false ) );
			  assertThat( booleans[1], @is( true ) );
			  assertThat( booleans[2], @is( false ) );

			  sbyte[] bytes = new sbyte[len];
			  scale = arrayIndexScale( bytes.GetType() );
			  @base = arrayBaseOffset( bytes.GetType() );
			  putByte( bytes, arrayOffset( 1, @base, scale ), ( sbyte ) - 1 );
			  assertThat( bytes[0], @is( ( sbyte ) 0 ) );
			  assertThat( bytes[1], @is( ( sbyte ) - 1 ) );
			  assertThat( bytes[2], @is( ( sbyte ) 0 ) );

			  short[] shorts = new short[len];
			  scale = arrayIndexScale( shorts.GetType() );
			  @base = arrayBaseOffset( shorts.GetType() );
			  putShort( shorts, arrayOffset( 1, @base, scale ), ( short ) - 1 );
			  assertThat( shorts[0], @is( ( short ) 0 ) );
			  assertThat( shorts[1], @is( ( short ) - 1 ) );
			  assertThat( shorts[2], @is( ( short ) 0 ) );

			  float[] floats = new float[len];
			  scale = arrayIndexScale( floats.GetType() );
			  @base = arrayBaseOffset( floats.GetType() );
			  putFloat( floats, arrayOffset( 1, @base, scale ), -1 );
			  assertThat( floats[0], @is( ( float ) 0 ) );
			  assertThat( floats[1], @is( ( float ) - 1 ) );
			  assertThat( floats[2], @is( ( float ) 0 ) );

			  char[] chars = new char[len];
			  scale = arrayIndexScale( chars.GetType() );
			  @base = arrayBaseOffset( chars.GetType() );
			  putChar( chars, arrayOffset( 1, @base, scale ), ( char ) - 1 );
			  assertThat( chars[0], @is( ( char ) 0 ) );
			  assertThat( chars[1], @is( ( char ) - 1 ) );
			  assertThat( chars[2], @is( ( char ) 0 ) );

			  int[] ints = new int[len];
			  scale = arrayIndexScale( ints.GetType() );
			  @base = arrayBaseOffset( ints.GetType() );
			  putInt( ints, arrayOffset( 1, @base, scale ), -1 );
			  assertThat( ints[0], @is( 0 ) );
			  assertThat( ints[1], @is( -1 ) );
			  assertThat( ints[2], @is( 0 ) );

			  long[] longs = new long[len];
			  scale = arrayIndexScale( longs.GetType() );
			  @base = arrayBaseOffset( longs.GetType() );
			  putLong( longs, arrayOffset( 1, @base, scale ), -1 );
			  assertThat( longs[0], @is( 0L ) );
			  assertThat( longs[1], @is( -1L ) );
			  assertThat( longs[2], @is( 0L ) );

			  double[] doubles = new double[len];
			  scale = arrayIndexScale( doubles.GetType() );
			  @base = arrayBaseOffset( doubles.GetType() );
			  putDouble( doubles, arrayOffset( 1, @base, scale ), -1 );
			  assertThat( doubles[0], @is( ( double ) 0 ) );
			  assertThat( doubles[1], @is( ( double ) - 1 ) );
			  assertThat( doubles[2], @is( ( double ) 0 ) );

			  object[] objects = new object[len];
			  scale = arrayIndexScale( objects.GetType() );
			  @base = arrayBaseOffset( objects.GetType() );
			  putObject( objects, arrayOffset( 1, @base, scale ), objects );
			  assertThat( objects[0], @is( nullValue() ) );
			  assertThat( objects[1], @is( sameInstance( objects ) ) );
			  assertThat( objects[2], @is( nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void directByteBufferCreationAndInitialisation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DirectByteBufferCreationAndInitialisation()
		 {
			  int sizeInBytes = 313;
			  long address = allocateMemory( sizeInBytes );
			  try
			  {
					setMemory( address, sizeInBytes, ( sbyte ) 0 );
					ByteBuffer a = newDirectByteBuffer( address, sizeInBytes );
					assertThat( a, @is( not( sameInstance( newDirectByteBuffer( address, sizeInBytes ) ) ) ) );
					assertThat( a.hasArray(), @is(false) );
					assertThat( a.Direct, @is( true ) );
					assertThat( a.capacity(), @is(sizeInBytes) );
					assertThat( a.limit(), @is(sizeInBytes) );
					assertThat( a.position(), @is(0) );
					assertThat( a.remaining(), @is(sizeInBytes) );
					assertThat( getByte( address ), @is( ( sbyte ) 0 ) );
					a.put( ( sbyte ) - 1 );
					assertThat( getByte( address ), @is( ( sbyte ) - 1 ) );

					a.position( 101 );
					a.mark();
					a.limit( 202 );

					int sizeInBytes2 = 424;
					long address2 = allocateMemory( sizeInBytes2 );
					try
					{
						 setMemory( address2, sizeInBytes2, ( sbyte ) 0 );
						 initDirectByteBuffer( a, address2, sizeInBytes2 );
						 assertThat( a.hasArray(), @is(false) );
						 assertThat( a.Direct, @is( true ) );
						 assertThat( a.capacity(), @is(sizeInBytes2) );
						 assertThat( a.limit(), @is(sizeInBytes2) );
						 assertThat( a.position(), @is(0) );
						 assertThat( a.remaining(), @is(sizeInBytes2) );
						 assertThat( getByte( address2 ), @is( ( sbyte ) 0 ) );
						 a.put( ( sbyte ) - 1 );
						 assertThat( getByte( address2 ), @is( ( sbyte ) - 1 ) );
					}
					finally
					{
						 free( address2, sizeInBytes2, GlobalMemoryTracker.INSTANCE );
					}
			  }
			  finally
			  {
					free( address, sizeInBytes, GlobalMemoryTracker.INSTANCE );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getAddressOfDirectByteBuffer()
		 internal virtual void getAddressOfDirectByteBuffer()
		 {
			  ByteBuffer buf = ByteBuffer.allocateDirect( 8 );
			  long address = UnsafeUtil.GetDirectByteBufferAddress( buf );
			  long expected = ThreadLocalRandom.current().nextLong();
			  // Disable native access checking, because UnsafeUtil doesn't know about the memory allocation in the
			  // ByteBuffer.allocateDirect( … ) call.
			  bool nativeAccessCheckEnabled = UnsafeUtil.ExchangeNativeAccessCheckEnabled( false );
			  try
			  {
					UnsafeUtil.PutLong( address, expected );
					long actual = buf.Long;
					assertThat( actual, isOneOf( expected, Long.reverseBytes( expected ) ) );
			  }
			  finally
			  {
					UnsafeUtil.ExchangeNativeAccessCheckEnabled( nativeAccessCheckEnabled );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAlignMemoryTo4ByteBoundary()
		 internal virtual void ShouldAlignMemoryTo4ByteBoundary()
		 {
			  // GIVEN
			  long allocatedMemory = currentTimeMillis();
			  int alignBy = 4;

			  // WHEN
			  for ( int i = 0; i < 10; i++ )
			  {
					// THEN
					long alignedMemory = UnsafeUtil.AlignedMemory( allocatedMemory, alignBy );
					assertTrue( alignedMemory >= allocatedMemory );
					assertEquals( 0, alignedMemory % Integer.BYTES );
					assertTrue( alignedMemory - allocatedMemory <= 3 );
					allocatedMemory++;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGetByteWiseLittleEndianShort()
		 internal virtual void ShouldPutAndGetByteWiseLittleEndianShort()
		 {
			  // GIVEN
			  int sizeInBytes = 2;
			  GlobalMemoryTracker tracker = GlobalMemoryTracker.INSTANCE;
			  long p = allocateMemory( sizeInBytes, tracker );
			  short value = ( short ) 0b11001100_10101010;

			  // WHEN
			  UnsafeUtil.PutShortByteWiseLittleEndian( p, value );
			  short readValue = UnsafeUtil.GetShortByteWiseLittleEndian( p );

			  // THEN
			  free( p, sizeInBytes, tracker );
			  assertEquals( value, readValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGetByteWiseLittleEndianInt()
		 internal virtual void ShouldPutAndGetByteWiseLittleEndianInt()
		 {
			  // GIVEN
			  int sizeInBytes = 4;
			  GlobalMemoryTracker tracker = GlobalMemoryTracker.INSTANCE;
			  long p = allocateMemory( sizeInBytes, tracker );
			  int value = 0b11001100_10101010_10011001_01100110;

			  // WHEN
			  UnsafeUtil.PutIntByteWiseLittleEndian( p, value );
			  int readValue = UnsafeUtil.GetIntByteWiseLittleEndian( p );

			  // THEN
			  free( p, sizeInBytes, tracker );
			  assertEquals( value, readValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGetByteWiseLittleEndianLong()
		 internal virtual void ShouldPutAndGetByteWiseLittleEndianLong()
		 {
			  // GIVEN
			  int sizeInBytes = 8;
			  GlobalMemoryTracker tracker = GlobalMemoryTracker.INSTANCE;
			  long p = allocateMemory( sizeInBytes, tracker );
			  long value = 0b11001100_10101010_10011001_01100110__10001000_01000100_00100010_00010001L;

			  // WHEN
			  UnsafeUtil.PutLongByteWiseLittleEndian( p, value );
			  long readValue = UnsafeUtil.GetLongByteWiseLittleEndian( p );

			  // THEN
			  free( p, sizeInBytes, tracker );
			  assertEquals( value, readValue );
		 }
	}

}