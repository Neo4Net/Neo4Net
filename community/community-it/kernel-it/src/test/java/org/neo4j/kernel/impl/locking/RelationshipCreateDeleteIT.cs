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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Race = Org.Neo4j.Test.Race;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeadlockOrCrashFromInconsistency() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeadlockOrCrashFromInconsistency()
		 {
			  // GIVEN (A) -[R]-> (B)
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a;
			  Node a;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node b;
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