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
namespace Neo4Net.@internal.Kernel.Api
{
	/// <summary>
	/// Describe the capability of an index to also return the exact property value for a given query.
	/// </summary>
	public sealed class IndexValueCapability
	{
		 public static readonly IndexValueCapability Yes = new IndexValueCapability( "Yes", InnerEnum.Yes, 3 ); // Can provide values for query
		 public static readonly IndexValueCapability Partial = new IndexValueCapability( "Partial", InnerEnum.Partial, 2 ); // Can provide values for query for part of result set, often depending on what type the value has
		 public static readonly IndexValueCapability No = new IndexValueCapability( "No", InnerEnum.No, 1 ); // Can not provide values for query

		 private static readonly IList<IndexValueCapability> valueList = new List<IndexValueCapability>();

		 static IndexValueCapability()
		 {
			 valueList.Add( Yes );
			 valueList.Add( Partial );
			 valueList.Add( No );
		 }

		 public enum InnerEnum
		 {
			 Yes,
			 Partial,
			 No
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 /// <summary>
		 /// Higher order indicate a higher capability.
		 /// </summary>
		 internal Private readonly;

		 internal IndexValueCapability( string name, InnerEnum innerEnum, int order )
		 {
			  this._order = order;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Positive result if this capability is higher than other.
		 /// Negative result if this capability is lower that other.
		 /// Zero if this has same capability as other.
		 /// </summary>
		 public int Compare( IndexValueCapability other )
		 {
			  return Integer.compare( _order, other._order );
		 }

		public static IList<IndexValueCapability> values()
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

		public static IndexValueCapability valueOf( string name )
		{
			foreach ( IndexValueCapability enumInstance in IndexValueCapability.valueList )
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