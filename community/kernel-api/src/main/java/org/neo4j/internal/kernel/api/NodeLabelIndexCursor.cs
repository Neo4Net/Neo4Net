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
namespace Org.Neo4j.@internal.Kernel.Api
{
	/// <summary>
	/// Cursor for scanning the nodes of a label index.
	/// </summary>
	public interface NodeLabelIndexCursor : NodeIndexCursor
	{
		 /// <summary>
		 /// Labels that we know, based on the index, are present on the current node under the cursor.
		 /// The node under the current cursor position might have other labels present.
		 /// </summary>
		 /// <returns> a set of labels that are known to be present on the current node under the cursor. </returns>
		 LabelSet Labels();
	}

}