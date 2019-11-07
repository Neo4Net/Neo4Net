using System;
using System.Collections.Generic;
using System.Threading;

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
	using RuleChain = org.junit.rules.RuleChain;


	using ByteUnit = Neo4Net.Io.ByteUnit;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFileCreationMonitor = Neo4Net.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogRotationImpl = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotationImpl;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using NullLog = Neo4Net.Logging.NullLog;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using Race = Neo4Net.Test.Race;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.command.Commands.createNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.command.Commands.createProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.tracing.LogAppendEvent_Fields.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.util.IdOrderingQueue.BYPASS;

	public class TransactionLogAppendAndRotateIT
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionLogAppendAndRotateIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = outerRule( _directory ).around( _life ).around( _fileSystemRule );
		}

		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain chain = outerRule(directory).around(life).around(fileSystemRule);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepTransactionsIntactWhenConcurrentlyRotationAndAppending() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepTransactionsIntactWhenConcurrentlyRotationAndAppending()
		 {
			  // GIVEN
			  LogVersionRepository logVersionRepository = new SimpleLogVersionRepository();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), _fileSystemRule.get() ).withLogVersionRepository(logVersionRepository).withRotationThreshold(ByteUnit.mebiBytes(1)).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  _life.add( logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean end = new AtomicBoolean();
			  AllTheMonitoring monitoring = new AllTheMonitoring( end, 100 );
			  TransactionIdStore txIdStore = new SimpleTransactionIdStore();
			  TransactionMetadataCache metadataCache = new TransactionMetadataCache();
			  monitoring.LogFile = logFiles.LogFile;
			  DatabaseHealth health = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLog.Instance );
			  LogRotation rotation = new LogRotationImpl( monitoring, logFiles, health );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TransactionAppender appender = life.add(new BatchingTransactionAppender(logFiles, rotation, metadataCache, txIdStore, BYPASS, health));
			  TransactionAppender appender = _life.add( new BatchingTransactionAppender( logFiles, rotation, metadataCache, txIdStore, BYPASS, health ) );

			  // WHEN
			  Race race = new Race();
			  for ( int i = 0; i < 10; i++ )
			  {
					race.AddContestant(() =>
					{
					 while ( !end.get() )
					 {
						  try
						  {
								appender.Append( new TransactionToApply( SillyTransaction( 1_000 ) ), NULL );
						  }
						  catch ( Exception e )
						  {
								e.printStackTrace( System.out );
								end.set( true );
								fail( e.Message );
						  }
					 }
					});
			  }
			  race.AddContestant( EndAfterMax( 10, SECONDS, end ) );
			  race.Go();

			  // THEN
			  assertTrue( monitoring.NumberOfRotations() > 0 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable endAfterMax(final int time, final java.util.concurrent.TimeUnit unit, final java.util.concurrent.atomic.AtomicBoolean end)
		 private ThreadStart EndAfterMax( int time, TimeUnit unit, AtomicBoolean end )
		 {
			  return () =>
			  {
				long endTime = currentTimeMillis() + unit.toMillis(time);
				while ( currentTimeMillis() < endTime && !end.get() )
				{
					 parkNanos( MILLISECONDS.toNanos( 50 ) );
				}
				end.set( true );
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertWholeTransactionsIn(Neo4Net.kernel.impl.transaction.log.files.LogFile logFile, long logVersion) throws java.io.IOException
		 private static void AssertWholeTransactionsIn( LogFile logFile, long logVersion )
		 {
			  using ( ReadableLogChannel reader = logFile.GetReader( new LogPosition( logVersion, LOG_HEADER_SIZE ) ) )
			  {
					VersionAwareLogEntryReader<ReadableLogChannel> entryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();
					LogEntry entry;
					bool inTx = false;
					int transactions = 0;
					while ( ( entry = entryReader.ReadLogEntry( reader ) ) != null )
					{
						 if ( !inTx ) // Expects start entry
						 {
							  assertTrue( entry is LogEntryStart );
							  inTx = true;
						 }
						 else // Expects command/commit entry
						 {
							  assertTrue( entry is LogEntryCommand || entry is LogEntryCommit );
							  if ( entry is LogEntryCommit )
							  {
									inTx = false;
									transactions++;
							  }
						 }
					}
					assertFalse( inTx );
					assertTrue( transactions > 0 );
			  }
		 }

		 private TransactionRepresentation SillyTransaction( int size )
		 {
			  ICollection<StorageCommand> commands = new List<StorageCommand>( size );
			  for ( int i = 0; i < size; i++ )
			  {
					// The actual data isn't super important
					commands.Add( createNode( i ) );
					commands.Add( createProperty( i, PropertyType.INT, 0 ) );
			  }
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return tx;
		 }

		 private class AllTheMonitoring : LogFileCreationMonitor, Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor
		 {
			  internal readonly AtomicBoolean End;
			  internal readonly int MaxNumberOfRotations;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile LogFile LogFileConflict;
			  internal volatile int Rotations;

			  internal AllTheMonitoring( AtomicBoolean end, int maxNumberOfRotations )
			  {
					this.End = end;
					this.MaxNumberOfRotations = maxNumberOfRotations;
			  }

			  internal virtual LogFile LogFile
			  {
				  set
				  {
						this.LogFileConflict = value;
				  }
			  }

			  public override void StartedRotating( long currentVersion )
			  {
			  }

			  public override void FinishedRotating( long currentVersion )
			  {
					try
					{
						 AssertWholeTransactionsIn( LogFileConflict, currentVersion );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
					finally
					{
						 if ( Rotations++ > MaxNumberOfRotations )
						 {
							  End.set( true );
						 }
					}
			  }

			  internal virtual int NumberOfRotations()
			  {
					return Rotations;
			  }

			  public override void Created( File logFile, long logVersion, long lastTransactionId )
			  {
			  }
		 }
	}

}