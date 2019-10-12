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
namespace Neo4Net.Values.Storable
{

	public abstract class NumberValue : ScalarValue
	{
		 public static double SafeCastFloatingPoint( string name, AnyValue value, double defaultValue )
		 {
			  if ( value == null )
			  {
					return defaultValue;
			  }
			  if ( value is IntegralValue )
			  {
					return ( ( IntegralValue ) value ).DoubleValue();
			  }
			  if ( value is FloatingPointValue )
			  {
					return ( ( FloatingPointValue ) value ).DoubleValue();
			  }
			  throw new System.ArgumentException( name + " must be a number value, but was a " + value.GetType().Name );
		 }

		 public abstract double DoubleValue();

		 public abstract long LongValue();

		 public abstract int CompareTo( IntegralValue other );

		 public abstract int CompareTo( FloatingPointValue other );

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  if ( otherValue is IntegralValue )
			  {
					return CompareTo( ( IntegralValue ) otherValue );
			  }
			  else if ( otherValue is FloatingPointValue )
			  {
					return CompareTo( ( FloatingPointValue ) otherValue );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot compare different values" );
			  }
		 }

		 public override abstract Number AsObjectCopy();

		 public override Number AsObject()
		 {
			  return AsObjectCopy();
		 }

		 public override sealed bool Equals( bool x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( char x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( string x )
		 {
			  return false;
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Number;
		 }

		 public abstract NumberValue Minus( long b );

		 public abstract NumberValue Minus( double b );

		 public abstract NumberValue Plus( long b );

		 public abstract NumberValue Plus( double b );

		 public abstract NumberValue Times( long b );

		 public abstract NumberValue Times( double b );

		 public abstract NumberValue DividedBy( long b );

		 public abstract NumberValue DividedBy( double b );

		 public virtual NumberValue Minus( NumberValue numberValue )
		 {
			  if ( numberValue is IntegralValue )
			  {
					return Minus( numberValue.LongValue() );
			  }
			  else if ( numberValue is FloatingPointValue )
			  {
					return Minus( numberValue.DoubleValue() );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot subtract " + numberValue );
			  }
		 }

		 public virtual NumberValue Plus( NumberValue numberValue )
		 {
			  if ( numberValue is IntegralValue )
			  {
					return Plus( numberValue.LongValue() );
			  }
			  else if ( numberValue is FloatingPointValue )
			  {
					return Plus( numberValue.DoubleValue() );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot add " + numberValue );
			  }
		 }

		 public virtual NumberValue Times( NumberValue numberValue )
		 {
			  if ( numberValue is IntegralValue )
			  {
					return Times( numberValue.LongValue() );
			  }
			  else if ( numberValue is FloatingPointValue )
			  {
					return Times( numberValue.DoubleValue() );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot multiply with " + numberValue );
			  }
		 }

		 public virtual NumberValue DivideBy( NumberValue numberValue )
		 {
			  if ( numberValue is IntegralValue )
			  {
					return DividedBy( numberValue.LongValue() );
			  }
			  else if ( numberValue is FloatingPointValue )
			  {
					return DividedBy( numberValue.DoubleValue() );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot divide by " + numberValue );
			  }
		 }
	}

}