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
	/// A <seealso cref="StorageCursor"/> for entities, i.e. which has properties.
	/// </summary>
	public interface StorageEntityCursor : StorageCursor
	{
		 /// <returns> {@code true} if the IEntity the cursor is at has any properties, otherwise {@code false}. </returns>
		 bool HasProperties();

		 /// <returns> a {@code long} reference to start reading properties for the IEntity this cursor is at. </returns>
		 long PropertiesReference();

		 /// <returns> reference to the IEntity this cursor currently is placed at. </returns>
		 long IEntityReference();
	}

}