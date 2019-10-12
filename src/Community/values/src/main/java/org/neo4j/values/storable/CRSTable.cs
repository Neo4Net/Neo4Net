using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Values.Storable
{
	public sealed class CRSTable
	{
		 public static readonly CRSTable Custom = new CRSTable( "Custom", InnerEnum.Custom, "custom", 0 );
		 public static readonly CRSTable Epsg = new CRSTable( "Epsg", InnerEnum.Epsg, "epsg", 1 );
		 public static readonly CRSTable SrOrg = new CRSTable( "SrOrg", InnerEnum.SrOrg, "sr-org", 2 );

		 private static readonly IList<CRSTable> valueList = new List<CRSTable>();

		 static CRSTable()
		 {
			 valueList.Add( Custom );
			 valueList.Add( Epsg );
			 valueList.Add( SrOrg );
		 }

		 public enum InnerEnum
		 {
			 Custom,
			 Epsg,
			 SrOrg
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private const;

		 internal Private readonly;

		 public static CRSTable Find( int tableId )
		 {
			  if ( tableId < _types.Length )
			  {
					return _types[tableId];
			  }
			  else
			  {
					throw new System.ArgumentException( "No known Coordinate Reference System table: " + tableId );
			  }
		 }

		 internal Private readonly;
		 internal Private readonly;

		 internal CRSTable( string name, InnerEnum innerEnum, string name, int tableId )
		 {
			  Debug.Assert( LowerCase( name ) );
			  this._name = name;
			  this._tableId = tableId;
			  this._prefix = tableId == 0 ? "crs://" + name + "/" : "http://spatialreference.org/ref/" + name + "/";

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string Href( int code )
		 {
			  return _prefix + code + "/";
		 }

		 private bool LowerCase( string @string )
		 {
			  return @string.ToLower().Equals(@string);
		 }

		 public string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public int TableId
		 {
			 get
			 {
				  return _tableId;
			 }
		 }

		public static IList<CRSTable> values()
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

		public static CRSTable valueOf( string name )
		{
			foreach ( CRSTable enumInstance in CRSTable.valueList )
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