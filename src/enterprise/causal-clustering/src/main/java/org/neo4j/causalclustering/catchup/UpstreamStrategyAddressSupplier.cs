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
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using UpstreamDatabaseSelectionException = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using Neo4Net.Functions;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	public class UpstreamStrategyAddressSupplier : ThrowingSupplier<AdvertisedSocketAddress, CatchupAddressResolutionException>
	{
		 private readonly UpstreamDatabaseStrategySelector _strategySelector;
		 private readonly TopologyService _topologyService;

		 internal UpstreamStrategyAddressSupplier( UpstreamDatabaseStrategySelector strategySelector, TopologyService topologyService )
		 {
			  this._strategySelector = strategySelector;
			  this._topologyService = topologyService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.helpers.AdvertisedSocketAddress get() throws CatchupAddressResolutionException
		 public override AdvertisedSocketAddress Get()
		 {
			  try
			  {
					MemberId upstreamMember = _strategySelector.bestUpstreamDatabase();
					return _topologyService.findCatchupAddress( upstreamMember ).orElseThrow( () => new CatchupAddressResolutionException(upstreamMember) );
			  }
			  catch ( UpstreamDatabaseSelectionException e )
			  {
					throw new CatchupAddressResolutionException( e );
			  }
		 }
	}

}