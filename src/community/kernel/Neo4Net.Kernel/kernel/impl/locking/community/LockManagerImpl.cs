using System;
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

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Helpers.Collections;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IllegalResourceException = Neo4Net.Kernel.impl.transaction.IllegalResourceException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

	public class LockManagerImpl
	{
		 private readonly IDictionary<object, RWLock> _resourceLockMap = new Dictionary<object, RWLock>();
		 private readonly RagManager _ragManager;
		 private readonly Clock _clock;

		 /// <summary>
		 /// Time within which any particular lock should be acquired. </summary>
		 /// <seealso cref= GraphDatabaseSettings#lock_acquisition_timeout </seealso>
		 private long _lockAcquisitionTimeoutMillis;

		 public LockManagerImpl( RagManager ragManager, Config config, Clock clock )
		 {
			  this._ragManager = ragManager;
			  this._clock = clock;
			  this._lockAcquisitionTimeoutMillis = config.Get( GraphDatabaseSettings.lock_acquisition_timeout ).toMillis();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean getReadLock(org.Neo4Net.storageengine.api.lock.LockTracer tracer, LockResource resource, Object tx) throws org.Neo4Net.kernel.DeadlockDetectedException, org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual bool GetReadLock( LockTracer tracer, LockResource resource, object tx )
		 {
			  return UnusedResourceGuard( resource, tx, GetRWLockForAcquiring( resource, tx ).acquireReadLock( tracer, tx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean tryReadLock(LockResource resource, Object tx) throws org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual bool TryReadLock( LockResource resource, object tx )
		 {
			  return UnusedResourceGuard( resource, tx, GetRWLockForAcquiring( resource, tx ).tryAcquireReadLock( tx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean getWriteLock(org.Neo4Net.storageengine.api.lock.LockTracer tracer, LockResource resource, Object tx) throws org.Neo4Net.kernel.DeadlockDetectedException, org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual bool GetWriteLock( LockTracer tracer, LockResource resource, object tx )
		 {
			  return UnusedResourceGuard( resource, tx, GetRWLockForAcquiring( resource, tx ).acquireWriteLock( tracer, tx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean tryWriteLock(LockResource resource, Object tx) throws org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual bool TryWriteLock( LockResource resource, object tx )
		 {
			  return UnusedResourceGuard( resource, tx, GetRWLockForAcquiring( resource, tx ).tryAcquireWriteLock( tx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void releaseReadLock(Object resource, Object tx) throws LockNotFoundException, org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual void ReleaseReadLock( object resource, object tx )
		 {
			  GetRWLockForReleasing( resource, tx, 1, 0, true ).releaseReadLock( tx );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void releaseWriteLock(Object resource, Object tx) throws LockNotFoundException, org.Neo4Net.kernel.impl.transaction.IllegalResourceException
		 public virtual void ReleaseWriteLock( object resource, object tx )
		 {
			  GetRWLockForReleasing( resource, tx, 0, 1, true ).releaseWriteLock( tx );
		 }

		 /// <summary>
		 /// Check if lock was obtained and in case if not will try to clear optimistically allocated lock from global
		 /// resource map
		 /// </summary>
		 /// <returns> {@code lockObtained }
		 ///  </returns>
		 private bool UnusedResourceGuard( object resource, object tx, bool lockObtained )
		 {
			  if ( !lockObtained )
			  {
					// if lock was not acquired cleaning up optimistically allocated value
					// for case when it was only used by current call, if it was used by somebody else
					// lock will be released during release call
					GetRWLockForReleasing( resource, tx, 0, 0, false );
			  }
			  return lockObtained;
		 }

		 /// <summary>
		 /// Visit all locks.
		 /// <p/>
		 /// The supplied visitor may not block.
		 /// </summary>
		 /// <param name="visitor"> visitor for visiting each lock. </param>
		 public virtual void Accept( Visitor<RWLock, Exception> visitor )
		 {
			  lock ( _resourceLockMap )
			  {
					foreach ( RWLock @lock in _resourceLockMap.Values )
					{
						 if ( visitor.Visit( @lock ) )
						 {
							  break;
						 }
					}
			  }
		 }

		 private void AssertValidArguments( object resource, object tx )
		 {
			  if ( resource == null || tx == null )
			  {
					throw new IllegalResourceException( "Null parameter: resource = " + resource + ", tx = " + tx );
			  }
		 }

		 private RWLock GetRWLockForAcquiring( LockResource resource, object tx )
		 {
			  AssertValidArguments( resource, tx );
			  lock ( _resourceLockMap )
			  {
					RWLock @lock = _resourceLockMap.computeIfAbsent( resource, k => CreateLock( resource ) );
					@lock.Mark();
					return @lock;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting protected RWLock createLock(LockResource resource)
		 protected internal virtual RWLock CreateLock( LockResource resource )
		 {
			  return new RWLock( resource, _ragManager, _clock, _lockAcquisitionTimeoutMillis );
		 }

		 private RWLock GetRWLockForReleasing( object resource, object tx, int readCountPrerequisite, int writeCountPrerequisite, bool strict )
		 {
			  AssertValidArguments( resource, tx );
			  lock ( _resourceLockMap )
			  {
					RWLock @lock = _resourceLockMap[resource];
					if ( @lock == null )
					{
						 if ( !strict )
						 {
							  return null;
						 }
						 throw new LockNotFoundException( "Lock not found for: " + resource + " tx:" + tx );
					}
					// we need to get info from a couple of synchronized methods
					// to make it info consistent we need to synchronized lock to make sure it will not change between
					// various calls
					//noinspection SynchronizationOnLocalVariableOrMethodParameter
					lock ( @lock )
					{
						 if ( !@lock.Marked && @lock.ReadCount == readCountPrerequisite && @lock.WriteCount == writeCountPrerequisite && @lock.WaitingThreadsCount == 0 )
						 {
							  _resourceLockMap.Remove( resource );
						 }
					}
					return @lock;
			  }
		 }
	}

}