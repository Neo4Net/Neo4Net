using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using QueryParserUtil = org.apache.lucene.queryparser.flexible.standard.QueryParserUtil;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using Timeout = org.junit.rules.Timeout;


	using Entity = Neo4Net.Graphdb.Entity;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using RepeatedLabelInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedLabelInSchemaException;
	using RepeatedPropertyInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedPropertyInSchemaException;
	using RepeatedRelationshipTypeInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedRelationshipTypeInSchemaException;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadTestUtils = Neo4Net.Test.ThreadTestUtils;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.DependencyResolver_SelectionStrategy.ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;

	public class FulltextProceduresTest
	{
		private bool InstanceFieldsInitialized = false;

		public FulltextProceduresTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _timeout ).around( _fs ).around( _testDirectory ).around( _expectedException ).around( _cleanup );
		}

		 private const string DB_INDEXES = "CALL db.indexes";
		 private const string DROP = "CALL db.index.fulltext.drop(\"%s\")";
		 private const string LIST_AVAILABLE_ANALYZERS = "CALL db.index.fulltext.listAvailableAnalyzers()";
		 private const string DB_AWAIT_INDEX = "CALL db.index.fulltext.awaitIndex(\"%s\")";
		 internal const string QUERY_NODES = "CALL db.index.fulltext.queryNodes(\"%s\", \"%s\")";
		 internal const string QUERY_RELS = "CALL db.index.fulltext.queryRelationships(\"%s\", \"%s\")";
		 internal const string AWAIT_REFRESH = "CALL db.index.fulltext.awaitEventuallyConsistentIndexRefresh()";
		 internal const string NODE_CREATE = "CALL db.index.fulltext.createNodeIndex(\"%s\", %s, %s )";
		 internal const string RELATIONSHIP_CREATE = "CALL db.index.fulltext.createRelationshipIndex(\"%s\", %s, %s)";

		 private const string SCORE = "score";
		 internal const string NODE = "node";
		 internal const string RELATIONSHIP = "relationship";
		 private const string DESCARTES_MEDITATIONES = "/meditationes--rene-descartes--public-domain.txt";
		 private static readonly Label _label = Label.label( "Label" );
		 private static readonly RelationshipType _rel = RelationshipType.withName( "REL" );

		 private readonly Timeout _timeout = VerboseTimeout.builder().withTimeout(1, TimeUnit.HOURS).build();
		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly CleanupRule _cleanup = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(timeout).around(fs).around(testDirectory).around(expectedException).around(cleanup);
		 public RuleChain Rules;

		 private GraphDatabaseAPI _db;
		 private GraphDatabaseBuilder _builder;
		 private const string PROP = "prop";
		 private const string EVENTUALLY_CONSISTENT = ", {eventually_consistent: 'true'}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  GraphDatabaseFactory factory = new GraphDatabaseFactory();
			  _builder = factory.NewEmbeddedDatabaseBuilder( _testDirectory.databaseDir() );
			  _builder.setConfig( GraphDatabaseSettings.store_internal_log_level, "DEBUG" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createNodeFulltextIndex()
		 public virtual void CreateNodeFulltextIndex()
		 {
			  _db = CreateDatabase();
			  _db.execute( format( NODE_CREATE, "test-index", Array( "Label1", "Label2" ), Array( "prop1", "prop2" ) ) ).close();
			  Result result;
			  IDictionary<string, object> row;
			  using ( Transaction tx = _db.beginTx() )
			  {
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					row = result.Next();
					assertEquals( "INDEX ON NODE:Label1, Label2(prop1, prop2)", row["description"] );
					assertEquals( asList( "Label1", "Label2" ), row["tokenNames"] );
					assertEquals( asList( "prop1", "prop2" ), row["properties"] );
					assertEquals( "test-index", row["indexName"] );
					assertEquals( "node_fulltext", row["type"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					AwaitIndexesOnline();
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "ONLINE", result.Next()["state"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					assertNotNull( _db.schema().getIndexByName("test-index") );
					tx.Success();
			  }
			  _db.shutdown();
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					row = result.Next();
					assertEquals( "INDEX ON NODE:Label1, Label2(prop1, prop2)", row["description"] );
					assertEquals( "ONLINE", row["state"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					//noinspection ConstantConditions
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					assertNotNull( _db.schema().getIndexByName("test-index") );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createRelationshipFulltextIndex()
		 public virtual void CreateRelationshipFulltextIndex()
		 {
			  _db = CreateDatabase();
			  _db.execute( format( RELATIONSHIP_CREATE, "test-index", Array( "Reltype1", "Reltype2" ), Array( "prop1", "prop2" ) ) ).close();
			  Result result;
			  IDictionary<string, object> row;
			  using ( Transaction tx = _db.beginTx() )
			  {
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					row = result.Next();
					assertEquals( "INDEX ON RELATIONSHIP:Reltype1, Reltype2(prop1, prop2)", row["description"] );
					assertEquals( asList( "Reltype1", "Reltype2" ), row["tokenNames"] );
					assertEquals( asList( "prop1", "prop2" ), row["properties"] );
					assertEquals( "test-index", row["indexName"] );
					assertEquals( "relationship_fulltext", row["type"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					AwaitIndexesOnline();
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "ONLINE", result.Next()["state"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					assertNotNull( _db.schema().getIndexByName("test-index") );
					tx.Success();
			  }
			  _db.shutdown();
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					result = _db.execute( DB_INDEXES );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					row = result.Next();
					assertEquals( "INDEX ON RELATIONSHIP:Reltype1, Reltype2(prop1, prop2)", row["description"] );
					assertEquals( "ONLINE", row["state"] );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					//noinspection ConstantConditions
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					assertNotNull( _db.schema().getIndexByName("test-index") );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropIndex()
		 public virtual void DropIndex()
		 {
			  _db = CreateDatabase();
			  _db.execute( format( NODE_CREATE, "node", Array( "Label1", "Label2" ), Array( "prop1", "prop2" ) ) ).close();
			  _db.execute( format( RELATIONSHIP_CREATE, "rel", Array( "Reltype1", "Reltype2" ), Array( "prop1", "prop2" ) ) ).close();
			  IDictionary<string, string> indexes = new Dictionary<string, string>();
			  _db.execute( "call db.indexes" ).forEachRemaining( m => indexes.put( ( string ) m.get( "indexName" ), ( string ) m.get( "description" ) ) );

			  _db.execute( format( DROP, "node" ) );
			  indexes.Remove( "node" );
			  IDictionary<string, string> newIndexes = new Dictionary<string, string>();
			  _db.execute( "call db.indexes" ).forEachRemaining( m => newIndexes.put( ( string ) m.get( "indexName" ), ( string ) m.get( "description" ) ) );
			  assertEquals( indexes, newIndexes );

			  _db.execute( format( DROP, "rel" ) );
			  indexes.Remove( "rel" );
			  newIndexes.Clear();
			  _db.execute( "call db.indexes" ).forEachRemaining( m => newIndexes.put( ( string ) m.get( "indexName" ), ( string ) m.get( "description" ) ) );
			  assertEquals( indexes, newIndexes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotBeAbleToCreateTwoIndexesWithSameName()
		 public virtual void MustNotBeAbleToCreateTwoIndexesWithSameName()
		 {
			  _db = CreateDatabase();
			  _db.execute( format( NODE_CREATE, "node", Array( "Label1", "Label2" ), Array( "prop1", "prop2" ) ) ).close();
			  _expectedException.expectMessage( "already exists" );
			  _db.execute( format( NODE_CREATE, "node", Array( "Label1", "Label2" ), Array( "prop3", "prop4" ) ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeIndexesMustHaveLabels()
		 public virtual void NodeIndexesMustHaveLabels()
		 {
			  _db = CreateDatabase();
			  _expectedException.expect( typeof( QueryExecutionException ) );
			  _db.execute( format( NODE_CREATE, "nodeIndex", Array(), Array(PROP) ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIndexesMustHaveRelationshipTypes()
		 public virtual void RelationshipIndexesMustHaveRelationshipTypes()
		 {
			  _db = CreateDatabase();
			  _expectedException.expect( typeof( QueryExecutionException ) );
			  _db.execute( format( RELATIONSHIP_CREATE, "relIndex", Array(), Array(PROP) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeIndexesMustHaveProperties()
		 public virtual void NodeIndexesMustHaveProperties()
		 {
			  _db = CreateDatabase();
			  _expectedException.expect( typeof( QueryExecutionException ) );
			  _db.execute( format( NODE_CREATE, "nodeIndex", Array( "Label" ), Array() ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIndexesMustHaveProperties()
		 public virtual void RelationshipIndexesMustHaveProperties()
		 {
			  _db = CreateDatabase();
			  _expectedException.expect( typeof( QueryExecutionException ) );
			  _db.execute( format( RELATIONSHIP_CREATE, "relIndex", Array( "RELTYPE" ), Array() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexesWhichImpliesTokenCreateMustNotBlockForever()
		 public virtual void CreatingIndexesWhichImpliesTokenCreateMustNotBlockForever()
		 {
			  _db = CreateDatabase();

			  using ( Transaction ignore = _db.beginTx() )
			  {
					// The property keys and labels we ask for do not exist, so those tokens will have to be allocated.
					// This test verifies that the locking required for the index modifications do not conflict with the
					// locking required for the token allocation.
					_db.execute( format( NODE_CREATE, "nodesA", Array( "SOME_LABEL" ), Array( "this" ) ) );
					_db.execute( format( RELATIONSHIP_CREATE, "relsA", Array( "SOME_REL_TYPE" ), Array( "foo" ) ) );
					_db.execute( format( NODE_CREATE, "nodesB", Array( "SOME_OTHER_LABEL" ), Array( "that" ) ) );
					_db.execute( format( RELATIONSHIP_CREATE, "relsB", Array( "SOME_OTHER_REL_TYPE" ), Array( "bar" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexWithSpecificAnalyzerMustUseThatAnalyzerForPopulationUpdatingAndQuerying()
		 public virtual void CreatingIndexWithSpecificAnalyzerMustUseThatAnalyzerForPopulationUpdatingAndQuerying()
		 {
			  _db = CreateDatabase();
			  LongHashSet noResults = new LongHashSet();
			  LongHashSet swedishNodes = new LongHashSet();
			  LongHashSet englishNodes = new LongHashSet();
			  LongHashSet swedishRels = new LongHashSet();
			  LongHashSet englishRels = new LongHashSet();

			  string labelledSwedishNodes = "labelledSwedishNodes";
			  string typedSwedishRelationships = "typedSwedishRelationships";

			  using ( Transaction tx = _db.beginTx() )
			  {
					// Nodes and relationships picked up by index population.
					Node nodeA = _db.createNode( _label );
					nodeA.SetProperty( PROP, "En apa och en tomte bodde i ett hus." );
					swedishNodes.add( nodeA.Id );
					Node nodeB = _db.createNode( _label );
					nodeB.SetProperty( PROP, "Hello and hello again, in the end." );
					englishNodes.add( nodeB.Id );
					Relationship relA = nodeA.CreateRelationshipTo( nodeB, _rel );
					relA.SetProperty( PROP, "En apa och en tomte bodde i ett hus." );
					swedishRels.add( relA.Id );
					Relationship relB = nodeB.CreateRelationshipTo( nodeA, _rel );
					relB.SetProperty( PROP, "Hello and hello again, in the end." );
					englishRels.add( relB.Id );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					string lbl = Array( _label.name() );
					string rel = Array( _rel.name() );
					string props = Array( PROP );
					string swedish = props + ", {analyzer: '" + FulltextAnalyzerTest.SWEDISH + "'}";
					_db.execute( format( NODE_CREATE, labelledSwedishNodes, lbl, swedish ) ).close();
					_db.execute( format( RELATIONSHIP_CREATE, typedSwedishRelationships, rel, swedish ) ).close();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					// Nodes and relationships picked up by index updates.
					Node nodeC = _db.createNode( _label );
					nodeC.SetProperty( PROP, "En apa och en tomte bodde i ett hus." );
					swedishNodes.add( nodeC.Id );
					Node nodeD = _db.createNode( _label );
					nodeD.SetProperty( PROP, "Hello and hello again, in the end." );
					englishNodes.add( nodeD.Id );
					Relationship relC = nodeC.CreateRelationshipTo( nodeD, _rel );
					relC.SetProperty( PROP, "En apa och en tomte bodde i ett hus." );
					swedishRels.add( relC.Id );
					Relationship relD = nodeD.CreateRelationshipTo( nodeC, _rel );
					relD.SetProperty( PROP, "Hello and hello again, in the end." );
					englishRels.add( relD.Id );
					tx.Success();
			  }
			  using ( Transaction ignore = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, labelledSwedishNodes, "and", englishNodes ); // english word
					// swedish stop word (ignored by swedish analyzer, and not among the english nodes)
					AssertQueryFindsIds( _db, true, labelledSwedishNodes, "ett", noResults );
					AssertQueryFindsIds( _db, true, labelledSwedishNodes, "apa", swedishNodes ); // swedish word

					AssertQueryFindsIds( _db, false, typedSwedishRelationships, "and", englishRels );
					AssertQueryFindsIds( _db, false, typedSwedishRelationships, "ett", noResults );
					AssertQueryFindsIds( _db, false, typedSwedishRelationships, "apa", swedishRels );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryShouldFindDataAddedInLaterTransactions()
		 public virtual void QueryShouldFindDataAddedInLaterTransactions()
		 {
			  _db = CreateDatabase();
			  _db.execute( format( NODE_CREATE, "node", Array( "Label1", "Label2" ), Array( "prop1", "prop2" ) ) ).close();
			  _db.execute( format( RELATIONSHIP_CREATE, "rel", Array( "Reltype1", "Reltype2" ), Array( "prop1", "prop2" ) ) ).close();
			  AwaitIndexesOnline();
			  long horseId;
			  long horseRelId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node zebra = _db.createNode();
					zebra.SetProperty( "prop1", "zebra" );
					Node horse = _db.createNode( Label.label( "Label1" ) );
					horse.SetProperty( "prop2", "horse" );
					horse.SetProperty( "prop3", "zebra" );
					Relationship horseRel = zebra.CreateRelationshipTo( horse, RelationshipType.withName( "Reltype1" ) );
					horseRel.SetProperty( "prop1", "horse" );
					Relationship loop = horse.CreateRelationshipTo( horse, RelationshipType.withName( "loop" ) );
					loop.SetProperty( "prop2", "zebra" );

					horseId = horse.Id;
					horseRelId = horseRel.Id;
					tx.Success();
			  }
			  AssertQueryFindsIds( _db, true, "node", "horse", newSetWith( horseId ) );
			  AssertQueryFindsIds( _db, true, "node", "horse zebra", newSetWith( horseId ) );

			  AssertQueryFindsIds( _db, false, "rel", "horse", newSetWith( horseRelId ) );
			  AssertQueryFindsIds( _db, false, "rel", "horse zebra", newSetWith( horseRelId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryShouldFindDataAddedInIndexPopulation()
		 public virtual void QueryShouldFindDataAddedInIndexPopulation()
		 {
			  // when
			  Node node1;
			  Node node2;
			  Relationship relationship;
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					node1 = _db.createNode( _label );
					node1.SetProperty( PROP, "This is a integration test." );
					node2 = _db.createNode( _label );
					node2.SetProperty( "otherprop", "This is a related integration test" );
					relationship = node1.CreateRelationshipTo( node2, _rel );
					relationship.SetProperty( PROP, "They relate" );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP, "otherprop") ) );
					_db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) ) );
					tx.Success();
			  }
			  AwaitIndexesOnline();

			  // then
			  AssertQueryFindsIds( _db, true, "node", "integration", node1.Id, node2.Id );
			  AssertQueryFindsIds( _db, true, "node", "test", node1.Id, node2.Id );
			  AssertQueryFindsIds( _db, true, "node", "related", newSetWith( node2.Id ) );
			  AssertQueryFindsIds( _db, false, "rel", "relate", newSetWith( relationship.Id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatesToEventuallyConsistentIndexMustEventuallyBecomeVisible()
		 public virtual void UpdatesToEventuallyConsistentIndexMustEventuallyBecomeVisible()
		 {
			  string value = "bla bla";
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					_db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					tx.Success();
			  }

			  int entityCount = 200;
			  LongHashSet nodeIds = new LongHashSet();
			  LongHashSet relIds = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < entityCount; i++ )
					{
						 Node node = _db.createNode( _label );
						 node.SetProperty( PROP, value );
						 Relationship rel = node.CreateRelationshipTo( node, _rel );
						 rel.SetProperty( PROP, value );
						 nodeIds.add( node.Id );
						 relIds.add( rel.Id );
					}
					tx.Success();
			  }

			  // Assert that we can observe our updates within 20 seconds from now. We have, after all, already committed the transaction.
			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.SECONDS.toMillis(20);
			  bool success = false;
			  do
			  {
					try
					{
						 AssertQueryFindsIds( _db, true, "node", "bla", nodeIds );
						 AssertQueryFindsIds( _db, false, "rel", "bla", relIds );
						 success = true;
					}
					catch ( Exception throwable )
					{
						 if ( deadline <= DateTimeHelper.CurrentUnixTimeMillis() )
						 {
							  // We're past the deadline. This test is not successful.
							  throw throwable;
						 }
					}
			  } while ( !success );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatesToEventuallyConsistentIndexMustBecomeVisibleAfterAwaitRefresh()
		 public virtual void UpdatesToEventuallyConsistentIndexMustBecomeVisibleAfterAwaitRefresh()
		 {
			  string value = "bla bla";
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					_db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					tx.Success();
			  }
			  AwaitIndexesOnline();

			  int entityCount = 200;
			  LongHashSet nodeIds = new LongHashSet();
			  LongHashSet relIds = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < entityCount; i++ )
					{
						 Node node = _db.createNode( _label );
						 node.SetProperty( PROP, value );
						 Relationship rel = node.CreateRelationshipTo( node, _rel );
						 rel.SetProperty( PROP, value );
						 nodeIds.add( node.Id );
						 relIds.add( rel.Id );
					}
					tx.Success();
			  }

			  _db.execute( AWAIT_REFRESH ).close();
			  AssertQueryFindsIds( _db, true, "node", "bla", nodeIds );
			  AssertQueryFindsIds( _db, false, "rel", "bla", relIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eventuallyConsistentIndexMustPopulateWithExistingDataWhenCreated()
		 public virtual void EventuallyConsistentIndexMustPopulateWithExistingDataWhenCreated()
		 {
			  string value = "bla bla";
			  _db = CreateDatabase();

			  int entityCount = 200;
			  LongHashSet nodeIds = new LongHashSet();
			  LongHashSet relIds = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < entityCount; i++ )
					{
						 Node node = _db.createNode( _label );
						 node.SetProperty( PROP, value );
						 Relationship rel = node.CreateRelationshipTo( node, _rel );
						 rel.SetProperty( PROP, value );
						 nodeIds.add( node.Id );
						 relIds.add( rel.Id );
					}
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					_db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					tx.Success();
			  }

			  AwaitIndexesOnline();
			  AssertQueryFindsIds( _db, true, "node", "bla", nodeIds );
			  AssertQueryFindsIds( _db, false, "rel", "bla", relIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentPopulationAndUpdatesToAnEventuallyConsistentIndexMustEventuallyResultInCorrectIndexState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentPopulationAndUpdatesToAnEventuallyConsistentIndexMustEventuallyResultInCorrectIndexState()
		 {
			  string oldValue = "red";
			  string newValue = "green";
			  _db = CreateDatabase();

			  int entityCount = 200;
			  LongHashSet nodeIds = new LongHashSet();
			  LongHashSet relIds = new LongHashSet();

			  // First we create the nodes and relationships with the property value "red".
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < entityCount; i++ )
					{
						 Node node = _db.createNode( _label );
						 node.SetProperty( PROP, oldValue );
						 Relationship rel = node.CreateRelationshipTo( node, _rel );
						 rel.SetProperty( PROP, oldValue );
						 nodeIds.add( node.Id );
						 relIds.add( rel.Id );
					}
					tx.Success();
			  }

			  // Then, in two concurrent transactions, we create our indexes AND change all the property values to "green".
			  System.Threading.CountdownEvent readyLatch = new System.Threading.CountdownEvent( 2 );
			  BinaryLatch startLatch = new BinaryLatch();
			  ThreadStart createIndexes = () =>
			  {
				readyLatch.Signal();
				startLatch.Await();
				using ( Transaction tx = _db.beginTx() )
				{
					 _db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					 _db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) );
					 tx.Success();
				}
			  };
			  ThreadStart makeAllEntitiesGreen = () =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 // Prepare our transaction state first.
					 nodeIds.forEach( nodeId => _db.getNodeById( nodeId ).setProperty( PROP, newValue ) );
					 relIds.forEach( relId => _db.getRelationshipById( relId ).setProperty( PROP, newValue ) );
					 tx.Success();
					 // Okay, NOW we're ready to race!
					 readyLatch.Signal();
					 startLatch.Await();
				}
			  };
			  ExecutorService executor = _cleanup.add( Executors.newFixedThreadPool( 2 ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future1 = executor.submit(createIndexes);
			  Future<object> future1 = executor.submit( createIndexes );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future2 = executor.submit(makeAllEntitiesGreen);
			  Future<object> future2 = executor.submit( makeAllEntitiesGreen );
			  readyLatch.await();
			  startLatch.Release();

			  // Finally, when everything has settled down, we should see that all of the nodes and relationships are indexed with the value "green".
			  future1.get();
			  future2.get();
			  AwaitIndexesOnline();
			  _db.execute( AWAIT_REFRESH ).close();
			  AssertQueryFindsIds( _db, true, "node", newValue, nodeIds );
			  AssertQueryFindsIds( _db, false, "rel", newValue, relIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexesMustBeEventuallyConsistentByDefaultWhenThisIsConfigured() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FulltextIndexesMustBeEventuallyConsistentByDefaultWhenThisIsConfigured()
		 {
			  _builder.setConfig( FulltextConfig.EventuallyConsistent, Settings.TRUE );
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "node", Array( _label.name() ), Array(PROP, "otherprop") ) );
					_db.execute( format( RELATIONSHIP_CREATE, "rel", Array( _rel.name() ), Array(PROP) ) );
					tx.Success();
			  }
			  AwaitIndexesOnline();

			  // Prevent index updates from being applied to eventually consistent indexes.
			  BinaryLatch indexUpdateBlocker = new BinaryLatch();
			  _db.DependencyResolver.resolveDependency( typeof( IJobScheduler ), ONLY ).schedule( Group.INDEX_UPDATING, indexUpdateBlocker.await );

			  LongHashSet nodeIds = new LongHashSet();
			  long relId;
			  try
			  {
					using ( Transaction tx = _db.beginTx() )
					{
						 Node node1 = _db.createNode( _label );
						 node1.SetProperty( PROP, "bla bla" );
						 Node node2 = _db.createNode( _label );
						 node2.SetProperty( "otherprop", "bla bla" );
						 Relationship relationship = node1.CreateRelationshipTo( node2, _rel );
						 relationship.SetProperty( PROP, "bla bla" );
						 nodeIds.add( node1.Id );
						 nodeIds.add( node2.Id );
						 relId = relationship.Id;
						 tx.Success();
					}

					// Index updates are still blocked for eventually consistent indexes, so we should not find anything at this point.
					AssertQueryFindsIds( _db, true, "node", "bla", new LongHashSet() );
					AssertQueryFindsIds( _db, false, "rel", "bla", new LongHashSet() );
			  }
			  finally
			  {
					// Uncork the eventually consistent fulltext index updates.
					Thread.Sleep( 10 );
					indexUpdateBlocker.Release();
			  }
			  // And wait for them to apply.
			  _db.execute( AWAIT_REFRESH ).close();

			  // Now we should see our data.
			  AssertQueryFindsIds( _db, true, "node", "bla", nodeIds );
			  AssertQueryFindsIds( _db, false, "rel", "bla", newSetWith( relId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToListAvailableAnalyzers()
		 public virtual void MustBeAbleToListAvailableAnalyzers()
		 {
			  _db = CreateDatabase();

			  // Verify that a couple of expected analyzers are available.
			  using ( Transaction tx = _db.beginTx() )
			  {
					ISet<string> analyzers = new HashSet<string>();
					using ( ResourceIterator<string> iterator = _db.execute( LIST_AVAILABLE_ANALYZERS ).columnAs( "analyzer" ) )
					{
						 while ( iterator.MoveNext() )
						 {
							  analyzers.Add( iterator.Current );
						 }
					}
					assertThat( analyzers, hasItem( "english" ) );
					assertThat( analyzers, hasItem( "swedish" ) );
					assertThat( analyzers, hasItem( "standard" ) );
					tx.Success();
			  }

			  // Verify that all analyzers have a description.
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( LIST_AVAILABLE_ANALYZERS ) )
					{
						 while ( result.MoveNext() )
						 {
							  IDictionary<string, object> row = result.Current;
							  object description = row["description"];
							  if ( !row.ContainsKey( "description" ) || !( description is string ) || ( ( string ) description ).Trim().Length == 0 )
							  {
									fail( "Found no description for analyzer: " + row );
							  }
						 }
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryNodesMustThrowWhenQueryingRelationshipIndex()
		 public virtual void QueryNodesMustThrowWhenQueryingRelationshipIndex()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_expectedException.expect( typeof( Exception ) );
					_db.execute( format( QUERY_NODES, "rels", "bla bla" ) ).close();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryRelationshipsMustThrowWhenQueryingNodeIndex()
		 public virtual void QueryRelationshipsMustThrowWhenQueryingNodeIndex()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_expectedException.expect( typeof( Exception ) );
					_db.execute( format( QUERY_RELS, "nodes", "bla bla" ) ).close();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexMustIgnoreNonStringPropertiesForUpdate()
		 public virtual void FulltextIndexMustIgnoreNonStringPropertiesForUpdate()
		 {
			  _db = CreateDatabase();

			  Label label = _label;
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "nodes", Array( label.Name() ), Array(PROP) ) ).close();
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  IList<Value> values = GenerateRandomNonStringValues();

			  using ( Transaction tx = _db.beginTx() )
			  {
					foreach ( Value value in values )
					{
						 Node node = _db.createNode( label );
						 object propertyValue = value.AsObject();
						 node.SetProperty( PROP, propertyValue );
						 node.CreateRelationshipTo( node, _rel ).setProperty( PROP, propertyValue );
					}
					tx.Success();
			  }

			  foreach ( Value value in values )
			  {
					string fulltextQuery = QuoteValueForQuery( value );
					string cypherQuery = format( QUERY_NODES, "nodes", fulltextQuery );
					Result nodes;
					try
					{
						 nodes = _db.execute( cypherQuery );
					}
					catch ( QueryExecutionException e )
					{
						 throw new AssertionError( "Failed to execute query: " + cypherQuery + " based on value " + value.PrettyPrint(), e );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( nodes.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 fail( "did not expect to find any nodes, but found at least: " + nodes.Next() );
					}
					nodes.Close();
					Result relationships = _db.execute( format( QUERY_RELS, "rels", fulltextQuery ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( relationships.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 fail( "did not expect to find any relationships, but found at least: " + relationships.Next() );
					}
					relationships.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexMustIgnoreNonStringPropertiesForPopulation()
		 public virtual void FulltextIndexMustIgnoreNonStringPropertiesForPopulation()
		 {
			  _db = CreateDatabase();

			  IList<Value> values = GenerateRandomNonStringValues();

			  using ( Transaction tx = _db.beginTx() )
			  {
					foreach ( Value value in values )
					{
						 Node node = _db.createNode( _label );
						 object propertyValue = value.AsObject();
						 node.SetProperty( PROP, propertyValue );
						 node.CreateRelationshipTo( node, _rel ).setProperty( PROP, propertyValue );
					}
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  foreach ( Value value in values )
			  {
					string fulltextQuery = QuoteValueForQuery( value );
					string cypherQuery = format( QUERY_NODES, "nodes", fulltextQuery );
					Result nodes;
					try
					{
						 nodes = _db.execute( cypherQuery );
					}
					catch ( QueryExecutionException e )
					{
						 throw new AssertionError( "Failed to execute query: " + cypherQuery + " based on value " + value.PrettyPrint(), e );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( nodes.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 fail( "did not expect to find any nodes, but found at least: " + nodes.Next() );
					}
					nodes.Close();
					Result relationships = _db.execute( format( QUERY_RELS, "rels", fulltextQuery ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( relationships.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 fail( "did not expect to find any relationships, but found at least: " + relationships.Next() );
					}
					relationships.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void entitiesMustBeRemovedFromFulltextIndexWhenPropertyValuesChangeAwayFromText()
		 public virtual void EntitiesMustBeRemovedFromFulltextIndexWhenPropertyValuesChangeAwayFromText()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					nodeId = node.Id;
					node.SetProperty( PROP, "bla bla" );
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.SetProperty( PROP, 42 );
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					Result result = _db.execute( format( QUERY_NODES, "nodes", "bla" ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void entitiesMustBeAddedToFulltextIndexWhenPropertyValuesChangeToText()
		 public virtual void EntitiesMustBeAddedToFulltextIndexWhenPropertyValuesChangeToText()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, 42 );
					nodeId = node.Id;
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.SetProperty( PROP, "bla bla" );
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "nodes", "bla", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertiesMustBeRemovedFromFulltextIndexWhenTheirValueTypeChangesAwayFromText()
		 public virtual void PropertiesMustBeRemovedFromFulltextIndexWhenTheirValueTypeChangesAwayFromText()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "nodes", Array( _label.name() ), Array("prop1", "prop2") ) ).close();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					nodeId = node.Id;
					node.SetProperty( "prop1", "foo" );
					node.SetProperty( "prop2", "bar" );
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.SetProperty( "prop2", 42 );
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "nodes", "foo", nodeId );
					Result result = _db.execute( format( QUERY_NODES, "nodes", "bar" ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					result.Close();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertiesMustBeAddedToFulltextIndexWhenTheirValueTypeChangesToText()
		 public virtual void PropertiesMustBeAddedToFulltextIndexWhenTheirValueTypeChangesToText()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "nodes", Array( _label.name() ), Array("prop1", "prop2") ) ).close();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					nodeId = node.Id;
					node.SetProperty( "prop1", "foo" );
					node.SetProperty( "prop2", 42 );
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.SetProperty( "prop2", "bar" );
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "nodes", "foo", nodeId );
					AssertQueryFindsIds( _db, true, "nodes", "bar", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToIndexHugeTextPropertiesInIndexUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToIndexHugeTextPropertiesInIndexUpdates()
		 {
			  string meditationes;
			  using ( StreamReader reader = new StreamReader( this.GetType().getResourceAsStream(DESCARTES_MEDITATIONES), Encoding.UTF8 ) )
			  {
					meditationes = reader.lines().collect(Collectors.joining("\n"));
			  }

			  _db = CreateDatabase();

			  Label label = Label.label( "Book" );
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "books", Array( label.Name() ), Array("title", "author", "contents") ) ).close();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( label );
					nodeId = node.Id;
					node.SetProperty( "title", "Meditationes de prima philosophia" );
					node.SetProperty( "author", "René Descartes" );
					node.SetProperty( "contents", meditationes );
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "books", "impellit scriptum offerendum", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToIndexHugeTextPropertiesInIndexPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToIndexHugeTextPropertiesInIndexPopulation()
		 {
			  string meditationes;
			  using ( StreamReader reader = new StreamReader( this.GetType().getResourceAsStream(DESCARTES_MEDITATIONES), Encoding.UTF8 ) )
			  {
					meditationes = reader.lines().collect(Collectors.joining("\n"));
			  }

			  _db = CreateDatabase();

			  Label label = Label.label( "Book" );
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( label );
					nodeId = node.Id;
					node.SetProperty( "title", "Meditationes de prima philosophia" );
					node.SetProperty( "author", "René Descartes" );
					node.SetProperty( "contents", meditationes );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "books", Array( label.Name() ), Array("title", "author", "contents") ) ).close();
					tx.Success();
			  }

			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "books", "impellit scriptum offerendum", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToQuerySpecificPropertiesViaLuceneSyntax()
		 public virtual void MustBeAbleToQuerySpecificPropertiesViaLuceneSyntax()
		 {
			  _db = CreateDatabase();
			  Label book = Label.label( "Book" );
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "books", Array( book.Name() ), Array("title", "author") ) ).close();
					tx.Success();
			  }

			  long book2id;
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node book1 = _db.createNode( book );
					book1.SetProperty( "author", "René Descartes" );
					book1.SetProperty( "title", "Meditationes de prima philosophia" );
					Node book2 = _db.createNode( book );
					book2.SetProperty( "author", "E. M. Curley" );
					book2.SetProperty( "title", "Descartes Against the Skeptics" );
					book2id = book2.Id;
					tx.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					LongHashSet ids = newSetWith( book2id );
					AssertQueryFindsIds( _db, true, "books", "title:Descartes", ids );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustIndexNodesByCorrectProperties()
		 public virtual void MustIndexNodesByCorrectProperties()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "nodes", Array( _label.name() ), Array("a", "b", "c", "d", "e", "f") ) ).close();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( "e", "value" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertQueryFindsIds( _db, true, "nodes", "e:value", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryingIndexInPopulatingStateMustBlockUntilIndexIsOnline()
		 public virtual void QueryingIndexInPopulatingStateMustBlockUntilIndexIsOnline()
		 {
			  _db = CreateDatabase();
			  long nodeCount = 10_000;
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < nodeCount; i++ )
					{

						 _db.createNode( _label ).setProperty( PROP, "value" );
					}
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( format( QUERY_NODES, "nodes", "value" ) ), Stream<IDictionary<string, object>> stream = result.stream(), )
					{
						 assertThat( stream.count(), @is(nodeCount) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryingIndexInPopulatingStateMustBlockUntilIndexIsOnlineEvenWhenTransactionHasState()
		 public virtual void QueryingIndexInPopulatingStateMustBlockUntilIndexIsOnlineEvenWhenTransactionHasState()
		 {
			  _db = CreateDatabase();
			  long nodeCount = 10_000;
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < nodeCount; i++ )
					{

						 _db.createNode( _label ).setProperty( PROP, "value" );
					}
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.createNode( _label ).setProperty( PROP, "value" );
					using ( Result result = _db.execute( format( QUERY_NODES, "nodes", "value" ) ), Stream<IDictionary<string, object>> stream = result.stream(), )
					{
						 assertThat( stream.count(), @is(nodeCount + 1) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryingIndexInTransactionItWasCreatedInMustThrow()
		 public virtual void QueryingIndexInTransactionItWasCreatedInMustThrow()
		 {
			  _db = CreateDatabase();
			  using ( Transaction ignore = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					_expectedException.expect( typeof( QueryExecutionException ) );
					_db.execute( format( QUERY_NODES, "nodes", "value" ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeNodesDeletedInOtherConcurrentlyCommittedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void QueryResultsMustNotIncludeNodesDeletedInOtherConcurrentlyCommittedTransactions()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeIdA;
			  long nodeIdB;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node nodeA = _db.createNode( _label );
					nodeA.SetProperty( PROP, "value" );
					nodeIdA = nodeA.Id;
					Node nodeB = _db.createNode( _label );
					nodeB.SetProperty( PROP, "value" );
					nodeIdB = nodeB.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( format( QUERY_NODES, "nodes", "value" ) ) )
					{
						 ThreadTestUtils.forkFuture(() =>
						 {
						  using ( Transaction forkedTx = _db.beginTx() )
						  {
								_db.getNodeById( nodeIdA ).delete();
								_db.getNodeById( nodeIdB ).delete();
								forkedTx.success();
						  }
						  return null;
						 }).get();
						 assertThat( result.Count(), @is(0L) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeRelationshipsDeletedInOtherConcurrentlyCommittedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void QueryResultsMustNotIncludeRelationshipsDeletedInOtherConcurrentlyCommittedTransactions()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relIdA;
			  long relIdB;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship relA = node.CreateRelationshipTo( node, _rel );
					relA.SetProperty( PROP, "value" );
					relIdA = relA.Id;
					Relationship relB = node.CreateRelationshipTo( node, _rel );
					relB.SetProperty( PROP, "value" );
					relIdB = relB.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( format( QUERY_RELS, "rels", "value" ) ) )
					{
						 ThreadTestUtils.forkFuture(() =>
						 {
						  using ( Transaction forkedTx = _db.beginTx() )
						  {
								_db.getRelationshipById( relIdA ).delete();
								_db.getRelationshipById( relIdB ).delete();
								forkedTx.success();
						  }
						  return null;
						 }).get();
						 assertThat( result.Count(), @is(0L) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeNodesDeletedInThisTransaction()
		 public virtual void QueryResultsMustNotIncludeNodesDeletedInThisTransaction()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeIdA;
			  long nodeIdB;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node nodeA = _db.createNode( _label );
					nodeA.SetProperty( PROP, "value" );
					nodeIdA = nodeA.Id;
					Node nodeB = _db.createNode( _label );
					nodeB.SetProperty( PROP, "value" );
					nodeIdB = nodeB.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeIdA ).delete();
					_db.getNodeById( nodeIdB ).delete();
					using ( Result result = _db.execute( format( QUERY_NODES, "nodes", "value" ) ) )
					{
						 assertThat( result.Count(), @is(0L) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeRelationshipsDeletedInThisTransaction()
		 public virtual void QueryResultsMustNotIncludeRelationshipsDeletedInThisTransaction()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relIdA;
			  long relIdB;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship relA = node.CreateRelationshipTo( node, _rel );
					relA.SetProperty( PROP, "value" );
					relIdA = relA.Id;
					Relationship relB = node.CreateRelationshipTo( node, _rel );
					relB.SetProperty( PROP, "value" );
					relIdB = relB.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getRelationshipById( relIdA ).delete();
					_db.getRelationshipById( relIdB ).delete();
					using ( Result result = _db.execute( format( QUERY_RELS, "rels", "value" ) ) )
					{
						 assertThat( result.Count(), @is(0L) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeNodesAddedInThisTransaction()
		 public virtual void QueryResultsMustIncludeNodesAddedInThisTransaction()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "value" );
					AssertQueryFindsIds( _db, true, "nodes", "value", newSetWith( node.Id ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeRelationshipsAddedInThisTransaction()
		 public virtual void QueryResultsMustIncludeRelationshipsAddedInThisTransaction()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					Relationship relationship = node.CreateRelationshipTo( node, _rel );
					relationship.SetProperty( PROP, "value" );
					AssertQueryFindsIds( _db, false, "rels", "value", newSetWith( relationship.Id ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeNodesWithPropertiesAddedToBeIndexed()
		 public virtual void QueryResultsMustIncludeNodesWithPropertiesAddedToBeIndexed()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					nodeId = _db.createNode( _label ).Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeId ).setProperty( PROP, "value" );
					AssertQueryFindsIds( _db, true, "nodes", "prop:value", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeRelationshipsWithPropertiesAddedToBeIndexed()
		 public virtual void QueryResultsMustIncludeRelationshipsWithPropertiesAddedToBeIndexed()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					relId = rel.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Relationship rel = _db.getRelationshipById( relId );
					rel.SetProperty( PROP, "value" );
					AssertQueryFindsIds( _db, false, "rels", "prop:value", relId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeNodesWithLabelsModifedToBeIndexed()
		 public virtual void QueryResultsMustIncludeNodesWithLabelsModifedToBeIndexed()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					node.SetProperty( PROP, "value" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.AddLabel( _label );
					AssertQueryFindsIds( _db, true, "nodes", "value", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeUpdatedValueOfChangedNodeProperties()
		 public virtual void QueryResultsMustIncludeUpdatedValueOfChangedNodeProperties()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "primo" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeId ).setProperty( PROP, "secundo" );
					AssertQueryFindsIds( _db, true, "nodes", "primo" );
					AssertQueryFindsIds( _db, true, "nodes", "secundo", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeUpdatedValuesOfChangedRelationshipProperties()
		 public virtual void QueryResultsMustIncludeUpdatedValuesOfChangedRelationshipProperties()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					rel.SetProperty( PROP, "primo" );
					relId = rel.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getRelationshipById( relId ).setProperty( PROP, "secundo" );
					AssertQueryFindsIds( _db, false, "rels", "primo" );
					AssertQueryFindsIds( _db, false, "rels", "secundo", relId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeNodesWithRemovedIndexedProperties()
		 public virtual void QueryResultsMustNotIncludeNodesWithRemovedIndexedProperties()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "value" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeId ).removeProperty( PROP );
					AssertQueryFindsIds( _db, true, "nodes", "value" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeRelationshipsWithRemovedIndexedProperties()
		 public virtual void QueryResultsMustNotIncludeRelationshipsWithRemovedIndexedProperties()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					rel.SetProperty( PROP, "value" );
					relId = rel.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getRelationshipById( relId ).removeProperty( PROP );
					AssertQueryFindsIds( _db, false, "rels", "value" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustNotIncludeNodesWithRemovedIndexedLabels()
		 public virtual void QueryResultsMustNotIncludeNodesWithRemovedIndexedLabels()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "value" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( nodeId ).removeLabel( _label );
					AssertQueryFindsIds( _db, true, "nodes", "nodes" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeOldNodePropertyValuesWhenModificationsAreUndone()
		 public virtual void QueryResultsMustIncludeOldNodePropertyValuesWhenModificationsAreUndone()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "primo" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					AssertQueryFindsIds( _db, true, "nodes", "primo", nodeId );
					AssertQueryFindsIds( _db, true, "nodes", "secundo" );
					node.SetProperty( PROP, "secundo" );
					AssertQueryFindsIds( _db, true, "nodes", "primo" );
					AssertQueryFindsIds( _db, true, "nodes", "secundo", nodeId );
					node.SetProperty( PROP, "primo" );
					AssertQueryFindsIds( _db, true, "nodes", "primo", nodeId );
					AssertQueryFindsIds( _db, true, "nodes", "secundo" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeOldRelationshipPropertyValuesWhenModificationsAreUndone()
		 public virtual void QueryResultsMustIncludeOldRelationshipPropertyValuesWhenModificationsAreUndone()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					rel.SetProperty( PROP, "primo" );
					relId = rel.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Relationship rel = _db.getRelationshipById( relId );
					AssertQueryFindsIds( _db, false, "rels", "primo", relId );
					AssertQueryFindsIds( _db, false, "rels", "secundo" );
					rel.SetProperty( PROP, "secundo" );
					AssertQueryFindsIds( _db, false, "rels", "primo" );
					AssertQueryFindsIds( _db, false, "rels", "secundo", relId );
					rel.SetProperty( PROP, "primo" );
					AssertQueryFindsIds( _db, false, "rels", "primo", relId );
					AssertQueryFindsIds( _db, false, "rels", "secundo" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeOldNodePropertyValuesWhenRemovalsAreUndone()
		 public virtual void QueryResultsMustIncludeOldNodePropertyValuesWhenRemovalsAreUndone()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "primo" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					AssertQueryFindsIds( _db, true, "nodes", "primo", nodeId );
					node.RemoveProperty( PROP );
					AssertQueryFindsIds( _db, true, "nodes", "primo" );
					node.SetProperty( PROP, "primo" );
					AssertQueryFindsIds( _db, true, "nodes", "primo", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeOldRelationshipPropertyValuesWhenRemovalsAreUndone()
		 public virtual void QueryResultsMustIncludeOldRelationshipPropertyValuesWhenRemovalsAreUndone()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					rel.SetProperty( PROP, "primo" );
					relId = rel.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Relationship rel = _db.getRelationshipById( relId );
					AssertQueryFindsIds( _db, false, "rels", "primo", relId );
					rel.RemoveProperty( PROP );
					AssertQueryFindsIds( _db, false, "rels", "primo" );
					rel.SetProperty( PROP, "primo" );
					AssertQueryFindsIds( _db, false, "rels", "primo", relId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsMustIncludeNodesWhenNodeLabelRemovalsAreUndone()
		 public virtual void QueryResultsMustIncludeNodesWhenNodeLabelRemovalsAreUndone()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "primo" );
					nodeId = node.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.RemoveLabel( _label );
					AssertQueryFindsIds( _db, true, "nodes", "primo" );
					node.AddLabel( _label );
					AssertQueryFindsIds( _db, true, "nodes", "primo", nodeId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryResultsFromTransactionStateMustSortTogetherWithResultFromBaseIndex()
		 public virtual void QueryResultsFromTransactionStateMustSortTogetherWithResultFromBaseIndex()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  long firstId;
			  long secondId;
			  long thirdId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node first = _db.createNode( _label );
					first.SetProperty( PROP, "God of War" );
					firstId = first.Id;
					Node third = _db.createNode( _label );
					third.SetProperty( PROP, "God Wars: Future Past" );
					thirdId = third.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node second = _db.createNode( _label );
					second.SetProperty( PROP, "God of War III Remastered" );
					secondId = second.Id;
					AssertQueryFindsIds( _db, true, "nodes", "god of war", firstId, secondId, thirdId );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryingDroppedIndexForNodesInDroppingTransactionMustThrow()
		 public virtual void QueryingDroppedIndexForNodesInDroppingTransactionMustThrow()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( DROP, "nodes" ) ).close();
					_expectedException.expect( typeof( QueryExecutionException ) );
					_db.execute( format( QUERY_NODES, "nodes", "blabla" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryingDroppedIndexForRelationshipsInDroppingTransactionMustThrow()
		 public virtual void QueryingDroppedIndexForRelationshipsInDroppingTransactionMustThrow()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( DROP, "rels" ) ).close();
					_expectedException.expect( typeof( QueryExecutionException ) );
					_db.execute( format( QUERY_RELS, "rels", "blabla" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingAndDroppingIndexesInSameTransactionMustNotThrow()
		 public virtual void CreatingAndDroppingIndexesInSameTransactionMustNotThrow()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					_db.execute( format( DROP, "nodes" ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleRelationshipIndex();
					_db.execute( format( DROP, "rels" ) ).close();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertFalse( _db.schema().Indexes.GetEnumerator().hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eventuallyConsistenIndexMustNotIncludeEntitiesAddedInTransaction()
		 public virtual void EventuallyConsistenIndexMustNotIncludeEntitiesAddedInTransaction()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "nodes", Array( _label.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) ).close();
					_db.execute( format( RELATIONSHIP_CREATE, "rels", Array( _rel.name() ), Array(PROP) + EVENTUALLY_CONSISTENT ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "value" );
					node.CreateRelationshipTo( node, _rel ).setProperty( PROP, "value" );

					AssertQueryFindsIds( _db, true, "nodes", "value" );
					AssertQueryFindsIds( _db, false, "rels", "value" );
					_db.execute( AWAIT_REFRESH ).close();
					AssertQueryFindsIds( _db, true, "nodes", "value" );
					AssertQueryFindsIds( _db, false, "rels", "value" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionStateMustNotPreventIndexUpdatesFromBeingApplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionStateMustNotPreventIndexUpdatesFromBeingApplied()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "value" );
					Relationship rel = node.CreateRelationshipTo( node, _rel );
					rel.SetProperty( PROP, "value" );
					LongHashSet nodeIds = new LongHashSet();
					LongHashSet relIds = new LongHashSet();
					nodeIds.add( node.Id );
					relIds.add( rel.Id );

					ExecutorService executor = _cleanup.add( Executors.newSingleThreadExecutor() );
					executor.submit(() =>
					{
					 using ( Transaction forkedTx = _db.beginTx() )
					 {
						  Node node2 = _db.createNode( _label );
						  node2.setProperty( PROP, "value" );
						  Relationship rel2 = node2.createRelationshipTo( node2, _rel );
						  rel2.setProperty( PROP, "value" );
						  nodeIds.add( node2.Id );
						  relIds.add( rel2.Id );
						  forkedTx.success();
					 }
					}).get();
					AssertQueryFindsIds( _db, true, "nodes", "value", nodeIds );
					AssertQueryFindsIds( _db, false, "rels", "value", relIds );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustNotApplyToRegularSchemaIndexes()
		 public virtual void DropMustNotApplyToRegularSchemaIndexes()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(_label).on(PROP).create();
					tx.Success();
			  }
			  AwaitIndexesOnline();
			  string schemaIndexName;
			  using ( Transaction ignore = _db.beginTx() )
			  {
					using ( Result result = _db.execute( "call db.indexes" ) )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 schemaIndexName = result.Next()["indexName"].ToString();
					}
					_expectedException.expect( typeof( QueryExecutionException ) );
					_db.execute( format( DROP, schemaIndexName ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexMustNotBeAvailableForRegularIndexSeeks()
		 public virtual void FulltextIndexMustNotBeAvailableForRegularIndexSeeks()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  string valueToQueryFor = "value to query for";
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					IList<Value> values = GenerateRandomSimpleValues();
					foreach ( Value value in values )
					{
						 _db.createNode( _label ).setProperty( PROP, value.AsObject() );
					}
					_db.createNode( _label ).setProperty( PROP, valueToQueryFor );
					tx.Success();
			  }
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["prop"] = valueToQueryFor;
			  using ( Result result = _db.execute( "profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher planner=rule profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 2.3 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 3.1 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 3.4 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexMustNotBeAvailableForRegularIndexSeeksAfterShutDown()
		 public virtual void FulltextIndexMustNotBeAvailableForRegularIndexSeeksAfterShutDown()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  _db.shutdown();
			  _db = CreateDatabase();
			  string valueToQueryFor = "value to query for";
			  using ( Transaction tx = _db.beginTx() )
			  {
					AwaitIndexesOnline();
					IList<Value> values = GenerateRandomSimpleValues();
					foreach ( Value value in values )
					{
						 _db.createNode( _label ).setProperty( PROP, value.AsObject() );
					}
					_db.createNode( _label ).setProperty( PROP, valueToQueryFor );
					tx.Success();
			  }
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["prop"] = valueToQueryFor;
			  using ( Result result = _db.execute( "profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher planner=rule profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 2.3 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 3.1 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
			  using ( Result result = _db.execute( "cypher 3.4 profile match (n:" + _label.name() + ") where n." + PROP + " = {prop} return n", @params ) )
			  {
					AssertNoIndexSeeks( result );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void awaitIndexProcedureMustWorkOnIndexNames()
		 public virtual void AwaitIndexProcedureMustWorkOnIndexNames()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < 1000; i++ )
					{
						 Node node = _db.createNode( _label );
						 node.SetProperty( PROP, "value" );
						 Relationship rel = node.CreateRelationshipTo( node, _rel );
						 rel.SetProperty( PROP, "value" );
					}
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					CreateSimpleRelationshipIndex();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( DB_AWAIT_INDEX, "nodes" ) ).close();
					_db.execute( format( DB_AWAIT_INDEX, "rels" ) ).close();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBePossibleToDropFulltextIndexByNameForWhichNormalIndexExistWithMatchingSchema()
		 public virtual void MustBePossibleToDropFulltextIndexByNameForWhichNormalIndexExistWithMatchingSchema()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE INDEX ON :Person(name)" ).close();
					_db.execute( "call db.index.fulltext.createNodeIndex('nameIndex', ['Person'], ['name'])" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					// This must not throw:
					_db.execute( "call db.index.fulltext.drop('nameIndex')" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertThat( single( _db.schema().Indexes ).Name, @is(not("nameIndex")) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexesMustNotPreventNormalSchemaIndexesFromBeingDropped()
		 public virtual void FulltextIndexesMustNotPreventNormalSchemaIndexesFromBeingDropped()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE INDEX ON :Person(name)" ).close();
					_db.execute( "call db.index.fulltext.createNodeIndex('nameIndex', ['Person'], ['name'])" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					// This must not throw:
					_db.execute( "DROP INDEX ON :Person(name)" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertThat( single( _db.schema().Indexes ).Name, @is("nameIndex") );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingNormalIndexWithFulltextProviderMustThrow()
		 public virtual void CreatingNormalIndexWithFulltextProviderMustThrow()
		 {
			  _db = CreateDatabase();
			  assertThat( FulltextIndexProviderFactory.Descriptor.name(), @is("fulltext-1.0") ); // Sanity check that this test is up to date.

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						_db.execute( "call db.createIndex( \":User(searchableString)\", \"" + FulltextIndexProviderFactory.Descriptor.name() + "\" );" ).close();
						tx.Success();
					  }
			  }
			  catch ( QueryExecutionException e )
			  {
					assertThat( e.Message, containsString( "only supports fulltext index descriptors" ) );
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					long indexCount = _db.execute( DB_INDEXES ).Count();
					assertThat( indexCount, @is( 0L ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSupportWildcardEndsLikeStartsWith()
		 public virtual void MustSupportWildcardEndsLikeStartsWith()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  LongHashSet ids = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "abcdef" );
					ids.add( node.Id );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "abcxyz" );
					ids.add( node.Id );

					AssertQueryFindsIds( _db, true, "nodes", "abc*", ids );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSupportWildcardBeginningsLikeEndsWith()
		 public virtual void MustSupportWildcardBeginningsLikeEndsWith()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  LongHashSet ids = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "defabc" );
					ids.add( node.Id );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "xyzabc" );
					ids.add( node.Id );

					AssertQueryFindsIds( _db, true, "nodes", "*abc", ids );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSupportWildcardBeginningsAndEndsLikeContains()
		 public virtual void MustSupportWildcardBeginningsAndEndsLikeContains()
		 {
			  _db = CreateDatabase();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateSimpleNodesIndex();
					tx.Success();
			  }
			  LongHashSet ids = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "defabcdef" );
					ids.add( node.Id );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROP, "xyzabcxyz" );
					ids.add( node.Id );

					AssertQueryFindsIds( _db, true, "nodes", "*abc*", ids );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustMatchCaseInsensitiveWithStandardAnalyzer()
		 public virtual void MustMatchCaseInsensitiveWithStandardAnalyzer()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'A'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'B'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'C'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'b'}))" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "myindex", Array( "Label" ), Array( "id" ) ) ).close();
					tx.Success();
			  }
			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "A" ) ) )
					{
						 assertThat( result.Count(), @is(0L) ); // The letter 'A' is a stop-word in English, so it is not indexed.
					}
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "B" ) ) )
					{
						 assertThat( result.Count(), @is(2000L) ); // Both upper- and lower-case 'B' nodes.
					}
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "C" ) ) )
					{
						 assertThat( result.Count(), @is(1000L) ); // We only have upper-case 'C' nodes.
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustMatchCaseInsensitiveWithSimpleAnalyzer()
		 public virtual void MustMatchCaseInsensitiveWithSimpleAnalyzer()
		 {
			  _db = CreateDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'A'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'B'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'C'}))" ).close();
					_db.execute( "foreach (x in range (1,1000) | create (n:Label {id:'b'}))" ).close();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "myindex", Array( "Label" ), Array( "id" ) + ", {analyzer: 'simple'}" ) ).close();
					tx.Success();
			  }
			  AwaitIndexesOnline();

			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "A" ) ) )
					{
						 assertThat( result.Count(), @is(1000L) ); // We only have upper-case 'A' nodes.
					}
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "B" ) ) )
					{
						 assertThat( result.Count(), @is(2000L) ); // Both upper- and lower-case 'B' nodes.
					}
					using ( Result result = _db.execute( format( QUERY_NODES, "myindex", "C" ) ) )
					{
						 assertThat( result.Count(), @is(1000L) ); // We only have upper-case 'C' nodes.
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFulltextIndexDoesNotBlockSchemaIndexOnSameSchemaPattern()
		 public virtual void MakeSureFulltextIndexDoesNotBlockSchemaIndexOnSameSchemaPattern()
		 {
			  _db = CreateDatabase();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label label = org.neo4j.graphdb.Label.label("label");
			  Label label = Label.label( "label" );
			  const string prop = "prop";
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "myindex", Array( label.Name() ), Array(prop) ) );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( DB_AWAIT_INDEX, "myindex" ) );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(label).on(prop).create();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.HOURS);
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( 2, Iterables.count( _db.schema().Indexes ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureSchemaIndexDoesNotBlockFulltextIndexOnSameSchemaPattern()
		 public virtual void MakeSureSchemaIndexDoesNotBlockFulltextIndexOnSameSchemaPattern()
		 {
			  _db = CreateDatabase();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label label = org.neo4j.graphdb.Label.label("label");
			  Label label = Label.label( "label" );
			  const string prop = "prop";
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(label).on(prop).create();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.HOURS);
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, "myindex", Array( label.Name() ), Array(prop) ) );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( DB_AWAIT_INDEX, "myindex" ) );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( 2, Iterables.count( _db.schema().Indexes ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateProperty()
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateProperty()
		 {
			  _db = CreateDatabase();

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						_db.execute( format( NODE_CREATE, "myindex", Array( "Label" ), Array( "id", "id" ) ) );
						fail( "Expected to fail when trying to create index with duplicate properties" );
					  }
			  }
			  catch ( Exception e )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Throwable cause = org.neo4j.helpers.Exceptions.rootCause(e);
					Exception cause = Exceptions.rootCause( e );
					assertThat( cause, instanceOf( typeof( RepeatedPropertyInSchemaException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateLabel()
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateLabel()
		 {
			  _db = CreateDatabase();

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						_db.execute( format( NODE_CREATE, "myindex", Array( "Label", "Label" ), Array( "id" ) ) );
						fail( "Expected to fail when trying to create index with duplicate labels" );
					  }
			  }
			  catch ( Exception e )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Throwable cause = org.neo4j.helpers.Exceptions.rootCause(e);
					Exception cause = Exceptions.rootCause( e );
					assertThat( cause, instanceOf( typeof( RepeatedLabelInSchemaException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateRelType()
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateRelType()
		 {
			  _db = CreateDatabase();

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						_db.execute( format( RELATIONSHIP_CREATE, "myindex", Array( "RelType", "RelType" ), Array( "id" ) ) );
						fail( "Expected to fail when trying to create index with duplicate relationship types" );
					  }
			  }
			  catch ( Exception e )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Throwable cause = org.neo4j.helpers.Exceptions.rootCause(e);
					Exception cause = Exceptions.rootCause( e );
					assertThat( cause, instanceOf( typeof( RepeatedRelationshipTypeInSchemaException ) ) );
			  }
		 }

		 private void AssertNoIndexSeeks( Result result )
		 {
			  assertThat( result.Count(), @is(1L) );
			  string planDescription = result.ExecutionPlanDescription.ToString();
			  assertThat( planDescription, containsString( "NodeByLabel" ) );
			  assertThat( planDescription, not( containsString( "IndexSeek" ) ) );
		 }

		 private GraphDatabaseAPI CreateDatabase()
		 {
			  return ( GraphDatabaseAPI ) _cleanup.add( _builder.newGraphDatabase() );
		 }

		 private void AwaitIndexesOnline()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 internal static void AssertQueryFindsIds( GraphDatabaseService db, bool queryNodes, string index, string query, params long[] ids )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					string queryCall = queryNodes ? QUERY_NODES : QUERY_RELS;
					Result result = Db.execute( format( queryCall, index, query ) );
					int num = 0;
					double? score = double.MaxValue;
					while ( result.MoveNext() )
					{
						 System.Collections.IDictionary entry = result.Current;
						 long? nextId = ( ( Entity ) entry[queryNodes ? NODE : RELATIONSHIP] ).Id;
						 double? nextScore = ( double? ) entry[SCORE];
						 assertThat( nextScore, lessThanOrEqualTo( score ) );
						 score = nextScore;
						 if ( num < ids.Length )
						 {
							  assertEquals( format( "Result returned id %d, expected %d", nextId, ids[num] ), ids[num], nextId.Value );
						 }
						 else
						 {
							  fail( format( "Result returned id %d, which is beyond the number of ids (%d) that were expected.", nextId, ids.Length ) );
						 }
						 num++;
					}
					assertEquals( "Number of results differ from expected", ids.Length, num );
					tx.Success();
			  }
		 }

		 internal static void AssertQueryFindsIds( GraphDatabaseService db, bool queryNodes, string index, string query, LongHashSet ids )
		 {
			  ids = new LongHashSet( ids ); // Create a defensive copy, because we're going to modify this instance.
			  string queryCall = queryNodes ? QUERY_NODES : QUERY_RELS;
			  System.Func<long, Entity> getEntity = queryNodes ? Db.getNodeById : Db.getRelationshipById;
			  long[] expectedIds = ids.toArray();
			  MutableLongSet actualIds = new LongHashSet();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Result result = Db.execute( format( queryCall, index, query ) );
					double? score = double.MaxValue;
					while ( result.MoveNext() )
					{
						 System.Collections.IDictionary entry = result.Current;
						 long nextId = ( ( Entity ) entry[queryNodes ? NODE : RELATIONSHIP] ).Id;
						 double? nextScore = ( double? ) entry[SCORE];
						 assertThat( nextScore, lessThanOrEqualTo( score ) );
						 score = nextScore;
						 actualIds.add( nextId );
						 if ( !ids.remove( nextId ) )
						 {
							  string msg = "This id was not expected: " + nextId;
							  FailQuery( getEntity, index, query, ids, expectedIds, actualIds, msg );
						 }
					}
					if ( !ids.Empty )
					{
						 string msg = "Not all expected ids were found: " + ids;
						 FailQuery( getEntity, index, query, ids, expectedIds, actualIds, msg );
					}
					tx.Success();
			  }
		 }

		 private static void FailQuery( System.Func<long, Entity> getEntity, string index, string query, MutableLongSet ids, long[] expectedIds, MutableLongSet actualIds, string msg )
		 {
			  StringBuilder message = ( new StringBuilder( msg ) ).Append( '\n' );
			  MutableLongIterator itr = ids.longIterator();
			  while ( itr.hasNext() )
			  {
					long id = itr.next();
					Entity entity = getEntity( id );
					message.Append( '\t' ).Append( entity ).Append( entity.AllProperties ).Append( '\n' );
			  }
			  message.Append( "for query: '" ).Append( query ).Append( "'\nin index: " ).Append( index ).Append( '\n' );
			  message.Append( "all expected ids: " ).Append( Arrays.ToString( expectedIds ) ).Append( '\n' );
			  message.Append( "actual ids: " ).Append( actualIds );
			  itr = actualIds.longIterator();
			  while ( itr.hasNext() )
			  {
					long id = itr.next();
					Entity entity = getEntity( id );
					message.Append( "\n\t" ).Append( entity ).Append( entity.AllProperties );
			  }
			  fail( message.ToString() );
		 }

		 internal static string Array( params string[] args )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.args.Select( s => "\"" + s + "\"" ).collect( Collectors.joining( ", ", "[", "]" ) );
		 }

		 private IList<Value> GenerateRandomNonStringValues()
		 {
			  System.Predicate<Value> nonString = v => v.valueGroup() != ValueGroup.TEXT;
			  return GenerateRandomValues( nonString );
		 }

		 private IList<Value> GenerateRandomSimpleValues()
		 {
			  EnumSet<ValueGroup> simpleTypes = EnumSet.of( ValueGroup.BOOLEAN, ValueGroup.BOOLEAN_ARRAY, ValueGroup.NUMBER, ValueGroup.NUMBER_ARRAY );
			  return GenerateRandomValues( v => simpleTypes.contains( v.valueGroup() ) );
		 }

		 private IList<Value> GenerateRandomValues( System.Predicate<Value> predicate )
		 {
			  int valuesToGenerate = 1000;
			  RandomValues generator = RandomValues.create();
			  IList<Value> values = new List<Value>( valuesToGenerate );
			  for ( int i = 0; i < valuesToGenerate; i++ )
			  {
					Value value;
					do
					{
						 value = generator.NextValue();
					} while ( !predicate( value ) );
					values.Add( value );
			  }
			  return values;
		 }

		 private string QuoteValueForQuery( Value value )
		 {
			  return QueryParserUtil.escape( value.PrettyPrint() ).replace("\\", "\\\\").replace("\"", "\\\"");
		 }

		 private void CreateSimpleRelationshipIndex()
		 {
			  _db.execute( format( RELATIONSHIP_CREATE, "rels", Array( _rel.name() ), Array(PROP) ) ).close();
		 }

		 private void CreateSimpleNodesIndex()
		 {
			  _db.execute( format( NODE_CREATE, "nodes", Array( _label.name() ), Array(PROP) ) ).close();
		 }
	}

}