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
namespace Neo4Net.GraphDb.spatial
{
	/// <summary>
	/// A point is a geometry described by a single coordinate in space.
	/// <para>
	/// A call to <seealso cref="getCoordinates()"/> must return a single element list.
	/// </para>
	/// </summary>
	public interface Point : Geometry
	{
		 /// <summary>
		 /// Returns the single coordinate in space defining this point.
		 /// </summary>
		 /// <returns> The coordinate of this point. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Coordinate getCoordinate()
	//	 {
	//		  return getCoordinates().get(0);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default String getGeometryType()
	//	 {
	//		  return "Point";
	//	 }
	}


}