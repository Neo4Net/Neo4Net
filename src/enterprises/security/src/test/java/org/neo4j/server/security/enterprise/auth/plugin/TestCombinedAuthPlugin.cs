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

	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthorizationExpiredException = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthorizationExpiredException;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using AuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationInfo;
	using AuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;
	using AuthorizationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationInfo;
	using AuthorizationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin;

	public class TestCombinedAuthPlugin : Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin_Adapter, AuthorizationPlugin
	{
		 public override string Name()
		 {
			  return this.GetType().Name;
		 }

		 public override AuthenticationInfo Authenticate( AuthToken authToken )
		 {
			  string principal = authToken.Principal();
			  char[] credentials = authToken.Credentials();

			  if ( principal.Equals( "neo4j" ) && Arrays.Equals( credentials, "neo4j".ToCharArray() ) )
			  {
					return AuthenticationInfo.of( "neo4j" );
			  }
			  else if ( principal.Equals( "authorization_expired_user" ) && Arrays.Equals( credentials, "neo4j".ToCharArray() ) )
			  {
					return ( AuthenticationInfo )() => "authorization_expired_user";
			  }
			  return null;
		 }

		 public override AuthorizationInfo Authorize( ICollection<spi.AuthorizationPlugin_PrincipalAndProvider> principals )
		 {
			  if ( principals.Any( p => "neo4j".Equals( p.principal() ) ) )
			  {
					return ( AuthorizationInfo )() => Collections.singleton(PredefinedRoles.READER);
			  }
			  else if ( principals.Any( p => "authorization_expired_user".Equals( p.principal() ) ) )
			  {
					throw new AuthorizationExpiredException( "authorization_expired_user needs to re-authenticate." );
			  }
			  return null;
		 }
	}

}