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
namespace Neo4Net.Kernel.api.properties
{
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public class PropertyKeyValue : StorageProperty
	{
		 private readonly int _propertyKeyId;
		 private readonly Value _value;

		 public PropertyKeyValue( int propertyKeyId, Value value )
		 {
			  Debug.Assert( value != null );
			  this._propertyKeyId = propertyKeyId;
			  this._value = value;
		 }

		 public override int PropertyKeyId()
		 {
			  return _propertyKeyId;
		 }

		 public override Value Value()
		 {
			  return _value;
		 }

		 public virtual bool Defined
		 {
			 get
			 {
				  return _value != Values.NO_VALUE;
			 }
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

			  PropertyKeyValue that = ( PropertyKeyValue ) o;

			  return _propertyKeyId == that._propertyKeyId && _value.Equals( that._value );
		 }

		 public override int GetHashCode()
		 {
			  int result = _propertyKeyId;
			  result = 31 * result + _value.GetHashCode();
			  return result;
		 }
	}

}