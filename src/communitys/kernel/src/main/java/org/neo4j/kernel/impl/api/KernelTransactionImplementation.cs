using System;
using System.Collections.Generic;
using System.Diagnostics;
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
namespace Neo4Net.Kernel.Impl.Api
{

	using Neo4Net.Collections.Pooling;
	using NotInTransactionException = Neo4Net.Graphdb.NotInTransactionException;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using CursorFactory = Neo4Net.@internal.Kernel.Api.CursorFactory;
	using ExecutionStatistics = Neo4Net.@internal.Kernel.Api.ExecutionStatistics;
	using ExplicitIndexRead = Neo4Net.@internal.Kernel.Api.ExplicitIndexRead;
	using ExplicitIndexWrite = Neo4Net.@internal.Kernel.Api.ExplicitIndexWrite;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Neo4Net.@internal.Kernel.Api.RelationshipScanCursor;
	using SchemaRead = Neo4Net.@internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using Token = Neo4Net.@internal.Kernel.Api.Token;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using InvalidTransactionTypeKernelException = Neo4Net.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ConstraintValidationException = Neo4Net.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using ConstraintViolationTransactionFailureException = Neo4Net.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using AuxiliaryTransactionStateCloseException = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateCloseException;
	using AuxiliaryTransactionStateHolder = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateHolder;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using AllStoreHolder = Neo4Net.Kernel.Impl.Newapi.AllStoreHolder;
	using DefaultCursors = Neo4Net.Kernel.Impl.Newapi.DefaultCursors;
	using IndexTxStateUpdater = Neo4Net.Kernel.Impl.Newapi.IndexTxStateUpdater;
	using KernelToken = Neo4Net.Kernel.Impl.Newapi.KernelToken;
	using Operations = Neo4Net.Kernel.Impl.Newapi.Operations;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionEvent = Neo4Net.Kernel.impl.transaction.tracing.TransactionEvent;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using CollectionsFactory = Neo4Net.Kernel.impl.util.collection.CollectionsFactory;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.INTERNAL;

	/// <summary>
	/// This class should replace the <seealso cref="org.neo4j.kernel.api.KernelTransaction"/> interface, and take its name, as soon
	/// as
	/// {@code TransitionalTxManagementKernelTransaction} is gone from {@code server}.
	/// </summary>
	public class KernelTransactionImplementation : KernelTransaction, TxStateHolder, ExecutionStatistics
	{
		 /*
		  * IMPORTANT:
		  * This class is pooled and re-used. If you add *any* state to it, you *must* make sure that:
		  *   - the #initialize() method resets that state for re-use
		  *   - the #release() method releases resources acquired in #initialize() or during the transaction's life time
		  */

		 // default values for not committed tx id and tx commit time
		 private const long NOT_COMMITTED_TRANSACTION_ID = -1;
		 private const long NOT_COMMITTED_TRANSACTION_COMMIT_TIME = -1;

		 private readonly CollectionsFactory _collectionsFactory;

		 // Logic
		 private readonly SchemaWriteGuard _schemaWriteGuard;
		 private readonly TransactionHooks _hooks;
		 private readonly ConstraintIndexCreator _constraintIndexCreator;
		 private readonly StorageEngine _storageEngine;
		 private readonly TransactionTracer _transactionTracer;
		 private readonly Pool<KernelTransactionImplementation> _pool;
		 private readonly AuxiliaryTransactionStateManager _auxTxStateManager;

		 // For committing
		 private readonly TransactionHeaderInformationFactory _headerInformationFactory;
		 private readonly TransactionCommitProcess _commitProcess;
		 private readonly TransactionMonitor _transactionMonitor;
		 private readonly PageCursorTracerSupplier _cursorTracerSupplier;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly StorageReader _storageReader;
		 private readonly ClockContext _clocks;
		 private readonly AccessCapability _accessCapability;
		 private readonly ConstraintSemantics _constraintSemantics;

		 // State that needs to be reset between uses. Most of these should be cleared or released in #release(),
		 // whereas others, such as timestamp or txId when transaction starts, even locks, needs to be set in #initialize().
		 private TxState _txState;
		 private AuxiliaryTransactionStateHolder _auxTxStateHolder;
		 private volatile TransactionWriteState _writeState;
		 private TransactionHooks.TransactionHooksState _hooksState;
		 private readonly KernelStatement _currentStatement;
		 private readonly IList<Neo4Net.Kernel.api.KernelTransaction_CloseListener> _closeListeners = new List<Neo4Net.Kernel.api.KernelTransaction_CloseListener>( 2 );
		 private SecurityContext _securityContext;
		 private volatile StatementLocks _statementLocks;
		 private volatile long _userTransactionId;
		 private bool _beforeHookInvoked;
		 private volatile bool _closing;
		 private volatile bool _closed;
		 private bool _failure;
		 private bool _success;
		 private volatile Status _terminationReason;
		 private long _startTimeMillis;
		 private long _startTimeNanos;
		 private long _timeoutMillis;
		 private long _lastTransactionIdWhenStarted;
		 private volatile long _lastTransactionTimestampWhenStarted;
		 private readonly Statistics _statistics;
		 private TransactionEvent _transactionEvent;
		 private Neo4Net.@internal.Kernel.Api.Transaction_Type _type;
		 private long _transactionId;
		 private long _commitTime;
		 private volatile int _reuseCount;
		 private volatile IDictionary<string, object> _userMetaData;
		 private readonly Operations _operations;

