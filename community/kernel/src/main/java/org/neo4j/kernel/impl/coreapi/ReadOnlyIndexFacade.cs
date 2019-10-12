using System;

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
namespace Org.Neo4j.Kernel.impl.coreapi
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;

	/// <summary>
	/// Wraps an explicit index to prevent writes to it - exposing it as a read-only index.
	/// </summary>
	public class ReadOnlyIndexFacade<T> : Index<T> where T : Org.Neo4j.Graphdb.PropertyContainer
	{
		 private readonly ReadableIndex<T> @delegate;

		 public ReadOnlyIndexFacade( ReadableIndex<T> @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override string Name
		 {
			 get
			 {
				  return @delegate.Name;
			 }
		 }

		 public override Type<T> EntityType
		 {
			 get
			 {
				  return @delegate.EntityType;
			 }
		 }

		 public override IndexHits<T> Get( string key, object value )
		 {
			  return @delegate.Get( key, value );
		 }

		 public override IndexHits<T> Query( string key, object queryOrQueryObject )
		 {
			  return @delegate.Query( key, queryOrQueryObject );
		 }

		 public override IndexHits<T> Query( object queryOrQueryObject )
		 {
			  return @delegate.Query( queryOrQueryObject );
		 }

		 private System.NotSupportedException ReadOnlyIndex()
		 {
			  return new System.NotSupportedException( "read only index" );
		 }

		 public override void Add( T entity, string key, object value )
		 {
			  throw ReadOnlyIndex();
		 }

		 public override T PutIfAbsent( T entity, string key, object value )
		 {
			  throw ReadOnlyIndex();
		 }

		 public override void Remove( T entity, string key, object value )
		 {
			  throw ReadOnlyIndex();
		 }

		 public override void Remove( T entity, string key )
		 {
			  throw ReadOnlyIndex();
		 }

		 public override void Remove( T entity )
		 {
			  throw ReadOnlyIndex();
		 }

		 public override void Delete()
		 {
			  throw ReadOnlyIndex();
		 }

		 public override bool Writeable
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override GraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return @delegate.GraphDatabase;
			 }
		 }
	}

}