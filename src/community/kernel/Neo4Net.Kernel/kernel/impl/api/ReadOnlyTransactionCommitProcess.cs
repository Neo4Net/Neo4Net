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
namespace Neo4Net.Kernel.Impl.Api
{
	using ReadOnlyDbException = Neo4Net.Kernel.Api.Exceptions.ReadOnlyDbException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

	/// <summary>
	/// For databases in dbms.read_only mode, the implementation of <seealso cref="Neo4Net.kernel.impl.api.TransactionCommitProcess"/>
	/// will simply always throw an exception on commit, to ensure that no changes are made.
	/// </summary>
	public class ReadOnlyTransactionCommitProcess : TransactionCommitProcess
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(TransactionToApply batch, Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode mode) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  throw new ReadOnlyDbException();
		 }
	}

}