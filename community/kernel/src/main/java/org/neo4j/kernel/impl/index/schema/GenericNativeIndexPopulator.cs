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

	using SpaceFillingCurveConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsWriter = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsWriter;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexes.deleteIndex;

	public class GenericNativeIndexPopulator : NativeIndexPopulator<GenericKey, NativeIndexValue>
	{
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings;
		 private readonly IndexDirectoryStructure _directoryStructure;
		 private readonly SpaceFillingCurveConfiguration _configuration;
		 private readonly IndexDropAction _dropAction;
		 private readonly bool _archiveFailedIndex;

		 internal GenericNativeIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<GenericKey, NativeIndexValue> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings, IndexDirectoryStructure directoryStructure, SpaceFillingCurveConfiguration configuration, IndexDropAction dropAction, bool archiveFailedIndex ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, new SpaceFillingCurveSettingsWriter( spatialSettings ) )
		 {
			  this._spatialSettings = spatialSettings;
			  this._directoryStructure = directoryStructure;
			  this._configuration = configuration;
			  this._dropAction = dropAction;
			  this._archiveFailedIndex = archiveFailedIndex;
		 }

		 public override void Create()
		 {
			  try
			  {
					// Archive and delete the index, if it exists. The reason why this isn't done in the generic implementation is that for all other cases a
					// native index populator lives under a fusion umbrella and the archive function sits on the top-level fusion folder, not every single sub-folder.
					deleteIndex( fileSystem, _directoryStructure, descriptor.Id, _archiveFailedIndex );

					// Now move on to do the actual creation.
					base.Create();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Drop()
		 {
			 lock ( this )
			 {
				  // Close resources
				  base.Drop();
				  // Cleanup directory
				  _dropAction.drop( descriptor.Id, _archiveFailedIndex );
			 }
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