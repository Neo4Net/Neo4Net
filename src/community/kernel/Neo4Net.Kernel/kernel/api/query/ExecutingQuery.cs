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
	using ToStringBuilder = org.apache.commons.lang3.builder.ToStringBuilder;


	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using PageCursorCounters = Neo4Net.Io.pagecache.tracing.cursor.PageCursorCounters;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Kernel.Api.StorageEngine.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;
	using MapValue = Neo4Net.Values.@virtual.MapValue;


	/// <summary>
	/// Represents a currently running query.
	/// </summary>
	public class ExecutingQuery
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_lockTracer = this.waitForLock;
		}

		 private static readonly AtomicLongFieldUpdater<ExecutingQuery> _waitTime = newUpdater( typeof( ExecutingQuery ), "waitTimeNanos" );
		 private readonly long _queryId;
		 private LockTracer _lockTracer;
		 private readonly PageCursorCounters _pageCursorCounters;
		 private readonly string _username;
		 private readonly ClientConnectionInfo _clientConnection;
		 private readonly string _queryText;
		 private readonly MapValue _queryParameters;
		 private readonly long _startTimeNanos;
		 private readonly long _startTimestampMillis;
		 /// <summary>
		 /// Uses write barrier of <seealso cref="status"/>. </summary>
		 private long _compilationCompletedNanos;
		 private System.Func<ExecutionPlanDescription> _planDescriptionSupplier;
		 private readonly long _threadExecutingTheQueryId;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "FieldCanBeLocal"}) private final String threadExecutingTheQueryName;
		 private readonly string _threadExecutingTheQueryName;
		 private readonly System.Func<long> _activeLockCount;
		 private readonly long _initialActiveLocks;
		 private readonly SystemNanoClock _clock;
		 private readonly CpuClock _cpuClock;
		 private readonly HeapAllocation _heapAllocation;
		 private readonly long _cpuTimeNanosWhenQueryStarted;
		 private readonly long _heapAllocatedBytesWhenQueryStarted;
		 private readonly IDictionary<string, object> _transactionAnnotationData;
		 /// <summary>
		 /// Uses write barrier of <seealso cref="status"/>. </summary>
		 private CompilerInfo _compilerInfo;
		 private volatile ExecutingQueryStatus _status = SimpleState.Planning();
		 /// <summary>
		 /// Updated through <seealso cref="WAIT_TIME"/> </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long waitTimeNanos;
		 private volatile long _waitTimeNanos;

		 public ExecutingQuery( long queryId, ClientConnectionInfo clientConnection, string username, string queryText, MapValue queryParameters, IDictionary<string, object> transactionAnnotationData, System.Func<long> activeLockCount, PageCursorCounters pageCursorCounters, long threadExecutingTheQueryId, string threadExecutingTheQueryName, SystemNanoClock clock, CpuClock cpuClock, HeapAllocation heapAllocation )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  // Capture timestamps first
			  this._cpuTimeNanosWhenQueryStarted = cpuClock.CpuTimeNanos( threadExecutingTheQueryId );
			  this._startTimeNanos = clock.Nanos();
			  this._startTimestampMillis = clock.Millis();
			  // then continue with assigning fields
			  this._queryId = queryId;
			  this._clientConnection = clientConnection;
			  this._pageCursorCounters = pageCursorCounters;
			  this._username = username;

			  ISet<string> passwordParams = new HashSet<string>();
			  this._queryText = QueryObfuscation.ObfuscateText( queryText, passwordParams );
			  this._queryParameters = QueryObfuscation.ObfuscateParams( queryParameters, passwordParams );
			  this._transactionAnnotationData = transactionAnnotationData;
			  this._activeLockCount = activeLockCount;
			  this._initialActiveLocks = activeLockCount();
			  this._threadExecutingTheQueryId = threadExecutingTheQueryId;
			  this._threadExecutingTheQueryName = threadExecutingTheQueryName;
			  this._cpuClock = cpuClock;
			  this._heapAllocation = heapAllocation;
			  this._clock = clock;
			  this._heapAllocatedBytesWhenQueryStarted = heapAllocation.AllocatedBytes( this._threadExecutingTheQueryId );
		 }

		 // update state

		 public virtual void CompilationCompleted( CompilerInfo compilerInfo, System.Func<ExecutionPlanDescription> planDescriptionSupplier )
		 {
			  this._compilerInfo = compilerInfo;
			  this._compilationCompletedNanos = _clock.nanos();
			  this._planDescriptionSupplier = planDescriptionSupplier;
			  this._status = SimpleState.Running(); // write barrier - must be last
		 }

		 public virtual LockTracer LockTracer()
		 {
			  return _lockTracer;
		 }

		 public virtual void WaitsForQuery( ExecutingQuery child )
		 {
			  if ( child == null )
			  {
					_waitTime.addAndGet( this, _status.waitTimeNanos( _clock.nanos() ) );
					this._status = SimpleState.Running();
			  }
			  else
			  {
					this._status = new WaitingOnQuery( child, _clock.nanos() );
			  }
		 }

		 // snapshot state

		 public virtual QuerySnapshot Snapshot()
		 {
			  // capture a consistent snapshot of the "live" state
			  ExecutingQueryStatus status;
			  long waitTimeNanos;
			  long currentTimeNanos;
			  long cpuTimeNanos;
			  do
			  {
					status = this._status; // read barrier, must be first
					waitTimeNanos = this._waitTimeNanos; // the reason for the retry loop: don't count the wait time twice
					cpuTimeNanos = _cpuClock.cpuTimeNanos( _threadExecutingTheQueryId );
					currentTimeNanos = _clock.nanos(); // capture the time as close to the snapshot as possible
			  } while ( this._status != status );
			  // guarded by barrier - unused if status is planning, stable otherwise
			  long compilationCompletedNanos = this._compilationCompletedNanos;
			  // guarded by barrier - like compilationCompletedNanos
			  CompilerInfo planner = status.Planning ? null : this._compilerInfo;
			  IList<ActiveLock> waitingOnLocks = status.WaitingOnLocks ? status.WaitingOnLocks() : Collections.emptyList();
			  // activeLockCount is not atomic to capture, so we capture it after the most sensitive part.
			  long totalActiveLocks = this._activeLockCount.AsLong;
			  // just needs to be captured at some point...
			  long heapAllocatedBytes = _heapAllocation.allocatedBytes( _threadExecutingTheQueryId );
			  PageCounterValues pageCounters = new PageCounterValues( _pageCursorCounters );

			  // - at this point we are done capturing the "live" state, and can start computing the snapshot -
			  long compilationTimeNanos = ( status.Planning ? currentTimeNanos : compilationCompletedNanos ) - _startTimeNanos;
			  long elapsedTimeNanos = currentTimeNanos - _startTimeNanos;
			  cpuTimeNanos -= _cpuTimeNanosWhenQueryStarted;
			  waitTimeNanos += status.WaitTimeNanos( currentTimeNanos );
			  // TODO: when we start allocating native memory as well during query execution,
			  // we should have a tracer that keeps track of how much memory we have allocated for the query,
			  // and get the value from that here.
			  heapAllocatedBytes = _heapAllocatedBytesWhenQueryStarted < 0 ? -1 : heapAllocatedBytes - _heapAllocatedBytesWhenQueryStarted;

			  return new QuerySnapshot( this, planner, pageCounters, NANOSECONDS.toMicros( compilationTimeNanos ), NANOSECONDS.toMicros( elapsedTimeNanos ), cpuTimeNanos == 0 && _cpuTimeNanosWhenQueryStarted == -1 ? -1 : NANOSECONDS.toMicros( cpuTimeNanos ), NANOSECONDS.toMicros( waitTimeNanos ), status.Name(), status.ToMap(currentTimeNanos), waitingOnLocks, totalActiveLocks - _initialActiveLocks, heapAllocatedBytes );
		 }

		 // basic methods

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ExecutingQuery that = ( ExecutingQuery ) o;

			  return _queryId == that._queryId;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( _queryId ^ ( ( long )( ( ulong )_queryId >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return ToStringBuilder.reflectionToString( this );
		 }

		 // access stable state

		 public virtual long InternalQueryId()
		 {
			  return _queryId;
		 }

		 public virtual string Username()
		 {
			  return _username;
		 }

		 public virtual string QueryText()
		 {
			  return _queryText;
		 }

		 public virtual System.Func<ExecutionPlanDescription> PlanDescriptionSupplier()
		 {
			  return _planDescriptionSupplier;
		 }

		 public virtual MapValue QueryParameters()
		 {
			  return _queryParameters;
		 }

		 public virtual long StartTimestampMillis()
		 {
			  return _startTimestampMillis;
		 }

		 public virtual long ElapsedNanos()
		 {
			  return _clock.nanos() - _startTimeNanos;
		 }

		 public virtual IDictionary<string, object> TransactionAnnotationData()
		 {
			  return _transactionAnnotationData;
		 }

		 public virtual long ReportedWaitingTimeNanos()
		 {
			  return _waitTimeNanos;
		 }

		 public virtual long TotalWaitingTimeNanos( long currentTimeNanos )
		 {
			  return _waitTimeNanos + _status.waitTimeNanos( currentTimeNanos );
		 }

		 internal virtual ClientConnectionInfo ClientConnection()
		 {
			  return _clientConnection;
		 }

		 private LockWaitEvent WaitForLock( bool exclusive, ResourceType resourceType, long[] resourceIds )
		 {
			  WaitingOnLockEvent @event = new WaitingOnLockEvent( exclusive ? Neo4Net.Kernel.impl.locking.ActiveLock_Fields.EXCLUSIVE_MODE : Neo4Net.Kernel.impl.locking.ActiveLock_Fields.SHARED_MODE, resourceType, resourceIds, this, _clock.nanos(), _status );
			  _status = @event;
			  return @event;
		 }

		 internal virtual void DoneWaitingOnLock( WaitingOnLockEvent waiting )
		 {
			  if ( _status != waiting )
			  {
					return; // already closed
			  }
			  _waitTime.addAndGet( this, waiting.WaitTimeNanos( _clock.nanos() ) );
			  _status = waiting.PreviousStatus();
		 }
	}

}