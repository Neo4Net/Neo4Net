using System;
using System.Collections.Generic;
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
namespace Neo4Net.Gis.Spatial.Index.curves
{

	/// <summary>
	/// This class is also used by Neo4j Spatial
	/// </summary>

	public abstract class SpaceFillingCurve
	{
		 /// <summary>
		 /// Description of the space filling curve structure
		 /// </summary>
		 internal abstract class CurveRule
		 {
			  internal readonly int Dimension;
			  internal readonly int[] NpointValues;

			  internal CurveRule( int dimension, int[] npointValues )
			  {
					this.Dimension = dimension;
					this.NpointValues = npointValues;
					Debug.Assert( npointValues.Length == Length() );
			  }

			  internal int Length()
			  {
					return ( int ) Math.Pow( 2, Dimension );
			  }

			  internal virtual int NpointForIndex( int derivedIndex )
			  {
					return NpointValues[derivedIndex];
			  }

			  internal virtual int IndexForNPoint( int npoint )
			  {
					for ( int index = 0; index < NpointValues.Length; index++ )
					{
						 if ( NpointValues[index] == npoint )
						 {
							  return index;
						 }
					}
					return -1;
			  }

			  internal abstract CurveRule ChildAt( int npoint );
		 }

		 private readonly Envelope _range;
		 private readonly int _nbrDim;
		 private readonly int _maxLevel;
		 private readonly long _width;
		 private readonly long _valueWidth;
		 private readonly int _quadFactor;
		 private readonly long _initialNormMask;

		 private double[] _scalingFactor;

		 internal SpaceFillingCurve( Envelope range, int maxLevel )
		 {
			  this._range = range;
			  this._nbrDim = range.Dimension;
			  this._maxLevel = maxLevel;
			  if ( maxLevel < 1 )
			  {
					throw new System.ArgumentException( "Hilbert index needs at least one level" );
			  }
			  if ( range.Dimension > 3 )
			  {
					throw new System.ArgumentException( "Hilbert index does not yet support more than 3 dimensions" );
			  }
			  this._width = ( long ) Math.Pow( 2, maxLevel );
			  this._scalingFactor = new double[_nbrDim];
			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					_scalingFactor[dim] = this._width / range.GetWidth( dim );
			  }
			  this._valueWidth = ( long ) Math.Pow( 2, maxLevel * _nbrDim );
			  this._initialNormMask = ( long )( Math.Pow( 2, _nbrDim ) - 1 ) << ( maxLevel - 1 ) * _nbrDim;
			  this._quadFactor = ( int ) Math.Pow( 2, _nbrDim );
		 }

		 public virtual int MaxLevel
		 {
			 get
			 {
				  return _maxLevel;
			 }
		 }

		 public virtual long Width
		 {
			 get
			 {
				  return _width;
			 }
		 }

		 public virtual long ValueWidth
		 {
			 get
			 {
				  return _valueWidth;
			 }
		 }

		 public virtual double GetTileWidth( int dimension, int level )
		 {
			  return _range.getWidth( dimension ) / Math.Pow( 2, level );
		 }

		 public virtual Envelope Range
		 {
			 get
			 {
				  return _range;
			 }
		 }

		 protected internal abstract CurveRule RootCurve();

		 /// <summary>
		 /// Given a coordinate in multiple dimensions, calculate its derived key for maxLevel
		 /// Needs to be public due to dependency from Neo4j Spatial
		 /// </summary>
		 public virtual long? DerivedValueFor( double[] coord )
		 {
			  return DerivedValueFor( coord, _maxLevel );
		 }

		 /// <summary>
		 /// Given a coordinate in multiple dimensions, calculate its derived key for given level
		 /// </summary>
		 private long? DerivedValueFor( double[] coord, int level )
		 {
			  AssertValidLevel( level );
			  long[] normalizedValues = GetNormalizedCoord( coord );
			  return DerivedValueFor( normalizedValues, level );
		 }

		 /// <summary>
		 /// Given a normalized coordinate in multiple dimensions, calculate its derived key for maxLevel
		 /// </summary>
		 public virtual long? DerivedValueFor( long[] normalizedValues )
		 {
			  return DerivedValueFor( normalizedValues, _maxLevel );
		 }

