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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using Log = Org.Neo4j.Logging.Log;
	using BasicPasswordPolicy = Org.Neo4j.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Org.Neo4j.Server.Security.Auth.InMemoryUserRepository;
	using RateLimitedAuthenticationStrategy = Org.Neo4j.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using SecurityLog = Org.Neo4j.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;

	public class PersonalUserManagerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

		 private PersonalUserManager _userManager;
		 private EvilUserManager _evilUserManager;
		 private Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFailureToCreateUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFailureToCreateUser()
		 {
			  // Given
			  _evilUserManager.setFailNextCall();

			  //Expect
			  ExpectedException.expect( typeof( IOException ) );
			  ExpectedException.expectMessage( "newUserException" );

			  // When
			  _userManager.newUser( "hewhoshallnotbenamed", password( "avada kedavra" ), false );
			  verify( _log ).error( WithSubject( SecurityContext.AUTH_DISABLED.subject(), "tried to create user `%s`: %s" ), "hewhoshallnotbenamed", "newUserException" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _evilUserManager = new EvilUserManager( this, new InternalFlatFileRealm( new InMemoryUserRepository(), new InMemoryRoleRepository(), new BasicPasswordPolicy(), new RateLimitedAuthenticationStrategy(Clock.systemUTC(), Config.defaults()), new InternalFlatFileRealmIT.TestJobScheduler(), new InMemoryUserRepository(), new InMemoryUserRepository() ) );
			  _log = spy( typeof( Log ) );
			  _userManager = new PersonalUserManager( _evilUserManager, AuthSubject.AUTH_DISABLED, new SecurityLog( _log ), true );
		 }

		 private string WithSubject( AuthSubject subject, string msg )
		 {
			  return "[" + subject.Username() + "] " + msg;
		 }

		 private class EvilUserManager : EnterpriseUserManager
		 {
			 private readonly PersonalUserManagerTest _outerInstance;

			  internal bool FailNextCall;
			  internal EnterpriseUserManager Delegate;

			  internal EvilUserManager( PersonalUserManagerTest outerInstance, EnterpriseUserManager @delegate )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
			  }

			  internal virtual void SetFailNextCall()
			  {
					FailNextCall = true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User newUser(String username, byte[] password, boolean changeRequired) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override User NewUser( string username, sbyte[] password, bool changeRequired )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "newUserException" );
					}
					return Delegate.newUser( username, password, changeRequired );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override bool DeleteUser( string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "deleteUserException" );
					}
					return Delegate.deleteUser( username );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User getUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override User GetUser( string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new InvalidArgumentsException( "getUserException" );
					}
					return Delegate.getUser( username );
			  }

			  public override User SilentlyGetUser( string username )
			  {
					return Delegate.silentlyGetUser( username );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void SetUserPassword( string username, sbyte[] password, bool requirePasswordChange )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "setUserPasswordException" );
					}
					Delegate.setUserPassword( username, password, requirePasswordChange );
			  }

			  public virtual ISet<string> AllUsernames
			  {
				  get
				  {
						return Delegate.AllUsernames;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void suspendUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void SuspendUser( string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "suspendUserException" );
					}
					Delegate.suspendUser( username );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activateUser(String username, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void ActivateUser( string username, bool requirePasswordChange )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "activateUserException" );
					}
					Delegate.activateUser( username, requirePasswordChange );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void newRole(String roleName, String... usernames) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void NewRole( string roleName, params string[] usernames )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "newRoleException" );
					}
					Delegate.newRole( roleName, usernames );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteRole(String roleName) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override bool DeleteRole( string roleName )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "deleteRoleException" );
					}
					return Delegate.deleteRole( roleName );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertRoleExists(String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void AssertRoleExists( string roleName )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new InvalidArgumentsException( "getRoleException" );
					}
					Delegate.assertRoleExists( roleName );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addRoleToUser(String roleName, String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void AddRoleToUser( string roleName, string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "addRoleToUserException" );
					}
					Delegate.addRoleToUser( roleName, username );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeRoleFromUser(String roleName, String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override void RemoveRoleFromUser( string roleName, string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new IOException( "removeRoleFromUserException" );
					}
					Delegate.removeRoleFromUser( roleName, username );
			  }

			  public virtual ISet<string> AllRoleNames
			  {
				  get
				  {
						return Delegate.AllRoleNames;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getRoleNamesForUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override ISet<string> GetRoleNamesForUser( string username )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new InvalidArgumentsException( "getRoleNamesForUserException" );
					}
					return Delegate.getRoleNamesForUser( username );
			  }

			  public override ISet<string> SilentlyGetRoleNamesForUser( string username )
			  {
					return Delegate.silentlyGetRoleNamesForUser( username );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getUsernamesForRole(String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
			  public override ISet<string> GetUsernamesForRole( string roleName )
			  {
					if ( FailNextCall )
					{
						 FailNextCall = false;
						 throw new InvalidArgumentsException( "getUsernamesForRoleException" );
					}
					return Delegate.getUsernamesForRole( roleName );
			  }

			  public override ISet<string> SilentlyGetUsernamesForRole( string roleName )
			  {
					return Delegate.silentlyGetUsernamesForRole( roleName );
			  }
		 }
	}

}