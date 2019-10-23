using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.GraphDb.impl
{

	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.Traversal;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;

	public sealed class OrderedByTypeExpander : StandardExpander.RegularExpander
	{
		 private readonly ICollection<Pair<IRelationshipType, Direction>> _orderedTypes;

		 public OrderedByTypeExpander() : this(Collections.emptyList())
		 {
		 }

		 private OrderedByTypeExpander( ICollection<Pair<IRelationshipType, Direction>> orderedTypes ) : base( Collections.emptyMap() )
		 {
			  _orderedTypes = orderedTypes;
		 }

		 public override StandardExpander Add( IRelationshipType type, Direction direction )
		 {
			  ICollection<Pair<IRelationshipType, Direction>> newTypes = new List<Pair<IRelationshipType, Direction>>( _orderedTypes );
			  newTypes.Add( Pair.of( type, direction ) );
			  return new OrderedByTypeExpander( newTypes );
		 }

		 public override StandardExpander Remove( IRelationshipType type )
		 {
			  ICollection<Pair<IRelationshipType, Direction>> newTypes = new List<Pair<IRelationshipType, Direction>>();
			  foreach ( Pair<IRelationshipType, Direction> pair in _orderedTypes )
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
			  ICollection<Pair<IRelationshipType, Direction>> newTypes = new List<Pair<IRelationshipType, Direction>>();
			  foreach ( Pair<IRelationshipType, Direction> pair in _orderedTypes )
			  {
					newTypes.Add( Pair.of( pair.First(), pair.Other().reverse() ) );
			  }
			  return new OrderedByTypeExpander( newTypes );
		 }

		 internal override RegularExpander CreateNew( IDictionary<Direction, IRelationshipType[]> newTypes )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: ResourceIterator<org.Neo4Net.graphdb.Relationship> doExpand(final org.Neo4Net.graphdb.Path path, org.Neo4Net.graphdb.traversal.BranchState state)
		 internal override ResourceIterator<IRelationship> DoExpand( IPath path, IBranchState state )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node node = path.endNode();
			  INode node = path.EndNode;
			  return new NestingResourceIteratorAnonymousInnerClass( this, _orderedTypes.GetEnumerator(), node );
		 }

		 private class NestingResourceIteratorAnonymousInnerClass : NestingResourceIterator<IRelationship, Pair<IRelationshipType, Direction>>
		 {
			 private readonly OrderedByTypeExpander _outerInstance;

			 private INode _node;

			 public NestingResourceIteratorAnonymousInnerClass( OrderedByTypeExpander outerInstance, UnknownType iterator, INode node ) : base( iterator )
			 {
				 this.outerInstance = outerInstance;
				 _node = node;
			 }

			 protected internal override ResourceIterator<IRelationship> createNestedIterator( Pair<IRelationshipType, Direction> entry )
			 {
				  IRelationshipType type = entry.First();
				  Direction dir = entry.Other();
				  IEnumerable<IRelationship> relationshipsIterable = ( dir == Direction.BOTH ) ? _node.getRelationships( type ) : _node.getRelationships( type, dir );
				  return Iterables.asResourceIterable( relationshipsIterable ).GetEnumerator();
			 }
		 }
	}

}