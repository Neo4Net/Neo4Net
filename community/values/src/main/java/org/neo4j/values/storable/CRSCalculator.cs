using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Values.Storable
{

	using Org.Neo4j.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.asin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.atan2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.cos;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.pow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.sin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.sqrt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toDegrees;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toRadians;

	public abstract class CRSCalculator
	{
		 public abstract double Distance( PointValue p1, PointValue p2 );

		 public abstract IList<Pair<PointValue, PointValue>> BoundingBox( PointValue center, double distance );

		 protected internal static double Pythagoras( double[] a, double[] b )
		 {
			  double sqrSum = 0.0;
			  for ( int i = 0; i < a.Length; i++ )
			  {
					double diff = a[i] - b[i];
					sqrSum += diff * diff;
			  }
			  return sqrt( sqrSum );
		 }

		 public class CartesianCalculator : CRSCalculator
		 {
			  internal int Dimension;

			  internal CartesianCalculator( int dimension )
			  {
					this.Dimension = dimension;
			  }

			  public override double Distance( PointValue p1, PointValue p2 )
			  {
					Debug.Assert( p1.CoordinateReferenceSystem.Dimension == Dimension );
					Debug.Assert( p2.CoordinateReferenceSystem.Dimension == Dimension );
					return Pythagoras( p1.Coordinate(), p2.Coordinate() );
			  }

			  public override IList<Pair<PointValue, PointValue>> BoundingBox( PointValue center, double distance )
			  {
					Debug.Assert( center.CoordinateReferenceSystem.Dimension == Dimension );
					double[] coordinates = center.Coordinate();
					double[] min = new double[Dimension];
					double[] max = new double[Dimension];
					for ( int i = 0; i < Dimension; i++ )
					{
						 min[i] = coordinates[i] - distance;
						 max[i] = coordinates[i] + distance;
					}
					CoordinateReferenceSystem crs = center.CoordinateReferenceSystem;
					return Collections.singletonList( Pair.of( Values.PointValue( crs, min ), Values.PointValue( crs, max ) ) );
			  }
		 }

		 public class GeographicCalculator : CRSCalculator
		 {
			  public const double EARTH_RADIUS_METERS = 6378140.0;
			  internal const double EXTENSION_FACTOR = 1.0001;
			  internal int Dimension;

			  internal GeographicCalculator( int dimension )
			  {
					this.Dimension = dimension;
			  }

			  public override double Distance( PointValue p1, PointValue p2 )
			  {
					Debug.Assert( p1.CoordinateReferenceSystem.Dimension == Dimension );
					Debug.Assert( p2.CoordinateReferenceSystem.Dimension == Dimension );
					double[] c1Coord = p1.Coordinate();
					double[] c2Coord = p2.Coordinate();
					double[] c1 = new double[]{ toRadians( c1Coord[0] ), toRadians( c1Coord[1] ) };
					double[] c2 = new double[]{ toRadians( c2Coord[0] ), toRadians( c2Coord[1] ) };
					double dx = c2[0] - c1[0];
					double dy = c2[1] - c1[1];
					double alpha = pow( sin( dy / 2 ), 2.0 ) + cos( c1[1] ) * cos( c2[1] ) * pow( sin( dx / 2.0 ), 2.0 );
					double greatCircleDistance = 2.0 * atan2( sqrt( alpha ), sqrt( 1 - alpha ) );
					if ( Dimension == 2 )
					{
						 return EARTH_RADIUS_METERS * greatCircleDistance;
					}
					else if ( Dimension == 3 )
					{
						 // get average height
						 double avgHeight = ( p1.Coordinate()[2] + p2.Coordinate()[2] ) / 2;
						 double distance2D = ( EARTH_RADIUS_METERS + avgHeight ) * greatCircleDistance;

						 double[] a = new double[Dimension - 1];
						 double[] b = new double[Dimension - 1];
						 a[0] = distance2D;
						 b[0] = 0.0;
						 for ( int i = 1; i < Dimension - 1; i++ )
						 {
							  a[i] = 0.0;
							  b[i] = c1Coord[i + 1] - c2Coord[i + 1];
						 }
						 return Pythagoras( a, b );
					}
					else
					{
						 // The above calculation works for more than 3D if all higher dimensions are orthogonal to the 3rd dimension.
						 // This might not be true in the general case, and so until we genuinely support higher dimensions fullstack
						 // we will explicitly disabled them here for now.
						 throw new System.NotSupportedException( "More than 3 dimensions are not supported for distance calculations." );
					}
			  }

			  public override IList<Pair<PointValue, PointValue>> BoundingBox( PointValue center, double distance )
			  {
					if ( distance == 0.0 )
					{
						 return Collections.singletonList( Pair.of( center, center ) );
					}

					// Extend the distance slightly to assure that all relevant points lies inside the bounding box,
					// with rounding errors taken into account
					double extendedDistance = distance * EXTENSION_FACTOR;

					CoordinateReferenceSystem crs = center.CoordinateReferenceSystem;
					double lat = center.Coordinate()[1];
					double lon = center.Coordinate()[0];

					double r = extendedDistance / EARTH_RADIUS_METERS;

					double latMin = lat - toDegrees( r );
					double latMax = lat + toDegrees( r );

					// If your query circle includes one of the poles
					if ( latMax >= 90 )
					{
						 return Collections.singletonList( BoundingBoxOf( -180, 180, latMin, 90, center, distance ) );
					}
					else if ( latMin <= -90 )
					{
						 return Collections.singletonList( BoundingBoxOf( -180, 180, -90, latMax, center, distance ) );
					}
					else
					{
						 double deltaLon = toDegrees( asin( sin( r ) / cos( toRadians( lat ) ) ) );
						 double lonMin = lon - deltaLon;
						 double lonMax = lon + deltaLon;

						 // If you query circle wraps around the dateline
						 if ( lonMin < -180 && lonMax > 180 )
						 {
							  // Large rectangle covering all longitudes
							  return Collections.singletonList( BoundingBoxOf( -180, 180, latMin, latMax, center, distance ) );
						 }
						 else if ( lonMin < -180 )
						 {
							  // two small rectangles east and west of dateline
							  Pair<PointValue, PointValue> box1 = BoundingBoxOf( lonMin + 360, 180, latMin, latMax, center, distance );
							  Pair<PointValue, PointValue> box2 = BoundingBoxOf( -180, lonMax, latMin, latMax, center, distance );
							  return Arrays.asList( box1, box2 );
						 }
						 else if ( lonMax > 180 )
						 {
							  // two small rectangles east and west of dateline
							  Pair<PointValue, PointValue> box1 = BoundingBoxOf( lonMin, 180, latMin, latMax, center, distance );
							  Pair<PointValue, PointValue> box2 = BoundingBoxOf( -180, lonMax - 360, latMin, latMax, center, distance );
							  return Arrays.asList( box1, box2 );
						 }
						 else
						 {
							  return Collections.singletonList( BoundingBoxOf( lonMin, lonMax, latMin, latMax, center, distance ) );
						 }
					}
			  }

			  internal virtual Pair<PointValue, PointValue> BoundingBoxOf( double minLon, double maxLon, double minLat, double maxLat, PointValue center, double distance )
			  {
					CoordinateReferenceSystem crs = center.CoordinateReferenceSystem;
					int dimension = center.CoordinateReferenceSystem.Dimension;
					double[] min = new double[dimension];
					double[] max = new double[dimension];
					min[0] = minLon;
					min[1] = minLat;
					max[0] = maxLon;
					max[1] = maxLat;
					if ( dimension > 2 )
					{
						 double[] coordinates = center.Coordinate();
						 for ( int i = 2; i < dimension; i++ )
						 {
							  min[i] = coordinates[i] - distance;
							  max[i] = coordinates[i] + distance;
						 }
					}
					return Pair.of( Values.PointValue( crs, min ), Values.PointValue( crs, max ) );
			  }
		 }
	}

}