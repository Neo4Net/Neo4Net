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
namespace Neo4Net.causalclustering.catchup
{
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	/// <summary>
	/// Address provider for catchup client.
	/// </summary>
	public interface ICatchupAddressProvider
	{
		 /// <returns> The address to the primary location where up to date requests are required. For a cluster aware provider the obvious choice would be the
		 /// leader address. </returns>
		 /// <exception cref="CatchupAddressResolutionException"> if the provider was unable to find an address to this location. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.helpers.AdvertisedSocketAddress primary() throws CatchupAddressResolutionException;
		 AdvertisedSocketAddress Primary();

		 /// <returns> The address to a secondary location that are not required to be up to date. If there are multiple secondary locations it is recommended to
		 /// do some simple load balancing for returned addresses. This is to avoid re-sending failed requests to the same instance immediately. </returns>
		 /// <exception cref="CatchupAddressResolutionException"> if the provider was unable to find an address to this location. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.helpers.AdvertisedSocketAddress secondary() throws CatchupAddressResolutionException;
		 AdvertisedSocketAddress Secondary();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CatchupAddressProvider fromSingleAddress(Neo4Net.helpers.AdvertisedSocketAddress advertisedSocketAddress)
	//	 {
	//		  return new SingleAddressProvider(advertisedSocketAddress);
	//	 }

		 /// <summary>
		 /// Uses leader address as primary and given upstream strategy as secondary address.
		 /// </summary>
	}

	 public class CatchupAddressProvider_SingleAddressProvider : CatchupAddressProvider
	 {
		  internal readonly AdvertisedSocketAddress SocketAddress;

		  public CatchupAddressProvider_SingleAddressProvider( AdvertisedSocketAddress socketAddress )
		  {
				this.SocketAddress = socketAddress;
		  }

		  public override AdvertisedSocketAddress Primary()
		  {
				return SocketAddress;
		  }

		  public override AdvertisedSocketAddress Secondary()
		  {
				return SocketAddress;
		  }
	 }

	 public class CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider : CatchupAddressProvider
	 {
		  internal readonly LeaderLocator LeaderLocator;
		  internal readonly TopologyService TopologyService;
		  internal UpstreamStrategyAddressSupplier SecondaryUpstreamStrategyAddressSupplier;

		  public CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider( LeaderLocator leaderLocator, TopologyService topologyService, UpstreamDatabaseStrategySelector strategySelector )
		  {
				this.LeaderLocator = leaderLocator;
				this.TopologyService = topologyService;
				this.SecondaryUpstreamStrategyAddressSupplier = new UpstreamStrategyAddressSupplier( strategySelector, topologyService );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.helpers.AdvertisedSocketAddress primary() throws CatchupAddressResolutionException
		  public override AdvertisedSocketAddress Primary()
		  {
				try
				{
					 MemberId leadMember = LeaderLocator.Leader;
					 return TopologyService.findCatchupAddress( leadMember ).orElseThrow( () => new CatchupAddressResolutionException(leadMember) );
				}
				catch ( NoLeaderFoundException e )
				{
					 throw new CatchupAddressResolutionException( e );
				}
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.helpers.AdvertisedSocketAddress secondary() throws CatchupAddressResolutionException
		  public override AdvertisedSocketAddress Secondary()
		  {
				return SecondaryUpstreamStrategyAddressSupplier.get();
		  }
	 }

}