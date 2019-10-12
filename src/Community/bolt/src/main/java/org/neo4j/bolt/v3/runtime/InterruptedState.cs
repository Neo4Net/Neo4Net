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
namespace Neo4Net.Bolt.v3.runtime
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	/// <summary>
	/// If the state machine has been INTERRUPTED then a RESET message
	/// has entered the queue and is waiting to be processed. The initial
	/// interrupt forces the current statement to stop and all subsequent
	/// requests to be IGNORED until the RESET itself is processed.
	/// </summary>
	public class InterruptedState : BoltStateMachineState
	{
		 private BoltStateMachineState _readyState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is InterruptSignal )
			  {
					return this;
			  }
			  if ( message is ResetMessage )
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
					return null;
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

		 private void AssertInitialized()
		 {
			  checkState( _readyState != null, "Ready state not set" );
		 }
	}

}