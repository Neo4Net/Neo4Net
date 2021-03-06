﻿using System;
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
namespace Org.Neo4j.Graphalgo.impl.path
{
	using MutableDouble = org.apache.commons.lang3.mutable.MutableDouble;

	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo;
	using DijkstraSelectorFactory = Org.Neo4j.Graphalgo.impl.util.DijkstraSelectorFactory;
	using Org.Neo4j.Graphalgo.impl.util;
	using PathInterestFactory = Org.Neo4j.Graphalgo.impl.util.PathInterestFactory;
	using WeightedPathIterator = Org.Neo4j.Graphalgo.impl.util.WeightedPathIterator;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.traversal;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Evaluators = Org.Neo4j.Graphdb.traversal.Evaluators;
	using Org.Neo4j.Graphdb.traversal;
	using Org.Neo4j.Graphdb.traversal;
	using TraversalMetadata = Org.Neo4j.Graphdb.traversal.TraversalMetadata;
	using Traverser = Org.Neo4j.Graphdb.traversal.Traverser;
	using Uniqueness = Org.Neo4j.Graphdb.traversal.Uniqueness;
	using MonoDirectionalTraversalDescription = Org.Neo4j.Kernel.impl.traversal.MonoDirectionalTraversalDescription;
	using NoneStrictMath = Org.Neo4j.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphalgo.impl.util.PathInterestFactory.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.firstOrNull;

	/// <summary>
	/// Find (one or some) simple shortest path(s) between two nodes.
	/// Shortest referring to least cost evaluated by provided <seealso cref="CostEvaluator"/>.
	/// 
	/// When asking for <seealso cref="findAllPaths(Node, Node)"/> behaviour will depending on
	/// which <seealso cref="PathInterest"/> is used.
	/// Recommended option is
	/// <seealso cref="PathInterestFactory.numberOfShortest(double,int)"/> - defined number of shortest path in increasing order
	/// 
	/// Also available
	/// <seealso cref="PathInterestFactory.allShortest(double)"/>          - Find all paths that are equal in length to shortest.
	///                                                            <seealso cref="DijkstraBidirectional"/> does this faster.
	/// <seealso cref="PathInterestFactory.all(double)"/>                  - Find all paths in increasing order. This option has
	///                                                            performance problem and is not recommended.
	/// 
	/// @author Tobias Ivarsson
	/// @author Martin Neumann
	/// @author Mattias Persson
	/// @author Anton Persson
	/// </summary>
	public class Dijkstra : PathFinder<WeightedPath>
	{
		 private readonly PathExpander _expander;
		 private readonly InitialBranchState _stateFactory;
		 private readonly CostEvaluator<double> _costEvaluator;
		 private Traverser _lastTraverser;
		 private readonly double _epsilon;
		 private readonly PathInterest<double> _interest;
		 private readonly bool _stateInUse;
		 // TODO: Remove stateInUse when removing deprecated constructors that uses InitialBranchState.
		 // TODO: ALso set traverser to always use DijkstraPathExpander and DijkstraEvaluator.

		 /// @deprecated Dijkstra should not be used with state
		 /// Use <seealso cref="Dijkstra(PathExpander, CostEvaluator)"/> instead. 
		 [Obsolete("Dijkstra should not be used with state")]
		 public Dijkstra( PathExpander expander, InitialBranchState stateFactory, CostEvaluator<double> costEvaluator ) : this( expander, stateFactory, costEvaluator, true )
		 {
		 }

		 /// @deprecated Dijkstra should not be used with state.
		 /// Use <seealso cref="Dijkstra(PathExpander, CostEvaluator, PathInterest)"/> instead. 
		 [Obsolete("Dijkstra should not be used with state.")]
		 public Dijkstra( PathExpander expander, InitialBranchState stateFactory, CostEvaluator<double> costEvaluator, bool stopAfterLowestCost )
		 {
			  this._expander = expander;
			  this._costEvaluator = costEvaluator;
			  this._stateFactory = stateFactory;
			  _interest = stopAfterLowestCost ? PathInterestFactory.allShortest( NoneStrictMath.EPSILON ) : PathInterestFactory.all( NoneStrictMath.EPSILON );
			  _epsilon = NoneStrictMath.EPSILON;
			  this._stateInUse = true;
		 }

		 /// <summary>
		 /// See <seealso cref="Dijkstra(PathExpander, CostEvaluator, double, PathInterest)"/>
		 /// Use <seealso cref="NoneStrictMath.EPSILON"/> as tolerance.
		 /// Use <seealso cref="PathInterestFactory.allShortest(double)"/> as PathInterest.
		 /// </summary>
		 public Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator ) : this( expander, costEvaluator, PathInterestFactory.allShortest( NoneStrictMath.EPSILON ) )
		 {
		 }

