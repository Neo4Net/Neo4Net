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
namespace Neo4Net.Kernel.Api.Internal
{
	/// <summary>
	/// After invoking <seealso cref="suspend()"/> this ICursor is closed and needs to be re-initialized.
	/// 
	/// A ICursor can be (re-)initialized by passing a position to the <seealso cref="resume(CursorPosition) resume method"/>.
	/// </summary>
	/// @param <Position> the type of position used by this cursor. </param>
	public interface ISuspendableCursor<Position> : ICursor where Position : CursorPosition<Position>
	{
		 Position Suspend();

		 void Resume( Position position );
	}

}