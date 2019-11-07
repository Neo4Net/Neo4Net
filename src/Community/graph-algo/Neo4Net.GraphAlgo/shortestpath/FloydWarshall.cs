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

	using Neo4Net.GraphAlgo;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;

	/// <summary>
	/// This provides an implementation of the Floyd Warshall algorithm solving the
	/// all pair shortest path problem.
	/// @complexity The <seealso cref="CostEvaluator"/> is called once for every relationship.
	///             The <seealso cref="CostAccumulator"/> and cost comparator are both called
	///             n^3 times. Assuming they run in constant time, the time
	///             complexity for this algorithm is O(n^3).
	/// @author Patrik Larsson </summary>
	/// @param <CostType>
	///            The datatype the edge weights are represented by. </param>
	public class FloydWarshall<CostType>
	{
		 protected internal CostType StartCost; // starting cost for all nodes
		 protected internal CostType InfinitelyBad; // starting value for calculation
		 protected internal Direction RelationDirection;
		 protected internal CostEvaluator<CostType> CostEvaluator;
		 protected internal CostAccumulator<CostType> CostAccumulator;
		 protected internal IComparer<CostType> CostComparator;
		 protected internal ISet<Node> NodeSet;
		 protected internal ISet<Relationship> RelationshipSet;
		 internal CostType[][] CostMatrix;
		 internal int?[][] Predecessors;
		 internal IDictionary<Node, int> NodeIndexes; // node ->index
		 internal Node[] IndexedNodes; // index -> node
		 protected internal bool DoneCalculation;

		 /// <param name="startCost">
		 ///            The cost for just starting (or ending) a path in a node. </param>
		 /// <param name="infinitelyBad">
		 ///            A cost worse than all others. This is used to initialize the
		 ///            distance matrix. </param>
		 /// <param name="relationDirection">
		 ///            The direction in which the paths should follow the
		 ///            relationships. </param>
		 /// <param name="costEvaluator"> </param>
		 /// <seealso cref= <seealso cref="CostEvaluator"/> </seealso>
		 /// <param name="costAccumulator"> </param>
		 /// <seealso cref= <seealso cref="CostAccumulator"/> </seealso>
		 /// <param name="costComparator"> </param>
		 /// <seealso cref= <seealso cref="CostAccumulator"/> or <seealso cref="CostEvaluator"/> </seealso>
		 /// <param name="nodeSet">
		 ///            The set of nodes the calculation should be run on. </param>
		 /// <param name="relationshipSet">
		 ///            The set of relationships that should be processed. </param>
		 public FloydWarshall( CostType startCost, CostType infinitelyBad, Direction relationDirection, CostEvaluator<CostType> costEvaluator, CostAccumulator<CostType> costAccumulator, IComparer<CostType> costComparator, ISet<Node> nodeSet, ISet<Relationship> relationshipSet ) : base()
		 {
			  this.StartCost = startCost;
			  this.InfinitelyBad = infinitelyBad;
			  this.RelationDirection = relationDirection;
			  this.CostEvaluator = costEvaluator;
			  this.CostAccumulator = costAccumulator;
			  this.CostComparator = costComparator;
			  this.NodeSet = nodeSet;
			  this.RelationshipSet = relationshipSet;
		 }

		 /// <summary>
		 /// This resets the calculation if we for some reason would like to redo it.
		 /// </summary>
		 public virtual void Reset()
		 {
			  DoneCalculation = false;
		 }

		 /// <summary>
		 /// Internal calculate method that will do the calculation. This can however
		 /// be called externally to manually trigger the calculation.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void calculate()
		 public virtual void Calculate()
		 {
			  // Don't do it more than once
			  if ( DoneCalculation )
			  {
					return;
			  }
			  DoneCalculation = true;
			  // Build initial matrix
			  int n = NodeSet.Count;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: CostMatrix = (CostType[][]) new object[n][n];
			  CostMatrix = ( CostType[][] ) RectangularArrays.RectangularObjectArray( n, n );
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Predecessors = new System.Nullable<int>[n][n];
			  Predecessors = RectangularArrays.RectangularSystemNullableArray<int>( n, n );
			  IndexedNodes = new Node[n];
			  NodeIndexes = new Dictionary<Node, int>();
			  for ( int i = 0; i < n; ++i )
			  {
					for ( int j = 0; j < n; ++j )
					{
						 CostMatrix[i][j] = InfinitelyBad;
					}
					CostMatrix[i][i] = StartCost;
			  }
			  int nodeIndex = 0;
			  foreach ( Node node in NodeSet )
			  {
					NodeIndexes[node] = nodeIndex;
					IndexedNodes[nodeIndex] = node;
					++nodeIndex;
			  }
			  // Put the relationships in there
			  foreach ( Relationship relationship in RelationshipSet )
			  {
					int? i1 = NodeIndexes[relationship.StartNode];
					int? i2 = NodeIndexes[relationship.EndNode];
					if ( i1 == null || i2 == null )
					{
						 // TODO: what to do here? pretend nothing happened? cast
						 // exception?
						 continue;
					}
					if ( RelationDirection.Equals( Direction.BOTH ) || RelationDirection.Equals( Direction.OUTGOING ) )
					{
						 CostMatrix[i1][i2] = CostEvaluator.getCost( relationship, Direction.OUTGOING );
						 Predecessors[i1][i2] = i1;
					}
					if ( RelationDirection.Equals( Direction.BOTH ) || RelationDirection.Equals( Direction.INCOMING ) )
					{
						 CostMatrix[i2][i1] = CostEvaluator.getCost( relationship, Direction.INCOMING );
						 Predecessors[i2][i1] = i2;
					}
			  }
			  // Do it!
			  for ( int v = 0; v < n; ++v )
			  {
					for ( int i = 0; i < n; ++i )
					{
						 for ( int j = 0; j < n; ++j )
						 {
							  CostType alternative = CostAccumulator.addCosts( CostMatrix[i][v], CostMatrix[v][j] );
							  if ( CostComparator.Compare( CostMatrix[i][j], alternative ) > 0 )
							  {
									CostMatrix[i][j] = alternative;
									Predecessors[i][j] = Predecessors[v][j];
							  }
						 }
					}
			  }
			  // TODO: detect negative cycles?
		 }

		 /// <summary>
		 /// This returns the cost for the shortest path between two nodes. </summary>
		 /// <param name="node1">
		 ///            The start node. </param>
		 /// <param name="node2">
		 ///            The end node. </param>
		 /// <returns> The cost for the shortest path. </returns>
		 public virtual CostType GetCost( Node node1, Node node2 )
		 {
			  Calculate();
			  return CostMatrix[NodeIndexes[node1]][NodeIndexes[node2]];
		 }

		 /// <summary>
		 /// This returns the shortest path between two nodes as list of nodes. </summary>
		 /// <param name="startNode">
		 ///            The start node. </param>
		 /// <param name="targetNode">
		 ///            The end node. </param>
		 /// <returns> The shortest path as a list of nodes. </returns>
		 public virtual IList<Node> GetPath( INode startNode, INode targetNode )
		 {
			  Calculate();
			  LinkedList<INode> path = new LinkedList<INode>();
			  int index = NodeIndexes[targetNode];
			  int startIndex = NodeIndexes[startNode];
			  Node n = targetNode;
			  while ( !n.Equals( startNode ) )
			  {
					path.AddFirst( n );
					index = Predecessors[startIndex][index].Value;
					n = IndexedNodes[index];
			  }
			  path.AddFirst( n );
			  return path;
		 }
	}

}