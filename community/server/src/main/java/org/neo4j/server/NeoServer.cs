using System.Collections.Generic;

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
namespace Org.Neo4j.Server
{

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Database = Org.Neo4j.Server.database.Database;
	using PluginManager = Org.Neo4j.Server.plugins.PluginManager;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using TransactionRegistry = Org.Neo4j.Server.rest.transactional.TransactionRegistry;

	public interface NeoServer
	{
		 void Start();

		 void Stop();

		 Config Config { get; }

		 Database Database { get; }

		 TransactionRegistry TransactionRegistry { get; }

		 PluginManager ExtensionManager { get; }

		 URI BaseUri();

		 IEnumerable<AdvertisableService> Services { get; }
	}

}