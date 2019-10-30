
//////////////////// OBSOLETE $!!$ tac
//////////////////////using System;

///////////////////////*
////////////////////// * Copyright © 2018-2020 "Neo4Net,"
////////////////////// * Team NeoN [http://neo4net.com]. All Rights Reserved.
////////////////////// *
////////////////////// * This file is part of Neo4Net.
////////////////////// *
////////////////////// * Neo4Net is free software: you can redistribute it and/or modify
////////////////////// * it under the terms of the GNU General Public License as published by
////////////////////// * the Free Software Foundation, either version 3 of the License, or
////////////////////// * (at your option) any later version.
////////////////////// *
////////////////////// * This program is distributed in the hope that it will be useful,
////////////////////// * but WITHOUT ANY WARRANTY; without even the implied warranty of
////////////////////// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
////////////////////// * GNU General Public License for more details.
////////////////////// *
////////////////////// * You should have received a copy of the GNU General Public License
////////////////////// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
////////////////////// */
//////////////////////namespace Neo4Net.GraphDb.Index
//////////////////////{

//////////////////////	/// <summary>
//////////////////////	/// An index to associate key/value pairs with entities (<seealso cref="INode"/>s or
//////////////////////	/// <seealso cref="IRelationship"/>s) for fast lookup and querying. Any number of key/value
//////////////////////	/// pairs can be associated with any number of entities using
//////////////////////	/// <seealso cref="add(PropertyContainer, string, object)"/> and dissociated with
//////////////////////	/// <seealso cref="remove(PropertyContainer, string, object)"/>. Querying is done using
//////////////////////	/// <seealso cref="get(string, object)"/> for exact lookups and <seealso cref="query(object)"/> or
//////////////////////	/// <seealso cref="query(string, object)"/> for more advanced querying, exposing querying
//////////////////////	/// capabilities from the backend which is backing this particular index.
//////////////////////	/// 
//////////////////////	/// Write operations participates in transactions so committing and rolling back
//////////////////////	/// works the same way as usual in Neo4Net.
//////////////////////	/// 
//////////////////////	/// @author Mattias Persson
//////////////////////	/// </summary>
//////////////////////	/// @param <T> The type of entities this index manages. It may be either
//////////////////////	/// <seealso cref="INode"/>s or <seealso cref="IRelationship"/>s.
//////////////////////	/// </param>
//////////////////////	/// @deprecated This API will be removed in the next major release. Please consider using schema indexes instead. 
//////////////////////	[Obsolete("This API will be removed in the next major release. Please consider using schema indexes instead.")]
//////////////////////	public interface IIndex<T> : ReadableIndex<T> where T : Neo4Net.GraphDb.PropertyContainer
//////////////////////	{
//////////////////////		 /// <summary>
//////////////////////		 /// Adds a key/value pair for {@code IEntity} to the index. If that key/value
//////////////////////		 /// pair for the IEntity is already in the index it's up to the
//////////////////////		 /// implementation to make it so that such an operation is idempotent.
//////////////////////		 /// </summary>
//////////////////////		 /// <param name="entity"> the IEntity (i.e <seealso cref="INode"/> or <seealso cref="IRelationship"/>)
//////////////////////		 /// to associate the key/value pair with. </param>
//////////////////////		 /// <param name="key"> the key in the key/value pair to associate with the IEntity. </param>
//////////////////////		 /// <param name="value"> the value in the key/value pair to associate with the
//////////////////////		 /// IEntity. </param>
//////////////////////		 [Obsolete]
//////////////////////		 void Add( T IEntity, string key, object value );