		 /// <summary>
		 /// Lock prevents transaction <seealso cref="markForTermination(Status)"/>  transaction termination} from interfering with
		 /// <seealso cref="close() transaction commit"/> and specifically with <seealso cref="release()"/>.
		 /// Termination can run concurrently with commit and we need to make sure that it terminates the right lock client
		 /// and the right transaction (with the right <seealso cref="reuseCount"/>) because <seealso cref="KernelTransactionImplementation"/>
		 /// instances are pooled.
		 /// </summary>
		 private readonly Lock _terminationReleaseLock = new ReentrantLock();

		 public KernelTransactionImplementation( Config config, StatementOperationParts statementOperations, SchemaWriteGuard schemaWriteGuard, TransactionHooks hooks, ConstraintIndexCreator constraintIndexCreator, Procedures procedures, TransactionHeaderInformationFactory headerInformationFactory, TransactionCommitProcess commitProcess, TransactionMonitor transactionMonitor, AuxiliaryTransactionStateManager auxTxStateManager, Pool<KernelTransactionImplementation> pool, SystemNanoClock clock, AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef, TransactionTracer transactionTracer, LockTracer lockTracer, PageCursorTracerSupplier cursorTracerSupplier, StorageEngine storageEngine, AccessCapability accessCapability, AutoIndexing autoIndexing, ExplicitIndexStore explicitIndexStore, VersionContextSupplier versionContextSupplier, CollectionsFactorySupplier collectionsFactorySupplier, ConstraintSemantics constraintSemantics, SchemaState schemaState, IndexingService indexingService, TokenHolders tokenHolders, Dependencies dataSourceDependencies )
		 {
			  this._schemaWriteGuard = schemaWriteGuard;
			  this._hooks = hooks;
			  this._constraintIndexCreator = constraintIndexCreator;
			  this._headerInformationFactory = headerInformationFactory;
			  this._commitProcess = commitProcess;
			  this._transactionMonitor = transactionMonitor;
			  this._storageReader = storageEngine.NewReader();
			  this._storageEngine = storageEngine;
			  this._auxTxStateManager = auxTxStateManager;
			  this._pool = pool;
			  this._clocks = new ClockContext( clock );
			  this._transactionTracer = transactionTracer;
			  this._cursorTracerSupplier = cursorTracerSupplier;
			  this._versionContextSupplier = versionContextSupplier;
			  this._currentStatement = new KernelStatement( this, this, _storageReader, lockTracer, statementOperations, this._clocks, versionContextSupplier );
			  this._accessCapability = accessCapability;
			  this._statistics = new Statistics( this, cpuClockRef, heapAllocationRef );
			  this._userMetaData = emptyMap();
			  this._constraintSemantics = constraintSemantics;
			  DefaultCursors cursors = new DefaultCursors( _storageReader );
			  AllStoreHolder allStoreHolder = new AllStoreHolder( _storageReader, this, cursors, explicitIndexStore, procedures, schemaState, dataSourceDependencies );
			  this._operations = new Operations( allStoreHolder, new IndexTxStateUpdater( allStoreHolder, indexingService ), _storageReader, this, new KernelToken( _storageReader, this, tokenHolders ), cursors, autoIndexing, constraintIndexCreator, constraintSemantics, indexingService, config );
			  this._collectionsFactory = collectionsFactorySupplier();
		 }

		 /// <summary>
		 /// Reset this transaction to a vanilla state, turning it into a logically new transaction.
		 /// </summary>
		 public virtual KernelTransactionImplementation Initialize( long lastCommittedTx, long lastTimeStamp, StatementLocks statementLocks, Neo4Net.@internal.Kernel.Api.Transaction_Type type, SecurityContext frozenSecurityContext, long transactionTimeout, long userTransactionId )
		 {
			  this._type = type;
			  this._statementLocks = statementLocks;
			  this._userTransactionId = userTransactionId;
			  this._terminationReason = null;
			  this._closing = false;
			  this._closed = false;
			  this._beforeHookInvoked = false;
			  this._failure = false;
			  this._success = false;
			  this._writeState = TransactionWriteState.None;
			  this._startTimeMillis = _clocks.systemClock().millis();
			  this._startTimeNanos = _clocks.systemClock().nanos();
			  this._timeoutMillis = transactionTimeout;
			  this._lastTransactionIdWhenStarted = lastCommittedTx;
			  this._lastTransactionTimestampWhenStarted = lastTimeStamp;
			  this._transactionEvent = _transactionTracer.beginTransaction();
			  Debug.Assert( _transactionEvent != null, "transactionEvent was null!" );
			  this._securityContext = frozenSecurityContext;
			  this._transactionId = NOT_COMMITTED_TRANSACTION_ID;
			  this._commitTime = NOT_COMMITTED_TRANSACTION_COMMIT_TIME;
			  PageCursorTracer pageCursorTracer = _cursorTracerSupplier.get();
			  this._statistics.init( Thread.CurrentThread.Id, pageCursorTracer );
			  this._currentStatement.initialize( statementLocks, pageCursorTracer );
			  this._operations.initialize();
			  return this;
		 }

