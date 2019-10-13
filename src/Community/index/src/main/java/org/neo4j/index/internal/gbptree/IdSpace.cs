﻿/*
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
namespace Neo4Net.Index.@internal.gbptree
{
	/// <summary>
	/// Defines special page ids for <seealso cref="GBPTree"/>.
	/// </summary>
	public class IdSpace
	{
		 /// <summary>
		 /// Page id of the meta page holding information about root id and custom user meta information.
		 /// This page id is statically allocated throughout the life of a tree.
		 /// </summary>
		 public const long META_PAGE_ID = 0L;

		 /// <summary>
		 /// State page with IDs such as free-list, highId, rootId and more. There are two such pages alternating
		 /// between checkpoints, this is the first.
		 /// </summary>
		 internal const long STATE_PAGE_A = 1L;

		 /// <summary>
		 /// State page with IDs such as free-list, highId, rootId and more. There are two such pages alternating
		 /// between checkpoints, this is the second.
		 /// </summary>
		 internal const long STATE_PAGE_B = 2L;

		 /// <summary>
		 /// Min value allowed as tree node id.
		 /// </summary>
		 internal const long MIN_TREE_NODE_ID = 3L;

		 private IdSpace()
		 {
		 }
	}

}