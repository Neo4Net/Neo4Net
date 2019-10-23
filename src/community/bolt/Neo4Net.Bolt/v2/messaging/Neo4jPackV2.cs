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
namespace Neo4Net.Bolt.v2.messaging
{

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using StructType = Neo4Net.Bolt.messaging.StructType;
	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using PackInput = Neo4Net.Bolt.v1.packstream.PackInput;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.epochDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.pointValue;

	public class Neo4NetPackV2 : Neo4NetPackV1
	{
		 public new const long VERSION = 2;

		 public const sbyte POINT_2_D = ( sbyte )'X';
		 public const int POINT_2_D_SIZE = 3;

		 public const sbyte POINT_3_D = ( sbyte )'Y';
		 public const int POINT_3_D_SIZE = 4;

		 public const sbyte DATE = ( sbyte )'D';
		 public const int DATE_SIZE = 1;

		 public const sbyte TIME = ( sbyte )'T';
		 public const int TIME_SIZE = 2;

		 public const sbyte LOCAL_TIME = ( sbyte )'t';
		 public const int LOCAL_TIME_SIZE = 1;

		 public const sbyte LOCAL_DATE_TIME = ( sbyte )'d';
		 public const int LOCAL_DATE_TIME_SIZE = 2;

		 public const sbyte DATE_TIME_WITH_ZONE_OFFSET = ( sbyte )'F';
		 public const int DATE_TIME_WITH_ZONE_OFFSET_SIZE = 3;

		 public const sbyte DATE_TIME_WITH_ZONE_NAME = ( sbyte )'f';
		 public const int DATE_TIME_WITH_ZONE_NAME_SIZE = 3;

		 public const sbyte DURATION = ( sbyte )'E';
		 public const int DURATION_SIZE = 4;

		 public override Neo4Net.Bolt.messaging.Neo4NetPack_Packer NewPacker( PackOutput output )
		 {
			  return new PackerV2( output );
		 }

		 public override Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker NewUnpacker( PackInput input )
		 {
			  return new UnpackerV2( input );
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }

