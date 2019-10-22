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
namespace Neo4Net.Bolt.runtime
{

	using AnyValue = Neo4Net.Values.AnyValue;

	/// <summary>
	/// Keeps state of the connection and bolt state machine.
	/// </summary>
	public class MutableConnectionState : BoltResponseHandler
	{
		 private Neo4NetError _pendingError;
		 private bool _pendingIgnore;
		 private volatile bool _terminated;
		 private bool _closed;

		 /// <summary>
		 /// Callback poised to receive the next response.
		 /// </summary>
		 private BoltResponseHandler _responseHandler;
		 /// <summary>
		 /// Component responsible for transaction handling and statement execution.
		 /// </summary>
		 private StatementProcessor _statementProcessor = StatementProcessor.EMPTY;
		 /// <summary>
		 /// This is incremented each time <seealso cref="BoltStateMachine.interrupt()"/> is called,
		 /// and decremented each time a {@code RESET} message
		 /// arrives. When this is above 0, all messages will be ignored.
		 /// This way, when a reset message arrives on the network, interrupt
		 /// can be called to "purge" all the messages ahead of the reset message.
		 /// </summary>
		 private readonly AtomicInteger _interruptCounter = new AtomicInteger();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void onRecords(BoltResult result, boolean pull) throws Exception
		 public override void OnRecords( BoltResult result, bool pull )
		 {
			  if ( _responseHandler != null )
			  {
					_responseHandler.onRecords( result, pull );
			  }
		 }

		 public override void OnMetadata( string key, AnyValue value )
		 {
			  if ( _responseHandler != null )
			  {
					_responseHandler.onMetadata( key, value );
			  }
		 }

		 public override void MarkIgnored()
		 {
			  if ( _responseHandler != null )
			  {
					_responseHandler.markIgnored();
			  }
			  else
			  {
					_pendingIgnore = true;
			  }
		 }

		 public override void MarkFailed( Neo4NetError error )
		 {
			  if ( _responseHandler != null )
			  {
					_responseHandler.markFailed( error );
			  }
			  else
			  {
					_pendingError = error;
			  }
		 }

		 public override void OnFinish()
		 {
			  if ( _responseHandler != null )
			  {
					_responseHandler.onFinish();
			  }
		 }

		 public virtual Neo4NetError PendingError
		 {
			 get
			 {
				  return _pendingError;
			 }
		 }

		 public virtual bool HasPendingIgnore()
		 {
			  return _pendingIgnore;
		 }

		 public virtual void ResetPendingFailedAndIgnored()
		 {
			  _pendingError = null;
			  _pendingIgnore = false;
		 }

		 public virtual bool CanProcessMessage()
		 {
			  return !_closed && _pendingError == null && !_pendingIgnore;
		 }

		 public virtual BoltResponseHandler ResponseHandler
		 {
			 get
			 {
				  return _responseHandler;
			 }
			 set
			 {
				  this._responseHandler = value;
			 }
		 }


		 public virtual StatementProcessor StatementProcessor
		 {
			 get
			 {
				  return _statementProcessor;
			 }
			 set
			 {
				  this._statementProcessor = value;
			 }
		 }


		 public virtual bool Interrupted
		 {
			 get
			 {
				  return _interruptCounter.get() > 0;
			 }
		 }

		 public virtual int IncrementInterruptCounter()
		 {
			  return _interruptCounter.incrementAndGet();
		 }

		 public virtual int DecrementInterruptCounter()
		 {
			  return _interruptCounter.decrementAndGet();
		 }

		 public virtual bool Terminated
		 {
			 get
			 {
				  return _terminated;
			 }
		 }

		 public virtual void MarkTerminated()
		 {
			  _terminated = true;
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _closed;
			 }
		 }

		 public virtual void MarkClosed()
		 {
			  _closed = true;
		 }
	}

}