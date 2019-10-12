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
namespace Org.Neo4j.Graphdb.factory.module.edition
{

	using Predicates = Org.Neo4j.Function.Predicates;
	using IdContextFactory = Org.Neo4j.Graphdb.factory.module.id.IdContextFactory;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using SecurityModule = Org.Neo4j.Kernel.api.security.SecurityModule;
	using NoAuthSecurityProvider = Org.Neo4j.Kernel.api.security.provider.NoAuthSecurityProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using DefaultLabelIdCreator = Org.Neo4j.Kernel.impl.core.DefaultLabelIdCreator;
	using DefaultPropertyTokenCreator = Org.Neo4j.Kernel.impl.core.DefaultPropertyTokenCreator;
	using DefaultRelationshipTypeCreator = Org.Neo4j.Kernel.impl.core.DefaultRelationshipTypeCreator;
	using DelegatingTokenHolder = Org.Neo4j.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Org.Neo4j.Kernel.impl.core.ReadOnlyTokenCreator;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenCreator = Org.Neo4j.Kernel.impl.core.TokenCreator;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CanWrite = Org.Neo4j.Kernel.impl.factory.CanWrite;
	using CommunityCommitProcessFactory = Org.Neo4j.Kernel.impl.factory.CommunityCommitProcessFactory;
	using ReadOnly = Org.Neo4j.Kernel.impl.factory.ReadOnly;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using LocksFactory = Org.Neo4j.Kernel.impl.locking.LocksFactory;
	using SimpleStatementLocksFactory = Org.Neo4j.Kernel.impl.locking.SimpleStatementLocksFactory;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using UsageData = Org.Neo4j.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.EditionLocksFactories.createLockFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.EditionLocksFactories.createLockManager;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Community edition.
	/// </summary>
	public class CommunityEditionModule : DefaultEditionModule
	{
		 public const string COMMUNITY_SECURITY_MODULE_ID = "community-security-module";

		 public CommunityEditionModule( PlatformModule platformModule )
		 {
			  Org.Neo4j.Kernel.impl.util.Dependencies dependencies = platformModule.Dependencies;
			  Config config = platformModule.Config;
			  LogService logging = platformModule.Logging;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
			  PageCache pageCache = platformModule.PageCache;
			  DataSourceManager dataSourceManager = platformModule.DataSourceManager;
			  LifeSupport life = platformModule.Life;
			  life.Add( platformModule.DataSourceManager );

			  WatcherServiceFactoryConflict = databaseDir => CreateFileSystemWatcherService( fileSystem, databaseDir, logging, platformModule.JobScheduler, config, FileWatcherFileNameFilter() );

			  this.AccessCapabilityConflict = config.Get( GraphDatabaseSettings.read_only ) ? new ReadOnly() : new CanWrite();

			  dependencies.SatisfyDependency( SslPolicyLoader.create( config, logging.InternalLogProvider ) ); // for bolt and web server

			  LocksFactory lockFactory = createLockFactory( config, logging );
			  LocksSupplierConflict = () => createLockManager(lockFactory, config, platformModule.Clock);
			  StatementLocksFactoryProviderConflict = locks => CreateStatementLocksFactory( locks, config, logging );

			  ThreadToTransactionBridgeConflict = dependencies.SatisfyDependency( new ThreadToStatementContextBridge( GetGlobalAvailabilityGuard( platformModule.Clock, logging, platformModule.Config ) ) );

			  IdContextFactoryConflict = CreateIdContextFactory( platformModule, fileSystem );

			  TokenHoldersProviderConflict = CreateTokenHolderProvider( platformModule );

			  File kernelContextDirectory = platformModule.StoreLayout.storeDirectory();
			  dependencies.SatisfyDependency( CreateKernelData( fileSystem, pageCache, kernelContextDirectory, config, life, dataSourceManager ) );

			  CommitProcessFactoryConflict = new CommunityCommitProcessFactory();

			  HeaderInformationFactoryConflict = CreateHeaderInformationFactory();

			  SchemaWriteGuardConflict = CreateSchemaWriteGuard();

			  TransactionStartTimeoutConflict = config.Get( GraphDatabaseSettings.transaction_start_timeout ).toMillis();

			  ConstraintSemanticsConflict = CreateSchemaRuleVerifier();

			  IoLimiterConflict = Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited;

			  ConnectionTrackerConflict = dependencies.SatisfyDependency( CreateConnectionTracker() );

			  PublishEditionInfo( dependencies.ResolveDependency( typeof( UsageData ) ), platformModule.DatabaseInfo, config );
		 }

