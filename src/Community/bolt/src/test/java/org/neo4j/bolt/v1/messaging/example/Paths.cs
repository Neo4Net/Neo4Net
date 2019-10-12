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
namespace Neo4Net.Bolt.v1.messaging.example
{
	using PathValue = Neo4Net.Values.@virtual.PathValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.ALICE_KNOWS_BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.ALICE_LIKES_CAROL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.CAROL_DISLIKES_BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.CAROL_MARRIED_TO_DAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.DAVE_WORKS_FOR_DAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.ALICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.CAROL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.DAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Support.edges;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Support.nodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.path;

	/*
	 * This class contains a number of paths used for testing, all based on
	 * the following graph:
	 * <pre>
	 *
	 *     (Bob)<--[:DISLIKES]---,
	 *       ^                   |
	 *       |                   |
	 *    [:KNOWS]               |
	 *       |                   |
	 *       |                   |
	 *     (Alice)--[:LIKES]-->(Carol)--[:MARRIED_TO]-->(Dave)-------------,
	 *                                                    ^                |
	 *                                                    |                |
	 *                                                    '--[:WORKS_FOR]--'
	 *
	 * </pre>
	*/
	public class Paths
	{
		 // Paths
		 public static readonly PathValue PathWithLengthZero = path( nodes( ALICE ), edges() );

		 public static readonly PathValue PathWithLengthOne = path( nodes( ALICE, BOB ), edges( ALICE_KNOWS_BOB ) );
		 public static readonly PathValue PathWithLengthTwo = path( nodes( ALICE, CAROL, DAVE ), edges( ALICE_LIKES_CAROL, CAROL_MARRIED_TO_DAVE ) );
		 public static readonly PathValue PathWithRelationshipTraversedAgainstItsDirection = path( nodes( ALICE, BOB, CAROL, DAVE ), edges( ALICE_KNOWS_BOB, CAROL_DISLIKES_BOB, CAROL_MARRIED_TO_DAVE ) );
		 public static readonly PathValue PathWithNodesVisitedMultipleTimes = path( nodes( ALICE, BOB, ALICE, CAROL, BOB, CAROL ), edges( ALICE_KNOWS_BOB, ALICE_KNOWS_BOB, ALICE_LIKES_CAROL, CAROL_DISLIKES_BOB, CAROL_DISLIKES_BOB ) );
		 public static readonly PathValue PathWithRelationshipTraversedMultipleTimesInSameDirection = path( nodes( ALICE, CAROL, BOB, ALICE, CAROL, DAVE ), edges( ALICE_LIKES_CAROL, CAROL_DISLIKES_BOB, ALICE_KNOWS_BOB,ALICE_LIKES_CAROL, CAROL_MARRIED_TO_DAVE ) );
		 public static readonly PathValue PathWithLoop = path( nodes( CAROL, DAVE, DAVE ), edges( CAROL_MARRIED_TO_DAVE, DAVE_WORKS_FOR_DAVE ) );

		 public static readonly PathValue[] AllPaths = new PathValue[]{ PathWithLengthZero, PathWithLengthOne, PathWithLengthTwo, PathWithRelationshipTraversedAgainstItsDirection, PathWithNodesVisitedMultipleTimes, PathWithRelationshipTraversedMultipleTimesInSameDirection, PathWithLoop };

		 private Paths()
		 {
		 }
	}

}