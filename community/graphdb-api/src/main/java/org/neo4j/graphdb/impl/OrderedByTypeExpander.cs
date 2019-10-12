using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Graphdb.impl
{

	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb.traversal;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;

	public sealed class OrderedByTypeExpander : StandardExpander.RegularExpander
	{
		 private readonly ICollection<Pair<RelationshipType, Direction>> _orderedTypes;

		 public OrderedByTypeExpander() : this(Collections.emptyList())
		 {
		 }

		 private OrderedByTypeExpander( ICollection<Pair<RelationshipType, Direction>> orderedTypes ) : base( Collections.emptyMap() )
		 {
			  this._orderedTypes = orderedTypes;
		 }

		 public override StandardExpander Add( RelationshipType type, Direction direction )
		 {
			  ICollection<Pair<RelationshipType, Direction>> newTypes = new List<Pair<RelationshipType, Direction>>( _orderedTypes );
			  newTypes.Add( Pair.of( type, direction ) );
			  return new OrderedByTypeExpander( newTypes );
		 }

		 public override StandardExpander Remove( RelationshipType type )
		 {
			  ICollection<Pair<RelationshipType, Direction>> newTypes = new List<Pair<RelationshipType, Direction>>();
			  foreach ( Pair<RelationshipType, Direction> pair in _orderedTypes )
			  {
					if ( !type.Name().Equals(pair.First().name()) )
					{
						 newTypes.Add( pair );
					}
			  }
			  return new OrderedByTypeExpander( newTypes );
		 }

		 internal override void BuildString( StringBuilder result )
		 {
			  result.Append( _orderedTypes.ToString() );
		 }

		 public override StandardExpander Reverse()
		 {
			  ICollection<Pair<RelationshipType, Direction>> newTypes = new List<Pair<RelationshipType, Direction>>();
			  foreach ( Pair<RelationshipType, Direction> pair in _orderedTypes )
			  {
					newTypes.Add( Pair.of( pair.First(), pair.Other().reverse() ) );
			  }
			  return new OrderedByTypeExpander( newTypes );
		 }

		 internal override RegularExpander CreateNew( IDictionary<Direction, RelationshipType[]> newTypes )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: ResourceIterator<org.neo4j.graphdb.Relationship> doExpand(final org.neo4j.graphdb.Path path, org.neo4j.graphdb.traversal.BranchState state)
		 internal override ResourceIterator<Relationship> DoExpand( Path path, BranchState state )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node = path.endNode();
			  Node node = path.EndNode();
			  return new NestingResourceIteratorAnonymousInnerClass( this, _orderedTypes.GetEnumerator(), node );
		 }

		 private class NestingResourceIteratorAnonymousInnerClass : NestingResourceIterator<Relationship, Pair<RelationshipType, Direction>>
		 {
			 private readonly OrderedByTypeExpander _outerInstance;

			 private Node _node;

			 public NestingResourceIteratorAnonymousInnerClass( OrderedByTypeExpander outerInstance, UnknownType iterator, Node node ) : base( iterator )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
			 }

			 protected internal override ResourceIterator<Relationship> createNestedIterator( Pair<RelationshipType, Direction> entry )
			 {
				  RelationshipType type = entry.First();
				  Direction dir = entry.Other();
				  IEnumerable<Relationship> relationshipsIterable = ( dir == Direction.BOTH ) ? _node.getRelationships( type ) : _node.getRelationships( type, dir );
				  return Iterables.asResourceIterable( relationshipsIterable ).GetEnumerator();
			 }
		 }
	}

}