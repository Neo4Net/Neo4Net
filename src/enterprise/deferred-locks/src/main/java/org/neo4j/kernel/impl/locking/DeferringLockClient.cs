using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.locking
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;


	using AcquireLockTimeoutException = Neo4Net.Kernel.Api.StorageEngine.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	public class DeferringLockClient : Locks_Client
	{
		 private readonly Locks_Client _clientDelegate;
		 private readonly IDictionary<LockUnit, MutableInt> _locks = new SortedDictionary<LockUnit, MutableInt>();
		 private volatile bool _stopped;

		 public DeferringLockClient( Locks_Client clientDelegate )
		 {
			  this._clientDelegate = clientDelegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, org.Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws org.Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
		 public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  foreach ( long resourceId in resourceIds )
			  {
					AddLock( resourceType, resourceId, false );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, org.Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws org.Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
		 public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  foreach ( long resourceId in resourceIds )
			  {
					AddLock( resourceType, resourceId, true );
			  }
		 }

		 public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException( "Should not be needed" );
		 }

		 public override bool TrySharedLock( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException( "Should not be needed" );
		 }

		 public override bool ReEnterShared( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException( "Should not be needed" );
		 }

		 public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException( "Should not be needed" );
		 }

		 public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();
			  foreach ( long resourceId in resourceIds )
			  {
					RemoveLock( resourceType, resourceId, false );
			  }

		 }

		 public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();
			  foreach ( long resourceId in resourceIds )
			  {
					RemoveLock( resourceType, resourceId, true );
			  }
		 }

		 internal virtual void AcquireDeferredLocks( LockTracer lockTracer )
		 {
			  AssertNotStopped();

			  long[] current = new long[10];
			  int cursor = 0;
			  ResourceType currentType = null;
			  bool currentExclusive = false;
			  foreach ( LockUnit lockUnit in _locks.Keys )
			  {
					if ( currentType == null || ( currentType.TypeId() != lockUnit.ResourceType().typeId() || currentExclusive != lockUnit.Exclusive ) )
					{
						 // New type, i.e. flush the current array down to delegate in one call
						 FlushLocks( lockTracer, current, cursor, currentType, currentExclusive );

						 cursor = 0;
						 currentType = lockUnit.ResourceType();
						 currentExclusive = lockUnit.Exclusive;
					}

					// Queue into current batch
					if ( cursor == current.Length )
					{
						 current = Arrays.copyOf( current, cursor * 2 );
					}
					current[cursor++] = lockUnit.ResourceId();
			  }
			  FlushLocks( lockTracer, current, cursor, currentType, currentExclusive );
		 }

		 private void FlushLocks( LockTracer lockTracer, long[] current, int cursor, ResourceType currentType, bool exclusive )
		 {
			  if ( cursor > 0 )
			  {
					long[] resourceIds = Arrays.copyOf( current, cursor );
					if ( exclusive )
					{
						 _clientDelegate.acquireExclusive( lockTracer, currentType, resourceIds );
					}
					else
					{
						 _clientDelegate.acquireShared( lockTracer, currentType, resourceIds );
					}
			  }
		 }

		 public override void Prepare()
		 {
			  _clientDelegate.prepare();
		 }

		 public override void Stop()
		 {
			  _stopped = true;
			  _clientDelegate.stop();
		 }

		 public override void Close()
		 {
			  _stopped = true;
			  _clientDelegate.close();
		 }

		 public virtual int LockSessionId
		 {
			 get
			 {
				  return _clientDelegate.LockSessionId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends ActiveLock> activeLocks()
		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return _locks.Keys.stream();
		 }

		 public override long ActiveLockCount()
		 {
			  return _locks.Count;
		 }

		 private void AssertNotStopped()
		 {
			  if ( _stopped )
			  {
					throw new LockClientStoppedException( this );
			  }
		 }

		 private void AddLock( ResourceType resourceType, long resourceId, bool exclusive )
		 {
			  LockUnit lockUnit = new LockUnit( resourceType, resourceId, exclusive );
			  MutableInt lockCount = _locks.computeIfAbsent( lockUnit, k => new MutableInt() );
			  lockCount.increment();
		 }

		 private void RemoveLock( ResourceType resourceType, long resourceId, bool exclusive )
		 {
			  LockUnit lockUnit = new LockUnit( resourceType, resourceId, exclusive );
			  MutableInt lockCount = _locks[lockUnit];
			  if ( lockCount == null )
			  {
					throw new System.InvalidOperationException( "Cannot release " + ( exclusive ? "exclusive" : "shared" ) + " lock that it " + "does not hold: " + resourceType + "[" + resourceId + "]." );
			  }

			  lockCount.decrement();

			  if ( lockCount.intValue() == 0 )
			  {
					_locks.Remove( lockUnit );
			  }
		 }
	}

}