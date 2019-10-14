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
namespace Neo4Net.Server.rest.domain
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using PropertyValueException = Neo4Net.Server.rest.web.PropertyValueException;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class PropertySettingStrategyTest
	{
		 private static GraphDatabaseAPI _db;
		 private Transaction _tx;
		 private static PropertySettingStrategy _propSetter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void createDb()
		 public static void CreateDb()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  _propSetter = new PropertySettingStrategy( _db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void closeDb()
		 public static void CloseDb()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void beginTx()
		 public virtual void BeginTx()
		 {
			  _tx = _db.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void rollbackTx()
		 public virtual void RollbackTx()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetSingleProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetSingleProperty()
		 {
			  // Given
			  Node node = _db.createNode();

			  // When
			  _propSetter.setProperty( node, "name", "bob" );

			  // Then
			  assertThat( node.GetProperty( "name" ), @is( "bob" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetMultipleProperties()
		 {
			  // Given
			  Node node = _db.createNode();

			  IList<string> anArray = new List<string>();
			  anArray.Add( "hello" );
			  anArray.Add( "Iamanarray" );

			  IDictionary<string, object> props = new Dictionary<string, object>();
			  props["name"] = "bob";
			  props["age"] = 12;
			  props["anArray"] = anArray;

			  // When
			  _propSetter.setProperties( node, props );

			  // Then
			  assertThat( node.GetProperty( "name" ), @is( "bob" ) );
			  assertThat( node.GetProperty( "age" ), @is( 12 ) );
			  assertThat( node.GetProperty( "anArray" ), @is( new string[]{ "hello", "Iamanarray" } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllProperties()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "name", "bob" );
			  node.SetProperty( "age", 12 );

			  // When
			  _propSetter.setAllProperties( node, map( "name", "Steven", "color", 123 ) );

			  // Then
			  assertThat( node.GetProperty( "name" ), @is( "Steven" ) );
			  assertThat( node.GetProperty( "color" ), @is( 123 ) );
			  assertThat( node.HasProperty( "age" ), @is( false ) );
		 }

		 // Handling empty collections

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailSettingEmptyArrayIfEntityAlreadyHasAnEmptyArrayAsValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFailSettingEmptyArrayIfEntityAlreadyHasAnEmptyArrayAsValue()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "arr", new string[]{} );

			  // When
			  _propSetter.setProperty( node, "arr", new List<>() );

			  // Then
			  assertThat( node.GetProperty( "arr" ), @is( new string[]{} ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailSettingEmptyArrayAndOtherValuesIfEntityAlreadyHasAnEmptyArrayAsValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFailSettingEmptyArrayAndOtherValuesIfEntityAlreadyHasAnEmptyArrayAsValue()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "arr", new string[]{} );

			  IDictionary<string, object> props = new Dictionary<string, object>();
			  props["name"] = "bob";
			  props["arr"] = new List<string>();

			  // When
			  _propSetter.setProperties( node, props );

			  // Then
			  assertThat( node.GetProperty( "name" ), @is( "bob" ) );
			  assertThat( node.GetProperty( "arr" ), @is( new string[]{} ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.web.PropertyValueException.class) public void shouldThrowPropertyErrorWhenSettingEmptyArrayOnEntityWithNoPreExistingProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowPropertyErrorWhenSettingEmptyArrayOnEntityWithNoPreExistingProperty()
		 {
			  // Given
			  Node node = _db.createNode();

			  // When
			  _propSetter.setProperty( node, "arr", new List<>() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.web.PropertyValueException.class) public void shouldThrowPropertyErrorWhenSettingEmptyArrayOnEntityWithNoPreExistingEmptyArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowPropertyErrorWhenSettingEmptyArrayOnEntityWithNoPreExistingEmptyArray()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "arr", "hello" );

			  // When
			  _propSetter.setProperty( node, "arr", new List<>() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseOriginalTypeWhenSettingEmptyArrayIfEntityAlreadyHasACollection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseOriginalTypeWhenSettingEmptyArrayIfEntityAlreadyHasACollection()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "arr", new string[]{ "a", "b" } );

			  // When
			  _propSetter.setProperty( node, "arr", new List<>() );

			  // Then
			  assertThat( node.GetProperty( "arr" ), @is( new string[]{} ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseOriginalTypeOnEmptyCollectionWhenSettingAllProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseOriginalTypeOnEmptyCollectionWhenSettingAllProperties()
		 {
			  // Given
			  Node node = _db.createNode();
			  node.SetProperty( "name", "bob" );
			  node.SetProperty( "arr", new string[]{ "a", "b" } );

			  // When
			  _propSetter.setAllProperties( node, map( "arr", new List<string>() ) );

			  // Then
			  assertThat( node.HasProperty( "name" ), @is( false ) );
			  assertThat( node.GetProperty( "arr" ), @is( new string[]{} ) );
		 }

	}

}