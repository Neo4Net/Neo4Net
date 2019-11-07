using System;
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


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using MemberIsAvailable = Neo4Net.cluster.member.paxos.MemberIsAvailable;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using ClusterMember = Neo4Net.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using ProcessStreamHandler = Neo4Net.Test.ProcessStreamHandler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.UNKNOWN;

	public class HardKillIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private ProcessStreamHandler _processHandler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasterSwitchHappensOnKillMinus9() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMasterSwitchHappensOnKillMinus9()
		 {
			  Process proc = null;
			  HighlyAvailableGraphDatabase dbWithId2 = null;
			  HighlyAvailableGraphDatabase dbWithId3 = null;
			  HighlyAvailableGraphDatabase oldMaster = null;

			  int clusterPort1 = PortAuthority.allocatePort();
			  int clusterPort2 = PortAuthority.allocatePort();
			  int clusterPort3 = PortAuthority.allocatePort();

			  int haPort1 = PortAuthority.allocatePort();
			  int haPort2 = PortAuthority.allocatePort();
			  int haPort3 = PortAuthority.allocatePort();

			  try
			  {
					proc = Run( 1, clusterPort1, haPort1, clusterPort1, clusterPort2 );
					dbWithId2 = StartDb( 2, Path( 2 ), clusterPort2, haPort2, clusterPort1, clusterPort2 );
					dbWithId3 = StartDb( 3, Path( 3 ), clusterPort3, haPort3, clusterPort1, clusterPort2 );

					WaitUntilAllProperlyAvailable( dbWithId2, 1, MASTER, 2, SLAVE, 3, SLAVE );
					WaitUntilAllProperlyAvailable( dbWithId3, 1, MASTER, 2, SLAVE, 3, SLAVE );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch newMasterAvailableLatch = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent newMasterAvailableLatch = new System.Threading.CountdownEvent( 1 );
					dbWithId2.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).addAtomicBroadcastListener(value =>
					{
								try
								{
									 object @event = ( new AtomicBroadcastSerializer( new ObjectStreamFactory(), new ObjectStreamFactory() ) ).receive(value);
									 if ( @event is MemberIsAvailable )
									 {
										  if ( HighAvailabilityModeSwitcher.MASTER.Equals( ( ( MemberIsAvailable ) @event ).Role ) )
										  {
												newMasterAvailableLatch.Signal();
										  }
									 }
								}
								catch ( Exception e )
								{
									 fail( e.ToString() );
								}
					});
					proc.destroy();
					proc = null;

					assertTrue( newMasterAvailableLatch.await( 60, SECONDS ) );

					assertTrue( dbWithId2.Master );
					assertTrue( !dbWithId3.Master );

					// Ensure that everyone has marked the killed instance as failed, otherwise it cannot rejoin
					WaitUntilAllProperlyAvailable( dbWithId2, 1, UNKNOWN, 2, MASTER, 3, SLAVE );
					WaitUntilAllProperlyAvailable( dbWithId3, 1, UNKNOWN, 2, MASTER, 3, SLAVE );

					oldMaster = StartDb( 1, Path( 1 ), clusterPort1, haPort1, clusterPort1, clusterPort2 );
					long oldMasterNode = CreateNamedNode( oldMaster, "Old master" );
					assertEquals( oldMasterNode, GetNamedNode( dbWithId2, "Old master" ) );
			  }
			  finally
			  {
					if ( proc != null )
					{
						 proc.destroy();
					}
					if ( oldMaster != null )
					{
						 oldMaster.Shutdown();
					}
					if ( dbWithId2 != null )
					{
						 dbWithId2.Shutdown();
					}
					if ( dbWithId3 != null )
					{
						 dbWithId3.Shutdown();
					}
			  }
		 }

		 private static long GetNamedNode( HighlyAvailableGraphDatabase db, string name )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 if ( name.Equals( node.GetProperty( "name", null ) ) )
						 {
							  return node.Id;
						 }
					}
					fail( "Couldn't find named node '" + name + "' at " + db );
					// The lone above will prevent this return from happening
					return -1;
			  }
		 }

		 private static long CreateNamedNode( HighlyAvailableGraphDatabase db, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "name", name );
					tx.Success();
					return node.Id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Process run(int machineId, int clusterPort, int haPort, int initialHost1, int initialHost2) throws java.io.IOException
		 private Process Run( int machineId, int clusterPort, int haPort, int initialHost1, int initialHost2 )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  IList<string> allArgs = new List<string>( Arrays.asList( "java", "-cp", System.getProperty( "java.class.path" ), "-Djava.awt.headless=true", typeof( HardKillIT ).FullName ) );
			  allArgs.Add( "" + machineId );
			  allArgs.Add( Path( machineId ).AbsolutePath );
			  allArgs.Add( clusterPort.ToString() );
			  allArgs.Add( haPort.ToString() );
			  allArgs.Add( initialHost1.ToString() );
			  allArgs.Add( initialHost2.ToString() );

			  Process process = Runtime.Runtime.exec( allArgs.ToArray() );
			  _processHandler = new ProcessStreamHandler( process, false );
			  _processHandler.launch();
			  return process;
		 }

		 /*
		  * Used to launch the master instance
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws InterruptedException
		 public static void Main( string[] args )
		 {
			  int serverId = int.Parse( args[0] );
			  File path = new File( args[1] );
			  int clusterPort = int.Parse( args[2] );
			  int haPort = int.Parse( args[3] );
			  int initialHost1 = int.Parse( args[4] );
			  int initialHost2 = int.Parse( args[5] );
			  HighlyAvailableGraphDatabase db = StartDb( serverId, path, clusterPort, haPort, initialHost1, initialHost2 );
			  WaitUntilAllProperlyAvailable( db, 1, MASTER, 2, SLAVE, 3, SLAVE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void waitUntilAllProperlyAvailable(HighlyAvailableGraphDatabase db, Object... expected) throws InterruptedException
		 private static void WaitUntilAllProperlyAvailable( HighlyAvailableGraphDatabase db, params object[] expected )
		 {
			  ClusterMembers members = Db.DependencyResolver.resolveDependency( typeof( ClusterMembers ) );
			  long endTime = currentTimeMillis() + SECONDS.toMillis(120);
			  IDictionary<int, string> expectedStates = ToExpectedStatesMap( expected );
			  while ( currentTimeMillis() < endTime && !AllMembersAreAsExpected(members, expectedStates) )
			  {
					Thread.Sleep( 100 );
			  }
			  if ( !AllMembersAreAsExpected( members, expectedStates ) )
			  {
					throw new System.InvalidOperationException( "Not all members available, according to " + db );
			  }
		 }

		 private static bool AllMembersAreAsExpected( ClusterMembers members, IDictionary<int, string> expectedStates )
		 {
			  int count = 0;
			  foreach ( ClusterMember member in members.Members )
			  {
					int serverId = member.InstanceId.toIntegerIndex();
					string expectedRole = expectedStates[serverId];
					bool whatExpected = ( expectedRole.Equals( UNKNOWN ) || member.Alive ) && member.HARole.Equals( expectedRole );
					if ( !whatExpected )
					{
						 return false;
					}
					count++;
			  }
			  return count == expectedStates.Count;
		 }

		 private static IDictionary<int, string> ToExpectedStatesMap( object[] expected )
		 {
			  IDictionary<int, string> expectedStates = new Dictionary<int, string>();
			  for ( int i = 0; i < expected.Length; )
			  {
					expectedStates[( int? ) expected[i++]] = ( string ) expected[i++];
			  }
			  return expectedStates;
		 }

		 private static HighlyAvailableGraphDatabase StartDb( int serverId, File path, int clusterPort, int haPort, int initialHost1, int initialHost2 )
		 {
			  return ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(path).setConfig(ClusterSettings.initial_hosts, string.Format("127.0.0.1:{0:D},127.0.0.1:{1:D}", initialHost1, initialHost2)).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.server_id, "" + serverId).setConfig(ClusterSettings.heartbeat_timeout, "5s").setConfig(ClusterSettings.default_timeout, "1s").setConfig(HaSettings.HaServer, ":" + haPort).setConfig(HaSettings.TxPushFactor, "0").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

		 private File Path( int i )
		 {
			  return TestDirectory.databaseDir( "" + i );
		 }
	}

}