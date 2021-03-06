﻿/*
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
namespace Org.Neo4j.Procedure
{
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;

	/// <summary>
	/// ProcedureTransaction allows to mark a transaction for termination, this will make it so that it can not commit.
	/// </summary>
	public interface ProcedureTransaction
	{
		 /// <summary>
		 /// Marks this transaction as terminated, which means that it will be, much like in the case of failure,
		 /// unconditionally rolled back when <seealso cref="Transaction.close()"/> is called. Once this method has been invoked, it doesn't
		 /// matter
		 /// if <seealso cref="Transaction.success()"/> is invoked afterwards -- the transaction will still be rolled back.
		 /// 
		 /// Additionally, terminating a transaction causes all subsequent operations carried out within that
		 /// transaction to throw a <seealso cref="TransactionTerminatedException"/> in the owning thread.
		 /// 
		 /// Note that, unlike the other transaction operations, this method can be called from threads other than
		 /// the owning thread of the transaction. When this method is called from a different thread,
		 /// it signals the owning thread to terminate the transaction and returns immediately.
		 /// 
		 /// Calling this method on an already closed transaction has no effect.
		 /// </summary>
		 void Terminate();

		 /// <summary>
		 /// Marks this transaction as failed, which means that it will
		 /// unconditionally be rolled back when <seealso cref="Transaction.close()"/> is called. Once
		 /// this method has been invoked, it doesn't matter if
		 /// <seealso cref="Transaction.success()"/> is invoked afterwards -- the transaction will still be
		 /// rolled back.
		 /// 
		 /// This method is not thread safe.
		 /// </summary>
		 void Failure();
	}

}