using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QueryRegistrationOperations = Neo4Net.Kernel.Impl.Api.operations.QueryRegistrationOperations;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MonotonicCounter = Neo4Net.Kernel.impl.util.MonotonicCounter;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public class StackingQueryRegistrationOperations : QueryRegistrationOperations
	{
		 private readonly MonotonicCounter _lastQueryId = MonotonicCounter.newAtomicMonotonicCounter();
		 private readonly SystemNanoClock _clock;
		 private readonly AtomicReference<CpuClock> _cpuClockRef;
		 private readonly AtomicReference<HeapAllocation> _heapAllocationRef;

		 public StackingQueryRegistrationOperations( SystemNanoClock clock, AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef )
		 {
			  this._clock = clock;
			  this._cpuClockRef = cpuClockRef;
			  this._heapAllocationRef = heapAllocationRef;
		 }

		 public override Stream<ExecutingQuery> ExecutingQueries( KernelStatement statement )
		 {
			  return statement.ExecutingQueryList().queries();
		 }

		 public override void RegisterExecutingQuery( KernelStatement statement, ExecutingQuery executingQuery )
		 {
			  statement.StartQueryExecution( executingQuery );
		 }

		 public override ExecutingQuery StartQueryExecution( KernelStatement statement, ClientConnectionInfo clientConnection, string queryText, MapValue queryParameters )
		 {
			  long queryId = _lastQueryId.incrementAndGet();
			  Thread thread = Thread.CurrentThread;
			  long threadId = thread.Id;
			  string threadName = thread.Name;
			  ExecutingQuery executingQuery = new ExecutingQuery( queryId, clientConnection, statement.Username(), queryText, queryParameters, statement.Transaction.MetaData, () => statement.Locks().activeLockCount(), statement.PageCursorTracer, threadId, threadName, _clock, _cpuClockRef.get(), _heapAllocationRef.get() );
			  RegisterExecutingQuery( statement, executingQuery );
			  return executingQuery;
		 }

		 public override void UnregisterExecutingQuery( KernelStatement statement, ExecutingQuery executingQuery )
		 {
			  statement.StopQueryExecution( executingQuery );
		 }
	}


}