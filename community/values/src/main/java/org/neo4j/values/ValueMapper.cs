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
namespace Org.Neo4j.Values
{

	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using BooleanArray = Org.Neo4j.Values.Storable.BooleanArray;
	using BooleanValue = Org.Neo4j.Values.Storable.BooleanValue;
	using ByteArray = Org.Neo4j.Values.Storable.ByteArray;
	using ByteValue = Org.Neo4j.Values.Storable.ByteValue;
	using CharArray = Org.Neo4j.Values.Storable.CharArray;
	using CharValue = Org.Neo4j.Values.Storable.CharValue;
	using DateArray = Org.Neo4j.Values.Storable.DateArray;
	using DateTimeArray = Org.Neo4j.Values.Storable.DateTimeArray;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DoubleArray = Org.Neo4j.Values.Storable.DoubleArray;
	using DoubleValue = Org.Neo4j.Values.Storable.DoubleValue;
	using DurationArray = Org.Neo4j.Values.Storable.DurationArray;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using FloatArray = Org.Neo4j.Values.Storable.FloatArray;
	using FloatValue = Org.Neo4j.Values.Storable.FloatValue;
	using FloatingPointArray = Org.Neo4j.Values.Storable.FloatingPointArray;
	using FloatingPointValue = Org.Neo4j.Values.Storable.FloatingPointValue;
	using IntArray = Org.Neo4j.Values.Storable.IntArray;
	using IntValue = Org.Neo4j.Values.Storable.IntValue;
	using IntegralArray = Org.Neo4j.Values.Storable.IntegralArray;
	using IntegralValue = Org.Neo4j.Values.Storable.IntegralValue;
	using LocalDateTimeArray = Org.Neo4j.Values.Storable.LocalDateTimeArray;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeArray = Org.Neo4j.Values.Storable.LocalTimeArray;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using LongArray = Org.Neo4j.Values.Storable.LongArray;
	using LongValue = Org.Neo4j.Values.Storable.LongValue;
	using NumberArray = Org.Neo4j.Values.Storable.NumberArray;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using PointArray = Org.Neo4j.Values.Storable.PointArray;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using ShortArray = Org.Neo4j.Values.Storable.ShortArray;
	using ShortValue = Org.Neo4j.Values.Storable.ShortValue;
	using StringArray = Org.Neo4j.Values.Storable.StringArray;
	using StringValue = Org.Neo4j.Values.Storable.StringValue;
	using TextArray = Org.Neo4j.Values.Storable.TextArray;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using TimeArray = Org.Neo4j.Values.Storable.TimeArray;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;

	public interface ValueMapper<Base>
	{
		 // Virtual

		 Base MapPath( PathValue value );

		 Base MapNode( VirtualNodeValue value );

		 Base MapRelationship( VirtualRelationshipValue value );

		 Base MapMap( MapValue value );

		 // Storable

		 Base MapNoValue();

		 Base MapSequence( SequenceValue value );

