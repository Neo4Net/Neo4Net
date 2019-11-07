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
	using Parameterized = org.junit.runners.Parameterized;


	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.discovery.IpFamily.IPV4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.discovery.IpFamily.IPV6;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.scenarios.EnterpriseDiscoveryServiceType.HAZELCAST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.scenarios.EnterpriseDiscoveryServiceType.SHARED;

	public class EnterpriseClusterIpFamilyIT : BaseClusterIpFamilyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} {1} useWildcard={2}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { SHARED, IPV4, false },
				  new object[] { SHARED, IPV6, true },
				  new object[] { HAZELCAST, IPV4, false },
				  new object[] { HAZELCAST, IPV6, false },
				  new object[] { HAZELCAST, IPV4, true },
				  new object[] { HAZELCAST, IPV6, true }
			  });
		 }

		 public EnterpriseClusterIpFamilyIT( DiscoveryServiceType discoveryServiceFactory, IpFamily ipFamily, bool useWildcard ) : base( discoveryServiceFactory, ipFamily, useWildcard )
		 {
		 }
	}

}