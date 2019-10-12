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
namespace Org.Neo4j.Graphdb.index
{

	/// <summary>
	/// An index that allows for read only operations. Can also be seen as a chopped
	/// down version of <seealso cref="Index"/> that disallows mutating operations.
	/// </summary>
	/// @param <T> The Primitive this Index holds </param>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public interface ReadableIndex<T> where T : Org.Neo4j.Graphdb.PropertyContainer
	{
		 /// <returns> the name of the index, i.e. the name this index was
		 /// created with. </returns>
		 [Obsolete]
		 string Name { get; }

		 /// <returns> the type of entities are managed by this index. </returns>
		 [Obsolete]
		 Type<T> EntityType { get; }

		 /// <summary>
		 /// Returns exact matches from this index, given the key/value pair. Matches
		 /// will be for key/value pairs just as they were added by the
		 /// <seealso cref="Index.add(PropertyContainer, string, object)"/> method.
		 /// </summary>
		 /// <param name="key"> the key in the key/value pair to match. </param>
		 /// <param name="value"> the value in the key/value pair to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 ///         result set isn't looped through, <seealso cref="IndexHits.close()"/> must
		 ///         be called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<T> Get( string key, object value );

		 /// <summary>
		 /// Returns matches from this index based on the supplied {@code key} and
		 /// query object, which can be a query string or an implementation-specific
		 /// query object.
		 /// </summary>
		 /// <param name="key"> the key in this query. </param>
		 /// <param name="queryOrQueryObject"> the query for the {@code key} to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 /// result set isn't looped through, <seealso cref="IndexHits.close()"/> must be
		 /// called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<T> Query( string key, object queryOrQueryObject );

		 /// <summary>
		 /// Returns matches from this index based on the supplied query object,
		 /// which can be a query string or an implementation-specific query object.
		 /// </summary>
		 /// <param name="queryOrQueryObject"> the query to match. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 /// result set isn't looped through, <seealso cref="IndexHits.close()"/> must be
		 /// called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<T> Query( object queryOrQueryObject );

		 /// <summary>
		 /// A ReadableIndex is possible to support mutating operations as well. This
		 /// method returns true iff such operations are supported by the
		 /// implementation.
		 /// </summary>
		 /// <returns> true iff mutating operations are supported. </returns>
		 [Obsolete]
		 bool Writeable { get; }

		 /// <summary>
		 /// Get the <seealso cref="GraphDatabaseService graph database"/> that owns this index. </summary>
		 /// <returns> the <seealso cref="GraphDatabaseService graph database"/> that owns this index. </returns>
		 [Obsolete]
		 GraphDatabaseService GraphDatabase { get; }
	}

}