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
namespace Neo4Net.Kernel.impl.transaction.tracing
{
	/// <summary>
	/// This represents the actual force call for the transaction log file, and so is a measure of the {@code f(data)sync}
	/// system call latency that we experience. Force calls are batched, so a single one might cause multiple transactions
	/// to be considered forced.
	/// </summary>
	public interface LogForceEvent : AutoCloseable
	{

		 /// <summary>
		 /// Marks the end of the force call on the transaction log file.
		 /// </summary>
		 void Close();
	}

	public static class LogForceEvent_Fields
	{
		 public static readonly LogForceEvent Null = () =>
		 {
		 };

	}

}