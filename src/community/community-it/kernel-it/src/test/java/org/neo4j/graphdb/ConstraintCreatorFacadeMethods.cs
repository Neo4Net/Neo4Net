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

	using ConstraintCreator = Neo4Net.GraphDb.schema.ConstraintCreator;

	public sealed class ConstraintCreatorFacadeMethods : Consumer<ConstraintCreator>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UNIQUE(new FacadeMethod<>("ConstraintCreator assertPropertyIsUnique()", self -> self.assertPropertyIsUnique("property"))),
		 public static readonly ConstraintCreatorFacadeMethods Create = new ConstraintCreatorFacadeMethods( "Create", InnerEnum.Create, new FacadeMethod<>( "ConstraintDefinition create()", Neo4Net.GraphDb.schema.ConstraintCreator::create ) );

		 private static readonly IList<ConstraintCreatorFacadeMethods> valueList = new List<ConstraintCreatorFacadeMethods>();

		 static ConstraintCreatorFacadeMethods()
		 {
			 valueList.Add( UNIQUE );
			 valueList.Add( Create );
		 }

		 public enum InnerEnum
		 {
			 UNIQUE,
			 Create
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal ConstraintCreatorFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Neo4Net.GraphDb.schema.ConstraintCreator> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Neo4Net.GraphDb.schema.ConstraintCreator constraintCreator )
		 {
			  _facadeMethod.accept( constraintCreator );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<ConstraintCreatorFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static ConstraintCreatorFacadeMethods valueOf( string name )
		{
			foreach ( ConstraintCreatorFacadeMethods enumInstance in ConstraintCreatorFacadeMethods.valueList )
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