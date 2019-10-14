using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using LogFileInformation = Neo4Net.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// @deprecated This will be moved to an internal package in the future. 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated("This will be moved to an internal package in the future.") @Service.Implementation(KernelExtensionFactory.class) public class OnlineBackupExtensionFactory extends org.neo4j.kernel.extension.KernelExtensionFactory<OnlineBackupExtensionFactory.Dependencies>
	[Obsolete("This will be moved to an internal package in the future.")]
	public class OnlineBackupExtensionFactory : KernelExtensionFactory<OnlineBackupExtensionFactory.Dependencies>
	{
		 internal const string KEY = "online backup";

		 [Obsolete]
		 public interface Dependencies
		 {
			  Config Config { get; }

			  GraphDatabaseAPI GraphDatabaseAPI { get; }

			  LogService LogService();

			  Monitors Monitors();

			  NeoStoreDataSource NeoStoreDataSource();

			  System.Func<CheckPointer> CheckPointer();

			  System.Func<TransactionIdStore> TransactionIdStoreSupplier();

			  System.Func<LogicalTransactionStore> LogicalTransactionStoreSupplier();

			  System.Func<LogFileInformation> LogFileInformationSupplier();

			  FileSystemAbstraction FileSystemAbstraction();

			  PageCache PageCache();

			  StoreCopyCheckPointMutex StoreCopyCheckPointMutex();
		 }

		 public OnlineBackupExtensionFactory() : base(ExtensionType.DATABASE, KEY)
		 {
		 }

		 [Obsolete]
		 public virtual Type<OnlineBackupSettings> SettingsClass
		 {
			 get
			 {
				  throw new AssertionError();
			 }
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  if ( !IsCausalClusterInstance( context ) && IsDefaultDatabase( dependencies.NeoStoreDataSource(), dependencies.Config ) )
			  {
					return new OnlineBackupKernelExtension( dependencies.Config, dependencies.GraphDatabaseAPI, dependencies.LogService().InternalLogProvider, dependencies.Monitors(), dependencies.NeoStoreDataSource(), dependencies.FileSystemAbstraction() );
			  }
			  return new LifecycleAdapter();
		 }

		 private static bool IsDefaultDatabase( NeoStoreDataSource neoStoreDataSource, Config config )
		 {
			  return neoStoreDataSource.DatabaseName.Equals( config.Get( GraphDatabaseSettings.active_database ) );
		 }

		 private static bool IsCausalClusterInstance( KernelContext kernelContext )
		 {
			  OperationalMode thisMode = kernelContext.DatabaseInfo().OperationalMode;
			  return OperationalMode.core.Equals( thisMode ) || OperationalMode.read_replica.Equals( thisMode );
		 }
	}

}