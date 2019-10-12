using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.messaging
{

	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using Neo4jError = Neo4Net.Bolt.runtime.Neo4jError;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using FatalFailureMessage = Neo4Net.Bolt.v1.messaging.response.FatalFailureMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using PackOutputClosedException = Neo4Net.Bolt.v1.packstream.PackOutputClosedException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.response.IgnoredMessage.IGNORED_MESSAGE;

	public class MessageProcessingHandler : BoltResponseHandler
	{
		 // Errors that are expected when the client disconnects mid-operation
		 private static readonly ISet<Status> _clientMidOpDisconnectErrors = new HashSet<Status>( Arrays.asList( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated, Neo4Net.Kernel.Api.Exceptions.Status_Transaction.LockClientStopped ) );
		 private readonly MapValueBuilder _metadata = new MapValueBuilder();

		 protected internal readonly Log Log;
		 protected internal readonly BoltConnection Connection;
		 protected internal readonly BoltResponseMessageWriter MessageWriter;

		 private Neo4jError _error;
		 private bool _ignored;

		 public MessageProcessingHandler( BoltResponseMessageWriter messageWriter, BoltConnection connection, Log logger )
		 {
			  this.MessageWriter = messageWriter;
			  this.Connection = connection;
			  this.Log = logger;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void onRecords(org.neo4j.bolt.runtime.BoltResult result, boolean pull) throws Exception
		 public override void OnRecords( BoltResult result, bool pull )
		 {
		 }

		 public override void OnMetadata( string key, AnyValue value )
		 {
			  _metadata.add( key, value );
		 }

		 public override void MarkIgnored()
		 {
			  this._ignored = true;
		 }

		 public override void MarkFailed( Neo4jError error )
		 {
			  this._error = error;
		 }

		 public override void OnFinish()
		 {
			  try
			  {
					if ( _ignored )
					{
						 MessageWriter.write( IGNORED_MESSAGE );
					}
					else if ( _error != null )
					{
						 PublishError( MessageWriter, _error );
					}
					else
					{
						 MessageWriter.write( new SuccessMessage( Metadata ) );
					}
			  }
			  catch ( Exception e )
			  {
					Connection.stop();
					Log.error( "Failed to write response to driver", e );
			  }
			  finally
			  {
					ClearState();
			  }
		 }

		 internal virtual MapValue Metadata
		 {
			 get
			 {
				  return _metadata.build();
			 }
		 }

		 private void ClearState()
		 {
			  _error = null;
			  _ignored = false;
			  _metadata.clear();
		 }

		 private void PublishError( BoltResponseMessageWriter messageWriter, Neo4jError error )
		 {
			  try
			  {
					if ( error.Fatal )
					{
						 messageWriter.Write( new FatalFailureMessage( error.Status(), error.Message() ) );
					}
					else
					{
						 messageWriter.Write( new FailureMessage( error.Status(), error.Message() ) );
					}
			  }
			  catch ( PackOutputClosedException e )
			  {
					// Can't write error to the client, because the connection is closed.
					// Very likely our error is related to the connection being closed.

					// If the error is that the transaction was terminated, then the error is a side-effect of
					// us cleaning up stuff that was running when the client disconnected. Log a warning without
					// stack trace to highlight clients are disconnecting while stuff is running:
					if ( _clientMidOpDisconnectErrors.Contains( error.Status() ) )
					{
						 Log.warn( "Client %s disconnected while query was running. Session has been cleaned up. " + "This can be caused by temporary network problems, but if you see this often, " + "ensure your applications are properly waiting for operations to complete before exiting.", e.ClientAddress() );
						 return;
					}

					// If the error isn't that the tx was terminated, log it to the console for debugging. It's likely
					// there are other "ok" errors that we can whitelist into the conditional above over time.
					Log.warn( "Unable to send error back to the client. " + e.Message, error.Cause() );
			  }
			  catch ( Exception t )
			  {
					// some unexpected error happened while writing exception back to the client
					// log it together with the original error being suppressed
					t.addSuppressed( error.Cause() );
					Log.error( "Unable to send error back to the client", t );
			  }
		 }
	}

}