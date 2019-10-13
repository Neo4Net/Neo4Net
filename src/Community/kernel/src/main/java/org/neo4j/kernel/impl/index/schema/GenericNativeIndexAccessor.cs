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
	using Neo4Net.Index.@internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsWriter = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsWriter;
	using Neo4Net.Kernel.impl.util;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

	internal class GenericNativeIndexAccessor : NativeIndexAccessor<GenericKey, NativeIndexValue>
	{
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _spaceFillingCurveSettings;
		 private readonly SpaceFillingCurveConfiguration _configuration;
		 private readonly IndexDropAction _dropAction;
		 private Validator<Value[]> _validator;

		 internal GenericNativeIndexAccessor( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<GenericKey, NativeIndexValue> layout, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spaceFillingCurveSettings, SpaceFillingCurveConfiguration configuration, IndexDropAction dropAction, bool readOnly ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, new SpaceFillingCurveSettingsWriter( spaceFillingCurveSettings ), readOnly )
		 {
			  this._spaceFillingCurveSettings = spaceFillingCurveSettings;
			  this._configuration = configuration;
			  this._dropAction = dropAction;
			  instantiateTree( recoveryCleanupWorkCollector, HeaderWriter );
		 }

		 public override void Drop()
		 {
			  base.Drop();
			  _dropAction.drop( descriptor.Id, false );
		 }

		 protected internal override void AfterTreeInstantiation( GBPTree<GenericKey, NativeIndexValue> tree )
		 {
			  _validator = new GenericIndexKeyValidator( tree.KeyValueSizeCap(), layout );
		 }

		 public override IndexReader NewReader()
		 {
			  assertOpen();
			  return new GenericNativeIndexReader( tree, layout, descriptor, _spaceFillingCurveSettings, _configuration );
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  _validator.validate( tuple );
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  // This accessor needs to use the header writer here because coordinate reference systems may have changed since last checkpoint.
			  tree.checkpoint( ioLimiter, HeaderWriter );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  _spaceFillingCurveSettings.visitIndexSpecificSettings( new SpatialConfigExtractor( map ) );
			  return map;
		 }

	}

}