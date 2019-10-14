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
namespace Neo4Net.Kernel.impl.transaction.log.files
{

	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// Main point of access to database transactional logs.
	/// Provide access to low level file based operations, log file headers, <seealso cref="LogFile"/>
	/// and <seealso cref="PhysicalLogVersionedStoreChannel"/>
	/// </summary>
	public interface LogFiles : Lifecycle
	{
		 long GetLogVersion( File historyLogFile );

		 long GetLogVersion( string historyLogFilename );

		 File[] LogFiles();

		 bool IsLogFile( File file );

		 File LogFilesDirectory();

		 File GetLogFileForVersion( long version );

		 File HighestLogFile { get; }

		 long HighestLogVersion { get; }

		 long LowestLogVersion { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.transaction.log.entry.LogHeader extractHeader(long version) throws java.io.IOException;
		 LogHeader ExtractHeader( long version );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel openForVersion(long version) throws java.io.IOException;
		 PhysicalLogVersionedStoreChannel OpenForVersion( long version );

		 bool VersionExists( long version );

		 bool HasAnyEntries( long version );

		 void Accept( LogVersionVisitor visitor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(LogHeaderVisitor visitor) throws java.io.IOException;
		 void Accept( LogHeaderVisitor visitor );

		 LogFile LogFile { get; }

		 TransactionLogFileInformation LogFileInformation { get; }
	}

}