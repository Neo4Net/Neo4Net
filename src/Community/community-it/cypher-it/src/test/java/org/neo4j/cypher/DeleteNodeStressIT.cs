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
namespace Neo4Net.Cypher
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class DeleteNodeStressIT
	{
		 private readonly ExecutorService _executorService = Executors.newFixedThreadPool( 10 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{

						 for ( int j = 0; j < 100; j++ )
						 {
							  Node node = Db.createNode( label( "L" ) );
							  node.SetProperty( "prop", i + j );
						 }
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executorService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnNodesWhileDeletingNode() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReturnNodesWhileDeletingNode()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (n:L {prop:42}) OPTIONAL MATCH (m:L {prop:1337}) WITH n MATCH (n) return n" );
			  Future query2 = ExecuteInThread( "MATCH (n:L {prop:42}) DELETE n" );

			  // Then
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCheckPropertiesWhileDeletingNode() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCheckPropertiesWhileDeletingNode()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (n:L {prop:42}) OPTIONAL MATCH (m:L {prop:1337}) WITH n MATCH (n) RETURN exists(n.prop)" );
			  Future query2 = ExecuteInThread( "MATCH (n:L {prop:42}) DELETE n" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemovePropertiesWhileDeletingNode() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemovePropertiesWhileDeletingNode()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (n:L {prop:42}) OPTIONAL MATCH (m:L {prop:1337}) WITH n MATCH (n) REMOVE n.prop" );
			  Future query2 = ExecuteInThread( "MATCH (n:L {prop:42}) DELETE n" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSetPropertiesWhileDeletingNode() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSetPropertiesWhileDeletingNode()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (n:L {prop:42}) OPTIONAL MATCH (m:L {prop:1337}) WITH n MATCH (n) SET n.foo = 'bar'" );
			  Future query2 = ExecuteInThread( "MATCH (n:L {prop:42}) DELETE n" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCheckLabelsWhileDeleting() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCheckLabelsWhileDeleting()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (n:L {prop:42}) OPTIONAL MATCH (m:L {prop:1337}) WITH n RETURN labels(n)" );
			  Future query2 = ExecuteInThread( "MATCH (n:L {prop:42}) DELETE n" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.concurrent.Future executeInThread(final String query)
		 private Future ExecuteInThread( string query )
		 {
			  return _executorService.submit( () => Db.execute(query).resultAsString() );
		 }
	}

}