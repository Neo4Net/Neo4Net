using System.Collections.Generic;

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
	using SimpleHash = org.apache.shiro.crypto.hash.SimpleHash;
	using Test = org.junit.Test;

	using AuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationInfo;
	using CacheableAuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.CacheableAuthenticationInfo;
	using CustomCacheableAuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class PluginAuthenticationInfoTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateCorrectAuthenticationInfo()
		 public virtual void ShouldCreateCorrectAuthenticationInfo()
		 {
			  PluginAuthenticationInfo internalAuthInfo = PluginAuthenticationInfo.CreateCacheable( AuthenticationInfo.of( "thePrincipal" ), "theRealm", null );

			  assertThat( ( IList<string> )internalAuthInfo.Principals.asList(), containsInAnyOrder("thePrincipal") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateCorrectAuthenticationInfoFromCacheable()
		 public virtual void ShouldCreateCorrectAuthenticationInfoFromCacheable()
		 {
			  SecureHasher hasher = mock( typeof( SecureHasher ) );
			  when( hasher.Hash( any() ) ).thenReturn(new SimpleHash("some-hash"));

			  PluginAuthenticationInfo internalAuthInfo = PluginAuthenticationInfo.CreateCacheable( CacheableAuthenticationInfo.of( "thePrincipal", new sbyte[]{ 1 } ), "theRealm", hasher );

			  assertThat( ( IList<string> )internalAuthInfo.Principals.asList(), containsInAnyOrder("thePrincipal") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateCorrectAuthenticationInfoFromCustomCacheable()
		 public virtual void ShouldCreateCorrectAuthenticationInfoFromCustomCacheable()
		 {
			  SecureHasher hasher = mock( typeof( SecureHasher ) );
			  when( hasher.Hash( any() ) ).thenReturn(new SimpleHash("some-hash"));

			  PluginAuthenticationInfo internalAuthInfo = PluginAuthenticationInfo.CreateCacheable( CustomCacheableAuthenticationInfo.of( "thePrincipal", ignoredAuthToken => true ), "theRealm", hasher );

			  assertThat( ( IList<string> )internalAuthInfo.Principals.asList(), containsInAnyOrder("thePrincipal") );
		 }
	}

}