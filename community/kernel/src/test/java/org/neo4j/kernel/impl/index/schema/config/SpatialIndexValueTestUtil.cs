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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.config
{
	using SpaceFillingCurve = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using Org.Neo4j.Helpers.Collection;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class SpatialIndexValueTestUtil
	{
		 public static Pair<PointValue, PointValue> PointsWithSameValueOnSpaceFillingCurve( Config config )
		 {
			  ConfiguredSpaceFillingCurveSettingsCache configuredCache = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  SpaceFillingCurveSettings spaceFillingCurveSettings = configuredCache.ForCRS( CoordinateReferenceSystem.WGS84 );
			  SpaceFillingCurve curve = spaceFillingCurveSettings.Curve();
			  double[] origin = new double[] { 0.0, 0.0 };
			  long? spaceFillingCurveMapForOrigin = curve.DerivedValueFor( origin );
			  double[] centerPointForOriginTile = curve.CenterPointFor( spaceFillingCurveMapForOrigin.Value );
			  PointValue originValue = Values.pointValue( CoordinateReferenceSystem.WGS84, origin );
			  PointValue centerPointValue = Values.pointValue( CoordinateReferenceSystem.WGS84, centerPointForOriginTile );
			  assertThat( "need non equal points for this test", origin, not( equalTo( centerPointValue ) ) );
			  return Pair.of( originValue, centerPointValue );
		 }
	}

}