using System.Collections.Generic;

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
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.BoltAuthenticationHelper.processAuthentication;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.checkState;

	/// <summary>
	/// Following the socket connection and a small handshake exchange to
	/// establish protocol version, the machine begins in the CONNECTED
	/// state. The <em>only</em> valid transition from here is through a
	/// correctly authorised INIT into the READY state. Any other action
	/// results in disconnection.
	/// </summary>
	public class ConnectedState : BoltStateMachineState
	{
		 private BoltStateMachineState _readyState;
		 private BoltStateMachineState _failedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.runtime.BoltStateMachineState process(Neo4Net.bolt.messaging.RequestMessage message, Neo4Net.bolt.runtime.StateMachineContext context) throws Neo4Net.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();
			  if ( message is InitMessage )
			  {
					InitMessage initMessage = ( InitMessage ) message;
					string userAgent = initMessage.UserAgent();
					IDictionary<string, object> authToken = initMessage.AuthToken();

					if ( processAuthentication( userAgent, authToken, context ) )
					{
						 return _readyState;
					}
					else
					{
						 return _failedState;
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

		 public virtual BoltStateMachineState FailedState
		 {
			 set
			 {
				  this._failedState = value;
			 }
		 }

		 private void AssertInitialized()
		 {
			  checkState( _readyState != null, "Ready state not set" );
			  checkState( _failedState != null, "Failed state not set" );
		 }
	}

}