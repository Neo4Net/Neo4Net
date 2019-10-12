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
namespace Org.Neo4j.ha
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using HeartbeatListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TestHighlyAvailableGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Org.Neo4j.Kernel.ha.UpdatePuller;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using StreamConsumer = Org.Neo4j.Test.StreamConsumer;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// This test case ensures that updates in HA are first written out to the log
	/// and then applied to the store. The problem appears after recovering an
	/// unclean shutdown of a slave where no transactions happened (hence the log
	/// buffer was not forced). Then it will try to retrieve the latest tx (as
	/// written in neostore) from its logs but it will not be present there. This
	/// will throw the exception of being unable to find the commit entry for that
	/// txid and that will lead to branching. The exception is thrown during startup,
	/// before the constructor returns, so we cannot test from userland. Instead we
	/// check for the symptom, which is the branched store. This is not nice, just a
	/// bit better than checking debug.log for certain entries. Another, more
	/// direct, test is present in community.
	/// </summary>
	public class PullUpdatesAppliedIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private SortedDictionary<int, Configuration> _configurations;
		 private IDictionary<int, HighlyAvailableGraphDatabase> _databases;

		 private class Configuration
		 {
			 private readonly PullUpdatesAppliedIT _outerInstance;

			  internal readonly int ServerId;
			  internal readonly int ClusterPort;
			  internal readonly int HaPort;
			  internal readonly File Directory;

			  internal Configuration( PullUpdatesAppliedIT outerInstance, int serverId, int clusterPort, int haPort, File directory )
			  {
				  this._outerInstance = outerInstance;
					this.ServerId = serverId;
					this.ClusterPort = clusterPort;
					this.HaPort = haPort;
					this.Directory = directory;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _configurations = CreateConfigurations();
			  _databases = StartDatabases();
		 }

		 private SortedDictionary<int, Configuration> CreateConfigurations()
		 {
			  SortedDictionary<int, Configuration> configurations = new SortedDictionary<int, Configuration>();

			  IntStream.range( 0, 3 ).forEach(serverId =>
			  {
						  int clusterPort = PortAuthority.allocatePort();
						  int haPort = PortAuthority.allocatePort();
						  File directory = TestDirectory.databaseDir( Convert.ToString( serverId ) ).AbsoluteFile;

						  configurations[serverId] = new Configuration( this, serverId, clusterPort, haPort, directory );
			  });

			  return configurations;
		 }

		 private IDictionary<int, HighlyAvailableGraphDatabase> StartDatabases()
		 {
			  IDictionary<int, HighlyAvailableGraphDatabase> databases = new Dictionary<int, HighlyAvailableGraphDatabase>();

			  foreach ( Configuration configuration in _configurations.Values )
			  {
					int serverId = configuration.ServerId;
					int clusterPort = configuration.ClusterPort;
					int haPort = configuration.HaPort;
					File directory = configuration.Directory;

					int initialHostPort = _configurations.Values.GetEnumerator().next().clusterPort;

					HighlyAvailableGraphDatabase hagdb = Database( serverId, clusterPort, haPort, directory, initialHostPort );

					databases[serverId] = hagdb;
			  }

			  foreach ( HighlyAvailableGraphDatabase database in databases.Values )
			  {
					database.IsAvailable( 5000 );
			  }

			  return databases;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void doAfter()
		 public virtual void DoAfter()
		 {
			  if ( _databases != null )
			  {
					_databases.Values.Where( Objects.nonNull ).ForEach( GraphDatabaseFacade.shutdown );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdatesAreWrittenToLogBeforeBeingAppliedToStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestUpdatesAreWrittenToLogBeforeBeingAppliedToStore()
		 {
			  int serverIdOfMaster = CurrentMaster;
			  AddNode( serverIdOfMaster );
			  int serverIdOfDatabaseToKill = FindSomeoneNotMaster( serverIdOfMaster );
			  HighlyAvailableGraphDatabase databaseToKill = FindDatabase( serverIdOfDatabaseToKill );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch1 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch1 = new System.Threading.CountdownEvent( 1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase masterDb = findDatabase(serverIdOfMaster);
			  HighlyAvailableGraphDatabase masterDb = FindDatabase( serverIdOfMaster );
			  masterDb.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).addClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, latch1, masterDb ) );

			  databaseToKill.Shutdown();

			  assertTrue( "Timeout waiting for instance to leave cluster", latch1.await( 60, TimeUnit.SECONDS ) );

			  AddNode( serverIdOfMaster ); // this will be pulled by tne next start up, applied but not written to log.

			  Configuration configuration = _configurations[serverIdOfDatabaseToKill];

			  int clusterPort = configuration.ClusterPort;
			  int haPort = configuration.HaPort;
			  File storeDirectory = configuration.Directory;

			  // Setup to detect shutdown of separate JVM, required since we don't exit cleanly. That is also why we go
			  // through the heartbeat and not through the cluster change as above.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch2 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch2 = new System.Threading.CountdownEvent( 1 );

			  masterDb.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).addHeartbeatListener( new HeartbeatListener_AdapterAnonymousInnerClass( this, masterDb, latch2 ) );

			  RunInOtherJvm( storeDirectory, serverIdOfDatabaseToKill, clusterPort, haPort, _configurations[serverIdOfMaster].clusterPort );

			  assertTrue( "Timeout waiting for instance to fail", latch2.await( 60, TimeUnit.SECONDS ) );

			  // This is to allow other instances to mark the dead instance as failed, otherwise on startup it will be denied.
			  // TODO This is to demonstrate shortcomings in our design. Fix this, you ugly, ugly hacker
			  Thread.Sleep( 15000 );

			  Restart( serverIdOfDatabaseToKill ); // recovery and branching.
			  bool hasBranchedData = ( new File( storeDirectory, "branched" ) ).listFiles().length > 0;
			  assertFalse( hasBranchedData );
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly PullUpdatesAppliedIT _outerInstance;

			 private System.Threading.CountdownEvent _latch1;
			 private HighlyAvailableGraphDatabase _masterDb;

			 public ClusterListener_AdapterAnonymousInnerClass( PullUpdatesAppliedIT outerInstance, System.Threading.CountdownEvent latch1, HighlyAvailableGraphDatabase masterDb )
			 {
				 this.outerInstance = outerInstance;
				 this._latch1 = latch1;
				 this._masterDb = masterDb;
			 }

			 public override void leftCluster( InstanceId instanceId, URI member )
			 {
				  _latch1.Signal();
				  _masterDb.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).removeClusterListener( this );
			 }
		 }

		 private class HeartbeatListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener_Adapter
		 {
			 private readonly PullUpdatesAppliedIT _outerInstance;

			 private HighlyAvailableGraphDatabase _masterDb;
			 private System.Threading.CountdownEvent _latch2;

			 public HeartbeatListener_AdapterAnonymousInnerClass( PullUpdatesAppliedIT outerInstance, HighlyAvailableGraphDatabase masterDb, System.Threading.CountdownEvent latch2 )
			 {
				 this.outerInstance = outerInstance;
				 this._masterDb = masterDb;
				 this._latch2 = latch2;
			 }

			 public override void failed( InstanceId server )
			 {
				  _latch2.Signal();
				  _masterDb.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).removeHeartbeatListener( this );
			 }
		 }

		 private HighlyAvailableGraphDatabase FindDatabase( int serverId )
		 {
			  return _databases[serverId];
		 }

		 private int FindSomeoneNotMaster( int serverIdOfMaster )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return _databases.Keys.Where( serverId => serverId != serverIdOfMaster ).First().orElseThrow(System.InvalidOperationException::new);
		 }

		 private void Restart( int serverId )
		 {
			  Configuration configuration = _configurations[serverId];

			  int clusterPort = configuration.ClusterPort;
			  int haPort = configuration.HaPort;
			  File directory = configuration.Directory;

			  HighlyAvailableGraphDatabase highlyAvailableGraphDatabase = Database( serverId, clusterPort, haPort, directory, _configurations.Values.GetEnumerator().next().clusterPort );

			  _databases[serverId] = highlyAvailableGraphDatabase;
		 }

		 private static HighlyAvailableGraphDatabase Database( int serverId, int clusterPort, int haPort, File path, int initialHostPort )
		 {
			  return ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(path).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + initialHostPort).setConfig(ClusterSettings.server_id, Convert.ToString(serverId)).setConfig(HaSettings.ha_server, "localhost:" + haPort).setConfig(HaSettings.pull_interval, "0ms").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runInOtherJvm(java.io.File directory, int serverIdOfDatabaseToKill, int clusterPort, int haPort, int initialHostPort) throws Exception
		 private static void RunInOtherJvm( File directory, int serverIdOfDatabaseToKill, int clusterPort, int haPort, int initialHostPort )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  IList<string> commandLine = new List<string>( Arrays.asList( "java", "-Djava.awt.headless=true", "-cp", System.getProperty( "java.class.path" ), typeof( PullUpdatesAppliedIT ).FullName ) );
			  commandLine.Add( directory.ToString() );
			  commandLine.Add( serverIdOfDatabaseToKill.ToString() );
			  commandLine.Add( clusterPort.ToString() );
			  commandLine.Add( haPort.ToString() );
			  commandLine.Add( initialHostPort.ToString() );

			  Process p = Runtime.Runtime.exec( commandLine.ToArray() );
			  IList<Thread> threads = new LinkedList<Thread>();
			  LaunchStreamConsumers( threads, p );
			  /*
			   * Yes, timeouts suck but HAGD does not terminate politely, since it still has
			   * threads running after main() completes, so we need to kill it. When? 10 seconds
			   * is good enough.
			   */
			  // a generous timeout; individual tests' latencies do not matter when running tests in parallel
			  Thread.Sleep( 30000 );
			  p.destroy();
			  foreach ( Thread t in threads )
			  {
					t.Join();
			  }
		 }

		 // For executing in a different process than the one running the test case.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  File storePath = new File( args[0] );
			  int serverId = int.Parse( args[1] );
			  int clusterPort = int.Parse( args[2] );
			  int haPort = int.Parse( args[3] );
			  int initialHostPort = int.Parse( args[4] );

			  HighlyAvailableGraphDatabase hagdb = Database( serverId, clusterPort, haPort, storePath, initialHostPort );

			  hagdb.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
			  // this is the bug trigger
			  // no shutdown, emulates a crash.
		 }

		 private static void LaunchStreamConsumers( IList<Thread> join, Process p )
		 {
			  Stream outStr = p.InputStream;
			  Stream errStr = p.ErrorStream;
			  Thread @out = new Thread( new StreamConsumer( outStr, System.out, false ) );
			  join.Add( @out );
			  Thread err = new Thread( new StreamConsumer( errStr, System.err, false ) );
			  join.Add( err );
			  @out.Start();
			  err.Start();
		 }

		 private void AddNode( int serverId )
		 {
			  HighlyAvailableGraphDatabase db = FindDatabase( serverId );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode().Id;
					tx.Success();
			  }
		 }

		 private int CurrentMaster
		 {
			 get
			 {
				  return _databases.SetOfKeyValuePairs().Where(entry => entry.Value.Master).First().orElseThrow(() => new System.InvalidOperationException("no master")).Key;
			 }
		 }
	}

}