//////////////////////		 /// <summary>
//////////////////////		 /// Removes a key/value pair for {@code IEntity} from the index. If that
//////////////////////		 /// key/value pair isn't associated with {@code IEntity} in this index this
//////////////////////		 /// operation doesn't do anything.
//////////////////////		 /// </summary>
//////////////////////		 /// <param name="entity"> the IEntity (i.e <seealso cref="INode"/> or <seealso cref="IRelationship"/>)
//////////////////////		 /// to dissociate the key/value pair from. </param>
//////////////////////		 /// <param name="key"> the key in the key/value pair to dissociate from the IEntity. </param>
//////////////////////		 /// <param name="value"> the value in the key/value pair to dissociate from the
//////////////////////		 /// IEntity. </param>
//////////////////////		 [Obsolete]
//////////////////////		 void Remove( T IEntity, string key, object value );

//////////////////////		 /// <summary>
//////////////////////		 /// Removes key/value pairs for {@code IEntity} where key is {@code key}
//////////////////////		 /// from the index.
//////////////////////		 /// 
//////////////////////		 /// Implementations can choose to not implement this method and should
//////////////////////		 /// in that case throw <seealso cref="System.NotSupportedException"/>.
//////////////////////		 /// </summary>
//////////////////////		 /// <param name="entity"> the IEntity (<seealso cref="INode"/> or <seealso cref="IRelationship"/>) to
//////////////////////		 /// remove the this index. </param>
//////////////////////		 /// <param name="key"> the key associated with the index entry </param>
//////////////////////		 [Obsolete]
//////////////////////		 void Remove( T IEntity, string key );

//////////////////////		 /// <summary>
//////////////////////		 /// Removes an IEntity from the index and all its key/value pairs which
//////////////////////		 /// has been previously associated using
//////////////////////		 /// <seealso cref="add(PropertyContainer, string, object)"/>.
//////////////////////		 /// 
//////////////////////		 /// Implementations can choose to not implement this method and should
//////////////////////		 /// in that case throw <seealso cref="System.NotSupportedException"/>.
//////////////////////		 /// </summary>
//////////////////////		 /// <param name="entity"> the IEntity (<seealso cref="INode"/> or <seealso cref="IRelationship"/>) to
//////////////////////		 /// remove the this index. </param>
//////////////////////		 [Obsolete]
//////////////////////		 void Remove( T IEntity );

//////////////////////		 /// <summary>
//////////////////////		 /// Clears the index and deletes the configuration associated with it. After
//////////////////////		 /// this it's invalid to call any other method on this index. However if the
//////////////////////		 /// transaction which the delete operation was called in gets rolled back
//////////////////////		 /// it again becomes ok to use this index.
//////////////////////		 /// </summary>
//////////////////////		 [Obsolete]
//////////////////////		 void Delete();

//////////////////////		 /// <summary>
//////////////////////		 /// Add the IEntity to this index for the given key/value pair if this particular
//////////////////////		 /// key/value pair doesn't already exist.
//////////////////////		 /// 
//////////////////////		 /// This ensures that only one IEntity will be associated with the key/value pair
//////////////////////		 /// even if multiple transactions are trying to add it at the same time. One of those
//////////////////////		 /// transactions will win and add it while the others will block, waiting for the
//////////////////////		 /// winning transaction to finish. If the winning transaction was successful these
//////////////////////		 /// other transactions will return the associated IEntity instead of adding it.
//////////////////////		 /// If it wasn't successful the waiting transactions will begin a new race to add it.
//////////////////////		 /// </summary>
//////////////////////		 /// <param name="entity"> the IEntity (i.e <seealso cref="INode"/> or <seealso cref="IRelationship"/>)
//////////////////////		 /// to associate the key/value pair with. </param>
//////////////////////		 /// <param name="key"> the key in the key/value pair to associate with the IEntity. </param>
//////////////////////		 /// <param name="value"> the value in the key/value pair to associate with the
//////////////////////		 /// IEntity. </param>
//////////////////////		 /// <returns> the previously indexed IEntity, or {@code null} if no IEntity was
//////////////////////		 /// indexed before (and the specified IEntity was added to the index). </returns>
//////////////////////		 [Obsolete]
//////////////////////		 T PutIfAbsent( T IEntity, string key, object value );
//////////////////////	}

//////////////////////}