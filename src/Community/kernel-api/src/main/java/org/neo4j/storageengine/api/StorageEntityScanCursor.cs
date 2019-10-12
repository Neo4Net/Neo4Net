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
namespace Neo4Net.Storageengine.Api
{
	public interface StorageEntityScanCursor : StorageEntityCursor
	{
		 /// <summary>
		 /// Initializes this cursor so that it will scan over all existing entities. Each call to <seealso cref="next()"/> will
		 /// advance the cursor so that the next entity is read.
		 /// </summary>
		 void Scan();

		 /// <summary>
		 /// Initializes this cursor so that the next call to <seealso cref="next()"/> will place this cursor at that entity. </summary>
		 /// <param name="reference"> entity to place this cursor at the next call to <seealso cref="next()"/>. </param>
		 void Single( long reference );
	}

}