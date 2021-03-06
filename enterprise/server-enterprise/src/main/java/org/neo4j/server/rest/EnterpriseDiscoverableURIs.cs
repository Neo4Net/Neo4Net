﻿/*
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
namespace Org.Neo4j.Server.rest
{
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using EnterpriseServerSettings = Org.Neo4j.Server.enterprise.EnterpriseServerSettings;
	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;
	using CommunityDiscoverableURIs = Org.Neo4j.Server.rest.discovery.CommunityDiscoverableURIs;
	using Builder = Org.Neo4j.Server.rest.discovery.DiscoverableURIs.Builder;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.CommunityDiscoverableURIs.communityDiscoverableURIs;

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