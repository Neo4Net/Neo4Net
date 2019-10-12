using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using TestHighlyAvailableGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ConcurrentInstanceStartupIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentStartupShouldWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentStartupShouldWork()
		 {
			  // Ensures that the instances don't race to create the test's base directory and only care about their own.
			  TestDirectory.directory( "nothingToSeeHereMoveAlong" );
			  int[] clusterPorts = new int[]{ PortAuthority.allocatePort(), PortAuthority.allocatePort(), PortAuthority.allocatePort() };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String initialHosts = initialHosts(clusterPorts);
			  string initialHosts = initialHosts( clusterPorts );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CyclicBarrier barrier = new java.util.concurrent.CyclicBarrier(clusterPorts.length);
			  CyclicBarrier barrier = new CyclicBarrier( clusterPorts.Length );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Thread> daThreads = new java.util.ArrayList<>(clusterPorts.length);
			  IList<Thread> daThreads = new List<Thread>( clusterPorts.Length );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighlyAvailableGraphDatabase[] dbs = new HighlyAvailableGraphDatabase[clusterPorts.length];
			  HighlyAvailableGraphDatabase[] dbs = new HighlyAvailableGraphDatabase[clusterPorts.Length];

			  for ( int i = 0; i < clusterPorts.Length; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int finalI = i;
					int finalI = i;

					Thread t = new Thread(() =>
					{
					 try
					 {
						  barrier.await();
						  dbs[finalI] = StartDbAtBase( finalI, initialHosts, clusterPorts[finalI] );
					 }
					 catch ( Exception e ) when ( e is InterruptedException || e is BrokenBarrierException )
					 {
						  throw new Exception( e );
					 }
					});
					daThreads.Add( t );
					t.Start();
			  }

			  foreach ( Thread daThread in daThreads )
			  {
					daThread.Join();
			  }

			  bool masterDone = false;

			  foreach ( HighlyAvailableGraphDatabase db in dbs )
			  {
					if ( Db.Master )
					{
						 if ( masterDone )
						 {
							  throw new Exception( "Two masters discovered" );
						 }
						 masterDone = true;
					}
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode();
						 tx.Success();
					}
			  }

			  assertTrue( masterDone );

			  foreach ( HighlyAvailableGraphDatabase db in dbs )
			  {
					Db.shutdown();
			  }
		 }

		 private string InitialHosts( int[] clusterPorts )
		 {
			  return IntStream.of( clusterPorts ).mapToObj( i => "127.0.0.1:" + i ).collect( Collectors.joining( "," ) );
		 }

		 private HighlyAvailableGraphDatabase StartDbAtBase( int i, string initialHosts, int clusterPort )
		 {
			  GraphDatabaseBuilder masterBuilder = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Path(i).AbsoluteFile).setConfig(ClusterSettings.initial_hosts, initialHosts).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.server_id, "" + i).setConfig(HaSettings.HaServer, ":" + PortAuthority.allocatePort()).setConfig(HaSettings.TxPushFactor, "0").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);
			  return ( HighlyAvailableGraphDatabase ) masterBuilder.NewGraphDatabase();
		 }

		 private File Path( int i )
		 {
			  return TestDirectory.databaseDir( i + "" );
		 }
	}

}