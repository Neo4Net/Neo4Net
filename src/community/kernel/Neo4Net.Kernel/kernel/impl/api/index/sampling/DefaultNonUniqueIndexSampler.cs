using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;

	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;

	public class DefaultNonUniqueIndexSampler : NonUniqueIndexSampler
	{
		 private readonly int _sampleSizeLimit;
		 private readonly MutableObjectLongMap<string> _values;

		 private int _sampledSteps;

		 // kept as longs to side step overflow issues

		 private long _accumulatedUniqueValues;
		 private long _accumulatedSampledSize;
		 private long _sampleSize;

		 public DefaultNonUniqueIndexSampler( int sampleSizeLimit )
		 {
			  this._values = new ObjectLongHashMap<string>( CalculateInitialSetSize( sampleSizeLimit ) );
			  this._sampleSizeLimit = sampleSizeLimit;
		 }

		 public override void Include( string value )
		 {
			  Include( value, 1 );
		 }

		 public override void Include( string value, long increment )
		 {
			  Debug.Assert( increment > 0 );
			  if ( _sampleSize >= _sampleSizeLimit )
			  {
					NextStep();
			  }

			  if ( _values.addToValue( value, increment ) == increment )
			  {
					_sampleSize += value.Length;
			  }
		 }

		 public override void Exclude( string value )
		 {
			  Exclude( value, 1 );
		 }

		 public override void Exclude( string value, long decrement )
		 {
			  Debug.Assert( decrement > 0 );
			  if ( _values.addToValue( value, -decrement ) <= 0 )
			  {
					_values.remove( value );
					_sampleSize -= value.Length;
			  }
		 }

		 public override IndexSample Result()
		 {
			  return Result( -1 );
		 }

		 public override IndexSample Result( int numDocs )
		 {
			  if ( !_values.Empty )
			  {
					NextStep();
			  }

			  long uniqueValues = _sampledSteps != 0 ? _accumulatedUniqueValues / _sampledSteps : 0;
			  long sampledSize = _sampledSteps != 0 ? _accumulatedSampledSize / _sampledSteps : 0;

			  return new IndexSample( numDocs < 0 ? _accumulatedSampledSize : numDocs, uniqueValues, sampledSize );
		 }

		 private void NextStep()
		 {
			  _accumulatedUniqueValues += _values.size();
			  _accumulatedSampledSize += _values.sum();
			  _sampleSize = 0;

			  _sampledSteps++;
			  _values.clear();
		 }

		 /// <summary>
		 /// Evaluate initial set size that evaluate initial set as log2(sampleSizeLimit) / 2 based on provided sample size
		 /// limit.
		 /// Minimum possible size is 1 << 10.
		 /// Maximum possible size is 1 << 16.
		 /// </summary>
		 /// <param name="sampleSizeLimit"> specified sample size limit </param>
		 /// <returns> initial set size </returns>
		 private int CalculateInitialSetSize( int sampleSizeLimit )
		 {
			  int basedOnSampleSize = Math.Max( 10, ( int )( Math.Log( sampleSizeLimit ) / Math.Log( 2 ) ) / 2 );
			  return 1 << Math.Min( 16, basedOnSampleSize );
		 }
	}

}