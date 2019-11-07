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
namespace Neo4Net.Values.Storable
{
	using Matchers = org.hamcrest.Matchers;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.BufferValueWriter.SpecialKind.BeginArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.BufferValueWriter.SpecialKind.EndArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.BufferValueWriter.SpecialKind.WriteByteArray;

	public class BufferValueWriter : ValueWriter<Exception>
	{
		 internal enum SpecialKind
		 {
			  WriteCharArray,
			  WriteByteArray,
			  BeginArray,
			  EndArray,
		 }

		 public class Special
		 {
			  internal readonly SpecialKind Kind;
			  internal readonly string Key;

			  public override bool Equals( object o )
			  {
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}

					Special special = ( Special ) o;
					return Kind == special.Kind && Key.Equals( special.Key );
			  }

			  public override int GetHashCode()
			  {
					return 31 * Kind.GetHashCode() + Key.GetHashCode();
			  }

			  internal Special( SpecialKind kind, string key )
			  {
					this.Kind = kind;
					this.Key = key;
			  }

			  internal Special( SpecialKind kind, int key )
			  {
					this.Kind = kind;
					this.Key = Convert.ToString( key );
			  }

			  public override string ToString()
			  {
					return format( "Special(%s)", Key );
			  }
		 }

		 protected internal IList<object> Buffer = new List<object>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public void assertBuffer(Object... writeEvents)
		 public virtual void AssertBuffer( params object[] writeEvents )
		 {
			  assertThat( Buffer, Matchers.contains( writeEvents ) );
		 }

		 public override void WriteNull()
		 {
			  Buffer.Add( null );
		 }

		 public override void WriteBoolean( bool value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteInteger( sbyte value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteInteger( short value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteInteger( int value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteInteger( long value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteFloatingPoint( float value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteFloatingPoint( double value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteString( string value )
		 {
			  Buffer.Add( value );
		 }

		 public override void WriteString( char value )
		 {
			  Buffer.Add( value );
		 }

		 public override void BeginArray( int size, ValueWriter_ArrayType arrayType )
		 {
			  Buffer.Add( Specials.BeginArray( size, arrayType ) );
		 }

		 public override void EndArray()
		 {
			  Buffer.Add( Specials.EndArray() );
		 }

		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  Buffer.Add( new PointValue( crs, coordinate ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws RuntimeException
		 public override void WriteByteArray( sbyte[] value )
		 {
			  Buffer.Add( Specials.ByteArray( value ) );
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  Buffer.Add( DurationValue.Duration( months, days, seconds, nanos ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws RuntimeException
		 public override void WriteDate( LocalDate localDate )
		 {
			  Buffer.Add( DateValue.Date( localDate ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws RuntimeException
		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  Buffer.Add( LocalTimeValue.LocalTime( localTime ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws RuntimeException
		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  Buffer.Add( TimeValue.Time( offsetTime ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws RuntimeException
		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  Buffer.Add( LocalDateTimeValue.LocalDateTime( localDateTime ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws RuntimeException
		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  Buffer.Add( DateTimeValue.Datetime( zonedDateTime ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static class Specials
		 public class Specials
		 {
			  public static Special ByteArray( sbyte[] value )
			  {
					return new Special( WriteByteArray, Arrays.GetHashCode( value ) );
			  }

			  public static Special BeginArray( int size, ValueWriter_ArrayType arrayType )
			  {
					return new Special( BeginArray, size + arrayType.ToString() );
			  }

			  public static Special EndArray()
			  {
					return new Special( EndArray, 0 );
			  }
		 }
	}

}