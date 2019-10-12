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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.stats
{
	/// <summary>
	/// <seealso cref="Stat Statistics"/> key. Has accessors for different types of information, like a name for identifying the key,
	/// a short name for brief display and a description.
	/// </summary>
	public interface Key
	{
		 /// <summary>
		 /// Name that identifies this key.
		 /// </summary>
		 string Name();

		 /// <summary>
		 /// Short name for outputs with tight space.
		 /// </summary>
		 string ShortName();

		 /// <summary>
		 /// Longer description of what this key represents.
		 /// </summary>
		 string Description();
	}

}