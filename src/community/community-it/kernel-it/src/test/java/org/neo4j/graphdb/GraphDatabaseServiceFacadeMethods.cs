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
namespace Neo4Net.GraphDb
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.FacadeMethod.consume;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

	/// <summary>
	/// Test convenience: all the methods on IGraphDatabaseService, callable using generic interface
	/// </summary>
	public sealed class IGraphDatabaseServiceFacadeMethods : Consumer<GraphDatabaseService>
	{
		 public static readonly IGraphDatabaseServiceFacadeMethods CreateNode = new IGraphDatabaseServiceFacadeMethods( "CreateNode", InnerEnum.CreateNode, new FacadeMethod<>( "Node createNode()", IGraphDatabaseService::createNode ) );
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
		 public static readonly IGraphDatabaseServiceFacadeMethods GetAllLabels = new IGraphDatabaseServiceFacadeMethods( "GetAllLabels", InnerEnum.GetAllLabels, new FacadeMethod<>( "Iterable<Label> getAllLabels()", IGraphDatabaseService::getAllLabels ) );
		 public static readonly IGraphDatabaseServiceFacadeMethods GetAllLabelsInUse = new IGraphDatabaseServiceFacadeMethods( "GetAllLabelsInUse", InnerEnum.GetAllLabelsInUse, new FacadeMethod<>( "Iterable<Label> getAllLabelsInUse()", IGraphDatabaseService::getAllLabelsInUse ) );
		 public static readonly IGraphDatabaseServiceFacadeMethods GetAllRelationshipTypes = new IGraphDatabaseServiceFacadeMethods( "GetAllRelationshipTypes", InnerEnum.GetAllRelationshipTypes, new FacadeMethod<>( "Iterable<RelationshipType> getAllRelationshipTypes()", IGraphDatabaseService::getAllRelationshipTypes ) );
		 public static readonly IGraphDatabaseServiceFacadeMethods GetAllRelationshipTypesInUse = new IGraphDatabaseServiceFacadeMethods( "GetAllRelationshipTypesInUse", InnerEnum.GetAllRelationshipTypesInUse, new FacadeMethod<>( "Iterable<RelationshipType> getAllRelationshipTypesInUse()", IGraphDatabaseService::getAllRelationshipTypesInUse ) );
		 public static readonly IGraphDatabaseServiceFacadeMethods GetAllPropertyKeys = new IGraphDatabaseServiceFacadeMethods( "GetAllPropertyKeys", InnerEnum.GetAllPropertyKeys, new FacadeMethod<>( "Iterable<String> getAllPropertyKeys()", IGraphDatabaseService::getAllPropertyKeys ) );
		 public static readonly IGraphDatabaseServiceFacadeMethods Schema = new IGraphDatabaseServiceFacadeMethods( "Schema", InnerEnum.Schema, new FacadeMethod<>( "Schema schema()", IGraphDatabaseService::schema ) );

		 private static readonly IList<GraphDatabaseServiceFacadeMethods> valueList = new List<GraphDatabaseServiceFacadeMethods>();

		 static IGraphDatabaseServiceFacadeMethods()
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

		 internal IGraphDatabaseServiceFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<GraphDatabaseService> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( IGraphDatabaseService IGraphDatabaseService )
		 {
			  _facadeMethod.accept( IGraphDatabaseService );
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

		public static IGraphDatabaseServiceFacadeMethods ValueOf( string name )
		{
			foreach ( IGraphDatabaseServiceFacadeMethods enumInstance in IGraphDatabaseServiceFacadeMethods.valueList )
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