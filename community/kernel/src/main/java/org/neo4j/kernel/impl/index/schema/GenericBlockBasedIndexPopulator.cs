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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using SpaceFillingCurveConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal class GenericBlockBasedIndexPopulator : BlockBasedIndexPopulator<GenericKey, NativeIndexValue>
	{
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings;
		 private readonly SpaceFillingCurveConfiguration _configuration;

		 internal GenericBlockBasedIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File file, IndexLayout<GenericKey, NativeIndexValue> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings, IndexDirectoryStructure directoryStructure, SpaceFillingCurveConfiguration configuration, IndexDropAction dropAction, bool archiveFailedIndex, ByteBufferFactory bufferFactory ) : base( pageCache, fs, file, layout, monitor, descriptor, spatialSettings, directoryStructure, dropAction, archiveFailedIndex, bufferFactory )
		 {
			  this._spatialSettings = spatialSettings;
			  this._configuration = configuration;
		 }

		 internal override NativeIndexReader<GenericKey, NativeIndexValue> NewReader()
		 {
			  return new GenericNativeIndexReader( tree, layout, descriptor, _spatialSettings, _configuration );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  _spatialSettings.visitIndexSpecificSettings( new SpatialConfigExtractor( map ) );
			  return map;
		 }
	}

}