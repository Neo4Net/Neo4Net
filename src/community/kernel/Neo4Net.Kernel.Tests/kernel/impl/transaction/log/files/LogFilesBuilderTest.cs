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
namespace Neo4Net.Kernel.impl.transaction.log.files
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ByteUnit = Neo4Net.Io.ByteUnit;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.activeFilesBuilder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.builder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.logFilesBasedOnlyBuilder;

	public class LogFilesBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private File _storeDirectory;
		 private DefaultFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _storeDirectory = TestDirectory.directory();
			  _fileSystem = FileSystemRule.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildActiveFilesOnlyContext() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildActiveFilesOnlyContext()
		 {
			  PageCache pageCache = PageCacheRule.getPageCache( _fileSystem );
			  TransactionLogFilesContext context = activeFilesBuilder( TestDirectory.databaseLayout(), _fileSystem, pageCache ).buildContext();

			  assertEquals( _fileSystem, context.FileSystem );
			  assertNotNull( context.LogEntryReader );
			  assertSame( LogFileCreationMonitor_Fields.NoMonitor, context.LogFileCreationMonitor );
			  assertEquals( long.MaxValue, context.RotationThreshold.get() );
			  assertEquals( 0, context.LastCommittedTransactionId );
			  assertEquals( 0, context.LogVersionRepository.CurrentLogVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildFilesBasedContext() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildFilesBasedContext()
		 {
			  TransactionLogFilesContext context = logFilesBasedOnlyBuilder( _storeDirectory, _fileSystem ).buildContext();
			  assertEquals( _fileSystem, context.FileSystem );
			  assertSame( LogFileCreationMonitor_Fields.NoMonitor, context.LogFileCreationMonitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildDefaultContext() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildDefaultContext()
		 {
			  TransactionLogFilesContext context = builder( TestDirectory.databaseLayout(), _fileSystem ).withLogVersionRepository(new SimpleLogVersionRepository(2)).withTransactionIdStore(new SimpleTransactionIdStore()).buildContext();
			  assertEquals( _fileSystem, context.FileSystem );
			  assertNotNull( context.LogEntryReader );
			  assertSame( LogFileCreationMonitor_Fields.NoMonitor, context.LogFileCreationMonitor );
			  assertEquals( ByteUnit.mebiBytes( 250 ), context.RotationThreshold.get() );
			  assertEquals( 1, context.LastCommittedTransactionId );
			  assertEquals( 2, context.LogVersionRepository.CurrentLogVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildDefaultContextWithDependencies() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildDefaultContextWithDependencies()
		 {
			  SimpleLogVersionRepository logVersionRepository = new SimpleLogVersionRepository( 2 );
			  SimpleTransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( logVersionRepository );
			  dependencies.SatisfyDependency( transactionIdStore );

			  TransactionLogFilesContext context = builder( TestDirectory.databaseLayout(), _fileSystem ).withDependencies(dependencies).buildContext();

			  assertEquals( _fileSystem, context.FileSystem );
			  assertNotNull( context.LogEntryReader );
			  assertSame( LogFileCreationMonitor_Fields.NoMonitor, context.LogFileCreationMonitor );
			  assertEquals( ByteUnit.mebiBytes( 250 ), context.RotationThreshold.get() );
			  assertEquals( 1, context.LastCommittedTransactionId );
			  assertEquals( 2, context.LogVersionRepository.CurrentLogVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildContextWithCustomLogFilesLocations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildContextWithCustomLogFilesLocations()
		 {
			  string customLogLocation = "customLogLocation";
			  Config customLogLocationConfig = Config.defaults( logical_logs_location, customLogLocation );
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();
			  LogFiles logFiles = builder( databaseLayout, _fileSystem ).withConfig( customLogLocationConfig ).withLogVersionRepository( new SimpleLogVersionRepository() ).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  logFiles.Init();
			  logFiles.Start();

			  assertEquals( databaseLayout.File( customLogLocation ), logFiles.HighestLogFile.ParentFile );
			  logFiles.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void buildContextWithCustomAbsoluteLogFilesLocations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BuildContextWithCustomAbsoluteLogFilesLocations()
		 {
			  File customLogDirectory = TestDirectory.directory( "absoluteCustomLogDirectory" );
			  Config customLogLocationConfig = Config.defaults( logical_logs_location, customLogDirectory.AbsolutePath );
			  LogFiles logFiles = builder( TestDirectory.databaseLayout(), _fileSystem ).withConfig(customLogLocationConfig).withLogVersionRepository(new SimpleLogVersionRepository()).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  logFiles.Init();
			  logFiles.Start();

			  assertEquals( customLogDirectory, logFiles.HighestLogFile.ParentFile );
			  logFiles.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = NullPointerException.class) public void failToBuildFullContextWithoutLogVersionRepo() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToBuildFullContextWithoutLogVersionRepo()
		 {
			  builder( TestDirectory.databaseLayout(), _fileSystem ).withTransactionIdStore(new SimpleTransactionIdStore()).buildContext();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = NullPointerException.class) public void failToBuildFullContextWithoutTransactionIdStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToBuildFullContextWithoutTransactionIdStore()
		 {
			  builder( TestDirectory.databaseLayout(), _fileSystem ).withLogVersionRepository(new SimpleLogVersionRepository(2)).buildContext();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnsupportedOperationException.class) public void fileBasedOperationsContextFailOnLastCommittedTransactionIdAccess() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FileBasedOperationsContextFailOnLastCommittedTransactionIdAccess()
		 {
			  logFilesBasedOnlyBuilder( _storeDirectory, _fileSystem ).buildContext().LastCommittedTransactionId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnsupportedOperationException.class) public void fileBasedOperationsContextFailOnLogVersionRepositoryAccess() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FileBasedOperationsContextFailOnLogVersionRepositoryAccess()
		 {
			  logFilesBasedOnlyBuilder( _storeDirectory, _fileSystem ).buildContext().LogVersionRepository;
		 }
	}

}