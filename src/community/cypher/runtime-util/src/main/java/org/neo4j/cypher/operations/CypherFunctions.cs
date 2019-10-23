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
	using CypherTypeException = Neo4Net.Cypher.Internal.v3_5.util.CypherTypeException;
	using InvalidArgumentException = Neo4Net.Cypher.Internal.v3_5.util.InvalidArgumentException;
	using ParameterWrongTypeException = Neo4Net.Cypher.Internal.v3_5.util.ParameterWrongTypeException;


	using DbAccess = Neo4Net.Cypher.Internal.runtime.DbAccess;
	using AnyValue = Neo4Net.Values.AnyValue;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static double.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.EMPTY_STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_LIST;

	/// <summary>
	/// This class contains static helper methods for the set of Cypher functions
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public final class CypherFunctions
	public sealed class CypherFunctions
	{
		 private static readonly decimal _maxLong = decimal.ValueOf( long.MaxValue );
		 private static readonly decimal _minLong = decimal.ValueOf( long.MinValue );
		 private static string[] _pointKeys = new string[]{ "crs", "x", "y", "z", "longitude", "latitude", "height", "srid" };

		 private CypherFunctions()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 public static DoubleValue Sin( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Sin( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "sin()" );
			  }
		 }

		 public static DoubleValue Asin( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Asin( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "asin()" );
			  }
		 }

		 public static DoubleValue Haversin( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( ( 1.0 - Math.Cos( ( ( NumberValue ) @in ).doubleValue() ) ) / 2 );
			  }
			  else
			  {
					throw NeedsNumbers( "haversin()" );
			  }
		 }

		 public static DoubleValue Cos( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Cos( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "cos()" );
			  }
		 }

		 public static DoubleValue Cot( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( 1.0 / Math.Tan( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "cot()" );
			  }
		 }

		 public static DoubleValue Acos( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Acos( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "acos()" );
			  }
		 }

		 public static DoubleValue Tan( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Tan( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "tan()" );
			  }
		 }

		 public static DoubleValue Atan( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Atan( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "atan()" );
			  }
		 }

		 public static DoubleValue Atan2( AnyValue y, AnyValue x )
		 {
			  if ( y is NumberValue && x is NumberValue )
			  {
					return doubleValue( Math.Atan2( ( ( NumberValue ) y ).doubleValue(), ((NumberValue) x).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "atan2()" );
			  }
		 }

		 public static DoubleValue Ceil( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Ceiling( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "ceil()" );
			  }
		 }

		 public static DoubleValue Floor( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Floor( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "floor()" );
			  }
		 }

		 public static DoubleValue Round( AnyValue @in )
		 {
			 if ( @in is NumberValue )
			 {
					return doubleValue( ( long )Math.Round( ( ( NumberValue ) @in ).doubleValue(), MidpointRounding.AwayFromZero ) );
			 }
			  else
			  {
					throw NeedsNumbers( "round()" );
			  }
		 }

		 public static NumberValue Abs( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					if ( @in is IntegralValue )
					{
						 return longValue( Math.Abs( ( ( NumberValue ) @in ).longValue() ) );
					}
					else
					{
						 return doubleValue( Math.Abs( ( ( NumberValue ) @in ).doubleValue() ) );
					}
			  }
			  else
			  {
					throw NeedsNumbers( "abs()" );
			  }
		 }

		 public static DoubleValue ToDegrees( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.toDegrees( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "toDegrees()" );
			  }
		 }

		 public static DoubleValue Exp( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Exp( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "exp()" );
			  }
		 }

		 public static DoubleValue Log( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Log( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "log()" );
			  }
		 }

		 public static DoubleValue Log10( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Log10( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "log10()" );
			  }
		 }

		 public static DoubleValue ToRadians( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.toRadians( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "toRadians()" );
			  }
		 }

		 public static ListValue Range( AnyValue startValue, AnyValue endValue )
		 {
			  return VirtualValues.range( AsLong( startValue ), AsLong( endValue ), 1L );
		 }

		 public static ListValue Range( AnyValue startValue, AnyValue endValue, AnyValue stepValue )
		 {
			  long step = AsLong( stepValue );
			  if ( step == 0L )
			  {
					throw new InvalidArgumentException( "step argument to range() cannot be zero", null );
			  }

			  return VirtualValues.range( AsLong( startValue ), AsLong( endValue ), step );
		 }

		 public static LongValue Signum( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return longValue( ( long ) Math.Sign( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "signum()" );
			  }
		 }

		 public static DoubleValue Sqrt( AnyValue @in )
		 {
			  if ( @in is NumberValue )
			  {
					return doubleValue( Math.Sqrt( ( ( NumberValue ) @in ).doubleValue() ) );
			  }
			  else
			  {
					throw NeedsNumbers( "sqrt()" );
			  }
		 }

		 public static DoubleValue Rand()
		 {
			  return doubleValue( ThreadLocalRandom.current().NextDouble() );
		 }

		 // TODO: Support better calculations, like https://en.wikipedia.org/wiki/Vincenty%27s_formulae
		 // TODO: Support more coordinate systems
		 public static Value Distance( AnyValue lhs, AnyValue rhs )
		 {
			  if ( lhs is PointValue && rhs is PointValue )
			  {
					return CalculateDistance( ( PointValue ) lhs, ( PointValue ) rhs );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 public static NodeValue StartNode( AnyValue anyValue, DbAccess access )
		 {
			  if ( anyValue is RelationshipValue )
			  {
					return access.RelationshipGetStartNode( ( RelationshipValue ) anyValue );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a RelationshipValue", anyValue ), null );
			  }
		 }

		 public static NodeValue EndNode( AnyValue anyValue, DbAccess access )
		 {
			  if ( anyValue is RelationshipValue )
			  {
					return access.RelationshipGetEndNode( ( RelationshipValue ) anyValue );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a RelationshipValue", anyValue ), null );
			  }
		 }

		 public static BooleanValue PropertyExists( string key, AnyValue container, DbAccess dbAccess )
		 {
			  if ( container is VirtualNodeValue )
			  {
					return dbAccess.NodeHasProperty( ( ( VirtualNodeValue ) container ).id(), dbAccess.PropertyKey(key) ) ? TRUE : FALSE;
			  }
			  else if ( container is VirtualRelationshipValue )
			  {
					return dbAccess.RelationshipHasProperty( ( ( VirtualRelationshipValue ) container ).id(), dbAccess.PropertyKey(key) ) ? TRUE : FALSE;
			  }
			  else if ( container is MapValue )
			  {
					return ( ( MapValue ) container ).get( key ) != NO_VALUE ? TRUE : FALSE;
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a property container", container ), null );
			  }
		 }

		 public static AnyValue PropertyGet( string key, AnyValue container, DbAccess dbAccess )
		 {
			  if ( container is VirtualNodeValue )
			  {
					return dbAccess.NodeProperty( ( ( VirtualNodeValue ) container ).id(), dbAccess.PropertyKey(key) );
			  }
			  else if ( container is VirtualRelationshipValue )
			  {
					return dbAccess.RelationshipProperty( ( ( VirtualRelationshipValue ) container ).id(), dbAccess.PropertyKey(key) );
			  }
			  else if ( container is MapValue )
			  {
					return ( ( MapValue ) container ).get( key );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (container instanceof org.Neo4Net.values.storable.TemporalValue<?,?>)
			  else if ( container is TemporalValue<object, ?> )
			  {
					return ( ( TemporalValue ) container ).get( key );
			  }
			  else if ( container is DurationValue )
			  {
					return ( ( DurationValue ) container ).get( key );
			  }
			  else if ( container is PointValue )
			  {
					try
					{
						 return ( ( PointValue ) container ).get( key );
					}
					catch ( InvalidValuesArgumentException e )
					{
						 throw new InvalidArgumentException( e.Message, e );
					}
			  }
			  else
			  {
					throw new CypherTypeException( format( "Type mismatch: expected a map but was %s", container.ToString() ), null );
			  }
		 }

		 public static AnyValue ContainerIndex( AnyValue container, AnyValue index, DbAccess dbAccess )
		 {
			  if ( container is VirtualNodeValue )
			  {
					return dbAccess.NodeProperty( ( ( VirtualNodeValue ) container ).id(), dbAccess.PropertyKey(AsString(index)) );
			  }
			  else if ( container is VirtualRelationshipValue )
			  {
					return dbAccess.RelationshipProperty( ( ( VirtualRelationshipValue ) container ).id(), dbAccess.PropertyKey(AsString(index)) );
			  }
			  if ( container is MapValue )
			  {
					return MapAccess( ( MapValue ) container, index );
			  }
			  else if ( container is SequenceValue )
			  {
					return ListAccess( ( SequenceValue ) container, index );
			  }
			  else
			  {
					throw new CypherTypeException( format( "`%s` is not a collection or a map. Element access is only possible by performing a collection " + "lookup using an integer index, or by performing a map lookup using a string key (found: %s[%s])", container, container, index ), null );
			  }
		 }

		 public static AnyValue Head( AnyValue container )
		 {
			  if ( container is SequenceValue )
			  {
					SequenceValue sequence = ( SequenceValue ) container;
					if ( sequence.Length() == 0 )
					{
						 return NO_VALUE;
					}

					return sequence.Value( 0 );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a list", container ), null );
			  }
		 }

		 public static ListValue Tail( AnyValue container )
		 {
			 if ( container is ListValue )
			 {
				  return ( ( ListValue ) container ).tail();
			 }
			 else if ( container is ArrayValue )
			 {
				  return VirtualValues.fromArray( ( ArrayValue ) container ).tail();
			 }
			 else
			 {
				  return EMPTY_LIST;
			 }
		 }

		 public static AnyValue Last( AnyValue container )
		 {
			  if ( container is SequenceValue )
			  {
					SequenceValue sequence = ( SequenceValue ) container;
					int length = sequence.Length();
					if ( length == 0 )
					{
						 return NO_VALUE;
					}

					return sequence.Value( length - 1 );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a list", container ), null );
			  }
		 }

		 public static TextValue Left( AnyValue @in, AnyValue endPos )
		 {
			  if ( @in is TextValue )
			  {
					int len = AsInt( endPos );
					return ( ( TextValue ) @in ).substring( 0, len );
			  }
			  else
			  {
					throw NotAString( "left", @in );
			  }
		 }

		 public static TextValue Ltrim( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( ( TextValue ) @in ).ltrim();
			  }
			  else
			  {
					throw NotAString( "ltrim", @in );
			  }
		 }

		 public static TextValue Rtrim( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( ( TextValue ) @in ).rtrim();
			  }
			  else
			  {
					throw NotAString( "rtrim", @in );
			  }
		 }

		 public static TextValue Trim( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( ( TextValue ) @in ).trim();
			  }
			  else
			  {
					throw NotAString( "trim", @in );
			  }
		 }

		 public static TextValue Replace( AnyValue original, AnyValue search, AnyValue replaceWith )
		 {
			  if ( original is TextValue )
			  {
					return ( ( TextValue ) original ).replace( AsString( search ), AsString( replaceWith ) );
			  }
			  else
			  {
					throw NotAString( "replace", original );
			  }
		 }

		 public static AnyValue Reverse( AnyValue original )
		 {
			  if ( original is TextValue )
			  {
					return ( ( TextValue ) original ).reverse();
			  }
			  else if ( original is ListValue )
			  {
					return ( ( ListValue ) original ).reverse();
			  }
			  else
			  {
					throw new CypherTypeException( "Expected a string or a list; consider converting it to a string with toString() or creating a list.", null );
			  }
		 }

		 public static TextValue Right( AnyValue original, AnyValue length )
		 {
			  if ( original is TextValue )
			  {
					TextValue asText = ( TextValue ) original;
					int len = AsInt( length );
					if ( len < 0 )
					{
						 throw new System.IndexOutOfRangeException( "negative length" );
					}
					int startVal = asText.Length() - len;
					return asText.Substring( Math.Max( 0, startVal ) );
			  }
			  else
			  {
					throw NotAString( "right", original );
			  }
		 }

		 public static ListValue Split( AnyValue original, AnyValue separator )
		 {
			  if ( original is TextValue )
			  {
					TextValue asText = ( TextValue ) original;
					if ( asText.Length() == 0 )
					{
						 return VirtualValues.list( EMPTY_STRING );
					}
					return asText.Split( AsString( separator ) );
			  }
			  else
			  {
					throw NotAString( "split", original );
			  }
		 }

		 public static TextValue Substring( AnyValue original, AnyValue start )
		 {
			  if ( original is TextValue )
			  {
					TextValue asText = ( TextValue ) original;

					return asText.Substring( AsInt( start ) );
			  }
			  else
			  {
					throw NotAString( "substring", original );
			  }
		 }

		 public static TextValue Substring( AnyValue original, AnyValue start, AnyValue length )
		 {
			  if ( original is TextValue )
			  {
					TextValue asText = ( TextValue ) original;

					return StringHelper.SubstringSpecial( asText, AsInt( start ), AsInt( length ) );
			  }
			  else
			  {
					throw NotAString( "substring", original );
			  }
		 }

		 public static TextValue ToLower( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( ( TextValue ) @in ).toLower();
			  }
			  else
			  {
					throw NotAString( "toLower", @in );
			  }
		 }

		 public static TextValue ToUpper( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( ( TextValue ) @in ).toUpper();
			  }
			  else
			  {
					throw NotAString( "toUpper", @in );
			  }
		 }

		 public static LongValue Id( AnyValue item )
		 {
			  if ( item is VirtualNodeValue )
			  {
					return longValue( ( ( VirtualNodeValue ) item ).id() );
			  }
			  else if ( item is VirtualRelationshipValue )
			  {
					return longValue( ( ( VirtualRelationshipValue ) item ).id() );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a node or relationship, but it was `%s`", item, item.GetType().Name ), null );

			  }
		 }

		 public static ListValue Labels( AnyValue item, DbAccess access )
		 {
			  if ( item is NodeValue )
			  {
					return access.GetLabelsForNode( ( ( NodeValue ) item ).id() );
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a Node, got: " + item, null );
			  }
		 }

		 public static bool HasLabel( AnyValue IEntity, int labelToken, DbAccess access )
		 {
			  if ( IEntity is NodeValue )
			  {
					return access.IsLabelSetOnNode( labelToken, ( ( NodeValue ) IEntity ).id() );
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a Node, got: " + IEntity, null );
			  }
		 }

		 public static TextValue Type( AnyValue item )
		 {
			  if ( item is RelationshipValue )
			  {
					return ( ( RelationshipValue ) item ).type();
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a Relationship, got: " + item, null );
			  }
		 }

		 public static ListValue Nodes( AnyValue @in )
		 {
			  if ( @in is PathValue )
			  {
					return VirtualValues.list( ( ( PathValue ) @in ).nodes() );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a path.", @in ), null );
			  }
		 }

		 public static ListValue Relationships( AnyValue @in )
		 {
			  if ( @in is PathValue )
			  {
					return VirtualValues.list( ( ( PathValue ) @in ).relationships() );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected %s to be a path.", @in ), null );
			  }
		 }

		 public static Value Point( AnyValue @in, DbAccess access )
		 {
			  if ( @in is VirtualNodeValue )
			  {
					return AsPoint( access, ( VirtualNodeValue ) @in );
			  }
			  else if ( @in is VirtualRelationshipValue )
			  {
					return AsPoint( access, ( VirtualRelationshipValue ) @in );
			  }
			  else if ( @in is MapValue )
			  {
					MapValue map = ( MapValue ) @in;
					if ( ContainsNull( map ) )
					{
						 return NO_VALUE;
					}
					return PointValue.fromMap( map );
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected a map but got %s", @in ), null );
			  }
		 }

		 public static ListValue Keys( AnyValue @in, DbAccess access )
		 {
			  if ( @in is VirtualNodeValue )
			  {
					return ExtractKeys( access, access.NodePropertyIds( ( ( VirtualNodeValue ) @in ).id() ) );
			  }
			  else if ( @in is VirtualRelationshipValue )
			  {
					return ExtractKeys( access, access.RelationshipPropertyIds( ( ( VirtualRelationshipValue ) @in ).id() ) );
			  }
			  else if ( @in is MapValue )
			  {
					return ( ( MapValue ) @in ).keys();
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected a node, a relationship or a literal map but got %s", @in ), null );
			  }
		 }

		 public static MapValue Properties( AnyValue @in, DbAccess access )
		 {
			  if ( @in is VirtualNodeValue )
			  {
					return access.NodeAsMap( ( ( VirtualNodeValue ) @in ).id() );
			  }
			  else if ( @in is VirtualRelationshipValue )
			  {
				  return access.RelationshipAsMap( ( ( VirtualRelationshipValue ) @in ).id() );
			  }
			  else if ( @in is MapValue )
			  {
					return ( MapValue ) @in;
			  }
			  else
			  {
					throw new CypherTypeException( format( "Expected a node, a relationship or a literal map but got %s", @in ), null );
			  }
		 }

		 public static IntegralValue Size( AnyValue item )
		 {
			  if ( item is PathValue )
			  {
					throw new CypherTypeException( "SIZE cannot be used on paths", null );
			  }
			  else if ( item is TextValue )
			  {
					return longValue( ( ( TextValue ) item ).length() );
			  }
			  else if ( item is SequenceValue )
			  {
					return longValue( ( ( SequenceValue ) item ).length() );
			  }
			  else
			  {
					return longValue( 1 );
			  }
		 }

		 //NOTE all usage except for paths is deprecated
		 public static IntegralValue Length( AnyValue item )
		 {
			  if ( item is PathValue )
			  {
					return longValue( ( ( PathValue ) item ).size() );
			  }
			  else if ( item is TextValue )
			  {
					return longValue( ( ( TextValue ) item ).length() );
			  }
			  else if ( item is SequenceValue )
			  {
					return longValue( ( ( SequenceValue ) item ).length() );
			  }
			  else
			  {
					return longValue( 1 );
			  }
		 }

		 public static Value ToBoolean( AnyValue @in )
		 {
			  if ( @in is BooleanValue )
			  {
					return ( BooleanValue ) @in;
			  }
			  else if ( @in is TextValue )
			  {
					switch ( ( ( TextValue ) @in ).trim().stringValue().ToLower() )
					{
					case "true":
						 return TRUE;
					case "false":
						 return FALSE;
					default:
						 return NO_VALUE;
					}
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a Boolean or String, got: " + @in.ToString(), null );
			  }
		 }

		 public static Value ToFloat( AnyValue @in )
		 {
			  if ( @in is DoubleValue )
			  {
					return ( DoubleValue ) @in;
			  }
			  else if ( @in is NumberValue )
			  {
					return doubleValue( ( ( NumberValue ) @in ).doubleValue() );
			  }
			  else if ( @in is TextValue )
			  {
					try
					{
						 return doubleValue( parseDouble( ( ( TextValue ) @in ).stringValue() ) );
					}
					catch ( System.FormatException )
					{
						 return NO_VALUE;
					}
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a String or Number, got: " + @in.ToString(), null );
			  }
		 }

		 public static Value ToInteger( AnyValue @in )
		 {
			  if ( @in is IntegralValue )
			  {
					return ( IntegralValue ) @in;
			  }
			  else if ( @in is NumberValue )
			  {
					return longValue( ( ( NumberValue ) @in ).longValue() );
			  }
			  else if ( @in is TextValue )
			  {
					return StringToLongValue( ( TextValue ) @in );
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a String or Number, got: " + @in.ToString(), null );
			  }
		 }

		 public static TextValue ToString( AnyValue @in )
		 {
			  if ( @in is TextValue )
			  {
					return ( TextValue ) @in;
			  }
			  else if ( @in is NumberValue )
			  {
					return stringValue( ( ( NumberValue ) @in ).prettyPrint() );
			  }
			  else if ( @in is BooleanValue )
			  {
					return stringValue( ( ( BooleanValue ) @in ).prettyPrint() );
			  }
			  else if ( @in is TemporalValue || @in is DurationValue || @in is PointValue )
			  {
					return stringValue( @in.ToString() );
			  }
			  else
			  {
					throw new ParameterWrongTypeException( "Expected a String, Number, Boolean, Temporal or Duration, got: " + @in.ToString(), null );
			  }
		 }

		 public static ListValue FromSlice( AnyValue collection, AnyValue fromValue )
		 {
			 int from = AsInt( fromValue );
			 ListValue list = MakeTraversable( collection );
			 if ( from >= 0 )
			 {
				  return list.Drop( from );
			 }
			 else
			 {
				  return list.Drop( list.Size() + from );
			 }
		 }

		 public static ListValue ToSlice( AnyValue collection, AnyValue fromValue )
		 {
			  int from = AsInt( fromValue );
			  ListValue list = MakeTraversable( collection );
			  if ( from >= 0 )
			  {
					return list.Take( from );
			  }
			  else
			  {
					return list.Take( list.Size() + from );
			  }
		 }

		 public static ListValue FullSlice( AnyValue collection, AnyValue fromValue, AnyValue toValue )
		 {
			  int from = AsInt( fromValue );
			  int to = AsInt( toValue );
			  ListValue list = MakeTraversable( collection );
			  int size = list.Size();
			  if ( from >= 0 && to >= 0 )
			  {
					return list.Slice( from, to );
			  }
			  else if ( from >= 0 )
			  {
					return list.Slice( from, size + to );
			  }
			  else if ( to >= 0 )
			  {
					return list.Slice( size + from, to );
			  }
			  else
			  {
					return list.Slice( size + from, size + to );
			  }
		 }

		 public static ListValue MakeTraversable( AnyValue collection )
		 {
			  ListValue list;
			  if ( collection == NO_VALUE )
			  {
					return VirtualValues.EMPTY_LIST;
			  }
			  else if ( collection is ListValue )
			  {
					return ( ListValue ) collection;
			  }
			  else if ( collection is ArrayValue )
			  {
					return VirtualValues.fromArray( ( ArrayValue ) collection );
			  }
			  else
			  {
					return VirtualValues.list( collection );
			  }
		 }

		 private static Value StringToLongValue( TextValue @in )
		 {
			  try
			  {
					return longValue( parseLong( @in.StringValue() ) );
			  }

			  catch ( Exception )
			  {
					try
					{
						 decimal bigDecimal = new decimal( @in.StringValue() );
						 if ( bigDecimal.CompareTo( _maxLong ) <= 0 && bigDecimal.CompareTo( _minLong ) >= 0 )
						 {
							  return longValue( bigDecimal.longValue() );
						 }
						 else
						 {
							  throw new CypherTypeException( format( "integer, %s, is too large", @in.StringValue() ), null );
						 }
					}
					catch ( System.FormatException )
					{
						 return NO_VALUE;
					}
			  }
		 }

		 private static ListValue ExtractKeys( DbAccess access, int[] keyIds )
		 {
			  string[] keysNames = new string[keyIds.Length];
			  for ( int i = 0; i < keyIds.Length; i++ )
			  {
					keysNames[i] = access.GetPropertyKeyName( keyIds[i] );
			  }
			  return VirtualValues.fromArray( Values.stringArray( keysNames ) );
		 }

		 private static Value AsPoint( DbAccess access, VirtualNodeValue nodeValue )
		 {
			  MapValueBuilder builder = new MapValueBuilder();
			  foreach ( string key in _pointKeys )
			  {
					Value value = access.NodeProperty( nodeValue.Id(), access.PropertyKey(key) );
					if ( value == NO_VALUE )
					{
						 continue;
					}
					builder.Add( key, value );
			  }

			  return PointValue.fromMap( builder.Build() );
		 }

		 private static Value AsPoint( DbAccess access, VirtualRelationshipValue relationshipValue )
		 {
			  MapValueBuilder builder = new MapValueBuilder();
			  foreach ( string key in _pointKeys )
			  {
					Value value = access.RelationshipProperty( relationshipValue.Id(), access.PropertyKey(key) );
					if ( value == NO_VALUE )
					{
						 continue;
					}
					builder.Add( key, value );
			  }

			  return PointValue.fromMap( builder.Build() );
		 }

		 private static bool ContainsNull( MapValue map )
		 {
			  bool[] hasNull = new bool[] { false };
			  map.Foreach((s, value) =>
			  {
			  if ( value == NO_VALUE )
			  {
				  hasNull[0] = true;
			  }
			  });
			  return hasNull[0];
		 }

		 private static AnyValue ListAccess( SequenceValue container, AnyValue index )
		 {
			  NumberValue number = AsNumberValue( index );
			  if ( !( number is IntegralValue ) )
			  {
					throw new CypherTypeException( format( "Cannot index a list using an non-integer number, got %s", number ), null );
			  }
			  long idx = number.LongValue();
			  if ( idx > int.MaxValue || idx < int.MinValue )
			  {
					throw new InvalidArgumentException( format( "Cannot index a list using a value greater than %d or lesser than %d, got %d", int.MaxValue, int.MinValue, idx ), null );
			  }

			  if ( idx < 0 )
			  {
					idx = container.Length() + idx;
			  }
			  if ( idx >= container.Length() || idx < 0 )
			  {
					return NO_VALUE;
			  }
			  return container.Value( ( int ) idx );
		 }

		 private static AnyValue MapAccess( MapValue container, AnyValue index )
		 {

			  return container.Get( AsString( index ) );
		 }

		 public static TextValue AsTextValue( AnyValue value )
		 {
			  if ( !( value is TextValue ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new CypherTypeException( format( "Expected %s to be a %s, but it was a %s", value, typeof( TextValue ).FullName, value.GetType().FullName ), null );
			  }
			  return ( TextValue ) value;
		 }

		 internal static string AsString( AnyValue value )
		 {
			 return AsTextValue( value ).stringValue();
		 }

		 private static NumberValue AsNumberValue( AnyValue value )
		 {
			  if ( !( value is NumberValue ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new CypherTypeException( format( "Expected %s to be a %s, but it was a %s", value, typeof( NumberValue ).FullName, value.GetType().FullName ), null );
			  }
			  return ( NumberValue ) value;
		 }

		 private static Value CalculateDistance( PointValue p1, PointValue p2 )
		 {
			  if ( p1.CoordinateReferenceSystem.Equals( p2.CoordinateReferenceSystem ) )
			  {
					return doubleValue( p1.CoordinateReferenceSystem.Calculator.distance( p1, p2 ) );
			  }
			  else
			  {
					return NO_VALUE;
			  }
		 }

		 private static long AsLong( AnyValue value )
		 {
			  if ( value is NumberValue )
			  {
					return ( ( NumberValue ) value ).longValue();
			  }
			  else
			  {
					throw new CypherTypeException( "Expected a numeric value but got: " + value.ToString(), null );
			  }
		 }

		 private static int AsInt( AnyValue value )
		 {
			  return ( int ) AsLong( value );
		 }

		 private static CypherTypeException NeedsNumbers( string method )
		 {
			  return new CypherTypeException( format( "%s requires numbers", method ), null );
		 }

		 private static CypherTypeException NotAString( string method, AnyValue @in )
		 {
			  return new CypherTypeException( format( "Expected a string value for `%s`, but got: %s; consider converting it to a string with " + "toString().", method, @in ), null );
		 }
	}

}