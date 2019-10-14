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
namespace Neo4Net.Graphdb.@event
{
	/// <summary>
	/// An object that describes a state from which a Neo4j Graph Database cannot
	/// continue.
	/// 
	/// </summary>
	public enum ErrorState
	{
		 /// <summary>
		 /// The Graph Database failed since the storage media where the graph
		 /// database data is stored is full and cannot be written to.
		 /// </summary>
		 StorageMediaFull,

		 /// <summary>
		 /// Not more transactions can be started or committed during this session
		 /// and the database needs to be shut down, possible for maintenance before
		 /// it can be started again.
		 /// </summary>
		 TxManagerNotOk,
	}

}