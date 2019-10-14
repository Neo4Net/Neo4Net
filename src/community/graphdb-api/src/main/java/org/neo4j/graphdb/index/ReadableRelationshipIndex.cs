using System;

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
namespace Neo4Net.Graphdb.index
{

	/// <summary>
	/// Extends the <seealso cref="ReadableIndex"/> interface with additional get/query methods
	/// which
	/// are specific to <seealso cref="Relationship"/>s. Each of <seealso cref="get(string, object)"/>,
	/// <seealso cref="query(string, object)"/> and <seealso cref="query(object)"/> have an additional
	/// method which allows efficient filtering on start/end node of the
	/// relationships.
	/// 
	/// @author Mattias Persson </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public interface ReadableRelationshipIndex : ReadableIndex<Relationship>
	{
		 /// <summary>
		 /// Returns exact matches from this index, given the key/value pair. Matches
		 /// will be for key/value pairs just as they were added by the
		 /// <seealso cref="Index.add(org.neo4j.graphdb.PropertyContainer, string, object)"/> method.
		 /// </summary>
		 /// <param name="key"> the key in the key/value pair to match. </param>
		 /// <param name="valueOrNull"> the value in the key/value pair to match. </param>
		 /// <param name="startNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with
		 ///            that given start node will be returned. </param>
		 /// <param name="endNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with that
		 ///            given end node will be returned. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 ///         result set isn't looped through, <seealso cref="IndexHits.close()"/> must
		 ///         be called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<Relationship> Get( string key, object valueOrNull, Node startNodeOrNull, Node endNodeOrNull );

		 /// <summary>
		 /// Returns matches from this index based on the supplied {@code key} and
		 /// query object, which can be a query string or an implementation-specific
		 /// query object.
		 /// </summary>
		 /// <param name="key"> the key in this query. </param>
		 /// <param name="queryOrQueryObjectOrNull"> the query for the {@code key} to match. </param>
		 /// <param name="startNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with
		 ///            that given start node will be returned. </param>
		 /// <param name="endNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with that
		 ///            given end node will be returned. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 ///         result set isn't looped through, <seealso cref="IndexHits.close()"/> must
		 ///         be called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<Relationship> Query( string key, object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull );

		 /// <summary>
		 /// Returns matches from this index based on the supplied query object, which
		 /// can be a query string or an implementation-specific query object.
		 /// </summary>
		 /// <param name="queryOrQueryObjectOrNull"> the query to match. </param>
		 /// <param name="startNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with
		 ///            that given start node will be returned. </param>
		 /// <param name="endNodeOrNull"> filter so that only <seealso cref="Relationship"/>s with that
		 ///            given end node will be returned. </param>
		 /// <returns> the result wrapped in an <seealso cref="IndexHits"/> object. If the entire
		 ///         result set isn't looped through, <seealso cref="IndexHits.close()"/> must
		 ///         be called before disposing of the result. </returns>
		 [Obsolete]
		 IndexHits<Relationship> Query( object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull );
	}

}