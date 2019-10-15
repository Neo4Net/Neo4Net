using System;
using System.Collections.Generic;
using System.Text;
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
	using StringUtils = org.apache.commons.lang.StringUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;


	using Neo4jPackV1 = Neo4Net.Bolt.v1.messaging.Neo4jPackV1;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Graphdb;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionGuardException = Neo4Net.Graphdb.TransactionGuardException;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;
	using EnterpriseBuiltInDbmsProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Neo4Net.Kernel.impl.util;
	using Log = Neo4Net.Logging.Log;
	using Context = Neo4Net.Procedure.Context;
	using Mode = Neo4Net.Procedure.Mode;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using TerminationGuard = Neo4Net.Procedure.TerminationGuard;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionTimedOut;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.EDITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;

	public abstract class ProcedureInteractionTestBase<S>
	{
		private bool InstanceFieldsInitialized = false;

		public ProcedureInteractionTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			InitialRoles = new string[] { ADMIN, ARCHITECT, PUBLISHER, EDITOR, READER, _emptyRole };
		}

		 internal const string PROCEDURE_TIMEOUT_ERROR = "Procedure got: Transaction guard check failed";
		 protected internal bool PwdChangeCheckFirst;
		 protected internal string ChangePwdErrMsg = AuthorizationViolationException.PERMISSION_DENIED;
		 private const string BOLT_PWD_ERR_MSG = "The credentials you provided were valid, but must be changed before you can use this instance.";
		 internal string ReadOpsNotAllowed = "Read operations are not allowed";
		 internal string WriteOpsNotAllowed = "Write operations are not allowed";
		 internal string TokenCreateOpsNotAllowed = "Token create operations are not allowed";
		 internal string SchemaOpsNotAllowed = "Schema operations are not allowed";

		 protected internal bool IsEmbedded = true;

		 internal virtual string PwdReqErrMsg( string errMsg )
		 {
			  return PwdChangeCheckFirst ? ChangePwdErrMsg : IsEmbedded ? errMsg : BOLT_PWD_ERR_MSG;
		 }

		 private readonly string _emptyRole = "empty";

		 protected internal S AdminSubject;
		 internal S SchemaSubject;
		 internal S WriteSubject;
		 internal S EditorSubject;
		 internal S ReadSubject;
		 internal S PwdSubject;
		 internal S NoneSubject;

		 internal string[] InitialUsers = new string[] { "adminSubject", "readSubject", "schemaSubject", "writeSubject", "editorSubject", "pwdSubject", "noneSubject", "neo4j" };
		 internal string[] InitialRoles;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threading = new org.neo4j.test.rule.concurrent.ThreadingRule();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly ThreadingRule ThreadingConflict = new ThreadingRule();

		 private ThreadingRule Threading()
		 {
			  return ThreadingConflict;
		 }

		 protected internal EnterpriseUserManager UserManager;

		 protected internal NeoInteractionLevel<S> Neo;
		 protected internal TransportTestUtil Util;
		 internal File SecurityLog;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String,String> defaultConfiguration() throws java.io.IOException
		 internal virtual IDictionary<string, string> DefaultConfiguration()
		 {
			  Path homeDir = Files.createTempDirectory( "logs" );
			  SecurityLog = new File( homeDir.toFile(), "security.log" );
			  return stringMap( GraphDatabaseSettings.logs_directory.name(), homeDir.toAbsolutePath().ToString(), SecuritySettings.procedure_roles.name(), "test.allowed*Procedure:role1;test.nestedAllowedFunction:role1;" + "test.allowedFunc*:role1;test.*estedAllowedProcedure:role1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  ConfiguredSetup( DefaultConfiguration() );
			  Util = new TransportTestUtil( new Neo4jPackV1() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void configuredSetup(java.util.Map<String,String> config) throws Throwable
		 internal virtual void ConfiguredSetup( IDictionary<string, string> config )
		 {
			  Neo = setUpNeoServer( config );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );
			  procedures.RegisterProcedure( typeof( ClassWithProcedures ) );
			  procedures.RegisterFunction( typeof( ClassWithFunctions ) );
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "noneSubject", password( "abc" ), false );
			  UserManager.newUser( "pwdSubject", password( "abc" ), true );
			  UserManager.newUser( "adminSubject", password( "abc" ), false );
			  UserManager.newUser( "schemaSubject", password( "abc" ), false );
			  UserManager.newUser( "writeSubject", password( "abc" ), false );
			  UserManager.newUser( "editorSubject", password( "abc" ), false );
			  UserManager.newUser( "readSubject", password( "123" ), false );
			  // Currently admin role is created by default
			  UserManager.addRoleToUser( ADMIN, "adminSubject" );
			  UserManager.addRoleToUser( ARCHITECT, "schemaSubject" );
			  UserManager.addRoleToUser( PUBLISHER, "writeSubject" );
			  UserManager.addRoleToUser( EDITOR, "editorSubject" );
			  UserManager.addRoleToUser( READER, "readSubject" );
			  UserManager.newRole( _emptyRole );
			  NoneSubject = Neo.login( "noneSubject", "abc" );
			  PwdSubject = Neo.login( "pwdSubject", "abc" );
			  ReadSubject = Neo.login( "readSubject", "123" );
			  EditorSubject = Neo.login( "editorSubject", "abc" );
			  WriteSubject = Neo.login( "writeSubject", "abc" );
			  SchemaSubject = Neo.login( "schemaSubject", "abc" );
			  AdminSubject = Neo.login( "adminSubject", "abc" );
			  using ( Transaction tx = Neo.LocalGraph.beginTx( 1, TimeUnit.HOURS ) )
			  {
					AssertEmpty( SchemaSubject, "CREATE (n) SET n:A:Test:NEWNODE:VeryUniqueLabel:Node " + "SET n.id = '2', n.square = '4', n.name = 'me', n.prop = 'a', n.number = '1' " + "DELETE n" );
					AssertEmpty( WriteSubject, "UNWIND range(0,2) AS number CREATE (:Node {number:number, name:'node'+number})" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract NeoInteractionLevel<S> setUpNeoServer(java.util.Map<String,String> config) throws Throwable;
		 protected internal abstract NeoInteractionLevel<S> SetUpNeoServer( IDictionary<string, string> config );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( Neo != null )
			  {
					Neo.tearDown();
			  }
		 }

		 protected internal virtual string[] With( string[] strs, params string[] moreStr )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.concat( Arrays.stream( strs ), Arrays.stream( moreStr ) ).toArray( string[]::new );
		 }

		 internal virtual IList<string> ListOf( params string[] values )
		 {
			  return Stream.of( values ).collect( toList() );
		 }

		 //------------- Helper functions---------------

		 internal virtual void TestSuccessfulRead( S subject, object count )
		 {
			  AssertSuccess(subject, "MATCH (n) RETURN count(n) as count", r =>
			  {
				IList<object> result = r.Select( s => s.get( "count" ) ).ToList();
				assertThat( result.size(), equalTo(1) );
				assertThat( result.get( 0 ), equalTo( ValueOf( count ) ) );
			  });
		 }

		 internal virtual void TestFailRead( S subject, int count )
		 {
			  TestFailRead( subject, count, ReadOpsNotAllowed );
		 }

		 internal virtual void TestFailRead( S subject, int count, string errMsg )
		 {
			  AssertFail( subject, "MATCH (n) RETURN count(n)", errMsg );
		 }

		 internal virtual void TestSuccessfulWrite( S subject )
		 {
			  AssertEmpty( subject, "CREATE (:Node)" );
		 }

		 internal virtual void TestFailWrite( S subject )
		 {
			  TestFailWrite( subject, WriteOpsNotAllowed );
		 }

		 internal virtual void TestFailWrite( S subject, string errMsg )
		 {
			  AssertFail( subject, "CREATE (:Node)", errMsg );
		 }

		 internal virtual void TestSuccessfulTokenWrite( S subject )
		 {
			  AssertEmpty( subject, "CALL db.createLabel('NewNodeName')" );
		 }

		 internal virtual void TestFailTokenWrite( S subject )
		 {
			  TestFailTokenWrite( subject, TokenCreateOpsNotAllowed );
		 }

		 internal virtual void TestFailTokenWrite( S subject, string errMsg )
		 {
			  AssertFail( subject, "CALL db.createLabel('NewNodeName')", errMsg );
		 }

		 internal virtual void TestSuccessfulSchema( S subject )
		 {
			  AssertEmpty( subject, "CREATE INDEX ON :Node(number)" );
		 }

		 internal virtual void TestFailSchema( S subject )
		 {
			  TestFailSchema( subject, SchemaOpsNotAllowed );
		 }

		 internal virtual void TestFailSchema( S subject, string errMsg )
		 {
			  AssertFail( subject, "CREATE INDEX ON :Node(number)", errMsg );
		 }

		 internal virtual void TestFailCreateUser( S subject, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.createUser('Craig', 'foo', false)", errMsg );
			  AssertFail( subject, "CALL dbms.security.createUser('Craig', '', false)", errMsg );
			  AssertFail( subject, "CALL dbms.security.createUser('', 'foo', false)", errMsg );
		 }

		 internal virtual void TestFailCreateRole( S subject, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.createRole('RealAdmins')", errMsg );
			  AssertFail( subject, "CALL dbms.security.createRole('RealAdmins')", errMsg );
			  AssertFail( subject, "CALL dbms.security.createRole('RealAdmins')", errMsg );
		 }

		 internal virtual void TestFailAddRoleToUser( S subject, string role, string username, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.addRoleToUser('" + role + "', '" + username + "')", errMsg );
		 }

		 internal virtual void TestFailRemoveRoleFromUser( S subject, string role, string username, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.removeRoleFromUser('" + role + "', '" + username + "')", errMsg );
		 }

		 internal virtual void TestFailDeleteUser( S subject, string username, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.deleteUser('" + username + "')", errMsg );
		 }

		 internal virtual void TestFailDeleteRole( S subject, string roleName, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.deleteRole('" + roleName + "')", errMsg );
		 }

		 internal virtual void TestSuccessfulListUsers( S subject, object[] users )
		 {
			  AssertSuccess( subject, "CALL dbms.security.listUsers() YIELD username", r => assertKeyIsArray( r, "username", users ) );
		 }

		 internal virtual void TestFailListUsers( S subject, int count, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.listUsers() YIELD username", errMsg );
		 }

		 internal virtual void TestSuccessfulListRoles( S subject, object[] roles )
		 {
			  AssertSuccess( subject, "CALL dbms.security.listRoles() YIELD role", r => assertKeyIsArray( r, "role", roles ) );
		 }

		 internal virtual void TestFailListRoles( S subject, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.listRoles() YIELD role", errMsg );
		 }

		 internal virtual void TestFailListUserRoles( S subject, string username, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.listRolesForUser('" + username + "') YIELD value AS roles RETURN count(roles)", errMsg );
		 }

		 internal virtual void TestFailListRoleUsers( S subject, string roleName, string errMsg )
		 {
			  AssertFail( subject, "CALL dbms.security.listUsersForRole('" + roleName + "') YIELD value AS users RETURN count(users)", errMsg );
		 }

		 internal virtual void TestFailTestProcs( S subject )
		 {
			  AssertFail( subject, "CALL test.allowedReadProcedure()", ReadOpsNotAllowed );
			  AssertFail( subject, "CALL test.allowedWriteProcedure()", WriteOpsNotAllowed );
			  AssertFail( subject, "CALL test.allowedSchemaProcedure()", SchemaOpsNotAllowed );
		 }

		 internal virtual void TestSuccessfulTestProcs( S subject )
		 {
			  AssertSuccess( subject, "CALL test.allowedReadProcedure()", r => assertKeyIs( r, "value", "foo" ) );
			  AssertSuccess( subject, "CALL test.allowedWriteProcedure()", r => assertKeyIs( r, "value", "a", "a" ) );
			  AssertSuccess( subject, "CALL test.allowedSchemaProcedure()", r => assertKeyIs( r, "value", "OK" ) );
		 }

		 internal virtual void AssertPasswordChangeWhenPasswordChangeRequired( S subject, string newPassword )
		 {
			  StringBuilder builder = new StringBuilder( 128 );
			  S subjectToUse;

			  // remove if-else ASAP
			  if ( IsEmbedded )
			  {
					subjectToUse = subject;
					builder.Append( "CALL dbms.security.changePassword('" );
					builder.Append( newPassword );
					builder.Append( "')" );
			  }
			  else
			  {
					subjectToUse = AdminSubject;
					builder.Append( "CALL dbms.security.changeUserPassword('" );
					builder.Append( Neo.nameOf( subject ) );
					builder.Append( "', '" );
					builder.Append( newPassword );
					builder.Append( "', false)" );
			  }

			  AssertEmpty( subjectToUse, builder.ToString() );
		 }

		 internal virtual void AssertFail( S subject, string call, string partOfErrorMsg )
		 {
			  string err = AssertCallEmpty( subject, call );
			  if ( StringUtils.isEmpty( partOfErrorMsg ) )
			  {
					assertThat( err, not( equalTo( "" ) ) );
			  }
			  else
			  {
					assertThat( err, containsString( partOfErrorMsg ) );
			  }
		 }

		 protected internal virtual void AssertEmpty( S subject, string call )
		 {
			  string err = AssertCallEmpty( subject, call );
			  assertThat( err, equalTo( "" ) );
		 }

		 internal virtual void AssertSuccess( S subject, string call, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  string err = Neo.executeQuery( subject, call, null, resultConsumer );
			  assertThat( err, equalTo( "" ) );
		 }

		 internal virtual IList<IDictionary<string, object>> CollectSuccessResult( S subject, string call )
		 {
			  IList<IDictionary<string, object>> result = new LinkedList<IDictionary<string, object>>();
			  AssertSuccess( subject, call, r => r.ForEach( result.add ) );
			  return result;
		 }

		 private string AssertCallEmpty( S subject, string call )
		 {
			  return Neo.executeQuery(subject, call, null, result =>
			  {
						  IList<IDictionary<string, object>> collect = result.ToList();
						  assertTrue( "Expected no results but got: " + collect, collect.Empty );
			  });
		 }

		 private void ExecuteQuery( S subject, string call )
		 {
			  Neo.executeQuery(subject, call, null, r =>
			  {
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean userHasRole(String user, String role) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 internal virtual bool UserHasRole( string user, string role )
		 {
			  return UserManager.getRoleNamesForUser( user ).Contains( role );
		 }

		 internal virtual IList<object> GetObjectsAsList( ResourceIterator<IDictionary<string, object>> r, string key )
		 {
			  return r.Select( s => s.get( key ) ).ToList();
		 }

		 internal virtual void AssertKeyIs( ResourceIterator<IDictionary<string, object>> r, string key, params object[] items )
		 {
			  AssertKeyIsArray( r, key, items );
		 }

		 private void AssertKeyIsArray( ResourceIterator<IDictionary<string, object>> r, string key, object[] items )
		 {
			  IList<object> results = GetObjectsAsList( r, key );
			  assertEquals( Arrays.asList( items ).size(), results.Count );
			  assertThat( results, containsInAnyOrder( java.util.items.Select( this.valueOf ).ToArray() ) );
		 }

		 internal static void AssertKeyIsMap( ResourceIterator<IDictionary<string, object>> r, string keyKey, string valueKey, object expected )
		 {
			  if ( expected is MapValue )
			  {
					AssertKeyIsMap( r, keyKey, valueKey, ( MapValue ) expected );
			  }
			  else
			  {
					assertKeyIsMap( r, keyKey, valueKey, ( IDictionary<string, object> ) expected );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static void assertKeyIsMap(org.neo4j.graphdb.ResourceIterator<java.util.Map<String,Object>> r, String keyKey, String valueKey, java.util.Map<String,Object> expected)
		 internal static void AssertKeyIsMap( ResourceIterator<IDictionary<string, object>> r, string keyKey, string valueKey, IDictionary<string, object> expected )
		 {
			  IList<IDictionary<string, object>> result = r.ToList();

			  assertEquals( "Results for should have size " + expected.Count + " but was " + result.Count, expected.Count, result.Count );

			  foreach ( IDictionary<string, object> row in result )
			  {
					string key = ( string ) row[keyKey];
					assertThat( expected, hasKey( key ) );
					assertThat( row, hasKey( valueKey ) );

					object objectValue = row[valueKey];
					if ( objectValue is System.Collections.IList )
					{
						 IList<string> value = ( IList<string> ) objectValue;
						 IList<string> expectedValues = ( IList<string> ) expected[key];
						 assertEquals( "sizes", value.Count, expectedValues.Count );
						 assertThat( value, containsInAnyOrder( expectedValues.ToArray() ) );
					}
					else
					{
						 string value = objectValue.ToString();
						 string expectedValue = expected[key].ToString();
						 assertThat( value, equalTo( expectedValue ) );
					}
			  }
		 }

		 internal static void AssertKeyIsMap( ResourceIterator<IDictionary<string, object>> r, string keyKey, string valueKey, MapValue expected )
		 {
			  IList<IDictionary<string, object>> result = r.ToList();

			  assertEquals( "Results for should have size " + expected.Size() + " but was " + result.Count, expected.Size(), result.Count );

			  foreach ( IDictionary<string, object> row in result )
			  {
					TextValue key = ( TextValue ) row[keyKey];
					assertTrue( expected.ContainsKey( key.StringValue() ) );
					assertThat( row, hasKey( valueKey ) );

					object objectValue = row[valueKey];
					if ( objectValue is ListValue )
					{
						 ListValue value = ( ListValue ) objectValue;
						 ListValue expectedValues = ( ListValue ) expected.Get( key.StringValue() );
						 assertEquals( "sizes", value.Size(), expectedValues.Size() );
						 assertThat( Arrays.asList( value.AsArray() ), containsInAnyOrder(expectedValues.AsArray()) );
					}
					else
					{
						 string value = ( ( TextValue ) objectValue ).stringValue();
						 string expectedValue = ( ( TextValue ) expected.Get( key.StringValue() ) ).stringValue();
						 assertThat( value, equalTo( expectedValue ) );
					}
			  }
		 }

		 // --------------------- helpers -----------------------
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shouldTerminateTransactionsForUser(S subject, String procedure) throws Throwable
		 internal virtual void ShouldTerminateTransactionsForUser( S subject, string procedure )
		 {
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<S> userThread = new ThreadedTransaction<S>( Neo, latch );
			  userThread.ExecuteCreateNode( Threading(), subject );
			  latch.StartAndWaitForAllToStart();

			  AssertEmpty( AdminSubject, "CALL " + format( procedure, Neo.nameOf( subject ) ) );

			  IDictionary<string, long> transactionsByUser = CountTransactionsByUsername();

			  assertThat( transactionsByUser[Neo.nameOf( subject )], equalTo( null ) );

			  latch.FinishAndWaitForAllToFinish();

			  userThread.CloseAndAssertExplicitTermination();

			  AssertEmpty( AdminSubject, "MATCH (n:Test) RETURN n.name AS name" );
		 }

		 private IDictionary<string, long> CountTransactionsByUsername()
		 {
			  return EnterpriseBuiltInDbmsProcedures.countTransactionByUsername( EnterpriseBuiltInDbmsProcedures.getActiveTransactions( Neo.LocalGraph.DependencyResolver ).Where( tx => !tx.terminationReason().Present ).Select(tx => tx.subject().username()) ).collect(Collectors.toMap(r => r.username, r => r.activeTransactions));
		 }

		 protected internal virtual object ToRawValue( object value )
		 {
			  if ( value is AnyValue )
			  {
					BaseToObjectValueWriter<Exception> writer = writer();
					( ( AnyValue ) value ).writeTo( writer );
					return writer.Value();
			  }
			  else
			  {
					return value;
			  }
		 }

		 internal virtual IDictionary<string, long> CountBoltConnectionsByUsername()
		 {
			  NetworkConnectionTracker connectionTracker = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( NetworkConnectionTracker ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return connectionTracker.ActiveConnections().Select(TrackedNetworkConnection::username).collect(groupingBy(identity(), counting()));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.bolt.v1.transport.socket.client.TransportConnection startBoltSession(String username, String password) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual TransportConnection StartBoltSession( string username, string password )
		 {
			  TransportConnection connection = new SocketConnection();
			  HostnamePort address = Neo.lookupConnector( DEFAULT_CONNECTOR_KEY );
			  IDictionary<string, object> authToken = map( "principal", username, "credentials", password, "scheme", "basic" );

			  connection.Connect( address ).send( Util.acceptedVersions( 1, 0, 0, 0 ) ).send( Util.chunk( new InitMessage( "TestClient/1.1", authToken ) ) );

			  assertThat( connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  assertThat( connection, Util.eventuallyReceives( msgSuccess() ) );
			  return connection;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static class CountResult
		 public class CountResult
		 {
			  public readonly string Count;

			  internal CountResult( long? count )
			  {
					this.Count = "" + count;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public static class ClassWithProcedures
		 public class ClassWithProcedures
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly AtomicReference<LatchedRunnables> TestLatchConflict = new AtomicReference<LatchedRunnables>();

			  internal static DoubleLatch DoubleLatch;

			  public static volatile DoubleLatch VolatileLatch;

			  public static IList<Exception> ExceptionsInProcedure = Collections.synchronizedList( new List<Exception>() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.procedure.TerminationGuard guard;
			  public TerminationGuard Guard;

			  [Procedure(name : "test.loop")]
			  public virtual void Loop()
			  {
					DoubleLatch latch = VolatileLatch;

					if ( latch != null )
					{
						 latch.StartAndWaitForAllToStart();
					}
					try
					{
						 //noinspection InfiniteLoopStatement
						 while ( true )
						 {
							  try
							  {
									Thread.Sleep( 250 );
							  }
							  catch ( InterruptedException )
							  {
									Thread.interrupted();
							  }
							  Guard.check();
						 }
					}
					catch ( Exception e ) when ( e is TransactionTerminatedException || e is TransactionGuardException )
					{
						 if ( e.status().Equals(TransactionTimedOut) )
						 {
							  throw new TransactionGuardException( TransactionTimedOut, PROCEDURE_TIMEOUT_ERROR, e );
						 }
						 else
						 {
							  throw e;
						 }
					}
					finally
					{
						 if ( latch != null )
						 {
							  latch.Finish();
						 }
					}
			  }

			  [Procedure(name : "test.neverEnding")]
			  public virtual void NeverEndingWithLock()
			  {
					DoubleLatch.start();
					DoubleLatch.finishAndWaitForAllToFinish();
			  }

			  [Procedure(name : "test.numNodes")]
			  public virtual Stream<CountResult> NumNodes()
			  {
					long? nNodes = Db.AllNodes.Count();
					return Stream.of( new CountResult( nNodes ) );
			  }

			  [Procedure(name : "test.staticReadProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> StaticReadProcedure()
			  {
					return Stream.of( new AuthProceduresBase.StringResult( "static" ) );
			  }

			  [Procedure(name : "test.staticWriteProcedure", mode : Neo4Net.Procedure.Mode.WRITE)]
			  public virtual Stream<AuthProceduresBase.StringResult> StaticWriteProcedure()
			  {
					return Stream.of( new AuthProceduresBase.StringResult( "static" ) );
			  }

			  [Procedure(name : "test.staticSchemaProcedure", mode : Neo4Net.Procedure.Mode.SCHEMA)]
			  public virtual Stream<AuthProceduresBase.StringResult> StaticSchemaProcedure()
			  {
					return Stream.of( new AuthProceduresBase.StringResult( "static" ) );
			  }

			  [Procedure(name : "test.allowedReadProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> AllowedProcedure1()
			  {
					Result result = Db.execute( "MATCH (:Foo) WITH count(*) AS c RETURN 'foo' AS foo" );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "foo" ).ToString() ) );
			  }

			  [Procedure(name : "test.otherAllowedReadProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> OtherAllowedProcedure()
			  {
					Result result = Db.execute( "MATCH (:Foo) WITH count(*) AS c RETURN 'foo' AS foo" );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "foo" ).ToString() ) );
			  }

			  [Procedure(name : "test.allowedWriteProcedure", mode : Neo4Net.Procedure.Mode.WRITE)]
			  public virtual Stream<AuthProceduresBase.StringResult> AllowedProcedure2()
			  {
					Db.execute( "UNWIND [1, 2] AS i CREATE (:VeryUniqueLabel {prop: 'a'})" );
					Result result = Db.execute( "MATCH (n:VeryUniqueLabel) RETURN n.prop AS a LIMIT 2" );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "a" ).ToString() ) );
			  }

			  [Procedure(name : "test.allowedSchemaProcedure", mode : Neo4Net.Procedure.Mode.SCHEMA)]
			  public virtual Stream<AuthProceduresBase.StringResult> AllowedProcedure3()
			  {
					Db.execute( "CREATE INDEX ON :VeryUniqueLabel(prop)" );
					return Stream.of( new AuthProceduresBase.StringResult( "OK" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "test.nestedAllowedProcedure", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<AuthProceduresBase.StringResult> nestedAllowedProcedure(@Name("nestedProcedure") String nestedProcedure)
			  [Procedure(name : "test.nestedAllowedProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> NestedAllowedProcedure( string nestedProcedure )
			  {
					Result result = Db.execute( "CALL " + nestedProcedure );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "value" ).ToString() ) );
			  }

			  [Procedure(name : "test.doubleNestedAllowedProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> DoubleNestedAllowedProcedure()
			  {
					Result result = Db.execute( "CALL test.nestedAllowedProcedure('test.allowedReadProcedure') YIELD value" );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "value" ).ToString() ) );
			  }

			  [Procedure(name : "test.failingNestedAllowedWriteProcedure", mode : Neo4Net.Procedure.Mode.WRITE)]
			  public virtual Stream<AuthProceduresBase.StringResult> FailingNestedAllowedWriteProcedure()
			  {
					Result result = Db.execute( "CALL test.nestedReadProcedure('test.allowedWriteProcedure') YIELD value" );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "value" ).ToString() ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "test.nestedReadProcedure", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<AuthProceduresBase.StringResult> nestedReadProcedure(@Name("nestedProcedure") String nestedProcedure)
			  [Procedure(name : "test.nestedReadProcedure", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<AuthProceduresBase.StringResult> NestedReadProcedure( string nestedProcedure )
			  {
					Result result = Db.execute( "CALL " + nestedProcedure );
					return result.Select( r => new AuthProceduresBase.StringResult( r.get( "value" ).ToString() ) );
			  }

			  [Procedure(name : "test.createNode", mode : WRITE)]
			  public virtual void CreateNode()
			  {
					Db.createNode();
			  }

			  [Procedure(name : "test.waitForLatch", mode : READ)]
			  public virtual void WaitForLatch()
			  {
					try
					{
						 TestLatchConflict.get().runBefore.run();
					}
					finally
					{
						 TestLatchConflict.get().doubleLatch.startAndWaitForAllToStart();
					}
					try
					{
						 TestLatchConflict.get().runAfter.run();
					}
					finally
					{
						 TestLatchConflict.get().doubleLatch.finishAndWaitForAllToFinish();
					}
			  }

			  [Procedure(name : "test.threadTransaction", mode : WRITE)]
			  public virtual void NewThreadTransaction()
			  {
					StartWriteThread();
			  }

			  [Procedure(name : "test.threadReadDoingWriteTransaction")]
			  public virtual void ThreadReadDoingWriteTransaction()
			  {
					StartWriteThread();
			  }

			  internal virtual void StartWriteThread()
			  {
					(new Thread(() =>
					{
					 DoubleLatch.start();
					 try
					 {
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Db.createNode( Label.label( "VeryUniqueLabel" ) );
							  tx.success();
						 }
					 }
					 catch ( Exception e )
					 {
						  ExceptionsInProcedure.Add( e );
					 }
					 finally
					 {
						  DoubleLatch.finish();
					 }
					})).Start();
			  }

			  protected internal class LatchedRunnables : IDisposable
			  {
					internal DoubleLatch DoubleLatch;
					internal ThreadStart RunBefore;
					internal ThreadStart RunAfter;

					internal LatchedRunnables( DoubleLatch doubleLatch, ThreadStart runBefore, ThreadStart runAfter )
					{
						 this.DoubleLatch = doubleLatch;
						 this.RunBefore = runBefore;
						 this.RunAfter = runAfter;
					}

					public override void Close()
					{
						 ClassWithProcedures.TestLatchConflict.set( null );
					}
			  }

			  internal static LatchedRunnables TestLatch
			  {
				  set
				  {
						ClassWithProcedures.TestLatchConflict.set( value );
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static class ClassWithFunctions
		 public class ClassWithFunctions
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

			  [UserFunction(name : "test.nonAllowedFunc")]
			  public virtual string NonAllowedFunc()
			  {
					return "success";
			  }

			  [UserFunction(name : "test.allowedFunc")]
			  public virtual string AllowedFunc()
			  {
					return "success for role1";
			  }

			  [UserFunction(name : "test.allowedFunction1")]
			  public virtual string AllowedFunction1()
			  {
					Result result = Db.execute( "MATCH (:Foo) WITH count(*) AS c RETURN 'foo' AS foo" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return result.Next()["foo"].ToString();
			  }

			  [UserFunction(name : "test.allowedFunction2")]
			  public virtual string AllowedFunction2()
			  {
					Result result = Db.execute( "MATCH (:Foo) WITH count(*) AS c RETURN 'foo' AS foo" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return result.Next()["foo"].ToString();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction(name = "test.nestedAllowedFunction") public String nestedAllowedFunction(@Name("nestedFunction") String nestedFunction)
			  [UserFunction(name : "test.nestedAllowedFunction")]
			  public virtual string NestedAllowedFunction( string nestedFunction )
			  {
					Result result = Db.execute( "RETURN " + nestedFunction + " AS value" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return result.Next()["value"].ToString();
			  }
		 }

		 protected internal abstract object ValueOf( object obj );

		 private BaseToObjectValueWriter<Exception> Writer()
		 {
			  return new BaseToObjectValueWriterAnonymousInnerClass( this );
		 }

		 private class BaseToObjectValueWriterAnonymousInnerClass : BaseToObjectValueWriter<Exception>
		 {
			 private readonly ProcedureInteractionTestBase<S> _outerInstance;

			 public BaseToObjectValueWriterAnonymousInnerClass( ProcedureInteractionTestBase<S> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Node newNodeProxyById( long id )
			 {
				  return null;
			 }

			 protected internal override Relationship newRelationshipProxyById( long id )
			 {
				  return null;
			 }

			 protected internal override Point newPoint( CoordinateReferenceSystem crs, double[] coordinate )
			 {
				  return null;
			 }
		 }
	}

}