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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using Values = Neo4Net.Values.Storable.Values;

	public class BoltAuthenticationHelper
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean processAuthentication(String userAgent, java.util.Map<String,Object> authToken, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public static bool ProcessAuthentication( string userAgent, IDictionary<string, object> authToken, StateMachineContext context )
		 {
			  try
			  {
					BoltStateMachineSPI boltSpi = context.BoltSpi();

					AuthenticationResult authResult = boltSpi.Authenticate( authToken );
					string username = authResult.LoginContext.subject().username();
					context.AuthenticatedAsUser( username, userAgent );

					StatementProcessor statementProcessor = new TransactionStateMachine( boltSpi.TransactionSpi(), authResult, context.Clock() );
					context.ConnectionState().StatementProcessor = statementProcessor;

					if ( authResult.CredentialsExpired() )
					{
						 context.ConnectionState().onMetadata("credentials_expired", Values.TRUE);
					}
					context.ConnectionState().onMetadata("server", Values.stringValue(boltSpi.Version()));
					boltSpi.UdcRegisterClient( userAgent );

					return true;
			  }
			  catch ( Exception t )
			  {
					context.HandleFailure( t, true );
					return false;
			  }
		 }
	}

}