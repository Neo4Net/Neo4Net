using System;
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
namespace Neo4Net.@unsafe.Batchinsert
{

	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.GraphDb.Index;

	/// <summary>
	/// The <seealso cref="BatchInserter"/> version of <seealso cref="Index"/>. Additions/updates to a
	/// <seealso cref="BatchInserterIndex"/> doesn't necessarily gets added to the actual index
	/// immediately, but are instead forced to be written when the index is shut
	/// down, <seealso cref="BatchInserterIndexProvider.shutdown()"/>.
	/// 
	/// To guarantee additions/updates are seen by <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>,
	/// <seealso cref="get(string, object)"/>, <seealso cref="query(string, object)"/> and
	/// <seealso cref="query(object)"/> a call to <seealso cref="flush()"/> must be made prior to
	/// calling such a method. This enables implementations more flexibility in
	/// making for performance optimizations.
	/// </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public interface BatchInserterIndex
	{
		 /// <summary>
		 /// Adds key/value pairs for {@code IEntity} to the index. If there's a
		 /// previous index for {@code IEntity} it will co-exist with this new one.
		 /// This behavior is because of performance reasons, to not being forced to
		 /// check if indexing for {@code IEntity} already exists or not. If you need
		 /// to update indexing for {@code IEntity} and it's ok with a slower indexing
		 /// process use <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/> instead.
		 /// 
		 /// Entries added to the index aren't necessarily written to the index and to
		 /// disk until <seealso cref="BatchInserterIndexProvider.shutdown()"/> has been called.
		 /// Entries added to the index isn't necessarily seen by other methods:
		 /// <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>, <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> until a call to
		 /// <seealso cref="flush()"/> has been made.
		 /// </summary>
		 /// <param name="entityId"> the IEntity (i.e id of <seealso cref="Node"/> or
		 ///            <seealso cref="Relationship"/>) to associate the key/value pairs with. </param>
		 /// <param name="properties"> key/value pairs to index for {@code IEntity}. </param>
		 [Obsolete]
		 void Add( long IEntityId, IDictionary<string, object> properties );

		 /// <summary>
		 /// Adds key/value pairs for {@code IEntity} to the index. If there's any
		 /// previous index for {@code IEntity} all such indexed key/value pairs will
		 /// be deleted first. This method can be considerably slower than
		 /// <seealso cref="add(long, System.Collections.IDictionary)"/> because it must check if there are properties
		 /// already indexed for {@code IEntity}. So if you know that there's no
		 /// previous indexing for {@code IEntity} use <seealso cref="add(long, System.Collections.IDictionary)"/> instead.
		 /// 
		 /// Entries added to the index aren't necessarily written to the index and to
		 /// disk until <seealso cref="BatchInserterIndexProvider.shutdown()"/> has been called.
		 /// Entries added to the index isn't necessarily seen by other methods:
		 /// <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>, <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> until a call to
		 /// <seealso cref="flush()"/> has been made. So only entries added before the most
		 /// recent <seealso cref="flush()"/> are guaranteed to be found by this method.
		 /// </summary>
		 /// <param name="entityId"> the IEntity (i.e id of <seealso cref="Node"/> or
		 ///            <seealso cref="Relationship"/>) to associate the key/value pairs with. </param>
		 /// <param name="properties"> key/value pairs to index for {@code IEntity}. </param>
		 [Obsolete]
		 void UpdateOrAdd( long IEntityId, IDictionary<string, object> properties );

		 /// <summary>
		 /// Returns exact matches from this index, given the key/value pair. Matches
		 /// will be for key/value pairs just as they were added by the
		 /// <seealso cref="add(long, System.Collections.IDictionary)"/> or <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/> method.
		 /// 
		 /// Entries added to the index aren't necessarily written to the index and to
		 /// disk until <seealso cref="BatchInserterIndexProvider.shutdown()"/> has been called.
		 /// Entries added to the index isn't necessarily seen by other methods:
		 /// <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>, <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> until a call to
		 /// <seealso cref="flush()"/> has been made. So only entries added before the most
		 /// recent <seealso cref="flush()"/> are guaranteed to be found by this method.
		 /// </summary>
		 /// <param name="key"> the key in the key/value pair to match. </param>
		 /// <param name="value"> the value in the key/value pair to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 ///         result set isn't looped through, <seealso cref="IndexHits.close()"/> must
		 ///         be called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<long> Get( string key, object value );

		 /// <summary>
		 /// Returns matches from this index based on the supplied {@code key} and
		 /// query object, which can be a query string or an implementation-specific
		 /// query object.
		 /// 
		 /// Entries added to the index aren't necessarily written to the index and
		 /// to disk until <seealso cref="BatchInserterIndexProvider.shutdown()"/> has been
		 /// called. Entries added to the index isn't necessarily seen by other
		 /// methods: <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>, <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> until a call
		 /// to <seealso cref="flush()"/> has been made. So only entries added before the most
		 /// recent <seealso cref="flush()"/> are guaranteed to be found by this method.
		 /// </summary>
		 /// <param name="key"> the key in this query. </param>
		 /// <param name="queryOrQueryObject"> the query for the {@code key} to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 /// result set isn't looped through, <seealso cref="IndexHits.close()"/> must be
		 /// called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<long> Query( string key, object queryOrQueryObject );

		 /// <summary>
		 /// Returns matches from this index based on the supplied query object,
		 /// which can be a query string or an implementation-specific query object.
		 /// 
		 /// Entries added to the index aren't necessarily written to the index and
		 /// to disk until <seealso cref="BatchInserterIndexProvider.shutdown()"/> has been
		 /// called. Entries added to the index isn't necessarily seen by other
		 /// methods: <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/>, <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> until a call
		 /// to <seealso cref="flush()"/> has been made. So only entries added before the most
		 /// recent <seealso cref="flush()"/> are guaranteed to be found by this method.
		 /// </summary>
		 /// <param name="queryOrQueryObject"> the query to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 /// result set isn't looped through, <seealso cref="IndexHits.close()"/> must be
		 /// called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<long> Query( object queryOrQueryObject );

		 /// <summary>
		 /// Makes sure additions/updates can be seen by <seealso cref="get(string, object)"/>,
		 /// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> so that they
		 /// are guaranteed to return correct results. Also
		 /// <seealso cref="updateOrAdd(long, System.Collections.IDictionary)"/> will find previous indexing correctly
		 /// after a flush.
		 /// </summary>
		 [Obsolete]
		 void Flush();

		 /// <summary>
		 /// Sets the cache size for key/value pairs for the given {@code key}.
		 /// Caching values may increase <seealso cref="get(string, object)"/> lookups significantly,
		 /// but may at the same time slow down insertion of data some.
		 /// 
		 /// Be sure to call this method to enable caching for keys that will be
		 /// used a lot in lookups. It's also best to call this method for your keys
		 /// right after the index has been created.
		 /// </summary>
		 /// <param name="key"> the key to set cache capacity for. </param>
		 /// <param name="size"> the number of values to cache results for. </param>
		 [Obsolete]
		 void SetCacheCapacity( string key, int size );
	}

}