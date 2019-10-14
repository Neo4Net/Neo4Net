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
namespace Neo4Net.Graphdb
{

	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.Graphdb.schema.ConstraintType;

	public sealed class ConstraintDefinitionFacadeMethods : Consumer<ConstraintDefinition>
	{
		 public static readonly ConstraintDefinitionFacadeMethods GetLabel = new ConstraintDefinitionFacadeMethods( "GetLabel", InnerEnum.GetLabel, new FacadeMethod<>( "Label getLabel()", Neo4Net.Graphdb.schema.ConstraintDefinition::getLabel ) );
		 public static readonly ConstraintDefinitionFacadeMethods GetRelationshipType = new ConstraintDefinitionFacadeMethods( "GetRelationshipType", InnerEnum.GetRelationshipType, new FacadeMethod<>( "RelationshipType getRelationshipType()", Neo4Net.Graphdb.schema.ConstraintDefinition::getRelationshipType ) );
		 public static readonly ConstraintDefinitionFacadeMethods Drop = new ConstraintDefinitionFacadeMethods( "Drop", InnerEnum.Drop, new FacadeMethod<>( "void drop()", Neo4Net.Graphdb.schema.ConstraintDefinition.drop ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IS_CONSTRAINT_TYPE(new FacadeMethod<>("boolean isConstraintType( ConstraintType type )", self -> self.isConstraintType(org.neo4j.graphdb.schema.ConstraintType.UNIQUENESS))),
		 public static readonly ConstraintDefinitionFacadeMethods GetPropertyKeys = new ConstraintDefinitionFacadeMethods( "GetPropertyKeys", InnerEnum.GetPropertyKeys, new FacadeMethod<>( "Iterable<String> getPropertyKeys()", Neo4Net.Graphdb.schema.ConstraintDefinition::getPropertyKeys ) );

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

		 internal ConstraintDefinitionFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Neo4Net.Graphdb.schema.ConstraintDefinition> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Neo4Net.Graphdb.schema.ConstraintDefinition constraintDefinition )
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