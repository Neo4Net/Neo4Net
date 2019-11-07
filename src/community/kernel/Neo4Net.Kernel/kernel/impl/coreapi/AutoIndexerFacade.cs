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
namespace Neo4Net.Kernel.impl.coreapi
{

	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Neo4Net.GraphDb.Index;
	using AutoIndexOperations = Neo4Net.Kernel.Api.explicitindex.AutoIndexOperations;

	/// <summary>
	/// Facade exposing auto indexing operations for nodes.
	/// </summary>
	public class AutoIndexerFacade<T> : Neo4Net.GraphDb.Index.AutoIndexer<T> where T : Neo4Net.GraphDb.PropertyContainer
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