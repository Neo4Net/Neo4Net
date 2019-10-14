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
namespace Neo4Net.causalclustering.routing.load_balancing
{
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Defines the interface for an implementation of the GetServersV2
	/// cluster discovery and load balancing procedure.
	/// </summary>
	public interface LoadBalancingPlugin : LoadBalancingProcessor
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void validate(org.neo4j.kernel.configuration.Config config, org.neo4j.logging.Log log) throws org.neo4j.graphdb.config.InvalidSettingException;
		 void Validate( Config config, Log log );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void init(org.neo4j.causalclustering.discovery.TopologyService topologyService, org.neo4j.causalclustering.core.consensus.LeaderLocator leaderLocator, org.neo4j.logging.LogProvider logProvider, org.neo4j.kernel.configuration.Config config) throws Throwable;
		 void Init( TopologyService topologyService, LeaderLocator leaderLocator, LogProvider logProvider, Config config );

		 string PluginName();
	}

}