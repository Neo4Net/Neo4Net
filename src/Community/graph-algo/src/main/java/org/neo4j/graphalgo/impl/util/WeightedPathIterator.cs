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
namespace Neo4Net.Graphalgo.impl.util
{
	using Neo4Net.Graphalgo;
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb;
	using Neo4Net.Helpers.Collections;
	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

	public class WeightedPathIterator : PrefetchingResourceIterator<WeightedPath>
	{
		 private readonly ResourceIterator<Path> _paths;
		 private readonly CostEvaluator<double> _costEvaluator;
		 private double? _foundWeight;
		 private int _foundTotal;
		 private readonly double _epsilon;
		 private readonly PathInterest _interest;

		 public WeightedPathIterator( ResourceIterator<Path> paths, CostEvaluator<double> costEvaluator, bool stopAfterLowestWeight ) : this( paths, costEvaluator, NoneStrictMath.EPSILON, stopAfterLowestWeight )
		 {
		 }

		 public WeightedPathIterator( ResourceIterator<Path> paths, CostEvaluator<double> costEvaluator, double epsilon, bool stopAfterLowestWeight ) : this( paths, costEvaluator, epsilon, stopAfterLowestWeight ? PathInterestFactory.AllShortest( epsilon ) : PathInterestFactory.All( epsilon ) )
		 {
		 }

		 public WeightedPathIterator( ResourceIterator<Path> paths, CostEvaluator<double> costEvaluator, double epsilon, PathInterest interest )
		 {
			  this._paths = paths;
			  this._costEvaluator = costEvaluator;
			  this._epsilon = epsilon;
			  this._interest = interest;
		 }

		 protected internal override WeightedPath FetchNextOrNull()
		 {
			  if ( !_interest.stillInteresting( ++_foundTotal ) )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !_paths.hasNext() )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  WeightedPath path = new WeightedPathImpl( _costEvaluator, _paths.next() );
			  if ( _interest.stopAfterLowestCost() && _foundWeight != null && NoneStrictMath.compare(path.Weight(), _foundWeight.Value, _epsilon) > 0 )
			  {
					return null;
			  }
			  _foundWeight = path.Weight();
			  return path;
		 }

		 public override void Close()
		 {
			  _paths.close();
		 }
	}

}