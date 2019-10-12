using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.checkpoint
{
	using BooleanPredicate = org.eclipse.collections.api.block.predicate.primitive.BooleanPredicate;


	using Resource = Org.Neo4j.Graphdb.Resource;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using LogPruning = Org.Neo4j.Kernel.impl.transaction.log.pruning.LogPruning;
	using CheckPointTracer = Org.Neo4j.Kernel.impl.transaction.tracing.CheckPointTracer;
	using LogCheckPointEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;

	public class CheckPointerImpl : LifecycleAdapter, CheckPointer
	{
		 private readonly TransactionAppender _appender;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly CheckPointThreshold _threshold;
		 private readonly StorageEngine _storageEngine;
		 private readonly LogPruning _logPruning;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly IOLimiter _ioLimiter;
		 private readonly Log _msgLog;
		 private readonly CheckPointTracer _tracer;
		 private readonly StoreCopyCheckPointMutex _mutex;

		 private long _lastCheckPointedTx;

		 public CheckPointerImpl( TransactionIdStore transactionIdStore, CheckPointThreshold threshold, StorageEngine storageEngine, LogPruning logPruning, TransactionAppender appender, DatabaseHealth databaseHealth, LogProvider logProvider, CheckPointTracer tracer, IOLimiter ioLimiter, StoreCopyCheckPointMutex mutex )
		 {
			  this._appender = appender;
			  this._transactionIdStore = transactionIdStore;
			  this._threshold = threshold;
			  this._storageEngine = storageEngine;
			  this._logPruning = logPruning;
			  this._databaseHealth = databaseHealth;
			  this._ioLimiter = ioLimiter;
			  this._msgLog = logProvider.GetLog( typeof( CheckPointerImpl ) );
			  this._tracer = tracer;
			  this._mutex = mutex;
		 }

		 public override void Start()
		 {
			  _threshold.initialize( _transactionIdStore.LastClosedTransactionId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long forceCheckPoint(TriggerInfo info) throws java.io.IOException
		 public override long ForceCheckPoint( TriggerInfo info )
		 {
			  _ioLimiter.disableLimit();
			  try
			  {
					  using ( Resource @lock = _mutex.checkPoint() )
					  {
						return DoCheckPoint( info );
					  }
			  }
			  finally
			  {
					_ioLimiter.enableLimit();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long tryCheckPoint(TriggerInfo info) throws java.io.IOException
		 public override long TryCheckPoint( TriggerInfo info )
		 {
			  return TryCheckPoint( info, () => false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long tryCheckPoint(TriggerInfo info, System.Func<boolean> timeout) throws java.io.IOException
		 public override long TryCheckPoint( TriggerInfo info, System.Func<bool> timeout )
		 {
			  _ioLimiter.disableLimit();
			  try
			  {
					Resource lockAttempt = _mutex.tryCheckPoint();
					if ( lockAttempt != null )
					{
						 using ( Resource @lock = lockAttempt )
						 {
							  return DoCheckPoint( info );
						 }
					}
					else
					{
						 using ( Resource @lock = _mutex.tryCheckPoint( timeout ) )
						 {
							  if ( @lock != null )
							  {
									_msgLog.info( info.Describe( _lastCheckPointedTx ) + " Check pointing was already running, completed now" );
									return _lastCheckPointedTx;
							  }
							  else
							  {
									return -1;
							  }
						 }
					}
			  }
			  finally
			  {
					_ioLimiter.enableLimit();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long checkPointIfNeeded(TriggerInfo info) throws java.io.IOException
		 public override long CheckPointIfNeeded( TriggerInfo info )
		 {
			  if ( _threshold.isCheckPointingNeeded( _transactionIdStore.LastClosedTransactionId, info ) )
			  {
					using ( Resource @lock = _mutex.checkPoint() )
					{
						 return DoCheckPoint( info );
					}
			  }
			  return -1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long doCheckPoint(TriggerInfo triggerInfo) throws java.io.IOException
		 private long DoCheckPoint( TriggerInfo triggerInfo )
		 {
			  try
			  {
					  using ( LogCheckPointEvent @event = _tracer.beginCheckPoint() )
					  {
						long[] lastClosedTransaction = _transactionIdStore.LastClosedTransaction;
						long lastClosedTransactionId = lastClosedTransaction[0];
						LogPosition logPosition = new LogPosition( lastClosedTransaction[1], lastClosedTransaction[2] );
						string prefix = triggerInfo.Describe( lastClosedTransactionId );
						/*
						 * Check kernel health before going into waiting for transactions to be closed, to avoid
						 * getting into a scenario where we would await a condition that would potentially never
						 * happen.
						 */
						_databaseHealth.assertHealthy( typeof( IOException ) );
						/*
						 * First we flush the store. If we fail now or during the flush, on recovery we'll find the
						 * earlier check point and replay from there all the log entries. Everything will be ok.
						 */
						_msgLog.info( prefix + " checkpoint started..." );
						long startTime = currentTimeMillis();
						_storageEngine.flushAndForce( _ioLimiter );
						/*
						 * Check kernel health before going to write the next check point.  In case of a panic this check point
						 * will be aborted, which is the safest alternative so that the next recovery will have a chance to
						 * repair the damages.
						 */
						_databaseHealth.assertHealthy( typeof( IOException ) );
						_appender.checkPoint( logPosition, @event );
						_threshold.checkPointHappened( lastClosedTransactionId );
						_msgLog.info( prefix + " checkpoint completed in " + duration( currentTimeMillis() - startTime ) );
						/*
						 * Prune up to the version pointed from the latest check point,
						 * since it might be an earlier version than the current log version.
						 */
						_logPruning.pruneLogs( logPosition.LogVersion );
						_lastCheckPointedTx = lastClosedTransactionId;
						return lastClosedTransactionId;
					  }
			  }
			  catch ( Exception t )
			  {
					// Why only log failure here? It's because check point can potentially be made from various
					// points of execution e.g. background thread triggering check point if needed and during
					// shutdown where it's better to have more control over failure handling.
					_msgLog.error( "Checkpoint failed", t );
					throw t;
			  }
		 }

		 public override long LastCheckPointedTransactionId()
		 {
			  return _lastCheckPointedTx;
		 }
	}

}