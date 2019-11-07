/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.checkState;

	/// <summary>
	/// If the state machine has been INTERRUPTED then a RESET message
	/// has entered the queue and is waiting to be processed. The initial
	/// interrupt forces the current statement to stop and all subsequent
	/// requests to be IGNORED until the RESET itself is processed.
	/// </summary>
	public class InterruptedState : BoltStateMachineState
	{
		 private BoltStateMachineState _readyState;
		 private BoltStateMachineState _failedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.runtime.BoltStateMachineState process(Neo4Net.bolt.messaging.RequestMessage message, Neo4Net.bolt.runtime.StateMachineContext context) throws Neo4Net.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is InterruptSignal )
			  {
					return this;
			  }
			  else if ( message is ResetMessage )
			  {
					if ( context.ConnectionState().decrementInterruptCounter() > 0 )
					{
						 context.ConnectionState().markIgnored();
						 return this;
					}

					if ( context.ResetMachine() )
					{
						 context.ConnectionState().resetPendingFailedAndIgnored();
						 return _readyState;
					}
					return _failedState;
			  }
			  else
			  {
					context.ConnectionState().markIgnored();
					return this;
			  }
		 }

		 public override string Name()
		 {
			  return "INTERRUPTED";
		 }

		 public virtual BoltStateMachineState ReadyState
		 {
			 set
			 {
				  this._readyState = value;
			 }
		 }

		 public virtual BoltStateMachineState FailedState
		 {
			 set
			 {
				  this._failedState = value;
			 }
		 }

		 private void AssertInitialized()
		 {
			  checkState( _readyState != null, "Ready state not set" );
			  checkState( _failedState != null, "Failed state not set" );
		 }
	}

}