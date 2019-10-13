using System;
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
namespace Neo4Net.Kernel.impl.coreapi
{

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Lock = Neo4Net.Graphdb.Lock;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using TransientFailureException = Neo4Net.Graphdb.TransientFailureException;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using ConstraintViolationTransactionFailureException = Neo4Net.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Status_Classification = Neo4Net.Kernel.Api.Exceptions.Status_Classification;
	using Status_Code = Neo4Net.Kernel.Api.Exceptions.Status_Code;

	public class TopLevelTransaction : InternalTransaction
	{
		 private static readonly PropertyContainerLocker _locker = new PropertyContainerLocker();
		 private bool _successCalled;
		 private readonly KernelTransaction _transaction;

		 public TopLevelTransaction( KernelTransaction transaction )
		 {
			  this._transaction = transaction;
		 }

		 public override void Failure()
		 {
			  _transaction.failure();
		 }

		 public override void Success()
		 {
			  _successCalled = true;
			  _transaction.success();
		 }

		 public override void Terminate()
		 {
			  this._transaction.markForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
		 }

		 public override void Close()
		 {
			  try
			  {
					if ( _transaction.Open )
					{
						 _transaction.close();
					}
			  }
			  catch ( TransientFailureException e )
			  {
					// We let transient exceptions pass through unchanged since they aren't really transaction failures
					// in the same sense as unexpected failures are. Such exception signals that the transaction
					// can be retried and might be successful the next time.
					throw e;
			  }
			  catch ( ConstraintViolationTransactionFailureException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( Exception e ) when ( e is KernelException || e is TransactionTerminatedException )
			  {
					Status_Code statusCode = e.status().code();
					if ( statusCode.Classification() == Status_Classification.TransientError )
					{
						 throw new TransientTransactionFailureException( CloseFailureMessage() + ": " + statusCode.Description(), e );
					}
					throw new TransactionFailureException( CloseFailureMessage(), e );
			  }
			  catch ( Exception e )
			  {
					throw new TransactionFailureException( CloseFailureMessage(), e );
			  }
		 }

		 private string CloseFailureMessage()
		 {
			  return _successCalled ? "Transaction was marked as successful, but unable to commit transaction so rolled back." : "Unable to rollback transaction";
		 }

		 public override Lock AcquireWriteLock( PropertyContainer entity )
		 {
			  return _locker.exclusiveLock( _transaction, entity );
		 }

		 public override Lock AcquireReadLock( PropertyContainer entity )
		 {
			  return _locker.sharedLock( _transaction, entity );
		 }

		 public override KernelTransaction.Type TransactionType()
		 {
			  return _transaction.transactionType();
		 }

		 public override SecurityContext SecurityContext()
		 {
			  return _transaction.securityContext();
		 }

		 public override Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
		 {
			  return _transaction.overrideWith( context );
		 }

		 public override Optional<Status> TerminationReason()
		 {
			  return _transaction.ReasonIfTerminated;
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 set
			 {
				  _transaction.MetaData = value;
			 }
		 }
	}

}