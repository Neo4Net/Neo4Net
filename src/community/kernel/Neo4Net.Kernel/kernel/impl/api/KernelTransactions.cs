using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.Impl.Api
{

	using Neo4Net.Collections.Pooling;
	using Neo4Net.Collections.Pooling;
	using Neo4Net.Functions;
	using DatabaseShutdownException = Neo4Net.GraphDb.DatabaseShutdownException;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexingProvidersService = Neo4Net.Kernel.Impl.Api.index.IndexingProvidersService;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using MonotonicCounter = Neo4Net.Kernel.impl.util.MonotonicCounter;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;


	/// <summary>
	/// Central source of transactions in the database.
	/// <para>
	/// This class maintains references to all transactions, a pool of passive kernel transactions, and provides
	/// capabilities
	/// for enumerating all running transactions. During normal operation, acquiring new transactions and enumerating live
	/// ones requires no synchronization (although the live list is not guaranteed to be exact).
	/// </para>
	/// </summary>
	public class KernelTransactions : LifecycleAdapter, System.Func<KernelTransactionsSnapshot>
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_factory = new KernelTransactionImplementationFactory( this, _allTransactions );
			_globalTxPool = new GlobalKernelTransactionPool( _allTransactions, _factory );
			_localTxPool = new MarshlandPool<KernelTransactionImplementation>( _globalTxPool );
		}

		 private readonly StatementLocksFactory _statementLocksFactory;
		 private readonly ConstraintIndexCreator _constraintIndexCreator;
		 private readonly StatementOperationParts _statementOperations;
		 private readonly SchemaWriteGuard _schemaWriteGuard;
		 private readonly TransactionHeaderInformationFactory _transactionHeaderInformationFactory;
		 private readonly TransactionCommitProcess _transactionCommitProcess;
		 private readonly AuxiliaryTransactionStateManager _auxTxStateManager;
		 private readonly TransactionHooks _hooks;
		 private readonly TransactionMonitor _transactionMonitor;
		 private readonly AvailabilityGuard _databaseAvailabilityGuard;
		 private readonly Tracers _tracers;
		 private readonly StorageEngine _storageEngine;
		 private readonly Procedures _procedures;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly AtomicReference<CpuClock> _cpuClockRef;
		 private readonly AtomicReference<HeapAllocation> _heapAllocationRef;
		 private readonly AccessCapability _accessCapability;
		 private readonly SystemNanoClock _clock;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly ReentrantReadWriteLock _newTransactionsLock = new ReentrantReadWriteLock();
		 private readonly MonotonicCounter _userTransactionIdCounter = MonotonicCounter.newAtomicMonotonicCounter();
		 private readonly AutoIndexing _autoIndexing;
		 private readonly ExplicitIndexStore _explicitIndexStore;
		 private readonly IndexingService _indexingService;
		 private readonly TokenHolders _tokenHolders;
		 private readonly string _currentDatabaseName;
		 private readonly Dependencies _dataSourceDependencies;
		 private readonly Config _config;
		 private readonly CollectionsFactorySupplier _collectionsFactorySupplier;
		 private readonly SchemaState _schemaState;

		 /// <summary>
		 /// Used to enumerate all transactions in the system, active and idle ones.
		 /// <para>
		 /// This data structure is *only* updated when brand-new transactions are created, or when transactions are disposed
		 /// of. During normal operation (where all transactions come from and are returned to the pool), this will be left
		 /// in peace, working solely as a collection of references to all transaction objects (idle and active) in the
		 /// database.
		 /// </para>
		 /// <para>
		 /// As such, it provides a good mechanism for listing all transactions without requiring synchronization when
		 /// starting and committing transactions.
		 /// </para>
		 /// </summary>
		 private readonly ISet<KernelTransactionImplementation> _allTransactions = newSetFromMap( new ConcurrentDictionary<KernelTransactionImplementation>() );

		 // This is the factory that actually builds brand-new instances.
		 private IFactory<KernelTransactionImplementation> _factory;
		 // Global pool of transactions, wrapped by the thread-local marshland pool and so is not used directly.
		 private GlobalKernelTransactionPool _globalTxPool;
		 // Pool of unused transactions.
		 private MarshlandPool<KernelTransactionImplementation> _localTxPool;
		 private readonly ConstraintSemantics _constraintSemantics;

		 /// <summary>
		 /// Kernel transactions component status. True when stopped, false when started.
		 /// Will not allow to start new transaction by stopped instance of kernel transactions.
		 /// Should simplify tracking of stopped component usage by up the stack components.
		 /// </summary>
		 private volatile bool _stopped = true;

		 public KernelTransactions( Config config, StatementLocksFactory statementLocksFactory, ConstraintIndexCreator constraintIndexCreator, StatementOperationParts statementOperations, SchemaWriteGuard schemaWriteGuard, TransactionHeaderInformationFactory txHeaderFactory, TransactionCommitProcess transactionCommitProcess, AuxiliaryTransactionStateManager auxTxStateManager, TransactionHooks hooks, TransactionMonitor transactionMonitor, AvailabilityGuard databaseAvailabilityGuard, Tracers tracers, StorageEngine storageEngine, Procedures procedures, TransactionIdStore transactionIdStore, SystemNanoClock clock, AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef, AccessCapability accessCapability, AutoIndexing autoIndexing, ExplicitIndexStore explicitIndexStore, VersionContextSupplier versionContextSupplier, CollectionsFactorySupplier collectionsFactorySupplier, ConstraintSemantics constraintSemantics, SchemaState schemaState, IndexingService indexingService, TokenHolders tokenHolders, string currentDatabaseName, Dependencies dataSourceDependencies )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._config = config;
			  this._statementLocksFactory = statementLocksFactory;
			  this._constraintIndexCreator = constraintIndexCreator;
			  this._statementOperations = statementOperations;
			  this._schemaWriteGuard = schemaWriteGuard;
			  this._transactionHeaderInformationFactory = txHeaderFactory;
			  this._transactionCommitProcess = transactionCommitProcess;
			  this._auxTxStateManager = auxTxStateManager;
			  this._hooks = hooks;
			  this._transactionMonitor = transactionMonitor;
			  this._databaseAvailabilityGuard = databaseAvailabilityGuard;
			  this._tracers = tracers;
			  this._storageEngine = storageEngine;
			  this._procedures = procedures;
			  this._transactionIdStore = transactionIdStore;
			  this._cpuClockRef = cpuClockRef;
			  this._heapAllocationRef = heapAllocationRef;
			  this._accessCapability = accessCapability;
			  this._autoIndexing = autoIndexing;
			  this._explicitIndexStore = explicitIndexStore;
			  this._indexingService = indexingService;
			  this._tokenHolders = tokenHolders;
			  this._currentDatabaseName = currentDatabaseName;
			  this._dataSourceDependencies = dataSourceDependencies;
			  this._versionContextSupplier = versionContextSupplier;
			  this._clock = clock;
			  DoBlockNewTransactions();
			  this._collectionsFactorySupplier = collectionsFactorySupplier;
			  this._constraintSemantics = constraintSemantics;
			  this._schemaState = schemaState;
		 }

		 public virtual KernelTransaction NewInstance( KernelTransaction.Type type, LoginContext loginContext, long timeout )
		 {
			  AssertCurrentThreadIsNotBlockingNewTransactions();
			  SecurityContext securityContext = loginContext.Authorize( _tokenHolders.propertyKeyTokens().getOrCreateId, _currentDatabaseName );
			  try
			  {
					while ( !_newTransactionsLock.readLock().tryLock(1, TimeUnit.SECONDS) )
					{
						 AssertRunning();
					}
					try
					{
						 AssertRunning();
						 TransactionId lastCommittedTransaction = _transactionIdStore.LastCommittedTransaction;
						 KernelTransactionImplementation tx = _localTxPool.acquire();
						 StatementLocks statementLocks = _statementLocksFactory.newInstance();
						 tx.Initialize( lastCommittedTransaction.TransactionIdConflict(), lastCommittedTransaction.CommitTimestamp(), statementLocks, type, securityContext, timeout, _userTransactionIdCounter.incrementAndGet() );
						 return tx;
					}
					finally
					{
						 _newTransactionsLock.readLock().unlock();
					}
			  }
			  catch ( InterruptedException ie )
			  {
					Thread.interrupted();
					throw new TransactionFailureException( "Fail to start new transaction.", ie );
			  }
		 }

		 /// <summary>
		 /// Give an approximate set of all transactions currently running.
		 /// This is not guaranteed to be exact, as transactions may stop and start while this set is gathered.
		 /// </summary>
		 /// <returns> the (approximate) set of open transactions. </returns>
		 public virtual ISet<KernelTransactionHandle> ActiveTransactions()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return _allTransactions.Select( this.createHandle ).Where( KernelTransactionHandle::isOpen ).collect( toSet() );
		 }

		 /// <summary>
		 /// Dispose of all pooled transactions. This is done on shutdown or on internal events (like an HA mode switch) that
		 /// require transactions to be re-created.
		 /// </summary>
		 public virtual void DisposeAll()
		 {
			  TerminateTransactions();
			  _localTxPool.close();
			  _globalTxPool.close();
		 }

		 public virtual void TerminateTransactions()
		 {
			  MarkAllTransactionsAsTerminated();
		 }

		 private void MarkAllTransactionsAsTerminated()
		 {
			  // we mark all transactions for termination since we want to make sure these transactions
			  // won't be reused, ever. Each transaction has, among other things, a Locks.Client and we
			  // certainly want to keep that from being reused from this point.
			  _allTransactions.forEach( tx => tx.markForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable ) );
		 }

		 public virtual bool HaveClosingTransaction()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _allTransactions.Any( KernelTransactionImplementation::isClosing );
		 }

		 public override void Start()
		 {
			  _stopped = false;
			  UnblockNewTransactions();
		 }

		 public override void Stop()
		 {
			  BlockNewTransactions();
			  _stopped = true;
		 }

		 public override void Shutdown()
		 {
			  DisposeAll();
		 }

		 public override KernelTransactionsSnapshot Get()
		 {
			  return new KernelTransactionsSnapshot( ActiveTransactions(), _clock.millis() );
		 }

		 /// <summary>
		 /// Do not allow new transactions to start until <seealso cref="unblockNewTransactions()"/> is called. Current thread have
		 /// responsibility of doing so.
		 /// <para>
		 /// Blocking call.
		 /// </para>
		 /// </summary>
		 public virtual void BlockNewTransactions()
		 {
			  DoBlockNewTransactions();
		 }

		 /// <summary>
		 /// This is private since it's called from the constructor.
		 /// </summary>
		 private void DoBlockNewTransactions()
		 {
			  _newTransactionsLock.writeLock().@lock();
		 }

		 /// <summary>
		 /// Allow new transactions to be started again if current thread is the one who called
		 /// <seealso cref="blockNewTransactions()"/>.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if current thread is not the one that called <seealso cref="blockNewTransactions()"/>. </exception>
		 public virtual void UnblockNewTransactions()
		 {
			  if ( !_newTransactionsLock.writeLock().HeldByCurrentThread )
			  {
					throw new System.InvalidOperationException( "This thread did not block transactions previously" );
			  }
			  _newTransactionsLock.writeLock().unlock();
		 }

		 /// <summary>
		 /// Create new handle for the given transaction.
		 /// <para>
		 /// <b>Note:</b> this method is package-private for testing <b>only</b>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="tx"> transaction to wrap. </param>
		 /// <returns> transaction handle. </returns>
		 internal virtual KernelTransactionHandle CreateHandle( KernelTransactionImplementation tx )
		 {
			  return new KernelTransactionImplementationHandle( tx, _clock );
		 }

		 private void AssertRunning()
		 {
			  if ( _databaseAvailabilityGuard.Shutdown )
			  {
					throw new DatabaseShutdownException();
			  }
			  if ( _stopped )
			  {
					throw new System.InvalidOperationException( "Can't start new transaction with stopped " + this.GetType() );
			  }
		 }

		 private void AssertCurrentThreadIsNotBlockingNewTransactions()
		 {
			  if ( _newTransactionsLock.WriteLockedByCurrentThread )
			  {
					throw new System.InvalidOperationException( "Thread that is blocking new transactions from starting can't start new transaction" );
			  }
		 }

		 private class KernelTransactionImplementationFactory : IFactory<KernelTransactionImplementation>
		 {
			 private readonly KernelTransactions _outerInstance;

			  internal readonly ISet<KernelTransactionImplementation> Transactions;

			  internal KernelTransactionImplementationFactory( KernelTransactions outerInstance, ISet<KernelTransactionImplementation> transactions )
			  {
				  this._outerInstance = outerInstance;
					this.Transactions = transactions;
			  }

			  public override KernelTransactionImplementation NewInstance()
			  {
					KernelTransactionImplementation tx = new KernelTransactionImplementation( outerInstance.config, outerInstance.statementOperations, outerInstance.schemaWriteGuard, outerInstance.hooks, outerInstance.constraintIndexCreator, outerInstance.procedures, outerInstance.transactionHeaderInformationFactory, outerInstance.transactionCommitProcess, outerInstance.transactionMonitor, outerInstance.auxTxStateManager, outerInstance.localTxPool, outerInstance.clock, outerInstance.cpuClockRef, outerInstance.heapAllocationRef, outerInstance.tracers.TransactionTracer, outerInstance.tracers.LockTracer, outerInstance.tracers.PageCursorTracerSupplier, outerInstance.storageEngine, outerInstance.accessCapability, outerInstance.autoIndexing, outerInstance.explicitIndexStore, outerInstance.versionContextSupplier, outerInstance.collectionsFactorySupplier, outerInstance.constraintSemantics, outerInstance.schemaState, outerInstance.indexingService, outerInstance.tokenHolders, outerInstance.dataSourceDependencies );
					this.Transactions.Add( tx );
					return tx;
			  }
		 }

		 private class GlobalKernelTransactionPool : LinkedQueuePool<KernelTransactionImplementation>
		 {
			  internal readonly ISet<KernelTransactionImplementation> Transactions;

			  internal GlobalKernelTransactionPool( ISet<KernelTransactionImplementation> transactions, IFactory<KernelTransactionImplementation> factory ) : base( 8, factory )
			  {
					this.Transactions = transactions;
			  }

			  protected internal override void Dispose( KernelTransactionImplementation tx )
			  {
					Transactions.remove( tx );
					tx.Dispose();
					base.Dispose( tx );
			  }
		 }
	}

}