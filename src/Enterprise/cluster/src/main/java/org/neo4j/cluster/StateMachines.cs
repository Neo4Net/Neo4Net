using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageSender = Neo4Net.cluster.com.message.MessageSender;
	using MessageSource = Neo4Net.cluster.com.message.MessageSource;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using Neo4Net.cluster.statemachine;
	using StateTransitionListener = Neo4Net.cluster.statemachine.StateTransitionListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.HEADER_CREATED_BY;


	/// <summary>
	/// Combines a set of state machines into one. This will
	/// typically receive messages from the network and then delegate
	/// to the correct state machine based on what type of message comes in.
	/// Only one message at a time can be processed.
	/// </summary>
	public class StateMachines : MessageProcessor, MessageSource
	{
		 public interface Monitor
		 {
			  void BeganProcessing( Message message );

			  void FinishedProcessing( Message message );
		 }

		 private readonly Log _log;

		 private readonly Monitor _monitor;
		 private readonly MessageSender _sender;
		 private DelayedDirectExecutor _executor;
		 private Executor _stateMachineExecutor;
		 private Timeouts _timeouts;
		 private readonly IDictionary<Type, StateMachine> _stateMachines = new LinkedHashMap<Type, StateMachine>();

		 private readonly IList<MessageProcessor> _outgoingProcessors = new List<MessageProcessor>();
		 private readonly OutgoingMessageHolder _outgoing;
		 // This is used to ensure fairness of message delivery
		 private readonly ReentrantReadWriteLock @lock = new ReentrantReadWriteLock( true );
		 private readonly string _instanceIdHeaderValue;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public StateMachines(org.neo4j.logging.LogProvider logProvider, Monitor monitor, org.neo4j.cluster.com.message.MessageSource source, final org.neo4j.cluster.com.message.MessageSender sender, org.neo4j.cluster.timeout.Timeouts timeouts, DelayedDirectExecutor executor, java.util.concurrent.Executor stateMachineExecutor, InstanceId instanceId)
		 public StateMachines( LogProvider logProvider, Monitor monitor, MessageSource source, MessageSender sender, Timeouts timeouts, DelayedDirectExecutor executor, Executor stateMachineExecutor, InstanceId instanceId )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._monitor = monitor;
			  this._sender = sender;
			  this._executor = executor;
			  this._stateMachineExecutor = stateMachineExecutor;
			  this._timeouts = timeouts;
			  this._instanceIdHeaderValue = instanceId.ToString();

			  _outgoing = new OutgoingMessageHolder( this );
			  timeouts.AddMessageProcessor( this );
			  source.AddMessageProcessor( this );

		 }

		 public virtual Timeouts Timeouts
		 {
			 get
			 {
				  return _timeouts;
			 }
		 }

		 public virtual void AddStateMachine( StateMachine stateMachine )
		 {
			 lock ( this )
			 {
				  _stateMachines[stateMachine.MessageType] = stateMachine;
			 }
		 }

		 public virtual void RemoveStateMachine( StateMachine stateMachine )
		 {
			 lock ( this )
			 {
				  _stateMachines.Remove( stateMachine.MessageType );
			 }
		 }

		 public virtual IEnumerable<StateMachine> GetStateMachines()
		 {
			  return _stateMachines.Values;
		 }

		 public override void AddMessageProcessor( MessageProcessor messageProcessor )
		 {
			  _outgoingProcessors.Add( messageProcessor );
		 }

		 public virtual OutgoingMessageHolder Outgoing
		 {
			 get
			 {
				  return _outgoing;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean process(final org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message)
		 public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  _stateMachineExecutor.execute( new RunnableAnonymousInnerClass( this, message ) );
			  return true;
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly StateMachines _outerInstance;

			 private Message<T1> _message;

			 public RunnableAnonymousInnerClass( StateMachines outerInstance, Message<T1> message )
			 {
				 this.outerInstance = outerInstance;
				 this._message = message;
				 temporaryOutgoing = new OutgoingMessageHolder( outerInstance );
			 }

			 internal OutgoingMessageHolder temporaryOutgoing;

			 public void run()
			 {
				  _outerInstance.monitor.beganProcessing( _message );

				  _outerInstance.@lock.writeLock().@lock();
				  try
				  {
						// Lock timeouts while we are processing the message
						lock ( _outerInstance.timeouts )
						{
							 StateMachine stateMachine = _outerInstance.stateMachines[_message.MessageType.GetType()];
							 if ( stateMachine == null )
							 {
								  return; // No StateMachine registered for this MessageType type - Ignore this
							 }

							 handleMessage( stateMachine, _message );

							 // Process and send messages
							 // Allow state machines to send messages to each other as well in this loop
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> outgoingMessage;
							 Message<MessageType> outgoingMessage;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> toSend = new java.util.LinkedList<>();
							 IList<Message<MessageType>> toSend = new LinkedList<Message<MessageType>>();
							 try
							 {
								  while ( ( outgoingMessage = _outerInstance.outgoing.nextOutgoingMessage() ) != null )
								  {
										_message.copyHeadersTo( outgoingMessage, HEADER_CONVERSATION_ID, HEADER_CREATED_BY );

										foreach ( MessageProcessor outgoingProcessor in _outerInstance.outgoingProcessors )
										{
											 try
											 {
												  if ( !outgoingProcessor.Process( outgoingMessage ) )
												  {
														break;
												  }
											 }
											 catch ( Exception e )
											 {
												  _outerInstance.log.warn( "Outgoing message processor threw exception", e );
											 }
										}

										if ( outgoingMessage.HasHeader( Message.HEADER_TO ) )
										{
											 outgoingMessage.SetHeader( Message.HEADER_INSTANCE_ID, _outerInstance.instanceIdHeaderValue );
											 toSend.Add( outgoingMessage );
										}
										else
										{
											 // Deliver internally if possible
											 StateMachine internalStatemachine = _outerInstance.stateMachines[outgoingMessage.MessageType.GetType()];
											 if ( internalStatemachine != null )
											 {
												  handleMessage( internalStatemachine, outgoingMessage );
											 }
										}
								  }
								  if ( toSend.Count > 0 ) // the check is necessary, sender may not have started yet
								  {
										_outerInstance.sender.process( toSend );
								  }
							 }
							 catch ( Exception e )
							 {
								  _outerInstance.log.warn( "Error processing message " + _message, e );
							 }
						}
				  }
				  finally
				  {
						_outerInstance.@lock.writeLock().unlock();
				  }

				  // Before returning, process delayed executions so that they are done before returning
				  // This will effectively trigger all notifications created by contexts
				  _outerInstance.executor.drain();
				  _outerInstance.monitor.finishedProcessing( _message );
			 }

			 private void handleMessage<T1>( StateMachine stateMachine, Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
			 {
				  stateMachine.handle( message, temporaryOutgoing );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> next; (next = temporaryOutgoing.nextOutgoingMessage()) != null;)
				  for ( Message<MessageType> next; ( next = temporaryOutgoing.nextOutgoingMessage() ) != null; )
				  {
						_outerInstance.outgoing.offer( next );
				  }
			 }
		 }

		 public virtual void AddStateTransitionListener( StateTransitionListener stateTransitionListener )
		 {
			  foreach ( StateMachine stateMachine in _stateMachines.Values )
			  {
					stateMachine.addStateTransitionListener( stateTransitionListener );
			  }
		 }

		 public virtual void RemoveStateTransitionListener( StateTransitionListener stateTransitionListener )
		 {
			  foreach ( StateMachine stateMachine in _stateMachines.Values )
			  {
					stateMachine.removeStateTransitionListener( stateTransitionListener );
			  }
		 }

		 public override string ToString()
		 {
			  IList<string> states = new List<string>();
			  foreach ( StateMachine stateMachine in _stateMachines.Values )
			  {
					states.Add( stateMachine.State.GetType().BaseType.SimpleName + ":" + stateMachine.State.ToString() );
			  }
			  return states.ToString();
		 }

		 public virtual StateMachine GetStateMachine( Type messageType )
		 {
			  return _stateMachines[messageType];
		 }

		 private class OutgoingMessageHolder : MessageHolder
		 {
			 private readonly StateMachines _outerInstance;

			 public OutgoingMessageHolder( StateMachines outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Deque<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> outgoingMessages = new java.util.ArrayDeque<>();
			  internal Deque<Message<MessageType>> OutgoingMessages = new LinkedList<Message<MessageType>>();

			  public override void Offer<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
				  lock ( this )
				  {
						OutgoingMessages.addFirst( message );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public synchronized org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> nextOutgoingMessage()
			  public virtual Message<MessageType> NextOutgoingMessage()
			  {
				  lock ( this )
				  {
						return OutgoingMessages.pollFirst();
				  }
			  }
		 }
	}

}