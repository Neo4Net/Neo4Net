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
namespace Neo4Net.Graphalgo.impl.shortestpath
{

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;

	/// <summary>
	/// Breadth first search to find all shortest uniform paths from a node to all
	/// others. I.e. assume the cost 1 for all relationships. This can be done by
	/// Dijkstra with the right arguments, but this should be faster.
	/// @complexity This algorithm runs in O(m) time (not including the case when m
	///             is zero).
	/// @author Patrik Larsson
	/// </summary>
	public class SingleSourceShortestPathBFS : SingleSourceShortestPath<int>
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal Node StartNodeConflict;
		 protected internal Direction RelationShipDirection;
		 protected internal RelationshipType[] RelationShipTypes;
		 protected internal Dictionary<Node, int> Distances = new Dictionary<Node, int>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal Dictionary<Node, IList<Relationship>> PredecessorsConflict = new Dictionary<Node, IList<Relationship>>();
		 // Limits
		 protected internal long MaxDepth = long.MaxValue;
		 protected internal long Depth;
		 internal LinkedList<Node> CurrentLayer = new LinkedList<Node>();
		 internal LinkedList<Node> NextLayer = new LinkedList<Node>();

		 public SingleSourceShortestPathBFS( Node startNode, Direction relationShipDirection, params RelationshipType[] relationShipTypes ) : base()
		 {
			  this.StartNodeConflict = startNode;
			  this.RelationShipDirection = relationShipDirection;
			  this.RelationShipTypes = relationShipTypes;
			  Reset();
		 }

		 /// <summary>
		 /// This sets the maximum depth to scan.
		 /// </summary>
		 public virtual void LimitDepth( long maxDepth )
		 {
			  this.MaxDepth = maxDepth;
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public virtual Node StartNode
		 {
			 set
			 {
				  StartNodeConflict = value;
				  Reset();
			 }
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override void Reset()
		 {
			  Distances = new Dictionary<Node, int>();
			  PredecessorsConflict = new Dictionary<Node, IList<Relationship>>();
			  CurrentLayer = new LinkedList<Node>();
			  NextLayer = new LinkedList<Node>();
			  CurrentLayer.AddLast( StartNodeConflict );
			  Depth = 0;
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override int? GetCost( Node targetNode )
		 {
			  Calculate( targetNode );
			  return Distances[targetNode];
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<PropertyContainer> GetPath( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructSinglePathToNode( targetNode, PredecessorsConflict, true, false );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<Node> GetPathAsNodes( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructSinglePathToNodeAsNodes( targetNode, PredecessorsConflict, true, false );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<Relationship> GetPathAsRelationships( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructSinglePathToNodeAsRelationships( targetNode, PredecessorsConflict, false );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<IList<PropertyContainer>> GetPaths( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructAllPathsToNode( targetNode, PredecessorsConflict, true, false );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<IList<Node>> GetPathsAsNodes( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructAllPathsToNodeAsNodes( targetNode, PredecessorsConflict, true, false );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<IList<Relationship>> GetPathsAsRelationships( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return Util.ConstructAllPathsToNodeAsRelationships( targetNode, PredecessorsConflict, false );
		 }

		 /// <summary>
		 /// Iterator-style "next" method. </summary>
		 /// <returns> True if evaluate was made. False if no more computation could be
		 ///         done. </returns>
		 public virtual bool ProcessNextNode()
		 {
			  // finished with current layer? increase depth
			  if ( CurrentLayer.Count == 0 )
			  {
					if ( NextLayer.Count == 0 )
					{
						 return false;
					}
					CurrentLayer = NextLayer;
					NextLayer = new LinkedList<Node>();
					++Depth;
			  }
			  Node node = CurrentLayer.RemoveFirst();
			  // Multiple paths to a certain node might make it appear several
			  // times, just process it once
			  if ( Distances.ContainsKey( node ) )
			  {
					return true;
			  }
			  // Put it in distances
			  Distances[node] = ( int ) Depth;
			  // Follow all edges
			  foreach ( RelationshipType relationshipType in RelationShipTypes )
			  {
					foreach ( Relationship relationship in node.GetRelationships( relationshipType, RelationShipDirection ) )
					{
						 Node targetNode = relationship.GetOtherNode( node );
						 // Are we going back into the already finished area?
						 // That would be more expensive.
						 if ( !Distances.ContainsKey( targetNode ) )
						 {
							  // Put this into the next layer and the predecessors
							  NextLayer.AddLast( targetNode );
							  IList<Relationship> targetPreds = PredecessorsConflict.computeIfAbsent( targetNode, k => new LinkedList<Relationship>() );
							  targetPreds.Add( relationship );
						 }
					}
			  }
			  return true;
		 }

		 /// <summary>
		 /// Internal calculate method that will do the calculation. This can however
		 /// be called externally to manually trigger the calculation.
		 /// </summary>
		 public virtual bool Calculate()
		 {
			  return Calculate( null );
		 }

		 /// <summary>
		 /// Internal calculate method that will run the calculation until either the
		 /// limit is reached or a result has been generated for a given node.
		 /// </summary>
		 public virtual bool Calculate( Node targetNode )
		 {
			  while ( Depth <= MaxDepth && ( targetNode == null || !Distances.ContainsKey( targetNode ) ) )
			  {
					if ( !ProcessNextNode() )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<Node> GetPredecessorNodes( Node node )
		 {
			  IList<Node> result = new LinkedList<Node>();
			  IList<Relationship> predecessorRelationShips = PredecessorsConflict[node];
			  if ( predecessorRelationShips == null || predecessorRelationShips.Count == 0 )
			  {
					return null;
			  }
			  foreach ( Relationship relationship in predecessorRelationShips )
			  {
					result.Add( relationship.GetOtherNode( node ) );
			  }
			  return result;
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public virtual IDictionary<Node, IList<Relationship>> Predecessors
		 {
			 get
			 {
				  Calculate();
				  return PredecessorsConflict;
			 }
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public virtual Direction Direction
		 {
			 get
			 {
				  return RelationShipDirection;
			 }
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public virtual RelationshipType[] RelationshipTypes
		 {
			 get
			 {
				  return RelationShipTypes;
			 }
		 }
	}

}