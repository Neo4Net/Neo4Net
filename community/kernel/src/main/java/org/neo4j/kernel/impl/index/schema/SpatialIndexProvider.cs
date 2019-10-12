using System;
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
	using MetadataMismatchException = Org.Neo4j.Index.@internal.gbptree.MetadataMismatchException;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Org.Neo4j.@internal.Kernel.Api.IndexCapability;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Org.Neo4j.@internal.Kernel.Api.IndexValueCapability;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsFactory = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using StoreMigrationParticipant = Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;

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
			  return Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
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
					return Org.Neo4j.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
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