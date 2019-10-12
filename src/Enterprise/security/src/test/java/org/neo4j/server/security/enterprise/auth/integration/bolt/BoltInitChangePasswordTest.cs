using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using BasicAuthentication = Neo4Net.Bolt.security.auth.BasicAuthentication;
	using Config = Neo4Net.Kernel.configuration.Config;
	using InMemoryUserRepository = Neo4Net.Server.Security.Auth.InMemoryUserRepository;
	using RateLimitedAuthenticationStrategy = Neo4Net.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using FullSecurityLog = Neo4Net.Server.security.enterprise.auth.MultiRealmAuthManagerRule.FullSecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class BoltInitChangePasswordTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.server.security.enterprise.auth.MultiRealmAuthManagerRule authManagerRule = new org.neo4j.server.security.enterprise.auth.MultiRealmAuthManagerRule(new org.neo4j.server.security.auth.InMemoryUserRepository(), new org.neo4j.server.security.auth.RateLimitedAuthenticationStrategy(java.time.Clock.systemUTC(), org.neo4j.kernel.configuration.Config.defaults()));
		 public MultiRealmAuthManagerRule AuthManagerRule = new MultiRealmAuthManagerRule( new InMemoryUserRepository(), new RateLimitedAuthenticationStrategy(Clock.systemUTC(), Config.defaults()) );
		 private BasicAuthentication _authentication;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _authentication = new BasicAuthentication( AuthManagerRule.Manager, AuthManagerRule.Manager );
			  AuthManagerRule.Manager.UserManager.newUser( "neo4j", password( "123" ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogInitPasswordChange() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogInitPasswordChange()
		 {
			  _authentication.authenticate( AuthToken( "neo4j", "123", "secret" ) );

			  MultiRealmAuthManagerRule.FullSecurityLog fullLog = AuthManagerRule.getFullSecurityLog();
			  fullLog.AssertHasLine( "neo4j", "logged in (password change required)" );
			  fullLog.AssertHasLine( "neo4j", "changed password" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailedInitPasswordChange()
		 public virtual void ShouldLogFailedInitPasswordChange()
		 {
			  assertException( () => _authentication.authenticate(AuthToken("neo4j", "123", "123")), typeof(AuthenticationException), "Old password and new password cannot be the same." );

			  MultiRealmAuthManagerRule.FullSecurityLog fullLog = AuthManagerRule.getFullSecurityLog();
			  fullLog.AssertHasLine( "neo4j", "logged in (password change required)" );
			  fullLog.AssertHasLine( "neo4j", "tried to change password: Old password and new password cannot be the same." );
		 }

		 private IDictionary<string, object> AuthToken( string username, string password, string newPassword )
		 {
			  return map( "principal", username, "credentials", password( password ), "new_credentials", password( newPassword ), "scheme", "basic" );
		 }
	}

}