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
namespace Neo4Net.Server.security.enterprise.auth
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseSecurityContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using OverriddenAccessMode = Neo4Net.Kernel.Impl.Api.security.OverriddenAccessMode;
	using RestrictedAccessMode = Neo4Net.Kernel.Impl.Api.security.RestrictedAccessMode;
	using InMemoryUserRepository = Neo4Net.Server.Security.Auth.InMemoryUserRepository;
	using RateLimitedAuthenticationStrategy = Neo4Net.Server.Security.Auth.RateLimitedAuthenticationStrategy;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;

	public class EnterpriseSecurityContextDescriptionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public MultiRealmAuthManagerRule authManagerRule = new MultiRealmAuthManagerRule(new org.neo4j.server.security.auth.InMemoryUserRepository(), new org.neo4j.server.security.auth.RateLimitedAuthenticationStrategy(java.time.Clock.systemUTC(), org.neo4j.kernel.configuration.Config.defaults()));
		 public MultiRealmAuthManagerRule AuthManagerRule = new MultiRealmAuthManagerRule( new InMemoryUserRepository(), new RateLimitedAuthenticationStrategy(Clock.systemUTC(), Config.defaults()) );

		 private EnterpriseUserManager _manager;
		 private readonly System.Func<string, int> _token = s => -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  AuthManagerRule.Manager.start();
			  _manager = AuthManagerRule.Manager.UserManager;
			  _manager.newUser( "mats", password( "foo" ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionWithoutRoles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNiceDescriptionWithoutRoles()
		 {
			  assertThat( Context().description(), equalTo("user 'mats' with no roles") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionWithRoles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNiceDescriptionWithRoles()
		 {
			  _manager.newRole( "role1", "mats" );
			  _manager.addRoleToUser( PUBLISHER, "mats" );

			  assertThat( Context().description(), equalTo("user 'mats' with roles [publisher,role1]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionWithMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNiceDescriptionWithMode()
		 {
			  _manager.newRole( "role1", "mats" );
			  _manager.addRoleToUser( PUBLISHER, "mats" );

			  EnterpriseSecurityContext modified = Context().withMode(Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.CredentialsExpired);
			  assertThat( modified.Description(), equalTo("user 'mats' with CREDENTIALS_EXPIRED") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionRestricted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNiceDescriptionRestricted()
		 {
			  _manager.newRole( "role1", "mats" );
			  _manager.addRoleToUser( PUBLISHER, "mats" );

			  EnterpriseSecurityContext context = context();
			  EnterpriseSecurityContext restricted = context.WithMode( new RestrictedAccessMode( context.Mode(), Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( restricted.Description(), equalTo("user 'mats' with roles [publisher,role1] restricted to READ") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionOverridden() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNiceDescriptionOverridden()
		 {
			  _manager.newRole( "role1", "mats" );
			  _manager.addRoleToUser( PUBLISHER, "mats" );

			  EnterpriseSecurityContext context = context();
			  EnterpriseSecurityContext overridden = context.WithMode( new OverriddenAccessMode( context.Mode(), Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( overridden.Description(), equalTo("user 'mats' with roles [publisher,role1] overridden by READ") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionAuthDisabled()
		 public virtual void ShouldMakeNiceDescriptionAuthDisabled()
		 {
			  EnterpriseSecurityContext disabled = EnterpriseSecurityContext.AUTH_DISABLED;
			  assertThat( disabled.Description(), equalTo("AUTH_DISABLED with FULL") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionAuthDisabledAndRestricted()
		 public virtual void ShouldMakeNiceDescriptionAuthDisabledAndRestricted()
		 {
			  EnterpriseSecurityContext disabled = EnterpriseSecurityContext.AUTH_DISABLED;
			  EnterpriseSecurityContext restricted = disabled.WithMode( new RestrictedAccessMode( disabled.Mode(), Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( restricted.Description(), equalTo("AUTH_DISABLED with FULL restricted to READ") );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.enterprise.api.security.EnterpriseSecurityContext context() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private EnterpriseSecurityContext Context()
		 {
			  return AuthManagerRule.Manager.login( authToken( "mats", "foo" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
		 }
	}

}