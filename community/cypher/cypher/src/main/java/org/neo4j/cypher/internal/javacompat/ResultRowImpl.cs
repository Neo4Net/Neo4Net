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
namespace Org.Neo4j.Cypher.@internal.javacompat
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Result = Org.Neo4j.Graphdb.Result;

	public class ResultRowImpl : Org.Neo4j.Graphdb.Result_ResultRow
	{
		 private IDictionary<string, object> _results;

		 public ResultRowImpl( IDictionary<string, object> results )
		 {
			  this._results = results;
		 }

		 public ResultRowImpl() : this(new Dictionary<string, object>())
		 {
		 }

		 public virtual void Set( string k, object value )
		 {
			  _results[k] = value;
		 }

		 public override object Get( string key )
		 {
			  return Get( key, typeof( object ) );
		 }

		 public override Node GetNode( string key )
		 {
			  return Get( key, typeof( Node ) );
		 }

		 public override Relationship GetRelationship( string key )
		 {
			  return Get( key, typeof( Relationship ) );
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

		 private T Get<T>( string key, Type type )
		 {
				 type = typeof( T );
			  object value = _results[key];
			  if ( value == null && !_results.ContainsKey( key ) )
			  {
					throw new System.ArgumentException( "No column \"" + key + "\" exists" );
			  }
			  try
			  {
					return type.cast( value );
			  }
			  catch ( System.InvalidCastException )
			  {
					string message = string.Format( "The current item in column \"{0}\" is not a {1}; it's \"{2}\"", key, type.Name, value );
					throw new NoSuchElementException( message );
			  }
		 }
	}

}