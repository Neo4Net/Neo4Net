using System;
using System.Collections.Generic;
using System.Diagnostics;

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


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using UTF8 = Neo4Net.Strings.UTF8;
	using TestGraphDatabaseBuilder = Neo4Net.Test.TestGraphDatabaseBuilder;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken.newBasicAuthToken;

	public class AuthProceduresIT
	{
		 private static readonly string _pwdChange = PASSWORD_CHANGE_REQUIRED.name().ToLower();

		 protected internal GraphDatabaseAPI Db;
		 private EphemeralFileSystemAbstraction _fs;
		 private BasicAuthManager _authManager;
		 private LoginContext _admin;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _fs = new EphemeralFileSystemAbstraction();
			  Db = ( GraphDatabaseAPI ) CreateGraphDatabase( _fs );
			  _authManager = Db.DependencyResolver.resolveDependency( typeof( BasicAuthManager ) );
			  _admin = Login( "neo4j", "neo4j" );
			  _admin.subject().setPasswordChangeNoLongerRequired();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  Db.shutdown();
			  _fs.Dispose();
		 }

		 //---------- change password -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChangePassword()
		 {

			  // Given
			  AssertEmpty( _admin, "CALL dbms.changePassword('abc')" );

			  Debug.Assert( _authManager.getUser( "neo4j" ).credentials().matchesPassword("abc") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeOwnPasswordIfNewPasswordInvalid()
		 public virtual void ShouldNotChangeOwnPasswordIfNewPasswordInvalid()
		 {
			  AssertFail( _admin, "CALL dbms.changePassword( '' )", "A password cannot be empty." );
			  AssertFail( _admin, "CALL dbms.changePassword( 'neo4j' )", "Old password and new password cannot be the same." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newUserShouldBeAbleToChangePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewUserShouldBeAbleToChangePassword()
		 {
			  // Given
			  _authManager.newUser( "andres", UTF8.encode( "banana" ), true );

			  // Then
			  AssertEmpty( Login( "andres", "banana" ), "CALL dbms.changePassword('abc')" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newUserShouldNotBeAbleToCallOtherProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewUserShouldNotBeAbleToCallOtherProcedures()
		 {
			  // Given
			  _authManager.newUser( "andres", UTF8.encode( "banana" ), true );
			  LoginContext user = Login( "andres", "banana" );

			  // Then
			  AssertFail( user, "CALL dbms.procedures", "The credentials you provided were valid, but must be changed before you can use this instance." );
		 }

		 //---------- create user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUser()
		 public virtual void ShouldCreateUser()
		 {
			  AssertEmpty( _admin, "CALL dbms.security.createUser('andres', '123', true)" );
			  try
			  {
					assertThat( _authManager.getUser( "andres" ).passwordChangeRequired(), equalTo(true) );
			  }
			  catch ( Exception )
			  {
					fail( "Expected no exception!" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUserWithNoPasswordChange()
		 public virtual void ShouldCreateUserWithNoPasswordChange()
		 {
			  AssertEmpty( _admin, "CALL dbms.security.createUser('andres', '123', false)" );
			  try
			  {
					assertThat( _authManager.getUser( "andres" ).passwordChangeRequired(), equalTo(false) );
			  }
			  catch ( Exception )
			  {
					fail( "Expected no exception!" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUserWithDefault()
		 public virtual void ShouldCreateUserWithDefault()
		 {
			  AssertEmpty( _admin, "CALL dbms.security.createUser('andres', '123')" );
			  try
			  {
					assertThat( _authManager.getUser( "andres" ).passwordChangeRequired(), equalTo(true) );
			  }
			  catch ( Exception )
			  {
					fail( "Expected no exception!" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateUserIfInvalidUsername()
		 public virtual void ShouldNotCreateUserIfInvalidUsername()
		 {
			  AssertFail( _admin, "CALL dbms.security.createUser('', '1234', true)", "The provided username is empty." );
			  AssertFail( _admin, "CALL dbms.security.createUser(',!', '1234', true)", "Username ',!' contains illegal characters." );
			  AssertFail( _admin, "CALL dbms.security.createUser(':ss!', '', true)", "Username ':ss!' contains illegal " + "characters." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateUserIfInvalidPassword()
		 public virtual void ShouldNotCreateUserIfInvalidPassword()
		 {
			  AssertFail( _admin, "CALL dbms.security.createUser('andres', '', true)", "A password cannot be empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateExistingUser()
		 public virtual void ShouldNotCreateExistingUser()
		 {
			  AssertFail( _admin, "CALL dbms.security.createUser('neo4j', '1234', true)", "The specified user 'neo4j' already exists" );
			  AssertFail( _admin, "CALL dbms.security.createUser('neo4j', '', true)", "A password cannot be empty." );
		 }

		 //---------- delete user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteUser()
		 {
			  _authManager.newUser( "andres", UTF8.encode( "123" ), false );
			  AssertEmpty( _admin, "CALL dbms.security.deleteUser('andres')" );
			  try
			  {
					_authManager.getUser( "andres" );
					fail( "Andres should no longer exist, expected exception." );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					assertThat( e.Message, containsString( "User 'andres' does not exist." ) );
			  }
			  catch ( Exception t )
			  {
					assertThat( t.GetType(), equalTo(typeof(InvalidArgumentsException)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeleteNonExistentUser()
		 public virtual void ShouldNotDeleteNonExistentUser()
		 {
			  AssertFail( _admin, "CALL dbms.security.deleteUser('nonExistentUser')", "User 'nonExistentUser' does not exist" );
		 }

		 //---------- list users -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUsers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUsers()
		 {
			  _authManager.newUser( "andres", UTF8.encode( "123" ), false );
			  AssertSuccess( _admin, "CALL dbms.security.listUsers() YIELD username", r => assertKeyIs( r, "username", "neo4j", "andres" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUsersWithFlags() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnUsersWithFlags()
		 {
			  _authManager.newUser( "andres", UTF8.encode( "123" ), false );
			  IDictionary<string, object> expected = map( "neo4j", ListOf( _pwdChange ), "andres", ListOf() );
			  AssertSuccess( _admin, "CALL dbms.security.listUsers()", r => assertKeyIsMap( r, "username", "flags", expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowCurrentUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowCurrentUser()
		 {
			  AssertSuccess( _admin, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "flags", map( "neo4j", ListOf( _pwdChange ) ) ) );

			  _authManager.newUser( "andres", UTF8.encode( "123" ), false );
			  LoginContext andres = Login( "andres", "123" );
			  AssertSuccess( andres, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "flags", map( "andres", ListOf() ) ) );
		 }

		 //---------- utility -----------

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.GraphDatabaseService createGraphDatabase(org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs) throws java.io.IOException
		 private GraphDatabaseService CreateGraphDatabase( EphemeralFileSystemAbstraction fs )
		 {
			  RemovePreviousAuthFile();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>, String> settings = new java.util.HashMap<>();
			  IDictionary<Setting<object>, string> settings = new Dictionary<Setting<object>, string>();
			  settings[GraphDatabaseSettings.auth_enabled] = "true";

			  TestGraphDatabaseBuilder graphDatabaseFactory = ( TestGraphDatabaseBuilder ) ( new TestGraphDatabaseFactory() ).setFileSystem(fs).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.auth_enabled, "true");

			  return graphDatabaseFactory.NewGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void removePreviousAuthFile() throws java.io.IOException
		 private void RemovePreviousAuthFile()
		 {
			  Path file = Paths.get( "target/test-data/impermanent-db/data/dbms/auth" );
			  if ( Files.exists( file ) )
			  {
					Files.delete( file );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.security.LoginContext login(String username, String password) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private LoginContext Login( string username, string password )
		 {
			  return _authManager.login( newBasicAuthToken( username, password ) );
		 }

		 private void AssertEmpty( LoginContext subject, string query )
		 {
			  assertThat(Execute(subject, query, r =>
			  {
						  Debug.Assert( !r.hasNext() );
			  }), equalTo( "" ));
		 }

		 private void AssertFail( LoginContext subject, string query, string partOfErrorMsg )
		 {
			  assertThat(Execute(subject, query, r =>
			  {
						  Debug.Assert( !r.hasNext() );
			  }), containsString( partOfErrorMsg ));
		 }

		 private void AssertSuccess( LoginContext subject, string query, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  assertThat( Execute( subject, query, resultConsumer ), equalTo( "" ) );
		 }

		 private string Execute( LoginContext subject, string query, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  try
			  {
					  using ( Transaction tx = Db.beginTransaction( KernelTransaction.Type.@implicit, subject ) )
					  {
						resultConsumer( Db.execute( query ) );
						tx.Success();
						return "";
					  }
			  }
			  catch ( Exception e )
			  {
					return e.Message;
			  }
		 }

		 private IList<object> GetObjectsAsList( ResourceIterator<IDictionary<string, object>> r, string key )
		 {
			  return r.Select( s => s.get( key ) ).ToList();
		 }

		 private void AssertKeyIs( ResourceIterator<IDictionary<string, object>> r, string key, params string[] items )
		 {
			  AssertKeyIsArray( r, key, items );
		 }

		 private void AssertKeyIsArray( ResourceIterator<IDictionary<string, object>> r, string key, string[] items )
		 {
			  IList<object> results = GetObjectsAsList( r, key );
			  assertEquals( Arrays.asList( items ).size(), results.Count );
			  assertThat( results, containsInAnyOrder( items ) );
		 }

		 protected internal virtual string[] With( string[] strs, params string[] moreStr )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.concat( Arrays.stream( strs ), Arrays.stream( moreStr ) ).toArray( string[]::new );
		 }

		 private IList<string> ListOf( params string[] values )
		 {
			  return Stream.of( values ).collect( Collectors.toList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static void assertKeyIsMap(org.neo4j.graphdb.ResourceIterator<java.util.Map<String,Object>> r, String keyKey, String valueKey, java.util.Map<String,Object> expected)
		 public static void AssertKeyIsMap( ResourceIterator<IDictionary<string, object>> r, string keyKey, string valueKey, IDictionary<string, object> expected )
		 {
			  IList<IDictionary<string, object>> result = r.ToList();

			  assertEquals( "Results for should have size " + expected.Count + " but was " + result.Count, expected.Count, result.Count );

			  foreach ( IDictionary<string, object> row in result )
			  {
					string key = ( string ) row[keyKey];
					assertTrue( "Unexpected key '" + key + "'", expected.ContainsKey( key ) );

					assertTrue( "Value key '" + valueKey + "' not found in results", row.ContainsKey( valueKey ) );
					object objectValue = row[valueKey];
					if ( objectValue is System.Collections.IList )
					{
						 IList<string> value = ( IList<string> ) objectValue;
						 IList<string> expectedValues = ( IList<string> ) expected[key];
						 assertEquals( "Results for '" + key + "' should have size " + expectedValues.Count + " but was " + value.Count, value.Count, expectedValues.Count );
						 assertThat( value, containsInAnyOrder( expectedValues.ToArray() ) );
					}
					else
					{
						 string value = objectValue.ToString();
						 string expectedValue = expected[key].ToString();
						 assertEquals( string.Format( "Wrong value for '{0}', expected '{1}', got '{2}'", key, expectedValue, value ), value, expectedValue );
					}
			  }
		 }
	}

}