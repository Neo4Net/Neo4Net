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
namespace Neo4Net.Cypher.Internal.runtime
{
	using Value = Neo4Net.Values.Storable.Value;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

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