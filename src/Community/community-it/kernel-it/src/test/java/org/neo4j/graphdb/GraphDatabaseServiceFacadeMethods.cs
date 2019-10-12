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
namespace Neo4Net.Graphdb
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.FacadeMethod.consume;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	/// <summary>
	/// Test convenience: all the methods on GraphDatabaseService, callable using generic interface
	/// </summary>
	public sealed class GraphDatabaseServiceFacadeMethods : Consumer<GraphDatabaseService>
	{
		 public static readonly GraphDatabaseServiceFacadeMethods CreateNode = new GraphDatabaseServiceFacadeMethods( "CreateNode", InnerEnum.CreateNode, new FacadeMethod<>( "Node createNode()", GraphDatabaseService::createNode ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CREATE_NODE_WITH_LABELS(new FacadeMethod<>("Node createNode( Label... labels )", gds -> gds.createNode(label("FOO")))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_NODE_BY_ID(new FacadeMethod<>("Node getNodeById( long id )", gds -> gds.getNodeById(42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIP_BY_ID(new FacadeMethod<>("Relationship getRelationshipById( long id )", gds -> gds.getRelationshipById(42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_ALL_NODES(new FacadeMethod<>("Iterable<Node> getAllNodes()", gds -> consume(gds.getAllNodes()))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FIND_NODES_BY_LABEL_AND_PROPERTY_DEPRECATED(new FacadeMethod<>("ResourceIterator<Node> findNodeByLabelAndProperty( Label label, String key, Object value )", gds -> consume(gds.findNodes(label("bar"), "baz", 23)))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FIND_NODES_BY_LABEL(new FacadeMethod<>("ResourceIterator<Node> findNodes( Label label )", gds -> consume(gds.findNodes(label("bar"))))),
		 public static readonly GraphDatabaseServiceFacadeMethods GetAllLabels = new GraphDatabaseServiceFacadeMethods( "GetAllLabels", InnerEnum.GetAllLabels, new FacadeMethod<>( "Iterable<Label> getAllLabels()", GraphDatabaseService::getAllLabels ) );
		 public static readonly GraphDatabaseServiceFacadeMethods GetAllLabelsInUse = new GraphDatabaseServiceFacadeMethods( "GetAllLabelsInUse", InnerEnum.GetAllLabelsInUse, new FacadeMethod<>( "Iterable<Label> getAllLabelsInUse()", GraphDatabaseService::getAllLabelsInUse ) );
		 public static readonly GraphDatabaseServiceFacadeMethods GetAllRelationshipTypes = new GraphDatabaseServiceFacadeMethods( "GetAllRelationshipTypes", InnerEnum.GetAllRelationshipTypes, new FacadeMethod<>( "Iterable<RelationshipType> getAllRelationshipTypes()", GraphDatabaseService::getAllRelationshipTypes ) );
		 public static readonly GraphDatabaseServiceFacadeMethods GetAllRelationshipTypesInUse = new GraphDatabaseServiceFacadeMethods( "GetAllRelationshipTypesInUse", InnerEnum.GetAllRelationshipTypesInUse, new FacadeMethod<>( "Iterable<RelationshipType> getAllRelationshipTypesInUse()", GraphDatabaseService::getAllRelationshipTypesInUse ) );
		 public static readonly GraphDatabaseServiceFacadeMethods GetAllPropertyKeys = new GraphDatabaseServiceFacadeMethods( "GetAllPropertyKeys", InnerEnum.GetAllPropertyKeys, new FacadeMethod<>( "Iterable<String> getAllPropertyKeys()", GraphDatabaseService::getAllPropertyKeys ) );
		 public static readonly GraphDatabaseServiceFacadeMethods Schema = new GraphDatabaseServiceFacadeMethods( "Schema", InnerEnum.Schema, new FacadeMethod<>( "Schema schema()", GraphDatabaseService::schema ) );

		 private static readonly IList<GraphDatabaseServiceFacadeMethods> valueList = new List<GraphDatabaseServiceFacadeMethods>();

		 static GraphDatabaseServiceFacadeMethods()
		 {
			 valueList.Add( CreateNode );
			 valueList.Add( CREATE_NODE_WITH_LABELS );
			 valueList.Add( GET_NODE_BY_ID );
			 valueList.Add( GET_RELATIONSHIP_BY_ID );
			 valueList.Add( GET_ALL_NODES );
			 valueList.Add( FIND_NODES_BY_LABEL_AND_PROPERTY_DEPRECATED );
			 valueList.Add( FIND_NODES_BY_LABEL );
			 valueList.Add( GetAllLabels );
			 valueList.Add( GetAllLabelsInUse );
			 valueList.Add( GetAllRelationshipTypes );
			 valueList.Add( GetAllRelationshipTypesInUse );
			 valueList.Add( GetAllPropertyKeys );
			 valueList.Add( Schema );
		 }

		 public enum InnerEnum
		 {
			 CreateNode,
			 CREATE_NODE_WITH_LABELS,
			 GET_NODE_BY_ID,
			 GET_RELATIONSHIP_BY_ID,
			 GET_ALL_NODES,
			 FIND_NODES_BY_LABEL_AND_PROPERTY_DEPRECATED,
			 FIND_NODES_BY_LABEL,
			 GetAllLabels,
			 GetAllLabelsInUse,
			 GetAllRelationshipTypes,
			 GetAllRelationshipTypesInUse,
			 GetAllPropertyKeys,
			 Schema
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal GraphDatabaseServiceFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<GraphDatabaseService> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( GraphDatabaseService graphDatabaseService )
		 {
			  _facadeMethod.accept( graphDatabaseService );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<GraphDatabaseServiceFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static GraphDatabaseServiceFacadeMethods valueOf( string name )
		{
			foreach ( GraphDatabaseServiceFacadeMethods enumInstance in GraphDatabaseServiceFacadeMethods.valueList )
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