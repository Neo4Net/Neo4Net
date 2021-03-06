﻿/*
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
namespace Org.Neo4j.Kernel.impl.locking
{
	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	public class ReadOnlyLocks : Locks
	{
		 private static readonly ReadOnlyClient _readOnlyClient = new ReadOnlyClient();

		 public override Locks_Client NewClient()
		 {
			  return _readOnlyClient;
		 }

		 public override void Accept( Locks_Visitor visitor )
		 {
		 }

		 public override void Close()
		 {
		 }

		 private class ReadOnlyClient : NoOpClient
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					Fail();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					Fail();
			  }

			  public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
			  {
					Fail();
					return false;
			  }

			  public override bool TrySharedLock( ResourceType resourceType, long resourceId )
			  {
					Fail();
					return false;
			  }

			  public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
			  {
					Fail();
			  }

			  public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
			  {
					Fail();
			  }

			  internal virtual void Fail()
			  {
					throw new System.InvalidOperationException( "Cannot acquire locks in read only mode" );
			  }
		 }
	}

}