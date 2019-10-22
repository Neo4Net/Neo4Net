using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.scenarios
{

	using DiscoveryServiceFactory = Neo4Net.causalclustering.discovery.DiscoveryServiceFactory;
	using HazelcastDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.HazelcastDiscoveryServiceFactory;
	using SharedDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SharedDiscoveryServiceFactory;

	public sealed class EnterpriseDiscoveryServiceType : DiscoveryServiceType
	{
		 public static readonly EnterpriseDiscoveryServiceType Shared = new EnterpriseDiscoveryServiceType( "Shared", InnerEnum.Shared, Neo4Net.causalclustering.discovery.SharedDiscoveryServiceFactory::new );
		 public static readonly EnterpriseDiscoveryServiceType Hazelcast = new EnterpriseDiscoveryServiceType( "Hazelcast", InnerEnum.Hazelcast, Neo4Net.causalclustering.discovery.HazelcastDiscoveryServiceFactory::new );

		 private static readonly IList<EnterpriseDiscoveryServiceType> valueList = new List<EnterpriseDiscoveryServiceType>();

		 static EnterpriseDiscoveryServiceType()
		 {
			 valueList.Add( Shared );
			 valueList.Add( Hazelcast );
		 }

		 public enum InnerEnum
		 {
			 Shared,
			 Hazelcast
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal EnterpriseDiscoveryServiceType( string name, InnerEnum innerEnum, System.Func<Neo4Net.causalclustering.discovery.DiscoveryServiceFactory> supplier )
		 {
			  this._supplier = supplier;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public Neo4Net.causalclustering.discovery.DiscoveryServiceFactory CreateFactory()
		 {
			  return _supplier.get();
		 }

		public static IList<EnterpriseDiscoveryServiceType> values()
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

		public static EnterpriseDiscoveryServiceType valueOf( string name )
		{
			foreach ( EnterpriseDiscoveryServiceType enumInstance in EnterpriseDiscoveryServiceType.valueList )
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