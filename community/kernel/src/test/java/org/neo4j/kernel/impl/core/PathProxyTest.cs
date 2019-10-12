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
namespace Org.Neo4j.Kernel.impl.core
{

	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Entity = Org.Neo4j.Graphdb.Entity;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class PathProxyTest
	{
		 private EmbeddedProxySPI _proxySPI;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _proxySPI = mock( typeof( EmbeddedProxySPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateThroughNodes()
		 public virtual void ShouldIterateThroughNodes()
		 {
			  // given
			  Path path = new PathProxy( _proxySPI, new long[] { 1, 2, 3 }, new long[] { 100, 200 }, new int[] { 0, ~0 } );

			  IEnumerator<Node> iterator = path.Nodes().GetEnumerator();
			  Node node;

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 1, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 2, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 3, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateThroughNodesInReverse()
		 public virtual void ShouldIterateThroughNodesInReverse()
		 {
			  // given
			  Path path = new PathProxy( _proxySPI, new long[] { 1, 2, 3 }, new long[] { 100, 200 }, new int[] { 0, ~0 } );

			  IEnumerator<Node> iterator = path.ReverseNodes().GetEnumerator();
			  Node node;

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 3, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 2, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( node = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 1, node.Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateThroughRelationships()
		 public virtual void ShouldIterateThroughRelationships()
		 {
			  // given
			  Path path = new PathProxy( _proxySPI, new long[] { 1, 2, 3 }, new long[] { 100, 200 }, new int[] { 0, ~0 } );

			  IEnumerator<Relationship> iterator = path.Relationships().GetEnumerator();
			  Relationship relationship;

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( relationship = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 100, relationship.Id );
			  assertEquals( 1, relationship.StartNodeId );
			  assertEquals( 2, relationship.EndNodeId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( relationship = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 200, relationship.Id );
			  assertEquals( 3, relationship.StartNodeId );
			  assertEquals( 2, relationship.EndNodeId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateThroughRelationshipsInReverse()
		 public virtual void ShouldIterateThroughRelationshipsInReverse()
		 {
			  // given
			  Path path = new PathProxy( _proxySPI, new long[] { 1, 2, 3 }, new long[] { 100, 200 }, new int[] { 0, ~0 } );

			  IEnumerator<Relationship> iterator = path.ReverseRelationships().GetEnumerator();
			  Relationship relationship;

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( relationship = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 200, relationship.Id );
			  assertEquals( 3, relationship.StartNodeId );
			  assertEquals( 2, relationship.EndNodeId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( relationship = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 100, relationship.Id );
			  assertEquals( 1, relationship.StartNodeId );
			  assertEquals( 2, relationship.EndNodeId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateAlternatingNodesAndRelationships()
		 public virtual void ShouldIterateAlternatingNodesAndRelationships()
		 {
			  // given
			  Path path = new PathProxy( _proxySPI, new long[] { 1, 2, 3 }, new long[] { 100, 200 }, new int[] { 0, ~0 } );

			  IEnumerator<PropertyContainer> iterator = path.GetEnumerator();
			  PropertyContainer entity;

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( entity = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 1, ( ( Entity ) entity ).Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( entity = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 100, ( ( Entity ) entity ).Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( entity = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 2, ( ( Entity ) entity ).Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( entity = iterator.next(), instanceOf(typeof(Relationship)) );
			  assertEquals( 200, ( ( Entity ) entity ).Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( entity = iterator.next(), instanceOf(typeof(Node)) );
			  assertEquals( 3, ( ( Entity ) entity ).Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }
	}

}