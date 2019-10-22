using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using ReadableClosableChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosableChannel;
	using UTF8 = Neo4Net.Strings.UTF8;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Values = Neo4Net.Values.Storable.Values;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;


	/// <summary>
	/// Utility for reading and writing property values from/into a channel. Supports Neo4Net property types,
	/// including arrays.
	/// </summary>
	public abstract class ValueType
	{
		 private static readonly IDictionary<Type, ValueType> _byClass = new Dictionary<Type, ValueType>();
		 private static readonly IDictionary<sbyte, ValueType> _byId = new Dictionary<sbyte, ValueType>();
		 private static ValueType _stringType;
		 private static sbyte _next;
		 static ValueType()
		 {
			  Add( new ValueTypeAnonymousInnerClass( Boolean.TYPE, typeof( Boolean ) ) );
			  Add( new ValueTypeAnonymousInnerClass2( Byte.TYPE, typeof( Byte ) ) );
			  Add( new ValueTypeAnonymousInnerClass3( Short.TYPE, typeof( Short ) ) );
			  Add( new ValueTypeAnonymousInnerClass4( Character.TYPE, typeof( Character ) ) );
			  Add( new ValueTypeAnonymousInnerClass5( Integer.TYPE, typeof( Integer ) ) );
			  Add( new ValueTypeAnonymousInnerClass6( Long.TYPE, typeof( Long ) ) );
			  Add( new ValueTypeAnonymousInnerClass7( Float.TYPE, typeof( Float ) ) );
			  Add( _stringType = new ValueTypeAnonymousInnerClass8( typeof( string ) ) );
			  Add( new ValueTypeAnonymousInnerClass9( typeof( Double ), Double.TYPE ) );
			  Add( new ValueTypeAnonymousInnerClass10( typeof( LocalDate ) ) );
			  Add( new ValueTypeAnonymousInnerClass11( typeof( LocalTime ) ) );
			  Add( new ValueTypeAnonymousInnerClass12( typeof( DateTime ) ) );
			  Add( new ValueTypeAnonymousInnerClass13( typeof( OffsetTime ) ) );
			  Add( new ValueTypeAnonymousInnerClass14( typeof( ZonedDateTime ) ) );
			  Add( new ValueTypeAnonymousInnerClass15( typeof( DurationValue ), typeof( Duration ), typeof( Period ) ) );
			  Add( new ValueTypeAnonymousInnerClass16( typeof( PointValue ) ) );
		 }

		 private class ValueTypeAnonymousInnerClass : ValueType
		 {
			 public ValueTypeAnonymousInnerClass( UnknownType TYPE, Type class )
			 {
				 private readonly ValueType.ValueTypeAnonymousInnerClass _outerInstance;

//JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
//ORIGINAL LINE: public ()
				 public ( ValueType.ValueTypeAnonymousInnerClass outerInstance )
				 {
					 this._outerInstance = outerInstance;
				 }

				 base( TYPE, class );
			 }

			 public object outerInstance.read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.get() == 0 ? false : true;
			 }

			 public int outerInstance.length( object value )
			 {
				  return Byte.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.put( ( bool? )value ? ( sbyte )1 : ( sbyte )0 );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass2 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass2( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.get();
			 }

			 public int Length( object value )
			 {
				  return Byte.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.put( ( sbyte? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass3 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass3( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.Short;
			 }

			 public int Length( object value )
			 {
				  return Short.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putShort( ( short? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass4 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass4( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return ( char )from.Int;
			 }

			 public int Length( object value )
			 {
				  return Character.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putInt( ( char? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass5 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass5( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.Int;
			 }

			 public int Length( object value )
			 {
				  return Integer.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putInt( ( int? ) value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass6 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass6( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.Long;
			 }

			 public int Length( object value )
			 {
				  return Long.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putLong( ( long? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass7 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass7( UnknownType TYPE, Type class )
			 {
				 base( TYPE, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.Float;
			 }

			 public int Length( object value )
			 {
				  return Float.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putFloat( ( float? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass8 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass8( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  int length = from.Int;
				  sbyte[] bytes = new sbyte[length]; // TODO wasteful
				  from.get( bytes, length );
				  return UTF8.decode( bytes );
			 }

			 public int Length( object value )
			 {
				  return Integer.BYTES + UTF8.encode( ( string )value ).Length * Character.BYTES; // pessimistic
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  sbyte[] bytes = UTF8.encode( ( string )value );
				  into.putInt( bytes.Length ).put( bytes, bytes.Length );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass9 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass9( Type class, UnknownType TYPE )
			 {
				 base( class, TYPE );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return from.Double;
			 }

			 public int Length( object value )
			 {
				  return Double.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putDouble( ( double? )value );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass10 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass10( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return LocalDate.ofEpochDay( from.Long );
			 }

			 public int Length( object value )
			 {
				  return Long.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putLong( ( ( LocalDate ) value ).toEpochDay() );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass11 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass11( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return LocalTime.ofNanoOfDay( from.Long );
			 }

			 public int Length( object value )
			 {
				  return Long.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  into.putLong( ( ( LocalTime ) value ).toNanoOfDay() );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass12 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass12( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return DateTime.ofEpochSecond( from.Long, from.Int, UTC );
			 }

			 public int Length( object value )
			 {
				  return Long.BYTES + Integer.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  DateTime ldt = ( DateTime ) value;
				  into.putLong( ldt.toEpochSecond( UTC ) );
				  into.putInt( ldt.Nano );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass13 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass13( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  return OffsetTime.ofInstant( Instant.ofEpochSecond( 0, from.Long ), ZoneOffset.ofTotalSeconds( from.Int ) );
			 }

			 public int Length( object value )
			 {
				  return Long.BYTES + Integer.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  OffsetTime ot = ( OffsetTime ) value;
				  into.putLong( TemporalUtil.getNanosOfDayUTC( ot ) );
				  into.putInt( ot.Offset.TotalSeconds );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass14 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass14( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  if ( from.get() == (sbyte) 0 )
				  {
						long epochSecondsUTC = from.Long;
						int nanos = from.Int;
						int offsetSeconds = from.Int;
						return ZonedDateTime.ofInstant( Instant.ofEpochSecond( epochSecondsUTC, nanos ), ZoneOffset.ofTotalSeconds( offsetSeconds ) );
				  }
				  else
				  {
						long epochSecondsUTC = from.Long;
						int nanos = from.Int;
						int zoneID = from.Int;
						string zone = TimeZones.map( ( short ) zoneID );
						return ZonedDateTime.ofInstant( Instant.ofEpochSecond( epochSecondsUTC, nanos ), ZoneId.of( zone ) );
				  }
			 }

			 public int Length( object value )
			 {
				  return 1 + Long.BYTES + Integer.BYTES + Integer.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  ZonedDateTime zonedDateTime = ( ZonedDateTime ) value;
				  long epochSecondUTC = zonedDateTime.toEpochSecond();
				  int nano = zonedDateTime.Nano;

				  ZoneId zone = zonedDateTime.Zone;
				  if ( zone is ZoneOffset )
				  {
						int offsetSeconds = ( ( ZoneOffset ) zone ).TotalSeconds;
						into.put( ( sbyte ) 0 );
						into.putLong( epochSecondUTC );
						into.putInt( nano );
						into.putInt( offsetSeconds );
				  }
				  else
				  {
						string zoneId = zone.Id;
						into.put( ( sbyte ) 1 );
						into.putLong( epochSecondUTC );
						into.putInt( nano );
						into.putInt( TimeZones.map( zoneId ) );
				  }
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass15 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass15( Type class, Type class, Type class )
			 {
				 base( class, class, class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  int nanos = from.Int;
				  long seconds = from.Long;
				  long days = from.Long;
				  long months = from.Long;
				  return DurationValue.duration( months, days, seconds, nanos );
			 }

			 public int Length( object value )
			 {
				  return Integer.BYTES + 3 * Long.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  DurationValue duration;
				  if ( value is Duration )
				  {
						duration = DurationValue.duration( ( Duration ) value );
				  }
				  else if ( value is Period )
				  {
						duration = DurationValue.duration( ( Period ) value );
				  }
				  else
				  {
						duration = ( DurationValue ) value;
				  }
				  into.putInt( ( int ) duration.get( NANOS ) );
				  into.putLong( duration.get( SECONDS ) );
				  into.putLong( duration.get( DAYS ) );
				  into.putLong( duration.get( MONTHS ) );
			 }
		 }

		 private static class ValueTypeAnonymousInnerClass16 extends ValueType
		 {
			 public ValueTypeAnonymousInnerClass16( Type class )
			 {
				 base( class );
			 }

			 public object Read( ReadableClosableChannel from ) throws IOException
			 {
				  int code = from.Int;
				  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( code );
				  int length = from.Int;
				  double[] coordinate = new double[length];
				  for ( int i = 0; i < length; i++ )
				  {
						coordinate[i] = from.Double;
				  }
				  return Values.pointValue( crs, coordinate );
			 }

			 public int Length( object value )
			 {
				  return 2 * Integer.BYTES + ( ( PointValue ) value ).coordinate().Length * Double.BYTES;
			 }

			 public void write( object value, FlushableChannel into ) throws IOException
			 {
				  PointValue pointValue = ( PointValue ) value;
				  // using code is not a future-proof solution like the one we have in the PropertyStore.
				  // But then again, this is not used from procution code.
				  into.putInt( pointValue.CoordinateReferenceSystem.Code );
				  double[] coordinate = pointValue.Coordinate();
				  into.putInt( coordinate.Length );
				  foreach ( double c in coordinate )
				  {
						into.putDouble( c );
				  }
			 }
		 }
		 private static final ValueType arrayType = new ValueTypeAnonymousInnerClass( this );

		 private final Type[] _classes;
		 private final sbyte _id = _next++;

		 private ValueType( Type... _classes )
		 {
			  this._classes = _classes;
		 }

		 private static void add( ValueType type )
		 {
			  foreach ( Type cls in type.classes )
			  {
					_byClass[cls] = type;
			  }
			  _byId[type.id()] = type;
		 }

		 public static ValueType typeOf( object value )
		 {
			  return typeOf( value.GetType() );
		 }

		 public static ValueType typeOf( Type cls )
		 {
			  if ( cls.Array )
			  {
					return arrayType;
			  }

			  ValueType type = _byClass[cls];
			  Debug.Assert( type != null, "Unrecognized value type " + cls );
			  return type;
		 }

		 public static ValueType typeOf( sbyte _id )
		 {
			  if ( _id == arrayType.id() )
			  {
					return arrayType;
			  }

			  ValueType type = _byId[_id];
			  Debug.Assert( type != null, "Unrecognized value type id " + _id );
			  return type;
		 }

		 public static ValueType StringType()
		 {
			  return _stringType;
		 }

		 private Type ComponentClass()
		 {
			  return _classes[0];
		 }

		 public final sbyte Id()
		 {
			  return _id;
		 }

		 public abstract object Read( ReadableClosableChannel from ) throws IOException;

		 public abstract int Length( object value );

		 public abstract void write( object value, FlushableChannel into ) throws IOException;
	}

}