		 /// @deprecated in favor for <seealso cref="Dijkstra(PathExpander, CostEvaluator, PathInterest)"/>  }. 
		 [Obsolete("in favor for <seealso cref=\"Dijkstra(PathExpander, CostEvaluator, PathInterest)\"/>  }.")]
		 public Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator, bool stopAfterLowestCost ) : this( expander, costEvaluator, NoneStrictMath.EPSILON, stopAfterLowestCost ? PathInterestFactory.allShortest( NoneStrictMath.EPSILON ) : PathInterestFactory.all( NoneStrictMath.EPSILON ) )
		 {
		 }

		 /// <summary>
		 /// See <seealso cref="Dijkstra(PathExpander, CostEvaluator, double, PathInterest)"/>
		 /// Use <seealso cref="NoneStrictMath.EPSILON"/> as tolerance.
		 /// </summary>
		 public Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator, PathInterest<double> interest ) : this( expander, costEvaluator, NoneStrictMath.EPSILON, interest )
		 {
		 }

		 /// <summary>
		 /// Construct new dijkstra algorithm. </summary>
		 /// <param name="expander">          <seealso cref="PathExpander"/> to be used to decide which relationships
		 ///                          to expand. </param>
		 /// <param name="costEvaluator">     <seealso cref="CostEvaluator"/> to be used to calculate cost of relationship </param>
		 /// <param name="epsilon">           The tolerance level to be used when comparing floating point numbers. </param>
		 /// <param name="interest">          <seealso cref="PathInterest"/> to be used when deciding if a path is interesting.
		 ///                          Recommend to use <seealso cref="PathInterestFactory"/> to get reliable behaviour. </param>
		 public Dijkstra( PathExpander expander, CostEvaluator<double> costEvaluator, double epsilon, PathInterest<double> interest )
		 {
			  this._expander = expander;
			  this._costEvaluator = costEvaluator;
			  this._epsilon = epsilon;
			  this._interest = interest;
			  this._stateFactory = InitialBranchState.DOUBLE_ZERO;
			  this._stateInUse = false;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.graphalgo.WeightedPath> findAllPaths(org.neo4j.graphdb.Node start, final org.neo4j.graphdb.Node end)
		 public override IEnumerable<WeightedPath> FindAllPaths( Node start, Node end )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.traversal.Traverser traverser = traverser(start, end, interest);
			  Traverser traverser = traverser( start, end, _interest );
			  return () => new WeightedPathIterator(traverser.GetEnumerator(), _costEvaluator, _epsilon, _interest);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.traversal.Traverser traverser(org.neo4j.graphdb.Node start, final org.neo4j.graphdb.Node end, org.neo4j.graphalgo.impl.util.PathInterest<double> interest)
		 private Traverser Traverser( Node start, Node end, PathInterest<double> interest )
		 {
			  PathExpander dijkstraExpander;
			  PathEvaluator dijkstraEvaluator;
			  if ( _stateInUse )
			  {
					dijkstraExpander = _expander;
					dijkstraEvaluator = Evaluators.includeWhereEndNodeIs( end );
			  }
			  else
			  {
					MutableDouble shortestSoFar = new MutableDouble( double.MaxValue );
					dijkstraExpander = new DijkstraPathExpander( _expander, shortestSoFar, _epsilon, interest.StopAfterLowestCost() );
					dijkstraEvaluator = new DijkstraEvaluator( shortestSoFar, end, _costEvaluator );
			  }
			  _lastTraverser = ( new MonoDirectionalTraversalDescription() ).uniqueness(Uniqueness.NODE_PATH).expand(dijkstraExpander, _stateFactory).order(new DijkstraSelectorFactory(interest, _costEvaluator)).evaluator(dijkstraEvaluator).traverse(start);
			  return _lastTraverser;
		 }

		 public override WeightedPath FindSinglePath( Node start, Node end )
		 {
			  return firstOrNull( new WeightedPathIterator( Traverser( start, end, single( _epsilon ) ).GetEnumerator(), _costEvaluator, _epsilon, _interest ) );
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastTraverser.metadata();
		 }

		 private class DijkstraPathExpander : PathExpander<double>
		 {
			  protected internal readonly PathExpander Source;
			  protected internal MutableDouble ShortestSoFar;
			  internal readonly double Epsilon;
			  protected internal readonly bool StopAfterLowestCost;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: DijkstraPathExpander(final org.neo4j.graphdb.PathExpander source, org.apache.commons.lang3.mutable.MutableDouble shortestSoFar, double epsilon, boolean stopAfterLowestCost)
			  internal DijkstraPathExpander( PathExpander source, MutableDouble shortestSoFar, double epsilon, bool stopAfterLowestCost )
			  {
					this.Source = source;
					this.ShortestSoFar = shortestSoFar;
					this.Epsilon = epsilon;
					this.StopAfterLowestCost = stopAfterLowestCost;
			  }

			  public override IEnumerable<Relationship> Expand( Path path, BranchState<double> state )
			  {
					if ( NoneStrictMath.compare( state.State, ShortestSoFar.doubleValue(), Epsilon ) > 0 && StopAfterLowestCost )
					{
						 return Collections.emptyList();
					}
					return Source.expand( path, state );
			  }

			  public override PathExpander<double> Reverse()
			  {
					return new DijkstraPathExpander( Source.reverse(), ShortestSoFar, Epsilon, StopAfterLowestCost );
			  }
		 }

		 private class DijkstraEvaluator : Org.Neo4j.Graphdb.traversal.PathEvaluator_Adapter<double>
		 {
			  internal readonly MutableDouble ShortestSoFar;
			  internal readonly Node EndNode;
			  internal readonly CostEvaluator<double> CostEvaluator;

			  internal DijkstraEvaluator( MutableDouble shortestSoFar, Node endNode, CostEvaluator<double> costEvaluator )
			  {
					this.ShortestSoFar = shortestSoFar;
					this.EndNode = endNode;
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
					if ( path.EndNode().Equals(EndNode) )
					{
						 ShortestSoFar.Value = Math.Min( ShortestSoFar.doubleValue(), nextState );
						 return Evaluation.INCLUDE_AND_PRUNE;
					}
					return Evaluation.EXCLUDE_AND_CONTINUE;
			  }
		 }

	}

}