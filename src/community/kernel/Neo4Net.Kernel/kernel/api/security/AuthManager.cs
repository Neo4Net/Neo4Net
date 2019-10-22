﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.api.security
{

	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// An AuthManager is used to do basic authentication and user management.
	/// </summary>
	public interface AuthManager : Lifecycle
	{
		 /// <summary>
		 /// Log in using the provided authentication token
		 /// 
		 /// NOTE: The authToken will be cleared of any credentials
		 /// </summary>
		 /// <param name="authToken"> The authentication token to login with. Typically contains principals and credentials. </param>
		 /// <returns> An AuthSubject representing the newly logged-in user </returns>
		 /// <exception cref="InvalidAuthTokenException"> if the authentication token is malformed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.internal.kernel.api.security.LoginContext login(java.util.Map<String,Object> authToken) throws org.Neo4Net.kernel.api.security.exception.InvalidAuthTokenException;
		 LoginContext Login( IDictionary<string, object> authToken );

		 /// <summary>
		 /// Implementation that does no authentication.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AuthManager NO_AUTH = new AuthManager()
	//	 {
	//		  @@Override public void init()
	//		  {
	//		  }
	//
	//		  @@Override public void start()
	//		  {
	//		  }
	//
	//		  @@Override public void stop()
	//		  {
	//		  }
	//
	//		  @@Override public void shutdown()
	//		  {
	//		  }
	//
	//		  @@Override public LoginContext login(Map<String,Object> authToken)
	//		  {
	//				AuthToken.clearCredentials(authToken);
	//				return LoginContext.AUTH_DISABLED;
	//		  }
	//	 };
	}

}