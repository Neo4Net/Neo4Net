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
namespace Neo4Net.Values
{

	using Point = Neo4Net.GraphDb.Spatial.Point;
	using BooleanArray = Neo4Net.Values.Storable.BooleanArray;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using ByteArray = Neo4Net.Values.Storable.ByteArray;
	using ByteValue = Neo4Net.Values.Storable.ByteValue;
	using CharArray = Neo4Net.Values.Storable.CharArray;
	using CharValue = Neo4Net.Values.Storable.CharValue;
	using DateArray = Neo4Net.Values.Storable.DateArray;
	using DateTimeArray = Neo4Net.Values.Storable.DateTimeArray;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DoubleArray = Neo4Net.Values.Storable.DoubleArray;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using DurationArray = Neo4Net.Values.Storable.DurationArray;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using FloatArray = Neo4Net.Values.Storable.FloatArray;
	using FloatValue = Neo4Net.Values.Storable.FloatValue;
	using FloatingPointArray = Neo4Net.Values.Storable.FloatingPointArray;
	using FloatingPointValue = Neo4Net.Values.Storable.FloatingPointValue;
	using IntArray = Neo4Net.Values.Storable.IntArray;
	using IntValue = Neo4Net.Values.Storable.IntValue;
	using IntegralArray = Neo4Net.Values.Storable.IntegralArray;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using LocalDateTimeArray = Neo4Net.Values.Storable.LocalDateTimeArray;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeArray = Neo4Net.Values.Storable.LocalTimeArray;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using LongArray = Neo4Net.Values.Storable.LongArray;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using NumberArray = Neo4Net.Values.Storable.NumberArray;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using ShortArray = Neo4Net.Values.Storable.ShortArray;
	using ShortValue = Neo4Net.Values.Storable.ShortValue;
	using StringArray = Neo4Net.Values.Storable.StringArray;
	using StringValue = Neo4Net.Values.Storable.StringValue;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeArray = Neo4Net.Values.Storable.TimeArray;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;

	public interface IValueMapper<Base>
	{
		 // Virtual

		 Base MapPath( PathValue value );

		 Base MapNode( VirtualNodeValue value );

		 Base MapRelationship( VirtualRelationshipValue value );

		 Base MapMap( MapValue value );

		 // Storable

		 Base MapNoValue();

		 Base MapSequence( ISequenceValue value );

		 Base MapText( TextValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapString(org.Neo4Net.values.storable.StringValue value)
	//	 {
	//		  return mapText(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapTextArray(org.Neo4Net.values.storable.TextArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapStringArray(org.Neo4Net.values.storable.StringArray value)
	//	 {
	//		  return mapTextArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapChar(org.Neo4Net.values.storable.CharValue value)
	//	 {
	//		  return mapText(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapCharArray(org.Neo4Net.values.storable.CharArray value)
	//	 {
	//		  return mapTextArray(value);
	//	 }

		 Base MapBoolean( BooleanValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapBooleanArray(org.Neo4Net.values.storable.BooleanArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

		 Base MapNumber( NumberValue value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapNumberArray(org.Neo4Net.values.storable.NumberArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntegral(org.Neo4Net.values.storable.IntegralValue value)
	//	 {
	//		  return mapNumber(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntegralArray(org.Neo4Net.values.storable.IntegralArray value)
	//	 {
	//		  return mapNumberArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapByte(org.Neo4Net.values.storable.ByteValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapByteArray(org.Neo4Net.values.storable.ByteArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapShort(org.Neo4Net.values.storable.ShortValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapShortArray(org.Neo4Net.values.storable.ShortArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapInt(org.Neo4Net.values.storable.IntValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapIntArray(org.Neo4Net.values.storable.IntArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLong(org.Neo4Net.values.storable.LongValue value)
	//	 {
	//		  return mapIntegral(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLongArray(org.Neo4Net.values.storable.LongArray value)
	//	 {
	//		  return mapIntegralArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatingPoint(org.Neo4Net.values.storable.FloatingPointValue value)
	//	 {
	//		  return mapNumber(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatingPointArray(org.Neo4Net.values.storable.FloatingPointArray value)
	//	 {
	//		  return mapNumberArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDouble(org.Neo4Net.values.storable.DoubleValue value)
	//	 {
	//		  return mapFloatingPoint(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDoubleArray(org.Neo4Net.values.storable.DoubleArray value)
	//	 {
	//		  return mapFloatingPointArray(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloat(org.Neo4Net.values.storable.FloatValue value)
	//	 {
	//		  return mapFloatingPoint(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapFloatArray(org.Neo4Net.values.storable.FloatArray value)
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
//		 default Base mapPointArray(org.Neo4Net.values.storable.PointArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDateTimeArray(org.Neo4Net.values.storable.DateTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLocalDateTimeArray(org.Neo4Net.values.storable.LocalDateTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapLocalTimeArray(org.Neo4Net.values.storable.LocalTimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapTimeArray(org.Neo4Net.values.storable.TimeArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDateArray(org.Neo4Net.values.storable.DateArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Base mapDurationArray(org.Neo4Net.values.storable.DurationArray value)
	//	 {
	//		  return mapSequence(value);
	//	 }
	}

	 public abstract class ValueMapper_JavaMapper : IValueMapper<object>
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