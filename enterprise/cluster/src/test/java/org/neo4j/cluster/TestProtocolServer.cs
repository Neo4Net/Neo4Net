using System;
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
namespace Org.Neo4j.cluster
{

	using Org.Neo4j.cluster.com.message;
	using MessageProcessor = Org.Neo4j.cluster.com.message.MessageProcessor;
	using MessageSender = Org.Neo4j.cluster.com.message.MessageSender;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using ObjectStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using StateTransitionListener = Org.Neo4j.cluster.statemachine.StateTransitionListener;
	using TimeoutStrategy = Org.Neo4j.cluster.timeout.TimeoutStrategy;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Org.Neo4j.Helpers;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	/// <summary>
	/// TODO
	/// </summary>
	public class TestProtocolServer : MessageProcessor
	{
		 protected internal readonly TestMessageSource Receiver;
		 protected internal readonly TestMessageSender Sender;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal ProtocolServer ServerConflict;
		 private readonly DelayedDirectExecutor _stateMachineExecutor;
		 private URI _serverUri;

		 public TestProtocolServer( LogProvider logProvider, TimeoutStrategy timeoutStrategy, ProtocolServerFactory factory, URI serverUri, InstanceId instanceId, AcceptorInstanceStore acceptorInstanceStore, ElectionCredentialsProvider electionCredentialsProvider )
		 {
			  this._serverUri = serverUri;
			  this.Receiver = new TestMessageSource( this );
			  this.Sender = new TestMessageSender( this );

			  _stateMachineExecutor = new DelayedDirectExecutor( logProvider );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.MaxAcceptors ) ).thenReturn( 10 );
			  when( config.Get( ClusterSettings.StrictInitialHosts ) ).thenReturn( false );

			  ServerConflict = factory.NewProtocolServer( instanceId, timeoutStrategy, Receiver, Sender, acceptorInstanceStore, electionCredentialsProvider, _stateMachineExecutor, new ObjectStreamFactory(), new ObjectStreamFactory(), config );

			  ServerConflict.listeningAt( serverUri );
		 }

		 public virtual ProtocolServer Server
		 {
			 get
			 {
				  return ServerConflict;
			 }
		 }

		 public virtual Timeouts Timeouts
		 {
			 get
			 {
				  return ServerConflict.Timeouts;
			 }
		 }

		 public override bool Process( Message message )
		 {
			  return Receiver.process( message );
		 }

		 public virtual void SendMessages( IList<Message> output )
		 {
			  Sender.sendMessages( output );
		 }

		 public virtual T NewClient<T>( Type clientProxyInterface )
		 {
				 clientProxyInterface = typeof( T );
			  return ServerConflict.newClient( clientProxyInterface );
		 }

		 public virtual TestProtocolServer AddStateTransitionListener( StateTransitionListener listener )
		 {
			  ServerConflict.addStateTransitionListener( listener );
			  return this;
		 }

		 public virtual void Tick( long now )
		 {
			  // Time passes - check timeouts
			  ServerConflict.Timeouts.tick( now );

			  _stateMachineExecutor.drain();
		 }

		 public override string ToString()
		 {
			  return ServerConflict.ServerId + ": " + Sender.Messages.Count + ServerConflict.ToString();
		 }

		 public class TestMessageSender : MessageSender
		 {
			 private readonly TestProtocolServer _outerInstance;

			 public TestMessageSender( TestProtocolServer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IList<Message> MessagesConflict = new List<Message>();

			  public override void Process<T1>( IList<T1> messages ) where T1 : Org.Neo4j.cluster.com.message.MessageType
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message : messages)
					foreach ( Message<MessageType> message in messages )
					{
						 Process( message );
					}
			  }

			  public override bool Process<T1>( Message<T1> message ) where T1 : Org.Neo4j.cluster.com.message.MessageType
			  {
					message.SetHeader( Message.HEADER_FROM, outerInstance.serverUri.toASCIIString() );
					MessagesConflict.Add( message );
					return true;
			  }

			  public virtual IList<Message> Messages
			  {
				  get
				  {
						return MessagesConflict;
				  }
			  }

			  public virtual void SendMessages( IList<Message> output )
			  {
					( ( IList<Message> )output ).AddRange( MessagesConflict );
					MessagesConflict.Clear();
			  }
		 }

		 public class TestMessageSource : MessageSource, MessageProcessor
		 {
			 private readonly TestProtocolServer _outerInstance;

			 public TestMessageSource( TestProtocolServer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly Listeners<MessageProcessor> Listeners = new Listeners<MessageProcessor>();

			  public override void AddMessageProcessor( MessageProcessor listener )
			  {
					Listeners.add( listener );
			  }

			  public override bool Process<T1>( Message<T1> message ) where T1 : Org.Neo4j.cluster.com.message.MessageType
			  {
					foreach ( MessageProcessor listener in Listeners )
					{
						 if ( !listener.Process( message ) )
						 {
							  return false;
						 }
					}
					return true;
			  }
		 }
	}

}