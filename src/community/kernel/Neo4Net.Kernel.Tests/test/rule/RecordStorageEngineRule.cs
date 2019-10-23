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
namespace Neo4Net.Test.rule
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using BatchTransactionApplierFacade = Neo4Net.Kernel.Impl.Api.BatchTransactionApplierFacade;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using ReentrantLockService = Neo4Net.Kernel.impl.locking.ReentrantLockService;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using BufferedIdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Neo4Net.Kernel.impl.store.id.IdReuseEligibility;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IndexActivator = Neo4Net.Kernel.impl.transaction.command.IndexActivator;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using SynchronizedArrayIdOrderingQueue = Neo4Net.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using KernelEventHandlers = Neo4Net.Kernel.Internal.KernelEventHandlers;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;
	using EphemeralIdGenerator = Neo4Net.Test.impl.EphemeralIdGenerator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.MockedNeoStores.mockedTokenHolders;

	/// <summary>
	/// Conveniently manages a <seealso cref="RecordStorageEngine"/> in a test. Needs <seealso cref="FileSystemAbstraction"/> and
	/// <seealso cref="PageCache"/>, which usually are managed by test rules themselves. That's why they are passed in
	/// when <seealso cref="getWith(FileSystemAbstraction, PageCache, DatabaseLayout) getting (constructing)"/> the engine. Further
	/// dependencies can be overridden in that returned builder as well.
	/// <para>
	/// Keep in mind that this rule must be created BEFORE <seealso cref="ConfigurablePageCacheRule"/> and any file system rule so that
	/// shutdown order gets correct.
	/// </para>
	/// </summary>
	public class RecordStorageEngineRule : ExternalResource
	{
		 private readonly LifeSupport _life = new LifeSupport();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void before() throws Throwable
		 protected internal override void Before()
		 {
			  base.Before();
			  _life.start();
		 }

		 public virtual Builder GetWith( FileSystemAbstraction fs, PageCache pageCache, DatabaseLayout databaseLayout )
		 {
			  return new Builder( this, fs, pageCache, databaseLayout );
		 }

		 private RecordStorageEngine Get( FileSystemAbstraction fs, PageCache pageCache, IndexProvider indexProvider, DatabaseHealth databaseHealth, DatabaseLayout databaseLayout, System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> transactionApplierTransformer, Monitors monitors, LockService lockService )
		 {
			  IdGeneratorFactory idGeneratorFactory = new EphemeralIdGenerator.Factory();
			  ExplicitIndexProvider explicitIndexProviderLookup = mock( typeof( ExplicitIndexProvider ) );
			  when( explicitIndexProviderLookup.AllIndexProviders() ).thenReturn(Iterables.empty());
			  IndexConfigStore indexConfigStore = new IndexConfigStore( databaseLayout, fs );
			  IJobScheduler scheduler = _life.add( createScheduler() );
			  Config config = Config.defaults( GraphDatabaseSettings.default_schema_provider, indexProvider.ProviderDescriptor.name() );

			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( indexProvider );

			  BufferingIdGeneratorFactory bufferingIdGeneratorFactory = new BufferingIdGeneratorFactory( idGeneratorFactory, Neo4Net.Kernel.impl.store.id.IdReuseEligibility_Fields.Always, new CommunityIdTypeConfigurationProvider() );
			  DefaultIndexProviderMap indexProviderMap = new DefaultIndexProviderMap( dependencies, config );
			  NullLogProvider nullLogProvider = NullLogProvider.Instance;
			  _life.add( indexProviderMap );
			  return _life.add( new ExtendedRecordStorageEngine( databaseLayout, config, pageCache, fs, nullLogProvider, nullLogProvider, mockedTokenHolders(), mock(typeof(SchemaState)), new StandardConstraintSemantics(), scheduler, mock(typeof(TokenNameLookup)), lockService, indexProviderMap, IndexingService.NO_MONITOR, databaseHealth, explicitIndexProviderLookup, indexConfigStore, new SynchronizedArrayIdOrderingQueue(), idGeneratorFactory, new BufferedIdController(bufferingIdGeneratorFactory, scheduler), transactionApplierTransformer, monitors, RecoveryCleanupWorkCollector.immediate(), OperationalMode.single ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void after(boolean successful) throws Throwable
		 protected internal override void After( bool successful )
		 {
			  _life.shutdown();
			  base.After( successful );
		 }

		 public class Builder
		 {
			 private readonly RecordStorageEngineRule _outerInstance;

			  internal readonly FileSystemAbstraction Fs;
			  internal readonly PageCache PageCache;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal DatabaseHealth DatabaseHealthConflict = new DatabaseHealth( new DatabasePanicEventGenerator( new KernelEventHandlers( NullLog.Instance ) ), NullLog.Instance );
			  internal readonly DatabaseLayout DatabaseLayout;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> TransactionApplierTransformerConflict = applierFacade => applierFacade;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IndexProvider IndexProviderConflict = IndexProvider.EMPTY;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Monitors MonitorsConflict = new Monitors();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal LockService LockServiceConflict = new ReentrantLockService();

			  public Builder( RecordStorageEngineRule outerInstance, FileSystemAbstraction fs, PageCache pageCache, DatabaseLayout databaseLayout )
			  {
				  this._outerInstance = outerInstance;
					this.Fs = fs;
					this.PageCache = pageCache;
					this.DatabaseLayout = databaseLayout;
			  }

			  public virtual Builder TransactionApplierTransformer( System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> transactionApplierTransformer )
			  {
					this.TransactionApplierTransformerConflict = transactionApplierTransformer;
					return this;
			  }

			  public virtual Builder IndexProvider( IndexProvider indexProvider )
			  {
					this.IndexProviderConflict = indexProvider;
					return this;
			  }

			  public virtual Builder DatabaseHealth( DatabaseHealth databaseHealth )
			  {
					this.DatabaseHealthConflict = databaseHealth;
					return this;
			  }

			  public virtual Builder Monitors( Monitors monitors )
			  {
					this.MonitorsConflict = monitors;
					return this;
			  }

			  public virtual Builder LockService( LockService lockService )
			  {
					this.LockServiceConflict = lockService;
					return this;
			  }

			  public virtual RecordStorageEngine Build()
			  {
					return outerInstance.get( Fs, PageCache, IndexProviderConflict, DatabaseHealthConflict, DatabaseLayout, TransactionApplierTransformerConflict, MonitorsConflict, LockServiceConflict );
			  }
		 }

		 private class ExtendedRecordStorageEngine : RecordStorageEngine
		 {
			  internal readonly System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> TransactionApplierTransformer;

			  internal ExtendedRecordStorageEngine( DatabaseLayout databaseLayout, Config config, PageCache pageCache, FileSystemAbstraction fs, LogProvider logProvider, LogProvider userLogProvider, TokenHolders tokenHolders, SchemaState schemaState, ConstraintSemantics constraintSemantics, IJobScheduler scheduler, TokenNameLookup tokenNameLookup, LockService lockService, IndexProviderMap indexProviderMap, IndexingService.Monitor indexingServiceMonitor, DatabaseHealth databaseHealth, ExplicitIndexProvider explicitIndexProviderLookup, IndexConfigStore indexConfigStore, IdOrderingQueue explicitIndexTransactionOrdering, IdGeneratorFactory idGeneratorFactory, IdController idController, System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> transactionApplierTransformer, Monitors monitors, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, OperationalMode operationalMode ) : base( databaseLayout, config, pageCache, fs, logProvider, userLogProvider, tokenHolders, schemaState, constraintSemantics, scheduler, tokenNameLookup, lockService, indexProviderMap, indexingServiceMonitor, databaseHealth, explicitIndexProviderLookup, indexConfigStore, explicitIndexTransactionOrdering, idGeneratorFactory, idController, monitors, recoveryCleanupWorkCollector, operationalMode, EmptyVersionContextSupplier.EMPTY )
			  {
					this.TransactionApplierTransformer = transactionApplierTransformer;
			  }

			  protected internal override BatchTransactionApplierFacade Applier( TransactionApplicationMode mode, IndexActivator indexActivator )
			  {
					BatchTransactionApplierFacade recordEngineApplier = base.Applier( mode, indexActivator );
					return TransactionApplierTransformer.apply( recordEngineApplier );
			  }
		 }
	}

}