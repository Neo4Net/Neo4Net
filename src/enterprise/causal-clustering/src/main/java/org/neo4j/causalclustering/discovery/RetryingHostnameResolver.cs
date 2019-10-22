using System;
using System.Collections.Generic;
using System.Threading;

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

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public abstract class RetryingHostnameResolver : HostnameResolver
	{
		public abstract bool UseOverrides();
		 private readonly int _minResolvedAddresses;
		 private readonly MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> _retryStrategy;

		 internal RetryingHostnameResolver( Config config, MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> retryStrategy )
		 {
			  _minResolvedAddresses = config.Get( CausalClusteringSettings.minimum_core_cluster_size_at_formation );
			  this._retryStrategy = retryStrategy;
		 }

		 internal static MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> DefaultRetryStrategy( Config config, LogProvider logProvider )
		 {
			  long retryIntervalMillis = config.Get( CausalClusteringSettings.discovery_resolution_retry_interval ).toMillis();
			  long clusterBindingTimeout = config.Get( CausalClusteringSettings.discovery_resolution_timeout ).toMillis();
			  long numRetries = ( clusterBindingTimeout / retryIntervalMillis ) + 1;
			  return new MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>>( retryIntervalMillis, numRetries, logProvider, RetryingHostnameResolver.sleep );
		 }

		 public ICollection<AdvertisedSocketAddress> Resolve( AdvertisedSocketAddress advertisedSocketAddress )
		 {
			  return _retryStrategy.apply( advertisedSocketAddress, this.resolveOnce, addrs => addrs.size() >= _minResolvedAddresses );
		 }

		 protected internal abstract ICollection<AdvertisedSocketAddress> ResolveOnce( AdvertisedSocketAddress advertisedSocketAddress );

		 private static void Sleep( long durationInMillis )
		 {
			  try
			  {
					Thread.Sleep( durationInMillis );
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}