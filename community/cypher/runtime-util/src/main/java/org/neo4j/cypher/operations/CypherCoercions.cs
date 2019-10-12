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
namespace Org.Neo4j.Cypher.operations
{
	using CypherTypeException = Org.Neo4j.Cypher.@internal.v3_5.util.CypherTypeException;


	using DbAccess = Org.Neo4j.Cypher.@internal.runtime.DbAccess;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using SequenceValue = Org.Neo4j.Values.SequenceValue;
	using Org.Neo4j.Values;
	using ArrayValue = Org.Neo4j.Values.Storable.ArrayValue;
	using BooleanValue = Org.Neo4j.Values.Storable.BooleanValue;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using FloatingPointValue = Org.Neo4j.Values.Storable.FloatingPointValue;
	using IntegralValue = Org.Neo4j.Values.Storable.IntegralValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Values = Org.Neo4j.Values.Storable.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTDuration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTGeometry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTLocalDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTLocalTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTRelationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.SequenceValue_IterationPreference.RANDOM_ACCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public final class CypherCoercions
	public sealed class CypherCoercions
	{
		 private CypherCoercions()
		 {
			  throw new System.NotSupportedException( "do not instantiate" );
		 }

		 public static TextValue AsTextValue( AnyValue value )
		 {
			  if ( !( value is TextValue ) )
			  {
					throw CantCoerce( value, "String" );
			  }
			  return ( TextValue ) value;
		 }

		 public static NodeValue AsNodeValue( AnyValue value )
		 {
			  if ( !( value is NodeValue ) )
			  {
					throw CantCoerce( value, "Node" );
			  }
			  return ( NodeValue ) value;
		 }

		 public static RelationshipValue AsRelationshipValue( AnyValue value )
		 {
			  if ( !( value is RelationshipValue ) )
			  {
					throw CantCoerce( value, "Relationship" );
			  }
			  return ( RelationshipValue ) value;
		 }

		 public static PathValue AsPathValue( AnyValue value )
		 {
			  if ( !( value is PathValue ) )
			  {
					throw CantCoerce( value, "Path" );
			  }
			  return ( PathValue ) value;
		 }

		 public static IntegralValue AsIntegralValue( AnyValue value )
		 {
			  if ( !( value is NumberValue ) )
			  {
					throw CantCoerce( value, "Integer" );
			  }
			  return Values.longValue( ( ( NumberValue ) value ).longValue() );
		 }

		 public static FloatingPointValue AsFloatingPointValue( AnyValue value )
		 {
			  if ( !( value is NumberValue ) )
			  {
					throw CantCoerce( value, "Float" );
			  }
			  return Values.doubleValue( ( ( NumberValue ) value ).doubleValue() );
		 }

		 public static BooleanValue AsBooleanValue( AnyValue value )
		 {
			  if ( !( value is BooleanValue ) )
			  {
					throw CantCoerce( value, "Boolean" );
			  }
			  return ( BooleanValue ) value;
		 }

		 public static NumberValue AsNumberValue( AnyValue value )
		 {
			  if ( !( value is NumberValue ) )
			  {
					throw CantCoerce( value, "Number" );
			  }
			  return ( NumberValue ) value;
		 }

		 public static PointValue AsPointValue( AnyValue value )
		 {
			  if ( !( value is PointValue ) )
			  {
					throw CantCoerce( value, "Point" );
			  }
			  return ( PointValue ) value;
		 }

		 public static DateValue AsDateValue( AnyValue value )
		 {
			  if ( !( value is DateValue ) )
			  {
					throw CantCoerce( value, "Date" );
			  }
			  return ( DateValue ) value;
		 }

		 public static TimeValue AsTimeValue( AnyValue value )
		 {
			  if ( !( value is TimeValue ) )
			  {
					throw CantCoerce( value, "Time" );
			  }
			  return ( TimeValue ) value;
		 }

		 public static LocalTimeValue AsLocalTimeValue( AnyValue value )
		 {
			  if ( !( value is LocalTimeValue ) )
			  {
					throw CantCoerce( value, "LocalTime" );
			  }
			  return ( LocalTimeValue ) value;
		 }

