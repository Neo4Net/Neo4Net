using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using Registers = Neo4Net.Register.Registers;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Barrier = Neo4Net.Test.Barrier;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using FeatureToggles = Neo4Net.Util.FeatureToggles;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.join;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	/// <summary>
	/// This test validates that we count the correct amount of index updates. In the process it also verifies that the populated index has
	/// the correct nodes, after the index have been flipped.
	/// <para>
	/// We build the index async with a node scan in the background and consume transactions to keep the index updated. Once
	/// the index is build and becomes online, we save the index size(number of entries) and begin tracking updates. These
	/// values can then be used to determine when to re-sample the index for example.
	/// </para>
	/// <para>
	/// The area around when the index population is done is controlled using a <seealso cref="Barrier"/> so that we can assert sample data
	/// with 100% accuracy against the updates we know that the test has done during the time the index was populating.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexStatisticsTest
	public class IndexStatisticsTest
	{
		 private const double UNIQUE_NAMES = 10.0;
		 private static readonly string[] _names = new string[]{ "Andres", "Davide", "Jakub", "Chris", "Tobias", "Stefan", "Petra", "Rickard", "Mattias", "Emil", "Chris", "Chris" };

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private static readonly int _creationMultiplier = Integer.getInteger( typeof( IndexStatisticsTest ).FullName + ".creationMultiplier", 1_000 );
		 private const string PERSON_LABEL = "Person";
		 private const string NAME_PROPERTY = "name";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public boolean multiThreadedPopulationEnabled;
		 public bool MultiThreadedPopulationEnabled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.index_background_sampling_enabled, "false").startLazily();
		 public readonly DatabaseRule DbRule = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.index_background_sampling_enabled, "false").startLazily();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private GraphDatabaseService _db;
		 private ThreadToStatementContextBridge _bridge;
		 private readonly IndexOnlineMonitor _indexOnlineMonitor = new IndexOnlineMonitor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "multiThreadedIndexPopulationEnabled = {0}") public static Object[] multiThreadedIndexPopulationEnabledValues()
		 public static object[] MultiThreadedIndexPopulationEnabledValues()
		 {
			  return new object[]{ true, false };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  DbRule.withSetting( GraphDatabaseSettings.multi_threaded_schema_index_population_enabled, MultiThreadedPopulationEnabled + "" );

			  int batchSize = Random.Next( 1, 5 );
			  FeatureToggles.set( typeof( MultipleIndexPopulator ), MultipleIndexPopulator.QUEUE_THRESHOLD_NAME, batchSize );
			  FeatureToggles.set( typeof( BatchingMultipleIndexPopulator ), MultipleIndexPopulator.QUEUE_THRESHOLD_NAME, batchSize );
			  FeatureToggles.set( typeof( MultipleIndexPopulator ), "print_debug", true );

			  GraphDatabaseAPI graphDatabaseAPI = DbRule.GraphDatabaseAPI;
			  this._db = graphDatabaseAPI;
			  DependencyResolver dependencyResolver = graphDatabaseAPI.DependencyResolver;
			  this._bridge = dependencyResolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
			  graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( Monitors ) ).addMonitorListener( _indexOnlineMonitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  FeatureToggles.clear( typeof( MultipleIndexPopulator ), MultipleIndexPopulator.QUEUE_THRESHOLD_NAME );
			  FeatureToggles.clear( typeof( BatchingMultipleIndexPopulator ), MultipleIndexPopulator.QUEUE_THRESHOLD_NAME );
			  FeatureToggles.clear( typeof( MultipleIndexPopulator ), "print_debug" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsForDataCreatedWhenPopulationBeforeTheIndexIsOnline() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsForDataCreatedWhenPopulationBeforeTheIndexIsOnline()
		 {
			  // given
			  _indexOnlineMonitor.initialize( 0 );
			  CreateSomePersons();

			  // when
			  IndexReference index = CreatePersonNameIndex();
			  AwaitIndexesOnline();

			  // then
			  assertEquals( 0.75d, IndexSelectivity( index ), 0d );
			  assertEquals( 4L, IndexSize( index ) );
			  assertEquals( 0L, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDataCreatedAfterPopulation() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDataCreatedAfterPopulation()
		 {
			  // given
			  _indexOnlineMonitor.initialize( 0 );
			  IndexReference index = CreatePersonNameIndex();
			  AwaitIndexesOnline();

			  // when
			  CreateSomePersons();

			  // then
			  assertEquals( 1.0d, IndexSelectivity( index ), 0d );
			  assertEquals( 0L, IndexSize( index ) );
			  assertEquals( 4L, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsForDataSeenDuringPopulationAndIgnoreDataCreatedAfterPopulation() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsForDataSeenDuringPopulationAndIgnoreDataCreatedAfterPopulation()
		 {
			  // given
			  _indexOnlineMonitor.initialize( 0 );
			  CreateSomePersons();
			  IndexReference index = CreatePersonNameIndex();
			  AwaitIndexesOnline();

			  // when
			  CreateSomePersons();

			  // then
			  assertEquals( 0.75d, IndexSelectivity( index ), 0d );
			  assertEquals( 4L, IndexSize( index ) );
			  assertEquals( 4L, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveIndexStatisticsAfterIndexIsDeleted() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveIndexStatisticsAfterIndexIsDeleted()
		 {
			  // given
			  _indexOnlineMonitor.initialize( 0 );
			  CreateSomePersons();
			  IndexReference index = CreatePersonNameIndex();
			  AwaitIndexesOnline();

			  SchemaStorage storage = new SchemaStorage( NeoStores().SchemaStore );
			  long indexId = storage.IndexGetForSchema( ( IndexDescriptor ) index ).Id;

			  // when
			  DropIndex( index );

			  // then
			  try
			  {
					IndexSelectivity( index );
					fail( "Expected IndexNotFoundKernelException to be thrown" );
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					Register_DoubleLongRegister actual = Tracker.indexSample( indexId, Registers.newDoubleLongRegister() );
					AssertDoubleLongEquals( 0L, 0L, actual );
			  }

			  // and then index size and index updates are zero on disk
			  Register_DoubleLongRegister actual = Tracker.indexUpdatesAndSize( indexId, Registers.newDoubleLongRegister() );
			  AssertDoubleLongEquals( 0L, 0L, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexSelectivityWhenThereAreManyDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexSelectivityWhenThereAreManyDuplicates()
		 {
			  // given some initial data
			  _indexOnlineMonitor.initialize( 0 );
			  int created = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier ).Length;

			  // when
			  IndexReference index = CreatePersonNameIndex();
			  AwaitIndexesOnline();

			  // then
			  double expectedSelectivity = UNIQUE_NAMES / created;
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( created, IndexSize( index ) );
			  assertEquals( 0L, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditions()
		 {
			  // given some initial data
			  _indexOnlineMonitor.initialize( 1 );
			  int initialNodes = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier ).Length;

			  // when populating while creating
			  IndexReference index = CreatePersonNameIndex();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpdatesTracker updatesTracker = executeCreations(CREATION_MULTIPLIER);
			  UpdatesTracker updatesTracker = ExecuteCreations( _creationMultiplier );
			  AwaitIndexesOnline();

			  // then
			  int seenWhilePopulating = initialNodes + updatesTracker.CreatedDuringPopulation();
			  double expectedSelectivity = UNIQUE_NAMES / seenWhilePopulating;
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( seenWhilePopulating, IndexSize( index ) );
			  AssertCorrectIndexUpdates( updatesTracker.CreatedAfterPopulation(), IndexUpdates(index) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndDeletions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndDeletions()
		 {
			  // given some initial data
			  _indexOnlineMonitor.initialize( 1 );
			  long[] nodes = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier );
			  int initialNodes = nodes.Length;

			  // when populating while creating
			  IndexReference index = CreatePersonNameIndex();
			  UpdatesTracker updatesTracker = ExecuteCreationsAndDeletions( nodes, _creationMultiplier );
			  AwaitIndexesOnline();

			  // then
			  AssertIndexedNodesMatchesStoreNodes();
			  int seenWhilePopulating = initialNodes + updatesTracker.CreatedDuringPopulation() - updatesTracker.DeletedDuringPopulation();
			  double expectedSelectivity = UNIQUE_NAMES / seenWhilePopulating;
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( seenWhilePopulating, IndexSize( index ) );
			  int expectedIndexUpdates = updatesTracker.DeletedAfterPopulation() + updatesTracker.CreatedAfterPopulation();
			  AssertCorrectIndexUpdates( expectedIndexUpdates, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndChanges()
		 {
			  // given some initial data
			  _indexOnlineMonitor.initialize( 1 );
			  long[] nodes = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier );
			  int initialNodes = nodes.Length;

			  // when populating while creating
			  IndexReference index = CreatePersonNameIndex();
			  UpdatesTracker updatesTracker = ExecuteCreationsAndUpdates( nodes, _creationMultiplier );
			  AwaitIndexesOnline();

			  // then
			  AssertIndexedNodesMatchesStoreNodes();
			  int seenWhilePopulating = initialNodes + updatesTracker.CreatedDuringPopulation();
			  double expectedSelectivity = UNIQUE_NAMES / seenWhilePopulating;
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( seenWhilePopulating, IndexSize( index ) );
			  int expectedIndexUpdates = updatesTracker.CreatedAfterPopulation() + updatesTracker.UpdatedAfterPopulation();
			  AssertCorrectIndexUpdates( expectedIndexUpdates, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndChangesAndDeletions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideIndexStatisticsWhenIndexIsBuiltViaPopulationAndConcurrentAdditionsAndChangesAndDeletions()
		 {
			  // given some initial data
			  _indexOnlineMonitor.initialize( 1 );
			  long[] nodes = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier );
			  int initialNodes = nodes.Length;

			  // when populating while creating
			  IndexReference index = CreatePersonNameIndex();
			  UpdatesTracker updatesTracker = ExecuteCreationsDeletionsAndUpdates( nodes, _creationMultiplier );
			  AwaitIndexesOnline();

			  // then
			  AssertIndexedNodesMatchesStoreNodes();
			  int seenWhilePopulating = initialNodes + updatesTracker.CreatedDuringPopulation() - updatesTracker.DeletedDuringPopulation();
			  double expectedSelectivity = UNIQUE_NAMES / seenWhilePopulating;
			  int expectedIndexUpdates = updatesTracker.DeletedAfterPopulation() + updatesTracker.CreatedAfterPopulation() + updatesTracker.UpdatedAfterPopulation();
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( seenWhilePopulating, IndexSize( index ) );
			  AssertCorrectIndexUpdates( expectedIndexUpdates, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhileHavingHeavyConcurrentUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWhileHavingHeavyConcurrentUpdates()
		 {
			  // given some initial data
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] nodes = repeatCreateNamedPeopleFor(NAMES.length * CREATION_MULTIPLIER);
			  long[] nodes = RepeatCreateNamedPeopleFor( _names.Length * _creationMultiplier );
			  int initialNodes = nodes.Length;
			  int threads = 5;
			  _indexOnlineMonitor.initialize( threads );
			  ExecutorService executorService = Executors.newFixedThreadPool( threads );

			  // when populating while creating
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.IndexReference index = createPersonNameIndex();
			  IndexReference index = CreatePersonNameIndex();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<java.util.concurrent.Callable<UpdatesTracker>> jobs = new java.util.ArrayList<>(threads);
			  ICollection<Callable<UpdatesTracker>> jobs = new List<Callable<UpdatesTracker>>( threads );
			  for ( int i = 0; i < threads; i++ )
			  {
					jobs.Add( () => ExecuteCreationsDeletionsAndUpdates(nodes, _creationMultiplier) );
			  }

			  IList<Future<UpdatesTracker>> futures = executorService.invokeAll( jobs );
			  // sum result into empty result
			  UpdatesTracker result = new UpdatesTracker();
			  result.NotifyPopulationCompleted();
			  foreach ( Future<UpdatesTracker> future in futures )
			  {
					result.Add( future.get() );
			  }
			  AwaitIndexesOnline();

			  executorService.shutdown();
			  assertTrue( executorService.awaitTermination( 1, TimeUnit.MINUTES ) );

			  // then
			  AssertIndexedNodesMatchesStoreNodes();
			  int seenWhilePopulating = initialNodes + result.CreatedDuringPopulation() - result.DeletedDuringPopulation();
			  double expectedSelectivity = UNIQUE_NAMES / seenWhilePopulating;
			  AssertCorrectIndexSelectivity( expectedSelectivity, IndexSelectivity( index ) );
			  AssertCorrectIndexSize( "Tracker had " + result, seenWhilePopulating, IndexSize( index ) );
			  int expectedIndexUpdates = result.DeletedAfterPopulation() + result.CreatedAfterPopulation() + result.UpdatedAfterPopulation();
			  AssertCorrectIndexUpdates( "Tracker had " + result, expectedIndexUpdates, IndexUpdates( index ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexedNodesMatchesStoreNodes() throws Exception
		 private void AssertIndexedNodesMatchesStoreNodes()
		 {
			  int nodesInStore = 0;
			  Label label = Label.label( PERSON_LABEL );
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					IList<string> mismatches = new List<string>();
					int labelId = ktx.TokenRead().nodeLabel(PERSON_LABEL);
					int propertyKeyId = ktx.TokenRead().propertyKey(NAME_PROPERTY);
					IndexReference index = ktx.SchemaRead().index(labelId, propertyKeyId);
					using ( NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor() )
					{
						 // Node --> Index
						 foreach ( Node node in filter( n => n.hasLabel( label ) && n.hasProperty( NAME_PROPERTY ), _db.AllNodes ) )
						 {
							  nodesInStore++;
							  string name = ( string ) node.GetProperty( NAME_PROPERTY );
							  ktx.DataRead().nodeIndexSeek(index, cursor, IndexOrder.NONE, false, IndexQuery.exact(propertyKeyId, name));
							  bool found = false;
							  while ( cursor.Next() )
							  {
									long indexedNode = cursor.NodeReference();
									if ( indexedNode == node.Id )
									{
										 if ( found )
										 {
											  mismatches.Add( "Index has multiple entries for " + name + " and " + indexedNode );
										 }
										 found = true;
									}
							  }
							  if ( !found )
							  {
									mismatches.Add( "Index is missing entry for " + name + " " + node );
							  }
						 }
						 if ( mismatches.Count > 0 )
						 {
							  fail( join( mismatches.ToArray(), format("%n") ) );
						 }
						 // Node count == indexed node count
						 ktx.DataRead().nodeIndexSeek(index, cursor, IndexOrder.NONE, false, IndexQuery.exists(propertyKeyId));
						 int nodesInIndex = 0;
						 while ( cursor.Next() )
						 {
							  nodesInIndex++;
						 }
						 assertEquals( nodesInStore, nodesInIndex );
					}
			  }
		 }

		 private void DeleteNode( long nodeId )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeId ).delete();
					tx.Success();
			  }
		 }

		 private bool ChangeName( long nodeId, object newValue )
		 {
			  bool changeIndexedNode = false;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					object oldValue = node.GetProperty( NAME_PROPERTY );
					if ( !oldValue.Equals( newValue ) )
					{
						 // Changes are only propagated when the value actually change
						 changeIndexedNode = true;
					}
					node.SetProperty( NAME_PROPERTY, newValue );
					tx.Success();
			  }
			  return changeIndexedNode;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createNamedPeople(long[] nodes, int offset) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private int CreateNamedPeople( long[] nodes, int offset )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _bridge.getKernelTransactionBoundToThisThread( true );
					foreach ( string name in _names )
					{
						 long nodeId = CreatePersonNode( ktx, name );
						 if ( nodes != null )
						 {
							  nodes[offset++] = nodeId;
						 }
					}
					tx.Success();
			  }
			  return _names.Length;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long[] repeatCreateNamedPeopleFor(int totalNumberOfPeople) throws Exception
		 private long[] RepeatCreateNamedPeopleFor( int totalNumberOfPeople )
		 {
			  // Parallelize the creation of persons
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] nodes = new long[totalNumberOfPeople];
			  long[] nodes = new long[totalNumberOfPeople];
			  const int threads = 100;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int peoplePerThread = totalNumberOfPeople / threads;
			  int peoplePerThread = totalNumberOfPeople / threads;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ExecutorService service = java.util.concurrent.Executors.newFixedThreadPool(threads);
			  ExecutorService service = Executors.newFixedThreadPool( threads );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.internal.kernel.api.exceptions.KernelException> exception = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<KernelException> exception = new AtomicReference<KernelException>();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.util.concurrent.Callable<Void>> jobs = new java.util.ArrayList<>(threads);
			  IList<Callable<Void>> jobs = new List<Callable<Void>>( threads );
			  // Start threads that creates these people, relying on batched writes to speed things up
			  for ( int i = 0; i < threads; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int finalI = i;
					int finalI = i;

					jobs.Add(() =>
					{
					 int offset = finalI * peoplePerThread;
					 while ( offset < ( finalI + 1 ) * peoplePerThread )
					 {
						  try
						  {
								offset += CreateNamedPeople( nodes, offset );
						  }
						  catch ( KernelException e )
						  {
								exception.compareAndSet( null, e );
								throw new Exception( e );
						  }
					 }
					 return null;
					});
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> job : service.invokeAll(jobs))
			  foreach ( Future<object> job in service.invokeAll( jobs ) )
			  {
					job.get();
			  }

			  service.awaitTermination( 1, TimeUnit.SECONDS );
			  service.shutdown();

			  // Make any KernelException thrown from a creation thread visible in the main thread
			  Exception ex = exception.get();
			  if ( ex != null )
			  {
					throw ex;
			  }

			  return nodes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropIndex(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void DropIndex( IndexReference index )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _bridge.getKernelTransactionBoundToThisThread( true );
					using ( Statement ignore = ktx.AcquireStatement() )
					{
						 ktx.SchemaWrite().indexDrop(index);
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long indexSize(org.neo4j.internal.kernel.api.IndexReference reference) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long IndexSize( IndexReference reference )
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( IndexingService ) ).indexUpdatesAndSize( reference.Schema() ).readSecond();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long indexUpdates(org.neo4j.internal.kernel.api.IndexReference reference) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long IndexUpdates( IndexReference reference )
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( IndexingService ) ).indexUpdatesAndSize( reference.Schema() ).readFirst();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private double indexSelectivity(org.neo4j.internal.kernel.api.IndexReference reference) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private double IndexSelectivity( IndexReference reference )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					double selectivity = GetSelectivity( reference );
					tx.Success();
					return selectivity;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private double getSelectivity(org.neo4j.internal.kernel.api.IndexReference reference) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private double GetSelectivity( IndexReference reference )
		 {

			  return _bridge.getKernelTransactionBoundToThisThread( true ).schemaRead().indexUniqueValuesSelectivity(reference);
		 }

		 private CountsTracker Tracker
		 {
			 get
			 {
				  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().Counts;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createSomePersons() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void CreateSomePersons()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _bridge.getKernelTransactionBoundToThisThread( true );
					CreatePersonNode( ktx, "Davide" );
					CreatePersonNode( ktx, "Stefan" );
					CreatePersonNode( ktx, "John" );
					CreatePersonNode( ktx, "John" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createPersonNode(org.neo4j.kernel.api.KernelTransaction ktx, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreatePersonNode( KernelTransaction ktx, object value )
		 {
			  int labelId = ktx.TokenWrite().labelGetOrCreateForName(PERSON_LABEL);
			  int propertyKeyId = ktx.TokenWrite().propertyKeyGetOrCreateForName(NAME_PROPERTY);
			  long nodeId = ktx.DataWrite().nodeCreate();
			  ktx.DataWrite().nodeAddLabel(nodeId, labelId);
			  ktx.DataWrite().nodeSetProperty(nodeId, propertyKeyId, Values.of(value));
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createPersonNameIndex() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private IndexReference CreatePersonNameIndex()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexReference index;
					KernelTransaction ktx = _bridge.getKernelTransactionBoundToThisThread( true );
					using ( Statement ignore = ktx.AcquireStatement() )
					{
						 int labelId = ktx.TokenWrite().labelGetOrCreateForName(PERSON_LABEL);
						 int propertyKeyId = ktx.TokenWrite().propertyKeyGetOrCreateForName(NAME_PROPERTY);
						 LabelSchemaDescriptor descriptor = forLabel( labelId, propertyKeyId );
						 index = ktx.SchemaWrite().indexCreate(descriptor);
					}
					tx.Success();
					return index;
			  }
		 }

		 private NeoStores NeoStores()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
		 }

		 private void AwaitIndexesOnline()
		 {
			  using ( Transaction ignored = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(3, TimeUnit.MINUTES);
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpdatesTracker executeCreations(int numberOfCreations) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private UpdatesTracker ExecuteCreations( int numberOfCreations )
		 {
			  return InternalExecuteCreationsDeletionsAndUpdates( null, numberOfCreations, false, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpdatesTracker executeCreationsAndDeletions(long[] nodes, int numberOfCreations) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private UpdatesTracker ExecuteCreationsAndDeletions( long[] nodes, int numberOfCreations )
		 {
			  return InternalExecuteCreationsDeletionsAndUpdates( nodes, numberOfCreations, true, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpdatesTracker executeCreationsAndUpdates(long[] nodes, int numberOfCreations) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private UpdatesTracker ExecuteCreationsAndUpdates( long[] nodes, int numberOfCreations )
		 {
			  return InternalExecuteCreationsDeletionsAndUpdates( nodes, numberOfCreations, false, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpdatesTracker executeCreationsDeletionsAndUpdates(long[] nodes, int numberOfCreations) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private UpdatesTracker ExecuteCreationsDeletionsAndUpdates( long[] nodes, int numberOfCreations )
		 {
			  return InternalExecuteCreationsDeletionsAndUpdates( nodes, numberOfCreations, true, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpdatesTracker internalExecuteCreationsDeletionsAndUpdates(long[] nodes, int numberOfCreations, boolean allowDeletions, boolean allowUpdates) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private UpdatesTracker InternalExecuteCreationsDeletionsAndUpdates( long[] nodes, int numberOfCreations, bool allowDeletions, bool allowUpdates )
		 {
			  if ( Random.nextBoolean() )
			  {
					// 50% of time await the start signal so that updater(s) race as much as possible with the populator.
					_indexOnlineMonitor.startSignal.await();
			  }
			  Random random = ThreadLocalRandom.current();
			  UpdatesTracker updatesTracker = new UpdatesTracker();
			  int offset = 0;
			  while ( updatesTracker.Created() < numberOfCreations )
			  {
					int created = CreateNamedPeople( nodes, offset );
					offset += created;
					updatesTracker.IncreaseCreated( created );
					NotifyIfPopulationCompleted( updatesTracker );

					// delete if allowed
					if ( allowDeletions && updatesTracker.Created() % 24 == 0 )
					{
						 long nodeId = nodes[random.Next( nodes.Length )];
						 try
						 {
							  DeleteNode( nodeId );
							  updatesTracker.IncreaseDeleted( 1 );
						 }
						 catch ( NotFoundException )
						 {
							  // ignore
						 }
						 NotifyIfPopulationCompleted( updatesTracker );
					}

					// update if allowed
					if ( allowUpdates && updatesTracker.Created() % 24 == 0 )
					{
						 int randomIndex = random.Next( nodes.Length );
						 try
						 {
							  if ( ChangeName( nodes[randomIndex], _names[random.Next( _names.Length )] ) )
							  {
									updatesTracker.IncreaseUpdated( 1 );
							  }
						 }
						 catch ( NotFoundException )
						 {
							  // ignore
						 }
						 NotifyIfPopulationCompleted( updatesTracker );
					}
			  }
			  // make sure population complete has been notified
			  NotifyPopulationCompleted( updatesTracker );
			  return updatesTracker;
		 }

		 private void NotifyPopulationCompleted( UpdatesTracker updatesTracker )
		 {
			  _indexOnlineMonitor.updatesDone();
			  updatesTracker.NotifyPopulationCompleted();
		 }

		 private void NotifyIfPopulationCompleted( UpdatesTracker updatesTracker )
		 {
			  if ( IsCompletedPopulation( updatesTracker ) )
			  {
					NotifyPopulationCompleted( updatesTracker );
			  }
		 }

		 private bool IsCompletedPopulation( UpdatesTracker updatesTracker )
		 {
			  return !updatesTracker.PopulationCompleted && _indexOnlineMonitor.IndexOnline;
		 }

		 private void AssertDoubleLongEquals( long expectedUniqueValue, long expectedSampledSize, Register_DoubleLongRegister register )
		 {
			  assertEquals( expectedUniqueValue, register.ReadFirst() );
			  assertEquals( expectedSampledSize, register.ReadSecond() );
		 }

		 private static void AssertCorrectIndexSize( long expected, long actual )
		 {
			  AssertCorrectIndexSize( "", expected, actual );
		 }

		 private static void AssertCorrectIndexSize( string info, long expected, long actual )
		 {
			  string message = format( "Expected number of entries to not differ (expected: %d actual: %d) %s", expected, actual, info );
			  assertEquals( message, 0L, Math.Abs( expected - actual ) );
		 }

		 private static void AssertCorrectIndexUpdates( long expected, long actual )
		 {
			  AssertCorrectIndexUpdates( "", expected, actual );
		 }

		 private static void AssertCorrectIndexUpdates( string info, long expected, long actual )
		 {
			  string message = format( "Expected number of index updates to not differ (expected: %d actual: %d). %s", expected, actual, info );
			  assertEquals( message, 0L, Math.Abs( expected - actual ) );
		 }

		 private static void AssertCorrectIndexSelectivity( double expected, double actual )
		 {
			  string message = format( "Expected number of entries to not differ (expected: %f actual: %f)", expected, actual );
			  assertEquals( message, expected, actual, 0d );
		 }

		 private class IndexOnlineMonitor : IndexingService.MonitorAdapter
		 {
			  internal System.Threading.CountdownEvent UpdateTrackerCompletionLatch;
			  internal readonly System.Threading.CountdownEvent StartSignal = new System.Threading.CountdownEvent( 1 );
			  internal volatile bool IsOnline;
			  internal Neo4Net.Test.Barrier_Control Barrier;

			  internal virtual void Initialize( int numberOfUpdateTrackers )
			  {
					UpdateTrackerCompletionLatch = new System.Threading.CountdownEvent( numberOfUpdateTrackers );
					if ( numberOfUpdateTrackers > 0 )
					{
						 Barrier = new Neo4Net.Test.Barrier_Control();
					}
			  }

			  internal virtual void UpdatesDone()
			  {
					UpdateTrackerCompletionLatch.Signal();
					try
					{
						 UpdateTrackerCompletionLatch.await();
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
					if ( Barrier != null )
					{
						 Barrier.reached();
					}
			  }

			  public override void IndexPopulationScanStarting()
			  {
					StartSignal.Signal();
			  }

			  /// <summary>
			  /// Index population is now completed, the populator hasn't yet been flipped and so sample hasn't been extracted.
			  /// The IndexPopulationJob, who is calling this method will now wait for the UpdatesTracker to notice that the index is online
			  /// so that it will complete whatever update it's doing and then snapshot its created/deleted values for later assertions.
			  /// When the UpdatesTracker notices this it will trigger this thread to continue and eventually call populationCompleteOn below,
			  /// completing the flip and the sampling. The barrier should prevent UpdatesTracker and IndexPopulationJob from racing in this area.
			  /// </summary>
			  public override void IndexPopulationScanComplete()
			  {
					IsOnline = true;
					if ( Barrier != null )
					{
						 try
						 {
							  Barrier.await();
						 }
						 catch ( InterruptedException e )
						 {
							  throw new Exception( e );
						 }
					}
			  }

			  public override void PopulationCompleteOn( StoreIndexDescriptor descriptor )
			  {
					if ( Barrier != null )
					{
						 Barrier.release();
					}
			  }

			  internal virtual bool IndexOnline
			  {
				  get
				  {
						return IsOnline;
				  }
			  }
		 }
	}

}