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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.executor
{

	/// <summary>
	/// Task submitted to a <seealso cref="TaskExecutor"/>, similar to <seealso cref="Callable"/> or <seealso cref="System.Threading.ThreadStart"/>,
	/// but tailored to <seealso cref="TaskExecutor"/> in that f.ex. it <seealso cref="run(object) runs"/> with a pre-defined
	/// thread-local state as parameter.
	/// </summary>
	/// @param <LOCAL> thread-local state provided by the <seealso cref="TaskExecutor"/> executing this task. </param>
	public delegate void Task<LOCAL>( LOCAL threadLocalState );

}