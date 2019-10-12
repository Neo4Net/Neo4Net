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
namespace Neo4Net.Storageengine.Api.@lock
{

	public interface ResourceLocker
	{
		 /// <summary>
		 /// Can be grabbed when no other client holds locks on the relevant resources. No other clients can hold locks
		 /// while one client holds an exclusive lock. If the lock cannot be acquired,
		 /// behavior is specified by the <seealso cref="WaitStrategy"/> for the given <seealso cref="ResourceType"/>.
		 /// </summary>
		 /// <param name="tracer"> </param>
		 /// <param name="resourceType"> type or resource(s) to lock. </param>
		 /// <param name="resourceIds"> id(s) of resources to lock. Multiple ids should be ordered consistently by all callers </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acquireExclusive(LockTracer tracer, ResourceType resourceType, long... resourceIds) throws AcquireLockTimeoutException;
		 void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds );
	}

	public static class ResourceLocker_Fields
	{
		 public static readonly ResourceLocker None = ( tracer, resourceType, resourceIds ) =>
		 {
		  throw new System.NotSupportedException( "Unexpected call to lock a resource " + resourceType + " " + Arrays.ToString( resourceIds ) );
		 };

	}

}