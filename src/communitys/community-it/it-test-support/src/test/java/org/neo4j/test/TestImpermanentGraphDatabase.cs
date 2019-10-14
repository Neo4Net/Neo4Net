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
namespace Neo4Net.Test
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TestImpermanentGraphDatabase
	{
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createDb()
		 public virtual void CreateDb()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).NewImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_keep_data_between_start_and_shutdown()
		 public virtual void ShouldKeepDataBetweenStartAndShutdown()
		 {
			  CreateNode();

			  assertEquals( "Expected one new node", 1, NodeCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void data_should_not_survive_shutdown()
		 public virtual void DataShouldNotSurviveShutdown()
		 {
			  CreateNode();
			  _db.shutdown();

			  CreateDb();

			  assertEquals( "Should not see anything.", 0, NodeCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_remove_all_data()
		 public virtual void ShouldRemoveAllData()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					RelationshipType relationshipType = RelationshipType.withName( "R" );

					Node n1 = _db.createNode();
					Node n2 = _db.createNode();
					Node n3 = _db.createNode();

					n1.CreateRelationshipTo( n2, relationshipType );
					n2.CreateRelationshipTo( n1, relationshipType );
					n3.CreateRelationshipTo( n1, relationshipType );

					tx.Success();
			  }

			  CleanDatabaseContent( _db );

			  assertThat( NodeCount(), @is(0L) );
		 }

		 private void CleanDatabaseContent( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.AllRelationships.forEach( Relationship.delete );
					Db.AllNodes.forEach( Node.delete );
					tx.Success();
			  }
		 }

		 private long NodeCount()
		 {
			  Transaction transaction = _db.beginTx();
			  long count = Iterables.count( _db.AllNodes );
			  transaction.Close();
			  return count;
		 }

		 private void CreateNode()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.createNode();
					tx.Success();
			  }
		 }
	}

}