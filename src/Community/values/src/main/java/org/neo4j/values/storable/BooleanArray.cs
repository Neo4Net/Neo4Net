using System;
using System.Diagnostics;

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

	using HashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;

	public class BooleanArray : ArrayValue
	{
		 private readonly bool[] _value;

		 internal BooleanArray( bool[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 public override int Length()
		 {
			  return _value.Length;
		 }

		 public virtual bool BooleanValue( int offset )
		 {
			  return _value[offset];
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "BooleanArray";
			 }
		 }

		 public virtual bool Equals( Value other )
		 {
			  return other.Equals( this._value );
		 }

		 public override bool Equals( bool[] x )
		 {
			  return Arrays.Equals( _value, x );
		 }

		 public override int ComputeHash()
		 {
			  return NumberValues.hash( _value );
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, _value.Length );
			  hash = hashFunction.Update( hash, GetHashCode() );
			  return hash;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapBooleanArray( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  PrimitiveArrayWriting.WriteTo( writer, _value );
		 }

		 public override bool[] AsObjectCopy()
		 {
			  return _value.Clone();
		 }

		 [Obsolete]
		 public override bool[] AsObject()
		 {
			  return _value;
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return NumberValues.CompareBooleanArrays( this, ( BooleanArray ) otherValue );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.BooleanArray;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 public override string PrettyPrint()
		 {
			  return Arrays.ToString( _value );
		 }

		 public override AnyValue Value( int position )
		 {
			  return Values.BooleanValue( BooleanValue( position ) );
		 }

		 public override string ToString()
		 {
			  return format( "%s%s", TypeName, Arrays.ToString( _value ) );
		 }
	}

}