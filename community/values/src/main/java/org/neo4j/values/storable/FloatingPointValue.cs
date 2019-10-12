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
namespace Org.Neo4j.Values.Storable
{
	using HashFunction = Org.Neo4j.Hashing.HashFunction;
	using ValueMath = Org.Neo4j.Values.utils.ValueMath;

	public abstract class FloatingPointValue : NumberValue
	{
		 public override bool Equals( long x )
		 {
			  return NumberValues.NumbersEqual( DoubleValue(), x );
		 }

		 public override bool Equals( double x )
		 {
			  return DoubleValue() == x;
		 }

		 public override int ComputeHash()
		 {
			  return NumberValues.Hash( DoubleValue() );
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  return hashFunction.Update( hash, System.BitConverter.DoubleToInt64Bits( DoubleValue() ) );
		 }

		 public override bool Eq( object other )
		 {
			  return other is Value && Equals( ( Value ) other );
		 }

		 public override bool Equals( Value other )
		 {
			  if ( other is FloatingPointValue )
			  {
					FloatingPointValue that = ( FloatingPointValue ) other;
					return this.DoubleValue() == that.DoubleValue();
			  }
			  else if ( other is IntegralValue )
			  {
					IntegralValue that = ( IntegralValue ) other;
					return NumberValues.NumbersEqual( this.DoubleValue(), that.LongValue() );
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.FloatingPoint;
		 }

		 public override int CompareTo( IntegralValue other )
		 {
			  return NumberValues.CompareDoubleAgainstLong( DoubleValue(), other.LongValue() );
		 }

		 public override int CompareTo( FloatingPointValue other )
		 {
			  return DoubleValue().CompareTo(other.DoubleValue());
		 }

		 public override bool NaN
		 {
			 get
			 {
				  return double.IsNaN( this.DoubleValue() );
			 }
		 }

		 public override long LongValue()
		 {
			  return ( long ) DoubleValue();
		 }

		 public override DoubleValue Minus( long b )
		 {
			  return ValueMath.subtract( DoubleValue(), b );
		 }

		 public override DoubleValue Minus( double b )
		 {
			  return ValueMath.subtract( DoubleValue(), b );
		 }

		 public override DoubleValue Plus( long b )
		 {
			  return ValueMath.add( DoubleValue(), b );
		 }

		 public override DoubleValue Plus( double b )
		 {
			  return ValueMath.add( DoubleValue(), b );
		 }

		 public override DoubleValue Times( long b )
		 {
			  return ValueMath.multiply( DoubleValue(), b );
		 }

		 public override DoubleValue Times( double b )
		 {
			  return ValueMath.multiply( DoubleValue(), b );
		 }

		 public override DoubleValue DividedBy( long b )
		 {
			  return Values.DoubleValue( DoubleValue() / b );
		 }

		 public override DoubleValue DividedBy( double b )
		 {
			  return Values.DoubleValue( DoubleValue() / b );
		 }
	}

}