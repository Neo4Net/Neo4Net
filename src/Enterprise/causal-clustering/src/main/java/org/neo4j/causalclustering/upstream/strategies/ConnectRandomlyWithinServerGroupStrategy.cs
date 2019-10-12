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
 */
namespace Neo4Net.causalclustering.upstream.strategies
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class ConnectRandomlyWithinServerGroupStrategy extends org.neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
	public class ConnectRandomlyWithinServerGroupStrategy : UpstreamDatabaseSelectionStrategy
	{
		 public const string IDENTITY = "connect-randomly-within-server-group";
		 private ConnectRandomlyToServerGroupImpl _strategyImpl;

		 public ConnectRandomlyWithinServerGroupStrategy() : base(IDENTITY)
		 {
		 }

		 public override void Init()
		 {
			  IList<string> groups = Config.get( CausalClusteringSettings.server_groups );
			  _strategyImpl = new ConnectRandomlyToServerGroupImpl( groups, TopologyService, Myself );
			  Log.warn( "Upstream selection strategy " + ReadableName + " is deprecated. Consider using " + IDENTITY + " instead." );
		 }

		 public override Optional<MemberId> UpstreamDatabase()
		 {
			  return _strategyImpl.upstreamDatabase();
		 }
	}

}