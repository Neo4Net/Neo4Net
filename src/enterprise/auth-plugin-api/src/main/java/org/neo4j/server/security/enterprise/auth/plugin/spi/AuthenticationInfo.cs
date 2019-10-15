﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.security.enterprise.auth.plugin.spi
{

	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;

	/// <summary>
	/// An object that can be returned as the result of successful authentication by an <seealso cref="AuthenticationPlugin"/>.
	/// </summary>
	/// <seealso cref= AuthenticationPlugin#authenticate(AuthToken) </seealso>
	public interface AuthenticationInfo
	{
		 /// <summary>
		 /// Should return a principal that uniquely identifies the authenticated subject within this authentication
		 /// provider.
		 /// 
		 /// <para>Typically this is the same as the principal within the auth token map.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a principal that uniquely identifies the authenticated subject within this authentication provider. </returns>
		 object Principal();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static AuthenticationInfo of(Object principal)
	//	 {
	//		  return (AuthenticationInfo)() -> principal;
	//	 }
	}

}