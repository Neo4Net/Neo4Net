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
namespace Neo4Net.Kernel.Api.StorageEngine
{
	/// <summary>
	/// Type of graph Entity. The three types, Nodes, Relationships and Graphs, represent objects that can have properties
	/// associated with them, as well as labeled with additional type information. Nodes have labels, and relationships
	/// have relationship types. Graphs can have properties, but are not labeled.
	/// </summary>
	public sealed class EntityType
	{
		 public static readonly EntityType Node = new EntityType( "Node", InnerEnum.Node, "label" );
		 public static readonly EntityType Relationship = new EntityType( "Relationship", InnerEnum.Relationship, "relationship type" );
		 public static readonly EntityType Graph = new EntityType( "Graph", InnerEnum.Graph, "" );

		 private static readonly IList<EntityType> _valueList = new List<EntityType>();

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

		 public readonly InnerEnum _innerEnumValue;
		 private readonly string _nameValue;
		 private readonly int _ordinalValue;
		 private static int _nextOrdinal = 0;

		 private readonly;

		 internal EntityType( string name, InnerEnum innerEnum, string labelingType )
		 {
			  this._labelingType = labelingType;

			 _nameValue = name;
			 _ordinalValue = nextOrdinal++;
			 _innerEnumValue = innerEnum;
		 }

		 /// <returns> the name of the labeling type for this Entity type </returns>
		 public string LabelingType
		 {
			 get
			 {
				  return _labelingType;
			 }
		 }

      public static IList<EntityType> values => valueList;

      public int ordinal => ordinalValue;

      public override string ToString()
		{
			return nameValue;
		}

		public static EntityType ValueOf( string name )
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