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
namespace Neo4Net.Server.rest
{
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using EnterpriseServerSettings = Neo4Net.Server.enterprise.EnterpriseServerSettings;
	using DiscoverableURIs = Neo4Net.Server.rest.discovery.DiscoverableURIs;
	using CommunityDiscoverableURIs = Neo4Net.Server.rest.discovery.CommunityDiscoverableURIs;
	using Builder = Neo4Net.Server.rest.discovery.DiscoverableURIs.Builder;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.discovery.CommunityDiscoverableURIs.communityDiscoverableURIs;

	public class EnterpriseDiscoverableURIs
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static DiscoverableURIs EnterpriseDiscoverableURIsConflict( Config config, ConnectorPortRegister ports )
		 {
			  DiscoverableURIs uris = communityDiscoverableURIs( config, ports );
			  if ( config.Get( EnterpriseEditionSettings.mode ) == EnterpriseEditionSettings.Mode.CORE )
			  {
					// DiscoverableURIs
					//       .discoverableBoltUri( "bolt+routing", config,
					//              EnterpriseServerSettings.bolt_routing_discoverable_address, ports )
					//     .ifPresent( uri -> uris.addAbsolute( "bolt_routing", uri ) );
					return ( new DiscoverableURIs.Builder( CommunityDiscoverableURIs.communityDiscoverableURIs( config, ports ) ) ).addBoltConnectorFromConfig( "bolt_routing", "bolt+routing", config, EnterpriseServerSettings.bolt_routing_discoverable_address, ports ).build();
			  }
			  return uris;
		 }
	}

}