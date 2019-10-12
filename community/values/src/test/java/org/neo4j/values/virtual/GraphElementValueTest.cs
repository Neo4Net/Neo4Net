using System;

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
namespace Org.Neo4j.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertNotEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.nodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.rel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.relationships;

	internal class GraphElementValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodeShouldEqualItself()
		 internal virtual void NodeShouldEqualItself()
		 {
			  assertEqual( node( 1L ), node( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodeShouldNotEqualOtherNode()
		 internal virtual void NodeShouldNotEqualOtherNode()
		 {
			  assertNotEqual( node( 1L ), node( 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void edgeShouldEqualItself()
		 internal virtual void EdgeShouldEqualItself()
		 {
			  assertEqual( rel( 1L, 1L, 2L ), rel( 1L,1L, 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void edgeShouldNotEqualOtherEdge()
		 internal virtual void EdgeShouldNotEqualOtherEdge()
		 {
			  assertNotEqual( rel( 1L, 1L, 2L ), rel( 2L, 1L, 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldEqualItself()
		 internal virtual void PathShouldEqualItself()
		 {
			  assertEqual( path( node( 1L ) ), path( node( 1L ) ) );
			  assertEqual( path( node( 1L ), rel( 2L, 1L, 3L ), node( 3L ) ), path( node( 1L ), rel( 2L, 1L, 3L ), node( 3L ) ) );

			  assertEqual( path( node( 1L ), rel( 2L, 1L, 3L ), node( 2L ), rel( 3L, 2L, 1L ), node( 1L ) ), path( node( 1L ), rel( 2L, 1L, 3L ), node( 2L ), rel( 3L, 2L, 1L ), node( 1L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldNotEqualOtherPath()
		 internal virtual void PathShouldNotEqualOtherPath()
		 {
			  assertNotEqual( path( node( 1L ) ), path( node( 2L ) ) );
			  assertNotEqual( path( node( 1L ) ), path( node( 1L ), rel( 1L, 1L, 2L ), node( 2L ) ) );
			  assertNotEqual( path( node( 1L ) ), path( node( 2L ), rel( 1L, 2L, 1L ), node( 1L ) ) );

			  assertNotEqual( path( node( 1L ), rel( 2L, 1L, 3L ), node( 3L ) ), path( node( 1L ), rel( 3L, 1L, 3L ), node( 3L ) ) );

			  assertNotEqual( path( node( 1L ), rel( 2L, 1L, 2L ), node( 2L ) ), path( node( 1L ), rel( 2L, 1L, 3L ), node( 3L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldNotOnlyContainRelationship()
		 internal virtual void PathShouldNotOnlyContainRelationship()
		 {
			  assertThrows( typeof( AssertionError ), () => VirtualValues.Path(nodes(), relationships(1L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldContainOneMoreNodeThenEdges()
		 internal virtual void PathShouldContainOneMoreNodeThenEdges()
		 {
			  assertThrows( typeof( Exception ), () => VirtualValues.Path(nodes(1L, 2L), relationships()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldHandleNulls()
		 internal virtual void PathShouldHandleNulls()
		 {
			  assertThrows( typeof( AssertionError ), () => VirtualValues.Path(null, null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldHandleNullEdge()
		 internal virtual void PathShouldHandleNullEdge()
		 {
			  assertThrows( typeof( AssertionError ), () => VirtualValues.Path(nodes(1L), null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathShouldHandleNullNodes()
		 internal virtual void PathShouldHandleNullNodes()
		 {
			  assertThrows( typeof( AssertionError ), () => VirtualValues.Path(null, relationships(1L)) );
		 }
	}

}