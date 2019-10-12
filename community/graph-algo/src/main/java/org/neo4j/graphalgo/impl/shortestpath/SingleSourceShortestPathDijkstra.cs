using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Graphalgo.impl.shortestpath
{

	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	/// <summary>
	/// Dijkstra implementation to solve the single source shortest path problem for
	/// weighted networks.
	/// @complexity The <seealso cref="CostEvaluator"/>, the <seealso cref="CostAccumulator"/> and the
	///             cost comparator will all be called once for every relationship
	///             traversed. Assuming they run in constant time, the time
	///             complexity for this algorithm is O(m + n * log(n)).
	/// @author Patrik Larsson </summary>
	/// @param <CostType>
	///            The datatype the edge weights are represented by. </param>
	public class SingleSourceShortestPathDijkstra<CostType> : Dijkstra<CostType>, SingleSourceShortestPath<CostType>
	{
		 internal DijkstraIterator DijkstraIterator;

		 /// <seealso cref= Dijkstra </seealso>
		 public SingleSourceShortestPathDijkstra( CostType startCost, Node startNode, CostEvaluator<CostType> costEvaluator, CostAccumulator<CostType> costAccumulator, IComparer<CostType> costComparator, Direction relationDirection, params RelationshipType[] costRelationTypes ) : base( startCost, startNode, null, costEvaluator, costAccumulator, costComparator, relationDirection, costRelationTypes )
		 {
			  Reset();
		 }

		 protected internal Dictionary<Node, CostType> Distances = new Dictionary<Node, CostType>();

		 public override void Reset()
		 {
			  base.Reset();
			  Distances = new Dictionary<Node, CostType>();
			  Dictionary<Node, CostType> seen1 = new Dictionary<Node, CostType>();
			  Dictionary<Node, CostType> seen2 = new Dictionary<Node, CostType>();
			  Dictionary<Node, CostType> dists2 = new Dictionary<Node, CostType>();
			  DijkstraIterator = new DijkstraIterator( this, StartNodeConflict, Predecessors1, seen1, seen2, Distances, dists2, false );
		 }

		 /// <summary>
		 /// Same as calculate(), but will set the flag to calculate all shortest
		 /// paths. It sets the flag and then calls calculate.
		 /// @return
		 /// </summary>
		 public virtual bool CalculateMultiple( Node targetNode )
		 {
			  if ( !CalculateAllShortestPaths )
			  {
					Reset();
					CalculateAllShortestPaths = true;
			  }
			  return Calculate( targetNode );
		 }

		 public override bool Calculate()
		 {
			  return Calculate( null );
		 }

		 /// <summary>
		 /// Internal calculate method that will run the calculation until either the
		 /// limit is reached or a result has been generated for a given node.
		 /// </summary>
		 public virtual bool Calculate( Node targetNode )
		 {
			  while ( ( targetNode == null || !Distances.ContainsKey( targetNode ) ) && DijkstraIterator.MoveNext() && !LimitReached() )
			  {
					DijkstraIterator.Current;
			  }
			  return true;
		 }

		 // We dont need to reset the calculation, so we just override this.
		 public override Node EndNode
		 {
			 set
			 {
				  this.EndNodeConflict = value;
			 }
		 }

		 /// <seealso cref= Dijkstra </seealso>
		 public override CostType GetCost( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  Calculate( targetNode );
			  return Distances[targetNode];
		 }

		 public override IList<IList<PropertyContainer>> GetPaths( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  CalculateMultiple( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return new LinkedList<IList<PropertyContainer>>( Util.ConstructAllPathsToNode( targetNode, Predecessors1, true, false ) );
		 }

		 public override IList<IList<Node>> GetPathsAsNodes( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  CalculateMultiple( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return new LinkedList<IList<Node>>( Util.ConstructAllPathsToNodeAsNodes( targetNode, Predecessors1, true, false ) );
		 }

		 public override IList<IList<Relationship>> GetPathsAsRelationships( Node targetNode )
		 {
			  if ( targetNode == null )
			  {
					throw new Exception( "No end node defined" );
			  }
			  CalculateMultiple( targetNode );
			  if ( !Distances.ContainsKey( targetNode ) )
			  {
					return null;
			  }
			  return new LinkedList<IList<Relationship>>( Util.ConstructAllPathsToNodeAsRelationships( targetNode, Predecessors1, false ) );
		 }

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
			  return Util.ConstructSinglePathToNode( targetNode, Predecessors1, true, false );
		 }

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
			  return Util.ConstructSinglePathToNodeAsNodes( targetNode, Predecessors1, true, false );
		 }

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
			  return Util.ConstructSinglePathToNodeAsRelationships( targetNode, Predecessors1, false );
		 }

		 // Override all the result-getters
		 public override CostType GetCost()
		 {
			  return GetCost( EndNodeConflict );
		 }

		 public override IList<PropertyContainer> GetPath()
		 {
			  return GetPath( EndNodeConflict );
		 }

		 public override IList<Node> GetPathAsNodes()
		 {
			  return GetPathAsNodes( EndNodeConflict );
		 }

		 public override IList<Relationship> GetPathAsRelationships()
		 {
			  return GetPathAsRelationships( EndNodeConflict );
		 }

		 public override IList<IList<PropertyContainer>> GetPaths()
		 {
			  return GetPaths( EndNodeConflict );
		 }

		 public override IList<IList<Node>> GetPathsAsNodes()
		 {
			  return GetPathsAsNodes( EndNodeConflict );
		 }

		 public override IList<IList<Relationship>> GetPathsAsRelationships()
		 {
			  return GetPathsAsRelationships( EndNodeConflict );
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override IList<Node> GetPredecessorNodes( Node node )
		 {
			  IList<Node> result = new LinkedList<Node>();
			  IList<Relationship> predecessorRelationShips = Predecessors1[node];
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
				  CalculateMultiple();
				  return Predecessors1;
			 }
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override Direction Direction
		 {
			 get
			 {
				  return RelationDirection;
			 }
		 }

		 /// <seealso cref= SingleSourceShortestPath </seealso>
		 public override RelationshipType[] RelationshipTypes
		 {
			 get
			 {
				  return CostRelationTypes;
			 }
		 }
	}

}