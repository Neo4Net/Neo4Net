using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogHeaderReader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeaderReader;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogRotationImpl = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotationImpl;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogForceEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceEvent;
	using LogForceWaitEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using LogRotateEvent = Neo4Net.Kernel.impl.transaction.tracing.LogRotateEvent;
	using SerializeTransactionEvent = Neo4Net.Kernel.impl.transaction.tracing.SerializeTransactionEvent;
	using SynchronizedArrayIdOrderingQueue = Neo4Net.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using KernelEventHandlers = Neo4Net.Kernel.Internal.KernelEventHandlers;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLog = Neo4Net.Logging.NullLog;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class BatchingTransactionAppenderRotationIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystem = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystem = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );
		 private readonly SimpleLogVersionRepository _logVersionRepository = new SimpleLogVersionRepository();
		 private readonly SimpleTransactionIdStore _transactionIdStore = new SimpleTransactionIdStore();
		 private readonly Monitors _monitors = new Monitors();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctLastAppliedToPreviousLogTransactionInHeaderOnLogFileRotation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CorrectLastAppliedToPreviousLogTransactionInHeaderOnLogFileRotation()
		 {
			  LogFiles logFiles = GetLogFiles( _logVersionRepository, _transactionIdStore );
			  Life.add( logFiles );
			  DatabaseHealth databaseHealth = DatabaseHealth;

			  LogRotationImpl logRotation = new LogRotationImpl( _monitors.newMonitor( typeof( Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor ) ), logFiles, databaseHealth );
			  TransactionMetadataCache transactionMetadataCache = new TransactionMetadataCache();
			  SynchronizedArrayIdOrderingQueue idOrderingQueue = new SynchronizedArrayIdOrderingQueue();

			  BatchingTransactionAppender transactionAppender = new BatchingTransactionAppender( logFiles, logRotation, transactionMetadataCache, _transactionIdStore, idOrderingQueue, databaseHealth );

			  Life.add( transactionAppender );

			  LogAppendEvent logAppendEvent = new RotationLogAppendEvent( logRotation );
			  TransactionToApply transactionToApply = PrepareTransaction();
			  transactionAppender.Append( transactionToApply, logAppendEvent );

			  assertEquals( 1, logFiles.HighestLogVersion );
			  File highestLogFile = logFiles.HighestLogFile;
			  LogHeader logHeader = LogHeaderReader.readLogHeader( FileSystem, highestLogFile );
			  assertEquals( 2, logHeader.LastCommittedTxId );
		 }

		 private static TransactionToApply PrepareTransaction()
		 {
			  IList<StorageCommand> commands = CreateCommands();
			  PhysicalTransactionRepresentation transactionRepresentation = new PhysicalTransactionRepresentation( commands );
			  transactionRepresentation.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return new TransactionToApply( transactionRepresentation );
		 }

		 private static IList<StorageCommand> CreateCommands()
		 {
			  return singletonList( new Command.NodeCommand( new NodeRecord( 1L ), new NodeRecord( 2L ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.files.LogFiles getLogFiles(org.neo4j.kernel.impl.transaction.SimpleLogVersionRepository logVersionRepository, org.neo4j.kernel.impl.transaction.SimpleTransactionIdStore transactionIdStore) throws java.io.IOException
		 private LogFiles GetLogFiles( SimpleLogVersionRepository logVersionRepository, SimpleTransactionIdStore transactionIdStore )
		 {
			  return LogFilesBuilder.builder( TestDirectory.databaseLayout(), FileSystem.get() ).withLogVersionRepository(logVersionRepository).withTransactionIdStore(transactionIdStore).build();
		 }

		 private static DatabaseHealth DatabaseHealth
		 {
			 get
			 {
				  DatabasePanicEventGenerator databasePanicEventGenerator = new DatabasePanicEventGenerator( new KernelEventHandlers( NullLog.Instance ) );
				  return new DatabaseHealth( databasePanicEventGenerator, NullLog.Instance );
			 }
		 }

		 private class RotationLogAppendEvent : LogAppendEvent
		 {

			  internal readonly LogRotationImpl LogRotation;

			  internal RotationLogAppendEvent( LogRotationImpl logRotation )
			  {
					this.LogRotation = logRotation;
			  }

			  public override LogForceWaitEvent BeginLogForceWait()
			  {
					return null;
			  }

			  public override LogForceEvent BeginLogForce()
			  {
					return null;
			  }

			  public override void Close()
			  {
			  }

			  public virtual bool LogRotated
			  {
				  set
				  {
   
				  }
			  }

			  public override LogRotateEvent BeginLogRotate()
			  {
					return null;
			  }

			  public override SerializeTransactionEvent BeginSerializeTransaction()
			  {
					return () =>
					{
					 try
					 {
						  LogRotation.rotateLogFile();
					 }
					 catch ( IOException e )
					 {
						  throw new Exception( "Should be able to rotate file", e );
					 }
					};
			  }
		 }
	}

}