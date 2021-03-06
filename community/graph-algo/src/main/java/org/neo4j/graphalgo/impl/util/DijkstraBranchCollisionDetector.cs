﻿/*
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
namespace Org.Neo4j.Graphalgo.impl.util
{
	using MutableDouble = org.apache.commons.lang3.mutable.MutableDouble;

	using Org.Neo4j.Graphalgo;
	using Path = Org.Neo4j.Graphdb.Path;
	using StandardBranchCollisionDetector = Org.Neo4j.Graphdb.impl.traversal.StandardBranchCollisionDetector;
	using Evaluator = Org.Neo4j.Graphdb.traversal.Evaluator;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using NoneStrictMath = Org.Neo4j.Kernel.impl.util.NoneStrictMath;

	/// <summary>
	/// @author Anton Persson
	/// </summary>
	public class DijkstraBranchCollisionDetector : StandardBranchCollisionDetector
	{
		 private readonly CostEvaluator _costEvaluator;
		 private readonly MutableDouble _shortestSoFar;
		 private readonly double _epsilon;

		 public DijkstraBranchCollisionDetector( Evaluator evaluator, CostEvaluator costEvaluator, MutableDouble shortestSoFar, double epsilon, System.Predicate<Path> pathPredicate ) : base( evaluator, pathPredicate )
		 {
			  this._costEvaluator = costEvaluator;
			  this._shortestSoFar = shortestSoFar;
			  this._epsilon = epsilon;
		 }

		 protected internal override bool IncludePath( Path path, TraversalBranch startBranch, TraversalBranch endBranch )
		 {
			  if ( !base.IncludePath( path, startBranch, endBranch ) )
			  {
					return false;
			  }

			  /*
			  In most cases we could prune startBranch and endBranch here.
	
			  Problem when assuming startBranch and endBranch are pruned:
	
			  Path (s) -...- (c) weight x
			  path (s) -...- (a) weight x
			  path (d) -...- (t) weight y
			  path (b) -...- (t) weight y
			  rel (c) - (b) weight z
			  rel (a) - (b) weight z
			  rel (a) - (d) weight z
	
			        - (c) ----   ---- (d) -
			     ...           X           ...
			     /  (prune)> /   \ <(prune)  \
			   (s) -^v^v- (a) -- (b) -^v^v- (t)
	
			   -^v^v- and ... meaning "some path"
	
			  We expect following collisions:
			          1. start (c) - (b) end. Result in path of weight x+z+y
			          2. start (a) - (d) end. Result in path of weight x+z+y
			          3. start (a) - (b) end. Result in path of weight x+z+y
			  However, if branches are pruned on collision 1 and 2. Collision 3 will never happen and thus
			  a path is missed.
			  */

			  double cost = ( new WeightedPathImpl( _costEvaluator, path ) ).weight();

			  if ( cost < _shortestSoFar.doubleValue() )
			  {
					_shortestSoFar.Value = cost;
			  }
			  return NoneStrictMath.compare( cost, _shortestSoFar.doubleValue(), _epsilon ) <= 0;

		 }
	}

}