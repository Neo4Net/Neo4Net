﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.security.enterprise.auth.plugin.api
{

	using AuthPlugin = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.AuthPlugin;
	using AuthenticationPlugin = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;

	/// <summary>
	/// The authentication token provided by the client, which is used to authenticate the subject's identity.
	/// 
	/// <para>A common scenario is to have principal be a username and credentials be a password.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthenticationPlugin#authenticate(AuthToken) </seealso>
	/// <seealso cref= AuthPlugin#authenticateAndAuthorize(AuthToken) </seealso>
	public interface AuthToken
	{
		 /// <summary>
		 /// Returns the identity to authenticate.
		 /// 
		 /// <para>Most commonly this is a username.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the identity to authenticate. </returns>
		 string Principal();

		 /// <summary>
		 /// Returns the credentials that verifies the identity.
		 /// 
		 /// <para>Most commonly this is a password.
		 /// 
		 /// </para>
		 /// <para>The reason this is a character array and not a <seealso cref="string"/>, is so that sensitive information
		 /// can be cleared from memory after usage without having to wait for the garbage collector to act.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the credentials that verifies the identity. </returns>
		 char[] Credentials();

		 /// <summary>
		 /// Returns an optional custom parameter map if provided by the client.
		 /// 
		 /// <para>This can be used as a vehicle to send arbitrary auth data from a client application
		 /// to a server-side auth plugin. Neo4j will act as a pure transport and will not touch the contents of this map.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a custom parameter map (or an empty map if no parameters where provided by the client) </returns>
		 IDictionary<string, object> Parameters();
	}

}