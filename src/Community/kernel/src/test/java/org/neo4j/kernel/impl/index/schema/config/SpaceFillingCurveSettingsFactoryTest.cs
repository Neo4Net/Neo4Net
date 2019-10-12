using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{
	using Test = org.junit.Test;

	using Envelope = Neo4Net.Gis.Spatial.Index.Envelope;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;

	public class SpaceFillingCurveSettingsFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDefaultSpaceFillingCurveSettingsForWGS84()
		 public virtual void ShouldGetDefaultSpaceFillingCurveSettingsForWGS84()
		 {
			  ShouldGetSettingsFor( Config.defaults(), CoordinateReferenceSystem.WGS84, 2, 60, new Envelope(-180, 180, -90, 90) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDefaultSpaceFillingCurveSettingsForWGS84_3D()
		 public virtual void ShouldGetDefaultSpaceFillingCurveSettingsForWGS84_3D()
		 {
			  ShouldGetSettingsFor( Config.defaults(), CoordinateReferenceSystem.WGS84_3D, 3, 60, new Envelope(new double[]{ -180, -90, -1000000 }, new double[]{ 180, 90, 1000000 }) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDefaultSpaceFillingCurveSettingsForCartesian()
		 public virtual void ShouldGetDefaultSpaceFillingCurveSettingsForCartesian()
		 {
			  ShouldGetSettingsFor( Config.defaults(), CoordinateReferenceSystem.Cartesian, 2, 60, new Envelope(-1000000, 1000000, -1000000, 1000000) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDefaultSpaceFillingCurveSettingsForCartesian_3D()
		 public virtual void ShouldGetDefaultSpaceFillingCurveSettingsForCartesian_3D()
		 {
			  ShouldGetSettingsFor( Config.defaults(), CoordinateReferenceSystem.Cartesian_3D, 3, 60, new Envelope(new double[]{ -1000000, -1000000, -1000000 }, new double[]{ 1000000, 1000000, 1000000 }) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetModifiedSpaceFillingCurveSettingsForWGS84()
		 public virtual void ShouldGetModifiedSpaceFillingCurveSettingsForWGS84()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.WGS84;
			  for ( int maxBits = 30; maxBits <= 60; maxBits += 10 )
			  {
					for ( int minx = -180; minx < 0; minx += 45 )
					{
						 for ( int miny = -180; miny < 0; miny += 45 )
						 {
							  for ( int width = 10; width < 90; width += 40 )
							  {
									for ( int height = 10; height < 90; height += 40 )
									{
										 ShouldGetCustomSettingsFor( crs, maxBits, new double[]{ minx, miny }, new double[]{ minx + width, miny + height } );
									}
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetModifiedSpaceFillingCurveSettingsForWGS84_3D()
		 public virtual void ShouldGetModifiedSpaceFillingCurveSettingsForWGS84_3D()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.WGS84_3D;
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -180, -90, -1000000 }, new double[]{ 180, 90, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -180, -90, -1000000 }, new double[]{ 180, 90, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ 0, -90, -1000000 }, new double[]{ 180, 0, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ 0, -90, -1000000 }, new double[]{ 180, 0, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -90, -45, -1000 }, new double[]{ 90, 45, 1000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -90, -90, -1000 }, new double[]{ 90, 45, 1000 } );
			  // invalid geographic limits should not affect settings or even the index, but will affect distance and bbox calculations
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -1000, -1000, -1000 }, new double[]{ 1000, 1000, 1000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -1000, -1000, -1000 }, new double[]{ 1000, 1000, 1000 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetModifiedSpaceFillingCurveSettingsForCartesian()
		 public virtual void ShouldGetModifiedSpaceFillingCurveSettingsForCartesian()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Cartesian;
			  for ( int maxBits = 30; maxBits <= 60; maxBits += 10 )
			  {
					for ( int minx = -1000000; minx < 0; minx += 200000 )
					{
						 for ( int miny = -1000000; miny < 0; miny += 2000000 )
						 {
							  for ( int width = 100000; width < 1000000; width += 200000 )
							  {
									for ( int height = 100000; height < 1000000; height += 200000 )
									{
										 ShouldGetCustomSettingsFor( crs, maxBits, new double[]{ minx, miny }, new double[]{ minx + width, miny + height } );
									}
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetModifiedSpaceFillingCurveSettingsForCartesian_3D()
		 public virtual void ShouldGetModifiedSpaceFillingCurveSettingsForCartesian_3D()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Cartesian_3D;
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -1000000, -1000000, -1000000 }, new double[]{ 1000000, 1000000, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -1000000, -1000000, -1000000 }, new double[]{ 1000000, 1000000, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ 0, -1000000, -1000000 }, new double[]{ 1000000, 0, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ 0, -1000000, -1000000 }, new double[]{ 1000000, 0, 1000000 } );
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -1000, -1000, -1000 }, new double[]{ 1000, 1000, 1000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -1000, -1000, -1000 }, new double[]{ 1000, 1000, 1000 } );
			  ShouldGetCustomSettingsFor( crs, 60, new double[]{ -1000000000, -1000000000, -1000000000 }, new double[]{ 1000000000, 1000000000, 1000000000 } );
			  ShouldGetCustomSettingsFor( crs, 30, new double[]{ -1000000000, -1000000000, -1000000000 }, new double[]{ 1000000000, 1000000000, 1000000000 } );
		 }

		 private void ShouldGetCustomSettingsFor( CoordinateReferenceSystem crs, int maxBits, double[] min, double[] max )
		 {
			  string crsPrefix = "unsupported.dbms.db.spatial.crs." + crs.Name;
			  Dictionary<string, string> settings = new Dictionary<string, string>();
			  settings["unsupported.dbms.index.spatial.curve.max_bits"] = Convert.ToString( maxBits );
			  for ( int i = 0; i < min.Length; i++ )
			  {
					char var = "xyz".ToCharArray()[i];
					settings[crsPrefix + "." + var + ".min"] = Convert.ToString( min[i] );
					settings[crsPrefix + "." + var + ".max"] = Convert.ToString( max[i] );
			  }
			  Config config = Config.defaults();
			  config.Augment( settings );
			  ShouldGetSettingsFor( config, crs, min.Length, maxBits, new Envelope( min, max ) );
		 }

		 private void ShouldGetSettingsFor( Config config, CoordinateReferenceSystem crs, int dimensions, int maxBits, Envelope envelope )
		 {
			  ConfiguredSpaceFillingCurveSettingsCache configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  SpaceFillingCurveSettings settings = configuredSettings.ForCRS( crs );
			  assertThat( "Expected " + dimensions + "D for " + crs.Name, settings.Dimensions, equalTo( dimensions ) );
			  int maxLevels = maxBits / dimensions;
			  assertThat( "Expected maxLevels=" + maxLevels + " for " + crs.Name, settings.MaxLevels, equalTo( maxLevels ) );
			  assertThat( "Should have normal geographic 2D extents", settings.IndexExtents(), equalTo(envelope) );
		 }
	}

}