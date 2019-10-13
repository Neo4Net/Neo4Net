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

	using Geometry = Neo4Net.Graphdb.spatial.Geometry;
	using Neo4Net.Values;

	public class PointArray : NonPrimitiveArray<PointValue>
	{
		 private readonly PointValue[] _value;

		 internal PointArray( PointValue[] value )
		 {
			  Debug.Assert( value != null );
			  this._value = value;
		 }

		 protected internal override PointValue[] Value()
		 {
			  return _value;
		 }

		 public virtual PointValue PointValue( int offset )
		 {
			  return Value()[offset];
		 }

		 public override bool Equals( Geometry[] x )
		 {
			  return Arrays.Equals( Value(), x );
		 }

		 public override bool Equals( Value other )
		 {
			  return other is PointArray && Arrays.Equals( this.Value(), ((PointArray) other).Value() );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.GeometryArray;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapPointArray( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  PrimitiveArrayWriting.WriteTo( writer, Value() );
		 }

		 public override AnyValue Value( int offset )
		 {
			  return Values.Point( _value[offset] );
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return CompareToNonPrimitiveArray( ( PointArray ) otherValue );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "PointArray";
			 }
		 }
	}

}