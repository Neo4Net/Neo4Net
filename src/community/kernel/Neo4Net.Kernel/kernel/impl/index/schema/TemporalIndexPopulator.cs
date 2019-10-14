using System.Collections.Generic;
using System.Diagnostics;

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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexSampler.combineSamples;

	internal class TemporalIndexPopulator : TemporalIndexCache<WorkSyncedNativeIndexPopulator<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard>>, IndexPopulator
	{
		 internal TemporalIndexPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, TemporalIndexFiles temporalIndexFiles, PageCache pageCache, FileSystemAbstraction fs, IndexProvider.Monitor monitor ) : base( new PartFactory( pageCache, fs, temporalIndexFiles, descriptor, samplingConfig, monitor ) )
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
						Select( ValueGroup.DATE );
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
		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.values.storable.ValueGroup,java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> batchMap = new java.util.HashMap<>();
			  IDictionary<ValueGroup, IList<IndexEntryUpdate<object>>> batchMap = new Dictionary<ValueGroup, IList<IndexEntryUpdate<object>>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					ValueGroup valueGroup = update.Values()[0].valueGroup();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch = batchMap.computeIfAbsent(valueGroup, k -> new java.util.ArrayList<>());
					IList<IndexEntryUpdate<object>> batch = batchMap.computeIfAbsent( valueGroup, k => new List<IndexEntryUpdate<object>>() );
					batch.Add( update );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<org.neo4j.values.storable.ValueGroup,java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> entry : batchMap.entrySet())
			  foreach ( KeyValuePair<ValueGroup, IList<IndexEntryUpdate<object>>> entry in batchMap.SetOfKeyValuePairs() )
			  {
					Select( entry.Key ).add( entry.Value );
			  }
		 }

		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  // No-op, uniqueness is checked for each update in add(IndexEntryUpdate)
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return new TemporalIndexPopulatingUpdater( this, accessor );
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
			  UncheckedSelect( values[0].ValueGroup() ).includeSample(update);
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

		 internal class PartPopulator<KEY> : NativeIndexPopulator<KEY, NativeIndexValue> where KEY : NativeIndexSingleValueKey<KEY>
		 {
			  internal PartPopulator( PageCache pageCache, FileSystemAbstraction fs, TemporalIndexFiles.FileLayout<KEY> fileLayout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor ) : base( pageCache, fs, fileLayout.IndexFile, fileLayout.Layout, monitor, descriptor, NO_HEADER_WRITER )
			  {
			  }

			  internal override NativeIndexReader<KEY, NativeIndexValue> NewReader()
			  {
					return new TemporalIndexPartReader<KEY, NativeIndexValue>( tree, layout, descriptor );
			  }
		 }

		 internal class PartFactory : TemporalIndexCache.Factory<WorkSyncedNativeIndexPopulator<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard>>
		 {
			  internal readonly PageCache PageCache;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly TemporalIndexFiles TemporalIndexFiles;
			  internal readonly StoreIndexDescriptor Descriptor;
			  internal readonly IndexSamplingConfig SamplingConfig;
			  internal readonly IndexProvider.Monitor Monitor;

			  internal PartFactory( PageCache pageCache, FileSystemAbstraction fs, TemporalIndexFiles temporalIndexFiles, StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, IndexProvider.Monitor monitor )
			  {
					this.PageCache = pageCache;
					this.Fs = fs;
					this.TemporalIndexFiles = temporalIndexFiles;
					this.Descriptor = descriptor;
					this.SamplingConfig = samplingConfig;
					this.Monitor = monitor;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newDate()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewDate()
			  {
					return Create( TemporalIndexFiles.date() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newLocalDateTime()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewLocalDateTime()
			  {
					return Create( TemporalIndexFiles.localDateTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newZonedDateTime()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewZonedDateTime()
			  {
					return Create( TemporalIndexFiles.zonedDateTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newLocalTime()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewLocalTime()
			  {
					return Create( TemporalIndexFiles.localTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newZonedTime()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewZonedTime()
			  {
					return Create( TemporalIndexFiles.zonedTime() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public WorkSyncedNativeIndexPopulator<?,?> newDuration()
			  public override WorkSyncedNativeIndexPopulator<object, ?> NewDuration()
			  {
					return Create( TemporalIndexFiles.duration() );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private <KEY extends NativeIndexSingleValueKey<KEY>> WorkSyncedNativeIndexPopulator<KEY,?> create(TemporalIndexFiles.FileLayout<KEY> fileLayout)
			  internal virtual WorkSyncedNativeIndexPopulator<KEY, ?> Create<KEY>( TemporalIndexFiles.FileLayout<KEY> fileLayout ) where KEY : NativeIndexSingleValueKey<KEY>
			  {
					PartPopulator<KEY> populator = new PartPopulator<KEY>( PageCache, Fs, fileLayout, Monitor, Descriptor );
					populator.create();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new WorkSyncedNativeIndexPopulator<>(populator);
					return new WorkSyncedNativeIndexPopulator<KEY, ?>( populator );
			  }
		 }
	}

}