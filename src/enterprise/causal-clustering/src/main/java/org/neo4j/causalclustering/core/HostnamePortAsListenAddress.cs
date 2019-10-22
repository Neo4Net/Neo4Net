/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core
{

	using Neo4Net.GraphDb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;

	internal class HostnamePortAsListenAddress
	{
		 private static readonly Pattern _portRange = Pattern.compile( "(:\\d+)(-\\d+)?$" );

		 internal static ListenSocketAddress Resolve( Config config, Setting<HostnamePort> setting )
		 {
			  System.Func<string, ListenSocketAddress> resolveFn = Settings.LISTEN_SOCKET_ADDRESS.compose( HostnamePortAsListenAddress.stripPortRange );
			  return config.Get( Settings.setting( setting.Name(), resolveFn, setting.DefaultValue ) );
		 }

		 private static string StripPortRange( string address )
		 {
			  Matcher m = _portRange.matcher( address );
			  return m.find() ? m.replaceAll("$1") : address;
		 }

	}

}