﻿/*
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
namespace Org.Neo4j.Kernel.impl.traversal
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Evaluators = Org.Neo4j.Graphdb.traversal.Evaluators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.EXCLUDE_AND_CONTINUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.INCLUDE_AND_CONTINUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.INCLUDE_AND_PRUNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.includeWhereEndNodeIs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.lastRelationshipTypeIs;

	public class TestEvaluators : TraversalTestBase
	{
		 private enum Types
		 {
			  A,
			  B,
			  C
		 }

		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createGraph()
		 public virtual void CreateGraph()
		 {
			  /*
			   * (a)--[A]->(b)--[B]-->(c)--[B]-->(d)--[C]-->(e)--[A]-->(j)
			   *   \        |
			   *   [B]     [C]-->(h)--[B]-->(i)--[C]-->(k)
			   *     \
			   *      v
			   *      (f)--[C]-->(g)
			   */

			  CreateGraph( "a A b", "b B c", "c B d", "d C e", "e A j", "b C h", "h B i", "i C k", "a B f", "f C g" );

			  _tx = BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastRelationshipTypeEvaluator()
		 public virtual void LastRelationshipTypeEvaluator()
		 {
			  Node a = GetNodeWithName( "a" );
			  ExpectPaths( GraphDb.traversalDescription().evaluator(lastRelationshipTypeIs(INCLUDE_AND_PRUNE, EXCLUDE_AND_CONTINUE, Types.C)).traverse(a), "a,b,c,d,e", "a,f,g", "a,b,h" );

			  ExpectPaths( GraphDb.traversalDescription().evaluator(lastRelationshipTypeIs(INCLUDE_AND_CONTINUE, EXCLUDE_AND_CONTINUE, Types.C)).traverse(a), "a,b,c,d,e", "a,f,g", "a,b,h", "a,b,h,i,k" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void endNodeIs()
		 public virtual void EndNodeIs()
		 {
			  Node a = GetNodeWithName( "a" );
			  Node c = GetNodeWithName( "c" );
			  Node h = GetNodeWithName( "h" );
			  Node g = GetNodeWithName( "g" );

			  ExpectPaths( GraphDb.traversalDescription().evaluator(includeWhereEndNodeIs(c, h, g)).traverse(a), "a,b,c", "a,b,h", "a,f,g" );
			  ExpectPaths( GraphDb.traversalDescription().evaluator(includeWhereEndNodeIs(g)).traverse(a), "a,f,g" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void depths()
		 public virtual void Depths()
		 {
			  Node a = GetNodeWithName( "a" );
			  ExpectPaths( GraphDb.traversalDescription().evaluator(Evaluators.atDepth(1)).traverse(a), "a,b", "a,f" );
			  ExpectPaths( GraphDb.traversalDescription().evaluator(Evaluators.fromDepth(2)).traverse(a), "a,f,g", "a,b,h", "a,b,h,i", "a,b,h,i,k", "a,b,c", "a,b,c,d", "a,b,c,d,e", "a,b,c,d,e,j" );
			  ExpectPaths( GraphDb.traversalDescription().evaluator(Evaluators.toDepth(2)).traverse(a), "a", "a,b", "a,b,c", "a,b,h", "a,f", "a,f,g" );
		 }
	}

}