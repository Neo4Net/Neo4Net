using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.@lock.trace
{

	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Storageengine.Api.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	public class RecordingLockTracer : LockTracer
	{
		 private IList<LockRecord> _requestedLocks = new CopyOnWriteArrayList<LockRecord>();

		 public override LockWaitEvent WaitForLock( bool exclusive, ResourceType resourceType, params long[] resourceIds )
		 {
			  foreach ( long resourceId in resourceIds )
			  {
					_requestedLocks.Add( LockRecord.Of( exclusive, resourceType, resourceId ) );
			  }
			  return null;
		 }

		 public virtual IList<LockRecord> RequestedLocks
		 {
			 get
			 {
				  return _requestedLocks;
			 }
		 }
	}

}