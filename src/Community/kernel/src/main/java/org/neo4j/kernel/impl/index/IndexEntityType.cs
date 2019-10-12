using System;
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
namespace Neo4Net.Kernel.impl.index
{
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;

	public abstract class IndexEntityType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Node((byte) 0) { public Class entityClass() { return org.neo4j.graphdb.Node.class; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Relationship((byte) 1) { public Class entityClass() { return org.neo4j.graphdb.Relationship.class; } };

		 private static readonly IList<IndexEntityType> valueList = new List<IndexEntityType>();

		 static IndexEntityType()
		 {
			 valueList.Add( Node );
			 valueList.Add( Relationship );
		 }

		 public enum InnerEnum
		 {
			 Node,
			 Relationship
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private IndexEntityType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly sbyte id;

		 IndexEntityType( sbyte id ) { this.id = id; } public sbyte id() { return id; } public abstract Type entityClass();

		 public static readonly IndexEntityType public static IndexEntityType byId( byte id )
		 {
			 for ( IndexEntityType type : values() )
			 {
				 if ( type.id() == id ) { return type; }
			 }
			 throw new IllegalArgumentException( "Unknown id " + id );
		 }
		 public String nameToLowerCase() { return this.name().toLowerCase(); } = new IndexEntityType("public static IndexEntityType byId(byte id) { for(IndexEntityType type : values()) { if(type.id() == id) { return type; } } throw new IllegalArgumentException("Unknown id " + id); } public String nameToLowerCase() { return this.name().toLowerCase(); }", InnerEnum.public static IndexEntityType byId(byte id)
		 {
			 for ( IndexEntityType type : values() )
			 {
				 if ( type.id() == id ) { return type; }
			 }
			 throw new IllegalArgumentException( "Unknown id " + id );
		 }
		 public String nameToLowerCase() { return this.name().toLowerCase(); });

		public static IList<IndexEntityType> values()
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

		public static IndexEntityType valueOf( string name )
		{
			foreach ( IndexEntityType enumInstance in IndexEntityType.valueList )
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