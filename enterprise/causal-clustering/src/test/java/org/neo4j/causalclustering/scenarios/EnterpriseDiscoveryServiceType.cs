﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.scenarios
{

	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using HazelcastDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.HazelcastDiscoveryServiceFactory;
	using SharedDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory;

	public sealed class EnterpriseDiscoveryServiceType : DiscoveryServiceType
	{
		 public static readonly EnterpriseDiscoveryServiceType Shared = new EnterpriseDiscoveryServiceType( "Shared", InnerEnum.Shared, Org.Neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory::new );
		 public static readonly EnterpriseDiscoveryServiceType Hazelcast = new EnterpriseDiscoveryServiceType( "Hazelcast", InnerEnum.Hazelcast, Org.Neo4j.causalclustering.discovery.HazelcastDiscoveryServiceFactory::new );

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

		 internal EnterpriseDiscoveryServiceType( string name, InnerEnum innerEnum, System.Func<Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory> supplier )
		 {
			  this._supplier = supplier;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory CreateFactory()
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