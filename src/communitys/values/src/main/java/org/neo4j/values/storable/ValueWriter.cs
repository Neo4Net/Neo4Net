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
namespace Neo4Net.Values.Storable
{

	/// <summary>
	/// Writer of values.
	/// <para>
	/// Has functionality to write all supported primitives, as well as arrays and different representations of Strings.
	/// 
	/// </para>
	/// </summary>
	/// @param <E> type of <seealso cref="System.Exception"/> thrown from writer methods. </param>
	public interface ValueWriter<E> where E : Exception
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeNull() throws E;
		 void WriteNull();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeBoolean(boolean value) throws E;
		 void WriteBoolean( bool value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeInteger(byte value) throws E;
		 void WriteInteger( sbyte value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeInteger(short value) throws E;
		 void WriteInteger( short value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeInteger(int value) throws E;
		 void WriteInteger( int value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeInteger(long value) throws E;
		 void WriteInteger( long value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeFloatingPoint(float value) throws E;
		 void WriteFloatingPoint( float value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeFloatingPoint(double value) throws E;
		 void WriteFloatingPoint( double value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeString(String value) throws E;
		 void WriteString( string value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeString(char value) throws E;
		 void WriteString( char value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void writeUTF8(byte[] bytes, int offset, int length) throws E
	//	 {
	//		  writeString(new String(bytes, offset, length, StandardCharsets.UTF_8));
	//	 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginArray(int size, ValueWriter_ArrayType arrayType) throws E;
		 void BeginArray( int size, ValueWriter_ArrayType arrayType );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void endArray() throws E;
		 void EndArray();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeByteArray(byte[] value) throws E;
		 void WriteByteArray( sbyte[] value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writePoint(CoordinateReferenceSystem crs, double[] coordinate) throws E;
		 void WritePoint( CoordinateReferenceSystem crs, double[] coordinate );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDuration(long months, long days, long seconds, int nanos) throws E;
		 void WriteDuration( long months, long days, long seconds, int nanos );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDate(java.time.LocalDate localDate) throws E;
		 void WriteDate( LocalDate localDate );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeLocalTime(java.time.LocalTime localTime) throws E;
		 void WriteLocalTime( LocalTime localTime );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeTime(java.time.OffsetTime offsetTime) throws E;
		 void WriteTime( OffsetTime offsetTime );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws E;
		 void WriteLocalDateTime( DateTime localDateTime );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws E;
		 void WriteDateTime( ZonedDateTime zonedDateTime );
	}

	 public enum ValueWriter_ArrayType
	 {
		  Byte,
		  Short,
		  Int,
		  Long,
		  Float,
		  Double,
		  Boolean,
		  String,
		  Char,
		  Point,
		  ZonedDateTime,
		  LocalDateTime,
		  Date,
		  ZonedTime,
		  LocalTime,
		  Duration
	 }

	 public class ValueWriter_Adapter<E> : ValueWriter<E> where E : Exception
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNull() throws E
		  public override void WriteNull()
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolean(boolean value) throws E
		  public override void WriteBoolean( bool value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(byte value) throws E
		  public override void WriteInteger( sbyte value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(short value) throws E
		  public override void WriteInteger( short value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(int value) throws E
		  public override void WriteInteger( int value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(long value) throws E
		  public override void WriteInteger( long value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(float value) throws E
		  public override void WriteFloatingPoint( float value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(double value) throws E
		  public override void WriteFloatingPoint( double value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(String value) throws E
		  public override void WriteString( string value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(char value) throws E
		  public override void WriteString( char value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginArray(int size, ValueWriter_ArrayType arrayType) throws E
		  public override void BeginArray( int size, ValueWriter_ArrayType arrayType )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endArray() throws E
		  public override void EndArray()
		  { // no-opa
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws E
		  public override void WriteByteArray( sbyte[] value )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(CoordinateReferenceSystem crs, double[] coordinate) throws E
		  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		  { // no-op
		  }

		  public override void WriteDuration( long months, long days, long seconds, int nanos )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws E
		  public override void WriteDate( LocalDate localDate )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws E
		  public override void WriteLocalTime( LocalTime localTime )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws E
		  public override void WriteTime( OffsetTime offsetTime )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws E
		  public override void WriteLocalDateTime( DateTime localDateTime )
		  { // no-op
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws E
		  public override void WriteDateTime( ZonedDateTime zonedDateTime )
		  { // no-op
		  }
	 }

}