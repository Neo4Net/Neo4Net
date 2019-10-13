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
namespace Neo4Net.Server.security.enterprise.auth
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using DelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Neo4Net.Server.Security.Auth;
	using Neo4Net.Server.Security.Auth;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;
	using UTF8 = Neo4Net.@string.UTF8;
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
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class FileRoleRepositoryTest
	{
		 private File _roleFile;
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;
		 private FileSystemAbstraction _fs;
		 private RoleRepository _roleRepository;

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
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fs = FileSystemRule.get();
			  _roleFile = new File( TestDirectory.directory( "dbms" ), "roles" );
			  _roleRepository = new FileRoleRepository( _fs, _roleFile, _logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreAndRetrieveRolesByName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreAndRetrieveRolesByName()
		 {
			  // Given
			  RoleRecord role = new RoleRecord( "admin", "petra", "olivia" );
			  _roleRepository.create( role );

			  // When
			  RoleRecord result = _roleRepository.getRoleByName( role.Name() );

			  // Then
			  assertThat( result, equalTo( role ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPersistRoles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPersistRoles()
		 {
			  // Given
			  RoleRecord role = new RoleRecord( "admin", "craig", "karl" );
			  _roleRepository.create( role );

			  _roleRepository = new FileRoleRepository( _fs, _roleFile, _logProvider );
			  _roleRepository.start();

			  // When
			  RoleRecord resultByName = _roleRepository.getRoleByName( role.Name() );

			  // Then
			  assertThat( resultByName, equalTo( role ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindRoleAfterDelete() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindRoleAfterDelete()
		 {
			  // Given
			  RoleRecord role = new RoleRecord( "jake", "admin" );
			  _roleRepository.create( role );

			  // When
			  _roleRepository.delete( role );

			  // Then
			  assertThat( _roleRepository.getRoleByName( role.Name() ), nullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowComplexNames() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowComplexNames()
		 {
			  // Given

			  // When
			  _roleRepository.assertValidRoleName( "neo4j" );
			  _roleRepository.assertValidRoleName( "johnosbourne" );
			  _roleRepository.assertValidRoleName( "john_osbourne" );

			  assertException( () => _roleRepository.assertValidRoleName(null), typeof(InvalidArgumentsException), "The provided role name is empty." );
			  assertException( () => _roleRepository.assertValidRoleName(""), typeof(InvalidArgumentsException), "The provided role name is empty." );
			  assertException( () => _roleRepository.assertValidRoleName(":"), typeof(InvalidArgumentsException), "Role name ':' contains illegal characters. Use simple ascii characters and numbers." );
			  assertException( () => _roleRepository.assertValidRoleName("john osbourne"), typeof(InvalidArgumentsException), "Role name 'john osbourne' contains illegal characters. Use simple ascii characters and numbers." );
			  assertException( () => _roleRepository.assertValidRoleName("john:osbourne"), typeof(InvalidArgumentsException), "Role name 'john:osbourne' contains illegal characters. Use simple ascii characters and numbers." );
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

			  _roleRepository = new FileRoleRepository( crashingFileSystem, _roleFile, _logProvider );
			  _roleRepository.start();
			  RoleRecord role = new RoleRecord( "admin", "jake" );

			  // When
			  try
			  {
					_roleRepository.create( role );
					fail( "Expected an IOException" );
			  }
			  catch ( IOException e )
			  {
					assertSame( exception, e );
			  }

			  // Then
			  assertFalse( crashingFileSystem.FileExists( _roleFile ) );
			  assertThat( crashingFileSystem.ListFiles( _roleFile.ParentFile ).Length, equalTo( 0 ) );
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly FileRoleRepositoryTest _outerInstance;

			 private IOException _exception;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( FileRoleRepositoryTest outerInstance, FileSystemAbstraction fs, IOException exception ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._exception = exception;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File oldLocation, java.io.File newLocation, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
			 public override void renameFile( File oldLocation, File newLocation, params CopyOption[] copyOptions )
			 {
				  if ( _outerInstance.roleFile.Name.Equals( newLocation.Name ) )
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
			  RoleRecord role = new RoleRecord( "admin", "steve", "bob" );
			  _roleRepository.create( role );

			  // When
			  RoleRecord updatedRole = new RoleRecord( "admins", "steve", "bob" );
			  try
			  {
					_roleRepository.update( role, updatedRole );
					fail( "expected exception not thrown" );
			  }
			  catch ( System.ArgumentException )
			  {
					// Then continue
			  }

			  assertThat( _roleRepository.getRoleByName( role.Name() ), equalTo(role) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfExistingRoleDoesNotMatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfExistingRoleDoesNotMatch()
		 {
			  // Given
			  RoleRecord role = new RoleRecord( "admin", "jake" );
			  _roleRepository.create( role );
			  RoleRecord modifiedRole = new RoleRecord( "admin", "jake", "john" );

			  // When
			  RoleRecord updatedRole = new RoleRecord( "admin", "john" );
			  try
			  {
					_roleRepository.update( modifiedRole, updatedRole );
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

			  _fs.mkdirs( _roleFile.ParentFile );
			  // First line is correctly formatted, second line has an extra field
			  FileRepositorySerializer.writeToFile( _fs, _roleFile, UTF8.encode( "neo4j:admin\n" + "admin:admin:\n" ) );

			  // When
			  _roleRepository = new FileRoleRepository( _fs, _roleFile, logProvider );

			  Thrown.expect( typeof( System.InvalidOperationException ) );
			  Thrown.expectMessage( startsWith( "Failed to read role file '" ) );

			  try
			  {
					_roleRepository.start();
			  }
			  // Then
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( _roleRepository.numberOfRoles(), equalTo(0) );
					logProvider.AssertExactly( AssertableLogProvider.inLog( typeof( FileRoleRepository ) ).error( "Failed to read role file \"%s\" (%s)", _roleFile.AbsolutePath, "wrong number of line fields [line 2]" ) );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddEmptyUserToRole() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddEmptyUserToRole()
		 {
			  // Given
			  _fs.mkdirs( _roleFile.ParentFile );
			  FileRepositorySerializer.writeToFile( _fs, _roleFile, UTF8.encode( "admin:neo4j\nreader:\n" ) );

			  // When
			  _roleRepository = new FileRoleRepository( _fs, _roleFile, _logProvider );
			  _roleRepository.start();

			  RoleRecord role = _roleRepository.getRoleByName( "admin" );
			  assertTrue( "neo4j should be assigned to 'admin'", role.Users().Contains("neo4j") );
			  assertEquals( "only one admin should exist", 1, role.Users().Count );

			  role = _roleRepository.getRoleByName( "reader" );
			  assertTrue( "no users should be assigned to 'reader'", role.Users().Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideRolesByUsernameEvenIfMidSetRoles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideRolesByUsernameEvenIfMidSetRoles()
		 {
			  // Given
			  _roleRepository = new FileRoleRepository( _fs, _roleFile, _logProvider );
			  _roleRepository.create( new RoleRecord( "admin", "oskar" ) );
			  DoubleLatch latch = new DoubleLatch( 2 );

			  // When
			  Future<object> setUsers = Threading.execute(o =>
			  {
				_roleRepository.Roles = new HangingListSnapshot( this, latch, 10L, java.util.Collections.emptyList() );
				return null;
			  }, null);

			  latch.StartAndWaitForAllToStart();

			  // Then
			  assertThat( _roleRepository.getRoleNamesByUsername( "oskar" ), containsInAnyOrder( "admin" ) );

			  latch.Finish();
			  setUsers.get();
		 }

		 internal class HangingListSnapshot : ListSnapshot<RoleRecord>
		 {
			 private readonly FileRoleRepositoryTest _outerInstance;

			  internal readonly DoubleLatch Latch;

			  internal HangingListSnapshot( FileRoleRepositoryTest outerInstance, DoubleLatch latch, long timestamp, IList<RoleRecord> values ) : base( timestamp, values, true )
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