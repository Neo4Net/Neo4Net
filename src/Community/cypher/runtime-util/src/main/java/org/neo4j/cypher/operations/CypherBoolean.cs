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
	using CypherTypeException = Neo4Net.Cypher.@internal.v3_5.util.CypherTypeException;
	using InternalException = Neo4Net.Cypher.@internal.v3_5.util.InternalException;
	using InvalidSemanticsException = Neo4Net.Cypher.@internal.v3_5.util.InvalidSemanticsException;


	using AnyValue = Neo4Net.Values.AnyValue;
	using AnyValues = Neo4Net.Values.AnyValues;
	using Comparison = Neo4Net.Values.Comparison;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using Neo4Net.Values;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using FloatingPointValue = Neo4Net.Values.Storable.FloatingPointValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;

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