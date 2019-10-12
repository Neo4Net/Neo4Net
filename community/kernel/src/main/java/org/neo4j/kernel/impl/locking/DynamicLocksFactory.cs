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
	using Service = Org.Neo4j.Helpers.Service;

	/// <summary>
	/// Factory for lock managers that can be loaded over service loading.
	/// </summary>
	public abstract class DynamicLocksFactory : Service, LocksFactory
	{
		public abstract Locks NewInstance( Org.Neo4j.Kernel.configuration.Config config, java.time.Clock clock, Org.Neo4j.Storageengine.Api.@lock.ResourceType[] resourceTypes );
		 public DynamicLocksFactory( string key, params string[] altKeys ) : base( key, altKeys )
		 {
		 }
	}

}