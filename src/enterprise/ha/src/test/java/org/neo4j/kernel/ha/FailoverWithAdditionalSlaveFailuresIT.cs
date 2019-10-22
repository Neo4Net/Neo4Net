using System.Collections.Generic;
using System.Diagnostics;
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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using TestRunConditions = Neo4Net.ha.TestRunConditions;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using RepairKit = Neo4Net.Kernel.impl.ha.ClusterManager.RepairKit;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FailoverWithAdditionalSlaveFailuresIT
	public class FailoverWithAdditionalSlaveFailuresIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TestName name = new org.junit.rules.TestName();
		 public TestName Name = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.LoggerRule logger = new org.Neo4Net.test.rule.LoggerRule();
		 public LoggerRule Logger = new LoggerRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory dir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Dir = TestDirectory.testDirectory();

		 // parameters
		 private int _clusterSize;
		 private int[] _slavesToFail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[]
				  {
					  5, new int[]{ 1 }
				  },
				  new object[]
				  {
					  5, new int[]{ 2 }
				  },
				  new object[]
				  {
					  5, new int[]{ 3 }
				  },
				  new object[]
				  {
					  5, new int[]{ 4 }
				  },
				  new object[]
				  {
					  6, new int[]{ 1 }
				  },
				  new object[]
				  {
					  6, new int[]{ 3 }
				  },
				  new object[]
				  {
					  6, new int[]{ 5 }
				  },
				  new object[]
				  {
					  7, new int[]{ 1, 2 }
				  },
				  new object[]
				  {
					  7, new int[]{ 3, 4 }
				  },
				  new object[]
				  {
					  7, new int[]{ 5, 6 }
				  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void shouldRun()
		 public virtual void ShouldRun()
		 {
			  assumeTrue( TestRunConditions.shouldRunAtClusterSize( _clusterSize ) );
		 }

		 public FailoverWithAdditionalSlaveFailuresIT( int clusterSize, int[] slavesToFail )
		 {
			  this._clusterSize = clusterSize;
			  this._slavesToFail = slavesToFail;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailoverWithAdditionalSlave() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailoverWithAdditionalSlave()
		 {
			  TestFailoverWithAdditionalSlave( _clusterSize, _slavesToFail );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testFailoverWithAdditionalSlave(int clusterSize, int[] slaveIndexes) throws Throwable
		 private void TestFailoverWithAdditionalSlave( int clusterSize, int[] slaveIndexes )
		 {
			  File root = Dir.cleanDirectory( "testcluster_" + Name.MethodName );
			  ClusterManager manager = ( new ClusterManager.Builder() ).withRootDirectory(root).withCluster(ClusterManager.clusterOfSize(clusterSize)).build();

			  try
			  {
					manager.Start();
					ClusterManager.ManagedCluster cluster = manager.Cluster;

					cluster.Await( allSeesAllAsAvailable() );
					cluster.Await( masterAvailable() );

					ICollection<HighlyAvailableGraphDatabase> failed = new List<HighlyAvailableGraphDatabase>();
					ICollection<ClusterManager.RepairKit> repairKits = new List<ClusterManager.RepairKit>();

					foreach ( int slaveIndex in slaveIndexes )
					{
						 HighlyAvailableGraphDatabase nthSlave = GetNthSlave( cluster, slaveIndex );
						 failed.Add( nthSlave );
						 ClusterManager.RepairKit repairKit = cluster.Fail( nthSlave );
						 repairKits.Add( repairKit );
					}

					HighlyAvailableGraphDatabase oldMaster = cluster.Master;
					failed.Add( oldMaster );
					repairKits.Add( cluster.Fail( oldMaster ) );

					cluster.Await( masterAvailable( ToArray( failed ) ) );

					foreach ( ClusterManager.RepairKit repairKit in repairKits )
					{
						 repairKit.Repair();
					}

					Thread.Sleep( 3000 ); // give repaired instances a chance to cleanly rejoin and exit faster
			  }
			  finally
			  {
					manager.SafeShutdown();
			  }
		 }

		 private HighlyAvailableGraphDatabase GetNthSlave( ClusterManager.ManagedCluster cluster, int slaveOrder )
		 {
			  Debug.Assert( slaveOrder > 0 );
			  HighlyAvailableGraphDatabase slave = null;

			  IList<HighlyAvailableGraphDatabase> excluded = new List<HighlyAvailableGraphDatabase>();
			  while ( slaveOrder-- > 0 )
			  {
					slave = cluster.GetAnySlave( ToArray( excluded ) );
					excluded.Add( slave );
			  }

			  return slave;
		 }

		 private HighlyAvailableGraphDatabase[] ToArray( ICollection<HighlyAvailableGraphDatabase> excluded )
		 {
			  return excluded.toArray( new HighlyAvailableGraphDatabase[excluded.Count] );
		 }
	}

}