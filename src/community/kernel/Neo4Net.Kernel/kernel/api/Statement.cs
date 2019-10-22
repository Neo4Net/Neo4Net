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
namespace Neo4Net.Kernel.api
{
	using Resource = Neo4Net.GraphDb.Resource;

	/// <summary>
	/// A statement which is a smaller coherent unit of work inside a <seealso cref="KernelTransaction"/>.
	/// 
	/// The main purpose of the statement is to keep resources open for the duration of the statement, and
	/// then close all resources at statement close.
	/// 
	/// Note that Statement used to be the access-point for all kernel reads and writes before 3.4. For
	/// accessing the graph now, see <seealso cref="org.Neo4Net.internal.kernel.api.Transaction"/>. The only remainder
	/// the QueryRegistryOperations, which will eventually also move from here.
	/// </summary>
	public interface Statement : Resource, ResourceManager
	{
		 /// <returns> interface exposing operations for associating metadata with this statement </returns>
		 QueryRegistryOperations QueryRegistration();
	}

}