		 /// <summary>
		 /// Given a normalized coordinate in multiple dimensions, calculate its derived key for given level
		 /// </summary>
		 private long? DerivedValueFor( long[] normalizedValues, int level )
		 {
			  AssertValidLevel( level );
			  long derivedValue = 0;
			  long mask = 1L << ( _maxLevel - 1 );

			  // The starting curve depends on the dimensions
			  CurveRule currentCurve = RootCurve();

			  for ( int i = 1; i <= _maxLevel; i++ )
			  {
					int bitIndex = _maxLevel - i;
					int npoint = 0;

					foreach ( long val in normalizedValues )
					{
						 npoint = npoint << 1 | ( int )( ( val & mask ) >> bitIndex );
					}

					int derivedIndex = currentCurve.IndexForNPoint( npoint );
					derivedValue = ( derivedValue << _nbrDim ) | derivedIndex;
					mask = mask >> 1;
					currentCurve = currentCurve.ChildAt( derivedIndex );
			  }

			  if ( level < _maxLevel )
			  {
					derivedValue = derivedValue << ( _nbrDim * _maxLevel - level );
			  }
			  return derivedValue;
		 }

		 /// <summary>
		 /// Given a derived key, find the center coordinate of the corresponding tile at maxLevel
		 /// </summary>
		 public virtual double[] CenterPointFor( long derivedValue )
		 {
			  return CenterPointFor( derivedValue, _maxLevel );
		 }

		 /// <summary>
		 /// Given a derived key, find the center coordinate of the corresponding tile at given level
		 /// </summary>
		 private double[] CenterPointFor( long derivedValue, int level )
		 {
			  long[] normalizedCoord = NormalizedCoordinateFor( derivedValue, level );
			  return GetDoubleCoord( normalizedCoord, level );
		 }

		 /// <summary>
		 /// Given a derived key, find the normalized coordinate it corresponds to on a specific level
		 /// </summary>
		 internal virtual long[] NormalizedCoordinateFor( long derivedValue, int level )
		 {
			  AssertValidLevel( level );
			  long mask = _initialNormMask;
			  long[] coordinate = new long[_nbrDim];

			  // First level is a single curveUp
			  CurveRule currentCurve = RootCurve();

			  for ( int i = 1; i <= level; i++ )
			  {

					int bitIndex = _maxLevel - i;

					int derivedIndex = ( int )( ( derivedValue & mask ) >> bitIndex * _nbrDim );
					int npoint = currentCurve.NpointForIndex( derivedIndex );
					int[] bitValues = bitValues( npoint );

					for ( int dim = 0; dim < _nbrDim; dim++ )
					{
						 coordinate[dim] = coordinate[dim] << 1 | bitValues[dim];
					}

					mask = mask >> _nbrDim;
					currentCurve = currentCurve.ChildAt( derivedIndex );
			  }

			  if ( level < _maxLevel )
			  {
					for ( int dim = 0; dim < _nbrDim; dim++ )
					{
						 coordinate[dim] = coordinate[dim] << _maxLevel - level;
					}
			  }

			  return coordinate;
		 }

		 /// <summary>
		 /// Given an envelope, find a collection of LongRange of tiles intersecting it on maxLevel and merge adjacent ones
		 /// </summary>
		 internal virtual IList<LongRange> GetTilesIntersectingEnvelope( Envelope referenceEnvelope )
		 {
			  return GetTilesIntersectingEnvelope( referenceEnvelope.Min, referenceEnvelope.Max, new StandardConfiguration() );
		 }

		 public virtual IList<LongRange> GetTilesIntersectingEnvelope( double[] fromOrNull, double[] toOrNull, SpaceFillingCurveConfiguration config )
		 {
			  double[] from = fromOrNull == null ? _range.Min : fromOrNull.Clone();
			  double[] to = toOrNull == null ? _range.Max : toOrNull.Clone();

			  for ( int i = 0; i < from.Length; i++ )
			  {
					if ( from[i] > to[i] )
					{
						 if ( fromOrNull == null )
						 {
							  to[i] = from[i];
						 }
						 else if ( toOrNull == null )
						 {
							  from[i] = to[i];
						 }
						 else
						 {
							  throw new System.ArgumentException( "Invalid range, min greater than max: " + from[i] + " > " + to[i] );
						 }
					}
			  }
			  Envelope referenceEnvelope = new Envelope( from, to );
			  return GetTilesIntersectingEnvelope( referenceEnvelope, config, null );
		 }

