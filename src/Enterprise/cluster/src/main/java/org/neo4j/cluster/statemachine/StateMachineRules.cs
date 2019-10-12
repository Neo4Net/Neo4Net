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
namespace Neo4Net.cluster.statemachine
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Trigger messages when given state transitions occur
	/// </summary>
	public class StateMachineRules : StateTransitionListener
	{
		 private readonly MessageHolder _outgoing;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Map<State<?,?>,java.util.List<StateMachineRule>> rules = new java.util.HashMap<>();
		 private IDictionary<State<object, ?>, IList<StateMachineRule>> _rules = new Dictionary<State<object, ?>, IList<StateMachineRule>>();

		 public StateMachineRules( MessageHolder outgoing )
		 {
			  this._outgoing = outgoing;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public StateMachineRules rule(State<?, ?> oldState, org.neo4j.cluster.com.message.MessageType messageType, State<?, ?> newState, org.neo4j.cluster.com.message.Message<?>... messages)
		 public virtual StateMachineRules Rule<T1, T2>( State<T1> oldState, MessageType messageType, State<T2> newState, params Message<object>[] messages )
		 {
			  IList<StateMachineRule> fromRules = _rules.computeIfAbsent( oldState, k => new List<StateMachineRule>() );
			  fromRules.Add( new StateMachineRule( this, oldState, messageType, newState, messages ) );
			  return this;
		 }

		 public override void StateTransition( StateTransition transition )
		 {
			  IList<StateMachineRule> oldStateRules = _rules[transition.OldState];
			  if ( oldStateRules != null )
			  {
					foreach ( StateMachineRule oldStateRule in oldStateRules )
					{
						 oldStateRule.StateTransition( transition );
					}
			  }
		 }

		 private class StateMachineRule : StateTransitionListener
		 {
			 private readonly StateMachineRules _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: State<?,?> oldState;
			  internal State<object, ?> OldState;
			  internal MessageType MessageType;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: State<?,?> newState;
			  internal State<object, ?> NewState;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<?>[] messages;
			  internal Message<object>[] Messages;

			  internal StateMachineRule<T1, T2, T3>( StateMachineRules outerInstance, State<T1> oldState, MessageType messageType, State<T2> newState, Message<T3>[] messages )
			  {
				  this._outerInstance = outerInstance;
					this.OldState = oldState;
					this.MessageType = messageType;
					this.NewState = newState;
					this.Messages = messages;
			  }

			  public override void StateTransition( StateTransition transition )
			  {
					if ( OldState.Equals( transition.OldState ) && transition.Message.MessageType.Equals( MessageType ) && NewState.Equals( transition.NewState ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.cluster.com.message.Message<?> message : messages)
						 foreach ( Message<object> message in Messages )
						 {
							  outerInstance.outgoing.Offer( message );
						 }
					}
			  }
		 }
	}

}