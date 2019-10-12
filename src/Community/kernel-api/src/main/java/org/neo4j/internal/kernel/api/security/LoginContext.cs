﻿/*
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
namespace Neo4Net.@internal.Kernel.Api.security
{

	/// <summary>
	/// The LoginContext hold the executing authenticated user (subject).
	/// By calling <seealso cref="authorize(ToIntFunction, string)"/> the user is also authorized, and a full SecurityContext is returned,
	/// which can be used to assert user permissions during query execution.
	/// </summary>
	public interface LoginContext
	{
		 /// <summary>
		 /// Get the authenticated user.
		 /// </summary>
		 AuthSubject Subject();

		 /// <summary>
		 /// Authorize the user and return a SecurityContext.
		 /// </summary>
		 /// <param name="propertyIdLookup"> token lookup, used to compile property level security verification </param>
		 /// <param name="dbName"> the name of the database the user should be authorized against </param>
		 /// <returns> the security context </returns>
		 SecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LoginContext AUTH_DISABLED = new LoginContext()
	//	 {
	//		  @@Override public AuthSubject subject()
	//		  {
	//				return AuthSubject.AUTH_DISABLED;
	//		  }
	//
	//		  @@Override public SecurityContext authorize(ToIntFunction<String> propertyIdLookup, String dbName)
	//		  {
	//				return SecurityContext.AUTH_DISABLED;
	//		  }
	//	 };
	}

}