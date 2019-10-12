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
namespace Org.Neo4j.Harness
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

	public class CausalClusterInProcessRunnerIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public static readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBootAndShutdownCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBootAndShutdownCluster()
		 {
			  Path clusterPath = TestDirectory.directory().toPath();
			  CausalClusterInProcessBuilder.PortPickingStrategy portPickingStrategy = new PortAuthorityPortPickingStrategy();

			  CausalClusterInProcessBuilder.CausalCluster cluster = CausalClusterInProcessBuilder.Init().withCores(3).withReplicas(3).withLogger(NullLogProvider.Instance).atPath(clusterPath).withOptionalPortsStrategy(portPickingStrategy).build();

			  cluster.Boot();
			  cluster.Shutdown();
		 }
	}

}