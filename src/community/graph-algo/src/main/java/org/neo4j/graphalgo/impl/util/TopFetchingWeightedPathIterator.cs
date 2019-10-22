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
namespace Neo4Net.Graphalgo.impl.util
{

	using Neo4Net.Graphalgo;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.Helpers.Collections;
	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

	/// <summary>
	/// @author Anton Persson
	/// </summary>
	public class TopFetchingWeightedPathIterator : PrefetchingIterator<WeightedPath>
	{
		 private readonly IEnumerator<Path> _paths;
		 private IList<WeightedPath> _shortestPaths;
		 private IEnumerator<WeightedPath> _shortestIterator;
		 private readonly CostEvaluator<double> _costEvaluator;
		 private double _foundWeight;
		 private readonly double _epsilon;

		 public TopFetchingWeightedPathIterator( IEnumerator<Path> paths, CostEvaluator<double> costEvaluator ) : this( paths, costEvaluator, NoneStrictMath.EPSILON )
		 {
		 }

		 public TopFetchingWeightedPathIterator( IEnumerator<Path> paths, CostEvaluator<double> costEvaluator, double epsilon )
		 {
			  this._paths = paths;
			  this._costEvaluator = costEvaluator;
			  this._epsilon = epsilon;
			  this._foundWeight = double.MaxValue;
		 }

		 protected internal override WeightedPath FetchNextOrNull()
		 {
			  if ( _shortestIterator == null )
			  {
					_shortestPaths = new List<WeightedPath>();

					while ( _paths.MoveNext() )
					{
						 WeightedPath path = new WeightedPathImpl( _costEvaluator, _paths.Current );

						 if ( NoneStrictMath.compare( path.Weight(), _foundWeight, _epsilon ) < 0 )
						 {
							  _foundWeight = path.Weight();
							  _shortestPaths.Clear();
						 }
						 if ( NoneStrictMath.compare( path.Weight(), _foundWeight, _epsilon ) <= 0 )
						 {
							  _shortestPaths.Add( path );
						 }
					}
					_shortestIterator = _shortestPaths.GetEnumerator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _shortestIterator.hasNext() ? _shortestIterator.next() : null;
		 }
	}

}