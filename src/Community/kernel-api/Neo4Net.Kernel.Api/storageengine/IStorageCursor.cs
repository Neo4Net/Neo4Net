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
using System;

namespace Neo4Net.Kernel.Api.StorageEngine
{
	/// <summary>
	/// Base interface for a ICursor accessing and reading data as part of <seealso cref="StorageReader"/>.
	/// </summary>
	public interface IStorageCursor : IDisposable
	{
		 /// <summary>
		 /// Positions this ICursor and reads the next item that it has been designated to read. </summary>
		 /// <returns> {@code true} if the item was read and in use, otherwise {@code false}. </returns>
		 bool Next();

		 /// <summary>
		 /// Resets state in this ICursor so that calls to <seealso cref="next()"/> will not return {@code true} anymore.
		 /// After this point this ICursor can still be used, after initializing it with any of the cursor-specific
		 /// initialization method.
		 /// </summary>
		 void Reset();

		 /// <summary>
		 /// Closes and releases resources allocated by this ICursor so that it cannot be initialized or used again after this call.
		 /// </summary>
		 void Close();
	}

}