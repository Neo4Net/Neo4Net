using System;

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
namespace Neo4Net.GraphAlgo
{
	using AStar = Neo4Net.GraphAlgo.Path.AStar;
	using AllPaths = Neo4Net.GraphAlgo.Path.AllPaths;
	using AllSimplePaths = Neo4Net.GraphAlgo.Path.AllSimplePaths;
	using Dijkstra = Neo4Net.GraphAlgo.Path.Dijkstra;
	using DijkstraBidirectional = Neo4Net.GraphAlgo.Path.DijkstraBidirectional;
	using ExactDepthPathFinder = Neo4Net.GraphAlgo.Path.ExactDepthPathFinder;
	using ShortestPath = Neo4Net.GraphAlgo.Path.ShortestPath;
	using DoubleEvaluator = Neo4Net.GraphAlgo.Utils.DoubleEvaluator;
	using PathInterestFactory = Neo4Net.GraphAlgo.Utils.PathInterestFactory;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.Traversal;
	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

	/// <summary>
	/// Static factory methods for the recommended implementations of common
	/// graph algorithms for Neo4Net. The algorithms exposed here are implementations
	/// which are tested extensively and also scale on bigger graphs.
	/// </summary>
	public abstract class GraphAlgoFactory
	{
		 /// <summary>
		 /// Returns an algorithm which can find all available paths between two
		 /// nodes. These returned paths can contain loops (i.e. a node can occur
		 /// more than once in any returned path).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="maxDepth"> the max <seealso cref="Path.length()"/> returned paths are
		 /// allowed to have. </param>
		 /// <returns> an algorithm which finds all paths between two nodes. </returns>
		 public static PathFinder<Path> AllPaths( PathExpander expander, int maxDepth )
		 {
			  return new AllPaths( maxDepth, expander );
		 }

		 /// <summary>
		 /// Returns an algorithm which can find all simple paths between two
		 /// nodes. These returned paths cannot contain loops (i.e. a node cannot
		 /// occur more than once in any returned path).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="maxDepth"> the max <seealso cref="Path.length()"/> returned paths are
		 /// allowed to have. </param>
		 /// <returns> an algorithm which finds simple paths between two nodes. </returns>
		 public static PathFinder<Path> AllSimplePaths( PathExpander expander, int maxDepth )
		 {
			  return new AllSimplePaths( maxDepth, expander );
		 }

		 /// <summary>
		 /// Returns an algorithm which can find all shortest paths (that is paths
		 /// with as short <seealso cref="Path.length()"/> as possible) between two nodes. These
		 /// returned paths cannot contain loops (i.e. a node cannot occur more than
		 /// once in any returned path).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 ///            <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="maxDepth"> the max <seealso cref="Path.length()"/> returned paths are allowed
		 ///            to have. </param>
		 /// <returns> an algorithm which finds shortest paths between two nodes. </returns>
		 public static PathFinder<Path> ShortestPath( PathExpander expander, int maxDepth )
		 {
			  return new ShortestPath( maxDepth, expander );
		 }

		 /// <summary>
		 /// Returns an algorithm which can find all shortest paths (that is paths
		 /// with as short <seealso cref="Path.length()"/> as possible) between two nodes. These
		 /// returned paths cannot contain loops (i.e. a node cannot occur more than
		 /// once in any returned path).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 ///            <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="maxDepth"> the max <seealso cref="Path.length()"/> returned paths are allowed
		 ///            to have. </param>
		 /// <param name="maxHitCount"> the maximum number of <seealso cref="Path"/>s to return.
		 /// If this number of found paths are encountered the traversal will stop. </param>
		 /// <returns> an algorithm which finds shortest paths between two nodes. </returns>
		 public static PathFinder<Path> ShortestPath( PathExpander expander, int maxDepth, int maxHitCount )
		 {
			  return new ShortestPath( maxDepth, expander, maxHitCount );
		 }

		 /// <summary>
		 /// Returns an algorithm which can find simple all paths of a certain length
		 /// between two nodes. These returned paths cannot contain loops (i.e. a node
		 /// could not occur more than once in any returned path).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Node"/>. </param>
		 /// <param name="length"> the <seealso cref="Path.length()"/> returned paths will have, if any
		 /// paths were found. </param>
		 /// <returns> an algorithm which finds paths of a certain length between two nodes. </returns>
		 public static PathFinder<Path> PathsWithLength( PathExpander expander, int length )
		 {
			  return new ExactDepthPathFinder( expander, length, int.MaxValue, false );
		 }

		 /// <summary>
		 /// Returns an <seealso cref="PathFinder"/> which uses the A* algorithm to find the
		 /// cheapest path between two nodes. The definition of "cheap" is the lowest
		 /// possible cost to get from the start node to the end node, where the cost
		 /// is returned from {@code lengthEvaluator} and {@code estimateEvaluator}.
		 /// These returned paths cannot contain loops (i.e. a node cannot occur more
		 /// than once in any returned path).
		 /// 
		 /// See http://en.wikipedia.org/wiki/A*_search_algorithm for more
		 /// information.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="lengthEvaluator"> evaluator that can return the cost represented
		 /// by each relationship the algorithm traverses. </param>
		 /// <param name="estimateEvaluator"> evaluator that returns an (optimistic)
		 /// estimation of the cost to get from the current node (in the traversal)
		 /// to the end node. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the A* algorithm. </returns>
		 public static PathFinder<WeightedPath> AStar( PathExpander expander, CostEvaluator<double> lengthEvaluator, EstimateEvaluator<double> estimateEvaluator )
		 {
			  return new AStar( expander, lengthEvaluator, estimateEvaluator );
		 }

