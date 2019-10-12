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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	/// <summary>
	/// Convenient way of selecting nodes based on their dense status.
	/// </summary>
	public class NodeType
	{
		 public const int NODE_TYPE_DENSE = 0x1;
		 public const int NODE_TYPE_SPARSE = 0x2;
		 public const int NODE_TYPE_ALL = NODE_TYPE_DENSE | NODE_TYPE_SPARSE;

		 private NodeType()
		 {
		 }

		 public static bool IsDense( int nodeTypes )
		 {
			  return Has( nodeTypes, NODE_TYPE_DENSE );
		 }

		 public static bool IsSparse( int nodeTypes )
		 {
			  return Has( nodeTypes, NODE_TYPE_SPARSE );
		 }

		 private static bool Has( int nodeTypes, int mask )
		 {
			  return ( nodeTypes & mask ) != 0;
		 }

		 public static bool MatchesDense( int nodeTypes, bool isDense )
		 {
			  int mask = isDense ? NODE_TYPE_DENSE : NODE_TYPE_SPARSE;
			  return Has( nodeTypes, mask );
		 }
	}

}