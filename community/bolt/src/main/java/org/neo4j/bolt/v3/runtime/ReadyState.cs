﻿/*
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
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using StatementMetadata = Org.Neo4j.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Org.Neo4j.Bolt.runtime.StatementProcessor;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Values = Org.Neo4j.Values.Storable.Values;

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
	public class ReadyState : FailSafeBoltStateMachineState
	{
		 private BoltStateMachineState _streamingState;
		 private BoltStateMachineState _txReadyState;

		 internal const string FIELDS_KEY = "fields";
		 internal const string FIRST_RECORD_AVAILABLE_KEY = "t_first";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState processUnsafe(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws Exception
		 public override BoltStateMachineState ProcessUnsafe( RequestMessage message, StateMachineContext context )
		 {
			  if ( message is RunMessage )
			  {
					return ProcessRunMessage( ( RunMessage ) message, context );
			  }
			  if ( message is BeginMessage )
			  {
					return ProcessBeginMessage( ( BeginMessage ) message, context );
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

		 public virtual BoltStateMachineState TransactionReadyState
		 {
			 set
			 {
				  this._txReadyState = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processRunMessage(org.neo4j.bolt.v3.messaging.request.RunMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private BoltStateMachineState ProcessRunMessage( RunMessage message, StateMachineContext context )
		 {
			  long start = context.Clock().millis();
			  StatementProcessor statementProcessor = context.ConnectionState().StatementProcessor;
			  StatementMetadata statementMetadata = statementProcessor.Run( message.Statement(), message.Params(), message.Bookmark(), message.TransactionTimeout(), message.TransactionMetadata() );
			  long end = context.Clock().millis();

			  context.ConnectionState().onMetadata(FIELDS_KEY, stringArray(statementMetadata.FieldNames()));
			  context.ConnectionState().onMetadata(FIRST_RECORD_AVAILABLE_KEY, Values.longValue(end - start));

			  return _streamingState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.runtime.BoltStateMachineState processBeginMessage(org.neo4j.bolt.v3.messaging.request.BeginMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws Exception
		 private BoltStateMachineState ProcessBeginMessage( BeginMessage message, StateMachineContext context )
		 {
			  StatementProcessor statementProcessor = context.ConnectionState().StatementProcessor;
			  statementProcessor.BeginTransaction( message.Bookmark(), message.TransactionTimeout(), message.TransactionMetadata() );
			  return _txReadyState;
		 }

		 protected internal override void AssertInitialized()
		 {
			  checkState( _streamingState != null, "Streaming state not set" );
			  checkState( _txReadyState != null, "TransactionReady state not set" );
			  base.AssertInitialized();
		 }

	}

}