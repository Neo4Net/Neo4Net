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
namespace Neo4Net.Bolt.v3.runtime
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using StatementMetadata = Neo4Net.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using CommitMessage = Neo4Net.Bolt.v3.messaging.request.CommitMessage;
	using RollbackMessage = Neo4Net.Bolt.v3.messaging.request.RollbackMessage;
	using RunMessage = Neo4Net.Bolt.v3.messaging.request.RunMessage;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.runtime.ReadyState.FIELDS_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.runtime.ReadyState.FIRST_RECORD_AVAILABLE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringArray;

	public class TransactionReadyState : FailSafeBoltStateMachineState
	{
		 private BoltStateMachineState _streamingState;
		 private BoltStateMachineState _readyState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.runtime.BoltStateMachineState processUnsafe(org.Neo4Net.bolt.messaging.RequestMessage message, org.Neo4Net.bolt.runtime.StateMachineContext context) throws Exception
		 public override BoltStateMachineState ProcessUnsafe( RequestMessage message, StateMachineContext context )
		 {
			  if ( message is RunMessage )
			  {
					return ProcessRunMessage( ( RunMessage ) message, context );
			  }
			  if ( message is CommitMessage )
			  {
					return ProcessCommitMessage( context );
			  }
			  if ( message is RollbackMessage )
			  {
					return ProcessRollbackMessage( context );
			  }
			  return null;
		 }

		 public override string Name()
		 {
			  return "TX_READY";
		 }

		 public virtual BoltStateMachineState TransactionStreamingState
		 {
			 set
			 {
				  this._streamingState = value;
			 }
		 }

		 public virtual BoltStateMachineState ReadyState
		 {
			 set
			 {
				  this._readyState = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.bolt.runtime.BoltStateMachineState processRunMessage(org.Neo4Net.bolt.v3.messaging.request.RunMessage message, org.Neo4Net.bolt.runtime.StateMachineContext context) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private BoltStateMachineState ProcessRunMessage( RunMessage message, StateMachineContext context )
		 {
			  long start = context.Clock().millis();
			  StatementProcessor statementProcessor = context.ConnectionState().StatementProcessor;
			  StatementMetadata statementMetadata = statementProcessor.Run( message.Statement(), message.Params() );
			  long end = context.Clock().millis();

			  context.ConnectionState().onMetadata(FIELDS_KEY, stringArray(statementMetadata.FieldNames()));
			  context.ConnectionState().onMetadata(FIRST_RECORD_AVAILABLE_KEY, Values.longValue(end - start));
			  return _streamingState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.bolt.runtime.BoltStateMachineState processCommitMessage(org.Neo4Net.bolt.runtime.StateMachineContext context) throws Exception
		 private BoltStateMachineState ProcessCommitMessage( StateMachineContext context )
		 {
			  StatementProcessor statementProcessor = context.ConnectionState().StatementProcessor;
			  Bookmark bookmark = statementProcessor.CommitTransaction();
			  bookmark.AttachTo( context.ConnectionState() );
			  return _readyState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.bolt.runtime.BoltStateMachineState processRollbackMessage(org.Neo4Net.bolt.runtime.StateMachineContext context) throws Exception
		 private BoltStateMachineState ProcessRollbackMessage( StateMachineContext context )
		 {
			  StatementProcessor statementProcessor = context.ConnectionState().StatementProcessor;
			  statementProcessor.RollbackTransaction();
			  return _readyState;
		 }

		 protected internal override void AssertInitialized()
		 {
			  checkState( _streamingState != null, "Streaming state not set" );
			  checkState( _readyState != null, "Ready state not set" );
			  base.AssertInitialized();
		 }
	}

}