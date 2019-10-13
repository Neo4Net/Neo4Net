using System;
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
namespace Neo4Net.Values.Storable
{

	using Neo4Net.Values;

	public class LocalTimeArray : TemporalArray<LocalTime, LocalTimeValue>
	{
		 private readonly LocalTime[] _value;

		 internal LocalTimeArray( LocalTime[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 protected internal override LocalTime[] Value()
		 {
			  return _value;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapLocalTimeArray( this );
		 }

		 public override bool Equals( Value other )
		 {
			  return other.Equals( _value );
		 }

		 public override bool Equals( LocalTime[] x )
		 {
			  return Arrays.Equals( _value, x );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  WriteTo( writer, ValueWriter_ArrayType.LocalTime,_value );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.LocalTimeArray;
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return compareToNonPrimitiveArray( ( LocalTimeArray ) otherValue );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "LocalTimeArray";
			 }
		 }
	}

}