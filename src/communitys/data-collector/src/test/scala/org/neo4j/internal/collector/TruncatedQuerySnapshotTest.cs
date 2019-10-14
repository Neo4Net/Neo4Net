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
namespace Neo4Net.@internal.Collector
{
	using Test = org.junit.jupiter.api.Test;

	using AnyValue = Neo4Net.Values.AnyValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeReference = Neo4Net.Values.@virtual.NodeReference;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipReference = Neo4Net.Values.@virtual.RelationshipReference;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TruncatedQuerySnapshotTest
	{
		 internal static NodeValue Node = VirtualValues.nodeValue( 42, Values.stringArray( "Phone" ), Map( "number", Values.stringValue( "07303725xx" ) ) );

		 internal static RelationshipValue Relationship = VirtualValues.relationshipValue( 100, Node, Node, Values.stringValue( "CALL" ), Map( "duration", Values.stringValue( "3 hours" ) ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTruncateNode()
		 internal virtual void ShouldTruncateNode()
		 {
			  // when
			  TruncatedQuerySnapshot x = new TruncatedQuerySnapshot( "", null, Map( "n", Node ), -1L, -1L, -1L, 100 );

			  // then
			  AnyValue truncatedNode = x.QueryParameters.get( "n" );
			  assertTrue( truncatedNode is NodeReference );
			  assertEquals( Node.id(), ((NodeReference)truncatedNode).id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTruncateRelationship()
		 internal virtual void ShouldTruncateRelationship()
		 {
			  // when
			  TruncatedQuerySnapshot x = new TruncatedQuerySnapshot( "", null, Map( "r", Relationship ), -1L, -1L, -1L, 100 );

			  // then
			  AnyValue truncatedRelationship = x.QueryParameters.get( "r" );
			  assertTrue( truncatedRelationship is RelationshipReference );
			  assertEquals( Relationship.id(), ((RelationshipReference)truncatedRelationship).id() );
		 }

		 private static MapValue Map( string key, AnyValue value )
		 {
			  string[] keys = new string[] { key };
			  AnyValue[] values = new AnyValue[] { value };
			  return VirtualValues.map( keys, values );
		 }
	}

}