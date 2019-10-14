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
namespace Neo4Net.Kernel.recovery
{
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;

	public interface RecoveryMonitor
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void recoveryRequired(org.neo4j.kernel.impl.transaction.log.LogPosition recoveryPosition)
	//	 {
	//		  // noop
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void transactionRecovered(long txId)
	//	 {
	//		  //noop
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void recoveryCompleted(int numberOfRecoveredTransactions)
	//	 {
	//		  //noop
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void reverseStoreRecoveryCompleted(long lowestRecoveredTxId)
	//	 {
	//		  //noop
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void failToRecoverTransactionsAfterCommit(Throwable t, org.neo4j.kernel.impl.transaction.log.entry.LogEntryCommit commitEntry, org.neo4j.kernel.impl.transaction.log.LogPosition recoveryToPosition)
	//	 {
	//		  //noop
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void failToRecoverTransactionsAfterPosition(Throwable t, org.neo4j.kernel.impl.transaction.log.LogPosition recoveryFromPosition)
	//	 {
	//		  //noop
	//	 }
	}

}