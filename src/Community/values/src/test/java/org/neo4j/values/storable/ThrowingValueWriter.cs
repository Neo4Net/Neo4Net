using System;

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
namespace Neo4Net.Values.Storable
{

	public abstract class ThrowingValueWriter<E> : ValueWriter<E> where E : Exception
	{
		public abstract void WriteUTF8( sbyte[] bytes, int offset, int length );
		 protected internal abstract E Exception( string method );

		 internal static ValueWriter<E> Throwing<E>( System.Func<E> exception ) where E : Exception
		 {
			  return new ThrowingValueWriterAnonymousInnerClass( exception );
		 }

		 private class ThrowingValueWriterAnonymousInnerClass : ThrowingValueWriter<E>
		 {
			 private System.Func<E> _exception;

			 public ThrowingValueWriterAnonymousInnerClass( System.Func<E> exception )
			 {
				 this._exception = exception;
			 }

			 protected internal override E exception( string method )
			 {
				  return _exception();
			 }
		 }

		 public abstract class AssertOnly : ThrowingValueWriter<Exception>
		 {
			  protected internal override Exception Exception( string method )
			  {
					throw new AssertionError( method );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNull() throws E
		 public override void WriteNull()
		 {
			  throw Exception( "writeNull" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolean(boolean value) throws E
		 public override void WriteBoolean( bool value )
		 {
			  throw Exception( "writeBoolean" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(byte value) throws E
		 public override void WriteInteger( sbyte value )
		 {
			  throw Exception( "writeInteger" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(short value) throws E
		 public override void WriteInteger( short value )
		 {
			  throw Exception( "writeInteger" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(int value) throws E
		 public override void WriteInteger( int value )
		 {
			  throw Exception( "writeInteger" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(long value) throws E
		 public override void WriteInteger( long value )
		 {
			  throw Exception( "writeInteger" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(float value) throws E
		 public override void WriteFloatingPoint( float value )
		 {
			  throw Exception( "writeFloatingPoint" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(double value) throws E
		 public override void WriteFloatingPoint( double value )
		 {
			  throw Exception( "writeFloatingPoint" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(String value) throws E
		 public override void WriteString( string value )
		 {
			  throw Exception( "writeString" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(char value) throws E
		 public override void WriteString( char value )
		 {
			  throw Exception( "writeString" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginArray(int size, ValueWriter_ArrayType arrayType) throws E
		 public override void BeginArray( int size, ValueWriter_ArrayType arrayType )
		 {
			  throw Exception( "beginArray" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endArray() throws E
		 public override void EndArray()
		 {
			  throw Exception( "endArray" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws E
		 public override void WriteByteArray( sbyte[] value )
		 {
			  throw Exception( "writeByteArray" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(CoordinateReferenceSystem crs, double[] coordinate) throws E
		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  throw Exception( "writePoint" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws E
		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  throw Exception( "writeDuration" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws E
		 public override void WriteDate( LocalDate localDate )
		 {
			  throw Exception( "writeDate" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws E
		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  throw Exception( "writeLocalTime" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws E
		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  throw Exception( "writeTime" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws E
		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  throw Exception( "writeLocalDateTime" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws E
		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  throw Exception( "writeDateTime" );
		 }
	}

}