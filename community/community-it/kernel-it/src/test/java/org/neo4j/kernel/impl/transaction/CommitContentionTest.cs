using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.transaction
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

	public class CommitContentionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory storeLocation = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory StoreLocation = TestDirectory.testDirectory();

		 internal readonly Semaphore Semaphore1 = new Semaphore( 1 );
		 internal readonly Semaphore Semaphore2 = new Semaphore( 1 );
		 internal readonly AtomicReference<Exception> Reference = new AtomicReference<Exception>();

		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  Semaphore1.acquire();
			  Semaphore2.acquire();
			  _db = CreateDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotContendOnCommitWhenPushingUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotContendOnCommitWhenPushingUpdates()
		 {
			  Thread thread = StartFirstTransactionWhichBlocksDuringPushUntilSecondTransactionFinishes();

			  RunAndFinishSecondTransaction();

			  thread.Join();

			  AssertNoFailures();
		 }

		 private void AssertNoFailures()
		 {
			  Exception e = Reference.get();

			  if ( e != null )
			  {
					throw new AssertionError( e );
			  }
		 }

		 private void RunAndFinishSecondTransaction()
		 {
			  CreateNode();

			  SignalSecondTransactionFinished();
		 }

		 private void CreateNode()
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					_db.createNode();
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Thread startFirstTransactionWhichBlocksDuringPushUntilSecondTransactionFinishes() throws InterruptedException
		 private Thread StartFirstTransactionWhichBlocksDuringPushUntilSecondTransactionFinishes()
		 {
			  Thread thread = new Thread( this.createNode );

			  thread.Start();

			  WaitForFirstTransactionToStartPushing();

			  return thread;
		 }

		 private GraphDatabaseService CreateDb()
		 {
			  GraphDatabaseFactoryState state = new GraphDatabaseFactoryState();
			  return new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, platformModule => new CommunityEditionModuleAnonymousInnerClass( this, platformModule ) )}private void waitForFirstTransactionToStartPushing() throws InterruptedException{if (!Semaphore1.tryAcquire(10, SECONDS)){throw new System.InvalidOperationException("First transaction never started pushing");
			 .newFacade( StoreLocation.storeDir(), Config.defaults(), state.DatabaseDependencies() );
		 }

			  private class CommunityEditionModuleAnonymousInnerClass : CommunityEditionModule
			  {
				  private readonly CommitContentionTest _outerInstance;

				  public CommunityEditionModuleAnonymousInnerClass( CommitContentionTest outerInstance, UnknownType platformModule ) : base( platformModule )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override DatabaseTransactionStats createTransactionMonitor()
				  {
						return new SkipTransactionDatabaseStats( _outerInstance );
				  }
			  }
	}

		 private void signalFirstTransactionStartedPushing()
		 {
			  semaphore1.release();
		 }

		 private void signalSecondTransactionFinished()
		 {
			  semaphore2.release();
		 }

		 private void waitForSecondTransactionToFinish()
		 {
			  try
			  {
					bool acquired = semaphore2.tryAcquire( 10, SECONDS );

					if ( !acquired )
					{
						 reference.set( new System.InvalidOperationException( "Second transaction never finished" ) );
					}
			  }
			  catch ( InterruptedException e )
			  {
					reference.set( e );
			  }
		 }

		 private class SkipTransactionDatabaseStats : DatabaseTransactionStats
		 {
			  internal bool skip;

			  public override void transactionFinished( bool committed, bool write )
			  {
					base.transactionFinished( committed, write );

					if ( committed )
					{
						 // skip signal and waiting for second transaction
						 if ( skip )
						 {
							  return;
						 }
						 skip = true;

						 signalFirstTransactionStartedPushing();

						 waitForSecondTransactionToFinish();
					}
			  }
		 }
}

}