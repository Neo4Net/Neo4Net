using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using TestRunConditions = Neo4Net.ha.TestRunConditions;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClusterFailoverIT
	public class ClusterFailoverIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.LoggerRule logger = new org.Neo4Net.test.rule.LoggerRule();
		 public LoggerRule Logger = new LoggerRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory dir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Dir = TestDirectory.testDirectory();

		 // parameters
		 private int _clusterSize;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "clusterSize:{0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { 3 },
				  new object[] { 4 },
				  new object[] { 5 },
				  new object[] { 6 },
				  new object[] { 7 }
			  });
		 }

		 public ClusterFailoverIT( int clusterSize )
		 {
			  this._clusterSize = clusterSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testFailOver(int clusterSize) throws Throwable
		 private void TestFailOver( int clusterSize )
		 {
			  // given
			  ClusterManager clusterManager = ( new ClusterManager.Builder() ).withRootDirectory(Dir.cleanDirectory("failover")).withCluster(ClusterManager.clusterOfSize(clusterSize)).build();

			  clusterManager.Start();
			  ClusterManager.ManagedCluster cluster = clusterManager.Cluster;

			  cluster.Await( ClusterManager.allSeesAllAsAvailable() );
			  HighlyAvailableGraphDatabase oldMaster = cluster.Master;

			  // When
			  long start = System.nanoTime();
			  ClusterManager.RepairKit repairKit = cluster.Fail( oldMaster );
			  Logger.Logger.warning( "Shut down master" );

			  // Then
			  cluster.Await( ClusterManager.masterAvailable( oldMaster ) );
			  long end = System.nanoTime();

			  Logger.Logger.warning( "Failover took:" + ( end - start ) / 1000000 + "ms" );

			  repairKit.Repair();
			  Thread.Sleep( 3000 ); // give repaired instance chance to cleanly rejoin and exit faster

			  clusterManager.SafeShutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailOver() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailOver()
		 {
			  assumeTrue( TestRunConditions.shouldRunAtClusterSize( _clusterSize ) );
			  TestFailOver( _clusterSize );
		 }
	}

}