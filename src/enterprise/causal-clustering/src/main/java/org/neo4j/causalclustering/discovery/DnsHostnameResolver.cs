using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.discovery
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

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