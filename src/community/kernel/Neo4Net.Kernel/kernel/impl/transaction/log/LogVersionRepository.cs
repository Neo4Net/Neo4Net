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
namespace Neo4Net.Kernel.impl.transaction.log
{
	public interface LogVersionRepository
	{

		 /// <summary>
		 /// Returns the current log version. It is non blocking.
		 /// </summary>
		 long CurrentLogVersion { get;set; }


		 /// <summary>
		 /// Increments (making sure it is persisted on disk) and returns the latest log version for this repository.
		 /// It does so atomically and can potentially block.
		 /// </summary>
		 long IncrementAndGetVersion();
	}

	public static class LogVersionRepository_Fields
	{
		 public const long INITIAL_LOG_VERSION = 0;
	}

}