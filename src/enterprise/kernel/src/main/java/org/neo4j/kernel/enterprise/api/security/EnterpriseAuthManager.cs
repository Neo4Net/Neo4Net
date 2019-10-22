﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.enterprise.api.security
{

	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;

	public interface EnterpriseAuthManager : AuthManager
	{
		 void ClearAuthCache();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EnterpriseLoginContext login(java.util.Map<String,Object> authToken) throws org.Neo4Net.kernel.api.security.exception.InvalidAuthTokenException;
		 EnterpriseLoginContext Login( IDictionary<string, object> authToken );

		 /// <summary>
		 /// Implementation that does no authentication.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 EnterpriseAuthManager NO_AUTH = new EnterpriseAuthManager()
	//	 {
	//		  @@Override public EnterpriseLoginContext login(Map<String,Object> authToken)
	//		  {
	//				AuthToken.clearCredentials(authToken);
	//				return EnterpriseLoginContext.AUTH_DISABLED;
	//		  }
	//
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
	//		  @@Override public void clearAuthCache()
	//		  {
	//		  }
	//	 };
	}

}