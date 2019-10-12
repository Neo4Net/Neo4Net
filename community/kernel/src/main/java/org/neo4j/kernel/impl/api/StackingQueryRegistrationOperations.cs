using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using ExecutingQuery = Org.Neo4j.Kernel.api.query.ExecutingQuery;
	using QueryRegistrationOperations = Org.Neo4j.Kernel.Impl.Api.operations.QueryRegistrationOperations;
	using ClientConnectionInfo = Org.Neo4j.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MonotonicCounter = Org.Neo4j.Kernel.impl.util.MonotonicCounter;
	using CpuClock = Org.Neo4j.Resources.CpuClock;
	using HeapAllocation = Org.Neo4j.Resources.HeapAllocation;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

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