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
namespace Neo4Net.Kernel.impl.transaction.tracing
{
	/// <summary>
	/// A trace event that represents a transaction with the database, and its lifetime.
	/// </summary>
	public interface TransactionEvent : AutoCloseable
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 TransactionEvent NULL = new TransactionEvent()
	//	 {
	//		  @@Override public void setSuccess(boolean success)
	//		  {
	//		  }
	//
	//		  @@Override public void setFailure(boolean failure)
	//		  {
	//		  }
	//
	//		  @@Override public CommitEvent beginCommitEvent()
	//		  {
	//				return CommitEvent.NULL;
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//
	//		  @@Override public void setTransactionWriteState(String transactionWriteState)
	//		  {
	//		  }
	//
	//		  @@Override public void setReadOnly(boolean wasReadOnly)
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// The transaction was marked as successful.
		 /// </summary>
		 bool Success { set; }

		 /// <summary>
		 /// The transaction was marked as failed.
		 /// </summary>
		 bool Failure { set; }

		 /// <summary>
		 /// Begin the process of committing the transaction.
		 /// </summary>
		 CommitEvent BeginCommitEvent();

		 /// <summary>
		 /// Mark the end of the transaction, after it has been committed or rolled back.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Set write state of the transaction, as given by
		 /// <seealso cref="org.neo4j.kernel.impl.api.KernelTransactionImplementation.TransactionWriteState"/>.
		 /// </summary>
		 string TransactionWriteState { set; }

		 /// <summary>
		 /// Specify that the transaction was read-only.
		 /// </summary>
		 bool ReadOnly { set; }
	}

}