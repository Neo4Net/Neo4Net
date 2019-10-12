/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup.impl
{
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;

	internal class AddressResolver
	{
		 internal virtual HostnamePort ResolveCorrectHAAddress( Config config, OptionalHostnamePort userProvidedAddress )
		 {
			  HostnamePort defaultValues = ReadDefaultConfigAddressHA( config );
			  return new HostnamePort( userProvidedAddress.Hostname.orElse( defaultValues.Host ), userProvidedAddress.Port.GetValueOrDefault( defaultValues.Port ) );
		 }

		 internal virtual AdvertisedSocketAddress ResolveCorrectCCAddress( Config config, OptionalHostnamePort userProvidedAddress )
		 {
			  AdvertisedSocketAddress defaultValue = ReadDefaultConfigAddressCC( config );
			  return new AdvertisedSocketAddress( userProvidedAddress.Hostname.orElse( defaultValue.Hostname ), userProvidedAddress.Port.GetValueOrDefault( defaultValue.Port ) );
		 }

		 private HostnamePort ReadDefaultConfigAddressHA( Config config )
		 {
			  return config.Get( OnlineBackupSettings.online_backup_server );
		 }

		 private AdvertisedSocketAddress ReadDefaultConfigAddressCC( Config config )
		 {
			  return AsAdvertised( config.Get( OnlineBackupSettings.online_backup_server ) );
		 }

		 private AdvertisedSocketAddress AsAdvertised( HostnamePort listenSocketAddress )
		 {
			  return new AdvertisedSocketAddress( listenSocketAddress.Host, listenSocketAddress.Port );
		 }
	}

}