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
namespace Neo4Net.Graphalgo.impl.util
{
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class PathImplTest
	{
		 private readonly EmbeddedProxySPI _spi = mock( typeof( EmbeddedProxySPI ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void singularNodeWorksForwardsAndBackwards()
		 internal virtual void SingularNodeWorksForwardsAndBackwards()
		 {
			  Node node = CreateNode( 1337L );
			  Path path = PathImpl.Singular( node );

			  assertEquals( node, path.StartNode() );
			  assertEquals( node, path.EndNode() );

			  IEnumerator<Node> forwardIterator = path.Nodes().GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( forwardIterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( node, forwardIterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( forwardIterator.hasNext() );

			  IEnumerator<Node> reverseIterator = path.ReverseNodes().GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( reverseIterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( node, reverseIterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( reverseIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathsWithTheSameContentsShouldBeEqual()
		 internal virtual void PathsWithTheSameContentsShouldBeEqual()
		 {
			  Node node = CreateNode( 1337L );
			  Relationship relationship = CreateRelationship( 1337L, 7331L );

			  // Given
			  Path firstPath = ( new PathImpl.Builder( node ) ).Push( relationship ).build();
			  Path secondPath = ( new PathImpl.Builder( node ) ).Push( relationship ).build();

			  // When Then
			  assertEquals( firstPath, secondPath );
			  assertEquals( secondPath, firstPath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathsWithDifferentLengthAreNotEqual()
		 internal virtual void PathsWithDifferentLengthAreNotEqual()
		 {
			  Node node = CreateNode( 1337L );
			  Relationship relationship = CreateRelationship( 1337L, 7331L );

			  // Given
			  Path firstPath = ( new PathImpl.Builder( node ) ).Push( relationship ).build();
			  Path secondPath = ( new PathImpl.Builder( node ) ).Push( relationship ).push( CreateRelationship( 1337L, 7331L ) ).build();

			  // When Then
			  assertThat( firstPath, not( equalTo( secondPath ) ) );
			  assertThat( secondPath, not( equalTo( firstPath ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPathReverseNodes()
		 internal virtual void TestPathReverseNodes()
		 {
			  when( _spi.newNodeProxy( Mockito.anyLong() ) ).thenAnswer(new NodeProxyAnswer(this));

			  Path path = ( new PathImpl.Builder( CreateNodeProxy( 1 ) ) ).push( CreateRelationshipProxy( 1, 2 ) ).push( CreateRelationshipProxy( 2, 3 ) ).build( new PathImpl.Builder( CreateNodeProxy( 3 ) ) );

			  IEnumerable<Node> nodes = path.ReverseNodes();
			  IList<Node> nodeList = Iterables.asList( nodes );

			  assertEquals( 3, nodeList.Count );
			  assertEquals( 3, nodeList[0].Id );
			  assertEquals( 2, nodeList[1].Id );
			  assertEquals( 1, nodeList[2].Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPathNodes()
		 internal virtual void TestPathNodes()
		 {
			  when( _spi.newNodeProxy( Mockito.anyLong() ) ).thenAnswer(new NodeProxyAnswer(this));

			  Path path = ( new PathImpl.Builder( CreateNodeProxy( 1 ) ) ).push( CreateRelationshipProxy( 1, 2 ) ).push( CreateRelationshipProxy( 2, 3 ) ).build( new PathImpl.Builder( CreateNodeProxy( 3 ) ) );

			  IEnumerable<Node> nodes = path.Nodes();
			  IList<Node> nodeList = Iterables.asList( nodes );

			  assertEquals( 3, nodeList.Count );
			  assertEquals( 1, nodeList[0].Id );
			  assertEquals( 2, nodeList[1].Id );
			  assertEquals( 3, nodeList[2].Id );
		 }

		 private RelationshipProxy CreateRelationshipProxy( int startNodeId, int endNodeId )
		 {
			  return new RelationshipProxy( _spi, 1L, startNodeId, 1, endNodeId );
		 }

		 private NodeProxy CreateNodeProxy( int nodeId )
		 {
			  return new NodeProxy( _spi, nodeId );
		 }

		 private static Node CreateNode( long nodeId )
		 {
			  Node node = mock( typeof( Node ) );
			  when( node.Id ).thenReturn( nodeId );
			  return node;
		 }

		 private static Relationship CreateRelationship( long startNodeId, long endNodeId )
		 {
			  Relationship relationship = mock( typeof( Relationship ) );
			  Node startNode = CreateNode( startNodeId );
			  Node endNode = CreateNode( endNodeId );
			  when( relationship.StartNode ).thenReturn( startNode );
			  when( relationship.EndNode ).thenReturn( endNode );
			  return relationship;
		 }

		 private class NodeProxyAnswer : Answer<NodeProxy>
		 {
			 private readonly PathImplTest _outerInstance;

			 public NodeProxyAnswer( PathImplTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override NodeProxy Answer( InvocationOnMock invocation )
			  {
					return outerInstance.createNodeProxy( ( ( Number ) invocation.getArgument( 0 ) ).intValue() );
			  }
		 }
	}

}