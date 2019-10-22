using System;
using System.Collections.Generic;

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
namespace Neo4Net.cluster.statemachine
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// State machine that wraps a context and a state, which can change when a Message comes in.
	/// Incoming messages must be of the particular type that the state understands.
	/// A state machine can only handle one message at a time, so the handle message is synchronized.
	/// </summary>
	public class StateMachine<CONTEXT, MESSAGETYPE> where MESSAGETYPE : Neo4Net.cluster.com.message.MessageType
	{
		 private CONTEXT _context;
		 private Type<MESSAGETYPE> _messageEnumType;
		 private State<CONTEXT, MESSAGETYPE> _state;
		 private LogProvider _logProvider;

		 private IList<StateTransitionListener> _listeners = new List<StateTransitionListener>();

		 public StateMachine( CONTEXT context, Type messageEnumType, State<CONTEXT, MESSAGETYPE> state, LogProvider logProvider )
		 {
				 messageEnumType = typeof( MESSAGETYPE );
			  this._context = context;
			  this._messageEnumType = messageEnumType;
			  this._state = state;
			  this._logProvider = logProvider;
		 }

		 public virtual Type<MESSAGETYPE> MessageType
		 {
			 get
			 {
				  return _messageEnumType;
			 }
		 }

		 public virtual State<CONTEXT, MESSAGETYPE> State
		 {
			 get
			 {
				  return _state;
			 }
		 }

		 public virtual object Context
		 {
			 get
			 {
				  return _context;
			 }
		 }

		 public virtual void AddStateTransitionListener( StateTransitionListener listener )
		 {
			  IList<StateTransitionListener> newlisteners = new List<StateTransitionListener>( _listeners );
			  newlisteners.Add( listener );
			  _listeners = newlisteners;
		 }

		 public virtual void RemoveStateTransitionListener( StateTransitionListener listener )
		 {
			  IList<StateTransitionListener> newListeners = new List<StateTransitionListener>( _listeners );
			  newListeners.Remove( listener );
			  _listeners = newListeners;
		 }

		 public virtual void Handle( Message<MESSAGETYPE> message, MessageHolder outgoing )
		 {
			 lock ( this )
			 {
				  try
				  {
						// Let the old state handle the incoming message and tell us what the new state should be
						State<CONTEXT, MESSAGETYPE> oldState = _state;
						State<CONTEXT, MESSAGETYPE> newState = oldState.Handle( _context, message, outgoing );
						_state = newState;
      
						// Notify any listeners of the new state
						StateTransition transition = new StateTransition( oldState, message, newState );
						foreach ( StateTransitionListener listener in _listeners )
						{
							 try
							 {
								  listener.StateTransition( transition );
							 }
							 catch ( Exception e )
							 {
								  // Ignore
								  _logProvider.getLog( listener.GetType() ).warn("Listener threw exception", e);
							 }
						}
				  }
				  catch ( Exception throwable )
				  {
						_logProvider.getLog( this.GetType() ).warn("Exception in message handling", throwable);
				  }
			 }
		 }

		 public override string ToString()
		 {
			  return _state.ToString() + ": " + _context.ToString();
		 }
	}

}