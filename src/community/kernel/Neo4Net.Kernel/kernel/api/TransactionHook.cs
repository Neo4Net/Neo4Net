using System;

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
namespace Neo4Net.Kernel.Api
{
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;

	/// <summary>
	/// A mechanism to augment and monitor transactions before and after commit/rollback.
	/// </summary>
	public interface TransactionHook<OUTCOME> where OUTCOME : TransactionHook_Outcome
	{

		 OUTCOME BeforeCommit( ReadableTransactionState state, KernelTransaction transaction, StorageReader storageReader );
		 void AfterCommit( ReadableTransactionState state, KernelTransaction transaction, OUTCOME outcome );
		 void AfterRollback( ReadableTransactionState state, KernelTransaction transaction, OUTCOME outcome );
	}

	 public interface TransactionHook_Outcome
	 {
		  bool Successful { get; }
		  Exception Failure();
	 }

}