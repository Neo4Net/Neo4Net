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
	using StatementMetadata = Neo4Net.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using AuthorizationExpiredException = Neo4Net.GraphDb.security.AuthorizationExpiredException;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isBegin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isCommit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isRollback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringArray;

	/// <summary>
	/// The READY state indicates that the connection is ready to accept a
	/// new RUN request. This is the "normal" state for a connection and
	/// becomes available after successful authorisation and when not
	/// executing another statement. It is this that ensures that statements
	/// must be executed in series and each must wait for the previous
	/// statement to complete.
	/// </summary>
	public class ReadyState : BoltStateMachineState
	{
		 private BoltStateMachineState _streamingState;
		 private BoltStateMachineState _interruptedState;
		 private BoltStateMachineState _failedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.runtime.BoltStateMachineState process(Neo4Net.bolt.messaging.RequestMessage message, Neo4Net.bolt.runtime.StateMachineContext context) throws Neo4Net.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is RunMessage )
			  {
					return ProcessRunMessage( ( RunMessage ) message, context );
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
			  return "READY";
		 }

		 public virtual BoltStateMachineState StreamingState
		 {
			 set
			 {
				  this._streamingState = value;
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
//ORIGINAL LINE: private Neo4Net.bolt.runtime.BoltStateMachineState processRunMessage(Neo4Net.bolt.v1.messaging.request.RunMessage message, Neo4Net.bolt.runtime.StateMachineContext context) throws Neo4Net.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessRunMessage( RunMessage message, StateMachineContext context )
		 {
			  try
			  {
					long start = context.Clock().millis();
					StatementMetadata statementMetadata = ProcessRunMessage( message, context.ConnectionState().StatementProcessor );
					long end = context.Clock().millis();

					context.ConnectionState().onMetadata("fields", stringArray(statementMetadata.FieldNames()));
					context.ConnectionState().onMetadata("result_available_after", Values.longValue(end - start));

					return _streamingState;
			  }
			  catch ( AuthorizationExpiredException e )
			  {
					context.HandleFailure( e, true );
					return _failedState;
			  }
			  catch ( Exception t )
			  {
					context.HandleFailure( t, false );
					return _failedState;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.bolt.runtime.StatementMetadata processRunMessage(Neo4Net.bolt.v1.messaging.request.RunMessage message, Neo4Net.bolt.runtime.StatementProcessor statementProcessor) throws Exception
		 private static StatementMetadata ProcessRunMessage( RunMessage message, StatementProcessor statementProcessor )
		 {
			  if ( isBegin( message ) )
			  {
					Bookmark bookmark = Bookmark.fromParamsOrNull( message.Params() );
					statementProcessor.BeginTransaction( bookmark );
					return StatementMetadata.EMPTY;
			  }
			  else if ( isCommit( message ) )
			  {
					statementProcessor.CommitTransaction();
					return StatementMetadata.EMPTY;
			  }
			  else if ( isRollback( message ) )
			  {
					statementProcessor.RollbackTransaction();
					return StatementMetadata.EMPTY;
			  }
			  else
			  {
					return statementProcessor.Run( message.Statement(), message.Params() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.bolt.runtime.BoltStateMachineState processResetMessage(Neo4Net.bolt.runtime.StateMachineContext context) throws Neo4Net.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineState ProcessResetMessage( StateMachineContext context )
		 {
			  bool success = context.ResetMachine();
			  return success ? this : _failedState;
		 }

		 private void AssertInitialized()
		 {
			  checkState( _streamingState != null, "Streaming state not set" );
			  checkState( _interruptedState != null, "Interrupted state not set" );
			  checkState( _failedState != null, "Failed state not set" );
		 }
	}

}