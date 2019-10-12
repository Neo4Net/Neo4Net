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
namespace Org.Neo4j.Graphdb
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;

	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Neo4jMatchers = Org.Neo4j.Test.mockito.matcher.Neo4jMatchers;
	using SpatialMocks = Org.Neo4j.Test.mockito.mock.SpatialMocks;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.findNodesByLabelAndProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.isEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockCartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockCartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockWGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockWGS84_3D;

	public class IndexingAcceptanceTest
	{
		 /* This test is a bit interesting. It tests a case where we've got a property that sits in one
		  * property block and the value is of a long type. So given that plus that there's an index for that
		  * label/property, do an update that changes the long value into a value that requires two property blocks.
		  * This is interesting because the transaction logic compares before/after views per property record and
		  * not per node as a whole.
		  *
		  * In this case this change will be converted into one "add" and one "remove" property updates instead of
		  * a single "change" property update. At the very basic level it's nice to test for this corner-case so
		  * that the externally observed behavior is correct, even if this test doesn't assert anything about
		  * the underlying add/remove vs. change internal details.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretPropertyAsChangedEvenIfPropertyMovesFromOneRecordToAnother()
		 public virtual void ShouldInterpretPropertyAsChangedEvenIfPropertyMovesFromOneRecordToAnother()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  long smallValue = 10L;
			  long bigValue = 1L << 62;
			  Node myNode;
			  {
					using ( Transaction tx = beansAPI.BeginTx() )
					{
						 myNode = beansAPI.CreateNode( _label1 );
						 myNode.SetProperty( "pad0", true );
						 myNode.SetProperty( "pad1", true );
						 myNode.SetProperty( "pad2", true );
						 // Use a small long here which will only occupy one property block
						 myNode.SetProperty( "key", smallValue );

						 tx.Success();
					}
			  }

			  Neo4jMatchers.createIndex( beansAPI, _label1, "key" );

			  // WHEN
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					// A big long value which will occupy two property blocks
					myNode.SetProperty( "key", bigValue );
					tx.Success();
			  }

			  // THEN
			  assertThat( findNodesByLabelAndProperty( _label1, "key", bigValue, beansAPI ), containsOnly( myNode ) );
			  assertThat( findNodesByLabelAndProperty( _label1, "key", smallValue, beansAPI ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDynamicPropertiesToIndexANodeWhenAddedAlongsideExistingPropertiesInASeparateTransaction()
		 public virtual void ShouldUseDynamicPropertiesToIndexANodeWhenAddedAlongsideExistingPropertiesInASeparateTransaction()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;

			  // When
			  long id;
			  {
					using ( Transaction tx = beansAPI.BeginTx() )
					{
						 Node myNode = beansAPI.CreateNode();
						 id = myNode.Id;
						 myNode.SetProperty( "key0", true );
						 myNode.SetProperty( "key1", true );

						 tx.Success();
					}
			  }

			  Neo4jMatchers.createIndex( beansAPI, _label1, "key2" );
			  Node myNode;
			  {
					using ( Transaction tx = beansAPI.BeginTx() )
					{
						 myNode = beansAPI.GetNodeById( id );
						 myNode.AddLabel( _label1 );
						 myNode.SetProperty( "key2", LONG_STRING );
						 myNode.SetProperty( "key3", LONG_STRING );

						 tx.Success();
					}
			  }

			  // Then
			  assertThat( myNode, inTx( beansAPI, hasProperty( "key2" ).withValue( LONG_STRING ) ) );
			  assertThat( myNode, inTx( beansAPI, hasProperty( "key3" ).withValue( LONG_STRING ) ) );
			  assertThat( findNodesByLabelAndProperty( _label1, "key2", LONG_STRING, beansAPI ), containsOnly( myNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void searchingForNodeByPropertyShouldWorkWithoutIndex()
		 public virtual void SearchingForNodeByPropertyShouldWorkWithoutIndex()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node myNode = CreateNode( beansAPI, map( "name", "Hawking" ), _label1 );

			  // When
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Hawking", beansAPI ), containsOnly( myNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void searchingUsesIndexWhenItExists()
		 public virtual void SearchingUsesIndexWhenItExists()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node myNode = CreateNode( beansAPI, map( "name", "Hawking" ), _label1 );
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );

			  // When
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Hawking", beansAPI ), containsOnly( myNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCorrectlyUpdateIndexesWhenChangingLabelsAndPropertyAtTheSameTime()
		 public virtual void ShouldCorrectlyUpdateIndexesWhenChangingLabelsAndPropertyAtTheSameTime()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node myNode = CreateNode( beansAPI, map( "name", "Hawking" ), _label1, _label2 );
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );
			  Neo4jMatchers.createIndex( beansAPI, _label2, "name" );
			  Neo4jMatchers.createIndex( beansAPI, _label3, "name" );

			  // When
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					myNode.RemoveLabel( _label1 );
					myNode.AddLabel( _label3 );
					myNode.SetProperty( "name", "Einstein" );
					tx.Success();
			  }

			  // Then
			  assertThat( myNode, inTx( beansAPI, hasProperty( "name" ).withValue( "Einstein" ) ) );
			  assertThat( Labels( myNode ), containsOnly( _label2, _label3 ) );

			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Einstein", beansAPI ), Empty );

			  assertThat( findNodesByLabelAndProperty( _label2, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label2, "name", "Einstein", beansAPI ), containsOnly( myNode ) );

			  assertThat( findNodesByLabelAndProperty( _label3, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label3, "name", "Einstein", beansAPI ), containsOnly( myNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCorrectlyUpdateIndexesWhenChangingLabelsAndPropertyMultipleTimesAllAtOnce()
		 public virtual void ShouldCorrectlyUpdateIndexesWhenChangingLabelsAndPropertyMultipleTimesAllAtOnce()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node myNode = CreateNode( beansAPI, map( "name", "Hawking" ), _label1, _label2 );
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );
			  Neo4jMatchers.createIndex( beansAPI, _label2, "name" );
			  Neo4jMatchers.createIndex( beansAPI, _label3, "name" );

			  // When
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					myNode.AddLabel( _label3 );
					myNode.SetProperty( "name", "Einstein" );
					myNode.RemoveLabel( _label1 );
					myNode.SetProperty( "name", "Feynman" );
					tx.Success();
			  }

			  // Then
			  assertThat( myNode, inTx( beansAPI, hasProperty( "name" ).withValue( "Feynman" ) ) );
			  assertThat( Labels( myNode ), containsOnly( _label2, _label3 ) );

			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Einstein", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Feynman", beansAPI ), Empty );

			  assertThat( findNodesByLabelAndProperty( _label2, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label2, "name", "Einstein", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label2, "name", "Feynman", beansAPI ), containsOnly( myNode ) );

			  assertThat( findNodesByLabelAndProperty( _label3, "name", "Hawking", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label3, "name", "Einstein", beansAPI ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label3, "name", "Feynman", beansAPI ), containsOnly( myNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void searchingByLabelAndPropertyReturnsEmptyWhenMissingLabelOrProperty()
		 public virtual void SearchingByLabelAndPropertyReturnsEmptyWhenMissingLabelOrProperty()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;

			  // When/Then
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Hawking", beansAPI ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeIndexUpdatesWhenQueryingOutsideTransaction()
		 public virtual void ShouldSeeIndexUpdatesWhenQueryingOutsideTransaction()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );
			  Node firstNode = CreateNode( beansAPI, map( "name", "Mattias" ), _label1 );

			  // WHEN THEN
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Mattias", beansAPI ), containsOnly( firstNode ) );
			  Node secondNode = CreateNode( beansAPI, map( "name", "Taylor" ), _label1 );
			  assertThat( findNodesByLabelAndProperty( _label1, "name", "Taylor", beansAPI ), containsOnly( secondNode ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdNodeShouldShowUpWithinTransaction()
		 public virtual void CreatedNodeShouldShowUpWithinTransaction()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );

			  // WHEN
			  Transaction tx = beansAPI.BeginTx();

			  Node firstNode = CreateNode( beansAPI, map( "name", "Mattias" ), _label1 );
			  long sizeBeforeDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );
			  firstNode.Delete();
			  long sizeAfterDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );

			  tx.Close();

			  // THEN
			  assertThat( sizeBeforeDelete, equalTo( 1L ) );
			  assertThat( sizeAfterDelete, equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletedNodeShouldShowUpWithinTransaction()
		 public virtual void DeletedNodeShouldShowUpWithinTransaction()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );
			  Node firstNode = CreateNode( beansAPI, map( "name", "Mattias" ), _label1 );

			  // WHEN
			  Transaction tx = beansAPI.BeginTx();

			  long sizeBeforeDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );
			  firstNode.Delete();
			  long sizeAfterDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );

			  tx.Close();

			  // THEN
			  assertThat( sizeBeforeDelete, equalTo( 1L ) );
			  assertThat( sizeAfterDelete, equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdNodeShouldShowUpInIndexQuery()
		 public virtual void CreatedNodeShouldShowUpInIndexQuery()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( beansAPI, _label1, "name" );
			  CreateNode( beansAPI, map( "name", "Mattias" ), _label1 );

			  // WHEN
			  Transaction tx = beansAPI.BeginTx();

			  long sizeBeforeDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );
			  CreateNode( beansAPI, map( "name", "Mattias" ), _label1 );
			  long sizeAfterDelete = count( beansAPI.FindNodes( _label1, "name", "Mattias" ) );

			  tx.Close();

			  // THEN
			  assertThat( sizeBeforeDelete, equalTo( 1L ) );
			  assertThat( sizeAfterDelete, equalTo( 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToQuerySupportedPropertyTypes()
		 public virtual void ShouldBeAbleToQuerySupportedPropertyTypes()
		 {
			  // GIVEN
			  string property = "name";
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( db, _label1, property );

			  // WHEN & THEN
			  AssertCanCreateAndFind( db, _label1, property, "A String" );
			  AssertCanCreateAndFind( db, _label1, property, true );
			  AssertCanCreateAndFind( db, _label1, property, false );
			  AssertCanCreateAndFind( db, _label1, property, ( sbyte ) 56 );
			  AssertCanCreateAndFind( db, _label1, property, 'z' );
			  AssertCanCreateAndFind( db, _label1, property, ( short )12 );
			  AssertCanCreateAndFind( db, _label1, property, 12 );
			  AssertCanCreateAndFind( db, _label1, property, 12L );
			  AssertCanCreateAndFind( db, _label1, property, ( float )12.0 );
			  AssertCanCreateAndFind( db, _label1, property, 12.0 );
			  AssertCanCreateAndFind( db, _label1, property, SpatialMocks.mockPoint( 12.3, 45.6, mockWGS84() ) );
			  AssertCanCreateAndFind( db, _label1, property, SpatialMocks.mockPoint( 123, 456, mockCartesian() ) );
			  AssertCanCreateAndFind( db, _label1, property, SpatialMocks.mockPoint( 12.3, 45.6, 100.0, mockWGS84_3D() ) );
			  AssertCanCreateAndFind( db, _label1, property, SpatialMocks.mockPoint( 123, 456, 789, mockCartesian_3D() ) );
			  AssertCanCreateAndFind( db, _label1, property, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) );
			  AssertCanCreateAndFind( db, _label1, property, Values.pointValue( CoordinateReferenceSystem.Cartesian, 123, 456 ) );
			  AssertCanCreateAndFind( db, _label1, property, Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.3, 45.6, 100.0 ) );
			  AssertCanCreateAndFind( db, _label1, property, Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 123, 456, 789 ) );

			  AssertCanCreateAndFind( db, _label1, property, new string[]{ "A String" } );
			  AssertCanCreateAndFind( db, _label1, property, new bool[]{ true } );
			  AssertCanCreateAndFind( db, _label1, property, new bool?[]{ false } );
			  AssertCanCreateAndFind( db, _label1, property, new sbyte[]{ 56 } );
			  AssertCanCreateAndFind( db, _label1, property, new sbyte?[]{ 57 } );
			  AssertCanCreateAndFind( db, _label1, property, new char[]{ 'a' } );
			  AssertCanCreateAndFind( db, _label1, property, new char?[]{ 'b' } );
			  AssertCanCreateAndFind( db, _label1, property, new short[]{ 12 } );
			  AssertCanCreateAndFind( db, _label1, property, new short?[]{ 13 } );
			  AssertCanCreateAndFind( db, _label1, property, new int[]{ 14 } );
			  AssertCanCreateAndFind( db, _label1, property, new int?[]{ 15 } );
			  AssertCanCreateAndFind( db, _label1, property, new long[]{ 16L } );
			  AssertCanCreateAndFind( db, _label1, property, new long?[]{ 17L } );
			  AssertCanCreateAndFind( db, _label1, property, new float[]{ ( float )18.0 } );
			  AssertCanCreateAndFind( db, _label1, property, new float?[]{ ( float )19.0 } );
			  AssertCanCreateAndFind( db, _label1, property, new double[]{ 20.0 } );
			  AssertCanCreateAndFind( db, _label1, property, new double?[]{ 21.0 } );
			  AssertCanCreateAndFind( db, _label1, property, new Point[]{ SpatialMocks.mockPoint( 12.3, 45.6, mockWGS84() ) } );
			  AssertCanCreateAndFind( db, _label1, property, new Point[]{ SpatialMocks.mockPoint( 123, 456, mockCartesian() ) } );
			  AssertCanCreateAndFind( db, _label1, property, new Point[]{ SpatialMocks.mockPoint( 12.3, 45.6, 100.0, mockWGS84_3D() ) } );
			  AssertCanCreateAndFind( db, _label1, property, new Point[]{ SpatialMocks.mockPoint( 123, 456, 789, mockCartesian_3D() ) } );
			  AssertCanCreateAndFind( db, _label1, property, new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) } );
			  AssertCanCreateAndFind( db, _label1, property, new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian, 123, 456 ) } );
			  AssertCanCreateAndFind( db, _label1, property, new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.3, 45.6, 100.0 ) } );
			  AssertCanCreateAndFind( db, _label1, property, new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 123, 456, 789 ) } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetrieveMultipleNodesWithSameValueFromIndex()
		 public virtual void ShouldRetrieveMultipleNodesWithSameValueFromIndex()
		 {
			  // this test was included here for now as a precondition for the following test

			  // given
			  GraphDatabaseService graph = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( graph, _label1, "name" );

			  Node node1;
			  Node node2;
			  using ( Transaction tx = graph.BeginTx() )
			  {
					node1 = graph.CreateNode( _label1 );
					node1.SetProperty( "name", "Stefan" );

					node2 = graph.CreateNode( _label1 );
					node2.SetProperty( "name", "Stefan" );
					tx.Success();
			  }

			  using ( Transaction tx = graph.BeginTx() )
			  {
					ResourceIterator<Node> result = graph.FindNodes( _label1, "name", "Stefan" );
					assertEquals( asSet( node1, node2 ), asSet( result ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenMultipleResultsForSingleNode()
		 public virtual void ShouldThrowWhenMultipleResultsForSingleNode()
		 {
			  // given
			  GraphDatabaseService graph = DbRule.GraphDatabaseAPI;
			  Neo4jMatchers.createIndex( graph, _label1, "name" );

			  Node node1;
			  Node node2;
			  using ( Transaction tx = graph.BeginTx() )
			  {
					node1 = graph.CreateNode( _label1 );
					node1.SetProperty( "name", "Stefan" );

					node2 = graph.CreateNode( _label1 );
					node2.SetProperty( "name", "Stefan" );
					tx.Success();
			  }

			  try
			  {
					  using ( Transaction tx = graph.BeginTx() )
					  {
						graph.FindNode( _label1, "name", "Stefan" );
						fail( "Expected MultipleFoundException but got none" );
					  }
			  }
			  catch ( MultipleFoundException e )
			  {
					assertThat( e.Message, equalTo( format( "Found multiple nodes with label: '%s', property name: 'name' " + "and property value: 'Stefan' while only one was expected.", _label1 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddIndexedPropertyToNodeWithDynamicLabels()
		 public virtual void ShouldAddIndexedPropertyToNodeWithDynamicLabels()
		 {
			  // Given
			  int indexesCount = 20;
			  string labelPrefix = "foo";
			  string propertyKeyPrefix = "bar";
			  string propertyValuePrefix = "baz";
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  for ( int i = 0; i < indexesCount; i++ )
			  {
					Neo4jMatchers.createIndexNoWait( db, Label.label( labelPrefix + i ), propertyKeyPrefix + i );
			  }
			  Neo4jMatchers.waitForIndexes( db );

			  // When
			  long nodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					nodeId = Db.createNode().Id;
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( nodeId );
					for ( int i = 0; i < indexesCount; i++ )
					{
						 node.AddLabel( Label.label( labelPrefix + i ) );
						 node.SetProperty( propertyKeyPrefix + i, propertyValuePrefix + i );
					}
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < indexesCount; i++ )
					{
						 Label label = Label.label( labelPrefix + i );
						 string key = propertyKeyPrefix + i;
						 string value = propertyValuePrefix + i;

						 ResourceIterator<Node> nodes = Db.findNodes( label, key, value );
						 assertEquals( 1, Iterators.count( nodes ) );
					}
					tx.Success();
			  }
		 }

		 private void AssertCanCreateAndFind( GraphDatabaseService db, Label label, string propertyKey, object value )
		 {
			  Node created = CreateNode( db, map( propertyKey, value ), label );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node found = Db.findNode( label, propertyKey, value );
					assertThat( found, equalTo( created ) );
					found.Delete();
					tx.Success();
			  }
		 }

		 public const string LONG_STRING = "a long string that has to be stored in dynamic records";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();

		 private Label _label1;
		 private Label _label2;
		 private Label _label3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupLabels()
		 public virtual void SetupLabels()
		 {
			  _label1 = Label.label( "LABEL1-" + TestName.MethodName );
			  _label2 = Label.label( "LABEL2-" + TestName.MethodName );
			  _label3 = Label.label( "LABEL3-" + TestName.MethodName );
		 }

		 private Node CreateNode( GraphDatabaseService beansAPI, IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					Node node = beansAPI.CreateNode( labels );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.mockito.matcher.Neo4jMatchers.Deferred<Label> labels(final Node myNode)
		 private Neo4jMatchers.Deferred<Label> Labels( Node myNode )
		 {
			  return new DeferredAnonymousInnerClass( this, DbRule.GraphDatabaseAPI, myNode );
		 }

		 private class DeferredAnonymousInnerClass : Neo4jMatchers.Deferred<Label>
		 {
			 private readonly IndexingAcceptanceTest _outerInstance;

			 private Org.Neo4j.Graphdb.Node _myNode;

			 public DeferredAnonymousInnerClass( IndexingAcceptanceTest outerInstance, Org.Neo4j.Kernel.@internal.GraphDatabaseAPI getGraphDatabaseAPI, Org.Neo4j.Graphdb.Node myNode ) : base( getGraphDatabaseAPI )
			 {
				 this.outerInstance = outerInstance;
				 this._myNode = myNode;
			 }

			 protected internal override IEnumerable<Label> manifest()
			 {
				  return _myNode.Labels;
			 }
		 }
	}

}