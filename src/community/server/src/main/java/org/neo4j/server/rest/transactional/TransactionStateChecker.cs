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
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;

	public class TransactionStateChecker : IDisposable
	{
		 private readonly Statement _statement;
		 private readonly IsNodeDeletedInCurrentTx _nodeCheck;
		 private readonly IsRelationshipDeletedInCurrentTx _relCheck;

		 internal TransactionStateChecker( Statement statement, IsNodeDeletedInCurrentTx nodeCheck, IsRelationshipDeletedInCurrentTx relCheck )
		 {
			  this._statement = statement;
			  this._nodeCheck = nodeCheck;
			  this._relCheck = relCheck;
		 }

		 public static TransactionStateChecker Create( TransitionalPeriodTransactionMessContainer container )
		 {
			  KernelTransaction topLevelTransactionBoundToThisThread = container.Bridge.getKernelTransactionBoundToThisThread( true );
			  KernelStatement kernelStatement = ( KernelStatement ) topLevelTransactionBoundToThisThread.AcquireStatement();

			  return new TransactionStateChecker( kernelStatement, nodeId => kernelStatement.HasTxStateWithChanges() && kernelStatement.TxState().nodeIsDeletedInThisTx(nodeId), relId => kernelStatement.HasTxStateWithChanges() && kernelStatement.TxState().relationshipIsDeletedInThisTx(relId) );
		 }

		 public override void Close()
		 {
			  _statement.close();
		 }

		 public virtual bool IsNodeDeletedInCurrentTx( long id )
		 {
			  return _nodeCheck( id );
		 }

		 public virtual bool IsRelationshipDeletedInCurrentTx( long id )
		 {
			  return _relCheck( id );
		 }

		 delegate bool IsNodeDeletedInCurrentTx( long id );

		 delegate bool IsRelationshipDeletedInCurrentTx( long id );
	}

}