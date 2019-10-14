using System;
using System.Text;

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
namespace Neo4Net.Gis.Spatial.Index
{

	public class Envelope
	{
		 internal const double MAXIMAL_ENVELOPE_SIDE_RATIO = 100_000;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly double[] MinConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly double[] MaxConflict;

		 /// <summary>
		 /// Copy constructor
		 /// </summary>
		 public Envelope( Envelope e ) : this( e.MinConflict, e.MaxConflict )
		 {
		 }

		 /// <summary>
		 /// General constructor for the n-dimensional case
		 /// </summary>
		 public Envelope( double[] min, double[] max )
		 {
			  this.MinConflict = min.Clone();
			  this.MaxConflict = max.Clone();
			  if ( !Valid )
			  {
					throw new System.ArgumentException( "Invalid envelope created " + ToString() );
			  }
		 }

		 /// <summary>
		 /// Special constructor for the 2D case
		 /// </summary>
		 public Envelope( double xmin, double xmax, double ymin, double ymax ) : this( new double[] { xmin, ymin }, new double[] { xmax, ymax } )
		 {
		 }

		 /// <returns> a copy of the envelope where the ratio of smallest to largest side is not more than 1:100 </returns>
		 public virtual Envelope WithSideRatioNotTooSmall()
		 {
			  double[] from = Arrays.copyOf( this.MinConflict, MinConflict.Length );
			  double[] to = Arrays.copyOf( this.MaxConflict, MaxConflict.Length );
			  double highestDiff = -double.MaxValue;
			  double[] diffs = new double[from.Length];
			  for ( int i = 0; i < from.Length; i++ )
			  {
					diffs[i] = to[i] - from[i];
					highestDiff = Math.Max( highestDiff, diffs[i] );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double mindiff = highestDiff / MAXIMAL_ENVELOPE_SIDE_RATIO;
			  double mindiff = highestDiff / MAXIMAL_ENVELOPE_SIDE_RATIO;
			  for ( int i = 0; i < from.Length; i++ )
			  {
					if ( diffs[i] < mindiff )
					{
						 to[i] = from[i] + mindiff;
					}
			  }
			  return new Envelope( from, to );
		 }

		 public virtual double[] Min
		 {
			 get
			 {
				  return MinConflict;
			 }
		 }

		 public virtual double[] Max
		 {
			 get
			 {
				  return MaxConflict;
			 }
		 }

		 public virtual double getMin( int dimension )
		 {
			  return MinConflict[dimension];
		 }

		 public virtual double getMax( int dimension )
		 {
			  return MaxConflict[dimension];
		 }

		 public virtual double MinX
		 {
			 get
			 {
				  return GetMin( 0 );
			 }
		 }

		 public virtual double MaxX
		 {
			 get
			 {
				  return GetMax( 0 );
			 }
		 }

		 public virtual double MinY
		 {
			 get
			 {
				  return GetMin( 1 );
			 }
		 }

		 public virtual double MaxY
		 {
			 get
			 {
				  return GetMax( 1 );
			 }
		 }

		 public virtual int Dimension
		 {
			 get
			 {
				  return MinConflict.Length;
			 }
		 }

		 /// <summary>
		 /// Note that this doesn't exclude the envelope boundary.
		 /// See JTS Envelope.
		 /// </summary>
		 public virtual bool Contains( Envelope other )
		 {
			  return Covers( other );
		 }

		 public virtual bool Covers( Envelope other )
		 {
			  bool covers = Dimension == other.Dimension;
			  for ( int i = 0; i < MinConflict.Length && covers; i++ )
			  {
					covers = other.MinConflict[i] >= MinConflict[i] && other.MaxConflict[i] <= MaxConflict[i];
			  }
			  return covers;
		 }

		 public virtual bool Intersects( Envelope other )
		 {
			  bool intersects = Dimension == other.Dimension;
			  for ( int i = 0; i < MinConflict.Length && intersects; i++ )
			  {
					intersects = other.MinConflict[i] <= MaxConflict[i] && other.MaxConflict[i] >= MinConflict[i];
			  }
			  return intersects;
		 }

		 public virtual void ExpandToInclude( Envelope other )
		 {
			  if ( Dimension != other.Dimension )
			  {
					throw new System.ArgumentException( "Cannot join Envelopes with different dimensions: " + this.Dimension + " != " + other.Dimension );
			  }
			  else
			  {
					for ( int i = 0; i < MinConflict.Length; i++ )
					{
						 if ( other.MinConflict[i] < MinConflict[i] )
						 {
							  MinConflict[i] = other.MinConflict[i];
						 }
						 if ( other.MaxConflict[i] > MaxConflict[i] )
						 {
							  MaxConflict[i] = other.MaxConflict[i];
						 }
					}
			  }
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj is Envelope )
			  {
					Envelope other = ( Envelope ) obj;
					if ( this.Dimension != other.Dimension )
					{
						 return false;
					}
					for ( int i = 0; i < Dimension; i++ )
					{
						 if ( this.MinConflict[i] != other.GetMin( i ) || this.MaxConflict[i] != other.GetMax( i ) )
						 {
							  return false;
						 }
					}
					return true;
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override int GetHashCode()
		 {
			  int result = 1;
			  foreach ( double element in MinConflict )
			  {
					long bits = System.BitConverter.DoubleToInt64Bits( element );
					result = 31 * result + ( int )( bits ^ ( ( long )( ( ulong )bits >> 32 ) ) );
			  }
			  foreach ( double element in MaxConflict )
			  {
					long bits = System.BitConverter.DoubleToInt64Bits( element );
					result = 31 * result + ( int )( bits ^ ( ( long )( ( ulong )bits >> 32 ) ) );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Return the distance between the two envelopes on one dimension. This can return negative values if the envelopes intersect on this dimension. </summary>
		 /// <returns> distance between envelopes </returns>
		 public virtual double Distance( Envelope other, int dimension )
		 {
			  if ( MinConflict[dimension] < other.MinConflict[dimension] )
			  {
					return other.MinConflict[dimension] - MaxConflict[dimension];
			  }
			  else
			  {
					return MinConflict[dimension] - other.MaxConflict[dimension];
			  }
		 }

		 /// <summary>
		 /// Find the pythagorean distance between two envelopes
		 /// </summary>
		 public virtual double Distance( Envelope other )
		 {
			  if ( Intersects( other ) )
			  {
					return 0;
			  }

			  double distance = 0.0;
			  for ( int i = 0; i < MinConflict.Length; i++ )
			  {
					double dist = distance( other, i );
					if ( dist > 0 )
					{
						 distance += dist * dist;
					}
			  }
			  return Math.Sqrt( distance );
		 }

		 /// <returns> getWidth(0) for special 2D case with the first dimension being x (width) </returns>
		 public virtual double Width
		 {
			 get
			 {
				  return GetWidth( 0 );
			 }
		 }

		 /// <summary>
		 /// Return the width of the envelope at the specified dimension </summary>
		 /// <returns> with of that dimension, ie. max[d] - min[d] </returns>
		 public virtual double getWidth( int dimension )
		 {
			  return MaxConflict[dimension] - MinConflict[dimension];
		 }

		 /// <summary>
		 /// Return the fractional widths of the envelope at all axes
		 /// </summary>
		 /// <param name="divisor"> the number of segments to divide by (a 2D envelope will be divided into quadrants using 2) </param>
		 /// <returns> double array of widths, ie. max[d] - min[d] </returns>
		 public virtual double[] GetWidths( int divisor )
		 {
			  double[] widths = Arrays.copyOf( MaxConflict, MaxConflict.Length );
			  for ( int d = 0; d < MaxConflict.Length; d++ )
			  {
					widths[d] -= MinConflict[d];
					widths[d] /= divisor;
			  }
			  return widths;
		 }

		 public virtual double Area
		 {
			 get
			 {
				  double area = 1.0;
				  for ( int i = 0; i < MinConflict.Length; i++ )
				  {
						area *= MaxConflict[i] - MinConflict[i];
				  }
				  return area;
			 }
		 }

		 public virtual double Overlap( Envelope other )
		 {
			  Envelope smallest = this.Area < other.Area ? this : other;
			  Envelope intersection = this.Intersection( other );
			  return intersection == null ? 0.0 : smallest.Point ? 1.0 : intersection.Area / smallest.Area;
		 }

		 public virtual bool Point
		 {
			 get
			 {
				  bool ans = true;
				  for ( int i = 0; i < MinConflict.Length && ans; i++ )
				  {
						ans = MinConflict[i] == MaxConflict[i];
				  }
				  return ans;
			 }
		 }

		 private bool Valid
		 {
			 get
			 {
				  bool valid = MinConflict != null && MaxConflict != null && MinConflict.Length == MaxConflict.Length;
				  for ( int i = 0; valid && i < MinConflict.Length; i++ )
				  {
						valid = MinConflict[i] <= MaxConflict[i];
				  }
				  return valid;
			 }
		 }

		 public override string ToString()
		 {
			  return "Envelope: min=" + MakeString( MinConflict ) + ", max=" + MakeString( MaxConflict );
		 }

		 private static string MakeString( double[] vals )
		 {
			  StringBuilder sb = new StringBuilder();
			  if ( vals == null )
			  {
					sb.Append( "null" );
			  }
			  else
			  {
					foreach ( double val in vals )
					{
						 if ( sb.Length > 0 )
						 {
							  sb.Append( "," );
						 }
						 else
						 {
							  sb.Append( "(" );
						 }
						 sb.Append( val );
					}
					if ( sb.Length > 0 )
					{
						 sb.Append( ")" );
					}
			  }
			  return sb.ToString();
		 }

		 public virtual Envelope Intersection( Envelope other )
		 {
			  if ( Dimension == other.Dimension )
			  {
					double[] iMin = new double[this.MinConflict.Length];
					double[] iMax = new double[this.MinConflict.Length];
					Arrays.fill( iMin, Double.NaN );
					Arrays.fill( iMax, Double.NaN );
					bool result = true;
					for ( int i = 0; i < MinConflict.Length; i++ )
					{
						 if ( other.MinConflict[i] <= this.MaxConflict[i] && other.MaxConflict[i] >= this.MinConflict[i] )
						 {
							  iMin[i] = Math.Max( this.MinConflict[i], other.MinConflict[i] );
							  iMax[i] = Math.Min( this.MaxConflict[i], other.MaxConflict[i] );
						 }
						 else
						 {
							  result = false;
						 }
					}
					return result ? new Envelope( iMin, iMax ) : null;
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot calculate intersection of Envelopes with different dimensions: " + this.Dimension + " != " + other.Dimension );
			  }
		 }
	}

}