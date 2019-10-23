﻿using System.Collections.Generic;

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
namespace Neo4Net.GraphAlgo.Path
{
	using MutableDouble = org.apache.commons.lang3.mutable.MutableDouble;

	using Neo4Net.GraphAlgo;
	using Neo4Net.GraphAlgo;
	using DijkstraBranchCollisionDetector = Neo4Net.GraphAlgo.Utils.DijkstraBranchCollisionDetector;
	using DijkstraSelectorFactory = Neo4Net.GraphAlgo.Utils.DijkstraSelectorFactory;
	using Neo4Net.GraphAlgo.Utils;
	using PathInterestFactory = Neo4Net.GraphAlgo.Utils.PathInterestFactory;
	using TopFetchingWeightedPathIterator = Neo4Net.GraphAlgo.Utils.TopFetchingWeightedPathIterator;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using BidirectionalTraversalDescription = Neo4Net.GraphDb.Traversal.BidirectionalTraversalDescription;
	using Neo4Net.GraphDb.Traversal;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using Evaluators = Neo4Net.GraphDb.Traversal.Evaluators;
	using Neo4Net.GraphDb.Traversal;
	using Neo4Net.GraphDb.Traversal;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using TraversalMetadata = Neo4Net.GraphDb.Traversal.TraversalMetadata;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.Traversal.Uniqueness;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.firstOrNull;

	/// <summary>
	/// Find (one or all) simple shortest path(s) between two nodes.
	/// Shortest referring to least cost evaluated by provided <seealso cref="CostEvaluator"/>.
	/// It starts a traversal from both ends and terminates when path(s) has been found.
	/// 
	/// Relationships are traversed in the specified directions from the start node,
	/// but in the reverse direction ( <seealso cref="org.Neo4Net.graphdb.Direction.reverse()"/> ) from the
	/// end node. This doesn't affect <seealso cref="org.Neo4Net.graphdb.Direction.BOTH"/>.
	/// 
	/// @author Anton Persson
	/// </summary>
	public class DijkstraBidirectional : PathFinder<WeightedPath>
	{
		 private readonly PathExpander _expander;
		 private readonly InitialBranchState _stateFactory;
		 private readonly CostEvaluator<double> _costEvaluator;
		 private readonly double _epsilon;
		 private Traverser _lastTraverser;

		 /// <summary>
		 /// See <seealso cref="DijkstraBidirectional(PathExpander, CostEvaluator, double)"/>
		 /// Using <seealso cref="NoneStrictMath.EPSILON"/> as tolerance.
		 /// </summary>
		 public DijkstraBidirectional( PathExpander expander, CostEvaluator<double> costEvaluator ) : this( expander, costEvaluator, NoneStrictMath.EPSILON )
		 {
		 }

