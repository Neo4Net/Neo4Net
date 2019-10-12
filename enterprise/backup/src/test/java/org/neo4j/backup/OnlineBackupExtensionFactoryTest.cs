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
namespace Org.Neo4j.backup
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using LogFileInformation = Org.Neo4j.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

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
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
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