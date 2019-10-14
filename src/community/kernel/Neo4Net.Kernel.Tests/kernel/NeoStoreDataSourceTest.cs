using System;

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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Logger = Neo4Net.Logging.Logger;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using NeoStoreDataSourceRule = Neo4Net.Test.rule.NeoStoreDataSourceRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class NeoStoreDataSourceTest
	{
		private bool InstanceFieldsInitialized = false;

		public NeoStoreDataSourceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Dir = TestDirectory.testDirectory( Fs.get() );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory(fs.get());
		 public TestDirectory Dir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.NeoStoreDataSourceRule dsRule = new org.neo4j.test.rule.NeoStoreDataSourceRule();
		 public NeoStoreDataSourceRule DsRule = new NeoStoreDataSourceRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseHealthShouldBeHealedOnStart() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseHealthShouldBeHealedOnStart()
		 {
			  NeoStoreDataSource theDataSource = null;
			  try
			  {
					DatabaseHealth databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLogProvider.Instance.getLog( typeof( DatabaseHealth ) ) );
					Dependencies dependencies = new Dependencies();
					dependencies.SatisfyDependency( databaseHealth );

					theDataSource = DsRule.getDataSource( Dir.databaseLayout(), Fs.get(), PageCacheRule.getPageCache(Fs.get()), dependencies );

					databaseHealth.Panic( new Exception() );

					theDataSource.Start();

					databaseHealth.AssertHealthy( typeof( Exception ) );
			  }
			  finally
			  {
					if ( theDataSource != null )
					{
						 theDataSource.Stop();
						 theDataSource.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushOfThePageCacheHappensOnlyOnceDuringShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlushOfThePageCacheHappensOnlyOnceDuringShutdown()
		 {
			  PageCache pageCache = spy( PageCacheRule.getPageCache( Fs.get() ) );
			  NeoStoreDataSource ds = DsRule.getDataSource( Dir.databaseLayout(), Fs.get(), pageCache );

			  ds.Start();
			  verify( pageCache, never() ).flushAndForce();
			  verify( pageCache, never() ).flushAndForce(any(typeof(IOLimiter)));

			  ds.Stop();
			  ds.Shutdown();
			  verify( pageCache ).flushAndForce( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushOfThePageCacheOnShutdownHappensIfTheDbIsHealthy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlushOfThePageCacheOnShutdownHappensIfTheDbIsHealthy()
		 {
			  PageCache pageCache = spy( PageCacheRule.getPageCache( Fs.get() ) );

			  NeoStoreDataSource ds = DsRule.getDataSource( Dir.databaseLayout(), Fs.get(), pageCache );

			  ds.Start();
			  verify( pageCache, never() ).flushAndForce();

			  ds.Stop();
			  ds.Shutdown();
			  verify( pageCache ).flushAndForce( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushOfThePageCacheOnShutdownDoesNotHappenIfTheDbIsUnhealthy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlushOfThePageCacheOnShutdownDoesNotHappenIfTheDbIsUnhealthy()
		 {
			  DatabaseHealth health = mock( typeof( DatabaseHealth ) );
			  when( health.Healthy ).thenReturn( false );
			  PageCache pageCache = spy( PageCacheRule.getPageCache( Fs.get() ) );

			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( health );
			  NeoStoreDataSource ds = DsRule.getDataSource( Dir.databaseLayout(), Fs.get(), pageCache, dependencies );

			  ds.Start();
			  verify( pageCache, never() ).flushAndForce();

			  ds.Stop();
			  ds.Shutdown();
			  verify( pageCache, never() ).flushAndForce(Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCorrectTransactionLogDiagnosticsForNoTransactionLogs()
		 public virtual void ShouldLogCorrectTransactionLogDiagnosticsForNoTransactionLogs()
		 {
			  // GIVEN
			  NeoStoreDataSource dataSource = NeoStoreDataSourceWithLogFilesContainingLowestTxId( NoLogs() );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Logger logger = logProvider.getLog( this.GetType() ).infoLogger();

			  // WHEN
			  DataSourceDiagnostics.TransactionRange.dump( dataSource, logger );

			  // THEN
			  logProvider.RawMessageMatcher().assertContains("No transactions");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCorrectTransactionLogDiagnosticsForTransactionsInOldestLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogCorrectTransactionLogDiagnosticsForTransactionsInOldestLog()
		 {
			  // GIVEN
			  long logVersion = 2;
			  long prevLogLastTxId = 45;
			  NeoStoreDataSource dataSource = NeoStoreDataSourceWithLogFilesContainingLowestTxId( LogWithTransactions( logVersion, prevLogLastTxId ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Logger logger = logProvider.getLog( this.GetType() ).infoLogger();

			  // WHEN
			  DataSourceDiagnostics.TransactionRange.dump( dataSource, logger );

			  // THEN
			  logProvider.RawMessageMatcher().assertContains("transaction " + (prevLogLastTxId + 1));
			  logProvider.RawMessageMatcher().assertContains("version " + logVersion);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCorrectTransactionLogDiagnosticsForTransactionsInSecondOldestLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogCorrectTransactionLogDiagnosticsForTransactionsInSecondOldestLog()
		 {
			  // GIVEN
			  long logVersion = 2;
			  long prevLogLastTxId = 45;
			  NeoStoreDataSource dataSource = NeoStoreDataSourceWithLogFilesContainingLowestTxId( LogWithTransactionsInNextToOldestLog( logVersion, prevLogLastTxId ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Logger logger = logProvider.getLog( this.GetType() ).infoLogger();

			  // WHEN
			  DataSourceDiagnostics.TransactionRange.dump( dataSource, logger );

			  // THEN
			  logProvider.RawMessageMatcher().assertContains("transaction " + (prevLogLastTxId + 1));
			  logProvider.RawMessageMatcher().assertContains("version " + (logVersion + 1));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logModuleSetUpError()
		 public virtual void LogModuleSetUpError()
		 {
			  Config config = Config.defaults();
			  IdGeneratorFactory idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
			  Exception openStoresError = new Exception( "Can't set up modules" );
			  doThrow( openStoresError ).when( idGeneratorFactory ).create( any( typeof( File ) ), anyLong(), anyBoolean() );

			  CommunityIdTypeConfigurationProvider idTypeConfigurationProvider = new CommunityIdTypeConfigurationProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  SimpleLogService logService = new SimpleLogService( logProvider, logProvider );
			  PageCache pageCache = PageCacheRule.getPageCache( Fs.get() );
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependencies( idGeneratorFactory, idTypeConfigurationProvider, config, logService );

			  NeoStoreDataSource dataSource = DsRule.getDataSource( Dir.databaseLayout(), Fs.get(), pageCache, dependencies );

			  try
			  {
					dataSource.Start();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertEquals( openStoresError, e );
			  }

			  logProvider.AssertAtLeastOnce( inLog( typeof( NeoStoreDataSource ) ).warn( equalTo( "Exception occurred while setting up store modules. Attempting to close things down." ), equalTo( openStoresError ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAlwaysShutdownLifeEvenWhenCheckPointingFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAlwaysShutdownLifeEvenWhenCheckPointingFails()
		 {
			  // Given
			  FileSystemAbstraction fs = this.Fs.get();
			  PageCache pageCache = PageCacheRule.getPageCache( fs );
			  DatabaseHealth databaseHealth = mock( typeof( DatabaseHealth ) );
			  when( databaseHealth.Healthy ).thenReturn( true );
			  IOException ex = new IOException( "boom!" );
			  doThrow( ex ).when( databaseHealth ).assertHealthy( typeof( IOException ) ); // <- this is a trick to simulate a failure during checkpointing
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependencies( databaseHealth );
			  NeoStoreDataSource dataSource = DsRule.getDataSource( Dir.databaseLayout(), fs, pageCache, dependencies );
			  dataSource.Start();

			  try
			  {
					// When
					dataSource.Stop();
					fail( "it should have thrown" );
			  }
			  catch ( LifecycleException e )
			  {
					// Then
					assertEquals( ex, e.InnerException );
			  }
		 }

		 private static NeoStoreDataSource NeoStoreDataSourceWithLogFilesContainingLowestTxId( LogFiles files )
		 {
			  DependencyResolver resolver = mock( typeof( DependencyResolver ) );
			  when( resolver.ResolveDependency( typeof( LogFiles ) ) ).thenReturn( files );
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.DependencyResolver ).thenReturn( resolver );
			  return dataSource;
		 }

		 private static LogFiles NoLogs()
		 {
			  LogFiles files = mock( typeof( TransactionLogFiles ) );
			  when( Files.LowestLogVersion ).thenReturn( -1L );
			  return files;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.impl.transaction.log.files.LogFiles logWithTransactions(long logVersion, long headerTxId) throws java.io.IOException
		 private static LogFiles LogWithTransactions( long logVersion, long headerTxId )
		 {
			  LogFiles files = mock( typeof( TransactionLogFiles ) );
			  when( Files.LowestLogVersion ).thenReturn( logVersion );
			  when( Files.hasAnyEntries( logVersion ) ).thenReturn( true );
			  when( Files.versionExists( logVersion ) ).thenReturn( true );
			  when( Files.extractHeader( logVersion ) ).thenReturn( new LogHeader( LogEntryVersion.CURRENT.byteCode(), logVersion, headerTxId ) );
			  return files;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.impl.transaction.log.files.LogFiles logWithTransactionsInNextToOldestLog(long logVersion, long prevLogLastTxId) throws java.io.IOException
		 private static LogFiles LogWithTransactionsInNextToOldestLog( long logVersion, long prevLogLastTxId )
		 {
			  LogFiles files = LogWithTransactions( logVersion + 1, prevLogLastTxId );
			  when( Files.LowestLogVersion ).thenReturn( logVersion );
			  when( Files.hasAnyEntries( logVersion ) ).thenReturn( false );
			  when( Files.versionExists( logVersion ) ).thenReturn( true );
			  return files;
		 }
	}

}