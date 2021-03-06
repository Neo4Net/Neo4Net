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
namespace Org.Neo4j.Graphdb
{
	/// <summary>
	/// Defines relationship directions used when getting relationships from a node
	/// or when creating traversers.
	/// <para>
	/// A relationship has a direction from a node's point of view. If a node is the
	/// start node of a relationship it will be an <seealso cref="OUTGOING"/> relationship
	/// from that node's point of view. If a node is the end node of a relationship
	/// it will be an <seealso cref="INCOMING"/> relationship from that node's point of view.
	/// The <seealso cref="BOTH"/> direction is used when direction is of no importance, such
	/// as "give me all" or "traverse all" relationships that are either
	/// <seealso cref="OUTGOING"/> or <seealso cref="INCOMING"/>.
	/// </para>
	/// </summary>
	public sealed class Direction
	{
		 /// <summary>
		 /// Defines outgoing relationships.
		 /// </summary>
		 public static readonly Direction Outgoing = new Direction( "Outgoing", InnerEnum.Outgoing );
		 /// <summary>
		 /// Defines incoming relationships.
		 /// </summary>
		 public static readonly Direction Incoming = new Direction( "Incoming", InnerEnum.Incoming );
		 /// <summary>
		 /// Defines both incoming and outgoing relationships.
		 /// </summary>
		 public static readonly Direction Both = new Direction( "Both", InnerEnum.Both );

		 private static readonly IList<Direction> valueList = new List<Direction>();

		 static Direction()
		 {
			 valueList.Add( Outgoing );
			 valueList.Add( Incoming );
			 valueList.Add( Both );
		 }

		 public enum InnerEnum
		 {
			 Outgoing,
			 Incoming,
			 Both
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private Direction( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Reverses the direction returning <seealso cref="INCOMING"/> if this equals
		 /// <seealso cref="OUTGOING"/>, <seealso cref="OUTGOING"/> if this equals <seealso cref="INCOMING"/> or
		 /// <seealso cref="BOTH"/> if this equals <seealso cref="BOTH"/>.
		 /// </summary>
		 /// <returns> The reversed direction. </returns>
		 public Direction Reverse()
		 {
			  switch ( this )
			  {
					case OUTGOING:
						 return INCOMING;
					case INCOMING:
						 return OUTGOING;
					case BOTH:
						 return BOTH;
					default:
						 throw new System.InvalidOperationException( "Unknown Direction " + "enum: " + this );
			  }
		 }

		public static IList<Direction> values()
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

		public static Direction valueOf( string name )
		{
			foreach ( Direction enumInstance in Direction.valueList )
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