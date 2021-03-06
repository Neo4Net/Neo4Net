﻿/*
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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using AuxiliaryTransactionStateManager = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using Org.Neo4j.Kernel.extension;
	using NonTransactionalTokenNameLookup = Org.Neo4j.Kernel.Impl.Api.NonTransactionalTokenNameLookup;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using Logger = Org.Neo4j.Logging.Logger;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.index.storage.DirectoryFactory.directoryFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesBySubProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class FulltextIndexProviderFactory extends org.neo4j.kernel.extension.KernelExtensionFactory<FulltextIndexProviderFactory.Dependencies>
	public class FulltextIndexProviderFactory : KernelExtensionFactory<FulltextIndexProviderFactory.Dependencies>
	{
		 private const string KEY = "fulltext";
		 public static readonly IndexProviderDescriptor Descriptor = new IndexProviderDescriptor( KEY, "1.0" );

		 public interface Dependencies
		 {
			  Config Config { get; }

			  FileSystemAbstraction FileSystem();

			  JobScheduler Scheduler();

			  TokenHolders TokenHolders();

			  Procedures Procedures();

			  LogService LogService { get; }

			  AuxiliaryTransactionStateManager AuxiliaryTransactionStateManager();
		 }

		 public FulltextIndexProviderFactory() : base(ExtensionType.DATABASE, KEY)
		 {
		 }

		 private static IndexDirectoryStructure.Factory SubProviderDirectoryStructure( File storeDir )
		 {
			  IndexDirectoryStructure parentDirectoryStructure = directoriesByProvider( storeDir ).forProvider( Descriptor );
			  return directoriesBySubProvider( parentDirectoryStructure );
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  Config config = dependencies.Config;
			  bool ephemeral = config.Get( GraphDatabaseSettings.ephemeral );
			  FileSystemAbstraction fileSystemAbstraction = dependencies.FileSystem();
			  DirectoryFactory directoryFactory = directoryFactory( ephemeral );
			  OperationalMode operationalMode = context.DatabaseInfo().OperationalMode;
			  JobScheduler scheduler = dependencies.Scheduler();
			  IndexDirectoryStructure.Factory directoryStructureFactory = SubProviderDirectoryStructure( context.Directory() );
			  TokenHolders tokenHolders = dependencies.TokenHolders();
			  Log log = dependencies.LogService.getInternalLog( typeof( FulltextIndexProvider ) );
			  AuxiliaryTransactionStateManager auxiliaryTransactionStateManager;
			  try
			  {
					auxiliaryTransactionStateManager = dependencies.AuxiliaryTransactionStateManager();
			  }
			  catch ( UnsatisfiedDependencyException e )
			  {
					string message = "Fulltext indexes failed to register as transaction state providers. This means that, if queried, they will not be able to " +
							  "uncommitted transactional changes into account. This is fine if the indexes are opened for non-transactional work, such as for " +
							  "consistency checking. The reason given is: " + e.Message;
					LogDependencyException( context, log.ErrorLogger(), message );
					auxiliaryTransactionStateManager = new NullAuxiliaryTransactionStateManager();
			  }

			  FulltextIndexProvider provider = new FulltextIndexProvider( Descriptor, directoryStructureFactory, fileSystemAbstraction, config, tokenHolders, directoryFactory, operationalMode, scheduler, auxiliaryTransactionStateManager, log );

			  string procedureRegistrationFailureMessage = "Failed to register the fulltext index procedures. The fulltext index provider will be loaded and " +
						 "updated like normal, but it might not be possible to query any fulltext indexes. The reason given is: ";
			  try
			  {
					dependencies.Procedures().registerComponent(typeof(FulltextAdapter), procContext => provider, true);
					dependencies.Procedures().registerProcedure(typeof(FulltextProcedures));
			  }
			  catch ( KernelException e )
			  {
					string message = procedureRegistrationFailureMessage + e.getUserMessage( new NonTransactionalTokenNameLookup( tokenHolders ) );
					// We use the 'warn' logger in this case, because it can occur due to multi-database shenanigans, or due to internal restarts in HA.
					// These scenarios are less serious, and will _probably_ not prevent FTS from working. Hence we only warn about this.
					LogDependencyException( context, log.WarnLogger(), message );
			  }
			  catch ( UnsatisfiedDependencyException e )
			  {
					string message = procedureRegistrationFailureMessage + e.Message;
					LogDependencyException( context, log.ErrorLogger(), message );
			  }

			  return provider;
		 }

		 private void LogDependencyException( KernelContext context, Logger dbmsLog, string message )
		 {
			  // We can for instance get unsatisfied dependency exceptions when the kernel extension is created as part of a consistency check run.
			  // In such cases, we will be running in a TOOL context, and we ignore such exceptions since they are harmless.
			  // Tools only read, and don't run queries, so there is no need for these advanced pieces of infrastructure.
			  if ( context.DatabaseInfo() != DatabaseInfo.TOOL )
			  {
					// If we are not in a "TOOL" context, then we log this at the "DBMS" level, since it might be important for correctness.
					dbmsLog.Log( message );
			  }
		 }
	}

}