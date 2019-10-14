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
namespace Neo4Net.Graphdb.index
{


	/// <summary>
	/// A utility class for creating unique (with regard to a given index) entities.
	/// 
	/// Uses the <seealso cref="Index.putIfAbsent(PropertyContainer, string, object) putIfAbsent() method"/> of the referenced index.
	/// </summary>
	/// @param <T> the type of entity created by this <seealso cref="UniqueFactory"/>.
	/// </param>
	/// @deprecated This API will be removed in next major release. Please consider using schema constraints and the Cypher {@code MERGE} clause instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema constraints and the Cypher {@code MERGE} clause instead.")]
	public abstract class UniqueFactory<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 private readonly Index<T> _index;

		 [Obsolete]
		 public class UniqueEntity<T> where T : Neo4Net.Graphdb.PropertyContainer
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly T EntityConflict;
			  internal readonly bool Created;

			  internal UniqueEntity( T entity, bool created )
			  {
					this.EntityConflict = entity;
					this.Created = created;
			  }

			  [Obsolete]
			  public virtual T Entity()
			  {
					return this.EntityConflict;
			  }

			  [Obsolete]
			  public virtual bool WasCreated()
			  {
					return this.Created;
			  }
		 }

		 /// <summary>
		 /// Implementation of <seealso cref="UniqueFactory"/> for <seealso cref="Node"/>.
		 /// </summary>
		 [Obsolete]
		 public abstract class UniqueNodeFactory : UniqueFactory<Node>
		 {
			  /// <summary>
			  /// Create a new <seealso cref="UniqueFactory"/> for nodes.
			  /// </summary>
			  /// <param name="index"> the index to store entities uniquely in. </param>
			  [Obsolete]
			  public UniqueNodeFactory( Index<Node> index ) : base( index )
			  {
			  }

			  /// <summary>
			  /// Create a new <seealso cref="UniqueFactory"/> for nodes.
			  /// </summary>
			  /// <param name="graphdb"> the graph database to get the index from. </param>
			  /// <param name="index"> the name of the index to store entities uniquely in. </param>
			  [Obsolete]
			  public UniqueNodeFactory( GraphDatabaseService graphdb, string index ) : base( graphdb.Index().forNodes(index) )
			  {
			  }

			  /// <summary>
			  /// Default implementation of <seealso cref="UniqueFactory.create(System.Collections.IDictionary)"/>, creates a plain node. Override to
			  /// retrieve the node to add to the index by some other means than by creating it. For initialization of the
			  /// <seealso cref="Node"/>, use the <seealso cref="UniqueFactory.initialize(PropertyContainer, System.Collections.IDictionary)"/> method.
			  /// </summary>
			  /// <seealso cref= UniqueFactory#create(Map) </seealso>
			  /// <seealso cref= UniqueFactory#initialize(PropertyContainer, Map) </seealso>
			  [Obsolete]
			  protected internal override Node Create( IDictionary<string, object> properties )
			  {
					return GraphDatabase().createNode();
			  }

			  /// <summary>
			  /// Default implementation of <seealso cref="UniqueFactory.delete(PropertyContainer)"/>. Invokes
			  /// <seealso cref="Node.delete()"/>.
			  /// </summary>
			  /// <seealso cref= UniqueFactory#delete(PropertyContainer) </seealso>
			  [Obsolete]
			  protected internal override void Delete( Node node )
			  {
					node.Delete();
			  }
		 }

		 /// <summary>
		 /// Implementation of <seealso cref="UniqueFactory"/> for <seealso cref="Relationship"/>.
		 /// </summary>
		 [Obsolete]
		 public abstract class UniqueRelationshipFactory : UniqueFactory<Relationship>
		 {
			  /// <summary>
			  /// Create a new <seealso cref="UniqueFactory"/> for relationships.
			  /// </summary>
			  /// <param name="index"> the index to store entities uniquely in. </param>
			  [Obsolete]
			  public UniqueRelationshipFactory( Index<Relationship> index ) : base( index )
			  {
			  }

			  /// <summary>
			  /// Create a new <seealso cref="UniqueFactory"/> for relationships.
			  /// </summary>
			  /// <param name="graphdb"> the graph database to get the index from. </param>
			  /// <param name="index"> the name of the index to store entities uniquely in. </param>
			  [Obsolete]
			  public UniqueRelationshipFactory( GraphDatabaseService graphdb, string index ) : base( graphdb.Index().forRelationships(index) )
			  {
			  }

			  /// <summary>
			  /// Default implementation of <seealso cref="UniqueFactory.initialize(PropertyContainer, System.Collections.IDictionary)"/>, does nothing
			  /// for <seealso cref="Relationship Relationships"/>. Override to perform some action with the guarantee that this method
			  /// is only invoked for the transaction that succeeded in creating the <seealso cref="Relationship"/>.
			  /// </summary>
			  /// <seealso cref= UniqueFactory#initialize(PropertyContainer, Map) </seealso>
			  /// <seealso cref= UniqueFactory#create(Map) </seealso>
			  [Obsolete]
			  protected internal override void Initialize( Relationship relationship, IDictionary<string, object> properties )
			  {
					// this class has the create() method, initialize() is optional
			  }

			  /// <summary>
			  /// Default implementation of <seealso cref="UniqueFactory.delete(PropertyContainer)"/>. Invokes
			  /// <seealso cref="Relationship.delete()"/>.
			  /// </summary>
			  /// <seealso cref= UniqueFactory#delete(PropertyContainer) </seealso>
			  [Obsolete]
			  protected internal override void Delete( Relationship relationship )
			  {
					relationship.Delete();
			  }
		 }

		 private UniqueFactory( Index<T> index )
		 {
			  this._index = index;
		 }

		 /// <summary>
		 /// Implement this method to create the <seealso cref="Node"/> or <seealso cref="Relationship"/> to index.
		 /// 
		 /// This method will be invoked exactly once per transaction that attempts to create an entry in the index.
		 /// The created entity might be discarded if another thread creates an entity with the same mapping concurrently.
		 /// </summary>
		 /// <param name="properties"> the properties that this entity will is to be indexed uniquely with. </param>
		 /// <returns> the entity to add to the index. </returns>
		 [Obsolete]
		 protected internal abstract T Create( IDictionary<string, object> properties );

		 /// <summary>
		 /// Implement this method to initialize the <seealso cref="Node"/> or <seealso cref="Relationship"/> created for being stored in the index.
		 /// 
		 /// This method will be invoked exactly once per created unique entity.
		 /// 
		 /// The created entity might be discarded if another thread creates an entity concurrently.
		 /// This method will however only be invoked in the transaction that succeeds in creating the node.
		 /// </summary>
		 /// <param name="created"> the created entity to initialize. </param>
		 /// <param name="properties"> the properties that this entity was indexed uniquely with. </param>
		 [Obsolete]
		 protected internal abstract void Initialize( T created, IDictionary<string, object> properties );

		 /// <summary>
		 /// Invoked after a new entity has been <seealso cref="create(System.Collections.IDictionary) created"/>, but adding it to the index failed (due to being
		 /// added by another transaction concurrently). The purpose of this method is to undo the {@link #create(Map)
		 /// creation of the entity}, the default implementations of this method remove the entity. Override this method to
		 /// define a different behavior.
		 /// </summary>
		 /// <param name="created"> the entity that was created but was not added to the index. </param>
		 [Obsolete]
		 protected internal abstract void Delete( T created );

		 /// <summary>
		 /// Get the indexed entity, creating it (exactly once) if no indexed entity exists. </summary>
		 /// <param name="key"> the key to find the entity under in the index. </param>
		 /// <param name="value"> the value the key is mapped to for the entity in the index. </param>
		 /// <returns> the unique entity in the index. </returns>
		 [Obsolete]
		 public T GetOrCreate( string key, object value )
		 {
			  return GetOrCreateWithOutcome( key, value ).entity();
		 }

		 /// <summary>
		 /// Get the indexed entity, creating it (exactly once) if no indexed entity exists.
		 /// Includes the outcome, i.e. whether the entity was created or not. </summary>
		 /// <param name="key"> the key to find the entity under in the index. </param>
		 /// <param name="value"> the value the key is mapped to for the entity in the index. </param>
		 /// <returns> the unique entity in the index as well as whether or not it was created,
		 /// wrapped in a <seealso cref="UniqueEntity"/>. </returns>
		 [Obsolete]
		 public UniqueEntity<T> GetOrCreateWithOutcome( string key, object value )
		 {
			  // Index reads implies asserting we're in a transaction.
			  T result = _index.get( key, value ).Single;
			  bool wasCreated = false;
			  if ( result == null )
			  {
					using ( Transaction tx = GraphDatabase().beginTx() )
					{
						 IDictionary<string, object> properties = Collections.singletonMap( key, value );
						 T created = Create( properties );
						 result = _index.putIfAbsent( created, key, value );
						 if ( result == null )
						 {
							  Initialize( created, properties );
							  result = created;
							  wasCreated = true;
						 }
						 else
						 {
							  Delete( created );
						 }
						 tx.Success();
					}
			  }
			  return new UniqueEntity<T>( result, wasCreated );
		 }

		 /// <summary>
		 /// Get the <seealso cref="GraphDatabaseService graph database"/> of the referenced index. </summary>
		 /// <returns> the <seealso cref="GraphDatabaseService graph database"/> of the referenced index. </returns>
		 [Obsolete]
		 protected internal GraphDatabaseService GraphDatabase()
		 {
			  return _index.GraphDatabase;
		 }

		 /// <summary>
		 /// Get the referenced index. </summary>
		 /// <returns> the referenced index. </returns>
		 [Obsolete]
		 protected internal Index<T> Index()
		 {
			  return _index;
		 }
	}

}