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
namespace Neo4Net.Kernel.api.query
{
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Storageengine.Api.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	/// <summary>
	/// This is both a status state in the state machine of <seealso cref="ExecutingQuery"/>, and a <seealso cref="LockWaitEvent"/>.
	/// The reason for this is to avoid unnecessary object allocation and indirection, since there is always a one-to-one
	/// mapping between the status corresponding to the lock we are waiting on (caused by
	/// {@link LockTracer#waitForLock(boolean, ResourceType, long...) the event of waiting
	/// on a lock}) and the event object used to <seealso cref="LockWaitEvent.close() signal the end of the wait"/>.
	/// </summary>
	internal class WaitingOnLockEvent : WaitingOnLock, LockWaitEvent
	{
		 private readonly ExecutingQueryStatus _previous;
		 private readonly ExecutingQuery _executingQuery;

		 internal WaitingOnLockEvent( string mode, ResourceType resourceType, long[] resourceIds, ExecutingQuery executingQuery, long currentTimeNanos, ExecutingQueryStatus previous ) : base( mode, resourceType, resourceIds, currentTimeNanos )
		 {
			  this._executingQuery = executingQuery;
			  this._previous = previous;
		 }

		 internal virtual ExecutingQueryStatus PreviousStatus()
		 {
			  return _previous;
		 }

		 public override void Close()
		 {
			  _executingQuery.doneWaitingOnLock( this );
		 }

		 internal override bool Planning
		 {
			 get
			 {
				  return _previous.Planning;
			 }
		 }
	}

}