		 public static LocalDateTimeValue AsLocalDateTimeValue( AnyValue value )
		 {
			  if ( !( value is LocalDateTimeValue ) )
			  {
					throw CantCoerce( value, "LocalDateTime" );
			  }
			  return ( LocalDateTimeValue ) value;
		 }

		 public static DateTimeValue AsDateTimeValue( AnyValue value )
		 {
			  if ( !( value is DateTimeValue ) )
			  {
					throw CantCoerce( value, "DateTime" );
			  }
			  return ( DateTimeValue ) value;
		 }

		 public static DurationValue AsDurationValue( AnyValue value )
		 {
			  if ( !( value is DurationValue ) )
			  {
					throw CantCoerce( value, "Duration" );
			  }
			  return ( DurationValue ) value;
		 }

		 public static MapValue AsMapValue( AnyValue value, DbAccess access )
		 {
			  if ( value is MapValue )
			  {
					return ( MapValue ) value;
			  }
			  else if ( value is NodeValue )
			  {
					return access.NodeAsMap( ( ( NodeValue ) value ).id() );
			  }
			  else if ( value is RelationshipValue )
			  {
					return access.RelationshipAsMap( ( ( RelationshipValue ) value ).id() );
			  }
			  else
			  {
					throw CantCoerce( value, "Map" );
			  }
		 }

		 public static ListValue AsList( AnyValue value, Neo4jTypes.AnyType innerType, DbAccess access )
		 {
			  return ( new ListCoercer() ).Apply(value, innerType, access);
		 }

		 private static CypherTypeException CantCoerce( AnyValue value, string type )
		 {
			  return new CypherTypeException( format( "Can't coerce `%s` to %s", value, type ), null );
		 }

		 private class ListMapper : ValueMapper<ListValue>
		 {

			  public override ListValue MapPath( PathValue value )
			  {
					return null;
			  }

			  public override ListValue MapNode( VirtualNodeValue value )
			  {
					return null;
			  }

			  public override ListValue MapRelationship( VirtualRelationshipValue value )
			  {
					return null;
			  }

			  public override ListValue MapMap( MapValue value )
			  {
					return null;
			  }

			  public override ListValue MapNoValue()
			  {
					return null;
			  }

			  public override ListValue MapSequence( SequenceValue value )
			  {
					return null;
			  }

			  public override ListValue MapText( TextValue value )
			  {
					return null;
			  }

			  public override ListValue MapBoolean( BooleanValue value )
			  {
					return null;
			  }

			  public override ListValue MapNumber( NumberValue value )
			  {
					return null;
			  }

			  public override ListValue MapDateTime( DateTimeValue value )
			  {
					return null;
			  }

			  public override ListValue MapLocalDateTime( LocalDateTimeValue value )
			  {
					return null;
			  }

			  public override ListValue MapDate( DateValue value )
			  {
					return null;
			  }

			  public override ListValue MapTime( TimeValue value )
			  {
					return null;
			  }

			  public override ListValue MapLocalTime( LocalTimeValue value )
			  {
					return null;
			  }

			  public override ListValue MapDuration( DurationValue value )
			  {
					return null;
			  }

			  public override ListValue MapPoint( PointValue value )
			  {
					return null;
			  }
		 }

		 delegate AnyValue Coercer( AnyValue value, Neo4jTypes.AnyType coerceTo, DbAccess access );

		 private static readonly IDictionary<Type, Coercer> _converters = new Dictionary<Type, Coercer>();

		 private AnyValue CoerceTo( AnyValue value, DbAccess access, Neo4jTypes.AnyType types )
		 {
			  Coercer function = _converters[types.GetType()];

			  return function( value, types, access );
		 }

