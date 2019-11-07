﻿/*
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
namespace Neo4Net.Values.@virtual
{
	/// <summary>
	/// The ValueGroup is the logical group or type of a Value. For example byte, short, int and long are all attempting
	/// to represent mathematical integers, meaning that for comparison purposes they should be treated the same.
	/// </summary>
	public enum VirtualValueGroup
	{
		 Map,
		 Node,
		 Edge,
		 List,
		 Path,
		 NoValue,
		 Error
	}

}