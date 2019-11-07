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
	using TransactionLifecycleException = Neo4Net.Server.rest.transactional.error.TransactionLifecycleException;

	/// <summary>
	/// Stores transaction contexts for the server, including handling concurrency safe ways to acquire
	/// transaction contexts back, as well as timing out and closing transaction contexts that have been
	/// left unused.
	/// </summary>
	public interface TransactionRegistry
	{
		 long Begin( TransactionHandle handle );

		 long Release( long id, TransactionHandle transactionHandle );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionHandle acquire(long id) throws Neo4Net.server.rest.transactional.error.TransactionLifecycleException;
		 TransactionHandle Acquire( long id );

		 void Forget( long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionHandle terminate(long id) throws Neo4Net.server.rest.transactional.error.TransactionLifecycleException;
		 TransactionHandle Terminate( long id );

		 void RollbackAllSuspendedTransactions();
	}

}