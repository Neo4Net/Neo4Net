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
namespace Neo4Net.Kernel.impl.core
{
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using GraphTransactionRule = Neo4Net.Test.rule.GraphTransactionRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class TestShortStringProperties
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.DatabaseRule graphdb = new Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public static DatabaseRule Graphdb = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.GraphTransactionRule tx = new Neo4Net.test.rule.GraphTransactionRule(graphdb);
		 public GraphTransactionRule Tx = new GraphTransactionRule( Graphdb );

		 public virtual void Commit()
		 {
			  Tx.success();
		 }

		 private void NewTx()
		 {
			  Tx.success();
			  Tx.begin();
		 }

		 private const string LONG_STRING = "this is a really long string, believe me!";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddMultipleShortStringsToTheSameNode()
		 public virtual void CanAddMultipleShortStringsToTheSameNode()
		 {
			  Node node = Graphdb.GraphDatabaseAPI.createNode();
			  node.SetProperty( "key", "value" );
			  node.SetProperty( "reverse", "esrever" );
			  Commit();
			  assertThat( node, inTx( Graphdb.GraphDatabaseAPI, hasProperty( "key" ).withValue( "value" ) ) );
			  assertThat( node, inTx( Graphdb.GraphDatabaseAPI, hasProperty( "reverse" ).withValue( "esrever" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddShortStringToRelationship()
		 public virtual void CanAddShortStringToRelationship()
		 {
			  IGraphDatabaseService db = Graphdb.GraphDatabaseAPI;
			  Relationship rel = Db.createNode().createRelationshipTo(Db.createNode(), withName("REL_TYPE"));
			  rel.SetProperty( "type", "dimsedut" );
			  Commit();
			  assertThat( rel, inTx( db, hasProperty( "type" ).withValue( "dimsedut" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canUpdateShortStringInplace()
		 public virtual void CanUpdateShortStringInplace()
		 {
			  Node node = Graphdb.GraphDatabaseAPI.createNode();
			  node.SetProperty( "key", "value" );

			  NewTx();

			  assertEquals( "value", node.GetProperty( "key" ) );

			  node.SetProperty( "key", "other" );
			  Commit();

			  assertThat( node, inTx( Graphdb.GraphDatabaseAPI, hasProperty( "key" ).withValue( "other" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReplaceLongStringWithShortString()
		 public virtual void CanReplaceLongStringWithShortString()
		 {
			  Node node = Graphdb.GraphDatabaseAPI.createNode();
			  node.SetProperty( "key", LONG_STRING );
			  NewTx();

			  assertEquals( LONG_STRING, node.GetProperty( "key" ) );

			  node.SetProperty( "key", "value" );
			  Commit();

			  assertThat( node, inTx( Graphdb.GraphDatabaseAPI, hasProperty( "key" ).withValue( "value" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReplaceShortStringWithLongString()
		 public virtual void CanReplaceShortStringWithLongString()
		 {
			  Node node = Graphdb.GraphDatabaseAPI.createNode();
			  node.SetProperty( "key", "value" );
			  NewTx();

			  assertEquals( "value", node.GetProperty( "key" ) );

			  node.SetProperty( "key", LONG_STRING );
			  Commit();

			  assertThat( node, inTx( Graphdb.GraphDatabaseAPI, hasProperty( "key" ).withValue( LONG_STRING ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canRemoveShortStringProperty()
		 public virtual void CanRemoveShortStringProperty()
		 {
			  IGraphDatabaseService db = Graphdb.GraphDatabaseAPI;
			  Node node = Db.createNode();
			  node.SetProperty( "key", "value" );
			  NewTx();

			  assertEquals( "value", node.GetProperty( "key" ) );

			  node.RemoveProperty( "key" );
			  Commit();

			  assertThat( node, inTx( db, not( hasProperty( "key" ) ) ) );
		 }
	}

}