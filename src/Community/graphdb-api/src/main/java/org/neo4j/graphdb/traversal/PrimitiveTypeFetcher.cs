using System.Collections.Generic;

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
namespace Neo4Net.Graphdb.traversal
{


	/// <summary>
	/// Used with uniqueness filters for simplifying node and relationship uniqueness evaluation.
	/// </summary>
	internal abstract class PrimitiveTypeFetcher
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE { long getId(org.neo4j.graphdb.Path source) { return source.endNode().getId(); } boolean idEquals(org.neo4j.graphdb.Path source, long idToCompare) { return getId(source) == idToCompare; } boolean containsDuplicates(org.neo4j.graphdb.Path source) { java.util.Set<org.neo4j.graphdb.Node> nodes = new java.util.HashSet<>(); for(org.neo4j.graphdb.Node node : source.reverseNodes()) { if(!nodes.add(node)) { return true; } } return false; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP { long getId(org.neo4j.graphdb.Path source) { return source.lastRelationship().getId(); } boolean idEquals(org.neo4j.graphdb.Path source, long idToCompare) { org.neo4j.graphdb.Relationship relationship = source.lastRelationship(); return relationship != null && relationship.getId() == idToCompare; } boolean containsDuplicates(org.neo4j.graphdb.Path source) { java.util.Set<org.neo4j.graphdb.Relationship> relationships = new java.util.HashSet<>(); for(org.neo4j.graphdb.Relationship relationship : source.reverseRelationships()) { if(!relationships.add(relationship)) { return true; } } return false; } };

		 private static readonly IList<PrimitiveTypeFetcher> valueList = new List<PrimitiveTypeFetcher>();

		 static PrimitiveTypeFetcher()
		 {
			 valueList.Add( NODE );
			 valueList.Add( RELATIONSHIP );
		 }

		 public enum InnerEnum
		 {
			 NODE,
			 RELATIONSHIP
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private PrimitiveTypeFetcher( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal abstract long getId( Neo4Net.Graphdb.Path path );

		 internal abstract bool idEquals( Neo4Net.Graphdb.Path path, long idToCompare );

		 internal abstract bool containsDuplicates( Neo4Net.Graphdb.Path path );

		public static IList<PrimitiveTypeFetcher> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static PrimitiveTypeFetcher valueOf( string name )
		{
			foreach ( PrimitiveTypeFetcher enumInstance in PrimitiveTypeFetcher.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}