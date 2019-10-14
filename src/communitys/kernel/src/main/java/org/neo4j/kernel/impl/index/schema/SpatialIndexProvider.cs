using System;
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
	using MetadataMismatchException = Neo4Net.Index.@internal.gbptree.MetadataMismatchException;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Neo4Net.@internal.Kernel.Api.IndexValueCapability;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsFactory = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

	public class SpatialIndexProvider : IndexProvider
	{
		 public const string KEY = "spatial";
		 internal static readonly IndexCapability Capability = new SpatialIndexCapability();
		 private static readonly IndexProviderDescriptor _spatialProviderDescriptor = new IndexProviderDescriptor( KEY, "1.0" );

		 private readonly PageCache _pageCache;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Monitor _monitor;
		 private readonly RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector;
		 private readonly bool _readOnly;
		 private readonly SpaceFillingCurveConfiguration _configuration;
		 private readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings;

		 public SpatialIndexProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly, Config config ) : base( _spatialProviderDescriptor, directoryStructure )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._monitor = monitor;
			  this._recoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
			  this._readOnly = readOnly;
			  this._configuration = SpaceFillingCurveSettingsFactory.getConfiguredSpaceFillingCurveConfiguration( config );
			  this._configuredSettings = GetConfiguredSpaceFillingCurveSettings( config );
		 }

		 private ConfiguredSpaceFillingCurveSettingsCache GetConfiguredSpaceFillingCurveSettings( Config config )
		 {
			  return new ConfiguredSpaceFillingCurveSettingsCache( config );
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  if ( _readOnly )
			  {
					throw new System.NotSupportedException( "Can't create populator for read only index" );
			  }
			  SpatialIndexFiles files = new SpatialIndexFiles( DirectoryStructure(), descriptor.Id, _fs, _configuredSettings );
			  return new SpatialIndexPopulator( descriptor, files, _pageCache, _fs, _monitor, _configuration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  SpatialIndexFiles files = new SpatialIndexFiles( DirectoryStructure(), descriptor.Id, _fs, _configuredSettings );
			  return new SpatialIndexAccessor( descriptor, _pageCache, _fs, _recoveryCleanupWorkCollector, _monitor, files, _configuration, _readOnly );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  SpatialIndexFiles spatialIndexFiles = new SpatialIndexFiles( DirectoryStructure(), descriptor.Id, _fs, _configuredSettings );

			  try
			  {
					foreach ( SpatialIndexFiles.SpatialFile subIndex in spatialIndexFiles.Existing() )
					{
						 string indexFailure = NativeIndexes.ReadFailureMessage( _pageCache, subIndex.IndexFile );
						 if ( !string.ReferenceEquals( indexFailure, null ) )
						 {
							  return indexFailure;
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  throw new System.InvalidOperationException( "Index " + descriptor.Id + " isn't failed" );
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  SpatialIndexFiles spatialIndexFiles = new SpatialIndexFiles( DirectoryStructure(), descriptor.Id, _fs, _configuredSettings );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Iterable<SpatialIndexFiles.SpatialFile> existing = spatialIndexFiles.existing();
			  IEnumerable<SpatialIndexFiles.SpatialFile> existing = spatialIndexFiles.Existing();
			  InternalIndexState state = InternalIndexState.ONLINE;
			  foreach ( SpatialIndexFiles.SpatialFile subIndex in existing )
			  {
					try
					{
						 switch ( NativeIndexes.ReadState( _pageCache, subIndex.IndexFile ) )
						 {
						 case FAILED:
							  return InternalIndexState.FAILED;
						 case POPULATING:
							  state = InternalIndexState.POPULATING;
							 goto default;
						 default: // continue
					 break;
						 }
					}
					catch ( Exception e ) when ( e is MetadataMismatchException || e is IOException )
					{
						 _monitor.failedToOpenIndex( descriptor, "Requesting re-population.", e );
						 return InternalIndexState.POPULATING;
					}
			  }
			  return state;
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return Capability;
		 }

		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  // Since this native provider is a new one, there's no need for migration on this level.
			  // Migration should happen in the combined layer for the time being.
			  return Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
		 }

		 /// <summary>
		 /// For single property spatial queries capabilities are
		 /// Order: NONE (can not provide results in ordering)
		 /// Value: NO (can not provide exact value)
		 /// </summary>
		 private class SpatialIndexCapability : IndexCapability
		 {
			  public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
			  {
					return Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
			  }

			  public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
			  {
					return IndexValueCapability.NO;
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
		 }
	}

}