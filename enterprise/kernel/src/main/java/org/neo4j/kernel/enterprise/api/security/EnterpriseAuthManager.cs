using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.enterprise.api.security
{

	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;

	public interface EnterpriseAuthManager : AuthManager
	{
		 void ClearAuthCache();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EnterpriseLoginContext login(java.util.Map<String,Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException;
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