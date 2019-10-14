using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core
{

	using DnsHostnameResolver = Neo4Net.causalclustering.discovery.DnsHostnameResolver;
	using DomainNameResolverImpl = Neo4Net.causalclustering.discovery.DomainNameResolverImpl;
	using RemoteMembersResolver = Neo4Net.causalclustering.discovery.RemoteMembersResolver;
	using KubernetesResolver = Neo4Net.causalclustering.discovery.KubernetesResolver;
	using NoOpHostnameResolver = Neo4Net.causalclustering.discovery.NoOpHostnameResolver;
	using SrvHostnameResolver = Neo4Net.causalclustering.discovery.SrvHostnameResolver;
	using SrvRecordResolverImpl = Neo4Net.causalclustering.discovery.SrvRecordResolverImpl;
	using Neo4Net.Graphdb.config;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.kubernetes_label_selector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.kubernetes_service_port_name;

	public sealed class DiscoveryType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DNS((logService, conf) -> org.neo4j.causalclustering.discovery.DnsHostnameResolver.resolver(logService, new org.neo4j.causalclustering.discovery.DomainNameResolverImpl(), conf), initial_discovery_members),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LIST((logService, conf) -> org.neo4j.causalclustering.discovery.NoOpHostnameResolver.resolver(conf), initial_discovery_members),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SRV((logService, conf) -> org.neo4j.causalclustering.discovery.SrvHostnameResolver.resolver(logService, new org.neo4j.causalclustering.discovery.SrvRecordResolverImpl(), conf), initial_discovery_members),

		 public static readonly DiscoveryType K8s = new DiscoveryType( "K8s", InnerEnum.K8s, Neo4Net.causalclustering.discovery.KubernetesResolver.resolver, kubernetes_label_selector, kubernetes_service_port_name );

		 private static readonly IList<DiscoveryType> valueList = new List<DiscoveryType>();

		 static DiscoveryType()
		 {
			 valueList.Add( DNS );
			 valueList.Add( LIST );
			 valueList.Add( SRV );
			 valueList.Add( K8s );
		 }

		 public enum InnerEnum
		 {
			 DNS,
			 LIST,
			 SRV,
			 K8s
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal DiscoveryType( string name, InnerEnum innerEnum, System.Func<Neo4Net.Logging.Internal.LogService, Neo4Net.Kernel.configuration.Config, Neo4Net.causalclustering.discovery.RemoteMembersResolver> resolverSupplier, params Neo4Net.Graphdb.config.Setting<JavaToDotNetGenericWildcard>[] requiredSettings )
		 {
			  this._resolverSupplier = resolverSupplier;
			  this._requiredSettings = Arrays.asList( requiredSettings );

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public Neo4Net.causalclustering.discovery.RemoteMembersResolver GetHostnameResolver( Neo4Net.Logging.Internal.LogService logService, Neo4Net.Kernel.configuration.Config config )
		 {
			  return this._resolverSupplier.apply( logService, config );
		 }

		 public ICollection<Neo4Net.Graphdb.config.Setting<JavaToDotNetGenericWildcard>> RequiredSettings()
		 {
			  return _requiredSettings;
		 }

		public static IList<DiscoveryType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static DiscoveryType valueOf( string name )
		{
			foreach ( DiscoveryType enumInstance in DiscoveryType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}