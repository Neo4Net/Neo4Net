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
namespace Neo4Net.Kernel.impl.locking.community
{
	using Procedure = org.eclipse.collections.api.block.procedure.Procedure;
	using IntObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.IntObjectProcedure;
	using LongObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure;
	using LongObjectMap = org.eclipse.collections.api.map.primitive.LongObjectMap;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;


	// Please note. Except separate test cases for particular classes related to community locking
	// see also Neo4Net.kernel.impl.locking.community.CommunityLocksCompatibility test suite

	public class CommunityLockClient : Neo4Net.Kernel.impl.locking.Locks_Client
	{
		 private readonly LockManagerImpl _manager;
		 private readonly LockTransaction _lockTransaction = new LockTransaction();

		 private readonly MutableIntObjectMap<MutableLongObjectMap<LockResource>> _sharedLocks = new IntObjectHashMap<MutableLongObjectMap<LockResource>>();
		 private readonly MutableIntObjectMap<MutableLongObjectMap<LockResource>> _exclusiveLocks = new IntObjectHashMap<MutableLongObjectMap<LockResource>>();
		 private readonly LongObjectProcedure<LockResource> _readReleaser;
		 private readonly LongObjectProcedure<LockResource> _writeReleaser;
		 private readonly Procedure<LongObjectMap<LockResource>> _typeReadReleaser;
		 private readonly Procedure<LongObjectMap<LockResource>> _typeWriteReleaser;

		 // To be able to close Locks.Client instance properly we should be able to do couple of things:
		 //  - have a possibility to prevent new clients to come
		 //  - wake up all the waiters and let them go
		 //  - have a possibility to see how many clients are still using us and wait for them to finish
		 // We need to do all of that to prevent a situation when a closing client will get a lock that will never be
		 // closed and eventually will block other clients.
		 private readonly LockClientStateHolder _stateHolder = new LockClientStateHolder();

		 public CommunityLockClient( LockManagerImpl manager )
		 {
			  this._manager = manager;

			  _readReleaser = ( key, lockResource ) => manager.releaseReadLock( lockResource, _lockTransaction );
			  _writeReleaser = ( key, lockResource ) => manager.releaseWriteLock( lockResource, _lockTransaction );
			  _typeReadReleaser = value => value.forEachKeyValue( _readReleaser );
			  _typeWriteReleaser = value => value.forEachKeyValue( _writeReleaser );
		 }

