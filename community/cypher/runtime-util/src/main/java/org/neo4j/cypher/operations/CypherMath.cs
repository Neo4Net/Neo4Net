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
namespace Org.Neo4j.Cypher.operations
{
	using ArithmeticException = Org.Neo4j.Cypher.@internal.v3_5.util.ArithmeticException;
	using CypherTypeException = Org.Neo4j.Cypher.@internal.v3_5.util.CypherTypeException;

	using AnyValue = Org.Neo4j.Values.AnyValue;
	using ArrayValue = Org.Neo4j.Values.Storable.ArrayValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using FloatingPointValue = Org.Neo4j.Values.Storable.FloatingPointValue;
	using IntegralValue = Org.Neo4j.Values.Storable.IntegralValue;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Org.Neo4j.Values.Storable;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.ZERO_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	/// <summary>
	/// This class contains static helper math methods used by the compiled expressions
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public final class CypherMath
	public sealed class CypherMath
	{
		 private CypherMath()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 //TODO this is horrible spaghetti code, we should push most of this down to AnyValue
		 public static AnyValue Add( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).plus( ( NumberValue ) rhs );
			  }
			  //List addition
			  //arrays are same as lists when it comes to addition
			  if ( lhs is ArrayValue )
			  {
					lhs = VirtualValues.fromArray( ( ArrayValue ) lhs );
			  }
			  if ( rhs is ArrayValue )
			  {
					rhs = VirtualValues.fromArray( ( ArrayValue ) rhs );
			  }

			  bool lhsIsListValue = lhs is ListValue;
			  if ( lhsIsListValue && rhs is ListValue )
			  {
					return VirtualValues.concat( ( ListValue ) lhs, ( ListValue ) rhs );
			  }
			  else if ( lhsIsListValue )
			  {
					return ( ( ListValue ) lhs ).append( rhs );
			  }
			  else if ( rhs is ListValue )
			  {
					return ( ( ListValue ) rhs ).prepend( lhs );
			  }

			  // String addition
			  if ( lhs is TextValue && rhs is TextValue )
			  {
					return ( ( TextValue ) lhs ).plus( ( TextValue ) rhs );
			  }
			  else if ( lhs is TextValue )
			  {
					if ( rhs is Value )
					{
						 // Unfortunately string concatenation is not defined for temporal and spatial types, so we need to
						 // exclude them
						 if ( !( rhs is TemporalValue || rhs is DurationValue || rhs is PointValue ) )
						 {
							  return stringValue( ( ( TextValue ) lhs ).stringValue() + ((Value) rhs).prettyPrint() );
						 }
						 else
						 {
							  return stringValue( ( ( TextValue ) lhs ).stringValue() + rhs.ToString() );
						 }
					}
			  }
			  else if ( rhs is TextValue )
			  {
					if ( lhs is Value )
					{
						 // Unfortunately string concatenation is not defined for temporal and spatial types, so we need to
						 // exclude them
						 if ( !( lhs is TemporalValue || lhs is DurationValue || lhs is PointValue ) )
						 {
							  return stringValue( ( ( Value ) lhs ).prettyPrint() + ((TextValue) rhs).stringValue() );
						 }
						 else
						 {
							  return stringValue( lhs.ToString() + ((TextValue) rhs).stringValue() );
						 }
					}
			  }

			  // Temporal values
			  if ( lhs is TemporalValue )
			  {
					if ( rhs is DurationValue )
					{
						 return ( ( TemporalValue ) lhs ).plus( ( DurationValue ) rhs );
					}
			  }
			  if ( lhs is DurationValue )
			  {
					if ( rhs is TemporalValue )
					{
						 return ( ( TemporalValue ) rhs ).plus( ( DurationValue ) lhs );
					}
					if ( rhs is DurationValue )
					{
						 return ( ( DurationValue ) lhs ).add( ( DurationValue ) rhs );
					}
			  }

			  throw new CypherTypeException( string.Format( "Cannot add `{0}` and `{1}`", lhs.TypeName, rhs.TypeName ), null );
		 }

		 public static AnyValue Subtract( AnyValue lhs, AnyValue rhs )
		 {
			  //numbers
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).minus( ( NumberValue ) rhs );
			  }
			  // Temporal values
			  if ( lhs is TemporalValue )
			  {
					if ( rhs is DurationValue )
					{
						 return ( ( TemporalValue ) lhs ).minus( ( DurationValue ) rhs );
					}
			  }
			  if ( lhs is DurationValue )
			  {
					if ( rhs is DurationValue )
					{
						 return ( ( DurationValue ) lhs ).sub( ( DurationValue ) rhs );
					}
			  }
			  throw new CypherTypeException( string.Format( "Cannot subtract `{0}` from `{1}`", rhs.TypeName, lhs.TypeName ), null );
		 }

		 public static AnyValue Multiply( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).times( ( NumberValue ) rhs );
			  }
			  // Temporal values
			  if ( lhs is DurationValue )
			  {
					if ( rhs is NumberValue )
					{
						 return ( ( DurationValue ) lhs ).mul( ( NumberValue ) rhs );
					}
			  }
			  if ( rhs is DurationValue )
			  {
					if ( lhs is NumberValue )
					{
						 return ( ( DurationValue ) rhs ).mul( ( NumberValue ) lhs );
					}
			  }
			  throw new CypherTypeException( string.Format( "Cannot multiply `{0}` and `{1}`", lhs.TypeName, rhs.TypeName ), null );
		 }

		 public static bool DivideCheckForNull( AnyValue lhs, AnyValue rhs )
		 {
			  if ( rhs is IntegralValue && rhs.Equals( ZERO_INT ) )
			  {
					throw new ArithmeticException( "/ by zero", null );
			  }
			  else
			  {
					return lhs == Values.NO_VALUE || rhs == Values.NO_VALUE;
			  }
		 }

		 public static AnyValue Divide( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).divideBy( ( NumberValue ) rhs );
			  }
			  // Temporal values
			  if ( lhs is DurationValue )
			  {
					if ( rhs is NumberValue )
					{
						 return ( ( DurationValue ) lhs ).div( ( NumberValue ) rhs );
					}
			  }
			  throw new CypherTypeException( string.Format( "Cannot divide `{0}` by `{1}`", lhs.TypeName, rhs.TypeName ), null );
		 }

		 public static AnyValue Modulo( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					if ( lhs is FloatingPointValue || rhs is FloatingPointValue )
					{
						 return doubleValue( ( ( NumberValue ) lhs ).doubleValue() % ((NumberValue) rhs).doubleValue() );
					}
					else
					{
						 return longValue( ( ( NumberValue ) lhs ).longValue() % ((NumberValue) rhs).longValue() );
					}
			  }
			  throw new CypherTypeException( string.Format( "Cannot calculate modulus of `{0}` and `{1}`", lhs.TypeName, rhs.TypeName ), null );
		 }

		 public static AnyValue Pow( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return doubleValue( Math.Pow( ( ( NumberValue ) lhs ).doubleValue(), ((NumberValue) rhs).doubleValue() ) );
			  }
			  throw new CypherTypeException( string.Format( "Cannot raise `{0}` to the power of `{1}`", lhs.TypeName, rhs.TypeName ), null );
		 }
	}

}