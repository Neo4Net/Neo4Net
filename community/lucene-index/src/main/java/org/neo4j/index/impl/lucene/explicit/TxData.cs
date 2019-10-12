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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;

	using QueryContext = Org.Neo4j.Index.lucene.QueryContext;

	internal abstract class TxData
	{
		 internal readonly LuceneExplicitIndex Index;

		 internal TxData( LuceneExplicitIndex index )
		 {
			  this.Index = index;
		 }

		 internal abstract void Add( TxDataHolder holder, EntityId entityId, string key, object value );

		 internal abstract void Remove( TxDataHolder holder, EntityId entityId, string key, object value );

		 internal abstract ICollection<EntityId> Query( TxDataHolder holder, Query query, QueryContext contextOrNull );

		 internal abstract ICollection<EntityId> Get( TxDataHolder holder, string key, object value );

		 internal abstract ICollection<EntityId> GetOrphans( string key );

		 internal abstract void Close();

		 internal abstract IndexSearcher AsSearcher( TxDataHolder holder, QueryContext context );
	}

}