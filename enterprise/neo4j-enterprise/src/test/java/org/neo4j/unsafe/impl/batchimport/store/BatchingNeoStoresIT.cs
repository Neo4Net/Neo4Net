﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.store
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using EnterpriseGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using MetricsExtension = Org.Neo4j.metrics.MetricsExtension;
	using MetricsSettings = Org.Neo4j.metrics.MetricsSettings;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class BatchingNeoStoresIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

		 private FileSystemAbstraction _fileSystem;
		 private File _databaseDirectory;
		 private AssertableLogProvider _provider;
		 private SimpleLogService _logService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fileSystem = FileSystemRule.get();
			  _databaseDirectory = TestDirectory.databaseDir();
			  _provider = new AssertableLogProvider();
			  _logService = new SimpleLogService( _provider, _provider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startBatchingNeoStoreWithMetricsPluginEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartBatchingNeoStoreWithMetricsPluginEnabled()
		 {
			  Config config = Config.defaults( MetricsSettings.metricsEnabled, "true" );
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), BatchingNeoStores batchingNeoStores = BatchingNeoStores.BatchingNeoStoresConflict(_fileSystem, _databaseDirectory, RecordFormatSelector.defaultFormat(), Configuration.DEFAULT, _logService, AdditionalInitialIds.EMPTY, config, jobScheduler) )
			  {
					batchingNeoStores.CreateNew();
			  }
			  _provider.assertNone( AssertableLogProvider.inLog( typeof( MetricsExtension ) ).any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createStoreWithNotEmptyInitialIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateStoreWithNotEmptyInitialIds()
		 {
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), BatchingNeoStores batchingNeoStores = BatchingNeoStores.BatchingNeoStoresConflict(_fileSystem, _databaseDirectory, RecordFormatSelector.defaultFormat(), Configuration.DEFAULT, _logService, new TestAdditionalInitialIds(), Config.defaults(), jobScheduler) )
			  {
					batchingNeoStores.CreateNew();
			  }

			  GraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(_databaseDirectory);
			  try
			  {
					TransactionIdStore transactionIdStore = GetTransactionIdStore( ( GraphDatabaseAPI ) database );
					assertEquals( 10, transactionIdStore.LastCommittedTransactionId );
			  }
			  finally
			  {
					database.Shutdown();
			  }
		 }

		 private static TransactionIdStore GetTransactionIdStore( GraphDatabaseAPI database )
		 {
			  DependencyResolver resolver = database.DependencyResolver;
			  return resolver.ResolveDependency( typeof( TransactionIdStore ) );
		 }

		 private class TestAdditionalInitialIds : AdditionalInitialIds
		 {
			  public override long LastCommittedTransactionId()
			  {
					return 10;
			  }

			  public override long LastCommittedTransactionChecksum()
			  {
					return 11;
			  }

			  public override long LastCommittedTransactionLogVersion()
			  {
					return 12;
			  }

			  public override long LastCommittedTransactionLogByteOffset()
			  {
					return 13;
			  }
		 }
	}

}