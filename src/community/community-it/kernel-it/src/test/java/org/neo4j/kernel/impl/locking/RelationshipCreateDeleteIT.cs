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
namespace Neo4Net.Kernel.impl.locking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Race = Neo4Net.Test.Race;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// Testing on database level that creating and deleting relationships over the the same two nodes
	/// doesn't create unnecessary deadlock scenarios, i.e. that locking order is predictable and symmetrical
	/// 
	/// Also test that relationship chains are consistently read during concurrent updates.
	/// </summary>
	public class RelationshipCreateDeleteIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeadlockOrCrashFromInconsistency() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeadlockOrCrashFromInconsistency()
		 {
			  // GIVEN (A) -[R]-> (B)
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node a;
			  Node a;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node b;
			  Node b;
			  using ( Transaction tx = Db.beginTx() )
			  {
					( a = Db.createNode() ).createRelationshipTo(b = Db.createNode(), MyRelTypes.TEST);
					tx.Success();
			  }

			  // WHEN
			  Race race = new Race();
			  // a bunch of deleters
			  for ( int i = 0; i < 30; i++ )
			  {
					race.AddContestant(() =>
					{
					 for ( int j = 0; j < 10; j++ )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								Node node = Random.nextBoolean() ? a : b;
								foreach ( Relationship relationship in node.Relationships )
								{
									 try
									 {
										  relationship.delete();
									 }
									 catch ( NotFoundException e )
									 {
										  // This is OK and expected since there are multiple threads deleting
										  assertTrue( e.Message.contains( "already deleted" ) );
									 }
								}
								tx.Success();
						  }
					 }
					});
			  }

			  // a bunch of creators
			  for ( int i = 0; i < 30; i++ )
			  {
					race.AddContestant(() =>
					{
					 for ( int j = 0; j < 10; j++ )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								bool order = Random.nextBoolean();
								Node start = order ? a : b;
								Node end = order ? b : a;
								start.createRelationshipTo( end, MyRelTypes.Test );
								tx.Success();
						  }
					 }
					});
			  }

			  // THEN there should be no thread throwing exception, especially DeadlockDetectedException
			  race.Go();
		 }
	}

}