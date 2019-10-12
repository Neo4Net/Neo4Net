using System;
using System.Collections.Generic;
using System.IO;

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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{

	using NetworkReceiver = Org.Neo4j.cluster.com.NetworkReceiver;
	using NetworkSender = Org.Neo4j.cluster.com.NetworkSender;
	using Cluster = Org.Neo4j.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Org.Neo4j.cluster.protocol.cluster.ClusterContext;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using ServerIdElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ServerIdElectionCredentialsProvider;
	using Heartbeat = Org.Neo4j.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener;
	using HeartbeatMessage = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatMessage;
	using FixedTimeoutStrategy = Org.Neo4j.cluster.timeout.FixedTimeoutStrategy;
	using MessageTimeoutStrategy = Org.Neo4j.cluster.timeout.MessageTimeoutStrategy;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

	/// <summary>
	/// Multi Paxos test server
	/// </summary>
	public class MultiPaxosServer
	{
		 private AtomicBroadcastSerializer _broadcastSerializer;
		 private ProtocolServer _server;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  ( new MultiPaxosServer() ).Start();
		 }

		 protected internal Cluster Cluster;
		 protected internal AtomicBroadcast Broadcast;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public virtual void Start()
		 {
			  _broadcastSerializer = new AtomicBroadcastSerializer( new ObjectStreamFactory(), new ObjectStreamFactory() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport life = new org.neo4j.kernel.lifecycle.LifeSupport();
			  LifeSupport life = new LifeSupport();
			  try
			  {
					MessageTimeoutStrategy timeoutStrategy = ( new MessageTimeoutStrategy( new FixedTimeoutStrategy( 5000 ) ) ).timeout( HeartbeatMessage.sendHeartbeat, 200 );

					Monitors monitors = new Monitors();
					NetworkedServerFactory serverFactory = new NetworkedServerFactory( life, new MultiPaxosServerFactory( new ClusterConfiguration( "default", NullLogProvider.Instance ), NullLogProvider.Instance, monitors.NewMonitor( typeof( StateMachines.Monitor ) ) ), timeoutStrategy, NullLogProvider.Instance, new ObjectStreamFactory(), new ObjectStreamFactory(), monitors.NewMonitor(typeof(NetworkReceiver.Monitor)), monitors.NewMonitor(typeof(NetworkSender.Monitor)), monitors.NewMonitor(typeof(NamedThreadFactory.Monitor)) );

					ServerIdElectionCredentialsProvider electionCredentialsProvider = new ServerIdElectionCredentialsProvider();
					_server = serverFactory.NewNetworkedServer( Config.defaults(), new InMemoryAcceptorInstanceStore(), electionCredentialsProvider );
					_server.addBindingListener( electionCredentialsProvider );
					_server.addBindingListener( me => Console.WriteLine( "Listening at:" + me ) );

					Cluster = _server.newClient( typeof( Cluster ) );
					Cluster.addClusterListener( new ClusterListenerAnonymousInnerClass( this ) );

					Heartbeat heartbeat = _server.newClient( typeof( Heartbeat ) );
					heartbeat.AddHeartbeatListener( new HeartbeatListenerAnonymousInnerClass( this ) );

					Broadcast = _server.newClient( typeof( AtomicBroadcast ) );
					Broadcast.addAtomicBroadcastListener(value =>
					{
					 try
					 {
						  Console.WriteLine( _broadcastSerializer.receive( value ) );
					 }
					 catch ( Exception e ) when ( e is IOException || e is ClassNotFoundException )
					 {
						  e.printStackTrace();
					 }
					});

					life.Start();

					string command;
					StreamReader reader = new StreamReader( System.in );
					while ( !( command = reader.ReadLine() ).Equals("quit") )
					{
						 string[] arguments = command.Split( " ", true );
						 System.Reflection.MethodInfo method = GetCommandMethod( arguments[0] );
						 if ( method != null )
						 {
							  string[] realArgs = new string[arguments.Length - 1];
							  Array.Copy( arguments, 1, realArgs, 0, realArgs.Length );
							  try
							  {
									method.invoke( this, ( object[] ) realArgs );
							  }
							  catch ( Exception e ) when ( e is IllegalAccessException || e is System.ArgumentException || e is InvocationTargetException )
							  {
									e.printStackTrace();
							  }
						 }
					}

					Cluster.leave();
			  }
			  finally
			  {
					life.Shutdown();
					Console.WriteLine( "Done" );
			  }
		 }

		 private class ClusterListenerAnonymousInnerClass : ClusterListener
		 {
			 private readonly MultiPaxosServer _outerInstance;

			 public ClusterListenerAnonymousInnerClass( MultiPaxosServer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void enteredCluster( ClusterConfiguration clusterConfiguration )
			 {
				  Console.WriteLine( "Entered cluster:" + clusterConfiguration );
			 }

			 public void joinedCluster( InstanceId instanceId, URI member )
			 {
				  Console.WriteLine( "Joined cluster:" + instanceId + " (at URI " + member + ")" );
			 }

			 public void leftCluster( InstanceId instanceId, URI member )
			 {
				  Console.WriteLine( "Left cluster:" + instanceId );
			 }

			 public void leftCluster()
			 {
				  Console.WriteLine( "Left cluster" );
			 }

			 public void elected( string role, InstanceId instanceId, URI electedMember )
			 {
				  Console.WriteLine( instanceId + " at URI " + electedMember + " was elected as " + role );
			 }

			 public void unelected( string role, InstanceId instanceId, URI electedMember )
			 {
				  Console.WriteLine( instanceId + " at URI " + electedMember + " was removed from " + role );
			 }
		 }

		 private class HeartbeatListenerAnonymousInnerClass : HeartbeatListener
		 {
			 private readonly MultiPaxosServer _outerInstance;

			 public HeartbeatListenerAnonymousInnerClass( MultiPaxosServer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void failed( InstanceId server )
			 {
				  Console.WriteLine( server + " failed" );
			 }

			 public void alive( InstanceId server )
			 {
				  Console.WriteLine( server + " alive" );
			 }
		 }

		 public virtual void Config()
		 {
			  ClusterConfiguration configuration = ( ( ClusterContext ) _server.StateMachines.getStateMachine( typeof( ClusterMessage ) ).Context ).Configuration;

			  ICollection<InstanceId> failed = ( ( HeartbeatContext ) _server.StateMachines.getStateMachine( typeof( HeartbeatMessage ) ).Context ).Failed;
		 }

		 private System.Reflection.MethodInfo GetCommandMethod( string name )
		 {
			  foreach ( System.Reflection.MethodInfo method in typeof( MultiPaxosServer ).GetMethods() )
			  {
					if ( method.Name.Equals( name ) )
					{
						 return method;
					}
			  }
			  return null;
		 }
	}

}