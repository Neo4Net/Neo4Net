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
namespace Neo4Net.Graphalgo.impl.path
{
	using Neo4Net.Graphalgo;
	using Neo4Net.Graphalgo;
	using Neo4Net.Graphalgo;
	using Neo4Net.Graphalgo.impl.util;
	using Neo4Net.Graphalgo.impl.util;
	using PathInterestFactory = Neo4Net.Graphalgo.impl.util.PathInterestFactory;
	using WeightedPathIterator = Neo4Net.Graphalgo.impl.util.WeightedPathIterator;
	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb.traversal;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;
	using TraversalMetadata = Neo4Net.Graphdb.traversal.TraversalMetadata;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;
	using Uniqueness = Neo4Net.Graphdb.traversal.Uniqueness;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.includeWhereEndNodeIs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.InitialBranchState.NO_STATE;

	/// <summary>
	/// Implementation of A* algorithm, see <seealso cref="AStar"/>, but using the traversal
	/// framework. It's still in an experimental state.
	/// </summary>
	public class TraversalAStar : PathFinder<WeightedPath>
	{
		 private readonly CostEvaluator<double> _costEvaluator;
		 private readonly PathExpander _expander;
		 private readonly InitialBranchState _initialState;
		 private Traverser _lastTraverser;

		 private readonly EstimateEvaluator<double> _estimateEvaluator;
		 private bool _stopAfterLowestWeight;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> TraversalAStar(org.neo4j.graphdb.PathExpander<T> expander, org.neo4j.graphalgo.CostEvaluator<double> costEvaluator, org.neo4j.graphalgo.EstimateEvaluator<double> estimateEvaluator)
		 public TraversalAStar<T>( PathExpander<T> expander, CostEvaluator<double> costEvaluator, EstimateEvaluator<double> estimateEvaluator ) : this( expander, NO_STATE, costEvaluator, estimateEvaluator, true )
		 {
		 }

		 public TraversalAStar<T>( PathExpander<T> expander, InitialBranchState<T> initialState, CostEvaluator<double> costEvaluator, EstimateEvaluator<double> estimateEvaluator ) : this( expander, initialState, costEvaluator, estimateEvaluator, true )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> TraversalAStar(org.neo4j.graphdb.PathExpander<T> expander, org.neo4j.graphalgo.CostEvaluator<double> costEvaluator, org.neo4j.graphalgo.EstimateEvaluator<double> estimateEvaluator, boolean stopAfterLowestWeight)
		 public TraversalAStar<T>( PathExpander<T> expander, CostEvaluator<double> costEvaluator, EstimateEvaluator<double> estimateEvaluator, bool stopAfterLowestWeight ) : this( expander, NO_STATE, costEvaluator, estimateEvaluator, stopAfterLowestWeight )
		 {
		 }

		 public TraversalAStar<T>( PathExpander<T> expander, InitialBranchState<T> initialState, CostEvaluator<double> costEvaluator, EstimateEvaluator<double> estimateEvaluator, bool stopAfterLowestWeight )
		 {
			  this._costEvaluator = costEvaluator;
			  this._estimateEvaluator = estimateEvaluator;
			  this._stopAfterLowestWeight = stopAfterLowestWeight;
			  this._expander = expander;
			  this._initialState = initialState;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.graphalgo.WeightedPath> findAllPaths(org.neo4j.graphdb.Node start, final org.neo4j.graphdb.Node end)
		 public override IEnumerable<WeightedPath> FindAllPaths( Node start, Node end )
		 {
			  return Iterables.asIterable( FindSinglePath( start, end ) );
		 }

		 public override WeightedPath FindSinglePath( Node start, Node end )
		 {
			  return Iterables.firstOrNull( FindPaths( start, end, false ) );
		 }

		 private ResourceIterable<WeightedPath> FindPaths( Node start, Node end, bool multiplePaths )
		 {
			  PathInterest interest;
			  if ( multiplePaths )
			  {
					interest = _stopAfterLowestWeight ? PathInterestFactory.allShortest() : PathInterestFactory.all();
			  }
			  else
			  {
					interest = PathInterestFactory.single();
			  }

			  GraphDatabaseService db = start.GraphDatabase;
			  TraversalDescription traversalDescription = Db.traversalDescription().uniqueness(Uniqueness.NONE).expand(_expander, _initialState);

			  _lastTraverser = traversalDescription.Order( new SelectorFactory( this, end, interest ) ).evaluator( includeWhereEndNodeIs( end ) ).traverse( start );
			  return Iterators.asResourceIterable( new WeightedPathIterator( _lastTraverser.GetEnumerator(), _costEvaluator, _stopAfterLowestWeight ) );
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastTraverser.metadata();
		 }

		 private class PositionData : IComparable<PositionData>
		 {
			  internal readonly double WayLengthG;
			  internal readonly double EstimateH;

			  internal PositionData( double wayLengthG, double estimateH )
			  {
					this.WayLengthG = wayLengthG;
					this.EstimateH = estimateH;
			  }

			  internal virtual double? F()
			  {
					return this.EstimateH + this.WayLengthG;
			  }

			  public override int CompareTo( PositionData o )
			  {
					return F().compareTo(o.F());
			  }

			  public override string ToString()
			  {
					return "g:" + WayLengthG + ", h:" + EstimateH;
			  }
		 }

		 private class SelectorFactory : BestFirstSelectorFactory<PositionData, double>
		 {
			 private readonly TraversalAStar _outerInstance;

			  internal readonly Node End;

			  internal SelectorFactory( TraversalAStar outerInstance, Node end, PathInterest interest ) : base( interest )
			  {
				  this._outerInstance = outerInstance;
					this.End = end;
			  }

			  protected internal override PositionData AddPriority( TraversalBranch source, PositionData currentAggregatedValue, double? value )
			  {
					return new PositionData( currentAggregatedValue.WayLengthG + value, outerInstance.estimateEvaluator.GetCost( source.EndNode(), End ) );
			  }

			  protected internal override double? CalculateValue( TraversalBranch next )
			  {
					return next.Length() == 0 ? 0d : outerInstance.costEvaluator.GetCost(next.LastRelationship(), Direction.OUTGOING);
			  }

			  protected internal override PositionData StartData
			  {
				  get
				  {
						return new PositionData( 0, 0 );
				  }
			  }
		 }
	}

}