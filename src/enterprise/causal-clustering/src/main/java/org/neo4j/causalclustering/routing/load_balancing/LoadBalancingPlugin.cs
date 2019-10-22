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
namespace Neo4Net.causalclustering.routing.load_balancing
{
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
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
//ORIGINAL LINE: void validate(org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.Log log) throws org.Neo4Net.graphdb.config.InvalidSettingException;
		 void Validate( Config config, Log log );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void init(org.Neo4Net.causalclustering.discovery.TopologyService topologyService, org.Neo4Net.causalclustering.core.consensus.LeaderLocator leaderLocator, org.Neo4Net.logging.LogProvider logProvider, org.Neo4Net.kernel.configuration.Config config) throws Throwable;
		 void Init( TopologyService topologyService, LeaderLocator leaderLocator, LogProvider logProvider, Config config );

		 string PluginName();
	}

}