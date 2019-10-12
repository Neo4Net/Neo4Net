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
	using AnyValue = Neo4Net.Values.AnyValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.ALICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.CAROL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.DAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Support.NO_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;

	public class Edges
	{
		 // Relationship types
		 public static readonly TextValue Knows = stringValue( "KNOWS" );
		 public static readonly TextValue Likes = stringValue( "LIKES" );
		 public static readonly TextValue Dislikes = stringValue( "DISLIKES" );
		 public static readonly TextValue MarriedTo = stringValue( "MARRIED_TO" );
		 public static readonly TextValue WorksFor = stringValue( "WORKS_FOR" );

		 // Edges
		 public static readonly RelationshipValue AliceKnowsBob = relationshipValue( 12L, ALICE, BOB, Knows, map( new string[]{ "since" }, new AnyValue[]{ longValue( 1999L ) } ) );
		 public static readonly RelationshipValue AliceLikesCarol = relationshipValue( 13L, ALICE, CAROL, Likes, NO_PROPERTIES );
		 public static readonly RelationshipValue CarolDislikesBob = relationshipValue( 32L, CAROL, BOB, Dislikes, NO_PROPERTIES );
		 public static readonly RelationshipValue CarolMarriedToDave = relationshipValue( 34L, CAROL, DAVE, MarriedTo, NO_PROPERTIES );
		 public static readonly RelationshipValue DaveWorksForDave = relationshipValue( 44L, DAVE, DAVE, WorksFor, NO_PROPERTIES );

		 private Edges()
		 {
		 }
	}

}