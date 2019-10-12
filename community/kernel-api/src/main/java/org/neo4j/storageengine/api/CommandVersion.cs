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
namespace Org.Neo4j.Storageengine.Api
{
	/// <summary>
	/// Many commands have before/after versions of their records. In some scenarios there's a need
	/// to parameterize which of those to work with.
	/// </summary>
	public enum CommandVersion
	{
		 /// <summary>
		 /// The "before" version of a command's record. I.e. the record how it looked before changes took place.
		 /// </summary>
		 Before,
		 /// <summary>
		 /// The "after" version of a command's record. I.e. the record how it looks after changes took place.
		 /// </summary>
		 After
	}

}