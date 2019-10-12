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
namespace Org.Neo4j.Bolt.runtime
{

	using AuthenticationException = Org.Neo4j.Bolt.security.auth.AuthenticationException;
	using AuthenticationResult = Org.Neo4j.Bolt.security.auth.AuthenticationResult;

	public interface BoltStateMachineSPI
	{
		 TransactionStateMachineSPI TransactionSpi();

		 void ReportError( Neo4jError err );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.bolt.security.auth.AuthenticationResult authenticate(java.util.Map<String,Object> authToken) throws org.neo4j.bolt.security.auth.AuthenticationException;
		 AuthenticationResult Authenticate( IDictionary<string, object> authToken );

		 void UdcRegisterClient( string clientName );

		 string Version();
	}

}