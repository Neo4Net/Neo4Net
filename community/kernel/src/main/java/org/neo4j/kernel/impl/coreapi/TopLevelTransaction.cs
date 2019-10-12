using System;
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
namespace Org.Neo4j.Kernel.impl.coreapi
{

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Lock = Org.Neo4j.Graphdb.Lock;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using TransientFailureException = Org.Neo4j.Graphdb.TransientFailureException;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using ConstraintViolationTransactionFailureException = Org.Neo4j.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Status_Classification = Org.Neo4j.Kernel.Api.Exceptions.Status_Classification;
	using Status_Code = Org.Neo4j.Kernel.Api.Exceptions.Status_Code;

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
			  this._transaction.markForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );
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

		 public override Org.Neo4j.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
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