using System.Collections.Generic;
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
namespace Org.Neo4j.Kernel.Impl.Api.index
{

	using Org.Neo4j.Graphdb;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using UpdateCountingIndexUpdater = Org.Neo4j.Kernel.Impl.Api.index.updater.UpdateCountingIndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class OnlineIndexProxy : IndexProxy
	{
		 private readonly long _indexId;
		 private readonly CapableIndexDescriptor _capableIndexDescriptor;
		 internal readonly IndexAccessor Accessor;
		 private readonly IndexStoreView _storeView;
		 private readonly IndexCountsRemover _indexCountsRemover;
		 private bool _started;

		 // About this flag: there are two online "modes", you might say...
		 // - One is the pure starting of an already online index which was cleanly shut down and all that.
		 //   This scenario is simple and doesn't need this idempotency mode.
		 // - The other is the creation or starting from an uncompleted population, where there will be a point
		 //   in the future where this index will flip from a populating index proxy to an online index proxy.
		 //   This is the problematic part. You see... we have been accidentally relying on the short-lived node
		 //   entity locks for this to work. The scenario where they have saved indexes from getting duplicate
		 //   nodes in them (one from populator and the other from a "normal" update is where a populator is nearing
		 //   its completion and wants to flip. Another thread is in the middle of applying a transaction which
		 //   in the end will feed an update to this index. Index updates are applied after store updates, so
		 //   the populator may see the created node and add it, index flips and then the updates comes in to the normal
		 //   online index and gets added again. The read lock here will have the populator wait for the transaction
		 //   to fully apply, e.g. also wait for the index update to reach the population job before adding that node
		 //   and flipping (the update mechanism in a populator is idempotent).
		 //     This strategy has changed slightly in 3.0 where transactions can be applied in whole batches
		 //   and index updates for the whole batch will be applied in the end. This is fine for everything except
		 //   the above scenario because the short-lived entity locks are per transaction, not per batch, and must
		 //   be so to not interfere with transactions creating constraints inside this batch. We do need to apply
		 //   index updates in batches because nowadays slave update pulling and application isn't special in any
		 //   way, it's simply applying transactions in batches and this needs to be very fast to not have instances
		 //   fall behind in a cluster.
		 //     So the sum of this is that during the session (until the next restart of the db) an index gets created
		 //   it will be in this forced idempotency mode where it applies additions idempotently, which may be
		 //   slightly more costly, but shouldn't make that big of a difference hopefully.
		 private readonly bool _forcedIdempotentMode;

		 internal OnlineIndexProxy( CapableIndexDescriptor capableIndexDescriptor, IndexAccessor accessor, IndexStoreView storeView, bool forcedIdempotentMode )
		 {
			  Debug.Assert( accessor != null );
			  this._indexId = capableIndexDescriptor.Id;
			  this._capableIndexDescriptor = capableIndexDescriptor;
			  this.Accessor = accessor;
			  this._storeView = storeView;
			  this._forcedIdempotentMode = forcedIdempotentMode;
			  this._indexCountsRemover = new IndexCountsRemover( storeView, _indexId );
		 }

		 public override void Start()
		 {
			  _started = true;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  IndexUpdater actual = Accessor.newUpdater( EscalateModeIfNecessary( mode ) );
			  return _started ? UpdateCountingUpdater( actual ) : actual;
		 }

		 private IndexUpdateMode EscalateModeIfNecessary( IndexUpdateMode mode )
		 {
			  if ( _forcedIdempotentMode )
			  {
					// If this proxy is flagged with taking extra care about idempotency then escalate ONLINE to ONLINE_IDEMPOTENT.
					if ( mode != IndexUpdateMode.Online )
					{
						 throw new System.ArgumentException( "Unexpected mode " + mode + " given that " + this + " has been marked with forced idempotent mode. Expected mode " + IndexUpdateMode.Online );
					}
					return IndexUpdateMode.OnlineIdempotent;
			  }
			  return mode;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexUpdater updateCountingUpdater(final org.neo4j.kernel.api.index.IndexUpdater indexUpdater)
		 private IndexUpdater UpdateCountingUpdater( IndexUpdater indexUpdater )
		 {
			  return new UpdateCountingIndexUpdater( _storeView, _indexId, indexUpdater );
		 }

		 public override void Drop()
		 {
			  _indexCountsRemover.remove();
			  Accessor.drop();
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  return _capableIndexDescriptor;
			 }
		 }

		 public virtual InternalIndexState State
		 {
			 get
			 {
				  return InternalIndexState.ONLINE;
			 }
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  Accessor.force( ioLimiter );
		 }

		 public override void Refresh()
		 {
			  Accessor.refresh();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Accessor.Dispose();
		 }

		 public override IndexReader NewReader()
		 {
			  return Accessor.newReader();
		 }

		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  return false; // the store scan is already completed
		 }

		 public override void Activate()
		 {
			  // ok, already active
		 }

		 public override void Validate()
		 {
			  // ok, it's online so it's valid
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  Accessor.validateBeforeCommit( tuple );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  throw new System.InvalidOperationException( this + " is ONLINE" );
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  return Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.Done;
			 }
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return Accessor.snapshotFiles();
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  return Accessor.indexConfig();
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[accessor:" + Accessor + ", descriptor:" + _capableIndexDescriptor + "]";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  Accessor.verifyDeferredConstraints( nodePropertyAccessor );
		 }
	}

}