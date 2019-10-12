/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.v1.runtime
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	/// <summary>
	/// The FAILED state occurs when a recoverable error is encountered.
	/// This might be something like a Cypher SyntaxError or
	/// ConstraintViolation. To exit the FAILED state, either a RESET
	/// or and ACK_FAILURE must be issued. All stream will be IGNORED
	/// until this is done.
	/// </summary>
	public class FailedState : BoltStateMachineState
	{
		 private BoltStateMachineState _readyState;
		 private BoltStateMachineState _interruptedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( ShouldIgnore( message ) )
			  {
					context.ConnectionState().markIgnored();
					return this;
			  }
			  if ( message is AckFailureMessage )
			  {
					context.ConnectionState().resetPendingFailedAndIgnored();
					return _readyState;
			  }
			  if ( message is ResetMessage )
			  {
					return ProcessResetMessage( context );
			  }
			  if ( message is InterruptSignal )
			  {
					return _interruptedState;
			  }
			  return null;
		 }

		 public override string Name()
		 {
			  return "FAILED";
		 }

		 public virtual BoltStateMachineState ReadyState
		 {
			 set
			 {
				  this._readyState = value;
			 }
		 }

		 public virtual BoltStateMachineState InterruptedState
		 {
			 set
			 {
				  this._interruptedState = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processResetMessage(org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessResetMessage( StateMachineContext context )
		 {
			  bool success = context.ResetMachine();
			  if ( success )
			  {
					context.ConnectionState().resetPendingFailedAndIgnored();
					return _readyState;
			  }
			  return this;
		 }

		 private void AssertInitialized()
		 {
			  checkState( _readyState != null, "Ready state not set" );
			  checkState( _interruptedState != null, "Interrupted state not set" );
		 }

		 private static bool ShouldIgnore( RequestMessage message )
		 {
			  return message is RunMessage || message is PullAllMessage || message is DiscardAllMessage;
		 }
	}

}