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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using Neo4Net.Graphdb;
	using Neo4Net.Helpers.Collection;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexConfigProvider = Neo4Net.Kernel.Api.Index.IndexConfigProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.concatResourceIterators;

	internal class FusionIndexAccessor : FusionIndexBase<IndexAccessor>, IndexAccessor
	{
		 private readonly StoreIndexDescriptor _descriptor;
		 private readonly IndexDropAction _dropAction;

		 internal FusionIndexAccessor( SlotSelector slotSelector, InstanceSelector<IndexAccessor> instanceSelector, StoreIndexDescriptor descriptor, IndexDropAction dropAction ) : base( slotSelector, instanceSelector )
		 {
			  this._descriptor = descriptor;
			  this._dropAction = dropAction;
		 }

		 public override void Drop()
		 {
			  InstanceSelector.forAll( IndexAccessor.drop );
			  _dropAction.drop( _descriptor.Id );
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  LazyInstanceSelector<IndexUpdater> updaterSelector = new LazyInstanceSelector<IndexUpdater>( slot => InstanceSelector.select( slot ).newUpdater( mode ) );
			  return new FusionIndexUpdater( SlotSelector, updaterSelector );
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  InstanceSelector.forAll( accessor => accessor.force( ioLimiter ) );
		 }

		 public override void Refresh()
		 {
			  InstanceSelector.forAll( IndexAccessor.refresh );
		 }

		 public override void Close()
		 {
			  InstanceSelector.close( IndexAccessor.close );
		 }

		 public override IndexReader NewReader()
		 {
			  LazyInstanceSelector<IndexReader> readerSelector = new LazyInstanceSelector<IndexReader>( slot => InstanceSelector.select( slot ).newReader() );
			  return new FusionIndexReader( SlotSelector, readerSelector, _descriptor );
		 }

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IEnumerable<BoundedIterable<long>> entries = InstanceSelector.transform( IndexAccessor::newAllEntriesReader );
			  return new BoundedIterableAnonymousInnerClass( this, entries );
		 }

		 private class BoundedIterableAnonymousInnerClass : BoundedIterable<long>
		 {
			 private readonly FusionIndexAccessor _outerInstance;

			 private IEnumerable<BoundedIterable<long>> _entries;

			 public BoundedIterableAnonymousInnerClass( FusionIndexAccessor outerInstance, IEnumerable<BoundedIterable<long>> entries )
			 {
				 this.outerInstance = outerInstance;
				 this._entries = entries;
			 }

			 public long maxCount()
			 {
				  long sum = 0;
				  foreach ( BoundedIterable entry in _entries )
				  {
						long maxCount = entry.maxCount();
						if ( maxCount == Neo4Net.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT )
						{
							 return Neo4Net.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT;
						}
						sum += maxCount;
				  }
				  return sum;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void close() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			 public override void close()
			 {
				  ForAll( BoundedIterable.close, _entries );
			 }

			 public IEnumerator<long> iterator()
			 {
				  return Iterables.concat( _entries ).GetEnumerator();
			 }
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return concatResourceIterators( InstanceSelector.transform( IndexAccessor::snapshotFiles ).GetEnumerator() );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> indexConfig = new Dictionary<string, Value>();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  InstanceSelector.transform( IndexAccessor::indexConfig ).forEach( source => IndexConfigProvider.putAllNoOverwrite( indexConfig, source ) );
			  return indexConfig;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					InstanceSelector.select( slot ).verifyDeferredConstraints( nodePropertyAccessor );
			  }
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return Iterables.stream( InstanceSelector.transform( IndexAccessor::isDirty ) ).anyMatch( bool?.booleanValue );
			 }
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  InstanceSelector.select( SlotSelector.selectSlot( tuple, GroupOf ) ).validateBeforeCommit( tuple );
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return FusionIndexBase.ConsistencyCheck( InstanceSelector.instances.Values, reporterFactory );
		 }
	}

}