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
namespace Neo4Net.Cypher.@internal.codegen
{

	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using Neo4Net.Values;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeReference = Neo4Net.Values.@virtual.NodeReference;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipReference = Neo4Net.Values.@virtual.RelationshipReference;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public sealed class CompiledMaterializeValueMapper
	{
		 public static AnyValue MapAnyValue( EmbeddedProxySPI proxySPI, AnyValue value )
		 {
			  // First do a dry run to determine if any conversion will actually be needed,
			  // because if it isn't we can just return the value as it is without having
			  // to recursively rewrite lists and maps, which could be very expensive.
			  // This is based on the assumption that returning full nodes and relationships is a
			  // relatively rare use-case compared to returning a selection of their properties.
			  // Hopefully the dry run will also heat up the caches for the real run, thus reducing its overhead.
			  DryRunMaterializeValueMapper dryRunMapper = new DryRunMaterializeValueMapper();
			  value.Map( dryRunMapper );
			  if ( dryRunMapper.NeedsConversion )
			  {
					WritingMaterializeValueMapper realMapper = new WritingMaterializeValueMapper( proxySPI );
					return value.Map( realMapper );
			  }
			  return value;
		 }

		 private sealed class WritingMaterializeValueMapper : AbstractMaterializeValueMapper
		 {
			  internal EmbeddedProxySPI ProxySpi;

			  internal WritingMaterializeValueMapper( EmbeddedProxySPI proxySpi )
			  {
					this.ProxySpi = proxySpi;
			  }

			  // Create proxy wrapping values for nodes and relationships that are not already such values

			  public override AnyValue MapNode( VirtualNodeValue value )
			  {
					if ( value is NodeValue )
					{
						 return value;
					}
					return ValueUtils.fromNodeProxy( ProxySpi.newNodeProxy( value.Id() ) );
			  }

			  public override AnyValue MapRelationship( VirtualRelationshipValue value )
			  {
					if ( value is RelationshipValue )
					{
						 return value;
					}
					return ValueUtils.fromRelationshipProxy( ProxySpi.newRelationshipProxy( value.Id() ) );
			  }

			  // Recurse through maps and sequences

			  public override AnyValue MapMap( MapValue value )
			  {
					MapValueBuilder builder = new MapValueBuilder();
					value.Foreach( ( k, v ) => builder.Add( k, v.map( this ) ) );
					return builder.Build();
			  }

			  public override AnyValue MapSequence( SequenceValue value )
			  {
					IList<AnyValue> list = new List<AnyValue>( value.Length() );
					value.forEach( v => list.Add( v.map( this ) ) );
					return VirtualValues.fromList( list );
			  }
		 }

		 private sealed class DryRunMaterializeValueMapper : AbstractMaterializeValueMapper
		 {
			  internal bool NeedsConversion;

			  public override AnyValue MapNode( VirtualNodeValue value )
			  {
					if ( !NeedsConversion )
					{
						 NeedsConversion = value is NodeReference;
					}
					return value;
			  }

			  public override AnyValue MapRelationship( VirtualRelationshipValue value )
			  {
					if ( !NeedsConversion )
					{
						 NeedsConversion = value is RelationshipReference;
					}
					return value;
			  }

			  // Recurse through maps and sequences

			  public override AnyValue MapMap( MapValue value )
			  {
					value.Foreach( ( k, v ) => v.map( this ) );
					return value;
			  }

			  public override AnyValue MapSequence( SequenceValue value )
			  {
					value.forEach( v => v.map( this ) );
					return ( AnyValue ) value;
			  }
		 }

		 internal abstract class AbstractMaterializeValueMapper : ValueMapper<AnyValue>
		 {
			 public abstract Base MapDurationArray( Neo4Net.Values.Storable.DurationArray value );
			 public abstract Base MapDateArray( Neo4Net.Values.Storable.DateArray value );
			 public abstract Base MapTimeArray( Neo4Net.Values.Storable.TimeArray value );
			 public abstract Base MapLocalTimeArray( Neo4Net.Values.Storable.LocalTimeArray value );
			 public abstract Base MapLocalDateTimeArray( Neo4Net.Values.Storable.LocalDateTimeArray value );
			 public abstract Base MapDateTimeArray( Neo4Net.Values.Storable.DateTimeArray value );
			 public abstract Base MapPointArray( Neo4Net.Values.Storable.PointArray value );
			 public abstract Base MapFloatArray( Neo4Net.Values.Storable.FloatArray value );
			 public abstract Base MapFloat( Neo4Net.Values.Storable.FloatValue value );
			 public abstract Base MapDoubleArray( Neo4Net.Values.Storable.DoubleArray value );
			 public abstract Base MapDouble( Neo4Net.Values.Storable.DoubleValue value );
			 public abstract Base MapFloatingPointArray( Neo4Net.Values.Storable.FloatingPointArray value );
			 public abstract Base MapFloatingPoint( Neo4Net.Values.Storable.FloatingPointValue value );
			 public abstract Base MapLongArray( Neo4Net.Values.Storable.LongArray value );
			 public abstract Base MapLong( Neo4Net.Values.Storable.LongValue value );
			 public abstract Base MapIntArray( Neo4Net.Values.Storable.IntArray value );
			 public abstract Base MapInt( Neo4Net.Values.Storable.IntValue value );
			 public abstract Base MapShortArray( Neo4Net.Values.Storable.ShortArray value );
			 public abstract Base MapShort( Neo4Net.Values.Storable.ShortValue value );
			 public abstract Base MapByteArray( Neo4Net.Values.Storable.ByteArray value );
			 public abstract Base MapByte( Neo4Net.Values.Storable.ByteValue value );
			 public abstract Base MapIntegralArray( Neo4Net.Values.Storable.IntegralArray value );
			 public abstract Base MapIntegral( Neo4Net.Values.Storable.IntegralValue value );
			 public abstract Base MapNumberArray( Neo4Net.Values.Storable.NumberArray value );
			 public abstract Base MapBooleanArray( Neo4Net.Values.Storable.BooleanArray value );
			 public abstract Base MapCharArray( Neo4Net.Values.Storable.CharArray value );
			 public abstract Base MapChar( Neo4Net.Values.Storable.CharValue value );
			 public abstract Base MapStringArray( Neo4Net.Values.Storable.StringArray value );
			 public abstract Base MapTextArray( Neo4Net.Values.Storable.TextArray value );
			 public abstract Base MapString( Neo4Net.Values.Storable.StringValue value );
			 public abstract Base MapSequence( SequenceValue value );
			 public abstract Base MapMap( MapValue value );
			 public abstract Base MapRelationship( VirtualRelationshipValue value );
			 public abstract Base MapNode( VirtualNodeValue value );
			  // Paths do not require any conversion at this point

			  public override AnyValue MapPath( PathValue value )
			  {
					return value;
			  }

			  // Preserve the scalar AnyValue types as they are

			  public override AnyValue MapNoValue()
			  {
					return Values.NO_VALUE;
			  }

			  public override AnyValue MapText( TextValue value )
			  {
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