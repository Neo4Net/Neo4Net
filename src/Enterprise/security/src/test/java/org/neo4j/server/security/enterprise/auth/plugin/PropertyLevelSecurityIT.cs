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
namespace Neo4Net.Server.security.enterprise.auth.plugin
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Result = Neo4Net.Graphdb.Result;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Context = Neo4Net.Procedure.Context;
	using Mode = Neo4Net.Procedure.Mode;
	using Procedure = Neo4Net.Procedure.Procedure;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;

	public class PropertyLevelSecurityIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseFacade _db;
		 private LoginContext _neo;
		 private LoginContext _smith;
		 private LoginContext _morpheus;
		 private LoginContext _jones;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  TestGraphDatabaseFactory s = new TestEnterpriseGraphDatabaseFactory();
			  _db = ( GraphDatabaseFacade ) s.NewImpermanentDatabaseBuilder( TestDirectory.storeDir() ).setConfig(SecuritySettings.property_level_authorization_enabled, "true").setConfig(SecuritySettings.property_level_authorization_permissions, "Agent=alias,secret").setConfig(SecuritySettings.procedure_roles, "test.*:procRole").setConfig(GraphDatabaseSettings.auth_enabled, "true").newGraphDatabase();
			  EnterpriseAuthAndUserManager authManager = ( EnterpriseAuthAndUserManager ) _db.DependencyResolver.resolveDependency( typeof( EnterpriseAuthManager ) );
			  Procedures procedures = _db.DependencyResolver.resolveDependency( typeof( Procedures ) );
			  procedures.RegisterProcedure( typeof( TestProcedure ) );
			  EnterpriseUserManager userManager = authManager.UserManager;
			  userManager.NewUser( "Neo", password( "eon" ), false );
			  userManager.NewUser( "Smith", password( "mr" ), false );
			  userManager.NewUser( "Jones", password( "mr" ), false );
			  userManager.NewUser( "Morpheus", password( "dealwithit" ), false );

			  userManager.NewRole( "procRole", "Jones" );
			  userManager.NewRole( "Agent", "Smith", "Jones" );

			  userManager.AddRoleToUser( PredefinedRoles.ARCHITECT, "Neo" );
			  userManager.AddRoleToUser( PredefinedRoles.READER, "Smith" );
			  userManager.AddRoleToUser( PredefinedRoles.READER, "Morpheus" );

			  _neo = authManager.Login( authToken( "Neo", "eon" ) );
			  _smith = authManager.Login( authToken( "Smith", "mr" ) );
			  _jones = authManager.Login( authToken( "Jones", "mr" ) );
			  _morpheus = authManager.Login( authToken( "Morpheus", "dealwithit" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotShowRestrictedTokensForRestrictedUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotShowRestrictedTokensForRestrictedUser()
		 {
			  Result result = Execute( _neo, "CREATE (n {name: 'Andersson', alias: 'neo'}) ", Collections.emptyMap() );
			  assertThat( result.QueryStatistics.NodesCreated, equalTo( 1 ) );
			  assertThat( result.QueryStatistics.PropertiesSet, equalTo( 2 ) );
			  result.Close();
			  Execute(_smith, "MATCH (n) WHERE n.name = 'Andersson' RETURN n, n.alias as alias", Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("alias"), equalTo(null) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowRestrictedTokensForUnrestrictedUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowRestrictedTokensForUnrestrictedUser()
		 {
			  Result result = Execute( _neo, "CREATE (n {name: 'Andersson', alias: 'neo'}) ", Collections.emptyMap() );
			  assertThat( result.QueryStatistics.NodesCreated, equalTo( 1 ) );
			  assertThat( result.QueryStatistics.PropertiesSet, equalTo( 2 ) );
			  result.Close();
			  Execute(_morpheus, "MATCH (n) WHERE n.name = 'Andersson' RETURN n, n.alias as alias", Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("alias"), equalTo("neo") );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissing()
		 {
			  Execute( _neo, "CREATE (n {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n) WHERE n.name = 'Andersson' RETURN n.alias as alias";

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("alias"), equalTo(null) );
			  });

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("alias"), equalTo(null) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingWhenFiltering() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingWhenFiltering()
		 {
			  Execute( _neo, "CREATE (n {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n) WHERE n.alias = 'neo' RETURN n";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(true)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForKeys() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForKeys()
		 {
			  Execute( _neo, "CREATE (n {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n) RETURN keys(n) AS keys";

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("keys"), equalTo(Collections.singletonList("name")) );
			  });

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("keys"), equalTo(Collections.singletonList("name")) );
			  });

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( ( IEnumerable<string> ) r.next().get("keys"), contains("name", "alias") );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForProperties() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForProperties()
		 {
			  Execute( _neo, "CREATE (n {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n) RETURN properties(n) AS props";

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(Collections.singletonMap("name", "Andersson")) );
			  });

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(Collections.singletonMap("name", "Andersson")) );
			  });

			  IDictionary<string, string> expected = new Dictionary<string, string>();
			  expected["name"] = "Andersson";
			  expected["alias"] = "neo";
			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(expected) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForExists()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) WHERE exists(n.alias) RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForStringBegins() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForStringBegins()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) WHERE n.alias starts with 'n' RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForNotContains() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForNotContains()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) WHERE NOT n.alias contains 'eo' RETURN n.alias, n.name";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Betasson', alias: 'beta'}) ", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Cetasson'}) ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				IDictionary<string, object> next = r.next();
				assertThat( next.get( "n.alias" ), equalTo( "beta" ) );
				assertThat( next.get( "n.name" ), equalTo( "Betasson" ) );
				assertThat( r.hasNext(), equalTo(false) );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForRange() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForRange()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) WHERE n.secret > 10 RETURN n.secret";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.secret = 42 ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.secret"), equalTo(42L) );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForCompositeQuery() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForCompositeQuery()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) WHERE n.name = 'Andersson' and n.alias = 'neo' RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

		 // INDEX

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingWhenFilteringWithIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingWhenFilteringWithIndex()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'})", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE INDEX ON :Person(alias)", Collections.emptyMap() ).close();
			  Execute( _neo, "CALL db.awaitIndexes", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) USING INDEX n:Person(alias) WHERE n.alias = 'neo' RETURN n";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.ExecutionPlanDescription.ToString(), containsString("NodeIndexSeek") );
				assertThat( r.hasNext(), equalTo(false) );
			  });

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(true)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForExistsWithIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForExistsWithIndex()
		 {
			  Execute( _neo, "CREATE INDEX ON :Person(alias)", Collections.emptyMap() ).close();
			  Execute( _neo, "CALL db.awaitIndexes", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) USING INDEX n:Person(alias) WHERE exists(n.alias) RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.ExecutionPlanDescription.ToString(), containsString("NodeIndexScan") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForStringBeginsWithIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForStringBeginsWithIndex()
		 {
			  Execute( _neo, "CREATE INDEX ON :Person(alias)", Collections.emptyMap() ).close();
			  Execute( _neo, "CALL db.awaitIndexes", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) USING INDEX n:Person(alias) WHERE n.alias starts with 'n' RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.ExecutionPlanDescription.ToString(), containsString("NodeIndexSeekByRange") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForRangeWithIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForRangeWithIndex()
		 {
			  Execute( _neo, "CREATE INDEX ON :Person(secret)", Collections.emptyMap() ).close();
			  Execute( _neo, "CALL db.awaitIndexes", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) USING INDEX n:Person(secret) WHERE n.secret > 10 RETURN n.secret";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.secret = 42 ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.ExecutionPlanDescription.ToString(), containsString("NodeIndexSeek") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.secret"), equalTo(42L) );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForCompositeWithIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForCompositeWithIndex()
		 {
			  Execute( _neo, "CREATE INDEX ON :Person(name , alias)", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE INDEX ON :Person(name)", Collections.emptyMap() ).close();
			  Execute( _neo, "CALL db.awaitIndexes", Collections.emptyMap() ).close();
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "MATCH (n:Person) USING INDEX n:Person(name, alias) WHERE n.name = 'Andersson' and n.alias = 'neo' RETURN n.alias";

			  Execute( _neo, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.ExecutionPlanDescription.ToString(), containsString("NodeIndexSeek") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("n.alias"), equalTo("neo") );
			  });

			  Execute( _smith, query, Collections.emptyMap(), r => assertThat(r.hasNext(), equalTo(false)) );
		 }

		 // RELATIONSHIPS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveLikeDataIsMissingForRelationshipProperties() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveLikeDataIsMissingForRelationshipProperties()
		 {
			  Execute( _neo, "CREATE (n {name: 'Andersson'}) CREATE (m { name: 'Betasson'}) CREATE (n)-[:Neighbour]->(m)", Collections.emptyMap() ).close();

			  string query = "MATCH (n)-[r]->(m) WHERE n.name = 'Andersson' AND m.name = 'Betasson' RETURN properties(r) AS props";

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(java.util.Collections.emptyMap()) );
			  });

			  Execute( _neo, "MATCH (n {name: 'Andersson'})-[r]->({name: 'Betasson'}) SET r.secret = 'lovers' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(java.util.Collections.emptyMap()) );
			  });

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("props"), equalTo(Collections.singletonMap("secret", "lovers")) );
			  });
		 }

		 // PROCS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveWithProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveWithProcedures()
		 {
			  Execute( _neo, "CREATE (n:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  string query = "CALL db.propertyKeys() YIELD propertyKey RETURN propertyKey ORDER BY propertyKey";

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("propertyKey"), equalTo("name") );
				assertThat( r.hasNext(), equalTo(false) );
			  });

			  Execute( _neo, "MATCH (n {name: 'Andersson'}) SET n.alias = 'neo' ", Collections.emptyMap() ).close();

			  Execute(_smith, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("propertyKey"), equalTo("name") );
				assertThat( r.hasNext(), equalTo(false) );
			  });

			  Execute(_neo, query, Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("propertyKey"), equalTo("alias") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("propertyKey"), equalTo("name") );
				assertThat( r.hasNext(), equalTo(true) );
				assertThat( r.next().get("propertyKey"), equalTo("secret") );
				assertThat( r.hasNext(), equalTo(false) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allowedProcedureShouldIgnorePropertyBlacklist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllowedProcedureShouldIgnorePropertyBlacklist()
		 {
			  Execute( _neo, "CREATE (:Person {name: 'Andersson'}) ", Collections.emptyMap() ).close();

			  AssertProcedureResult( _morpheus, Collections.singletonMap( "Andersson", "N/A" ) );
			  AssertProcedureResult( _smith, Collections.singletonMap( "Andersson", "N/A" ) );
			  AssertProcedureResult( _jones, Collections.singletonMap( "Andersson", "N/A" ) );

			  Execute( _neo, "MATCH (n:Person) WHERE n.name = 'Andersson' SET n.alias = 'neo' RETURN n", Collections.emptyMap() ).close();

			  AssertProcedureResult( _morpheus, Collections.singletonMap( "Andersson", "neo" ) );
			  AssertProcedureResult( _smith, Collections.singletonMap( "Andersson", "N/A" ) );
			  AssertProcedureResult( _jones, Collections.singletonMap( "Andersson", "neo" ) );
		 }

		 private void AssertProcedureResult( LoginContext user, IDictionary<string, string> nameAliasMap )
		 {
			  Execute(user, "CALL test.getAlias", Collections.emptyMap(), r =>
			  {
				assertThat( r.hasNext(), equalTo(true) );
				IDictionary<string, object> next = r.next();
				string name = ( string ) next.get( "name" );
				assertThat( nameAliasMap.ContainsKey( name ), equalTo( true ) );
				assertThat( next.get( "alias" ), equalTo( nameAliasMap[name] ) );
			  });
		 }

		 private void Execute( LoginContext subject, string query, IDictionary<string, object> @params, System.Action<Result> consumer )
		 {
			  Result result;
			  using ( InternalTransaction tx = _db.beginTransaction( @explicit, subject ) )
			  {
					result = _db.execute( tx, query, ValueUtils.asMapValue( @params ) );
					consumer( result );
					tx.Success();
					result.Close();
			  }
		 }

		 private Result Execute( LoginContext subject, string query, IDictionary<string, object> @params )
		 {
			  Result result;
			  using ( InternalTransaction tx = _db.beginTransaction( @explicit, subject ) )
			  {
					result = _db.execute( tx, query, ValueUtils.asMapValue( @params ) );
					result.ResultAsString();
					tx.Success();
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static class TestProcedure
		 public class TestProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

			  [Procedure(name : "test.getAlias", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<MyOutputRecord> Alias
			  {
				  get
				  {
						ResourceIterator<Node> nodes = Db.findNodes( Label.label( "Person" ) );
						return nodes.Select( n => new MyOutputRecord( ( string ) n.getProperty( "name" ), ( string ) n.getProperty( "alias", "N/A" ) ) );
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static class MyOutputRecord
		 public class MyOutputRecord
		 {
			  public string Name;
			  public string Alias;

			  public MyOutputRecord( string name, string alias )
			  {
					this.Name = name;
					this.Alias = alias;
			  }
		 }
	}

}