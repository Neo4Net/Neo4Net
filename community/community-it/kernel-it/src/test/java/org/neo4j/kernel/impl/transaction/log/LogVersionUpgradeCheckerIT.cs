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
namespace Org.Neo4j.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LogEntryVersion = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using NestedThrowableMatcher = Org.Neo4j.Test.matchers.NestedThrowableMatcher;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;

	public class LogVersionUpgradeCheckerIT
	{
		private bool InstanceFieldsInitialized = false;

		public LogVersionUpgradeCheckerIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _storeDirectory ).around( _fs ).around( _pageCacheRule );
		}

		 private readonly TestDirectory _storeDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(storeDirectory).around(fs).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startAsNormalWhenUpgradeIsNotAllowed()
		 public virtual void StartAsNormalWhenUpgradeIsNotAllowed()
		 {
			  CreateGraphDbAndKillIt();

			  // Try to start with upgrading disabled
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failToStartFromOlderTransactionLogsIfNotAllowed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToStartFromOlderTransactionLogsIfNotAllowed()
		 {
			  CreateStoreWithLogEntryVersion( LogEntryVersion.V2_3 );

			  Expect.expect( new NestedThrowableMatcher( typeof( UpgradeNotAllowedByConfigurationException ) ) );

			  // Try to start with upgrading disabled
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startFromOlderTransactionLogsIfAllowed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartFromOlderTransactionLogsIfAllowed()
		 {
			  CreateStoreWithLogEntryVersion( LogEntryVersion.V2_3 );

			  // Try to start with upgrading enabled
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
			  Db.shutdown();
		 }

		 private void CreateGraphDbAndKillIt()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label( "FOO" ) );
					Db.createNode( label( "BAR" ) );
					tx.Success();
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createStoreWithLogEntryVersion(org.neo4j.kernel.impl.transaction.log.entry.LogEntryVersion logEntryVersion) throws Exception
		 private void CreateStoreWithLogEntryVersion( LogEntryVersion logEntryVersion )
		 {
			  CreateGraphDbAndKillIt();
			  AppendCheckpoint( logEntryVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendCheckpoint(org.neo4j.kernel.impl.transaction.log.entry.LogEntryVersion logVersion) throws java.io.IOException
		 private void AppendCheckpoint( LogEntryVersion logVersion )
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( _storeDirectory.databaseLayout(), _fs, pageCache ).withLogEntryReader(logEntryReader).build();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  LogTailScanner.LogTailInformation tailInformation = tailScanner.TailInformation;

			  using ( Lifespan lifespan = new Lifespan( logFiles ) )
			  {
					FlushablePositionAwareChannel channel = logFiles.LogFile.Writer;

					LogPosition logPosition = tailInformation.LastCheckPoint.LogPosition;

					// Fake record
					channel.Put( logVersion.byteCode() ).put(CHECK_POINT).putLong(logPosition.LogVersion).putLong(logPosition.ByteOffset);

					channel.PrepareForFlush().flush();
			  }
		 }
	}

}