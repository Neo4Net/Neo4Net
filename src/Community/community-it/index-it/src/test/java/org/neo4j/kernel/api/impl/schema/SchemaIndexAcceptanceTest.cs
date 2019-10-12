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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Schema_IndexState = Neo4Net.Graphdb.schema.Schema_IndexState;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.loop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.createIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.findNodesByLabelAndProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.getIndexes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.haveState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.isEmpty;

	public class SchemaIndexAcceptanceTest
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaIndexAcceptanceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_label = _label( "PERSON" );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 private GraphDatabaseService _db;
		 private Label _label;
		 private readonly string _propertyKey = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _db = NewDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexOnExistingDataBuildsIndexWhichWillBeOnlineNextStartup()
		 public virtual void CreatingIndexOnExistingDataBuildsIndexWhichWillBeOnlineNextStartup()
		 {
			  Node node1;
			  Node node2;
			  Node node3;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node1 = CreateNode( _label, "name", "One" );
					node2 = CreateNode( _label, "name", "Two" );
					node3 = CreateNode( _label, "name", "Three" );
					tx.Success();
			  }

			  createIndex( _db, _label, _propertyKey );

			  Restart();

			  assertThat( findNodesByLabelAndProperty( _label, "name", "One", _db ), containsOnly( node1 ) );
			  assertThat( findNodesByLabelAndProperty( _label, "name", "Two", _db ), containsOnly( node2 ) );
			  assertThat( findNodesByLabelAndProperty( _label, "name", "Three", _db ), containsOnly( node3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexArrays()
		 public virtual void ShouldIndexArrays()
		 {
			  long[] arrayPropertyValue = new long[] { 42, 23, 87 };
			  createIndex( _db, _label, _propertyKey );
			  Node node1;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node1 = CreateNode( _label, _propertyKey, arrayPropertyValue );
					tx.Success();
			  }

			  Restart();

			  assertThat( getIndexes( _db, _label ), inTx( _db, haveState( _db, Schema_IndexState.ONLINE ) ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, arrayPropertyValue, _db ), containsOnly( node1 ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, new long[]{ 42, 23 }, _db ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, Arrays.ToString( arrayPropertyValue ), _db ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexStringArrays()
		 public virtual void ShouldIndexStringArrays()
		 {
			  string[] arrayPropertyValue = new string[] { "A, B", "C" };
			  createIndex( _db, _label, _propertyKey );
			  Node node1;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node1 = CreateNode( _label, _propertyKey, arrayPropertyValue );
					tx.Success();
			  }

			  Restart();

			  assertThat( getIndexes( _db, _label ), inTx( _db, haveState( _db, Schema_IndexState.ONLINE ) ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, arrayPropertyValue, _db ), containsOnly( node1 ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, new string[]{ "A", "B, C" }, _db ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, Arrays.ToString( arrayPropertyValue ), _db ), Empty );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexArraysPostPopulation()
		 public virtual void ShouldIndexArraysPostPopulation()
		 {
			  long[] arrayPropertyValue = new long[] { 42, 23, 87 };
			  Node node1;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node1 = CreateNode( _label, _propertyKey, arrayPropertyValue );
					tx.Success();
			  }

			  createIndex( _db, _label, _propertyKey );

			  Restart();

			  assertThat( getIndexes( _db, _label ), inTx( _db, haveState( _db, Schema_IndexState.ONLINE ) ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, arrayPropertyValue, _db ), containsOnly( node1 ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, new long[]{ 42, 23 }, _db ), Empty );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, Arrays.ToString( arrayPropertyValue ), _db ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoveryAfterCreateAndDropIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoveryAfterCreateAndDropIndex()
		 {
			  // GIVEN
			  IndexDefinition indexDefinition = createIndex( _db, _label, _propertyKey );
			  CreateSomeData( _label, _propertyKey );
			  DoStuff( _db, _label, _propertyKey );
			  DropIndex( indexDefinition );
			  DoStuff( _db, _label, _propertyKey );

			  // WHEN
			  CrashAndRestart();

			  // THEN
			  assertThat( getIndexes( _db, _label ), Empty );
		 }

		 private GraphDatabaseService NewDb()
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(FsRule.get())).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void crashAndRestart() throws Exception
		 private void CrashAndRestart()
		 {
			  FsRule.snapshot( _db.shutdown );
			  _db = NewDb();
		 }

		 private void Restart()
		 {
			  _db.shutdown();
			  _db = NewDb();
		 }

		 private Node CreateNode( Label label, params object[] properties )
		 {
			  Node node = _db.createNode( label );
			  foreach ( KeyValuePair<string, object> property in map( properties ).entrySet() )
			  {
					node.SetProperty( property.Key, property.Value );
			  }
			  return node;
		 }

		 private void DropIndex( IndexDefinition indexDefinition )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					indexDefinition.Drop();
					tx.Success();
			  }
		 }

		 private static void DoStuff( GraphDatabaseService db, Label label, string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in loop( Db.findNodes( label, propertyKey, 3323 ) ) )
					{
						 count( node.Labels );
					}
			  }
		 }

		 private void CreateSomeData( Label label, string propertyKey )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( label );
					node.SetProperty( propertyKey, "yeah" );
					tx.Success();
			  }
		 }
	}

}