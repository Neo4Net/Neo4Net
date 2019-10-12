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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

	/// <summary>
	/// Stores collision values efficiently for retrieval later. The idea is that there's a single thread <seealso cref="add(object) adding"/>
	/// ids, each gets assigned an offset, and later use those offsets to get back the added ids.
	/// </summary>
	public interface CollisionValues : Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable, AutoCloseable
	{
		 long Add( object id );

		 object Get( long offset );

		 void Close();
	}

}