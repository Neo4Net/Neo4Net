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
namespace Neo4Net.Locking
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactoryState = Neo4Net.Graphdb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using LockAcquisitionTimeoutException = Neo4Net.Kernel.impl.locking.LockAcquisitionTimeoutException;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using CommunityLockClient = Neo4Net.Kernel.impl.locking.community.CommunityLockClient;
	using CommunityLockManger = Neo4Net.Kernel.impl.locking.community.CommunityLockManger;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using Neo4Net.Test;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Neo4Net.Test.mockito.matcher;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class CommunityLockAcquisitionTimeoutIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public static readonly TestDirectory Directory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

		 private readonly OtherThreadExecutor<Void> _secondTransactionExecutor = new OtherThreadExecutor<Void>( "transactionExecutor", null );
		 private readonly OtherThreadExecutor<Void> _clockExecutor = new OtherThreadExecutor<Void>( "clockExecutor", null );

		 private const int TEST_TIMEOUT = 5000;
		 private const string TEST_PROPERTY_NAME = "a";
		 private static readonly Label _marker = Label.label( "marker" );
		 private static readonly FakeClock _fakeClock = Clocks.fakeClock();

		 private static GraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp()
		 public static void SetUp()
		 {
			  CustomClockFacadeFactory facadeFactory = new CustomClockFacadeFactory();
			  _database = ( new CustomClockTestGraphDatabaseFactory( facadeFactory ) ).NewEmbeddedDatabaseBuilder( Directory.storeDir() ).setConfig(GraphDatabaseSettings.lock_acquisition_timeout, "2s").setConfig("dbms.backup.enabled", "false").newGraphDatabase();

			  CreateTestNode( _marker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownClass()
		 public static void TearDownClass()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _secondTransactionExecutor.Dispose();
			  _clockExecutor.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void timeoutOnAcquiringExclusiveLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TimeoutOnAcquiringExclusiveLock()
		 {
			  ExpectedException.expect( new RootCauseMatcher<>( typeof( LockAcquisitionTimeoutException ), "The transaction has been terminated. " + "Retry your operation in a new transaction, and you should see a successful result. " + "Unable to acquire lock within configured timeout (dbms.lock.acquisition.timeout). " + "Unable to acquire lock for resource: NODE with id: 0 within 2000 millis." ) );

			  using ( Transaction ignored = _database.beginTx() )
			  {
					ResourceIterator<Node> nodes = _database.findNodes( _marker );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node node = nodes.next();
					node.SetProperty( TEST_PROPERTY_NAME, "b" );

					Future<Void> propertySetFuture = _secondTransactionExecutor.executeDontWait(state =>
					{
					 using ( Transaction transaction1 = _database.beginTx() )
					 {
						  node.SetProperty( TEST_PROPERTY_NAME, "b" );
						  transaction1.success();
					 }
					 return null;
					});

					_secondTransactionExecutor.waitUntilWaiting( ExclusiveLockWaitingPredicate() );
					_clockExecutor.execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
					{
					 _fakeClock.forward( 3, TimeUnit.SECONDS );
					 return null;
					});
					propertySetFuture.get();

					fail( "Should throw termination exception." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void timeoutOnAcquiringSharedLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TimeoutOnAcquiringSharedLock()
		 {
			  ExpectedException.expect( new RootCauseMatcher<>( typeof( LockAcquisitionTimeoutException ), "The transaction has been terminated. " + "Retry your operation in a new transaction, and you should see a successful result. " + "Unable to acquire lock within configured timeout (dbms.lock.acquisition.timeout). " + "Unable to acquire lock for resource: LABEL with id: 1 within 2000 millis." ) );

			  using ( Transaction ignored = _database.beginTx() )
			  {
					Locks lockManger = LockManager;
					lockManger.NewClient().acquireExclusive(LockTracer.NONE, ResourceTypes.LABEL, 1);

					Future<Void> propertySetFuture = _secondTransactionExecutor.executeDontWait(state =>
					{
					 using ( Transaction nestedTransaction = _database.beginTx() )
					 {
						  ResourceIterator<Node> nodes = _database.findNodes( _marker );
						  Node node = nodes.next();
						  node.addLabel( Label.label( "anotherLabel" ) );
						  nestedTransaction.success();
					 }
					 return null;
					});

					_secondTransactionExecutor.waitUntilWaiting( SharedLockWaitingPredicate() );
					_clockExecutor.execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
					{
					 _fakeClock.forward( 3, TimeUnit.SECONDS );
					 return null;
					});
					propertySetFuture.get();

					fail( "Should throw termination exception." );
			  }
		 }

		 protected internal virtual Locks LockManager
		 {
			 get
			 {
				  return DependencyResolver.resolveDependency( typeof( CommunityLockManger ) );
			 }
		 }

		 protected internal virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return ( ( GraphDatabaseAPI ) _database ).DependencyResolver;
			 }
		 }

		 protected internal virtual System.Predicate<OtherThreadExecutor.WaitDetails> ExclusiveLockWaitingPredicate()
		 {
			  return waitDetails => waitDetails.isAt( typeof( CommunityLockClient ), "acquireExclusive" );
		 }

		 protected internal virtual System.Predicate<OtherThreadExecutor.WaitDetails> SharedLockWaitingPredicate()
		 {
			  return waitDetails => waitDetails.isAt( typeof( CommunityLockClient ), "acquireShared" );
		 }

		 private static void CreateTestNode( Label marker )
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.createNode( marker );
					transaction.Success();
			  }
		 }

		 private class CustomClockTestGraphDatabaseFactory : TestGraphDatabaseFactory
		 {
			  internal GraphDatabaseFacadeFactory CustomFacadeFactory;

			  internal CustomClockTestGraphDatabaseFactory( GraphDatabaseFacadeFactory customFacadeFactory )
			  {
					this.CustomFacadeFactory = customFacadeFactory;
			  }

			  protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
			  {
					return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			  }

			  private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			  {
				  private readonly CustomClockTestGraphDatabaseFactory _outerInstance;

				  private File _storeDir;
				  private GraphDatabaseFactoryState _state;

				  public DatabaseCreatorAnonymousInnerClass( CustomClockTestGraphDatabaseFactory outerInstance, File storeDir, GraphDatabaseFactoryState state )
				  {
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._state = state;
				  }

				  public GraphDatabaseService newDatabase( Config config )
				  {
						return _outerInstance.customFacadeFactory.newFacade( _storeDir, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
				  }
			  }
		 }

		 private class CustomClockFacadeFactory : GraphDatabaseFacadeFactory
		 {

			  internal CustomClockFacadeFactory() : base(DatabaseInfo.COMMUNITY, CommunityEditionModule::new)
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  }

			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					return new PlatformModuleAnonymousInnerClass( this, storeDir, config, DatabaseInfo, dependencies );
			  }

			  private class PlatformModuleAnonymousInnerClass : PlatformModule
			  {
				  private readonly CustomClockFacadeFactory _outerInstance;

				  public PlatformModuleAnonymousInnerClass( CustomClockFacadeFactory outerInstance, File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override SystemNanoClock createClock()
				  {
						return _fakeClock;
				  }
			  }

		 }
	}

}