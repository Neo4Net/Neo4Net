using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.readreplica
{

	using ReadOnlyDbException = Neo4Net.Kernel.Api.Exceptions.ReadOnlyDbException;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	public class ReadReplicaLockManager : Locks
	{
		 public override Neo4Net.Kernel.impl.locking.Locks_Client NewClient()
		 {
			  return new Client( this );
		 }

		 public override void Accept( Neo4Net.Kernel.impl.locking.Locks_Visitor visitor )
		 {
		 }

		 public override void Close()
		 {
		 }

		 private class Client : Neo4Net.Kernel.impl.locking.Locks_Client
		 {
			 private readonly ReadReplicaLockManager _outerInstance;

			 public Client( ReadReplicaLockManager outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					throw new Exception( new ReadOnlyDbException() );
			  }

			  public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
			  {
					throw new Exception( new ReadOnlyDbException() );
			  }

			  public override bool TrySharedLock( ResourceType resourceType, long resourceId )
			  {
					return false;
			  }

			  public override bool ReEnterShared( ResourceType resourceType, long resourceId )
			  {
					return false;
			  }

			  public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
			  {
					throw new System.InvalidOperationException( "Should never happen" );
			  }

			  public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
			  {
			  }

			  public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
			  {
					throw new System.InvalidOperationException( "Should never happen" );
			  }

			  public override void Prepare()
			  {
			  }

			  public override void Stop()
			  {
			  }

			  public override void Close()
			  {
			  }

			  public virtual int LockSessionId
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public override Stream<ActiveLock> ActiveLocks()
			  {
					return Stream.empty();
			  }

			  public override long ActiveLockCount()
			  {
					return 0;
			  }
		 }
	}

}