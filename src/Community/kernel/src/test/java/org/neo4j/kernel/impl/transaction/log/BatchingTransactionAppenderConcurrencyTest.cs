using System;
using System.Threading;

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
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Adversary = Neo4Net.Adversaries.Adversary;
	using ClassGuardedAdversary = Neo4Net.Adversaries.ClassGuardedAdversary;
	using CountingAdversary = Neo4Net.Adversaries.CountingAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using DelegatingStoreChannel = Neo4Net.Graphdb.mockfs.DelegatingStoreChannel;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileSystemLifecycleAdapter = Neo4Net.Io.fs.FileSystemLifecycleAdapter;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogForceWaitEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using NullLog = Neo4Net.Logging.NullLog;
	using Race = Neo4Net.Test.Race;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.DoubleLatch.awaitLatch;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.ThreadTestUtils.awaitThreadState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.ThreadTestUtils.fork;

	public class BatchingTransactionAppenderConcurrencyTest
	{
		private bool InstanceFieldsInitialized = false;

		public BatchingTransactionAppenderConcurrencyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fileSystemRule );
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDirectory ).around( _life );
		}


		 private static readonly long _millisecondsToWait = TimeUnit.SECONDS.toMillis( 10 );

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpExecutor()
		 public static void SetUpExecutor()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownExecutor()
		 public static void TearDownExecutor()
		 {
			  _executor.shutdown();
			  _executor = null;
		 }

		 private readonly LifeRule _life = new LifeRule();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDirectory).around(life);
		 public RuleChain RuleChain;

		 private readonly LogAppendEvent _logAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null;
		 private readonly LogFiles _logFiles = mock( typeof( TransactionLogFiles ) );
		 private readonly LogFile _logFile = mock( typeof( LogFile ) );
		 private readonly LogRotation _logRotation = LogRotation.NO_ROTATION;
		 private readonly TransactionMetadataCache _transactionMetadataCache = new TransactionMetadataCache();
		 private readonly TransactionIdStore _transactionIdStore = new SimpleTransactionIdStore();
		 private readonly SimpleLogVersionRepository _logVersionRepository = new SimpleLogVersionRepository();
		 private readonly IdOrderingQueue _explicitIndexTransactionOrdering = IdOrderingQueue.BYPASS;
		 private readonly DatabaseHealth _databaseHealth = mock( typeof( DatabaseHealth ) );
		 private readonly Semaphore _forceSemaphore = new Semaphore( 0 );

		 private readonly BlockingQueue<ChannelCommand> _channelCommandQueue = new LinkedBlockingQueue<ChannelCommand>( 2 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _logFiles.LogFile ).thenReturn( _logFile );
			  when( _logFile.Writer ).thenReturn( new CommandQueueChannel( this ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceLogChannel() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForceLogChannel()
		 {
			  BatchingTransactionAppender appender = _life.add( CreateTransactionAppender() );
			  _life.start();

			  appender.ForceAfterAppend( _logAppendEvent );

			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.EmptyBufferIntoChannelAndClearIt) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Force) );
			  assertTrue( _channelCommandQueue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitForOngoingForceToCompleteBeforeForcingAgain() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForOngoingForceToCompleteBeforeForcingAgain()
		 {
			  _channelCommandQueue.put( ChannelCommand.Dummy );

			  // The 'emptyBuffer...' command will be put into the queue, and then it'll block on 'force' because the queue
			  // will be at capacity.

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BatchingTransactionAppender appender = life.add(createTransactionAppender());
			  BatchingTransactionAppender appender = _life.add( CreateTransactionAppender() );
			  _life.start();

			  ThreadStart runnable = CreateForceAfterAppendRunnable( appender );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(runnable);
			  Future<object> future = _executor.submit( runnable );

			  _forceSemaphore.acquire();

			  Thread otherThread = fork( runnable );
			  awaitThreadState( otherThread, _millisecondsToWait, Thread.State.TIMED_WAITING );

			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Dummy) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.EmptyBufferIntoChannelAndClearIt) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Force) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.EmptyBufferIntoChannelAndClearIt) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Force) );
			  future.get();
			  otherThread.Join();
			  assertTrue( _channelCommandQueue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchUpMultipleWaitingForceRequests() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBatchUpMultipleWaitingForceRequests()
		 {
			  _channelCommandQueue.put( ChannelCommand.Dummy );

			  // The 'emptyBuffer...' command will be put into the queue, and then it'll block on 'force' because the queue
			  // will be at capacity.

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BatchingTransactionAppender appender = life.add(createTransactionAppender());
			  BatchingTransactionAppender appender = _life.add( CreateTransactionAppender() );
			  _life.start();

			  ThreadStart runnable = CreateForceAfterAppendRunnable( appender );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(runnable);
			  Future<object> future = _executor.submit( runnable );

			  _forceSemaphore.acquire();

			  Thread[] otherThreads = new Thread[10];
			  for ( int i = 0; i < otherThreads.Length; i++ )
			  {
					otherThreads[i] = fork( runnable );
			  }
			  foreach ( Thread otherThread in otherThreads )
			  {
					awaitThreadState( otherThread, _millisecondsToWait, Thread.State.TIMED_WAITING );
			  }

			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Dummy) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.EmptyBufferIntoChannelAndClearIt) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Force) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.EmptyBufferIntoChannelAndClearIt) );
			  assertThat( _channelCommandQueue.take(), @is(ChannelCommand.Force) );
			  future.get();
			  foreach ( Thread otherThread in otherThreads )
			  {
					otherThread.Join();
			  }
			  assertTrue( _channelCommandQueue.Empty );
		 }

		 /*
		  * There was an issue where if multiple concurrent appending threads did append and they moved on
		  * to await a force, where the force would fail and the one doing the force would raise a panic...
		  * the other threads may not notice the panic and move on to mark those transactions as committed
		  * and notice the panic later (which would be too late).
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAllConcurrentAppendersSeePanic() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveAllConcurrentAppendersSeePanic()
		 {
			  // GIVEN
			  Adversary adversary = new ClassGuardedAdversary( new CountingAdversary( 1, true ), FailMethod( typeof( BatchingTransactionAppender ), "force" ) );
			  EphemeralFileSystemAbstraction efs = new EphemeralFileSystemAbstraction();
			  FileSystemAbstraction fs = new AdversarialFileSystemAbstraction( adversary, efs );
			  _life.add( new FileSystemLifecycleAdapter( fs ) );
			  DatabaseHealth databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLog.Instance );
			  LogFiles logFiles = LogFilesBuilder.builder( _testDirectory.databaseLayout(), fs ).withLogVersionRepository(_logVersionRepository).withTransactionIdStore(_transactionIdStore).build();
			  _life.add( logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BatchingTransactionAppender appender = life.add(new BatchingTransactionAppender(logFiles, logRotation, transactionMetadataCache, transactionIdStore, explicitIndexTransactionOrdering, databaseHealth));
			  BatchingTransactionAppender appender = _life.add( new BatchingTransactionAppender( logFiles, _logRotation, _transactionMetadataCache, _transactionIdStore, _explicitIndexTransactionOrdering, databaseHealth ) );
			  _life.start();

			  // WHEN
			  int numberOfAppenders = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch trap = new java.util.concurrent.CountDownLatch(numberOfAppenders);
			  System.Threading.CountdownEvent trap = new System.Threading.CountdownEvent( numberOfAppenders );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.tracing.LogAppendEvent beforeForceTrappingEvent = new org.neo4j.kernel.impl.transaction.tracing.LogAppendEvent_Empty()
			  LogAppendEvent beforeForceTrappingEvent = new LogAppendEvent_EmptyAnonymousInnerClass( this, trap );
			  Race race = new Race();
			  for ( int i = 0; i < numberOfAppenders; i++ )
			  {
					race.AddContestant(() =>
					{
					 try
					 {
						  // Append to the log, the LogAppenderEvent will have all of the appending threads
						  // do wait for all of the other threads to start the force thing
						  appender.Append( Tx(), beforeForceTrappingEvent );
						  fail( "No transaction should be considered appended" );
					 }
					 catch ( IOException )
					 {
						  // Good, we know that this test uses an adversarial file system which will throw
						  // an exception in BatchingTransactionAppender#force, and since all these transactions
						  // will append and be forced in the same batch, where the force will fail then
						  // all these transactions should fail. If there's any transaction not failing then
						  // it just didn't notice the panic, which would be potentially hazardous.
					 }
					});
			  }

			  // THEN perform the race. The relevant assertions are made inside the contestants.
			  race.Go();
		 }

		 private class LogAppendEvent_EmptyAnonymousInnerClass : Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Empty
		 {
			 private readonly BatchingTransactionAppenderConcurrencyTest _outerInstance;

			 private System.Threading.CountdownEvent _trap;

			 public LogAppendEvent_EmptyAnonymousInnerClass( BatchingTransactionAppenderConcurrencyTest outerInstance, System.Threading.CountdownEvent trap )
			 {
				 this.outerInstance = outerInstance;
				 this._trap = trap;
			 }

			 public override LogForceWaitEvent beginLogForceWait()
			 {
				  _trap.Signal();
				  awaitLatch( _trap );
				  return base.beginLogForceWait();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databasePanicShouldHandleOutOfMemoryErrors() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabasePanicShouldHandleOutOfMemoryErrors()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch panicLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent panicLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch adversaryLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent adversaryLatch = new System.Threading.CountdownEvent( 1 );
			  OutOfMemoryAwareFileSystem fs = new OutOfMemoryAwareFileSystem();

			  _life.add( new FileSystemLifecycleAdapter( fs ) );
			  DatabaseHealth slowPanicDatabaseHealth = new SlowPanickingDatabaseHealth( panicLatch, adversaryLatch );
			  LogFiles logFiles = LogFilesBuilder.builder( _testDirectory.databaseLayout(), fs ).withLogVersionRepository(_logVersionRepository).withTransactionIdStore(_transactionIdStore).build();
			  _life.add( logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BatchingTransactionAppender appender = life.add(new BatchingTransactionAppender(logFiles, logRotation, transactionMetadataCache, transactionIdStore, explicitIndexTransactionOrdering, slowPanicDatabaseHealth));
			  BatchingTransactionAppender appender = _life.add( new BatchingTransactionAppender( logFiles, _logRotation, _transactionMetadataCache, _transactionIdStore, _explicitIndexTransactionOrdering, slowPanicDatabaseHealth ) );
			  _life.start();

			  // Commit initial transaction
			  appender.Append( Tx(), Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null );

			  // Try to commit one transaction, will fail during flush with OOM, but not actually panic
			  fs.ShouldOOM = true;
			  Future<long> failingTransaction = _executor.submit( () => appender.Append(Tx(), Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null) );
			  panicLatch.await();

			  // Try to commit one additional transaction, should fail since database has already panicked
			  fs.ShouldOOM = false;
			  try
			  {
					appender.Append( Tx(), new LogAppendEvent_EmptyAnonymousInnerClass2(this, adversaryLatch) );
					fail( "Should have failed since database should have panicked" );
			  }
			  catch ( IOException e )
			  {
					assertTrue( e.Message.contains( "The database has encountered a critical error" ) );
			  }

			  // Check that we actually got an OutOfMemoryError
			  try
			  {
					failingTransaction.get();
					fail( "Should have failed with OutOfMemoryError error" );
			  }
			  catch ( ExecutionException e )
			  {
					assertTrue( e.InnerException is System.OutOfMemoryException );
			  }

			  // Check number of transactions, should only have one
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();

			  assertEquals( logFiles.LowestLogVersion, logFiles.HighestLogVersion );
			  long version = logFiles.HighestLogVersion;

			  using ( LogVersionedStoreChannel channel = logFiles.OpenForVersion( version ), ReadAheadLogChannel readAheadLogChannel = new ReadAheadLogChannel( channel ), LogEntryCursor cursor = new LogEntryCursor( logEntryReader, readAheadLogChannel ) )
			  {
					LogEntry entry;
					long numberOfTransactions = 0;
					while ( cursor.Next() )
					{
						 entry = cursor.Get();
						 if ( entry is LogEntryCommit )
						 {
							  numberOfTransactions++;
						 }
					}
					assertEquals( 1, numberOfTransactions );
			  }
		 }

		 private class LogAppendEvent_EmptyAnonymousInnerClass2 : Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Empty
		 {
			 private readonly BatchingTransactionAppenderConcurrencyTest _outerInstance;

			 private System.Threading.CountdownEvent _adversaryLatch;

			 public LogAppendEvent_EmptyAnonymousInnerClass2( BatchingTransactionAppenderConcurrencyTest outerInstance, System.Threading.CountdownEvent adversaryLatch )
			 {
				 this.outerInstance = outerInstance;
				 this._adversaryLatch = adversaryLatch;
			 }

			 public override LogForceWaitEvent beginLogForceWait()
			 {
				  _adversaryLatch.Signal();
				  return base.beginLogForceWait();
			 }
		 }

		 private class OutOfMemoryAwareFileSystem : EphemeralFileSystemAbstraction
		 {
			  internal volatile bool ShouldOOM;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
			  public override StoreChannel Open( File fileName, OpenMode openMode )
			  {
				  lock ( this )
				  {
						return new DelegatingStoreChannelAnonymousInnerClass( this, base.Open( fileName, openMode ) );
				  }
			  }

			  private class DelegatingStoreChannelAnonymousInnerClass : DelegatingStoreChannel
			  {
				  private readonly OutOfMemoryAwareFileSystem _outerInstance;

				  public DelegatingStoreChannelAnonymousInnerClass( OutOfMemoryAwareFileSystem outerInstance, StoreChannel open ) : base( open )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
				  public override void writeAll( ByteBuffer src )
				  {
						if ( _outerInstance.shouldOOM )
						{
							 // Allocation of a temporary buffer happens the first time a thread tries to write
							 // so this is a perfectly plausible scenario.
							 throw new System.OutOfMemoryException( "Temporary buffer allocation failed" );
						}
						base.writeAll( src );
				  }
			  }
		 }

		 private class SlowPanickingDatabaseHealth : DatabaseHealth
		 {
			  internal readonly System.Threading.CountdownEvent PanicLatch;
			  internal readonly System.Threading.CountdownEvent AdversaryLatch;

			  internal SlowPanickingDatabaseHealth( System.Threading.CountdownEvent panicLatch, System.Threading.CountdownEvent adversaryLatch ) : base( mock( typeof( DatabasePanicEventGenerator ) ), NullLog.Instance )
			  {
					this.PanicLatch = panicLatch;
					this.AdversaryLatch = adversaryLatch;
			  }

			  public override void Panic( Exception cause )
			  {
					PanicLatch.Signal();
					try
					{
						 AdversaryLatch.await();
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
					base.Panic( cause );
			  }
		 }

		 protected internal virtual TransactionToApply Tx()
		 {
			  NodeRecord before = new NodeRecord( 0 );
			  NodeRecord after = new NodeRecord( 0 );
			  after.InUse = true;
			  Command.NodeCommand nodeCommand = new Command.NodeCommand( before, after );
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( singletonList( nodeCommand ) );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return new TransactionToApply( tx );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createForceAfterAppendRunnable(final BatchingTransactionAppender appender)
		 private ThreadStart CreateForceAfterAppendRunnable( BatchingTransactionAppender appender )
		 {
			  return () =>
			  {
				try
				{
					 appender.ForceAfterAppend( _logAppendEvent );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Predicate<StackTraceElement> failMethod(final Class klass, final String methodName)
		 private System.Predicate<StackTraceElement> FailMethod( Type klass, string methodName )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return element => element.ClassName.Equals( klass.FullName ) && element.MethodName.Equals( methodName );
		 }

		 private BatchingTransactionAppender CreateTransactionAppender()
		 {
			  return new BatchingTransactionAppender( _logFiles, _logRotation, _transactionMetadataCache, _transactionIdStore, _explicitIndexTransactionOrdering, _databaseHealth );
		 }

		 private enum ChannelCommand
		 {
			  EmptyBufferIntoChannelAndClearIt,
			  Force,
			  Dummy
		 }

		 internal class CommandQueueChannel : InMemoryClosableChannel, Flushable
		 {
			 private readonly BatchingTransactionAppenderConcurrencyTest _outerInstance;

			 public CommandQueueChannel( BatchingTransactionAppenderConcurrencyTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override Flushable PrepareForFlush()
			  {
					try
					{
						 outerInstance.channelCommandQueue.put( ChannelCommand.EmptyBufferIntoChannelAndClearIt );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
			  public override void Flush()
			  {
					try
					{
						 outerInstance.forceSemaphore.release();
						 outerInstance.channelCommandQueue.put( ChannelCommand.Force );
					}
					catch ( InterruptedException e )
					{
						 throw new IOException( e );
					}
			  }
		 }
	}

}