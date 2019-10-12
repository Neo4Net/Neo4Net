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
namespace Org.Neo4j.Kernel.impl.transaction.tracing
{
	/// <summary>
	/// Represents the complete process of forcing the transaction log file, after transaction commands have been written
	/// to the log. This transaction might not actually end up performing the force itself, but could be included in a
	/// force batch performed by another thread. This is the total force latency that this thread experienced.
	/// </summary>
	public interface LogForceWaitEvent : AutoCloseable
	{

		 /// <summary>
		 /// Mark the end of forcing the transaction log.
		 /// </summary>
		 void Close();
	}

	public static class LogForceWaitEvent_Fields
	{
		 public static readonly LogForceWaitEvent Null = () =>
		 {
		 };

	}

}