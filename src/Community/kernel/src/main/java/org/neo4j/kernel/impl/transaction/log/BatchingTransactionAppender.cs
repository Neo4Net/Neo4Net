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

	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogCheckPointEvent = Neo4Net.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using LogForceEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceEvent;
	using LogForceEvents = Neo4Net.Kernel.impl.transaction.tracing.LogForceEvents;
	using LogForceWaitEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using SerializeTransactionEvent = Neo4Net.Kernel.impl.transaction.tracing.SerializeTransactionEvent;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.TransactionToApply.TRANSACTION_ID_NOT_SPECIFIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryStart.checksum;

	/// <summary>
	/// Concurrently appends transactions to the transaction log, while coordinating with the log rotation and forcing the
	/// log file in batches for higher throughput in a concurrent scenario.
	/// </summary>
	public class BatchingTransactionAppender : LifecycleAdapter, TransactionAppender
	{
		 // For the graph store and schema indexes order-of-updates are managed by the high level entity locks
		 // such that changes are applied to the affected records in the same order that they are written to the
		 // log. For the explicit indexes there are no such locks, and hence no such ordering. This queue below
		 // is introduced to manage just that and is only used for transactions that contain any explicit index changes.
		 private readonly IdOrderingQueue _explicitIndexTransactionOrdering;

		 private readonly AtomicReference<ThreadLink> _threadLinkHead = new AtomicReference<ThreadLink>( ThreadLink.End );
		 private readonly TransactionMetadataCache _transactionMetadataCache;
		 private readonly LogFile _logFile;
		 private readonly LogRotation _logRotation;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogPositionMarker _positionMarker = new LogPositionMarker();
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly Lock _forceLock = new ReentrantLock();

		 private FlushablePositionAwareChannel _writer;
		 private TransactionLogWriter _transactionLogWriter;
		 private IndexCommandDetector _indexCommandDetector;

		 public BatchingTransactionAppender( LogFiles logFiles, LogRotation logRotation, TransactionMetadataCache transactionMetadataCache, TransactionIdStore transactionIdStore, IdOrderingQueue explicitIndexTransactionOrdering, DatabaseHealth databaseHealth )
		 {
			  this._logFile = logFiles.LogFile;
			  this._logRotation = logRotation;
			  this._transactionIdStore = transactionIdStore;
			  this._explicitIndexTransactionOrdering = explicitIndexTransactionOrdering;
			  this._databaseHealth = databaseHealth;
			  this._transactionMetadataCache = transactionMetadataCache;
		 }

		 public override void Start()
		 {
			  this._writer = _logFile.Writer;
			  this._indexCommandDetector = new IndexCommandDetector();
			  this._transactionLogWriter = new TransactionLogWriter( new LogEntryWriter( _writer ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long append(org.neo4j.kernel.impl.api.TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.LogAppendEvent logAppendEvent) throws java.io.IOException
		 public override long Append( TransactionToApply batch, LogAppendEvent logAppendEvent )
		 {
			  // Assigned base tx id just to make compiler happy
			  long lastTransactionId = TransactionIdStore_Fields.BASE_TX_ID;
			  // Synchronized with logFile to get absolute control over concurrent rotations happening
			  lock ( _logFile )
			  {
					// Assert that kernel is healthy before making any changes
					_databaseHealth.assertHealthy( typeof( IOException ) );
					using ( SerializeTransactionEvent serialiseEvent = logAppendEvent.BeginSerializeTransaction() )
					{
						 // Append all transactions in this batch to the log under the same logFile monitor
						 TransactionToApply tx = batch;
						 while ( tx != null )
						 {
							  long transactionId = _transactionIdStore.nextCommittingTransactionId();

							  // If we're in a scenario where we're merely replicating transactions, i.e. transaction
							  // id have already been generated by another entity we simply check that our id
							  // that we generated match that id. If it doesn't we've run into a problem we can't ´
							  // really recover from and would point to a bug somewhere.
							  MatchAgainstExpectedTransactionIdIfAny( transactionId, tx );

							  TransactionCommitment commitment = AppendToLog( tx.TransactionRepresentation(), transactionId );
							  tx.Commitment( commitment, transactionId );
							  tx.LogPosition( commitment.LogPosition() );
							  tx = tx.Next();
							  lastTransactionId = transactionId;
						 }
					}
			  }

			  // At this point we've appended all transactions in this batch, but we can't mark any of them
			  // as committed since they haven't been forced to disk yet. So here we force, or potentially
			  // piggy-back on another force, but anyway after this call below we can be sure that all our transactions
			  // in this batch exist durably on disk.
			  if ( ForceAfterAppend( logAppendEvent ) )
			  {
					// We got lucky and were the one forcing the log. It's enough if ones of all doing concurrent committers
					// checks the need for log rotation.
					bool logRotated = _logRotation.rotateLogIfNeeded( logAppendEvent );
					logAppendEvent.LogRotated = logRotated;
			  }

			  // Mark all transactions as committed
			  PublishAsCommitted( batch );

			  return lastTransactionId;
		 }

		 private void MatchAgainstExpectedTransactionIdIfAny( long transactionId, TransactionToApply tx )
		 {
			  long expectedTransactionId = tx.TransactionId();
			  if ( expectedTransactionId != TRANSACTION_ID_NOT_SPECIFIED )
			  {
					if ( transactionId != expectedTransactionId )
					{
						 System.InvalidOperationException ex = new System.InvalidOperationException( "Received " + tx.TransactionRepresentation() + " with txId:" + expectedTransactionId + " to be applied, but appending it ended up generating an unexpected txId:" + transactionId );
						 _databaseHealth.panic( ex );
						 throw ex;
					}
			  }
		 }

		 private void PublishAsCommitted( TransactionToApply batch )
		 {
			  while ( batch != null )
			  {
					batch.Commitment().publishAsCommitted();
					batch = batch.Next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkPoint(LogPosition logPosition, org.neo4j.kernel.impl.transaction.tracing.LogCheckPointEvent logCheckPointEvent) throws java.io.IOException
		 public override void CheckPoint( LogPosition logPosition, LogCheckPointEvent logCheckPointEvent )
		 {
			  // Synchronized with logFile to get absolute control over concurrent rotations happening
			  lock ( _logFile )
			  {
					try
					{
						 _transactionLogWriter.checkPoint( logPosition );
					}
					catch ( Exception cause )
					{
						 _databaseHealth.panic( cause );
						 throw cause;
					}
			  }
			  ForceAfterAppend( logCheckPointEvent );
		 }

		 /// <returns> A TransactionCommitment instance with metadata about the committed transaction, such as whether or not
		 /// this transaction contains any explicit index changes. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private TransactionCommitment appendToLog(org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction, long transactionId) throws java.io.IOException
		 private TransactionCommitment AppendToLog( TransactionRepresentation transaction, long transactionId )
		 {
			  // Reset command writer so that we, after we've written the transaction, can ask it whether or
			  // not any explicit index command was written. If so then there's additional ordering to care about below.
			  _indexCommandDetector.reset();

			  // The outcome of this try block is either of:
			  // a) transaction successfully appended, at which point we return a Commitment to be used after force
			  // b) transaction failed to be appended, at which point a kernel panic is issued
			  // The reason that we issue a kernel panic on failure in here is that at this point we're still
			  // holding the logFile monitor, and a failure to append needs to be communicated with potential
			  // log rotation, which will wait for all transactions closed or fail on kernel panic.
			  try
			  {
					LogPosition logPositionBeforeCommit = _writer.getCurrentPosition( _positionMarker ).newPosition();
					_transactionLogWriter.append( transaction, transactionId );
					LogPosition logPositionAfterCommit = _writer.getCurrentPosition( _positionMarker ).newPosition();

					long transactionChecksum = checksum( transaction.AdditionalHeader(), transaction.MasterId, transaction.AuthorId );
					_transactionMetadataCache.cacheTransactionMetadata( transactionId, logPositionBeforeCommit, transaction.MasterId, transaction.AuthorId, transactionChecksum, transaction.TimeCommitted );

					transaction.Accept( _indexCommandDetector );
					bool hasExplicitIndexChanges = _indexCommandDetector.hasWrittenAnyExplicitIndexCommand();
					if ( hasExplicitIndexChanges )
					{
						 // Offer this transaction id to the queue so that the explicit index applier can take part in the ordering
						 _explicitIndexTransactionOrdering.offer( transactionId );
					}
					return new TransactionCommitment( hasExplicitIndexChanges, transactionId, transactionChecksum, transaction.TimeCommitted, logPositionAfterCommit, _transactionIdStore );
			  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Throwable panic)
			  catch ( Exception panic )
			  {
					_databaseHealth.panic( panic );
					throw panic;
			  }
		 }

		 /// <summary>
		 /// Called by the appender that just appended a transaction to the log.
		 /// </summary>
		 /// <param name="logForceEvents"> A trace event for the given log append operation. </param>
		 /// <returns> {@code true} if we got lucky and were the ones forcing the log. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean forceAfterAppend(org.neo4j.kernel.impl.transaction.tracing.LogForceEvents logForceEvents) throws java.io.IOException
		 protected internal virtual bool ForceAfterAppend( LogForceEvents logForceEvents )
		 {
			  // There's a benign race here, where we add our link before we update our next pointer.
			  // This is okay, however, because unparkAll() spins when it sees a null next pointer.
			  ThreadLink threadLink = new ThreadLink( Thread.CurrentThread );
			  threadLink.Next = _threadLinkHead.getAndSet( threadLink );
			  bool attemptedForce = false;

			  using ( LogForceWaitEvent logForceWaitEvent = logForceEvents.BeginLogForceWait() )
			  {
					do
					{
						 if ( _forceLock.tryLock() )
						 {
							  attemptedForce = true;
							  try
							  {
									ForceLog( logForceEvents );
									// In the event of any failure a database panic will be raised and thrown here
							  }
							  finally
							  {
									_forceLock.unlock();

									// We've released the lock, so unpark anyone who might have decided park while we were working.
									// The most recently parked thread is the one most likely to still have warm caches, so that's
									// the one we would prefer to unpark. Luckily, the stack nature of the ThreadLinks makes it easy
									// to get to.
									ThreadLink nextWaiter = _threadLinkHead.get();
									nextWaiter.Unpark();
							  }
						 }
						 else
						 {
							  WaitForLogForce();
						 }
					} while ( !threadLink.Done );

					// If there were many threads committing simultaneously and I wasn't the lucky one
					// actually doing the forcing (where failure would throw panic exception) I need to
					// explicitly check if everything is OK before considering this transaction committed.
					if ( !attemptedForce )
					{
						 _databaseHealth.assertHealthy( typeof( IOException ) );
					}
			  }
			  return attemptedForce;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forceLog(org.neo4j.kernel.impl.transaction.tracing.LogForceEvents logForceEvents) throws java.io.IOException
		 private void ForceLog( LogForceEvents logForceEvents )
		 {
			  ThreadLink links = _threadLinkHead.getAndSet( ThreadLink.End );
			  try
			  {
					  using ( LogForceEvent logForceEvent = logForceEvents.BeginLogForce() )
					  {
						Force();
					  }
			  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Throwable panic)
			  catch ( Exception panic )
			  {
					_databaseHealth.panic( panic );
					throw panic;
			  }
			  finally
			  {
					UnparkAll( links );
			  }
		 }

		 private void UnparkAll( ThreadLink links )
		 {
			  do
			  {
					links.Done = true;
					links.Unpark();
					ThreadLink tmp;
					do
					{
						 // Spin because of the race:y update when consing.
						 tmp = links.Next;
					} while ( tmp == null );
					links = tmp;
			  } while ( links != ThreadLink.End );
		 }

		 private void WaitForLogForce()
		 {
			  long parkTime = TimeUnit.MILLISECONDS.toNanos( 100 );
			  LockSupport.parkNanos( this, parkTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void force() throws java.io.IOException
		 private void Force()
		 {
			  // Empty buffer into writer. We want to synchronize with appenders somehow so that they
			  // don't append while we're doing that. The way rotation is coordinated we can't synchronize
			  // on logFile because it would cause deadlocks. Synchronizing on writer assumes that appenders
			  // also synchronize on writer.
			  Flushable flushable;
			  lock ( _logFile )
			  {
					_databaseHealth.assertHealthy( typeof( IOException ) );
					flushable = _writer.prepareForFlush();
			  }
			  // Force the writer outside of the lock.
			  // This allows other threads access to the buffer while the writer is being forced.
			  try
			  {
					flushable.flush();
			  }
			  catch ( ClosedChannelException )
			  {
					// This is ok, we were already successful in emptying the buffer, so the channel being closed here means
					// that some other thread is rotating the log and has closed the underlying channel. But since we were
					// successful in emptying the buffer *UNDER THE LOCK* we know that the rotating thread included the changes
					// we emptied into the channel, and thus it is already flushed by that thread.
			  }
		 }
	}

}