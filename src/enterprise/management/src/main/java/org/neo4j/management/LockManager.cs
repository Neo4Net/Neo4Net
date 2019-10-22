using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.management
{

	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;
	using LockInfo = Neo4Net.Kernel.info.LockInfo;

	[ManagementInterface(name : LockManager_Fields.NAME), Description("Information about the Neo4Net lock status")]
	public interface LockManager
	{

		 [Description("The number of lock sequences that would have lead to a deadlock situation that " + "Neo4Net has detected and averted (by throwing DeadlockDetectedException).")]
		 long NumberOfAvertedDeadlocks { get; }

		 [Description("Information about all locks held by Neo4Net")]
		 IList<LockInfo> Locks { get; }

		 [Description("Information about contended locks (locks where at least one thread is waiting) held by Neo4Net. " + "The parameter is used to get locks where threads have waited for at least the specified number " + "of milliseconds, a value of 0 retrieves all contended locks.")]
		 IList<LockInfo> GetContendedLocks( long minWaitTime );
	}

	public static class LockManager_Fields
	{
		 public const string NAME = "Locking";
	}

}