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
	/// A coordinate reference system (CRS) determines how a $<seealso cref="Coordinate"/> should be interpreted
	/// <para>
	/// The CRS is defined by three properties a code, a type, and a link to CRS parameters on the web.
	/// Example:
	/// <code>
	/// {
	/// code: 4326,
	/// type: "WGS-84",
	/// href: "http://spatialreference.org/ref/epsg/4326/"
	/// }
	/// </code>
	/// </para>
	/// </summary>
	public interface CRS
	{

		 /// <summary>
		 /// The numerical code associated with the CRS
		 /// </summary>
		 /// <returns> a numerical code associated with the CRS </returns>
		 int Code { get; }

		 /// <summary>
		 /// The type of the CRS is a descriptive name, indicating which CRS is used
		 /// </summary>
		 /// <returns> the type of the CRS </returns>
		 string Type { get; }

		 /// <summary>
		 /// A link uniquely identifying the CRS.
		 /// </summary>
		 /// <returns> A link to where the CRS is described. </returns>
		 string Href { get; }
	}

}