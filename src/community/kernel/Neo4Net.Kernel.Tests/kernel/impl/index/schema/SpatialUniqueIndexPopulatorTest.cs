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

	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	public class SpatialUniqueIndexPopulatorTest : NativeIndexPopulatorTests.Unique<SpatialIndexKey, NativeIndexValue>
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
			  return new SpatialValueCreatorUtil( TestIndexDescriptorFactory.uniqueForLabel( 42, 666 ).withId( 0 ), ValueCreatorUtil.FRACTION_DUPLICATE_UNIQUE );
		 }

		 internal override IndexLayout<SpatialIndexKey, NativeIndexValue> CreateLayout()
		 {
			  return new SpatialLayout( _crs, _configuredSettings.forCRS( _crs ).curve() );
		 }

		 public override void AddShouldThrowOnDuplicateValues()
		 { // Spatial can not throw on duplicate values during population because it
			  // might throw for points that are in fact unique. Instead, uniqueness will
			  // be verified by ConstraintIndexCreator when population is finished.
		 }

		 public override void UpdaterShouldThrowOnDuplicateValues()
		 { // Spatial can not throw on duplicate values during population because it
			  // might throw for points that are in fact unique. Instead, uniqueness will
			  // be verified by ConstraintIndexCreator when population is finished.
		 }
	}

}