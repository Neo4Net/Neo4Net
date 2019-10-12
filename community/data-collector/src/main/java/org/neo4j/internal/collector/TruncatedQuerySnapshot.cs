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
namespace Org.Neo4j.@internal.Collector
{

	using ExecutionPlanDescription = Org.Neo4j.Graphdb.ExecutionPlanDescription;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using SequenceValue = Org.Neo4j.Values.SequenceValue;
	using Org.Neo4j.Values;
	using BooleanValue = Org.Neo4j.Values.Storable.BooleanValue;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Values = Org.Neo4j.Values.Storable.Values;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

	/// <summary>
	/// Variant of QuerySnapshot that truncates queryText and queryParameter data to limit the memory footprint of
	/// constant query collection. This is crucial to avoid bloating memory use for data import scenarios, and in general
	/// to avoid hogging lot's of memory that will be long-lived and likely tenured.
	/// </summary>
	internal class TruncatedQuerySnapshot
	{
		 internal readonly int FullQueryTextHash;
		 internal readonly string QueryText;
		 internal readonly System.Func<ExecutionPlanDescription> QueryPlanSupplier;
		 internal readonly MapValue QueryParameters;
		 internal readonly long? ElapsedTimeMicros;
		 internal readonly long? CompilationTimeMicros;
		 internal readonly long? StartTimestampMillis;

		 internal TruncatedQuerySnapshot( string fullQueryText, System.Func<ExecutionPlanDescription> queryPlanSupplier, MapValue queryParameters, long? elapsedTimeMicros, long? compilationTimeMicros, long? startTimestampMillis, int maxQueryTextLength )
		 {
			  this.FullQueryTextHash = fullQueryText.GetHashCode();
			  this.QueryText = TruncateQueryText( fullQueryText, maxQueryTextLength );
			  this.QueryPlanSupplier = queryPlanSupplier;
			  this.QueryParameters = TruncateParameters( queryParameters );
			  this.ElapsedTimeMicros = elapsedTimeMicros;
			  this.CompilationTimeMicros = compilationTimeMicros;
			  this.StartTimestampMillis = startTimestampMillis;
		 }

		 private static string TruncateQueryText( string queryText, int maxLength )
		 {
			  return queryText.Length > maxLength ? queryText.Substring( 0, maxLength ) : queryText;
		 }

		 private static MapValue TruncateParameters( MapValue parameters )
		 {
			  string[] keys = new string[parameters.Size()];
			  AnyValue[] values = new AnyValue[keys.Length];

			  int i = 0;
			  foreach ( string key in parameters.Keys )
			  {
					keys[i] = key.Length <= _maxParameterKeyLength ? key : key.Substring( 0, _maxParameterKeyLength );
					values[i] = parameters.Get( key ).map( _valueTruncater );
					i++;
			  }

			  return VirtualValues.map( keys, values );
		 }

		 private static ValueTruncater _valueTruncater = new ValueTruncater();
		 private static int _maxTextParameterLength = 100;
		 private static int _maxParameterKeyLength = 1000;

		 internal class ValueTruncater : ValueMapper<AnyValue>
		 {

			  public override AnyValue MapPath( PathValue value )
			  {
					return Values.stringValue( "§PATH[" + value.Size() + "]" );
			  }

			  public override AnyValue MapNode( VirtualNodeValue value )
			  {
					if ( value is NodeValue )
					{
						 // Note: we do not want to keep a reference to the whole node value as it could contain a lot of data.
						 return VirtualValues.node( value.Id() );
					}
					return value;
			  }

			  public override AnyValue MapRelationship( VirtualRelationshipValue value )
			  {
					if ( value is RelationshipValue )
					{
						 // Note: we do not want to keep a reference to the whole relationship value as it could contain a lot of data.
						 return VirtualValues.relationship( value.Id() );
					}
					return value;
			  }

			  public override AnyValue MapMap( MapValue map )
			  {
					return Values.stringValue( "§MAP[" + map.Size() + "]" );
			  }

			  public override AnyValue MapNoValue()
			  {
					return Values.NO_VALUE;
			  }

			  public override AnyValue MapSequence( SequenceValue value )
			  {
					return Values.stringValue( "§LIST[" + value.Length() + "]" );
			  }

			  public override AnyValue MapText( TextValue value )
			  {
					if ( value.Length() > _maxTextParameterLength )
					{
						 return Values.stringValue( value.StringValue().Substring(0, _maxTextParameterLength) );
					}
					return value;
			  }

			  public override AnyValue MapBoolean( BooleanValue value )
			  {
					return value;
			  }

			  public override AnyValue MapNumber( NumberValue value )
			  {
					return value;
			  }

			  public override AnyValue MapDateTime( DateTimeValue value )
			  {
					return value;
			  }

			  public override AnyValue MapLocalDateTime( LocalDateTimeValue value )
			  {
					return value;
			  }

			  public override AnyValue MapDate( DateValue value )
			  {
					return value;
			  }

			  public override AnyValue MapTime( TimeValue value )
			  {
					return value;
			  }

			  public override AnyValue MapLocalTime( LocalTimeValue value )
			  {
					return value;
			  }

			  public override AnyValue MapDuration( DurationValue value )
			  {
					return value;
			  }

			  public override AnyValue MapPoint( PointValue value )
			  {
					return value;
			  }
		 }
	}

}