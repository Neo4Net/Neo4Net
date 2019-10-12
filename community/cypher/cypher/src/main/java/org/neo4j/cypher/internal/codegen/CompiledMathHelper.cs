using System;
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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using ArithmeticException = Org.Neo4j.Cypher.@internal.v3_5.util.ArithmeticException;
	using CypherTypeException = Org.Neo4j.Cypher.@internal.v3_5.util.CypherTypeException;


	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using ArrayValue = Org.Neo4j.Values.Storable.ArrayValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using IntegralValue = Org.Neo4j.Values.Storable.IntegralValue;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Org.Neo4j.Values.Storable;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

	/// <summary>
	/// This is a helper class used by compiled plans for doing basic math operations
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public final class CompiledMathHelper
	public sealed class CompiledMathHelper
	{
		 private static readonly double _epsilon = Math.Pow( 1, -10 );

		 /// <summary>
		 /// Do not instantiate this class
		 /// </summary>
		 private CompiledMathHelper()
		 {
		 }

		 /// <summary>
		 /// Utility function for doing addition
		 /// </summary>
		 public static object Add( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  //List addition
			  bool lhsIsListValue = lhs is ListValue;
			  if ( lhsIsListValue && rhs is ListValue )
			  {
					return VirtualValues.concat( ( ListValue ) lhs, ( ListValue ) rhs );
			  }
			  else if ( lhsIsListValue )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (rhs instanceof java.util.List<?>)
					if ( rhs is IList<object> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return org.neo4j.values.virtual.VirtualValues.concat((org.neo4j.values.virtual.ListValue) lhs, org.neo4j.kernel.impl.util.ValueUtils.asListValue((java.util.List<?>) rhs));
						 return VirtualValues.concat( ( ListValue ) lhs, ValueUtils.asListValue( ( IList<object> ) rhs ) );
					}
					else if ( rhs is AnyValue )
					{
						 return ( ( ListValue ) lhs ).append( ( AnyValue ) rhs );
					}
					else
					{
						 return ( ( ListValue ) lhs ).append( ValueUtils.of( rhs ) );
					}
			  }
			  else if ( rhs is ListValue )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (lhs instanceof java.util.List<?>)
					if ( lhs is IList<object> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return org.neo4j.values.virtual.VirtualValues.concat(org.neo4j.kernel.impl.util.ValueUtils.asListValue((java.util.List<?>) lhs), (org.neo4j.values.virtual.ListValue) rhs);
						 return VirtualValues.concat( ValueUtils.asListValue( ( IList<object> ) lhs ), ( ListValue ) rhs );
					}
					else if ( lhs is AnyValue )
					{
						 return ( ( ListValue ) rhs ).prepend( ( AnyValue ) lhs );
					}
					else
					{
						 return ( ( ListValue ) rhs ).prepend( ValueUtils.of( lhs ) );
					}
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (lhs instanceof java.util.List<?> && rhs instanceof java.util.List<?>)
			  else if ( lhs is IList<object> && rhs is IList<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> lhsList = (java.util.List<?>) lhs;
					IList<object> lhsList = ( IList<object> ) lhs;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> rhsList = (java.util.List<?>) rhs;
					IList<object> rhsList = ( IList<object> ) rhs;
					IList<object> result = new List<object>( lhsList.Count + rhsList.Count );
					( ( IList<object> )result ).AddRange( lhsList );
					( ( IList<object> )result ).AddRange( rhsList );
					return result;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (lhs instanceof java.util.List<?>)
			  else if ( lhs is IList<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> lhsList = (java.util.List<?>) lhs;
					IList<object> lhsList = ( IList<object> ) lhs;
					IList<object> result = new List<object>( lhsList.Count + 1 );
					( ( IList<object> )result ).AddRange( lhsList );
					result.Add( rhs );
					return result;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (rhs instanceof java.util.List<?>)
			  else if ( rhs is IList<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> rhsList = (java.util.List<?>) rhs;
					IList<object> rhsList = ( IList<object> ) rhs;
					IList<object> result = new List<object>( rhsList.Count + 1 );
					result.Add( lhs );
					( ( IList<object> )result ).AddRange( rhsList );
					return result;
			  }

			  // String addition
			  if ( lhs is TextValue )
			  {
					lhs = ( ( TextValue ) lhs ).stringValue();
			  }
			  if ( rhs is TextValue )
			  {
					rhs = ( ( TextValue ) rhs ).stringValue();
			  }
			  if ( lhs is string )
			  {
					if ( rhs is Value )
					{
						 // Unfortunately string concatenation is not defined for temporal and spatial types, so we need to exclude them
						 if ( !( rhs is TemporalValue || rhs is DurationValue || rhs is PointValue ) )
						 {
							  return lhs.ToString() + ((Value) rhs).prettyPrint();
						 }
					}
					else
					{
						 return lhs.ToString() + rhs.ToString();
					}
			  }
			  if ( rhs is string )
			  {
					if ( lhs is Value )
					{
						 // Unfortunately string concatenation is not defined for temporal and spatial types, so we need to exclude them
						 if ( !( lhs is TemporalValue || lhs is DurationValue || lhs is PointValue ) )
						 {
							  return ( ( Value ) lhs ).prettyPrint() + rhs.ToString();
						 }
					}
					else
					{
						 return lhs.ToString() + rhs.ToString();
					}
			  }

			  // array addition

			  // Extract arrays from ArrayValues
			  if ( lhs is ArrayValue )
			  {
					lhs = ( ( ArrayValue ) lhs ).asObject();
			  }
			  if ( rhs is ArrayValue )
			  {
					rhs = ( ( ArrayValue ) rhs ).asObject();
			  }

			  Type lhsClass = lhs.GetType();
			  Type rhsClass = rhs.GetType();
			  if ( lhsClass.IsArray && rhsClass.IsArray )
			  {
					return AddArrays( lhs, rhs );
			  }
			  else if ( lhsClass.IsArray )
			  {
					return AddArrayWithObject( lhs, rhs );
			  }
			  else if ( rhsClass.IsArray )
			  {
					return AddObjectWithArray( lhs, rhs );
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).plus( ( NumberValue ) rhs );
			  }
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  if ( lhs is Number )
			  {
					if ( rhs is Number )
					{
						 if ( lhs is double? || rhs is double? || lhs is float? || rhs is float? )
						 {
							  return ( ( Number ) lhs ).doubleValue() + ((Number) rhs).doubleValue();
						 }
						 if ( lhs is long? || rhs is long? || lhs is int? || rhs is int? || lhs is short? || rhs is short? || lhs is sbyte? || rhs is sbyte? )
						 {
							  return Math.addExact( ( ( Number ) lhs ).longValue(), ((Number) rhs).longValue() );
							  // Remap java.lang.ArithmeticException later instead of:
							  //catch ( java.lang.ArithmeticException e )
							  //{
							  //    throw new ArithmeticException(
							  //            String.format( "result of %d + %d cannot be represented as an integer",
							  //                    ((Number) lhs).longValue(), ((Number) rhs).longValue() ), e );
							  //}
						 }
					}
					// other numbers we cannot add
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

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot add `{0}` and `{1}`", lhsValue.TypeName, rhsValue.TypeName ), null );
		 }

		 public static object Subtract( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).minus( ( NumberValue ) rhs );
			  }
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  if ( lhs is Number && rhs is Number )
			  {
					if ( lhs is double? || rhs is double? || lhs is float? || rhs is float? )
					{
						 return ( ( Number ) lhs ).doubleValue() - ((Number) rhs).doubleValue();
					}
					if ( lhs is long? || rhs is long? || lhs is int? || rhs is int? || lhs is short? || rhs is short? || lhs is sbyte? || rhs is sbyte? )
					{
						 return Math.subtractExact( ( ( Number ) lhs ).longValue(), ((Number) rhs).longValue() );
						 // Remap java.lang.ArithmeticException later instead of:
						 //catch ( java.lang.ArithmeticException e )
						 //{
						 //    throw new ArithmeticException(
						 //            String.format( "result of %d - %d cannot be represented as an integer",
						 //                    ((Number) lhs).longValue(), ((Number) rhs).longValue() ), e );
						 //}
					}
					// other numbers we cannot subtract
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

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot subtract `{0}` from `{1}`", rhsValue.TypeName, lhsValue.TypeName ), null );
		 }

		 public static object Multiply( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  // Temporal values
			  if ( lhs is DurationValue )
			  {
					if ( rhs is NumberValue )
					{
						 return ( ( DurationValue ) lhs ).mul( ( NumberValue ) rhs );
					}
					if ( rhs is Number )
					{
						 return ( ( DurationValue ) lhs ).mul( Values.numberValue( ( Number ) rhs ) );
					}
			  }
			  if ( rhs is DurationValue )
			  {
					if ( lhs is NumberValue )
					{
						 return ( ( DurationValue ) rhs ).mul( ( NumberValue ) lhs );
					}
					if ( lhs is Number )
					{
						 return ( ( DurationValue ) rhs ).mul( Values.numberValue( ( Number ) lhs ) );
					}
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return ( ( NumberValue ) lhs ).times( ( NumberValue ) rhs );
			  }
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  if ( lhs is Number && rhs is Number )
			  {
					if ( lhs is double? || rhs is double? || lhs is float? || rhs is float? )
					{
						 return ( ( Number ) lhs ).doubleValue() * ((Number) rhs).doubleValue();
					}
					if ( lhs is long? || rhs is long? || lhs is int? || rhs is int? || lhs is short? || rhs is short? || lhs is sbyte? || rhs is sbyte? )
					{
						 return Math.multiplyExact( ( ( Number ) lhs ).longValue(), ((Number) rhs).longValue() );
						 // Remap java.lang.ArithmeticException later instead of:
						 //catch ( java.lang.ArithmeticException e )
						 //{
						 //    throw new ArithmeticException(
						 //            String.format( "result of %d * %d cannot be represented as an integer",
						 //                    ((Number) lhs).longValue(), ((Number) rhs).longValue() ), e );
						 //}
					}
					// other numbers we cannot multiply
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot multiply `{0}` and `{1}`", lhsValue.TypeName, rhsValue.TypeName ), null );
		 }

		 public static object Divide( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  // Temporal values
			  if ( lhs is DurationValue )
			  {
					if ( rhs is NumberValue )
					{
						 return ( ( DurationValue ) lhs ).div( ( NumberValue ) rhs );
					}
					if ( rhs is Number )
					{
						 return ( ( DurationValue ) lhs ).div( Values.numberValue( ( Number ) rhs ) );
					}
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue && rhs is NumberValue )
			  {
					if ( rhs is IntegralValue )
					{
						 long right = ( ( IntegralValue ) rhs ).longValue();
						 if ( right == 0 )
						 {
							  throw new ArithmeticException( "/ by zero", null );
						 }
					}
					return ( ( NumberValue ) lhs ).divideBy( ( NumberValue ) rhs );
			  }
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  if ( lhs is Number && rhs is Number )
			  {
					if ( lhs is double? || rhs is double? || lhs is float? || rhs is float? )
					{
						 double left = ( ( Number ) lhs ).doubleValue();
						 double right = ( ( Number ) rhs ).doubleValue();
						 return left / right;
					}
					if ( lhs is long? || rhs is long? || lhs is int? || rhs is int? || lhs is short? || rhs is short? || lhs is sbyte? || rhs is sbyte? )
					{
						 long left = ( ( Number ) lhs ).longValue();
						 long right = ( ( Number ) rhs ).longValue();
						 if ( right == 0 )
						 {
							  throw new ArithmeticException( "/ by zero", null );
						 }
						 return left / right;
					}
					// other numbers we cannot divide
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot divide `{0}` by `{1}`", lhsValue.TypeName, rhsValue.TypeName ), null );
		 }

		 public static object Modulo( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  if ( lhs is Number && rhs is Number )
			  {
					if ( lhs is double? || rhs is double? || lhs is float? || rhs is float? )
					{
						 double left = ( ( Number ) lhs ).doubleValue();
						 double right = ( ( Number ) rhs ).doubleValue();
						 return left % right;
					}
					if ( lhs is long? || rhs is long? || lhs is int? || rhs is int? || lhs is short? || rhs is short? || lhs is sbyte? || rhs is sbyte? )
					{
						 long left = ( ( Number ) lhs ).longValue();
						 long right = ( ( Number ) rhs ).longValue();
						 if ( right == 0 )
						 {
							  throw new ArithmeticException( "/ by zero", null );
						 }
						 return left % right;
					}
					// other numbers we cannot divide
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot calculate modulus of `{0}` and `{1}`", lhsValue.TypeName, rhsValue.TypeName ), null );
		 }

		 public static object Pow( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == Values.NO_VALUE || rhs == Values.NO_VALUE )
			  {
					return null;
			  }

			  // Handle NumberValues
			  if ( lhs is NumberValue )
			  {
					lhs = ( ( NumberValue ) lhs ).asObject();
			  }
			  if ( rhs is NumberValue )
			  {
					rhs = ( ( NumberValue ) rhs ).asObject();
			  }

			  // now we have Numbers
			  if ( lhs is Number && rhs is Number )
			  {
					return Math.Pow( ( ( Number ) lhs ).doubleValue(), ((Number) rhs).doubleValue() );
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : Values.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : Values.of( rhs );

			  throw new CypherTypeException( string.Format( "Cannot raise `{0}` to the power of `{1}`", lhsValue.TypeName, rhsValue.TypeName ), null );
		 }

		 public static int TransformToInt( object value )
		 {
			  if ( value == null )
			  {
					throw new CypherTypeException( "Expected a numeric value but got null", null );
			  }
			  if ( value is NumberValue )
			  {
					value = ( ( NumberValue ) value ).asObject();
			  }
			  if ( value is Number )
			  {
					Number number = ( Number ) value;
					if ( number.longValue() > int.MaxValue )
					{
						 throw new CypherTypeException( value.ToString() + " is too large to cast to an int32", null );
					}
					return number.intValue();
			  }
			  throw new CypherTypeException( string.Format( "Expected a numeric value but got {0}", value.ToString() ), null );
		 }

		 public static long TransformToLong( object value )
		 {
			  if ( value == null )
			  {
					throw new CypherTypeException( "Expected a numeric value but got null", null );
			  }
			  if ( value is NumberValue )
			  {
					NumberValue number = ( NumberValue ) value;
					return number.LongValue();
			  }
			  if ( value is Number )
			  {
					Number number = ( Number ) value;
					return number.longValue();
			  }

			  throw new CypherTypeException( string.Format( "Expected a numeric value but got {0}", value.ToString() ), null );
		 }

		 /// <summary>
		 /// Both a1 and a2 must be arrays
		 /// </summary>
		 private static object AddArrays( object a1, object a2 )
		 {
			  int l1 = Array.getLength( a1 );
			  int l2 = Array.getLength( a2 );
			  object[] ret = new object[l1 + l2];
			  for ( int i = 0; i < l1; i++ )
			  {
					ret[i] = Array.get( a1, i );
			  }
			  for ( int i = 0; i < l2; i++ )
			  {
					ret[l1 + i] = Array.get( a2, i );
			  }
			  return ret;
		 }

		 /// <summary>
		 /// array must be an array
		 /// </summary>
		 private static object AddArrayWithObject( object array, object @object )
		 {
			  int l = Array.getLength( array );
			  object[] ret = new object[l + 1];
			  int i = 0;
			  for ( ; i < l; i++ )
			  {
					ret[i] = Array.get( array, i );
			  }
			  ret[i] = @object;

			  return ret;
		 }

		 /// <summary>
		 /// array must be an array
		 /// </summary>
		 private static object AddObjectWithArray( object @object, object array )
		 {
			  int l = Array.getLength( array );
			  object[] ret = new object[l + 1];
			  ret[0] = @object;
			  for ( int i = 1; i < ret.Length; i++ )
			  {
					ret[i] = Array.get( array, i );
			  }

			  return ret;
		 }
	}

}