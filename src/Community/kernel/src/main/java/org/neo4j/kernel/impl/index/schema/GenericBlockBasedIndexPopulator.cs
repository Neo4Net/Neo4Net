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

	using SpaceFillingCurveConfiguration = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

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