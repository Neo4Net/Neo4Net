using System;
using System.Collections.Generic;
using System.Threading;

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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using DelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PasswordPolicy = Neo4Net.Kernel.api.security.PasswordPolicy;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CentralJobScheduler = Neo4Net.Kernel.impl.scheduler.CentralJobScheduler;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using BasicPasswordPolicy = Neo4Net.Server.Security.Auth.BasicPasswordPolicy;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;
	using RateLimitedAuthenticationStrategy = Neo4Net.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class InternalFlatFileRealmIT
	{
		 internal File UserStoreFile;
		 internal File RoleStoreFile;

		 internal TestJobScheduler JobScheduler = new TestJobScheduler();
		 internal LogProvider LogProvider = NullLogProvider.Instance;
		 internal InternalFlatFileRealm Realm;
		 internal EvilFileSystem Fs;

		 private static int _largeNumber = 123;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  Fs = new EvilFileSystem( this, new EphemeralFileSystemAbstraction() );
			  UserStoreFile = new File( "dbms", "auth" );
			  RoleStoreFile = new File( "dbms", "roles" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository userRepository = new org.neo4j.server.security.auth.FileUserRepository(fs, userStoreFile, logProvider);
			  UserRepository userRepository = new FileUserRepository( Fs, UserStoreFile, LogProvider );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RoleRepository roleRepository = new FileRoleRepository(fs, roleStoreFile, logProvider);
			  RoleRepository roleRepository = new FileRoleRepository( Fs, RoleStoreFile, LogProvider );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository initialUserRepository = org.neo4j.server.security.auth.CommunitySecurityModule.getInitialUserRepository(org.neo4j.kernel.configuration.Config.defaults(), logProvider, fs);
			  UserRepository initialUserRepository = CommunitySecurityModule.getInitialUserRepository( Config.defaults(), LogProvider, Fs );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository defaultAdminRepository = EnterpriseSecurityModule.getDefaultAdminRepository(org.neo4j.kernel.configuration.Config.defaults(), logProvider, fs);
			  UserRepository defaultAdminRepository = EnterpriseSecurityModule.GetDefaultAdminRepository( Config.defaults(), LogProvider, Fs );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.security.PasswordPolicy passwordPolicy = new org.neo4j.server.security.auth.BasicPasswordPolicy();
			  PasswordPolicy passwordPolicy = new BasicPasswordPolicy();
			  AuthenticationStrategy authenticationStrategy = new RateLimitedAuthenticationStrategy( Clocks.systemClock(), Config.defaults() );

			  Realm = new InternalFlatFileRealm( userRepository, roleRepository, passwordPolicy, authenticationStrategy, true, true, JobScheduler, initialUserRepository, defaultAdminRepository );
			  Realm.init();
			  Realm.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  Realm.shutdown();
			  Fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReloadAuthFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReloadAuthFiles()
		 {
			  Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34A4:password_change_required\n", "admin:Mia\n" + "publisher:Hanna,Carol\n" );

			  JobScheduler.scheduledRunnable.run();

			  assertThat( Realm.AllUsernames, containsInAnyOrder( "Hanna", "Carol", "Mia" ) );
			  assertThat( Realm.getUsernamesForRole( "admin" ), containsInAnyOrder( "Mia" ) );
			  assertThat( Realm.getUsernamesForRole( "publisher" ), containsInAnyOrder( "Hanna", "Carol" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReloadAuthFilesUntilValid() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReloadAuthFilesUntilValid()
		 {
			  // we start with invalid auth file
			  Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34", "THIS_WILL_NOT_BE_READ" );

			  // now the roles file has non-existent users
			  Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34A4:password_change_required\n", "admin:neo4j,Mao\n" + "publisher:Hanna\n" );

			  // finally valid files
			  Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34A4:password_change_required\n", "admin:Mia\n" + "publisher:Hanna,Carol\n" );

			  JobScheduler.scheduledRunnable.run();

			  assertThat( Realm.AllUsernames, containsInAnyOrder( "Hanna", "Carol", "Mia" ) );
			  assertThat( Realm.getUsernamesForRole( "admin" ), containsInAnyOrder( "Mia" ) );
			  assertThat( Realm.getUsernamesForRole( "publisher" ), containsInAnyOrder( "Hanna", "Carol" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEventuallyFailReloadAttempts()
		 public virtual void ShouldEventuallyFailReloadAttempts()
		 {
			  // the roles file has non-existent users
			  Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34A4:password_change_required\n", "admin:neo4j,Mao\n" + "publisher:Hanna\n" );

			  // perma-broken auth file
			  for ( int i = 0; i < _largeNumber - 1; i++ )
			  {
					Fs.addUserRoleFilePair( "Hanna:SHA-256,FE0056C37E,A543:\n" + "Carol:SHA-256,FE0056C37E,A543:\n" + "Mia:SHA-256,0E1FFFC23E,34", "admin:Mia\n" + "publisher:Hanna,Carol\n" );
			  }

			  try
			  {
					JobScheduler.scheduledRunnable.run();
					fail( "Expected exception due to invalid auth file combo." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, containsString( "Unable to load valid flat file repositories! Attempts failed with:" ) );
					File authFile = new File( "dbms/auth" );
					assertThat( e.Message, containsString( "Failed to read authentication file: " + authFile ) );
					assertThat( e.Message, containsString( "Role-auth file combination not valid" ) );
			  }
		 }

		 internal class TestJobScheduler : CentralJobScheduler
		 {
			  internal ThreadStart ScheduledRunnable;

			  public override JobHandle Schedule( Group group, ThreadStart r, long initialDelay, TimeUnit timeUnit )
			  {
					this.ScheduledRunnable = r;
					return null;
			  }
		 }

		 private class EvilFileSystem : DelegatingFileSystemAbstraction
		 {
			 private readonly InternalFlatFileRealmIT _outerInstance;

			  internal LinkedList<string> UserStoreVersions = new LinkedList<string>();
			  internal LinkedList<string> RoleStoreVersions = new LinkedList<string>();

			  internal EvilFileSystem( InternalFlatFileRealmIT outerInstance, FileSystemAbstraction @delegate ) : base( @delegate )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal virtual void AddUserRoleFilePair( string usersVersion, string rolesVersion )
			  {
					UserStoreVersions.AddLast( usersVersion );
					RoleStoreVersions.AddLast( rolesVersion );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
			  public override Reader OpenAsReader( File fileName, Charset charset )
			  {
					if ( fileName.Equals( outerInstance.UserStoreFile ) )
					{
						 return new CharArrayReader( UserStoreVersions.RemoveFirst().ToCharArray() );
					}
					if ( fileName.Equals( outerInstance.RoleStoreFile ) )
					{
						 if ( UserStoreVersions.Count < RoleStoreVersions.Count - 1 )
						 {
							  RoleStoreVersions.RemoveFirst();
						 }
						 return new CharArrayReader( RoleStoreVersions.RemoveFirst().ToCharArray() );
					}
					return base.OpenAsReader( fileName, charset );
			  }

			  public override long LastModifiedTime( File fileName )
			  {
					if ( fileName.Equals( outerInstance.UserStoreFile ) )
					{
						 return _largeNumber + 1 - UserStoreVersions.Count;
					}
					if ( fileName.Equals( outerInstance.RoleStoreFile ) )
					{
						 return _largeNumber + 1 - RoleStoreVersions.Count;
					}
					return base.LastModifiedTime( fileName );
			  }
		 }
	}

}