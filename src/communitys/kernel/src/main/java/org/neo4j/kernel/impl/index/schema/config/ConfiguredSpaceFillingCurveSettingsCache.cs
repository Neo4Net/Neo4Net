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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	/// <summary>
	/// These settings affect the creation of the 2D (or 3D) to 1D mapper.
	/// Changing these will change the values of the 1D mapping, but this will not invalidate existing indexes. They store the settings used to create
	/// them, and will not use these settings at all. Changes will only affect future indexes made. In order to change existing indexes, you will need
	/// to drop and recreate any indexes you wish to affect.
	/// </summary>
	public class ConfiguredSpaceFillingCurveSettingsCache
	{
		 private readonly int _maxBits;
		 private readonly Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> _settings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();

		 public ConfiguredSpaceFillingCurveSettingsCache( Config config )
		 {
			  this._maxBits = config.Get( SpatialIndexSettings.SpaceFillingCurveMaxBits );
			  Dictionary<CoordinateReferenceSystem, EnvelopeSettings> env = EnvelopeSettings.EnvelopeSettingsFromConfig( config );
			  foreach ( KeyValuePair<CoordinateReferenceSystem, EnvelopeSettings> entry in env.SetOfKeyValuePairs() )
			  {
					CoordinateReferenceSystem crs = entry.Key;
					_settings[crs] = SpaceFillingCurveSettingsFactory.FromConfig( this._maxBits, entry.Value );
			  }
		 }

		 /// <summary>
		 /// The space filling curve is configured up front to cover a specific region of 2D (or 3D) space,
		 /// and the mapping tree is configured up front to have a specific maximum depth. These settings
		 /// are stored in an instance of SpaceFillingCurveSettings and are determined by the Coordinate
		 /// Reference System, and any neo4j.conf settings to override the CRS defaults.
		 /// </summary>
		 /// <returns> The default settings for the specified coordinate reference system </returns>
		 public virtual SpaceFillingCurveSettings ForCRS( CoordinateReferenceSystem crs )
		 {
			  if ( _settings.ContainsKey( crs ) )
			  {
					return _settings[crs];
			  }
			  else
			  {
					return SpaceFillingCurveSettingsFactory.FromConfig( _maxBits, new EnvelopeSettings( crs ) );
			  }
		 }
	}

}