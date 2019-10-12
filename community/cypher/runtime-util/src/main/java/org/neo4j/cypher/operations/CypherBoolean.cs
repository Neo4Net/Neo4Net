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
	using CypherTypeException = Org.Neo4j.Cypher.@internal.v3_5.util.CypherTypeException;
	using InternalException = Org.Neo4j.Cypher.@internal.v3_5.util.InternalException;
	using InvalidSemanticsException = Org.Neo4j.Cypher.@internal.v3_5.util.InvalidSemanticsException;


	using AnyValue = Org.Neo4j.Values.AnyValue;
	using AnyValues = Org.Neo4j.Values.AnyValues;
	using Comparison = Org.Neo4j.Values.Comparison;
	using SequenceValue = Org.Neo4j.Values.SequenceValue;
	using Org.Neo4j.Values;
	using BooleanValue = Org.Neo4j.Values.Storable.BooleanValue;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using FloatingPointValue = Org.Neo4j.Values.Storable.FloatingPointValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.TRUE;

	/// <summary>
	/// This class contains static helper boolean methods used by the compiled expressions
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public final class CypherBoolean
	public sealed class CypherBoolean
	{
		 private static readonly BooleanMapper _booleanMapper = new BooleanMapper();

		 private CypherBoolean()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 public static Value Xor( AnyValue lhs, AnyValue rhs )
		 {
			  return ( lhs == TRUE ) ^ ( rhs == TRUE ) ? TRUE : FALSE;
		 }

		 public static Value Not( AnyValue @in )
		 {
			  return @in != TRUE ? TRUE : FALSE;
		 }

		 public static Value Equals( AnyValue lhs, AnyValue rhs )
		 {
			  bool? compare = lhs.TernaryEquals( rhs );
			  if ( compare == null )
			  {
					return NO_VALUE;
			  }
			  return compare ? TRUE : FALSE;
		 }

		 public static Value NotEquals( AnyValue lhs, AnyValue rhs )
		 {
			  bool? compare = lhs.TernaryEquals( rhs );
			  if ( compare == null )
			  {
					return NO_VALUE;
			  }
			  return compare ? FALSE : TRUE;
		 }

		 public static BooleanValue Regex( TextValue lhs, TextValue rhs )
		 {
			  string regexString = rhs.StringValue();
			  try
			  {
					bool matches = Pattern.compile( regexString ).matcher( lhs.StringValue() ).matches();
					return matches ? TRUE : FALSE;
			  }
			  catch ( PatternSyntaxException e )
			  {
					throw new InvalidSemanticsException( "Invalid Regex: " + e.Message );
			  }
		 }

		 public static BooleanValue Regex( TextValue text, Pattern pattern )
		 {
			  bool matches = pattern.matcher( text.StringValue() ).matches();
			  return matches ? TRUE : FALSE;
		 }

		 public static Value LessThan( AnyValue lhs, AnyValue rhs )
		 {
			  if ( IsNan( lhs ) || IsNan( rhs ) )
			  {
					return NO_VALUE;
			  }
			  else if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TextValue && rhs is TextValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is BooleanValue && rhs is BooleanValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is PointValue && rhs is PointValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateValue && rhs is DateValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalTimeValue && rhs is LocalTimeValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TimeValue && rhs is TimeValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalDateTimeValue && rhs is LocalDateTimeValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateTimeValue && rhs is DateTimeValue )
			  {
					return LessThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 public static Value LessThanOrEqual( AnyValue lhs, AnyValue rhs )
		 {
			  if ( IsNan( lhs ) || IsNan( rhs ) )
			  {
					return NO_VALUE;
			  }
			  else if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TextValue && rhs is TextValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is BooleanValue && rhs is BooleanValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is PointValue && rhs is PointValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateValue && rhs is DateValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalTimeValue && rhs is LocalTimeValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TimeValue && rhs is TimeValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalDateTimeValue && rhs is LocalDateTimeValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateTimeValue && rhs is DateTimeValue )
			  {
					return LessThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 public static Value GreaterThan( AnyValue lhs, AnyValue rhs )
		 {
			  if ( IsNan( lhs ) || IsNan( rhs ) )
			  {
					return NO_VALUE;
			  }
			  else if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TextValue && rhs is TextValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is BooleanValue && rhs is BooleanValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is PointValue && rhs is PointValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateValue && rhs is DateValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalTimeValue && rhs is LocalTimeValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TimeValue && rhs is TimeValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalDateTimeValue && rhs is LocalDateTimeValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateTimeValue && rhs is DateTimeValue )
			  {
					return GreaterThan( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 public static Value GreaterThanOrEqual( AnyValue lhs, AnyValue rhs )
		 {
			  if ( IsNan( lhs ) || IsNan( rhs ) )
			  {
					return NO_VALUE;
			  }
			  else if ( lhs is NumberValue && rhs is NumberValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TextValue && rhs is TextValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is BooleanValue && rhs is BooleanValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is PointValue && rhs is PointValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateValue && rhs is DateValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalTimeValue && rhs is LocalTimeValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is TimeValue && rhs is TimeValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is LocalDateTimeValue && rhs is LocalDateTimeValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else if ( lhs is DateTimeValue && rhs is DateTimeValue )
			  {
					return GreaterThanOrEqual( AnyValues.TERNARY_COMPARATOR.ternaryCompare( lhs, rhs ) );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 public static Value CoerceToBoolean( AnyValue value )
		 {
			  return value.Map( _booleanMapper );
		 }

		 private static Value LessThan( Comparison comparison )
		 {
			  switch ( comparison.innerEnumValue )
			  {
			  case Comparison.InnerEnum.GREATER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.GREATER_THAN:
			  case Comparison.InnerEnum.EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN_AND_EQUAL:
					return FALSE;
			  case Comparison.InnerEnum.SMALLER_THAN:
					return TRUE;
			  case Comparison.InnerEnum.UNDEFINED:
					return NO_VALUE;
			  default:
					throw new InternalException( comparison + " is not a known comparison", null );
			  }
		 }

		 private static Value LessThanOrEqual( Comparison comparison )
		 {
			  switch ( comparison.innerEnumValue )
			  {
			  case Comparison.InnerEnum.GREATER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.GREATER_THAN:
					return FALSE;
			  case Comparison.InnerEnum.EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN:
					return TRUE;
			  case Comparison.InnerEnum.UNDEFINED:
					return NO_VALUE;
			  default:
					throw new InternalException( comparison + " is not a known comparison", null );
			  }
		 }

		 private static Value GreaterThanOrEqual( Comparison comparison )
		 {
			  switch ( comparison.innerEnumValue )
			  {
			  case Comparison.InnerEnum.GREATER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.GREATER_THAN:
			  case Comparison.InnerEnum.EQUAL:
					return TRUE;
			  case Comparison.InnerEnum.SMALLER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN:
					return FALSE;
			  case Comparison.InnerEnum.UNDEFINED:
					return NO_VALUE;
			  default:
					throw new InternalException( comparison + " is not a known comparison", null );
			  }
		 }

		 private static Value GreaterThan( Comparison comparison )
		 {
			  switch ( comparison.innerEnumValue )
			  {
			  case Comparison.InnerEnum.GREATER_THAN:
					return TRUE;
			  case Comparison.InnerEnum.GREATER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN_AND_EQUAL:
			  case Comparison.InnerEnum.SMALLER_THAN:
					return FALSE;
			  case Comparison.InnerEnum.UNDEFINED:
					return NO_VALUE;
			  default:
					throw new InternalException( comparison + " is not a known comparison", null );
			  }
		 }

		 private static bool IsNan( AnyValue value )
		 {
			  return value is FloatingPointValue && ( ( FloatingPointValue ) value ).NaN;
		 }

		 private sealed class BooleanMapper : ValueMapper<Value>
		 {
			  public override Value MapPath( PathValue value )
			  {
					return value.Size() > 0 ? TRUE : FALSE;
			  }

			  public override Value MapNode( VirtualNodeValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapRelationship( VirtualRelationshipValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapMap( MapValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapNoValue()
			  {
					return NO_VALUE;
			  }

			  public override Value MapSequence( SequenceValue value )
			  {
					return value.Length() > 0 ? TRUE : FALSE;
			  }

			  public override Value MapText( TextValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapBoolean( BooleanValue value )
			  {
					return value;
			  }

			  public override Value MapNumber( NumberValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapDateTime( DateTimeValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapLocalDateTime( LocalDateTimeValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapDate( DateValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapTime( TimeValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapLocalTime( LocalTimeValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapDuration( DurationValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }

			  public override Value MapPoint( PointValue value )
			  {
					throw new CypherTypeException( "Don't know how to treat that as a boolean: " + value, null );
			  }
		 }
	}

}