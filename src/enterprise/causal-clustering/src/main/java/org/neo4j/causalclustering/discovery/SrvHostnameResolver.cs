using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.discovery
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

	public class SrvHostnameResolver : RetryingHostnameResolver
	{
		 private readonly Log _userLog;
		 private readonly Log _log;
		 private readonly SrvRecordResolver _srvRecordResolver;

		 public static RemoteMembersResolver Resolver( LogService logService, SrvRecordResolver srvHostnameResolver, Config config )
		 {
			  SrvHostnameResolver hostnameResolver = new SrvHostnameResolver( logService, srvHostnameResolver, config, DefaultRetryStrategy( config, logService.InternalLogProvider ) );
			  return new InitialDiscoveryMembersResolver( hostnameResolver, config );
		 }

		 internal SrvHostnameResolver( LogService logService, SrvRecordResolver srvRecordResolver, Config config, MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> retryStrategy ) : base( config, retryStrategy )
		 {
			  _log = logService.GetInternalLog( this.GetType() );
			  _userLog = logService.GetUserLog( this.GetType() );
			  this._srvRecordResolver = srvRecordResolver;
		 }

		 public override ICollection<AdvertisedSocketAddress> ResolveOnce( AdvertisedSocketAddress initialAddress )
		 {
			  try
			  {
					ISet<AdvertisedSocketAddress> addresses = _srvRecordResolver.resolveSrvRecord( initialAddress.Hostname ).map( srvRecord => new AdvertisedSocketAddress( srvRecord.host, srvRecord.port ) ).collect( Collectors.toSet() );

					_userLog.info( "Resolved initial host '%s' to %s", initialAddress, addresses );

					if ( addresses.Count == 0 )
					{
						 _log.error( "Failed to resolve srv records for '%s'", initialAddress.Hostname );
					}

					return addresses;
			  }
			  catch ( NamingException e )
			  {
					_log.error( string.Format( "Failed to resolve srv records for '{0}'", initialAddress.Hostname ), e );
					return Collections.emptySet();
			  }
		 }
	}

}