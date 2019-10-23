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
namespace Neo4Net.GraphDb.impl.traversal
{

	using Paths = Neo4Net.GraphDb.Traversal.Paths;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;

	internal class BidirectionalTraversalBranchPath : IPath
	{
		 private readonly TraversalBranch _start;
		 private readonly TraversalBranch _end;
		 private readonly INode _endNode;
		 private readonly IRelationship _lastRelationship;

		 private INode _cachedStartNode;
		 private LinkedList<IRelationship> _cachedRelationships;

		 internal BidirectionalTraversalBranchPath( TraversalBranch start, TraversalBranch end )
		 {
			  this._start = start;
			  this._end = end;

			  // Most used properties: endNode and lastRelationship, so cache them right away (semi-expensive though).
			  IEnumerator<PropertyContainer> endPathEntities = end.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  this._endNode = ( INode ) endPathEntities.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  this._lastRelationship = endPathEntities.hasNext() ? (IRelationship) endPathEntities.next() : start.LastRelationship();
		 }

		 public override INode StartNode()
		 {
			  // Getting the start node is expensive in some Path implementations, so cache it
			  if ( _cachedStartNode == null )
			  {
					_cachedStartNode = _start.startNode();
			  }
			  return _cachedStartNode;
		 }

		 public override INode EndNode()
		 {
			  return this._endNode;
		 }

		 public override IRelationship LastRelationship()
		 {
			  return this._lastRelationship;
		 }

		 public override IEnumerable<IRelationship> Relationships()
		 {
			  // Cache the relationships since we use them in hashCode/equals too.
			  if ( _cachedRelationships == null )
			  {
					_cachedRelationships = GatherRelationships( _start, _end );
			  }
			  return _cachedRelationships;
		 }

		 public override IEnumerable<IRelationship> ReverseRelationships()
		 {
			  return GatherRelationships( _end, _start );
		 }

		 private LinkedList<IRelationship> GatherRelationships( TraversalBranch first, TraversalBranch then )
		 {
			  // TODO Don't loop through them all up front
			  LinkedList<IRelationship> relationships = new LinkedList<IRelationship>();
			  TraversalBranch branch = first;
			  while ( branch.Length() > 0 )
			  {
					relationships.AddFirst( branch.LastRelationship() );
					branch = branch.Parent();
			  }
			  // We can might as well cache start node since we're right now there anyway
			  if ( _cachedStartNode == null && first == _start && branch.Length() >= 0 )
			  {
					_cachedStartNode = branch.EndNode();
			  }
			  branch = then;
			  while ( branch.Length() > 0 )
			  {
					relationships.AddLast( branch.LastRelationship() );
					branch = branch.Parent();
			  }
			  if ( _cachedStartNode == null && then == _start && branch.Length() >= 0 )
			  {
					_cachedStartNode = branch.EndNode();
			  }
			  return relationships;
		 }

		 public override IEnumerable<INode> Nodes()
		 {
			  return GatherNodes( _start, _end );
		 }

		 public override IEnumerable<INode> ReverseNodes()
		 {
			  return GatherNodes( _end, _start );
		 }

		 private IEnumerable<INode> GatherNodes( TraversalBranch first, TraversalBranch then )
		 {
			  // TODO Don't loop through them all up front
			  LinkedList<INode> nodes = new LinkedList<INode>();
			  TraversalBranch branch = first;
			  while ( branch.Length() > 0 )
			  {
					nodes.AddFirst( branch.EndNode() );
					branch = branch.Parent();
			  }
			  if ( _cachedStartNode == null && first == _start && branch.Length() >= 0 )
			  {
					_cachedStartNode = branch.EndNode();
			  }
			  nodes.AddFirst( branch.EndNode() );
			  branch = then.Parent();
			  if ( branch != null )
			  {
					while ( branch.Length() > 0 )
					{
						 nodes.AddLast( branch.EndNode() );
						 branch = branch.Parent();
					}
					if ( branch.Length() >= 0 )
					{
						 nodes.AddLast( branch.EndNode() );
					}
			  }
			  if ( _cachedStartNode == null && then == _start && branch != null && branch.Length() >= 0 )
			  {
					_cachedStartNode = branch.EndNode();
			  }
			  return nodes;
		 }

		 public override int Length()
		 {
			  return _start.length() + _end.length();
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  // TODO Don't loop through them all up front
			  LinkedList<PropertyContainer> entities = new LinkedList<PropertyContainer>();
			  TraversalBranch branch = _start;
			  while ( branch.Length() > 0 )
			  {
					entities.AddFirst( branch.EndNode() );
					entities.AddFirst( branch.LastRelationship() );
					branch = branch.Parent();
			  }
			  entities.AddFirst( branch.EndNode() );
			  if ( _cachedStartNode == null )
			  {
					_cachedStartNode = branch.EndNode();
			  }
			  if ( _end.length() > 0 )
			  {
					entities.AddLast( _end.lastRelationship() );
					branch = _end.parent();
					while ( branch.Length() > 0 )
					{
						 entities.AddLast( branch.EndNode() );
						 entities.AddLast( branch.LastRelationship() );
						 branch = branch.Parent();
					}
					entities.AddLast( branch.EndNode() );
			  }
			  return entities.GetEnumerator();
		 }

		 public override int GetHashCode()
		 {
			  return Relationships().GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj == this )
			  {
					return true;
			  }
			  if ( !( obj is IPath ) )
			  {
					return false;
			  }

			  IPath other = ( IPath ) obj;
			  return Relationships().Equals(other.Relationships()) && other.StartNode().Equals(_cachedStartNode);
		 }

		 public override string ToString()
		 {
			  return Paths.defaultPathToString( this );
		 }
	}

}