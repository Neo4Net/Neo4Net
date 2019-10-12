using System;
using System.Text;

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
namespace Org.Neo4j.Kernel.Api.Index
{

	using UTF8 = Org.Neo4j.@string.UTF8;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Org.Neo4j.Values.Storable;
	using Values = Org.Neo4j.Values.Storable.Values;

	public sealed class ArrayEncoder
	{
		 private static readonly Base64.Encoder _base64Encoder = Base64.Encoder;

		 private ArrayEncoder()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static string Encode( Value array )
		 {
			  if ( !Values.isArrayValue( array ) )
			  {
					throw new System.ArgumentException( "Only works with arrays" );
			  }

			  ValueEncoder encoder = new ValueEncoder();
			  array.WriteTo( encoder );
			  return encoder.Result();
		 }

		 internal class ValueEncoder : ValueWriter<Exception>
		 {
			  internal StringBuilder Builder;

			  internal ValueEncoder()
			  {
					Builder = new StringBuilder();
			  }

			  public virtual string Result()
			  {
					return Builder.ToString();
			  }

			  public override void WriteNull()
			  {
			  }

			  public override void WriteBoolean( bool value )
			  {
					Builder.Append( value );
					Builder.Append( '|' );
			  }

			  public override void WriteInteger( sbyte value )
			  {
					Builder.Append( ( double )value );
					Builder.Append( '|' );
			  }

			  public override void WriteInteger( short value )
			  {
					Builder.Append( ( double )value );
					Builder.Append( '|' );
			  }

			  public override void WriteInteger( int value )
			  {
					Builder.Append( ( double )value );
					Builder.Append( '|' );
			  }

			  public override void WriteInteger( long value )
			  {
					Builder.Append( ( double )value );
					Builder.Append( '|' );
			  }

			  public override void WriteFloatingPoint( float value )
			  {
					Builder.Append( ( double )value );
					Builder.Append( '|' );
			  }

			  public override void WriteFloatingPoint( double value )
			  {
					Builder.Append( value );
					Builder.Append( '|' );
			  }

			  public override void WriteString( string value )
			  {
					Builder.Append( _base64Encoder.encodeToString( UTF8.encode( value ) ) );
					Builder.Append( '|' );
			  }

			  public override void WriteString( char value )
			  {
					Builder.Append( _base64Encoder.encodeToString( UTF8.encode( Convert.ToString( value ) ) ) );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(org.neo4j.values.storable.CoordinateReferenceSystem crs, double[] coordinate) throws RuntimeException
			  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					Builder.Append( crs.Table.TableId );
					Builder.Append( ':' );
					Builder.Append( crs.Code );
					Builder.Append( ':' );
					int index = 0;
					foreach ( double c in coordinate )
					{
						 if ( index > 0 )
						 {
							  Builder.Append( ';' );
						 }
						 Builder.Append( c );
						 index++;
					}
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws RuntimeException
			  public override void WriteDuration( long months, long days, long seconds, int nanos )
			  {
					Builder.Append( DurationValue.duration( months, days, seconds, nanos ).prettyPrint() );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws RuntimeException
			  public override void WriteDate( LocalDate localDate )
			  {
					Builder.Append( DateValue.date( localDate ).prettyPrint() );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws RuntimeException
			  public override void WriteLocalTime( LocalTime localTime )
			  {
					Builder.Append( LocalTimeValue.localTime( localTime ).prettyPrint() );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws RuntimeException
			  public override void WriteTime( OffsetTime offsetTime )
			  {
					Builder.Append( TimeValue.time( offsetTime ).prettyPrint() );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws RuntimeException
			  public override void WriteLocalDateTime( DateTime localDateTime )
			  {
					Builder.Append( LocalDateTimeValue.localDateTime( localDateTime ).prettyPrint() );
					Builder.Append( '|' );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws RuntimeException
			  public override void WriteDateTime( ZonedDateTime zonedDateTime )
			  {
					Builder.Append( DateTimeValue.datetime( zonedDateTime ).prettyPrint() );
					Builder.Append( '|' );
			  }

			  public override void BeginArray( int size, Org.Neo4j.Values.Storable.ValueWriter_ArrayType arrayType )
			  {
					if ( size > 0 )
					{
						 Builder.Append( TypeChar( arrayType ) );
					}
			  }

			  public override void EndArray()
			  {
			  }

			  public override void WriteByteArray( sbyte[] value )
			  {
					Builder.Append( 'D' );
					foreach ( sbyte b in value )
					{
						 Builder.Append( ( double )b );
						 Builder.Append( '|' );
					}
			  }

			  internal virtual char TypeChar( Org.Neo4j.Values.Storable.ValueWriter_ArrayType arrayType )
			  {
					switch ( arrayType )
					{
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Boolean:
						return 'Z';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Byte:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Short:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Int:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Long:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Float:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Double:
						return 'D';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Char:
						return 'L';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.String:
						return 'L';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Point:
						return 'P';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.ZonedDateTime:
						return 'T';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.LocalDateTime:
						return 'T';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Date:
						return 'T';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.ZonedTime:
						return 'T';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.LocalTime:
						return 'T';
					case Org.Neo4j.Values.Storable.ValueWriter_ArrayType.Duration:
						return 'A';
					default:
						throw new System.NotSupportedException( "Not supported array type: " + arrayType );
					}
			  }
		 }
	}

}