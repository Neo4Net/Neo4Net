using System;

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
namespace Neo4Net.Kernel.impl.transaction.stats
{

	public class DatabaseTransactionStats : TransactionMonitor, TransactionCounters
	{
		 private readonly AtomicLong _activeReadTransactionCount = new AtomicLong();
		 private readonly LongAdder _startedTransactionCount = new LongAdder();
		 private readonly LongAdder _activeWriteTransactionCount = new LongAdder();
		 private readonly LongAdder _committedReadTransactionCount = new LongAdder();
		 private readonly LongAdder _committedWriteTransactionCount = new LongAdder();
		 private readonly LongAdder _rolledBackReadTransactionCount = new LongAdder();
		 private readonly LongAdder _rolledBackWriteTransactionCount = new LongAdder();
		 private readonly LongAdder _terminatedReadTransactionCount = new LongAdder();
		 private readonly LongAdder _terminatedWriteTransactionCount = new LongAdder();
		 private volatile long _peakTransactionCount;

		 public override void TransactionStarted()
		 {
			  _startedTransactionCount.increment();
			  long active = _activeReadTransactionCount.incrementAndGet();
			  _peakTransactionCount = Math.Max( _peakTransactionCount, active );
		 }

		 public override void TransactionFinished( bool committed, bool write )
		 {
			  if ( write )
			  {
					_activeWriteTransactionCount.decrement();
			  }
			  else
			  {
					_activeReadTransactionCount.decrementAndGet();
			  }
			  if ( committed )
			  {
					IncrementCounter( _committedReadTransactionCount, _committedWriteTransactionCount, write );
			  }
			  else
			  {
					IncrementCounter( _rolledBackReadTransactionCount, _rolledBackWriteTransactionCount, write );
			  }
		 }

		 public override void TransactionTerminated( bool write )
		 {
			  IncrementCounter( _terminatedReadTransactionCount, _terminatedWriteTransactionCount, write );
		 }

		 public override void UpgradeToWriteTransaction()
		 {
			  _activeReadTransactionCount.decrementAndGet();
			  _activeWriteTransactionCount.increment();
		 }

		 public virtual long PeakConcurrentNumberOfTransactions
		 {
			 get
			 {
				  return _peakTransactionCount;
			 }
		 }

		 public virtual long NumberOfStartedTransactions
		 {
			 get
			 {
				  return _startedTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfCommittedTransactions
		 {
			 get
			 {
				  return NumberOfCommittedReadTransactions + NumberOfCommittedWriteTransactions;
			 }
		 }

		 public virtual long NumberOfCommittedReadTransactions
		 {
			 get
			 {
				  return _committedReadTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfCommittedWriteTransactions
		 {
			 get
			 {
				  return _committedWriteTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfActiveTransactions
		 {
			 get
			 {
				  return NumberOfActiveReadTransactions + NumberOfActiveWriteTransactions;
			 }
		 }

		 public virtual long NumberOfActiveReadTransactions
		 {
			 get
			 {
				  return _activeReadTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfActiveWriteTransactions
		 {
			 get
			 {
				  return _activeWriteTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfTerminatedTransactions
		 {
			 get
			 {
				  return NumberOfTerminatedReadTransactions + NumberOfTerminatedWriteTransactions;
			 }
		 }

		 public virtual long NumberOfTerminatedReadTransactions
		 {
			 get
			 {
				  return _terminatedReadTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfTerminatedWriteTransactions
		 {
			 get
			 {
				  return _terminatedWriteTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfRolledBackTransactions
		 {
			 get
			 {
				  return NumberOfRolledBackReadTransactions + NumberOfRolledBackWriteTransactions;
			 }
		 }

		 public virtual long NumberOfRolledBackReadTransactions
		 {
			 get
			 {
				  return _rolledBackReadTransactionCount.longValue();
			 }
		 }

		 public virtual long NumberOfRolledBackWriteTransactions
		 {
			 get
			 {
				  return _rolledBackWriteTransactionCount.longValue();
			 }
		 }

		 private static void IncrementCounter( LongAdder readCount, LongAdder writeCount, bool write )
		 {
			  if ( write )
			  {
					writeCount.increment();
			  }
			  else
			  {
					readCount.increment();
			  }
		 }
	}

}