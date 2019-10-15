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

	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;

	public class StringArray : TextArray
	{
		 private readonly string[] _value;

		 internal StringArray( string[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 public override int Length()
		 {
			  return _value.Length;
		 }

		 public override string StringValue( int offset )
		 {
			  return _value[offset];
		 }

		 public virtual bool Equals( Value other )
		 {
			  return other.Equals( _value );
		 }

		 public virtual bool Equals( char[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( string[] x )
		 {
			  return Arrays.Equals( _value, x );
		 }

		 public override int ComputeHash()
		 {
			  return Arrays.GetHashCode( _value );
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, _value.Length );
			  foreach ( string s in _value )
			  {
					hash = StringWrappingStringValue.UpdateHash( hashFunction, hash, s );
			  }
			  return hash;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  PrimitiveArrayWriting.WriteTo( writer, _value );
		 }

		 public override string[] AsObjectCopy()
		 {
			  return _value.Clone();
		 }

		 [Obsolete]
		 public override string[] AsObject()
		 {
			  return _value;
		 }

		 public override string PrettyPrint()
		 {
			  return Arrays.ToString( _value );
		 }

		 public override AnyValue Value( int offset )
		 {
			  return Values.StringOrNoValue( StringValue( offset ) );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapStringArray( this );
		 }

		 public override string ToString()
		 {
			  return format( "%s%s", TypeName, Arrays.ToString( _value ) );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "StringArray";
			 }
		 }
	}

}