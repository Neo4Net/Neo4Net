using System;
using System.Collections.Generic;

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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using PerFieldAnalyzerWrapper = org.apache.lucene.analysis.miscellaneous.PerFieldAnalyzerWrapper;
	using Term = Org.Apache.Lucene.Index.Term;
	using Operator = org.apache.lucene.queryparser.classic.QueryParser.Operator;
	using Occur = org.apache.lucene.search.BooleanClause.Occur;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using Sort = org.apache.lucene.search.Sort;
	using SortField = org.apache.lucene.search.SortField;
	using SortedNumericSortField = org.apache.lucene.search.SortedNumericSortField;
	using SortedSetSortField = org.apache.lucene.search.SortedSetSortField;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using DefaultSimilarity = org.apache.lucene.search.similarities.DefaultSimilarity;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Test = org.junit.Test;


	using Direction = Neo4Net.GraphDb.Direction;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using Neo4Net.GraphDb.index;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using QueryContext = Neo4Net.Index.lucene.QueryContext;
	using ValueContext = Neo4Net.Index.lucene.ValueContext;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.search.NumericRangeQuery.newIntRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyIterable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Neo4NetTestCase.assertContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Neo4NetTestCase.assertContainsInOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.impl.lucene.@explicit.LuceneIndexImplementation.EXACT_CONFIG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.impl.lucene.@explicit.LuceneIndexImplementation.FULLTEXT_CONFIG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.lucene.QueryContext.numericRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.lucene.ValueContext.numeric;

	public class TestLuceneIndex : AbstractLuceneIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void makeSureAdditionsCanBeRead(org.Neo4Net.graphdb.index.Index<T> index, IEntityCreator<T> IEntityCreator)
		 private void MakeSureAdditionsCanBeRead<T>( Index<T> index, IEntityCreator<T> IEntityCreator ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string key = "name";
			  string value = "Mattias";
			  assertThat( index.get( key, value ).Single, @is( nullValue() ) );
			  assertThat( index.get( key, value ), emptyIterable() );

			  assertThat( index.query( key, "*" ), emptyIterable() );

			  T IEntity1 = IEntityCreator.Create();
			  T IEntity2 = IEntityCreator.Create();
			  index.Add( IEntity1, key, value );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, value ), Contains.ContainsConflict( IEntity1 ) );
					assertThat( index.query( key, "*" ), Contains.ContainsConflict( IEntity1 ) );
					assertThat( index.get( key, value ), Contains.ContainsConflict( IEntity1 ) );

					RestartTx();
			  }

			  index.Add( IEntity2, key, value );
			  assertThat( index.get( key, value ), Contains.ContainsConflict( IEntity1, IEntity2 ) );

			  RestartTx();
			  assertThat( index.get( key, value ), Contains.ContainsConflict( IEntity1, IEntity2 ) );
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByNumeric()
		 public virtual void QueryIndexWithSortByNumeric()
		 {
			  Index<Node> index = NodeIndex();
			  CommitTx();

			  string numericProperty = "NODE_ID";

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					for ( int i = 0; i < 15; i++ )
					{
						 Node node = GraphDb.createNode();
						 node.SetProperty( numericProperty, i );
						 index.Add( node, numericProperty, ( new ValueContext( i ) ).indexNumeric() );
					}
					transaction.Success();
			  }

			  QueryAndSortNodesByNumericProperty( index, numericProperty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByString()
		 public virtual void QueryIndexWithSortByString()
		 {
			  Index<Node> index = NodeIndex();
			  CommitTx();

			  string stringProperty = "NODE_NAME";

			  string[] names = new string[]{ "Fry", "Leela", "Bender", "Amy", "Hubert", "Calculon" };
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					foreach ( string name in names )
					{
						 Node node = GraphDb.createNode();
						 node.SetProperty( stringProperty, name );
						 index.Add( node, stringProperty, name );
					}
					transaction.Success();
			  }

			  string[] sortedNames = new string[]{ "Leela", "Hubert", "Fry", "Calculon", "Bender", "Amy" };
			  QueryAndSortNodesByStringProperty( index, stringProperty, sortedNames );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByNumericAfterOtherPropertyUpdate()
		 public virtual void QueryIndexWithSortByNumericAfterOtherPropertyUpdate()
		 {
			  Index<Node> index = NodeIndex();
			  CommitTx();

			  string yearProperty = "year";
			  string priceProperty = "price";

			  Label nodeLabel = Label.label( "priceyNodes" );

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					for ( int i = 0; i < 15; i++ )
					{
						 Node node = GraphDb.createNode( nodeLabel );
						 node.SetProperty( yearProperty, i );
						 node.SetProperty( priceProperty, i );
						 index.Add( node, yearProperty, ( new ValueContext( i ) ).indexNumeric() );
						 index.Add( node, priceProperty, ( new ValueContext( i ) ).indexNumeric() );
					}
					transaction.Success();
			  }

			  DoubleNumericPropertyValueForAllNodesWithLabel( index, priceProperty, nodeLabel );

			  QueryAndSortNodesByNumericProperty( index, yearProperty );
			  QueryAndSortNodesByNumericProperty( index, priceProperty, value => value * 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByNumericAfterSamePropertyUpdate()
		 public virtual void QueryIndexWithSortByNumericAfterSamePropertyUpdate()
		 {
			  Index<Node> index = NodeIndex( stringMap() );
			  CommitTx();
			  string numericProperty = "PRODUCT_ID";

			  Label updatableIndexesProperty = Label.label( "updatableIndexes" );

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					for ( int i = 0; i < 15; i++ )
					{
						 Node node = GraphDb.createNode( updatableIndexesProperty );
						 node.SetProperty( numericProperty, i );
						 index.Add( node, numericProperty, ( new ValueContext( i ) ).indexNumeric() );
					}
					transaction.Success();
			  }

			  DoubleNumericPropertyValueForAllNodesWithLabel( index, numericProperty, updatableIndexesProperty );
			  QueryAndSortNodesByNumericProperty( index, numericProperty, i => i * 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByStringAfterOtherPropertyUpdate()
		 public virtual void QueryIndexWithSortByStringAfterOtherPropertyUpdate()
		 {

			  Index<Node> index = NodeIndex( stringMap() );
			  CommitTx();

			  string nameProperty = "NODE_NAME";
			  string jobNameProperty = "NODE_JOB_NAME";

			  string[] names = new string[]{ "Fry", "Leela", "Bender", "Amy", "Hubert", "Calculon" };
			  string[] jobs = new string[]{ "delivery boy", "pilot", "gambler", "intern", "professor", "actor" };
			  Label characters = Label.label( "characters" );
			  SetPropertiesAndUpdateToJunior( index, nameProperty, jobNameProperty, names, jobs, characters );

			  string[] sortedNames = new string[]{ "Leela", "Hubert", "Fry", "Calculon", "Bender", "Amy" };
			  string[] sortedJobs = new string[] { "junior professor", "junior pilot", "junior intern", "junior gambler", "junior delivery boy", "junior actor" };
			  QueryAndSortNodesByStringProperty( index, nameProperty, sortedNames );
			  QueryAndSortNodesByStringProperty( index, jobNameProperty, sortedJobs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryCustomIndexWithSortByStringAfterOtherPropertyUpdate()
		 public virtual void QueryCustomIndexWithSortByStringAfterOtherPropertyUpdate()
		 {

			  Index<Node> index = NodeIndex( MapUtil.stringMap( LuceneIndexImplementation.KEY_TYPE, "exact", LuceneIndexImplementation.KEY_TO_LOWER_CASE, "true" ) );
			  CommitTx();

			  string nameProperty = "NODE_NAME_CUSTOM";
			  string jobNameProperty = "NODE_JOB_NAME_CUSTOM";

			  string[] names = new string[]{ "Fry", "Leela", "Bender", "Amy", "Hubert", "Calculon" };
			  string[] jobs = new string[]{ "delivery boy", "pilot", "gambler", "intern", "professor", "actor" };
			  Label characters = Label.label( "characters_custom" );
			  SetPropertiesAndUpdateToJunior( index, nameProperty, jobNameProperty, names, jobs, characters );

			  string[] sortedNames = new string[]{ "Leela", "Hubert", "Fry", "Calculon", "Bender", "Amy" };
			  string[] sortedJobs = new string[] { "junior professor", "junior pilot", "junior intern", "junior gambler", "junior delivery boy", "junior actor" };
			  QueryAndSortNodesByStringProperty( index, nameProperty, sortedNames );
			  QueryAndSortNodesByStringProperty( index, jobNameProperty, sortedJobs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryIndexWithSortByStringAfterSamePropertyUpdate()
		 public virtual void QueryIndexWithSortByStringAfterSamePropertyUpdate()
		 {
			  Index<Node> index = NodeIndex( stringMap() );
			  CommitTx();

			  string nameProperty = "NODE_NAME";
			  Label heroes = Label.label( "heroes" );

			  string[] names = new string[]{ "Fry", "Leela", "Bender", "Amy", "Hubert", "Calculon" };
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					foreach ( string name in names )
					{
						 Node node = GraphDb.createNode( heroes );
						 node.SetProperty( nameProperty, name );
						 index.Add( node, nameProperty, name );
					}
					transaction.Success();
			  }

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					ResourceIterator<Node> nodes = GraphDb.findNodes( heroes );
					nodes.ForEach(node =>
					{
					 node.setProperty( nameProperty, "junior " + node.getProperty( nameProperty ) );
					 index.Remove( node, nameProperty );
					 index.Add( node, nameProperty, node.getProperty( nameProperty ) );
					});
					transaction.Success();
			  }

			  string[] sortedNames = new string[]{ "junior Leela", "junior Hubert", "junior Fry", "junior Calculon", "junior Bender", "junior Amy" };
			  QueryAndSortNodesByStringProperty( index, nameProperty, sortedNames );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureYouGetLatestTxModificationsInQueryByDefault()
		 public virtual void MakeSureYouGetLatestTxModificationsInQueryByDefault()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  assertThat( index.query( "key:value" ), Contains.ContainsConflict( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLuceneIndexesReportAsWriteable()
		 public virtual void MakeSureLuceneIndexesReportAsWriteable()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  assertTrue( index.Writeable );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeReadNodeExact()
		 public virtual void MakeSureAdditionsCanBeReadNodeExact()
		 {
			  MakeSureAdditionsCanBeRead( NodeIndex( LuceneIndexImplementation.ExactConfig ), NODE_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeReadNodeFulltext()
		 public virtual void MakeSureAdditionsCanBeReadNodeFulltext()
		 {
			  MakeSureAdditionsCanBeRead( NodeIndex( LuceneIndexImplementation.FulltextConfig ), NODE_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeReadRelationshipExact()
		 public virtual void MakeSureAdditionsCanBeReadRelationshipExact()
		 {
			  MakeSureAdditionsCanBeRead( RelationshipIndex( LuceneIndexImplementation.ExactConfig ), RELATIONSHIP_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeReadRelationshipFulltext()
		 public virtual void MakeSureAdditionsCanBeReadRelationshipFulltext()
		 {
			  MakeSureAdditionsCanBeRead( RelationshipIndex( LuceneIndexImplementation.FulltextConfig ), RELATIONSHIP_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeRemovedInSameTx()
		 public virtual void MakeSureAdditionsCanBeRemovedInSameTx()
		 {
			  MakeSureAdditionsCanBeRemoved( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingAnIndexedNodeWillAlsoRemoveItFromTheIndex()
		 public virtual void RemovingAnIndexedNodeWillAlsoRemoveItFromTheIndex()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  node.SetProperty( "poke", 1 );
			  index.Add( node, "key", "value" );
			  CommitTx();

			  BeginTx();
			  node.Delete();
			  CommitTx();

			  BeginTx();
			  IndexHits<Node> nodes = index.get( "key", "value" );
			  // IndexHits.size is allowed to be inaccurate in this case:
			  assertThat( nodes.Size(), isOneOf(0, 1) );
			  foreach ( Node n in nodes )
			  {
					n.GetProperty( "poke" );
					fail( "Found node " + n );
			  }
			  CommitTx();

			  BeginTx();
			  IndexHits<Node> nodesAgain = index.get( "key", "value" );
			  // After a read, the index should be repaired:
			  assertThat( nodesAgain.Size(), @is(0) );
			  foreach ( Node n in nodesAgain )
			  {
					n.GetProperty( "poke" );
					fail( "Found node " + n );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingAnIndexedRelationshipWillAlsoRemoveItFromTheIndex()
		 public virtual void RemovingAnIndexedRelationshipWillAlsoRemoveItFromTheIndex()
		 {
			  Index<Relationship> index = RelationshipIndex( LuceneIndexImplementation.ExactConfig );
			  Node a = GraphDb.createNode();
			  Node b = GraphDb.createNode();
			  Relationship rel = a.CreateRelationshipTo( b, withName( "REL" ) );
			  rel.SetProperty( "poke", 1 );
			  index.Add( rel, "key", "value" );
			  CommitTx();

			  BeginTx();
			  rel.Delete();
			  CommitTx();

			  BeginTx();
			  IndexHits<Relationship> rels = index.get( "key", "value" );
			  // IndexHits.size is allowed to be inaccurate in this case:
			  assertThat( rels.Size(), isOneOf(0, 1) );
			  foreach ( Relationship r in rels )
			  {
					r.GetProperty( "poke" );
					fail( "Found relationship " + r );
			  }
			  CommitTx();

			  BeginTx();
			  IndexHits<Relationship> relsAgain = index.get( "key", "value" );
			  // After a read, the index should be repaired:
			  assertThat( relsAgain.Size(), @is(0) );
			  foreach ( Relationship r in relsAgain )
			  {
					r.GetProperty( "poke" );
					fail( "Found relationship " + r );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureYouCanAskIfAnIndexExistsOrNot()
		 public virtual void MakeSureYouCanAskIfAnIndexExistsOrNot()
		 {
			  string name = CurrentIndexName();
			  assertFalse( GraphDb.index().existsForNodes(name) );
			  GraphDb.index().forNodes(name);
			  assertTrue( GraphDb.index().existsForNodes(name) );

			  assertFalse( GraphDb.index().existsForRelationships(name) );
			  GraphDb.index().forRelationships(name);
			  assertTrue( GraphDb.index().existsForRelationships(name) );
		 }

		 private void MakeSureAdditionsCanBeRemoved( bool restartTx )
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "name";
			  string value = "Mattias";
			  assertNull( index.get( key, value ).Single );
			  Node node = GraphDb.createNode();
			  index.Add( node, key, value );
			  if ( restartTx )
			  {
					restartTx();
			  }
			  assertEquals( node, index.get( key, value ).Single );
			  index.Remove( node, key, value );
			  assertNull( index.get( key, value ).Single );
			  restartTx();
			  assertNull( index.get( key, value ).Single );
			  node.Delete();
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAdditionsCanBeRemoved()
		 public virtual void MakeSureAdditionsCanBeRemoved()
		 {
			  MakeSureAdditionsCanBeRemoved( true );
		 }

		 private void MakeSureSomeAdditionsCanBeRemoved( bool restartTx )
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key1 = "name";
			  string key2 = "title";
			  string value1 = "Mattias";
			  assertNull( index.get( key1, value1 ).Single );
			  assertNull( index.get( key2, value1 ).Single );
			  Node node = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  index.Add( node, key1, value1 );
			  index.Add( node, key2, value1 );
			  index.Add( node2, key1, value1 );
			  if ( restartTx )
			  {
					restartTx();
			  }
			  index.Remove( node, key1, value1 );
			  index.Remove( node, key2, value1 );
			  assertEquals( node2, index.get( key1, value1 ).Single );
			  assertNull( index.get( key2, value1 ).Single );
			  assertEquals( node2, index.get( key1, value1 ).Single );
			  assertNull( index.get( key2, value1 ).Single );
			  node.Delete();
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureSomeAdditionsCanBeRemovedInSameTx()
		 public virtual void MakeSureSomeAdditionsCanBeRemovedInSameTx()
		 {
			  MakeSureSomeAdditionsCanBeRemoved( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureSomeAdditionsCanBeRemoved()
		 public virtual void MakeSureSomeAdditionsCanBeRemoved()
		 {
			  MakeSureSomeAdditionsCanBeRemoved( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureThereCanBeMoreThanOneValueForAKeyAndEntity()
		 public virtual void MakeSureThereCanBeMoreThanOneValueForAKeyAndEntity()
		 {
			  MakeSureThereCanBeMoreThanOneValueForAKeyAndEntity( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureThereCanBeMoreThanOneValueForAKeyAndEntitySameTx()
		 public virtual void MakeSureThereCanBeMoreThanOneValueForAKeyAndEntitySameTx()
		 {
			  MakeSureThereCanBeMoreThanOneValueForAKeyAndEntity( true );
		 }

		 private void MakeSureThereCanBeMoreThanOneValueForAKeyAndEntity( bool restartTx )
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "name";
			  string value1 = "Lucene";
			  string value2 = "Index";
			  string value3 = "Rules";
			  assertThat( index.query( key, "*" ), emptyIterable() );
			  Node node = GraphDb.createNode();
			  index.Add( node, key, value1 );
			  index.Add( node, key, value2 );
			  if ( restartTx )
			  {
					restartTx();
			  }
			  index.Add( node, key, value3 );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, value1 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, value2 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, value3 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, "whatever" ), emptyIterable() );
					restartTx();
			  }
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexHitsFromQueryingRemovedDoesNotReturnNegativeCount()
		 public virtual void IndexHitsFromQueryingRemovedDoesNotReturnNegativeCount()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node theNode = GraphDb.createNode();
			  index.Remove( theNode );
			  using ( IndexHits<Node> hits = index.query( "someRandomKey", theNode.Id ) )
			  {
					assertTrue( hits.Size() >= 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetLatestTxModificationsWhenChoosingSpeedQueries()
		 public virtual void ShouldNotGetLatestTxModificationsWhenChoosingSpeedQueries()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  QueryContext queryContext = ( new QueryContext( "value" ) ).tradeCorrectnessForSpeed();
			  assertThat( index.query( "key", queryContext ), emptyIterable() );
			  assertThat( index.query( "key", "value" ), Contains.ContainsConflict( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureArrayValuesAreSupported()
		 public virtual void MakeSureArrayValuesAreSupported()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "name";
			  string value1 = "Lucene";
			  string value2 = "Index";
			  string value3 = "Rules";
			  assertThat( index.query( key, "*" ), emptyIterable() );
			  Node node = GraphDb.createNode();
			  index.Add( node, key, new string[]{ value1, value2, value3 } );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, value1 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, value2 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, value3 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, "whatever" ), emptyIterable() );
					RestartTx();
			  }

			  index.Remove( node, key, new string[]{ value2, value3 } );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, value1 ), Contains.ContainsConflict( node ) );
					assertThat( index.get( key, value2 ), emptyIterable() );
					assertThat( index.get( key, value3 ), emptyIterable() );
					RestartTx();
			  }
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureWildcardQueriesCanBeAsked()
		 public virtual void MakeSureWildcardQueriesCanBeAsked()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "name";
			  string value1 = "Neo4Net";
			  string value2 = "nescafe";
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  index.Add( node1, key, value1 );
			  index.Add( node2, key, value2 );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( key, "neo*" ), Contains.ContainsConflict( node1 ) );
					assertThat( index.query( key, "n?o4j" ), Contains.ContainsConflict( node1 ) );
					assertThat( index.query( key, "ne*" ), Contains.ContainsConflict( node1, node2 ) );
					assertThat( index.query( key + ":Neo4Net" ), Contains.ContainsConflict( node1 ) );
					assertThat( index.query( key + ":neo*" ), Contains.ContainsConflict( node1 ) );
					assertThat( index.query( key + ":n?o4j" ), Contains.ContainsConflict( node1 ) );
					assertThat( index.query( key + ":ne*" ), Contains.ContainsConflict( node1, node2 ) );

					RestartTx();
			  }
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureCompositeQueriesCanBeAsked()
		 public virtual void MakeSureCompositeQueriesCanBeAsked()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node neo = GraphDb.createNode();
			  Node trinity = GraphDb.createNode();
			  index.Add( neo, "username", "neo@matrix" );
			  index.Add( neo, "sex", "male" );
			  index.Add( trinity, "username", "trinity@matrix" );
			  index.Add( trinity, "sex", "female" );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( "username:*@matrix AND sex:male" ), Contains.ContainsConflict( neo ) );
					assertThat( index.query( ( new QueryContext( "username:*@matrix sex:male" ) ).defaultOperator( Operator.AND ) ), Contains.ContainsConflict( neo ) );
					assertThat( index.query( "username:*@matrix OR sex:male" ), Contains.ContainsConflict( neo, trinity ) );
					assertThat( index.query( ( new QueryContext( "username:*@matrix sex:male" ) ).defaultOperator( Operator.OR ) ), Contains.ContainsConflict( neo, trinity ) );

					RestartTx();
			  }
			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void doSomeRandomUseCaseTestingWithExactIndex(org.Neo4Net.graphdb.index.Index<T> index, IEntityCreator<T> creator)
		 private void DoSomeRandomUseCaseTestingWithExactIndex<T>( Index<T> index, IEntityCreator<T> creator ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string name = "name";
			  string mattias = "Mattias Persson";
			  string title = "title";
			  string hacker = "Hacker";

			  assertThat( index.get( name, mattias ), emptyIterable() );

			  T IEntity1 = creator.Create();
			  T IEntity2 = creator.Create();

			  assertNull( index.get( name, mattias ).Single );
			  index.Add( IEntity1, name, mattias );
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );

			  assertContains( index.query( name, "\"" + mattias + "\"" ), IEntity1 );
			  assertContains( index.query( "name:\"" + mattias + "\"" ), IEntity1 );

			  assertEquals( IEntity1, index.get( name, mattias ).Single );

			  assertContains( index.query( "name", "Mattias*" ), IEntity1 );

			  CommitTx();

			  BeginTx();
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.query( name, "\"" + mattias + "\"" ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.query( "name:\"" + mattias + "\"" ), Contains.ContainsConflict( IEntity1 ) );
			  assertEquals( IEntity1, index.get( name, mattias ).Single );
			  assertThat( index.query( "name", "Mattias*" ), Contains.ContainsConflict( IEntity1 ) );
			  CommitTx();

			  BeginTx();
			  index.Add( IEntity2, title, hacker );
			  index.Add( IEntity1, title, hacker );
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.get( title, hacker ), Contains.ContainsConflict( IEntity1, IEntity2 ) );

			  assertContains( index.query( "name:\"" + mattias + "\" OR title:\"" + hacker + "\"" ), IEntity1, IEntity2 );

			  CommitTx();

			  BeginTx();
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.get( title, hacker ), Contains.ContainsConflict( IEntity1, IEntity2 ) );
			  assertThat( index.query( "name:\"" + mattias + "\" OR title:\"" + hacker + "\"" ), Contains.ContainsConflict( IEntity1, IEntity2 ) );
			  assertThat( index.query( "name:\"" + mattias + "\" AND title:\"" + hacker + "\"" ), Contains.ContainsConflict( IEntity1 ) );
			  CommitTx();

			  BeginTx();
			  index.Remove( IEntity2, title, hacker );
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.get( title, hacker ), Contains.ContainsConflict( IEntity1 ) );

			  assertContains( index.query( "name:\"" + mattias + "\" OR title:\"" + hacker + "\"" ), IEntity1 );

			  CommitTx();

			  BeginTx();
			  assertThat( index.get( name, mattias ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.get( title, hacker ), Contains.ContainsConflict( IEntity1 ) );
			  assertThat( index.query( "name:\"" + mattias + "\" OR title:\"" + hacker + "\"" ), Contains.ContainsConflict( IEntity1 ) );
			  CommitTx();

			  BeginTx();
			  index.Remove( IEntity1, title, hacker );
			  index.Remove( IEntity1, name, mattias );
			  index.Delete();
			  CommitTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doSomeRandomUseCaseTestingWithExactNodeIndex()
		 public virtual void DoSomeRandomUseCaseTestingWithExactNodeIndex()
		 {
			  DoSomeRandomUseCaseTestingWithExactIndex( NodeIndex( LuceneIndexImplementation.ExactConfig ), NODE_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doSomeRandomUseCaseTestingWithExactRelationshipIndex()
		 public virtual void DoSomeRandomUseCaseTestingWithExactRelationshipIndex()
		 {
			  DoSomeRandomUseCaseTestingWithExactIndex( RelationshipIndex( LuceneIndexImplementation.ExactConfig ), RELATIONSHIP_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void doSomeRandomTestingWithFulltextIndex(org.Neo4Net.graphdb.index.Index<T> index, IEntityCreator<T> creator)
		 private void DoSomeRandomTestingWithFulltextIndex<T>( Index<T> index, IEntityCreator<T> creator ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  T IEntity1 = creator.Create();
			  T IEntity2 = creator.Create();

			  string key = "name";
			  index.Add( IEntity1, key, "The quick brown fox" );
			  index.Add( IEntity2, key, "brown fox jumped over" );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, "The quick brown fox" ), Contains.ContainsConflict( IEntity1 ) );
					assertThat( index.get( key, "brown fox jumped over" ), Contains.ContainsConflict( IEntity2 ) );
					assertThat( index.query( key, "quick" ), Contains.ContainsConflict( IEntity1 ) );
					assertThat( index.query( key, "brown" ), Contains.ContainsConflict( IEntity1, IEntity2 ) );
					assertThat( index.query( key, "quick OR jumped" ), Contains.ContainsConflict( IEntity1, IEntity2 ) );
					assertThat( index.query( key, "brown AND fox" ), Contains.ContainsConflict( IEntity1, IEntity2 ) );

					RestartTx();
			  }

			  index.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doSomeRandomTestingWithNodeFulltextInde()
		 public virtual void DoSomeRandomTestingWithNodeFulltextInde()
		 {
			  DoSomeRandomTestingWithFulltextIndex( NodeIndex( LuceneIndexImplementation.FulltextConfig ), NODE_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doSomeRandomTestingWithRelationshipFulltextInde()
		 public virtual void DoSomeRandomTestingWithRelationshipFulltextInde()
		 {
			  DoSomeRandomTestingWithFulltextIndex( RelationshipIndex( LuceneIndexImplementation.FulltextConfig ), RELATIONSHIP_CREATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeLocalRelationshipIndex()
		 public virtual void TestNodeLocalRelationshipIndex()
		 {
			  RelationshipIndex index = RelationshipIndex( LuceneIndexImplementation.ExactConfig );

			  RelationshipType type = withName( "YO" );
			  Node startNode = GraphDb.createNode();
			  Node endNode1 = GraphDb.createNode();
			  Node endNode2 = GraphDb.createNode();
			  Relationship rel1 = startNode.CreateRelationshipTo( endNode1, type );
			  Relationship rel2 = startNode.CreateRelationshipTo( endNode2, type );
			  index.add( rel1, "name", "something" );
			  index.add( rel2, "name", "something" );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( "name:something" ), Contains.ContainsConflict( rel1, rel2 ) );
					assertThat( index.Query( "name:something", null, endNode1 ), Contains.ContainsConflict( rel1 ) );
					assertThat( index.Query( "name:something", startNode, endNode2 ), Contains.ContainsConflict( rel2 ) );
					assertThat( index.Query( null, startNode, endNode1 ), Contains.ContainsConflict( rel1 ) );
					assertThat( index.Get( "name", "something", null, endNode1 ), Contains.ContainsConflict( rel1 ) );
					assertThat( index.Get( "name", "something", startNode, endNode2 ), Contains.ContainsConflict( rel2 ) );
					assertThat( index.Get( null, null, startNode, endNode1 ), Contains.ContainsConflict( rel1 ) );

					RestartTx();
			  }

			  rel2.Delete();
			  rel1.Delete();
			  startNode.Delete();
			  endNode1.Delete();
			  endNode2.Delete();
			  index.delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSortByRelevance()
		 public virtual void TestSortByRelevance()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Node node3 = GraphDb.createNode();
			  index.Add( node1, "name", "something" );
			  index.Add( node2, "name", "something" );
			  index.Add( node2, "foo", "yes" );
			  index.Add( node3, "name", "something" );
			  index.Add( node3, "foo", "yes" );
			  index.Add( node3, "bar", "yes" );
			  RestartTx();

			  IndexHits<Node> hits = index.query( ( new QueryContext( "+name:something foo:yes bar:yes" ) ).sort( Sort.RELEVANCE ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( node3, hits.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( node2, hits.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( node1, hits.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( hits.hasNext() );
			  index.Delete();
			  node1.Delete();
			  node2.Delete();
			  node3.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSorting()
		 public virtual void TestSorting()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string name = "name";
			  string title = "title";
			  string other = "other";
			  string sex = "sex";
			  Node adam = GraphDb.createNode();
			  Node adam2 = GraphDb.createNode();
			  Node jack = GraphDb.createNode();
			  Node eva = GraphDb.createNode();

			  index.Add( adam, name, "Adam" );
			  index.Add( adam, title, "Software developer" );
			  index.Add( adam, sex, "male" );
			  index.Add( adam, other, "aaa" );
			  index.Add( adam2, name, "Adam" );
			  index.Add( adam2, title, "Blabla" );
			  index.Add( adam2, sex, "male" );
			  index.Add( adam2, other, "bbb" );
			  index.Add( jack, name, "Jack" );
			  index.Add( jack, title, "Apple sales guy" );
			  index.Add( jack, sex, "male" );
			  index.Add( jack, other, "ccc" );
			  index.Add( eva, name, "Eva" );
			  index.Add( eva, title, "Secretary" );
			  index.Add( eva, sex, "female" );
			  index.Add( eva, other, "ddd" );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertContainsInOrder( index.query( ( new QueryContext( "name:*" ) ).sort( name, title ) ), adam2, adam, eva, jack );
					assertContainsInOrder( index.query( ( new QueryContext( "name:*" ) ).sort( name, other ) ), adam, adam2, eva, jack );
					assertContainsInOrder( index.query( ( new QueryContext( "name:*" ) ).sort( sex, title ) ), eva, jack, adam2, adam );
					assertContainsInOrder( index.query( name, ( new QueryContext( "*" ) ).sort( sex, title ) ), eva, jack, adam2, adam );
					assertContainsInOrder( index.query( ( new QueryContext( "name:*" ) ).sort( name, title ).top( 2 ) ), adam2, adam );

					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumericValuesExactIndex()
		 public virtual void TestNumericValuesExactIndex()
		 {
			  TestNumericValues( NodeIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumericValuesFulltextIndex()
		 public virtual void TestNumericValuesFulltextIndex()
		 {
			  TestNumericValues( NodeIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

		 private void TestNumericValues( Index<Node> index )
		 {
			  Node node10 = GraphDb.createNode();
			  Node node6 = GraphDb.createNode();
			  Node node31 = GraphDb.createNode();

			  string key = "key";
			  index.Add( node10, key, numeric( 10 ) );
			  index.Add( node6, key, numeric( 6 ) );
			  index.Add( node31, key, numeric( 31 ) );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( NumericRangeQuery.newIntRange( key, 4, 40, true, true ) ), Contains.ContainsConflict( node10, node6, node31 ) );
					assertThat( index.query( NumericRangeQuery.newIntRange( key, 6, 15, true, true ) ), Contains.ContainsConflict( node10, node6 ) );
					assertThat( index.query( NumericRangeQuery.newIntRange( key, 6, 15, false, true ) ), Contains.ContainsConflict( node10 ) );
					RestartTx();
			  }

			  index.Remove( node6, key, numeric( 6 ) );
			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 4, 40, true, true ) ), Contains.ContainsConflict( node10, node31 ) );
			  RestartTx();
			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 4, 40, true, true ) ), Contains.ContainsConflict( node10, node31 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumericValueArrays()
		 public virtual void TestNumericValueArrays()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );

			  Node node1 = GraphDb.createNode();
			  index.Add( node1, "number", new ValueContext[]{ numeric( 45 ), numeric( 98 ) } );
			  Node node2 = GraphDb.createNode();
			  index.Add( node2, "number", new ValueContext[]{ numeric( 47 ), numeric( 100 ) } );

			  IndexHits<Node> indexResult1 = index.query( "number", newIntRange( "number", 47, 98, true, true ) );
			  assertThat( indexResult1, Contains.ContainsConflict( node1, node2 ) );
			  assertThat( indexResult1.Size(), @is(2) );

			  IndexHits<Node> indexResult2 = index.query( "number", newIntRange( "number", 44, 46, true, true ) );
			  assertThat( indexResult2, Contains.ContainsConflict( node1 ) );
			  assertThat( indexResult2.Size(), @is(1) );

			  IndexHits<Node> indexResult3 = index.query( "number", newIntRange( "number", 99, 101, true, true ) );
			  assertThat( indexResult3, Contains.ContainsConflict( node2 ) );
			  assertThat( indexResult3.Size(), @is(1) );

			  IndexHits<Node> indexResult4 = index.query( "number", newIntRange( "number", 47, 98, false, false ) );
			  assertThat( indexResult4, emptyIterable() );

			  IndexHits<Node> indexResult5 = index.query( "number", numericRange( "number", null, 98, true, true ) );
			  assertContains( indexResult5, node1, node2 );

			  IndexHits<Node> indexResult6 = index.query( "number", numericRange( "number", 47, null, true, true ) );
			  assertContains( indexResult6, node1, node2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveNumericValues()
		 public virtual void TestRemoveNumericValues()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  string key = "key";
			  index.Add( node1, key, ( new ValueContext( 15 ) ).indexNumeric() );
			  index.Add( node2, key, ( new ValueContext( 5 ) ).indexNumeric() );
			  index.Remove( node1, key, ( new ValueContext( 15 ) ).indexNumeric() );

			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), Contains.ContainsConflict( node2 ) );

			  index.Remove( node2, key, ( new ValueContext( 5 ) ).indexNumeric() );

			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), emptyIterable() );

			  RestartTx();
			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), emptyIterable() );

			  index.Add( node1, key, ( new ValueContext( 15 ) ).indexNumeric() );
			  index.Add( node2, key, ( new ValueContext( 5 ) ).indexNumeric() );
			  RestartTx();
			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), Contains.ContainsConflict( node1, node2 ) );
			  index.Remove( node1, key, ( new ValueContext( 15 ) ).indexNumeric() );

			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), Contains.ContainsConflict( node2 ) );

			  RestartTx();
			  assertThat( index.query( NumericRangeQuery.newIntRange( key, 0, 20, false, false ) ), Contains.ContainsConflict( node2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sortNumericValues()
		 public virtual void SortNumericValues()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Node node3 = GraphDb.createNode();
			  string key = "key";
			  index.Add( node1, key, numeric( 5 ) );
			  index.Add( node2, key, numeric( 15 ) );
			  index.Add( node3, key, numeric( 10 ) );
			  RestartTx();

			  assertContainsInOrder( index.query( numericRange( key, 5, 15 ).sortNumeric( key, false ) ), node1, node3, node2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexNumberAsString()
		 public virtual void TestIndexNumberAsString()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node1 = GraphDb.createNode();
			  index.Add( node1, "key", 10 );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertEquals( node1, index.get( "key", 10 ).Single );
					assertEquals( node1, index.get( "key", "10" ).Single );
					assertEquals( node1, index.query( "key", 10 ).Single );
					assertEquals( node1, index.query( "key", "10" ).Single );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void makeSureIndexGetsCreatedImmediately()
		 public virtual void MakeSureIndexGetsCreatedImmediately()
		 {
			  // Since index creation is done outside of the normal transactions,
			  // a rollback will not roll back index creation.

			  NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  assertTrue( GraphDb.index().existsForNodes(CurrentIndexName()) );
			  RollbackTx();
			  BeginTx();
			  assertTrue( GraphDb.index().existsForNodes(CurrentIndexName()) );
			  NodeIndex( LuceneIndexImplementation.ExactConfig );
			  RollbackTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFulltextConfigIsCaseInsensitiveByDefault()
		 public virtual void MakeSureFulltextConfigIsCaseInsensitiveByDefault()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node node = GraphDb.createNode();
			  string key = "name";
			  string value = "Mattias Persson";
			  index.Add( node, key, value );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( "name", "[A TO Z]" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "[a TO z]" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "Mattias" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "mattias" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "Matt*" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "matt*" ), Contains.ContainsConflict( node ) );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFulltextIndexCanBeCaseSensitive()
		 public virtual void MakeSureFulltextIndexCanBeCaseSensitive()
		 {
			  Index<Node> index = NodeIndex( MapUtil.stringMap( new Dictionary<Node>( LuceneIndexImplementation.FulltextConfig ), "to_lower_case", "false" ) );
			  Node node = GraphDb.createNode();
			  string key = "name";
			  string value = "Mattias Persson";
			  index.Add( node, key, value );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.query( "name", "[A TO Z]" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "[a TO z]" ), emptyIterable() );
					assertThat( index.query( "name", "Matt*" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "matt*" ), emptyIterable() );
					assertThat( index.query( "name", "Persson" ), Contains.ContainsConflict( node ) );
					assertThat( index.query( "name", "persson" ), emptyIterable() );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureCustomAnalyzerCanBeUsed()
		 public virtual void MakeSureCustomAnalyzerCanBeUsed()
		 {
			  CustomAnalyzer.Called = false;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Index<Node> index = NodeIndex( MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "analyzer", typeof( CustomAnalyzer ).FullName, "to_lower_case", "true" ) );
			  Node node = GraphDb.createNode();
			  string key = "name";
			  string value = "The value";
			  index.Add( node, key, value );
			  RestartTx();
			  assertTrue( CustomAnalyzer.Called );
			  assertThat( index.query( key, "[A TO Z]" ), Contains.ContainsConflict( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureCustomAnalyzerCanBeUsed2()
		 public virtual void MakeSureCustomAnalyzerCanBeUsed2()
		 {
			  CustomAnalyzer.Called = false;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Index<Node> index = NodeIndex( "w-custom-analyzer-2", MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "analyzer", typeof( CustomAnalyzer ).FullName, "to_lower_case", "true", "type", "fulltext" ) );
			  Node node = GraphDb.createNode();
			  string key = "name";
			  string value = "The value";
			  index.Add( node, key, value );
			  RestartTx();
			  assertTrue( CustomAnalyzer.Called );
			  assertThat( index.query( key, "[A TO Z]" ), Contains.ContainsConflict( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIndexNameAndConfigCanBeReachedFromIndex()
		 public virtual void MakeSureIndexNameAndConfigCanBeReachedFromIndex()
		 {
			  string indexName = "my-index-1";
			  Index<Node> nodeIndex = nodeIndex( indexName, LuceneIndexImplementation.ExactConfig );
			  assertEquals( indexName, nodeIndex.Name );
			  assertEquals( LuceneIndexImplementation.ExactConfig, GraphDb.index().getConfiguration(nodeIndex) );

			  string indexName2 = "my-index-2";
			  Index<Relationship> relIndex = RelationshipIndex( indexName2, LuceneIndexImplementation.FulltextConfig );
			  assertEquals( indexName2, relIndex.Name );
			  assertEquals( LuceneIndexImplementation.FulltextConfig, GraphDb.index().getConfiguration(relIndex) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringQueryVsQueryObject()
		 public virtual void TestStringQueryVsQueryObject()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "name", "Mattias Persson" );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertContains( index.query( "name:Mattias AND name:Per*" ), node );
					assertContains( index.query( "name:mattias" ), node );
					assertContains( index.query( new TermQuery( new Term( "name", "mattias" ) ) ), node );
					RestartTx();
			  }
			  assertNull( index.query( new TermQuery( new Term( "name", "Mattias" ) ) ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void testAbandonedIds(EntityCreator<T> creator, org.Neo4Net.graphdb.index.Index<T> index)
		 private void TestAbandonedIds<T>( IEntityCreator<T> creator, Index<T> index ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  // TODO This doesn't actually test that they are deleted, it just triggers it
			  // so that you manually can inspect what's going on
			  T a = creator.Create();
			  T b = creator.Create();
			  T c = creator.Create();
			  string key = "name";
			  string value = "value";
			  index.Add( a, key, value );
			  index.Add( b, key, value );
			  index.Add( c, key, value );
			  RestartTx();

			  creator.Delete( b );
			  RestartTx();

			  Iterators.count( index.get( key, value ) );
			  RollbackTx();
			  BeginTx();

			  Iterators.count( index.get( key, value ) );
			  index.Add( c, "something", "whatever" );
			  RestartTx();

			  Iterators.count( index.get( key, value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAbandonedNodeIds()
		 public virtual void TestAbandonedNodeIds()
		 {
			  TestAbandonedIds( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAbandonedNodeIdsFulltext()
		 public virtual void TestAbandonedNodeIdsFulltext()
		 {
			  TestAbandonedIds( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAbandonedRelIds()
		 public virtual void TestAbandonedRelIds()
		 {
			  TestAbandonedIds( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAbandonedRelIdsFulltext()
		 public virtual void TestAbandonedRelIdsFulltext()
		 {
			  TestAbandonedIds( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureYouCanRemoveFromRelationshipIndex()
		 public virtual void MakeSureYouCanRemoveFromRelationshipIndex()
		 {
			  Node n1 = GraphDb.createNode();
			  Node n2 = GraphDb.createNode();
			  Relationship r = n1.CreateRelationshipTo( n2, withName( "foo" ) );
			  RelationshipIndex index = GraphDb.index().forRelationships("rel-index");
			  string key = "bar";
			  index.remove( r, key, "value" );
			  index.add( r, key, "otherValue" );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertThat( index.get( key, "value" ), emptyIterable() );
					assertThat( index.get( key, "otherValue" ), Contains.ContainsConflict( r ) );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureYouCanGetEntityTypeFromIndex()
		 public virtual void MakeSureYouCanGetEntityTypeFromIndex()
		 {
			  Index<Node> nodeIndex = nodeIndex( MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "exact" ) );
			  Index<Relationship> relIndex = RelationshipIndex( MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "exact" ) );
			  assertEquals( typeof( Node ), nodeIndex.EntityType );
			  assertEquals( typeof( Relationship ), relIndex.EntityType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureConfigurationCanBeModified()
		 public virtual void MakeSureConfigurationCanBeModified()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  try
			  {
					GraphDb.index().setConfiguration(index, Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "something");
					fail( "Shouldn't be able to modify provider" );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  try
			  {
					GraphDb.index().removeConfiguration(index, Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER);
					fail( "Shouldn't be able to modify provider" );
			  }
			  catch ( System.ArgumentException )
			  {
			  }

			  string key = "my-key";
			  string value = "my-value";
			  string newValue = "my-new-value";
			  assertNull( GraphDb.index().setConfiguration(index, key, value) );
			  assertEquals( value, GraphDb.index().getConfiguration(index).get(key) );
			  assertEquals( value, GraphDb.index().setConfiguration(index, key, newValue) );
			  assertEquals( newValue, GraphDb.index().getConfiguration(index).get(key) );
			  assertEquals( newValue, GraphDb.index().removeConfiguration(index, key) );
			  assertNull( GraphDb.index().getConfiguration(index).get(key) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureSlightDifferencesInIndexConfigCanBeSupplied()
		 public virtual void MakeSureSlightDifferencesInIndexConfigCanBeSupplied()
		 {
			  IDictionary<string, string> config = MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext" );
			  string name = CurrentIndexName();
			  NodeIndex( name, config );
			  NodeIndex( name, MapUtil.stringMap( new Dictionary<>( config ), "to_lower_case", "true" ) );
			  try
			  {
					NodeIndex( name, MapUtil.stringMap( new Dictionary<>( config ), "to_lower_case", "false" ) );
					fail( "Shouldn't be able to get index with these kinds of differences in config" );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  NodeIndex( name, MapUtil.stringMap( new Dictionary<>( config ), "whatever", "something" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testScoring()
		 public virtual void TestScoring()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  string key = "text";
			  // Where the heck did I get this sentence from?
			  index.Add( node1, key, "a time where no one was really awake" );
			  index.Add( node2, key, "once upon a time there was" );
			  RestartTx();

			  QueryContext queryContext = ( new QueryContext( "once upon a time was" ) ).sort( Sort.RELEVANCE );
			  using ( IndexHits<Node> hits = index.query( key, queryContext ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node hit1 = hits.next();
					float score1 = hits.CurrentScore();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node hit2 = hits.next();
					float score2 = hits.CurrentScore();
					assertEquals( node2, hit1 );
					assertEquals( node1, hit2 );
					assertTrue( "Score 1 (" + score1 + ") should have been higher than score 2 (" + score2 + ")", score1 > score2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTopHits()
		 public virtual void TestTopHits()
		 {
			  Index<Relationship> index = RelationshipIndex( LuceneIndexImplementation.FulltextConfig );
			  IEntityCreator<Relationship> creator = RELATIONSHIP_CREATOR;
			  string key = "text";
			  Relationship rel1 = creator.Create( key, "one two three four five six seven eight nine ten" );
			  Relationship rel2 = creator.Create( key, "one two three four five six seven eight other things" );
			  Relationship rel3 = creator.Create( key, "one two three four five six some thing else" );
			  Relationship rel4 = creator.Create( key, "one two three four five what ever" );
			  Relationship rel5 = creator.Create( key, "one two three four all that is good and bad" );
			  Relationship rel6 = creator.Create( key, "one two three hill or something" );
			  Relationship rel7 = creator.Create( key, "one two other time than this" );
			  index.Add( rel2, key, rel2.GetProperty( key ) );
			  index.Add( rel1, key, rel1.GetProperty( key ) );
			  index.Add( rel3, key, rel3.GetProperty( key ) );
			  index.Add( rel7, key, rel7.GetProperty( key ) );
			  index.Add( rel5, key, rel5.GetProperty( key ) );
			  index.Add( rel4, key, rel4.GetProperty( key ) );
			  index.Add( rel6, key, rel6.GetProperty( key ) );
			  string query = "one two three four five six seven";

			  for ( int i = 0; i < 2; i++ )
			  {
					assertContainsInOrder( index.query( key, ( new QueryContext( query ) ).top( 3 ).sort( Sort.RELEVANCE ) ), rel1, rel2, rel3 );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimilarity()
		 public virtual void TestSimilarity()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Index<Node> index = NodeIndex( MapUtil.stringMap( Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext", "similarity", typeof( DefaultSimilarity ).FullName ) );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  RestartTx();
			  assertContains( index.get( "key", "value" ), node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCombinedHitsSizeProblem()
		 public virtual void TestCombinedHitsSizeProblem()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Node node3 = GraphDb.createNode();
			  string key = "key";
			  string value = "value";
			  index.Add( node1, key, value );
			  index.Add( node2, key, value );
			  RestartTx();
			  index.Add( node3, key, value );
			  using ( IndexHits<Node> hits = index.get( key, value ) )
			  {
					assertEquals( 3, hits.Size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void testRemoveWithoutKey(EntityCreator<T> creator, org.Neo4Net.graphdb.index.Index<T> index)
		 private void TestRemoveWithoutKey<T>( IEntityCreator<T> creator, Index<T> index ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string key1 = "key1";
			  string key2 = "key2";
			  string value = "value";

			  T IEntity1 = creator.Create();
			  index.Add( IEntity1, key1, value );
			  index.Add( IEntity1, key2, value );
			  T IEntity2 = creator.Create();
			  index.Add( IEntity2, key1, value );
			  index.Add( IEntity2, key2, value );
			  RestartTx();

			  assertContains( index.get( key1, value ), IEntity1, IEntity2 );
			  assertContains( index.get( key2, value ), IEntity1, IEntity2 );
			  index.Remove( IEntity1, key2 );
			  assertContains( index.get( key1, value ), IEntity1, IEntity2 );
			  assertContains( index.get( key2, value ), IEntity2 );
			  index.Add( IEntity1, key2, value );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertContains( index.get( key1, value ), IEntity1, IEntity2 );
					assertContains( index.get( key2, value ), IEntity1, IEntity2 );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyNodes()
		 public virtual void TestRemoveWithoutKeyNodes()
		 {
			  TestRemoveWithoutKey( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyRelationships()
		 public virtual void TestRemoveWithoutKeyRelationships()
		 {
			  TestRemoveWithoutKey( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void testRemoveWithoutKeyValue(EntityCreator<T> creator, org.Neo4Net.graphdb.index.Index<T> index)
		 private void TestRemoveWithoutKeyValue<T>( IEntityCreator<T> creator, Index<T> index ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string key1 = "key1";
			  string value1 = "value1";
			  string key2 = "key2";
			  string value2 = "value2";

			  T IEntity1 = creator.Create();
			  index.Add( IEntity1, key1, value1 );
			  index.Add( IEntity1, key2, value2 );
			  T IEntity2 = creator.Create();
			  index.Add( IEntity2, key1, value1 );
			  index.Add( IEntity2, key2, value2 );
			  RestartTx();

			  assertContains( index.get( key1, value1 ), IEntity1, IEntity2 );
			  assertContains( index.get( key2, value2 ), IEntity1, IEntity2 );
			  index.Remove( IEntity1 );
			  assertContains( index.get( key1, value1 ), IEntity2 );
			  assertContains( index.get( key2, value2 ), IEntity2 );
			  index.Add( IEntity1, key1, value1 );

			  for ( int i = 0; i < 2; i++ )
			  {
					assertContains( index.get( key1, value1 ), IEntity1, IEntity2 );
					assertContains( index.get( key2, value2 ), IEntity2 );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyValueNodes()
		 public virtual void TestRemoveWithoutKeyValueNodes()
		 {
			  TestRemoveWithoutKeyValue( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyValueRelationships()
		 public virtual void TestRemoveWithoutKeyValueRelationships()
		 {
			  TestRemoveWithoutKeyValue( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.ExactConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void testRemoveWithoutKeyFulltext(EntityCreator<T> creator, org.Neo4Net.graphdb.index.Index<T> index)
		 private void TestRemoveWithoutKeyFulltext<T>( IEntityCreator<T> creator, Index<T> index ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string key1 = "key1";
			  string key2 = "key2";
			  string value1 = "value one";
			  string value2 = "other value";
			  string value = "value";

			  T IEntity1 = creator.Create();
			  index.Add( IEntity1, key1, value1 );
			  index.Add( IEntity1, key2, value1 );
			  index.Add( IEntity1, key2, value2 );
			  T IEntity2 = creator.Create();
			  index.Add( IEntity2, key1, value1 );
			  index.Add( IEntity2, key2, value1 );
			  index.Add( IEntity2, key2, value2 );
			  RestartTx();

			  assertContains( index.query( key1, value ), IEntity1, IEntity2 );
			  assertContains( index.query( key2, value ), IEntity1, IEntity2 );
			  index.Remove( IEntity1, key2 );
			  assertContains( index.query( key1, value ), IEntity1, IEntity2 );
			  assertContains( index.query( key2, value ), IEntity2 );
			  index.Add( IEntity1, key2, value1 );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertContains( index.query( key1, value ), IEntity1, IEntity2 );
					assertContains( index.query( key2, value ), IEntity1, IEntity2 );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyFulltextNode()
		 public virtual void TestRemoveWithoutKeyFulltextNode()
		 {
			  TestRemoveWithoutKeyFulltext( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyFulltextRelationship()
		 public virtual void TestRemoveWithoutKeyFulltextRelationship()
		 {
			  TestRemoveWithoutKeyFulltext( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.Neo4Net.graphdb.PropertyContainer> void testRemoveWithoutKeyValueFulltext(EntityCreator<T> creator, org.Neo4Net.graphdb.index.Index<T> index)
		 private void TestRemoveWithoutKeyValueFulltext<T>( IEntityCreator<T> creator, Index<T> index ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string value = "value";
			  string key1 = "key1";
			  string value1 = value + " one";
			  string key2 = "key2";
			  string value2 = value + " two";

			  T IEntity1 = creator.Create();
			  index.Add( IEntity1, key1, value1 );
			  index.Add( IEntity1, key2, value2 );
			  T IEntity2 = creator.Create();
			  index.Add( IEntity2, key1, value1 );
			  index.Add( IEntity2, key2, value2 );
			  RestartTx();

			  assertContains( index.query( key1, value ), IEntity1, IEntity2 );
			  assertContains( index.query( key2, value ), IEntity1, IEntity2 );
			  index.Remove( IEntity1 );
			  assertContains( index.query( key1, value ), IEntity2 );
			  assertContains( index.query( key2, value ), IEntity2 );
			  index.Add( IEntity1, key1, value1 );
			  for ( int i = 0; i < 2; i++ )
			  {
					assertContains( index.query( key1, value ), IEntity1, IEntity2 );
					assertContains( index.query( key2, value ), IEntity2 );
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyValueFulltextNode()
		 public virtual void TestRemoveWithoutKeyValueFulltextNode()
		 {
			  TestRemoveWithoutKeyValueFulltext( NODE_CREATOR, NodeIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveWithoutKeyValueFulltextRelationship()
		 public virtual void TestRemoveWithoutKeyValueFulltextRelationship()
		 {
			  TestRemoveWithoutKeyValueFulltext( RELATIONSHIP_CREATOR, RelationshipIndex( LuceneIndexImplementation.FulltextConfig ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSortingWithTopHitsInPartCommittedPartLocal()
		 public virtual void TestSortingWithTopHitsInPartCommittedPartLocal()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  Node first = GraphDb.createNode();
			  Node second = GraphDb.createNode();
			  Node third = GraphDb.createNode();
			  Node fourth = GraphDb.createNode();
			  string key = "key";

			  index.Add( third, key, "ccc" );
			  index.Add( second, key, "bbb" );
			  RestartTx();
			  index.Add( fourth, key, "ddd" );
			  index.Add( first, key, "aaa" );

			  assertContainsInOrder( index.query( key, ( new QueryContext( "*" ) ).sort( key ) ), first, second, third, fourth );
			  assertContainsInOrder( index.query( key, ( new QueryContext( "*" ) ).sort( key ).top( 2 ) ), first, second );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindValueDeletedInSameTx()
		 public virtual void ShouldNotFindValueDeletedInSameTx()
		 {
			  Index<Node> nodeIndex = GraphDb.index().forNodes("size-after-removal");
			  Node node = GraphDb.createNode();
			  nodeIndex.Add( node, "key", "value" );
			  RestartTx();

			  nodeIndex.Remove( node );
			  for ( int i = 0; i < 2; i++ )
			  {
					IndexHits<Node> hits = nodeIndex.get( "key", "value" );
					assertEquals( 0, hits.Size() );
					assertNull( hits.Single );
					hits.Close();
					RestartTx();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notAbleToIndexWithForbiddenKey()
		 public virtual void NotAbleToIndexWithForbiddenKey()
		 {
			  Index<Node> index = GraphDb.index().forNodes("check-for-null");
			  Node node = GraphDb.createNode();
			  try
			  {
					index.Add( node, null, "not allowed" );
					fail( "Shouldn't be able to index something with null key" );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }

			  try
			  {
					index.Add( node, "_id_", "not allowed" );
					fail( "Shouldn't be able to index something with null key" );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }
		 }

		 private Node CreateAndIndexNode( Index<Node> index, string key, string value )
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( key, value );
			  index.Add( node, key, value );
			  return node;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveNodeFromIndex()
		 public virtual void TestRemoveNodeFromIndex()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "key";
			  string value = "MYID";
			  Node node = CreateAndIndexNode( index, key, value );
			  index.Remove( node );
			  node.Delete();

			  Node node2 = CreateAndIndexNode( index, key, value );
			  assertEquals( node2, index.get( key, value ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canQueryWithWildcardEvenIfAlternativeRemovalMethodsUsedInSameTx1()
		 public virtual void CanQueryWithWildcardEvenIfAlternativeRemovalMethodsUsedInSameTx1()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  RestartTx();
			  index.Remove( node, "key" );
			  assertNull( index.query( "key", "v*" ).Single );
			  assertNull( index.query( "key", "*" ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canQueryWithWildcardEvenIfAlternativeRemovalMethodsUsedInSameTx2()
		 public virtual void CanQueryWithWildcardEvenIfAlternativeRemovalMethodsUsedInSameTx2()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "key", "value" );
			  RestartTx();
			  index.Remove( node );
			  assertNull( index.query( "key", "v*" ).Single );
			  assertNull( index.query( "key", "*" ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateIndex()
		 public virtual void UpdateIndex()
		 {
			  const string text = "text";
			  const string numeric = "numeric";
			  const string text_1 = "text_1";

			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node n = GraphDb.createNode();
			  index.Add( n, numeric, ( new ValueContext( 5 ) ).indexNumeric() );
			  index.Add( n, text, "text" );
			  index.Add( n, text_1, "text" );
			  CommitTx();

			  BeginTx();
			  assertNotNull( index.query( QueryContext.numericRange( numeric, 5, 5, true, true ) ).Single );
			  assertNotNull( index.get( text_1, "text" ).Single );
			  index.Remove( n, text, "text" );
			  index.Add( n, text, "text 1" );
			  CommitTx();

			  BeginTx();
			  assertNotNull( index.get( text_1, "text" ).Single );
			  assertNotNull( index.query( QueryContext.numericRange( numeric, 5, 5, true, true ) ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exactIndexWithCaseInsensitive()
		 public virtual void ExactIndexWithCaseInsensitive()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Index<Node> index = NodeIndex( stringMap( "analyzer", typeof( LowerCaseKeywordAnalyzer ).FullName ) );
			  Node node = GraphDb.createNode();
			  index.Add( node, "name", "Thomas Anderson" );
			  assertContains( index.query( "name", "\"Thomas Anderson\"" ), node );
			  assertContains( index.query( "name", "\"thoMas ANDerson\"" ), node );
			  RestartTx();
			  assertContains( index.query( "name", "\"Thomas Anderson\"" ), node );
			  assertContains( index.query( "name", "\"thoMas ANDerson\"" ), node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exactIndexWithCaseInsensitiveWithBetterConfig()
		 public virtual void ExactIndexWithCaseInsensitiveWithBetterConfig()
		 {
			  Index<Node> index = GraphDb.index().forNodes("exact-case-insensitive", stringMap("type", "exact", "to_lower_case", "true"));
			  Node node = GraphDb.createNode();
			  index.Add( node, "name", "Thomas Anderson" );
			  assertContains( index.query( "name", "\"Thomas Anderson\"" ), node );
			  assertContains( index.query( "name", "\"thoMas ANDerson\"" ), node );
			  RestartTx();
			  assertContains( index.query( "name", "\"Thomas Anderson\"" ), node );
			  assertContains( index.query( "name", "\"thoMas ANDerson\"" ), node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notAbleToRemoveWithForbiddenKey()
		 public virtual void NotAbleToRemoveWithForbiddenKey()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  index.Add( node, "name", "Mattias" );
			  RestartTx();
			  try
			  {
					index.Remove( node, null );
					fail( "Shouldn't be able to" );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }
			  try
			  {
					index.Remove( node, "_id_" );
					fail( "Shouldn't be able to" );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putIfAbsentSingleThreaded()
		 public virtual void PutIfAbsentSingleThreaded()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  string key = "name";
			  string value = "Mattias";
			  string value2 = "Persson";
			  assertNull( index.PutIfAbsent( node, key, value ) );
			  assertEquals( node, index.get( key, value ).Single );
			  assertNotNull( index.PutIfAbsent( node, key, value ) );
			  assertNull( index.PutIfAbsent( node, key, value2 ) );
			  assertNotNull( index.PutIfAbsent( node, key, value2 ) );
			  RestartTx();
			  assertNotNull( index.PutIfAbsent( node, key, value ) );
			  assertNotNull( index.PutIfAbsent( node, key, value2 ) );
			  assertEquals( node, index.get( key, value ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putIfAbsentMultiThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PutIfAbsentMultiThreaded()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  CommitTx();
			  string key = "name";
			  string value = "Mattias";

			  WorkThread t1 = new WorkThread( "t1", index, GraphDb, node );
			  WorkThread t2 = new WorkThread( "t2", index, GraphDb, node );
			  t1.BeginTransaction();
			  t2.BeginTransaction();
			  assertNull( t2.PutIfAbsent( node, key, value ).get() );
			  Future<Node> futurePut = t1.PutIfAbsent( node, key, value );
			  t1.WaitUntilWaiting();
			  t2.Commit();
			  assertNotNull( futurePut.get() );
			  t1.Commit();
			  t1.Dispose();
			  t2.Dispose();

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					assertEquals( node, index.get( key, value ).Single );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putIfAbsentOnOtherValueInOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PutIfAbsentOnOtherValueInOtherThread()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  CommitTx();
			  string key = "name";
			  string value = "Mattias";
			  string otherValue = "Tobias";

			  WorkThread t1 = new WorkThread( "t1", index, GraphDb, node );
			  WorkThread t2 = new WorkThread( "t2", index, GraphDb, node );
			  t1.BeginTransaction();
			  t2.BeginTransaction();
			  assertNull( t2.PutIfAbsent( node, key, value ).get() );
			  Future<Node> futurePut = t1.PutIfAbsent( node, key, otherValue );
			  t2.Commit();
			  assertNull( futurePut.get() );
			  t1.Commit();
			  t1.Dispose();
			  t2.Dispose();

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					assertEquals( node, index.get( key, value ).Single );
					assertEquals( node, index.get( key, otherValue ).Single );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putIfAbsentOnOtherKeyInOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PutIfAbsentOnOtherKeyInOtherThread()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  CommitTx();
			  string key = "name";
			  string otherKey = "friend";
			  string value = "Mattias";

			  WorkThread t1 = new WorkThread( "t1", index, GraphDb, node );
			  WorkThread t2 = new WorkThread( "t2", index, GraphDb, node );
			  t1.BeginTransaction();
			  t2.BeginTransaction();
			  assertNull( t2.PutIfAbsent( node, key, value ).get() );
			  assertNull( t1.PutIfAbsent( node, otherKey, value ).get() );
			  t2.Commit();
			  t1.Commit();
			  t1.Dispose();
			  t2.Dispose();

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					assertEquals( node, index.get( key, value ).Single );
					assertEquals( node, index.get( otherKey, value ).Single );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putIfAbsentShouldntBlockIfNotAbsent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PutIfAbsentShouldntBlockIfNotAbsent()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  Node node = GraphDb.createNode();
			  string key = "key";
			  string value = "value";
			  index.Add( node, key, value );
			  RestartTx();

			  WorkThread otherThread = new WorkThread( "other thread", index, GraphDb, node );
			  otherThread.BeginTransaction();

			  // Should not grab lock
			  index.PutIfAbsent( node, key, value );

			  // Should be able to complete right away
			  assertNotNull( otherThread.PutIfAbsent( node, key, value ).get() );

			  otherThread.Commit();
			  CommitTx();

			  otherThread.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateNodeWithUniqueFactory()
		 public virtual void getOrCreateNodeWithUniqueFactory()
		 {
			  const string key = "name";
			  const string value = "Mattias";
			  const string property = "counter";

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.Index<org.Neo4Net.graphdb.Node> index = nodeIndex(LuceneIndexImplementation.EXACT_CONFIG);
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();
			  UniqueFactory<Node> factory = new UniqueNodeFactoryAnonymousInnerClass( this, index, key, value, property, counter );
			  Node unique = factory.GetOrCreate( key, value );

			  assertNotNull( unique );
			  assertEquals( "not initialized", 0, unique.GetProperty( property, null ) );
			  assertEquals( unique, index.get( key, value ).Single );

			  assertEquals( unique, factory.GetOrCreate( key, value ) );
			  assertEquals( "initialized more than once", 0, unique.GetProperty( property ) );
			  assertEquals( unique, index.get( key, value ).Single );
			  FinishTx( false );
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly TestLuceneIndex _outerInstance;

			 private string _key;
			 private string _value;
			 private string _property;
			 private AtomicInteger _counter;

			 public UniqueNodeFactoryAnonymousInnerClass( TestLuceneIndex outerInstance, Index<Node> index, string key, string value, string property, AtomicInteger counter ) : base( index )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 this._property = property;
				 this._counter = counter;
			 }

			 protected internal override void initialize( Node node, IDictionary<string, object> properties )
			 {
				  assertEquals( _value, properties[_key] );
				  assertEquals( 1, properties.Count );
				  node.SetProperty( _property, _counter.AndIncrement );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateRelationshipWithUniqueFactory()
		 public virtual void getOrCreateRelationshipWithUniqueFactory()
		 {
			  const string key = "name";
			  const string value = "Mattias";

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node root = graphDb.createNode();
			  Node root = GraphDb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.Index<org.Neo4Net.graphdb.Relationship> index = relationshipIndex(LuceneIndexImplementation.EXACT_CONFIG);
			  Index<Relationship> index = RelationshipIndex( LuceneIndexImplementation.ExactConfig );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.RelationshipType type = withName("SINGLE");
			  RelationshipType type = withName( "SINGLE" );
			  UniqueFactory<Relationship> factory = new UniqueRelationshipFactoryAnonymousInnerClass( this, index, key, value, root, type );

			  Relationship unique = factory.GetOrCreate( key, value );
			  assertEquals( unique, root.GetSingleRelationship( type, Direction.BOTH ) );
			  assertNotNull( unique );

			  assertEquals( unique, index.get( key, value ).Single );

			  assertEquals( unique, factory.GetOrCreate( key, value ) );
			  assertEquals( unique, root.GetSingleRelationship( type, Direction.BOTH ) );
			  assertEquals( unique, index.get( key, value ).Single );

			  FinishTx( false );
		 }

		 private class UniqueRelationshipFactoryAnonymousInnerClass : UniqueFactory.UniqueRelationshipFactory
		 {
			 private readonly TestLuceneIndex _outerInstance;

			 private string _key;
			 private string _value;
			 private Node _root;
			 private RelationshipType _type;

			 public UniqueRelationshipFactoryAnonymousInnerClass( TestLuceneIndex outerInstance, Index<Relationship> index, string key, string value, Node root, RelationshipType type ) : base( index )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 this._root = root;
				 this._type = type;
			 }

			 protected internal override Relationship create( IDictionary<string, object> properties )
			 {
				  assertEquals( _value, properties[_key] );
				  assertEquals( 1, properties.Count );
				  return _root.createRelationshipTo( graphDatabase().createNode(), _type );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOrCreateMultiThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getOrCreateMultiThreaded()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  string key = "name";
			  string value = "Mattias";

			  WorkThread t1 = new WorkThread( "t1", index, GraphDb, null );
			  WorkThread t2 = new WorkThread( "t2", index, GraphDb, null );
			  t1.BeginTransaction();
			  t2.BeginTransaction();
			  Node node = t2.GetOrCreate( key, value, 0 ).get();
			  assertNotNull( node );
			  assertEquals( 0, t2.GetProperty( node, key ) );
			  Future<Node> futurePut = t1.GetOrCreate( key, value, 1 );
			  t1.WaitUntilWaiting();
			  t2.Commit();
			  assertEquals( node, futurePut.get() );
			  assertEquals( 0, t1.GetProperty( node, key ) );
			  t1.Commit();

			  assertEquals( node, index.get( key, value ).Single );

			  t1.Dispose();
			  t2.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useStandardAnalyzer()
		 public virtual void UseStandardAnalyzer()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Index<Node> index = NodeIndex( stringMap( "analyzer", typeof( MyStandardAnalyzer ).FullName ) );
			  Node node = GraphDb.createNode();
			  index.Add( node, "name", "Mattias" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void numericValueForGetInExactIndex()
		 public virtual void NumericValueForGetInExactIndex()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );
			  NumericValueForGet( index );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void numericValueForGetInFulltextIndex()
		 public virtual void NumericValueForGetInFulltextIndex()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.FulltextConfig );
			  NumericValueForGet( index );
		 }

		 private void NumericValueForGet( Index<Node> index )
		 {
			  Node node = GraphDb.createNode();
			  long id = 100L;
			  index.Add( node, "name", ValueContext.numeric( id ) );
			  assertEquals( node, index.get( "name", ValueContext.numeric( id ) ).Single );
			  RestartTx();
			  assertEquals( node, index.get( "name", ValueContext.numeric( id ) ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void combinedNumericalQuery()
		 public virtual void CombinedNumericalQuery()
		 {
			  Index<Node> index = NodeIndex( LuceneIndexImplementation.ExactConfig );

			  Node node = GraphDb.createNode();
			  index.Add( node, "start", ValueContext.numeric( 10 ) );
			  index.Add( node, "end", ValueContext.numeric( 20 ) );
			  RestartTx();

			  BooleanQuery q = new BooleanQuery();
			  q.add( LuceneUtil.RangeQuery( "start", 9, null, true, true ), Occur.MUST );
			  q.add( LuceneUtil.RangeQuery( "end", null, 30, true, true ), Occur.MUST );
			  assertContains( index.query( q ), node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failureToCreateAnIndexShouldNotLeaveConfigurationBehind()
		 public virtual void FailureToCreateAnIndexShouldNotLeaveConfigurationBehind()
		 {
			  // WHEN
			  try
			  {
					// PerFieldAnalyzerWrapper is invalid since it has no public no-arg constructor
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					NodeIndex( stringMap( "analyzer", typeof( PerFieldAnalyzerWrapper ).FullName ) );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					assertThat( e.Message, CoreMatchers.containsString( typeof( PerFieldAnalyzerWrapper ).FullName ) );
			  }

			  // THEN - assert that there's no index config about this index left behind
			  assertFalse( "There should be no index config for index '" + CurrentIndexName() + "' left behind", ((GraphDatabaseAPI)GraphDb).DependencyResolver.resolveDependency(typeof(IndexConfigStore)).has(typeof(Node), CurrentIndexName()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToQueryAllMatchingDocsAfterRemovingWithWildcard()
		 public virtual void ShouldBeAbleToQueryAllMatchingDocsAfterRemovingWithWildcard()
		 {
			  // GIVEN
			  Index<Node> index = NodeIndex( EXACT_CONFIG );
			  Node node1 = GraphDb.createNode();
			  index.Add( node1, "name", "Mattias" );
			  FinishTx( true );
			  BeginTx();

			  // WHEN
			  index.Remove( node1, "name" );
			  ISet<Node> nodes = Iterables.asSet( index.query( "*:*" ) );

			  // THEN
			  assertEquals( asSet(), nodes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDeletedRelationshipWhenQueryingWithStartAndEndNode()
		 public virtual void ShouldNotSeeDeletedRelationshipWhenQueryingWithStartAndEndNode()
		 {
			  // GIVEN
			  RelationshipIndex index = RelationshipIndex( EXACT_CONFIG );
			  Node start = GraphDb.createNode();
			  Node end = GraphDb.createNode();
			  RelationshipType type = withName( "REL" );
			  Relationship rel = start.CreateRelationshipTo( end, type );
			  index.add( rel, "Type", type.Name() );
			  FinishTx( true );
			  BeginTx();

			  // WHEN
			  IndexHits<Relationship> hits = index.Get( "Type", type.Name(), start, end );
			  assertEquals( 1, count( hits ) );
			  assertEquals( 1, hits.Size() );
			  index.remove( rel );

			  // THEN
			  hits = index.Get( "Type", type.Name(), start, end );
			  assertEquals( 0, count( hits ) );
			  assertEquals( 0, hits.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToAddNullValuesToNodeIndex()
		 public virtual void ShouldNotBeAbleToAddNullValuesToNodeIndex()
		 {
			  // GIVEN
			  Index<Node> index = NodeIndex( EXACT_CONFIG );

			  // WHEN single null
			  try
			  {
					index.Add( GraphDb.createNode(), "key", null );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  {
					// THEN Good
			  }

			  // WHEN null in array
			  try
			  {
					index.Add( GraphDb.createNode(), "key", new string[] { "a", null, "c" } );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToAddNullValuesToRelationshipIndex()
		 public virtual void ShouldNotBeAbleToAddNullValuesToRelationshipIndex()
		 {
			  // GIVEN
			  RelationshipIndex index = RelationshipIndex( EXACT_CONFIG );

			  // WHEN single null
			  try
			  {
					index.add( GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), MyRelTypes.TEST), "key", null );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  {
					// THEN Good
			  }

			  // WHEN null in array
			  try
			  {
					index.add( GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), MyRelTypes.TEST), "key", new string[] { "a", null, "c" } );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tooLongValuesAreNotAllowedInNodeIndex()
		 public virtual void TooLongValuesAreNotAllowedInNodeIndex()
		 {
			  Index<Node> index = NodeIndex( EXACT_CONFIG );
			  string tooLongValue = RandomStringUtils.randomAlphabetic( 36000 );
			  try
			  {
					index.Add( GraphDb.createNode(), "key", tooLongValue );
					fail( "Validation exception expected. Such long values are not allowed in the index." );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }

			  try
			  {
					index.Add( GraphDb.createNode(), "key", new string[] { "a", tooLongValue, "c" } );
					fail( "Validation exception expected. Such long values are not allowed in the index." );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tooLongValuesAreNotAllowedInRelationshipIndex()
		 public virtual void TooLongValuesAreNotAllowedInRelationshipIndex()
		 {
			  RelationshipIndex index = RelationshipIndex( EXACT_CONFIG );
			  string tooLongValue = RandomStringUtils.randomAlphabetic( 36000 );
			  try
			  {
					index.add( GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), MyRelTypes.TEST), "key", tooLongValue );
					fail( "Validation exception expected. Such long values are not allowed in the index." );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }

			  try
			  {
					index.add( GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), MyRelTypes.TEST), "key", new string[] { "a", tooLongValue, "c" } );
					fail( "Validation exception expected. Such long values are not allowed in the index." );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveAllMatchingIndexEntriesWhenCallingRemoveWithEntityAndKey()
		 public virtual void ShouldRemoveAllMatchingIndexEntriesWhenCallingRemoveWithEntityAndKey()
		 {
			  // GIVEN
			  Index<Node> index = NodeIndex( FULLTEXT_CONFIG );
			  string key = "the-key";
			  string value1 = "First";
			  string value2 = "Second";
			  Node node = GraphDb.createNode();
			  node.SetProperty( key, value1 );
			  index.Add( node, key, value1 );
			  RestartTx();
			  assertEquals( 1, CountHits( index.query( "*:*" ) ) );

			  // WHEN adding one more property to the index
			  node.SetProperty( key, value2 );
			  index.Add( node, key, value2 ); // intentionally just add to the index
			  RestartTx();
			  assertEquals( 1, CountHits( index.query( "*:*" ) ) );

			  // and WHEN removing by node and key
			  node.RemoveProperty( key );
			  index.Remove( node, key );
			  RestartTx();

			  // THEN
			  assertEquals( 0, CountHits( index.query( "*:*" ) ) );
		 }

		 private long CountHits( IndexHits<Node> query )
		 {
			  try
			  {
					return count( query );
			  }
			  finally
			  {
					query.Close();
			  }
		 }

		 private void QueryAndSortNodesByNumericProperty( Index<Node> index, string numericProperty )
		 {
			  QueryAndSortNodesByNumericProperty( index, numericProperty, i => i );
		 }

		 private void QueryAndSortNodesByNumericProperty<T1>( Index<Node> index, string numericProperty, System.Func<T1> expectedValueProvider ) where T1 : Number
		 {
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					QueryContext queryContext = new QueryContext( numericProperty + ":**" );
					queryContext.Sort( new Sort( new SortedNumericSortField( numericProperty, SortField.Type.INT, false ) ) );
					IndexHits<Node> nodes = index.query( queryContext );

					int nodeIndex = 0;
					foreach ( Node node in nodes )
					{
						 assertEquals( "Nodes should be sorted by numeric property", expectedValueProvider( nodeIndex++ ), node.GetProperty( numericProperty ) );
					}
					transaction.Success();
			  }
		 }

		 private void QueryAndSortNodesByStringProperty( Index<Node> index, string stringProperty, string[] values )
		 {
			  QueryAndSortNodesByStringProperty( index, stringProperty, i => values[i] );
		 }

		 private void QueryAndSortNodesByStringProperty( Index<Node> index, string stringProperty, System.Func<int, string> expectedValueProvider )
		 {
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					QueryContext queryContext = new QueryContext( stringProperty + ":**" );
					queryContext.Sort( new Sort( new SortedSetSortField( stringProperty, true ) ) );
					IndexHits<Node> nodes = index.query( queryContext );

					int nodeIndex = 0;
					foreach ( Node node in nodes )
					{
						 assertEquals( "Nodes should be sorted by string property", expectedValueProvider( nodeIndex++ ), node.GetProperty( stringProperty ) );
					}
					transaction.Success();
			  }
		 }

		 private void DoubleNumericPropertyValueForAllNodesWithLabel( Index<Node> index, string numericProperty, Label label )
		 {
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					ResourceIterator<Node> nodes = GraphDb.findNodes( label );
					nodes.ForEach(node =>
					{
					 node.setProperty( numericProperty, ( int? ) node.getProperty( numericProperty ) * 2 );
					 index.Remove( node, numericProperty );
					 index.Add( node, numericProperty, ( new ValueContext( node.getProperty( numericProperty ) ) ).indexNumeric() );
					});
					transaction.Success();
			  }
		 }

		 private void SetPropertiesAndUpdateToJunior( Index<Node> index, string nameProperty, string jobNameProperty, string[] names, string[] jobs, Label characters )
		 {
			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					for ( int i = 0; i < names.Length; i++ )
					{
						 Node node = GraphDb.createNode( characters );
						 node.SetProperty( nameProperty, names[i] );
						 node.SetProperty( jobNameProperty, jobs[i] );
						 index.Add( node, nameProperty, names[i] );
						 index.Add( node, jobNameProperty, jobs[i] );
					}
					transaction.Success();
			  }

			  using ( Transaction transaction = GraphDb.beginTx() )
			  {
					ResourceIterator<Node> nodes = GraphDb.findNodes( characters );
					nodes.ForEach(node =>
					{
					 node.setProperty( jobNameProperty, "junior " + node.getProperty( jobNameProperty ) );
					 index.Add( node, jobNameProperty, node.getProperty( jobNameProperty ) );
					});
					transaction.Success();
			  }
		 }
	}

}