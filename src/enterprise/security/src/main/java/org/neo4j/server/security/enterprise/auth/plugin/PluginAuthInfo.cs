using System.Collections.Generic;

/*
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
namespace Neo4Net.Server.security.enterprise.auth.plugin
{
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using Permission = org.apache.shiro.authz.Permission;
	using SimpleHash = org.apache.shiro.crypto.hash.SimpleHash;
	using ByteSource = org.apache.shiro.util.ByteSource;


	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;
	using AuthInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthInfo;
	using CacheableAuthInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.CacheableAuthInfo;

	public class PluginAuthInfo : ShiroAuthenticationInfo, AuthorizationInfo
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal ISet<string> RolesConflict;

		 private PluginAuthInfo( object principal, string realmName, ISet<string> roles ) : base( principal, realmName, AuthenticationResult.SUCCESS )
		 {
			  this.RolesConflict = roles;
		 }

		 private PluginAuthInfo( object principal, object hashedCredentials, ByteSource credentialsSalt, string realmName, ISet<string> roles ) : base( principal, hashedCredentials, credentialsSalt, realmName, AuthenticationResult.SUCCESS )
		 {
			  this.RolesConflict = roles;
		 }

		 private PluginAuthInfo( AuthInfo authInfo, SimpleHash hashedCredentials, string realmName ) : this( authInfo.Principal(), hashedCredentials.Bytes, hashedCredentials.Salt, realmName, new HashSet<string>(authInfo.Roles()) )
		 {
		 }

		 public static PluginAuthInfo Create( AuthInfo authInfo, string realmName )
		 {
			  return new PluginAuthInfo( authInfo.Principal(), realmName, new HashSet<>(authInfo.Roles()) );
		 }

		 public static PluginAuthInfo CreateCacheable( AuthInfo authInfo, string realmName, SecureHasher secureHasher )
		 {
			  if ( authInfo is CacheableAuthInfo )
			  {
					sbyte[] credentials = ( ( CacheableAuthInfo ) authInfo ).credentials();
					SimpleHash hashedCredentials = secureHasher.Hash( credentials );
					return new PluginAuthInfo( authInfo, hashedCredentials, realmName );
			  }
			  else
			  {
					return new PluginAuthInfo( authInfo.Principal(), realmName, new HashSet<>(authInfo.Roles()) );
			  }
		 }

		 public override ICollection<string> Roles
		 {
			 get
			 {
				  return RolesConflict;
			 }
		 }

		 public override ICollection<string> StringPermissions
		 {
			 get
			 {
				  return Collections.emptyList();
			 }
		 }

		 public override ICollection<Permission> ObjectPermissions
		 {
			 get
			 {
				  return Collections.emptyList();
			 }
		 }
	}

}