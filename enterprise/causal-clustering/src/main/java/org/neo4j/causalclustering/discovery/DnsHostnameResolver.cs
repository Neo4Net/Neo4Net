﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{

	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public class DnsHostnameResolver : RetryingHostnameResolver
	{
		 private readonly Log _userLog;
		 private readonly Log _log;
		 private readonly DomainNameResolver _domainNameResolver;

		 public static RemoteMembersResolver Resolver( LogService logService, DomainNameResolver domainNameResolver, Config config )
		 {
			  DnsHostnameResolver hostnameResolver = new DnsHostnameResolver( logService, domainNameResolver, config, DefaultRetryStrategy( config, logService.InternalLogProvider ) );
			  return new InitialDiscoveryMembersResolver( hostnameResolver, config );
		 }

		 internal DnsHostnameResolver( LogService logService, DomainNameResolver domainNameResolver, Config config, MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> retryStrategy ) : base( config, retryStrategy )
		 {
			  _log = logService.GetInternalLog( this.GetType() );
			  _userLog = logService.GetUserLog( this.GetType() );
			  this._domainNameResolver = domainNameResolver;
		 }

		 protected internal override ICollection<AdvertisedSocketAddress> ResolveOnce( AdvertisedSocketAddress initialAddress )
		 {
			  ISet<AdvertisedSocketAddress> addresses = new HashSet<AdvertisedSocketAddress>();
			  InetAddress[] ipAddresses;
			  ipAddresses = _domainNameResolver.resolveDomainName( initialAddress.Hostname );
			  if ( ipAddresses.Length == 0 )
			  {
					_log.error( "Failed to resolve host '%s'", initialAddress.Hostname );
			  }

			  foreach ( InetAddress ipAddress in ipAddresses )
			  {
					addresses.Add( new AdvertisedSocketAddress( ipAddress.HostAddress, initialAddress.Port ) );
			  }

			  _userLog.info( "Resolved initial host '%s' to %s", initialAddress, addresses );
			  return addresses;
		 }

	}

}