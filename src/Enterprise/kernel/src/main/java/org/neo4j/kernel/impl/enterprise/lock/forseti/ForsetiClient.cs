using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using LongIntMap = org.eclipse.collections.api.map.primitive.LongIntMap;
	using MutableLongIntMap = org.eclipse.collections.api.map.primitive.MutableLongIntMap;
	using LongIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongIntHashMap;


	using Neo4Net.Collections.Pooling;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DeadlockResolutionStrategy = Neo4Net.Kernel.impl.enterprise.@lock.forseti.ForsetiLockManager.DeadlockResolutionStrategy;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using LockAcquisitionTimeoutException = Neo4Net.Kernel.impl.locking.LockAcquisitionTimeoutException;
	using LockClientStateHolder = Neo4Net.Kernel.impl.locking.LockClientStateHolder;
	using LockClientStoppedException = Neo4Net.Kernel.impl.locking.LockClientStoppedException;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using SimpleBitSet = Neo4Net.Kernel.impl.util.collection.SimpleBitSet;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Storageengine.Api.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Neo4Net.Storageengine.Api.@lock;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	// Please note. Except separate test cases for particular classes related to community locking
	// see also LockingCompatibilityTestSuite test suite

	/// <summary>
	/// These clients act as agents against the lock manager. The clients hold and release locks.
	/// <p/>
	/// The Forseti client tracks which locks it already holds, and will only communicate with the global lock manager if
	/// necessary. Grabbing the same lock multiple times will honor reentrancy et cetera, but the client will track in
	/// local fields how many times the lock has been grabbed, such that it will only grab and release the lock once from
	/// the
	/// global lock manager.
	/// </summary>
	public class ForsetiClient : Neo4Net.Kernel.impl.locking.Locks_Client
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_myExclusiveLock = new ExclusiveLock( this );
			_releaseExclusiveAndClearSharedVisitor = new ReleaseExclusiveLocksAndClearSharedVisitor( this );
			_releaseSharedDontCheckExclusiveVisitor = new ReleaseSharedDontCheckExclusiveVisitor( this );
		}

		 /// <summary>
		 /// Id for this client </summary>
		 private readonly int _clientId;

		 /// <summary>
		 /// resourceType -> lock map. These are the global lock maps, shared across all clients. </summary>
		 private readonly ConcurrentMap<long, ForsetiLockManager.Lock>[] _lockMaps;

		 /// <summary>
		 /// resourceType -> wait strategy </summary>
		 private readonly WaitStrategy<AcquireLockTimeoutException>[] _waitStrategies;

		 /// <summary>
		 /// How to resolve deadlocks. </summary>
		 private readonly DeadlockResolutionStrategy _deadlockResolutionStrategy;

		 /// <summary>
		 /// Handle to return client to pool when closed. </summary>
		 private readonly Pool<ForsetiClient> _clientPool;

		 /// <summary>
		 /// Look up a client by id </summary>
		 private readonly System.Func<int, ForsetiClient> _clientById;

		 /// <summary>
		 /// The client uses this to track which locks it holds. It is solely an optimization to ensure we don't need to
		 /// coordinate if we grab the same lock multiple times.
		 /// <p/>
		 /// The data structure looks like:
		 /// Array[ resourceType -> Map( resourceId -> num locks ) ]
		 /// </summary>
		 private readonly MutableLongIntMap[] _sharedLockCounts;

		 /// <seealso cref= #sharedLockCounts </seealso>
		 private readonly MutableLongIntMap[] _exclusiveLockCounts;

		 /// <summary>
		 /// Time within which any particular lock should be acquired.
		 /// </summary>
		 /// <seealso cref= GraphDatabaseSettings#lock_acquisition_timeout </seealso>
		 private readonly long _lockAcquisitionTimeoutMillis;
		 private readonly Clock _clock;

		 /// <summary>
		 /// List of other clients this client is waiting for. </summary>
		 private readonly SimpleBitSet _waitList = new SimpleBitSet( 64 );
		 private long _waitListCheckPoint;

		 // To be able to close Locks.Client instance properly we should be able to do couple of things:
		 //  - have a possibility to prevent new clients to come
		 //  - wake up all the waiters and let them go
		 //  - have a possibility to see how many clients are still using us and wait for them to finish
		 // We need to do all of that to prevent a situation when a closing client will get a lock that will never be
		 // closed and eventually will block other clients.
		 private readonly LockClientStateHolder _stateHolder = new LockClientStateHolder();

		 /// <summary>
		 /// For exclusive locks, we only need a single re-usable one per client. We simply CAS this lock into whatever slots
		 /// we want to hold in the global lock map.
		 /// </summary>
		 private ExclusiveLock _myExclusiveLock;

		 private volatile bool _hasLocks;

		 private ReleaseExclusiveLocksAndClearSharedVisitor _releaseExclusiveAndClearSharedVisitor;
		 private ReleaseSharedDontCheckExclusiveVisitor _releaseSharedDontCheckExclusiveVisitor;

		 /// <summary>
		 /// When we *wait* for a specific lock to be released to us, we assign it to this field. This helps us during the
		 /// secondary deadlock verification process, where we traverse the waiter/lock-owner dependency graph.
		 /// </summary>
		 private volatile ForsetiLockManager.Lock _waitingForLock;

		 public ForsetiClient( int id, ConcurrentMap<long, ForsetiLockManager.Lock>[] lockMaps, WaitStrategy<AcquireLockTimeoutException>[] waitStrategies, Pool<ForsetiClient> clientPool, DeadlockResolutionStrategy deadlockResolutionStrategy, System.Func<int, ForsetiClient> clientById, long lockAcquisitionTimeoutMillis, Clock clock )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._clientId = id;
			  this._lockMaps = lockMaps;
			  this._waitStrategies = waitStrategies;
			  this._deadlockResolutionStrategy = deadlockResolutionStrategy;
			  this._clientPool = clientPool;
			  this._clientById = clientById;
			  this._sharedLockCounts = new MutableLongIntMap[lockMaps.Length];
			  this._exclusiveLockCounts = new MutableLongIntMap[lockMaps.Length];
			  this._lockAcquisitionTimeoutMillis = lockAcquisitionTimeoutMillis;
			  this._clock = clock;

			  for ( int i = 0; i < _sharedLockCounts.Length; i++ )
			  {
					_sharedLockCounts[i] = new CountableLongIntHashMap();
					_exclusiveLockCounts[i] = new CountableLongIntHashMap();
			  }
		 }

		 /// <summary>
		 /// Reset current client state. Make it ready for next bunch of operations.
		 /// Should be used before factory release client to public usage.
		 /// </summary>
		 public virtual void Reset()
		 {
			  _stateHolder.reset();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  _hasLocks = true;
			  _stateHolder.incrementActiveClients( this );
			  LockWaitEvent waitEvent = null;

			  try
			  {
					// Grab the global lock map we will be using
					ConcurrentMap<long, ForsetiLockManager.Lock> lockMap = _lockMaps[resourceType.TypeId()];

					// And grab our local lock maps
					MutableLongIntMap heldShareLocks = _sharedLockCounts[resourceType.TypeId()];
					MutableLongIntMap heldExclusiveLocks = _exclusiveLockCounts[resourceType.TypeId()];

					foreach ( long resourceId in resourceIds )
					{
						 // First, check if we already hold this as a shared lock
						 int heldCount = heldShareLocks.getIfAbsent( resourceId, -1 );
						 if ( heldCount != -1 )
						 {
							  // We already have a lock on this, just increment our local reference counter.
							  heldShareLocks.put( resourceId, Math.incrementExact( heldCount ) );
							  continue;
						 }

						 // Second, check if we hold it as an exclusive lock
						 if ( heldExclusiveLocks.containsKey( resourceId ) )
						 {
							  // We already have an exclusive lock, so just leave that in place.
							  // When the exclusive lock is released, it will be automatically downgraded to a shared lock,
							  // since we bumped the share lock reference count.
							  heldShareLocks.put( resourceId, 1 );
							  continue;
						 }

						 // We don't hold the lock, so we need to grab it via the global lock map
						 int tries = 0;
						 SharedLock mySharedLock = null;
						 long waitStartMillis = _clock.millis();

						 // Retry loop
						 while ( true )
						 {
							  AssertValid( waitStartMillis, resourceType, resourceId );

							  // Check if there is a lock for this entity in the map
							  ForsetiLockManager.Lock existingLock = lockMap.get( resourceId );

							  // No lock
							  if ( existingLock == null )
							  {
									// Try to create a new shared lock
									if ( mySharedLock == null )
									{
										 mySharedLock = new SharedLock( this );
									}

									if ( lockMap.putIfAbsent( resourceId, mySharedLock ) == null )
									{
										 // Success, we now hold the shared lock.
										 break;
									}
									else
									{
										 continue;
									}
							  }

							  // Someone holds shared lock on this entity, try and get in on that action
							  else if ( existingLock is SharedLock )
							  {
									if ( ( ( SharedLock ) existingLock ).Acquire( this ) )
									{
										 // Success!
										 break;
									}
							  }

							  // Someone holds an exclusive lock on this entity
							  else if ( existingLock is ExclusiveLock )
							  {
									// We need to wait, just let the loop run.
							  }
							  else
							  {
									throw new System.NotSupportedException( "Unknown lock type: " + existingLock );
							  }

							  if ( waitEvent == null )
							  {
									waitEvent = tracer.WaitForLock( false, resourceType, resourceId );
							  }
							  // And take note of who we are waiting for. This is used for deadlock detection.
							  WaitFor( existingLock, resourceType, resourceId, false, tries++ );
						 }

						 // Make a local note about the fact that we now hold this lock
						 heldShareLocks.put( resourceId, 1 );
					}
			  }
			  finally
			  {
					if ( waitEvent != null )
					{
						 waitEvent.Close();
					}
					ClearWaitList();
					_waitingForLock = null;
					_stateHolder.decrementActiveClients();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  _hasLocks = true;
			  _stateHolder.incrementActiveClients( this );
			  LockWaitEvent waitEvent = null;

			  try
			  {
					ConcurrentMap<long, ForsetiLockManager.Lock> lockMap = _lockMaps[resourceType.TypeId()];
					MutableLongIntMap heldLocks = _exclusiveLockCounts[resourceType.TypeId()];

					foreach ( long resourceId in resourceIds )
					{
						 int heldCount = heldLocks.getIfAbsent( resourceId, -1 );
						 if ( heldCount != -1 )
						 {
							  // We already have a lock on this, just increment our local reference counter.
							  heldLocks.put( resourceId, Math.incrementExact( heldCount ) );
							  continue;
						 }

						 // Grab the global lock
						 ForsetiLockManager.Lock existingLock;
						 int tries = 0;
						 long waitStartMillis = _clock.millis();
						 while ( ( existingLock = lockMap.putIfAbsent( resourceId, _myExclusiveLock ) ) != null )
						 {
							  AssertValid( waitStartMillis, resourceType, resourceId );

							  // If this is a shared lock:
							  // Given a grace period of tries (to try and not starve readers), grab an update lock and wait
							  // for it to convert to an exclusive lock.
							  if ( tries > 50 && existingLock is SharedLock )
							  {
									// Then we should upgrade that lock
									SharedLock sharedLock = ( SharedLock ) existingLock;
									if ( TryUpgradeSharedToExclusive( tracer, waitEvent, resourceType, lockMap, resourceId, sharedLock, waitStartMillis ) )
									{
										 break;
									}
							  }

							  if ( waitEvent == null )
							  {
									waitEvent = tracer.WaitForLock( true, resourceType, resourceId );
							  }
							  WaitFor( existingLock, resourceType, resourceId, true, tries++ );
						 }

						 heldLocks.put( resourceId, 1 );
					}
			  }
			  finally
			  {
					if ( waitEvent != null )
					{
						 waitEvent.Close();
					}
					ClearWaitList();
					_waitingForLock = null;
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
		 {
			  _hasLocks = true;
			  _stateHolder.incrementActiveClients( this );

			  try
			  {
					ConcurrentMap<long, ForsetiLockManager.Lock> lockMap = _lockMaps[resourceType.TypeId()];
					MutableLongIntMap heldLocks = _exclusiveLockCounts[resourceType.TypeId()];

					int heldCount = heldLocks.getIfAbsent( resourceId, -1 );
					if ( heldCount != -1 )
					{
						 // We already have a lock on this, just increment our local reference counter.
						 heldLocks.put( resourceId, Math.incrementExact( heldCount ) );
						 return true;
					}

					// Grab the global lock
					ForsetiLockManager.Lock @lock;
					if ( ( @lock = lockMap.putIfAbsent( resourceId, _myExclusiveLock ) ) != null )
					{
						 if ( @lock is SharedLock && _sharedLockCounts[resourceType.TypeId()].containsKey(resourceId) )
						 {
							  SharedLock sharedLock = ( SharedLock ) @lock;
							  if ( sharedLock.TryAcquireUpdateLock( this ) )
							  {
									if ( sharedLock.NumberOfHolders() == 1 )
									{
										 heldLocks.put( resourceId, 1 );
										 return true;
									}
									else
									{
										 sharedLock.ReleaseUpdateLock();
										 return false;
									}
							  }
						 }
						 return false;
					}

					heldLocks.put( resourceId, 1 );
					return true;
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool TrySharedLock( ResourceType resourceType, long resourceId )
		 {
			  _hasLocks = true;
			  _stateHolder.incrementActiveClients( this );

			  try
			  {
					ConcurrentMap<long, ForsetiLockManager.Lock> lockMap = _lockMaps[resourceType.TypeId()];
					MutableLongIntMap heldShareLocks = _sharedLockCounts[resourceType.TypeId()];
					MutableLongIntMap heldExclusiveLocks = _exclusiveLockCounts[resourceType.TypeId()];

					int heldCount = heldShareLocks.getIfAbsent( resourceId, -1 );
					if ( heldCount != -1 )
					{
						 // We already have a lock on this, just increment our local reference counter.
						 heldShareLocks.put( resourceId, Math.incrementExact( heldCount ) );
						 return true;
					}

					if ( heldExclusiveLocks.containsKey( resourceId ) )
					{
						 // We already have an exclusive lock, so just leave that in place. When the exclusive lock is released,
						 // it will be automatically downgraded to a shared lock, since we bumped the share lock reference count.
						 heldShareLocks.put( resourceId, 1 );
						 return true;
					}

					long waitStartMillis = _clock.millis();
					while ( true )
					{
						 AssertValid( waitStartMillis, resourceType, resourceId );

						 ForsetiLockManager.Lock existingLock = lockMap.get( resourceId );
						 if ( existingLock == null )
						 {
							  // Try to create a new shared lock
							  if ( lockMap.putIfAbsent( resourceId, new SharedLock( this ) ) == null )
							  {
									// Success!
									break;
							  }
						 }
						 else if ( existingLock is SharedLock )
						 {
							  // Note that there is a "safe" race here where someone may be releasing the last reference to a lock
							  // and thus removing that lock instance (making it unacquirable). In this case, we allow retrying,
							  // even though this is a try-lock call.
							  if ( ( ( SharedLock ) existingLock ).Acquire( this ) )
							  {
									// Success!
									break;
							  }
							  else if ( ( ( SharedLock ) existingLock ).UpdateLock )
							  {
									return false;
							  }
						 }
						 else if ( existingLock is ExclusiveLock )
						 {
							  return false;
						 }
						 else
						 {
							  throw new System.NotSupportedException( "Unknown lock type: " + existingLock );
						 }
					}
					heldShareLocks.put( resourceId, 1 );
					return true;
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool ReEnterShared( ResourceType resourceType, long resourceId )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
					MutableLongIntMap heldShareLocks = _sharedLockCounts[resourceType.TypeId()];
					MutableLongIntMap heldExclusiveLocks = _exclusiveLockCounts[resourceType.TypeId()];

					int heldCount = heldShareLocks.getIfAbsent( resourceId, -1 );
					if ( heldCount != -1 )
					{
						 // We already have a lock on this, just increment our local reference counter.
						 heldShareLocks.put( resourceId, Math.incrementExact( heldCount ) );
						 return true;
					}

					if ( heldExclusiveLocks.containsKey( resourceId ) )
					{
						 // We already have an exclusive lock, so just leave that in place. When the exclusive lock is released,
						 // it will be automatically downgraded to a shared lock, since we bumped the share lock reference count.
						 heldShareLocks.put( resourceId, 1 );
						 return true;
					}

					// We didn't hold a lock already, so we cannot re-enter.
					return false;
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
					MutableLongIntMap heldLocks = _exclusiveLockCounts[resourceType.TypeId()];

					int heldCount = heldLocks.getIfAbsent( resourceId, -1 );
					if ( heldCount != -1 )
					{
						 // We already have a lock on this, just increment our local reference counter.
						 heldLocks.put( resourceId, Math.incrementExact( heldCount ) );
						 return true;
					}

					// We didn't hold a lock already, so we cannot re-enter.
					return false;
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
		 {
			  _stateHolder.incrementActiveClients( this );

			  try
			  {
					MutableLongIntMap sharedLocks = _sharedLockCounts[resourceType.TypeId()];
					MutableLongIntMap exclusiveLocks = _exclusiveLockCounts[resourceType.TypeId()];
					ConcurrentMap<long, ForsetiLockManager.Lock> resourceTypeLocks = _lockMaps[resourceType.TypeId()];
					foreach ( long resourceId in resourceIds )
					{
						 if ( ReleaseLocalLock( resourceType, resourceId, sharedLocks ) )
						 {
							  continue;
						 }
						 // Only release if we were not holding an exclusive lock as well
						 if ( !exclusiveLocks.containsKey( resourceId ) )
						 {
							  ReleaseGlobalLock( resourceTypeLocks, resourceId );
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
		 {
			  _stateHolder.incrementActiveClients( this );

			  try
			  {
					ConcurrentMap<long, ForsetiLockManager.Lock> resourceTypeLocks = _lockMaps[resourceType.TypeId()];
					MutableLongIntMap exclusiveLocks = _exclusiveLockCounts[resourceType.TypeId()];
					MutableLongIntMap sharedLocks = _sharedLockCounts[resourceType.TypeId()];
					foreach ( long resourceId in resourceIds )
					{
						 if ( ReleaseLocalLock( resourceType, resourceId, exclusiveLocks ) )
						 {
							  continue;
						 }

						 if ( sharedLocks.containsKey( resourceId ) )
						 {
							  // We are still holding a shared lock, so we will release it to be reused
							  ForsetiLockManager.Lock @lock = resourceTypeLocks.get( resourceId );
							  if ( @lock is SharedLock )
							  {
									SharedLock sharedLock = ( SharedLock ) @lock;
									if ( sharedLock.UpdateLock )
									{
										 sharedLock.ReleaseUpdateLock();
									}
									else
									{
										 throw new System.InvalidOperationException( "Incorrect state of exclusive lock. Lock should be updated " + "to exclusive before attempt to release it. Lock: " + this );
									}
							  }
							  else
							  {
									// in case if current lock is exclusive we swap it to new shared lock
									SharedLock sharedLock = new SharedLock( this );
									resourceTypeLocks.put( resourceId, sharedLock );
							  }
						 }
						 else
						 {
							  // we do not hold shared lock so we just releasing it
							  ReleaseGlobalLock( resourceTypeLocks, resourceId );
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 private void ReleaseAllClientLocks()
		 {
			  // Force the release of all locks held.
			  for ( int i = 0; i < _exclusiveLockCounts.Length; i++ )
			  {
					MutableLongIntMap exclusiveLocks = _exclusiveLockCounts[i];
					MutableLongIntMap sharedLocks = _sharedLockCounts[i];

					// Begin releasing exclusive locks, as we may hold both exclusive and shared locks on the same resource,
					// and so releasing exclusive locks means we can "throw away" our shared lock (which would normally have
					// been re-instated after releasing the exclusive lock).
					if ( exclusiveLocks != null )
					{
						 int size = exclusiveLocks.size();
						 exclusiveLocks.forEachKey( _releaseExclusiveAndClearSharedVisitor.initialize( sharedLocks, _lockMaps[i] ) );
						 if ( size <= 32 )
						 {
							  // If the map is small, its fast and nice to GC to clear it. However, if its large, it is
							  // 1) Faster to simply allocate a new one and
							  // 2) Safer, because we guard against clients getting giant maps over time
							  if ( size > 0 )
							  {
									exclusiveLocks.clear();
							  }
						 }
						 else
						 {
							  _exclusiveLockCounts[i] = new LongIntHashMap();
						 }
					}

					// Then release all remaining shared locks
					if ( sharedLocks != null )
					{
						 int size = sharedLocks.size();
						 sharedLocks.forEachKey( _releaseSharedDontCheckExclusiveVisitor.initialize( _lockMaps[i] ) );
						 if ( size <= 32 )
						 {
							  // If the map is small, its fast and nice to GC to clear it. However, if its large, it is
							  // 1) Faster to simply allocate a new one and
							  // 2) Safer, because we guard against clients getting giant maps over time
							  if ( size > 0 )
							  {
									sharedLocks.clear();
							  }
						 }
						 else
						 {
							  _sharedLockCounts[i] = new LongIntHashMap();
						 }
					}
			  }
		 }

		 public override void Prepare()
		 {
			  _stateHolder.prepare( this );
		 }

		 public override void Stop()
		 {
			  // marking client as closed
			  if ( _stateHolder.stopClient() )
			  {
					// waiting for all operations to be completed
					WaitForAllClientsToLeave();
					ReleaseAllLocks();
			  }
		 }

		 private void WaitForAllClientsToLeave()
		 {
			  while ( _stateHolder.hasActiveClients() )
			  {
					try
					{
						 Thread.Sleep( 10 );
					}
					catch ( InterruptedException )
					{
						 Thread.interrupted();
					}
			  }
		 }

		 public override void Close()
		 {
			  _stateHolder.closeClient();
			  WaitForAllClientsToLeave();
			  ReleaseAllLocks();
			  _clientPool.release( this );
		 }

		 private void ReleaseAllLocks()
		 {
			  if ( _hasLocks )
			  {
					ReleaseAllClientLocks();
					ClearWaitList();
					_hasLocks = false;
			  }
		 }

		 public virtual int LockSessionId
		 {
			 get
			 {
				  return _clientId;
			 }
		 }

		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  IList<ActiveLock> locks = new List<ActiveLock>();
			  CollectActiveLocks( _exclusiveLockCounts, locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory_Fields.ExclusiveLock );
			  CollectActiveLocks( _sharedLockCounts, locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory_Fields.SharedLock );
			  return locks.stream();
		 }

		 public override long ActiveLockCount()
		 {
			  return CountLocks( _exclusiveLockCounts ) + CountLocks( _sharedLockCounts );
		 }

		 private static void CollectActiveLocks( LongIntMap[] counts, IList<ActiveLock> locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory activeLock )
		 {
			  for ( int typeId = 0; typeId < Counts.Length; typeId++ )
			  {
					LongIntMap lockCounts = counts[typeId];
					if ( lockCounts != null )
					{
						 ResourceType resourceType = ResourceTypes.fromId( typeId );
						 lockCounts.forEachKeyValue( ( resourceId, count ) => locks.Add( activeLock.Create( resourceType, resourceId ) ) );
					}
			  }
		 }

		 private long CountLocks( LongIntMap[] lockCounts )
		 {
			  long count = 0;
			  foreach ( LongIntMap lockCount in lockCounts )
			  {
					if ( lockCount != null )
					{
						 count += lockCount.size();
					}
			  }
			  return count;
		 }

		 internal virtual int WaitListSize()
		 {
			  return _waitList.size();
		 }

		 internal virtual void CopyWaitListTo( SimpleBitSet other )
		 {
			  other.Put( _waitList );
		 }

		 internal virtual bool IsWaitingFor( int clientId )
		 {
			  return clientId != this._clientId && _waitList.contains( clientId );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ForsetiClient that = ( ForsetiClient ) o;

			  return _clientId == that._clientId;
		 }

		 public override int GetHashCode()
		 {
			  return _clientId;
		 }

		 public override string ToString()
		 {
			  return string.Format( "ForsetiClient[{0:D}]", _clientId );
		 }

		 /// <summary>
		 /// Release a lock from the global pool. </summary>
		 private void ReleaseGlobalLock( ConcurrentMap<long, ForsetiLockManager.Lock> lockMap, long resourceId )
		 {
			  ForsetiLockManager.Lock @lock = lockMap.get( resourceId );
			  if ( @lock is ExclusiveLock )
			  {
					lockMap.remove( resourceId );
			  }
			  else if ( @lock is SharedLock && ( ( SharedLock ) @lock ).Release( this ) )
			  {
					// We were the last to hold this lock, it is now dead and we should remove it.
					// Also cleaning updater reference that can hold lock in memory
					( ( SharedLock ) @lock ).CleanUpdateHolder();
					lockMap.remove( resourceId );
			  }
		 }

		 /// <summary>
		 /// Release a lock locally, and return true if we still hold more references to that lock. </summary>
		 private bool ReleaseLocalLock( ResourceType type, long resourceId, MutableLongIntMap localLocks )
		 {
			  int lockCount = localLocks.removeKeyIfAbsent( resourceId, -1 );
			  if ( lockCount == -1 )
			  {
					throw new System.InvalidOperationException( this + " cannot release lock that it does not hold: " + type + "[" + resourceId + "]." );
			  }

			  if ( lockCount > 1 )
			  {
					localLocks.put( resourceId, lockCount - 1 );
					return true;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Attempt to upgrade a share lock to an exclusive lock, grabbing the share lock if we don't hold it.
		 /// 
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean tryUpgradeSharedToExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.LockWaitEvent waitEvent, org.neo4j.storageengine.api.lock.ResourceType resourceType, java.util.concurrent.ConcurrentMap<long,ForsetiLockManager.Lock> lockMap, long resourceId, SharedLock sharedLock, long waitStartMillis) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 private bool TryUpgradeSharedToExclusive( LockTracer tracer, LockWaitEvent waitEvent, ResourceType resourceType, ConcurrentMap<long, ForsetiLockManager.Lock> lockMap, long resourceId, SharedLock sharedLock, long waitStartMillis )
		 {
			  int tries = 0;
			  bool holdsSharedLock = _sharedLockCounts[resourceType.TypeId()].containsKey(resourceId);
			  if ( !holdsSharedLock )
			  {
					// We don't hold the shared lock, we need to grab it to upgrade it to an exclusive one
					if ( !sharedLock.Acquire( this ) )
					{
						 return false;
					}

					try
					{
						 if ( TryUpgradeToExclusiveWithShareLockHeld( tracer, waitEvent, resourceType, resourceId, sharedLock, tries, waitStartMillis ) )
						 {
							  return true;
						 }
						 else
						 {
							  ReleaseGlobalLock( lockMap, resourceId );
							  return false;
						 }
					}
					catch ( Exception e )
					{
						 ReleaseGlobalLock( lockMap, resourceId );
						 throw e;
					}
			  }
			  else
			  {
					// We do hold the shared lock, so no reason to deal with the complexity in the case above.
					return TryUpgradeToExclusiveWithShareLockHeld( tracer, waitEvent, resourceType, resourceId, sharedLock, tries, waitStartMillis );
			  }
		 }

		 /// <summary>
		 /// Attempt to upgrade a share lock that we hold to an exclusive lock. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean tryUpgradeToExclusiveWithShareLockHeld(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.LockWaitEvent priorEvent, org.neo4j.storageengine.api.lock.ResourceType resourceType, long resourceId, SharedLock sharedLock, int tries, long waitStartMillis) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 private bool TryUpgradeToExclusiveWithShareLockHeld( LockTracer tracer, LockWaitEvent priorEvent, ResourceType resourceType, long resourceId, SharedLock sharedLock, int tries, long waitStartMillis )
		 {
			  if ( sharedLock.TryAcquireUpdateLock( this ) )
			  {
					LockWaitEvent waitEvent = null;
					try
					{
						 // Now we just wait for all clients to release the the share lock
						 while ( sharedLock.NumberOfHolders() > 1 )
						 {
							  AssertValid( waitStartMillis, resourceType, resourceId );
							  if ( waitEvent == null && priorEvent == null )
							  {
									waitEvent = tracer.WaitForLock( true, resourceType, resourceId );
							  }
							  WaitFor( sharedLock, resourceType, resourceId, true, tries++ );
						 }

						 return true;
					}
					catch ( Exception e )
					{
						 sharedLock.ReleaseUpdateLock();
						 if ( e is DeadlockDetectedException || e is LockClientStoppedException )
						 {
							  throw ( Exception ) e;
						 }
						 throw new TransactionFailureException( "Failed to upgrade shared lock to exclusive: " + sharedLock, e );
					}
					finally
					{
						 if ( waitEvent != null )
						 {
							  waitEvent.Close();
						 }
						 ClearWaitList();
						 _waitingForLock = null;
					}
			  }
			  return false;
		 }

		 private void ClearWaitList()
		 {
			  _waitListCheckPoint = _waitList.checkPointAndPut( _waitListCheckPoint, _clientId );
		 }

		 private void WaitFor( ForsetiLockManager.Lock @lock, ResourceType type, long resourceId, bool exclusive, int tries )
		 {
			  _waitingForLock = @lock;
			  ClearAndCopyWaitList( @lock );
			  _waitStrategies[type.TypeId()].apply(tries);

			  int b = @lock.DetectDeadlock( Id() );
			  if ( b != -1 && _deadlockResolutionStrategy.shouldAbort( this, _clientById.apply( b ) ) )
			  {
					// Force the operations below to happen after the reads we do for deadlock
					// detection in the lines above, as a way to cut down on false-positive deadlocks
					UnsafeUtil.loadFence();

					// Create message before we clear the wait-list, to lower the chance of the message being insane
					string message = this + " can't acquire " + @lock + " on " + type + "(" + resourceId +
										  "), because holders of that lock " +
										  "are waiting for " + this + ".\n Wait list:" + @lock.DescribeWaitList();

					// Minimize the risk of false positives by double-checking that the deadlock remains
					// after we've generated a description of it.
					if ( @lock.DetectDeadlock( Id() ) != -1 )
					{
						 // If the deadlock is real, then an owner of this lock must be (transitively) waiting on a lock that
						 // we own. So to verify the deadlock, we traverse the lock owners and their `waitingForLock` fields,
						 // to find a lock that has us among the owners.
						 // We only act upon the result of this method if the `tries` count is above some threshold. The reason
						 // is that the Lock.collectOwners, which is algorithm relies upon, is inherently racy, and so only
						 // reduces the probably of a false positive, but does not eliminate them.
						 if ( IsDeadlockReal( @lock, tries ) )
						 {
							  // After checking several times, this really does look like a real deadlock.
							  throw new DeadlockDetectedException( message );
						 }
					}
			  }
		 }

		 private void ClearAndCopyWaitList( ForsetiLockManager.Lock @lock )
		 {
			  ClearWaitList();
			  @lock.CopyHolderWaitListsInto( _waitList );
		 }

		 private bool IsDeadlockReal( ForsetiLockManager.Lock @lock, int tries )
		 {
			  ISet<ForsetiLockManager.Lock> waitedUpon = new HashSet<ForsetiLockManager.Lock>();
			  ISet<ForsetiClient> owners = new HashSet<ForsetiClient>();
			  ISet<ForsetiLockManager.Lock> nextWaitedUpon = new HashSet<ForsetiLockManager.Lock>();
			  ISet<ForsetiClient> nextOwners = new HashSet<ForsetiClient>();
			  @lock.CollectOwners( owners );

			  do
			  {
					waitedUpon.addAll( nextWaitedUpon );
					CollectNextOwners( waitedUpon, owners, nextWaitedUpon, nextOwners );
					if ( nextOwners.Contains( this ) && tries > 20 )
					{
						 // Worrying... let's take a deep breath
						 nextOwners.Clear();
						 LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
						 // ... and check again
						 CollectNextOwners( waitedUpon, owners, nextWaitedUpon, nextOwners );
						 if ( nextOwners.Contains( this ) )
						 {
							  // Yes, this deadlock looks real.
							  return true;
						 }
					}
					owners.Clear();
					ISet<ForsetiClient> ownersTmp = owners;
					owners = nextOwners;
					nextOwners = ownersTmp;
			  } while ( nextWaitedUpon.Count > 0 );
			  // Nope, we didn't find any real wait cycles.
			  return false;
		 }

		 private void CollectNextOwners( ISet<ForsetiLockManager.Lock> waitedUpon, ISet<ForsetiClient> owners, ISet<ForsetiLockManager.Lock> nextWaitedUpon, ISet<ForsetiClient> nextOwners )
		 {
			  nextWaitedUpon.Clear();
			  foreach ( ForsetiClient owner in owners )
			  {
					ForsetiLockManager.Lock waitingForLock = owner._waitingForLock;
					if ( waitingForLock != null && !waitedUpon.Contains( waitingForLock ) )
					{
						 nextWaitedUpon.Add( waitingForLock );
					}
			  }
			  foreach ( ForsetiLockManager.Lock lck in nextWaitedUpon )
			  {
					lck.CollectOwners( nextOwners );
			  }
		 }

		 internal virtual string DescribeWaitList()
		 {
			  StringBuilder sb = new StringBuilder( format( "%nClient[%d] waits for [", Id() ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.IntIterator iter = waitList.iterator();
			  IntIterator iter = _waitList.GetEnumerator();
			  for ( bool first = true; iter.hasNext(); )
			  {
					int next = iter.next();
					if ( next == _clientId )
					{
						 // Skip our own id from the wait list, that's an implementation detail
						 continue;
					}
					sb.Append( ( !first ) ? "," : "" ).Append( next );
					first = false;
			  }
			  sb.Append( "]" );
			  return sb.ToString();
		 }

		 public virtual int Id()
		 {
			  return _clientId;
		 }

		 private void AssertValid( long waitStartMillis, ResourceType resourceType, long resourceId )
		 {
			  AssertNotStopped();
			  AssertNotExpired( waitStartMillis, resourceType, resourceId );
		 }

		 private void AssertNotStopped()
		 {
			  if ( _stateHolder.Stopped )
			  {
					throw new LockClientStoppedException( this );
			  }
		 }

		 private void AssertNotExpired( long waitStartMillis, ResourceType resourceType, long resourceId )
		 {
			  if ( _lockAcquisitionTimeoutMillis > 0 )
			  {
					if ( ( _lockAcquisitionTimeoutMillis + waitStartMillis ) < _clock.millis() )
					{
						 throw new LockAcquisitionTimeoutException( resourceType, resourceId, _lockAcquisitionTimeoutMillis );
					}
			  }
		 }

		 // Visitors used for bulk ops on the lock maps (such as releasing all locks)

		 /// <summary>
		 /// Release all shared locks, assuming that there will be no exclusive locks held by this client, such that there
		 /// is no need to check for those. It is used when releasing all locks.
		 /// </summary>
		 private class ReleaseSharedDontCheckExclusiveVisitor : LongProcedure
		 {
			 private readonly ForsetiClient _outerInstance;

			 public ReleaseSharedDontCheckExclusiveVisitor( ForsetiClient outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal ConcurrentMap<long, ForsetiLockManager.Lock> LockMap;

			  internal virtual LongProcedure Initialize( ConcurrentMap<long, ForsetiLockManager.Lock> lockMap )
			  {
					this.LockMap = lockMap;
					return this;
			  }

			  public override void Value( long resourceId )
			  {
					outerInstance.releaseGlobalLock( LockMap, resourceId );
			  }
		 }

		 /// <summary>
		 /// Release exclusive locks and remove any local reference to the shared lock.
		 /// This is an optimization used when releasing all locks.
		 /// </summary>
		 private class ReleaseExclusiveLocksAndClearSharedVisitor : LongProcedure
		 {
			 private readonly ForsetiClient _outerInstance;

			 public ReleaseExclusiveLocksAndClearSharedVisitor( ForsetiClient outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal MutableLongIntMap SharedLockCounts;
			  internal ConcurrentMap<long, ForsetiLockManager.Lock> LockMap;

			  internal virtual LongProcedure Initialize( MutableLongIntMap sharedLockCounts, ConcurrentMap<long, ForsetiLockManager.Lock> lockMap )
			  {
					this.SharedLockCounts = sharedLockCounts;
					this.LockMap = lockMap;
					return this;
			  }

			  public override void Value( long resourceId )
			  {
					outerInstance.releaseGlobalLock( LockMap, resourceId );

					// If we hold this as a shared lock, we can throw that shared lock away directly, since we haven't
					// followed the down-grade protocol.
					if ( SharedLockCounts != null )
					{
						 SharedLockCounts.remove( resourceId );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ExternalizableWithoutPublicNoArgConstructor") private static class CountableLongIntHashMap extends org.eclipse.collections.impl.map.mutable.primitive.LongIntHashMap
		 private class CountableLongIntHashMap : LongIntHashMap
		 {
			  internal CountableLongIntHashMap() : base()
			  {
			  }

			  public override int Size()
			  {
					SentinelValues sentinelValues = SentinelValues;
					return OccupiedWithData + ( sentinelValues == null ? 0 : sentinelValues.size() );
			  }
		 }
	}

}