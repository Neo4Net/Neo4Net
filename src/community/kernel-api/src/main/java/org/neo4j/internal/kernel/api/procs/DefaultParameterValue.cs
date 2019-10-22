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
namespace Neo4Net.Internal.Kernel.Api.procs
{

	public class DefaultParameterValue
	{
		 private readonly object _value;
		 private readonly Neo4NetTypes.AnyType _type;

		 public DefaultParameterValue( object value, Neo4NetTypes.AnyType type )
		 {
			  this._value = value;
			  this._type = type;
		 }

		 public virtual object Value()
		 {
			  return _value;
		 }

		 public virtual Neo4NetTypes.AnyType Neo4NetType()
		 {
			  return _type;
		 }

		 public static DefaultParameterValue NtString( string value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTString );
		 }

		 public static DefaultParameterValue NtInteger( long value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTInteger );
		 }

		 public static DefaultParameterValue NtFloat( double value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTFloat );
		 }

		 public static DefaultParameterValue NtBoolean( bool value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTBoolean );
		 }

		 public static DefaultParameterValue NtMap( IDictionary<string, object> value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTMap );
		 }

		 public static DefaultParameterValue NtByteArray( sbyte[] value )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTByteArray );
		 }

		 public static DefaultParameterValue NtList<T1>( IList<T1> value, Neo4NetTypes.AnyType inner )
		 {
			  return new DefaultParameterValue( value, Neo4NetTypes.NTList( inner ) );
		 }

		 public static DefaultParameterValue NullValue( Neo4NetTypes.AnyType type )
		 {
			  return new DefaultParameterValue( null, type );
		 }

		 public override string ToString()
		 {
			  return "DefaultParameterValue{" +
						"value=" + _value +
						", type=" + _type +
						'}';
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  DefaultParameterValue that = ( DefaultParameterValue ) o;

			  if ( _value != null ?!_value.Equals( that._value ) : that._value != null )
			  {
					return false;
			  }
			  return _type.Equals( that._type );
		 }

		 public override int GetHashCode()
		 {
			  int result = _value != null ? _value.GetHashCode() : 0;
			  result = 31 * result + _type.GetHashCode();
			  return result;
		 }
	}

}