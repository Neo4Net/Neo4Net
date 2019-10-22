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
namespace Neo4Net.Cypher.export
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using ExecutionResult = Neo4Net.Cypher.Internal.javacompat.ExecutionResult;
	using PathImpl = Neo4Net.Graphalgo.impl.util.PathImpl;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ExportTest
	{

		 private IGraphDatabaseService _gdb;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _gdb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  _tx = _gdb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
			  _gdb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEmptyGraph()
		 public virtual void TestEmptyGraph()
		 {
			  assertEquals( "", DoExportGraph( _gdb ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeWithProperties()
		 public virtual void TestNodeWithProperties()
		 {
			  _gdb.createNode().setProperty("name", "Andres");
			  assertEquals( "create (_0 {`name`:\"Andres\"})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(_gdb) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeWithFloatProperty()
		 public virtual void TestNodeWithFloatProperty()
		 {
			  const float floatValue = 10.1f;
			  const string expected = "10.100000";
			  _gdb.createNode().setProperty("float", floatValue);
			  assertEquals( "create (_0 {`float`:" + expected + "})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(_gdb) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeWithDoubleProperty()
		 public virtual void TestNodeWithDoubleProperty()
		 {
			  const double doubleValue = 123456.123456;
			  const string expected = "123456.123456";
			  _gdb.createNode().setProperty("double", doubleValue);
			  assertEquals( "create (_0 {`double`:" + expected + "})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(_gdb) );
		 }

		 private string DoExportGraph( IGraphDatabaseService db )
		 {
			  SubGraph graph = DatabaseSubGraph.From( db );
			  return DoExportGraph( graph );
		 }

		 private string DoExportGraph( SubGraph graph )
		 {
			  StringWriter @out = new StringWriter();
			  ( new SubGraphExporter( graph ) ).Export( new PrintWriter( @out ) );
			  return @out.ToString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFromSimpleCypherResult()
		 public virtual void TestFromSimpleCypherResult()
		 {
			  Node n = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + ")" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleNode()
		 public virtual void TestSingleNode()
		 {
			  Node n = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + ")" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleNodeWithProperties()
		 public virtual void TestSingleNodeWithProperties()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", "Node1" );
			  n.SetProperty( "age", 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`age`:42, `name`:\"Node1\"})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingOfNodeStringPropertyValue()
		 public virtual void TestEscapingOfNodeStringPropertyValue()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", "Brutus \"Brutal\" Howell" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`name`:\"Brutus \\\"Brutal\\\" Howell\"})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingOfNodeStringArrayPropertyValue()
		 public virtual void TestEscapingOfNodeStringArrayPropertyValue()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", new string[]{ "Brutus \"Brutal\" Howell", "Dr." } );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`name`:[\"Brutus \\\"Brutal\\\" Howell\", \"Dr.\"]})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingOfRelationshipStringPropertyValue()
		 public virtual void TestEscapingOfRelationshipStringPropertyValue()
		 {
			  Node n = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Relationship rel = n.createRelationshipTo(n, org.Neo4Net.graphdb.RelationshipType.withName("REL"));
			  Relationship rel = n.CreateRelationshipTo( n, RelationshipType.withName( "REL" ) );
			  rel.SetProperty( "name", "Brutus \"Brutal\" Howell" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("rel", rel);
			  ExecutionResult result = result( "rel", rel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create (_0)" + lineSeparator() + "create (_0)-[:`REL` {`name`:\"Brutus \\\"Brutal\\\" Howell\"}]->(_0)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingOfRelationshipStringArrayPropertyValue()
		 public virtual void TestEscapingOfRelationshipStringArrayPropertyValue()
		 {
			  Node n = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Relationship rel = n.createRelationshipTo(n, org.Neo4Net.graphdb.RelationshipType.withName("REL"));
			  Relationship rel = n.CreateRelationshipTo( n, RelationshipType.withName( "REL" ) );
			  rel.SetProperty( "name", new string[]{ "Brutus \"Brutal\" Howell", "Dr." } );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("rel", rel);
			  ExecutionResult result = result( "rel", rel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create (_0)" + lineSeparator() + "create (_0)-[:`REL` {`name`:[\"Brutus \\\"Brutal\\\" Howell\", \"Dr.\"]}]->(_0)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingStringPropertyWithBackslash()
		 public virtual void TestEscapingStringPropertyWithBackslash()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", "Some\\thing" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`name`:\"Some\\\\thing\"})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEscapingStringPropertyWithBackslashAndDoubleQuote()
		 public virtual void TestEscapingStringPropertyWithBackslashAndDoubleQuote()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", "Some\\\"thing" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`name`:\"Some\\\\\\\"thing\"})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleNodeWithArrayProperties()
		 public virtual void TestSingleNodeWithArrayProperties()
		 {
			  Node n = _gdb.createNode();
			  n.SetProperty( "name", new string[]{ "a", "b" } );
			  n.SetProperty( "age", new int[]{ 1, 2 } );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + " {`age`:[1, 2], `name`:[\"a\", \"b\"]})" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleNodeLabels()
		 public virtual void TestSingleNodeLabels()
		 {
			  Node n = _gdb.createNode();
			  n.AddLabel( Label.label( "Foo" ) );
			  n.AddLabel( Label.label( "Bar" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, false);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, false );
			  assertEquals( "create (_" + n.Id + ":`Foo`:`Bar`)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExportIndex()
		 public virtual void TestExportIndex()
		 {
			  _gdb.schema().indexFor(Label.label("Foo")).on("bar").create();
			  assertEquals( "create index on :`Foo`(`bar`);" + lineSeparator(), DoExportGraph(_gdb) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExportUniquenessConstraint()
		 public virtual void TestExportUniquenessConstraint()
		 {
			  _gdb.schema().constraintFor(Label.label("Foo")).assertPropertyIsUnique("bar").create();
			  assertEquals( "create constraint on (n:`Foo`) assert n.`bar` is unique;" + lineSeparator(), DoExportGraph(_gdb) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExportIndexesViaCypherResult()
		 public virtual void TestExportIndexesViaCypherResult()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Label label = org.Neo4Net.graphdb.Label.label("Foo");
			  Label label = Label.label( "Foo" );
			  _gdb.schema().indexFor(label).on("bar").create();
			  _gdb.schema().indexFor(label).on("bar2").create();
			  CommitAndStartNewTransactionAfterSchemaChanges();
			  Node n = _gdb.createNode( label );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create index on :`Foo`(`bar2`);" + lineSeparator() + "create index on :`Foo`(`bar`);" + lineSeparator() + "create (_0:`Foo`)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExportConstraintsViaCypherResult()
		 public virtual void TestExportConstraintsViaCypherResult()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Label label = org.Neo4Net.graphdb.Label.label("Foo");
			  Label label = Label.label( "Foo" );
			  _gdb.schema().constraintFor(label).assertPropertyIsUnique("bar").create();
			  _gdb.schema().constraintFor(label).assertPropertyIsUnique("bar2").create();
			  CommitAndStartNewTransactionAfterSchemaChanges();
			  Node n = _gdb.createNode( label );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("node", n);
			  ExecutionResult result = result( "node", n );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create constraint on (n:`Foo`) assert n.`bar2` is unique;" + lineSeparator() + "create constraint on (n:`Foo`) assert n.`bar` is unique;" + lineSeparator() + "create (_0:`Foo`)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

		 private void CommitAndStartNewTransactionAfterSchemaChanges()
		 {
			  _tx.success();
			  _tx.close();
			  _tx = _gdb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFromRelCypherResult()
		 public virtual void TestFromRelCypherResult()
		 {
			  Node n = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Relationship rel = n.createRelationshipTo(n, org.Neo4Net.graphdb.RelationshipType.withName("REL"));
			  Relationship rel = n.CreateRelationshipTo( n, RelationshipType.withName( "REL" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("rel", rel);
			  ExecutionResult result = result( "rel", rel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create (_0)" + lineSeparator() + "create (_0)-[:`REL`]->(_0)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFromPathCypherResult()
		 public virtual void TestFromPathCypherResult()
		 {
			  Node n1 = _gdb.createNode();
			  Node n2 = _gdb.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Relationship rel = n1.createRelationshipTo(n2, org.Neo4Net.graphdb.RelationshipType.withName("REL"));
			  Relationship rel = n1.CreateRelationshipTo( n2, RelationshipType.withName( "REL" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Path path = new org.Neo4Net.graphalgo.impl.util.PathImpl.Builder(n1).push(rel).build();
			  Path path = ( new PathImpl.Builder( n1 ) ).push( rel ).build();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.cypher.internal.javacompat.ExecutionResult result = result("path", path);
			  ExecutionResult result = result( "path", path );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = CypherResultSubGraph.from(result, gdb, true);
			  SubGraph graph = CypherResultSubGraph.From( result, _gdb, true );
			  assertEquals( "create (_0)" + lineSeparator() + "create (_1)" + lineSeparator() + "create (_0)-[:`REL`]->(_1)" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.Neo4Net.cypher.internal.javacompat.ExecutionResult result(String column, Object value)
		 private ExecutionResult Result( string column, object value )
		 {
			  ExecutionResult result = Mockito.mock( typeof( ExecutionResult ) );
			  Mockito.when( result.Columns() ).thenReturn(asList(column));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<java.util.Map<String, Object>> inner = asList(singletonMap(column, value)).iterator();
			  IEnumerator<IDictionary<string, object>> inner = asList( singletonMap( column, value ) ).GetEnumerator();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.ResourceIterator<java.util.Map<String, Object>> iterator = new org.Neo4Net.graphdb.ResourceIterator<java.util.Map<String, Object>>()
			  ResourceIterator<IDictionary<string, object>> iterator = new ResourceIteratorAnonymousInnerClass( this, inner );

			  Mockito.when( result.GetEnumerator() ).thenReturn(iterator);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Mockito.when( result.HasNext() ).thenAnswer(invocation => iterator.hasNext());
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Mockito.when( result.Next() ).thenAnswer(invocation => iterator.next());
			  return result;
		 }

		 private class ResourceIteratorAnonymousInnerClass : ResourceIterator<IDictionary<string, object>>
		 {
			 private readonly ExportTest _outerInstance;

			 private IEnumerator<IDictionary<string, object>> _inner;

			 public ResourceIteratorAnonymousInnerClass( ExportTest outerInstance, IEnumerator<IDictionary<string, object>> inner )
			 {
				 this.outerInstance = outerInstance;
				 this._inner = inner;
			 }

			 public void close()
			 {
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _inner.hasNext();
			 }

			 public IDictionary<string, object> next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _inner.next();
			 }

			 public void remove()
			 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
				  _inner.remove();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFromSimpleGraph()
		 public virtual void TestFromSimpleGraph()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node n0 = gdb.createNode();
			  Node n0 = _gdb.createNode();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node n1 = gdb.createNode();
			  Node n1 = _gdb.createNode();
			  n1.SetProperty( "name", "Node1" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Relationship relationship = n0.createRelationshipTo(n1, org.Neo4Net.graphdb.RelationshipType.withName("REL"));
			  Relationship relationship = n0.CreateRelationshipTo( n1, RelationshipType.withName( "REL" ) );
			  relationship.SetProperty( "related", true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SubGraph graph = DatabaseSubGraph.from(gdb);
			  SubGraph graph = DatabaseSubGraph.From( _gdb );
			  assertEquals( "create (_" + n0.Id + ")" + lineSeparator() + "create (_" + n1.Id + " {`name`:\"Node1\"})" + lineSeparator() + "create (_" + n0.Id + ")-[:`REL` {`related`:true}]->(_" + n1.Id + ")" + lineSeparator() + ";" + lineSeparator(), DoExportGraph(graph) );
		 }
	}

}