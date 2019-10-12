using System;

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
namespace Org.Neo4j.upgrade
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ConfigurableStandalonePageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using HighLimit = Org.Neo4j.Kernel.impl.store.format.highlimit.HighLimit;
	using HighLimitV3_0_0 = Org.Neo4j.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using UnexpectedUpgradingStoreFormatException = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader.UnexpectedUpgradingStoreFormatException;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

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
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(_fileSystemRule.get(), config, jobScheduler) )
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