/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise
{

	using Predicates = Neo4Net.Functions.Predicates;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using IdContextFactory = Neo4Net.GraphDb.factory.module.id.IdContextFactory;
	using IdContextFactoryBuilder = Neo4Net.GraphDb.factory.module.id.IdContextFactoryBuilder;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NetworkConnectionTracker = Neo4Net.Kernel.Api.net.NetworkConnectionTracker;
	using SecurityModule = Neo4Net.Kernel.Api.security.SecurityModule;
	using SecurityProvider = Neo4Net.Kernel.Api.security.provider.SecurityProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseNoAuthSecurityProvider = Neo4Net.Kernel.enterprise.api.security.provider.EnterpriseNoAuthSecurityProvider;
	using EnterpriseBuiltInDbmsProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using EnterpriseIdTypeConfigurationProvider = Neo4Net.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using ConfigurableIOLimiter = Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using StatementLocksFactorySelector = Neo4Net.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using DefaultNetworkConnectionTracker = Neo4Net.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Neo4Net.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Enterprise edition, without HA
	/// </summary>
	public class EnterpriseEditionModule : CommunityEditionModule
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(Neo4Net.kernel.impl.proc.Procedures procedures) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override void RegisterEditionSpecificProcedures( Procedures procedures )
		 {
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInDbmsProcedures ), true );
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInProcedures ), true );
		 }

		 public EnterpriseEditionModule( PlatformModule platformModule ) : base( platformModule )
		 {
			  IoLimiterConflict = new ConfigurableIOLimiter( platformModule.Config );
		 }

		 protected internal override IdContextFactory CreateIdContextFactory( PlatformModule platformModule, FileSystemAbstraction fileSystem )
		 {
			  return IdContextFactoryBuilder.of( new EnterpriseIdTypeConfigurationProvider( platformModule.Config ), platformModule.JobScheduler ).withFileSystem( fileSystem ).build();
		 }

		 protected internal override System.Predicate<string> FileWatcherFileNameFilter()
		 {
			  return EnterpriseNonClusterFileWatcherFileNameFilter();
		 }

		 internal static System.Predicate<string> EnterpriseNonClusterFileWatcherFileNameFilter()
		 {
			  return Predicates.any( fileName => fileName.StartsWith( TransactionLogFiles.DEFAULT_NAME ), fileName => fileName.StartsWith( IndexConfigStore.INDEX_DB_FILE_NAME ), filename => filename.EndsWith( PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }

		 protected internal override ConstraintSemantics CreateSchemaRuleVerifier()
		 {
			  return new EnterpriseConstraintSemantics();
		 }

		 protected internal override NetworkConnectionTracker CreateConnectionTracker()
		 {
			  return new DefaultNetworkConnectionTracker();
		 }

		 protected internal override StatementLocksFactory CreateStatementLocksFactory( Locks locks, Config config, LogService logService )
		 {
			  return ( new StatementLocksFactorySelector( locks, config, logService ) ).select();
		 }

		 public override void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  EnterpriseEditionModule.CreateEnterpriseSecurityModule( this, platformModule, procedures );
		 }

		 public static void CreateEnterpriseSecurityModule( AbstractEditionModule editionModule, PlatformModule platformModule, Procedures procedures )
		 {
			  SecurityProvider securityProvider;
			  if ( platformModule.Config.get( GraphDatabaseSettings.auth_enabled ) )
			  {
					SecurityModule securityModule = SetupSecurityModule( platformModule, editionModule, platformModule.Logging.getUserLog( typeof( EnterpriseEditionModule ) ), procedures, platformModule.Config.get( EnterpriseEditionSettings.security_module ) );
					platformModule.Life.add( securityModule );
					securityProvider = securityModule;
			  }
			  else
			  {
					EnterpriseNoAuthSecurityProvider provider = EnterpriseNoAuthSecurityProvider.INSTANCE;
					platformModule.Life.add( provider );
					securityProvider = provider;
			  }
			  editionModule.SecurityProvider = securityProvider;
		 }
	}

}