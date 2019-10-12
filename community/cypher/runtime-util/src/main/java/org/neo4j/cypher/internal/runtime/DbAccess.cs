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
namespace Org.Neo4j.Cypher.@internal.runtime
{
	using Value = Org.Neo4j.Values.Storable.Value;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;

	/// <summary>
	/// Used to expose db access to expressions
	/// </summary>
	public interface DbAccess : EntityById
	{
		 Value NodeProperty( long node, int property );

		 int[] NodePropertyIds( long node );

		 int PropertyKey( string name );

		 int NodeLabel( string name );

		 int RelationshipType( string name );

		 bool NodeHasProperty( long node, int property );

		 Value RelationshipProperty( long node, int property );

		 int[] RelationshipPropertyIds( long node );

		 bool RelationshipHasProperty( long node, int property );

		 int NodeGetOutgoingDegree( long node );

		 int NodeGetOutgoingDegree( long node, int relationship );

		 int NodeGetIncomingDegree( long node );

		 int NodeGetIncomingDegree( long node, int relationship );

		 int NodeGetTotalDegree( long node );

		 int NodeGetTotalDegree( long node, int relationship );

		 NodeValue RelationshipGetStartNode( RelationshipValue relationship );

		 NodeValue RelationshipGetEndNode( RelationshipValue relationship );

		 ListValue GetLabelsForNode( long id );

		 bool IsLabelSetOnNode( int label, long id );

		 string GetPropertyKeyName( int token );

		 MapValue NodeAsMap( long id );

		 MapValue RelationshipAsMap( long id );

	}

}