using System.Collections.Generic;

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
 *
 * See: https://github.com/neo4j/neo4j/blob/3.5.0-beta03/enterprise/causal-clustering/src/main/java/org/neo4j/causalclustering/discovery/InitialDiscoveryMembersResolver.java
 */
namespace Org.Neo4j.causalclustering.discovery
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Org.Neo4j.Helpers.SocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;

	public class InitialDiscoveryMembersResolver : RemoteMembersResolver
	{

		 private readonly HostnameResolver _hostnameResolver;
		 private readonly ICollection<AdvertisedSocketAddress> _advertisedSocketAddresses;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly System.Collections.IComparer _advertisedSockedAddressComparator = System.Collections.IComparer.comparing( SocketAddress::getHostname ).thenComparingInt( SocketAddress::getPort );

		 /// <param name="hostnameResolver"> </param>
		 /// <param name="config"> </param>
		 public InitialDiscoveryMembersResolver( HostnameResolver hostnameResolver, Config config )
		 {
			  this._hostnameResolver = hostnameResolver;
			  this._advertisedSocketAddresses = ( System.Collections.IList ) config.Get( CausalClusteringSettings.initial_discovery_members );
		 }

		 /// 
		 /// <param name="transform"> </param>
		 /// @param <T>
		 /// @return </param>
		 public override ICollection<T> Resolve<T>( System.Func<AdvertisedSocketAddress, T> transform )
		 {
			  return _advertisedSocketAddresses.stream().flatMap(raw => _hostnameResolver.resolve(raw).stream()).map(transform).collect(Collectors.toSet());
		 }

		 /// <summary>
		 /// @return
		 /// </summary>
		 public static System.Collections.IComparer AdvertisedSocketAddressComparator()
		 {
			  return _advertisedSockedAddressComparator;
		 }

		 /// <summary>
		 /// Override the default that is provided in the HostnameResolver.java interface.
		 /// 
		 /// @return
		 /// </summary>
		 public virtual bool UseOverrides()
		 {
			  return this._hostnameResolver.useOverrides();
		 }
	}

}