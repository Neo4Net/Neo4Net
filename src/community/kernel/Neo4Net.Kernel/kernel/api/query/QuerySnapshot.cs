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

	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public class QuerySnapshot
	{
		 private readonly ExecutingQuery _query;
		 private readonly CompilerInfo _compilerInfo;
		 private readonly long _compilationTimeMicros;
		 private readonly long _elapsedTimeMicros;
		 private readonly long _cpuTimeMicros;
		 private readonly long _waitTimeMicros;
		 private readonly string _status;
		 private readonly IDictionary<string, object> _resourceInfo;
		 private readonly IList<ActiveLock> _waitingLocks;
		 private readonly long _activeLockCount;
		 private readonly long _allocatedBytes;
		 private readonly PageCounterValues _page;

		 internal QuerySnapshot( ExecutingQuery query, CompilerInfo compilerInfo, PageCounterValues page, long compilationTimeMicros, long elapsedTimeMicros, long cpuTimeMicros, long waitTimeMicros, string status, IDictionary<string, object> resourceInfo, IList<ActiveLock> waitingLocks, long activeLockCount, long allocatedBytes )
		 {
			  this._query = query;
			  this._compilerInfo = compilerInfo;
			  this._page = page;
			  this._compilationTimeMicros = compilationTimeMicros;
			  this._elapsedTimeMicros = elapsedTimeMicros;
			  this._cpuTimeMicros = cpuTimeMicros;
			  this._waitTimeMicros = waitTimeMicros;
			  this._status = status;
			  this._resourceInfo = resourceInfo;
			  this._waitingLocks = waitingLocks;
			  this._activeLockCount = activeLockCount;
			  this._allocatedBytes = allocatedBytes;
		 }

		 public virtual long InternalQueryId()
		 {
			  return _query.internalQueryId();
		 }

		 public virtual string QueryText()
		 {
			  return _query.queryText();
		 }

		 public virtual System.Func<ExecutionPlanDescription> QueryPlanSupplier()
		 {
			  return _query.planDescriptionSupplier();
		 }

		 public virtual MapValue QueryParameters()
		 {
			  return _query.queryParameters();
		 }

		 public virtual string Username()
		 {
			  return _query.username();
		 }

		 public virtual ClientConnectionInfo ClientConnection()
		 {
			  return _query.clientConnection();
		 }

		 public virtual IDictionary<string, object> TransactionAnnotationData()
		 {
			  return _query.transactionAnnotationData();
		 }

		 public virtual long ActiveLockCount()
		 {
			  return _activeLockCount;
		 }

		 public virtual string Planner()
		 {
			  return _compilerInfo == null ? null : _compilerInfo.planner();
		 }

		 public virtual string Runtime()
		 {
			  return _compilerInfo == null ? null : _compilerInfo.runtime();
		 }

		 public virtual IList<IDictionary<string, string>> Indexes()
		 {
			  if ( _compilerInfo == null )
			  {
					return Collections.emptyList();
			  }
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _compilerInfo.indexes().Select(IndexUsage::asMap).ToList();
		 }

		 public virtual string Status()
		 {
			  return _status;
		 }

		 public virtual IDictionary<string, object> ResourceInformation()
		 {
			  return _resourceInfo;
		 }

		 public virtual long StartTimestampMillis()
		 {
			  return _query.startTimestampMillis();
		 }

		 /// <summary>
		 /// The time spent planning the query, before the query actually starts executing.
		 /// </summary>
		 /// <returns> the time in microseconds spent planning the query. </returns>
		 public virtual long CompilationTimeMicros()
		 {
			  return _compilationTimeMicros;
		 }

		 /// <summary>
		 /// The time that has been spent waiting on locks or other queries, as opposed to actively executing this query.
		 /// </summary>
		 /// <returns> the time in microseconds spent waiting on locks. </returns>
		 public virtual long WaitTimeMicros()
		 {
			  return _waitTimeMicros;
		 }

		 /// <summary>
		 /// The time (wall time) that has elapsed since the execution of this query started.
		 /// </summary>
		 /// <returns> the time in microseconds since execution of this query started. </returns>
		 public virtual long ElapsedTimeMicros()
		 {
			  return _elapsedTimeMicros;
		 }

		 /// <summary>
		 /// Time that the CPU has actively spent working on things related to this query.
		 /// </summary>
		 /// <returns> the time in microseconds that the CPU has spent on this query, or {@code null} if the cpu time could not
		 /// be measured. </returns>
		 public virtual long? CpuTimeMicros()
		 {
			  return _cpuTimeMicros < 0 ? null : _cpuTimeMicros;
		 }

		 /// <summary>
		 /// Time from the start of this query that the computer spent doing other things than working on this query, even
		 /// though the query was runnable.
		 /// <para>
		 /// In rare cases the idle time can be negative. This is due to the fact that the Thread does not go to sleep
		 /// immediately after we start measuring the wait-time, there is still some "lock bookkeeping time" that counts as
		 /// both cpu time (because the CPU is actually actively working on this thread) and wait time (because the query is
		 /// actually waiting on the lock rather than doing active work). In most cases such "lock bookkeeping time" is going
		 /// to be dwarfed by the idle time.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the time in microseconds that this query was de-scheduled, or {@code null} if the cpu time could not be
		 /// measured. </returns>
		 public virtual long? IdleTimeMicros()
		 {
			  return _cpuTimeMicros < 0 ? null : ( _elapsedTimeMicros - _cpuTimeMicros - _waitTimeMicros );
		 }

		 /// <summary>
		 /// The number of bytes allocated by the query.
		 /// </summary>
		 /// <returns> the number of bytes allocated by the execution of the query, or {@code null} if the memory allocation
		 /// could not be measured. </returns>
		 public virtual long? AllocatedBytes()
		 {
			  return _allocatedBytes < 0 ? null : _allocatedBytes;
		 }

		 public virtual long PageHits()
		 {
			  return _page.hits;
		 }

		 public virtual long PageFaults()
		 {
			  return _page.faults;
		 }

		 public virtual IList<ActiveLock> WaitingLocks()
		 {
			  return _waitingLocks;
		 }
	}

}