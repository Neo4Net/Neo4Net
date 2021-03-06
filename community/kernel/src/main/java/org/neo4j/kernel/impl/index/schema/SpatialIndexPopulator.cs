﻿using System.Collections.Generic;
using System.Diagnostics;

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
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexConfigProvider = Org.Neo4j.Kernel.Api.Index.IndexConfigProvider;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using SpaceFillingCurveSettings = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexSampler.combineSamples;

	internal class SpatialIndexPopulator : SpatialIndexCache<WorkSyncedNativeIndexPopulator<SpatialIndexKey, NativeIndexValue>>, IndexPopulator
	{
		 internal SpatialIndexPopulator( StoreIndexDescriptor descriptor, SpatialIndexFiles spatialIndexFiles, PageCache pageCache, FileSystemAbstraction fs, IndexProvider.Monitor monitor, SpaceFillingCurveConfiguration configuration ) : base( new PartFactory( pageCache, fs, spatialIndexFiles, descriptor, monitor, configuration ) )
		 {
		 }

		 public override void Create()
		 {
			 lock ( this )
			 {
				  forAll( p => p.Actual.clear(), this );
      
				  // We must make sure to have at least one subindex:
				  // to be able to persist failure and to have the right state in the beginning
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( !this.GetEnumerator().hasNext() )
				  {
						Select( CoordinateReferenceSystem.WGS84 );
				  }
			 }
		 }

		 public override void Drop()
		 {
			 lock ( this )
			 {
				  forAll( IndexPopulator.drop, this );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Org.Neo4j.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.values.storable.CoordinateReferenceSystem,java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> batchMap = new java.util.HashMap<>();
			  IDictionary<CoordinateReferenceSystem, IList<IndexEntryUpdate<object>>> batchMap = new Dictionary<CoordinateReferenceSystem, IList<IndexEntryUpdate<object>>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					PointValue point = ( PointValue ) update.Values()[0];
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch = batchMap.computeIfAbsent(point.getCoordinateReferenceSystem(), k -> new java.util.ArrayList<>());
					IList<IndexEntryUpdate<object>> batch = batchMap.computeIfAbsent( point.CoordinateReferenceSystem, k => new List<IndexEntryUpdate<object>>() );
					batch.Add( update );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<org.neo4j.values.storable.CoordinateReferenceSystem,java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> entry : batchMap.entrySet())
			  foreach ( KeyValuePair<CoordinateReferenceSystem, IList<IndexEntryUpdate<object>>> entry in batchMap.SetOfKeyValuePairs() )
			  {
					IndexPopulator partPopulator = Select( entry.Key );
					partPopulator.Add( entry.Value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  foreach ( IndexPopulator part in this )
			  {
					part.VerifyDeferredConstraints( nodePropertyAccessor );
			  }
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return new SpatialIndexPopulatingUpdater( this, accessor );
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			 lock ( this )
			 {
				  CloseInstantiateCloseLock();
				  foreach ( IndexPopulator part in this )
				  {
						part.Close( populationCompletedSuccessfully );
				  }
			 }
		 }

		 public override void MarkAsFailed( string failure )
		 {
			 lock ( this )
			 {
				  foreach ( IndexPopulator part in this )
				  {
						part.MarkAsFailed( failure );
				  }
			 }
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  Value[] values = update.Values();
			  Debug.Assert( values.Length == 1 );
			  UncheckedSelect( ( ( PointValue ) values[0] ).CoordinateReferenceSystem ).includeSample( update );
		 }

		 public override IndexSample SampleResult()
		 {
			  IList<IndexSample> samples = new List<IndexSample>();
			  foreach ( IndexPopulator partPopulator in this )
			  {
					samples.Add( partPopulator.SampleResult() );
			  }
			  return combineSamples( samples );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> indexConfig = new Dictionary<string, Value>();
			  foreach ( IndexPopulator part in this )
			  {
					IndexConfigProvider.putAllNoOverwrite( indexConfig, part.IndexConfig() );
			  }
			  return indexConfig;
		 }

		 internal class PartPopulator : NativeIndexPopulator<SpatialIndexKey, NativeIndexValue>
		 {
			  internal readonly SpaceFillingCurveConfiguration Configuration;
			  internal readonly SpaceFillingCurveSettings Settings;
			  internal readonly CoordinateReferenceSystem Crs;

			  internal PartPopulator( PageCache pageCache, FileSystemAbstraction fs, SpatialIndexFiles.SpatialFileLayout fileLayout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, SpaceFillingCurveConfiguration configuration ) : base( pageCache, fs, fileLayout.IndexFile, fileLayout.Layout, monitor, descriptor, NO_HEADER_WRITER )
			  {
					this.Configuration = configuration;
					this.Settings = fileLayout.Settings;
					this.Crs = fileLayout.SpatialFile.crs;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
			  {
					SpatialVerifyDeferredConstraint.Verify( nodePropertyAccessor, layout, tree, descriptor );
					base.VerifyDeferredConstraints( nodePropertyAccessor );
			  }

			  internal override bool CanCheckConflictsWithoutStoreAccess()
			  {
					return false;
			  }

			  internal override ConflictDetectingValueMerger<SpatialIndexKey, NativeIndexValue, Value[]> MainConflictDetector
			  {
				  get
				  {
						// Because of lossy point representation in index we need to always compare on node id,
						// even for unique indexes. If we don't we risk throwing constraint violation exception
						// for points that are in fact unique.
						return new ThrowingConflictDetector<SpatialIndexKey, NativeIndexValue, Value[]>( true );
				  }
			  }

			  internal override NativeIndexReader<SpatialIndexKey, NativeIndexValue> NewReader()
			  {
					return new SpatialIndexPartReader<SpatialIndexKey, NativeIndexValue>( tree, layout, descriptor, Configuration );
			  }

			  public override void Create()
			  {
				  lock ( this )
				  {
						Create( Settings.headerWriter( BYTE_POPULATING ) );
				  }
			  }

			  internal override void MarkTreeAsOnline()
			  {
					tree.checkpoint( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited, Settings.headerWriter( BYTE_ONLINE ) );
			  }

			  public override IDictionary<string, Value> IndexConfig()
			  {
					IDictionary<string, Value> map = new Dictionary<string, Value>();
					SpatialIndexConfig.AddSpatialConfig( map, Crs, Settings );
					return map;
			  }
		 }

		 internal class PartFactory : Factory<WorkSyncedNativeIndexPopulator<SpatialIndexKey, NativeIndexValue>>
		 {
			  internal readonly PageCache PageCache;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly SpatialIndexFiles SpatialIndexFiles;
			  internal readonly StoreIndexDescriptor Descriptor;
			  internal readonly IndexProvider.Monitor Monitor;
			  internal readonly SpaceFillingCurveConfiguration Configuration;

			  internal PartFactory( PageCache pageCache, FileSystemAbstraction fs, SpatialIndexFiles spatialIndexFiles, StoreIndexDescriptor descriptor, IndexProvider.Monitor monitor, SpaceFillingCurveConfiguration configuration )
			  {
					this.PageCache = pageCache;
					this.Fs = fs;
					this.SpatialIndexFiles = spatialIndexFiles;
					this.Descriptor = descriptor;
					this.Monitor = monitor;
					this.Configuration = configuration;
			  }

			  public override WorkSyncedNativeIndexPopulator<SpatialIndexKey, NativeIndexValue> NewSpatial( CoordinateReferenceSystem crs )
			  {
					return Create( SpatialIndexFiles.forCrs( crs ).LayoutForNewIndex );
			  }

			  internal virtual WorkSyncedNativeIndexPopulator<SpatialIndexKey, NativeIndexValue> Create( SpatialIndexFiles.SpatialFileLayout fileLayout )
			  {
					PartPopulator populator = new PartPopulator( PageCache, Fs, fileLayout, Monitor, Descriptor, Configuration );
					populator.Create();
					return new WorkSyncedNativeIndexPopulator<SpatialIndexKey, NativeIndexValue>( populator );
			  }
		 }
	}

}