		 internal virtual int ReuseCount
		 {
			 get
			 {
				  return _reuseCount;
			 }
		 }

		 public override long StartTime()
		 {
			  return _startTimeMillis;
		 }

		 public override long StartTimeNanos()
		 {
			  return _startTimeNanos;
		 }

		 public override long Timeout()
		 {
			  return _timeoutMillis;
		 }

		 public override long LastTransactionIdWhenStarted()
		 {
			  return _lastTransactionIdWhenStarted;
		 }

		 public override void Success()
		 {
			  this._success = true;
		 }

		 internal virtual bool Success
		 {
			 get
			 {
				  return _success;
			 }
		 }

		 public override void Failure()
		 {
			  _failure = true;
		 }

		 public virtual Optional<Status> ReasonIfTerminated
		 {
			 get
			 {
				  return Optional.ofNullable( _terminationReason );
			 }
		 }

		 internal virtual bool MarkForTermination( long expectedReuseCount, Status reason )
		 {
			  _terminationReleaseLock.@lock();
			  try
			  {
					return expectedReuseCount == _reuseCount && MarkForTerminationIfPossible( reason );
			  }
			  finally
			  {
					_terminationReleaseLock.unlock();
			  }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// <para>
		 /// This method is guarded by <seealso cref="terminationReleaseLock"/> to coordinate concurrent
		 /// <seealso cref="close()"/> and <seealso cref="release()"/> calls.
		 /// </para>
		 /// </summary>
		 public override void MarkForTermination( Status reason )
		 {
			  _terminationReleaseLock.@lock();
			  try
			  {
					MarkForTerminationIfPossible( reason );
			  }
			  finally
			  {
					_terminationReleaseLock.unlock();
			  }
		 }

		 public virtual bool SchemaTransaction
		 {
			 get
			 {
				  return _writeState == TransactionWriteState.Schema;
			 }
		 }

		 private bool MarkForTerminationIfPossible( Status reason )
		 {
			  if ( CanBeTerminated() )
			  {
					_failure = true;
					_terminationReason = reason;
					if ( _statementLocks != null )
					{
						 _statementLocks.stop();
					}
					_transactionMonitor.transactionTerminated( HasTxStateWithChanges() );
					return true;
			  }
			  return false;
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return !_closed && !_closing;
			 }
		 }

		 public override SecurityContext SecurityContext()
		 {
			  if ( _securityContext == null )
			  {
					throw new NotInTransactionException();
			  }
			  return _securityContext;
		 }

		 public override AuthSubject SubjectOrAnonymous()
		 {
			  SecurityContext context = this._securityContext;
			  return context == null ? AuthSubject.ANONYMOUS : context.Subject();
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 set
			 {
				  this._userMetaData = value;
			 }
			 get
			 {
				  return _userMetaData;
			 }
		 }


		 public override KernelStatement AcquireStatement()
		 {
			  AssertTransactionOpen();
			  _currentStatement.acquire();
			  return _currentStatement;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexDescriptor indexUniqueCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor schema, String provider) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override IndexDescriptor IndexUniqueCreate( SchemaDescriptor schema, string provider )
		 {
			  return _operations.indexUniqueCreate( schema, provider );
		 }

		 public override long PageHits()
		 {
			  return _cursorTracerSupplier.get().hits();
		 }

		 public override long PageFaults()
		 {
			  return _cursorTracerSupplier.get().faults();
		 }

