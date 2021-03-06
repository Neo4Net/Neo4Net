﻿/*
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

	public abstract class IntegralValue : NumberValue
	{
		 public static long SafeCastIntegral( string name, AnyValue value, long defaultValue )
		 {
			  if ( value == null || value == Values.NoValue )
			  {
					return defaultValue;
			  }
			  if ( value is IntegralValue )
			  {
					return ( ( IntegralValue ) value ).LongValue();
			  }
			  throw new System.ArgumentException( name + " must be an integer value, but was a " + value.GetType().Name );
		 }

		 public override bool Equals( long x )
		 {
			  return LongValue() == x;
		 }

		 public override bool Equals( double x )
		 {
			  return NumberValues.NumbersEqual( x, LongValue() );
		 }

		 public override int ComputeHash()
		 {
			  return NumberValues.Hash( LongValue() );
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  return hashFunction.Update( hash, LongValue() );
		 }

		 public override bool Eq( object other )
		 {
			  return other is Value && Equals( ( Value ) other );
		 }

		 public override bool Equals( Value other )
		 {
			  if ( other is IntegralValue )
			  {
					IntegralValue that = ( IntegralValue ) other;
					return this.LongValue() == that.LongValue();
			  }
			  else if ( other is FloatingPointValue )
			  {
					FloatingPointValue that = ( FloatingPointValue ) other;
					return NumberValues.NumbersEqual( that.DoubleValue(), this.LongValue() );
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override int CompareTo( IntegralValue other )
		 {
			  return Long.compare( LongValue(), other.LongValue() );
		 }

		 public override int CompareTo( FloatingPointValue other )
		 {
			  return NumberValues.CompareLongAgainstDouble( LongValue(), other.DoubleValue() );
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.Integral;
		 }

		 public override double DoubleValue()
		 {
			  return LongValue();
		 }

		 public override LongValue Minus( long b )
		 {
			  return ValueMath.subtract( LongValue(), b );
		 }

		 public override DoubleValue Minus( double b )
		 {
			  return ValueMath.subtract( LongValue(), b );
		 }

		 public override LongValue Plus( long b )
		 {
			  return ValueMath.add( LongValue(), b );
		 }

		 public override DoubleValue Plus( double b )
		 {
			  return ValueMath.add( LongValue(), b );
		 }

		 public override LongValue Times( long b )
		 {
			  return ValueMath.multiply( LongValue(), b );
		 }

		 public override DoubleValue Times( double b )
		 {
			  return ValueMath.multiply( LongValue(), b );
		 }

		 public override LongValue DividedBy( long b )
		 {
			  return Values.LongValue( LongValue() / b );
		 }

		 public override DoubleValue DividedBy( double b )
		 {
			  return Values.DoubleValue( DoubleValue() / b );
		 }
	}

}