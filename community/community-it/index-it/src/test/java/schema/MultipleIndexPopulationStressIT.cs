﻿using System;
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
namespace Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using Result = Org.Neo4j.Consistency.ConsistencyCheckService.Result;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using TimeUtil = Org.Neo4j.Helpers.TimeUtil;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using BatchingMultipleIndexPopulator = Org.Neo4j.Kernel.Impl.Api.index.BatchingMultipleIndexPopulator;
	using MultipleIndexPopulator = Org.Neo4j.Kernel.Impl.Api.index.MultipleIndexPopulator;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using RepeatRule = Org.Neo4j.Test.rule.RepeatRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using BatchImporter = Org.Neo4j.@unsafe.Impl.Batchimport.BatchImporter;
	using Org.Neo4j.@unsafe.Impl.Batchimport;
	using InputIterable = Org.Neo4j.@unsafe.Impl.Batchimport.InputIterable;
	using ParallelBatchImporter = Org.Neo4j.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using RandomsStates = Org.Neo4j.@unsafe.Impl.Batchimport.RandomsStates;
	using NumberArrayFactory = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using IdMappers = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMappers;
	using BadCollector = Org.Neo4j.@unsafe.Impl.Batchimport.input.BadCollector;
	using Collector = Org.Neo4j.@unsafe.Impl.Batchimport.input.Collector;
	using Input = Org.Neo4j.@unsafe.Impl.Batchimport.input.Input;
	using ExecutionMonitors = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.progress.ProgressMonitorFactory.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.GeneratingInputIterator.EMPTY_ITERABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Inputs.knownEstimates;

	/// <summary>
	/// Idea is to test a <seealso cref="MultipleIndexPopulator"/> and <seealso cref="BatchingMultipleIndexPopulator"/> with a bunch of indexes,
	/// some which can fail randomly.
	/// Also updates are randomly streaming in during population. In the end all the indexes should have been populated
	/// with correct data.
	/// </summary>
	public class MultipleIndexPopulationStressIT
	{
		private bool InstanceFieldsInitialized = false;

		public MultipleIndexPopulationStressIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _random ).around( _repeat ).around( _directory ).around( _cleanup ).around( _fileSystemRule );
		}

		 private static readonly string[] _tokens = new string[]{ "One", "Two", "Three", "Four" };
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();

		 private readonly RandomRule _random = new RandomRule();
		 private readonly CleanupRule _cleanup = new CleanupRule();
		 private readonly RepeatRule _repeat = new RepeatRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(random).around(repeat).around(directory).around(cleanup).around(fileSystemRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateMultipleIndexWithSeveralNodesSingleThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateMultipleIndexWithSeveralNodesSingleThreaded()
		 {
			  PrepareAndRunTest( false, 10, TimeUnit.SECONDS.toMillis( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateMultipleIndexWithSeveralNodesMultiThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateMultipleIndexWithSeveralNodesMultiThreaded()
		 {
			  PrepareAndRunTest( true, 10, TimeUnit.SECONDS.toMillis( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateMultipleIndexPopulatorsUnderStressSingleThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateMultipleIndexPopulatorsUnderStressSingleThreaded()
		 {
			  ReadConfigAndRunTest( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateMultipleIndexPopulatorsUnderStressMultiThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateMultipleIndexPopulatorsUnderStressMultiThreaded()
		 {
			  int concurrentUpdatesQueueFlushThreshold = _random.Next( 100, 5000 );
			  FeatureToggles.set( typeof( BatchingMultipleIndexPopulator ), BatchingMultipleIndexPopulator.QUEUE_THRESHOLD_NAME, concurrentUpdatesQueueFlushThreshold );
			  try
			  {
					ReadConfigAndRunTest( true );
			  }
			  finally
			  {
					FeatureToggles.clear( typeof( BatchingMultipleIndexPopulator ), BatchingMultipleIndexPopulator.QUEUE_THRESHOLD_NAME );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readConfigAndRunTest(boolean multiThreaded) throws Exception
		 private void ReadConfigAndRunTest( bool multiThreaded )
		 {
			  // GIVEN a database with random data in it
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  int nodeCount = ( int ) Settings.parseLongWithUnit( System.getProperty( this.GetType().FullName + ".nodes", "200k" ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  long duration = TimeUtil.parseTimeMillis.apply( System.getProperty( this.GetType().FullName + ".duration", "5s" ) );
			  PrepareAndRunTest( multiThreaded, nodeCount, duration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareAndRunTest(boolean multiThreaded, int nodeCount, long durationMillis) throws Exception
		 private void PrepareAndRunTest( bool multiThreaded, int nodeCount, long durationMillis )
		 {
			  CreateRandomData( nodeCount );
			  long endTime = currentTimeMillis() + durationMillis;

			  // WHEN/THEN run tests for at least the specified durationMillis
			  for ( int i = 0; currentTimeMillis() < endTime; i++ )
			  {
					RunTest( nodeCount, i, multiThreaded );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void runTest(int nodeCount, int run, boolean multiThreaded) throws Exception
		 private void RunTest( int nodeCount, int run, bool multiThreaded )
		 {
			  // WHEN creating the indexes under stressful updates
			  PopulateDbAndIndexes( nodeCount, multiThreaded );
			  ConsistencyCheckService cc = new ConsistencyCheckService();
			  ConsistencyCheckService.Result result = cc.RunFullConsistencyCheck( _directory.databaseLayout(), Config.defaults(GraphDatabaseSettings.pagecache_memory, "8m"), NONE, NullLogProvider.Instance, false );
			  assertTrue( result.Successful );
			  DropIndexes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void populateDbAndIndexes(int nodeCount, boolean multiThreaded) throws InterruptedException
		 private void PopulateDbAndIndexes( int nodeCount, bool multiThreaded )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().newEmbeddedDatabaseBuilder(directory.databaseDir()).setConfig(org.neo4j.graphdb.factory.GraphDatabaseSettings.multi_threaded_schema_index_population_enabled, multiThreaded + "").newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_directory.databaseDir()).setConfig(GraphDatabaseSettings.multi_threaded_schema_index_population_enabled, multiThreaded + "").newGraphDatabase();
			  try
			  {
					CreateIndexes( db );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean();
					AtomicBoolean end = new AtomicBoolean();
					ExecutorService executor = _cleanup.add( Executors.newCachedThreadPool() );
					for ( int i = 0; i < 10; i++ )
					{
						 executor.submit(() =>
						 {
						  RandomValues randomValues = RandomValues.create();
						  while ( !end.get() )
						  {
								ChangeRandomNode( db, nodeCount, randomValues );
						  }
						 });
					}

					while ( !IndexesAreOnline( db ) )
					{
						 Thread.Sleep( 100 );
					}
					end.set( true );
					executor.shutdown();
					executor.awaitTermination( 10, SECONDS );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private void DropIndexes()
		 {
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_directory.databaseDir()).setConfig(GraphDatabaseSettings.pagecache_memory, "8m").newGraphDatabase();
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						foreach ( IndexDefinition index in Db.schema().Indexes )
						{
							 index.Drop();
						}
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private bool IndexesAreOnline( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( IndexDefinition index in Db.schema().Indexes )
					{
						 switch ( Db.schema().getIndexState(index) )
						 {
						 case ONLINE:
							  break; // Good
							 goto case POPULATING;
						 case POPULATING:
							  return false; // Still populating
						 case FAILED:
							  fail( index + " entered failed state: " + Db.schema().getIndexFailure(index) );
							 goto default;
						 default:
							  throw new System.NotSupportedException();
						 }
					}
					tx.Success();
			  }
			  return true;
		 }

		 /// <summary>
		 /// Create a bunch of indexes in a single transaction. This will have all the indexes being built
		 /// using a single store scan... and this is the gist of what we're testing.
		 /// </summary>
		 private void CreateIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( string label in _random.selection( _tokens, 3, 3, false ) )
					{
						 foreach ( string propertyKey in _random.selection( _tokens, 3, 3, false ) )
						 {
							  Db.schema().indexFor(Label.label(label)).on(propertyKey).create();
						 }
					}
					tx.Success();
			  }
		 }

		 private void ChangeRandomNode( GraphDatabaseService db, int nodeCount, RandomValues random )
		 {
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						long nodeId = random.Next( nodeCount );
						Node node = Db.getNodeById( nodeId );
						object[] keys = Iterables.asCollection( node.PropertyKeys ).ToArray();
						string key = ( string ) random.Among( keys );
						if ( random.NextFloat() < 0.1 )
						{ // REMOVE
							 node.RemoveProperty( key );
						}
						else
						{ // CHANGE
							 node.SetProperty( key, random.NextValue().asObject() );
						}
						tx.Success();
					  }
			  }
			  catch ( NotFoundException )
			  { // It's OK, it happens if some other thread deleted that property in between us reading it and
					// removing or setting it
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRandomData(int count) throws Exception
		 private void CreateRandomData( int count )
		 {
			  Config config = Config.defaults();
			  RecordFormats recordFormats = RecordFormatSelector.selectForConfig( config, NullLogProvider.Instance );
			  using ( RandomDataInput input = new RandomDataInput( this, count ), JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					BatchImporter importer = new ParallelBatchImporter( _directory.databaseLayout(), _fileSystemRule.get(), null, DEFAULT, NullLogService.Instance, ExecutionMonitors.invisible(), EMPTY, config, recordFormats, NO_MONITOR, jobScheduler );
					importer.DoImport( input );
			  }
		 }

		 private class RandomNodeGenerator : GeneratingInputIterator<RandomValues>
		 {
			 private readonly MultipleIndexPopulationStressIT _outerInstance;

			  internal RandomNodeGenerator( MultipleIndexPopulationStressIT outerInstance, int count, Generator<RandomValues> randomsGenerator ) : base( count, 1_000, new RandomsStates( outerInstance.random.Seed() ), randomsGenerator, 0 )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }

		 private class RandomDataInput : Input, AutoCloseable
		 {
			 private readonly MultipleIndexPopulationStressIT _outerInstance;

			  internal readonly int Count;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly BadCollector BadCollectorConflict;

			  internal RandomDataInput( MultipleIndexPopulationStressIT outerInstance, int count )
			  {
				  this._outerInstance = outerInstance;
					this.Count = count;
					this.BadCollectorConflict = CreateBadCollector();
			  }

			  public override InputIterable Relationships()
			  {
					return EMPTY_ITERABLE;
			  }

			  public override InputIterable Nodes()
			  {
					return () => new RandomNodeGenerator(_outerInstance, Count, (state, visitor, id) =>
					{
					string[] keys = outerInstance.random.RandomValues().selection(_tokens, 1, _tokens.Length, false);
					foreach ( string key in keys )
					{
						visitor.property( key, outerInstance.random.NextValueAsObject() );
					}
					visitor.labels( outerInstance.random.Selection( _tokens, 1, _tokens.Length, false ) );
					});
			  }

			  public override IdMapper IdMapper( NumberArrayFactory numberArrayFactory )
			  {
					return IdMappers.actual();
			  }

			  public override Collector BadCollector()
			  {
					return BadCollectorConflict;
			  }

			  internal virtual BadCollector CreateBadCollector()
			  {
					try
					{
						 return new BadCollector( outerInstance.fileSystemRule.Get().openAsOutputStream(new File(outerInstance.directory.DatabaseDir(), "bad"), false), 0, 0 );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  public override Org.Neo4j.@unsafe.Impl.Batchimport.input.Input_Estimates CalculateEstimates( System.Func<Value[], int> valueSizeCalculator )
			  {
					return knownEstimates( Count, 0, Count * _tokens.Length / 2, 0, Count * _tokens.Length / 2 * Long.BYTES, 0, 0 );
			  }

			  public override void Close()
			  {
					BadCollectorConflict.close();
			  }
		 }

	}

}