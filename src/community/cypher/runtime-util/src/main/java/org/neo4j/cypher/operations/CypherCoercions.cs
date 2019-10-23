using System;
using System.Collections.Generic;

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
	using CypherTypeException = Neo4Net.Cypher.Internal.v3_5.util.CypherTypeException;


	using DbAccess = Neo4Net.Cypher.Internal.runtime.DbAccess;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using AnyValue = Neo4Net.Values.AnyValue;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using Neo4Net.Values;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using FloatingPointValue = Neo4Net.Values.Storable.FloatingPointValue;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDuration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTGeometry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTLocalDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTLocalTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTRelationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.SequenceValue_IterationPreference.RANDOM_ACCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

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

		 public static ListValue AsList( AnyValue value, Neo4NetTypes.AnyType innerType, DbAccess access )
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

		 delegate AnyValue Coercer( AnyValue value, Neo4NetTypes.AnyType coerceTo, DbAccess access );

		 private static readonly IDictionary<Type, Coercer> _converters = new Dictionary<Type, Coercer>();

		 private AnyValue CoerceTo( AnyValue value, DbAccess access, Neo4NetTypes.AnyType types )
		 {
			  Coercer function = _converters[types.GetType()];

			  return function( value, types, access );
		 }

		 private class ListCoercer : Coercer
		 {
			  public override ListValue Apply( AnyValue value, Neo4NetTypes.AnyType innerType, DbAccess access )
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
					Neo4NetTypes.AnyType nextInner = nextInner( innerType );
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

		 private static Neo4NetTypes.AnyType NextInner( Neo4NetTypes.AnyType type )
		 {
			  if ( type is Neo4NetTypes.ListType )
			  {
					return ( ( Neo4NetTypes.ListType ) type ).innerType();
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
			  _converters[typeof( Neo4NetTypes.ListType )] = new ListCoercer();
		 }
	}

}