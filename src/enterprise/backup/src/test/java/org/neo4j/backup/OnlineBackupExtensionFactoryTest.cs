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
namespace Neo4Net.backup
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using LogFileInformation = Neo4Net.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class OnlineBackupExtensionFactoryTest
	internal class OnlineBackupExtensionFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
		 private OnlineBackupExtensionFactory _extensionFactory = new OnlineBackupExtensionFactory();
		 private Dependencies _dependencies = new Dependencies();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCreateExtensionForNonDefaultDatabase()
		 internal virtual void DoNotCreateExtensionForNonDefaultDatabase()
		 {
			  SimpleKernelContext kernelContext = new SimpleKernelContext( _testDirectory.databaseDir(), DatabaseInfo.ENTERPRISE, _dependencies );
			  Lifecycle instance = _extensionFactory.newInstance( kernelContext, new TestBackDependencies( "another" ) );
			  assertThat( instance, not( instanceOf( typeof( OnlineBackupKernelExtension ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createExtensionForDefaultDatabase()
		 internal virtual void CreateExtensionForDefaultDatabase()
		 {
			  SimpleKernelContext kernelContext = new SimpleKernelContext( _testDirectory.databaseDir(), DatabaseInfo.ENTERPRISE, _dependencies );
			  Lifecycle instance = _extensionFactory.newInstance( kernelContext, new TestBackDependencies( GraphDatabaseSettings.DEFAULT_DATABASE_NAME ) );
			  assertThat( instance, instanceOf( typeof( OnlineBackupKernelExtension ) ) );
		 }

		 private class TestBackDependencies : OnlineBackupExtensionFactory.Dependencies
		 {
			  internal readonly string DatabaseName;

			  internal TestBackDependencies( string databaseName )
			  {
					this.DatabaseName = databaseName;
			  }

			  public virtual Config Config
			  {
				  get
				  {
						return Config.defaults();
				  }
			  }

			  public virtual GraphDatabaseAPI GraphDatabaseAPI
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override LogService LogService()
			  {
					return new SimpleLogService( NullLogProvider.Instance );
			  }

			  public override Monitors Monitors()
			  {
					return null;
			  }

			  public override NeoStoreDataSource NeoStoreDataSource()
			  {
					NeoStoreDataSource neoStoreDataSource = mock( typeof( NeoStoreDataSource ) );
					when( neoStoreDataSource.DatabaseName ).thenReturn( DatabaseName );
					return neoStoreDataSource;
			  }

			  public override System.Func<CheckPointer> CheckPointer()
			  {
					return null;
			  }

			  public override System.Func<TransactionIdStore> TransactionIdStoreSupplier()
			  {
					return null;
			  }

			  public override System.Func<LogicalTransactionStore> LogicalTransactionStoreSupplier()
			  {
					return null;
			  }

			  public override System.Func<LogFileInformation> LogFileInformationSupplier()
			  {
					return null;
			  }

			  public override FileSystemAbstraction FileSystemAbstraction()
			  {
					return null;
			  }

			  public override PageCache PageCache()
			  {
					return null;
			  }

			  public override StoreCopyCheckPointMutex StoreCopyCheckPointMutex()
			  {
					return null;
			  }
		 }
	}

}