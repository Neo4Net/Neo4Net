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
namespace Neo4Net.GraphAlgo.ShortestPath
{

	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;

	/// <summary>
	/// This is a holder for some utility functions regarding paths, such as
	/// constructing them from sets of predecessors or counting them. These functions
	/// are lifted out here because they can be used by algorithms for too different
	/// problems.
	/// @author Patrik Larsson
	/// </summary>
	public class Util
	{
		 private Util()
		 {
		 }

		 /// <summary>
		 /// Constructs a path to a given node, for a given set of predecessors </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="includeNode">
		 ///            Boolean which determines if the start node should be included
		 ///            in the paths </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed </param>
		 /// <returns> A path as a list of nodes. </returns>
		 public static IList<Node> ConstructSinglePathToNodeAsNodes( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  IList<PropertyContainer> singlePathToNode = ConstructSinglePathToNode( node, predecessors, includeNode, backwards );
			  IEnumerator<PropertyContainer> iterator = singlePathToNode.GetEnumerator();
			  // When going backwards and not including the node the first element is
			  // a relationship. Thus skip it.
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( backwards && !includeNode && iterator.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					iterator.next();
			  }
			  LinkedList<Node> path = new LinkedList<Node>();
			  while ( iterator.MoveNext() )
			  {
					path.AddLast( ( Node ) iterator.Current );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
						 iterator.Current;
					}
			  }
			  return path;
		 }

