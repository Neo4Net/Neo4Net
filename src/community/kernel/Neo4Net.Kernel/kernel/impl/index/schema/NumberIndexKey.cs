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
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Includes value and IEntity id (to be able to handle non-unique values).
	/// A value can be any <seealso cref="Number"/> and is represented as a {@code long} to store the raw bits and a type
	/// to say if it's a long, double or float.
	/// 
	/// Distinction between double and float exists because coercions between each other and long may differ.
	/// TODO this should be figured out and potentially reduced to long, double types only.
	/// </summary>
	internal class NumberIndexKey : NativeIndexSingleValueKey<NumberIndexKey>
	{
		 internal static readonly int Size = Byte.BYTES + Long.BYTES + IEntity_ID_SIZE; // (Long.BYTES) IEntityId

		 internal sbyte Type;
		 internal long RawValueBits;

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !Values.isNumberValue( value ) )
			  {
					throw new System.ArgumentException( "Key layout does only support numbers, tried to create key from " + value );
			  }
			  return value;
		 }

		 internal override NumberValue AsValue()
		 {
			  return RawBits.AsNumberValue( RawValueBits, Type );
		 }

		 internal override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  writeFloatingPoint( double.NegativeInfinity );
		 }

		 internal override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  writeFloatingPoint( double.PositiveInfinity );
		 }

		 /// <summary>
		 /// Compares the value of this key to that of another key.
		 /// This method is expected to be called in scenarios where inconsistent reads may happen (and later retried).
		 /// </summary>
		 /// <param name="other"> the <seealso cref="NumberIndexKey"/> to compare to. </param>
		 /// <returns> comparison against the {@code other} <seealso cref="NumberIndexKey"/>. </returns>
		 internal override int CompareValueTo( NumberIndexKey other )
		 {
			  return RawBits.Compare( RawValueBits, Type, other.RawValueBits, other.Type );
		 }

		 public override string ToString()
		 {
			  return format( "type=%d,rawValue=%d,value=%s,entityId=%d", Type, RawValueBits, AsValue(), IEntityId );
		 }

		 public override void WriteInteger( sbyte value )
		 {
			  Type = RawBits.BYTE;
			  RawValueBits = value;
		 }

		 public override void WriteInteger( short value )
		 {
			  Type = RawBits.SHORT;
			  RawValueBits = value;
		 }

		 public override void WriteInteger( int value )
		 {
			  Type = RawBits.INT;
			  RawValueBits = value;
		 }

		 public override void WriteInteger( long value )
		 {
			  Type = RawBits.LONG;
			  RawValueBits = value;
		 }

		 public override void WriteFloatingPoint( float value )
		 {
			  Type = RawBits.FLOAT;
			  RawValueBits = Float.floatToIntBits( value );
		 }

		 public override void WriteFloatingPoint( double value )
		 {
			  Type = RawBits.DOUBLE;
			  RawValueBits = System.BitConverter.DoubleToInt64Bits( value );
		 }
	}

}