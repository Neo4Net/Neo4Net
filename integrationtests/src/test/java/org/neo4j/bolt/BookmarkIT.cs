using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using Transaction = Org.Neo4j.driver.v1.Transaction;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class BookmarkIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(getClass());
		 public readonly TestDirectory Directory = TestDirectory.testDirectory( this.GetType() );

		 private Driver _driver;
		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  IOUtils.closeAllSilently( _driver );
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUpToDateBookmarkWhenSomeTransactionIsCommitting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnUpToDateBookmarkWhenSomeTransactionIsCommitting()
		 {
			  CommitBlocker commitBlocker = new CommitBlocker();
			  _db = CreateDb( commitBlocker );
			  _driver = GraphDatabase.driver( BoltAddress( _db ) );

			  string firstBookmark = CreateNode( _driver );

			  // make next transaction append to the log and then pause before applying to the store
			  // this makes it allocate a transaction ID but wait before acknowledging the commit operation
			  commitBlocker.BlockNextTransaction();
			  CompletableFuture<string> secondBookmarkFuture = CompletableFuture.supplyAsync( () => CreateNode(_driver) );
			  assertEventually( "Transaction did not block as expected", commitBlocker.hasBlockedTransaction, @is( true ), 1, MINUTES );

			  ISet<string> otherBookmarks = Stream.generate( () => CreateNode(_driver) ).limit(10).collect(toSet());

			  commitBlocker.Unblock();
			  string lastBookmark = secondBookmarkFuture.get();

			  // first and last bookmarks should not be null and should be different
			  assertNotNull( firstBookmark );
			  assertNotNull( lastBookmark );
			  assertNotEquals( firstBookmark, lastBookmark );

			  // all bookmarks received while a transaction was blocked committing should be unique
			  assertThat( otherBookmarks, hasSize( 10 ) );
		 }

		 private GraphDatabaseAPI CreateDb( CommitBlocker commitBlocker )
		 {
			  return CreateDb( platformModule => new CustomCommunityEditionModule( platformModule, commitBlocker ) );
		 }

		 private GraphDatabaseAPI CreateDb( System.Func<PlatformModule, AbstractEditionModule> editionModuleFactory )
		 {
			  GraphDatabaseFactoryState state = new GraphDatabaseFactoryState();
			  GraphDatabaseFacadeFactory facadeFactory = new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, editionModuleFactory );
			  return facadeFactory.NewFacade( Directory.databaseDir(), ConfigWithBoltEnabled(), state.DatabaseDependencies() );
		 }

		 private static string CreateNode( Driver driver )
		 {
			  using ( Session session = driver.session() )
			  {
					using ( Transaction tx = session.beginTransaction() )
					{
						 tx.run( "CREATE ()" );
						 tx.success();
					}
					return session.lastBookmark();
			  }
		 }

		 private static Config ConfigWithBoltEnabled()
		 {
			  Config config = Config.defaults();

			  config.augment( "dbms.connector.bolt.enabled", TRUE );
			  config.Augment( "dbms.connector.bolt.listen_address", "localhost:0" );

			  return config;
		 }

		 private static string BoltAddress( GraphDatabaseAPI db )
		 {
			  ConnectorPortRegister portRegister = Db.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
			  return "bolt://" + portRegister.GetLocalAddress( "bolt" );
		 }

		 private class CustomCommunityEditionModule : CommunityEditionModule
		 {
			  internal CustomCommunityEditionModule( PlatformModule platformModule, CommitBlocker commitBlocker ) : base( platformModule )
			  {
					CommitProcessFactoryConflict = new CustomCommitProcessFactory( commitBlocker );
			  }
		 }

		 private class CustomCommitProcessFactory : CommitProcessFactory
		 {
			  internal readonly CommitBlocker CommitBlocker;

			  internal CustomCommitProcessFactory( CommitBlocker commitBlocker )
			  {
					this.CommitBlocker = commitBlocker;
			  }

			  public override TransactionCommitProcess Create( TransactionAppender appender, StorageEngine storageEngine, Config config )
			  {
					return new CustomCommitProcess( appender, storageEngine, CommitBlocker );
			  }
		 }

		 private class CustomCommitProcess : TransactionRepresentationCommitProcess
		 {
			  internal readonly CommitBlocker CommitBlocker;

			  internal CustomCommitProcess( TransactionAppender appender, StorageEngine storageEngine, CommitBlocker commitBlocker ) : base( appender, storageEngine )
			  {
					this.CommitBlocker = commitBlocker;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void applyToStore(org.neo4j.kernel.impl.api.TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  protected internal override void ApplyToStore( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
			  {
					CommitBlocker.blockWhileWritingToStoreIfNeeded();
					base.ApplyToStore( batch, commitEvent, mode );
			  }
		 }

		 private class CommitBlocker
		 {
			  internal readonly ReentrantLock Lock = new ReentrantLock();
			  internal volatile bool ShouldBlock;

			  internal virtual void BlockNextTransaction()
			  {
					ShouldBlock = true;
					Lock.@lock();
			  }

			  internal virtual void BlockWhileWritingToStoreIfNeeded()
			  {
					if ( ShouldBlock )
					{
						 ShouldBlock = false;
						 Lock.@lock();
					}
			  }

			  internal virtual void Unblock()
			  {
					Lock.unlock();
			  }

			  internal virtual bool HasBlockedTransaction()
			  {
					return Lock.QueueLength == 1;
			  }
		 }
	}

}