using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.query
{

	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;

	/// <summary>
	/// Internal representation of the status of an executing query.
	/// <para>
	/// This is used for inspecting the state of a query.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= ExecutingQuery#status </seealso>
	internal abstract class ExecutingQueryStatus
	{
		 internal const string PLANNING_STATE = "planning";
		 internal const string RUNNING_STATE = "running";
		 internal const string WAITING_STATE = "waiting";
		 /// <summary>
		 /// Time in nanoseconds that has been spent waiting in the current state.
		 /// This is the portion of wait time not included in the <seealso cref="ExecutingQuery.waitTimeNanos"/> field.
		 /// </summary>
		 /// <param name="currentTimeNanos">
		 ///         the current timestamp on the nano clock. </param>
		 /// <returns> the time between the time this state started waiting and the provided timestamp. </returns>
		 internal abstract long WaitTimeNanos( long currentTimeNanos );

		 internal abstract IDictionary<string, object> ToMap( long currentTimeNanos );

		 internal abstract string Name();

		 internal virtual bool Planning
		 {
			 get
			 {
				  return false;
			 }
		 }

		 /// <summary>
		 /// Is query waiting on a locks </summary>
		 /// <returns> true if waiting on locks, false otherwise </returns>
		 internal virtual bool WaitingOnLocks
		 {
			 get
			 {
				  return false;
			 }
		 }

		 /// <summary>
		 /// List of locks query is waiting on. Will be empty for all of the statuses except for <seealso cref="WaitingOnLock"/>. </summary>
		 /// <returns> list of locks query is waiting on, empty list if query is not waiting. </returns>
		 internal virtual IList<ActiveLock> WaitingOnLocks()
		 {
			  return Collections.emptyList();
		 }
	}

}