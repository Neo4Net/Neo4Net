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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Config = Org.Neo4j.Kernel.configuration.Config;

	/// <summary>
	/// Factory to create <seealso cref="StatementLocks"/> instances.
	/// </summary>
	public interface StatementLocksFactory
	{
		 /// <summary>
		 /// Initialize this factory with the given {@code locks} and {@code config}. Callers should ensure this method
		 /// is called once during database startup.
		 /// </summary>
		 /// <param name="locks"> the locks to use. </param>
		 /// <param name="config"> the database config that can contain settings interesting for factory implementations. </param>
		 void Initialize( Locks locks, Config config );

		 /// <summary>
		 /// Create new <seealso cref="StatementLocks"/> instance.
		 /// </summary>
		 /// <returns> new statement locks. </returns>
		 StatementLocks NewInstance();
	}

}