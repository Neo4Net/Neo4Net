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
namespace Neo4Net.Kernel.impl.coreapi
{

	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Neo4Net.Graphdb.index;
	using AutoIndexOperations = Neo4Net.Kernel.api.explicitindex.AutoIndexOperations;

	/// <summary>
	/// Facade exposing auto indexing operations for nodes.
	/// </summary>
	public class AutoIndexerFacade<T> : Neo4Net.Graphdb.index.AutoIndexer<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 private readonly System.Func<ReadableIndex<T>> _indexProvider;
		 private readonly AutoIndexOperations _autoIndexing;

		 public AutoIndexerFacade( System.Func<ReadableIndex<T>> indexProvider, AutoIndexOperations autoIndexing )
		 {
			  this._indexProvider = indexProvider;
			  this._autoIndexing = autoIndexing;
		 }

		 public virtual bool Enabled
		 {
			 set
			 {
				  _autoIndexing.enabled( value );
			 }
			 get
			 {
				  return _autoIndexing.enabled();
			 }
		 }


		 public virtual ReadableIndex<T> AutoIndex
		 {
			 get
			 {
				  return _indexProvider.get();
			 }
		 }

		 public override void StartAutoIndexingProperty( string propName )
		 {
			  _autoIndexing.startAutoIndexingProperty( propName );
		 }

		 public override void StopAutoIndexingProperty( string propName )
		 {
			  _autoIndexing.stopAutoIndexingProperty( propName );
		 }

		 public virtual ISet<string> AutoIndexedProperties
		 {
			 get
			 {
				  return _autoIndexing.AutoIndexedProperties;
			 }
		 }
	}

}