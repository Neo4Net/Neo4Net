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
namespace Org.Neo4j.Kernel.impl.locking
{
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	/// <summary>
	/// Used in lock clients for cases when we unable to acquire a lock for a time that exceed configured
	/// timeout, if any.
	/// </summary>
	/// <seealso cref= Locks.Client </seealso>
	/// <seealso cref= GraphDatabaseSettings#lock_acquisition_timeout </seealso>
	public class LockAcquisitionTimeoutException : TransactionTerminatedException
	{
		 public LockAcquisitionTimeoutException( ResourceType resourceType, long resourceId, long timeoutMillis ) : base( org.neo4j.kernel.api.exceptions.Status_Transaction.LockAcquisitionTimeout, string.Format( "Unable to acquire lock for resource: {0} with id: {1:D} within {2:D} millis.", resourceType, resourceId, timeoutMillis ) )
		 {
		 }
	}

}