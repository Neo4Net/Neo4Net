using System;

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
namespace Neo4Net.Helpers
{
	/// <summary>
	/// Represents a control handle passed to a child task which allows the creator to cancel it. The child task can
	/// use the <seealso cref="AutoCloseable"/> interface and call <seealso cref="close()"/> to signal completion of the task, regardless if
	/// it was cancelled or completed successfully.
	/// </summary>
	[Obsolete]
	public interface TaskControl : CancellationRequest, AutoCloseable
	{
		 void Close();
	}

}