		 Base MapText( TextValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapString(org.neo4j.values.storable.StringValue value)
	//	 {
	//		  return mapText(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapTextArray(org.neo4j.values.storable.TextArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapStringArray(org.neo4j.values.storable.StringArray value)
	//	 {
	//		  return mapTextArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapChar(org.neo4j.values.storable.CharValue value)
	//	 {
	//		  return mapText(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapCharArray(org.neo4j.values.storable.CharArray value)
	//	 {
	//		  return mapTextArray(value);
	//	 }

		 Base MapBoolean( BooleanValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapBooleanArray(org.neo4j.values.storable.BooleanArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

		 Base MapNumber( NumberValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapNumberArray(org.neo4j.values.storable.NumberArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntegral(org.neo4j.values.storable.IntegralValue value)
	//	 {
	//		  return mapNumber(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntegralArray(org.neo4j.values.storable.IntegralArray value)
	//	 {
	//		  return mapNumberArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapByte(org.neo4j.values.storable.ByteValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapByteArray(org.neo4j.values.storable.ByteArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapShort(org.neo4j.values.storable.ShortValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapShortArray(org.neo4j.values.storable.ShortArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapInt(org.neo4j.values.storable.IntValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntArray(org.neo4j.values.storable.IntArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLong(org.neo4j.values.storable.LongValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLongArray(org.neo4j.values.storable.LongArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatingPoint(org.neo4j.values.storable.FloatingPointValue value)
	//	 {
	//		  return mapNumber(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatingPointArray(org.neo4j.values.storable.FloatingPointArray value)
	//	 {
	//		  return mapNumberArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDouble(org.neo4j.values.storable.DoubleValue value)
	//	 {
	//		  return mapFloatingPoint(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDoubleArray(org.neo4j.values.storable.DoubleArray value)
	//	 {
	//		  return mapFloatingPointArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloat(org.neo4j.values.storable.FloatValue value)
	//	 {
	//		  return mapFloatingPoint(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatArray(org.neo4j.values.storable.FloatArray value)
	//	 {
	//		  return mapFloatingPointArray(value);
	//	 }

		 Base MapDateTime( DateTimeValue value );

		 Base MapLocalDateTime( LocalDateTimeValue value );

		 Base MapDate( DateValue value );

		 Base MapTime( TimeValue value );

		 Base MapLocalTime( LocalTimeValue value );

		 Base MapDuration( DurationValue value );

		 Base MapPoint( PointValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapPointArray(org.neo4j.values.storable.PointArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDateTimeArray(org.neo4j.values.storable.DateTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLocalDateTimeArray(org.neo4j.values.storable.LocalDateTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLocalTimeArray(org.neo4j.values.storable.LocalTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapTimeArray(org.neo4j.values.storable.TimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDateArray(org.neo4j.values.storable.DateArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDurationArray(org.neo4j.values.storable.DurationArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }
	}

	 public abstract class ValueMapper_JavaMapper : ValueMapper<object>
	 {
		 public abstract Base MapDurationArray( storable.DurationArray value );
		 public abstract Base MapDateArray( storable.DateArray value );
		 public abstract Base MapTimeArray( storable.TimeArray value );
		 public abstract Base MapLocalTimeArray( storable.LocalTimeArray value );
		 public abstract Base MapLocalDateTimeArray( storable.LocalDateTimeArray value );
		 public abstract Base MapDateTimeArray( storable.DateTimeArray value );
		 public abstract Base MapPointArray( storable.PointArray value );
		 public abstract Base MapFloat( storable.FloatValue value );
		 public abstract Base MapDouble( storable.DoubleValue value );
		 public abstract Base MapFloatingPointArray( storable.FloatingPointArray value );
		 public abstract Base MapFloatingPoint( storable.FloatingPointValue value );
		 public abstract Base MapLong( storable.LongValue value );
		 public abstract Base MapInt( storable.IntValue value );
		 public abstract Base MapShort( storable.ShortValue value );
		 public abstract Base MapByte( storable.ByteValue value );
		 public abstract Base MapIntegralArray( storable.IntegralArray value );
		 public abstract Base MapIntegral( storable.IntegralValue value );
		 public abstract Base MapNumberArray( storable.NumberArray value );
		 public abstract Base MapTextArray( storable.TextArray value );
		 public abstract Base MapString( storable.StringValue value );
		 public abstract Base MapRelationship( @virtual.VirtualRelationshipValue value );
		 public abstract Base MapNode( @virtual.VirtualNodeValue value );
		 public abstract Base MapPath( @virtual.PathValue value );
		  public override object MapNoValue()
		  {
				return null;
		  }

		  public override object MapMap( MapValue value )
		  {
				IDictionary<object, object> map = new Dictionary<object, object>();
				value.Foreach( ( k, v ) => map.put( k, v.map( this ) ) );
				return map;
		  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.List<?> mapSequence(SequenceValue value)
		  public override IList<object> MapSequence( SequenceValue value )
		  {
				IList<object> list = new List<object>( value.Length() );
				value.forEach( v => list.Add( v.map( this ) ) );
				return list;
		  }

		  public override char? MapChar( CharValue value )
		  {
				return value.Value();
		  }

		  public override string MapText( TextValue value )
		  {
				return value.StringValue();
		  }

		  public override string[] MapStringArray( StringArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override char[] MapCharArray( CharArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override bool? MapBoolean( BooleanValue value )
		  {
				return value.BooleanValueConflict();
		  }

		  public override bool[] MapBooleanArray( BooleanArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override Number MapNumber( NumberValue value )
		  {
				return value.AsObject();
		  }

		  public override sbyte[] MapByteArray( ByteArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override short[] MapShortArray( ShortArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override int[] MapIntArray( IntArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override long[] MapLongArray( LongArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override float[] MapFloatArray( FloatArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override double[] MapDoubleArray( DoubleArray value )
		  {
				return value.AsObjectCopy();
		  }

		  public override ZonedDateTime MapDateTime( DateTimeValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override DateTime MapLocalDateTime( LocalDateTimeValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override LocalDate MapDate( DateValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override OffsetTime MapTime( TimeValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override LocalTime MapLocalTime( LocalTimeValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override TemporalAmount MapDuration( DurationValue value )
		  {
				return value.AsObjectCopy();
		  }

		  public override Point MapPoint( PointValue value )
		  {
				return value.AsObjectCopy();
		  }
	 }

}