		 protected internal virtual System.Func<string, TokenHolders> CreateTokenHolderProvider( PlatformModule platform )
		 {
			  Config config = platform.Config;
			  DataSourceManager dataSourceManager = platform.DataSourceManager;
			  return ignored => new TokenHolders( new DelegatingTokenHolder( CreatePropertyKeyCreator( config, dataSourceManager ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder( CreateLabelIdCreator( config, dataSourceManager ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL ), new DelegatingTokenHolder( CreateRelationshipTypeCreator( config, dataSourceManager ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE ) );
		 }

		 protected internal virtual IdContextFactory CreateIdContextFactory( PlatformModule platformModule, FileSystemAbstraction fileSystem )
		 {
			  return IdContextFactoryBuilder.of( fileSystem, platformModule.JobScheduler ).build();
		 }

		 protected internal virtual System.Predicate<string> FileWatcherFileNameFilter()
		 {
			  return CommunityFileWatcherFileNameFilter();
		 }

		 internal static System.Predicate<string> CommunityFileWatcherFileNameFilter()
		 {
			  return Predicates.any( fileName => fileName.StartsWith( TransactionLogFiles.DEFAULT_NAME ), fileName => fileName.StartsWith( IndexConfigStore.INDEX_DB_FILE_NAME ) );
		 }

		 protected internal virtual ConstraintSemantics CreateSchemaRuleVerifier()
		 {
			  return new StandardConstraintSemantics();
		 }

		 protected internal virtual StatementLocksFactory CreateStatementLocksFactory( Locks locks, Config config, LogService logService )
		 {
			  return new SimpleStatementLocksFactory( locks );
		 }

		 protected internal virtual SchemaWriteGuard CreateSchemaWriteGuard()
		 {
			  return Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard_Fields.AllowAllWrites;
		 }

		 protected internal static TokenCreator CreateRelationshipTypeCreator( Config config, System.Func<Kernel> kernelSupplier )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }
			  else
			  {
					return new DefaultRelationshipTypeCreator( kernelSupplier );
			  }
		 }

		 protected internal static TokenCreator CreatePropertyKeyCreator( Config config, System.Func<Kernel> kernelSupplier )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }
			  else
			  {
					return new DefaultPropertyTokenCreator( kernelSupplier );
			  }
		 }

		 protected internal static TokenCreator CreateLabelIdCreator( Config config, System.Func<Kernel> kernelSupplier )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }
			  else
			  {
					return new DefaultLabelIdCreator( kernelSupplier );
			  }
		 }

		 private KernelData CreateKernelData( FileSystemAbstraction fileSystem, PageCache pageCache, File storeDir, Config config, LifeSupport life, DataSourceManager dataSourceManager )
		 {
			  return life.Add( new KernelData( fileSystem, pageCache, storeDir, config, dataSourceManager ) );
		 }

		 protected internal virtual TransactionHeaderInformationFactory CreateHeaderInformationFactory()
		 {
			  return TransactionHeaderInformationFactory.DEFAULT;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void RegisterEditionSpecificProcedures( Procedures procedures )
		 {
			  // Community does not add any extra procedures
		 }

		 public override void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  if ( platformModule.Config.get( GraphDatabaseSettings.auth_enabled ) )
			  {
					SecurityModule securityModule = SetupSecurityModule( platformModule, this, platformModule.Logging.getUserLog( this.GetType() ), procedures, COMMUNITY_SECURITY_MODULE_ID );
					platformModule.Life.add( securityModule );
					this.SecurityProviderConflict = securityModule;
			  }
			  else
			  {
					NoAuthSecurityProvider noAuthSecurityProvider = NoAuthSecurityProvider.INSTANCE;
					platformModule.Life.add( noAuthSecurityProvider );
					this.SecurityProviderConflict = noAuthSecurityProvider;
			  }
		 }
	}

}