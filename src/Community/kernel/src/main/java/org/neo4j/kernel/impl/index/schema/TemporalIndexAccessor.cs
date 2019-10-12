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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Graphdb;
	using Neo4Net.Helpers.Collection;
	using Neo4Net.Helpers.Collection;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using Neo4Net.Kernel.Impl.Index.Schema.fusion;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.concatResourceIterators;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;

	internal class TemporalIndexAccessor : TemporalIndexCache<TemporalIndexAccessor.PartAccessor<JavaToDotNetGenericWildcard>>, IndexAccessor
	{
		 private readonly IndexDescriptor _descriptor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TemporalIndexAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.index.internal.gbptree.RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, org.neo4j.kernel.api.index.IndexProvider.Monitor monitor, TemporalIndexFiles temporalIndexFiles, boolean readOnly) throws java.io.IOException
		 internal TemporalIndexAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, PageCache pageCache, FileSystemAbstraction fs, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, TemporalIndexFiles temporalIndexFiles, bool readOnly ) : base( new PartFactory( pageCache, fs, recoveryCleanupWorkCollector, monitor, descriptor, samplingConfig, temporalIndexFiles, readOnly ) )
		 {
			  this._descriptor = descriptor;

			  temporalIndexFiles.LoadExistingIndexes( this );
		 }

		 public override void Drop()
		 {
			  forAll( NativeIndexAccessor.drop, this );
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return new TemporalIndexUpdater( this, mode );
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
			  return new TemporalIndexReader( _descriptor, this );
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
			 private readonly TemporalIndexAccessor _outerInstance;

			 private List<BoundedIterable<long>> _allEntriesReader;

			 public BoundedIterableAnonymousInnerClass( TemporalIndexAccessor outerInstance, List<BoundedIterable<long>> allEntriesReader )
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
						if ( partMaxCount == Neo4Net.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT )
						{
							 return Neo4Net.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT;
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

		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  // Not needed since uniqueness is verified automatically w/o cost for every update.
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

		 internal class PartAccessor<KEY> : NativeIndexAccessor<KEY, NativeIndexValue> where KEY : NativeIndexSingleValueKey<KEY>
		 {
			  internal readonly IndexLayout<KEY, NativeIndexValue> Layout;
			  internal readonly IndexDescriptor Descriptor;

			  internal PartAccessor( PageCache pageCache, FileSystemAbstraction fs, TemporalIndexFiles.FileLayout<KEY> fileLayout, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, bool readOnly ) : base( pageCache, fs, fileLayout.IndexFile, fileLayout.Layout, monitor, descriptor, NO_HEADER_WRITER, readOnly )
			  {
					this.Layout = fileLayout.Layout;
					this.Descriptor = descriptor;
					instantiateTree( recoveryCleanupWorkCollector, headerWriter );
			  }

			  public override TemporalIndexPartReader<KEY> NewReader()
			  {
					assertOpen();
					return new TemporalIndexPartReader<KEY>( tree, Layout, Descriptor );
			  }
		 }

		 internal class PartFactory : TemporalIndexCache.Factory<PartAccessor<JavaToDotNetGenericWildcard>>
		 {
			  internal readonly PageCache PageCache;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector;
			  internal readonly IndexProvider.Monitor Monitor;
			  internal readonly StoreIndexDescriptor Descriptor;
			  internal readonly IndexSamplingConfig SamplingConfig;
			  internal readonly TemporalIndexFiles TemporalIndexFiles;
			  internal readonly bool ReadOnly;

			  internal PartFactory( PageCache pageCache, FileSystemAbstraction fs, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, TemporalIndexFiles temporalIndexFiles, bool readOnly )
			  {
					this.PageCache = pageCache;
					this.Fs = fs;
					this.RecoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
					this.Monitor = monitor;
					this.Descriptor = descriptor;
					this.SamplingConfig = samplingConfig;
					this.TemporalIndexFiles = temporalIndexFiles;
					this.ReadOnly = readOnly;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newDate()
			  public override PartAccessor<object> NewDate()
			  {
					return CreatePartAccessor( TemporalIndexFiles.date() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newLocalDateTime()
			  public override PartAccessor<object> NewLocalDateTime()
			  {
					return CreatePartAccessor( TemporalIndexFiles.localDateTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newZonedDateTime()
			  public override PartAccessor<object> NewZonedDateTime()
			  {
					return CreatePartAccessor( TemporalIndexFiles.zonedDateTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newLocalTime()
			  public override PartAccessor<object> NewLocalTime()
			  {
					return CreatePartAccessor( TemporalIndexFiles.localTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newZonedTime()
			  public override PartAccessor<object> NewZonedTime()
			  {
					return CreatePartAccessor( TemporalIndexFiles.zonedTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public PartAccessor<?> newDuration()
			  public override PartAccessor<object> NewDuration()
			  {
					return CreatePartAccessor( TemporalIndexFiles.duration() );
			  }

			  internal virtual PartAccessor<KEY> CreatePartAccessor<KEY>( TemporalIndexFiles.FileLayout<KEY> fileLayout ) where KEY : NativeIndexSingleValueKey<KEY>
			  {
					if ( !Fs.fileExists( fileLayout.IndexFile ) )
					{
						 CreateEmptyIndex( fileLayout );
					}
					return new PartAccessor<KEY>( PageCache, Fs, fileLayout, RecoveryCleanupWorkCollector, Monitor, Descriptor, ReadOnly );
			  }

			  internal virtual void CreateEmptyIndex<KEY>( TemporalIndexFiles.FileLayout<KEY> fileLayout ) where KEY : NativeIndexSingleValueKey<KEY>
			  {
					IndexPopulator populator = new TemporalIndexPopulator.PartPopulator<>( PageCache, Fs, fileLayout, Monitor, Descriptor );
					populator.Create();
					populator.Close( true );
			  }
		 }
	}

}