		 /// <summary>
		 /// Construct a new bidirectional dijkstra algorithm. </summary>
		 /// <param name="expander">          The <seealso cref="PathExpander"/> to be used to decide which relationships
		 ///                          to expand for each node </param>
		 /// <param name="costEvaluator">     The <seealso cref="CostEvaluator"/> to be used for calculating cost of a
		 ///                          relationship </param>
		 /// <param name="epsilon">           The tolerance level to be used when comparing floating point numbers. </param>
		 public DijkstraBidirectional( PathExpander expander, CostEvaluator<double> costEvaluator, double epsilon )
		 {
			  this._expander = expander;
			  this._costEvaluator = costEvaluator;
			  this._epsilon = epsilon;
			  this._stateFactory = InitialBranchState.DOUBLE_ZERO;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.Neo4Net.graphalgo.WeightedPath> findAllPaths(org.Neo4Net.graphdb.Node start, final org.Neo4Net.graphdb.Node end)
		 public override IEnumerable<WeightedPath> FindAllPaths( Node start, Node end )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.traversal.Traverser traverser = traverser(start, end, org.Neo4Net.graphalgo.impl.util.PathInterestFactory.allShortest(epsilon));
			  Traverser traverser = traverser( start, end, PathInterestFactory.allShortest( _epsilon ) );
			  return () => new TopFetchingWeightedPathIterator(traverser.GetEnumerator(), _costEvaluator);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.graphdb.traversal.Traverser traverser(org.Neo4Net.graphdb.Node start, final org.Neo4Net.graphdb.Node end, org.Neo4Net.graphalgo.impl.util.PathInterest interest)
		 private Traverser Traverser( Node start, Node end, PathInterest interest )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableDouble shortestSoFar = new org.apache.commons.lang3.mutable.MutableDouble(Double.MAX_VALUE);
			  MutableDouble shortestSoFar = new MutableDouble( double.MaxValue );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableDouble startSideShortest = new org.apache.commons.lang3.mutable.MutableDouble(0);
			  MutableDouble startSideShortest = new MutableDouble( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableDouble endSideShortest = new org.apache.commons.lang3.mutable.MutableDouble(0);
			  MutableDouble endSideShortest = new MutableDouble( 0 );
			  PathExpander dijkstraExpander = new DijkstraBidirectionalPathExpander( _expander, shortestSoFar, true, startSideShortest, endSideShortest, _epsilon );

			  IGraphDatabaseService db = start.GraphDatabase;

			  TraversalDescription side = Db.traversalDescription().expand(dijkstraExpander, _stateFactory).order(new DijkstraSelectorFactory(interest, _costEvaluator)).evaluator(new DijkstraBidirectionalEvaluator(_costEvaluator)).uniqueness(Uniqueness.NODE_PATH);

			  TraversalDescription startSide = side;
			  TraversalDescription endSide = side.Reverse();

			  BidirectionalTraversalDescription traversal = Db.bidirectionalTraversalDescription().startSide(startSide).endSide(endSide).collisionEvaluator(Evaluators.all()).collisionPolicy((evaluator, pathPredicate) => new DijkstraBranchCollisionDetector(evaluator, _costEvaluator, shortestSoFar, _epsilon, pathPredicate));

			  _lastTraverser = traversal.Traverse( start, end );
			  return _lastTraverser;
		 }

		 public override WeightedPath FindSinglePath( Node start, Node end )
		 {
			  return firstOrNull( new TopFetchingWeightedPathIterator( Traverser( start, end, PathInterestFactory.single( _epsilon ) ).GetEnumerator(), _costEvaluator ) );
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastTraverser.metadata();
		 }

		 private class DijkstraBidirectionalPathExpander : PathExpander<double>
		 {
			  internal readonly PathExpander Source;
			  internal readonly MutableDouble ShortestSoFar;
			  internal readonly MutableDouble OtherSideShortest;
			  internal readonly double Epsilon;
			  internal readonly MutableDouble ThisSideShortest;
			  internal readonly bool StopAfterLowestCost;

			  internal DijkstraBidirectionalPathExpander( PathExpander source, MutableDouble shortestSoFar, bool stopAfterLowestCost, MutableDouble thisSideShortest, MutableDouble otherSideShortest, double epsilon )
			  {
					this.Source = source;
					this.ShortestSoFar = shortestSoFar;
					this.StopAfterLowestCost = stopAfterLowestCost;
					this.ThisSideShortest = thisSideShortest;
					this.OtherSideShortest = otherSideShortest;
					this.Epsilon = epsilon;
			  }

			  public override IEnumerable<Relationship> Expand( Path path, BranchState<double> state )
			  {
					double thisState = state.State;
					ThisSideShortest.Value = thisState;
					if ( NoneStrictMath.compare( thisState + OtherSideShortest.doubleValue(), ShortestSoFar.doubleValue(), Epsilon ) > 0 && StopAfterLowestCost )
					{
						 return Iterables.emptyResourceIterable();
					}
					return Source.expand( path, state );
			  }

			  public override PathExpander<double> Reverse()
			  {
					return new DijkstraBidirectionalPathExpander( Source.reverse(), ShortestSoFar, StopAfterLowestCost, OtherSideShortest, ThisSideShortest, Epsilon );
			  }
		 }

		 private class DijkstraBidirectionalEvaluator : Neo4Net.GraphDb.Traversal.PathEvaluator_Adapter<double>
		 {
			  internal readonly CostEvaluator<double> CostEvaluator;

			  internal DijkstraBidirectionalEvaluator( CostEvaluator<double> costEvaluator )
			  {
					this.CostEvaluator = costEvaluator;
			  }

			  public override Evaluation Evaluate( Path path, BranchState<double> state )
			  {
					double nextState = state.State;
					if ( path.Length() > 0 )
					{
						 nextState += CostEvaluator.getCost( path.LastRelationship(), OUTGOING );
						 state.State = nextState;
					}
					return Evaluation.EXCLUDE_AND_CONTINUE;
			  }
		 }
	}

}