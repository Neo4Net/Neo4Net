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
//	import static org.Neo4Net.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.FacadeMethod.BAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.FacadeMethod.FOO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.FacadeMethod.QUUX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.FacadeMethod.consume;

	public sealed class NodeFacadeMethods : Consumer<Node>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_PROPERTY(new FacadeMethod<>("boolean hasProperty( String key )", n -> n.hasProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY(new FacadeMethod<>("Object getProperty( String key )", n -> n.getProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY_WITH_DEFAULT(new FacadeMethod<>("Object getProperty( String key, Object defaultValue )", n -> n.getProperty("foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SET_PROPERTY(new FacadeMethod<>("void setProperty( String key, Object value )", n -> n.setProperty("foo", 42))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_PROPERTY(new FacadeMethod<>("Object removeProperty( String key )", n -> n.removeProperty("foo"))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_PROPERTY_KEYS(new FacadeMethod<>("Iterable<String> getPropertyKeys()", n -> consume(n.getPropertyKeys()))),
		 public static readonly NodeFacadeMethods Delete = new NodeFacadeMethods( "Delete", InnerEnum.Delete, new FacadeMethod<>( "void delete()", Node.delete ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIPS(new FacadeMethod<>("Iterable<Relationship> getRelationships()", n -> consume(n.getRelationships()))),
		 public static readonly NodeFacadeMethods HasRelationship = new NodeFacadeMethods( "HasRelationship", InnerEnum.HasRelationship, new FacadeMethod<>( "boolean hasRelationship()", Node::hasRelationship ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIPS_BY_TYPE(new FacadeMethod<>("Iterable<Relationship> getRelationships( RelationshipType... types )", n -> consume(n.getRelationships(FOO, BAR)))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIPS_BY_DIRECTION_AND_TYPES(new FacadeMethod<>("Iterable<Relationship> getRelationships( Direction direction, RelationshipType... types )", n -> consume(n.getRelationships(BOTH, FOO, BAR)))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_RELATIONSHIP_BY_TYPE(new FacadeMethod<>("boolean hasRelationship( RelationshipType... types )", n -> n.hasRelationship(FOO))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_RELATIONSHIP_BY_DIRECTION_AND_TYPE(new FacadeMethod<>("boolean hasRelationship( Direction direction, RelationshipType... types )", n -> n.hasRelationship(BOTH, FOO))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIPS_BY_DIRECTION(new FacadeMethod<>("Iterable<Relationship> getRelationships( Direction dir )", n -> consume(n.getRelationships(BOTH)))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_RELATIONSHIP_BY_DIRECTION(new FacadeMethod<>("boolean hasRelationship( Direction dir )", n -> n.hasRelationship(BOTH))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIPS_BY_TYPE_AND_DIRECTION(new FacadeMethod<>("Iterable<Relationship> getRelationships( RelationshipType type, Direction dir )", n -> consume(n.getRelationships(FOO, BOTH)))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_RELATIONSHIP_BY_TYPE_AND_DIRECTION(new FacadeMethod<>("boolean hasRelationship( RelationshipType type, Direction dir )", n -> n.hasRelationship(FOO, BOTH))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_SINGLE_RELATIONSHIP(new FacadeMethod<>("Relationship getSingleRelationship( RelationshipType type, Direction dir )", n -> n.getSingleRelationship(FOO, BOTH))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CREATE_RELATIONSHIP_TO(new FacadeMethod<>("Relationship createRelationshipTo( Node otherNode, RelationshipType type )", n -> n.createRelationshipTo(n, FOO))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ADD_LABEL(new FacadeMethod<>("void addLabel( Label label )", n -> n.addLabel(QUUX))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVE_LABEL(new FacadeMethod<>("void removeLabel( Label label )", n -> n.removeLabel(QUUX))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HAS_LABEL(new FacadeMethod<>("boolean hasLabel( Label label )", n -> n.hasLabel(QUUX))),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_LABELS(new FacadeMethod<>("ResourceIterable<Label> getLabels()", n -> consume(n.getLabels())));

		 private static readonly IList<NodeFacadeMethods> valueList = new List<NodeFacadeMethods>();

		 static NodeFacadeMethods()
		 {
			 valueList.Add( HAS_PROPERTY );
			 valueList.Add( GET_PROPERTY );
			 valueList.Add( GET_PROPERTY_WITH_DEFAULT );
			 valueList.Add( SET_PROPERTY );
			 valueList.Add( REMOVE_PROPERTY );
			 valueList.Add( GET_PROPERTY_KEYS );
			 valueList.Add( Delete );
			 valueList.Add( GET_RELATIONSHIPS );
			 valueList.Add( HasRelationship );
			 valueList.Add( GET_RELATIONSHIPS_BY_TYPE );
			 valueList.Add( GET_RELATIONSHIPS_BY_DIRECTION_AND_TYPES );
			 valueList.Add( HAS_RELATIONSHIP_BY_TYPE );
			 valueList.Add( HAS_RELATIONSHIP_BY_DIRECTION_AND_TYPE );
			 valueList.Add( GET_RELATIONSHIPS_BY_DIRECTION );
			 valueList.Add( HAS_RELATIONSHIP_BY_DIRECTION );
			 valueList.Add( GET_RELATIONSHIPS_BY_TYPE_AND_DIRECTION );
			 valueList.Add( HAS_RELATIONSHIP_BY_TYPE_AND_DIRECTION );
			 valueList.Add( GET_SINGLE_RELATIONSHIP );
			 valueList.Add( CREATE_RELATIONSHIP_TO );
			 valueList.Add( ADD_LABEL );
			 valueList.Add( REMOVE_LABEL );
			 valueList.Add( HAS_LABEL );
			 valueList.Add( GET_LABELS );
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
			 GET_RELATIONSHIPS,
			 HasRelationship,
			 GET_RELATIONSHIPS_BY_TYPE,
			 GET_RELATIONSHIPS_BY_DIRECTION_AND_TYPES,
			 HAS_RELATIONSHIP_BY_TYPE,
			 HAS_RELATIONSHIP_BY_DIRECTION_AND_TYPE,
			 GET_RELATIONSHIPS_BY_DIRECTION,
			 HAS_RELATIONSHIP_BY_DIRECTION,
			 GET_RELATIONSHIPS_BY_TYPE_AND_DIRECTION,
			 HAS_RELATIONSHIP_BY_TYPE_AND_DIRECTION,
			 GET_SINGLE_RELATIONSHIP,
			 CREATE_RELATIONSHIP_TO,
			 ADD_LABEL,
			 REMOVE_LABEL,
			 HAS_LABEL,
			 GET_LABELS
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly FacadeMethod<Node> facadeMethod;

		 internal NodeFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Node> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Node node )
		 {
			  _facadeMethod.accept( node );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<NodeFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static NodeFacadeMethods valueOf( string name )
		{
			foreach ( NodeFacadeMethods enumInstance in NodeFacadeMethods.valueList )
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