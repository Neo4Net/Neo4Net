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
namespace Neo4Net.Kernel.extension
{
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// This determines the life cycle that the extension should be part of.
	/// <para>
	/// <seealso cref="DATABASE"/> implies that the extension should be able to handle restarts where you can have
	/// any number of <seealso cref="Lifecycle.start()"/> and <seealso cref="Lifecycle.stop()"/> calls in between the
	/// <seealso cref="Lifecycle.init()"/> and <seealso cref="Lifecycle.shutdown()"/>.
	/// </para>
	/// </summary>
	public enum ExtensionType
	{
		 Global,
		 Database
	}

}