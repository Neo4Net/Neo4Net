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
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Index.Internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.Kernel.Api.Internal.IndexCapability;
	using IndexLimitation = Neo4Net.Kernel.Api.Internal.IndexLimitation;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexValueCapability = Neo4Net.Kernel.Api.Internal.IndexValueCapability;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using SpaceFillingCurveSettingsReader = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsReader;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.config.SpaceFillingCurveSettingsFactory.getConfiguredSpaceFillingCurveConfiguration;

	/// <summary>
	/// Native index able to handle all value types in a single <seealso cref="GBPTree"/>. Single-key as well as composite-key is supported.
	/// 
	/// A composite index query have one predicate per slot / column.
	/// The predicate comes in the form of an index query. Any of "exact", "range" or "exist".
	/// Other index providers have support for exact predicate on all columns or exists predicate on all columns (full scan).
	/// This index provider have some additional capabilities. It can combine the slot predicates under the following rules:
	/// a. Exact can only follow another Exact or be in first slot.
	/// b. Range can only follow Exact or be in first slot.
	/// 
	/// We use the following notation for the predicates:
	/// x: exact predicate
	/// -: exists predicate
	/// >: range predicate (this could be ranges with zero or one open end)
	/// 
	/// With an index on 5 slots as en example we can build several different composite queries:
	///     p1 p2 p3 p4 p5 (order is important)
	/// 1:  x  x  x  x  x
	/// 2:  -  -  -  -  -
	/// 3:  x  -  -  -  -
	/// 4:  x  x  x  x  -
	/// 5:  >  -  -  -  -
	/// 6:  x  >  -  -  -
	/// 7:  x  x  x  x  >
	/// 8:  >  x  -  -  - (not allowed!)
	/// 9:  >  >  -  -  - (not allowed!)
	/// 10: -  x  -  -  - (not allowed!)
	/// 11: -  >  -  -  - (not allowed!)
	/// 
	/// 1: Exact match on all slots. Supported by all index providers.
	/// 2: Exists scan on all slots. Supported by all index providers.
	/// 3: Exact match on first column and exists on the rest.
	/// 4: Exact match on all columns but the last.
	/// 5: Range on first column and exists on rest.
	/// 6: Exact on first, range on second and exists on rest.
	/// 7: Exact on all but last column. Range on last.
	/// 8: Not allowed because Exact can only follow another Exact.
	/// 9: Not allowed because range can only follow Exact.
	/// 10: Not allowed because Exact can only follow another Exact.
	/// 11: Not allowed because range can only follow Exact.
	/// 
	/// WHY?
	/// In short, we only allow "restrictive" predicates (exact or range) if they help us restrict the scan range.
	/// Let's take query 11 as example
	/// p1 p2 p3 p4 p5
	/// -  >  -  -  -
	/// Index is sorted first by p1, then p2, etc.
	/// Because we have a complete scan on p1 the range predicate on p2 can not restrict the range of the index we need to scan.
	/// We COULD allow this query and do filter during scan instead and take the extra cost into account when planning queries.
	/// As of writing this, there is no such filtering implementation.
	/// </summary>
	public class GenericNativeIndexProvider : NativeIndexProvider<GenericKey, NativeIndexValue, GenericLayout>
	{
		 public static readonly string Key = NATIVE_BTREE10.providerKey();
		 public static readonly IndexProviderDescriptor Descriptor = new IndexProviderDescriptor( Key, NATIVE_BTREE10.providerVersion() );
		 public static readonly IndexCapability Capability = new GenericIndexCapability();
		 public const string BLOCK_BASED_POPULATION_NAME = "blockBasedPopulation";
		 // todo turn OFF by default before releasing next patch. For now ON by default to test it.
		 private readonly bool _blockBasedPopulation = FeatureToggles.flag( typeof( GenericNativeIndexPopulator ), BLOCK_BASED_POPULATION_NAME, false );

		 /// <summary>
		 /// Cache of all setting for various specific CRS's found in the config at instantiation of this provider.
		 /// The config is read once and all relevant CRS configs cached here.
		 /// </summary>
		 private readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings;

		 /// <summary>
		 /// A space filling curve configuration used when reading spatial index values.
		 /// </summary>
		 private readonly SpaceFillingCurveConfiguration _configuration;
		 private readonly bool _archiveFailedIndex;
		 private readonly IndexDropAction _dropAction;

		 internal GenericNativeIndexProvider( IndexDirectoryStructure.Factory directoryStructureFactory, PageCache pageCache, FileSystemAbstraction fs, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly, Config config ) : base( Descriptor, directoryStructureFactory, pageCache, fs, monitor, recoveryCleanupWorkCollector, readOnly )
		 {

			  this._configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  this._configuration = getConfiguredSpaceFillingCurveConfiguration( config );
			  this._archiveFailedIndex = config.Get( GraphDatabaseSettings.archive_failed_index );
			  this._dropAction = new FileSystemIndexDropAction( fs, directoryStructure() );
		 }

		 internal override GenericLayout Layout( StoreIndexDescriptor descriptor, File storeFile )
		 {
			  try
			  {
					int numberOfSlots = descriptor.Properties().Length;
					IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> settings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
					if ( storeFile != null && Fs.fileExists( storeFile ) )
					{
						 // The index file exists and is sane so use it to read header information from.
						 GBPTree.readHeader( PageCache, storeFile, new NativeIndexHeaderReader( new SpaceFillingCurveSettingsReader( settings ) ) );
					}
					return new GenericLayout( numberOfSlots, new IndexSpecificSpaceFillingCurveSettingsCache( _configuredSettings, settings ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 protected internal override IndexPopulator NewIndexPopulator( File storeFile, GenericLayout layout, StoreIndexDescriptor descriptor, ByteBufferFactory bufferFactory )
		 {
			  if ( _blockBasedPopulation )
			  {
					return new GenericBlockBasedIndexPopulator( PageCache, Fs, storeFile, layout, Monitor, descriptor, layout.SpaceFillingCurveSettings, directoryStructure(), _configuration, _dropAction, _archiveFailedIndex, bufferFactory );
			  }
			  return new WorkSyncedNativeIndexPopulator<>( new GenericNativeIndexPopulator( PageCache, Fs, storeFile, layout, Monitor, descriptor, layout.SpaceFillingCurveSettings, directoryStructure(), _configuration, _dropAction, _archiveFailedIndex ) );
		 }

		 protected internal override IndexAccessor NewIndexAccessor( File storeFile, GenericLayout layout, StoreIndexDescriptor descriptor, bool readOnly )
		 {
			  return new GenericNativeIndexAccessor( PageCache, Fs, storeFile, layout, RecoveryCleanupWorkCollector, Monitor, descriptor, layout.SpaceFillingCurveSettings, _configuration, _dropAction, readOnly );
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return Capability;
		 }

		 private class GenericIndexCapability : IndexCapability
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IndexLimitation[] LimitationsConflict = new IndexLimitation[] { IndexLimitation.SLOW_CONTAINS };

			  public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
			  {
					if ( SupportOrdering( valueCategories ) )
					{
						 return Neo4Net.Kernel.Api.Internal.IndexCapability_Fields.OrderBoth;
					}
					return Neo4Net.Kernel.Api.Internal.IndexCapability_Fields.OrderNone;
			  }

			  public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
			  {
					return IndexValueCapability.YES;
			  }

			  public virtual bool FulltextIndex
			  {
				  get
				  {
						return false;
				  }
			  }

			  public virtual bool EventuallyConsistent
			  {
				  get
				  {
						return false;
				  }
			  }

			  internal virtual bool SupportOrdering( ValueCategory[] valueCategories )
			  {
					foreach ( ValueCategory valueCategory in valueCategories )
					{
						 if ( valueCategory == ValueCategory.GEOMETRY || valueCategory == ValueCategory.GEOMETRY_ARRAY || valueCategory == ValueCategory.UNKNOWN )
						 {
							  return false;
						 }
					}
					return true;
			  }

			  public override IndexLimitation[] Limitations()
			  {
					return LimitationsConflict;
			  }
		 }
	}

}