		 internal virtual ExecutingQueryList ExecutingQueries()
		 {
			  return _currentStatement.executingQueryList();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void upgradeToDataWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 internal virtual void UpgradeToDataWrites()
		 {
			  _writeState = _writeState.upgradeToDataWrites();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void upgradeToSchemaWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 internal virtual void UpgradeToSchemaWrites()
		 {
			  _schemaWriteGuard.assertSchemaWritesAllowed();
			  _writeState = _writeState.upgradeToSchemaWrites();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropCreatedConstraintIndexes() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void DropCreatedConstraintIndexes()
		 {
			  if ( HasTxStateWithChanges() )
			  {
					foreach ( IndexDescriptor createdConstraintIndex in TxState().constraintIndexesCreatedInTx() )
					{
						 // TODO logically, which statement should this operation be performed on?
						 _constraintIndexCreator.dropUniquenessConstraintIndex( createdConstraintIndex );
					}
			  }
		 }

		 public override TransactionState TxState()
		 {
			  if ( _txState == null )
			  {
					_transactionMonitor.upgradeToWriteTransaction();
					_txState = new TxState( _collectionsFactory );
			  }
			  return _txState;
		 }

		 private AuxiliaryTransactionStateHolder AuxTxStateHolder
		 {
			 get
			 {
				  if ( _auxTxStateHolder == null )
				  {
						_auxTxStateHolder = _auxTxStateManager.openStateHolder();
				  }
				  return _auxTxStateHolder;
			 }
		 }

		 public override AuxiliaryTransactionState AuxiliaryTxState( object providerIdentityKey )
		 {
			  return AuxTxStateHolder.getState( providerIdentityKey );
		 }

		 public override ExplicitIndexTransactionState ExplicitIndexTxState()
		 {
			  return ( ExplicitIndexTransactionState ) AuxTxStateHolder.getState( ExplicitIndexTransactionStateProvider.PROVIDER_KEY );
		 }

		 public override bool HasTxStateWithChanges()
		 {
			  return _txState != null && _txState.hasChanges();
		 }

		 private void MarkAsClosed( long txId )
		 {
			  AssertTransactionOpen();
			  _closed = true;
			  NotifyListeners( txId );
			  CloseCurrentStatementIfAny();
		 }

		 private void NotifyListeners( long txId )
		 {
			  foreach ( Neo4Net.Kernel.api.KernelTransaction_CloseListener closeListener in _closeListeners )
			  {
					closeListener.Notify( txId );
			  }
		 }

		 private void CloseCurrentStatementIfAny()
		 {
			  _currentStatement.forceClose();
		 }

		 private void AssertTransactionNotClosing()
		 {
			  if ( _closing )
			  {
					throw new System.InvalidOperationException( "This transaction is already being closed." );
			  }
		 }

		 private void AssertTransactionOpen()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "This transaction has already been completed." );
			  }
		 }

		 public override void AssertOpen()
		 {
			  Status reason = this._terminationReason;
			  if ( reason != null )
			  {
					throw new TransactionTerminatedException( reason );
			  }
			  if ( _closed )
			  {
					throw new NotInTransactionException( "The transaction has been closed." );
			  }
		 }

		 private bool HasChanges()
		 {
			  return HasTxStateWithChanges() || HasAuxTxStateChanges();
		 }

		 private bool HasAuxTxStateChanges()
		 {
			  return _auxTxStateHolder != null && AuxTxStateHolder.hasChanges();
		 }

