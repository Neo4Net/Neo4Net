/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server.Security.Auth
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class InitialUserTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 protected internal Config Config;
		 protected internal UserRepository Users;

		 protected internal abstract AuthManager AuthManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateDefaultUserIfNoneExist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateDefaultUserIfNoneExist()
		 {
			  // When
			  AuthManager().start();

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = users.getUserByName("neo4j");
			  User user = Users.getUserByName( "neo4j" );
			  assertNotNull( user );
			  assertTrue( user.Credentials().matchesPassword("neo4j") );
			  assertTrue( user.PasswordChangeRequired() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadInitialUserIfNoneExist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadInitialUserIfNoneExist()
		 {
			  // Given
			  FileUserRepository initialUserRepository = CommunitySecurityModule.GetInitialUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  initialUserRepository.Start();
			  initialUserRepository.create(new User.Builder("neo4j", LegacyCredential.ForPassword("123"))
									.withRequiredPasswordChange( false ).build());
			  initialUserRepository.Shutdown();

			  // When
			  AuthManager().start();

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = users.getUserByName("neo4j");
			  User user = Users.getUserByName( "neo4j" );
			  assertNotNull( user );
			  assertTrue( user.Credentials().matchesPassword("123") );
			  assertFalse( user.PasswordChangeRequired() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadInitialUserIfNoneExistEvenWithSamePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadInitialUserIfNoneExistEvenWithSamePassword()
		 {
			  // Given
			  FileUserRepository initialUserRepository = CommunitySecurityModule.GetInitialUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  initialUserRepository.Start();
			  initialUserRepository.create(new User.Builder("neo4j", LegacyCredential.ForPassword("neo4j"))
									.withRequiredPasswordChange( false ).build());
			  initialUserRepository.Shutdown();

			  // When
			  AuthManager().start();

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = users.getUserByName("neo4j");
			  User user = Users.getUserByName( "neo4j" );
			  assertNotNull( user );
			  assertTrue( user.Credentials().matchesPassword("neo4j") );
			  assertFalse( user.PasswordChangeRequired() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddInitialUserIfUsersExist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddInitialUserIfUsersExist()
		 {
			  // Given
			  FileUserRepository initialUserRepository = CommunitySecurityModule.GetInitialUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  initialUserRepository.Start();
			  initialUserRepository.Create( NewUser( "initUser", "123", false ) );
			  initialUserRepository.Shutdown();
			  Users.start();
			  Users.create( NewUser( "oldUser", "321", false ) );
			  Users.shutdown();

			  // When
			  AuthManager().start();

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User initUser = users.getUserByName("initUser");
			  User initUser = Users.getUserByName( "initUser" );
			  assertNull( initUser );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User oldUser = users.getUserByName("oldUser");
			  User oldUser = Users.getUserByName( "oldUser" );
			  assertNotNull( oldUser );
			  assertTrue( oldUser.Credentials().matchesPassword("321") );
			  assertFalse( oldUser.PasswordChangeRequired() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateUserIfInitialUserExist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotUpdateUserIfInitialUserExist()
		 {
			  // Given
			  FileUserRepository initialUserRepository = CommunitySecurityModule.GetInitialUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  initialUserRepository.Start();
			  initialUserRepository.Create( NewUser( "oldUser", "newPassword", false ) );
			  initialUserRepository.Shutdown();
			  Users.start();
			  Users.create( NewUser( "oldUser", "oldPassword", true ) );
			  Users.shutdown();

			  // When
			  AuthManager().start();

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User oldUser = users.getUserByName("oldUser");
			  User oldUser = Users.getUserByName( "oldUser" );
			  assertNotNull( oldUser );
			  assertTrue( oldUser.Credentials().matchesPassword("oldPassword") );
			  assertTrue( oldUser.PasswordChangeRequired() );
		 }

		 protected internal virtual User NewUser( string userName, string password, bool pwdChange )
		 {
			  return ( new User.Builder( userName, LegacyCredential.ForPassword( password ) ) ).withRequiredPasswordChange( pwdChange ).build();
		 }
	}

}