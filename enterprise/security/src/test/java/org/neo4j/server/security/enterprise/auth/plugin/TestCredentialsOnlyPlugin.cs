﻿using System;

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
namespace Org.Neo4j.Server.security.enterprise.auth.plugin
{

	using AuthToken = Org.Neo4j.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthenticationPlugin = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;
	using CustomCacheableAuthenticationInfo = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo;

	public class TestCredentialsOnlyPlugin : Org.Neo4j.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin_Adapter
	{
		 public override string Name()
		 {
			  return this.GetType().Name;
		 }

		 public override AuthenticationInfo Authenticate( AuthToken authToken )
		 {
			  string username = ValidateCredentials( authToken.Credentials() );
			  return new AuthenticationInfo( this, username, authToken.Credentials() );
		 }

		 /// <summary>
		 /// Performs decryptions of the credentials and returns the decrypted username if successful
		 /// </summary>
		 private string ValidateCredentials( char[] credentials )
		 {
			  return "trinity@MATRIX.NET";
		 }

		 [Serializable]
		 internal class AuthenticationInfo : CustomCacheableAuthenticationInfo, Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher
		 {
			 private readonly TestCredentialsOnlyPlugin _outerInstance;

			  internal readonly string Username;
			  internal readonly char[] Credentials;

			  internal AuthenticationInfo( TestCredentialsOnlyPlugin outerInstance, string username, char[] credentials )
			  {
				  this._outerInstance = outerInstance;
					this.Username = username;
					// Since the credentials array will be cleared we make need to make a copy here
					// (in a real world scenario you would probably not store this copy in clear text)
					this.Credentials = credentials.Clone();
			  }

			  public override object Principal()
			  {
					return Username;
			  }

			  public override spi.CustomCacheableAuthenticationInfo_CredentialsMatcher CredentialsMatcher()
			  {
					return this;
			  }

			  public override bool DoCredentialsMatch( AuthToken authToken )
			  {
					return Arrays.Equals( authToken.Credentials(), Credentials );
			  }
		 }
	}

}