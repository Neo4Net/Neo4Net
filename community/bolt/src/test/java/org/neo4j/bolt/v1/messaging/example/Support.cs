﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.v1.messaging.example
{

	using TextArray = Org.Neo4j.Values.Storable.TextArray;
	using Values = Org.Neo4j.Values.Storable.Values;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

	public class Support
	{
		 internal static readonly TextArray NoLabels = Values.stringArray();
		 internal static readonly MapValue NoProperties = VirtualValues.EMPTY_MAP;

		 private Support()
		 {
		 }

		 // Helper to produce literal list of nodes
		 public static NodeValue[] Nodes( params NodeValue[] nodes )
		 {
			  return nodes;
		 }

		 // Helper to extract list of nodes from a path
		 public static IList<NodeValue> Nodes( PathValue path )
		 {
			  return Arrays.asList( path.Nodes() );
		 }

		 // Helper to produce literal list of relationships
		 public static RelationshipValue[] Edges( params RelationshipValue[] edgeValues )
		 {
			  return edgeValues;
		 }
	}

}