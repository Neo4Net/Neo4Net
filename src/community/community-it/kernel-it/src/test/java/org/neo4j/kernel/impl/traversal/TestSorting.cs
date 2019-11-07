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
namespace Neo4Net.Kernel.impl.traversal
{
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluators.excludeStartPosition;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Sorting.endNodeProperty;

	public class TestSorting : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sortFriendsByName()
		 public virtual void SortFriendsByName()
		 {
			  /*
			   *      (Abraham)
			   *          |
			   *         (me)--(George)--(Dan)
			   *          |        |
			   *       (Zack)---(Andreas)
			   *                   |
			   *              (Nicholas)
			   */

			  string me = "me";
			  string abraham = "Abraham";
			  string george = "George";
			  string dan = "Dan";
			  string zack = "Zack";
			  string andreas = "Andreas";
			  string nicholas = "Nicholas";
			  string knows = "KNOWS";
			  CreateGraph( Triplet( me, knows, abraham ), Triplet( me, knows, george ), Triplet( george, knows, dan ), Triplet( me, knows, zack ), Triplet( zack, knows, andreas ), Triplet( george, knows, andreas ), Triplet( andreas, knows, nicholas ) );

			  using ( Transaction tx = BeginTx() )
			  {
					IList<Node> nodes = AsNodes( abraham, george, dan, zack, andreas, nicholas );
					assertEquals( nodes, Iterables.asCollection( GraphDb.traversalDescription().evaluator(excludeStartPosition()).sort(endNodeProperty("name")).traverse(GetNodeWithName(me)).nodes() ) );
					tx.Success();
			  }
		 }

		 private IList<Node> AsNodes( string abraham, string george, string dan, string zack, string andreas, string nicholas )
		 {
			  IList<string> allNames = new IList<string> { abraham, george, dan, zack, andreas, nicholas };
			  allNames.Sort();
			  IList<Node> all = new List<Node>();
			  foreach ( string name in allNames )
			  {
					all.Add( GetNodeWithName( name ) );
			  }
			  return all;
		 }

		 private static string Triplet( string i, string type, string you )
		 {
			  return i + " " + type + " " + you;
		 }
	}

}