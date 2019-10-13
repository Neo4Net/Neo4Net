using System;
using System.Collections.Generic;

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
namespace Neo4Net.@internal.Kernel.Api.helpers
{

	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	public class StubPropertyCursor : PropertyCursor
	{
		 private int _offset = -1;
		 private int?[] _keys;
		 private Value[] _values;

		 internal virtual void Init( IDictionary<int, Value> properties )
		 {
			  _offset = -1;
			  _keys = properties.Keys.toArray( new int?[0] );
			  _values = properties.Values.toArray( new Value[0] );
		 }

		 public override bool Next()
		 {
			  return ++_offset < _keys.Length;
		 }

		 public override void Close()
		 {

		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override int PropertyKey()
		 {
			  return _keys[_offset].Value;
		 }

		 public override ValueGroup PropertyType()
		 {
			  return _values[_offset].valueGroup();
		 }

		 public override Value PropertyValue()
		 {
			  return _values[_offset];
		 }

		 public override void WriteTo<E>( ValueWriter<E> target ) where E : Exception
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool BooleanValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override string StringValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long LongValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override double DoubleValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( long value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( double value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( string value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueMatches( Pattern regex )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThan( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThan( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThan( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThan( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThanOrEqualTo( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThanOrEqualTo( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThanOrEqualTo( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThanOrEqualTo( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }
	}

}