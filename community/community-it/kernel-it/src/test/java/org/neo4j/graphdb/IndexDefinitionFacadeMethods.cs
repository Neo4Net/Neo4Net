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
namespace Org.Neo4j.Graphdb
{

	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.FacadeMethod.consume;

	public sealed class IndexDefinitionFacadeMethods : Consumer<IndexDefinition>
	{
		 public static readonly IndexDefinitionFacadeMethods GetLabel = new IndexDefinitionFacadeMethods( "GetLabel", InnerEnum.GetLabel, new FacadeMethod<>( "Label getLabel()", Org.Neo4j.Graphdb.schema.IndexDefinition::getLabel ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_LABELS(new FacadeMethod<>("Iterable<Label> getLabels()", self -> consume(self.getLabels()))),
		 public static readonly IndexDefinitionFacadeMethods GetRelationshipType = new IndexDefinitionFacadeMethods( "GetRelationshipType", InnerEnum.GetRelationshipType, new FacadeMethod<>( "RelationshipType getRelationshipType()", Org.Neo4j.Graphdb.schema.IndexDefinition::getRelationshipType ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GET_RELATIONSHIP_TYPES(new FacadeMethod<>("Iterable<RelationshipType> getRelationshipTypes()", self -> consume(self.getRelationshipTypes()))),
		 public static readonly IndexDefinitionFacadeMethods GetPropertyKeys = new IndexDefinitionFacadeMethods( "GetPropertyKeys", InnerEnum.GetPropertyKeys, new FacadeMethod<>( "Iterable<String> getPropertyKeys()", Org.Neo4j.Graphdb.schema.IndexDefinition::getPropertyKeys ) );
		 public static readonly IndexDefinitionFacadeMethods Drop = new IndexDefinitionFacadeMethods( "Drop", InnerEnum.Drop, new FacadeMethod<>( "void drop()", Org.Neo4j.Graphdb.schema.IndexDefinition.drop ) );
		 public static readonly IndexDefinitionFacadeMethods IsConstraintIndex = new IndexDefinitionFacadeMethods( "IsConstraintIndex", InnerEnum.IsConstraintIndex, new FacadeMethod<>( "boolean isConstraintIndex()", Org.Neo4j.Graphdb.schema.IndexDefinition::isConstraintIndex ) );
		 public static readonly IndexDefinitionFacadeMethods IsNodeIndex = new IndexDefinitionFacadeMethods( "IsNodeIndex", InnerEnum.IsNodeIndex, new FacadeMethod<>( "boolean isNodeIndex()", Org.Neo4j.Graphdb.schema.IndexDefinition::isNodeIndex ) );
		 public static readonly IndexDefinitionFacadeMethods IsRelationshipIndex = new IndexDefinitionFacadeMethods( "IsRelationshipIndex", InnerEnum.IsRelationshipIndex, new FacadeMethod<>( "boolean isRelationshipIndex()", Org.Neo4j.Graphdb.schema.IndexDefinition::isRelationshipIndex ) );
		 public static readonly IndexDefinitionFacadeMethods IsMultiTokenIndex = new IndexDefinitionFacadeMethods( "IsMultiTokenIndex", InnerEnum.IsMultiTokenIndex, new FacadeMethod<>( "boolean isMultiTokenIndex()", Org.Neo4j.Graphdb.schema.IndexDefinition::isMultiTokenIndex ) );
		 public static readonly IndexDefinitionFacadeMethods IsCompositeIndex = new IndexDefinitionFacadeMethods( "IsCompositeIndex", InnerEnum.IsCompositeIndex, new FacadeMethod<>( "boolean isCompositeIndex()", Org.Neo4j.Graphdb.schema.IndexDefinition::isCompositeIndex ) );

		 private static readonly IList<IndexDefinitionFacadeMethods> valueList = new List<IndexDefinitionFacadeMethods>();

		 static IndexDefinitionFacadeMethods()
		 {
			 valueList.Add( GetLabel );
			 valueList.Add( GET_LABELS );
			 valueList.Add( GetRelationshipType );
			 valueList.Add( GET_RELATIONSHIP_TYPES );
			 valueList.Add( GetPropertyKeys );
			 valueList.Add( Drop );
			 valueList.Add( IsConstraintIndex );
			 valueList.Add( IsNodeIndex );
			 valueList.Add( IsRelationshipIndex );
			 valueList.Add( IsMultiTokenIndex );
			 valueList.Add( IsCompositeIndex );
		 }

		 public enum InnerEnum
		 {
			 GetLabel,
			 GET_LABELS,
			 GetRelationshipType,
			 GET_RELATIONSHIP_TYPES,
			 GetPropertyKeys,
			 Drop,
			 IsConstraintIndex,
			 IsNodeIndex,
			 IsRelationshipIndex,
			 IsMultiTokenIndex,
			 IsCompositeIndex
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal IndexDefinitionFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Org.Neo4j.Graphdb.schema.IndexDefinition> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Org.Neo4j.Graphdb.schema.IndexDefinition indexDefinition )
		 {
			  _facadeMethod.accept( indexDefinition );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<IndexDefinitionFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static IndexDefinitionFacadeMethods valueOf( string name )
		{
			foreach ( IndexDefinitionFacadeMethods enumInstance in IndexDefinitionFacadeMethods.valueList )
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