		 /// <summary>
		 /// Constructs a path to a given node, for a given set of predecessors </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed </param>
		 /// <returns> A path as a list of relationships. </returns>
		 public static IList<Relationship> ConstructSinglePathToNodeAsRelationships( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool backwards )
		 {
			  IList<PropertyContainer> singlePathToNode = ConstructSinglePathToNode( node, predecessors, true, backwards );
			  IEnumerator<PropertyContainer> iterator = singlePathToNode.GetEnumerator();
			  // Skip the first, it is a node
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( iterator.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					iterator.next();
			  }
			  LinkedList<Relationship> path = new LinkedList<Relationship>();
			  while ( iterator.MoveNext() )
			  {
					path.AddLast( ( Relationship ) iterator.Current );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
						 iterator.Current;
					}
			  }
			  return path;
		 }

		 /// <summary>
		 /// Constructs a path to a given node, for a given set of predecessors. The
		 /// result is a list of alternating Node/Relationship. </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="includeNode">
		 ///            Boolean which determines if the start node should be included
		 ///            in the paths </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed </param>
		 /// <returns> A path as a list of alternating Node/Relationship. </returns>
		 public static IList<PropertyContainer> ConstructSinglePathToNode( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  LinkedList<PropertyContainer> path = new LinkedList<PropertyContainer>();
			  if ( includeNode )
			  {
					if ( backwards )
					{
						 path.AddLast( node );
					}
					else
					{
						 path.AddFirst( node );
					}
			  }
			  Node currentNode = node;
			  IList<Relationship> currentPreds = predecessors[currentNode];
			  // Traverse predecessors until we have added a node without predecessors
			  while ( currentPreds != null && currentPreds.Count != 0 )
			  {
					// Get next node
					Relationship currentRelationship = currentPreds[0];
					currentNode = currentRelationship.GetOtherNode( currentNode );
					// Add current
					if ( backwards )
					{
						 path.AddLast( currentRelationship );
						 path.AddLast( currentNode );
					}
					else
					{
						 path.AddFirst( currentRelationship );
						 path.AddFirst( currentNode );
					}
					// Continue with the next node
					currentPreds = predecessors[currentNode];
			  }
			  return path;
		 }

		 /// <summary>
		 /// Constructs all paths to a given node, for a given set of predecessors </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="includeNode">
		 ///            Boolean which determines if the start node should be included
		 ///            in the paths </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed
		 /// @return </param>
		 public static IList<IList<Node>> ConstructAllPathsToNodeAsNodes( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  return new LinkedList<IList<Node>>( ConstructAllPathsToNodeAsNodeLinkedLists( node, predecessors, includeNode, backwards ) );
		 }

		 /// <summary>
		 /// Same as constructAllPathsToNodeAsNodes, but different return type
		 /// </summary>
		 protected internal static IList<LinkedList<Node>> ConstructAllPathsToNodeAsNodeLinkedLists( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  IList<LinkedList<Node>> paths = new LinkedList<LinkedList<Node>>();
			  IList<Relationship> current = predecessors[node];
			  // First build all paths to this node's predecessors
			  if ( current != null )
			  {
					foreach ( Relationship r in current )
					{
						 Node n = r.GetOtherNode( node );
						 ( ( IList<LinkedList<Node>> )paths ).AddRange( ConstructAllPathsToNodeAsNodeLinkedLists( n, predecessors, true, backwards ) );
					}
			  }
			  // If no paths exists to this node, just create an empty one (which will
			  // have this node added to it)
			  if ( paths.Count == 0 )
			  {
					paths.Add( new LinkedList<>() );
			  }
			  // Then add this node to all those paths
			  if ( includeNode )
			  {
					foreach ( LinkedList<Node> path in paths )
					{
						 if ( backwards )
						 {
							  path.AddFirst( node );
						 }
						 else
						 {
							  path.AddLast( node );
						 }
					}
			  }
			  return paths;
		 }

		 /// <summary>
		 /// Constructs all paths to a given node, for a given set of predecessors </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="includeNode">
		 ///            Boolean which determines if the start node should be included
		 ///            in the paths </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed </param>
		 /// <returns> List of lists of alternating Node/Relationship. </returns>
		 public static IList<IList<PropertyContainer>> ConstructAllPathsToNode( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  return new LinkedList<IList<PropertyContainer>>( ConstructAllPathsToNodeAsLinkedLists( node, predecessors, includeNode, backwards ) );
		 }

		 /// <summary>
		 /// Same as constructAllPathsToNode, but different return type
		 /// </summary>
		 protected internal static IList<LinkedList<PropertyContainer>> ConstructAllPathsToNodeAsLinkedLists( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool includeNode, bool backwards )
		 {
			  IList<LinkedList<PropertyContainer>> paths = new LinkedList<LinkedList<PropertyContainer>>();
			  IList<Relationship> current = predecessors[node];
			  // First build all paths to this node's predecessors
			  if ( current != null )
			  {
					foreach ( Relationship r in current )
					{
						 Node n = r.GetOtherNode( node );
						 IList<LinkedList<PropertyContainer>> newPaths = ConstructAllPathsToNodeAsLinkedLists( n, predecessors, true, backwards );
						 ( ( IList<LinkedList<PropertyContainer>> )paths ).AddRange( newPaths );
						 // Add the relationship
						 foreach ( LinkedList<PropertyContainer> path in newPaths )
						 {
							  if ( backwards )
							  {
									path.AddFirst( r );
							  }
							  else
							  {
									path.AddLast( r );
							  }
						 }
					}
			  }
			  // If no paths exists to this node, just create an empty one (which will
			  // have this node added to it)
			  if ( paths.Count == 0 )
			  {
					paths.Add( new LinkedList<>() );
			  }
			  // Then add this node to all those paths
			  if ( includeNode )
			  {
					foreach ( LinkedList<PropertyContainer> path in paths )
					{
						 if ( backwards )
						 {
							  path.AddFirst( node );
						 }
						 else
						 {
							  path.AddLast( node );
						 }
					}
			  }
			  return paths;
		 }

		 /// <summary>
		 /// Constructs all paths to a given node, for a given set of predecessors. </summary>
		 /// <param name="node">
		 ///            The start node </param>
		 /// <param name="predecessors">
		 ///            The predecessors set </param>
		 /// <param name="backwards">
		 ///            Boolean, if true the order of the nodes in the paths will be
		 ///            reversed </param>
		 /// <returns> List of lists of relationships. </returns>
		 public static IList<IList<Relationship>> ConstructAllPathsToNodeAsRelationships( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool backwards )
		 {
			  return new LinkedList<IList<Relationship>>( ConstructAllPathsToNodeAsRelationshipLinkedLists( node, predecessors, backwards ) );
		 }

		 /// <summary>
		 /// Same as constructAllPathsToNodeAsRelationships, but different return type
		 /// </summary>
		 protected internal static IList<LinkedList<Relationship>> ConstructAllPathsToNodeAsRelationshipLinkedLists( Node node, IDictionary<Node, IList<Relationship>> predecessors, bool backwards )
		 {
			  IList<LinkedList<Relationship>> paths = new LinkedList<LinkedList<Relationship>>();
			  IList<Relationship> current = predecessors[node];
			  // First build all paths to this node's predecessors
			  if ( current != null )
			  {
					foreach ( Relationship r in current )
					{
						 Node n = r.GetOtherNode( node );
						 IList<LinkedList<Relationship>> newPaths = ConstructAllPathsToNodeAsRelationshipLinkedLists( n, predecessors, backwards );
						 ( ( IList<LinkedList<Relationship>> )paths ).AddRange( newPaths );
						 // Add the relationship
						 foreach ( LinkedList<Relationship> path in newPaths )
						 {
							  if ( backwards )
							  {
									path.AddFirst( r );
							  }
							  else
							  {
									path.AddLast( r );
							  }
						 }
					}
			  }
			  // If no paths exists to this node, just create an empty one
			  if ( paths.Count == 0 )
			  {
					paths.Add( new LinkedList<>() );
			  }
			  return paths;
		 }

		 /// <summary>
		 /// This can be used for counting the number of paths from the start node
		 /// (implicit from the predecessors) and some target nodes.
		 /// </summary>
		 public class PathCounter
		 {
			  internal IDictionary<Node, IList<Relationship>> Predecessors;
			  internal IDictionary<Node, int> PathCounts = new Dictionary<Node, int>();

			  public PathCounter( IDictionary<Node, IList<Relationship>> predecessors ) : base()
			  {
					this.Predecessors = predecessors;
			  }

			  public virtual int GetNumberOfPathsToNode( Node node )
			  {
					int? i = PathCounts[node];
					if ( i != null )
					{
						 return i.Value;
					}
					IList<Relationship> preds = Predecessors[node];
					if ( preds == null || preds.Count == 0 )
					{
						 return 1;
					}
					int result = 0;
					foreach ( Relationship relationship in preds )
					{
						 result += GetNumberOfPathsToNode( relationship.GetOtherNode( node ) );
					}
					PathCounts[node] = result;
					return result;
			  }
		 }

		 /// <summary>
		 /// This can be used to generate the inverse of a structure with
		 /// predecessors, i.e. the successors. </summary>
		 /// <param name="predecessors">
		 /// @return </param>
		 public static IDictionary<Node, IList<Relationship>> ReversedPredecessors( IDictionary<Node, IList<Relationship>> predecessors )
		 {
			  IDictionary<Node, IList<Relationship>> result = new Dictionary<Node, IList<Relationship>>();
			  ISet<Node> keys = predecessors.Keys;
			  foreach ( Node node in keys )
			  {
					IList<Relationship> preds = predecessors[node];
					foreach ( Relationship relationship in preds )
					{
						 Node otherNode = relationship.GetOtherNode( node );
						 // We add node as a predecessor to otherNode, instead of the
						 // other way around
						 IList<Relationship> otherPreds = result.computeIfAbsent( otherNode, k => new LinkedList<Relationship>() );
						 otherPreds.Add( relationship );
					}
			  }
			  return result;
		 }
	}

}