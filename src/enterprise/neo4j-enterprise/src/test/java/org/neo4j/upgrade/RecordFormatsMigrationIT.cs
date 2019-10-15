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
namespace Neo4Net.upgrade
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ConfigurableStandalonePageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using HighLimitV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using UnexpectedUpgradingStoreFormatException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UnexpectedUpgradingStoreFormatException;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class RecordFormatsMigrationIT
	{
		private bool InstanceFieldsInitialized = false;

		public RecordFormatsMigrationIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fileSystemRule.get() );
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule );
		}

		 private static readonly Label _label = Label.label( "Centipede" );
		 private const string PROPERTY = "legs";
		 private const int VALUE = 42;

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateLatestStandardToLatestHighLimit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrateLatestStandardToLatestHighLimit()
		 {
			  ExecuteAndStopDb( StartStandardFormatDb(), RecordFormatsMigrationIT.createNode );
			  AssertLatestStandardStore();

			  ExecuteAndStopDb( StartHighLimitFormatDb(), RecordFormatsMigrationIT.assertNodeExists );
			  AssertLatestHighLimitStore();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateHighLimitV3_0ToLatestHighLimit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrateHighLimitV3_0ToLatestHighLimit()
		 {
			  ExecuteAndStopDb( StartDb( HighLimitV3_0_0.NAME ), RecordFormatsMigrationIT.createNode );
			  AssertStoreFormat( HighLimitV3_0_0.RECORD_FORMATS );

			  ExecuteAndStopDb( StartHighLimitFormatDb(), RecordFormatsMigrationIT.assertNodeExists );
			  AssertLatestHighLimitStore();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateHighLimitToStandard() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrateHighLimitToStandard()
		 {
			  ExecuteAndStopDb( StartHighLimitFormatDb(), RecordFormatsMigrationIT.createNode );
			  AssertLatestHighLimitStore();

			  try
			  {
					StartStandardFormatDb();
					fail( "Should not be possible to downgrade" );
			  }
			  catch ( Exception e )
			  {
					assertThat( Exceptions.rootCause( e ), instanceOf( typeof( UnexpectedUpgradingStoreFormatException ) ) );
			  }
			  AssertLatestHighLimitStore();
		 }

		 private static void CreateNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode( _label );
					start.SetProperty( PROPERTY, VALUE );
					tx.Success();
			  }
		 }

		 private static void AssertNodeExists( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertNotNull( Db.findNode( _label, PROPERTY, VALUE ) );
					tx.Success();
			  }
		 }

		 private GraphDatabaseService StartStandardFormatDb()
		 {
			  return StartDb( Standard.LATEST_NAME );
		 }

		 private GraphDatabaseService StartHighLimitFormatDb()
		 {
			  return StartDb( HighLimit.NAME );
		 }

		 private GraphDatabaseService StartDb( string recordFormatName )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_testDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, recordFormatName).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLatestStandardStore() throws Exception
		 private void AssertLatestStandardStore()
		 {
			  AssertStoreFormat( Standard.LATEST_RECORD_FORMATS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLatestHighLimitStore() throws Exception
		 private void AssertLatestHighLimitStore()
		 {
			  AssertStoreFormat( HighLimit.RECORD_FORMATS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertStoreFormat(org.neo4j.kernel.impl.store.format.RecordFormats expected) throws Exception
		 private void AssertStoreFormat( RecordFormats expected )
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );
			  using ( IJobScheduler jobScheduler = new ThreadPoolJobScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(_fileSystemRule.get(), config, jobScheduler) )
			  {
					RecordFormats actual = RecordFormatSelector.selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fileSystemRule, pageCache, NullLogProvider.Instance );
					assertNotNull( actual );
					assertEquals( expected.StoreVersion(), actual.StoreVersion() );
			  }
		 }

		 private static void ExecuteAndStopDb( GraphDatabaseService db, System.Action<GraphDatabaseService> action )
		 {
			  try
			  {
					action( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}