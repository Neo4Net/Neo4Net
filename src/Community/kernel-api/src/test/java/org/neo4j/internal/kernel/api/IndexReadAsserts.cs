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
namespace Neo4Net.@internal.Kernel.Api
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class IndexReadAsserts
	{
		 internal static void AssertNodes( NodeIndexCursor node, MutableLongSet uniqueIds, params long[] expected )
		 {
			  uniqueIds.clear();
			  foreach ( long count in expected )
			  {
					assertTrue( "at least " + expected.Length + " nodes", node.Next() );
					assertTrue( uniqueIds.add( node.NodeReference() ) );
			  }
			  assertFalse( "no more than " + expected.Length + " nodes", node.Next() );
			  assertEquals( "all nodes are unique", expected.Length, uniqueIds.size() );
			  foreach ( long expectedNode in expected )
			  {
					assertTrue( "expected node " + expectedNode, uniqueIds.contains( expectedNode ) );
			  }
		 }

		 internal static void AssertNodeCount( NodeIndexCursor node, int expectedCount, MutableLongSet uniqueIds )
		 {
			  uniqueIds.clear();
			  for ( int i = 0; i < expectedCount; i++ )
			  {
					assertTrue( "at least " + expectedCount + " nodes", node.Next() );
					assertTrue( uniqueIds.add( node.NodeReference() ) );
			  }
			  assertFalse( "no more than " + expectedCount + " nodes", node.Next() );
		 }

		 internal static void AssertFoundRelationships( RelationshipIndexCursor edge, int edges, MutableLongSet uniqueIds )
		 {
			  for ( int i = 0; i < edges; i++ )
			  {
					assertTrue( "at least " + edges + " relationships", edge.Next() );
					assertTrue( uniqueIds.add( edge.RelationshipReference() ) );
			  }
			  assertFalse( "no more than " + edges + " relationships", edge.Next() );
		 }
	}

}