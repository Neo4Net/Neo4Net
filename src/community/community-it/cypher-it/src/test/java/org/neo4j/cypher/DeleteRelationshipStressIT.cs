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
namespace Neo4Net.Cypher
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class DeleteRelationshipStressIT
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

						 Node prev = null;
						 for ( int j = 0; j < 100; j++ )
						 {
							  Node node = Db.createNode( label( "L" ) );

							  if ( prev != null )
							  {
									Relationship rel = prev.CreateRelationshipTo( node, RelationshipType.withName( "T" ) );
									rel.SetProperty( "prop", i + j );
							  }
							  prev = node;
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
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnRelsWhileDeletingRelationship() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReturnRelsWhileDeletingRelationship()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) OPTIONAL MATCH (:L)-[:T {prop:1337}]-(:L) WITH r MATCH ()-[r]-() return r" );
			  Future query2 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) DELETE r" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPropertyWhileDeletingRelationship() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetPropertyWhileDeletingRelationship()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) OPTIONAL MATCH (:L)-[:T {prop:1337}]-(:L) WITH r MATCH ()-[r]-() return r.prop" );
			  Future query2 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) DELETE r" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCheckPropertiesWhileDeletingRelationship() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCheckPropertiesWhileDeletingRelationship()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) OPTIONAL MATCH (:L)-[:T {prop:1337}]-(:L) WITH r MATCH ()-[r]-() return exists(r.prop)" );
			  Future query2 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) DELETE r" );

			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemovePropertiesWhileDeletingRelationship() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemovePropertiesWhileDeletingRelationship()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) OPTIONAL MATCH (:L)-[:T {prop:1337}]-(:L) WITH r MATCH ()-[r]-() REMOVE r.prop" );
			  Future query2 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) DELETE r" );

			  // When
			  query1.get();
			  query2.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSetPropertiesWhileDeletingRelationship() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSetPropertiesWhileDeletingRelationship()
		 {
			  // Given
			  Future query1 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) OPTIONAL MATCH (:L)-[:T {prop:1337}]-(:L) WITH r MATCH ()-[r]-() SET r.foo = 'bar'" );
			  Future query2 = ExecuteInThread( "MATCH (:L)-[r:T {prop:42}]-(:L) DELETE r" );

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