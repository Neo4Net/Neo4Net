using System;

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
namespace Neo4Net.GraphAlgo.Utils
{
	using Neo4Net.GraphAlgo;
	using Node = Neo4Net.GraphDb.Node;

	public class GeoEstimateEvaluator : EstimateEvaluator<double>
	{
		 private const double EARTH_RADIUS = 6371 * 1000; // Meters

		 private Node _cachedGoal;
		 private double[] _cachedGoalCoordinates;
		 private readonly string _latitudePropertyKey;
		 private readonly string _longitudePropertyKey;

		 public GeoEstimateEvaluator( string latitudePropertyKey, string longitudePropertyKey )
		 {
			  this._latitudePropertyKey = latitudePropertyKey;
			  this._longitudePropertyKey = longitudePropertyKey;
		 }

		 public override double? GetCost( Node node, Node goal )
		 {
			  double[] nodeCoordinates = GetCoordinates( node );
			  if ( _cachedGoal == null || !_cachedGoal.Equals( goal ) )
			  {
					_cachedGoalCoordinates = GetCoordinates( goal );
					_cachedGoal = goal;
			  }
			  return Distance( nodeCoordinates[0], nodeCoordinates[1], _cachedGoalCoordinates[0], _cachedGoalCoordinates[1] );
		 }

		 private double[] GetCoordinates( Node node )
		 {
			  return new double[] { ( ( Number ) node.GetProperty( _latitudePropertyKey ) ).doubleValue(), ((Number) node.GetProperty(_longitudePropertyKey)).doubleValue() };
		 }

		 private double Distance( double latitude1, double longitude1, double latitude2, double longitude2 )
		 {
			  latitude1 = Math.toRadians( latitude1 );
			  longitude1 = Math.toRadians( longitude1 );
			  latitude2 = Math.toRadians( latitude2 );
			  longitude2 = Math.toRadians( longitude2 );
			  double cLa1 = Math.Cos( latitude1 );
			  double xA = EARTH_RADIUS * cLa1 * Math.Cos( longitude1 );
			  double yA = EARTH_RADIUS * cLa1 * Math.Sin( longitude1 );
			  double zA = EARTH_RADIUS * Math.Sin( latitude1 );
			  double cLa2 = Math.Cos( latitude2 );
			  double xB = EARTH_RADIUS * cLa2 * Math.Cos( longitude2 );
			  double yB = EARTH_RADIUS * cLa2 * Math.Sin( longitude2 );
			  double zB = EARTH_RADIUS * Math.Sin( latitude2 );
			  return Math.Sqrt( ( xA - xB ) * ( xA - xB ) + ( yA - yB ) * ( yA - yB ) + ( zA - zB ) * ( zA - zB ) );
		 }
	}

}