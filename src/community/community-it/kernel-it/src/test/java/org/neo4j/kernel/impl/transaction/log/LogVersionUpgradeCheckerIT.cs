/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using NestedThrowableMatcher = Neo4Net.Test.matchers.NestedThrowableMatcher;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;

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
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(Neo4Net.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
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
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(Neo4Net.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "false").newGraphDatabase();
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
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).setConfig(Neo4Net.graphdb.factory.GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).setConfig(GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
			  Db.shutdown();
		 }

		 private void CreateGraphDbAndKillIt()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fs).newImpermanentDatabaseBuilder(storeDirectory.databaseDir()).newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs).newImpermanentDatabaseBuilder(_storeDirectory.databaseDir()).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label( "FOO" ) );
					Db.createNode( label( "BAR" ) );
					tx.Success();
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createStoreWithLogEntryVersion(Neo4Net.kernel.impl.transaction.log.entry.LogEntryVersion logEntryVersion) throws Exception
		 private void CreateStoreWithLogEntryVersion( LogEntryVersion logEntryVersion )
		 {
			  CreateGraphDbAndKillIt();
			  AppendCheckpoint( logEntryVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendCheckpoint(Neo4Net.kernel.impl.transaction.log.entry.LogEntryVersion logVersion) throws java.io.IOException
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