using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	/// <summary>
	/// A <seealso cref="KernelTransactionHandle"/> that wraps the given <seealso cref="KernelTransactionImplementation"/>.
	/// This handle knows that <seealso cref="KernelTransactionImplementation"/>s can be reused and represents a single logical
	/// transaction. This means that methods like <seealso cref="markForTermination(Status)"/> can only terminate running
	/// transaction this handle was created for.
	/// </summary>
	internal class KernelTransactionImplementationHandle : KernelTransactionHandle
	{
		 private const string USER_TRANSACTION_NAME_PREFIX = "transaction-";

		 private readonly long _txReuseCount;
		 private readonly long _lastTransactionIdWhenStarted;
		 private readonly long _lastTransactionTimestampWhenStarted;
		 private readonly long _startTime;
		 private readonly long _startTimeNanos;
		 private readonly long _timeoutMillis;
		 private readonly KernelTransactionImplementation _tx;
		 private readonly SystemNanoClock _clock;
		 private readonly AuthSubject _subject;
		 private readonly Optional<Status> _terminationReason;
		 private readonly ExecutingQueryList _executingQueries;
		 private readonly IDictionary<string, object> _metaData;
		 private readonly long _userTransactionId;

		 internal KernelTransactionImplementationHandle( KernelTransactionImplementation tx, SystemNanoClock clock )
		 {
			  this._txReuseCount = tx.ReuseCount;
			  this._lastTransactionIdWhenStarted = tx.LastTransactionIdWhenStarted();
			  this._lastTransactionTimestampWhenStarted = tx.LastTransactionTimestampWhenStarted();
			  this._startTime = tx.StartTime();
			  this._startTimeNanos = tx.StartTimeNanos();
			  this._timeoutMillis = tx.Timeout();
			  this._subject = tx.SubjectOrAnonymous();
			  this._terminationReason = tx.ReasonIfTerminated;
			  this._executingQueries = tx.ExecutingQueries();
			  this._metaData = tx.MetaData;
			  this._userTransactionId = tx.UserTransactionId();
			  this._tx = tx;
			  this._clock = clock;
		 }

		 public override long LastTransactionIdWhenStarted()
		 {
			  return _lastTransactionIdWhenStarted;
		 }

		 public override long LastTransactionTimestampWhenStarted()
		 {
			  return _lastTransactionTimestampWhenStarted;
		 }

		 public override long StartTime()
		 {
			  return _startTime;
		 }

		 public override long StartTimeNanos()
		 {
			  return _startTimeNanos;
		 }

		 public override long TimeoutMillis()
		 {
			  return _timeoutMillis;
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _tx.Open && _txReuseCount == _tx.ReuseCount;
			 }
		 }

		 public override bool MarkForTermination( Status reason )
		 {
			  return _tx.markForTermination( _txReuseCount, reason );
		 }

		 public override AuthSubject Subject()
		 {
			  return _subject;
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 get
			 {
				  return _metaData;
			 }
		 }

		 public override Optional<Status> TerminationReason()
		 {
			  return _terminationReason;
		 }

		 public override bool IsUnderlyingTransaction( KernelTransaction tx )
		 {
			  return this._tx == tx;
		 }

		 public virtual long UserTransactionId
		 {
			 get
			 {
				  return _userTransactionId;
			 }
		 }

		 public virtual string UserTransactionName
		 {
			 get
			 {
				  return USER_TRANSACTION_NAME_PREFIX + UserTransactionId;
			 }
		 }

		 public override Stream<ExecutingQuery> ExecutingQueries()
		 {
			  return _executingQueries.queries();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks()
		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return _tx.activeLocks();
		 }

		 public override TransactionExecutionStatistic TransactionStatistic()
		 {
			  if ( _txReuseCount == _tx.ReuseCount )
			  {
					return new TransactionExecutionStatistic( _tx, _clock, _startTime );
			  }
			  else
			  {
					return TransactionExecutionStatistic.NotAvailable;
			  }
		 }

		 public virtual bool SchemaTransaction
		 {
			 get
			 {
				  return _tx.SchemaTransaction;
			 }
		 }

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
			  KernelTransactionImplementationHandle that = ( KernelTransactionImplementationHandle ) o;
			  return _txReuseCount == that._txReuseCount && _tx.Equals( that._tx );
		 }

		 public override int GetHashCode()
		 {
			  return 31 * ( int )( _txReuseCount ^ ( ( long )( ( ulong )_txReuseCount >> 32 ) ) ) + _tx.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "KernelTransactionImplementationHandle{txReuseCount=" + _txReuseCount + ", tx=" + _tx + "}";
		 }
	}

}