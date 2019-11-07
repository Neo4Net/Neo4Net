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

	using Lock = Neo4Net.GraphDb.Lock;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class PlaceboTransaction : InternalTransaction
	{
		 private static readonly IPropertyContainerLocker _locker = new IPropertyContainerLocker();
		 private readonly KernelTransaction _currentTransaction;
		 private bool _success;

		 public PlaceboTransaction( KernelTransaction currentTransaction )
		 {
			  this._currentTransaction = currentTransaction;
		 }

		 public override void Terminate()
		 {
			  _currentTransaction.markForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
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

		 public override Lock AcquireWriteLock( IPropertyContainer IEntity )
		 {
			  return _locker.exclusiveLock( _currentTransaction, IEntity );
		 }

		 public override Lock AcquireReadLock( IPropertyContainer IEntity )
		 {
			  return _locker.sharedLock( _currentTransaction, IEntity );
		 }

		 public override KernelTransaction.Type TransactionType()
		 {
			  return _currentTransaction.transactionType();
		 }

		 public override SecurityContext SecurityContext()
		 {
			  return _currentTransaction.securityContext();
		 }

		 public override Neo4Net.Kernel.Api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
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