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
namespace Neo4Net.Graphdb.spatial
{


	/// <summary>
	/// A geometry is defined by a list of coordinates and a coordinate reference system.
	/// </summary>
	public interface Geometry
	{
		 /// <summary>
		 /// Get string description of most specific type of this instance
		 /// </summary>
		 /// <returns> The instance type implementing Geometry </returns>
		 string GeometryType { get; }

		 /// <summary>
		 /// Get all coordinates of the geometry.
		 /// </summary>
		 /// <returns> The coordinates of the geometry. </returns>
		 IList<Coordinate> Coordinates { get; }

		 /// <summary>
		 /// Returns the coordinate reference system associated with the geometry
		 /// </summary>
		 /// <returns> A $<seealso cref="CRS"/> associated with the geometry </returns>
		 CRS CRS { get; }
	}

}