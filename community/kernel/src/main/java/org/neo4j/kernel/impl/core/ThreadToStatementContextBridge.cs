﻿using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.core
{

	using DatabaseShutdownException = Org.Neo4j.Graphdb.DatabaseShutdownException;
	using NotInTransactionException = Org.Neo4j.Graphdb.NotInTransactionException;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;

	/// <summary>
	/// This is meant to serve as the bridge that tie transactions to threads.
	/// APIs will use this to get the appropriate <seealso cref="Statement"/> when it performs operations.
	/// </summary>
	public class ThreadToStatementContextBridge : System.Func<Statement>
	{
		 private readonly ThreadLocal<KernelTransaction> _threadToTransactionMap = new ThreadLocal<KernelTransaction>();
		 private readonly AvailabilityGuard _availabilityGuard;

		 public ThreadToStatementContextBridge( AvailabilityGuard availabilityGuard )
		 {
			  this._availabilityGuard = availabilityGuard;
		 }

		 public virtual bool HasTransaction()
		 {
			  KernelTransaction kernelTransaction = _threadToTransactionMap.get();
			  if ( kernelTransaction != null )
			  {
					AssertInUnterminatedTransaction( kernelTransaction );
					return true;
			  }
			  return false;
		 }

		 public virtual void BindTransactionToCurrentThread( KernelTransaction transaction )
		 {
			  if ( HasTransaction() )
			  {
					throw new System.InvalidOperationException( Thread.CurrentThread + " already has a transaction bound" );
			  }
			  _threadToTransactionMap.set( transaction );
		 }

		 public virtual void UnbindTransactionFromCurrentThread()
		 {
			  _threadToTransactionMap.remove();
		 }

		 public override Statement Get()
		 {
			  return GetKernelTransactionBoundToThisThread( true ).acquireStatement();
		 }

		 public virtual void AssertInUnterminatedTransaction()
		 {
			  AssertInUnterminatedTransaction( _threadToTransactionMap.get() );
		 }

		 public virtual KernelTransaction GetKernelTransactionBoundToThisThread( bool strict )
		 {
			  KernelTransaction transaction = _threadToTransactionMap.get();
			  if ( strict )
			  {
					AssertInUnterminatedTransaction( transaction );
			  }
			  return transaction;
		 }

		 private void AssertInUnterminatedTransaction( KernelTransaction transaction )
		 {
			  if ( _availabilityGuard.Shutdown )
			  {
					throw new DatabaseShutdownException();
			  }
			  if ( transaction == null )
			  {
					throw new BridgeNotInTransactionException();
			  }
			  if ( transaction.Terminated )
			  {
					throw new TransactionTerminatedException( transaction.ReasonIfTerminated.orElse( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated ) );
			  }
		 }

		 private class BridgeNotInTransactionException : NotInTransactionException, Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus
		 {
			  public override Status Status()
			  {
					return Org.Neo4j.Kernel.Api.Exceptions.Status_Request.TransactionRequired;
			  }
		 }
	}

}