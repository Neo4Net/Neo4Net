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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;

	/// <summary>
	/// Represents a commitment that invoking <seealso cref="TransactionAppender.append(TransactionToApply, LogAppendEvent)"/>
	/// means. As a transaction is carried through the <seealso cref="TransactionCommitProcess"/> this commitment is updated
	/// when <seealso cref="publishAsCommitted() committed"/> (which happens when appending to log), but also
	/// when <seealso cref="publishAsClosed() closing"/>.
	/// </summary>
	public interface Commitment
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Commitment NO_COMMITMENT = new Commitment()
	//	 {
	//		  @@Override public void publishAsCommitted()
	//		  {
	//		  }
	//
	//		  @@Override public void publishAsClosed()
	//		  {
	//		  }
	//
	//		  @@Override public boolean markedAsCommitted()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean hasExplicitIndexChanges()
	//		  {
	//				return false;
	//		  }
	//	 };

		 /// <summary>
		 /// Marks the transaction as committed and makes this fact public.
		 /// </summary>
		 void PublishAsCommitted();

		 /// <summary>
		 /// Marks the transaction as closed and makes this fact public.
		 /// </summary>
		 void PublishAsClosed();

		 /// <returns> whether or not <seealso cref="publishAsCommitted()"/> have been called. </returns>
		 bool MarkedAsCommitted();

		 /// <returns> whether or not this transaction contains explicit index changes. </returns>
		 bool HasExplicitIndexChanges();
	}

}