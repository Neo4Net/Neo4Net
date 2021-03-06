﻿using System;
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using Result = Org.Neo4j.Consistency.ConsistencyCheckService.Result;
	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using IdMapper = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Group = Org.Neo4j.@unsafe.Impl.Batchimport.input.Group;
	using Groups = Org.Neo4j.@unsafe.Impl.Batchimport.input.Groups;
	using InputChunk = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntity = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputEntity;
	using InputEntityVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputEntityVisitor;
	using Inputs = Org.Neo4j.@unsafe.Impl.Batchimport.input.Inputs;
	using ExecutionMonitor = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using StageExecution = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageExecution;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.IdMappers.longs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.IdMappers.strings;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Collectors.silentBadCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Inputs.knownEstimates;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ProcessorAssignmentStrategies.eagerRandomSaturation;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ParallelBatchImporterTest
	public class ParallelBatchImporterTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _random ).around( _fileSystemRule ).around( _suppressOutput );
		}

		 private const int NUMBER_OF_ID_GROUPS = 5;
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(random).around(fileSystemRule).around(suppressOutput);
		 public RuleChain RuleChain;

		 private const int NODE_COUNT = 10_000;
		 private const int RELATIONSHIPS_PER_NODE = 5;
		 private static readonly int _relationshipCount = NODE_COUNT * RELATIONSHIPS_PER_NODE;
		 private const int RELATIONSHIP_TYPES = 3;
		 protected internal readonly Configuration config = new ConfigurationAnonymousInnerClass();

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 public int batchSize()
			 {
				  // Set to extra low to exercise the internals a bit more.
				  return 100;
			 }

			 public int denseNodeThreshold()
			 {
				  // This will have statistically half the nodes be considered dense
				  return RELATIONSHIPS_PER_NODE * 2;
			 }

			 public int maxNumberOfProcessors()
			 {
				  // Let's really crank up the number of threads to try and flush out all and any parallelization issues.
				  int cores = Runtime.Runtime.availableProcessors();
				  return outerInstance.random.intBetween( cores, cores + 100 );
			 }

			 public long maxMemoryUsage()
			 {
				  // This calculation is just to try and hit some sort of memory limit so that relationship import
				  // is split up into multiple rounds. Also to see that relationship group defragmentation works
				  // well when doing multiple rounds.
				  double ratio = NODE_COUNT / 1_000D;
				  long mebi = mebiBytes( 1 );
				  return outerInstance.random.Next( ( int )( ratio * mebi / 2 ), ( int )( ratio * mebi ) );
			 }
		 }
		 private readonly InputIdGenerator _inputIdGenerator;
		 private readonly System.Func<Groups, IdMapper> _idMapper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0},{1},{3}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[]
			  {
				  new LongInputIdGenerator(),
				  ( System.Func<Groups, IdMapper> ) groups => longs( AUTO_WITHOUT_PAGECACHE, groups )
			  }, new object[]
			  {
				  new StringInputIdGenerator(),
				  ( System.Func<Groups, IdMapper> ) groups => strings( AUTO_WITHOUT_PAGECACHE, groups )
			  });
		 }

		 public ParallelBatchImporterTest( InputIdGenerator inputIdGenerator, System.Func<Groups, IdMapper> idMapper )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._inputIdGenerator = inputIdGenerator;
			  this._idMapper = idMapper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportCsvData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportCsvData()
		 {
			  // GIVEN
			  ExecutionMonitor processorAssigner = eagerRandomSaturation( config.maxNumberOfProcessors() );
			  CapturingMonitor monitor = new CapturingMonitor( processorAssigner );
			  DatabaseLayout databaseLayout = _directory.databaseLayout( "dir" + _random.nextAlphaNumericString( 8, 8 ) );

			  bool successful = false;
			  Groups groups = new Groups();
			  IdGroupDistribution groupDistribution = new IdGroupDistribution( NODE_COUNT, NUMBER_OF_ID_GROUPS, _random.random(), groups );
			  long nodeRandomSeed = _random.nextLong();
			  long relationshipRandomSeed = _random.nextLong();
			  JobScheduler jobScheduler = new ThreadPoolJobScheduler();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BatchImporter inserter = new ParallelBatchImporter(databaseLayout, fileSystemRule.get(), null, config, org.neo4j.logging.internal.NullLogService.getInstance(), monitor, EMPTY, org.neo4j.kernel.configuration.Config.defaults(), getFormat(), NO_MONITOR, jobScheduler);
			  BatchImporter inserter = new ParallelBatchImporter( databaseLayout, _fileSystemRule.get(), null, config, NullLogService.Instance, monitor, EMPTY, Config.defaults(), Format, NO_MONITOR, jobScheduler );
			  LongAdder propertyCount = new LongAdder();
			  LongAdder relationshipCount = new LongAdder();
			  try
			  {
					// WHEN
					inserter.DoImport( Inputs.input( Nodes( nodeRandomSeed, NODE_COUNT, config.batchSize(), _inputIdGenerator, groupDistribution, propertyCount ), Relationships(relationshipRandomSeed, _relationshipCount, config.batchSize(), _inputIdGenerator, groupDistribution, propertyCount, relationshipCount), _idMapper.apply(groups), silentBadCollector(_relationshipCount), knownEstimates(NODE_COUNT, _relationshipCount, NODE_COUNT * _tokens.Length / 2, _relationshipCount * _tokens.Length / 2, NODE_COUNT * _tokens.Length / 2 * Long.BYTES, _relationshipCount * _tokens.Length / 2 * Long.BYTES, NODE_COUNT * _tokens.Length / 2) ) );
					assertThat( monitor.AdditionalInformation, containsString( NODE_COUNT + " nodes" ) );
					assertThat( monitor.AdditionalInformation, containsString( relationshipCount + " relationships" ) );
					assertThat( monitor.AdditionalInformation, containsString( propertyCount + " properties" ) );

					// THEN
					GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseLayout.DatabaseDirectory()).setConfig("dbms.backup.enabled", "false").newGraphDatabase();
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 _inputIdGenerator.reset();
							 VerifyData( NODE_COUNT, _relationshipCount, db, groupDistribution, nodeRandomSeed, relationshipRandomSeed );
							 tx.Success();
							}
					}
					finally
					{
						 Db.shutdown();
					}
					AssertConsistent( databaseLayout );
					successful = true;
			  }
			  finally
			  {
					jobScheduler.close();
					if ( !successful )
					{
						 File failureFile = new File( databaseLayout.DatabaseDirectory(), "input" );
						 using ( PrintStream @out = new PrintStream( failureFile ) )
						 {
							  @out.println( "Seed used in this failing run: " + _random.seed() );
							  @out.println( _inputIdGenerator );
							  _inputIdGenerator.reset();
							  @out.println();
							  @out.println( "Processor assignments" );
							  @out.println( processorAssigner.ToString() );
						 }
						 Console.Error.WriteLine( "Additional debug information stored in " + failureFile );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertConsistent(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 protected internal virtual void AssertConsistent( DatabaseLayout databaseLayout )
		 {
			  ConsistencyCheckService consistencyChecker = new ConsistencyCheckService();
			  ConsistencyCheckService.Result result = consistencyChecker.RunFullConsistencyCheck( databaseLayout, Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" ), ProgressMonitorFactory.NONE, NullLogProvider.Instance, false );
			  assertTrue( "Database contains inconsistencies, there should be a report in " + databaseLayout.DatabaseDirectory(), result.Successful );
		 }

		 protected internal virtual RecordFormats Format
		 {
			 get
			 {
				  return Standard.LATEST_RECORD_FORMATS;
			 }
		 }

		 private class ExistingId
		 {
			  internal readonly object Id;
			  internal readonly long NodeIndex;

			  internal ExistingId( object id, long nodeIndex )
			  {
					this.Id = id;
					this.NodeIndex = nodeIndex;
			  }
		 }

		 public abstract class InputIdGenerator
		 {
			  internal abstract void Reset();

			  internal abstract object NextNodeId( RandomValues random, long item );

			  internal abstract ExistingId RandomExisting( RandomValues random );

			  internal abstract object Miss( RandomValues random, object id, float chance );

			  internal abstract bool IsMiss( object id );

			  internal virtual string RandomType( RandomValues random )
			  {
					return "TYPE" + random.Next( RELATIONSHIP_TYPES );
			  }

			  public override string ToString()
			  {
					return this.GetType().Name;
			  }
		 }

		 private class LongInputIdGenerator : InputIdGenerator
		 {
			  internal override void Reset()
			  {
			  }

			  internal override object NextNodeId( RandomValues random, long item )
			  {
				  lock ( this )
				  {
						return item;
				  }
			  }

			  internal override ExistingId RandomExisting( RandomValues random )
			  {
					long index = random.Next( NODE_COUNT );
					return new ExistingId( index, index );
			  }

			  internal override object Miss( RandomValues random, object id, float chance )
			  {
					return random.NextFloat() < chance ? (long?) id + 100_000_000 : id;
			  }

			  internal override bool IsMiss( object id )
			  {
					return ( long? ) id >= 100_000_000;
			  }
		 }

		 private class StringInputIdGenerator : InputIdGenerator
		 {
			  internal readonly string[] Strings = new string[NODE_COUNT];

			  internal override void Reset()
			  {
					Arrays.fill( Strings, null );
			  }

			  internal override object NextNodeId( RandomValues random, long item )
			  {
					sbyte[] randomBytes = random.NextByteArray( 10, 10 ).asObjectCopy();
					string result = System.Guid.nameUUIDFromBytes( randomBytes ).ToString();
					Strings[toIntExact( item )] = result;
					return result;
			  }

			  internal override ExistingId RandomExisting( RandomValues random )
			  {
					int index = random.Next( Strings.Length );
					return new ExistingId( Strings[index], index );
			  }

			  internal override object Miss( RandomValues random, object id, float chance )
			  {
					return random.NextFloat() < chance ? "_" + id : id;
			  }

			  internal override bool IsMiss( object id )
			  {
					return ( ( string )id ).StartsWith( "_", StringComparison.Ordinal );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyData(int nodeCount, int relationshipCount, org.neo4j.graphdb.GraphDatabaseService db, IdGroupDistribution groups, long nodeRandomSeed, long relationshipRandomSeed) throws java.io.IOException
		 private void VerifyData( int nodeCount, int relationshipCount, GraphDatabaseService db, IdGroupDistribution groups, long nodeRandomSeed, long relationshipRandomSeed )
		 {
			  // Read all nodes, relationships and properties ad verify against the input data.
			  LongAdder propertyCount = new LongAdder();
			  using ( InputIterator nodes = nodes( nodeRandomSeed, nodeCount, config.batchSize(), _inputIdGenerator, groups, propertyCount ).GetEnumerator(), InputIterator relationships = relationships(relationshipRandomSeed, relationshipCount, config.batchSize(), _inputIdGenerator, groups, propertyCount, new LongAdder()).GetEnumerator(), ResourceIterator<Node> dbNodes = Db.AllNodes.GetEnumerator() )
			  {
					// Nodes
					IDictionary<string, Node> nodeByInputId = new Dictionary<string, Node>( nodeCount );
					while ( dbNodes.MoveNext() )
					{
						 Node node = dbNodes.Current;
						 string id = ( string ) node.GetProperty( "id" );
						 assertNull( nodeByInputId.put( id, node ) );
					}

					int verifiedNodes = 0;
					long allNodesScanLabelCount = 0;
					InputChunk chunk = nodes.NewChunk();
					InputEntity input = new InputEntity();
					while ( nodes.Next( chunk ) )
					{
						 while ( chunk.Next( input ) )
						 {
							  string iid = UniqueId( input.IdGroup, input.ObjectId );
							  Node node = nodeByInputId[iid];
							  AssertNodeEquals( input, node );
							  verifiedNodes++;
							  AssertDegrees( node );
							  allNodesScanLabelCount += Iterables.count( node.Labels );
						 }
					}
					assertEquals( nodeCount, verifiedNodes );

					// Labels
					long labelScanStoreEntryCount = Db.AllLabels.stream().flatMap(l => Db.findNodes(l).stream()).count();

					assertEquals( format( "Expected label scan store and node store to have same number labels. But %n" + "#labelsInNodeStore=%d%n" + "#labelsInLabelScanStore=%d%n", allNodesScanLabelCount, labelScanStoreEntryCount ), allNodesScanLabelCount, labelScanStoreEntryCount );

					// Relationships
					chunk = relationships.NewChunk();
					IDictionary<string, Relationship> relationshipByName = new Dictionary<string, Relationship>();
					foreach ( Relationship relationship in Db.AllRelationships )
					{
						 relationshipByName[( string ) relationship.GetProperty( "id" )] = relationship;
					}
					int verifiedRelationships = 0;
					while ( relationships.Next( chunk ) )
					{
						 while ( chunk.Next( input ) )
						 {
							  if ( !_inputIdGenerator.isMiss( input.ObjectStartId ) && !_inputIdGenerator.isMiss( input.ObjectEndId ) )
							  {
									// A relationship referring to missing nodes. The InputIdGenerator is expected to generate
									// some (very few) of those. Skip it.
									string name = ( string ) PropertyOf( input, "id" );
									Relationship relationship = relationshipByName[name];
									assertNotNull( "Expected there to be a relationship with name '" + name + "'", relationship );
									assertEquals( nodeByInputId[UniqueId( input.StartIdGroup, input.ObjectStartId )], relationship.StartNode );
									assertEquals( nodeByInputId[UniqueId( input.EndIdGroup, input.ObjectEndId )], relationship.EndNode );
									AssertRelationshipEquals( input, relationship );
							  }
							  verifiedRelationships++;
						 }
					}
					assertEquals( relationshipCount, verifiedRelationships );
			  }
		 }

		 private void AssertDegrees( Node node )
		 {
			  foreach ( RelationshipType type in node.RelationshipTypes )
			  {
					foreach ( Direction direction in Direction.values() )
					{
						 long degree = node.GetDegree( type, direction );
						 long actualDegree = count( node.GetRelationships( type, direction ) );
						 assertEquals( actualDegree, degree );
					}
			  }
		 }

		 private string UniqueId( Group group, PropertyContainer entity )
		 {
			  return UniqueId( group, entity.GetProperty( "id" ) );
		 }

		 private string UniqueId( Group group, object id )
		 {
			  return group.Name() + "_" + id;
		 }

		 private object PropertyOf( InputEntity input, string key )
		 {
			  object[] properties = input.Properties();
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					if ( properties[i++].Equals( key ) )
					{
						 return properties[i];
					}
			  }
			  throw new System.InvalidOperationException( key + " not found on " + input );
		 }

		 private void AssertRelationshipEquals( InputEntity input, Relationship relationship )
		 {
			  // properties
			  AssertPropertiesEquals( input, relationship );

			  // type
			  assertEquals( input.StringType, relationship.Type.name() );
		 }

		 private void AssertNodeEquals( InputEntity input, Node node )
		 {
			  // properties
			  AssertPropertiesEquals( input, node );

			  // labels
			  ISet<string> expectedLabels = asSet( input.Labels() );
			  foreach ( Label label in node.Labels )
			  {
					assertTrue( expectedLabels.remove( label.Name() ) );
			  }
			  assertTrue( expectedLabels.Count == 0 );
		 }

		 private void AssertPropertiesEquals( InputEntity input, PropertyContainer entity )
		 {
			  object[] properties = input.Properties();
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					string key = ( string ) properties[i++];
					object value = properties[i];
					AssertPropertyValueEquals( input, entity, key, value, entity.GetProperty( key ) );
			  }
		 }

		 private void AssertPropertyValueEquals( InputEntity input, PropertyContainer entity, string key, object expected, object array )
		 {
			  if ( expected.GetType().IsArray )
			  {
					int length = Array.getLength( expected );
					assertEquals( input + ", " + entity, length, Array.getLength( array ) );
					for ( int i = 0; i < length; i++ )
					{
						 AssertPropertyValueEquals( input, entity, key, Array.get( expected, i ), Array.get( array, i ) );
					}
			  }
			  else
			  {
					assertEquals( input + ", " + entity + " for key:" + key, Values.of( expected ), Values.of( array ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private InputIterable relationships(final long randomSeed, final long count, int batchSize, final InputIdGenerator idGenerator, final IdGroupDistribution groups, java.util.concurrent.atomic.LongAdder propertyCount, java.util.concurrent.atomic.LongAdder relationshipCount)
		 private InputIterable Relationships( long randomSeed, long count, int batchSize, InputIdGenerator idGenerator, IdGroupDistribution groups, LongAdder propertyCount, LongAdder relationshipCount )
		 {
			  return () => new GeneratingInputIterator<>(count, batchSize, new RandomsStates(randomSeed), (randoms, visitor, id) =>
			  {
			  int thisPropertyCount = RandomProperties( randoms, "Name " + id, visitor );
			  ExistingId startNodeExistingId = idGenerator.RandomExisting( randoms );
			  Group startNodeGroup = groups.GroupOf( startNodeExistingId.NodeIndex );
			  ExistingId endNodeExistingId = idGenerator.RandomExisting( randoms );
			  Group endNodeGroup = groups.GroupOf( endNodeExistingId.NodeIndex );
			  object startNode = idGenerator.Miss( randoms, startNodeExistingId.Id, 0.001f );
			  object endNode = idGenerator.Miss( randoms, endNodeExistingId.Id, 0.001f );
			  if ( !_inputIdGenerator.isMiss( startNode ) && !_inputIdGenerator.isMiss( endNode ) )
			  {
				  relationshipCount.increment();
				  propertyCount.add( thisPropertyCount );
			  }
			  visitor.startId( startNode, startNodeGroup );
			  visitor.endId( endNode, endNodeGroup );
			  string type = idGenerator.RandomType( randoms );
			  if ( randoms.nextFloat() < 0.00005 )
			  {
				  type += "_odd";
			  }
			  visitor.type( type );
			  }, 0);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private InputIterable nodes(final long randomSeed, final long count, int batchSize, final InputIdGenerator inputIdGenerator, final IdGroupDistribution groups, java.util.concurrent.atomic.LongAdder propertyCount)
		 private InputIterable Nodes( long randomSeed, long count, int batchSize, InputIdGenerator inputIdGenerator, IdGroupDistribution groups, LongAdder propertyCount )
		 {
			  return () => new GeneratingInputIterator<>(count, batchSize, new RandomsStates(randomSeed), (randoms, visitor, id) =>
			  {
			  object nodeId = inputIdGenerator.NextNodeId( randoms, id );
			  Group group = groups.GroupOf( id );
			  visitor.id( nodeId, group );
			  propertyCount.add( RandomProperties( randoms, UniqueId( group, nodeId ), visitor ) );
			  visitor.labels( randoms.selection( _tokens, 0, _tokens.Length, true ) );
			  }, 0);
		 }

		 private static readonly string[] _tokens = new string[] { "token1", "token2", "token3", "token4", "token5", "token6", "token7" };

		 private int RandomProperties( RandomValues randoms, object id, InputEntityVisitor visitor )
		 {
			  string[] keys = randoms.Selection( _tokens, 0, _tokens.Length, false );
			  foreach ( string key in keys )
			  {
					visitor.Property( key, randoms.NextValue().asObject() );
			  }
			  visitor.Property( "id", id );
			  return keys.Length + 1;
		 }

		 private class CapturingMonitor : ExecutionMonitor
		 {
			  internal readonly ExecutionMonitor Delegate;
			  internal string AdditionalInformation;

			  internal CapturingMonitor( ExecutionMonitor @delegate )
			  {
					this.Delegate = @delegate;
			  }

			  public override void Initialize( DependencyResolver dependencyResolver )
			  {
					Delegate.initialize( dependencyResolver );
			  }

			  public override void Start( StageExecution execution )
			  {
					Delegate.start( execution );
			  }

			  public override void End( StageExecution execution, long totalTimeMillis )
			  {
					Delegate.end( execution, totalTimeMillis );
			  }

			  public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
			  {
					this.AdditionalInformation = additionalInformation;
					Delegate.done( successful, totalTimeMillis, additionalInformation );
			  }

			  public override long NextCheckTime()
			  {
					return Delegate.nextCheckTime();
			  }

			  public override void Check( StageExecution execution )
			  {
					Delegate.check( execution );
			  }
		 }
	}

}