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
namespace Org.Neo4j.Graphdb
{
	/// <summary>
	/// An Entity is a <seealso cref="PropertyContainer"/> that is persisted in the database, and identified by an <seealso cref="getId() id"/>.
	/// <para>
	/// <seealso cref="Node Nodes"/> and <seealso cref="Relationship Relationships"/> are Entities.
	/// </para>
	/// </summary>
	public interface Entity : PropertyContainer
	{
		 /// <summary>
		 /// Returns the unique id of this Entity. Id's are garbage
		 /// collected over time so they are only guaranteed to be unique during a
		 /// specific time span: if the Entity is deleted, it's
		 /// likely that a new Entity at some point will get the old
		 /// id. <b>Note</b>: this makes Entity id's brittle as
		 /// public APIs.
		 /// </summary>
		 /// <returns> The id of this Entity. </returns>
		 long Id { get; }
	}

}