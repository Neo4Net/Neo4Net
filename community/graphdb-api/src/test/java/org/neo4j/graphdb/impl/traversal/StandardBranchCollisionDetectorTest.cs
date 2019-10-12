using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb.impl.traversal
{
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Evaluator = Org.Neo4j.Graphdb.traversal.Evaluator;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class StandardBranchCollisionDetectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFilteredPathEvaluation()
		 internal virtual void TestFilteredPathEvaluation()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.PropertyContainer endNode = mock(org.neo4j.graphdb.Node.class);
			  PropertyContainer endNode = mock( typeof( Node ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.PropertyContainer alternativeEndNode = mock(org.neo4j.graphdb.Node.class);
			  PropertyContainer alternativeEndNode = mock( typeof( Node ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node startNode = mock(org.neo4j.graphdb.Node.class);
			  Node startNode = mock( typeof( Node ) );
			  Evaluator evaluator = mock( typeof( Evaluator ) );
			  TraversalBranch branch = mock( typeof( TraversalBranch ) );
			  TraversalBranch alternativeBranch = mock( typeof( TraversalBranch ) );

			  when( branch.GetEnumerator() ).thenAnswer(new IteratorAnswer(endNode));
			  when( alternativeBranch.GetEnumerator() ).thenAnswer(new IteratorAnswer(alternativeEndNode));
			  when( alternativeBranch.StartNode() ).thenReturn(startNode);
			  when( evaluator.Evaluate( Mockito.any( typeof( Path ) ) ) ).thenReturn( Evaluation.INCLUDE_AND_CONTINUE );
			  StandardBranchCollisionDetector collisionDetector = new StandardBranchCollisionDetector( evaluator, path => alternativeEndNode.Equals( path.endNode() ) && startNode.Equals(path.startNode()) );

			  ICollection<Path> incoming = collisionDetector.Evaluate( branch, Direction.INCOMING );
			  ICollection<Path> outgoing = collisionDetector.Evaluate( branch, Direction.OUTGOING );
			  ICollection<Path> alternativeIncoming = collisionDetector.Evaluate( alternativeBranch, Direction.INCOMING );
			  ICollection<Path> alternativeOutgoing = collisionDetector.Evaluate( alternativeBranch, Direction.OUTGOING );

			  assertNull( incoming );
			  assertNull( outgoing );
			  assertNull( alternativeIncoming );
			  assertEquals( 1, alternativeOutgoing.Count );
		 }

		 private class IteratorAnswer : Answer<object>
		 {
			  internal readonly PropertyContainer EndNode;

			  internal IteratorAnswer( PropertyContainer endNode )
			  {
					this.EndNode = endNode;
			  }

			  public override object Answer( InvocationOnMock invocation )
			  {
					return Arrays.asList( EndNode ).GetEnumerator();
			  }
		 }
	}

}