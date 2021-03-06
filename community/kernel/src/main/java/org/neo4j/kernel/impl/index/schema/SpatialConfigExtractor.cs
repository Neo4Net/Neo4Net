﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal class SpatialConfigExtractor : IndexSpecificSpaceFillingCurveSettingsCache.SettingVisitor
	{
		 private readonly IDictionary<string, Value> _map;

		 internal SpatialConfigExtractor( IDictionary<string, Value> map )
		 {
			  this._map = map;
		 }

		 public override void Count( int count )
		 {
		 }

		 public override void Visit( CoordinateReferenceSystem crs, SpaceFillingCurveSettings settings )
		 {
			  SpatialIndexConfig.AddSpatialConfig( _map, crs, settings );
		 }
	}

}