using System;

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
namespace Org.Neo4j.Gis.Spatial.Index.curves
{

	/// <summary>
	/// N-dimensional searchEnvelope
	/// </summary>
	internal class SearchEnvelope
	{
		 private long[] _min; // inclusive lower bounds
		 private long[] _max; // exclusive upper bounds
		 private int _nbrDim;

		 internal SearchEnvelope( SpaceFillingCurve curve, Envelope referenceEnvelope )
		 {
			  this._min = curve.GetNormalizedCoord( referenceEnvelope.Min );
			  this._max = curve.GetNormalizedCoord( referenceEnvelope.Max );
			  this._nbrDim = referenceEnvelope.Dimension;
			  for ( int i = 0; i < _nbrDim; i++ )
			  {
					// getNormalizedCoord gives inclusive bounds. Need to increment to make the upper exclusive.
					this._max[i] += 1;
			  }
		 }

		 private SearchEnvelope( long[] min, long[] max )
		 {
			  this._min = min;
			  this._max = max;
			  this._nbrDim = min.Length;
		 }

		 internal SearchEnvelope( long min, long max, int nbrDim )
		 {
			  this._nbrDim = nbrDim;
			  this._min = new long[nbrDim];
			  this._max = new long[nbrDim];

			  for ( int dim = 0; dim < nbrDim; dim++ )
			  {
					this._min[dim] = min;
					this._max[dim] = max;
			  }
		 }

		 internal virtual SearchEnvelope Quadrant( int[] quadNbrs )
		 {
			  long[] newMin = new long[_nbrDim];
			  long[] newMax = new long[_nbrDim];

			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					long extent = ( _max[dim] - _min[dim] ) / 2;
					newMin[dim] = this._min[dim] + quadNbrs[dim] * extent;
					newMax[dim] = this._min[dim] + ( quadNbrs[dim] + 1 ) * extent;
			  }
			  return new SearchEnvelope( newMin, newMax );
		 }

		 internal virtual bool Contains( long[] coord )
		 {
			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					if ( coord[dim] < _min[dim] || coord[dim] >= _max[dim] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 internal virtual bool Intersects( SearchEnvelope other )
		 {
			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					if ( this._max[dim] <= other._min[dim] || other._max[dim] <= this._min[dim] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <summary>
		 /// Calculates the faction of the overlapping area between {@code this} and {@code} other compared
		 /// to the area of {@code this}.
		 /// 
		 /// Must only be called for intersecting envelopes
		 /// </summary>
		 internal virtual double FractionOf( SearchEnvelope other )
		 {
			  double fraction = 1.0;
			  for ( int i = 0; i < _nbrDim; i++ )
			  {
					long min = Math.Max( this._min[i], other._min[i] );
					long max = Math.Min( this._max[i], other._max[i] );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double innerFraction = (double)(max - min) / (double)(other.max[i] - other.min[i]);
					double innerFraction = ( double )( max - min ) / ( double )( other._max[i] - other._min[i] );
					fraction *= innerFraction;
			  }
			  return fraction;
		 }

		 /// <summary>
		 /// The smallest possible envelope has unit area 1
		 /// </summary>
		 public virtual long Area
		 {
			 get
			 {
				  long area = 1;
				  for ( int i = 0; i < _nbrDim; i++ )
				  {
						area *= _max[i] - _min[i];
				  }
				  return area;
			 }
		 }
	}

}