		 public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
					MutableLongObjectMap<LockResource> localLocks = LocalShared( resourceType );
					foreach ( long resourceId in resourceIds )
					{
						 LockResource resource = localLocks.get( resourceId );
						 if ( resource != null )
						 {
							  resource.AcquireReference();
						 }
						 else
						 {
							  resource = new LockResource( resourceType, resourceId );
							  if ( _manager.getReadLock( tracer, resource, _lockTransaction ) )
							  {
									localLocks.put( resourceId, resource );
							  }
							  else
							  {
									throw new LockClientStoppedException( this );
							  }
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
					MutableLongObjectMap<LockResource> localLocks = LocalExclusive( resourceType );
					foreach ( long resourceId in resourceIds )
					{
						 LockResource resource = localLocks.get( resourceId );
						 if ( resource != null )
						 {
							  resource.AcquireReference();
						 }
						 else
						 {
							  resource = new LockResource( resourceType, resourceId );
							  if ( _manager.getWriteLock( tracer, resource, _lockTransaction ) )
							  {
									localLocks.put( resourceId, resource );
							  }
							  else
							  {
									throw new LockClientStoppedException( this );
							  }
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<LockResource> localLocks = localExclusive(resourceType);
					MutableLongObjectMap<LockResource> localLocks = LocalExclusive( resourceType );
					LockResource resource = localLocks.get( resourceId );
					if ( resource != null )
					{
						 resource.AcquireReference();
						 return true;
					}
					else
					{
						 resource = new LockResource( resourceType, resourceId );
						 if ( _manager.tryWriteLock( resource, _lockTransaction ) )
						 {
							  localLocks.put( resourceId, resource );
							  return true;
						 }
						 else
						 {
							  return false;
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override bool TrySharedLock( ResourceType resourceType, long resourceId )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<LockResource> localLocks = localShared(resourceType);
					MutableLongObjectMap<LockResource> localLocks = LocalShared( resourceType );
					LockResource resource = localLocks.get( resourceId );
					if ( resource != null )
					{
						 resource.AcquireReference();
						 return true;
					}
					else
					{
						 resource = new LockResource( resourceType, resourceId );
						 if ( _manager.tryReadLock( resource, _lockTransaction ) )
						 {
							  localLocks.put( resourceId, resource );
							  return true;
						 }
						 else
						 {
							  return false;
						 }
					}
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
					return ReEnter( LocalShared( resourceType ), resourceId );
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
					return ReEnter( LocalExclusive( resourceType ), resourceId );
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 private bool ReEnter( LongObjectMap<LockResource> localLocks, long resourceId )
		 {
			  LockResource resource = localLocks.get( resourceId );
			  if ( resource != null )
			  {
					resource.AcquireReference();
					return true;
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
		 {
			  _stateHolder.incrementActiveClients( this );
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<LockResource> localLocks = localShared(resourceType);
					MutableLongObjectMap<LockResource> localLocks = LocalShared( resourceType );
					foreach ( long resourceId in resourceIds )
					{
						 LockResource resource = localLocks.get( resourceId );
						 if ( resource.ReleaseReference() == 0 )
						 {
							  localLocks.remove( resourceId );
							  _manager.releaseReadLock( new LockResource( resourceType, resourceId ), _lockTransaction );
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
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<LockResource> localLocks = localExclusive(resourceType);
					MutableLongObjectMap<LockResource> localLocks = LocalExclusive( resourceType );
					foreach ( long resourceId in resourceIds )
					{
						 LockResource resource = localLocks.get( resourceId );
						 if ( resource.ReleaseReference() == 0 )
						 {
							  localLocks.remove( resourceId );
							  _manager.releaseWriteLock( new LockResource( resourceType, resourceId ), _lockTransaction );
						 }
					}
			  }
			  finally
			  {
					_stateHolder.decrementActiveClients();
			  }
		 }

		 public override void Prepare()
		 {
			  _stateHolder.prepare( this );
		 }

		 public override void Stop()
		 {
			  // closing client to prevent any new client to come
			  if ( _stateHolder.stopClient() )
			  {
					// wake up and terminate waiters
					TerminateAllWaitersAndWaitForClientsToLeave();
					ReleaseLocks();
			  }
		 }

		 private void TerminateAllWaitersAndWaitForClientsToLeave()
		 {
			  TerminateAllWaiters();
			  // wait for all active clients to go and terminate latecomers
			  while ( _stateHolder.hasActiveClients() )
			  {
					TerminateAllWaiters();
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 20 ) );
			  }
		 }

		 public override void Close()
		 {
			  _stateHolder.closeClient();
			  TerminateAllWaitersAndWaitForClientsToLeave();
			  ReleaseLocks();
		 }

		 private void ReleaseLocks()
		 {
			 lock ( this )
			 {
				  _exclusiveLocks.forEachValue( _typeWriteReleaser );
				  _sharedLocks.forEachValue( _typeReadReleaser );
				  _exclusiveLocks.clear();
				  _sharedLocks.clear();
			 }
		 }

		 // waking up and terminate all waiters that were waiting for any lock for current client
		 private void TerminateAllWaiters()
		 {
			  _manager.accept(@lock =>
			  {
				@lock.terminateLockRequestsForLockTransaction( _lockTransaction );
				return false;
			  });
		 }

		 public virtual int LockSessionId
		 {
			 get
			 {
				  return _lockTransaction.Id;
			 }
		 }

		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  IList<ActiveLock> locks = new List<ActiveLock>();
			  _exclusiveLocks.forEachKeyValue( CollectActiveLocks( locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory_Fields.ExclusiveLock ) );
			  _sharedLocks.forEachKeyValue( CollectActiveLocks( locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory_Fields.SharedLock ) );
			  return locks.stream();
		 }

		 public override long ActiveLockCount()
		 {
			  LockCounter counter = new LockCounter();
			  _exclusiveLocks.forEachKeyValue( counter );
			  _sharedLocks.forEachKeyValue( counter );
			  return counter.Locks;
		 }

		 private class LockCounter : IntObjectProcedure<LongObjectMap<LockResource>>
		 {
			  internal long Locks;

			  public override void Value( int key, LongObjectMap<LockResource> value )
			  {
					Locks += value.size();
			  }
		 }

		 private static IntObjectProcedure<LongObjectMap<LockResource>> CollectActiveLocks( IList<ActiveLock> locks, Neo4Net.Kernel.impl.locking.ActiveLock_Factory activeLock )
		 {
			  return ( typeId, exclusive ) =>
			  {
				ResourceType resourceType = ResourceTypes.fromId( typeId );
				exclusive.forEachKeyValue((resourceId, @lock) =>
				{
					 locks.Add( activeLock.Create( resourceType, resourceId ) );
				});
			  };
		 }

		 private MutableLongObjectMap<LockResource> LocalShared( ResourceType resourceType )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return _sharedLocks.getIfAbsentPut( resourceType.TypeId(), LongObjectHashMap::new );
		 }

		 private MutableLongObjectMap<LockResource> LocalExclusive( ResourceType resourceType )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return _exclusiveLocks.getIfAbsentPut( resourceType.TypeId(), LongObjectHashMap::new );
		 }

		 public override string ToString()
		 {
			  return format( "%s[%d]", this.GetType().Name, LockSessionId );
		 }
	}

}