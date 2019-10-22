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
//	import static org.Neo4Net.graphdb.RelationshipType.withName;

	public sealed class RelationshipFacadeMethods : Consumer<Relationship>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_PROPERTY(new FacadeMethod<>("boolean hasProperty( String key )", r -> r.hasProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY(new FacadeMethod<>("Object getProperty( String key )", r -> r.getProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY_WITH_DEFAULT(new FacadeMethod<>("Object getProperty( String key, Object defaultValue )", r -> r.getProperty("foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SET_PROPERTY(new FacadeMethod<>("void setProperty( String key, Object value )", r -> r.setProperty("foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_PROPERTY(new FacadeMethod<>("Object removeProperty( String key )", r -> r.removeProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY_KEYS(new FacadeMethod<>("Iterable<String> getPropertyKeys()", r -> consume(r.getPropertyKeys()))),
		 public static readonly RelationshipFacadeMethods Delete = new RelationshipFacadeMethods( "Delete", InnerEnum.Delete, new FacadeMethod<>( "void delete()", Relationship.delete ) );
		 public static readonly RelationshipFacadeMethods GetStartNode = new RelationshipFacadeMethods( "GetStartNode", InnerEnum.GetStartNode, new FacadeMethod<>( "Node getStartNode()", Relationship::getStartNode ) );
		 public static readonly RelationshipFacadeMethods GetEndNode = new RelationshipFacadeMethods( "GetEndNode", InnerEnum.GetEndNode, new FacadeMethod<>( "Node getEndNode()", Relationship::getEndNode ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_OTHER_NODE(new FacadeMethod<>("Node getOtherNode( Node node )", r -> r.getOtherNode(null))),
		 public static readonly RelationshipFacadeMethods GetNodes = new RelationshipFacadeMethods( "GetNodes", InnerEnum.GetNodes, new FacadeMethod<>( "Node[] getNodes()", Relationship::getNodes ) );
		 public static readonly RelationshipFacadeMethods GetType = new RelationshipFacadeMethods( "GetType", InnerEnum.GetType, new FacadeMethod<>( "RelationshipType getType()", Relationship::getType ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IS_TYPE(new FacadeMethod<>("boolean isType( RelationshipType type )", r -> r.isType(withName("foo"))));

		 private static readonly IList<RelationshipFacadeMethods> valueList = new List<RelationshipFacadeMethods>();

		 static RelationshipFacadeMethods()
		 {
			 valueList.Add( HAS_PROPERTY );
			 valueList.Add( GET_PROPERTY );
			 valueList.Add( GET_PROPERTY_WITH_DEFAULT );
			 valueList.Add( SET_PROPERTY );
			 valueList.Add( REMOVE_PROPERTY );
			 valueList.Add( GET_PROPERTY_KEYS );
			 valueList.Add( Delete );
			 valueList.Add( GetStartNode );
			 valueList.Add( GetEndNode );
			 valueList.Add( GET_OTHER_NODE );
			 valueList.Add( GetNodes );
			 valueList.Add( GetType );
			 valueList.Add( IS_TYPE );
		 }

		 public enum InnerEnum
		 {
			 HAS_PROPERTY,
			 GET_PROPERTY,
			 GET_PROPERTY_WITH_DEFAULT,
			 SET_PROPERTY,
			 REMOVE_PROPERTY,
			 GET_PROPERTY_KEYS,
			 Delete,
			 GetStartNode,
			 GetEndNode,
			 GET_OTHER_NODE,
			 GetNodes,
			 GetType,
			 IS_TYPE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly FacadeMethod<Relationship> facadeMethod;

		 internal RelationshipFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Relationship> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Relationship relationship )
		 {
			  _facadeMethod.accept( relationship );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<RelationshipFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static RelationshipFacadeMethods valueOf( string name )
		{
			foreach ( RelationshipFacadeMethods enumInstance in RelationshipFacadeMethods.valueList )
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