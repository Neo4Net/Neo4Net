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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	internal class UpdateLock : ReentrantReadWriteLock
	{
		 internal UpdateLock() : base(true)
		 {
		 }

		 public override string ToString()
		 {
			  return "AbstractKeyValyeStore-UpdateLock[owner = " + Owner +
						 ", is write locked = " + WriteLocked +
						 ", writer holds count = " + WriteHoldCount +
						 ", read holds count = " + ReadHoldCount +
						 ", readers count = " + ReadLockCount +
						 ", threads waiting for write lock = " + QueuedWriterThreads +
						 ", threads waiting for read lock = " + QueuedReaderThreads +
						 "] " + base.ToString();
		 }
	}

}