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
namespace Org.Neo4j.Kernel.api.properties
{
	using StorageProperty = Org.Neo4j.Storageengine.Api.StorageProperty;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

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