﻿/*
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
	/// Cursor over relationships.
	/// </summary>
	public interface StorageRelationshipScanCursor : StorageRelationshipCursor, StorageEntityScanCursor
	{
		 /// <summary>
		 /// Initializes this cursor so that it will scan over existing relationships. Each call to <seealso cref="next()"/> will
		 /// advance the cursor so that the next node is read.
		 /// </summary>
		 /// <param name="type"> relationship type to scan over, or -1 for all relationships regardless of type. </param>
		 void Scan( int type );
	}

}