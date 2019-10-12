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
namespace Org.Neo4j.Bolt.v3.runtime
{
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using CommitMessage = Org.Neo4j.Bolt.v3.messaging.request.CommitMessage;
	using RollbackMessage = Org.Neo4j.Bolt.v3.messaging.request.RollbackMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;

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
			  if ( message is InterruptSignal )
			  {
					return _interruptedState;
			  }
			  return null;
		 }

		 public virtual BoltStateMachineState InterruptedState
		 {
			 set
			 {
				  this._interruptedState = value;
			 }
		 }

		 protected internal virtual void AssertInitialized()
		 {
			  checkState( _interruptedState != null, "Interrupted state not set" );
		 }

		 public override string Name()
		 {
			  return "FAILED";
		 }

		 private static bool ShouldIgnore( RequestMessage message )
		 {
			  return message is RunMessage || message is PullAllMessage || message is DiscardAllMessage || message is CommitMessage || message is RollbackMessage;
		 }
	}

}