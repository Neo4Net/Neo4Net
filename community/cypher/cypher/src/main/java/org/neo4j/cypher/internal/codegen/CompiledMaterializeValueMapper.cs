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

	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
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
	using MapValueBuilder = Org.Neo4j.Values.@virtual.MapValueBuilder;
	using NodeReference = Org.Neo4j.Values.@virtual.NodeReference;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipReference = Org.Neo4j.Values.@virtual.RelationshipReference;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

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
			 public abstract Base MapDurationArray( Org.Neo4j.Values.Storable.DurationArray value );
			 public abstract Base MapDateArray( Org.Neo4j.Values.Storable.DateArray value );
			 public abstract Base MapTimeArray( Org.Neo4j.Values.Storable.TimeArray value );
			 public abstract Base MapLocalTimeArray( Org.Neo4j.Values.Storable.LocalTimeArray value );
			 public abstract Base MapLocalDateTimeArray( Org.Neo4j.Values.Storable.LocalDateTimeArray value );
			 public abstract Base MapDateTimeArray( Org.Neo4j.Values.Storable.DateTimeArray value );
			 public abstract Base MapPointArray( Org.Neo4j.Values.Storable.PointArray value );
			 public abstract Base MapFloatArray( Org.Neo4j.Values.Storable.FloatArray value );
			 public abstract Base MapFloat( Org.Neo4j.Values.Storable.FloatValue value );
			 public abstract Base MapDoubleArray( Org.Neo4j.Values.Storable.DoubleArray value );
			 public abstract Base MapDouble( Org.Neo4j.Values.Storable.DoubleValue value );
			 public abstract Base MapFloatingPointArray( Org.Neo4j.Values.Storable.FloatingPointArray value );
			 public abstract Base MapFloatingPoint( Org.Neo4j.Values.Storable.FloatingPointValue value );
			 public abstract Base MapLongArray( Org.Neo4j.Values.Storable.LongArray value );
			 public abstract Base MapLong( Org.Neo4j.Values.Storable.LongValue value );
			 public abstract Base MapIntArray( Org.Neo4j.Values.Storable.IntArray value );
			 public abstract Base MapInt( Org.Neo4j.Values.Storable.IntValue value );
			 public abstract Base MapShortArray( Org.Neo4j.Values.Storable.ShortArray value );
			 public abstract Base MapShort( Org.Neo4j.Values.Storable.ShortValue value );
			 public abstract Base MapByteArray( Org.Neo4j.Values.Storable.ByteArray value );
			 public abstract Base MapByte( Org.Neo4j.Values.Storable.ByteValue value );
			 public abstract Base MapIntegralArray( Org.Neo4j.Values.Storable.IntegralArray value );
			 public abstract Base MapIntegral( Org.Neo4j.Values.Storable.IntegralValue value );
			 public abstract Base MapNumberArray( Org.Neo4j.Values.Storable.NumberArray value );
			 public abstract Base MapBooleanArray( Org.Neo4j.Values.Storable.BooleanArray value );
			 public abstract Base MapCharArray( Org.Neo4j.Values.Storable.CharArray value );
			 public abstract Base MapChar( Org.Neo4j.Values.Storable.CharValue value );
			 public abstract Base MapStringArray( Org.Neo4j.Values.Storable.StringArray value );
			 public abstract Base MapTextArray( Org.Neo4j.Values.Storable.TextArray value );
			 public abstract Base MapString( Org.Neo4j.Values.Storable.StringValue value );
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