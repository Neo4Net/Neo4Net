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
namespace Org.Neo4j.Kernel.impl.store.counts.keys
{
	public sealed class CountsKeyType
	{
		 public static readonly CountsKeyType Empty = new CountsKeyType( "Empty", InnerEnum.Empty, 0 );
		 public static readonly CountsKeyType EntityNode = new CountsKeyType( "EntityNode", InnerEnum.EntityNode, 2 );
		 public static readonly CountsKeyType EntityRelationship = new CountsKeyType( "EntityRelationship", InnerEnum.EntityRelationship, 3 );
		 public static readonly CountsKeyType IndexStatistics = new CountsKeyType( "IndexStatistics", InnerEnum.IndexStatistics, 4 );
		 public static readonly CountsKeyType IndexSample = new CountsKeyType( "IndexSample", InnerEnum.IndexSample, 5 );

		 private static readonly IList<CountsKeyType> valueList = new List<CountsKeyType>();

		 static CountsKeyType()
		 {
			 valueList.Add( Empty );
			 valueList.Add( EntityNode );
			 valueList.Add( EntityRelationship );
			 valueList.Add( IndexStatistics );
			 valueList.Add( IndexSample );
		 }

		 public enum InnerEnum
		 {
			 Empty,
			 EntityNode,
			 EntityRelationship,
			 IndexStatistics,
			 IndexSample
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public readonly;

		 internal CountsKeyType( string name, InnerEnum innerEnum, int code )
		 {
			  this.Code = ( sbyte ) code;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static CountsKeyType Value( sbyte val )
		 {
			  switch ( val )
			  {
			  case 2:
					return CountsKeyType.EntityNode;
			  case 3:
					return CountsKeyType.EntityRelationship;
			  case 4:
					return CountsKeyType.IndexStatistics;
			  case 5:
					return CountsKeyType.IndexSample;
			  default:
					throw new System.ArgumentException( "Parsed key type from count store deserialization of unknown type." );
			  }
		 }

		public static IList<CountsKeyType> values()
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

		public static CountsKeyType valueOf( string name )
		{
			foreach ( CountsKeyType enumInstance in CountsKeyType.valueList )
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