using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.scenarios
{
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.EnterpriseDiscoveryServiceType.SHARED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.EnterpriseDiscoveryServiceType.HAZELCAST;

	public class EnterpriseMultiClusteringIT : BaseMultiClusteringIT
	{
		 public EnterpriseMultiClusteringIT( string ignoredName, int numCores, int numReplicas, ISet<string> dbNames, DiscoveryServiceType discoveryServiceType ) : base( ignoredName, numCores, numReplicas, dbNames, discoveryServiceType )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { "[shared discovery, 6 core hosts, 2 databases]", 6, 0, DbNames_2, SHARED },
				  new object[] { "[hazelcast discovery, 6 core hosts, 2 databases]", 6, 0, DbNames_2, HAZELCAST },
				  new object[] { "[shared discovery, 5 core hosts, 1 database]", 5, 0, DbNames_1, SHARED },
				  new object[] { "[hazelcast discovery, 5 core hosts, 2 databases]", 5, 0, DbNames_2, HAZELCAST },
				  new object[] { "[hazelcast discovery, 9 core hosts, 3 read replicas, 3 databases]", 9, 3, DbNames_3, HAZELCAST },
				  new object[] { "[shared discovery, 8 core hosts, 2 read replicas, 3 databases]", 8, 2, DbNames_3, SHARED }
			  });
		 }
	}

}