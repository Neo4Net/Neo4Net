using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.state.storeview
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using IOUtils = Org.Neo4j.Io.IOUtils;
	using Org.Neo4j.Kernel.Api.Index;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using MultipleIndexPopulator = Org.Neo4j.Kernel.Impl.Api.index.MultipleIndexPopulator;
	using PhaseTracker = Org.Neo4j.Kernel.Impl.Api.index.PhaseTracker;
	using Org.Neo4j.Kernel.Impl.Api.index;
	using Lock = Org.Neo4j.Kernel.impl.locking.Lock;
	using StorageEntityScanCursor = Org.Neo4j.Storageengine.Api.StorageEntityScanCursor;
	using StoragePropertyCursor = Org.Neo4j.Storageengine.Api.StoragePropertyCursor;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using Value = Org.Neo4j.Values.Storable.Value;

	public abstract class PropertyAwareEntityStoreScan<CURSOR, FAILURE> : StoreScan<FAILURE> where CURSOR : Org.Neo4j.Storageengine.Api.StorageEntityScanCursor where FAILURE : Exception
	{
		 internal readonly CURSOR EntityCursor;
		 private readonly StoragePropertyCursor _propertyCursor;
		 private readonly StorageReader _storageReader;
		 private volatile bool _continueScanning;
		 private long _count;
		 private long _totalCount;
		 private readonly System.Func<int, bool> _propertyKeyIdFilter;
		 private readonly System.Func<long, Lock> _lockFunction;
		 private PhaseTracker _phaseTracker;

		 protected internal PropertyAwareEntityStoreScan( StorageReader storageReader, long totalEntityCount, System.Func<int, bool> propertyKeyIdFilter, System.Func<long, Lock> lockFunction )
		 {
			  this._storageReader = storageReader;
			  this.EntityCursor = AllocateCursor( storageReader );
			  this._propertyCursor = storageReader.AllocatePropertyCursor();
			  this._propertyKeyIdFilter = propertyKeyIdFilter;
			  this._lockFunction = lockFunction;
			  this._totalCount = totalEntityCount;
			  this._phaseTracker = Org.Neo4j.Kernel.Impl.Api.index.PhaseTracker_Fields.NullInstance;
		 }

		 protected internal abstract CURSOR AllocateCursor( StorageReader storageReader );

		 internal static bool ContainsAnyEntityToken( int[] entityTokenFilter, params long[] entityTokens )
		 {
			  foreach ( long candidate in entityTokens )
			  {
					if ( ArrayUtils.contains( entityTokenFilter, Math.toIntExact( candidate ) ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 internal virtual bool HasRelevantProperty( CURSOR cursor, EntityUpdates.Builder updates )
		 {
			  if ( !cursor.hasProperties() )
			  {
					return false;
			  }
			  bool hasRelevantProperty = false;
			  _propertyCursor.init( cursor.propertiesReference() );
			  while ( _propertyCursor.next() )
			  {
					int propertyKeyId = _propertyCursor.propertyKey();
					if ( _propertyKeyIdFilter.test( propertyKeyId ) )
					{
						 // This relationship has a property of interest to us
						 Value value = _propertyCursor.propertyValue();
						 // No need to validate values before passing them to the updater since the index implementation
						 // is allowed to fail in which ever way it wants to. The result of failure will be the same as
						 // a failed validation, i.e. population FAILED.
						 updates.Added( propertyKeyId, value );
						 hasRelevantProperty = true;
					}
			  }
			  return hasRelevantProperty;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws FAILURE
		 public override void Run()
		 {
			  EntityCursor.scan();
			  try
			  {
					  using ( EntityIdIterator entityIdIterator = EntityIdIterator )
					  {
						_continueScanning = true;
						while ( _continueScanning && entityIdIterator.hasNext() )
						{
							 _phaseTracker.enterPhase( Org.Neo4j.Kernel.Impl.Api.index.PhaseTracker_Phase.Scan );
							 long id = entityIdIterator.next();
							 using ( Lock ignored = _lockFunction.apply( id ) )
							 {
								  _count++;
								  if ( Process( EntityCursor ) )
								  {
										entityIdIterator.InvalidateCache();
								  }
							 }
						}
					  }
			  }
			  finally
			  {
					IOUtils.closeAllUnchecked( _propertyCursor, EntityCursor, _storageReader );
			  }
		 }

		 public override void AcceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
		 {
			  if ( update.EntityId <= currentlyIndexedNodeId )
			  {
					updater.Process( update );
			  }
		 }

		 /// <summary>
		 /// Process the given {@code record}.
		 /// </summary>
		 /// <param name="cursor"> CURSOR with information to process. </param>
		 /// <returns> {@code true} if external updates have been applied such that the scan iterator needs to be 100% up to date with store,
		 /// i.e. invalidate any caches if it has any. </returns>
		 /// <exception cref="FAILURE"> on failure. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract boolean process(CURSOR cursor) throws FAILURE;
		 protected internal abstract bool Process( CURSOR cursor );

		 public override void Stop()
		 {
			  _continueScanning = false;
		 }

		 public virtual PopulationProgress Progress
		 {
			 get
			 {
				  if ( _totalCount > 0 )
				  {
						return PopulationProgress.single( _count, _totalCount );
				  }
   
				  // nothing to do 100% completed
				  return Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.Done;
			 }
		 }

		 public virtual PhaseTracker PhaseTracker
		 {
			 set
			 {
				  this._phaseTracker = value;
			 }
		 }

		 protected internal virtual EntityIdIterator EntityIdIterator
		 {
			 get
			 {
				  return new EntityIdIteratorAnonymousInnerClass( this );
			 }
		 }

		 private class EntityIdIteratorAnonymousInnerClass : EntityIdIterator
		 {
			 private readonly PropertyAwareEntityStoreScan<CURSOR, FAILURE> _outerInstance;

			 public EntityIdIteratorAnonymousInnerClass( PropertyAwareEntityStoreScan<CURSOR, FAILURE> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private bool hasSeenNext;
			 private bool hasNext;

			 public void invalidateCache()
			 {
				  // Nothing to invalidate, we're reading directly from the store
			 }

			 public long next()
			 {
				  if ( !hasNext() )
				  {
						throw new System.InvalidOperationException();
				  }
				  hasSeenNext = false;
				  hasNext = false;
				  return _outerInstance.entityCursor.entityReference();
			 }

			 public bool hasNext()
			 {
				  if ( !hasSeenNext )
				  {
						 hasNext = _outerInstance.entityCursor.next();
						 hasSeenNext = true;
				  }
				  return hasNext;
			 }

			 public void close()
			 {
				  // Nothing to close
			 }
		 }
	}

}