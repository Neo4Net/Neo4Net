using System;
using System.Diagnostics;

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
namespace Neo4Net.Values.@virtual
{
	using Neo4Net.Values;
	using TextValue = Neo4Net.Values.Storable.TextValue;

	public abstract class RelationshipValue : VirtualRelationshipValue
	{
		 private readonly long _id;

		 protected internal RelationshipValue( long id )
		 {
			  this._id = id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteRelationship( _id, StartNode().id(), EndNode().id(), Type(), Properties() );
		 }

		 public override string ToString()
		 {
			  return format( "-[%d]-", _id );
		 }

		 public abstract NodeValue StartNode();

		 public abstract NodeValue EndNode();

		 public override long Id()
		 {
			  return _id;
		 }

		 public abstract TextValue Type();

		 public abstract MapValue Properties();

		 public virtual NodeValue OtherNode( VirtualNodeValue node )
		 {
			  return node.Equals( StartNode() ) ? EndNode() : StartNode();
		 }

		 public virtual long OtherNodeId( long node )
		 {
			  return node == StartNode().id() ? EndNode().id() : StartNode().id();
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Relationship";
			 }
		 }

		 internal class DirectRelationshipValue : RelationshipValue
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly NodeValue StartNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly NodeValue EndNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TextValue TypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly MapValue PropertiesConflict;

			  internal DirectRelationshipValue( long id, NodeValue startNode, NodeValue endNode, TextValue type, MapValue properties ) : base( id )
			  {
					Debug.Assert( properties != null );

					this.StartNodeConflict = startNode;
					this.EndNodeConflict = endNode;
					this.TypeConflict = type;
					this.PropertiesConflict = properties;
			  }

			  public override NodeValue StartNode()
			  {
					return StartNodeConflict;
			  }

			  public override NodeValue EndNode()
			  {
					return EndNodeConflict;
			  }

			  public override TextValue Type()
			  {
					return TypeConflict;
			  }

			  public override MapValue Properties()
			  {
					return PropertiesConflict;
			  }
		 }
	}

}