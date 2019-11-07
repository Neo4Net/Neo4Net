using System;
using System.Diagnostics;

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
namespace Neo4Net.Server.rest.transactional
{

	using NotInTransactionException = Neo4Net.GraphDb.NotInTransactionException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	internal class TransitionalTxManagementKernelTransaction
	{
		 private readonly GraphDatabaseFacade _db;
		 private readonly KernelTransaction.Type _type;
		 private readonly LoginContext _loginContext;
		 private readonly long _customTransactionTimeout;
		 private readonly ThreadToStatementContextBridge _bridge;

		 private InternalTransaction _tx;
		 private KernelTransaction _suspendedTransaction;

		 internal TransitionalTxManagementKernelTransaction( GraphDatabaseFacade db, KernelTransaction.Type type, LoginContext loginContext, long customTransactionTimeout, ThreadToStatementContextBridge bridge )
		 {
			  this._db = db;
			  this._type = type;
			  this._loginContext = loginContext;
			  this._customTransactionTimeout = customTransactionTimeout;
			  this._bridge = bridge;
			  this._tx = StartTransaction();
		 }

		 internal virtual void SuspendSinceTransactionsAreStillThreadBound()
		 {
			  Debug.Assert( _suspendedTransaction == null, "Can't suspend the transaction if it already is suspended." );
			  _suspendedTransaction = _bridge.getKernelTransactionBoundToThisThread( true );
			  _bridge.unbindTransactionFromCurrentThread();
		 }

		 internal virtual void ResumeSinceTransactionsAreStillThreadBound()
		 {
			  Debug.Assert( _suspendedTransaction != null, "Can't resume the transaction if it has not first been suspended." );
			  _bridge.bindTransactionToCurrentThread( _suspendedTransaction );
			  _suspendedTransaction = null;
		 }

		 public virtual void Terminate()
		 {
			  _tx.terminate();
		 }

		 public virtual void Rollback()
		 {
			  try
			  {
					KernelTransaction kernelTransactionBoundToThisThread = _bridge.getKernelTransactionBoundToThisThread( false );
					kernelTransactionBoundToThisThread.Failure();
					kernelTransactionBoundToThisThread.Close();
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					_bridge.unbindTransactionFromCurrentThread();
			  }
		 }

		 public virtual void Commit()
		 {
			  try
			  {
					KernelTransaction kernelTransactionBoundToThisThread = _bridge.getKernelTransactionBoundToThisThread( true );
					kernelTransactionBoundToThisThread.Success();
					kernelTransactionBoundToThisThread.Close();
			  }
			  catch ( NotInTransactionException )
			  {
					// if the transaction was already terminated there is nothing more to do
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					_bridge.unbindTransactionFromCurrentThread();
			  }
		 }

		 internal virtual void CloseTransactionForPeriodicCommit()
		 {
			  _tx.close();
		 }

		 internal virtual void ReopenAfterPeriodicCommit()
		 {
			  _tx = StartTransaction();
		 }

		 private InternalTransaction StartTransaction()
		 {
			  return _customTransactionTimeout > GraphDatabaseSettings.UNSPECIFIED_TIMEOUT ? _db.BeginTransaction( _type, _loginContext, _customTransactionTimeout, TimeUnit.MILLISECONDS ) : _db.BeginTransaction( _type, _loginContext );
		 }
	}

}