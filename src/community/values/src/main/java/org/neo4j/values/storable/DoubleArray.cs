﻿using System;
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

	public class DoubleArray : FloatingPointArray
	{
		 private readonly double[] _value;

		 internal DoubleArray( double[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 public override int Length()
		 {
			  return _value.Length;
		 }

		 public override double DoubleValue( int index )
		 {
			  return _value[index];
		 }

		 public override int ComputeHash()
		 {
			  return NumberValues.hash( _value );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapDoubleArray( this );
		 }

		 public virtual bool Equals( Value other )
		 {
			  return other.Equals( _value );
		 }

		 public virtual bool Equals( sbyte[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( short[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( int[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( long[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( float[] x )
		 {
			  return PrimitiveArrayValues.Equals( x, _value );
		 }

		 public virtual bool Equals( double[] x )
		 {
			  return Arrays.Equals( x, _value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  PrimitiveArrayWriting.WriteTo( writer, _value );
		 }

		 public override double[] AsObjectCopy()
		 {
			  return _value.Clone();
		 }

		 [Obsolete]
		 public override double[] AsObject()
		 {
			  return _value;
		 }

		 public override string PrettyPrint()
		 {
			  return Arrays.ToString( _value );
		 }

		 public override AnyValue Value( int position )
		 {
			  return Values.DoubleValue( DoubleValue( position ) );
		 }

		 public override string ToString()
		 {
			  return format( "%s%s", TypeName, Arrays.ToString( _value ) );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "DoubleArray";
			 }
		 }
	}

}