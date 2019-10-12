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

	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Org.Neo4j.Graphdb.schema.ConstraintType;

	public sealed class ConstraintDefinitionFacadeMethods : Consumer<ConstraintDefinition>
	{
		 public static readonly ConstraintDefinitionFacadeMethods GetLabel = new ConstraintDefinitionFacadeMethods( "GetLabel", InnerEnum.GetLabel, new FacadeMethod<>( "Label getLabel()", Org.Neo4j.Graphdb.schema.ConstraintDefinition::getLabel ) );
		 public static readonly ConstraintDefinitionFacadeMethods GetRelationshipType = new ConstraintDefinitionFacadeMethods( "GetRelationshipType", InnerEnum.GetRelationshipType, new FacadeMethod<>( "RelationshipType getRelationshipType()", Org.Neo4j.Graphdb.schema.ConstraintDefinition::getRelationshipType ) );
		 public static readonly ConstraintDefinitionFacadeMethods Drop = new ConstraintDefinitionFacadeMethods( "Drop", InnerEnum.Drop, new FacadeMethod<>( "void drop()", Org.Neo4j.Graphdb.schema.ConstraintDefinition.drop ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IS_CONSTRAINT_TYPE(new FacadeMethod<>("boolean isConstraintType( ConstraintType type )", self -> self.isConstraintType(org.neo4j.graphdb.schema.ConstraintType.UNIQUENESS))),
		 public static readonly ConstraintDefinitionFacadeMethods GetPropertyKeys = new ConstraintDefinitionFacadeMethods( "GetPropertyKeys", InnerEnum.GetPropertyKeys, new FacadeMethod<>( "Iterable<String> getPropertyKeys()", Org.Neo4j.Graphdb.schema.ConstraintDefinition::getPropertyKeys ) );

		 private static readonly IList<ConstraintDefinitionFacadeMethods> valueList = new List<ConstraintDefinitionFacadeMethods>();

		 static ConstraintDefinitionFacadeMethods()
		 {
			 valueList.Add( GetLabel );
			 valueList.Add( GetRelationshipType );
			 valueList.Add( Drop );
			 valueList.Add( IS_CONSTRAINT_TYPE );
			 valueList.Add( GetPropertyKeys );
		 }

		 public enum InnerEnum
		 {
			 GetLabel,
			 GetRelationshipType,
			 Drop,
			 IS_CONSTRAINT_TYPE,
			 GetPropertyKeys
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal ConstraintDefinitionFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Org.Neo4j.Graphdb.schema.ConstraintDefinition> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Org.Neo4j.Graphdb.schema.ConstraintDefinition constraintDefinition )
		 {
			  _facadeMethod.accept( constraintDefinition );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<ConstraintDefinitionFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static ConstraintDefinitionFacadeMethods valueOf( string name )
		{
			foreach ( ConstraintDefinitionFacadeMethods enumInstance in ConstraintDefinitionFacadeMethods.valueList )
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