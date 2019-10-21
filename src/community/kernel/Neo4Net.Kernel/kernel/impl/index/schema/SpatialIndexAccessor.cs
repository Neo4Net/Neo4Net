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
	using Neo4Net.Graphdb;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexConfigProvider = Neo4Net.Kernel.Api.Index.IndexConfigProvider;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using Neo4Net.Kernel.Impl.Index.Schema.fusion;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.concatResourceIterators;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;

	internal class SpatialIndexAccessor : SpatialIndexCache<SpatialIndexAccessor.PartAccessor>, IndexAccessor
	{
		 private readonly StoreIndexDescriptor _descriptor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SpatialIndexAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.index.internal.gbptree.RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, org.neo4j.kernel.api.index.IndexProvider.Monitor monitor, SpatialIndexFiles spatialIndexFiles, org.neo4j.gis.spatial.index.curves.SpaceFillingCurveConfiguration searchConfiguration, boolean readOnly) throws java.io.IOException
		 internal SpatialIndexAccessor( StoreIndexDescriptor descriptor, PageCache pageCache, FileSystemAbstraction fs, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, SpatialIndexFiles spatialIndexFiles, SpaceFillingCurveConfiguration searchConfiguration, bool readOnly ) : base( new PartFactory( pageCache, fs, recoveryCleanupWorkCollector, monitor, descriptor, spatialIndexFiles, searchConfiguration, readOnly ) )
		 {
			  this._descriptor = descriptor;
			  spatialIndexFiles.LoadExistingIndexes( this );
		 }

		 public override void Drop()
		 {
			  forAll( NativeIndexAccessor.drop, this );
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return new SpatialIndexUpdater( this, mode );
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  foreach ( NativeIndexAccessor part in this )
			  {
					part.force( ioLimiter );
			  }
		 }

		 public override void Refresh()
		 {
			  // not required in this implementation
		 }

		 public override void Close()
		 {
			  closeInstantiateCloseLock();
			  forAll( NativeIndexAccessor.close, this );
		 }

		 public override IndexReader NewReader()
		 {
			  return new SpatialIndexReader( _descriptor, this );
		 }

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
			  List<BoundedIterable<long>> allEntriesReader = new List<BoundedIterable<long>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexAccessor<?,?> part : this)
			  foreach ( NativeIndexAccessor<object, ?> part in this )
			  {
					allEntriesReader.Add( part.NewAllEntriesReader() );
			  }

			  return new BoundedIterableAnonymousInnerClass( this, allEntriesReader );
		 }

		 private class BoundedIterableAnonymousInnerClass : BoundedIterable<long>
		 {
			 private readonly SpatialIndexAccessor _outerInstance;

			 private List<BoundedIterable<long>> _allEntriesReader;

			 public BoundedIterableAnonymousInnerClass( SpatialIndexAccessor outerInstance, List<BoundedIterable<long>> allEntriesReader )
			 {
				 this.outerInstance = outerInstance;
				 this._allEntriesReader = allEntriesReader;
			 }

			 public long maxCount()
			 {
				  long sum = 0L;
				  foreach ( BoundedIterable<long> part in _allEntriesReader )
				  {
						long partMaxCount = part.MaxCount();
						if ( partMaxCount == Neo4Net.Helpers.Collections.BoundedIterable_Fields.UNKNOWN_MAX_COUNT )
						{
							 return Neo4Net.Helpers.Collections.BoundedIterable_Fields.UNKNOWN_MAX_COUNT;
						}
						sum += partMaxCount;
				  }
				  return sum;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			 public void close()
			 {
				  forAll( BoundedIterable.close, _allEntriesReader );
			 }

			 public IEnumerator<long> iterator()
			 {
				  return ( new CombiningIterable<long>( _allEntriesReader ) ).GetEnumerator();
			 }
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  IList<ResourceIterator<File>> snapshotFiles = new List<ResourceIterator<File>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexAccessor<?,?> part : this)
			  foreach ( NativeIndexAccessor<object, ?> part in this )
			  {
					snapshotFiles.Add( part.SnapshotFiles() );
			  }
			  return concatResourceIterators( snapshotFiles.GetEnumerator() );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> indexConfig = new Dictionary<string, Value>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexAccessor<?,?> part : this)
			  foreach ( NativeIndexAccessor<object, ?> part in this )
			  {
					IndexConfigProvider.putAllNoOverwrite( indexConfig, part.indexConfig() );
			  }
			  return indexConfig;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexAccessor<?,?> part : this)
			  foreach ( NativeIndexAccessor<object, ?> part in this )
			  {
					part.VerifyDeferredConstraints( nodePropertyAccessor );
			  }
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return Iterators.stream( iterator() ).anyMatch(NativeIndexAccessor::isDirty);
			 }
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return FusionIndexBase.consistencyCheck( this, reporterFactory );
		 }

		 internal class PartAccessor : NativeIndexAccessor<SpatialIndexKey, NativeIndexValue>
		 {
			  internal readonly IndexLayout<SpatialIndexKey, NativeIndexValue> Layout;
			  internal readonly StoreIndexDescriptor Descriptor;
			  internal readonly SpaceFillingCurveConfiguration SearchConfiguration;
			  internal CoordinateReferenceSystem Crs;
			  internal SpaceFillingCurveSettings Settings;

			  internal PartAccessor( PageCache pageCache, FileSystemAbstraction fs, SpatialIndexFiles.SpatialFileLayout fileLayout, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, SpaceFillingCurveConfiguration searchConfiguration, bool readOnly ) : base( pageCache, fs, fileLayout.IndexFile, fileLayout.Layout, monitor, descriptor, NO_HEADER_WRITER, readOnly )
			  {
					this.Layout = fileLayout.Layout;
					this.Descriptor = descriptor;
					this.SearchConfiguration = searchConfiguration;
					this.Crs = fileLayout.SpatialFile.crs;
					this.Settings = fileLayout.Settings;
					instantiateTree( recoveryCleanupWorkCollector, HeaderWriter );
			  }

			  public override SpatialIndexPartReader<NativeIndexValue> NewReader()
			  {
					assertOpen();
					return new SpatialIndexPartReader<NativeIndexValue>( tree, Layout, Descriptor, SearchConfiguration );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
			  {
					SpatialVerifyDeferredConstraint.Verify( nodePropertyAccessor, Layout, tree, Descriptor );
					base.VerifyDeferredConstraints( nodePropertyAccessor );
			  }

			  public override IDictionary<string, Value> IndexConfig()
			  {
					IDictionary<string, Value> map = new Dictionary<string, Value>();
					SpatialIndexConfig.AddSpatialConfig( map, Crs, Settings );
					return map;
			  }
		 }

		 internal class PartFactory : IFactory<PartAccessor>
		 {
			  internal readonly PageCache PageCache;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector;
			  internal readonly IndexProvider.Monitor Monitor;
			  internal readonly StoreIndexDescriptor Descriptor;
			  internal readonly SpatialIndexFiles SpatialIndexFiles;
			  internal readonly SpaceFillingCurveConfiguration SearchConfiguration;
			  internal readonly bool ReadOnly;

			  internal PartFactory( PageCache pageCache, FileSystemAbstraction fs, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, SpatialIndexFiles spatialIndexFiles, SpaceFillingCurveConfiguration searchConfiguration, bool readOnly )
			  {
					this.PageCache = pageCache;
					this.Fs = fs;
					this.RecoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
					this.Monitor = monitor;
					this.Descriptor = descriptor;
					this.SpatialIndexFiles = spatialIndexFiles;
					this.SearchConfiguration = searchConfiguration;
					this.ReadOnly = readOnly;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PartAccessor newSpatial(org.neo4j.values.storable.CoordinateReferenceSystem crs) throws java.io.IOException
			  public override PartAccessor NewSpatial( CoordinateReferenceSystem crs )
			  {
					SpatialIndexFiles.SpatialFile spatialFile = SpatialIndexFiles.forCrs( crs );
					if ( !Fs.fileExists( spatialFile.IndexFile ) )
					{
						 SpatialIndexFiles.SpatialFileLayout fileLayout = spatialFile.LayoutForNewIndex;
						 CreateEmptyIndex( fileLayout );
						 return CreatePartAccessor( fileLayout );
					}
					else
					{
						 return CreatePartAccessor( spatialFile.GetLayoutForExistingIndex( PageCache ) );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PartAccessor createPartAccessor(SpatialIndexFiles.SpatialFileLayout fileLayout) throws java.io.IOException
			  internal virtual PartAccessor CreatePartAccessor( SpatialIndexFiles.SpatialFileLayout fileLayout )
			  {
					return new PartAccessor( PageCache, Fs, fileLayout, RecoveryCleanupWorkCollector, Monitor, Descriptor, SearchConfiguration, ReadOnly );
			  }

			  internal virtual void CreateEmptyIndex( SpatialIndexFiles.SpatialFileLayout fileLayout )
			  {
					IndexPopulator populator = new SpatialIndexPopulator.PartPopulator( PageCache, Fs, fileLayout, Monitor, Descriptor, SearchConfiguration );
					populator.Create();
					populator.Close( true );
			  }
		 }
	}

}