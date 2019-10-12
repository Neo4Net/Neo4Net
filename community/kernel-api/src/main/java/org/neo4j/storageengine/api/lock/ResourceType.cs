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
namespace Org.Neo4j.Storageengine.Api.@lock
{
	/// <summary>
	/// Locks are split by resource types. It is up to the implementation to define the contract for these. </summary>
	public interface ResourceType
	{
		 /// <summary>
		 /// Must be unique among all existing resource types, should preferably be a sequence starting at 0. </summary>
		 int TypeId();

		 /// <summary>
		 /// What to do if the lock cannot immediately be acquired. </summary>
		 WaitStrategy WaitStrategy();

		 /// <summary>
		 /// Must be unique among all existing resource types. </summary>
		 string Name();
	}

}