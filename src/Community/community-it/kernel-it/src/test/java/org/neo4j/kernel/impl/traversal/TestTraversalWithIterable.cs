﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.traversal
{
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Evaluators = Neo4Net.Graphdb.traversal.Evaluators;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;
	using Neo4Net.Helpers.Collection;

	public class TestTraversalWithIterable : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traverseWithIterableForStartNodes()
		 public virtual void TraverseWithIterableForStartNodes()
		 {
			  /*
			   * (a)-->(b)-->(c)
			   * (d)-->(e)-->(f)
			   *
			   */

			  CreateGraph( "a TO b", "b TO c", "d TO e", "e TO f" );

			  using ( Transaction tx = BeginTx() )
			  {
					TraversalDescription basicTraverser = GraphDb.traversalDescription().evaluator(Evaluators.atDepth(2));

					ICollection<Node> startNodes = new List<Node>();
					startNodes.Add( GetNodeWithName( "a" ) );
					startNodes.Add( GetNodeWithName( "d" ) );

					IEnumerable<Node> iterableStartNodes = startNodes;

					ExpectPaths( basicTraverser.Traverse( iterableStartNodes ), "a,b,c", "d,e,f" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useTraverserInsideTraverser()
		 public virtual void UseTraverserInsideTraverser()
		 {
			  /*
			   * (a)-->(b)-->(c)
			   *  |
			   * \/
			   * (d)-->(e)-->(f)
			   *
			   */

			  CreateGraph( "a FIRST d", "a TO b", "b TO c", "d TO e", "e TO f" );

			  using ( Transaction tx = BeginTx() )
			  {
					TraversalDescription firstTraverser = GraphDb.traversalDescription().relationships(RelationshipType.withName("FIRST")).evaluator(Evaluators.toDepth(1));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Iterable<org.neo4j.graphdb.Path> firstResult = firstTraverser.traverse(getNodeWithName("a"));
					IEnumerable<Path> firstResult = firstTraverser.Traverse( GetNodeWithName( "a" ) );

					IEnumerable<Node> startNodesForNestedTraversal = new IterableWrapperAnonymousInnerClass( this, firstResult );

					TraversalDescription nestedTraversal = GraphDb.traversalDescription().evaluator(Evaluators.atDepth(2));
					ExpectPaths( nestedTraversal.Traverse( startNodesForNestedTraversal ), "a,b,c", "d,e,f" );
					tx.Success();
			  }
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Node, Path>
		 {
			 private readonly TestTraversalWithIterable _outerInstance;

			 public IterableWrapperAnonymousInnerClass( TestTraversalWithIterable outerInstance, IEnumerable<Path> firstResult ) : base( firstResult )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Node underlyingObjectToObject( Path path )
			 {
				  return path.EndNode();
			 }
		 }

	}

}