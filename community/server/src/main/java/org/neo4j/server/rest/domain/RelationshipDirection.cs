﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.domain
{
	using Direction = Org.Neo4j.Graphdb.Direction;

	public sealed class RelationshipDirection
	{
		 public static readonly RelationshipDirection All = new RelationshipDirection( "All", InnerEnum.All, Org.Neo4j.Graphdb.Direction.Both );
		 public static readonly RelationshipDirection In = new RelationshipDirection( "In", InnerEnum.In, Org.Neo4j.Graphdb.Direction.Incoming );
		 public static readonly RelationshipDirection Out = new RelationshipDirection( "Out", InnerEnum.Out, Org.Neo4j.Graphdb.Direction.Outgoing );

		 private static readonly IList<RelationshipDirection> valueList = new List<RelationshipDirection>();

		 static RelationshipDirection()
		 {
			 valueList.Add( All );
			 valueList.Add( In );
			 valueList.Add( Out );
		 }

		 public enum InnerEnum
		 {
			 All,
			 In,
			 Out
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 internal Final org;

		 internal RelationshipDirection( string name, InnerEnum innerEnum, Org.Neo4j.Graphdb.Direction @internal )
		 {
			  this.Internal = @internal;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		public static IList<RelationshipDirection> values()
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

		public static RelationshipDirection valueOf( string name )
		{
			foreach ( RelationshipDirection enumInstance in RelationshipDirection.valueList )
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