/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth.plugin
{

	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationInfo;
	using AuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;
	using CustomCacheableAuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo;

	public class TestCustomCacheableAuthenticationPlugin : Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin_CachingEnabledAdapter
	{
		 public override string Name()
		 {
			  return this.GetType().Name;
		 }

		 public override AuthenticationInfo Authenticate( AuthToken authToken )
		 {
			  GetAuthenticationInfoCallCount.incrementAndGet();

			  string principal = authToken.Principal();
			  char[] credentials = authToken.Credentials();

			  if ( principal.Equals( "Neo4Net" ) && Arrays.Equals( credentials, "Neo4Net".ToCharArray() ) )
			  {
					return CustomCacheableAuthenticationInfo.of("Neo4Net", token =>
					{
								char[] tokenCredentials = token.credentials();
								return tokenCredentials.SequenceEqual( "Neo4Net".ToCharArray() );
					});
			  }
			  return null;
		 }

		 // For testing purposes
		 public static AtomicInteger GetAuthenticationInfoCallCount = new AtomicInteger( 0 );
	}

}