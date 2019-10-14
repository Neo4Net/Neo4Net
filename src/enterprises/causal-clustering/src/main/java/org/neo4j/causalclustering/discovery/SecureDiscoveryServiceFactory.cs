/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */
namespace Neo4Net.causalclustering.discovery
{
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

	/// <summary>
	/// Implement an interface to allow for future expansion from just Hazelcast for clustering. I.E. AKKA, etc.
	/// </summary>
	public interface SecureDiscoveryServiceFactory : DiscoveryServiceFactory
	{
		 SslPolicy SslPolicy { set; }
	}


}