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

	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;

	public sealed class IndexCreatorFacadeMethods : Consumer<IndexCreator>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ON(new FacadeMethod<>("IndexCreator on( String propertyKey )", self -> self.on("property"))),
		 public static readonly IndexCreatorFacadeMethods Create = new IndexCreatorFacadeMethods( "Create", InnerEnum.Create, new FacadeMethod<>( "IndexDefinition create()", Org.Neo4j.Graphdb.schema.IndexCreator::create ) );

		 private static readonly IList<IndexCreatorFacadeMethods> valueList = new List<IndexCreatorFacadeMethods>();

		 static IndexCreatorFacadeMethods()
		 {
			 valueList.Add( ON );
			 valueList.Add( Create );
		 }

		 public enum InnerEnum
		 {
			 ON,
			 Create
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal IndexCreatorFacadeMethods( string name, InnerEnum innerEnum, FacadeMethod<Org.Neo4j.Graphdb.schema.IndexCreator> facadeMethod )
		 {
			  this._facadeMethod = facadeMethod;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public void Accept( Org.Neo4j.Graphdb.schema.IndexCreator indexCreator )
		 {
			  _facadeMethod.accept( indexCreator );
		 }

		 public override string ToString()
		 {
			  return _facadeMethod.ToString();
		 }

		public static IList<IndexCreatorFacadeMethods> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static IndexCreatorFacadeMethods valueOf( string name )
		{
			foreach ( IndexCreatorFacadeMethods enumInstance in IndexCreatorFacadeMethods.valueList )
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