		 private class ListCoercer : Coercer
		 {
			  public override ListValue Apply( AnyValue value, Neo4jTypes.AnyType innerType, DbAccess access )
			  {
					//Fast route
					if ( innerType == NTAny )
					{
						 return FastListConversion( value );
					}

					//slow route, recursively convert the list
					if ( !( value is SequenceValue ) )
					{
						 throw CantCoerce( value, "List" );
					}
					SequenceValue listValue = ( SequenceValue ) value;
					Coercer innerCoercer = _converters[innerType.GetType()];
					AnyValue[] coercedValues = new AnyValue[listValue.Length()];
					Neo4jTypes.AnyType nextInner = nextInner( innerType );
					if ( listValue.IterationPreference() == RANDOM_ACCESS )
					{
						 for ( int i = 0; i < coercedValues.Length; i++ )
						 {
							  AnyValue nextItem = listValue.Value( i );
							  coercedValues[i] = nextItem == NO_VALUE ? NO_VALUE : innerCoercer( nextItem, nextInner, access );
						 }
					}
					else
					{
						 int i = 0;
						 foreach ( AnyValue anyValue in listValue )
						 {
							  AnyValue nextItem = listValue.Value( i );
							  coercedValues[i++] = nextItem == NO_VALUE ? NO_VALUE : innerCoercer( anyValue, nextInner, access );
						 }
					}
					return VirtualValues.list( coercedValues );
			  }
		 }

		 private static Neo4jTypes.AnyType NextInner( Neo4jTypes.AnyType type )
		 {
			  if ( type is Neo4jTypes.ListType )
			  {
					return ( ( Neo4jTypes.ListType ) type ).innerType();
			  }
			  else
			  {
					return type;
			  }
		 }

		 private static ListValue FastListConversion( AnyValue value )
		 {
			  if ( value is ListValue )
			  {
					return ( ListValue ) value;
			  }
			  else if ( value is ArrayValue )
			  {
					return VirtualValues.fromArray( ( ArrayValue ) value );
			  }
			  else if ( value is PathValue )
			  {
					return ( ( PathValue ) value ).asList();
			  }
			  throw CantCoerce( value, "List" );
		 }

		 static CypherCoercions()
		 {
			  _converters[NTAny.GetType()] = (a, ignore1, ignore2) => a;
			  _converters[NTString.GetType()] = (a, ignore1, ignore2) => AsTextValue(a);
			  _converters[NTNumber.GetType()] = (a, ignore1, ignore2) => AsNumberValue(a);
			  _converters[NTInteger.GetType()] = (a, ignore1, ignore2) => AsIntegralValue(a);
			  _converters[NTFloat.GetType()] = (a, ignore1, ignore2) => AsFloatingPointValue(a);
			  _converters[NTBoolean.GetType()] = (a, ignore1, ignore2) => AsBooleanValue(a);
			  _converters[NTMap.GetType()] = (a, ignore, c) => AsMapValue(a, c);
			  _converters[NTNode.GetType()] = (a, ignore1, ignore2) => AsNodeValue(a);
			  _converters[NTRelationship.GetType()] = (a, ignore1, ignore2) => AsRelationshipValue(a);
			  _converters[NTPath.GetType()] = (a, ignore1, ignore2) => AsPathValue(a);
			  _converters[NTGeometry.GetType()] = (a, ignore1, ignore2) => AsPointValue(a);
			  _converters[NTPoint.GetType()] = (a, ignore1, ignore2) => AsPointValue(a);
			  _converters[NTDateTime.GetType()] = (a, ignore1, ignore2) => AsDateTimeValue(a);
			  _converters[NTLocalDateTime.GetType()] = (a, ignore1, ignore2) => AsLocalDateTimeValue(a);
			  _converters[NTDate.GetType()] = (a, ignore1, ignore2) => AsDateValue(a);
			  _converters[NTTime.GetType()] = (a, ignore1, ignore2) => AsTimeValue(a);
			  _converters[NTLocalTime.GetType()] = (a, ignore1, ignore2) => AsLocalTimeValue(a);
			  _converters[NTDuration.GetType()] = (a, ignore1, ignore2) => AsDurationValue(a);
			  _converters[typeof( Neo4jTypes.ListType )] = new ListCoercer();
		 }
	}

}