using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.cluster.statemachine
{

	using Neo4Net.cluster.com.message;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Used to generate dynamic proxies whose methods are backed by a <seealso cref="StateMachine"/>. Method
	/// calls will be translated to the corresponding message, and the parameters are set as payload.
	/// <para>
	/// Methods in the interface to be proxied can either return void or Future<T>. If a method returns
	/// a future, then the value of it will be set when a message named nameResponse or nameFailure is created,
	/// where "name" corresponds to the name of the method.
	/// </para>
	/// </summary>
	public class StateMachineProxyFactory : MessageProcessor
	{
		 private readonly StateMachines _stateMachines;
		 private readonly StateMachineConversations _conversations;
		 private readonly Log _log;
		 private volatile InstanceId _me;

		 private readonly IDictionary<string, ResponseFuture> _responseFutureMap = new ConcurrentDictionary<string, ResponseFuture>();

		 public StateMachineProxyFactory( StateMachines stateMachines, StateMachineConversations conversations, InstanceId me, LogProvider logProvider )
		 {
			  this._stateMachines = stateMachines;
			  this._conversations = conversations;
			  this._me = me;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <CLIENT> CLIENT newProxy(Class<CLIENT> proxyInterface) throws IllegalArgumentException
		 public virtual CLIENT NewProxy<CLIENT>( Type proxyInterface )
		 {
				 proxyInterface = typeof( CLIENT );
			  // Get the state machine whose messages correspond to the methods of the proxy interface
			  StateMachine stateMachine = GetStateMachine( proxyInterface );

			  // Create a new dynamic proxy and handler that converts calls to state machine invocations
			  return proxyInterface.cast( Proxy.newProxyInstance( proxyInterface.ClassLoader, new Type[]{ proxyInterface }, new StateMachineProxyHandler( this, stateMachine ) ) );
		 }

		 internal virtual object Invoke( StateMachine stateMachine, System.Reflection.MethodInfo method, object arg )
		 {
			  if ( method.Name.Equals( "toString" ) )
			  {
					return _me.ToString();
			  }

			  if ( method.Name.Equals( "equals" ) )
			  {
					return ( ( StateMachineProxyHandler ) Proxy.getInvocationHandler( arg ) ).StateMachineProxyFactory.me.Equals( _me );
			  }

			  string conversationId = _conversations.NextConversationId;

			  try
			  {
					MessageType typeAsEnum = ( MessageType ) Enum.valueOf( stateMachine.MessageType, method.Name );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<?> message = org.neo4j.cluster.com.message.Message.internal(typeAsEnum, arg);
					Message<object> message = Message.@internal( typeAsEnum, arg );
					if ( _me != null )
					{
						 message.SetHeader( Message.HEADER_CONVERSATION_ID, conversationId ).setHeader( Message.HEADER_CREATED_BY,_me.ToString() );
					}

					if ( method.ReturnType.Equals( Void.TYPE ) )
					{
						 _stateMachines.process( message );
						 return null;
					}
					else
					{
						 ResponseFuture future = new ResponseFuture( conversationId, typeAsEnum, _responseFutureMap );
						 _responseFutureMap[conversationId] = future;
						 _log.debug( "Added response future for conversation id %s", conversationId );
						 _stateMachines.process( message );

						 return future;
					}
			  }
			  catch ( System.ArgumentException )
			  {
					throw new System.InvalidOperationException( "No state machine can handle the method " + method.Name );
			  }
		 }

		 public override bool Process<T1>( Message<T1> message )
		 {
			  if ( _responseFutureMap.Count > 0 )
			  {
					if ( !message.HasHeader( Message.HEADER_TO ) )
					{
						 string conversationId = message.GetHeader( Message.HEADER_CONVERSATION_ID );
						 ResponseFuture future = _responseFutureMap[conversationId];
						 if ( future != null )
						 {
							  if ( future.setPotentialResponse( message ) )
							  {
									_responseFutureMap.Remove( conversationId );
							  }
						 }
						 else
						 {
							  _log.warn( "Unable to find the client (with the conversation id %s) waiting for the response %s.", conversationId, message );
						 }
					}
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private StateMachine getStateMachine(Class proxyInterface) throws IllegalArgumentException
		 private StateMachine GetStateMachine( Type proxyInterface )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  System.ArgumentException exception = new System.ArgumentException( "No state machine can handle the " + "interface:" + proxyInterface.FullName );

			  foreach ( StateMachine stateMachine in _stateMachines.getStateMachines() )
			  {
					bool foundMatch = false;

					foreach ( System.Reflection.MethodInfo method in proxyInterface.GetMethods() )
					{
						 if ( !( method.ReturnType.Equals( Void.TYPE ) || method.ReturnType.Equals( typeof( Future ) ) ) )
						 {
							  throw new System.ArgumentException( "Methods must return either void or Future" );
						 }

						 try
						 {
							  Enum.valueOf( stateMachine.MessageType, method.Name );

							  // Ok!
							  foundMatch = true;
						 }
						 catch ( Exception )
						 {
							  if ( foundMatch )
							  {
							  // State machine could only partially handle this interface
									exception = new System.ArgumentException( "State machine for " + stateMachine.MessageType.Name + " cannot handle method:" + method.Name );
							  }

							  // Continue searching
							  goto statemachineContinue;
						 }
					}

					// All methods are implemented by this state machine - return it!
					return stateMachine;
				  statemachineContinue:;
			  }
			  statemachineBreak:

			  // Could not find any state machine that can handle this interface
			  throw exception;
		 }

		 private class ResponseFuture : Future<object>
		 {
			  internal readonly string ConversationId;
			  internal readonly MessageType InitiatedByMessageType;
			  internal readonly IDictionary<string, ResponseFuture> ResponseFutureMap; // temporary for debug logging

			  internal Message Response;

			  internal ResponseFuture( string conversationId, MessageType initiatedByMessageType, IDictionary<string, ResponseFuture> responseFutureMap )
			  {
					this.ConversationId = conversationId;
					this.InitiatedByMessageType = initiatedByMessageType;
					this.ResponseFutureMap = responseFutureMap;
			  }

			  internal virtual bool setPotentialResponse( Message response )
			  {
				  lock ( this )
				  {
						if ( IsResponse( response ) )
						{
							 this.Response = response;
							 Monitor.PulseAll( this );
							 return true;
						}
						else
						{
							 return false;
						}
				  }
			  }

			  internal virtual bool IsResponse( Message response )
			  {
					return response.MessageType.name().Equals(InitiatedByMessageType.name() + "Response") || response.MessageType.name().Equals(InitiatedByMessageType.name() + "Failure");
			  }

			  public override bool Cancel( bool mayInterruptIfRunning )
			  {
					return false;
			  }

			  public override bool Cancelled
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override bool Done
			  {
				  get
				  {
						return Response != null;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized Object get() throws InterruptedException, java.util.concurrent.ExecutionException
			  public override object Get()
			  {
				  lock ( this )
				  {
						if ( Response != null )
						{
							 return Result;
						}
      
						while ( Response == null )
						{
							 Monitor.Wait( this, TimeSpan.FromMilliseconds( 50 ) );
						}
      
						return Result;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized Object getResult() throws InterruptedException, java.util.concurrent.ExecutionException
			  internal virtual object Result
			  {
				  get
				  {
					  lock ( this )
					  {
							if ( Response.MessageType.name().Equals(InitiatedByMessageType.name() + "Failure") )
							{
								 // Call failed
								 if ( Response.Payload != null )
								 {
									  if ( Response.Payload is Exception )
									  {
											throw new ExecutionException( ( Exception ) Response.Payload );
									  }
									  else
									  {
											throw new InterruptedException( Response.Payload.ToString() );
									  }
								 }
								 else
								 {
									  // No message specified
									  throw new InterruptedException();
								 }
							}
							else
							{
								 // Return result
								 return Response.Payload;
							}
					  }
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized Object get(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
			  public override object Get( long timeout, TimeUnit unit )
			  {
				  lock ( this )
				  {
						if ( Response != null )
						{
							 Result;
						}
      
						Monitor.Wait( this, TimeSpan.FromMilliseconds( unit.toMillis( timeout ) ) );
      
						if ( Response == null )
						{
							 throw new TimeoutException( format( "Conversation-response mapping:%n" + ResponseFutureMap ) );
						}
						return Result;
				  }
			  }

			  public override string ToString()
			  {
					return "ResponseFuture{" + "conversationId='" + ConversationId + '\'' + ", initiatedByMessageType=" + InitiatedByMessageType + ", response=" + Response + '}';
			  }
		 }
	}

}