		 private bool HasDataChanges()
		 {
			  return HasTxStateWithChanges() && _txState.hasDataChanges();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long closeTransaction() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override long CloseTransaction()
		 {
			  AssertTransactionOpen();
			  AssertTransactionNotClosing();
			  _closing = true;
			  try
			  {
					if ( _failure || !_success || Terminated )
					{
						 Rollback();
						 FailOnNonExplicitRollbackIfNeeded();
						 return Neo4Net.@internal.Kernel.Api.Transaction_Fields.ROLLBACK;
					}
					else
					{
						 return Commit();
					}
			  }
			  finally
			  {
					try
					{
						 _closed = true;
						 _closing = false;
						 _transactionEvent.Success = _success;
						 _transactionEvent.Failure = _failure;
						 _transactionEvent.TransactionWriteState = _writeState.name();
						 _transactionEvent.ReadOnly = _txState == null || !_txState.hasChanges();
						 _transactionEvent.close();
					}
					finally
					{
						 Release();
					}
			  }
		 }

		 public virtual bool Closing
		 {
			 get
			 {
				  return _closing;
			 }
		 }

		 /// <summary>
		 /// Throws exception if this transaction was marked as successful but failure flag has also been set to true.
		 /// <para>
		 /// This could happen when:
		 /// <ul>
		 /// <li>caller explicitly calls both <seealso cref="success()"/> and <seealso cref="failure()"/></li>
		 /// <li>caller explicitly calls <seealso cref="success()"/> but transaction execution fails</li>
		 /// <li>caller explicitly calls <seealso cref="success()"/> but transaction is terminated</li>
		 /// </ul>
		 /// </para>
		 /// <para>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="TransactionFailureException"> when execution failed </exception>
		 /// <exception cref="TransactionTerminatedException"> when transaction was terminated </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void failOnNonExplicitRollbackIfNeeded() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void FailOnNonExplicitRollbackIfNeeded()
		 {
			  if ( _success && Terminated )
			  {
					throw new TransactionTerminatedException( _terminationReason );
			  }
			  if ( _success )
			  {
					// Success was called, but also failure which means that the client code using this
					// transaction passed through a happy path, but the transaction was still marked as
					// failed for one or more reasons. Tell the user that although it looked happy it
					// wasn't committed, but was instead rolled back.
					throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionMarkedAsFailed, "Transaction rolled back even if marked as successful" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long commit() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private long Commit()
		 {
			  bool success = false;
			  long txId = Neo4Net.@internal.Kernel.Api.Transaction_Fields.READ_ONLY;

			  try
			  {
					  using ( CommitEvent commitEvent = _transactionEvent.beginCommitEvent() )
					  {
						// Trigger transaction "before" hooks.
						if ( HasDataChanges() )
						{
							 try
							 {
								  _hooksState = _hooks.beforeCommit( _txState, this, _storageReader );
								  if ( _hooksState != null && _hooksState.failed() )
								  {
										Exception cause = _hooksState.failure();
										throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionHookFailed, cause, "" );
								  }
							 }
							 finally
							 {
								  _beforeHookInvoked = true;
							 }
						}
      
						// Convert changes into commands and commit
						if ( HasChanges() )
						{
							 // grab all optimistic locks now, locks can't be deferred any further
							 _statementLocks.prepareForCommit( _currentStatement.lockTracer() );
							 // use pessimistic locks for the rest of the commit process, locks can't be deferred any further
							 Neo4Net.Kernel.impl.locking.Locks_Client commitLocks = _statementLocks.pessimistic();
      
							 // Gather up commands from the various sources
							 ICollection<StorageCommand> extractedCommands = new List<StorageCommand>();
							 _storageEngine.createCommands( extractedCommands, _txState, _storageReader, commitLocks, _lastTransactionIdWhenStarted, this.enforceConstraints );
							 if ( HasAuxTxStateChanges() )
							 {
								  _auxTxStateHolder.extractCommands( extractedCommands );
							 }
      
							 /* Here's the deal: we track a quick-to-access hasChanges in transaction state which is true
							  * if there are any changes imposed by this transaction. Some changes made inside a transaction undo
							  * previously made changes in that same transaction, and so at some point a transaction may have
							  * changes and at another point, after more changes seemingly,
							  * the transaction may not have any changes.
							  * However, to track that "undoing" of the changes is a bit tedious, intrusive and hard to maintain
							  * and get right.... So to really make sure the transaction has changes we re-check by looking if we
							  * have produced any commands to add to the logical log.
							  */
							 if ( extractedCommands.Count > 0 )
							 {
								  // Finish up the whole transaction representation
								  PhysicalTransactionRepresentation transactionRepresentation = new PhysicalTransactionRepresentation( extractedCommands );
								  TransactionHeaderInformation headerInformation = _headerInformationFactory.create();
								  long timeCommitted = _clocks.systemClock().millis();
								  transactionRepresentation.SetHeader( headerInformation.AdditionalHeader, headerInformation.MasterId, headerInformation.AuthorId, _startTimeMillis, _lastTransactionIdWhenStarted, timeCommitted, commitLocks.LockSessionId );
      
								  // Commit the transaction
								  success = true;
								  TransactionToApply batch = new TransactionToApply( transactionRepresentation, _versionContextSupplier.VersionContext );
								  txId = _transactionId = _commitProcess.commit( batch, commitEvent, INTERNAL );
								  _commitTime = timeCommitted;
							 }
						}
						success = true;
						return txId;
					  }
			  }
			  catch ( Exception e ) when ( e is ConstraintValidationException || e is CreateConstraintFailureException )
			  {
					throw new ConstraintViolationTransactionFailureException( e.getUserMessage( new SilentTokenNameLookup( TokenRead() ) ), e );
			  }
			  finally
			  {
					if ( !success )
					{
						 Rollback();
					}
					else
					{
						 AfterCommit( txId );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rollback() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void Rollback()
		 {
			  try
			  {
					try
					{
						 DropCreatedConstraintIndexes();
					}
					catch ( Exception e ) when ( e is System.InvalidOperationException || e is SecurityException )
					{
						 throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionRollbackFailed, e, "Could not drop created constraint indexes" );
					}

					// Free any acquired id's
					if ( _txState != null )
					{
						 try
						 {
							  _txState.accept( new TxStateVisitor_AdapterAnonymousInnerClass( this ) );
						 }
						 catch ( Exception e ) when ( e is ConstraintValidationException || e is CreateConstraintFailureException )
						 {
							  throw new System.InvalidOperationException( "Releasing locks during rollback should perform no constraints checking.", e );
						 }
					}
			  }
			  finally
			  {
					AfterRollback();
			  }
		 }

		 private class TxStateVisitor_AdapterAnonymousInnerClass : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly KernelTransactionImplementation _outerInstance;

			 public TxStateVisitor_AdapterAnonymousInnerClass( KernelTransactionImplementation outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void visitCreatedNode( long id )
			 {
				  _outerInstance.storageReader.releaseNode( id );
			 }

			 public override void visitCreatedRelationship( long id, int type, long startNode, long endNode )
			 {
				  _outerInstance.storageReader.releaseRelationship( id );
			 }
		 }

		 public override Read DataRead()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsReads, "Read" );
			  return _operations.dataRead();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.Write dataWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 public override Write DataWrite()
		 {
			  _accessCapability.assertCanWrite();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsWrites, "Write" );
			  UpgradeToDataWrites();
			  return _operations;
		 }

		 public override TokenWrite TokenWrite()
		 {
			  _accessCapability.assertCanWrite();
			  return _operations.token();
		 }

		 public override Token Token()
		 {
			  _accessCapability.assertCanWrite();
			  return _operations.token();
		 }

		 public override TokenRead TokenRead()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsReads, "Read" );
			  return _operations.token();
		 }

