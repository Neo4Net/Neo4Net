/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.impl.enterprise
{

	using Predicates = Org.Neo4j.Function.Predicates;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using IdContextFactory = Org.Neo4j.Graphdb.factory.module.id.IdContextFactory;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using SecurityModule = Org.Neo4j.Kernel.api.security.SecurityModule;
	using SecurityProvider = Org.Neo4j.Kernel.api.security.provider.SecurityProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseNoAuthSecurityProvider = Org.Neo4j.Kernel.enterprise.api.security.provider.EnterpriseNoAuthSecurityProvider;
	using EnterpriseBuiltInDbmsProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using ConfigurableIOLimiter = Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using StatementLocksFactorySelector = Org.Neo4j.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using DefaultNetworkConnectionTracker = Org.Neo4j.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Enterprise edition, without HA
	/// </summary>
	public class EnterpriseEditionModule : CommunityEditionModule
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
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