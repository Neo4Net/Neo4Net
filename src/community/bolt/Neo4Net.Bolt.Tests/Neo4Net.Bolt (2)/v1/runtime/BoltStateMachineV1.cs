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
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;

	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionAuthFatality = Neo4Net.Bolt.runtime.BoltConnectionAuthFatality;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltProtocolBreachFatality = Neo4Net.Bolt.runtime.BoltProtocolBreachFatality;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using Neo4jError = Neo4Net.Bolt.runtime.Neo4jError;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using BoltStateMachineV1Context = Neo4Net.Bolt.v1.messaging.BoltStateMachineV1Context;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using AuthorizationExpiredException = Neo4Net.Graphdb.security.AuthorizationExpiredException;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// This state machine oversees the exchange of messages for the Bolt protocol.
	/// Central to this are the five active states -- CONNECTED, READY, STREAMING,
	/// FAILED and INTERRUPTED -- as well as the transitions between them which
	/// correspond to the Bolt protocol request messages INIT, ACK_FAILURE, RESET,
	/// RUN, DISCARD_ALL and PULL_ALL. Of particular note is RESET which exhibits
	/// dual behaviour in both marking the current query for termination and clearing
	/// down the current connection state.
	/// <para>
	/// To help ensure a secure protocol, any transition not explicitly defined here
	/// (i.e. a message sent out of sequence) will result in an immediate failure
	/// response and a closed connection.
	/// </para>
	/// </summary>
	public class BoltStateMachineV1 : BoltStateMachine
	{
		 private readonly string _id;
		 private readonly BoltChannel _boltChannel;
		 private readonly BoltStateMachineSPI _spi;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly MutableConnectionState ConnectionStateConflict;
		 private readonly StateMachineContext _context;

		 private BoltStateMachineState _state;
		 private readonly BoltStateMachineState _failedState;

		 public BoltStateMachineV1( BoltStateMachineSPI spi, BoltChannel boltChannel, Clock clock )
		 {
			  this._id = boltChannel.Id();
			  this._boltChannel = boltChannel;
			  this._spi = spi;
			  this.ConnectionStateConflict = new MutableConnectionState();
			  this._context = new BoltStateMachineV1Context( this, boltChannel, spi, ConnectionStateConflict, clock );

			  States states = BuildStates();
			  this._state = states.Initial;
			  this._failedState = states.Failed;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.BoltResponseHandler handler) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override void Process( RequestMessage message, BoltResponseHandler handler )
		 {
			  Before( handler );
			  try
			  {
					if ( message.SafeToProcessInAnyState() || ConnectionStateConflict.canProcessMessage() )
					{
						 NextState( message, _context );
					}
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void before(org.neo4j.bolt.runtime.BoltResponseHandler handler) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private void Before( BoltResponseHandler handler )
		 {
			  if ( ConnectionStateConflict.Terminated )
			  {
					Close();
			  }
			  else if ( ConnectionStateConflict.Interrupted )
			  {
					NextState( InterruptSignal.INSTANCE, _context );
			  }
			  ConnectionStateConflict.ResponseHandler = handler;
		 }

		 protected internal virtual void After()
		 {
			  if ( ConnectionStateConflict.ResponseHandler != null )
			  {
					try
					{
						 Neo4jError pendingError = ConnectionStateConflict.PendingError;
						 if ( pendingError != null )
						 {
							  ConnectionStateConflict.markFailed( pendingError );
						 }

						 if ( ConnectionStateConflict.hasPendingIgnore() )
						 {
							  ConnectionStateConflict.markIgnored();
						 }

						 ConnectionStateConflict.resetPendingFailedAndIgnored();
						 ConnectionStateConflict.ResponseHandler.onFinish();
					}
					finally
					{
						 ConnectionStateConflict.ResponseHandler = null;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void nextState(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private void NextState( RequestMessage message, StateMachineContext context )
		 {
			  BoltStateMachineState preState = _state;
			  _state = _state.process( message, context );
			  if ( _state == null )
			  {
					string msg = "Message '" + message + "' cannot be handled by a session in the " + preState.Name() + " state.";
					Fail( Neo4jError.fatalFrom( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, msg ) );
					throw new BoltProtocolBreachFatality( msg );
			  }
		 }

		 public override void MarkFailed( Neo4jError error )
		 {
			  Fail( error );
			  _state = _failedState;
		 }

		 /// <summary>
		 /// When this is invoked, the machine will make attempts
		 /// at interrupting any currently running action,
		 /// and will then ignore all inbound messages until a {@code RESET}
		 /// message is received. If this is called multiple times, an equivalent number
		 /// of reset messages must be received before the SSM goes back to a good state.
		 /// <para>
		 /// You can imagine this is as a "call ahead" mechanism used by RESET to
		 /// cancel any statements ahead of it in line, without compromising the single-
		 /// threaded processing of messages that the state machine does.
		 /// </para>
		 /// <para>
		 /// This can be used to cancel a long-running statement or transaction.
		 /// </para>
		 /// </summary>
		 public override void Interrupt()
		 {
			  ConnectionStateConflict.incrementInterruptCounter();
			  StatementProcessor().markCurrentTransactionForTermination();
		 }

		 /// <summary>
		 /// When this is invoked, the machine will check whether the related transaction is
		 /// marked for termination and will reset the TransactionStateMachine to AUTO_COMMIT mode
		 /// while releasing the related transactional resources.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void ValidateTransaction()
		 {
			  StatementProcessor().validateTransaction();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handleExternalFailure(org.neo4j.bolt.runtime.Neo4jError error, org.neo4j.bolt.runtime.BoltResponseHandler handler) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override void HandleExternalFailure( Neo4jError error, BoltResponseHandler handler )
		 {
			  Before( handler );
			  try
			  {
					if ( ConnectionStateConflict.canProcessMessage() )
					{
						 Fail( error );
						 _state = _failedState;
					}
			  }
			  finally
			  {
					After();
			  }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return ConnectionStateConflict.Closed;
			 }
		 }

		 public override void Close()
		 {
			  try
			  {
					_boltChannel.close();
			  }
			  finally
			  {
					ConnectionStateConflict.markClosed();
					// However a new transaction may have been created so we must always to reset
					ResetStatementProcessor();
			  }
		 }

		 public override string Id()
		 {
			  return _id;
		 }

		 public override void MarkForTermination()
		 {
			  /*
			   * This is a side-channel call and we should not close anything directly.
			   * Just mark the transaction and set isTerminated to true and then the session
			   * thread will close down the connection eventually.
			   */
			  ConnectionStateConflict.markTerminated();
			  StatementProcessor().markCurrentTransactionForTermination();
		 }

		 public override bool ShouldStickOnThread()
		 {
			  // Currently, we're doing our best to keep things together
			  // We should not switch threads when there's an active statement (executing/streaming)
			  // Also, we're currently sticking to the thread when there's an open transaction due to
			  // cursor errors we receive when a transaction is picked up by another thread linearly.
			  return StatementProcessor().hasTransaction() || StatementProcessor().hasOpenStatement();
		 }

		 public override bool HasOpenStatement()
		 {
			  return StatementProcessor().hasOpenStatement();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean reset() throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override bool Reset()
		 {
			  try
			  {
					ResetStatementProcessor();
					return true;
			  }
			  catch ( Exception t )
			  {
					HandleFailure( t, true );
					return false;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handleFailure(Throwable cause, boolean fatal) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override void HandleFailure( Exception cause, bool fatal )
		 {
			  if ( ExceptionUtils.indexOfType( cause, typeof( BoltConnectionFatality ) ) != -1 )
			  {
					fatal = true;
			  }

			  Neo4jError error = fatal ? Neo4jError.fatalFrom( cause ) : Neo4jError.from( cause );
			  Fail( error );

			  if ( error.Fatal )
			  {
					if ( ExceptionUtils.indexOfType( cause, typeof( AuthorizationExpiredException ) ) != -1 )
					{
						 throw new BoltConnectionAuthFatality( "Failed to process a bolt message", cause );
					}
					if ( cause is AuthenticationException )
					{
						 throw new BoltConnectionAuthFatality( ( AuthenticationException ) cause );
					}

					throw new BoltConnectionFatality( "Failed to process a bolt message", cause );
			  }
		 }

		 public virtual BoltStateMachineState State()
		 {
			  return _state;
		 }

		 public virtual StatementProcessor StatementProcessor()
		 {
			  return ConnectionStateConflict.StatementProcessor;
		 }

		 public virtual MutableConnectionState ConnectionState()
		 {
			  return ConnectionStateConflict;
		 }

		 private void Fail( Neo4jError neo4jError )
		 {
			  _spi.reportError( neo4jError );
			  if ( _state == _failedState )
			  {
					ConnectionStateConflict.markIgnored();
			  }
			  else
			  {
					ConnectionStateConflict.markFailed( neo4jError );
			  }
		 }

		 private void ResetStatementProcessor()
		 {
			  try
			  {
					StatementProcessor().reset();
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
		 }

		 protected internal virtual States BuildStates()
		 {
			  ConnectedState connected = new ConnectedState();
			  ReadyState ready = new ReadyState();
			  StreamingState streaming = new StreamingState();
			  FailedState failed = new FailedState();
			  InterruptedState interrupted = new InterruptedState();

			  connected.ReadyState = ready;
			  connected.FailedState = failed;

			  ready.StreamingState = streaming;
			  ready.InterruptedState = interrupted;
			  ready.FailedState = failed;

			  streaming.ReadyState = ready;
			  streaming.InterruptedState = interrupted;
			  streaming.FailedState = failed;

			  failed.ReadyState = ready;
			  failed.InterruptedState = interrupted;

			  interrupted.ReadyState = ready;
			  interrupted.FailedState = failed;

			  return new States( connected, failed );
		 }

		 public class States
		 {
			  internal readonly BoltStateMachineState Initial;
			  internal readonly BoltStateMachineState Failed;

			  public States( BoltStateMachineState initial, BoltStateMachineState failed )
			  {
					this.Initial = initial;
					this.Failed = failed;
			  }
		 }
	}

}