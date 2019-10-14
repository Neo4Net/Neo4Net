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
	using Document = org.apache.lucene.document.Document;
	using Store = org.apache.lucene.document.Field.Store;
	using StringField = org.apache.lucene.document.StringField;

	using Relationship = Neo4Net.Graphdb.Relationship;

	/// <summary>
	/// Represents id data about an entity that is to be indexed.
	/// </summary>
	public interface EntityId
	{
		 /// <returns> the entity id. </returns>
		 long Id();

		 /// <summary>
		 /// Enhances a <seealso cref="Document"/>, adding more id data to it if necessary. </summary>
		 /// <param name="document"> the <seealso cref="Document"/> to enhance. </param>
		 void Enhance( Document document );

		 /// <summary>
		 /// <seealso cref="EntityId"/> only carrying entity id.
		 /// </summary>

		 /// <summary>
		 /// <seealso cref="EntityId"/> including additional start/end node for <seealso cref="Relationship relationships"/>.
		 /// </summary>

		 /// <summary>
		 /// Used in <seealso cref="Collection.contains(object)"/> and <seealso cref="Collection.remove(object)"/> f.ex. to save
		 /// object allocations where you have primitive {@code long} ids and want to call those methods
		 /// on a <seealso cref="System.Collections.ICollection"/> containing <seealso cref="EntityId"/> instances.
		 /// </summary>
	}

	 public abstract class EntityId_AbstractData : EntityId
	 {
		 public abstract void Enhance( Document document );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  protected internal long IdConflict;

		  internal EntityId_AbstractData( long id )
		  {
				this.IdConflict = id;
		  }

		  public override long Id()
		  {
				return IdConflict;
		  }

		  public override bool Equals( object obj )
		  {
				return obj is EntityId && ( ( EntityId ) obj ).Id() == IdConflict;
		  }

		  public override int GetHashCode()
		  {
				return ( int )( ( ( long )( ( ulong )IdConflict >> 32 ) ) ^ IdConflict );
		  }
	 }

	 public class EntityId_IdData : EntityId_AbstractData
	 {
		  public EntityId_IdData( long id ) : base( id )
		  {
		  }

		  public override void Enhance( Document document )
		  { // Nothing to enhance here
		  }
	 }

	 public class EntityId_RelationshipData : EntityId_AbstractData
	 {
		  internal readonly long StartNode;
		  internal readonly long EndNode;

		  public EntityId_RelationshipData( long id, long startNode, long endNode ) : base( id )
		  {
				this.StartNode = startNode;
				this.EndNode = endNode;
		  }

		  public override void Enhance( Document document )
		  {
				document.add( new StringField( LuceneExplicitIndex.KEY_START_NODE_ID, "" + StartNode, Store.YES ) );
				document.add( new StringField( LuceneExplicitIndex.KEY_END_NODE_ID, "" + EndNode, Store.YES ) );
		  }
	 }

	 public class EntityId_LongCostume : EntityId_IdData
	 {
		  public EntityId_LongCostume() : base(-1)
		  {
		  }

		  public virtual EntityId_LongCostume setId( long id )
		  {
				this.IdConflict = id;
				return this;
		  }
	 }

}