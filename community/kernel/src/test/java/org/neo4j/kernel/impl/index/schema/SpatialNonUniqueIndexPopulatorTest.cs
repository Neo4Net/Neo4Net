﻿/*
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

	using StandardConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.StandardConfiguration;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

	public class SpatialNonUniqueIndexPopulatorTest : NativeIndexPopulatorTests.NonUnique<SpatialIndexKey, NativeIndexValue>
	{
		 private static readonly CoordinateReferenceSystem _crs = CoordinateReferenceSystem.WGS84;
		 private static readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );

		 private SpatialIndexFiles.SpatialFile _spatialFile;

		 internal override NativeIndexPopulator<SpatialIndexKey, NativeIndexValue> CreatePopulator()
		 {
			  _spatialFile = new SpatialIndexFiles.SpatialFile( _crs, _configuredSettings, base.IndexFile );
			  return new SpatialIndexPopulator.PartPopulator( pageCache, fs, _spatialFile.LayoutForNewIndex, monitor, indexDescriptor, new StandardConfiguration() );
		 }

		 public override File IndexFile
		 {
			 get
			 {
				  return _spatialFile.indexFile;
			 }
		 }

		 protected internal override ValueCreatorUtil<SpatialIndexKey, NativeIndexValue> CreateValueCreatorUtil()
		 {
			  return new SpatialValueCreatorUtil( TestIndexDescriptorFactory.forLabel( 42, 666 ).withId( 0 ), ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 internal override IndexLayout<SpatialIndexKey, NativeIndexValue> CreateLayout()
		 {
			  return new SpatialLayout( _crs, _configuredSettings.forCRS( _crs ).curve() );
		 }
	}

}