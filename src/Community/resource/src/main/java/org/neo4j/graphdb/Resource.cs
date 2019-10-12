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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// Resource that should be closed when not needed anymore. Extends <seealso cref="AutoCloseable"/>
	/// with <seealso cref="close()"/> not throwing any checked exception.
	/// </summary>
	public interface Resource : AutoCloseable
	{
		 void Close();

		 /// <summary>
		 /// Empty resource that doesn't <seealso cref="close() close"/> anything.
		 /// </summary>
	}

	public static class Resource_Fields
	{
		 public static readonly Resource Empty = () =>
		 {
		 };

	}

}