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
namespace Org.Neo4j.Test.rule
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using BatchTransactionApplierFacade = Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplierFacade;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using SchemaState = Org.Neo4j.Kernel.Impl.Api.SchemaState;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using DatabasePanicEventGenerator = Org.Neo4j.Kernel.impl.core.DatabasePanicEventGenerator;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using ReentrantLockService = Org.Neo4j.Kernel.impl.locking.ReentrantLockService;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility;
	using CommunityIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IndexActivator = Org.Neo4j.Kernel.impl.transaction.command.IndexActivator;
	using DefaultIndexProviderMap = Org.Neo4j.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using IdOrderingQueue = Org.Neo4j.Kernel.impl.util.IdOrderingQueue;
	using SynchronizedArrayIdOrderingQueue = Org.Neo4j.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using KernelEventHandlers = Org.Neo4j.Kernel.@internal.KernelEventHandlers;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using EphemeralIdGenerator = Org.Neo4j.Test.impl.EphemeralIdGenerator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

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
			  JobScheduler scheduler = _life.add( createScheduler() );
			  Config config = Config.defaults( GraphDatabaseSettings.default_schema_provider, indexProvider.ProviderDescriptor.name() );

			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( indexProvider );

			  BufferingIdGeneratorFactory bufferingIdGeneratorFactory = new BufferingIdGeneratorFactory( idGeneratorFactory, Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility_Fields.Always, new CommunityIdTypeConfigurationProvider() );
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

			  internal ExtendedRecordStorageEngine( DatabaseLayout databaseLayout, Config config, PageCache pageCache, FileSystemAbstraction fs, LogProvider logProvider, LogProvider userLogProvider, TokenHolders tokenHolders, SchemaState schemaState, ConstraintSemantics constraintSemantics, JobScheduler scheduler, TokenNameLookup tokenNameLookup, LockService lockService, IndexProviderMap indexProviderMap, IndexingService.Monitor indexingServiceMonitor, DatabaseHealth databaseHealth, ExplicitIndexProvider explicitIndexProviderLookup, IndexConfigStore indexConfigStore, IdOrderingQueue explicitIndexTransactionOrdering, IdGeneratorFactory idGeneratorFactory, IdController idController, System.Func<BatchTransactionApplierFacade, BatchTransactionApplierFacade> transactionApplierTransformer, Monitors monitors, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, OperationalMode operationalMode ) : base( databaseLayout, config, pageCache, fs, logProvider, userLogProvider, tokenHolders, schemaState, constraintSemantics, scheduler, tokenNameLookup, lockService, indexProviderMap, indexingServiceMonitor, databaseHealth, explicitIndexProviderLookup, indexConfigStore, explicitIndexTransactionOrdering, idGeneratorFactory, idController, monitors, recoveryCleanupWorkCollector, operationalMode, EmptyVersionContextSupplier.EMPTY )
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