using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.tools.txlog
{
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using LongLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap;


	using Args = Neo4Net.Helpers.Args;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using Neo4Net.tools.txlog.checktypes;
	using CheckTypes = Neo4Net.tools.txlog.checktypes.CheckTypes;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.util.TransactionLogUtils.openLogs;

	/// <summary>
	/// Tool that verifies consistency of transaction logs.
	/// 
	/// Transaction log is considered consistent when every command's before state is the same as after state for
	/// corresponding record in previously committed transaction.
	/// 
	/// Tool expects a single argument - directory with transaction logs.
	/// It then simply iterates over all commands in those logs, compares before state for current record with previously
	/// seen after state and stores after state for current record, if before state is consistent.
	/// </summary>
	public class CheckTxLogs
	{
		 private const string HELP_FLAG = "help";
		 private const string VALIDATE_CHECKPOINTS_FLAG = "validate-checkpoints";
		 private const string CHECKS = "checks";
		 private const string SEPARATOR = ",";

		 private readonly PrintStream @out;
		 private readonly FileSystemAbstraction _fs;

		 public CheckTxLogs( PrintStream @out, FileSystemAbstraction fs )
		 {
			  this.@out = @out;
			  this._fs = fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  PrintStream @out = System.out;
			  Args arguments = Args.withFlags( HELP_FLAG, VALIDATE_CHECKPOINTS_FLAG ).parse( args );
			  if ( arguments.GetBoolean( HELP_FLAG ) )
			  {
					PrintUsageAndExit( @out );
			  }

			  bool validateCheckPoints = arguments.GetBoolean( VALIDATE_CHECKPOINTS_FLAG );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.tools.txlog.checktypes.CheckType<?,?>[] checkTypes = parseChecks(arguments);
			  CheckType<object, ?>[] checkTypes = ParseChecks( arguments );
			  File dir = ParseDir( @out, arguments );

			  bool success = false;
			  using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction() )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( dir, fs ).build();
					int numberOfLogFilesFound = ( int )( logFiles.HighestLogVersion - logFiles.LowestLogVersion + 1 );
					@out.println( "Found " + numberOfLogFilesFound + " log files to verify in " + dir.CanonicalPath );

					CheckTxLogs tool = new CheckTxLogs( @out, fs );
					PrintingInconsistenciesHandler handler = new PrintingInconsistenciesHandler( @out );
					success = tool.Scan( logFiles, handler, checkTypes );

					if ( validateCheckPoints )
					{
						 success &= tool.ValidateCheckPoints( logFiles, handler );
					}
			  }

			  if ( !success )
			  {
					Environment.Exit( 1 );
			  }
		 }

		 // used in other projects do not remove!
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean checkAll(java.io.File storeDirectory) throws java.io.IOException
		 public virtual bool CheckAll( File storeDirectory )
		 {
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDirectory, _fs ).build();
			  InconsistenciesHandler handler = new PrintingInconsistenciesHandler( @out );
			  bool success = Scan( logFiles, handler, CheckTypes.CHECK_TYPES );
			  success &= ValidateCheckPoints( logFiles, handler );
			  return success;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean validateCheckPoints(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, InconsistenciesHandler handler) throws java.io.IOException
		 internal virtual bool ValidateCheckPoints( LogFiles logFiles, InconsistenciesHandler handler )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long lowestLogVersion = logFiles.getLowestLogVersion();
			  long lowestLogVersion = logFiles.LowestLogVersion;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long highestLogVersion = logFiles.getHighestLogVersion();
			  long highestLogVersion = logFiles.HighestLogVersion;
			  bool success = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongLongMap logFileSizes = new org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap();
			  MutableLongLongMap logFileSizes = new LongLongHashMap();
			  for ( long i = lowestLogVersion; i <= highestLogVersion; i++ )
			  {
					logFileSizes.put( i, _fs.getFileSize( logFiles.GetLogFileForVersion( i ) ) );
			  }

			  using ( LogEntryCursor logEntryCursor = OpenLogEntryCursor( logFiles ) )
			  {
					while ( logEntryCursor.Next() )
					{
						 LogEntry logEntry = logEntryCursor.Get();
						 if ( logEntry is CheckPoint )
						 {
							  LogPosition logPosition = logEntry.As<CheckPoint>().LogPosition;
							  // if the file has been pruned we cannot validate the check point
							  if ( logPosition.LogVersion >= lowestLogVersion )
							  {
									long size = logFileSizes.getIfAbsent( logPosition.LogVersion, -1 );
									if ( logPosition.ByteOffset < 0 || size < 0 || logPosition.ByteOffset > size )
									{
										 long currentLogVersion = logEntryCursor.CurrentLogVersion;
										 handler.ReportInconsistentCheckPoint( currentLogVersion, logPosition, size );
										 success = false;
									}

							  }
						 }
					}
			  }
			  return success;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.log.LogEntryCursor openLogEntryCursor(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
		 internal virtual LogEntryCursor OpenLogEntryCursor( LogFiles logFiles )
		 {
			  return openLogs( _fs, logFiles );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean scan(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, InconsistenciesHandler handler, org.Neo4Net.tools.txlog.checktypes.CheckType<?,?>... checkTypes) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 internal virtual bool Scan( LogFiles logFiles, InconsistenciesHandler handler, params CheckType<object, ?>[] checkTypes )
		 {
			  bool success = true;
			  bool checkTxIds = true;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.tools.txlog.checktypes.CheckType<?,?> checkType : checkTypes)
			  foreach ( CheckType<object, ?> checkType in checkTypes )
			  {
					success &= Scan( logFiles, handler, checkType, checkTxIds );
					checkTxIds = false;
			  }
			  return success;
		 }

		 private class CommandAndLogVersion
		 {
			 private readonly CheckTxLogs _outerInstance;

			  internal StorageCommand Command;
			  internal long LogVersion;

			  internal CommandAndLogVersion( CheckTxLogs outerInstance, StorageCommand command, long logVersion )
			  {
				  this._outerInstance = outerInstance;
					this.Command = command;
					this.LogVersion = logVersion;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <C extends org.Neo4Net.kernel.impl.transaction.command.Command, R extends org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord> boolean scan(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, InconsistenciesHandler handler, org.Neo4Net.tools.txlog.checktypes.CheckType<C,R> check, boolean checkTxIds) throws java.io.IOException
		 private bool Scan<C, R>( LogFiles logFiles, InconsistenciesHandler handler, CheckType<C, R> check, bool checkTxIds ) where C : Neo4Net.Kernel.impl.transaction.command.Command where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  @out.println( "Checking logs for " + check.Name() + " inconsistencies" );
			  CommittedRecords<R> state = new CommittedRecords<R>( check );

			  IList<CommandAndLogVersion> txCommands = new List<CommandAndLogVersion>();
			  bool validLogs = true;
			  long commandsRead = 0;
			  long lastSeenTxId = BASE_TX_ID;
			  using ( LogEntryCursor logEntryCursor = OpenLogEntryCursor( logFiles ) )
			  {
					while ( logEntryCursor.Next() )
					{
						 LogEntry entry = logEntryCursor.Get();
						 if ( entry is LogEntryCommand )
						 {
							  StorageCommand command = ( ( LogEntryCommand ) entry ).Command;
							  if ( check.CommandClass().IsInstanceOfType(command) )
							  {
									long logVersion = logEntryCursor.CurrentLogVersion;
									txCommands.Add( new CommandAndLogVersion( this, command, logVersion ) );
							  }
						 }
						 else if ( entry is LogEntryCommit )
						 {
							  long txId = ( ( LogEntryCommit ) entry ).TxId;
							  if ( checkTxIds )
							  {
									validLogs &= CheckNoDuplicatedTxsInTheLog( lastSeenTxId, txId, handler );
									lastSeenTxId = txId;
							  }
							  foreach ( CommandAndLogVersion txCommand in txCommands )
							  {
									validLogs &= CheckAndHandleInconsistencies( txCommand, check, state, txId, handler );
							  }
							  txCommands.Clear();
						 }
						 commandsRead++;
					}
			  }
			  @out.println( "Processed " + commandsRead + " commands" );
			  @out.println( state );

			  if ( txCommands.Count > 0 )
			  {
					@out.println( "Found " + txCommands.Count + " uncommitted commands at the end." );
					foreach ( CommandAndLogVersion txCommand in txCommands )
					{
						 validLogs &= CheckAndHandleInconsistencies( txCommand, check, state, -1, handler );
					}
					txCommands.Clear();
			  }

			  return validLogs;
		 }

		 private bool CheckNoDuplicatedTxsInTheLog( long lastTxId, long currentTxId, InconsistenciesHandler handler )
		 {
			  bool isValid = lastTxId <= BASE_TX_ID || lastTxId + 1 == currentTxId;
			  if ( !isValid )
			  {
					handler.ReportInconsistentTxIdSequence( lastTxId, currentTxId );
			  }
			  return isValid;
		 }

		 private bool CheckAndHandleInconsistencies<C, R>( CommandAndLogVersion txCommand, CheckType<C, R> check, CommittedRecords<R> state, long txId, InconsistenciesHandler handler ) where C : Neo4Net.Kernel.impl.transaction.command.Command where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  C command = check.CommandClass().cast(txCommand.Command);

			  R before = check.Before( command );
			  R after = check.After( command );

			  Debug.Assert( before.Id == after.Id );

			  RecordInfo<R> lastSeen = state.Get( after.Id );

			  bool isValidRecord = ( lastSeen == null ) || check.Equal( before, lastSeen.Record() );
			  if ( !isValidRecord )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: handler.reportInconsistentCommand(lastSeen, new RecordInfo<>(before, txCommand.logVersion, txId));
					handler.ReportInconsistentCommand( lastSeen, new RecordInfo<object>( before, txCommand.LogVersion, txId ) );
			  }

			  state.Put( after, txCommand.LogVersion, txId );

			  return isValidRecord;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static org.Neo4Net.tools.txlog.checktypes.CheckType<?,?>[] parseChecks(org.Neo4Net.helpers.Args arguments)
		 private static CheckType<object, ?>[] ParseChecks( Args arguments )
		 {
			  string checks = arguments.Get( CHECKS );
			  if ( string.ReferenceEquals( checks, null ) )
			  {
					return CheckTypes.CHECK_TYPES;
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( checks.Split( SEPARATOR, true ) ).map( CheckTypes.fromName ).toArray( CheckType[]::new );
		 }

		 private static File ParseDir( PrintStream printStream, Args args )
		 {
			  if ( args.Orphans().Count != 1 )
			  {
					PrintUsageAndExit( printStream );
			  }
			  File dir = new File( args.Orphans()[0] );
			  if ( !dir.Directory )
			  {
					printStream.println( "Invalid directory: '" + dir + "'" );
					PrintUsageAndExit( printStream );
			  }
			  return dir;
		 }

		 private static void PrintUsageAndExit( PrintStream @out )
		 {
			  @out.println( "Tool expects single argument - directory with tx logs" );
			  @out.println( "Usage:" );
			  @out.println( "\t./checkTxLogs [options] <directory>" );
			  @out.println( "Options:" );
			  @out.println( "\t--help\t\tprints this description" );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  @out.println( "\t--checks='checkname[,...]'\t\tthe list of checks to perform. Checks available: " + java.util.org.Neo4Net.tools.txlog.checktypes.CheckTypes.CHECK_TYPES.Select( CheckType::name ).collect( Collectors.joining( SEPARATOR ) ) );
			  Environment.Exit( 1 );
		 }
	}

}