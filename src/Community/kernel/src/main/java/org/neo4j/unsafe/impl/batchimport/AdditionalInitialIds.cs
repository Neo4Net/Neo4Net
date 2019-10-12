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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using StoreMigrator = Neo4Net.Kernel.impl.storemigration.participant.StoreMigrator;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

	/// <summary>
	/// Migrating a store uses the <seealso cref="ParallelBatchImporter"/> to do so, where node/relationship stores
	/// are created with data read from legacy node/relationship stores. The batch import also populates
	/// a counts store, which revolves around tokens and their ids. Knowing those high token ids before hand greatly helps
	/// the batch importer code do things efficiently, instead of figuring that out as it goes. When doing
	/// the migration there are no token stores, although nodes and relationships gets importer with existing
	/// token ids in them, so this is a way for the <seealso cref="StoreMigrator"/> to communicate those ids to the
	/// <seealso cref="ParallelBatchImporter"/>.
	/// 
	/// When actually writing out the counts store on disk the last committed transaction id at that point is also
	/// stored, and that's why the <seealso cref="StoreMigrator"/> needs to communicate that using
	/// <seealso cref="lastCommittedTransactionId()"/> as well.
	/// </summary>
	public interface AdditionalInitialIds
	{
		 long LastCommittedTransactionId();

		 long LastCommittedTransactionChecksum();

		 long LastCommittedTransactionLogVersion();

		 long LastCommittedTransactionLogByteOffset();

		 /// <summary>
		 /// High ids of zero, useful when creating a completely new store with <seealso cref="ParallelBatchImporter"/>.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AdditionalInitialIds EMPTY = new AdditionalInitialIds()
	//	 {
	//		  @@Override public long lastCommittedTransactionId()
	//		  {
	//				return TransactionIdStore.BASE_TX_ID;
	//		  }
	//
	//		  @@Override public long lastCommittedTransactionChecksum()
	//		  {
	//				return TransactionIdStore.BASE_TX_CHECKSUM;
	//		  }
	//
	//		  @@Override public long lastCommittedTransactionLogVersion()
	//		  {
	//				return TransactionIdStore.BASE_TX_LOG_VERSION;
	//		  }
	//
	//		  @@Override public long lastCommittedTransactionLogByteOffset()
	//		  {
	//				return TransactionIdStore.BASE_TX_LOG_BYTE_OFFSET;
	//		  }
	//	 };
	}

}