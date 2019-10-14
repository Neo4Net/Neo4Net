using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Utility class with static method for extracting relevant spatial index configurations from <seealso cref="CoordinateReferenceSystem"/> and
	/// <seealso cref="SpaceFillingCurveSettings"/>. Configurations will be put into a map, prefixed by <seealso cref="SPATIAL_CONFIG_PREFIX"/> and
	/// <seealso cref="CoordinateReferenceSystem.getName()"/>.
	/// By using this class when extracting configurations we make sure that the same name and format is used for the same configuration.
	/// </summary>
	internal sealed class SpatialIndexConfig
	{
		 private const string SPATIAL_CONFIG_PREFIX = "spatial";

		 private SpatialIndexConfig()
		 {
		 }

		 /// <summary>
		 /// Extract spatial index configuration and put into provided map.
		 /// </summary>
		 /// <param name="map"> <seealso cref="System.Collections.IDictionary"/> into which extracted configurations should be inserted. </param>
		 /// <param name="crs"> <seealso cref="CoordinateReferenceSystem"/> from which to extract configurations. </param>
		 /// <param name="settings"> <seealso cref="SpaceFillingCurveSettings"/> from which to extract configurations. </param>
		 internal static void AddSpatialConfig( IDictionary<string, Value> map, CoordinateReferenceSystem crs, SpaceFillingCurveSettings settings )
		 {
			  string crsName = crs.Name;
			  int tableId = crs.Table.TableId;
			  int code = crs.Code;
			  int dimensions = settings.Dimensions;
			  int maxLevels = settings.MaxLevels;
			  double[] min = settings.IndexExtents().Min;
			  double[] max = settings.IndexExtents().Max;

			  string prefix = prefix( crsName );
			  map[prefix + ".tableId"] = Values.intValue( tableId );
			  map[prefix + ".code"] = Values.intValue( code );
			  map[prefix + ".dimensions"] = Values.intValue( dimensions );
			  map[prefix + ".maxLevels"] = Values.intValue( maxLevels );
			  map[prefix + ".min"] = Values.doubleArray( min );
			  map[prefix + ".max"] = Values.doubleArray( max );
		 }

		 private static string Prefix( string crsName )
		 {
			  return SPATIAL_CONFIG_PREFIX + "." + crsName;
		 }
	}

}