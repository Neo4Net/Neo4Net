﻿/*
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
	using SimpleHash = org.apache.shiro.crypto.hash.SimpleHash;
	using ByteSource = org.apache.shiro.util.ByteSource;

	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;
	using AuthenticationInfo = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.AuthenticationInfo;
	using CacheableAuthenticationInfo = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CacheableAuthenticationInfo;
	using CustomCacheableAuthenticationInfo = Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo;

	internal class PluginAuthenticationInfo : ShiroAuthenticationInfo, CustomCredentialsMatcherSupplier
	{
		 private Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher _credentialsMatcher;

		 private PluginAuthenticationInfo( object principal, string realmName, Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher credentialsMatcher ) : base( principal, realmName, AuthenticationResult.SUCCESS )
		 {
			  this._credentialsMatcher = credentialsMatcher;
		 }

		 private PluginAuthenticationInfo( object principal, object hashedCredentials, ByteSource credentialsSalt, string realmName ) : base( principal, hashedCredentials, credentialsSalt, realmName, AuthenticationResult.SUCCESS )
		 {
		 }

		 public virtual Org.Neo4j.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher CredentialsMatcher
		 {
			 get
			 {
				  return _credentialsMatcher;
			 }
		 }

		 private static PluginAuthenticationInfo Create( AuthenticationInfo authenticationInfo, string realmName )
		 {
			  return new PluginAuthenticationInfo( authenticationInfo.Principal(), realmName, null );
		 }

		 private static PluginAuthenticationInfo Create( AuthenticationInfo authenticationInfo, SimpleHash hashedCredentials, string realmName )
		 {
			  return new PluginAuthenticationInfo( authenticationInfo.Principal(), hashedCredentials.Bytes, hashedCredentials.Salt, realmName );
		 }

		 public static PluginAuthenticationInfo CreateCacheable( AuthenticationInfo authenticationInfo, string realmName, SecureHasher secureHasher )
		 {
			  if ( authenticationInfo is CustomCacheableAuthenticationInfo )
			  {
					CustomCacheableAuthenticationInfo info = ( CustomCacheableAuthenticationInfo ) authenticationInfo;
					return new PluginAuthenticationInfo( authenticationInfo.Principal(), realmName, info.CredentialsMatcher() );
			  }
			  else if ( authenticationInfo is CacheableAuthenticationInfo )
			  {
					sbyte[] credentials = ( ( CacheableAuthenticationInfo ) authenticationInfo ).credentials();
					SimpleHash hashedCredentials = secureHasher.Hash( credentials );
					return PluginAuthenticationInfo.Create( authenticationInfo, hashedCredentials, realmName );
			  }
			  else
			  {
					return PluginAuthenticationInfo.Create( authenticationInfo, realmName );
			  }
		 }
	}

}