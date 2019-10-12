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
namespace Neo4Net.Kernel.Impl.Api
{
	using ReadOnlyDbException = Neo4Net.Kernel.Api.Exceptions.ReadOnlyDbException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

	/// <summary>
	/// For databases in dbms.read_only mode, the implementation of <seealso cref="org.neo4j.kernel.impl.api.TransactionCommitProcess"/>
	/// will simply always throw an exception on commit, to ensure that no changes are made.
	/// </summary>
	public class ReadOnlyTransactionCommitProcess : TransactionCommitProcess
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  throw new ReadOnlyDbException();
		 }
	}

}