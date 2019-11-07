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
namespace Neo4Net.cluster.protocol.cluster
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using NetworkReceiver = Neo4Net.cluster.com.NetworkReceiver;
	using NetworkSender = Neo4Net.cluster.com.NetworkSender;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using InMemoryAcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using ServerIdElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ServerIdElectionCredentialsProvider;
	using FixedTimeoutStrategy = Neo4Net.cluster.timeout.FixedTimeoutStrategy;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(value = Parameterized.class) public class ClusterNetworkIT
	public class ClusterNetworkIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { 3, ( new ClusterTestScriptDSL() ).Join(10L, 1, 2, 3).join(0L, 2, 1, 3).join(0L, 3, 1, 2).leave(10000L, 3).leave(100L, 2).leave(100L, 1) },
				  new object[] { 3, ( new ClusterTestScriptDSL() ).Join(10L, 1).join(10L, 2).join(100L, 3).leave(100L, 3).leave(100L, 2).leave(100L, 1) },
				  new object[] { 3, ( new ClusterTestScriptDSL() ).Join(100L, 1).join(100L, 2).join(100L, 3).join(100L, 4).join(100L, 5).join(100L, 6).join(100L, 7).leave(100L, 7).leave(100L, 6).leave(100L, 5).leave(100L, 4).leave(100L, 3).leave(100L, 2).leave(100L, 1) },
				  new object[] { 4, ( new ClusterTestScriptDSL() ).Join(100L, 1).join(100L, 2).join(10L, 3).leave(500L, 3).leave(100L, 2).leave(100L, 1) },
				  new object[] { 3, ( new ClusterTestScriptDSL() ).Join(100L, 1).join(100L, 2).leave(90L, 2).join(20L, 3) },
				  new object[] { 3, new ClusterTestScriptRandom( 1337830212532839000L ) }
			  });
		 }

		 private class ClusterActionAnonymousInnerClass : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Cluster joinCluster = _servers[joinServer - 1].newClient( typeof( Cluster ) );
				  foreach ( Cluster cluster in @out )
				  {
						if ( cluster.Equals( joinCluster ) )
						{
							 @out.Remove( cluster );
							 Logger.Logger.fine( "Join:" + cluster.ToString() );
							 if ( joinServers.length == 0 )
							 {
								  if ( @in.Count == 0 )
								  {
										cluster.create( "default" );
								  }
								  else
								  {
										// Use test info to figure out who to join
										URI[] toJoin = new URI[_servers.Count];
										for ( int i = 0; i < _servers.Count; i++ )
										{
											 toJoin[i] = _servers[i].Server.boundAt();
										}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<ClusterConfiguration> result = cluster.join("default", toJoin);
										Future<ClusterConfiguration> result = cluster.join( "default", toJoin );
										ThreadStart joiner = () =>
										{
										 try
										 {
											  ClusterConfiguration clusterConfiguration = result.get();
											  Logger.Logger.fine( "**** Cluster configuration:" + clusterConfiguration );
										 }
										 catch ( Exception e )
										 {
											  Logger.Logger.warning( "**** Node could not join cluster:" + e.Message );
											  @out.Add( cluster );
										 }
										};
										network.addFutureWaiter( result, joiner );
								  }
							 }
							 else
							 {
								  // List of servers to join was explicitly specified, so use that
								  URI[] instanceUris = new URI[joinServers.length];
								  for ( int i = 0; i < joinServers.length; i++ )
								  {
										int server = joinServers[i];
										instanceUris[i] = URI.create( "server" + server );
								  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<ClusterConfiguration> result = cluster.join("default", instanceUris);
								  Future<ClusterConfiguration> result = cluster.join( "default", instanceUris );
								  ThreadStart joiner = () =>
								  {
									try
									{
										 ClusterConfiguration clusterConfiguration = result.get();
										 Logger.Logger.fine( "**** Cluster configuration:" + clusterConfiguration );
									}
									catch ( Exception e )
									{
										 Logger.Logger.warning( "**** Node " + joinServer + " could not join cluster:" + e.Message );
										 if ( !( e.InnerException is System.InvalidOperationException ) )
										 {
											  cluster.create( "default" );
										 }
										 else
										 {
											  Logger.Logger.warning( "*** Incorrectly configured cluster? " + e.InnerException.Message );
										 }
									}
								  };
								  network.addFutureWaiter( result, joiner );
							 }
							 break;
						}
				  }
			 }
		 }

		 private class ClusterActionAnonymousInnerClass2 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass2( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Cluster leaveCluster = _servers[leaveServer - 1].newClient( typeof( Cluster ) );
				  foreach ( Cluster cluster in @in )
				  {
						if ( cluster.Equals( leaveCluster ) )
						{
							 @in.Remove( cluster );
							 cluster.Leave();
							 Logger.Logger.fine( "Leave:" + cluster.ToString() );
							 break;
						}
				  }
			 }
		 }

		 private class ClusterActionAnonymousInnerClass3 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass3( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Cluster server = _servers[serverDown - 1].newClient( typeof( Cluster ) );
				  network.NetworkLatencyStrategy.getStrategy( typeof( ScriptableNetworkFailureLatencyStrategy ) ).nodeIsDown( "server" + server.ToString() );
				  Logger.Logger.fine( server + " is down" );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass4 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass4( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Cluster server = _servers[serverUp - 1].newClient( typeof( Cluster ) );
				  network.NetworkLatencyStrategy.getStrategy( typeof( ScriptableNetworkFailureLatencyStrategy ) ).nodeIsUp( "server" + server.ToString() );
				  Logger.Logger.fine( server + " is up" );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass5 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass5( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  AtomicBroadcast broadcast = _servers[server - 1].newClient( typeof( AtomicBroadcast ) );
				  try
				  {
						broadcast.broadcast( serializer.broadcast( value ) );
				  }
				  catch ( IOException e )
				  {
						Console.WriteLine( e.ToString() );
						Console.Write( e.StackTrace );
				  }
			 }
		 }

		 private class ClusterActionAnonymousInnerClass6 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass6( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Logger.Logger.fine( "Slept for " + sleepTime );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass7 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass7( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Logger.Logger.fine( msg );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass8 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass8( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  outerInstance.outerInstance.verifyConfigurations( description );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass9 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass9( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  outerInstance.outerInstance.getRoles( roles );
			 }
		 }

		 private class ClusterActionAnonymousInnerClass10 : ClusterAction
		 {
			 private readonly ClusterTestScriptDSL outerInstance;

			 public ClusterActionAnonymousInnerClass10( ClusterTestScriptDSL outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				  Dictionary<string, InstanceId> roles = new Dictionary<string, InstanceId>();
				  outerInstance.outerInstance.getRoles( roles );
				  InstanceId oldCoordinator = comparedTo.get( ClusterConfiguration.COORDINATOR );
				  InstanceId newCoordinator = roles.get( ClusterConfiguration.COORDINATOR );
				  assertNotNull( "Should have had a coordinator before bringing it down", oldCoordinator );
				  assertNotNull( "Should have a new coordinator after the previous failed", newCoordinator );
				  assertTrue( "Should have elected a new coordinator", !oldCoordinator.Equals( newCoordinator ) );
			 }
		 }

		 private static IList<Cluster> _servers = new List<Cluster>();
		 private static IList<Cluster> @out = new List<Cluster>();
		 private static IList<Cluster> @in = new List<Cluster>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.LoggerRule logger = new Neo4Net.test.rule.LoggerRule(java.util.logging.Level.OFF);
		 public static LoggerRule Logger = new LoggerRule( Level.OFF );

		 private IList<AtomicReference<ClusterConfiguration>> _configurations = new List<AtomicReference<ClusterConfiguration>>();

		 private ClusterTestScript _script;

		 private Timer _timer = new Timer();

		 private LifeSupport _life = new LifeSupport();

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterNetworkIT(int nrOfServers, ClusterTestScript script) throws java.net.URISyntaxException
		 public ClusterNetworkIT( int nrOfServers, ClusterTestScript script )
		 {
			  this._script = script;

			  @out.Clear();
			  @in.Clear();

			  for ( int i = 0; i < nrOfServers; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URI uri = new java.net.URI("Neo4Net://localhost:" + Neo4Net.ports.allocation.PortAuthority.allocatePort());
					URI uri = new URI( "Neo4Net://localhost:" + PortAuthority.allocatePort() );

					Monitors monitors = new Monitors();
					NetworkedServerFactory factory = new NetworkedServerFactory( _life, new MultiPaxosServerFactory( new ClusterConfiguration( "default", NullLogProvider.Instance ), NullLogProvider.Instance, monitors.NewMonitor( typeof( StateMachines.Monitor ) ) ), new FixedTimeoutStrategy( 1000 ), NullLogProvider.Instance, new ObjectStreamFactory(), new ObjectStreamFactory(), monitors.NewMonitor(typeof(NetworkReceiver.Monitor)), monitors.NewMonitor(typeof(NetworkSender.Monitor)), monitors.NewMonitor(typeof(NamedThreadFactory.Monitor)) );

					ServerIdElectionCredentialsProvider electionCredentialsProvider = new ServerIdElectionCredentialsProvider();
					ProtocolServer server = factory.NewNetworkedServer( Config.defaults( MapUtil.stringMap( ClusterSettings.cluster_server.name(), uri.Host + ":" + uri.Port, ClusterSettings.server_id.name(), "" + i ) ), new InMemoryAcceptorInstanceStore(), electionCredentialsProvider );
					server.AddBindingListener( electionCredentialsProvider );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Cluster cluster2 = server.newClient(Cluster.class);
					Cluster cluster2 = server.NewClient( typeof( Cluster ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<ClusterConfiguration> config2 = clusterStateListener(uri, cluster2);
					AtomicReference<ClusterConfiguration> config2 = ClusterStateListener( uri, cluster2 );

					_servers.Add( cluster2 );
					@out.Add( cluster2 );
					_configurations.Add( config2 );
			  }

			  _life.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _executor = Executors.newSingleThreadExecutor( new NamedThreadFactory( "Threaded actions" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executor.shutdownNow();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCluster() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCluster()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long start = System.currentTimeMillis();
			  long start = DateTimeHelper.CurrentUnixTimeMillis();
			  _timer.scheduleAtFixedRate(new TimerTaskAnonymousInnerClass(this, start)
			 , 0, 10);

			  // Let messages settle
			  Thread.Sleep( _script.Length + 1000 );

			  Logger.Logger.fine( "All nodes leave" );

			  // All leave
			  foreach ( Cluster cluster in new List<>( @in ) )
			  {
					Logger.Logger.fine( "Leaving:" + cluster );
					cluster.Leave();
					Thread.Sleep( 100 );
			  }
		 }

		 private class TimerTaskAnonymousInnerClass : TimerTask
		 {
			 private readonly ClusterNetworkIT _outerInstance;

			 private long _start;

			 public TimerTaskAnonymousInnerClass( ClusterNetworkIT outerInstance, long start )
			 {
				 this.outerInstance = outerInstance;
				 this._start = start;
			 }

			 internal int i;

			 public override void run()
			 {
				  long now = DateTimeHelper.CurrentUnixTimeMillis() - _start;
				  Logger.Logger.fine( "Round " + i + ", time:" + now );

				  _outerInstance.script.tick( now );

				  if ( ++i == 1000 )
				  {
						_outerInstance.timer.cancel();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown()
		 public virtual void Shutdown()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.concurrent.atomic.AtomicReference<ClusterConfiguration> clusterStateListener(final java.net.URI uri, final Cluster cluster)
		 private AtomicReference<ClusterConfiguration> ClusterStateListener( URI uri, Cluster cluster )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<ClusterConfiguration> config = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<ClusterConfiguration> config = new AtomicReference<ClusterConfiguration>();
			  cluster.AddClusterListener( new ClusterListenerAnonymousInnerClass( this, uri, cluster, config ) );
			  return config;
		 }

		 private class ClusterListenerAnonymousInnerClass : ClusterListener
		 {
			 private readonly ClusterNetworkIT _outerInstance;

			 private URI _uri;
			 private Neo4Net.cluster.protocol.cluster.Cluster _cluster;
			 private AtomicReference<ClusterConfiguration> _config;

			 public ClusterListenerAnonymousInnerClass( ClusterNetworkIT outerInstance, URI uri, Neo4Net.cluster.protocol.cluster.Cluster cluster, AtomicReference<ClusterConfiguration> config )
			 {
				 this.outerInstance = outerInstance;
				 this._uri = uri;
				 this._cluster = cluster;
				 this._config = config;
			 }

			 public void enteredCluster( ClusterConfiguration clusterConfiguration )
			 {
				  Logger.Logger.fine( _uri + " entered cluster:" + clusterConfiguration.MemberURIs );
				  _config.set( new ClusterConfiguration( clusterConfiguration ) );
				  @in.Add( _cluster );
			 }

			 public void joinedCluster( InstanceId instanceId, URI member )
			 {
				  Logger.Logger.fine( _uri + " sees a join from " + instanceId + " at URI " + member.ToString() );
				  _config.get().joined(instanceId, member);
			 }

			 public void leftCluster( InstanceId instanceId, URI member )
			 {
				  Logger.Logger.fine( _uri + " sees a leave:" + instanceId );
				  _config.get().left(instanceId);
			 }

			 public void leftCluster()
			 {
				  @out.Add( _cluster );
				  _config.set( null );
			 }

			 public void elected( string role, InstanceId instanceId, URI electedMember )
			 {
				  Logger.Logger.fine( _uri + " sees an election:" + instanceId + "was elected as " + role + " on URI " + electedMember );
			 }

			 public void unelected( string role, InstanceId instanceId, URI electedMember )
			 {
				  Logger.Logger.fine( _uri + " sees an unelection:" + instanceId + "was removed from " + role + " on URI " + electedMember );
			 }
		 }

		 internal interface ClusterTestScript
		 {
			  void Tick( long time );

			  long Length { get; }
		 }

		 private class ClusterTestScriptDSL : ClusterTestScript
		 {
			  internal abstract class ClusterAction : ThreadStart
			  {
					public long Time;
			  }

			  internal LinkedList<ClusterAction> Actions = new LinkedList<ClusterAction>();

			  internal long Now;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterTestScriptDSL join(long time, final int joinServer, final int... joinServers)
			  public virtual ClusterTestScriptDSL Join( long time, int joinServer, params int[] joinServers )
			  {
					ClusterAction joinAction = new ClusterActionAnonymousInnerClass( this, joinServer, joinServers );
					joinAction.Time = Now + time;
					Actions.AddLast( joinAction );
					Now += time;
					return this;
			  }

			  private class ClusterActionAnonymousInnerClass : ClusterAction
			  {
				  private readonly ClusterTestScriptDSL _outerInstance;

				  private int _joinServer;
				  private int[] _joinServers;

				  public ClusterActionAnonymousInnerClass( ClusterTestScriptDSL outerInstance, int joinServer, int[] joinServers )
				  {
					  this.outerInstance = outerInstance;
					  this._joinServer = joinServer;
					  this._joinServers = joinServers;
				  }

				  public override void run()
				  {
						Cluster joinCluster = _servers[_joinServer - 1];
						foreach ( Cluster cluster in @out )
						{
							 if ( cluster.Equals( joinCluster ) )
							 {
								  @out.Remove( cluster );
								  Logger.Logger.fine( "Join:" + cluster.ToString() );
								  if ( _joinServers.Length == 0 )
								  {
										if ( @in.Count == 0 )
										{
											 cluster.create( "default" );
										}
										else
										{
											 // Use test info to figure out who to join
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<ClusterConfiguration> result = cluster.join("default", java.net.URI.create(in.get(0).toString()));
											 Future<ClusterConfiguration> result = cluster.join( "default", URI.create( @in[0].ToString() ) );
											 _executor.submit(() =>
											 {
											  try
											  {
													ClusterConfiguration clusterConfiguration = result.get();
													Logger.Logger.fine( "**** Cluster configuration:" + clusterConfiguration );
											  }
											  catch ( Exception e )
											  {
													Logger.Logger.fine( "**** Node " + _joinServer + " could not " + "join cluster:" + e.Message );
													@out.Add( cluster );
											  }
											 });
										}
								  }
								  else
								  {
										// List of servers to join was explicitly specified, so use that
										URI[] instanceUris = new URI[_joinServers.Length];
										for ( int i = 0; i < _joinServers.Length; i++ )
										{
											 int server = _joinServers[i];
											 instanceUris[i] = URI.create( _servers[server - 1].ToString() );
										}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<ClusterConfiguration> result = cluster.join("default", instanceUris);
										Future<ClusterConfiguration> result = cluster.join( "default", instanceUris );
										_executor.submit(() =>
										{
										 try
										 {
											  ClusterConfiguration clusterConfiguration = result.get();
											  Logger.Logger.fine( "**** Cluster configuration:" + clusterConfiguration );
										 }
										 catch ( Exception e )
										 {
											  if ( !( e.InnerException is System.InvalidOperationException ) )
											  {
													cluster.create( "default" );
											  }
											  else
											  {
													Logger.Logger.fine( "*** Incorrectly configured cluster? " + e.InnerException.Message );
											  }
										 }
										});
								  }
								  /*
								  if ( in.isEmpty() )
								  {
								      cluster.create( "default" );
								  }
								  else
								  {
								      try
								      {
								          cluster.join( "default", new URI( in.get( 0 ).toString() ) );
								      }
								      catch ( URISyntaxException e )
								      {
								          e.printStackTrace();
								      }
								  }*/
								  break;
							 }
						}
				  }
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterTestScriptDSL leave(long time, final int leaveServer)
			  public virtual ClusterTestScriptDSL Leave( long time, int leaveServer )
			  {
					ClusterAction leaveAction = new ClusterActionAnonymousInnerClass2( this, leaveServer );
					leaveAction.Time = Now + time;
					Actions.AddLast( leaveAction );
					Now += time;
					return this;
			  }

			  private class ClusterActionAnonymousInnerClass2 : ClusterAction
			  {
				  private readonly ClusterTestScriptDSL _outerInstance;

				  private int _leaveServer;

				  public ClusterActionAnonymousInnerClass2( ClusterTestScriptDSL outerInstance, int leaveServer )
				  {
					  this.outerInstance = outerInstance;
					  this._leaveServer = leaveServer;
				  }

				  public override void run()
				  {
						Cluster leaveCluster = _servers[_leaveServer - 1];
						foreach ( Cluster cluster in @in )
						{
							 if ( cluster.Equals( leaveCluster ) )
							 {
								  @in.Remove( cluster );
								  cluster.Leave();
								  Logger.Logger.fine( "Leave:" + cluster.ToString() );
								  break;
							 }
						}
				  }
			  }

			  public override void Tick( long time )
			  {
	//            logger.getLogger().debug( actions.size()+" actions remaining" );
					while ( Actions.Count > 0 && Actions.First.Value.time <= time )
					{
						 Actions.RemoveFirst().run();
					}
			  }

			  public virtual long Length
			  {
				  get
				  {
						return Now;
				  }
			  }
		 }

		 public class ClusterTestScriptRandom : ClusterTestScript
		 {
			  internal readonly long Seed;
			  internal readonly Random Random;

			  public ClusterTestScriptRandom( long seed )
			  {
					if ( seed == -1 )
					{
						 seed = System.nanoTime();
					}
					this.Seed = seed;
					Random = new Random( seed );
			  }

			  public override void Tick( long time )
			  {
					if ( time == 0 )
					{
						 Logger.Logger.fine( "Random seed:" + Seed );
					}

					if ( Random.NextDouble() >= 0.9 )
					{
						 if ( Random.NextDouble() > 0.5 && @out.Count > 0 )
						 {
							  int idx = Random.Next( @out.Count );
							  Cluster cluster = @out.RemoveAt( idx );

							  if ( @in.Count == 0 )
							  {
									cluster.Create( "default" );
							  }
							  else
							  {
									try
									{
										 cluster.Join( "default", new URI( @in[0].ToString() ) );
									}
									catch ( URISyntaxException e )
									{
										 Console.WriteLine( e.ToString() );
										 Console.Write( e.StackTrace );
									}
							  }
							  Logger.Logger.fine( "Enter cluster:" + cluster.ToString() );

						 }
						 else if ( @in.Count > 0 )
						 {
							  int idx = Random.Next( @in.Count );
							  Cluster cluster = @in.RemoveAt( idx );
							  cluster.Leave();
							  Logger.Logger.fine( "Leave cluster:" + cluster.ToString() );
						 }
					}
			  }

			  public virtual long Length
			  {
				  get
				  {
						return 5000;
				  }
			  }
		 }
	}

}