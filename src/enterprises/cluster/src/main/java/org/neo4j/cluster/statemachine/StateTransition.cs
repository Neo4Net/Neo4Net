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
namespace Neo4Net.cluster.statemachine
{
	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// A single state transition that occurred in
	/// a state machine as a consequence of handling a message.
	/// </summary>
	public class StateTransition
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private State<?,?> oldState;
		 private State<object, ?> _oldState;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message;
		 private Message<MessageType> _message;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private State<?,?> newState;
		 private State<object, ?> _newState;

		 public StateTransition<T1, T2, T3>( State<T1> oldState, Message<T2> message, State<T3> newState ) where T2 : Neo4Net.cluster.com.message.MessageType
		 {
			  this._oldState = oldState;
			  this._message = message;
			  this._newState = newState;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public State<?,?> getOldState()
		 public virtual State<object, ?> OldState
		 {
			 get
			 {
				  return _oldState;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> getMessage()
		 public virtual Message<MessageType> Message
		 {
			 get
			 {
				  return _message;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public State<?,?> getNewState()
		 public virtual State<object, ?> NewState
		 {
			 get
			 {
				  return _newState;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  StateTransition that = ( StateTransition ) o;

			  if ( !_message.Equals( that._message ) )
			  {
					return false;
			  }
			  if ( !_newState.Equals( that._newState ) )
			  {
					return false;
			  }
			  return _oldState.Equals( that._oldState );
		 }

		 public override int GetHashCode()
		 {
			  int result = _oldState.GetHashCode();
			  result = 31 * result + _message.GetHashCode();
			  result = 31 * result + _newState.GetHashCode();
			  return result;
		 }

		 public override string ToString()
		 {
			  if ( _message.Payload is string )
			  {
					return OldState.ToString() + "-[" + Message.MessageType + ":" + Message.Payload +
							  "]->" + NewState.ToString();
			  }
			  else
			  {
					return OldState.ToString() + "-[" + Message.MessageType + "]->" + NewState.ToString();
			  }
		 }
	}

}