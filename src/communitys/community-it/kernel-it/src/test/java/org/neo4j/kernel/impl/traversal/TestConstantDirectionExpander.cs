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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using PathExpanders = Neo4Net.Graphdb.PathExpanders;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;

	public class TestConstantDirectionExpander : TraversalTestBase
	{
		 private enum Types
		 {
			  A,
			  B
		 }

		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createGraph()
		 public virtual void CreateGraph()
		 {
			  /*
			   *   (l)--[A]-->(m)--[A]-->(n)<--[A]--(o)<--[B]--(p)<--[B]--(q)
			   */
			  CreateGraph( "l A m", "m A n", "o A n", "p B o", "q B p" );
			  _tx = BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathWithConstantDirection()
		 public virtual void PathWithConstantDirection()
		 {
			  Node l = GetNodeWithName( "l" );
			  ExpectPaths( GraphDb.traversalDescription().expand(PathExpanders.forConstantDirectionWithTypes(Types.A)).traverse(l), "l", "l,m", "l,m,n" );

			  Node n = GetNodeWithName( "n" );
			  ExpectPaths( GraphDb.traversalDescription().expand(PathExpanders.forConstantDirectionWithTypes(Types.A)).traverse(n), "n", "n,m", "n,m,l", "n,o" );

			  Node q = GetNodeWithName( "q" );
			  ExpectPaths( GraphDb.traversalDescription().expand(PathExpanders.forConstantDirectionWithTypes(Types.B)).traverse(q), "q", "q,p", "q,p,o" );

		 }
	}

}