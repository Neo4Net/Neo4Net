/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.util
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogVersionBridge = Neo4Net.Kernel.impl.transaction.log.LogVersionBridge;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using ReaderLogVersionBridge = Neo4Net.Kernel.impl.transaction.log.ReaderLogVersionBridge;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;

	public class TransactionLogUtils
	{
		 /// <summary>
		 /// Opens a <seealso cref="LogEntryCursor"/> over all log files found in the storeDirectory
		 /// </summary>
		 /// <param name="fs"> <seealso cref="FileSystemAbstraction"/> to find {@code storeDirectory} in. </param>
		 /// <param name="logFiles"> the physical log files to read from </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.log.LogEntryCursor openLogs(final org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static LogEntryCursor OpenLogs( FileSystemAbstraction fs, LogFiles logFiles )
		 {
			  File firstFile = logFiles.GetLogFileForVersion( logFiles.LowestLogVersion );
			  return OpenLogEntryCursor( fs, firstFile, new ReaderLogVersionBridge( logFiles ) );
		 }

		 /// <summary>
		 /// Opens a <seealso cref="LogEntryCursor"/> for requested file
		 /// </summary>
		 /// <param name="fileSystem"> to find {@code file} in. </param>
		 /// <param name="file"> file to open </param>
		 /// <param name="readerLogVersionBridge"> log version bridge to use </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.log.LogEntryCursor openLogEntryCursor(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, org.neo4j.kernel.impl.transaction.log.LogVersionBridge readerLogVersionBridge) throws java.io.IOException
		 public static LogEntryCursor OpenLogEntryCursor( FileSystemAbstraction fileSystem, File file, LogVersionBridge readerLogVersionBridge )
		 {
			  LogVersionedStoreChannel channel = OpenVersionedChannel( fileSystem, file );
			  ReadableLogChannel logChannel = new ReadAheadLogChannel( channel, readerLogVersionBridge );
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  return new LogEntryCursor( logEntryReader, logChannel );
		 }

		 /// <summary>
		 /// Opens a file in given {@code fileSystem} as a <seealso cref="LogVersionedStoreChannel"/>.
		 /// </summary>
		 /// <param name="fileSystem"> <seealso cref="FileSystemAbstraction"/> containing the file to open. </param>
		 /// <param name="file"> file to open as a channel. </param>
		 /// <returns> <seealso cref="LogVersionedStoreChannel"/> for the file. Its version is determined by its log header. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel openVersionedChannel(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException
		 public static PhysicalLogVersionedStoreChannel OpenVersionedChannel( FileSystemAbstraction fileSystem, File file )
		 {
			  StoreChannel fileChannel = fileSystem.Open( file, OpenMode.READ );
			  LogHeader logHeader = readLogHeader( ByteBuffer.allocate( LOG_HEADER_SIZE ), fileChannel, true, file );
			  PhysicalLogVersionedStoreChannel channel = new PhysicalLogVersionedStoreChannel( fileChannel, logHeader.LogVersion, logHeader.LogFormatVersion );
			  return channel;
		 }
	}

}