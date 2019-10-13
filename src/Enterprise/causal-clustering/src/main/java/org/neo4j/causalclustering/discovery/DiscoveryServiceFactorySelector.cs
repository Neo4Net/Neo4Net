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
namespace Neo4Net.causalclustering.discovery
{
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Config = Neo4Net.Kernel.configuration.Config;

	public abstract class DiscoveryServiceFactorySelector<T> where T : DiscoveryServiceFactory
	{
		 public static DiscoveryImplementation Default = DiscoveryImplementation.Hazelcast;

		 public virtual T Select( Config config )
		 {
			  DiscoveryImplementation middleware = config.Get( CausalClusteringSettings.discovery_implementation );
			  return Select( middleware );
		 }

		 protected internal abstract T Select( DiscoveryImplementation middleware );

		 public enum DiscoveryImplementation
		 {
			  Hazelcast,
			  Akka
		 }
	}

}