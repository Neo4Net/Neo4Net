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
namespace Neo4Net.Storageengine.Api
{
	/// <summary>
	/// Type of graph IEntity. The three types, Nodes, Relationships and Graphs, represent objects that can have properties
	/// associated with them, as well as labeled with additional type information. Nodes have labels, and relationships
	/// have relationship types. Graphs can have properties, but are not labeled.
	/// </summary>
	public sealed class IEntityType
	{
		 public static readonly IEntityType Node = new IEntityType( "Node", InnerEnum.Node, "label" );
		 public static readonly IEntityType Relationship = new IEntityType( "Relationship", InnerEnum.Relationship, "relationship type" );
		 public static readonly IEntityType Graph = new IEntityType( "Graph", InnerEnum.Graph, "" );

		 private static readonly IList<EntityType> valueList = new List<EntityType>();

		 static IEntityType()
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

		 internal IEntityType( string name, InnerEnum innerEnum, string labelingType )
		 {
			  this._labelingType = labelingType;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <returns> the name of the labeling type for this IEntity type </returns>
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

		public static IEntityType valueOf( string name )
		{
			foreach ( IEntityType enumInstance in IEntityType.valueList )
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