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
namespace Neo4Net.Kernel.impl.locking
{
	using AcquireLockTimeoutException = Neo4Net.Kernel.Api.StorageEngine.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

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
//ORIGINAL LINE: public void acquireShared(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, org.Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws org.Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
			  public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					Fail();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, org.Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws org.Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
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