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
namespace Org.Neo4j.Values.Storable
{

	using Geometry = Org.Neo4j.Graphdb.spatial.Geometry;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public abstract class TemporalArray<T extends java.time.temporal.Temporal & Comparable<? super T>, V extends TemporalValue<T,V>> extends NonPrimitiveArray<T>
	public abstract class TemporalArray<T, V> : NonPrimitiveArray<T> where V : TemporalValue<T,V>
	{

		 public override bool Equals( Geometry[] x )
		 {
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final <E extends Exception> void writeTo(ValueWriter<E> writer, ValueWriter_ArrayType type, java.time.temporal.Temporal[] values) throws E
		 protected internal void WriteTo<E>( ValueWriter<E> writer, ValueWriter_ArrayType type, Temporal[] values ) where E : Exception
		 {
			  writer.BeginArray( values.Length, type );
			  foreach ( Temporal x in values )
			  {
					Value value = Values.TemporalValue( x );
					value.WriteTo( writer );
			  }
			  writer.EndArray();
		 }

		 public override AnyValue Value( int offset )
		 {
			  return Values.TemporalValue( value()[offset] );
		 }
	}

}