		 internal virtual IList<LongRange> GetTilesIntersectingEnvelope( Envelope referenceEnvelope, SpaceFillingCurveConfiguration config, SpaceFillingCurveMonitor monitor )
		 {
			  SearchEnvelope search = new SearchEnvelope( this, referenceEnvelope );
			  SearchEnvelope wholeExtent = new SearchEnvelope( 0, this.Width, _nbrDim );
			  List<LongRange> results = new List<LongRange>( config.InitialRangesListCapacity() );

			  if ( monitor != null )
			  {
					monitor.RegisterSearchArea( search.Area );
			  }

			  AddTilesIntersectingEnvelopeAt( config, monitor, 0, config.MaxDepth( referenceEnvelope, this._range, _nbrDim, _maxLevel ), search, wholeExtent, RootCurve(), 0, this.ValueWidth, results );
			  return results;
		 }

		 private void AddTilesIntersectingEnvelopeAt( SpaceFillingCurveConfiguration config, SpaceFillingCurveMonitor monitor, int depth, int maxDepth, SearchEnvelope search, SearchEnvelope currentExtent, CurveRule curve, long left, long right, List<LongRange> results )
		 {
			  if ( right - left == 1 )
			  {
					long[] coord = NormalizedCoordinateFor( left, _maxLevel );
					if ( search.Contains( coord ) )
					{
						 LongRange current = ( results.Count > 0 ) ? results[results.Count - 1] : null;
						 if ( current != null && current.Max == left - 1 )
						 {
							  current.ExpandToMax( left );
						 }
						 else
						 {
							  current = new LongRange( left );
							  results.Add( current );
						 }
						 if ( monitor != null )
						 {
							  monitor.AddRangeAtDepth( depth );
							  monitor.AddToCoveredArea( currentExtent.Area );
						 }
					}
			  }
			  else if ( search.Intersects( currentExtent ) )
			  {
					double overlap = search.FractionOf( currentExtent );
					if ( config.StopAtThisDepth( overlap, depth, maxDepth ) )
					{
						 // Note that LongRange upper bound is inclusive, hence the '-1' in several places
						 LongRange current = ( results.Count > 0 ) ? results[results.Count - 1] : null;
						 if ( current != null && current.Max == left - 1 )
						 {
							  current.ExpandToMax( right - 1 );
						 }
						 else
						 {
							  current = new LongRange( left, right - 1 );
							  results.Add( current );
						 }
						 if ( monitor != null )
						 {
							  monitor.AddRangeAtDepth( depth );
							  monitor.AddToCoveredArea( currentExtent.Area );
						 }
					}
					else
					{
						 long width = ( right - left ) / _quadFactor;
						 for ( int i = 0; i < _quadFactor; i++ )
						 {
							  int npoint = curve.NpointForIndex( i );

							  SearchEnvelope quadrant = currentExtent.Quadrant( BitValues( npoint ) );
							  AddTilesIntersectingEnvelopeAt( config, monitor, depth + 1, maxDepth, search, quadrant, curve.ChildAt( i ), left + i * width, left + ( i + 1 ) * width, results );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Bit index describing the in which quadrant an npoint corresponds to
		 /// </summary>
		 private int[] BitValues( int npoint )
		 {
			  int[] bitValues = new int[_nbrDim];

			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					int shift = _nbrDim - dim - 1;
					bitValues[dim] = ( npoint & ( 1 << shift ) ) >> shift;
			  }
			  return bitValues;
		 }

		 /// <summary>
		 /// Given a coordinate, find the corresponding normalized coordinate
		 /// </summary>
		 internal virtual long[] GetNormalizedCoord( double[] coord )
		 {
			  long[] normalizedCoord = new long[_nbrDim];

			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					double value = Clamp( coord[dim], _range.getMin( dim ), _range.getMax( dim ) );
					// Avoiding awkward rounding errors
					if ( value - _range.getMin( dim ) == _range.getMax( dim ) - _range.getMin( dim ) )
					{
						 normalizedCoord[dim] = _width - 1;
					}
					else
					{
						 /*
						  * We are converting a world coordinate in range [min,max) to a long-int coordinate in range [0,width).
						  * The fact that the origins are not aligned means we can get numerical rounding errors of points near the world origin, but far from
						  * the normalized origin, due to very high precision in doubles near 0.0, and much lower precision of doubles of values far from 0.0.
						  * The symptom of this is points very close to tile edges end up in the adjacent tiles instead.
						  * We fix this by first converting to normalized coordinates, and then using the new tile as a new origin,
						  * and re-converting based on that origin.
						  * This should lead to a number of 0, which means we're in the origin tile (no numerical rounding errors),
						  * but when an error occurs, we could have a tile offset of +1 or -1, and we move to the adjacent tile instead.
						  */
						 normalizedCoord[dim] = ( long )( ( value - _range.getMin( dim ) ) * _scalingFactor[dim] );
						 // Calculating with an origin at the min can lead to numerical rouding errors, which can be corrected by recalculating using a closer origin
						 double tileCenter = ( ( double ) normalizedCoord[dim] ) / _scalingFactor[dim] + _range.getMin( dim ) + GetTileWidth( dim, _maxLevel ) / 2.0;
						 // The 1E-16 is to create the behavior of the [min,max) bounds without an expensive if...else if...else check
						 long normalizedOffset = ( long )( ( value - tileCenter ) * _scalingFactor[dim] - 0.5 + 1E-16 );
						 // normalizedOffset is almost always 0, but can be +1 or -1 if there were rounding errors we need to correct for
						 normalizedCoord[dim] += normalizedOffset;
					}
			  }
			  return normalizedCoord;
		 }

		 /// <summary>
		 /// Given a normalized coordinate, find the center coordinate of that tile  on the given level
		 /// </summary>
		 private double[] GetDoubleCoord( long[] normalizedCoord, int level )
		 {
			  double[] coord = new double[_nbrDim];

			  for ( int dim = 0; dim < _nbrDim; dim++ )
			  {
					double coordinate = ( ( double ) normalizedCoord[dim] ) / _scalingFactor[dim] + _range.getMin( dim ) + GetTileWidth( dim, level ) / 2.0;
					coord[dim] = Clamp( coordinate, _range.getMin( dim ), _range.getMax( dim ) );
			  }
			  return coord;
		 }

		 private static double Clamp( double val, double min, double max )
		 {
			  if ( val <= min )
			  {
					return min;
			  }
			  if ( val >= max )
			  {
					return max;
			  }
			  return val;
		 }

		 /// <summary>
		 /// Assert that a given level is valid
		 /// </summary>
		 private void AssertValidLevel( int level )
		 {
			  if ( level > _maxLevel )
			  {
					throw new System.ArgumentException( "Level " + level + " greater than max-level " + _maxLevel );
			  }
		 }

		 /// <summary>
		 /// Class for ranges of tiles
		 /// </summary>
		 public class LongRange
		 {
			  public readonly long Min;
			  public long Max;

			  internal LongRange( long value ) : this( value, value )
			  {
			  }

			  internal LongRange( long min, long max )
			  {
					this.Min = min;
					this.Max = max;
			  }

			  internal virtual void ExpandToMax( long other )
			  {
					this.Max = other;
			  }

			  public override bool Equals( object other )
			  {
					return ( other is LongRange ) && this.Equals( ( LongRange ) other );
			  }

			  public virtual bool Equals( LongRange other )
			  {
					return this.Min == other.Min && this.Max == other.Max;
			  }

			  public override int GetHashCode()
			  {
					return ( int )( this.Min << 16 + this.Max );
			  }

			  public override string ToString()
			  {
					return "LongRange(" + Min + "," + Max + ")";
			  }
		 }
	}

}