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
namespace Neo4Net.Graphdb.index
{

	/// <summary>
	/// An index to associate key/value pairs with entities (<seealso cref="Node"/>s or
	/// <seealso cref="Relationship"/>s) for fast lookup and querying. Any number of key/value
	/// pairs can be associated with any number of entities using
	/// <seealso cref="add(PropertyContainer, string, object)"/> and dissociated with
	/// <seealso cref="remove(PropertyContainer, string, object)"/>. Querying is done using
	/// <seealso cref="get(string, object)"/> for exact lookups and <seealso cref="query(object)"/> or
	/// <seealso cref="query(string, object)"/> for more advanced querying, exposing querying
	/// capabilities from the backend which is backing this particular index.
	/// 
	/// Write operations participates in transactions so committing and rolling back
	/// works the same way as usual in Neo4j.
	/// 
	/// @author Mattias Persson
	/// </summary>
	/// @param <T> The type of entities this index manages. It may be either
	/// <seealso cref="Node"/>s or <seealso cref="Relationship"/>s.
	/// </param>
	/// @deprecated This API will be removed in the next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in the next major release. Please consider using schema indexes instead.")]
	public interface Index<T> : ReadableIndex<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 /// <summary>
		 /// Adds a key/value pair for {@code entity} to the index. If that key/value
		 /// pair for the entity is already in the index it's up to the
		 /// implementation to make it so that such an operation is idempotent.
		 /// </summary>
		 /// <param name="entity"> the entity (i.e <seealso cref="Node"/> or <seealso cref="Relationship"/>)
		 /// to associate the key/value pair with. </param>
		 /// <param name="key"> the key in the key/value pair to associate with the entity. </param>
		 /// <param name="value"> the value in the key/value pair to associate with the
		 /// entity. </param>
		 [Obsolete]
		 void Add( T entity, string key, object value );

		 /// <summary>
		 /// Removes a key/value pair for {@code entity} from the index. If that
		 /// key/value pair isn't associated with {@code entity} in this index this
		 /// operation doesn't do anything.
		 /// </summary>
		 /// <param name="entity"> the entity (i.e <seealso cref="Node"/> or <seealso cref="Relationship"/>)
		 /// to dissociate the key/value pair from. </param>
		 /// <param name="key"> the key in the key/value pair to dissociate from the entity. </param>
		 /// <param name="value"> the value in the key/value pair to dissociate from the
		 /// entity. </param>
		 [Obsolete]
		 void Remove( T entity, string key, object value );

		 /// <summary>
		 /// Removes key/value pairs for {@code entity} where key is {@code key}
		 /// from the index.
		 /// 
		 /// Implementations can choose to not implement this method and should
		 /// in that case throw <seealso cref="System.NotSupportedException"/>.
		 /// </summary>
		 /// <param name="entity"> the entity (<seealso cref="Node"/> or <seealso cref="Relationship"/>) to
		 /// remove the this index. </param>
		 /// <param name="key"> the key associated with the index entry </param>
		 [Obsolete]
		 void Remove( T entity, string key );

		 /// <summary>
		 /// Removes an entity from the index and all its key/value pairs which
		 /// has been previously associated using
		 /// <seealso cref="add(PropertyContainer, string, object)"/>.
		 /// 
		 /// Implementations can choose to not implement this method and should
		 /// in that case throw <seealso cref="System.NotSupportedException"/>.
		 /// </summary>
		 /// <param name="entity"> the entity (<seealso cref="Node"/> or <seealso cref="Relationship"/>) to
		 /// remove the this index. </param>
		 [Obsolete]
		 void Remove( T entity );

		 /// <summary>
		 /// Clears the index and deletes the configuration associated with it. After
		 /// this it's invalid to call any other method on this index. However if the
		 /// transaction which the delete operation was called in gets rolled back
		 /// it again becomes ok to use this index.
		 /// </summary>
		 [Obsolete]
		 void Delete();

		 /// <summary>
		 /// Add the entity to this index for the given key/value pair if this particular
		 /// key/value pair doesn't already exist.
		 /// 
		 /// This ensures that only one entity will be associated with the key/value pair
		 /// even if multiple transactions are trying to add it at the same time. One of those
		 /// transactions will win and add it while the others will block, waiting for the
		 /// winning transaction to finish. If the winning transaction was successful these
		 /// other transactions will return the associated entity instead of adding it.
		 /// If it wasn't successful the waiting transactions will begin a new race to add it.
		 /// </summary>
		 /// <param name="entity"> the entity (i.e <seealso cref="Node"/> or <seealso cref="Relationship"/>)
		 /// to associate the key/value pair with. </param>
		 /// <param name="key"> the key in the key/value pair to associate with the entity. </param>
		 /// <param name="value"> the value in the key/value pair to associate with the
		 /// entity. </param>
		 /// <returns> the previously indexed entity, or {@code null} if no entity was
		 /// indexed before (and the specified entity was added to the index). </returns>
		 [Obsolete]
		 T PutIfAbsent( T entity, string key, object value );
	}

}