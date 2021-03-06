﻿using System;

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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using StatementMetadata = Org.Neo4j.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Org.Neo4j.Bolt.runtime.StatementProcessor;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;
	using AuthorizationExpiredException = Org.Neo4j.Graphdb.security.AuthorizationExpiredException;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.RunMessageChecker.isBegin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.RunMessageChecker.isCommit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.RunMessageChecker.isRollback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;

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
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
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
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processRunMessage(org.neo4j.bolt.v1.messaging.request.RunMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
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
//ORIGINAL LINE: private static org.neo4j.bolt.runtime.StatementMetadata processRunMessage(org.neo4j.bolt.v1.messaging.request.RunMessage message, org.neo4j.bolt.runtime.StatementProcessor statementProcessor) throws Exception
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
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processResetMessage(org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
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