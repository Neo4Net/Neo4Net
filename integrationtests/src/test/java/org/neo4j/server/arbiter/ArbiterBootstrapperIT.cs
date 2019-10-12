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
namespace Org.Neo4j.Server.arbiter
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using ClusterClientModule = Org.Neo4j.cluster.client.ClusterClientModule;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using ClusterListener_Adapter = Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter;
	using ServerIdElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ServerIdElectionCredentialsProvider;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using InputStreamAwaiter = Org.Neo4j.Test.InputStreamAwaiter;
	using ProcessStreamHandler = Org.Neo4j.Test.ProcessStreamHandler;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.cluster_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.initial_hosts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.server_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.store;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.StreamConsumer.IGNORE_FAILURES;

	public class ArbiterBootstrapperIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canJoinWithExplicitInitialHosts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanJoinWithExplicitInitialHosts()
		 {
			  StartAndAssertJoined( 5003, stringMap( initial_hosts.name(), ":5001", server_id.name(), "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void willFailJoinIfIncorrectInitialHostsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WillFailJoinIfIncorrectInitialHostsSet()
		 {
			  assumeFalse( "Cannot kill processes on windows.", SystemUtils.IS_OS_WINDOWS );
			  StartAndAssertJoined( _shouldNotJoin, stringMap( initial_hosts.name(), ":5011", server_id.name(), "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canSetSpecificPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanSetSpecificPort()
		 {
			  StartAndAssertJoined( 5010, stringMap( initial_hosts.name(), ":5001", server_id.name(), "3", cluster_server.name(), ":5010" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usesPortRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UsesPortRange()
		 {
			  StartAndAssertJoined( 5012, stringMap( initial_hosts.name(), ":5001", cluster_server.name(), ":5012-5020", server_id.name(), "3" ) );
		 }

		 // === Everything else ===

		 private static int? _shouldNotJoin;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _directory;
		 private LifeSupport _life;
		 private ClusterClient[] _clients;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _directory = TestDirectory.directory( "temp" );
			  _life = new LifeSupport();
			  _life.start(); // So that the clients get started as they are added
			  _clients = new ClusterClient[2];
			  for ( int i = 1; i <= _clients.Length; i++ )
			  {
					IDictionary<string, string> config = stringMap();
					config[cluster_server.name()] = ":" + (5000 + i);
					config[server_id.name()] = "" + i;
					config[initial_hosts.name()] = ":5001";

					LifeSupport moduleLife = new LifeSupport();
					ClusterClientModule clusterClientModule = new ClusterClientModule( moduleLife, new Dependencies(), new Monitors(), Config.defaults(config), NullLogService.Instance, new ServerIdElectionCredentialsProvider() );

					ClusterClient client = clusterClientModule.ClusterClient;
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
					client.AddClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, client, latch ) );
					_life.add( moduleLife );
					_clients[i - 1] = client;
					assertTrue( "Didn't join the cluster", latch.await( 20, SECONDS ) );
			  }
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : ClusterListener_Adapter
		 {
			 private readonly ArbiterBootstrapperIT _outerInstance;

			 private ClusterClient _client;
			 private System.Threading.CountdownEvent _latch;

			 public ClusterListener_AdapterAnonymousInnerClass( ArbiterBootstrapperIT outerInstance, ClusterClient client, System.Threading.CountdownEvent latch )
			 {
				 this.outerInstance = outerInstance;
				 this._client = client;
				 this._latch = latch;
			 }

			 public override void enteredCluster( ClusterConfiguration configuration )
			 {
				  _latch.Signal();
				  _client.removeClusterListener( this );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File writeConfig(java.util.Map<String, String> config) throws java.io.IOException
		 private File WriteConfig( IDictionary<string, string> config )
		 {
			  config[GraphDatabaseSettings.logs_directory.name()] = _directory.Path;
			  File configFile = new File( _directory, Config.DEFAULT_CONFIG_FILE_NAME );
			  store( config, configFile );
			  return _directory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startAndAssertJoined(System.Nullable<int> expectedAssignedPort, java.util.Map<String, String> config) throws Exception
		 private void StartAndAssertJoined( int? expectedAssignedPort, IDictionary<string, string> config )
		 {
			  File configDir = WriteConfig( config );
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  AtomicInteger port = new AtomicInteger();
			  _clients[0].addClusterListener( JoinAwaitingListener( latch, port ) );

			  bool arbiterStarted = StartArbiter( configDir, latch );
			  if ( expectedAssignedPort == null )
			  {
					assertFalse( format( "Should not be able to start arbiter given config file:%s", config ), arbiterStarted );
			  }
			  else
			  {
					assertTrue( format( "Should be able to start arbiter given config file:%s", config ), arbiterStarted );
					assertEquals( expectedAssignedPort.Value, port.get() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.cluster.protocol.cluster.ClusterListener_Adapter joinAwaitingListener(final java.util.concurrent.CountDownLatch latch, final java.util.concurrent.atomic.AtomicInteger port)
		 private ClusterListener_Adapter JoinAwaitingListener( System.Threading.CountdownEvent latch, AtomicInteger port )
		 {
			  return new ClusterListener_AdapterAnonymousInnerClass2( this, latch, port );
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass2 : ClusterListener_Adapter
		 {
			 private readonly ArbiterBootstrapperIT _outerInstance;

			 private System.Threading.CountdownEvent _latch;
			 private AtomicInteger _port;

			 public ClusterListener_AdapterAnonymousInnerClass2( ArbiterBootstrapperIT outerInstance, System.Threading.CountdownEvent latch, AtomicInteger port )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._port = port;
			 }

			 public override void joinedCluster( InstanceId member, URI memberUri )
			 {
				  _port.set( memberUri.Port );
				  _latch.Signal();
				  _outerInstance.clients[0].removeClusterListener( this );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean startArbiter(java.io.File configDir, java.util.concurrent.CountDownLatch latch) throws Exception
		 private bool StartArbiter( File configDir, System.Threading.CountdownEvent latch )
		 {
			  Process process = null;
			  ProcessStreamHandler handler = null;
			  try
			  {
					process = StartArbiterProcess( configDir );
					( new InputStreamAwaiter( process.InputStream ) ).awaitLine( ArbiterBootstrapperTestProxy.START_SIGNAL, 20, SECONDS );
					handler = new ProcessStreamHandler( process, false, "", IGNORE_FAILURES );
					handler.Launch();

					// Latch is triggered when the arbiter we just spawned joins the cluster,
					// or rather when the first client sees it as joined. If the latch awaiting times out it
					// (most likely) means that the arbiter couldn't be started. The reason for not
					// being able to start is assumed in this test to be that the specified port already is in use.
					return latch.await( 10, SECONDS );
			  }
			  finally
			  {
					if ( process != null )
					{
						 // Tell it to leave the cluster and shut down now
						 using ( Stream inputToOtherProcess = process.OutputStream )
						 {
							  inputToOtherProcess.WriteByte( 0 );
							  inputToOtherProcess.Flush();
						 }
						 if ( !process.waitFor( 10, SECONDS ) )
						 {
							  Kill( process );
						 }
					}
					if ( handler != null )
					{
						 handler.Done();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Process startArbiterProcess(java.io.File configDir) throws Exception
		 private Process StartArbiterProcess( File configDir )
		 {
			  IList<string> args = new IList<string> { "java", "-cp", getProperty( "java.class.path" ) };
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  args.Add( typeof( ArbiterBootstrapperTestProxy ).FullName );
			  if ( configDir != null )
			  {
					args.Add( string.Format( "--{0}={1}", ServerCommandLineArgs.CONFIG_DIR_ARG, configDir ) );
			  }
			  return Runtime.exec( args.ToArray() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void kill(Process process) throws NoSuchFieldException, IllegalAccessException, java.io.IOException, InterruptedException
		 private static void Kill( Process process )
		 {
			  if ( SystemUtils.IS_OS_WINDOWS )
			  {
					process.destroy();
			  }
			  else
			  {
					int pid = ( ( Number ) Accessible( process.GetType().getDeclaredField("pid") ).get(process) ).intValue();
					( new ProcessBuilder( "kill", "-9", "" + pid ) ).start().waitFor();
			  }
		 }

		 private static T Accessible<T>( T obj ) where T : AccessibleObject
		 {
			  obj.Accessible = true;
			  return obj;
		 }
	}

}