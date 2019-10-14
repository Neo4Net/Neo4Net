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
namespace Neo4Net.Kernel.impl.traversal
{
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.atDepth;

	public class TestMultipleStartNodes : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void myFriendsAsWellAsYourFriends()
		 public virtual void MyFriendsAsWellAsYourFriends()
		 {
			  /*
			   * Hey, this looks like a futuristic gun or something
			   *
			   *  (f8)     _----(f1)--(f5)
			   *   |      /      /
			   * (f7)--(you)--(me)--(f2)--(f6)
			   *         |   /   \
			   *         (f4)    (f3)
			   */

			  CreateGraph( "you KNOW me", "you KNOW f1", "you KNOW f4", "me KNOW f1", "me KNOW f4", "me KNOW f2", "me KNOW f3", "f1 KNOW f5", "f2 KNOW f6", "you KNOW f7", "f7 KNOW f8" );

			  using ( Transaction tx = BeginTx() )
			  {
					RelationshipType knowRelType = withName( "KNOW" );
					Node you = GetNodeWithName( "you" );
					Node me = GetNodeWithName( "me" );

					string[] levelOneFriends = new string[]{ "f1", "f2", "f3", "f4", "f7" };
					TraversalDescription levelOneTraversal = GraphDb.traversalDescription().relationships(knowRelType).evaluator(atDepth(1));
					ExpectNodes( levelOneTraversal.DepthFirst().traverse(you, me), levelOneFriends );
					ExpectNodes( levelOneTraversal.BreadthFirst().traverse(you, me), levelOneFriends );

					string[] levelTwoFriends = new string[]{ "f5", "f6", "f8" };
					TraversalDescription levelTwoTraversal = GraphDb.traversalDescription().relationships(knowRelType).evaluator(atDepth(2));
					ExpectNodes( levelTwoTraversal.DepthFirst().traverse(you, me), levelTwoFriends );
					ExpectNodes( levelTwoTraversal.BreadthFirst().traverse(you, me), levelTwoFriends );
			  }
		 }
	}

}