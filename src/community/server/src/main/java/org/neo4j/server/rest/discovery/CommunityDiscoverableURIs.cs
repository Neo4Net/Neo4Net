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
namespace Neo4Net.Server.rest.discovery
{
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.discovery.DiscoverableURIs.Precedence.NORMAL;

	public class CommunityDiscoverableURIs
	{
		 /// <summary>
		 /// URIs exposed at the root HTTP endpoint, to help clients discover the rest of the service.
		 /// </summary>
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static DiscoverableURIs CommunityDiscoverableURIsConflict( Config config, ConnectorPortRegister portRegister )
		 {
			  return ( new DiscoverableURIs.Builder() ).add("data", config.Get(ServerSettings.rest_api_path).Path + "/", NORMAL).add("management", config.Get(ServerSettings.management_api_path).Path + "/", NORMAL).addBoltConnectorFromConfig("bolt", "bolt", config, ServerSettings.bolt_discoverable_address, portRegister).build();
		 }
	}

}