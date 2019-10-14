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
namespace Neo4Net.Test
{
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;

	public sealed class Property
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Property PropertyConflict( string key, object value )
		 {
			  return new Property( key, value );
		 }

		 public static E Set<E>( E entity, params Property[] properties ) where E : Neo4Net.Graphdb.PropertyContainer
		 {
			  foreach ( Property property in properties )
			  {
					entity.setProperty( property._key, property._value );
			  }
			  return entity;
		 }

		 private readonly string _key;
		 private readonly object _value;

		 private Property( string key, object value )
		 {
			  this._key = key;
			  this._value = value;
		 }

		 public string Key()
		 {
			  return _key;
		 }

		 public object Value()
		 {
			  return _value;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}: {1}", _key, _value );
		 }
	}

}