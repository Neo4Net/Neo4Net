/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.Security.Auth
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AccessMode = Neo4Net.Internal.Kernel.Api.security.AccessMode;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OverriddenAccessMode = Neo4Net.Kernel.Impl.Api.security.OverriddenAccessMode;
	using RestrictedAccessMode = Neo4Net.Kernel.Impl.Api.security.RestrictedAccessMode;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.auth.SecurityTestUtils.authToken;

	public class SecurityContextDescriptionTest
	{
		 private BasicAuthManager _manager;
		 private SecurityContext _context;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _manager = new BasicAuthManager( new InMemoryUserRepository(), new BasicPasswordPolicy(), Clocks.systemClock(), new InMemoryUserRepository(), Config.defaults() );
			  _manager.init();
			  _manager.start();
			  _manager.newUser( "johan", password( "bar" ), false );
			  _context = _manager.login( authToken( "johan", "bar" ) ).authorize( s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  _manager.stop();
			  _manager.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescription()
		 public virtual void ShouldMakeNiceDescription()
		 {
			  assertThat( _context.description(), equalTo("user 'johan' with FULL") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionWithMode()
		 public virtual void ShouldMakeNiceDescriptionWithMode()
		 {
			  SecurityContext modified = _context.withMode( Neo4Net.Internal.Kernel.Api.security.AccessMode_Static.Write );
			  assertThat( modified.Description(), equalTo("user 'johan' with WRITE") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionRestricted()
		 public virtual void ShouldMakeNiceDescriptionRestricted()
		 {
			  SecurityContext restricted = _context.withMode( new RestrictedAccessMode( _context.mode(), Neo4Net.Internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( restricted.Description(), equalTo("user 'johan' with FULL restricted to READ") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionOverridden()
		 public virtual void ShouldMakeNiceDescriptionOverridden()
		 {
			  SecurityContext overridden = _context.withMode( new OverriddenAccessMode( _context.mode(), Neo4Net.Internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( overridden.Description(), equalTo("user 'johan' with FULL overridden by READ") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionAuthDisabled()
		 public virtual void ShouldMakeNiceDescriptionAuthDisabled()
		 {
			  SecurityContext disabled = SecurityContext.AUTH_DISABLED;
			  assertThat( disabled.Description(), equalTo("AUTH_DISABLED with FULL") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNiceDescriptionAuthDisabledAndRestricted()
		 public virtual void ShouldMakeNiceDescriptionAuthDisabledAndRestricted()
		 {
			  SecurityContext disabled = SecurityContext.AUTH_DISABLED;
			  SecurityContext restricted = disabled.WithMode( new RestrictedAccessMode( disabled.Mode(), Neo4Net.Internal.Kernel.Api.security.AccessMode_Static.Read ) );
			  assertThat( restricted.Description(), equalTo("AUTH_DISABLED with FULL restricted to READ") );
		 }

	}

}