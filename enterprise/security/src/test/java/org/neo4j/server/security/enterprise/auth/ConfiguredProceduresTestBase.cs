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
	using Test = org.junit.Test;


	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using UserFunctionSignature = Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionSignature;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using SecuritySettings = Org.Neo4j.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.util.collections.Sets.newSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
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

	public abstract class ConfiguredProceduresTestBase<S> : ProcedureInteractionTestBase<S>
	{

		 public override void SetUp()
		 {
			  // tests are required to setup database with specific configs
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateLongRunningProcedureThatChecksTheGuardRegularlyOnTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateLongRunningProcedureThatChecksTheGuardRegularlyOnTimeout()
		 {
			  ConfiguredSetup( stringMap( GraphDatabaseSettings.transaction_timeout.name(), "4s" ) );

			  AssertFail( AdminSubject, "CALL test.loop", PROCEDURE_TIMEOUT_ERROR );

			  Result result = Neo.LocalGraph.execute( "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllowedToConfigSetting() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllowedToConfigSetting()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.default_allowed.name(), "nonEmpty" ) );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );

			  ProcedureSignature numNodes = procedures.Procedure( new QualifiedName( new string[]{ "test" }, "numNodes" ) ).signature();
			  assertThat( Arrays.asList( numNodes.Allowed() ), containsInAnyOrder("nonEmpty") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllowedToDefaultValueAndRunningWorks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllowedToDefaultValueAndRunningWorks()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.default_allowed.name(), "role1" ) );

			  UserManager.newRole( "role1", "noneSubject" );
			  AssertSuccess( NoneSubject, "CALL test.numNodes", itr => assertKeyIs( itr, "count", "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunProcedureWithMatchingWildcardAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunProcedureWithMatchingWildcardAllowed()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.*:role1" ) );

			  UserManager.newRole( "role1", "noneSubject" );
			  AssertSuccess( NoneSubject, "CALL test.numNodes", itr => assertKeyIs( itr, "count", "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRunProcedureWithMismatchingWildCardAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRunProcedureWithMismatchingWildCardAllowed()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "tes.*:role1" ) );

			  UserManager.newRole( "role1", "noneSubject" );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );

			  ProcedureSignature numNodes = procedures.Procedure( new QualifiedName( new string[]{ "test" }, "numNodes" ) ).signature();
			  assertThat( Arrays.asList( numNodes.Allowed() ), empty() );
			  AssertFail( NoneSubject, "CALL test.numNodes", "Read operations are not allowed" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSetProcedureAllowedIfSettingNotSet() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSetProcedureAllowedIfSettingNotSet()
		 {
			  ConfiguredSetup( DefaultConfiguration() );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );

			  ProcedureSignature numNodes = procedures.Procedure( new QualifiedName( new string[]{ "test" }, "numNodes" ) ).signature();
			  assertThat( Arrays.asList( numNodes.Allowed() ), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalGetWithoutIsPresent") @Test public void shouldSetAllowedToConfigSettingForUDF() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllowedToConfigSettingForUDF()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.default_allowed.name(), "nonEmpty" ) );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );

			  UserFunctionSignature funcSig = procedures.Function( new QualifiedName( new string[]{ "test" }, "nonAllowedFunc" ) ).signature();
			  assertThat( Arrays.asList( funcSig.Allowed() ), containsInAnyOrder("nonEmpty") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllowedToDefaultValueAndRunningWorksForUDF() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllowedToDefaultValueAndRunningWorksForUDF()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.default_allowed.name(), "role1" ) );

			  UserManager.newRole( "role1", "noneSubject" );
			  AssertSuccess( Neo.login( "noneSubject", "abc" ), "RETURN test.allowedFunc() AS c", itr => assertKeyIs( itr, "c", "success for role1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalGetWithoutIsPresent") @Test public void shouldNotSetProcedureAllowedIfSettingNotSetForUDF() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSetProcedureAllowedIfSettingNotSetForUDF()
		 {
			  ConfiguredSetup( DefaultConfiguration() );
			  Procedures procedures = Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) );

			  UserFunctionSignature funcSig = procedures.Function( new QualifiedName( new string[]{ "test" }, "nonAllowedFunc" ) ).signature();
			  assertThat( Arrays.asList( funcSig.Allowed() ), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetWildcardRoleConfigOnlyIfNotAnnotated() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetWildcardRoleConfigOnlyIfNotAnnotated()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.*:tester" ) );

			  UserManager.newRole( "tester", "noneSubject" );

			  AssertSuccess( NoneSubject, "CALL test.numNodes", itr => assertKeyIs( itr, "count", "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllMatchingWildcardRoleConfigs() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllMatchingWildcardRoleConfigs()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.*:tester;test.create*:other" ) );

			  UserManager.newRole( "tester", "noneSubject" );
			  UserManager.newRole( "other", "readSubject" );

			  AssertSuccess( ReadSubject, "CALL test.allowedReadProcedure", itr => assertKeyIs( itr, "value", "foo" ) );
			  AssertSuccess( NoneSubject, "CALL test.createNode", ResourceIterator.close );
			  AssertSuccess( ReadSubject, "CALL test.createNode", ResourceIterator.close );
			  AssertSuccess( NoneSubject, "CALL test.numNodes", itr => assertKeyIs( itr, "count", "5" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllMatchingWildcardRoleConfigsWithDefaultForUDFs() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllMatchingWildcardRoleConfigsWithDefaultForUDFs()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.*:tester;test.create*:other", SecuritySettings.default_allowed.name(), "default" ) );

			  UserManager.newRole( "tester", "noneSubject" );
			  UserManager.newRole( "default", "noneSubject" );
			  UserManager.newRole( "other", "readSubject" );

			  AssertSuccess( NoneSubject, "RETURN test.nonAllowedFunc() AS f", itr => assertKeyIs( itr, "f", "success" ) );
			  AssertSuccess( ReadSubject, "RETURN test.allowedFunction1() AS f", itr => assertKeyIs( itr, "f", "foo" ) );
			  AssertSuccess( ReadSubject, "RETURN test.nonAllowedFunc() AS f", itr => assertKeyIs( itr, "f", "success" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWriteAfterAllowedReadProcedureWithAuthDisabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleWriteAfterAllowedReadProcedureWithAuthDisabled()
		 {
			  Neo = setUpNeoServer( stringMap( GraphDatabaseSettings.auth_enabled.name(), "false" ) );

			  Neo.LocalGraph.DependencyResolver.resolveDependency( typeof( Procedures ) ).registerProcedure( typeof( ClassWithProcedures ) );

			  S subject = Neo.login( "no_auth", "" );
			  AssertEmpty( subject, "CALL test.allowedReadProcedure() YIELD value CREATE (:NewNode {name: value})" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleRolesSpecifiedForMapping() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleRolesSpecifiedForMapping()
		 {
			  // Given
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.*:tester, other" ) );

			  // When
			  UserManager.newRole( "tester", "noneSubject" );
			  UserManager.newRole( "other", "readSubject" );

			  // Then
			  AssertSuccess( ReadSubject, "CALL test.createNode", ResourceIterator.close );
			  AssertSuccess( NoneSubject, "CALL test.numNodes", itr => assertKeyIs( itr, "count", "4" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListCorrectRolesForDBMSProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListCorrectRolesForDBMSProcedures()
		 {
			  ConfiguredSetup( DefaultConfiguration() );

			  IDictionary<string, ISet<string>> expected = genericMap( "dbms.changePassword", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.functions", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.killQueries", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.killQuery", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.listActiveLocks", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.listConfig", newSet( ADMIN ), "dbms.listQueries", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.procedures", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.security.activateUser", newSet( ADMIN ), "dbms.security.addRoleToUser", newSet( ADMIN ), "dbms.security.changePassword", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.security.changeUserPassword", newSet( ADMIN ), "dbms.security.clearAuthCache", newSet( ADMIN ), "dbms.security.createRole", newSet( ADMIN ), "dbms.security.createUser", newSet( ADMIN ), "dbms.security.deleteRole", newSet( ADMIN ), "dbms.security.deleteUser", newSet( ADMIN ), "dbms.security.listRoles", newSet( ADMIN ), "dbms.security.listRolesForUser", newSet( ADMIN ), "dbms.security.listUsers", newSet( ADMIN ), "dbms.security.listUsersForRole", newSet( ADMIN ), "dbms.security.removeRoleFromUser", newSet( ADMIN ), "dbms.security.showCurrentUser", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.showCurrentUser", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.security.suspendUser", newSet( ADMIN ), "dbms.setTXMetaData", newSet( READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ) );

			  AssertListProceduresHasRoles( ReadSubject, expected, "CALL dbms.procedures" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowAllowedRolesWhenListingProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowAllowedRolesWhenListingProcedures()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.numNodes:counter,user", SecuritySettings.default_allowed.name(), "default" ) );

			  IDictionary<string, ISet<string>> expected = genericMap( "test.staticReadProcedure", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "test.staticWriteProcedure", newSet( "default", EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "test.staticSchemaProcedure", newSet( "default", ARCHITECT, ADMIN ), "test.annotatedProcedure", newSet( "annotated", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "test.numNodes", newSet( "counter", "user", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "db.labels", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.security.changePassword", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.procedures", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.listQueries", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "dbms.security.createUser", newSet( ADMIN ), "db.createLabel", newSet( "default", EDITOR, PUBLISHER, ARCHITECT, ADMIN ) );

			  string call = "CALL dbms.procedures";
			  AssertListProceduresHasRoles( AdminSubject, expected, call );
			  AssertListProceduresHasRoles( SchemaSubject, expected, call );
			  AssertListProceduresHasRoles( WriteSubject, expected, call );
			  AssertListProceduresHasRoles( ReadSubject, expected, call );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowAllowedRolesWhenListingFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowAllowedRolesWhenListingFunctions()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.procedure_roles.name(), "test.allowedFunc:counter,user", SecuritySettings.default_allowed.name(), "default" ) );

			  IDictionary<string, ISet<string>> expected = genericMap( "test.annotatedFunction", newSet( "annotated", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "test.allowedFunc", newSet( "counter", "user", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ), "test.nonAllowedFunc", newSet( "default", READER, EDITOR, PUBLISHER, ARCHITECT, ADMIN ) );

			  string call = "CALL dbms.functions";
			  AssertListProceduresHasRoles( AdminSubject, expected, call );
			  AssertListProceduresHasRoles( SchemaSubject, expected, call );
			  AssertListProceduresHasRoles( WriteSubject, expected, call );
			  AssertListProceduresHasRoles( ReadSubject, expected, call );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceMessageAtFailWhenTryingToKill() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveNiceMessageAtFailWhenTryingToKill()
		 {
			  ConfiguredSetup( stringMap( GraphDatabaseSettings.kill_query_verbose.name(), "true" ) );

			  string query = "CALL dbms.killQuery('query-9999999999')";
			  IDictionary<string, object> expected = new Dictionary<string, object>();
			  expected["queryId"] = ValueOf( "query-9999999999" );
			  expected["username"] = ValueOf( "n/a" );
			  expected["message"] = ValueOf( "No Query found with this id" );
			  AssertSuccess( AdminSubject, query, r => assertThat( r.next(), equalTo(expected) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGiveNiceMessageAtFailWhenTryingToKillWhenConfigured() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotGiveNiceMessageAtFailWhenTryingToKillWhenConfigured()
		 {
			  ConfiguredSetup( stringMap( GraphDatabaseSettings.kill_query_verbose.name(), "false" ) );
			  string query = "CALL dbms.killQuery('query-9999999999')";
			  AssertSuccess( AdminSubject, query, r => assertThat( r.hasNext(), @is(false) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceMessageAtFailWhenTryingToKillMoreThenOne() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveNiceMessageAtFailWhenTryingToKillMoreThenOne()
		 {
			  //Given
			  ConfiguredSetup( stringMap( GraphDatabaseSettings.kill_query_verbose.name(), "true" ) );
			  string query = "CALL dbms.killQueries(['query-9999999999', 'query-9999999989'])";

			  //Expect
			  ISet<IDictionary<string, object>> expected = new HashSet<IDictionary<string, object>>();
			  IDictionary<string, object> firstResultExpected = new Dictionary<string, object>();
			  firstResultExpected["queryId"] = ValueOf( "query-9999999989" );
			  firstResultExpected["username"] = ValueOf( "n/a" );
			  firstResultExpected["message"] = ValueOf( "No Query found with this id" );
			  IDictionary<string, object> secondResultExpected = new Dictionary<string, object>();
			  secondResultExpected["queryId"] = ValueOf( "query-9999999999" );
			  secondResultExpected["username"] = ValueOf( "n/a" );
			  secondResultExpected["message"] = ValueOf( "No Query found with this id" );
			  expected.Add( firstResultExpected );
			  expected.Add( secondResultExpected );

			  //Then
			  AssertSuccess(AdminSubject, query, r =>
			  {
				ISet<IDictionary<string, object>> actual = r.collect( toSet() );
				assertThat( actual, equalTo( expected ) );
			  });
		 }

		 private void AssertListProceduresHasRoles( S subject, IDictionary<string, ISet<string>> expected, string call )
		 {
			  AssertSuccess(subject, call, itr =>
			  {
				IList<string> failures = itr.Where(record =>
				{
					 string name = ToRawValue( record.get( "name" ) ).ToString();
					 IList<object> roles = ( IList<object> ) ToRawValue( record.get( "roles" ) );
					 return expected.ContainsKey( name ) && !expected[name].SetEquals( new HashSet<>( roles ) );
				}).Select(record =>
				{
					 string name = ToRawValue( record.get( "name" ) ).ToString();
					 return name + ": expected '" + expected[name] + "' but was '" + record.get( "roles" ) + "'";
				}).ToList();

				assertThat( "Expectations violated: " + failures.ToString(), failures.Empty );
			  });
		 }
	}

}