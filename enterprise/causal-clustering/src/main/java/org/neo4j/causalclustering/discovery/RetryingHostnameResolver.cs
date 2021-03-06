﻿using System;
using System.Collections.Generic;
using System.Threading;

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

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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