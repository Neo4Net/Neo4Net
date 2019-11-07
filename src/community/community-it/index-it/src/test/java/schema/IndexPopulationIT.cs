using System.Collections.Generic;
using System.Threading;

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
namespace Schema
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using IndexPopulationJob = Neo4Net.Kernel.Impl.Api.index.IndexPopulationJob;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;

	public class IndexPopulationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final Neo4Net.test.rule.TestDirectory directory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public static readonly TestDirectory Directory = TestDirectory.testDirectory();

		 private const int TEST_TIMEOUT = 120_000;
		 private static IGraphDatabaseService _database;
		 private static ExecutorService _executorService;
		 private static AssertableLogProvider _logProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp()
		 public static void SetUp()
		 {
			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  _logProvider = new AssertableLogProvider( true );
			  factory.InternalLogProvider = _logProvider;
			  _database = factory.NewEmbeddedDatabase( Directory.storeDir() );
			  _executorService = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDown()
		 public static void TearDown()
		 {
			  _executorService.shutdown();
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void indexCreationDoNotBlockQueryExecutions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexCreationDoNotBlockQueryExecutions()
		 {
			  Label nodeLabel = Label.label( "nodeLabel" );
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.createNode( nodeLabel );
					transaction.Success();
			  }

			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.schema().indexFor(Label.label("testLabel")).on("testProperty").create();

					Future<Number> countFuture = _executorService.submit( CountNodes() );
					assertEquals( 1, countFuture.get().intValue() );

					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void createIndexesFromDifferentTransactionsWithoutBlocking() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexesFromDifferentTransactionsWithoutBlocking()
		 {
			  long numberOfIndexesBeforeTest = CountIndexes();
			  Label nodeLabel = Label.label( "nodeLabel2" );
			  string testProperty = "testProperty";
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.schema().indexFor(Label.label("testLabel2")).on(testProperty).create();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> creationFuture = executorService.submit(createIndexForLabelAndProperty(nodeLabel, testProperty));
					Future<object> creationFuture = _executorService.submit( CreateIndexForLabelAndProperty( nodeLabel, testProperty ) );
					creationFuture.get();
					transaction.Success();
			  }
			  WaitForOnlineIndexes();

			  assertEquals( numberOfIndexesBeforeTest + 2, CountIndexes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void indexCreationDoNotBlockWritesOnOtherLabel() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexCreationDoNotBlockWritesOnOtherLabel()
		 {
			  Label markerLabel = Label.label( "testLabel3" );
			  Label nodesLabel = Label.label( "testLabel4" );
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.schema().indexFor(markerLabel).on("testProperty").create();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> creation = executorService.submit(createNodeWithLabel(nodesLabel));
					Future<object> creation = _executorService.submit( CreateNodeWithLabel( nodesLabel ) );
					creation.get();

					transaction.Success();
			  }

			  using ( Transaction transaction = _database.beginTx() )
			  {
					using ( IResourceIterator<Node> nodes = _database.findNodes( nodesLabel ) )
					{
						 assertEquals( 1, Iterators.count( nodes ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shutdownDatabaseDuringIndexPopulations()
		 public virtual void ShutdownDatabaseDuringIndexPopulations()
		 {
			  AssertableLogProvider assertableLogProvider = new AssertableLogProvider( true );
			  File storeDir = Directory.directory( "shutdownDbTest" );
			  Label testLabel = Label.label( "testLabel" );
			  string propertyName = "testProperty";
			  IGraphDatabaseService shutDownDb = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(assertableLogProvider).newEmbeddedDatabase(storeDir);
			  PrePopulateDatabase( shutDownDb, testLabel, propertyName );

			  using ( Transaction transaction = shutDownDb.BeginTx() )
			  {
					shutDownDb.Schema().indexFor(testLabel).on(propertyName).create();
					transaction.Success();
			  }
			  shutDownDb.Shutdown();
			  assertableLogProvider.AssertNone( AssertableLogProvider.inLog( typeof( IndexPopulationJob ) ).anyError() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogPhaseTracker()
		 public virtual void MustLogPhaseTracker()
		 {
			  Label nodeLabel = Label.label( "testLabel5" );
			  string key = "key";
			  string value = "hej";
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.createNode( nodeLabel ).setProperty( key, value );
					transaction.Success();
			  }

			  // when
			  using ( Transaction tx = _database.beginTx() )
			  {
					_database.schema().indexFor(nodeLabel).on(key).create();
					tx.Success();
			  }
			  WaitForOnlineIndexes();

			  // then
			  using ( Transaction tx = _database.beginTx() )
			  {
					ResourceIterator<Node> nodes = _database.findNodes( nodeLabel, key, value );
					long nodeCount = Iterators.count( nodes );
					assertEquals( "expected exactly one hit in index but was ",1, nodeCount );
					nodes.Close();
					tx.Success();
			  }
			  AssertableLogProvider.LogMatcher matcher = inLog( typeof( IndexPopulationJob ) ).info( containsString( "TIME/PHASE Final:" ) );
			  _logProvider.assertAtLeastOnce( matcher );
		 }

		 private void PrePopulateDatabase( IGraphDatabaseService database, Label testLabel, string propertyName )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.RandomValues randomValues = Neo4Net.values.storable.RandomValues.create();
			  RandomValues randomValues = RandomValues.create();
			  for ( int j = 0; j < 10_000; j++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 Node node = database.CreateNode( testLabel );
						 object property = randomValues.NextValue().asObject();
						 node.SetProperty( propertyName, property );
						 transaction.Success();
					}
			  }
		 }

		 private ThreadStart CreateNodeWithLabel( Label label )
		 {
			  return () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 _database.createNode( label );
					 transaction.success();
				}
			  };
		 }

		 private long CountIndexes()
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					return Iterables.count( _database.schema().Indexes );
			  }
		 }

		 private ThreadStart CreateIndexForLabelAndProperty( Label label, string propertyKey )
		 {
			  return () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 _database.schema().indexFor(label).on(propertyKey).create();
					 transaction.success();
				}

				WaitForOnlineIndexes();
			  };
		 }

		 private void WaitForOnlineIndexes()
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					transaction.Success();
			  }
		 }

		 private Callable<Number> CountNodes()
		 {
			  return () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 Result result = _database.execute( "MATCH (n) RETURN count(n) as count" );
					 IDictionary<string, object> resultMap = result.next();
					 return ( Number ) resultMap.get( "count" );
				}
			  };
		 }
	}

}