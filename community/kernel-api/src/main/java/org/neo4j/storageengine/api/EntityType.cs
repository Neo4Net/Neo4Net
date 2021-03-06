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
namespace Org.Neo4j.Storageengine.Api
{
	/// <summary>
	/// Type of graph entity. The three types, Nodes, Relationships and Graphs, represent objects that can have properties
	/// associated with them, as well as labeled with additional type information. Nodes have labels, and relationships
	/// have relationship types. Graphs can have properties, but are not labeled.
	/// </summary>
	public sealed class EntityType
	{
		 public static readonly EntityType Node = new EntityType( "Node", InnerEnum.Node, "label" );
		 public static readonly EntityType Relationship = new EntityType( "Relationship", InnerEnum.Relationship, "relationship type" );
		 public static readonly EntityType Graph = new EntityType( "Graph", InnerEnum.Graph, "" );

		 private static readonly IList<EntityType> valueList = new List<EntityType>();

		 static EntityType()
		 {
			 valueList.Add( Node );
			 valueList.Add( Relationship );
			 valueList.Add( Graph );
		 }

		 public enum InnerEnum
		 {
			 Node,
			 Relationship,
			 Graph
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal EntityType( string name, InnerEnum innerEnum, string labelingType )
		 {
			  this._labelingType = labelingType;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <returns> the name of the labeling type for this entity type </returns>
		 public string LabelingType
		 {
			 get
			 {
				  return _labelingType;
			 }
		 }

		public static IList<EntityType> values()
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

		public static EntityType valueOf( string name )
		{
			foreach ( EntityType enumInstance in EntityType.valueList )
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