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
namespace Neo4Net.Bolt.v3.runtime
{

	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.BoltAuthenticationHelper.processAuthentication;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	/// <summary>
	/// Following the socket connection and a small handshake exchange to
	/// establish protocol version, the machine begins in the CONNECTED
	/// state. The <em>only</em> valid transition from here is through a
	/// correctly authorised HELLO into the READY state. Any other action
	/// results in disconnection.
	/// </summary>
	public class ConnectedState : BoltStateMachineState
	{
		 private const string CONNECTION_ID_KEY = "connection_id";

		 private BoltStateMachineState _readyState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is HelloMessage )
			  {
					HelloMessage helloMessage = ( HelloMessage ) message;
					string userAgent = helloMessage.UserAgent();
					IDictionary<string, object> authToken = helloMessage.AuthToken();

					if ( processAuthentication( userAgent, authToken, context ) )
					{
						 context.ConnectionState().onMetadata(CONNECTION_ID_KEY, Values.stringValue(context.ConnectionId()));
						 return _readyState;
					}
					else
					{
						 return null;
					}
			  }
			  return null;
		 }

		 public override string Name()
		 {
			  return "CONNECTED";
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