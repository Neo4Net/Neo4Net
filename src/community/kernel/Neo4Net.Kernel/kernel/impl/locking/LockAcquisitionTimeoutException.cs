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
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	/// <summary>
	/// Used in lock clients for cases when we unable to acquire a lock for a time that exceed configured
	/// timeout, if any.
	/// </summary>
	/// <seealso cref= Locks.Client </seealso>
	/// <seealso cref= GraphDatabaseSettings#lock_acquisition_timeout </seealso>
	public class LockAcquisitionTimeoutException : TransactionTerminatedException
	{
		 public LockAcquisitionTimeoutException( ResourceType resourceType, long resourceId, long timeoutMillis ) : base( org.Neo4Net.kernel.api.exceptions.Status_Transaction.LockAcquisitionTimeout, string.Format( "Unable to acquire lock for resource: {0} with id: {1:D} within {2:D} millis.", resourceType, resourceId, timeoutMillis ) )
		 {
		 }
	}

}