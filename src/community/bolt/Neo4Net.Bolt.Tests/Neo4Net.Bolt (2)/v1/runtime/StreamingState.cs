using System;

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
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using AuthorizationExpiredException = Neo4Net.Graphdb.security.AuthorizationExpiredException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	/// <summary>
	/// When STREAMING, a result is available as a stream of records.
	/// These must be PULLed or DISCARDed before any further statements
	/// can be executed.
	/// </summary>
	public class StreamingState : BoltStateMachineState
	{
		 private BoltStateMachineState _readyState;
		 private BoltStateMachineState _interruptedState;
		 private BoltStateMachineState _failedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is PullAllMessage )
			  {
					return ProcessPullAllMessage( context );
			  }
			  if ( message is DiscardAllMessage )
			  {
					return ProcessDiscardAllMessage( context );
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
			  return "STREAMING";
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

		 public virtual BoltStateMachineState FailedState
		 {
			 set
			 {
				  this._failedState = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processPullAllMessage(org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessPullAllMessage( StateMachineContext context )
		 {
			  return ProcessStreamResultMessage( true, context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processDiscardAllMessage(org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessDiscardAllMessage( StateMachineContext context )
		 {
			  return ProcessStreamResultMessage( false, context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processResetMessage(org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessResetMessage( StateMachineContext context )
		 {
			  bool success = context.ResetMachine();
			  return success ? _readyState : _failedState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processStreamResultMessage(boolean pull, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessStreamResultMessage( bool pull, StateMachineContext context )
		 {
			  try
			  {
					context.ConnectionState().StatementProcessor.streamResult(recordStream => context.ConnectionState().ResponseHandler.onRecords(recordStream, pull));

					return _readyState;
			  }
			  catch ( AuthorizationExpiredException e )
			  {
					context.HandleFailure( e, true );
					return _failedState;
			  }
			  catch ( Exception e )
			  {
					context.HandleFailure( e, false );
					return _failedState;
			  }
		 }

		 private void AssertInitialized()
		 {
			  checkState( _readyState != null, "Ready state not set" );
			  checkState( _interruptedState != null, "Interrupted state not set" );
			  checkState( _failedState != null, "Failed state not set" );
		 }
	}

}