		 private class PackerV2 : Neo4NetPackV1.PackerV1
		 {
			  internal PackerV2( PackOutput output ) : base( output )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(org.Neo4Net.values.storable.CoordinateReferenceSystem crs, double[] coordinate) throws java.io.IOException
			  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					if ( coordinate.Length == 2 )
					{
						 PackStructHeader( POINT_2_D_SIZE, POINT_2_D );
						 Pack( crs.Code );
						 Pack( coordinate[0] );
						 Pack( coordinate[1] );
					}
					else if ( coordinate.Length == 3 )
					{
						 PackStructHeader( POINT_3_D_SIZE, POINT_3_D );
						 Pack( crs.Code );
						 Pack( coordinate[0] );
						 Pack( coordinate[1] );
						 Pack( coordinate[2] );
					}
					else
					{
						 throw new System.ArgumentException( "Point with 2D or 3D coordinate expected, " + "got crs=" + crs + ", coordinate=" + Arrays.ToString( coordinate ) );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws java.io.IOException
			  public override void WriteDuration( long months, long days, long seconds, int nanos )
			  {
					PackStructHeader( DURATION_SIZE, DURATION );
					Pack( months );
					Pack( days );
					Pack( seconds );
					Pack( nanos );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws java.io.IOException
			  public override void WriteDate( LocalDate localDate )
			  {
					long epochDay = localDate.toEpochDay();

					PackStructHeader( DATE_SIZE, DATE );
					Pack( epochDay );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws java.io.IOException
			  public override void WriteLocalTime( LocalTime localTime )
			  {
					long nanoOfDay = localTime.toNanoOfDay();

					PackStructHeader( LOCAL_TIME_SIZE, LOCAL_TIME );
					Pack( nanoOfDay );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws java.io.IOException
			  public override void WriteTime( OffsetTime offsetTime )
			  {
					long nanosOfDayLocal = offsetTime.toLocalTime().toNanoOfDay();
					int offsetSeconds = offsetTime.Offset.TotalSeconds;

					PackStructHeader( TIME_SIZE, TIME );
					Pack( nanosOfDayLocal );
					Pack( offsetSeconds );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws java.io.IOException
			  public override void WriteLocalDateTime( DateTime localDateTime )
			  {
					long epochSecond = localDateTime.toEpochSecond( UTC );
					int nano = localDateTime.Nano;

					PackStructHeader( LOCAL_DATE_TIME_SIZE, LOCAL_DATE_TIME );
					Pack( epochSecond );
					Pack( nano );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws java.io.IOException
			  public override void WriteDateTime( ZonedDateTime zonedDateTime )
			  {
					long epochSecondLocal = zonedDateTime.toLocalDateTime().toEpochSecond(UTC);
					int nano = zonedDateTime.Nano;

					ZoneId zone = zonedDateTime.Zone;
					if ( zone is ZoneOffset )
					{
						 int offsetSeconds = ( ( ZoneOffset ) zone ).TotalSeconds;

						 PackStructHeader( DATE_TIME_WITH_ZONE_OFFSET_SIZE, DATE_TIME_WITH_ZONE_OFFSET );
						 Pack( epochSecondLocal );
						 Pack( nano );
						 Pack( offsetSeconds );
					}
					else
					{
						 string zoneId = zone.Id;

						 PackStructHeader( DATE_TIME_WITH_ZONE_NAME_SIZE, DATE_TIME_WITH_ZONE_NAME );
						 Pack( epochSecondLocal );
						 Pack( nano );
						 Pack( zoneId );
					}
			  }
		 }

		 private class UnpackerV2 : Neo4NetPackV1.UnpackerV1
		 {
			  internal UnpackerV2( PackInput input ) : base( input )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.values.AnyValue unpackStruct(char signature, long size) throws java.io.IOException
			  protected internal override AnyValue UnpackStruct( char signature, long size )
			  {
					try
					{
						 switch ( signature )
						 {
						 case POINT_2_D:
							  EnsureCorrectStructSize( StructType.POINT_2D, POINT_2_D_SIZE, size );
							  return UnpackPoint2D();
						 case POINT_3_D:
							  EnsureCorrectStructSize( StructType.POINT_3D, POINT_3_D_SIZE, size );
							  return UnpackPoint3D();
						 case DURATION:
							  EnsureCorrectStructSize( StructType.DURATION, DURATION_SIZE, size );
							  return UnpackDuration();
						 case DATE:
							  EnsureCorrectStructSize( StructType.DATE, DATE_SIZE, size );
							  return UnpackDate();
						 case LOCAL_TIME:
							  EnsureCorrectStructSize( StructType.LOCAL_TIME, LOCAL_TIME_SIZE, size );
							  return UnpackLocalTime();
						 case TIME:
							  EnsureCorrectStructSize( StructType.TIME, TIME_SIZE, size );
							  return UnpackTime();
						 case LOCAL_DATE_TIME:
							  EnsureCorrectStructSize( StructType.LOCAL_DATE_TIME, LOCAL_DATE_TIME_SIZE, size );
							  return UnpackLocalDateTime();
						 case DATE_TIME_WITH_ZONE_OFFSET:
							  EnsureCorrectStructSize( StructType.DATE_TIME_WITH_ZONE_OFFSET, DATE_TIME_WITH_ZONE_OFFSET_SIZE, size );
							  return UnpackDateTimeWithZoneOffset();
						 case DATE_TIME_WITH_ZONE_NAME:
							  EnsureCorrectStructSize( StructType.DATE_TIME_WITH_ZONE_NAME, DATE_TIME_WITH_ZONE_NAME_SIZE, size );
							  return UnpackDateTimeWithZoneName();
						 default:
							  return base.UnpackStruct( signature, size );
						 }
					}
					catch ( Exception ex ) when ( ex is PackStream.PackStreamException || ex is BoltIOException )
					{
						 throw ex;
					}
					catch ( Exception ex )
					{
						 StructType type = StructType.ValueOf( signature );
						 if ( type != null )
						 {
							  throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError, string.Format( "Unable to construct {0} value: `{1}`", type.description(), ex.Message ), ex );
						 }

						 throw ex;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.PointValue unpackPoint2D() throws java.io.IOException
			  internal virtual PointValue UnpackPoint2D()
			  {

					int crsCode = UnpackInteger();
					CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( crsCode );
					double[] coordinates = new double[] { UnpackDouble(), UnpackDouble() };
					return pointValue( crs, coordinates );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.PointValue unpackPoint3D() throws java.io.IOException
			  internal virtual PointValue UnpackPoint3D()
			  {
					int crsCode = UnpackInteger();
					CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( crsCode );
					double[] coordinates = new double[] { UnpackDouble(), UnpackDouble(), UnpackDouble() };
					return pointValue( crs, coordinates );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.DurationValue unpackDuration() throws java.io.IOException
			  internal virtual DurationValue UnpackDuration()
			  {
					long months = UnpackLong();
					long days = UnpackLong();
					long seconds = UnpackLong();
					long nanos = UnpackInteger();
					return duration( months, days, seconds, nanos );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.DateValue unpackDate() throws java.io.IOException
			  internal virtual DateValue UnpackDate()
			  {
					long epochDay = UnpackLong();
					return epochDate( epochDay );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.LocalTimeValue unpackLocalTime() throws java.io.IOException
			  internal virtual LocalTimeValue UnpackLocalTime()
			  {
					long nanoOfDay = UnpackLong();
					return localTime( nanoOfDay );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.TimeValue unpackTime() throws java.io.IOException
			  internal virtual TimeValue UnpackTime()
			  {
					long nanosOfDayLocal = UnpackLong();
					int offsetSeconds = UnpackInteger();
					return time( TemporalUtil.nanosOfDayToUTC( nanosOfDayLocal, offsetSeconds ), ZoneOffset.ofTotalSeconds( offsetSeconds ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.LocalDateTimeValue unpackLocalDateTime() throws java.io.IOException
			  internal virtual LocalDateTimeValue UnpackLocalDateTime()
			  {
					long epochSecond = UnpackLong();
					long nano = UnpackLong();
					return localDateTime( epochSecond, nano );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.DateTimeValue unpackDateTimeWithZoneOffset() throws java.io.IOException
			  internal virtual DateTimeValue UnpackDateTimeWithZoneOffset()
			  {
					long epochSecondLocal = UnpackLong();
					long nano = UnpackLong();
					int offsetSeconds = UnpackInteger();
					return datetime( NewZonedDateTime( epochSecondLocal, nano, ZoneOffset.ofTotalSeconds( offsetSeconds ) ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.storable.DateTimeValue unpackDateTimeWithZoneName() throws java.io.IOException
			  internal virtual DateTimeValue UnpackDateTimeWithZoneName()
			  {
					long epochSecondLocal = UnpackLong();
					long nano = UnpackLong();
					string zoneId = UnpackString();
					return datetime( NewZonedDateTime( epochSecondLocal, nano, ZoneId.of( zoneId ) ) );
			  }

			  internal static ZonedDateTime NewZonedDateTime( long epochSecondLocal, long nano, ZoneId zoneId )
			  {
					Instant instant = Instant.ofEpochSecond( epochSecondLocal, nano );
					DateTime localDateTime = DateTime.ofInstant( instant, UTC );
					return ZonedDateTime.of( localDateTime, zoneId );
			  }
		 }
	}

}