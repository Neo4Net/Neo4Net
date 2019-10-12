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
namespace Org.Neo4j.Values.Storable
{
	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;

	/// <summary>
	/// Defines all valid field accessors for points
	/// </summary>
	public abstract class PointFields
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       X("x") { Value get(PointValue value) { return value.getNthCoordinate(0, propertyKey, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Y("y") { Value get(PointValue value) { return value.getNthCoordinate(1, propertyKey, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Z("z") { Value get(PointValue value) { return value.getNthCoordinate(2, propertyKey, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LONGITUDE("longitude") { Value get(PointValue value) { return value.getNthCoordinate(0, propertyKey, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LATITUDE("latitude") { Value get(PointValue value) { return value.getNthCoordinate(1, propertyKey, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HEIGHT("height") { Value get(PointValue value) { return value.getNthCoordinate(2, propertyKey, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CRS("crs") { Value get(PointValue value) { return Values.stringValue(value.getCoordinateReferenceSystem().toString()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SRID("srid") { Value get(PointValue value) { return Values.intValue(value.getCoordinateReferenceSystem().getCode()); } };

		 private static readonly IList<PointFields> valueList = new List<PointFields>();

		 static PointFields()
		 {
			 valueList.Add( X );
			 valueList.Add( Y );
			 valueList.Add( Z );
			 valueList.Add( LONGITUDE );
			 valueList.Add( LATITUDE );
			 valueList.Add( HEIGHT );
			 valueList.Add( CRS );
			 valueList.Add( SRID );
		 }

		 public enum InnerEnum
		 {
			 X,
			 Y,
			 Z,
			 LONGITUDE,
			 LATITUDE,
			 HEIGHT,
			 CRS,
			 SRID
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private PointFields( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string propertyKey;

		 internal PointFields( string name, InnerEnum innerEnum, string propertyKey )
		 {
			  this.PropertyKey = propertyKey;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static PointFields FromName( string fieldName )
		 {
			  switch ( fieldName.ToLower() )
			  {
			  case "x":
					return X;
			  case "y":
					return Y;
			  case "z":
					return Z;
			  case "longitude":
					return LONGITUDE;
			  case "latitude":
					return LATITUDE;
			  case "height":
					return HEIGHT;
			  case "crs":
					return CRS;
			  case "srid":
					return SRID;
			  default:
					throw new InvalidValuesArgumentException( "No such field: " + fieldName );
			  }
		 }

		 internal abstract Value get( PointValue value );

		public static IList<PointFields> values()
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

		public static PointFields valueOf( string name )
		{
			foreach ( PointFields enumInstance in PointFields.valueList )
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