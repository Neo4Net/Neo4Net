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

	using Lock = Org.Neo4j.Graphdb.Lock;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

	public class PlaceboTransaction : InternalTransaction
	{
		 private static readonly PropertyContainerLocker _locker = new PropertyContainerLocker();
		 private readonly KernelTransaction _currentTransaction;
		 private bool _success;

		 public PlaceboTransaction( KernelTransaction currentTransaction )
		 {
			  this._currentTransaction = currentTransaction;
		 }

		 public override void Terminate()
		 {
			  _currentTransaction.markForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );
		 }

		 public override void Failure()
		 {
			  _currentTransaction.failure();
		 }

		 public override void Success()
		 {
			  _success = true;
		 }

		 public override void Close()
		 {
			  if ( !_success )
			  {
					_currentTransaction.failure();
			  }
		 }

		 public override Lock AcquireWriteLock( PropertyContainer entity )
		 {
			  return _locker.exclusiveLock( _currentTransaction, entity );
		 }

		 public override Lock AcquireReadLock( PropertyContainer entity )
		 {
			  return _locker.sharedLock( _currentTransaction, entity );
		 }

		 public override KernelTransaction.Type TransactionType()
		 {
			  return _currentTransaction.transactionType();
		 }

		 public override SecurityContext SecurityContext()
		 {
			  return _currentTransaction.securityContext();
		 }

		 public override Org.Neo4j.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
		 {
			  return _currentTransaction.overrideWith( context );
		 }

		 public override Optional<Status> TerminationReason()
		 {
			  return _currentTransaction.ReasonIfTerminated;
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 set
			 {
				  _currentTransaction.MetaData = value;
			 }
		 }
	}

}