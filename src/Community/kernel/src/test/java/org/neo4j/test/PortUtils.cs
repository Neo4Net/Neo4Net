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
namespace Neo4Net.Test
{
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;

	public sealed class PortUtils
	{
		 private PortUtils()
		 {
			  // nop
		 }

		 public static int GetBoltPort( GraphDatabaseService db )
		 {
			  return GetConnectorAddress( ( GraphDatabaseAPI ) db, "bolt" ).Port;
		 }

		 public static HostnamePort GetConnectorAddress( GraphDatabaseAPI db, string connectorKey )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.configuration.ConnectorPortRegister portRegister = db.getDependencyResolver().resolveDependency(org.neo4j.kernel.configuration.ConnectorPortRegister.class);
			  ConnectorPortRegister portRegister = Db.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
			  return requireNonNull( portRegister.GetLocalAddress( connectorKey ), "Connector not found: " + connectorKey );
		 }
	}

}