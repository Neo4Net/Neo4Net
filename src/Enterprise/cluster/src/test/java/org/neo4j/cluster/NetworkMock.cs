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
namespace Neo4Net.cluster
{

	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using InMemoryAcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ServerIdElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ServerIdElectionCredentialsProvider;
	using StateTransitionLogger = Neo4Net.cluster.statemachine.StateTransitionLogger;
	using MessageTimeoutStrategy = Neo4Net.cluster.timeout.MessageTimeoutStrategy;
	using Neo4Net.Helpers.Collection;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;

	/// <summary>
	/// This mocks message delivery, message loss, and time for timeouts and message latency
	/// between protocol servers.
	/// </summary>
	public class NetworkMock
	{
		 internal IDictionary<string, TestProtocolServer> Participants = new LinkedHashMap<string, TestProtocolServer>();

		 private IList<MessageDelivery> _messageDeliveries = new List<MessageDelivery>();

		 private long _now;
		 private Monitors _monitors;
		 private long _tickDuration;
		 private readonly MultipleFailureLatencyStrategy _strategy;
		 private MessageTimeoutStrategy _timeoutStrategy;
		 private LogService _logService;
		 protected internal readonly Log Log;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<org.neo4j.helpers.collection.Pair<java.util.concurrent.Future<?>, Runnable>> futureWaiter;
		 private readonly IList<Pair<Future<object>, ThreadStart>> _futureWaiter;