		 /// <summary>
		 /// Returns a <seealso cref="PathFinder"/> which uses the Dijkstra algorithm to find
		 /// the cheapest path between two nodes. The definition of "cheap" is the
		 /// lowest possible cost to get from the start node to the end node, where
		 /// the cost is returned from {@code costEvaluator}. These returned paths
		 /// cannot contain loops (i.e. a node cannot occur more than once in any
		 /// returned path).
		 /// 
		 /// Dijkstra assumes none negative costs on all considered relationships.
		 /// If this is not the case behaviour is undefined. Do not use Dijkstra
		 /// with negative weights or use a <seealso cref="CostEvaluator"/> that handles
		 /// negative weights.
		 /// 
		 /// See http://en.wikipedia.org/wiki/Dijkstra%27s_algorithm for more
		 /// information.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="costEvaluator"> evaluator that can return the cost represented
		 /// by each relationship the algorithm traverses. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator )
		 {
			  return new DijkstraBidirectional( expander, costEvaluator );
		 }

		 /// <summary>
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/> for documentation.
		 /// 
		 /// Uses a cost evaluator which uses the supplied property key to
		 /// represent the cost (values of type <b>double</b>).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="relationshipPropertyRepresentingCost"> the property to represent cost
		 /// on each relationship the algorithm traverses. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, string relationshipPropertyRepresentingCost )
		 {
			  return dijkstra( expander, new DoubleEvaluator( relationshipPropertyRepresentingCost ) );
		 }

		 /// <summary>
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/> for documentation
		 /// 
		 /// Instead of finding all shortest paths with equal cost, find the top {@code numberOfWantedPaths} paths.
		 /// This is usually slower than finding all shortest paths with equal cost.
		 /// 
		 /// Uses a cost evaluator which uses the supplied property key to
		 /// represent the cost (values of type <b>double</b>).
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="relationshipPropertyRepresentingCost"> the property to represent cost
		 /// on each relationship the algorithm traverses. </param>
		 /// <param name="numberOfWantedPaths"> number of paths to find. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, string relationshipPropertyRepresentingCost, int numberOfWantedPaths )
		 {
			  return dijkstra( expander, new DoubleEvaluator( relationshipPropertyRepresentingCost ), numberOfWantedPaths );
		 }

		 /// <summary>
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/> for documentation
		 /// 
		 /// Instead of finding all shortest paths with equal cost, find the top {@code numberOfWantedPaths} paths.
		 /// This is usually slower than finding all shortest paths with equal cost.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="costEvaluator"> evaluator that can return the cost represented
		 /// by each relationship the algorithm traverses. </param>
		 /// <param name="numberOfWantedPaths"> number of paths to find. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator, int numberOfWantedPaths )
		 {
			  return new Dijkstra( expander, costEvaluator, NoneStrictMath.EPSILON, PathInterestFactory.numberOfShortest( NoneStrictMath.EPSILON, numberOfWantedPaths ) );
		 }

		 /// @deprecated Dijkstra should not be used with state on <seealso cref="PathExpander"/>
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/>.
		 /// 
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/> for documentation.
		 /// 
		 /// Uses a cost evaluator which uses the supplied property key to
		 /// represent the cost (values of type <b>double</b>).
		 /// 
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="stateFactory"> initial state for the traversal branches. </param>
		 /// <param name="costEvaluator"> the cost evaluator for each relationship the algorithm traverses. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 [Obsolete("Dijkstra should not be used with state on <seealso cref=\"PathExpander\"/>")]
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, InitialBranchState stateFactory, CostEvaluator<double> costEvaluator )
		 {
			  return new Dijkstra( expander, stateFactory, costEvaluator );
		 }

		 /// @deprecated Dijkstra should not be used with state on <seealso cref="PathExpander"/>
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/>.
		 /// 
		 /// See <seealso cref="dijkstra(PathExpander, CostEvaluator)"/> for documentation.
		 /// 
		 /// Uses a cost evaluator which uses the supplied property key to
		 /// represent the cost (values of type <b>double</b>).
		 /// 
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for expanding
		 /// <seealso cref="Relationship"/>s for each <seealso cref="Path"/>. </param>
		 /// <param name="stateFactory"> initial state for the traversal branches. </param>
		 /// <param name="relationshipPropertyRepresentingCost"> the property to represent cost
		 /// on each relationship the algorithm traverses. </param>
		 /// <returns> an algorithm which finds the cheapest path between two nodes
		 /// using the Dijkstra algorithm. </returns>
		 [Obsolete("Dijkstra should not be used with state on <seealso cref=\"PathExpander\"/>")]
		 public static PathFinder<WeightedPath> Dijkstra( PathExpander expander, InitialBranchState stateFactory, string relationshipPropertyRepresentingCost )
		 {
			  return dijkstra( expander, stateFactory, new DoubleEvaluator( relationshipPropertyRepresentingCost ) );
		 }
	}

}