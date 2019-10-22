/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.dump
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;
	using LogVersionBridge = Neo4Net.Kernel.impl.transaction.log.LogVersionBridge;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReaderLogVersionBridge = Neo4Net.Kernel.impl.transaction.log.ReaderLogVersionBridge;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using InvalidLogEntryHandler = Neo4Net.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogEntryCursor = Neo4Net.tools.dump.log.TransactionLogEntryCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.LogVersionBridge_Fields.NO_MORE_CHANNELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.util.TransactionLogUtils.openVersionedChannel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;

	/// <summary>
	/// Merely a utility which, given a store directory or log file, reads the transaction log(s) as a stream of transactions
	/// and invokes methods on <seealso cref="Monitor"/>.
	/// </summary>
	public class TransactionLogAnalyzer
	{
		 /// <summary>
		 /// Receiving call-backs for all kinds of different events while analyzing the stream of transactions.
		 /// </summary>
		 public interface Monitor
		 {
			  /// <summary>
			  /// Called when transitioning to a new log file, crossing a log version bridge. This is also called for the
			  /// first log file opened.
			  /// </summary>
			  /// <param name="file"> <seealso cref="File"/> pointing to the opened log file. </param>
			  /// <param name="logVersion"> log version. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void logFile(java.io.File file, long logVersion) throws java.io.IOException
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// A complete transaction with <seealso cref="LogEntryStart"/>, one or more <seealso cref="LogEntryCommand"/> and <seealso cref="LogEntryCommit"/>.
			  /// </summary>
			  /// <param name="transactionEntries"> the log entries making up the transaction, including start/commit entries. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void transaction(org.Neo4Net.kernel.impl.transaction.log.entry.LogEntry[] transactionEntries)
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// <seealso cref="CheckPoint"/> log entry in between transactions.
			  /// </summary>
			  /// <param name="checkpoint"> the <seealso cref="CheckPoint"/> log entry. </param>
			  /// <param name="checkpointEntryPosition"> <seealso cref="LogPosition"/> of the checkpoint entry itself. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void checkpoint(org.Neo4Net.kernel.impl.transaction.log.entry.CheckPoint checkpoint, org.Neo4Net.kernel.impl.transaction.log.LogPosition checkpointEntryPosition)
	//		  { // no-op by default
	//		  }
		 }

		 public static Monitor All( params Monitor[] monitors )
		 {
			  return new CombinedMonitor( monitors );
		 }

		 /// <summary>
		 /// Analyzes transactions found in log file(s) specified by {@code storeDirOrLogFile} calling methods on the supplied
		 /// <seealso cref="Monitor"/> for each encountered data item.
		 /// </summary>
		 /// <param name="fileSystem"> <seealso cref="FileSystemAbstraction"/> to find the files on. </param>
		 /// <param name="storeDirOrLogFile"> <seealso cref="File"/> pointing either to a directory containing transaction log files, or directly
		 /// pointing to a single transaction log file to analyze. </param>
		 /// <param name="invalidLogEntryHandler"> <seealso cref="InvalidLogEntryHandler"/> to pass in to the internal <seealso cref="LogEntryReader"/>. </param>
		 /// <param name="monitor"> <seealso cref="Monitor"/> receiving call-backs for all <seealso cref="Monitor.transaction(LogEntry[]) transactions"/>,
		 /// <seealso cref="Monitor.checkpoint(CheckPoint, LogPosition) checkpoints"/> and <seealso cref="Monitor.logFile(File, long) log file transitions"/>
		 /// encountered during the analysis. </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void analyze(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, java.io.File storeDirOrLogFile, org.Neo4Net.kernel.impl.transaction.log.entry.InvalidLogEntryHandler invalidLogEntryHandler, Monitor monitor) throws java.io.IOException
		 public static void Analyze( FileSystemAbstraction fileSystem, File storeDirOrLogFile, InvalidLogEntryHandler invalidLogEntryHandler, Monitor monitor )
		 {
			  File firstFile;
			  LogVersionBridge bridge;
			  ReadAheadLogChannel channel;
			  LogEntryReader<ReadableClosablePositionAwareChannel> entryReader;
			  LogPositionMarker positionMarker;
			  if ( storeDirOrLogFile.Directory )
			  {
					// Use natural log version bridging if a directory is supplied
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles = org.Neo4Net.kernel.impl.transaction.log.files.LogFilesBuilder.logFilesBasedOnlyBuilder(storeDirOrLogFile, fileSystem).build();
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDirOrLogFile, fileSystem ).build();
					bridge = new ReaderLogVersionBridgeAnonymousInnerClass( logFiles, monitor, channel );
					long lowestLogVersion = logFiles.LowestLogVersion;
					if ( lowestLogVersion < 0 )
					{
						 throw new System.InvalidOperationException( format( "Transaction logs at '%s' not found.", storeDirOrLogFile ) );
					}
					firstFile = logFiles.GetLogFileForVersion( lowestLogVersion );
					monitor.LogFile( firstFile, lowestLogVersion );
			  }
			  else
			  {
					// Use no bridging, simply reading this single log file if a file is supplied
					firstFile = storeDirOrLogFile;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles = org.Neo4Net.kernel.impl.transaction.log.files.LogFilesBuilder.logFilesBasedOnlyBuilder(storeDirOrLogFile, fileSystem).build();
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDirOrLogFile, fileSystem ).build();
					monitor.LogFile( firstFile, logFiles.GetLogVersion( firstFile ) );
					bridge = NO_MORE_CHANNELS;
			  }

			  channel = new ReadAheadLogChannel( openVersionedChannel( fileSystem, firstFile ), bridge );
			  entryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), invalidLogEntryHandler );
			  positionMarker = new LogPositionMarker();
			  using ( TransactionLogEntryCursor cursor = new TransactionLogEntryCursor( new LogEntryCursor( entryReader, channel ) ) )
			  {
					channel.GetCurrentPosition( positionMarker );
					while ( cursor.Next() )
					{
						 LogEntry[] tx = cursor.Get();
						 if ( tx.Length == 1 && tx[0].Type == CHECK_POINT )
						 {
							  monitor.Checkpoint( tx[0].As(), positionMarker.NewPosition() );
						 }
						 else
						 {
							  monitor.Transaction( tx );
						 }
					}
			  }
		 }

		 private class ReaderLogVersionBridgeAnonymousInnerClass : ReaderLogVersionBridge
		 {
			 private Neo4Net.tools.dump.TransactionLogAnalyzer.Monitor _monitor;
			 private ReadAheadLogChannel _channel;
			 private LogFiles _logFiles;

			 public ReaderLogVersionBridgeAnonymousInnerClass( LogFiles logFiles, Neo4Net.tools.dump.TransactionLogAnalyzer.Monitor monitor, ReadAheadLogChannel channel ) : base( logFiles )
			 {
				 this._monitor = monitor;
				 this._channel = channel;
				 this._logFiles = logFiles;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.transaction.log.LogVersionedStoreChannel next(org.Neo4Net.kernel.impl.transaction.log.LogVersionedStoreChannel channel) throws java.io.IOException
			 public override LogVersionedStoreChannel next( LogVersionedStoreChannel channel )
			 {
				  LogVersionedStoreChannel next = base.next( channel );
				  if ( next != channel )
				  {
						_monitor.logFile( _logFiles.getLogFileForVersion( next.Version ), next.Version );
				  }
				  return next;
			 }
		 }

		 private class CombinedMonitor : Monitor
		 {
			  internal readonly Monitor[] Monitors;

			  internal CombinedMonitor( Monitor[] monitors )
			  {
					this.Monitors = monitors;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void logFile(java.io.File file, long logVersion) throws java.io.IOException
			  public override void LogFile( File file, long logVersion )
			  {
					foreach ( Monitor monitor in Monitors )
					{
						 monitor.LogFile( file, logVersion );
					}
			  }

			  public override void Transaction( LogEntry[] transactionEntries )
			  {
					foreach ( Monitor monitor in Monitors )
					{
						 monitor.Transaction( transactionEntries );
					}
			  }

			  public override void Checkpoint( CheckPoint checkpoint, LogPosition checkpointEntryPosition )
			  {
					foreach ( Monitor monitor in Monitors )
					{
						 monitor.Checkpoint( checkpoint, checkpointEntryPosition );
					}
			  }
		 }
	}

}