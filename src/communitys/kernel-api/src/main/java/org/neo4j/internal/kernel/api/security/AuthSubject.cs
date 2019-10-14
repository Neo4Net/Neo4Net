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
namespace Neo4Net.@internal.Kernel.Api.security
{
	public interface AuthSubject
	{
		 void Logout();

		 // TODO: Refine this API into something more polished
		 AuthenticationResult AuthenticationResult { get; }

		 /// <summary>
		 /// Changes the <seealso cref="AuthenticationResult"/> status to <seealso cref="AuthenticationResult.SUCCESS SUCCESS"/>
		 /// if it was <seealso cref="AuthenticationResult.PASSWORD_CHANGE_REQUIRED PASSWORD_CHANGE_REQUIRED"/>.
		 /// This allows users that changed their password to become authorized for continued processing.
		 /// </summary>
		 void SetPasswordChangeNoLongerRequired();

		 /// <param name="username"> a username </param>
		 /// <returns> true if the provided username is the underlying user name of this subject </returns>
		 bool HasUsername( string username );

		 /// <summary>
		 /// Get the username associated with the auth subject </summary>
		 /// <returns> the username </returns>
		 string Username();

		 /// <summary>
		 /// Implementation to use when authentication has not yet been performed. Allows nothing.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AuthSubject ANONYMOUS = new AuthSubject()
	//	 {
	//		  @@Override public void logout()
	//		  {
	//		  }
	//
	//		  @@Override public AuthenticationResult getAuthenticationResult()
	//		  {
	//				return AuthenticationResult.FAILURE;
	//		  }
	//
	//		  @@Override public void setPasswordChangeNoLongerRequired()
	//		  {
	//		  }
	//
	//		  @@Override public boolean hasUsername(String username)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public String username()
	//		  {
	//				return ""; // Should never clash with a valid username
	//		  }
	//
	//	 };

		 /// <summary>
		 /// Implementation to use when authentication is disabled. Allows everything.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AuthSubject AUTH_DISABLED = new AuthSubject()
	//	 {
	//		  @@Override public String username()
	//		  {
	//				return ""; // Should never clash with a valid username
	//		  }
	//
	//		  @@Override public void logout()
	//		  {
	//		  }
	//
	//		  @@Override public AuthenticationResult getAuthenticationResult()
	//		  {
	//				return AuthenticationResult.SUCCESS;
	//		  }
	//
	//		  @@Override public void setPasswordChangeNoLongerRequired()
	//		  {
	//		  }
	//
	//		  @@Override public boolean hasUsername(String username)
	//		  {
	//				return false;
	//		  }
	//	 };
	}

}