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
	using Point = Neo4Net.Graphdb.spatial.Point;

	/// <summary>
	/// Static methods for writing primitive arrays to a ValueWriter.
	/// </summary>
	public sealed class PrimitiveArrayWriting
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, byte[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, sbyte[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Byte );
			  foreach ( sbyte x in values )
			  {
					writer.WriteInteger( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, short[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, short[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Short );
			  foreach ( short x in values )
			  {
					writer.WriteInteger( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, int[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, int[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Int );
			  foreach ( int x in values )
			  {
					writer.WriteInteger( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, long[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, long[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Long );
			  foreach ( long x in values )
			  {
					writer.WriteInteger( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, float[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, float[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Float );
			  foreach ( float x in values )
			  {
					writer.WriteFloatingPoint( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, double[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, double[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Double );
			  foreach ( double x in values )
			  {
					writer.WriteFloatingPoint( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, boolean[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, bool[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Boolean );
			  foreach ( bool x in values )
			  {
					writer.WriteBoolean( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, char[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, char[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Char );
			  foreach ( char x in values )
			  {
					writer.WriteString( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, String[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, string[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.String );
			  foreach ( string x in values )
			  {
					writer.WriteString( x );
			  }
			  writer.EndArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> void writeTo(ValueWriter<E> writer, org.neo4j.graphdb.spatial.Point[] values) throws E
		 public static void WriteTo<E>( ValueWriter<E> writer, Point[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, ValueWriter_ArrayType.Point );
			  foreach ( Point x in values )
			  {
					PointValue value = Values.Point( x );
					writer.WritePoint( value.CoordinateReferenceSystem, value.Coordinate() );
			  }
			  writer.EndArray();
		 }
	}

}