		 public override ExplicitIndexRead IndexRead()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsReads, "Read" );

			  return _operations.indexRead();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.ExplicitIndexWrite indexWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 public override ExplicitIndexWrite IndexWrite()
		 {
			  _accessCapability.assertCanWrite();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsWrites, "Write" );
			  UpgradeToDataWrites();

			  return _operations;
		 }

		 public override SchemaRead SchemaRead()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsReads, "Read" );
			  return _operations.schemaRead();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.SchemaWrite schemaWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 public override SchemaWrite SchemaWrite()
		 {
			  _accessCapability.assertCanWrite();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  AssertAllows( AccessMode::allowsSchemaWrites, "Schema" );

			  UpgradeToSchemaWrites();
			  return _operations;
		 }

		 public override Neo4Net.@internal.Kernel.Api.Locks Locks()
		 {
			 return _operations.locks();
		 }

		 public virtual StatementLocks StatementLocks()
		 {
			  AssertOpen();
			  return _statementLocks;
		 }

		 public override CursorFactory Cursors()
		 {
			  return _operations.cursors();
		 }

		 public override Neo4Net.@internal.Kernel.Api.Procedures Procedures()
		 {
			  return _operations.procedures();
		 }

		 public override ExecutionStatistics ExecutionStatistics()
		 {
			  return this;
		 }

		 public virtual LockTracer LockTracer()
		 {
			  return _currentStatement.lockTracer();
		 }

		 public virtual void AssertAllows( System.Func<AccessMode, bool> allows, string mode )
		 {
			  AccessMode accessMode = SecurityContext().mode();
			  if ( !allows( accessMode ) )
			  {
					throw accessMode.OnViolation( format( "%s operations are not allowed for %s.", mode, SecurityContext().description() ) );
			  }
		 }

		 private void AfterCommit( long txId )
		 {
			  try
			  {
					MarkAsClosed( txId );
					if ( _beforeHookInvoked )
					{
						 _hooks.afterCommit( _txState, this, _hooksState );
					}
			  }
			  finally
			  {
					_transactionMonitor.transactionFinished( true, HasTxStateWithChanges() );
			  }
		 }

		 private void AfterRollback()
		 {
			  try
			  {
					MarkAsClosed( Neo4Net.@internal.Kernel.Api.Transaction_Fields.ROLLBACK );
					if ( _beforeHookInvoked )
					{
						 _hooks.afterRollback( _txState, this, _hooksState );
					}
			  }
			  finally
			  {
					_transactionMonitor.transactionFinished( false, HasTxStateWithChanges() );
			  }
		 }

		 /// <summary>
		 /// Release resources held up by this transaction & return it to the transaction pool.
		 /// This method is guarded by <seealso cref="terminationReleaseLock"/> to coordinate concurrent
		 /// <seealso cref="markForTermination(Status)"/> calls.
		 /// </summary>
		 private void Release()
		 {
			  AuxiliaryTransactionStateCloseException auxStateCloseException = null;
			  _terminationReleaseLock.@lock();
			  try
			  {
					_statementLocks.close();
					_statementLocks = null;
					_terminationReason = null;
					_type = null;
					_securityContext = null;
					_transactionEvent = null;
					if ( _auxTxStateHolder != null )
					{
						 auxStateCloseException = CloseAuxTxState();
					}
					_txState = null;
					_collectionsFactory.release();
					_hooksState = null;
					_closeListeners.Clear();
					_reuseCount++;
					_userMetaData = emptyMap();
					_userTransactionId = 0;
					_statistics.reset();
					_operations.release();
					_pool.release( this );
			  }
			  finally
			  {
					_terminationReleaseLock.unlock();
			  }
			  if ( auxStateCloseException != null )
			  {
					throw auxStateCloseException;
			  }
		 }

		 private AuxiliaryTransactionStateCloseException CloseAuxTxState()
		 {
			  AuxiliaryTransactionStateHolder holder = _auxTxStateHolder;
			  _auxTxStateHolder = null;
			  try
			  {
					holder.Close();
			  }
			  catch ( AuxiliaryTransactionStateCloseException e )
			  {
					return e;
			  }
			  return null;
		 }

		 /// <summary>
		 /// Transaction can be terminated only when it is not closed and not already terminated.
		 /// Otherwise termination does not make sense.
		 /// </summary>
		 private bool CanBeTerminated()
		 {
			  return !_closed && !Terminated;
		 }

		 public virtual bool Terminated
		 {
			 get
			 {
				  return _terminationReason != null;
			 }
		 }

		 public override long LastTransactionTimestampWhenStarted()
		 {
			  return _lastTransactionTimestampWhenStarted;
		 }

		 public override void RegisterCloseListener( Neo4Net.Kernel.api.KernelTransaction_CloseListener listener )
		 {
			  Debug.Assert( listener != null );
			  _closeListeners.Add( listener );
		 }

		 public override Neo4Net.@internal.Kernel.Api.Transaction_Type TransactionType()
		 {
			  return _type;
		 }

		 public virtual long TransactionId
		 {
			 get
			 {
				  if ( _transactionId == NOT_COMMITTED_TRANSACTION_ID )
				  {
						throw new System.InvalidOperationException( "Transaction id is not assigned yet. " + "It will be assigned during transaction commit." );
				  }
				  return _transactionId;
			 }
		 }

		 public virtual long CommitTime
		 {
			 get
			 {
				  if ( _commitTime == NOT_COMMITTED_TRANSACTION_COMMIT_TIME )
				  {
						throw new System.InvalidOperationException( "Transaction commit time is not assigned yet. " + "It will be assigned during transaction commit." );
				  }
				  return _commitTime;
			 }
		 }

		 public override Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
		 {
			  SecurityContext oldContext = this._securityContext;
			  this._securityContext = context;
			  return () => this._securityContext = oldContext;
		 }

		 public override string ToString()
		 {
			  string lockSessionId = _statementLocks == null ? "statementLocks == null" : ( _statementLocks.pessimistic().LockSessionId ).ToString();

			  return "KernelTransaction[" + lockSessionId + "]";
		 }

		 public virtual void Dispose()
		 {
			  _storageReader.close();
		 }

		 /// <summary>
		 /// This method will be invoked by concurrent threads for inspecting the locks held by this transaction.
		 /// <para>
		 /// The fact that <seealso cref="statementLocks"/> is a volatile fields, grants us enough of a read barrier to get a good
		 /// enough snapshot of the lock state (as long as the underlying methods give us such guarantees).
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the locks held by this transaction. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks()
		 public virtual Stream<ActiveLock> ActiveLocks()
		 {
			  StatementLocks locks = this._statementLocks;
			  return locks == null ? Stream.empty() : locks.ActiveLocks();
		 }

		 internal virtual long UserTransactionId()
		 {
			  return _userTransactionId;
		 }

		 public virtual Statistics GetStatistics()
		 {
			  return _statistics;
		 }

		 private TxStateVisitor EnforceConstraints( TxStateVisitor txStateVisitor )
		 {
			  return _constraintSemantics.decorateTxStateVisitor( _storageReader, _operations.dataRead(), _operations.cursors(), _txState, txStateVisitor );
		 }

		 /// <summary>
		 /// The revision of the data changes in this transaction. This number is opaque, except that it is zero if there have been no data changes in this
		 /// transaction. And making and then undoing a change does not count as "no data changes." This number will always change when there is a data change in a
		 /// transaction, however, such that one can reliably tell whether or not there has been any data changes in a transaction since last time the transaction
		 /// data revision was obtained for the given transaction. </summary>
		 /// <returns> The opaque data revision for this transaction, or zero if there has been no data changes in this transaction. </returns>
		 public virtual long TransactionDataRevision
		 {
			 get
			 {
				  return HasDataChanges() ? _txState.DataRevision : 0;
			 }
		 }

		 public class Statistics
		 {
			  internal volatile long CpuTimeNanosWhenQueryStarted;
			  internal volatile long HeapAllocatedBytesWhenQueryStarted;
			  internal volatile long WaitingTimeNanos;
			  internal volatile long TransactionThreadId;
			  internal volatile PageCursorTracer PageCursorTracer = PageCursorTracer.NULL;
			  internal readonly KernelTransactionImplementation Transaction;
			  internal readonly AtomicReference<CpuClock> CpuClockRef;
			  internal readonly AtomicReference<HeapAllocation> HeapAllocationRef;
			  internal CpuClock CpuClock;
			  internal HeapAllocation HeapAllocation;

			  public Statistics( KernelTransactionImplementation transaction, AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef )
			  {
					this.Transaction = transaction;
					this.CpuClockRef = cpuClockRef;
					this.HeapAllocationRef = heapAllocationRef;
			  }

			  protected internal virtual void Init( long threadId, PageCursorTracer pageCursorTracer )
			  {
					this.CpuClock = CpuClockRef.get();
					this.HeapAllocation = HeapAllocationRef.get();
					this.TransactionThreadId = threadId;
					this.PageCursorTracer = pageCursorTracer;
					this.CpuTimeNanosWhenQueryStarted = CpuClock.cpuTimeNanos( TransactionThreadId );
					this.HeapAllocatedBytesWhenQueryStarted = HeapAllocation.allocatedBytes( TransactionThreadId );
			  }

			  /// <summary>
			  /// Returns number of allocated bytes by current transaction. </summary>
			  /// <returns> number of allocated bytes by the thread. </returns>
			  internal virtual long HeapAllocatedBytes()
			  {
					return HeapAllocation.allocatedBytes( TransactionThreadId ) - HeapAllocatedBytesWhenQueryStarted;
			  }

			  /// <summary>
			  /// Returns amount of direct memory allocated by current transaction.
			  /// </summary>
			  /// <returns> amount of direct memory allocated by the thread in bytes. </returns>
			  internal virtual long DirectAllocatedBytes()
			  {
					return Transaction.collectionsFactory.MemoryTracker.usedDirectMemory();
			  }

			  /// <summary>
			  /// Return CPU time used by current transaction in milliseconds </summary>
			  /// <returns> the current CPU time used by the transaction, in milliseconds. </returns>
			  public virtual long CpuTimeMillis()
			  {
					long cpuTimeNanos = CpuClock.cpuTimeNanos( TransactionThreadId ) - CpuTimeNanosWhenQueryStarted;
					return NANOSECONDS.toMillis( cpuTimeNanos );
			  }

			  /// <summary>
			  /// Return total number of page cache hits that current transaction performed </summary>
			  /// <returns> total page cache hits </returns>
			  internal virtual long TotalTransactionPageCacheHits()
			  {
					return PageCursorTracer.accumulatedHits();
			  }

			  /// <summary>
			  /// Return total number of page cache faults that current transaction performed </summary>
			  /// <returns> total page cache faults </returns>
			  internal virtual long TotalTransactionPageCacheFaults()
			  {
					return PageCursorTracer.accumulatedFaults();
			  }

			  /// <summary>
			  /// Report how long any particular query was waiting during it's execution </summary>
			  /// <param name="waitTimeNanos"> query waiting time in nanoseconds </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("NonAtomicOperationOnVolatileField") void addWaitingTime(long waitTimeNanos)
			  internal virtual void AddWaitingTime( long waitTimeNanos )
			  {
					WaitingTimeNanos += waitTimeNanos;
			  }

			  /// <summary>
			  /// Accumulated transaction waiting time that includes waiting time of all already executed queries
			  /// plus waiting time of currently executed query. </summary>
			  /// <returns> accumulated transaction waiting time </returns>
			  /// <param name="nowNanos"> current moment in nanoseconds </param>
			  internal virtual long GetWaitingTimeNanos( long nowNanos )
			  {
					ExecutingQueryList queryList = Transaction.executingQueries();
					long waitingTime = WaitingTimeNanos;
					if ( queryList != null )
					{
						 long? latestQueryWaitingNanos = queryList.Top( executingQuery => executingQuery.totalWaitingTimeNanos( nowNanos ) );
						 waitingTime = latestQueryWaitingNanos != null ? waitingTime + latestQueryWaitingNanos : waitingTime;
					}
					return waitingTime;
			  }

			  internal virtual void Reset()
			  {
					PageCursorTracer = PageCursorTracer.NULL;
					CpuTimeNanosWhenQueryStarted = 0;
					HeapAllocatedBytesWhenQueryStarted = 0;
					WaitingTimeNanos = 0;
					TransactionThreadId = -1;
			  }
		 }

		 public override ClockContext Clocks()
		 {
			  return _clocks;
		 }

		 public override NodeCursor AmbientNodeCursor()
		 {
			  return _operations.nodeCursor();
		 }

		 public override RelationshipScanCursor AmbientRelationshipCursor()
		 {
			  return _operations.relationshipCursor();
		 }

		 public override PropertyCursor AmbientPropertyCursor()
		 {
			  return _operations.propertyCursor();
		 }

		 /// <summary>
		 /// It is not allowed for the same transaction to perform database writes as well as schema writes.
		 /// This enum tracks the current write transactionStatus of the transaction, allowing it to transition from
		 /// no writes (NONE) to data writes (DATA) or schema writes (SCHEMA), but it cannot transition between
		 /// DATA and SCHEMA without throwing an InvalidTransactionTypeKernelException. Note that this behavior
		 /// is orthogonal to the SecurityContext which manages what the transaction or statement is allowed to do
		 /// based on authorization.
		 /// </summary>
		 private sealed class TransactionWriteState
		 {
			  public static readonly TransactionWriteState None = new TransactionWriteState( "None", InnerEnum.None );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           DATA { TransactionWriteState upgradeToSchemaWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException { throw new org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException("Cannot perform schema updates in a transaction that has performed data updates."); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SCHEMA { TransactionWriteState upgradeToDataWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException { throw new org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException("Cannot perform data updates in a transaction that has performed schema updates."); } };

			  private static readonly IList<TransactionWriteState> valueList = new List<TransactionWriteState>();

			  static TransactionWriteState()
			  {
				  valueList.Add( None );
				  valueList.Add( DATA );
				  valueList.Add( SCHEMA );
			  }

			  public enum InnerEnum
			  {
				  None,
				  DATA,
				  SCHEMA
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private TransactionWriteState( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionWriteState upgradeToDataWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
			  internal TransactionWriteState UpgradeToDataWrites()
			  {
					return DATA;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionWriteState upgradeToSchemaWrites() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
			  internal TransactionWriteState UpgradeToSchemaWrites()
			  {
					return SCHEMA;
			  }

			 public static IList<TransactionWriteState> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static TransactionWriteState valueOf( string name )
			 {
				 foreach ( TransactionWriteState enumInstance in TransactionWriteState.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}