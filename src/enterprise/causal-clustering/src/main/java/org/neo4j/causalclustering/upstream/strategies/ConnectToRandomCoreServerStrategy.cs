﻿using System;

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
namespace Neo4Net.causalclustering.upstream.strategies
{

	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class ConnectToRandomCoreServerStrategy extends org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
	public class ConnectToRandomCoreServerStrategy : UpstreamDatabaseSelectionStrategy
	{
		 public const string IDENTITY = "connect-to-random-core-server";
		 private readonly Random _random = new Random();

		 public ConnectToRandomCoreServerStrategy() : base(IDENTITY)
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.Neo4Net.causalclustering.identity.MemberId> upstreamDatabase() throws org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException
		 public override Optional<MemberId> UpstreamDatabase()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.discovery.CoreTopology coreTopology = topologyService.localCoreServers();
			  CoreTopology coreTopology = TopologyService.localCoreServers();

			  if ( coreTopology.Members().Count == 0 )
			  {
					throw new UpstreamDatabaseSelectionException( "No core servers available" );
			  }

			  int skippedServers = _random.Next( coreTopology.Members().Count );

			  return coreTopology.Members().Keys.Skip(skippedServers).First();
		 }
	}

}