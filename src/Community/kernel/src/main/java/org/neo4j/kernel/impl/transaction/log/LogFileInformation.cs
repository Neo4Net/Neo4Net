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

	public interface LogFileInformation
	{
		 /// <returns> the reachable entry that is farthest back of them all, in any existing version. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getFirstExistingEntryId() throws java.io.IOException;
		 long FirstExistingEntryId { get; }

		 /// <param name="version"> the log version to get first committed tx for. </param>
		 /// <returns> the first committed entry id for the log with {@code version}.
		 /// If that log doesn't exist -1 is returned. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getFirstEntryId(long version) throws java.io.IOException;
		 long GetFirstEntryId( long version );

		 /// <returns> the last committed entry id for this Log </returns>
		 long LastEntryId { get; }

		 /// <param name="version"> the log version to get first entry timestamp for. </param>
		 /// <returns> the timestamp for the start record for the first encountered entry
		 /// in the log {@code version}. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getFirstStartRecordTimestamp(long version) throws java.io.IOException;
		 long GetFirstStartRecordTimestamp( long version );

		 /// <summary>
		 /// Checks if a transaction with the given transaction id exists on disk </summary>
		 /// <param name="transactionId"> The id of the transaction to check </param>
		 /// <returns> True if the transaction with the given id is contained in an existing log file, false otherwise </returns>
		 /// <exception cref="IOException"> If an IO exception occurred during scan of the log files </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean transactionExistsOnDisk(long transactionId) throws java.io.IOException;
		 bool TransactionExistsOnDisk( long transactionId );
	}

}