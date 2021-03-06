﻿using System;
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
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using StoreMigrationParticipant = Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;

	public class TemporalIndexProvider : IndexProvider
	{
		 public const string KEY = "temporal";
		 internal static readonly IndexCapability Capability = new TemporalIndexCapability();
		 private static readonly IndexProviderDescriptor _temporalProviderDescriptor = new IndexProviderDescriptor( KEY, "1.0" );

		 private readonly PageCache _pageCache;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Monitor _monitor;
		 private readonly RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector;
		 private readonly bool _readOnly;

		 public TemporalIndexProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly ) : base( _temporalProviderDescriptor, directoryStructure )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._monitor = monitor;
			  this._recoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
			  this._readOnly = readOnly;
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  if ( _readOnly )
			  {
					throw new System.NotSupportedException( "Can't create populator for read only index" );
			  }
			  TemporalIndexFiles files = new TemporalIndexFiles( DirectoryStructure(), descriptor, _fs );
			  return new TemporalIndexPopulator( descriptor, samplingConfig, files, _pageCache, _fs, _monitor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  TemporalIndexFiles files = new TemporalIndexFiles( DirectoryStructure(), descriptor, _fs );
			  return new TemporalIndexAccessor( descriptor, samplingConfig, _pageCache, _fs, _recoveryCleanupWorkCollector, _monitor, files, _readOnly );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  TemporalIndexFiles temporalIndexFiles = new TemporalIndexFiles( DirectoryStructure(), descriptor, _fs );

			  try
			  {
					foreach ( TemporalIndexFiles.FileLayout subIndex in temporalIndexFiles.Existing() )
					{
						 string indexFailure = NativeIndexes.ReadFailureMessage( _pageCache, subIndex.indexFile );
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
			  TemporalIndexFiles temporalIndexFiles = new TemporalIndexFiles( DirectoryStructure(), descriptor, _fs );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Iterable<TemporalIndexFiles.FileLayout> existing = temporalIndexFiles.existing();
			  IEnumerable<TemporalIndexFiles.FileLayout> existing = temporalIndexFiles.Existing();
			  InternalIndexState state = InternalIndexState.ONLINE;
			  foreach ( TemporalIndexFiles.FileLayout subIndex in existing )
			  {
					try
					{
						 switch ( NativeIndexes.ReadState( _pageCache, subIndex.indexFile ) )
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
		 /// For single property temporal queries capabilities are
		 /// Order: ASCENDING
		 /// Value: YES (can provide exact value)
		 /// 
		 /// For other queries there is no support
		 /// </summary>
		 private class TemporalIndexCapability : IndexCapability
		 {
			  public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
			  {
					if ( Support( valueCategories ) )
					{
						 return Org.Neo4j.@internal.Kernel.Api.IndexCapability_Fields.OrderBoth;
					}
					return Org.Neo4j.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
			  }

			  public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
			  {
					if ( Support( valueCategories ) )
					{
						 return IndexValueCapability.YES;
					}
					if ( SingleWildcard( valueCategories ) )
					{
						 return IndexValueCapability.PARTIAL;
					}
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

			  internal virtual bool Support( ValueCategory[] valueCategories )
			  {
					return valueCategories.Length == 1 && valueCategories[0] == ValueCategory.TEMPORAL;
			  }
		 }
	}

}