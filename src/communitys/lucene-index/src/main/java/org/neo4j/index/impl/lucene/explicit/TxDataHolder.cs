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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;


	using QueryContext = Neo4Net.Index.lucene.QueryContext;

	internal class TxDataHolder : System.IDisposable
	{
		 internal readonly LuceneExplicitIndex Index;
		 private TxData _data;

		 internal TxDataHolder( LuceneExplicitIndex index, TxData initialData )
		 {
			  this.Index = index;
			  this._data = initialData;
		 }

		 internal virtual void Add( EntityId entityId, string key, object value )
		 {
			  this._data.add( this, entityId, key, value );
		 }

		 internal virtual void Remove( EntityId entityId, string key, object value )
		 {
			  this._data.remove( this, entityId, key, value );
		 }

		 internal virtual ICollection<EntityId> Query( Query query, QueryContext contextOrNull )
		 {
			  return this._data.query( this, query, contextOrNull );
		 }

		 internal virtual ICollection<EntityId> Get( string key, object value )
		 {
			  return this._data.get( this, key, value );
		 }

		 internal virtual ICollection<EntityId> GetOrphans( string key )
		 {
			  return this._data.getOrphans( key );
		 }

		 public override void Close()
		 {
			  this._data.close();
		 }

		 internal virtual IndexSearcher AsSearcher( QueryContext context )
		 {
			  return this._data.asSearcher( this, context );
		 }

		 internal virtual void Set( TxData newData )
		 {
			  this._data = newData;
		 }
	}

}