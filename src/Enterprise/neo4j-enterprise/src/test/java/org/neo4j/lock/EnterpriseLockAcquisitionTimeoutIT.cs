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
namespace Neo4Net.@lock
{

	using ForsetiClient = Neo4Net.Kernel.impl.enterprise.@lock.forseti.ForsetiClient;
	using ForsetiLockManager = Neo4Net.Kernel.impl.enterprise.@lock.forseti.ForsetiLockManager;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using CommunityLockAcquisitionTimeoutIT = Neo4Net.Locking.CommunityLockAcquisitionTimeoutIT;
	using Neo4Net.Test;

	public class EnterpriseLockAcquisitionTimeoutIT : CommunityLockAcquisitionTimeoutIT
	{
		 protected internal override System.Predicate<OtherThreadExecutor.WaitDetails> ExclusiveLockWaitingPredicate()
		 {
			  return waitDetails => waitDetails.isAt( typeof( ForsetiClient ), "acquireExclusive" );
		 }

		 protected internal override System.Predicate<OtherThreadExecutor.WaitDetails> SharedLockWaitingPredicate()
		 {
			  return waitDetails => waitDetails.isAt( typeof( ForsetiClient ), "acquireShared" );
		 }

		 protected internal override Locks LockManager
		 {
			 get
			 {
				  return DependencyResolver.resolveDependency( typeof( ForsetiLockManager ) );
			 }
		 }
	}

}