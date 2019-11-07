using System;

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
namespace Neo4Net.Cypher.operations
{
	using ArithmeticException = Neo4Net.Cypher.Internal.v3_5.util.ArithmeticException;
	using CypherTypeException = Neo4Net.Cypher.Internal.v3_5.util.CypherTypeException;

	using AnyValue = Neo4Net.Values.AnyValue;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using FloatingPointValue = Neo4Net.Values.Storable.FloatingPointValue;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.ZERO_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

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