﻿using System;
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
namespace Org.Neo4j.Values.Storable
{

	using HashFunction = Org.Neo4j.Hashing.HashFunction;
	using Org.Neo4j.Values;

	public class CharArray : TextArray
	{
		 private readonly char[] _value;

		 internal CharArray( char[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 public virtual bool Equals( Value other )
		 {
			  return other.Equals( _value );
		 }

		 // TODO: should we support this?
	//    @Override
	//    boolean equals( String x )
	//    {
	//        return false;
	//    }

		 public virtual bool Equals( char[] x )
		 {
			  return Arrays.Equals( _value, x );
		 }

		 public virtual bool Equals( string[] x )
		 {
			  return PrimitiveArrayValues.Equals( _value, x );
		 }

		 public override int ComputeHash()
		 {
			  return NumberValues.hash( _value );
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, _value.Length );
			  foreach ( char c in _value )
			  {
					hash = CharValue.UpdateHash( hashFunction, hash, c );
			  }
			  return hash;
		 }

		 public override int Length()
		 {
			  return _value.Length;
		 }

		 public override string StringValue( int offset )
		 {
			  return Convert.ToString( _value[offset] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  PrimitiveArrayWriting.WriteTo( writer, _value );
		 }

		 public override char[] AsObjectCopy()
		 {
			  return _value.Clone();
		 }

		 [Obsolete]
		 public override char[] AsObject()
		 {
			  return _value;
		 }

		 public override string PrettyPrint()
		 {
			  return Arrays.ToString( _value );
		 }

		 public override AnyValue Value( int position )
		 {
			  return Values.CharValue( _value[position] );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapCharArray( this );
		 }

		 public override string ToString()
		 {
			  return format( "%s%s", TypeName, Arrays.ToString( _value ) );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "CharArray";
			 }
		 }
	}

}