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
namespace Neo4Net.cluster.protocol.cluster
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;


	using AtomicBroadcast = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcast;
	using AtomicBroadcastListener = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastListener;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using Payload = Neo4Net.cluster.protocol.atomicbroadcast.Payload;
	using Heartbeat = Neo4Net.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using HeartbeatMessage = Neo4Net.cluster.protocol.heartbeat.HeartbeatMessage;
	using Neo4Net.cluster.statemachine;
	using FixedTimeoutStrategy = Neo4Net.cluster.timeout.FixedTimeoutStrategy;
	using MessageTimeoutStrategy = Neo4Net.cluster.timeout.MessageTimeoutStrategy;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// Base class for cluster tests
	/// </summary>
	public class ClusterMockTest
	{

		 public static NetworkMock DefaultNetwork()
		 {
			  return new NetworkMock(NullLogService.Instance, new Monitors(), 10, new MultipleFailureLatencyStrategy(new FixedNetworkLatencyStrategy(10), new ScriptableNetworkFailureLatencyStrategy()), new MessageTimeoutStrategy(new FixedTimeoutStrategy(500))
									.timeout( HeartbeatMessage.sendHeartbeat, 200 ));
		 }

		 internal IList<TestProtocolServer> Servers = new List<TestProtocolServer>();
		 internal IList<Cluster> Out = new List<Cluster>();
		 internal IList<Cluster> In = new List<Cluster>();
		 internal IDictionary<int, URI> Members = new Dictionary<int, URI>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.LoggerRule logger = new org.neo4j.test.rule.LoggerRule(java.util.logging.Level.OFF);
		 public LoggerRule Logger = new LoggerRule( Level.OFF );

		 public NetworkMock Network;

		 internal ClusterTestScript Script;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  Logger.Logger.fine( "Current threads" );
			  foreach ( KeyValuePair<Thread, StackTraceElement[]> threadEntry in Thread.AllStackTraces.entrySet() )
			  {
					Logger.Logger.fine( threadEntry.Key.Name );
					foreach ( StackTraceElement stackTraceElement in threadEntry.Value )
					{
						 Logger.Logger.fine( "   " + stackTraceElement.ToString() );
					}
			  }

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void testCluster(int nrOfServers, org.neo4j.cluster.NetworkMock mock, ClusterTestScript script) throws java.net.URISyntaxException
		 protected internal virtual void TestCluster( int nrOfServers, NetworkMock mock, ClusterTestScript script )
		 {
			  int[] serverIds = new int[nrOfServers];
			  for ( int i = 1; i <= nrOfServers; i++ )
			  {
					serverIds[i - 1] = i;
			  }
			  TestCluster( serverIds, null, mock, script );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void testCluster(int[] serverIds, org.neo4j.cluster.VerifyInstanceConfiguration[] finalConfig, org.neo4j.cluster.NetworkMock mock, ClusterTestScript script) throws java.net.URISyntaxException
		 protected internal virtual void TestCluster( int[] serverIds, VerifyInstanceConfiguration[] finalConfig, NetworkMock mock, ClusterTestScript script )
		 {
			  this.Script = script;

			  Network = mock;
			  Servers.Clear();
			  Out.Clear();
			  In.Clear();

			  for ( int i = 0; i < serverIds.Length; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URI uri = new java.net.URI("server" + (i + 1));
					URI uri = new URI( "server" + ( i + 1 ) );
					Members[serverIds[i]] = uri;
					TestProtocolServer server = Network.addServer( serverIds[i], uri );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Cluster cluster = server.newClient(Cluster.class);
					Cluster cluster = server.NewClient( typeof( Cluster ) );
					ClusterStateListener( uri, cluster );

					server.NewClient( typeof( Heartbeat ) ).addHeartbeatListener( new HeartbeatListenerAnonymousInnerClass( this, uri, server ) );
					server.NewClient( typeof( AtomicBroadcast ) ).addAtomicBroadcastListener( new AtomicBroadcastListenerAnonymousInnerClass( this, uri ) );

					Servers.Add( server );
					Out.Add( cluster );
			  }

			  // Run test
			  for ( int i = 0; i < script.Rounds(); i++ )
			  {
					Logger.Logger.fine( "Round " + i + ", time:" + Network.Time );

					script.Tick( Network.Time.Value );

					Network.tick();
			  }

			  // Let messages settle
			  Network.tick( 100 );
			  if ( finalConfig == null )
			  {
					VerifyConfigurations( "final config" );
			  }
			  else
			  {
					VerifyConfigurations( finalConfig );
			  }

			  Logger.Logger.fine( "All nodes leave" );

			  // All leave
			  foreach ( Cluster cluster in new List<>( In ) )
			  {
					Logger.Logger.fine( "Leaving:" + cluster );
					cluster.Leave();
					In.Remove( cluster );
					Network.tick( 400 );
			  }

			  if ( finalConfig != null )
			  {
					VerifyConfigurations( finalConfig );
			  }
			  else
			  {
					VerifyConfigurations( "test over" );
			  }
		 }

		 private class HeartbeatListenerAnonymousInnerClass : HeartbeatListener
		 {
			 private readonly ClusterMockTest _outerInstance;

			 private URI _uri;
			 private TestProtocolServer _server;

			 public HeartbeatListenerAnonymousInnerClass( ClusterMockTest outerInstance, URI uri, TestProtocolServer server )
			 {
				 this.outerInstance = outerInstance;
				 this._uri = uri;
				 this._server = server;
			 }

			 public void failed( InstanceId server )
			 {
				  _outerInstance.logger.Logger.warning( _uri + ": Failed:" + server );
			 }

			 public void alive( InstanceId server )
			 {
				  _outerInstance.logger.Logger.fine( _uri + ": Alive:" + server );
			 }
		 }

		 private class AtomicBroadcastListenerAnonymousInnerClass : AtomicBroadcastListener
		 {
			 private readonly ClusterMockTest _outerInstance;

			 private URI _uri;

			 public AtomicBroadcastListenerAnonymousInnerClass( ClusterMockTest outerInstance, URI uri )
			 {
				 this.outerInstance = outerInstance;
				 this._uri = uri;
				 serializer = new AtomicBroadcastSerializer( new ObjectStreamFactory(), new ObjectStreamFactory() );
			 }

			 internal AtomicBroadcastSerializer serializer;

			 public void receive( Payload value )
			 {
				  try
				  {
						_outerInstance.logger.Logger.fine( _uri + " received: " + serializer.receive( value ) );
				  }
				  catch ( Exception e ) when ( e is IOException || e is ClassNotFoundException )
				  {
						e.printStackTrace();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void clusterStateListener(final java.net.URI uri, final Cluster cluster)
		 private void ClusterStateListener( URI uri, Cluster cluster )
		 {
			  cluster.AddClusterListener( new ClusterListenerAnonymousInnerClass( this, uri, cluster ) );
		 }

		 private class ClusterListenerAnonymousInnerClass : ClusterListener
		 {
			 private readonly ClusterMockTest _outerInstance;

			 private URI _uri;
			 private Neo4Net.cluster.protocol.cluster.Cluster _cluster;

			 public ClusterListenerAnonymousInnerClass( ClusterMockTest outerInstance, URI uri, Neo4Net.cluster.protocol.cluster.Cluster cluster )
			 {
				 this.outerInstance = outerInstance;
				 this._uri = uri;
				 this._cluster = cluster;
			 }

			 public void enteredCluster( ClusterConfiguration clusterConfiguration )
			 {
				  _outerInstance.logger.Logger.fine( _uri + " entered cluster:" + clusterConfiguration.MemberURIs );
				  _outerInstance.@in.Add( _cluster );
			 }

			 public void joinedCluster( InstanceId id, URI member )
			 {
				  _outerInstance.logger.Logger.fine( _uri + " sees a join from " + id + " at URI " + member );
			 }

			 public void leftCluster( InstanceId id, URI member )
			 {
				  _outerInstance.logger.Logger.fine( _uri + " sees a leave from " + id );
			 }

			 public void leftCluster()
			 {
				  _outerInstance.logger.Logger.fine( _uri + " left cluster" );
				  _outerInstance.@out.Add( _cluster );
			 }

			 public void elected( string role, InstanceId id, URI electedMember )
			 {
				  _outerInstance.logger.Logger.fine( _uri + " sees an election: " + id + " elected as " + role + " at URI " + electedMember );
			 }

			 public void unelected( string role, InstanceId instanceId, URI electedMember )
			 {
				  _outerInstance.logger.Logger.fine( _uri + " sees an unelection: " + instanceId + " removed from " + role + " at URI " + electedMember );
			 }
		 }

		 public virtual void VerifyConfigurations( VerifyInstanceConfiguration[] toCheckAgainst )
		 {
			  Logger.Logger.fine( "Verify configurations against given" );

			  IList<URI> members;
			  IDictionary<string, InstanceId> roles;
			  ISet<InstanceId> failed;

			  IList<AssertionError> errors = new LinkedList<AssertionError>();

			  IList<TestProtocolServer> protocolServers = Network.Servers;

			  assertEquals( "You must provide a configuration for all instances", protocolServers.Count, toCheckAgainst.Length );

			  for ( int j = 0; j < protocolServers.Count; j++ )
			  {
					members = toCheckAgainst[j].Members;
					roles = toCheckAgainst[j].Roles;
					failed = toCheckAgainst[j].Failed;
					StateMachines stateMachines = protocolServers[j].Server.StateMachines;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.statemachine.State<?, ?> clusterState = stateMachines.getStateMachine(ClusterMessage.class).getState();
					State<object, ?> clusterState = stateMachines.GetStateMachine( typeof( ClusterMessage ) ).State;
					if ( !clusterState.Equals( ClusterState.Entered ) )
					{
						 Logger.Logger.warning( "Instance " + ( j + 1 ) + " is not in the cluster (" + clusterState + ")" );
						 continue;
					}

					ClusterContext context = ( ClusterContext ) stateMachines.GetStateMachine( typeof( ClusterMessage ) ).Context;
					HeartbeatContext heartbeatContext = ( HeartbeatContext ) stateMachines.GetStateMachine( typeof( HeartbeatMessage ) ).Context;
					ClusterConfiguration clusterConfiguration = context.Configuration;
					if ( clusterConfiguration.MemberURIs.Count > 0 )
					{
						 Logger.Logger.fine( "   Server " + ( j + 1 ) + ": Cluster:" + clusterConfiguration.MemberURIs + ", Roles:" + clusterConfiguration.Roles + ", Failed:" + heartbeatContext.Failed );
						 VerifyConfigurations( stateMachines, members, roles, failed, errors );
					}
			  }

	//        assertEquals( "In:" + in + ", Out:" + out, protocolServers.size(), Iterables.count( Iterables.<Cluster,
	//                List<Cluster>>flatten( in, out ) ) );

			  if ( errors.Count > 0 )
			  {
					foreach ( AssertionError error in errors )
					{
						 Logger.Logger.severe( error.ToString() );
					}
					throw errors[0];
			  }
		 }

		 public virtual void VerifyConfigurations( string description )
		 {
			  Logger.Logger.fine( "Verify configurations" );

			  IList<URI> members = null;
			  IDictionary<string, InstanceId> roles = null;
			  ISet<InstanceId> failed = null;

			  IList<AssertionError> errors = new LinkedList<AssertionError>();

			  IList<TestProtocolServer> protocolServers = Network.Servers;

			  for ( int j = 0; j < protocolServers.Count; j++ )
			  {
					StateMachines stateMachines = protocolServers[j].Server.StateMachines;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.statemachine.State<?, ?> clusterState = stateMachines.getStateMachine(ClusterMessage.class).getState();
					State<object, ?> clusterState = stateMachines.GetStateMachine( typeof( ClusterMessage ) ).State;
					if ( !clusterState.Equals( ClusterState.Entered ) )
					{
						 Logger.Logger.fine( "Instance " + ( j + 1 ) + " is not in the cluster (" + clusterState + ")" );
						 continue;
					}

					ClusterContext context = ( ClusterContext ) stateMachines.GetStateMachine( typeof( ClusterMessage ) ).Context;
					HeartbeatContext heartbeatContext = ( HeartbeatContext ) stateMachines.GetStateMachine( typeof( HeartbeatMessage ) ).Context;
					ClusterConfiguration clusterConfiguration = context.Configuration;
					if ( clusterConfiguration.MemberURIs.Count > 0 )
					{
						 Logger.Logger.fine( "   Server " + ( j + 1 ) + ": Cluster:" + clusterConfiguration.MemberURIs + ", Roles:" + clusterConfiguration.Roles + ", Failed:" + heartbeatContext.Failed );
						 if ( members == null )
						 {
							  members = clusterConfiguration.MemberURIs;
							  roles = clusterConfiguration.Roles;
							  failed = heartbeatContext.Failed;
						 }
						 else
						 {
							  VerifyConfigurations( stateMachines, members, roles, failed, errors );
						 }
					}
			  }

			  assertEquals( description + ": In:" + In + ", Out:" + Out, protocolServers.Count, Iterables.count( Iterables.flatten( In, Out ) ) );

			  if ( errors.Count > 0 )
			  {
					foreach ( AssertionError error in errors )
					{
						 Logger.Logger.severe( error.ToString() );
					}
					throw errors[0];
			  }
		 }

		 private void VerifyConfigurations( StateMachines stateMachines, IList<URI> members, IDictionary<string, InstanceId> roles, ISet<InstanceId> failed, IList<AssertionError> errors )
		 {

			  ClusterContext context = ( ClusterContext ) stateMachines.GetStateMachine( typeof( ClusterMessage ) ).Context;
			  int myId = context.MyId.toIntegerIndex();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.statemachine.State<?, ?> clusterState = stateMachines.getStateMachine(ClusterMessage.class).getState();
			  State<object, ?> clusterState = stateMachines.GetStateMachine( typeof( ClusterMessage ) ).State;
			  if ( !clusterState.Equals( ClusterState.Entered ) )
			  {
					Logger.Logger.warning( "Instance " + myId + " is not in the cluster (" + clusterState + ")" );
					return;
			  }

			  HeartbeatContext heartbeatContext = ( HeartbeatContext ) stateMachines.GetStateMachine( typeof( HeartbeatMessage ) ).Context;
			  ClusterConfiguration clusterConfiguration = context.Configuration;
			  try
			  {
					assertEquals("Config for server" + myId + " is wrong", new HashSet<>(members), new HashSet<>(clusterConfiguration.MemberURIs)
				  );
			  }
			  catch ( AssertionError e )
			  {
					errors.Add( e );
			  }
			  try
			  {
					assertEquals( "Roles for server" + myId + " is wrong", roles, clusterConfiguration.Roles );
			  }
			  catch ( AssertionError e )
			  {
					errors.Add( e );
			  }
			  try
			  {
					assertEquals( "Failed for server" + myId + " is wrong", failed, heartbeatContext.Failed );
			  }
			  catch ( AssertionError e )
			  {
					errors.Add( e );
			  }
		 }

		 public interface ClusterTestScript
		 {
			  int Rounds();

			  void Tick( long time );
		 }

		 public class ClusterTestScriptDSL : ClusterTestScript
		 {
			 private readonly ClusterMockTest _outerInstance;

			 public ClusterTestScriptDSL( ClusterMockTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public abstract class ClusterAction : ThreadStart
			  {
				  private readonly ClusterMockTest.ClusterTestScriptDSL _outerInstance;

				  public ClusterAction( ClusterMockTest.ClusterTestScriptDSL outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					public long Time;
			  }

			  internal readonly LinkedList<ClusterAction> Actions = new LinkedList<ClusterAction>();
			  private final AtomicBroadcastSerializer serializer = new AtomicBroadcastSerializer(new ObjectStreamFactory()
						, new ObjectStreamFactory());

			  private int RoundsConflict = 100;
			  private long Now;

			  public ClusterTestScriptDSL Rounds( int n )
			  {
					RoundsConflict = n;
					return this;
			  }

			  public ClusterTestScriptDSL Join( int time, final int joinServer, final int... joinServers )
			  {
					return addAction(new ClusterActionAnonymousInnerClass(this)
				  , time);
			  }

			  public ClusterTestScriptDSL Leave( long time, final int leaveServer )
			  {
					return addAction(new ClusterActionAnonymousInnerClass2(this)
				  , time);
			  }

			  public ClusterTestScriptDSL Down( int time, final int serverDown )
			  {
					return addAction(new ClusterActionAnonymousInnerClass3(this)
				  , time);
			  }

			  public ClusterTestScriptDSL Up( int time, final int serverUp )
			  {
					return addAction(new ClusterActionAnonymousInnerClass4(this)
				  , time);
			  }

			  public ClusterTestScriptDSL Broadcast( int time, final int server, final object value )
			  {
					return addAction(new ClusterActionAnonymousInnerClass5(this)
				  , time);
			  }

			  public ClusterTestScriptDSL Sleep( final int sleepTime )
			  {
					return addAction(new ClusterActionAnonymousInnerClass6(this)
				  , sleepTime);
			  }

			  public ClusterTestScriptDSL Message( int time, final string msg )
			  {
					return addAction(new ClusterActionAnonymousInnerClass7(this)
				  , time);
			  }

			  public ClusterTestScriptDSL VerifyConfigurations( final string description, long time )
			  {
					return addAction(new ClusterActionAnonymousInnerClass8(this)
				  , time);
			  }

			  private ClusterTestScriptDSL AddAction( ClusterAction action, long time )
			  {
					action.time = Now + time;
					Actions.AddLast( action );
					Now += time;
					return this;
			  }

			  public int Rounds()
			  {
					return RoundsConflict;
			  }

			  public void tick( long time )
			  {
					while ( Actions.Count > 0 && Actions.First.Value.time == time )
					{
						 Actions.RemoveFirst().run();
					}
			  }

			  public ClusterTestScriptDSL GetRoles( final IDictionary<string, InstanceId> roles )
			  {
					return addAction(new ClusterActionAnonymousInnerClass9(this)
				  , 0);
			  }

			  public ClusterTestScriptDSL VerifyCoordinatorRoleSwitched( final IDictionary<string, InstanceId> comparedTo )
			  {
					return addAction(new ClusterActionAnonymousInnerClass10(this)
				  , 0);
			  }
		 }

		 private void getRoles( IDictionary<string, InstanceId> roles )
		 {
			  IList<TestProtocolServer> protocolServers = Network.Servers;
			  for ( int j = 0; j < protocolServers.Count; j++ )
			  {
					StateMachines stateMachines = protocolServers[j].Server.StateMachines;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.statemachine.State<?, ?> clusterState = stateMachines.getStateMachine(ClusterMessage.class).getState();
					State<object, ?> clusterState = stateMachines.GetStateMachine( typeof( ClusterMessage ) ).State;
					if ( !clusterState.Equals( ClusterState.Entered ) )
					{
						 Logger.Logger.warning( "Instance " + ( j + 1 ) + " is not in the cluster (" + clusterState + ")" );
						 continue;
					}

					ClusterContext context = ( ClusterContext ) stateMachines.GetStateMachine( typeof( ClusterMessage ) ).Context;
					ClusterConfiguration clusterConfiguration = context.Configuration;
					roles.putAll( clusterConfiguration.Roles );
			  }
		 }
	}

}