		 public NetworkMock( LogService logService, Monitors monitors, long tickDuration, MultipleFailureLatencyStrategy strategy, MessageTimeoutStrategy timeoutStrategy )
		 {
			  this._monitors = monitors;
			  this._tickDuration = tickDuration;
			  this._strategy = strategy;
			  this._timeoutStrategy = timeoutStrategy;
			  this._logService = logService;
			  this.Log = logService.GetInternalLog( typeof( NetworkMock ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: futureWaiter = new java.util.LinkedList<>();
			  _futureWaiter = new LinkedList<Pair<Future<object>, ThreadStart>>();
		 }

		 public virtual TestProtocolServer AddServer( int serverId, URI serverUri )
		 {
			  TestProtocolServer server = NewTestProtocolServer( serverId, serverUri );

			  Debug( serverUri.ToString(), "joins network" );

			  Participants[serverUri.ToString()] = server;

			  return server;
		 }

		 protected internal virtual TestProtocolServer NewTestProtocolServer( int serverId, URI serverUri )
		 {
			  ProtocolServerFactory protocolServerFactory = new MultiPaxosServerFactory( new ClusterConfiguration( "default", _logService.InternalLogProvider ), _logService.InternalLogProvider, _monitors.newMonitor( typeof( StateMachines.Monitor ) ) );

			  ServerIdElectionCredentialsProvider electionCredentialsProvider = new ServerIdElectionCredentialsProvider();
			  electionCredentialsProvider.ListeningAt( serverUri );
			  TestProtocolServer protocolServer = new TestProtocolServer( _logService.InternalLogProvider, _timeoutStrategy, protocolServerFactory, serverUri, new InstanceId( serverId ), new InMemoryAcceptorInstanceStore(), electionCredentialsProvider );
			  protocolServer.AddStateTransitionListener( new StateTransitionLogger( _logService.InternalLogProvider, new AtomicBroadcastSerializer( new ObjectStreamFactory(), new ObjectStreamFactory() ) ) );
			  return protocolServer;
		 }

		 private void Debug( string participant, string @string )
		 {
			  Log.debug( "=== " + participant + " " + @string );
		 }

		 public virtual void RemoveServer( string serverId )
		 {
			  Debug( serverId, "leaves network" );
			  Participants.Remove( serverId );
		 }

		 public virtual void AddFutureWaiter<T1>( Future<T1> future, ThreadStart toRun )
		 {
			  _futureWaiter.Add( Pair.of( future, toRun ) );
		 }

		 public virtual int Tick()
		 {
			  // Deliver messages whose delivery time has passed
			  _now += _tickDuration;

			  //       logger.debug( "tick:"+now );

			  IEnumerator<MessageDelivery> iter = _messageDeliveries.GetEnumerator();
			  while ( iter.MoveNext() )
			  {
					MessageDelivery messageDelivery = iter.Current;
					if ( messageDelivery.MessageDeliveryTime <= _now )
					{
						 long delay = _strategy.messageDelay( messageDelivery.Message, messageDelivery.Server.Server.boundAt().ToString() );
						 if ( delay != NetworkLatencyStrategy_Fields.LOST )
						 {
							  messageDelivery.Server.process( messageDelivery.Message );
						 }
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 iter.remove();
					}
			  }

			  // Check and trigger timeouts
			  foreach ( TestProtocolServer testServer in Participants.Values )
			  {
					testServer.Tick( _now );
			  }

			  // Get all sent messages from all test servers
			  IList<Message> messages = new List<Message>();
			  foreach ( TestProtocolServer testServer in Participants.Values )
			  {
					testServer.SendMessages( messages );
			  }

			  // Now send them and figure out latency
			  foreach ( Message message in messages )
			  {
					string to = message.getHeader( Message.HEADER_TO );
					long delay = 0;
					if ( message.getHeader( Message.HEADER_TO ).Equals( message.getHeader( Message.HEADER_FROM ) ) )
					{
						 Log.debug( "Sending message to itself; zero latency" );
					}
					else
					{
						 delay = _strategy.messageDelay( message, to );
					}

					if ( delay == NetworkLatencyStrategy_Fields.LOST )
					{
						 Log.debug( "Send message to " + to + " was lost" );
					}
					else
					{
						 TestProtocolServer server = Participants[to];
						 Log.debug( "Send to " + to + ": " + message );
						 _messageDeliveries.Add( new MessageDelivery( _now + delay, message, server ) );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<org.neo4j.helpers.collection.Pair<java.util.concurrent.Future<?>, Runnable>> waiters = futureWaiter.iterator();
			  IEnumerator<Pair<Future<object>, ThreadStart>> waiters = _futureWaiter.GetEnumerator();
			  while ( waiters.MoveNext() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<java.util.concurrent.Future<?>, Runnable> next = waiters.Current;
					Pair<Future<object>, ThreadStart> next = waiters.Current;
					if ( next.First().Done )
					{
						 next.Other().run();
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 waiters.remove();
					}
			  }

			  return _messageDeliveries.Count;
		 }

		 public virtual void Tick( int iterations )
		 {
			  for ( int i = 0; i < iterations; i++ )
			  {
					Tick();
			  }
		 }

		 public virtual void TickUntilDone()
		 {
			  while ( Tick() + TotalCurrentTimeouts() > 0 )
			  {
			  }
		 }

		 private int TotalCurrentTimeouts()
		 {
			  int count = 0;
			  foreach ( TestProtocolServer testProtocolServer in Participants.Values )
			  {
					count += testProtocolServer.Timeouts.Timeouts.Count;
			  }
			  return count;
		 }

		 public override string ToString()
		 {
			  StringWriter stringWriter = new StringWriter();
			  PrintWriter @out = new PrintWriter( stringWriter, true );
			  @out.printf( "Now:%s \n", _now );
			  @out.printf( "Pending messages:%s \n", _messageDeliveries.Count );
			  @out.printf( "Pending timeouts:%s \n", TotalCurrentTimeouts() );

			  foreach ( TestProtocolServer testProtocolServer in Participants.Values )
			  {
					@out.println( "  " + testProtocolServer );
			  }
			  return stringWriter.ToString();
		 }

		 public virtual long? Time
		 {
			 get
			 {
				  return _now;
			 }
		 }

		 public virtual IList<TestProtocolServer> Servers
		 {
			 get
			 {
				  return new List<TestProtocolServer>( Participants.Values );
			 }
		 }

		 public virtual MultipleFailureLatencyStrategy NetworkLatencyStrategy
		 {
			 get
			 {
				  return _strategy;
			 }
		 }

		 public virtual MessageTimeoutStrategy TimeoutStrategy
		 {
			 get
			 {
				  return _timeoutStrategy;
			 }
		 }

		 private class MessageDelivery
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long MessageDeliveryTimeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message;
			  internal Message<MessageType> MessageConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal TestProtocolServer ServerConflict;

			  internal MessageDelivery<T1>( long messageDeliveryTime, Message<T1> message, TestProtocolServer server ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
					this.MessageDeliveryTimeConflict = messageDeliveryTime;
					this.MessageConflict = message;
					this.ServerConflict = server;
			  }

			  public virtual long MessageDeliveryTime
			  {
				  get
				  {
						return MessageDeliveryTimeConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> getMessage()
			  public virtual Message<MessageType> Message
			  {
				  get
				  {
						return MessageConflict;
				  }
			  }

			  public virtual TestProtocolServer Server
			  {
				  get
				  {
						return ServerConflict;
				  }
			  }

			  public override string ToString()
			  {
					return "Deliver " + MessageConflict.MessageType.name() + " to " + ServerConflict.Server.ServerId + " at "
							  + MessageDeliveryTimeConflict;
			  }
		 }
	}

}