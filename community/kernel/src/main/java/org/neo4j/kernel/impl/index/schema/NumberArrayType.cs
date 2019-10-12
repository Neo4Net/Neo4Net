using System;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Org.Neo4j.Values.Storable;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.setCursorException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NumberType.numberKeySize;

	// Raw Number type is mostly for show as internally specific primitive int/long/short etc. arrays are created instead
	internal class NumberArrayType : AbstractArrayType<Number>
	{
		 // Affected key state:
		 // long0Array (value)
		 // long1 (number type)

		 internal NumberArrayType( sbyte typeId ) : base( ValueGroup.NUMBER_ARRAY, typeId, ( o1, o2, i ) -> NumberType.Compare( o1.long0Array[i], o1.long1, o2.long0Array[i], o2.long1 ), null, null, null, null, null )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, numberKeySize( state.Long1 ) ) + GenericKey.SizeNumberType;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  to.Long1 = from.Long1;
			  InitializeArray( to, length );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
		 }

		 internal override void InitializeArray( GenericKey key, int length, Org.Neo4j.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  InitializeArray( key, length );
			  switch ( arrayType )
			  {
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Byte:
					key.Long1 = RawBits.BYTE;
					break;
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Short:
					key.Long1 = RawBits.SHORT;
					break;
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Int:
					key.Long1 = RawBits.INT;
					break;
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Long:
					key.Long1 = RawBits.LONG;
					break;
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Float:
					key.Long1 = RawBits.FLOAT;
					break;
			  case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Double:
					key.Long1 = RawBits.DOUBLE;
					break;
			  default:
					throw new System.ArgumentException( "Invalid number array type " + arrayType );
			  }
		 }

		 private void InitializeArray( GenericKey key, int length )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
			  // plain long1 for number type
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  sbyte numberType = ( sbyte ) state.Long1;
			  switch ( numberType )
			  {
			  case RawBits.BYTE:
					sbyte[] byteArray = new sbyte[state.ArrayLength];
					for ( int i = 0; i < state.ArrayLength; i++ )
					{
						 byteArray[i] = ( sbyte ) state.Long0Array[i];
					}
					return Values.byteArray( byteArray );
			  case RawBits.SHORT:
					short[] shortArray = new short[state.ArrayLength];
					for ( int i = 0; i < state.ArrayLength; i++ )
					{
						 shortArray[i] = ( short ) state.Long0Array[i];
					}
					return Values.shortArray( shortArray );
			  case RawBits.INT:
					int[] intArray = new int[state.ArrayLength];
					for ( int i = 0; i < state.ArrayLength; i++ )
					{
						 intArray[i] = ( int ) state.Long0Array[i];
					}
					return Values.intArray( intArray );
			  case RawBits.LONG:
					return Values.longArray( Arrays.copyOf( state.Long0Array, state.ArrayLength ) );
			  case RawBits.FLOAT:
					float[] floatArray = new float[state.ArrayLength];
					for ( int i = 0; i < state.ArrayLength; i++ )
					{
						 floatArray[i] = Float.intBitsToFloat( ( int ) state.Long0Array[i] );
					}
					return Values.floatArray( floatArray );
			  case RawBits.DOUBLE:
					double[] doubleArray = new double[state.ArrayLength];
					for ( int i = 0; i < state.ArrayLength; i++ )
					{
						 doubleArray[i] = Double.longBitsToDouble( state.Long0Array[i] );
					}
					return Values.doubleArray( doubleArray );
			  default:
					throw new System.ArgumentException( "Unknown number type " + numberType );
			  }
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  cursor.PutByte( ( sbyte ) state.Long1 );
			  PutArray( cursor, state, NumberArrayElementWriter( state ) );
		 }

		 private ArrayElementWriter NumberArrayElementWriter( GenericKey key )
		 {
			  switch ( ( int ) key.Long1 )
			  {
			  case RawBits.BYTE:
					return ( c, k, i ) => c.putByte( ( sbyte ) k.long0Array[i] );
			  case RawBits.SHORT:
					return ( c, k, i ) => c.putShort( ( short ) k.long0Array[i] );
			  case RawBits.INT:
			  case RawBits.FLOAT:
					return ( c, k, i ) => c.putInt( ( int ) k.long0Array[i] );
			  case RawBits.LONG:
			  case RawBits.DOUBLE:
					return ( c, k, i ) => c.putLong( k.long0Array[i] );
			  default:
					throw new System.ArgumentException( "Unknown number type " + key.Long1 );
			  }
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  into.Long1 = cursor.Byte; // number type, like: byte, int, short a.s.o.
			  Org.Neo4j.Values.Storable.ValueWriter_ArrayType numberType = NumberArrayTypeOf( ( sbyte ) into.Long1 );
			  if ( numberType == null )
			  {
					setCursorException( cursor, "non-valid number type for array, " + into.Long1 );
					return false;
			  }
			  return ReadArray( cursor, numberType, NumberArrayElementReader( into ), into );
		 }

		 internal override void InitializeAsLowest( GenericKey state )
		 {
			  state.InitializeArrayMeta( 0 );
			  InitializeArray( state, 0, Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Byte );
		 }

		 internal override void InitializeAsHighest( GenericKey state )
		 {
			  state.InitializeArrayMeta( 0 );
			  InitializeArray( state, 0, Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Byte );
			  state.IsHighestArray = true;
		 }

		 private static Org.Neo4j.Values.Storable.ValueWriter_ArrayType NumberArrayTypeOf( sbyte numberType )
		 {
			  switch ( numberType )
			  {
			  case RawBits.BYTE:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Byte;
			  case RawBits.SHORT:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Short;
			  case RawBits.INT:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Int;
			  case RawBits.LONG:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Long;
			  case RawBits.FLOAT:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Float;
			  case RawBits.DOUBLE:
					return Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Double;
			  default:
					// bad read, hopefully
					return null;
			  }
		 }

		 private ArrayElementReader NumberArrayElementReader( GenericKey key )
		 {
			  switch ( ( int ) key.Long1 )
			  {
			  case RawBits.BYTE:
					return ( c, into ) =>
					{
					 key.writeInteger( c.Byte );
					 return true;
					};
			  case RawBits.SHORT:
					return ( c, into ) =>
					{
					 key.writeInteger( c.Short );
					 return true;
					};
			  case RawBits.INT:
					return ( c, into ) =>
					{
					 key.writeInteger( c.Int );
					 return true;
					};
			  case RawBits.LONG:
					return ( c, into ) =>
					{
					 key.writeInteger( c.Long );
					 return true;
					};
			  case RawBits.FLOAT:
					return ( c, into ) =>
					{
					 key.writeFloatingPoint( Float.intBitsToFloat( c.Int ) );
					 return true;
					};
			  case RawBits.DOUBLE:
					return ( c, into ) =>
					{
					 key.writeFloatingPoint( Double.longBitsToDouble( c.Long ) );
					 return true;
					};
			  default:
					throw new System.ArgumentException( "Unknown number type " + key.Long1 );
			  }
		 }

		 internal virtual void Write( GenericKey state, int offset, long value )
		 {
			  state.Long0Array[offset] = value;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}