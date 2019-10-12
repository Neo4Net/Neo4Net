using System;
using System.Collections.Generic;

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
namespace Neo4Net.Cypher.@internal.javacompat
{

	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Result = Neo4Net.Graphdb.Result;

	public class MapRow : Neo4Net.Graphdb.Result_ResultRow
	{
		 private readonly IDictionary<string, object> _map;

		 public MapRow( IDictionary<string, object> map )
		 {
			  this._map = map;
		 }

		 private T Get<T>( string key, Type type )
		 {
				 type = typeof( T );
			  object value = _map[key];
			  if ( value == null )
			  {
					if ( !_map.ContainsKey( key ) )
					{
						 throw new NoSuchElementException( "No such entry: " + key );
					}
			  }
			  try
			  {
					return type.cast( value );
			  }
			  catch ( System.InvalidCastException e )
			  {
					throw ( NoSuchElementException ) ( new NoSuchElementException( "Element '" + key + "' is not a " + type.Name ) ).initCause( e );
			  }
		 }

		 public override Node GetNode( string key )
		 {
			  return Get( key, typeof( Node ) );
		 }

		 public override Relationship GetRelationship( string key )
		 {
			  return Get( key, typeof( Relationship ) );
		 }

		 public override object Get( string key )
		 {
			  return Get( key, typeof( object ) );
		 }

		 public override string GetString( string key )
		 {
			  return Get( key, typeof( string ) );
		 }

		 public override Number GetNumber( string key )
		 {
			  return Get( key, typeof( Number ) );
		 }

		 public override bool? GetBoolean( string key )
		 {
			  return Get( key, typeof( Boolean ) );
		 }

		 public override Path GetPath( string key )
		 {
			  return Get( key, typeof( Path ) );
		 }
	}

}