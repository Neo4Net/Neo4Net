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
	using Neo4Net.Values;

	public sealed class IntValue : IntegralValue
	{
		 private readonly int _value;

		 internal IntValue( int value )
		 {
			  this._value = value;
		 }

		 public int Value()
		 {
			  return _value;
		 }

		 public override long LongValue()
		 {
			  return _value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteInteger( _value );
		 }

		 public override int? AsObjectCopy()
		 {
			  return _value;
		 }

		 public override string PrettyPrint()
		 {
			  return Convert.ToString( _value );
		 }

		 public override string ToString()
		 {
			  return format( "Int(%d)", _value );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapInt( this );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Integer";
			 }
		 }
	}

}