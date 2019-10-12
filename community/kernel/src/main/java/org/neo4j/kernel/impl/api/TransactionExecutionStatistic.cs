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
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class TransactionExecutionStatistic
	{
		 public static readonly TransactionExecutionStatistic NotAvailable = new TransactionExecutionStatistic();

		 private readonly long? _heapAllocatedBytes;
		 private readonly long? _directAllocatedBytes;
		 private readonly long? _cpuTimeMillis;
		 private readonly long _waitTimeMillis;
		 private readonly long _elapsedTimeMillis;
		 private readonly long? _idleTimeMillis;
		 private readonly long _pageFaults;
		 private readonly long _pageHits;

		 private TransactionExecutionStatistic()
		 {
			  _heapAllocatedBytes = null;
			  _directAllocatedBytes = null;
			  _cpuTimeMillis = null;
			  _waitTimeMillis = -1;
			  _elapsedTimeMillis = -1;
			  _idleTimeMillis = null;
			  _pageFaults = 0;
			  _pageHits = 0;
		 }

		 public TransactionExecutionStatistic( KernelTransactionImplementation tx, SystemNanoClock clock, long startTimeMillis )
		 {
			  long nowMillis = clock.Millis();
			  long nowNanos = clock.Nanos();
			  KernelTransactionImplementation.Statistics statistics = tx.GetStatistics();
			  this._waitTimeMillis = NANOSECONDS.toMillis( statistics.GetWaitingTimeNanos( nowNanos ) );
			  this._heapAllocatedBytes = NullIfNegative( statistics.HeapAllocatedBytes() );
			  this._directAllocatedBytes = NullIfNegative( statistics.DirectAllocatedBytes() );
			  this._cpuTimeMillis = NullIfNegative( statistics.CpuTimeMillis() );
			  this._pageFaults = statistics.TotalTransactionPageCacheFaults();
			  this._pageHits = statistics.TotalTransactionPageCacheHits();
			  this._elapsedTimeMillis = nowMillis - startTimeMillis;
			  this._idleTimeMillis = this._cpuTimeMillis != null ? _elapsedTimeMillis - this._cpuTimeMillis - _waitTimeMillis : null;
		 }

		 public virtual long? HeapAllocatedBytes
		 {
			 get
			 {
				  return _heapAllocatedBytes;
			 }
		 }

		 public virtual long? DirectAllocatedBytes
		 {
			 get
			 {
				  return _directAllocatedBytes;
			 }
		 }

		 public virtual long? CpuTimeMillis
		 {
			 get
			 {
				  return _cpuTimeMillis;
			 }
		 }

		 public virtual long WaitTimeMillis
		 {
			 get
			 {
				  return _waitTimeMillis;
			 }
		 }

		 public virtual long ElapsedTimeMillis
		 {
			 get
			 {
				  return _elapsedTimeMillis;
			 }
		 }

		 public virtual long? IdleTimeMillis
		 {
			 get
			 {
				  return _idleTimeMillis;
			 }
		 }

		 public virtual long PageHits
		 {
			 get
			 {
				  return _pageHits;
			 }
		 }

		 public virtual long PageFaults
		 {
			 get
			 {
				  return _pageFaults;
			 }
		 }

		 private static long? NullIfNegative( long value )
		 {
			  return value >= 0 ? value : null;
		 }
	}

}