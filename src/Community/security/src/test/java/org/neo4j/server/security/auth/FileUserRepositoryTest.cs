using System.Collections.Generic;

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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using DelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Neo4Net.Kernel.impl.security.User;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;
	using UTF8 = Neo4Net.Strings.UTF8;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class FileUserRepositoryTest
	{
		 private File _authFile;
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Thrown = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threading = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fs = FileSystemRule.get();
			  _authFile = new File( TestDirectory.directory( "dbms" ), "auth" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreAndRetrieveUsersByName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreAndRetrieveUsersByName()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  users.Create( user );

			  // When
			  User result = users.GetUserByName( user.Name() );

			  // Then
			  assertThat( result, equalTo( user ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPersistUsers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPersistUsers()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  users.Create( user );

			  users = new FileUserRepository( _fs, _authFile, _logProvider );
			  users.Start();

			  // When
			  User resultByName = users.GetUserByName( user.Name() );

			  // Then
			  assertThat( resultByName, equalTo( user ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindUserAfterDelete() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindUserAfterDelete()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  users.Create( user );

			  // When
			  users.Delete( user );

			  // Then
			  assertThat( users.GetUserByName( user.Name() ), nullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowComplexNames() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowComplexNames()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );

			  // When
			  users.AssertValidUsername( "neo4j" );
			  users.AssertValidUsername( "johnosbourne" );
			  users.AssertValidUsername( "john_osbourne" );

			  assertException( () => users.assertValidUsername(null), typeof(InvalidArgumentsException), "The provided username is empty." );
			  assertException( () => users.assertValidUsername(""), typeof(InvalidArgumentsException), "The provided username is empty." );
			  assertException( () => users.assertValidUsername(","), typeof(InvalidArgumentsException), "Username ',' contains illegal characters. Use ascii characters that are not ',', ':' or whitespaces" + "." );
			  assertException( () => users.assertValidUsername("with space"), typeof(InvalidArgumentsException), "Username 'with space' contains illegal characters. Use ascii characters that are not ',', ':' or " + "whitespaces." );
			  assertException( () => users.assertValidUsername("with:colon"), typeof(InvalidArgumentsException), "Username 'with:colon' contains illegal characters. Use ascii characters that are not ',', ':' or " + "whitespaces." );
			  assertException( () => users.assertValidUsername("withå"), typeof(InvalidArgumentsException), "Username 'withå' contains illegal characters. Use ascii characters that are not ',', ':' or " + "whitespaces." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverIfCrashedDuringMove() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverIfCrashedDuringMove()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.IOException exception = new java.io.IOException("simulated IO Exception on create");
			  IOException exception = new IOException( "simulated IO Exception on create" );
			  FileSystemAbstraction crashingFileSystem = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, _fs, exception );

			  FileUserRepository users = new FileUserRepository( crashingFileSystem, _authFile, _logProvider );
			  users.Start();
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();

			  // When
			  try
			  {
					users.Create( user );
					fail( "Expected an IOException" );
			  }
			  catch ( IOException e )
			  {
					assertSame( exception, e );
			  }

			  // Then
			  assertFalse( crashingFileSystem.FileExists( _authFile ) );
			  assertThat( crashingFileSystem.ListFiles( _authFile.ParentFile ).Length, equalTo( 0 ) );
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly FileUserRepositoryTest _outerInstance;

			 private IOException _exception;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( FileUserRepositoryTest outerInstance, FileSystemAbstraction fs, IOException exception ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._exception = exception;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File oldLocation, java.io.File newLocation, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
			 public override void renameFile( File oldLocation, File newLocation, params CopyOption[] copyOptions )
			 {
				  if ( _outerInstance.authFile.Name.Equals( newLocation.Name ) )
				  {
						throw _exception;
				  }
				  base.renameFile( oldLocation, newLocation, copyOptions );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfUpdateChangesName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfUpdateChangesName()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  users.Create( user );

			  // When
			  User updatedUser = ( new User.Builder( "john", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  try
			  {
					users.Update( user, updatedUser );
					fail( "expected exception not thrown" );
			  }
			  catch ( System.ArgumentException )
			  {
					// Then continue
			  }

			  assertThat( users.GetUserByName( user.Name() ), equalTo(user) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfExistingUserDoesNotMatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfExistingUserDoesNotMatch()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  User user = ( new User.Builder( "jake", LegacyCredential.Inaccessible ) ).withRequiredPasswordChange( true ).build();
			  users.Create( user );
			  User modifiedUser = user.Augment().withCredentials(LegacyCredential.ForPassword("foo")).build();

			  // When
			  User updatedUser = user.Augment().withCredentials(LegacyCredential.ForPassword("bar")).build();
			  try
			  {
					users.Update( modifiedUser, updatedUser );
					fail( "expected exception not thrown" );
			  }
			  catch ( ConcurrentModificationException )
			  {
					// Then continue
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnReadingInvalidEntries() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnReadingInvalidEntries()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  _fs.mkdir( _authFile.ParentFile );
			  // First line is correctly formatted, second line has an extra field
			  FileRepositorySerializer.WriteToFile( _fs, _authFile, UTF8.encode( "admin:SHA-256,A42E541F276CF17036DB7818F8B09B1C229AAD52A17F69F4029617F3A554640F,FB7E8AE08A6A7C741F678AD22217808F:\n" + "neo4j:fc4c600b43ffe4d5857b4439c35df88f:SHA-256," + "A42E541F276CF17036DB7818F8B09B1C229AAD52A17F69F4029617F3A554640F,FB7E8AE08A6A7C741F678AD22217808F:\n" ) );

			  // When
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, logProvider );

			  Thrown.expect( typeof( System.InvalidOperationException ) );
			  Thrown.expectMessage( startsWith( "Failed to read authentication file: " ) );

			  try
			  {
					users.Start();
			  }
			  // Then
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( users.NumberOfUsers(), equalTo(0) );
					logProvider.AssertExactly( AssertableLogProvider.inLog( typeof( FileUserRepository ) ).error( "Failed to read authentication file \"%s\" (%s)", _authFile.AbsolutePath, "wrong number of line fields, expected 3, got 4 [line 2]" ) );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideUserByUsernameEvenIfMidSetUsers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideUserByUsernameEvenIfMidSetUsers()
		 {
			  // Given
			  FileUserRepository users = new FileUserRepository( _fs, _authFile, _logProvider );
			  users.Create( ( new User.Builder( "oskar", LegacyCredential.ForPassword( "hidden" ) ) ).build() );
			  DoubleLatch latch = new DoubleLatch( 2 );

			  // When
			  Future<object> setUsers = Threading.execute(o =>
			  {
					 users.Users = new HangingListSnapshot( this, latch, 10L, java.util.Collections.emptyList() );
					 return null;
			  }, null);

			  latch.StartAndWaitForAllToStart();

			  // Then
			  assertNotNull( users.GetUserByName( "oskar" ) );

			  latch.Finish();
			  setUsers.get();
		 }

		 internal class HangingListSnapshot : ListSnapshot<User>
		 {
			 private readonly FileUserRepositoryTest _outerInstance;

			  internal readonly DoubleLatch Latch;

			  internal HangingListSnapshot( FileUserRepositoryTest outerInstance, DoubleLatch latch, long timestamp, IList<User> values ) : base( timestamp, values, true )
			  {
				  this._outerInstance = outerInstance;
					this.Latch = latch;
			  }

			  public override long Timestamp()
			  {
					Latch.start();
					Latch.finishAndWaitForAllToFinish();
					return base.Timestamp();
			  }
		 }
	}

}