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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using DatabaseShutdownException = Neo4Net.Graphdb.DatabaseShutdownException;
	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using NotInTransactionException = Neo4Net.Graphdb.NotInTransactionException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;

	public class TestNeo4jApiExceptions
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotInTransactionException()
		 public virtual void TestNotInTransactionException()
		 {
			  Node node1 = _graph.createNode();
			  node1.SetProperty( "test", 1 );
			  Node node2 = _graph.createNode();
			  Node node3 = _graph.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.SetProperty( "test", 11 );
			  Commit();
			  try
			  {
					_graph.createNode();
					fail( "Create node with no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
					fail( "Create relationship with no transaction should " + "throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					node1.SetProperty( "test", 2 );
					fail( "Set property with no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					rel.SetProperty( "test", 22 );
					fail( "Set property with no transaction should throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					node3.Delete();
					fail( "Delete node with no transaction should " + "throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  try
			  {
					rel.Delete();
					fail( "Delete relationship with no transaction should " + "throw exception" );
			  }
			  catch ( NotInTransactionException )
			  { // good
			  }
			  NewTransaction();
			  assertEquals( node1.GetProperty( "test" ), 1 );
			  assertEquals( rel.GetProperty( "test" ), 11 );
			  assertEquals( rel, node1.GetSingleRelationship( MyRelTypes.TEST, Direction.OUTGOING ) );
			  node1.Delete();
			  node2.Delete();
			  rel.Delete();
			  node3.Delete();

			  // Finally
			  Rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotFoundException()
		 public virtual void TestNotFoundException()
		 {
			  Node node1 = _graph.createNode();
			  Node node2 = _graph.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  long nodeId = node1.Id;
			  long relId = rel.Id;
			  rel.Delete();
			  node2.Delete();
			  node1.Delete();
			  NewTransaction();
			  try
			  {
					_graph.getNodeById( nodeId );
					fail( "Get node by id on deleted node should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					_graph.getRelationshipById( relId );
					fail( "Get relationship by id on deleted node should " + "throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }

			  // Finally
			  Rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorWhenShutdownKernelApi()
		 public virtual void ShouldGiveNiceErrorWhenShutdownKernelApi()
		 {
			  GraphDatabaseService graphDb = _graph;
			  Node node = graphDb.CreateNode();
			  Commit();
			  graphDb.Shutdown();

			  try
			  {
					count( node.Labels.GetEnumerator() );
					fail( "Did not get a nice exception" );
			  }
			  catch ( DatabaseShutdownException )
			  { // good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorWhenShutdownLegacy()
		 public virtual void ShouldGiveNiceErrorWhenShutdownLegacy()
		 {
			  GraphDatabaseService graphDb = _graph;
			  Node node = graphDb.CreateNode();
			  Commit();
			  graphDb.Shutdown();

			  try
			  {
					node.Relationships;
					fail( "Did not get a nice exception" );
			  }
			  catch ( DatabaseShutdownException )
			  { // good
			  }
			  try
			  {
					graphDb.CreateNode();
					fail( "Create node did not produce expected error" );
			  }
			  catch ( DatabaseShutdownException )
			  { // good
			  }
		 }

		 private Transaction _tx;
		 private GraphDatabaseService _graph;

		 private void NewTransaction()
		 {
			  if ( _tx != null )
			  {
					_tx.success();
					_tx.close();
			  }
			  _tx = _graph.beginTx();
		 }

		 public virtual void Commit()
		 {
			  if ( _tx != null )
			  {
					_tx.success();
					_tx.close();
					_tx = null;
			  }
		 }

		 public virtual void Rollback()
		 {
			  if ( _tx != null )
			  {
					_tx.close();
					_tx = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _graph = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  NewTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanUp()
		 public virtual void CleanUp()
		 {
			  Rollback();
			  _graph.shutdown();
		 }

	}

}