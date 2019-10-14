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
namespace Neo4Net.Bolt.security.auth
{
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;

	public interface AuthenticationResult
	{
		 LoginContext LoginContext { get; }

		 bool CredentialsExpired();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AuthenticationResult AUTH_DISABLED = new AuthenticationResult()
	//	 {
	//		  @@Override public LoginContext getLoginContext()
	//		  {
	//				return LoginContext.AUTH_DISABLED;
	//		  }
	//
	//		  @@Override public boolean credentialsExpired()
	//		  {
	//				return false;
	//		  }
	//	 };
	}

}