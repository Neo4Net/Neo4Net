﻿using System.Collections.Generic;

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

	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Org.Neo4j.Kernel.api.query.ExecutingQuery;
	using ActiveLock = Org.Neo4j.Kernel.impl.locking.ActiveLock;

	/// <summary>
	/// A test implementation of <seealso cref="KernelTransactionHandle"/> that simply wraps a given <seealso cref="KernelTransaction"/>.
	/// </summary>
	public class TestKernelTransactionHandle : KernelTransactionHandle
	{
		 private const string USER_TRANSACTION_NAME_PREFIX = "transaction-";
		 private readonly KernelTransaction _tx;

		 public TestKernelTransactionHandle( KernelTransaction tx )
		 {
			  this._tx = Objects.requireNonNull( tx );
		 }

		 public override long LastTransactionIdWhenStarted()
		 {
			  return _tx.lastTransactionIdWhenStarted();
		 }

		 public override long LastTransactionTimestampWhenStarted()
		 {
			  return _tx.lastTransactionTimestampWhenStarted();
		 }

		 public override long StartTime()
		 {
			  return _tx.startTime();
		 }

		 public override long StartTimeNanos()
		 {
			  return _tx.startTimeNanos();
		 }

		 public override long TimeoutMillis()
		 {
			  return _tx.timeout();
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _tx.Open;
			 }
		 }

		 public override bool MarkForTermination( Status reason )
		 {
			  _tx.markForTermination( reason );
			  return true;
		 }

		 public override AuthSubject Subject()
		 {
			  return _tx.subjectOrAnonymous();
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 get
			 {
				  return Collections.emptyMap();
			 }
		 }

		 public override Optional<Status> TerminationReason()
		 {
			  return _tx.ReasonIfTerminated;
		 }

		 public override bool IsUnderlyingTransaction( KernelTransaction tx )
		 {
			  return this._tx == tx;
		 }

		 public virtual long UserTransactionId
		 {
			 get
			 {
				  return _tx.TransactionId;
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
			  throw new System.NotSupportedException();
		 }

		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return Stream.empty();
		 }

		 public override TransactionExecutionStatistic TransactionStatistic()
		 {
			  return TransactionExecutionStatistic.NotAvailable;
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
			  TestKernelTransactionHandle that = ( TestKernelTransactionHandle ) o;
			  return _tx.Equals( that._tx );
		 }

		 public override int GetHashCode()
		 {
			  return _tx.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "TestKernelTransactionHandle{tx=" + _tx + "}";
		 }
	}

}