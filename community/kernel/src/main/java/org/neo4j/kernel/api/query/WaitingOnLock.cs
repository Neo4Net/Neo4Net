﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.api.query
{

	using ActiveLock = Org.Neo4j.Kernel.impl.locking.ActiveLock;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	internal class WaitingOnLock : ExecutingQueryStatus
	{
		 private readonly string _mode;
		 private readonly ResourceType _resourceType;
		 private readonly long[] _resourceIds;
		 private readonly long _startTimeNanos;

		 internal WaitingOnLock( string mode, ResourceType resourceType, long[] resourceIds, long startTimeNanos )
		 {
			  this._mode = mode;
			  this._resourceType = resourceType;
			  this._resourceIds = resourceIds;
			  this._startTimeNanos = startTimeNanos;
		 }

		 internal override long WaitTimeNanos( long currentTimeNanos )
		 {
			  return currentTimeNanos - _startTimeNanos;
		 }

		 internal override IDictionary<string, object> ToMap( long currentTimeNanos )
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["lockMode"] = _mode;
			  map["waitTimeMillis"] = TimeUnit.NANOSECONDS.toMillis( WaitTimeNanos( currentTimeNanos ) );
			  map["resourceType"] = _resourceType.ToString();
			  map["resourceIds"] = _resourceIds;
			  return map;
		 }

		 internal override string Name()
		 {
			  return WAITING_STATE;
		 }

		 internal override bool WaitingOnLocks
		 {
			 get
			 {
				  return true;
			 }
		 }

		 internal override IList<ActiveLock> WaitingOnLocks()
		 {
			  IList<ActiveLock> locks = new List<ActiveLock>();
			  switch ( _mode )
			  {
			  case Org.Neo4j.Kernel.impl.locking.ActiveLock_Fields.EXCLUSIVE_MODE:

					foreach ( long resourceId in _resourceIds )
					{
						 locks.Add( ActiveLock.exclusiveLock( _resourceType, resourceId ) );
					}
					break;
			  case Org.Neo4j.Kernel.impl.locking.ActiveLock_Fields.SHARED_MODE:
					foreach ( long resourceId in _resourceIds )
					{
						 locks.Add( ActiveLock.sharedLock( _resourceType, resourceId ) );
					}
					break;
			  default:
					throw new System.ArgumentException( "Unsupported type of lock mode: " + _mode );
			  }
			  return locks;
		 }
	}

}