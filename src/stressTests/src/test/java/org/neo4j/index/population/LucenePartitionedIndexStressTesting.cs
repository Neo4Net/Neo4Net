using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Index.population
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.SystemUtils.JAVA_IO_TMPDIR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helper.StressTestingHelper.fromEnv;

	public class LucenePartitionedIndexStressTesting
	{
		 private const string LABEL = "label";
		 private const string PROPERTY_PREFIX = "property";
		 private const string UNIQUE_PROPERTY_PREFIX = "uniqueProperty";

		 private const int NUMBER_OF_PROPERTIES = 2;

		 private static readonly int _numberOfPopulators = Convert.ToInt32( fromEnv( "LUCENE_INDEX_NUMBER_OF_POPULATORS", ( Runtime.Runtime.availableProcessors() - 1 ).ToString() ) );
		 private static readonly int _batchSize = Convert.ToInt32( fromEnv( "LUCENE_INDEX_POPULATION_BATCH_SIZE", 10000.ToString() ) );

		 private static readonly long _numberOfNodes = Convert.ToInt64( fromEnv( "LUCENE_PARTITIONED_INDEX_NUMBER_OF_NODES", 100000.ToString() ) );
		 private static readonly string _workDirectory = fromEnv( "LUCENE_PARTITIONED_INDEX_WORKING_DIRECTORY", JAVA_IO_TMPDIR );
		 private static readonly int _waitDurationMinutes = Convert.ToInt32( fromEnv( "LUCENE_PARTITIONED_INDEX_WAIT_TILL_ONLINE", 30.ToString() ) );

		 private ExecutorService _populators;
		 private IGraphDatabaseService _db;
		 private File _storeDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _storeDir = PrepareStoreDir();
			  Console.WriteLine( string.Format( "Starting database at: {0}", _storeDir ) );

			  _populators = Executors.newFixedThreadPool( _numberOfPopulators );
			  _db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_storeDir).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _db.shutdown();
			  _populators.shutdown();
			  FileUtils.deleteRecursively( _storeDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexCreationStressTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexCreationStressTest()
		 {
			  CreateIndexes();
			  CreateUniqueIndexes();
			  PopulationResult populationResult = PopulateDatabase();
			  FindLastTrackedNodesByLabelAndProperties( _db, populationResult );
			  DropAllIndexes();

			  CreateUniqueIndexes();
			  CreateIndexes();
			  FindLastTrackedNodesByLabelAndProperties( _db, populationResult );
		 }

		 private void DropAllIndexes()
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					Schema schema = _db.schema();
					Schema.Constraints.forEach( ConstraintDefinition.drop );
					Schema.Indexes.forEach( IndexDefinition.drop );
					transaction.Success();
			  }
		 }

		 private void CreateIndexes()
		 {
			  CreateIndexes( false );
		 }

		 private void CreateUniqueIndexes()
		 {
			  CreateIndexes( true );
		 }

		 private void CreateIndexes( bool unique )
		 {
			  Console.WriteLine( string.Format( "Creating {0:D}{1} indexes.", NUMBER_OF_PROPERTIES, unique ? " unique" : "" ) );
			  long creationStart = System.nanoTime();
			  CreateAndWaitForIndexes( unique );
			  Console.WriteLine( string.Format( "{0:D}{1} indexes created.", NUMBER_OF_PROPERTIES, unique ? " unique" : "" ) );
			  Console.WriteLine( "Creation took: " + TimeUnit.NANOSECONDS.toMillis( System.nanoTime() - creationStart ) + " ms." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PopulationResult populateDatabase() throws java.util.concurrent.ExecutionException, InterruptedException
		 private PopulationResult PopulateDatabase()
		 {
			  Console.WriteLine( "Starting database population." );
			  long populationStart = System.nanoTime();
			  PopulationResult populationResult = PopulateDb( _db );

			  Console.WriteLine( "Database population completed. Inserted " + populationResult.NumberOfNodes + " nodes." );
			  Console.WriteLine( "Population took: " + TimeUnit.NANOSECONDS.toMillis( System.nanoTime() - populationStart ) + " ms." );
			  return populationResult;
		 }

		 private void FindLastTrackedNodesByLabelAndProperties( IGraphDatabaseService db, PopulationResult populationResult )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					Node nodeByUniqueStringProperty = Db.findNode( Label.label( LABEL ), UniqueStringProperty, populationResult.MaxPropertyId + "" );
					Node nodeByStringProperty = Db.findNode( Label.label( LABEL ), StringProperty, populationResult.MaxPropertyId + "" );
					assertNotNull( "Should find last inserted node", nodeByStringProperty );
					assertEquals( "Both nodes should be the same last inserted node", nodeByStringProperty, nodeByUniqueStringProperty );

					Node nodeByUniqueLongProperty = Db.findNode( Label.label( LABEL ), UniqueLongProperty, populationResult.MaxPropertyId );
					Node nodeByLongProperty = Db.findNode( Label.label( LABEL ), LongProperty, populationResult.MaxPropertyId );
					assertNotNull( "Should find last inserted node", nodeByLongProperty );
					assertEquals( "Both nodes should be the same last inserted node", nodeByLongProperty, nodeByUniqueLongProperty );

			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File prepareStoreDir() throws java.io.IOException
		 private static File PrepareStoreDir()
		 {
			  Path storeDirPath = Paths.get( _workDirectory ).resolve( Paths.get( "storeDir" ) );
			  File storeDirectory = storeDirPath.toFile();
			  FileUtils.deleteRecursively( storeDirectory );
			  storeDirectory.deleteOnExit();
			  return storeDirectory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PopulationResult populateDb(org.Neo4Net.graphdb.GraphDatabaseService db) throws java.util.concurrent.ExecutionException, InterruptedException
		 private PopulationResult PopulateDb( IGraphDatabaseService db )
		 {
			  AtomicLong nodesCounter = new AtomicLong();

			  IList<Future<long>> futures = new List<Future<long>>( _numberOfPopulators );
			  for ( int i = 0; i < _numberOfPopulators; i++ )
			  {
					futures.Add( _populators.submit( new Populator( i, _numberOfPopulators, db, nodesCounter ) ) );
			  }

			  long maxPropertyId = 0;
			  foreach ( Future<long> future in futures )
			  {
					maxPropertyId = Math.Max( maxPropertyId, future.get() );
			  }
			  return new PopulationResult( this, maxPropertyId, nodesCounter.get() );
		 }

		 private void CreateAndWaitForIndexes( bool unique )
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					for ( int i = 0; i < NUMBER_OF_PROPERTIES; i++ )
					{
						 if ( unique )
						 {
							  CreateUniqueConstraint( i );
						 }
						 else
						 {
							  CreateIndex( i );
						 }
					}
					transaction.Success();
			  }
			  AwaitIndexesOnline( _db );
		 }

		 private void CreateUniqueConstraint( int index )
		 {
			  _db.schema().constraintFor(Label.label(LABEL)).assertPropertyIsUnique(UNIQUE_PROPERTY_PREFIX + index).create();
		 }

		 private void CreateIndex( int index )
		 {
			  _db.schema().indexFor(Label.label(LABEL)).on(PROPERTY_PREFIX + index).create();
		 }

		 private void AwaitIndexesOnline( IGraphDatabaseService db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					Schema schema = Db.schema();
					Schema.awaitIndexesOnline( _waitDurationMinutes, TimeUnit.MINUTES );
			  }
		 }

		 private static string LongProperty
		 {
			 get
			 {
				  return PROPERTY_PREFIX + 1;
			 }
		 }

		 private static string StringProperty
		 {
			 get
			 {
				  return PROPERTY_PREFIX + 0;
			 }
		 }

		 private static string UniqueLongProperty
		 {
			 get
			 {
				  return UNIQUE_PROPERTY_PREFIX + 1;
			 }
		 }

		 private static string UniqueStringProperty
		 {
			 get
			 {
				  return UNIQUE_PROPERTY_PREFIX + 0;
			 }
		 }

		 private class SequentialStringSupplier : System.Func<string>
		 {
			  internal readonly int Step;
			  internal long Value;

			  internal SequentialStringSupplier( int populatorNumber, int step )
			  {
					this.Value = populatorNumber;
					this.Step = step;
			  }

			  public override string Get()
			  {
					Value += Step;
					return Value + "";
			  }
		 }

		 private class SequentialLongSupplier : System.Func<long>
		 {
			  internal long Value;
			  internal int Step;

			  internal SequentialLongSupplier( int populatorNumber, int step )
			  {
					Value = populatorNumber;
					this.Step = step;
			  }

			  public override long AsLong
			  {
				  get
				  {
						Value += Step;
						return Value;
				  }
			  }
		 }

		 private class Populator : Callable<long>
		 {
			  internal readonly int PopulatorNumber;
			  internal readonly int Step;
			  internal IGraphDatabaseService Db;
			  internal AtomicLong NodesCounter;

			  internal Populator( int populatorNumber, int step, IGraphDatabaseService db, AtomicLong nodesCounter )
			  {
					this.PopulatorNumber = populatorNumber;
					this.Step = step;
					this.Db = db;
					this.NodesCounter = nodesCounter;
			  }

			  public override long? Call()
			  {
					SequentialLongSupplier longSupplier = new SequentialLongSupplier( PopulatorNumber, Step );
					SequentialStringSupplier stringSupplier = new SequentialStringSupplier( PopulatorNumber, Step );

					while ( NodesCounter.get() < _numberOfNodes )
					{
						 long nodesInTotal = NodesCounter.addAndGet( InsertBatchNodes( Db, stringSupplier, longSupplier ) );
						 if ( nodesInTotal % 1_000_000 == 0 )
						 {
							  Console.WriteLine( "Inserted " + nodesInTotal + " nodes." );
						 }
					}
					return longSupplier.Value;
			  }

			  internal virtual int InsertBatchNodes( IGraphDatabaseService db, System.Func<string> stringValueSupplier, System.Func<long> longSupplier )
			  {
					using ( Transaction transaction = Db.beginTx() )
					{
						 for ( int i = 0; i < _batchSize; i++ )
						 {
							  Node node = Db.createNode( Label.label( LABEL ) );

							  string stringValue = stringValueSupplier();
							  long longValue = longSupplier();

							  node.SetProperty( StringProperty, stringValue );
							  node.SetProperty( LongProperty, longValue );

							  node.SetProperty( UniqueStringProperty, stringValue );
							  node.SetProperty( UniqueLongProperty, longValue );
						 }
						 transaction.Success();
					}
					return _batchSize;
			  }
		 }

		 private class PopulationResult
		 {
			 private readonly LucenePartitionedIndexStressTesting _outerInstance;

			  internal long MaxPropertyId;
			  internal long NumberOfNodes;

			  internal PopulationResult( LucenePartitionedIndexStressTesting outerInstance, long maxPropertyId, long numberOfNodes )
			  {
				  this._outerInstance = outerInstance;
					this.MaxPropertyId = maxPropertyId;
					this.NumberOfNodes = numberOfNodes;
			  }
		 }
	}

}